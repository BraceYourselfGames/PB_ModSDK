using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataHelperAssetBundle : MonoBehaviour
{
    [FolderPath]
    public string assetBundlePath = "Assets/AssetBundles";
    
    #if UNITY_EDITOR
    
    [Button (SdfIconType.Download, "Build", ButtonHeight = (int)ButtonSizes.Large)]
    private void BuildAllAssetBundles()
    {
        if (!Directory.Exists (assetBundlePath))
        {
            Debug.LogWarning ($"Asset bundle directory doesn't exist: {assetBundlePath}");
            return;
        }
        
        BuildPipeline.BuildAssetBundles (assetBundlePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
    
    #endif
}
