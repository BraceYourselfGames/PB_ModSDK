using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class ProceduralMeshSimple : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;

    [System.Serializable]
    public class ProceduralMeshLoop
    {
        public Vector3 posIn = Vector3.one;
        public Vector3 posOut = Vector3.one * 2;
    }

    public List<ProceduralMeshLoop> loops;

    private void OnEnable ()
    {
        if (Utilities.isPlaymodeChanging)
            return;

        Rebuild ();
    }

    [Button ("Rebuild")]
    private void Rebuild ()
    {
        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer> ();

        if (mf == null || mr == null)
            return;

        if (loops.Count == 0)
            return;

        Mesh mesh = new Mesh ();
        mf.mesh = mesh;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Vector3[] vertices = new Vector3[16 * loops.Count];
        int[] triangles = new int[24 * loops.Count];
        Vector3[] normals = new Vector3[16 * loops.Count];
        Vector2[] uv = new Vector2[16 * loops.Count];

        for (int i = 0; i < loops.Count; ++i)
        {
            Vector3 posIn = loops[i].posIn;
            posIn = new Vector3 (0.5f * posIn.x, posIn.y, 0.5f * posIn.z);

            Vector3 posOut = loops[i].posOut;
            posOut = new Vector3 (0.5f * posOut.x, posOut.y, 0.5f * posOut.z);

            int indexShift16 = 16 * i;
            int indexShift24 = 24 * i;

            vertices[0 + indexShift16] = new Vector3 (-posOut.x, posOut.y, posOut.z);
            vertices[1 + indexShift16] = new Vector3 (posOut.x, posOut.y, posOut.z);
            vertices[2 + indexShift16] = new Vector3 (posIn.x, posIn.y, posIn.z);
            vertices[3 + indexShift16] = new Vector3 (-posIn.x, posIn.y, posIn.z);

            vertices[4 + indexShift16] = new Vector3 (posOut.x, posOut.y, posOut.z);
            vertices[5 + indexShift16] = new Vector3 (posOut.x, posOut.y, -posOut.z);
            vertices[6 + indexShift16] = new Vector3 (posIn.x, posIn.y, -posIn.z);
            vertices[7 + indexShift16] = new Vector3 (posIn.x, posIn.y, posIn.z);

            vertices[8 + indexShift16] = new Vector3 (posOut.x, posOut.y, -posOut.z);
            vertices[9 + indexShift16] = new Vector3 (-posOut.x, posOut.y, -posOut.z);
            vertices[10 + indexShift16] = new Vector3 (-posIn.x, posIn.y, -posIn.z);
            vertices[11 + indexShift16] = new Vector3 (posIn.x, posIn.y, -posIn.z);

            vertices[12 + indexShift16] = new Vector3 (-posOut.x, posOut.y, -posOut.z);
            vertices[13 + indexShift16] = new Vector3 (-posOut.x, posOut.y, posOut.z);
            vertices[14 + indexShift16] = new Vector3 (-posIn.x, posIn.y, posIn.z);
            vertices[15 + indexShift16] = new Vector3 (-posIn.x, posIn.y, -posIn.z);

            for (int t = 0; t < 4; ++t)
            {
                int indexShift4 = 4 * t;
                int indexShift6 = 6 * t;
                triangles[0 + indexShift24 + indexShift6] = 0 + indexShift16 + indexShift4;
                triangles[1 + indexShift24 + indexShift6] = 2 + indexShift16 + indexShift4;
                triangles[2 + indexShift24 + indexShift6] = 3 + indexShift16 + indexShift4;
                triangles[3 + indexShift24 + indexShift6] = 0 + indexShift16 + indexShift4;
                triangles[4 + indexShift24 + indexShift6] = 1 + indexShift16 + indexShift4;
                triangles[5 + indexShift24 + indexShift6] = 2 + indexShift16 + indexShift4;
            }

            for (int a = 0; a < 16; ++a)
            {
                normals[a + indexShift16] = Vector3.up;
                uv[a + indexShift16] = new Vector2 (vertices[a + indexShift16].x, vertices[a + indexShift16].z);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.RecalculateTangents ();
        mesh.RecalculateBounds ();
    }
}
