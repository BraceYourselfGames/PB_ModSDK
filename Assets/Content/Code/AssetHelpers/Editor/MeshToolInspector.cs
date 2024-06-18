using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor (typeof (MeshTool))]
public class MeshToolInspector : Editor
{
    private static string lastSavePath = string.Empty;
    private static string lastFileName = string.Empty;

    public override void OnInspectorGUI ()
    {
        MeshTool t = (MeshTool) target;

        MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer> ();
        if (mr == null)
        {
            EditorGUILayout.HelpBox ("No MeshRenderer component present on this object!", MessageType.Warning);
            return;
        }

        MeshFilter mf = t.gameObject.GetComponent<MeshFilter> ();
        if (mf == null)
        {
            EditorGUILayout.HelpBox ("No MeshFilter component present on this object!", MessageType.Warning);
            return;
        }

        DrawDefaultInspector ();

        if (t.meshOriginal == null || t.meshModified == null)
        {
            EditorGUILayout.HelpBox ("No original mesh present - set it to continue", MessageType.Info);
            if (GUILayout.Button ("Open the mesh in the filter"))
            {
                if (mf.sharedMesh != null)
                {
                    t.meshOriginal = mf.sharedMesh;
                    t.meshModified = Instantiate (t.meshOriginal);
                    t.meshModified.name = t.meshOriginal.name + "*";
                    mf.sharedMesh = t.meshModified;

                    t.materialsOriginal = mr.sharedMaterials;
                }
                else
                    Debug.LogWarning ("MP | Unable to retrieve the original mesh: filter has nothing referenced");
            }
        }
        else
        {
            if (GUILayout.Button ("Reset to original state"))
            {
                mf.sharedMesh = t.meshOriginal;
                t.meshOriginal = null;
                t.meshModified = null;

                mr.sharedMaterials = t.materialsOriginal;
                t.materialsOriginal = null;
                return;
            }

            if (t.meshModified.subMeshCount > 1)
            {
                GUI.enabled = mr.sharedMaterials.Length == t.meshModified.subMeshCount;
                if (!GUI.enabled)
                    EditorGUILayout.HelpBox ("The number of materials on MeshRenderer is not equal to the number of submeshes in modified mesh - rectify this to access the merge option", MessageType.Warning);
                if (GUILayout.Button ("Merge submeshes/materials"))
                {
                    List<int> triangles = new List<int> ();
                    for (int submeshIndex = 0; submeshIndex < t.meshModified.subMeshCount; ++submeshIndex)
                    {
                        int[] trianglesOfSubmesh = t.meshModified.GetTriangles (submeshIndex);
                        triangles.AddRange (trianglesOfSubmesh);
                    }
                    t.meshModified.SetTriangles (triangles, 0);
                    t.meshModified.subMeshCount = 1;
                    mr.sharedMaterials = new Material[] { mr.sharedMaterials[0] };
                }
                GUI.enabled = true;
            }

            if (GUILayout.Button ("Save modified mesh"))
            {
                SaveMesh (t, mf);
            }

            if (GUILayout.Button ("Remove"))
            {
                DestroyImmediate (t);
            }
        }
    }

    private void SaveMesh (MeshTool t, MeshFilter mf)
    {
        string directory;
        if (string.IsNullOrEmpty (lastSavePath))
        {
            directory = Application.dataPath;
        }
        else
        {
            directory = Path.GetDirectoryName (lastSavePath);
        }

        if (string.IsNullOrEmpty (lastFileName))
            lastFileName = "modified_mesh.asset";

        string pathFromPanel = EditorUtility.SaveFilePanel ("Save new mesh", directory, lastFileName, "asset");
        if (!string.IsNullOrEmpty (pathFromPanel))
        {
            int pathFromPanelTrimIndex = Application.dataPath.Length - 6;
            string pathFromPanelTrimmed = pathFromPanel.Substring (pathFromPanelTrimIndex);
            string filenameFromPanel = Path.GetFileNameWithoutExtension (pathFromPanel);

            t.meshModified.name = filenameFromPanel;
            t.meshModified = UtilityAssetDatabase.CreateAssetSafely (t.meshModified, pathFromPanelTrimmed);
            mf.sharedMesh = t.meshModified;

            lastSavePath = pathFromPanel;
            lastFileName = filenameFromPanel;

            Debug.Log ("Path from panel: " + pathFromPanel + " | Trimmed: " + pathFromPanelTrimmed + " | Last filename: " + lastFileName + " | Directory path: " + directory);
        }
    }

    /*
    public void MergeDuplicateVertices (MeshTool t)
    {
        if (t == null || mf == null || mr == null)
            return;

        MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer> ();
        MeshFilter mf = t.gameObject.GetComponent<MeshFilter> ();

        if (mr == null || mf == null)
        {
            Debug.LogWarning ("MP | RevertToOriginal | Filter or renderer is absent from this object");
            return;
        }

        if (t.meshModified == null)
        {
            Debug.LogWarning ("MP | RevertToOriginal | Modified mesh not available for use");
            return;
        }

        int vertexCountOld = t.meshModified.vertexCount;
        Vector3[] positionsOld = t.meshModified.vertices;
        Vector3[] normalsOld = t.meshModified.normals;
        Vector4[] tangentsOld = t.meshModified.tangents;
        Vector2[] uvOld = t.meshModified.uv;
        Color[] colorsOld = t.meshModified.colors.Length == vertexCountOld ? t.meshModified.colors : new Color[vertexCountOld];
        int[] trianglesOld = t.meshModified.triangles;

        List<Vector3> positionsNew = new List<Vector3> ();
        List<Vector3> normalsNew = new List<Vector3> ();
        List<Vector4> tangentsNew = new List<Vector4> ();
        List<Vector2> uvNew = new List<Vector2> ();
        List<Color> colorsNew = new List<Color> ();
        List<int> trianglesNew = new List<int> ();

        Dictionary<int, int> matchDictionary = new Dictionary<int, int> ();

        for (int a = 0; a < positionsOld.Length; ++a)
        {
            bool matchExists = false;

            Vector3 positionA = positionsOld[a];
            Vector3 normalA = normalsOld[a];

            for (int b = 0; b < positionsOld.Length; ++b)
            {
                Vector3 positionB = positionsOld[b];
                Vector3 normalB = normalsOld[b];

                bool matchedPosition = RoughlyEqual (positionA, positionB, 0.001f);
                bool matchedNormal = RoughlyEqual (normalA, normalB, 0.01f);
                matchExists = matchedPosition && matchedNormal;

                if (matchExists)
                {
                    if (!matchDictionary.ContainsKey (b))
                        matchDictionary.Add (b, a);
                }
            }
        }

        foreach (KeyValuePair<int, int> kvp in matchDictionary)
        {
            int indexToReplace = kvp.Key;
            int indexReplacement = kvp.Value;

            for (int a = 0; a < trianglesOld.Length; ++a)
            {
                int indexInTriangle = trianglesOld[a];
                if (indexInTriangle == indexToReplace)
                {
                    trianglesOld[a] = indexReplacement;
                }
            }
        }

        Dictionary<int, int> indexDictionary = new Dictionary<int, int> ();

        for (int i = 0; i < positionsOld.Length; ++i)
        {
            positionsNew.Add (positionsOld[i]);
            normalsNew.Add (normalsOld[i]);
            tangentsNew.Add (tangentsOld[i]);
            uvNew.Add (uvOld[i]);
            colorsNew.Add (colorsOld[i]);
            indexDictionary.Add (i, positionsNew.Count - 1);
        }

        for (int i = 0; i < trianglesOld.Length; ++i)
        {
            int indexOld = trianglesOld[i];
            int indexNew = indexDictionary[indexOld];
            trianglesNew.Add (indexNew);
        }

        Debug.Log
        (
            "MP | MergeDuplicateVertices | Operating on " + t.meshModified.name +
            " | Vertices: " + positionsOld.Length + " -> " + positionsNew.Count +
            " | Normals: " + normalsOld.Length + " -> " + normalsNew.Count +
            " | Tangents: " + tangentsOld.Length + " -> " + tangentsNew.Count +
            " | UVs: " + uvOld.Length + " -> " + uvNew.Count +
            " | Triangles: " + trianglesOld.Length + " -> " + trianglesNew.Count
        );

        Mesh meshNew = Instantiate (t.meshModified);
        meshNew.vertices = positionsNew.ToArray ();
        meshNew.normals = normalsNew.ToArray ();
        meshNew.tangents = tangentsNew.ToArray ();
        meshNew.uv = uvNew.ToArray ();
        meshNew.colors = colorsNew.ToArray ();
        meshNew.triangles = trianglesNew.ToArray ();
        meshNew.name = t.meshModified.name;

        t.meshModified = meshNew;
        mf.sharedMesh = t.meshModified;
    }

    public void RevertToOriginal (MeshTool t)
    {
        MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer> ();
        MeshFilter mf = t.gameObject.GetComponent<MeshFilter> ();

        if (mr == null || mf == null)
        {
            Debug.LogWarning ("MP | RevertToOriginal | Filter or renderer is absent from this object");
            return;
        }

        if (t.meshOriginal != null)
            mf.sharedMesh = t.meshOriginal;
        else
        {
            Debug.LogWarning ("MP | RevertToOriginal | No original mesh reference available");
        }
    }

    private bool RoughlyEqual (Vector3 a, Vector3 b, float tolerance)
    {
        bool equalX = UtilityMath.NearlyEqual (a.x, b.x, tolerance);
        bool equalY = UtilityMath.NearlyEqual (a.y, b.y, tolerance);
        bool equalZ = UtilityMath.NearlyEqual (a.z, b.z, tolerance);
        return equalX && equalY && equalZ;
    }
    */
}
