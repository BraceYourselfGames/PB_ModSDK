using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class UtilityAssetDatabase
{
    #if UNITY_EDITOR
    private const char slash = '/';

    public static T CreateAssetSafely<T> (T savedAsset, string path, bool load = true) where T : Object
    {
        string[] pathSplit = path.Split (slash);
        ValidateFolderRecursively (pathSplit, pathSplit.Length - 1);

        T existingAsset = AssetDatabase.LoadAssetAtPath (path, typeof (T)) as T;
        if (existingAsset == null)
        {
            // if asset is not present, it's safe to use CreateAsset method without risking broken references
            AssetDatabase.CreateAsset (savedAsset, path);
            if (load)
                existingAsset = AssetDatabase.LoadAssetAtPath (path, typeof (T)) as T;
        }
        else
        {
            // if asset is present, we can use serialized copy method to gracefully replace it's content
            if (typeof (T) == typeof (Mesh))
            {
                // if the saved asset is Mesh, we need to clear it before writing to sidestep Unity bug with Mesh reloading
                Mesh existingMesh = (Mesh)(object)existingAsset;
                Mesh savedMesh = (Mesh)(object)savedAsset;

                // we have to be careful not to apply this workaround to the saved data if saved and existing asset are the same
                // in that case, we have to create a copy and preserve it to avoid clearing saved data
                if (existingMesh == savedMesh)
                {
                    Mesh savedMeshCopy = Object.Instantiate (savedMesh);
                    savedMeshCopy.name = savedMesh.name;
                    savedAsset = savedMeshCopy as T;
                }
                existingMesh.Clear ();
            }
            EditorUtility.CopySerialized (savedAsset, existingAsset);
        }
        return existingAsset;
    }

    private static void ValidateFolderRecursively (string[] pathSplit, int offset)
    {
        if (offset == 1)
            return;

        StringBuilder sb = new StringBuilder ();
        for (int i = 0; i < pathSplit.Length - offset; ++i)
        {
            if (i != 0)
                sb.Append (slash);
            sb.Append (pathSplit[i]);
        }

        string pathStep = sb.ToString ();
        string pathName = pathSplit[pathSplit.Length - offset];

        if (!AssetDatabase.IsValidFolder (pathStep + slash + pathName))
            AssetDatabase.CreateFolder (pathStep, pathName);

        ValidateFolderRecursively (pathSplit, offset - 1);
    }
    #endif
}
