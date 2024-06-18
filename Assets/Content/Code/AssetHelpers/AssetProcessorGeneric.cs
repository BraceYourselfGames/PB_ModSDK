using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AssetProcessorFile
{
    public string path;
    public System.IO.FileInfo fileInfo;
    public bool import = false;
}

[System.Serializable]
public class AssetProcessorMaterialReplacement
{
    public Material replacementMaterial = null;
    public string textureName = "name";
}

public static class AssetProcessorGeneric
{
    #if UNITY_EDITOR

    public static List<AssetProcessorFile> GetFilesFromFolder (string path, List<string> extensions = null, List<string> exceptions = null, List<AssetProcessorFile> fileDescriptionsOld = null)
    {
        Debug.Log ("APG | GetFilesFromFolder | Path: " + path);
        if (string.IsNullOrEmpty (path) || !path.StartsWith ("Assets/") || !AssetDatabase.IsValidFolder (path))
        {
            Debug.LogWarning ("APG | GetFilesFromFolder | Provided path is either null, empty, not started correctly or invalid: " + path);
            return null;
        }

        if (path.EndsWith ("/"))
            path = path.TrimEnd ('/');

        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo (path);
        List<System.IO.FileInfo> files = new List<System.IO.FileInfo> ();
        for (int i = 0; i < extensions.Count; ++i)
        {
            System.IO.FileInfo[] filesWithExtension = dirInfo.GetFiles ("*." + extensions[i]);
            List<System.IO.FileInfo> filesFiltered = new List<System.IO.FileInfo> (filesWithExtension.Length);

            for (int a = 0; a < filesWithExtension.Length; ++a)
            {
                bool excepted = false;
                for (int b = 0; b < exceptions.Count; ++b)
                {
                    if (filesWithExtension[a].FullName.Contains (exceptions[b]))
                    {
                        excepted = true;
                        break;
                    }
                }
                if (!excepted)
                    filesFiltered.Add (filesWithExtension[a]);
            }
            files.AddRange (filesFiltered);
        }

        List<AssetProcessorFile> fileDescriptions = new List<AssetProcessorFile> ();
        for (int i = 0; i < files.Count; ++i)
        {
            AssetProcessorFile apf = new AssetProcessorFile ();
            apf.fileInfo = files[i];
            apf.path = "Assets" + apf.fileInfo.FullName.Replace (@"\", "/").Replace (Application.dataPath, "");
            apf.import = false;
            fileDescriptions.Add (apf);
        }

        if (fileDescriptionsOld != null)
        {
            for (int i = 0; i < fileDescriptions.Count; ++i)
            {
                AssetProcessorFile apf = fileDescriptions[i];
                for (int o = 0; o < fileDescriptionsOld.Count; ++o)
                {
                    AssetProcessorFile apfOld = fileDescriptionsOld[o];
                    if (apf.path.Equals (apfOld.path))
                        apf.import = apfOld.import;
                }
            }
        }

        return fileDescriptions;
    }

    public static List<GameObject> GetInstancesFromFiles (List<AssetProcessorFile> files, Transform parent, int depth)
    {
        if (files == null)
        {
            Debug.LogWarning ("APG | GetProcessedObjectsFromFiles | Provided files list was null");
            return null;
        }

        // Create the instance list and start iterating through file descriptions
        List<GameObject> instances = new List<GameObject> ();
        for (int i = 0; i < files.Count; ++i)
        {
            // First, grab and validate the file description
            AssetProcessorFile apf = files[i];
            if (!apf.import)
                continue;

            if (string.IsNullOrEmpty (apf.path))
            {
                Debug.LogWarning ("APG | GetProcessedObjectsFromFiles | File description " + i + " contains no file info and/or no file path, skipping...");
                continue;
            }

            // Fetch the GameObject asset at the file path
            GameObject asset = AssetDatabase.LoadAssetAtPath (apf.path, (typeof (GameObject))) as GameObject;
            Debug.Log ("APG | GetProcessedObjectsFromFiles | Loading asset: " + apf.path + " | Asset is " + (asset == null ? "null" : "present with " + asset.transform.childCount + " children"));

            // Find the transforms at required depth within the GameObject hierarchy
            List<Transform> assetTransforms = new List<Transform> ();
            GetChildrenAtDepthRecursive (assetTransforms, asset.transform, depth);
            Debug.Log ("APG | GetProcessedObjectsFromFiles | Child count from requested transform depth of " + depth + ": " + assetTransforms.Count);

            // Instantiate each found object
            for (int t = 0; t < assetTransforms.Count; ++t)
            {
                GameObject source = assetTransforms[t].gameObject;
                GameObject instance = GameObject.Instantiate (source);
                instance.transform.ParentWithAlignment (parent);
                instance.name = source.name;
                instances.Add (instance);
            }
        }

        return instances;
    }

    private static void GetChildrenAtDepthRecursive (List<Transform> list, Transform parent, int depth)
    {
        if (parent == null)
            return;

        if (depth == 0)
        {
            list.Add (parent);
            return;
        }

        depth -= 1;
        for (int i = 0; i < parent.childCount; ++i)
            GetChildrenAtDepthRecursive (list, parent.GetChild (i), depth);
    }

    public static void ReplaceMaterialsRecursive (Transform parent, List<AssetProcessorMaterialReplacement> replacements)
    {
        // Check the replacement list, since it's pointless to continue without it
        if (replacements == null || replacements.Count == 0 || replacements[0].replacementMaterial == null)
        {
            Debug.LogWarning ("APG | ReplaceMaterialsRecursive | Received material replacement list is either null or empty, aborting...");
            return;
        }

        // Invoke the same method again for each child
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild (i);
            ReplaceMaterialsRecursive (child, replacements);
        }

        // Grab a MeshRenderer and only continue if it's present, since we're working with materials
        MeshRenderer mr = parent.GetComponent<MeshRenderer> ();
        if (mr == null)
            return;

        // Create a new material array with the length of existing one
        Material[] materialsNew = new Material[mr.sharedMaterials.Length];
        for (int i = 0; i < mr.sharedMaterials.Length; ++i)
        {
            // Grab the material in question and create a bool to check whether a replacement was performed
            Material materialOld = mr.sharedMaterials[i];
            bool replacementPerformed = false;

            // Check if an encountered material is null - if it is, just give the entry to the first replacement material
            if (materialOld == null)
            {
                Debug.LogWarning ("APG | ReplaceMaterialsRecursive | Material " + i + " in " + parent.name + " is null, assigning replacement 0");
                materialsNew[i] = replacements[0].replacementMaterial;
                replacementPerformed = true;
            }

            // If the material isn't null, it's worth checking it's texture 
            else
            {
                // If there isn't one, then we can't really check matches to the replacement list, so we just assign the first one
                if (materialOld.mainTexture == null)
                {
                    Debug.LogWarning ("APG | ReplaceMaterialsRecursive | Material " + i + " in " + parent.name + " had no texture, assigning replacement 0");
                    materialsNew[i] = replacements[0].replacementMaterial;
                    replacementPerformed = true;
                }

                // If the texture is present, we can do proper replacement by checking for texture name matches
                else
                {
                    for (int m = 0; m < replacements.Count; ++m)
                    {
                        if (materialOld.mainTexture.name.Equals (replacements[m].textureName))
                        {
                            materialsNew[i] = replacements[m].replacementMaterial;
                            replacementPerformed = true;
                            break;
                        }
                    }
                }
            }

            // If no replacement occurred, we have to fall back to keeping the old material
            if (!replacementPerformed)
            {
                Debug.LogWarning ("APG | ReplaceMaterialsRecursive | Material " + i + " in " + parent.name + " had no match in the replacement list!");
                materialsNew[i] = materialOld;
            }
        }

        // Finally, we assign the new material array to the mesh renderer
        mr.sharedMaterials = materialsNew;
    }

    public static List<GameObject> GetMergedInstances (List<GameObject> instancesSource, bool removeSource, bool combineToSubmeshes)
    {
        List<GameObject> instancesMerged = new List<GameObject> ();
        for (int i = 0; i < instancesSource.Count; ++i)
        {
            GameObject instanceSource = instancesSource[i];
            if (instanceSource != null)
                instancesMerged.Add (GetMergedInstance (instanceSource.transform, removeSource, combineToSubmeshes));
        }

        return instancesMerged;
    }

    private static GameObject GetMergedInstance (Transform source, bool removeSource, bool combineToSubmeshes)
    {
        GameObject mergedObject = CombineMeshes.CombineChildren (source, source.parent);

        // Check for combineToSubmeshes bool once SaveAssets method supports multi-mesh object export
        if (combineToSubmeshes || !combineToSubmeshes)
        {
            Vector3 sourceParentPositionOld = source.parent.position;
            source.parent.position = Vector3.zero;

            MeshFilter[] filters = mergedObject.GetComponentsInChildren<MeshFilter> ();
            CombineInstance[] instances = new CombineInstance[filters.Length];
            List<Material> materials = new List<Material> ();

            for (int i = 0; i < filters.Length; ++i)
            {
                instances[i].mesh = filters[i].sharedMesh;
                instances[i].transform = filters[i].transform.worldToLocalMatrix;

                Material material = filters[i].gameObject.GetComponent<MeshRenderer> ().sharedMaterial;
                if (!materials.Contains (material))
                    materials.Add (material);
            }

            Mesh meshWithSubmeshes = new Mesh ();
            meshWithSubmeshes.CombineMeshes (instances, false);

            MeshFilter mf = mergedObject.AddComponent<MeshFilter> ();
            MeshRenderer mr = mergedObject.AddComponent<MeshRenderer> ();
            UtilityGameObjects.ClearChildren (mergedObject.transform);

            mf.sharedMesh = meshWithSubmeshes;
            mr.sharedMaterials = materials.ToArray ();
            source.parent.position = sourceParentPositionOld;
        }

        if (removeSource)
            GameObject.DestroyImmediate (source.gameObject);

        return mergedObject;
    }

    private static void ParentWithAlignment (this Transform t, Transform parent)
    {
        t.parent = parent;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    public delegate void SaveAssetDelegate (string path, GameObject instance);

    public static void SaveAssetsFromInstances (string path, List<GameObject> instances, SaveAssetDelegate optionalOperation = null)
    {
        // No point continuing without objects
        if (instances == null || instances.Count == 0)
        {
            Debug.LogWarning ("APG | SaveAssetsFromInstances | Instance list is null or empty, aborting...");
            return;
        }

        // Set up the progress bar first, since export can be a lengthy process with no way to redraw Editor UI
        string progressBarHeader = "Saving " + instances.Count + " instances";
        float progressBarPercentage = 0.0f;
        EditorUtility.DisplayProgressBar (progressBarHeader, "Starting...", progressBarPercentage);

        // Next iterate through all received objects
        for (int i = 0; i < instances.Count; ++i)
        {
            // Grab and validate an object
            GameObject instance = instances[i];
            if (instance == null)
                continue;

            // Update the progress bar
            progressBarPercentage = Mathf.Min (1f, (float)(i + 1) / (float)instances.Count);
            string progressDesc = (int)(progressBarPercentage * 100f) + "% done | Processing object: " + instance.name;
            EditorUtility.DisplayProgressBar (progressBarHeader, progressDesc, progressBarPercentage);

            // Grab a MeshRenderer - no point to proceed without it
            MeshFilter mf = instance.GetComponent<MeshFilter> ();
            if (mf == null)
                continue;

            // Create an asset name (with a suffix denoting the asset type, since Resources.Load doesn't support paths with extensions)
            string pathMesh = path + "/" + instance.name + "_mesh.asset";
            Mesh meshFromAsset = UtilityAssetDatabase.CreateAssetSafely (mf.sharedMesh, pathMesh);
            if (meshFromAsset != null)
                mf.sharedMesh = meshFromAsset;
            else
            {
                Debug.LogWarning ("APG | SaveAssetsFromInstances | Failed to load mesh " + pathMesh + ", skipping further operations on instance " + i);
                continue;
            }

            // Next, we need to create the prefab and swap GameObject/MeshFilter references to new ones from freshly loaded objects
            string pathPrefab = path + "/" + instance.name + "_prefab.prefab";
            instance = PrefabUtility.CreatePrefab (pathPrefab, instance, ReplacePrefabOptions.Default);
            mf = instance.GetComponent<MeshFilter> ();

            // If there is an optional operation defined, now is the time to run it: for example, to parse a name and create a scriptable object
            if (optionalOperation != null)
                optionalOperation.Invoke (path, instance);
        }

        // Finally, time to run all the usual post-operations
        AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh ();
        ResourceDatabaseManager.RebuildDatabase ();
        EditorUtility.ClearProgressBar ();
    }

#endif
}
