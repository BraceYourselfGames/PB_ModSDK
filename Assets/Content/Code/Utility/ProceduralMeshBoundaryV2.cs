using System;
using System.Collections.Generic;
using Area;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralMeshBoundaryV2 : MonoBehaviour
{
    public enum Direction
    {
        North, //Vector3.forward
        South, //Vector3.back
        East, //Vector3.right,
        West, //Vector3.left
    }
    
    private const string gizmoGroup = "Gizmo Controls";
    private const string referenceGroup = "References";

    [Header("General Settings")]
    public AreaManager am;
    public ProceduralMeshTerrainV2 terrain;
    
    [SerializeField, HideInInspector]
    private bool collidersUsedInternal = false;
    [ShowInInspector]
    public bool collidersUsed { get { return collidersUsedInternal; } set { collidersUsedInternal = value; SetCollidersUsed (value); } }
    
    public float innerSkirtSize = 30;
    public float outerSkirtSize;

    [Header("Skirt settings")]
    public int skirtSubdivisionCount = 0;
    
    public PlanarCurves interpolationCurves;
    public AnimationCurve innerSkirtSmoothingCurve;

    public List<Collider> collidersRuntime = new List<Collider> ();

    //public AnimationCurve skirtBlendingCurve;
    
    [FoldoutGroup (referenceGroup)] 
    //[Header("Terrain Floor Skirt Mesh Filters")] 
    //public MeshFilter terrainFloorSkirt;
    
    //[FoldoutGroup (referenceGroup)] 
    //public MeshFilter terrainInnerSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    [Header("Edge Skirt Mesh Filters")]
    public MeshFilter northSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter southSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter eastSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter westSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    [Header("Outer Skirt Mesh Filters")]
    public MeshFilter northOuterSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter northEastSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter northWestSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter southOuterSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter southWestSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter southEastSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter eastOuterSkirt;
    
    [FoldoutGroup (referenceGroup)] 
    public MeshFilter westOuterSkirt;

    

    [FoldoutGroup (gizmoGroup)] 
    public bool collectDebugData = false;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool showGridPoints;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool drawOnce = false;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool drawContinual = false;
    
    [FoldoutGroup (gizmoGroup)] 
    public Color pointColor;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool showHeightfield;
    
    [FoldoutGroup (gizmoGroup)] 
    public Color heightFieldColor;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool showBoundingLines;
    
    [FoldoutGroup (gizmoGroup)] 
    public Color boundingPointColor;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool showCurveBounds;
    
    [FoldoutGroup (gizmoGroup)] 
    public Color curveBoundsColor;
    
    [FoldoutGroup (gizmoGroup)] 
    public Color levelBoundsColor;
    
    [FoldoutGroup (gizmoGroup)] 
    public bool showSourcePoint;
    
    [FoldoutGroup (gizmoGroup)] 
    public Color sourcePointColor;

    [NonSerialized] private List<BoundingLine> skirtDebugLines = new List<BoundingLine>();
    [NonSerialized] private List<Vector3> skirtDebugGrid = new List<Vector3>();

    [NonSerialized] private int[,] heightfield;
    [NonSerialized] private bool[,] holeMask;

    [OnValueChanged ("SourcePointUpdate")]
    public Vector3 sourcePointTest;

    [ReadOnly]
    public float sourcePointHeight; 
    
    //Temporary and scratch data arrays (These are recycled to reduce memory thrashing so be careful of usage)
    [NonSerialized] private List<BoundingLine> northBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> southBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> eastBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> westBoundingLines = new List<BoundingLine>();
    
    [NonSerialized] private List<BoundingLine> northOuterBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> southOuterBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> eastOuterBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> westOuterBoundingLines = new List<BoundingLine>();
    
    [NonSerialized] private List<BoundingLine> southWestOuterQuadrantLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> southEastOuterQuadrantLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> northWestOuterQuadrantLines = new List<BoundingLine>();
    [NonSerialized] private List<BoundingLine> northEastOuterQuadrantLines = new List<BoundingLine>();
    
    [NonSerialized] private List<BoundingLine> cornerBoundingLines = new List<BoundingLine>();
    [NonSerialized] private List<Vector3> skirtGrid = new List<Vector3>();
    [NonSerialized] private List<Vector3> skirtEdge = new List<Vector3>();

    [Button ("Toggle Colliders", ButtonSizes.Medium), ButtonGroup ("A")]
    private void ToggleCollidersUsed () =>
        collidersUsed = !collidersUsed;
    
    private void SetCollidersUsed (bool collidersUsed)
    {    
        //SetColliderUsed (terrainFloorSkirt, collidersUsed);
        //SetColliderUsed (terrainInnerSkirt, collidersUsed);
        
        SetColliderUsed (northSkirt, collidersUsed);
        SetColliderUsed (southSkirt, collidersUsed);
        SetColliderUsed (eastSkirt, collidersUsed);
        SetColliderUsed (westSkirt, collidersUsed);
    
        SetColliderUsed (northOuterSkirt, collidersUsed);
        SetColliderUsed (northEastSkirt, collidersUsed);
        SetColliderUsed (northWestSkirt, collidersUsed);
        
        SetColliderUsed (southOuterSkirt, collidersUsed);
        SetColliderUsed (southWestSkirt, collidersUsed);
        SetColliderUsed (southEastSkirt, collidersUsed);
        
        SetColliderUsed (eastOuterSkirt, collidersUsed);
        SetColliderUsed (westOuterSkirt, collidersUsed);
    }

    private void SetColliderUsed (MeshFilter mf, bool colliderUsed)
    {
        if (mf == null || mf.sharedMesh == null)
            return;

        var mc = mf.gameObject.GetComponent<MeshCollider> ();
        if (mc != null)
            mc.enabled = colliderUsed;
    }
    
    private void RefreshColliders ()
    {
        //RefreshCollider (terrainFloorSkirt);
        //RefreshCollider (terrainInnerSkirt);
        
        RefreshCollider (northSkirt);
        RefreshCollider (southSkirt);
        RefreshCollider (eastSkirt);
        RefreshCollider (westSkirt);
    
        RefreshCollider (northOuterSkirt);
        RefreshCollider (northEastSkirt);
        RefreshCollider (northWestSkirt);
        
        RefreshCollider (southOuterSkirt);
        RefreshCollider (southWestSkirt);
        RefreshCollider (southEastSkirt);
        
        RefreshCollider (eastOuterSkirt);
        RefreshCollider (westOuterSkirt);
    }
    
    private void RefreshCollider (MeshFilter mf)
    {
        if (mf == null || mf.sharedMesh == null)
            return;

        var mc = mf.gameObject.GetComponent<MeshCollider> ();
        if (mc == null)
            mc = mf.gameObject.AddComponent<MeshCollider> ();

        mc.sharedMesh = null;
        mc.sharedMesh = mf.sharedMesh;
    }

    public void DestroyMeshes ()
    {
        //DestroyMeshFilterContent (terrainFloorSkirt);
        //DestroyMeshFilterContent (terrainInnerSkirt);
        
        DestroyMeshFilterContent (northSkirt);
        DestroyMeshFilterContent (southSkirt);
        DestroyMeshFilterContent (eastSkirt);
        DestroyMeshFilterContent (westSkirt);
        
        DestroyMeshFilterContent (northOuterSkirt);
        DestroyMeshFilterContent (northEastSkirt);
        DestroyMeshFilterContent (northWestSkirt);
        DestroyMeshFilterContent (southOuterSkirt);
        DestroyMeshFilterContent (southWestSkirt);
        DestroyMeshFilterContent (southEastSkirt);
        DestroyMeshFilterContent (eastOuterSkirt);
        DestroyMeshFilterContent (westOuterSkirt);

        if (terrain != null)
            DestroyMeshFilterContent (terrain.mf);
    }

    private void DestroyMeshFilterContent (MeshFilter filter)
    {
        if (filter != null)
        {
            var sm = filter.sharedMesh;
            if (sm != null)
            {
                filter.sharedMesh = null;
                DestroyImmediate (sm);
            }
        }
    }

    private void SourcePointUpdate ()
    {
        if (am != null && interpolationCurves != null)
            sourcePointHeight = interpolationCurves.SamplePoint (sourcePointTest, true);
    }
    
    public void RebuildFromArea ()
    {
        GenerateHeightfieldFromArea ();
        TestSkirtPointCreation ();
    }

    public void SetRuntimeCollidersActive (bool active)
    {
        if (collidersRuntime != null)
        {
            foreach (var col in collidersRuntime)
            {
                if (col != null)
                    col.enabled = active;
            }
        }
    }

    [Button("Update Bounds", ButtonSizes.Medium), ButtonGroup ("A")]
    public void UpdateInterpolationBounds ()
    {
        if (am != null && interpolationCurves != null)
        {
            var c = new Vector3
            (
                am.boundsFull.x * 0.5f, 
                am.boundsFull.y * -0.5f, 
                am.boundsFull.z * 0.5f
            );

            c *= TilesetUtility.blockAssetSize;
            interpolationCurves.boundsCurve.center = c;
            interpolationCurves.boundsLevel.center = c;
            
            var e = new Vector3
            (
                am.boundsFull.x * 0.5f, 
                am.boundsFull.y * 0.5f, 
                am.boundsFull.z * 0.5f
            );
            e *= TilesetUtility.blockAssetSize;
            
            interpolationCurves.boundsLevel.extents = e;
        }
    }

    [Button ("Generate Skirts", ButtonSizes.Large), ButtonGroup]
    public void TestSkirtPointCreation()
    {
        if (skirtDebugLines == null)
            skirtDebugLines = new List<BoundingLine>();
        else
            skirtDebugLines.Clear();

        if (skirtDebugGrid == null)
            skirtDebugGrid = new List<Vector3>();
        else
            skirtDebugGrid.Clear();
        
        //Update the interpolation bounds center point
        UpdateInterpolationBounds();

        //Get the points from the Area
        GetSkirtLinesFromArea();
        
        RefreshColliders ();
    }

    [Button ("Generate Heightfield from Area", ButtonSizes.Large), ButtonGroup]
    private void GenerateHeightfieldFromArea()
    {
        if(am == null)
            return;
        
        if(heightfield == null || (heightfield.GetLength(0) != am.boundsFull.x && heightfield.GetLength(1) != am.boundsFull.z))
            heightfield = new int[am.boundsFull.x, am.boundsFull.z];
        
        ProceduralMeshUtilities.CollectSurfacePoints(am, heightfield);
    }

    private void SetCornerBoundingLines(List<Vector3> inputPoints, List<BoundingLine> outputLines, Vector3 offset)
    {
        //Corner bounding lines are a bit of a special case, the start points need to match so they can be sewn up seamlessly
        for(int i = 0; i < inputPoints.Count; ++i)
        {
            Vector3 startPoint = inputPoints[i];
            
            Vector3 destinationPoint = ProceduralMeshUtilities.SetPointByCurves(startPoint + offset, interpolationCurves);

            outputLines.Add(new BoundingLine(startPoint, destinationPoint));
        }
    }

    private void ClearLines()
    {
        westBoundingLines.Clear();
        northBoundingLines.Clear();
        southBoundingLines.Clear();
        eastBoundingLines.Clear();
        
        northOuterBoundingLines.Clear();
        southOuterBoundingLines.Clear();
        eastOuterBoundingLines.Clear();
        westOuterBoundingLines.Clear();
        
        southWestOuterQuadrantLines.Clear();
        southEastOuterQuadrantLines.Clear();
        northWestOuterQuadrantLines.Clear();
        northEastOuterQuadrantLines.Clear();
        
        cornerBoundingLines.Clear ();
        skirtEdge.Clear ();
        skirtGrid.Clear ();
    }
    
    /// <summary>
    /// This is used to create our bounding terrain skirts, it will first construct the interior layer bounding lines, which are extruded out to the horizon
    /// From there, it will generate patches of meshes for the outer skirt to fill in the blanks in the level's skirt terrain
    /// </summary>
    /// <param name="blockSize"></param>
    /// <param name="depthOffset"></param>
    private void GenerateTerrainSkirts()
    {
        ClearLines();
        
        //Order of operations matters here, since we're extracting data from previously created bounding lines
        GenerateInnerSkirtBoundingLines();

        //Outer skirts are dependent on inner skirt data being ready
        GenerateOuterSkirtBoundingLines();
        
        //Final outer quadrant generation
        GenerateOuterQuadrantBoundingLines();

        //Final meshing
        ConvertGridsToMeshes();
        
        //Cleanup
        ClearLines();
        skirtGrid.Clear();
        skirtEdge.Clear();
        
    }

    private void GenerateInnerSkirtBoundingLines()
    {
        //Generate the Eastern and Western Skirts Bounding lines first, since we use them in order
        
        //East Inner Skirt
        skirtEdge.Clear();
        
        GetTerrainEdge (skirtEdge, Direction.East, terrain);
        GetTerrainBoundingLines(skirtEdge, eastBoundingLines, Vector3.right * innerSkirtSize);
        
        //Western Inner Skirt
        skirtEdge.Clear();
        
        GetTerrainEdge (skirtEdge, Direction.West, terrain);
        GetTerrainBoundingLines(skirtEdge, westBoundingLines, Vector3.left * innerSkirtSize);
        
        //North Inner Skirt
        
        //Extract the boundary edges, we'll need this for sewing up the corners
        BoundingLine northEastCornerLine = new BoundingLine(eastBoundingLines[eastBoundingLines.Count - 1]);
        BoundingLine northWestCornerLine = new BoundingLine(westBoundingLines[0]);
        
        //North East InnerCorner
        skirtEdge.Clear();
        
        ProceduralMeshUtilities.SubdivideLine(northEastCornerLine.origin,  northEastCornerLine.destination, skirtEdge, skirtSubdivisionCount, innerSkirtSmoothingCurve);
        //Invert the order
        skirtEdge.Reverse();
        SetCornerBoundingLines(skirtEdge, cornerBoundingLines, Vector3.forward * innerSkirtSize);
        
        northBoundingLines.AddRange(cornerBoundingLines);
        
        //Pop the last line so we can seamlessly stitch the corners
        northBoundingLines.RemoveAt(northBoundingLines.Count - 1);
        cornerBoundingLines.Clear();
        
        //North Inner Terrain Edge
        skirtEdge.Clear();
        GetTerrainEdge (skirtEdge, Direction.North, terrain);
        GetTerrainBoundingLines(skirtEdge, northBoundingLines, Vector3.forward * innerSkirtSize);
        
        //Pop the last line so we can seamlessly stitch the corners
        northBoundingLines.RemoveAt(northBoundingLines.Count - 1);
        
        //North West InnerCorner
        skirtEdge.Clear();
        
        ProceduralMeshUtilities.SubdivideLine(northWestCornerLine.origin,  northWestCornerLine.destination, skirtEdge, skirtSubdivisionCount, innerSkirtSmoothingCurve);
        SetCornerBoundingLines(skirtEdge, cornerBoundingLines, Vector3.forward * innerSkirtSize);
        
        northBoundingLines.AddRange(cornerBoundingLines);
        cornerBoundingLines.Clear();
        
        //South Inner Skirt
        BoundingLine southWestCornerLine = new BoundingLine(westBoundingLines[westBoundingLines.Count - 1]);
        BoundingLine southEastCornerLine = new BoundingLine (eastBoundingLines[0]);
        

        //South West Inner Corner
        skirtEdge.Clear();
        
        ProceduralMeshUtilities.SubdivideLine(southWestCornerLine.origin,  southWestCornerLine.destination, skirtEdge, skirtSubdivisionCount, innerSkirtSmoothingCurve);
        skirtEdge.Reverse();
        SetCornerBoundingLines(skirtEdge, cornerBoundingLines, Vector3.back * innerSkirtSize);

        
        southBoundingLines.AddRange(cornerBoundingLines);
        cornerBoundingLines.Clear();
        
        //Pop the last line so we can seamlessly stitch the corners
        southBoundingLines.RemoveAt(southBoundingLines.Count -1);

        //South Terrain Inner Edge
        skirtEdge.Clear();
        GetTerrainEdge(skirtEdge, Direction.South, terrain);
        GetTerrainBoundingLines(skirtEdge, southBoundingLines, Vector3.back * innerSkirtSize);
        
        //Pop the last line so we can seamlessly stitch the corners
        southBoundingLines.RemoveAt(southBoundingLines.Count -1);
        
        //South East Inner Corner
        skirtEdge.Clear();
        
        ProceduralMeshUtilities.SubdivideLine(southEastCornerLine.origin, southEastCornerLine.destination, skirtEdge, skirtSubdivisionCount, innerSkirtSmoothingCurve);
        SetCornerBoundingLines(skirtEdge, cornerBoundingLines, Vector3.back * innerSkirtSize);
        
        southBoundingLines.AddRange(cornerBoundingLines);

        cornerBoundingLines.Clear();
    }

    private void GenerateOuterSkirtBoundingLines()
    {
        //North Outer Skirt
        //First, let's extract the edge from the inner bounding lines
        skirtEdge.Clear();

        ProceduralMeshUtilities.GetEdgeFromBoundingLines(northBoundingLines, skirtEdge);
        GenerateBoundingLines(skirtEdge, northOuterBoundingLines, Vector3.forward * outerSkirtSize);


        //South Outer Skirt
        //First, let's extract the edge from the inner bounding lines
        skirtEdge.Clear();

        ProceduralMeshUtilities.GetEdgeFromBoundingLines(southBoundingLines, skirtEdge);
        GenerateBoundingLines(skirtEdge, southOuterBoundingLines, Vector3.back * outerSkirtSize);

        
        //East Outer Skirt
        //First, let's extract the edge from the inner bounding lines
        skirtEdge.Clear();
        
        //Generate SouthEast Outer Corner
        BoundingLine southEastOuterCorner = southBoundingLines[southBoundingLines.Count - 1];
        ProceduralMeshUtilities.SubdivideLine(southEastOuterCorner.destination, southEastOuterCorner.origin, skirtEdge, skirtSubdivisionCount);
        
        //Remove Duplicate
        skirtEdge.RemoveAt(skirtEdge.Count - 1);
        
        //South to North Stretch
        ProceduralMeshUtilities.GetEdgeFromBoundingLines(eastBoundingLines, skirtEdge);
        
        //Remove Duplicate
        skirtEdge.RemoveAt(skirtEdge.Count - 1);
        
        //Generate North East Outer Corner
        BoundingLine northEastOuterCorner = northBoundingLines[0];
        ProceduralMeshUtilities.SubdivideLine(northEastOuterCorner.origin, northEastOuterCorner.destination, skirtEdge, skirtSubdivisionCount);

        //Final Extrusion
        GenerateBoundingLines(skirtEdge, eastOuterBoundingLines, Vector3.right * outerSkirtSize);

        //West Outer Skirt
        //First, let's extract the edge from the inner bounding lines
        skirtEdge.Clear();
        
        //Generate NorthWest Outer Corner
        BoundingLine northWestOuterCorner = northBoundingLines[northBoundingLines.Count - 1];
        ProceduralMeshUtilities.SubdivideLine(northWestOuterCorner.destination, northWestOuterCorner.origin, skirtEdge, skirtSubdivisionCount);
        
        //Remove Duplicate
        skirtEdge.RemoveAt(skirtEdge.Count - 1);

        //North to South Stretch
        ProceduralMeshUtilities.GetEdgeFromBoundingLines(westBoundingLines, skirtEdge);
        
        //Remove duplicate
        skirtEdge.RemoveAt(skirtEdge.Count - 1);
        
        BoundingLine southWestOuterCorner = southBoundingLines[0];
        ProceduralMeshUtilities.SubdivideLine(southWestOuterCorner.origin, southWestOuterCorner.destination, skirtEdge, skirtSubdivisionCount);

        //Final extrusion
        GenerateBoundingLines(skirtEdge, westOuterBoundingLines, Vector3.left * outerSkirtSize);
    }

    private void GenerateOuterQuadrantBoundingLines()
    {
        //Outer Quadrants
        
        //South West Outer Quadrant
        skirtEdge.Clear();

        Vector3 innerSkirtEdge = (Vector3.left * innerSkirtSize) + (Vector3.back * innerSkirtSize);
        Vector3 outerSkirtEdge = innerSkirtEdge + (Vector3.left * outerSkirtSize); 
        
        BoundingLine southWestOuterQuadrant = new BoundingLine(outerSkirtEdge, innerSkirtEdge);
        ProceduralMeshUtilities.SubdivideLine(southWestOuterQuadrant.origin, southWestOuterQuadrant.destination, skirtEdge, skirtSubdivisionCount);
        GenerateBoundingLines(skirtEdge, southWestOuterQuadrantLines, Vector3.back * outerSkirtSize);
        
        
        //South East Outer Quadrant
        skirtEdge.Clear();

        innerSkirtEdge =(new Vector3((am.boundsFull.x - 1)* TilesetUtility.blockAssetSize, 0, 0)) + (Vector3.right * innerSkirtSize) + (Vector3.back * innerSkirtSize);
        outerSkirtEdge = innerSkirtEdge + (Vector3.back * outerSkirtSize);
        
        BoundingLine southEastOuterQuadrant = new BoundingLine(outerSkirtEdge, innerSkirtEdge);
        ProceduralMeshUtilities.SubdivideLine(southEastOuterQuadrant.origin, southEastOuterQuadrant.destination, skirtEdge, skirtSubdivisionCount);
        GenerateBoundingLines(skirtEdge, southEastOuterQuadrantLines, Vector3.right * outerSkirtSize);
        
        
        //North East Outer Quadrant
        skirtEdge.Clear();
        
        innerSkirtEdge =(new Vector3((am.boundsFull.x - 1) * TilesetUtility.blockAssetSize, 0, (am.boundsFull.z - 1) * TilesetUtility.blockAssetSize)) + (Vector3.right * innerSkirtSize) + (Vector3.forward * innerSkirtSize);
        outerSkirtEdge = innerSkirtEdge + (Vector3.right * outerSkirtSize); 
        
        BoundingLine northEastOuterQuadrant = new BoundingLine(outerSkirtEdge, innerSkirtEdge);
        ProceduralMeshUtilities.SubdivideLine(northEastOuterQuadrant.origin, northEastOuterQuadrant.destination, skirtEdge, skirtSubdivisionCount);
        GenerateBoundingLines(skirtEdge, northEastOuterQuadrantLines, Vector3.forward * outerSkirtSize);
        
        
        //North West Outer Quadrant
        skirtEdge.Clear();
        
        innerSkirtEdge = (new Vector3(0, 0, (am.boundsFull.z - 1) * TilesetUtility.blockAssetSize)) + (Vector3.left * innerSkirtSize) + (Vector3.forward * innerSkirtSize);
        outerSkirtEdge = innerSkirtEdge + (Vector3.forward * outerSkirtSize); 
        
        BoundingLine northWestOuterQuadrant = new BoundingLine(outerSkirtEdge, innerSkirtEdge);
        ProceduralMeshUtilities.SubdivideLine(northWestOuterQuadrant.origin, northWestOuterQuadrant.destination, skirtEdge, skirtSubdivisionCount);
        GenerateBoundingLines(skirtEdge, northWestOuterQuadrantLines, Vector3.left * outerSkirtSize);
    }

    private void ConvertGridsToMeshes()
    {
        //North Inner SkirtMesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(northBoundingLines, skirtGrid, skirtSubdivisionCount, null, innerSkirtSmoothingCurve, true);
        
        //Let's go through and fix the final edges before submitting this for meshing
        for (int i = 0; i <= skirtSubdivisionCount; ++i)
        {
            int offset = (skirtGrid.Count - 1 - skirtSubdivisionCount) + i;

            skirtGrid[offset] = ProceduralMeshUtilities.SetPointByCurves(skirtGrid[offset], interpolationCurves);
            skirtGrid[i] = ProceduralMeshUtilities.SetPointByCurves(skirtGrid[i], interpolationCurves);
        }
        
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, northBoundingLines.Count, skirtSubdivisionCount, northSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(northBoundingLines);
        }
        
        //South Inner Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(southBoundingLines, skirtGrid, skirtSubdivisionCount, null, innerSkirtSmoothingCurve, true);
        
        //Let's go through and fix the final edges before submitting this for meshing
        for (int i = 0; i <= skirtSubdivisionCount; ++i)
        {
            int offset = (skirtGrid.Count - 1 - skirtSubdivisionCount) + i;

            skirtGrid[offset] = ProceduralMeshUtilities.SetPointByCurves(skirtGrid[offset], interpolationCurves);
            skirtGrid[i] = ProceduralMeshUtilities.SetPointByCurves(skirtGrid[i], interpolationCurves);
        }
        
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, southBoundingLines.Count, skirtSubdivisionCount, southSkirt);
        
        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(southBoundingLines);
        }

        //East Inner Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(eastBoundingLines, skirtGrid, skirtSubdivisionCount, null, innerSkirtSmoothingCurve, true);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, eastBoundingLines.Count, skirtSubdivisionCount, eastSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(eastBoundingLines);
        }
        
        //West Inner Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(westBoundingLines, skirtGrid, skirtSubdivisionCount, null, innerSkirtSmoothingCurve, true);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, westBoundingLines.Count, skirtSubdivisionCount, westSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(westBoundingLines);
        }
        
        //North Outer Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(northOuterBoundingLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, northOuterBoundingLines.Count, skirtSubdivisionCount, northOuterSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(northOuterBoundingLines);
        }
        
        //South Outer Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(southOuterBoundingLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, southOuterBoundingLines.Count, skirtSubdivisionCount, southOuterSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(southOuterBoundingLines);
        }

        //East Outer Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(eastOuterBoundingLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, eastOuterBoundingLines.Count, skirtSubdivisionCount, eastOuterSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(southOuterBoundingLines);
        }
        
        //West Outer Skirt Mesh
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(westOuterBoundingLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, westOuterBoundingLines.Count, skirtSubdivisionCount, westOuterSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(westOuterBoundingLines);
        }
        
        //South West Outer Quadrant
        skirtGrid.Clear();

        ProceduralMeshUtilities.GenerateGridFromBoundingLines(southWestOuterQuadrantLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, southWestOuterQuadrantLines.Count, skirtSubdivisionCount, southWestSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(southWestOuterQuadrantLines);
        }

        //South East Outer Quadrant
        skirtGrid.Clear();

        ProceduralMeshUtilities.GenerateGridFromBoundingLines(southEastOuterQuadrantLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, southEastOuterQuadrantLines.Count, skirtSubdivisionCount, southEastSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(southEastOuterQuadrantLines);
        }

        //North East Outer Quadrant
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(northEastOuterQuadrantLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, northEastOuterQuadrantLines.Count, skirtSubdivisionCount, northEastSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(northEastOuterQuadrantLines);
        }
        
        //North West Outer Quadrant
        skirtGrid.Clear();
        
        ProceduralMeshUtilities.GenerateGridFromBoundingLines(northWestOuterQuadrantLines, skirtGrid, skirtSubdivisionCount, interpolationCurves);
        ProceduralMeshUtilities.GenerateMeshPatch(skirtGrid, northWestOuterQuadrantLines.Count, skirtSubdivisionCount, northWestSkirt);

        if (collectDebugData)
        {
            skirtDebugGrid.AddRange(skirtGrid);
            skirtDebugLines.AddRange(northWestOuterQuadrantLines);
        }

        skirtGrid.Clear();
    }

    private void GenerateBoundingLines(List<Vector3> inputPoints, List<BoundingLine> outputLines, Vector3 extrusionDirection)
    {
        foreach (var startPoint in inputPoints)
        {
            Vector3 endPoint = startPoint + extrusionDirection;
            outputLines.Add(new BoundingLine(startPoint, endPoint));
        }
    }

    private void GetTerrainBoundingLines(List<Vector3> terrainPoints, List<BoundingLine> boundingLines, Vector3 extrusionDirection)
    {
        foreach (var startPoint in terrainPoints)
        {
            Vector3 endPoint = startPoint + extrusionDirection;
            endPoint = ProceduralMeshUtilities.SetPointByCurves(endPoint, interpolationCurves);
            
            boundingLines.Add(new BoundingLine(startPoint, endPoint));
        }
    }
    
    private void GetTerrainEdge(List<Vector3> outputPoints, Direction direction, ProceduralMeshTerrainV2 sourceTerrain)
    {
        var sourceData = sourceTerrain.GetTerrainSpots ();
        var vertexData = sourceTerrain.GetVertexData ();

        //Handle the case where there's no terrain data to reference / Revert to old control point / heightfield handling
        if (sourceData == null || vertexData == null)
        {
            GetTerrainEdge(outputPoints, direction, TilesetUtility.blockAssetSize);
            
            return;
        }
            

        int columns = am.boundsFull.x - 1;
        int rows = am.boundsFull.z - 1;
        int edgeIndicesCount = sourceTerrain.subdivisions + 1;

        switch (direction)
        {
            case Direction.North:
                //moving east to west
                for (int c = columns - 1; c >=  0; --c)
                {
                    var currentSpot = sourceData[c, rows - 1];

                    if (currentSpot.empty)
                    {
                        Vector3 startPoint = currentSpot.controlPointNorthEast;
                        Vector3 endPoint = currentSpot.controlPointNorthWest;

                        float interpolationStep = 1f / (edgeIndicesCount - 1);
                        for (int i = 0; i < edgeIndicesCount; ++i)
                        {
                            if (i == edgeIndicesCount - 1 && c != 0)
                            {
                                continue;
                            }
                            
                            outputPoints.Add (Vector3.Lerp (startPoint, endPoint, interpolationStep * i));
                        }
                    }
                    else
                    {
                        //Due to indices being duplicated on the seams of sub grids, we're going to skip
                        //the last vertex of each sub grid here, except in the very last terrain spot,
                        //Which has no neighbor, and therefore needs the final vertex
                        for (int i = edgeIndicesCount - 1; i >= 0; --i)
                        {
                            if (i == 0 && c != 0)
                            {
                                continue;
                            }
                        
                            int vertexIndex = currentSpot.indices[i, edgeIndicesCount - 1];
                            outputPoints.Add (vertexData[vertexIndex]);
                        }   
                    }
                }
                break;
            case Direction.South:
                //moving from west to east
                for (int c = 0; c < columns; ++c)
                {
                    var currentSpot = sourceData[c, 0];

                    if (currentSpot.empty)
                    {
                        Vector3 startPoint = currentSpot.controlPointSouthWest;
                        Vector3 endPoint = currentSpot.controlPointSouthEast;

                        float interpolationStep = 1f / (edgeIndicesCount - 1);
                        for (int i = 0; i < edgeIndicesCount; ++i)
                        {
                            if (i == edgeIndicesCount - 1 && c != columns - 1)
                            {
                                continue;
                            }
                            
                            outputPoints.Add (Vector3.Lerp (startPoint, endPoint, interpolationStep * i));
                        }
                    }
                    else
                    {
                        //Due to indices being duplicated on the seams of sub grids, we're going to skip
                        //the last index of each sub grid here, except in the very last terrain spot,
                        //Which has no neighbor, and therefore needs the final vertex
                        for (int i = 0; i < edgeIndicesCount; ++i)
                        {
                            if (i == edgeIndicesCount - 1 && c != columns - 1)
                            {
                                continue;
                            }

                            int vertexIndex = currentSpot.indices[i, 0];
                            outputPoints.Add (vertexData[vertexIndex]);
                        }
                    }
                }
                break;
            case Direction.East:
                //moving south to north
                for (int r = 0; r < rows; ++r)
                {
                    var currentSpot = sourceData[columns - 1, r];
                    
                    
                    if (currentSpot.empty)
                    {
                        Vector3 startPoint = currentSpot.controlPointSouthEast;
                        Vector3 endPoint = currentSpot.controlPointNorthEast;

                        float interpolationStep = 1f / (edgeIndicesCount - 1);
                        for (int i = 0; i < edgeIndicesCount; ++i)
                        {
                            if (i == edgeIndicesCount - 1 && r != rows - 1)
                            {
                                continue;
                            }
                            
                            outputPoints.Add (Vector3.Lerp (startPoint, endPoint, interpolationStep * i));
                        }
                    }
                    else
                    {

                        for (int i = 0; i < edgeIndicesCount; ++i)
                        {
                            if (i == edgeIndicesCount - 1 && r != rows - 1)
                            {
                                continue;
                            }

                            int vertexIndex = currentSpot.indices[edgeIndicesCount - 1, i];
                            outputPoints.Add (vertexData[vertexIndex]);
                        }
                    }
                }

                break;
            case Direction.West:
                //moving north to south
                for (int r = rows - 1; r >= 0; --r)
                {
                    var currentSpot = sourceData[0, r];

                    if (currentSpot.empty)
                    {
                        Vector3 startPoint = currentSpot.controlPointNorthWest;
                        Vector3 endPoint = currentSpot.controlPointSouthWest;

                        float interpolationStep = 1f / (edgeIndicesCount - 1);
                        for (int i = 0; i < edgeIndicesCount; ++i)
                        {
                            if (i == edgeIndicesCount - 1 && r != 0)
                            {
                                continue;
                            }
                            
                            outputPoints.Add (Vector3.Lerp (startPoint, endPoint, interpolationStep * i));
                        }
                    }
                    else
                    {
                        for (int i = edgeIndicesCount - 1; i >= 0; --i)
                        {
                            if (i == 0 && r != 0)
                            {
                                continue;
                            }

                            int vertexIndex = currentSpot.indices[0, i];
                            outputPoints.Add (vertexData[vertexIndex]);
                        }
                    }
                }

                break;
        }
    }

    private void GetTerrainEdge(List<Vector3> outputPoints, Direction direction, float blockSize)
    {
        switch (direction)
        {
            case Direction.North:
                //moving east to west
                for (int x = am.boundsFull.x - 1; x >= 0; --x)
                {
                    outputPoints.Add(ProceduralMeshUtilities.GetScaledPositionFromHeightField(x, am.boundsFull.z - 1, blockSize, heightfield));
                }
                break;
            case Direction.South:
                //moving from west to east
                for (int x = 0; x < am.boundsFull.x; ++x)
                {
                    outputPoints.Add(ProceduralMeshUtilities.GetScaledPositionFromHeightField(x, 0, blockSize, heightfield));
                }
                break;
            case Direction.East:
                //moving south to north
                for (int z = 0; z < am.boundsFull.z; ++z)
                {
                    outputPoints.Add(ProceduralMeshUtilities.GetScaledPositionFromHeightField(am.boundsFull.x - 1, z, blockSize, heightfield));
                }
                break;
            case Direction.West:
                //moving north to south
                for (int z = am.boundsFull.z - 1; z >= 0; --z)
                {
                    outputPoints.Add(ProceduralMeshUtilities.GetScaledPositionFromHeightField(0, z, blockSize, heightfield));
                }
                break;
        }
    }

    private void GenerateInnerTerrainSkirts(float innerSkirtDepth, float blockSize)
    {
        /*
        // -- Creation of interior skirts
        
        //For the sake of simplicity, I'm just generating this quickly by going around the interior in a clockwise fashion
        //South inner skirt
        
        List<Vector3> edgePoints = new List<Vector3> ();
        
        GetTerrainEdge (edgePoints, Direction.South, terrain);
        for (int i = 0; i < edgePoints.Count - 1; ++i)
        {
            Vector3 target = edgePoints[i];
            Vector3 source = new Vector3(target.x, innerSkirtDepth, target.z);
            
            tempLines.Add(new BoundingLine {destination = target, origin = source});            
        }
        edgePoints.Clear ();
        
        GetTerrainEdge (edgePoints, Direction.East, terrain);
        for (int i = 0; i < edgePoints.Count - 1; ++i)
        {
            Vector3 target = edgePoints[i];
            Vector3 source = new Vector3(target.x, innerSkirtDepth, target.z);
            
            tempLines.Add(new BoundingLine {destination = target, origin = source});            
        }
        edgePoints.Clear ();
        
        GetTerrainEdge (edgePoints, Direction.North, terrain);
        for (int i = 0; i < edgePoints.Count - 1; ++i)
        {
            Vector3 target = edgePoints[i];
            Vector3 source = new Vector3(target.x, innerSkirtDepth, target.z);
            
            tempLines.Add(new BoundingLine {destination = target, origin = source});            
        }
        edgePoints.Clear ();
        
        
        GetTerrainEdge (edgePoints, Direction.West, terrain);
        for (int i = 0; i < edgePoints.Count; ++i)
        {
            Vector3 target = edgePoints[i];
            Vector3 source = new Vector3(target.x, innerSkirtDepth, target.z);
            
            tempLines.Add(new BoundingLine {destination = target, origin = source});            
        }
        edgePoints.Clear ();

        
        //-- Create a mesh from this Patch --
        ProceduralMeshUtilities.CreateMeshPatch(terrainInnerSkirt, tempLines, 2);
        tempLines.Clear();
        //-- End Mesh Creation --

        */
    }

    private void GenerateTerrainFloorSkirt(float innerSkirtDepth, float blockSize)
    {
        /*
        // -- Construction of Terrain Floor Skirt
        
        //Really quick, make some bounding lines to construct the terrain floor skirt with
        //North West to Southwest
        tempLines.Add(new BoundingLine{origin = new Vector3(0, innerSkirtDepth, (am.boundsFull.z - 1) * blockSize), destination = new Vector3(0, innerSkirtDepth, 0)});
        //North East to Southeast
        tempLines.Add(new BoundingLine{origin = new Vector3((am.boundsFull.x - 1) * blockSize, innerSkirtDepth, (am.boundsFull.z - 1) * blockSize), destination = new Vector3((am.boundsFull.x - 1) * blockSize, innerSkirtDepth, 0)});

        //-- Create a mesh from this Patch --
        ProceduralMeshUtilities.CreateMeshPatch(terrainFloorSkirt, tempLines, 2);
        tempLines.Clear();
        //-- End Mesh Creation --
        */
    }

    private List<BoundingLine> tempLines = new List<BoundingLine>();
    private void GetSkirtLinesFromArea()
    {
        if (am == null || am.points.Count < 8)
            return;

        GenerateHeightfieldFromArea();

        skirtSubdivisionCount = Mathf.Max(1, skirtSubdivisionCount);

        if (heightfield == null)
            return;

        //Okay, first, let's extract the points we need for the first skirt section

        skirtDebugLines.Clear();
        skirtDebugGrid.Clear();

        tempLines.Clear();

        float blockSize = TilesetUtility.blockAssetSize;

        float innerSkirtDepth = (am.boundsFull.y + 1) * -blockSize;
        
        //Interior Hiding meshes
        //GenerateInnerTerrainSkirts(innerSkirtDepth, blockSize);
        //GenerateTerrainFloorSkirt(innerSkirtDepth, blockSize);
        
        GenerateTerrainSkirts();
    }

    private void DrawPointList(List<Vector3> points)
    {
        if (points == null)
            return;

        for (int i = 0; i < points.Count; ++i)
        {
            Gizmos.color = i == 0 ? Color.blue : pointColor;
            Gizmos.color = i == points.Count - 1 ? Color.red : Gizmos.color;

            //Gizmos.DrawLine(points[i], points[i] + (Vector3.up * 2f));
            Gizmos.DrawWireCube(points[i], Vector3.one * 0.33f);
        }
    }

    private void DrawBoundingLineGizmos(List<BoundingLine> points)
    {
        if (points == null)
            return;

        for (int i = 0; i < points.Count; ++i)
        {
            var point = points[i];
            Gizmos.color = i == 0 ? Color.blue : boundingPointColor;
            Gizmos.color = i == points.Count - 1 ? Color.red : Gizmos.color;
            Gizmos.DrawLine(point.origin, point.destination);

            Gizmos.color = Color.green;
            Gizmos.DrawCube(point.origin, Vector3.one * 0.33f);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(point.destination, Vector3.one * 0.33f);
        }
    }

    private void DrawHeightFieldGizmos()
    {
        if (heightfield == null)
            return;

        float blockSize = TilesetUtility.blockAssetSize;
        for (int x = 0; x < am.boundsFull.x; ++x)
        {
            for (int z = 0; z < am.boundsFull.z; ++z)
            {
                Gizmos.color = x == 0 && z <= 2 ? Color.blue : heightFieldColor;
                Gizmos.color = x <= 2 && z == 0 ? Color.red : Gizmos.color;

                Vector3 position = ProceduralMeshUtilities.GetScaledPositionFromHeightField(x, z, blockSize, heightfield);
                Gizmos.DrawWireCube(position, Vector3.one * 0.33f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (am == null)
            return;

        if (!drawOnce && !drawContinual)
            return;

        if (drawOnce)
            drawOnce = false;

        if (showGridPoints)
        {
            Gizmos.color = pointColor;
            DrawPointList(skirtDebugGrid);
        }

        if (showBoundingLines)
        {
            Gizmos.color = boundingPointColor;
            DrawBoundingLineGizmos(skirtDebugLines);
        }

        if (showHeightfield)
        {
            Gizmos.color = heightFieldColor;
            DrawHeightFieldGizmos();
        }
        
        if (showSourcePoint)
        {
            var c = interpolationCurves.boundsCurve.center;
            var e = interpolationCurves.boundsCurve.extents;
            var p1 = new Vector3 (sourcePointTest.x, c.y + e.y, sourcePointTest.z);
            var p2 = new Vector3 (sourcePointTest.x, sourcePointHeight, sourcePointTest.z);
            
            Gizmos.color = sourcePointColor;
            Gizmos.DrawLine (p1, p2);
        }
        
        if (showCurveBounds)
        {
            var c = interpolationCurves.boundsCurve.center;
            var e = interpolationCurves.boundsCurve.extents;
        
            Gizmos.color = curveBoundsColor;
            Gizmos.DrawWireCube (c, e * 2f);
            Gizmos.DrawLine (c - Vector3.up * e.y, c + Vector3.up * e.y);
            
            c = interpolationCurves.boundsLevel.center;
            e = interpolationCurves.boundsLevel.extents;
            
            Gizmos.color = levelBoundsColor;
            Gizmos.DrawWireCube (c, e * 2f);
        }

        Gizmos.color = Color.white;
    }


}