using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ProceduralMeshQuadList : MonoBehaviour
{
    public struct QuadInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Color color;
    }
    
    public MeshFilter mf;
    public MeshRenderer mr;

    public float pointSize;
    public List<QuadInfo> quads;
    private Mesh mesh;

    [Button ("Rebuild")]
    public void Rebuild ()
    {
        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer> ();

        if (mf == null || mr == null)
            return;

        if (mesh == null)
        {
            mesh = new Mesh ();
            mesh.indexFormat = IndexFormat.UInt32;
        }

        mf.sharedMesh = mesh;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Vector3[] vertices = new Vector3 [4 * quads.Count];
        int[] triangles = new int[6 * quads.Count];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        Color[] vc = new Color[vertices.Length];
        
        var uvXNegZNeg = new Vector2 (0, 0);
        var uvXPosZNeg = new Vector2 (1, 0);
        var uvXNegZPos = new Vector2 (0, 1);
        var uvXPosZPos = new Vector2 (1, 1);
        var normalUp = Vector3.up;

        for (int i = 0; i < quads.Count; ++i)
        {
            var info = quads[i];
            var posBase = info.position;
            var rotBase = info.rotation;
            var col = info.color;
            
            var posXNegZNeg = posBase + rotBase * new Vector3 (-pointSize, 0f, -pointSize);
            var posXPosZNeg = posBase + rotBase * new Vector3 (pointSize, 0f, -pointSize);
            var posXNegZPos = posBase + rotBase * new Vector3 (-pointSize, 0f, pointSize);
            var posXPosZPos = posBase + rotBase * new Vector3 (pointSize, 0f, pointSize);
            
            int indexShiftVertices = 4 * i;
            int indexShiftTriangles = 6 * i;

            var indexXNegZNeg = 0 + indexShiftVertices;
            var indexXPosZNeg = 1 + indexShiftVertices;
            var indexXNegZPos = 2 + indexShiftVertices;
            var indexXPosZPos = 3 + indexShiftVertices;

            vertices[indexXNegZNeg] = posXNegZNeg;
            vertices[indexXPosZNeg] = posXPosZNeg;
            vertices[indexXNegZPos] = posXNegZPos;
            vertices[indexXPosZPos] = posXPosZPos;
            
            uv[indexXNegZNeg] = uvXNegZNeg;
            uv[indexXPosZNeg] = uvXPosZNeg;
            uv[indexXNegZPos] = uvXNegZPos;
            uv[indexXPosZPos] = uvXPosZPos;
            
            vc[indexXNegZNeg] = col;
            vc[indexXPosZNeg] = col;
            vc[indexXNegZPos] = col;
            vc[indexXPosZPos] = col;
            
            normals[indexXNegZNeg] = normalUp;
            normals[indexXPosZNeg] = normalUp;
            normals[indexXNegZPos] = normalUp;
            normals[indexXPosZPos] = normalUp;

            triangles[0 + indexShiftTriangles] = indexXNegZNeg;
            triangles[1 + indexShiftTriangles] = indexXNegZPos;
            triangles[2 + indexShiftTriangles] = indexXPosZNeg;
            triangles[3 + indexShiftTriangles] = indexXNegZPos;
            triangles[4 + indexShiftTriangles] = indexXPosZPos;
            triangles[5 + indexShiftTriangles] = indexXPosZNeg;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.colors = vc;
        mesh.RecalculateTangents ();
        mesh.RecalculateBounds ();
    }
}
