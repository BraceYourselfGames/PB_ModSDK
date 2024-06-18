using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using PhantomBrigade.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhantomBrigade.ModTools
{
    public class DataHelperAssetBundle : MonoBehaviour
    {
        [FolderPath]
        public string buildPath = "Assets/AssetBundles/Builds";
        
        [ShowInInspector]
        public List<AssetBundleDesc> bundleDefinitions = new List<AssetBundleDesc> ();

        #if UNITY_EDITOR

        [Button (SdfIconType.Check, "Verify directory", ButtonHeight = (int)ButtonSizes.Large)]
        private void VerifyDirectory ()
        {
            if (!Directory.Exists (buildPath))
            {
                Debug.LogWarning ($"Asset bundle build directory doesn't exist: {buildPath}");
                return;
            }

            Debug.Log ("Directory found");
        }

        [Button (SdfIconType.Download, "Build all", ButtonHeight = (int)ButtonSizes.Large)]
        private void BuildAllAssetBundles ()
        {
            ModToolsAssetBundles.BuildAllAssetBundles (buildPath);
        }

        [Button (SdfIconType.Download, "Build from list", ButtonHeight = (int)ButtonSizes.Large)]
        private void BuildAllAssetBundlesFromList ()
        {
            ModToolsAssetBundles.BuildAllAssetBundlesFromList (buildPath, bundleDefinitions);
        }

        #endif
    }
}