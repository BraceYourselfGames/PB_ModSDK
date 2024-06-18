using System.Collections.Generic;
using Area;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class ProceduralMeshHeightmap : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;

    public ProceduralMeshQuadList quadList;
    public float heightRange = 1f;
    public float size = 1f;


    public int neighborhoodSizeX = 14;
    public int neighborhoodSizeY = 3;
    public int neighborhoodOffsetX = -17;
    public int neighborhoodOffsetY = -3;
    public float neighborhoodNormalizedDepthMultiplier = 0.5f;

    [Button ("Rebuild flat")]
    public void RebuildQuads ()
    {
        var core = DataMultiLinkerCombatArea.GetCurrentLevelDataCore ();
        var dataUnpacked = DataMultiLinkerCombatArea.GetCurrentLevelDataRoot ();
        
        if 
        (
            dataUnpacked == null || 
            dataUnpacked.points.Length < 8 || 
            core.bounds.x < 2 && 
            core.bounds.y < 2 && 
            core.bounds.z < 2
        )
        {
            Debug.LogWarning ("ProceduralMeshHeightmap | RebuildQuads | Failed to rebuild from area, no loaded data available, point count is too low or bounds are too small");
            gameObject.SetActive (false);
            return;
        }

        ProceduralMeshUtilities.CollectSurfacePointsFromCompressedArea ();

        if (ProceduralMeshUtilities.heightfieldCompressed == null || quadList == null)
        {
            Debug.LogWarning ("ProceduralMeshHeightmap | RebuildQuads | Failed to rebuild from area, no heightmap or quad list available");
            gameObject.SetActive (false);
            return;
        }

        if (quadList.quads == null)
            quadList.quads = new List<ProceduralMeshQuadList.QuadInfo> ();

        var quads = quadList.quads;
        quads.Clear ();

        var heightfield = ProceduralMeshUtilities.heightfieldCompressed;
        var boundsFull = core.bounds;
        int spriteID = 0;

        size = Mathf.Max (0.1f, size);
        float pointSizeX = size / boundsFull.x;
        float pointSizeZ = size / boundsFull.z;

        for (int x = 0; x < boundsFull.x; ++x)
        {
            for (int z = 0; z < boundsFull.z; ++z)
            {
                var height = heightfield[x, z];
                var heightNormalized = 1f - Mathf.Clamp01 ((float)height / boundsFull.y);

                quads.Add (new ProceduralMeshQuadList.QuadInfo
                {
                    position = new Vector3 (pointSizeX * x, heightNormalized * heightRange, pointSizeZ * z),
                    rotation = Quaternion.identity
                });
            }
        }

        quadList.pointSize = pointSizeX * 0.5f;
        quadList.Rebuild ();
    }

    [Button ("Rebuild cubes")]
    public void RebuildCubes (bool buildNeighborhood)
    {
        var core = DataMultiLinkerCombatArea.GetCurrentLevelDataCore ();
        var dataUnpacked = DataMultiLinkerCombatArea.GetCurrentLevelDataRoot ();
        
        if 
        (
            dataUnpacked == null || 
            dataUnpacked.points.Length < 8 || 
            core.bounds.x < 2 && 
            core.bounds.y < 2 && 
            core.bounds.z < 2
        )
        {
            Debug.LogWarning ("ProceduralMeshHeightmap | RebuildCubes | Failed to rebuild from area, no loaded data available, point count is too low or bounds are too small");
            gameObject.SetActive (false);
            return;
        }

        if (quadList == null)
        {
            Debug.LogWarning ("ProceduralMeshHeightmap | RebuildCubes | Failed to rebuild from area, no quad list available");
            gameObject.SetActive (false);
            return;
        }

        if (quadList.quads == null)
            quadList.quads = new List<ProceduralMeshQuadList.QuadInfo> ();

        var quads = quadList.quads;
        quads.Clear ();

        var boundsFull = core.bounds;
        int spriteID = 0;

        size = Mathf.Max (0.1f, size);
        float pointSize = size / Mathf.Max (boundsFull.x, boundsFull.z);
        float quadOffsetSize = pointSize * 0.45f;

        var topOffset = Vector3.up * quadOffsetSize;
        var topRotation = Quaternion.identity;
        int pointLimit = boundsFull.x * boundsFull.y * boundsFull.z;
        var offsetFromHeight = Vector3.up * boundsFull.y * pointSize;

        for (int i = 0; i < pointLimit; ++i)
        {
            var point = dataUnpacked.points[i];
            if (point == false)
                continue;

            var pos = AreaUtility.GetVolumePositionFromIndex (i, core.bounds);
            var posBase = new Vector3 (pos.x, -pos.y, pos.z) * pointSize + offsetFromHeight;
            
            var xNormalized = Mathf.Clamp01 ((float)pos.x / boundsFull.x);
            var yNormalized = Mathf.Clamp01 ((float)pos.y / boundsFull.y);
            var zNormalized = Mathf.Clamp01 ((float)pos.z / boundsFull.z); // Mathf.Pow ((float)pos.y / boundsFull.y, 0.45f);
            
            quads.Add (new ProceduralMeshQuadList.QuadInfo
            {
                position = posBase + topOffset,
                rotation = topRotation,
                color = new Color (xNormalized, 1f - yNormalized, zNormalized) // Color.HSVToRGB (yNormalized * 0.25f, 1f, 1f - yNormalized)
            });
        }

        if (buildNeighborhood)
        {
            int neighborhoodLimit = boundsFull.z * neighborhoodSizeX * neighborhoodSizeY;
            var neighborhoodBounds = new Vector3Int (neighborhoodSizeX, neighborhoodSizeY, boundsFull.z);
            var offsetFromHeightNeighbor = Vector3.up * neighborhoodSizeY * pointSize;

            for (int i = 0; i < neighborhoodLimit; ++i)
            {
                var pos = AreaUtility.GetVolumePositionFromIndex (i, neighborhoodBounds);
                var posBase = new Vector3 (pos.x + neighborhoodOffsetX, -pos.y + neighborhoodOffsetY, pos.z) * pointSize;

                var xNormalized = Mathf.Clamp01 ((float)pos.x / boundsFull.x);
                var yNormalized = 1f - Mathf.Clamp01 ((1f - (float)pos.y / neighborhoodSizeY) * neighborhoodNormalizedDepthMultiplier);
                var zNormalized = Mathf.Clamp01 ((float)pos.z / boundsFull.z); // Mathf.Pow ((float)pos.y / boundsFull.y, 0.45f);

                quads.Add (new ProceduralMeshQuadList.QuadInfo
                {
                    position = posBase + topOffset + offsetFromHeightNeighbor,
                    rotation = topRotation,
                    color = new Color (xNormalized, 1f - yNormalized, zNormalized) // Color.HSVToRGB (yNormalized * 0.25f, 1f, 1f - yNormalized)
                });
            }
        }

        quadList.pointSize = pointSize * 0.5f;
        quadList.Rebuild ();
    }

    [Button ("Rebuild skin", ButtonSizes.Large)]
    public void RebuildSurface ()
    {
        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer> ();

        if (mf == null || mr == null)
            return;

        var core = DataMultiLinkerCombatArea.GetCurrentLevelDataCore ();
        var dataUnpacked = DataMultiLinkerCombatArea.GetCurrentLevelDataRoot ();
        
        if 
        (
            dataUnpacked == null || 
            dataUnpacked.points.Length < 8 || 
            core.bounds.x < 2 && 
            core.bounds.y < 2 && 
            core.bounds.z < 2
        )
        {
            Debug.LogWarning ("ProceduralMeshHeightmap | RebuildSurface | Failed to rebuild from area, no loaded data available, point count is too low or bounds are too small");
            gameObject.SetActive (false);
            return;
        }

        ProceduralMeshUtilities.CollectSurfacePointsFromCompressedArea ();

        if (ProceduralMeshUtilities.heightfieldCompressed == null)
        {
            Debug.LogWarning ("ProceduralMeshHeightmap | RebuildSurface | Failed to rebuild from area, no heightmap available");
            gameObject.SetActive (false);
            return;
        }

        var heightfield = ProceduralMeshUtilities.heightfieldCompressed;
        var boundsFull = core.bounds;
        int spriteID = 0;

        size = Mathf.Max (0.1f, size);
        float pointSizeX = size / boundsFull.x;
        float pointSizeZ = size / boundsFull.z;

        int sizeOnX = heightfield.GetLength (0);
        int sizeOnZ = heightfield.GetLength (1);

        int vertexCount = sizeOnX * sizeOnZ;
        int triangleCount = (sizeOnX - 1) * (sizeOnZ - 1) * 2 * 3;
        // var spotShift = new Vector3 (0f, 1.5f, 0f);

        Mesh mesh = mf.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh ();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = "procedural_mesh_terrain";
            mf.sharedMesh = mesh;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector2[] uv2 = new Vector2[vertexCount];
        Quaternion normalRotation = Quaternion.Euler (90f, 0f, 0f);
        Color[] colors = new Color[vertexCount];
        Color vertexColorDefault = Color.white.WithAlpha (1f);

        int triangleIndex = 0;
        for (int x = 0, safeLimitX = sizeOnX - 1; x < safeLimitX; ++x)
        {
            for (int z = 0, safeLimitZ = sizeOnZ - 1; z < safeLimitZ; ++z)
            {
                var posXNegZNeg = new Vector3 (x, boundsFull.y - heightfield[x, z], z) * pointSizeX;
                var posXPosZNeg = new Vector3 (x + 1, boundsFull.y - heightfield[x + 1, z], z) * pointSizeX;
                var posXNegZPos = new Vector3 (x, boundsFull.y - heightfield[x, z + 1], z + 1) * pointSizeX;
                var posXPosZPos = new Vector3 (x + 1, boundsFull.y - heightfield[x + 1, z + 1], z + 1) * pointSizeX;

                int indexXNegZNeg = AreaUtility.GetIndexFromInternalPosition (x, z, sizeOnX, sizeOnZ);
                int indexXPosZNeg = AreaUtility.GetIndexFromInternalPosition (x + 1, z, sizeOnX, sizeOnZ);
                int indexXNegZPos = AreaUtility.GetIndexFromInternalPosition (x, z + 1, sizeOnX, sizeOnZ);
                int indexXPosZPos = AreaUtility.GetIndexFromInternalPosition (x + 1, z + 1, sizeOnX, sizeOnZ);
                int indexBySix = triangleIndex * 6;
                ++triangleIndex;

                vertices[indexXNegZNeg] = posXNegZNeg;
                vertices[indexXPosZNeg] = posXPosZNeg;
                vertices[indexXNegZPos] = posXNegZPos;
                vertices[indexXPosZPos] = posXPosZPos;

                triangles[indexBySix] = indexXNegZNeg;
                triangles[indexBySix + 1] = indexXNegZPos;
                triangles[indexBySix + 2] = indexXPosZPos;

                triangles[indexBySix + 3] = indexXNegZNeg;
                triangles[indexBySix + 4] = indexXPosZPos;
                triangles[indexBySix + 5] = indexXPosZNeg;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.colors = colors;
        mesh.RecalculateNormals ();
    }
}
