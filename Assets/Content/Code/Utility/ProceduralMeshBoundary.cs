using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Area;

[ExecuteInEditMode]
public class ProceduralMeshBoundary : MonoBehaviour
{
    [Serializable]
    public class QuadInfo
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 d;
    }

    public MeshFilter mf;
    public MeshRenderer mr;
    public AreaManager am;

    public int outerSize = 30;
    public int horizonSize = 30;
    
    public int subdX = 10;
    public int subdY = 10;

    public List<QuadInfo> quads = new List<QuadInfo> ();
    
    [NonSerialized]
    private List<QuadInfo> quadsFromArea = new List<QuadInfo> ();
    
    [NonSerialized]
    private int[,] heightfield;
    
    [NonSerialized]
    private int[,] heightfieldScaled;

    private void OnEnable ()
    {
        if (Utilities.isPlaymodeChanging)
            return;

        // Rebuild ();
    }

    /*
    private void OnDrawGizmosSelected ()
    {
        if (heightfield == null)
            return;

        Vector3 shift = new Vector3 (0f, offset, 0f);

        for (int x = 0, safeLimitX = heightfield.GetLength (0) - 1; x < safeLimitX; ++x)
        {
            for (int z = 0, safeLimitZ = heightfield.GetLength (1) - 1; z < safeLimitZ; ++z)
            {
                Vector3 pos0 = new Vector3 (x, heightfield[x, z], z) * TilesetUtility.blockAssetSize + shift;
                Vector3 pos1 = new Vector3 (x + 1, heightfield[x + 1, z], z) * TilesetUtility.blockAssetSize + shift;
                Vector3 pos2 = new Vector3 (x, heightfield[x, z + 1], z + 1) * TilesetUtility.blockAssetSize + shift;
                Vector3 pos3 = new Vector3 (x + 1, heightfield[x + 1, z + 1], z + 1) * TilesetUtility.blockAssetSize + shift;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine (pos3, pos2);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine (pos3, pos1);

                if (z == 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine (pos0, pos1);
                }

                if (x == 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine (pos0, pos2);
                }
            }
        }
    }
    */

    [Button ("Rebuild from area", ButtonSizes.Large), ButtonGroup]
    public void RebuildFromArea ()
    {
        quadsFromArea.Clear ();
        
        if (am == null || am.points.Count < 8)
        {
            Debug.LogWarning ($"Failed to build boundary mesh for area");
            return;
        }
        
        int size = am.boundsFull.x * am.boundsFull.z;
        int sizeX = am.boundsFull.x - 1;
        int sizeY = am.boundsFull.y - 1;
        int sizeZ = am.boundsFull.z - 1;
        
        int heightfieldLength = am.boundsFull.x * am.boundsFull.z;
        heightfield = new int[am.boundsFull.x, am.boundsFull.z];
        heightfieldScaled = new int[am.boundsFull.x, am.boundsFull.z];
        
        for (int i = 0; i < heightfieldLength; ++i)
        {
            var point = am.points[i];
            if (point == null)
            {
                Debug.LogWarning ($"Failed to build boundary mesh due to null point {i} in current area {am.areaName}");
                return;
            }

            int iteration = 0;
            bool emptyFound = false;
            
            while (true)
            {
                if (point.pointState != AreaVolumePointState.Empty)
                    break;

                var pointNext = point.pointsInSpot[4];
                if (pointNext == null)
                    break;

                point = pointNext;
                iteration += 1;
                if (iteration > 100)
                {
                    Debug.Log ("Breaking out of while loop, something is wrong");
                    break;
                }
            }

            heightfield[point.pointPositionIndex.x, point.pointPositionIndex.z] = -point.pointPositionIndex.y;
            heightfieldScaled[point.pointPositionIndex.x, point.pointPositionIndex.z] = -point.pointPositionIndex.y * TilesetUtility.blockAssetSize;
        }

        float sizeXScaled = sizeX * TilesetUtility.blockAssetSize;
        float sizeYScaled = sizeY * TilesetUtility.blockAssetSize;
        float sizeZScaled = sizeZ * TilesetUtility.blockAssetSize;
        float outerSizeNeg = -outerSize;
        
        float bottomDepth = -(sizeY + 1) * TilesetUtility.blockAssetSize;
        float bottomOffset = 2f * TilesetUtility.blockAssetSize;

        var heightXNegZNeg = heightfieldScaled[0, 0];
        var heightXPosZNeg = heightfieldScaled[sizeX, 0];
        var heightXNegZPos = heightfieldScaled[0, sizeZ];
        var heightXPosZPos = heightfieldScaled[sizeX, sizeZ];
        float heightOuterAverage = (heightXNegZNeg + heightXPosZNeg + heightXNegZPos + heightXPosZPos) * 0.25f; // (i % 2) * 6f

        // Corners
        
        quadsFromArea.Add (new QuadInfo
        {
            a = new Vector3 (outerSizeNeg, heightOuterAverage, outerSizeNeg),
            b = new Vector3 (0f, heightOuterAverage, outerSizeNeg),
            c = new Vector3 (outerSizeNeg, heightOuterAverage, 0f),
            d = new Vector3 (0f, heightXNegZNeg, 0f)
        });
        
        
        quadsFromArea.Add (new QuadInfo
        {
            a = new Vector3 (sizeXScaled, heightOuterAverage, outerSizeNeg),
            b = new Vector3 (sizeXScaled + outerSize, heightOuterAverage, outerSizeNeg),
            c = new Vector3 (sizeXScaled, heightXPosZNeg, 0f),
            d = new Vector3 (sizeXScaled + outerSize, heightOuterAverage, 0f)
        });
        
        
        quadsFromArea.Add (new QuadInfo
        {
            a = new Vector3 (outerSizeNeg, heightOuterAverage, sizeZScaled),
            b = new Vector3 (0f, heightXNegZPos, sizeZScaled),
            c = new Vector3 (outerSizeNeg, heightOuterAverage, sizeZScaled + outerSize),
            d = new Vector3 (0f, heightOuterAverage, sizeZScaled + outerSize)
        });
        
        
        quadsFromArea.Add (new QuadInfo
        {
            a = new Vector3 (sizeXScaled, heightXPosZPos, sizeZScaled),
            b = new Vector3 (sizeXScaled + outerSize, heightOuterAverage, sizeZScaled),
            c = new Vector3 (sizeXScaled, heightOuterAverage, sizeZScaled + outerSize),
            d = new Vector3 (sizeXScaled + outerSize, heightOuterAverage, sizeZScaled + outerSize)
        });

        // Outer strips
        
        AddStripX 
        (
            sizeX, 
            sizeXScaled, 
            0, 
            0f, 
            heightOuterAverage, 
            false, 
            outerSize
        );
        
        AddStripX 
        (
            sizeX,
            sizeXScaled, 
            sizeZ, 
            sizeZScaled, 
            heightOuterAverage, 
            true, 
            outerSize
        );
        
        AddStripZ 
        (
            sizeZ, 
            sizeZScaled, 
            0, 
            0f, 
            heightOuterAverage, 
            false, 
            outerSize
        );
        
        AddStripZ
        (
            sizeZ,
            sizeZScaled, 
            sizeX, 
            sizeXScaled, 
            heightOuterAverage, 
            true, 
            outerSize
        );
        
        // Inner strips
        
        AddStripX 
        (
            sizeX, 
            sizeXScaled, 
            0, 
            0f, 
            bottomDepth, 
            true, 
            bottomOffset
        );
        
        AddStripX 
        (
            sizeX,
            sizeXScaled, 
            sizeZ, 
            sizeZScaled, 
            bottomDepth, 
            false, 
            bottomOffset
        );
        
        AddStripZ 
        (
            sizeZ, 
            sizeZScaled, 
            0, 
            0f, 
            bottomDepth, 
            true, 
            bottomOffset
        );
        
        AddStripZ
        (
            sizeZ,
            sizeZScaled, 
            sizeX, 
            sizeXScaled, 
            bottomDepth, 
            false, 
            bottomOffset
        );
        
        // Bottom
        
        quadsFromArea.Add (new QuadInfo
        {
            a = new Vector3 (0f, bottomDepth, 0f),
            b = new Vector3 (sizeXScaled, bottomDepth, 0f),
            c = new Vector3 (0f, bottomDepth, sizeZScaled),
            d = new Vector3 (sizeXScaled, bottomDepth, sizeZScaled),
        });
        
        // Outer ring
        
        // 45 x+ z+
        // 135 x+ z-
        // -45 x- z+
        // -135 x- z-

        var dirXNegZNeg = Quaternion.Euler (0f, -135f, 0f) * Vector3.forward;
        var dirXPosZNeg = Quaternion.Euler (0f, 135f, 0f) * Vector3.forward;
        var dirXNegZPos = Quaternion.Euler (0f, -45f, 0f) * Vector3.forward;
        var dirXPosZPos = Quaternion.Euler (0f, 45f, 0f) * Vector3.forward;

        var cornerXNegZNeg = new Vector3 (-outerSize, heightOuterAverage, -outerSize);
        var cornerXPosZNeg = new Vector3 (sizeXScaled + outerSize, heightOuterAverage, -outerSize);
        var cornerXNegZPos = new Vector3 (-outerSize, heightOuterAverage, sizeZScaled + outerSize);
        var cornerXPosZPos = new Vector3 (sizeXScaled + outerSize, heightOuterAverage, sizeZScaled + outerSize);

        var horizonXNegZNeg = cornerXNegZNeg + dirXNegZNeg * horizonSize;
        var horizonXPosZNeg = cornerXPosZNeg + dirXPosZNeg * horizonSize;
        var horizonXNegZPos = cornerXNegZPos + dirXNegZPos * horizonSize;
        var horizonXPosZPos = cornerXPosZPos + dirXPosZPos * horizonSize;
        
        // Z-
        quadsFromArea.Add (new QuadInfo
        {
            a = horizonXNegZNeg,
            b = horizonXPosZNeg,
            c = cornerXNegZNeg,
            d = cornerXPosZNeg
        });
        
        // Z+
        quadsFromArea.Add (new QuadInfo
        {
            a = cornerXNegZPos,
            b = cornerXPosZPos,
            c = horizonXNegZPos,
            d = horizonXPosZPos
        });
        
        // X-
        quadsFromArea.Add (new QuadInfo
        {
            a = horizonXNegZNeg,
            b = cornerXNegZNeg,
            c = horizonXNegZPos,
            d = cornerXNegZPos
        });
        
        // X+
        quadsFromArea.Add (new QuadInfo
        {
            a = cornerXPosZNeg,
            b = horizonXPosZNeg,
            c = cornerXPosZPos,
            d = horizonXPosZPos
        });
        
        // Finalize
        
        RebuildFromList (quadsFromArea);
    }
    
    private void AddStripX (int sizeX, float sizeXScaled, int heightIndexOffsetZ, float positionOffsetZ, float heightOuterAverage, bool otherAxisPos, float outerSize)
    {
        for (int i = 0; i < sizeX; ++i)
        {
            int iNext = i + 1;
            float factorXNeg = (float)i / sizeX;
            float factorXPos = (float)iNext / sizeX;

            float xNeg = factorXNeg * sizeXScaled;
            float xPos = factorXPos * sizeXScaled;
            
            float heightXNeg = heightfieldScaled[i, heightIndexOffsetZ];
            float heightXPos = heightfieldScaled[iNext, heightIndexOffsetZ];

            if (otherAxisPos)
            {
                var localXNegZNeg = new Vector3 (xNeg, heightXNeg, positionOffsetZ);
                var localXPosZNeg = new Vector3 (xPos, heightXPos, positionOffsetZ);
                var localXNegZPos = new Vector3 (xNeg, heightOuterAverage, positionOffsetZ + outerSize);
                var localXPosZPos = new Vector3 (xPos, heightOuterAverage, positionOffsetZ + outerSize);
            
                quadsFromArea.Add (new QuadInfo
                {
                    a = localXNegZNeg,
                    b = localXPosZNeg,
                    c = localXNegZPos,
                    d = localXPosZPos
                });
            }
            else
            {
                var localXNegZNeg = new Vector3 (xNeg, heightOuterAverage, positionOffsetZ - outerSize);
                var localXPosZNeg = new Vector3 (xPos, heightOuterAverage, positionOffsetZ - outerSize);
                var localXNegZPos = new Vector3 (xNeg, heightXNeg, positionOffsetZ);
                var localXPosZPos = new Vector3 (xPos, heightXPos, positionOffsetZ);
            
                quadsFromArea.Add (new QuadInfo
                {
                    a = localXNegZNeg,
                    b = localXPosZNeg,
                    c = localXNegZPos,
                    d = localXPosZPos
                });
            }
        }
    }
    
    private void AddStripZ (int sizeZ, float sizeZScaled, int heightIndexOffsetX, float positionOffsetX, float heightOuterAverage, bool otherAxisPos, float outerSize)
    {
        for (int i = 0; i < sizeZ; ++i)
        {
            int iNext = i + 1;
            float factorZNeg = (float)i / sizeZ;
            float factorZPos = (float)iNext / sizeZ;

            float zNeg = factorZNeg * sizeZScaled;
            float zPos = factorZPos * sizeZScaled;
            
            float heightZNeg = heightfieldScaled[heightIndexOffsetX, i];
            float heightZPos = heightfieldScaled[heightIndexOffsetX, iNext];

            if (otherAxisPos)
            {
                var localXNegZNeg = new Vector3 (positionOffsetX, heightZNeg, zNeg);
                var localXPosZNeg = new Vector3 (positionOffsetX + outerSize, heightOuterAverage, zNeg);
                var localXNegZPos = new Vector3 (positionOffsetX, heightZPos, zPos);
                var localXPosZPos = new Vector3 (positionOffsetX + outerSize, heightOuterAverage, zPos);
            
                quadsFromArea.Add (new QuadInfo
                {
                    a = localXNegZNeg,
                    b = localXPosZNeg,
                    c = localXNegZPos,
                    d = localXPosZPos
                });
            }
            else
            {
                var localXNegZNeg = new Vector3 (positionOffsetX - outerSize, heightOuterAverage, zNeg);
                var localXPosZNeg = new Vector3 (positionOffsetX, heightZNeg, zNeg);
                var localXNegZPos = new Vector3 (positionOffsetX - outerSize, heightOuterAverage, zPos);
                var localXPosZPos = new Vector3 (positionOffsetX, heightZPos, zPos);
            
                quadsFromArea.Add (new QuadInfo
                {
                    a = localXNegZNeg,
                    b = localXPosZNeg,
                    c = localXNegZPos,
                    d = localXPosZPos
                });
            }
        }
    }

    [Button ("Rebuild from quads", ButtonSizes.Large), ButtonGroup]
    private void Rebuild ()
    {
        if (quads == null || quads.Count == 0)
        {
            Debug.LogWarning ($"No custom quad data available");
            return;
        }
        
        RebuildFromList (quads);
    }

    private void RebuildFromList (List<QuadInfo> quadsUsed)
    {
        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer> ();

        if (mf == null || mr == null)
            return;

        if (quadsUsed == null || quadsUsed.Count == 0)
            return;

        Mesh mesh = mf.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh ();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = "procedural_mesh_boundary";
            mf.sharedMesh = mesh;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        int vertexCount = quadsUsed.Count * 4;
        int triangleCount = quadsUsed.Count * 6;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector2[] uv2 = new Vector2[vertexCount];
        Quaternion normalRotation = Quaternion.Euler (90f, 0f, 0f);
        Color[] colors = new Color[vertexCount];
        Color vertexColorDefault = Color.white.WithAlpha (1f);

        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int i = 0; i < quadsUsed.Count; ++i)
        {
            var quad = quadsUsed[i];
            AddQuad (vertices, triangles, quad, ref vertexIndex, ref triangleIndex);
        }
        
        Debug.Log ($"Quads: {quadsUsed.Count} | Vertex count: {vertexCount} | Triangle index count: {triangleCount}");

        mesh.Clear ();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.colors = colors;
        
        mesh.RecalculateNormals ();
        normals = mesh.normals;
    }

    private void AddQuad (Vector3[] vertices, int[] triangles, QuadInfo quad, ref int vertexIndex, ref int triangleIndex)
    {
        vertices[vertexIndex] = quad.a;
        vertices[vertexIndex + 1] = quad.b;
        vertices[vertexIndex + 2] = quad.c;
        vertices[vertexIndex + 3] = quad.d;
                
        triangles[triangleIndex] = vertexIndex;
        triangles[triangleIndex + 1] = vertexIndex + 2;
        triangles[triangleIndex + 2] = vertexIndex + 3;
            
        triangles[triangleIndex + 3] = vertexIndex;
        triangles[triangleIndex + 4] = vertexIndex + 3;
        triangles[triangleIndex + 5] = vertexIndex + 1;
        
        vertexIndex += 4;
        triangleIndex += 6;
    }
}
