using System;
using System.Collections.Generic;
using Area;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

public static class ProceduralMeshUtilities
{
    public static int[,] heightfieldCompressed;

    public static void CollectSurfacePointsFromCompressedArea ()
    {
        var core = DataMultiLinkerCombatArea.GetCurrentLevelDataCore ();
        var dataUnpacked = DataMultiLinkerCombatArea.GetCurrentLevelDataRoot ();
        
        if (dataUnpacked == null || dataUnpacked.points.Length < 8 || core == null || core.bounds.x < 2 || core.bounds.y < 2 || core.bounds.z < 2)
        {
            Debug.LogWarning ("ProceduralMeshUtilities | Failed to collect surface points from area, no loaded data available or point count is too low");
            return;
        }

        var boundsFull = core.bounds;
        int maxDepth = boundsFull.y;

        if (heightfieldCompressed == null || (heightfieldCompressed.GetLength (0) != boundsFull.x && heightfieldCompressed.GetLength (1) != boundsFull.z))
            heightfieldCompressed = new int[boundsFull.x, boundsFull.z];

        for (int x = 0; x < boundsFull.x; ++x)
        {
            for (int z = 0; z < boundsFull.z; ++z)
            {
                var height = maxDepth - 1;
                int iteration = 0;
                
                var posIndex = new Vector3Int (x, 0, z);
                var index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);

                while (true)
                {
                    var pointFull = dataUnpacked.points[index];
                    if (pointFull)
                    {
                        // Backing up one step
                        
                        posIndex = new Vector3Int (x, posIndex.y - 1, z);
                        index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
                        
                        pointFull = dataUnpacked.points[index];
                        height = Mathf.Max (0, posIndex.y - 1);
                        break;
                    }
                        
                    // Get point below
                    posIndex = new Vector3Int (x, Mathf.Clamp (posIndex.y + 1, 0, boundsFull.y - 1), z);
                    index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
                    pointFull = dataUnpacked.points[index];

                    iteration += 1;
                    if (iteration > maxDepth)
                    {
                        // Debug.Log ("Breaking out of while loop, something is wrong");
                        break;
                    }
                }

                heightfieldCompressed[posIndex.x, posIndex.z] = height;
            }
        }
    }
    
    public static Vector3 GetScaledPositionFromHeightField (int x, int z, float blockSize, int[,] heightfield)
    {
        //Replace this with your fancy code that gets points in more detail maybe?
        return new Vector3 (x * blockSize, heightfield[x, z] * -blockSize - blockSize * 0.5f, z * blockSize);
    }

    public static void CollectSurfacePoints (AreaManager am, int[,] heightField = null, AreaVolumePoint[,] volumePoints = null)
    {
        if (am == null || am.points.Count < 8)
        {
            Debug.LogWarning ("Area Manager is null, or point count is too low");
            return;
        }

        var boundsFull = am.boundsFull;
        int size = boundsFull.x * boundsFull.z;
        int maxDepth = am.boundsFull.y;

        if (heightField == null || (heightField.GetLength (0) != boundsFull.x && heightField.GetLength (1) != boundsFull.z))
        {
            heightField = null;
        }

        if (volumePoints == null || (volumePoints.GetLength (0) != boundsFull.x && volumePoints.GetLength (1) != boundsFull.z))
        {
            volumePoints = null;
        }

        bool heightFieldAvailable = heightField != null;
        bool volumePointsAvailable = volumePoints != null;

        if (!heightFieldAvailable && !volumePointsAvailable)
        {
            //Nothing to do, abort
            return;
        }

        // Debug.Log("Size: " + size + " | Bounds: " + boundsFull);
        int depthMinusOne = boundsFull.y - 1;
        
        for (int x = 0; x < boundsFull.x; ++x)
        {
            for (int z = 0; z < boundsFull.z; ++z)
            {
                var posIndex = new Vector3Int (x, 0, z);
                var height = depthMinusOne;
                AreaVolumePoint pointSurface = null;

                var index = TilesetUtility.GetIndexFromVolumePosition (posIndex, boundsFull);
                var pointCurrent = am.points[index];
                int iteration = 0;
                    
                while (true)
                {
                    bool full = pointCurrent.pointState != AreaVolumePointState.Empty;
                    if (full)
                    {
                        // Backing up one step
                        pointCurrent = pointCurrent.pointsWithSurroundingSpots[3];
                        if (pointCurrent != null)
	                        pointSurface = pointCurrent;
	                    
                        break;
                    }
                        
                    // Get point above
                    pointCurrent = pointCurrent.pointsInSpot[4];
                    if (pointCurrent == null)
                        break;

                    iteration += 1;
                    if (iteration > maxDepth)
                    {
                        // Debug.Log ("Breaking out of while loop, something is wrong");
                        break;
                    }
                }

                if (pointSurface != null)
                    height = pointSurface.pointPositionIndex.y;

                if (heightFieldAvailable)
                    heightField[posIndex.x, posIndex.z] = height;

                if (volumePointsAvailable && pointSurface != null)
                    volumePoints[posIndex.x, posIndex.z] = pointSurface;
            }
        }
    }

    private static bool IsTerrainInvolved (AreaVolumePoint point)
    {
        bool terrainTilesetDetected = false;
        for (int s = 0; s < 4; ++s)
        {
            AreaVolumePoint pointWithNeighbourSpot = point.pointsWithSurroundingSpots[s];
            if (pointWithNeighbourSpot == null)
                continue;

            if (pointWithNeighbourSpot.blockTileset == AreaTilesetHelper.idOfTerrain)
            {
                terrainTilesetDetected = true;
                continue;
            }
        }
        
        return terrainTilesetDetected;
    }

    private static List<Vector3> tempGrid = new List<Vector3> ();

    public static void CreateMeshPatch (MeshFilter targetFilter, List<BoundingLine> lines, int subdivisionCount, AnimationCurve smoothingCurve = null, PlanarCurves planarCurves = null)
    {
        tempGrid.Clear ();

        //Convert the lines to a regular grid
        SubdivideBoundingLines (lines, tempGrid, subdivisionCount, smoothingCurve);

        //Post Process the grid we've created, and offset the interior points
        int columns = lines.Count;
        int rows = subdivisionCount + 1;

        SetGridByCurves (tempGrid, columns, rows, planarCurves);

        //Create the mesh
        Mesh convertedMesh = ConvertGridToMesh (tempGrid, lines.Count, subdivisionCount + 1, targetFilter.sharedMesh);
        
        //Assign the mesh
        targetFilter.sharedMesh = convertedMesh;

        //Clear working data
        tempGrid.Clear ();
    }

    public static void SubdivideLine (Vector3 origin, Vector3 destination, List<Vector3> output, int subdivisions = 1, AnimationCurve interpolationCurve = null)
    {
        if (output == null)
        {
            Debug.LogWarning ("Must provide a valid list to output the points to");
            return;
        }

        subdivisions = Mathf.Max (1, subdivisions);

        float interpolationStep = 1f / subdivisions;

        float currentInterpolator = 0f;
        output.Add (origin);
        float elevationDifference = destination.y - origin.y;
        float horizontalDistance = Vector3.Distance (origin.Flatten (), destination.Flatten ());
        Vector3 lineDirection = Utilities.GetDirection (origin.Flatten (), destination.Flatten ());

        for (int subD = 0; subD < subdivisions; ++subD)
        {
            currentInterpolator += interpolationStep;

            Vector3 newPoint = origin;

            if (interpolationCurve == null)
            {
                newPoint = Vector3.Lerp (origin, destination, currentInterpolator);
            }
            else
            {
                float modifiedInterpolator = interpolationCurve.Evaluate (currentInterpolator);

                //Apply interpolation along the direction of our line
                newPoint = origin + (lineDirection * horizontalDistance * currentInterpolator);

                //Interpolation along elevation
                newPoint += new Vector3 (0, modifiedInterpolator * elevationDifference, 0);
            }

            output.Add (newPoint);
        }
    }

    /// <summary>
    /// Given a series of bounding points, we'll create a regular grid of sub divisions with interpolation 
    /// </summary>
    public static void SubdivideBoundingLines (List<BoundingLine> source, List<Vector3> output, int subdivisions = 1, AnimationCurve innerInterpolationCurve = null)
    {
        if (source == null || source.Count <= 1)
        {
            Debug.LogWarning ("Cannot create a grid, source point count too low or null");
            return;
        }

        foreach (var point in source)
        {
            SubdivideLine (point.origin, point.destination, output, subdivisions, innerInterpolationCurve);
        }
    }

    public static void GenerateGridFromBoundingLines (List<BoundingLine> boundingLines, List<Vector3> outputPoints, int subdivisionCount, PlanarCurves gridSmoothing = null, AnimationCurve lineSmoothing = null, bool preserveOrigins = false, bool preserveDestinations = false)
    {
        //Convert the lines to a regular grid
        SubdivideBoundingLines (boundingLines, outputPoints, subdivisionCount, lineSmoothing);

        //Post Process the grid we've created, and offset the interior points
        int columns = boundingLines.Count;
        int rows = subdivisionCount + 1;

        SetGridByCurves (outputPoints, columns, rows, gridSmoothing, preserveOrigins, preserveDestinations);
    }

    public static void SetGridByCurves (List<Vector3> gridPoints, int columns, int rows, PlanarCurves curves, bool preserveOriginRow = false, bool preserveFinalRow = false)
    {
        //Nothing to do, skip interpolation
        if (curves == null)
            return;

        if (gridPoints.Count != columns * rows)
        {
            Debug.LogWarning ($"Cannot interpolate this grid, columns : {columns} and rows : {rows} do not match count, got : {columns * rows}, expected {gridPoints.Count}");
            return;
        }

        for (int col = 0; col < columns; ++col)
        {
            for (int row = 0; row < rows; ++row)
            {
                if (preserveOriginRow && row == 0)
                    continue;

                if (preserveFinalRow && row == rows - 1)
                    continue;

                int index1D = row + (col * rows);

                gridPoints[index1D] = SetPointByCurves (gridPoints[index1D], curves);
            }
        }
    }

    public static Vector3 SetPointByCurves (Vector3 sourcePoint, PlanarCurves curves)
    {
        return curves.SetPointDepth (sourcePoint);
    }

    public static void GetEdgeFromBoundingLines (List<BoundingLine> inputLines, List<Vector3> outputPoints, bool outerEdge = true)
    {
        if (outerEdge)
        {
            foreach (var line in inputLines)
            {
                outputPoints.Add (line.destination);
            }
        }
        else
        {
            foreach (var line in inputLines)
            {
                outputPoints.Add (line.origin);
            }
        }
    }

    public static void GenerateMeshPatch (List<Vector3> gridPoints, int lineCount, int subdivisionCount, MeshFilter targetFilter)
    {
        //Create the mesh
        Mesh convertedMesh = ConvertGridToMesh (gridPoints, lineCount, subdivisionCount + 1, targetFilter.sharedMesh);
        //Assign the mesh
        targetFilter.sharedMesh = convertedMesh;
        // Force motion vector generation to Object for Motion Blur to work properly
        var mr = targetFilter.gameObject.GetComponent <MeshRenderer> ();
        if (mr != null)
            mr.motionVectorGenerationMode = MotionVectorGenerationMode.Object;
    }

    public static Mesh ConvertGridToMesh (List<Vector3> gridPoints, int columns, int rows, Mesh targetMesh = null, int subMesh = 0)
    {
        if (gridPoints == null)
        {
            return null;
        }

        if (gridPoints.Count < 4)
        {
            Debug.LogWarning ("Improper configuration of grid points");
            return null;
        }

        //Each point on the grid represents one vertex, they are ordered for fast triangle locality / access
        List<int> indices = new List<int> ();

        int quadCount = 0;
        int triCount = 0;

        //Given a 1 dimensional array arranged by column ordered grid points, create a quad mesh patch from it
        /*   col row ----->
         *    |  A - C  
         *    |  | / | 
         *    |  B - D 
         */

        //Go by Column -> to row
        for (int col = 0; col < columns - 1; ++col)
        {
            for (int row = 0; row < rows - 1; ++row)
            {
                int a = row + (col * rows);
                int b = a + 1;
                int c = row + ((col + 1) * rows);
                int d = c + 1;

                if (a >= gridPoints.Count || b >= gridPoints.Count || c >= gridPoints.Count || d >= gridPoints.Count)
                {
                    Debug.LogWarning ("Logical error");
                    continue;
                }

                //Clockwise winding
                indices.Add (a);
                indices.Add (c);
                indices.Add (b);

                indices.Add (b);
                indices.Add (c);
                indices.Add (d);

                quadCount++;
                triCount += 2;
            }
        }

        // Debug.Log($"Generated Mesh from grid of {gridPoints.Count} ({columns}:{rows}) total triangles:{triCount}, total quads:{quadCount}");

        return GenerateMesh (gridPoints, indices, targetMesh);
    }

    private static List<Color32> tempColors = new List<Color32> ();
    private static Color32 colorFallback = new Color (1f, 1f, 1f, 0.5f);

    public static Mesh GenerateMesh
    (
        List<Vector3> verts,
        List<int> tris,
        Mesh targetMesh = null,
        List<Vector3> normals = null,
        List<Vector4> tangents = null,
        List<Vector4> vertexData = null
    )
    {
        if (verts == null || verts.Count <= 3)
            return targetMesh;

        if (tris == null || tris.Count < 1)
            return targetMesh;

        var vertexCount = verts.Count;
        if (targetMesh == null)
            targetMesh = new Mesh ();

        targetMesh.Clear ();
        targetMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        targetMesh.SetVertices (verts);
        targetMesh.SetIndices (tris, MeshTopology.Triangles, 0, true);

        if (normals == null)
            targetMesh.RecalculateNormals ();
        else
            targetMesh.normals = normals.ToArray ();

        if (tangents == null)
            targetMesh.RecalculateTangents ();
        else
            targetMesh.tangents = tangents.ToArray ();

        if (vertexData != null && vertexData.Count == vertexCount)
        {
            tempColors.Clear ();
            foreach (var color in vertexData)
            {
                tempColors.Add (new Color32
                (
                    color.x.GetByte (),
                    color.y.GetByte (),
                    color.z.GetByte (),
                    color.w.GetByte ()
                ));
            }

            targetMesh.SetColors (tempColors);
        }
        else
        {
            tempColors.Clear ();
            for (int i = 0; i < vertexCount; ++i)
                tempColors.Add (colorFallback);
                
            targetMesh.SetColors (tempColors);
        }

        return targetMesh;
    }

    public static byte GetByte (this float f)
    {
        f = Mathf.Floor (f >= 1.0f ? 255f : f * 256.0f);

        return (byte) f;
    }
}

public struct BoundingLine
{
    public Vector3 origin;
    public Vector3 destination;

    public BoundingLine (BoundingLine source)
    {
        origin = source.origin;
        destination = source.destination;
    }

    public BoundingLine (Vector3 origin, Vector3 destination)
    {
        this.origin = origin;
        this.destination = destination;
    }
}

[Serializable]
public class PlanarCurves
{
    [LabelText("Northern Curve (Z+)")]
    public AnimationCurve northernCurve;
    [LabelText("Southern Curve (Z-)")]
    public AnimationCurve southernCurve;
    [LabelText("Eastern Curve (X+)")]
    public AnimationCurve easternCurve;
    [LabelText("Western Curve (X-)")]
    public AnimationCurve westernCurve;

    public float groundHeight;
    public Bounds boundsCurve;
    public Bounds boundsLevel;

    public Vector3 OffsetPointDepthByCurves (Vector3 sourcePoint)
    {
        float heightOffset = SamplePoint (sourcePoint);
        return new Vector3 (sourcePoint.x, sourcePoint.y + heightOffset, sourcePoint.z);
    }

    public Vector3 SetPointDepth (Vector3 sourcePoint)
    {
        float newHeight = SamplePoint (sourcePoint);
        return new Vector3 (sourcePoint.x, newHeight, sourcePoint.z);
    }

    public float SamplePoint (Vector3 sourcePoint, bool log = false)
    {
        float ewInterpolantOuter = GetEastWestInterpolant (sourcePoint.x);
        float nsInterpolantOuter = GetNorthSouthInterpolant (sourcePoint.z);

        float ewInterpolantInner = Mathf.InverseLerp (boundsLevel.min.x, boundsLevel.max.x, sourcePoint.x);
        float nsInterpolantInner = Mathf.InverseLerp (boundsLevel.min.z, boundsLevel.max.z, sourcePoint.z);

        // ewInterpolant = EaseInOutSine(0, 1, ewInterpolant);
        // nsInterpolant = EaseInOutSine(0, 1, nsInterpolant);
        float northSample = northernCurve.Evaluate (ewInterpolantOuter);
        float southSample = southernCurve.Evaluate (ewInterpolantOuter);
        float snSample = Mathf.Lerp (southSample, northSample, nsInterpolantInner);

        float eastSample = easternCurve.Evaluate (nsInterpolantOuter);
        float westSample = westernCurve.Evaluate (nsInterpolantOuter);
        float ewSample = Mathf.Lerp (westSample, eastSample, ewInterpolantInner);

        //float finalHeight = Mathf.Lerp(0, bounds.max.y, snSample + ewSample);

        float finalHeight = (snSample + ewSample) * boundsCurve.max.y * 0.5f - 1.5f + groundHeight;
        if (log)
            Debug.Log ($"EW-OI / NS-OI: {ewInterpolantOuter:F2} / {nsInterpolantOuter:F2} | EW-II / NS-II: {ewInterpolantInner:F2} / {nsInterpolantInner:F2}");

        return finalHeight;
    }

    private static float EaseInOutSine (float start, float end, float val)
    {
        end -= start;
        return (-end * 0.5f) * (Mathf.Cos (Mathf.PI * val / 1) - 1) + start;
    }

    private float GetEastWestInterpolant (float x)
    {
        return Mathf.InverseLerp (boundsCurve.min.x, boundsCurve.max.x, x);
    }

    private float GetNorthSouthInterpolant (float z)
    {
        return Mathf.InverseLerp (boundsCurve.min.z, boundsCurve.max.z, z);
    }
}