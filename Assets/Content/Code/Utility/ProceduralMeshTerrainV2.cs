using System.Collections.Generic;
using Area;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class ProceduralMeshTerrainV2 : MonoBehaviour
{
    public MeshFilter mf;
    public AreaManager am;
    public MeshCollider meshCollider;

    [Header ("Generator Controls")] public bool enableSmoothing = false;
    [PropertyRange (1, 16)] public int subdivisions = 0;
    [PropertyRange (0, 16)] public int smoothingIterations;
    [PropertyRange (0, 1)] public float averageWeight = 1;

    public bool outputCurvatureToVertexColors = false;
    public float curvatureScalar = 1f;

    [NonSerialized] private AreaVolumePoint[,] surfacePoints;
    [NonSerialized] private int[,] heightfield;
    [NonSerialized] private TerrainSpot[,] terrainSpots;

    [NonSerialized] private int[] grid;

    //Mesh Data
    [NonSerialized] private List<int> indices = new List<int> ();
    [NonSerialized] private List<Vector3> vertices = new List<Vector3> ();
    [NonSerialized] private readonly List<Vector3> verticesSmoothed = new List<Vector3>();
    [NonSerialized] private List<Vector3> normals = new List<Vector3> ();
    [NonSerialized] private List<Vector4> vertexData = new List<Vector4> ();
    [NonSerialized] private Mesh surfaceMesh;

    [Header ("Gizmo Controls")] public bool drawOnce = false;
    public bool drawContinual = false;

    public bool showHeightField;
    public Color heightFieldColor;

    public bool showAreaVolumePoints;
    public Color surfacePointsColor;

    public bool showFiltering = false;
    public Color filteredSurfacePointColor;

    public bool showTerrainSpots;
    public Color terrainSpotFilledColor;
    public Color terrainSpotEmptyColor;
    public Color terrainSpotFlatColor;
    public Vector4 vertexDataDefault = new Vector4 (0f, 1f, 0f, 0.5f);

    [NonSerialized] private Vector3 previousNormalA;
    [NonSerialized] private Vector3 previousNormalB;
    [NonSerialized] private Vector3 previousNormalC;
    [NonSerialized] private Vector3 previousNormalD;

    public void ClearData ()
    {
        //Clear data after it has been used by external systems
    }

    public struct TerrainSpot
    {
        public bool flat;
        public bool empty;
        public bool flipTriangles;

        //Structure that holds vertex indices
        public int[,] indices;

        //Clockwise layout
        public Vector3 controlPointSouthWest;
        public Vector3 controlPointNorthWest;
        public Vector3 controlPointNorthEast;
        public Vector3 controlPointSouthEast;
    }

    [Button ("Rebuild", ButtonSizes.Large)]
    public void Rebuild (bool fastMode = false)
    {
        if (am == null || am.points.Count < 8)
        {
            return;
        }
        gameObject.layer = LayerMask.NameToLayer ("Environment");
        #if PB_MODSDK
        if (am.useNewTerrainMeshAlgorithm)
        {
            CollectSurfacePointsNewAlgo ();
        }
        else
        {
            // XXX keeping around for comparison until I'm more certain about the new algorithm.
            CollectSurfacePoints ();
        }
        #else
        CollectSurfacePoints ();
        #endif
        RebuildSurfaceMesh (fastMode);
    }

    public TerrainSpot[,] GetTerrainSpots ()
    {
        return terrainSpots;
    }

    public List<Vector3> GetVertexData ()
    {
        return vertices;
    }

    private void ConstructByWalking (bool fastMode)
    {
        if (am == null)
        {
            return;
        }

        vertices.Clear ();
        indices.Clear ();
        vertexData.Clear ();

        //Let's go through and iterate each of these spots to figure out what sort of configuration they have,

        //First setup and initialize our array of spots if we need to
        var columns = am.boundsFull.x - 1;
        var rows = am.boundsFull.z - 1;

        subdivisions = Mathf.Max (1, subdivisions);
        var edgeIndicesCount = subdivisions + 1;

        //Pass One
        //Advance down each column, and extract base data
        GatherTerrainSpots (columns, rows, edgeIndicesCount);

        //Pass Two
        //Iterate the collection to build vertex arrays
        BuildVertexArrays (columns, rows, edgeIndicesCount);

        //Pass Three - Smoothing
        if (enableSmoothing)
        {
            SmoothEdges (columns, rows, edgeIndicesCount);
        }

        //Begin Vertex AO Pass
        if (outputCurvatureToVertexColors && !fastMode)
        {
            CalculateAverageNormals (columns, rows, edgeIndicesCount);
            CalculateCurvature (columns, rows, edgeIndicesCount);
        }
    }

    void GatherTerrainSpots (int columns, int rows, int edgeIndicesCount)
    {
        if (terrainSpots == null || (terrainSpots.GetLength (0) != columns || terrainSpots.GetLength (1) != rows))
        {
            terrainSpots = new TerrainSpot[columns, rows];
        }

        for (var c = 0; c < columns; ++c)
        {
            for (var r = 0; r < rows; ++r)
            {
                var newSpot = new TerrainSpot ();

                var surfaceA = surfacePoints[c, r];
                var surfaceB = surfacePoints[c, r + 1];
                var surfaceC = surfacePoints[c + 1, r + 1];
                var surfaceD = surfacePoints[c + 1, r];

                //Determine if this spot is empty or not,
                newSpot.empty = !AreaManager.IsPointTerrain (surfaceA);

                //Determine if this is flat or not, we're looking at the indices for now, and not anything to do with offsets, that might get a little bit tricky?
                //Need to discuss this a bit more with Artyom

                // Avoid NREs popping up for some reason
                var surfaceAHeight = surfaceA?.pointPositionIndex.y ?? 0f;
                var surfaceBHeight = surfaceB?.pointPositionIndex.y ?? 0f;
                var surfaceCHeight = surfaceC?.pointPositionIndex.y ?? 0f;
                var surfaceDHeight = surfaceD?.pointPositionIndex.y ?? 0f;

                newSpot.flat = surfaceAHeight.RoughlyEqual (surfaceBHeight) &&
                    surfaceBHeight.RoughlyEqual (surfaceCHeight) &&
                    surfaceCHeight.RoughlyEqual (surfaceDHeight);

                newSpot.indices = new int[edgeIndicesCount, edgeIndicesCount];

                newSpot.controlPointSouthWest = am.GetSpotVertexPosition (surfaceA);
                newSpot.controlPointNorthWest = am.GetSpotVertexPosition (surfaceB);
                newSpot.controlPointNorthEast = am.GetSpotVertexPosition (surfaceC);
                newSpot.controlPointSouthEast = am.GetSpotVertexPosition (surfaceD);

                terrainSpots[c, r] = newSpot;
            }
        }
    }

    void BuildVertexArrays (int columns, int rows, int edgeIndicesCount)
    {
        var interpolationStep = 1f / subdivisions;
        for (var c = 0; c < columns; ++c)
        {
            for (var r = 0; r < rows; ++r)
            {
                var currentSpot = terrainSpots[c, r];

                if (currentSpot.empty)
                {
                    continue;
                }

                var southOccupied = r - 1 >= 0 && !terrainSpots[c, r - 1].empty;
                var westOccupied = c - 1 >= 0 && !terrainSpots[c - 1, r].empty;

                //If a neighbor tile is occupied we can extract vertex indices from it

                //First case, we can't extract any data and must make new vertices from scratch
                if (!southOccupied && !westOccupied)
                {
                    //Create the entire grid of indices
                    for (var x = 0; x < edgeIndicesCount; ++x)
                    {
                        for (var z = 0; z < edgeIndicesCount; ++z)
                        {
                            vertices.Add (GetInterpolatedVertexPosition (x, z, interpolationStep, currentSpot));
                            vertexData.Add (vertexDataDefault);
                            currentSpot.indices[x, z] = vertices.Count - 1;
                        }
                    }
                }
                else
                {
                    //Both are occupied, previously we eliminated the case of neither being occupied
                    if (southOccupied && westOccupied)
                    {
                        var southSpot = terrainSpots[c, r - 1];
                        var westSpot = terrainSpots[c - 1, r];

                        for (var i = 0; i < edgeIndicesCount; ++i)
                        {
                            currentSpot.indices[i, 0] = southSpot.indices[i, edgeIndicesCount - 1];
                            currentSpot.indices[0, i] = westSpot.indices[edgeIndicesCount - 1, i];
                        }
                    }
                    //At this point only one neighbor is occupied
                    //Only South is occupied
                    else if (southOccupied)
                    {
                        var southSpot = terrainSpots[c, r - 1];
                        //first, let's fill the vertices from the southern spot's northern edge
                        for (var i = 0; i < edgeIndicesCount; ++i)
                        {
                            currentSpot.indices[i, 0] = southSpot.indices[i, edgeIndicesCount - 1];
                        }

                        //Build the western edge from scratch, skipping the corner index
                        for (var i = 1; i < edgeIndicesCount; ++i)
                        {
                            vertices.Add (GetInterpolatedVertexPosition (0, i, interpolationStep, currentSpot));
                            vertexData.Add (vertexDataDefault);
                            currentSpot.indices[0, i] = vertices.Count - 1;
                        }
                    }
                    //Logically, west must be occupied, as it is the only option left
                    else
                    {
                        var westSpot = terrainSpots[c - 1, r];
                        for (var i = 0; i < edgeIndicesCount; ++i)
                        {
                            currentSpot.indices[0, i] = westSpot.indices[edgeIndicesCount - 1, i];
                        }

                        for (var i = 1; i < edgeIndicesCount; ++i)
                        {
                            vertices.Add (GetInterpolatedVertexPosition (i, 0, interpolationStep, currentSpot));
                            vertexData.Add (vertexDataDefault);
                            currentSpot.indices[i, 0] = vertices.Count - 1;
                        }
                    }

                    //By now, we're guaranteed to have filled the edge vertices along the western and southern sides
                    //We still need to create the rest of vertices in the interior of the grid, as well as
                    //storing the edge indices for the northern and eastern sides

                    for (var x = 1; x < edgeIndicesCount; ++x)
                    {
                        for (var z = 1; z < edgeIndicesCount; ++z)
                        {
                            vertices.Add (GetInterpolatedVertexPosition (x, z, interpolationStep, currentSpot));
                            vertexData.Add (vertexDataDefault);
                            currentSpot.indices[x, z] = vertices.Count - 1;
                        }
                    }
                }

                //Since we're using a struct, we must write our data back, or it will be lost
                terrainSpots[c, r] = currentSpot;

                //Finally, let's go ahead and create the indices for this, we should have all the data we need now
                //Layout of Vertices
                // -- Using Clock wise winding

                for (var x = 0; x < edgeIndicesCount - 1; ++x)
                {
                    for (var z = 0; z < edgeIndicesCount - 1; ++z)
                    {
                        var indexA = currentSpot.indices[x, z];
                        var indexB = currentSpot.indices[x, z + 1];
                        var indexC = currentSpot.indices[x + 1, z + 1];
                        var indexD = currentSpot.indices[x + 1, z];

                        if (currentSpot.flipTriangles)
                        {
                            // tri1 = ABD
                            indices.Add (indexA);
                            indices.Add (indexB);
                            indices.Add (indexD);
                            // tri2 = BCD
                            indices.Add (indexB);
                            indices.Add (indexC);
                            indices.Add (indexD);
                        }
                        else
                        {
                            // Standard Layout
                            // B -- C
                            // | /  |
                            // A -- D

                            // Standard Layout indices
                            // tri1 = ABC
                            indices.Add (indexA);
                            indices.Add (indexB);
                            indices.Add (indexC);
                            // tri2 = ACD
                            indices.Add (indexA);
                            indices.Add (indexC);
                            indices.Add (indexD);
                        }
                    }
                }
            }
        }
    }

    void SmoothEdges (int columns, int rows, int edgeIndicesCount)
    {
        heightOp.AverageWeight = averageWeight;
        heightOp.Source = vertices;
        heightOp.Destination = verticesSmoothed;
        heightOp.Destination.Clear ();
        heightOp.Destination.AddRange (vertices);

        for (var i = 0; i < smoothingIterations; i += 1)
        {
            ApplyVertexComputation (columns, rows, edgeIndicesCount, heightOp);
            //Swap?
            var temp = heightOp.Source;
            heightOp.Source = heightOp.Destination;
            heightOp.Destination = temp;
        }

        if (smoothingIterations % 2 != 0)
        {
            vertices = heightOp.Source;
        }
    }

    void CalculateAverageNormals (int columns, int rows, int edgeIndicesCount)
    {
        normalsOp.Vertices = vertices;
        normalsOp.VertexData = vertexData;
        ApplyVertexComputation (columns, rows, edgeIndicesCount, normalsOp);
    }

    void CalculateCurvature (int columns, int rows, int edgeIndicesCount)
    {
        curvatureOp.CurvatureScalar = curvatureScalar;
        curvatureOp.VertexData = vertexData;
        ApplyVertexComputation (columns, rows, edgeIndicesCount, curvatureOp);
    }

    void ApplyVertexComputation (int columns, int rows, int edgeIndicesCount, VertexComputation op)
    {
        var spec = nis;
        spec.Columns = columns;
        spec.Rows = rows;
        spec.EdgeIndicesCount = edgeIndicesCount;

        for (var c = 0; c < columns; c += 1)
        {
            spec.C = c;
            for (var r = 0; r < rows; r += 1)
            {
                spec.CurrentSpot = terrainSpots[c, r];
                if (spec.CurrentSpot.empty)
                {
                    continue;
                }

                spec.R = r;
                spec.NorthOccupied = r + 1 < rows && !terrainSpots[c, r + 1].empty;
                spec.SouthOccupied = r - 1 >= 0 && !terrainSpots[c, r - 1].empty;
                spec.EastOccupied = c + 1 < columns && !terrainSpots[c + 1, r].empty;
                spec.WestOccupied = c - 1 >= 0 && !terrainSpots[c - 1, r].empty;

                //Iterating our interior vertex array, on this spot
                //A Continue here, is synonymous with snapping the vertex to its original, linearly interpolated value
                for (var x = 0; x < edgeIndicesCount; x += 1)
                {
                    spec.X = x;
                    for (var z = 0; z < edgeIndicesCount; z += 1)
                    {
                        // -- Corner Snapping Detection
                        if (AnyCornerEmpty (x, z, c, r, columns, rows, edgeIndicesCount))
                        {
                            continue;
                        }
                        // -- End Corner Snapping detection

                        //Now we're going to copy index data for our neighbors, that will point into our vertex array
                        spec.Z = z;
                        var indexes = GetNeighborIndexes (spec);
                        if (!indexes.OK)
                        {
                            continue;
                        }

                        op.Apply (indexes);
                    }
                }
            }
        }
    }

    bool AnyCornerEmpty (int x, int z, int c, int r, int columns, int rows, int edgeIndicesCount)
    {
        if (x == 0 && z == 0 && c - 1 >= 0 && r - 1 >= 0)
        {
            //SW
            if (terrainSpots[c - 1, r - 1].empty)
            {
                return true;
            }
        }
        if (x == 0 && z == edgeIndicesCount - 1 && r + 1 < rows && c - 1 >= 0)
        {
            //NW
            if (terrainSpots[c - 1, r + 1].empty)
            {
                return true;
            }
        }
        if (x == edgeIndicesCount - 1 && z == edgeIndicesCount - 1 && r + 1 < rows && c + 1 < columns)
        {
            //NE
            if (terrainSpots[c + 1, r + 1].empty)
            {
                return true;
            }
        }
        if (x == edgeIndicesCount - 1 && z == 0 && r - 1 >= 0 && c + 1 < columns)
        {
            //SE
            if (terrainSpots[c + 1, r - 1].empty)
            {
                return true;
            }
        }
        return false;
    }

    NeighborIndexResult GetNeighborIndexes (NeighborIndexSpec spec)
    {
        nri.OK = false;
        nri.Current = spec.CurrentSpot.indices[spec.X, spec.Z];

        //This is our western edge
        if (spec.X > 0)
        {
            nri.West = spec.CurrentSpot.indices[spec.X - 1, spec.Z];
        }
        else if (spec.WestOccupied)
        {
            var westNeighbor = terrainSpots[spec.C - 1, spec.R];
            //Since the edges are copies of shared vertices, we need to ensure we grab the next one, by shifting one step deeper
            nri.West = westNeighbor.indices[spec.EdgeIndicesCount - 2, spec.Z];
        }
        else if (spec.C == 0)
        {
            //Don't snap on world boundaries, instead treat it as a flat spot (continuation of our neighbor)
            nri.West = nri.Current;
        }
        else
        {
            return nri;
        }

        //Eastern Edge
        if (spec.X < spec.EdgeIndicesCount - 1)
        {
            nri.East = spec.CurrentSpot.indices[spec.X + 1, spec.Z];
        }
        else if (spec.EastOccupied)
        {
            var eastNeighbor = terrainSpots[spec.C + 1, spec.R];
            //This is offset from 0 to 1, to skip edge vertex
            nri.East = eastNeighbor.indices[1, spec.Z];
        }
        else if (spec.C == spec.Columns - 1)
        {
            nri.East = nri.Current;
        }
        else
        {
            return nri;
        }

        //Southern Edge
        if (spec.Z > 0)
        {
            nri.South = spec.CurrentSpot.indices[spec.X, spec.Z - 1];
        }
        else if (spec.SouthOccupied)
        {
            var southernNeighbor = terrainSpots[spec.C, spec.R - 1];
            nri.South = southernNeighbor.indices[spec.X, spec.EdgeIndicesCount - 2];
        }
        else if (spec.R == 0)
        {
            nri.South = nri.Current;
        }
        else
        {
            return nri;
        }

        //Northern Edge
        if (spec.Z < spec.EdgeIndicesCount - 1)
        {
            nri.North = spec.CurrentSpot.indices[spec.X, spec.Z + 1];
        }
        else if (spec.NorthOccupied)
        {
            var northernNeighbor = terrainSpots[spec.C, spec.R + 1];
            nri.North = northernNeighbor.indices[spec.X, 1];
        }
        else if (spec.R == spec.Rows - 1)
        {
            nri.North = nri.Current;
        }
        else
        {
            return nri;
        }

        nri.AllSidesOccupied = spec.NorthOccupied && spec.SouthOccupied && spec.EastOccupied && spec.WestOccupied;
        nri.OK = true;
        return nri;
    }

    private Vector3 GetInterpolatedVertexPosition(int x, int z, float interpolationStep, TerrainSpot spot)
    {
        var xInterpolant = interpolationStep * x;
        var zInterpolant = interpolationStep * z;
        var northInterpolatedPosition = Vector3.Lerp(spot.controlPointNorthWest, spot.controlPointNorthEast, xInterpolant);
        var southernInterpolatedPosition = Vector3.Lerp(spot.controlPointSouthWest, spot.controlPointSouthEast, xInterpolant);

        return Vector3.Lerp(southernInterpolatedPosition, northInterpolatedPosition, zInterpolant);
    }

    private void RebuildSurfaceMesh(bool fastMode)
    {
        if (am == null)
        {
            return;
        }

        if (mf.sharedMesh != null)
        {
            surfaceMesh = mf.sharedMesh;
        }

        normals.Clear();
        vertices.Clear();
        indices.Clear();

        //Let's walk each point here, and try to maintain a sensible structure of vertices
        ConstructByWalking(fastMode);
        mf.sharedMesh = ProceduralMeshUtilities.GenerateMesh(vertices, indices, surfaceMesh, null, null, vertexData);
        meshCollider.enabled = Application.isPlaying;
        meshCollider.sharedMesh = mf.sharedMesh;
    }

    private void CollectSurfacePoints ()
    {
        if (am == null)
            return;

        if (heightfield == null ||
            (heightfield.GetLength(0) != am.boundsFull.x && heightfield.GetLength(1) != am.boundsFull.z))
            heightfield = new int[am.boundsFull.x, am.boundsFull.z];

        if (surfacePoints == null || (surfacePoints.GetLength(0) != am.boundsFull.x &&
            surfacePoints.GetLength(1) != am.boundsFull.z))
            surfacePoints = new AreaVolumePoint[am.boundsFull.x, am.boundsFull.z];

        ProceduralMeshUtilities.CollectSurfacePoints(am, heightfield, surfacePoints);
    }

    #if PB_MODSDK
    public static bool firstTileCollectPolicy;
    private void CollectSurfacePointsNewAlgo ()
    {
        if (am == null)
        {
            return;
        }

        var boundsX = am.boundsFull.x;
        var boundsZ = am.boundsFull.z;
        if (heightfield == null
            || (heightfield.GetLength (0) != boundsX && heightfield.GetLength (1) != boundsZ))
        {
            heightfield = new int[boundsX, boundsZ];
        }
        else
        {
            Array.Clear (heightfield, 0, heightfield.Length);
        }
        if (surfacePoints == null
            || (surfacePoints.GetLength (0) != boundsX && surfacePoints.GetLength (1) != boundsZ))
        {
            surfacePoints = new AreaVolumePoint[boundsX, boundsZ];
        }
        else
        {
            Array.Clear (surfacePoints, 0, surfacePoints.Length);
        }

        var gridCount = boundsX * boundsZ;
        var arrayLength = gridCount / 16 + 1;
        if (grid == null || grid.Length != arrayLength)
        {
            grid = new int[arrayLength];
        }
        else
        {
            Array.Clear (grid, 0, grid.Length);
        }

        var bounds = am.boundsFull;
        for (var y = am.boundsFull.y - 1; y >= 0; y -= 1)
        {
            var gridBit = 0;
            var gridIndex = 0;
            for (var z = 0; z < boundsZ; z += 1)
            {
                for (var x = 0; x < boundsX; x += 1, gridBit += 2, gridIndex += gridBit >> 5, gridBit &= 0x1F)
                {
                    var terrainBit = 1 << gridBit;
                    var surfaceBit = 1 << (gridBit + 1);
                    if (firstTileCollectPolicy && (grid[gridIndex] & terrainBit) == terrainBit)
                    {
                        continue;
                    }

                    var position = new Vector3Int (x, y, z);
                    var index = AreaUtility.GetIndexFromVolumePosition (position, bounds, skipBoundsCheck: true);
                    var point = am.points[index];
                    if (point.pointState != AreaVolumePointState.Empty)
                    {
                        if (y == 0)
                        {
                            Debug.LogWarningFormat
                            (
                                "Invalid point in layer 0 -- not empty | index: {0} {1} | state: {2} | configuration: {3} | tileset: {4}",
                                index,
                                position,
                                point.pointState,
                                point.spotConfiguration,
                                point.blockTileset
                            );
                        }
                        continue;
                    }

                    if (point.spotConfiguration == TilesetUtility.configurationEmpty)
                    {
                        if ((x == boundsX - 1 || z == boundsZ - 1) && (grid[gridIndex] & terrainBit) == 0)
                        {
                            // Points on the north and east boundaries are weird and need special handling.
                            heightfield[x, z] = y;
                            surfacePoints[x, z] = point;
                            grid[gridIndex] |= terrainBit;
                        }
                        continue;
                    }
                    if ((point.spotConfiguration & TilesetUtility.configurationBitTopSelf) != 0)
                    {
                        Debug.LogWarningFormat
                        (
                            "Invalid configuration for empty point -- point state is empty but top configuration bit is 1 | index: {0} {1} | configuration: {2} | tileset: {3}",
                            index,
                            position,
                            point.spotConfiguration,
                            point.blockTileset
                        );
                        continue;
                    }
                    if ((point.spotConfiguration & TilesetUtility.configurationBitBottomSelf) == 0)
                    {
                        continue;
                    }

                    var isTerrain = AreaManager.IsPointTerrain(point);
                    if (!isTerrain && (grid[gridIndex] & (terrainBit | surfaceBit)) != 0)
                    {
                        continue;
                    }

                    heightfield[x, z] = y;
                    surfacePoints[x, z] = point;
                    grid[gridIndex] |= isTerrain ? terrainBit : surfaceBit;
                }
            }
        }

        for (int z = 0, gridBit = 0, gridIndex = 0; z < boundsZ; z += 1)
        {
            for (var x = 0; x < boundsX; x += 1, gridBit += 2, gridIndex += gridBit >> 5, gridBit &= 0x1F)
            {
                var terrainBit = 1 << gridBit;
                if ((grid[gridIndex] & terrainBit) != terrainBit)
                {
                    continue;
                }
                var spot = surfacePoints[x, z];
                var h = heightfield[x, z];
                var northAccessible = z + 1 < boundsZ;
                var eastAccessible = x + 1 < boundsX;

                if (northAccessible)
                {
                    TryPairNeighbor (spot.pointsInSpot[2], x, z + 1, h);
                }
                if (eastAccessible)
                {
                    TryPairNeighbor (spot.pointsInSpot[1], x + 1, z, h);
                }
                if (northAccessible && eastAccessible)
                {
                    TryPairNeighbor (spot.pointsInSpot[3], x + 1, z + 1, h);
                }
            }
        }
    }

    void TryPairNeighbor (AreaVolumePoint neighbor, int x, int z, int h)
    {
        if (neighbor == null)
        {
            return;
        }
        if (AreaManager.IsPointTerrain(neighbor))
        {
            return;
        }
        if (neighbor.pointState == AreaVolumePointState.Empty && neighbor.spotConfiguration == TilesetUtility.configurationEmpty)
        {
            return;
        }
        if (neighbor.pointState == AreaVolumePointState.Full && neighbor.spotConfiguration == TilesetUtility.configurationFull)
        {
            return;
        }
        if ((neighbor.spotConfiguration & TilesetUtility.configurationBitBottomSelf) == 0)
        {
            return;
        }
        if (neighbor.pointPositionIndex.y > heightfield[x, z])
        {
            return;
        }
        heightfield[x, z] = neighbor.pointPositionIndex.y;
        surfacePoints[x, z] = neighbor;
    }
    #endif

    private void DrawHeightFieldGizmos()
    {
        if (heightfield == null)
            return;

        Gizmos.color = heightFieldColor;

        float blockSize = TilesetUtility.blockAssetSize;
        for (int x = 0; x < am.boundsFull.x; ++x)
        {
            for (int z = 0; z < am.boundsFull.z; ++z)
            {
                Gizmos.color = x == 0 && z <= 2 ? Color.blue : heightFieldColor;
                Gizmos.color = x <= 2 && z == 0 ? Color.red : Gizmos.color;

                Vector3 position =
                    ProceduralMeshUtilities.GetScaledPositionFromHeightField(x, z, blockSize, heightfield);
                Gizmos.DrawWireCube(position, Vector3.one * 0.33f);
            }
        }
    }

    private void DrawSurfacePoints()
    {
        if (surfacePoints == null)
            return;

        Gizmos.color = surfacePointsColor;

        float blockSize = TilesetUtility.blockAssetSize;
        for (int x = 0; x < am.boundsFull.x; ++x)
        {
            for (int z = 0; z < am.boundsFull.z; ++z)
            {
                Gizmos.color = x == 0 && z <= 2 ? Color.blue : surfacePointsColor;
                Gizmos.color = x <= 2 && z == 0 ? Color.red : Gizmos.color;

                var surfacePoint = surfacePoints[x, z];

                if (AreaManager.IsPointTerrain(surfacePoint))
                {
                    Gizmos.color = surfacePointsColor;
                }
                else if (showFiltering)
                {
                    Gizmos.color = filteredSurfacePointColor;
                }

                Vector3 position = am.GetSpotVertexPosition(surfacePoint);
                Gizmos.DrawWireCube(position, Vector3.one * 0.33f);
            }
        }
    }

    private void DrawTerrainSpots()
    {
        int columns = am.boundsFull.x - 1;
        int rows = am.boundsFull.z - 1;

        float blockSize = TilesetUtility.blockAssetSize;
        float halfSize = blockSize * 0.5f;

        for (int c = 0; c < columns; ++c)
        {
            for (int r = 0; r < rows; ++r)
            {
                var terrainSpot = terrainSpots[c, r];

                Gizmos.color = terrainSpot.empty
                    ? terrainSpotEmptyColor
                    : (terrainSpot.flat ? terrainSpotFlatColor : terrainSpotFilledColor);

                Vector3 centerPoint = terrainSpot.controlPointSouthWest + Vector3.right * halfSize + Vector3.forward * halfSize;

                Gizmos.DrawWireCube(centerPoint, Vector3.one * 0.33f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (am == null)
            return;

        if (!drawOnce && !drawContinual)
            return;

        if (drawOnce)
            drawOnce = false;

        if (showHeightField)
        {
            DrawHeightFieldGizmos();
        }

        if (showAreaVolumePoints)
        {
            DrawSurfacePoints();
        }

        if (showTerrainSpots)
        {
            DrawTerrainSpots();
        }
    }

    // Used in the routines to build the mesh vertex array as an optimization
    // to avoid generating garbage.
    readonly NeighborIndexSpec nis = new NeighborIndexSpec ();
    readonly NeighborIndexResult nri = new NeighborIndexResult ();
    readonly AverageHeightComputation heightOp = new AverageHeightComputation ();
    readonly AverageNormalsComputation normalsOp = new AverageNormalsComputation ();
    readonly CurvatureComputation curvatureOp = new CurvatureComputation ();

    class NeighborIndexSpec
    {
        public TerrainSpot CurrentSpot;
        public bool NorthOccupied;
        public bool SouthOccupied;
        public bool EastOccupied;
        public bool WestOccupied;
        public int X;
        public int Z;
        public int R;
        public int C;
        public int Rows;
        public int Columns;
        public int EdgeIndicesCount;
    }

    class NeighborIndexResult
    {
        public bool OK;
        public bool AllSidesOccupied;
        public int Current;
        public int North;
        public int South;
        public int East;
        public int West;
    }

    interface VertexComputation
    {
        void Apply (NeighborIndexResult indexes);
    }

    sealed class AverageHeightComputation : VertexComputation
    {
        public float AverageWeight;
        public List<Vector3> Source;
        public List<Vector3> Destination;

        public void Apply (NeighborIndexResult indexes)
        {
            var northVertex = Source[indexes.North];
            var southVertex = Source[indexes.South];
            var eastVertex = Source[indexes.East];
            var westVertex = Source[indexes.West];

            var northHeight = northVertex.y;
            var southHeight = southVertex.y;
            var eastHeight = eastVertex.y;
            var westHeight = westVertex.y;

            var currentPosition = Source[indexes.Current];
            var averageHeight = (northHeight + southHeight + eastHeight + westHeight) * 0.25f;
            var newPosition = new Vector3
            (
                currentPosition.x,
                Mathf.Lerp (currentPosition.y, averageHeight, AverageWeight),
                currentPosition.z
            );

            Destination[indexes.Current] = newPosition;
        }
    }

    sealed class AverageNormalsComputation : VertexComputation
    {
        public List<Vector3> Vertices;
        public List<Vector4> VertexData;

        public void Apply (NeighborIndexResult indexes)
        {
            //Collect our Neighbors
            var northVertex = Vertices[indexes.North];
            var southVertex = Vertices[indexes.South];
            var eastVertex = Vertices[indexes.East];
            var westVertex = Vertices[indexes.West];

            var currentPosition = Vertices[indexes.Current];
            //NW Tri
            var nwNormal = new Plane (currentPosition, westVertex, northVertex).normal;
            //NE Tri
            var neNormal = new Plane (currentPosition, northVertex, eastVertex).normal;
            //SW Tri
            var swNormal = new Plane (currentPosition, southVertex, westVertex).normal;
            //SE Tri
            var seNormal = new Plane (currentPosition, eastVertex, southVertex).normal;
            //AverageNormal
            var averageNormal = (nwNormal + neNormal + swNormal + seNormal) * 0.25f;
            //Compute final color
            var vertexDataNew = new Vector4 (averageNormal.x, averageNormal.y, averageNormal.z, 0.5f);
            VertexData[indexes.Current] = vertexDataNew;
        }
    }

    sealed class CurvatureComputation : VertexComputation
    {
        public float CurvatureScalar;
        public List<Vector4> VertexData;

        public void Apply (NeighborIndexResult indexes)
        {
            Vector3 currentNormal = VertexData[indexes.Current];
            Vector4 vertexColor;
            if (indexes.AllSidesOccupied)
            {
                Vector3 northNormal = VertexData[indexes.North];
                Vector3 southNormal = VertexData[indexes.South];
                Vector3 eastNormal = VertexData[indexes.East];
                Vector3 westNormal = VertexData[indexes.West];

                var nAngle = Vector3.SignedAngle (currentNormal, northNormal, Vector3.right);
                var sAngle = Vector3.SignedAngle (currentNormal, southNormal, Vector3.left);
                var eAngle = Vector3.SignedAngle (currentNormal, eastNormal, Vector3.back);
                var wAngle = Vector3.SignedAngle (currentNormal, westNormal, Vector3.forward);

                var averageAngle = (nAngle + sAngle + eAngle + wAngle) * 0.25f;
                averageAngle /= 360;
                averageAngle *= CurvatureScalar;
                averageAngle = Mathf.Clamp01 (averageAngle + 0.5f);

                vertexColor = new Vector4 (currentNormal.x, currentNormal.y, currentNormal.z, averageAngle);
            }
            else
            {
                vertexColor = new Vector4 (currentNormal.x, currentNormal.y, currentNormal.z, 0.5f);
            }
            VertexData[indexes.Current] = vertexColor;
        }
    }
}
