using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (AssetProcessorTileset))]
public class AssetProcessorTilesetInspector : Editor
{
    private bool foldout = true;

    public override void OnInspectorGUI ()
    {
        AssetProcessorTileset t = (AssetProcessorTileset) target;

        string pathImport = EditorGUILayout.TextField ("Import folder", t.pathImport);
        string pathExport = EditorGUILayout.TextField ("Export folder", t.pathExport);

        EditorGUILayout.BeginVertical ();
        if (GUILayout.Button ("Load files"))
            t.LoadFiles ();
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.BeginVertical ();
        if (GUILayout.Button ("Rebuild", GUILayout.Height (102f)))
            t.Rebuild ();
        EditorGUILayout.EndVertical ();
        EditorGUILayout.BeginVertical ();
        if (GUILayout.Button ("Reset"))
            t.Reset ();
        if (GUILayout.Button ("Instantiate"))
            t.Instantiate ();
        if (GUILayout.Button ("Replace materials"))
            t.ReplaceMaterials ();
        if (GUILayout.Button ("Merge"))
            t.Merge ();
        if (GUILayout.Button ("Save"))
            t.Save ();
        EditorGUILayout.EndVertical ();
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.EndVertical ();
        
        GUILayout.BeginVertical ("Box");
        GUILayout.BeginHorizontal ();
        if (GUILayout.Button ("Toggle all", EditorStyles.miniButton))
            t.ToggleImportFlags ();
        GUILayout.FlexibleSpace ();
        if (GUILayout.Button ("Models " + (foldout ? "■" : "▼"), EditorStyles.miniLabel))
            foldout = !foldout;
        GUILayout.EndHorizontal ();
        if (foldout)
        {
            for (int i = 0; i < t.files.Count; ++i)
            {
                GUILayout.BeginHorizontal ();
                t.files[i].import = EditorGUILayout.Toggle (t.files[i].import, GUILayout.MinWidth (20f), GUILayout.MaxWidth (20f));
                GUILayout.Label (t.files[i].path.Replace (t.pathImport + "/", string.Empty), EditorStyles.miniLabel);
                GUILayout.EndHorizontal ();
            }
        }
        GUILayout.EndVertical ();

        if (GUI.changed)
        {
            Undo.RecordObject (t, "AssetProcessor settings changed");
            t.pathImport = pathImport;
            t.pathExport = pathExport;
            GUI.changed = false;
        }

        DrawDefaultInspector ();
    }
}
