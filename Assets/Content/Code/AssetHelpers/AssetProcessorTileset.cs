using UnityEngine;
using System.Collections.Generic;

public class AssetProcessorTileset : MonoBehaviour
{
    #if UNITY_EDITOR 

    [HideInInspector]
    public string pathImport = "Assets/Content/Folder";

    [HideInInspector]
    public string pathExport = "Assets/Content/Folder";

    public int fileDepth = 1;
    public List<string> fileExtensions = new List<string> (new string[] { "skp" });
    public List<string> fileExceptions = new List<string> ();

    public List<AssetProcessorFile> files = new List<AssetProcessorFile> ();
    public List<AssetProcessorMaterialReplacement> materialReplacements = new List<AssetProcessorMaterialReplacement> ();

    public List<GameObject> instancesOriginal = new List<GameObject> ();
    public List<GameObject> instancesMerged = new List<GameObject> ();

    public void Rebuild ()
    {
        Reset ();
        Instantiate ();
        ReplaceMaterials ();
        Merge ();
        Save ();
    }

    [ContextMenu ("Reset")]
    public void Reset ()
    {
        UtilityGameObjects.ClearChildren (gameObject);
        instancesOriginal.Clear ();
        instancesMerged.Clear ();
    }

    [ContextMenu ("Load files")]
    public void LoadFiles ()
    {
        Debug.Log ("MI | FindFiles");
        files = AssetProcessorGeneric.GetFilesFromFolder (pathImport, extensions: fileExtensions, exceptions: fileExceptions, fileDescriptionsOld: files);
    }

    public void ToggleImportFlags ()
    {
        for (int i = 0; i < files.Count; ++i)
            files[i].import = !files[i].import;
    }

    [ContextMenu ("Get original instances")]
    public void Instantiate ()
    {
        instancesOriginal = AssetProcessorGeneric.GetInstancesFromFiles (files, transform, fileDepth);
    }

    [ContextMenu ("Replace materials")]
    public void ReplaceMaterials ()
    {
        AssetProcessorGeneric.ReplaceMaterialsRecursive (transform, materialReplacements);
    }

    [ContextMenu ("Get merged instances")]
    public void Merge ()
    {
        instancesMerged = AssetProcessorGeneric.GetMergedInstances (instancesOriginal, true, true);
    }

    public void Save ()
    {
        AssetProcessorGeneric.SaveAssetsFromInstances (pathExport, instancesMerged);
    }

    #endif
}
