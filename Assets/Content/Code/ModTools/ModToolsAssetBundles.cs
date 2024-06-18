using System;
using System.Collections.Generic;
using System.IO;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PhantomBrigade.ModTools
{
    [Serializable]
    public class AssetBundleDesc
    {
        [HideLabel, HorizontalGroup (20f)]
        public bool enabled = true;
        
        [EnableIf ("enabled")]
        [HorizontalGroup, HideLabel]
        public string name;

        [EnableIf ("enabled")]
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new AssetBundleDescFile ()", ElementColor = "GetElementColor")]
        public List<AssetBundleDescFile> files = new List<AssetBundleDescFile> ();
        
        #region Editor
        #if UNITY_EDITOR
	    
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
	    
        #endif
        #endregion
    }
    
    [Serializable, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class AssetBundleDescFile
    {
        // [HideLabel, HorizontalGroup (20f)]
        // public bool enabled = true;

        [Sirenix.OdinInspector.FilePath (AbsolutePath = false, IncludeFileExtension = true, RequireExistingPath = true, UseBackslashes = false)]
        [HideLabel]
        public string path;
    }
    
    public static class ModToolsAssetBundles
    {
        public static void BuildAllAssetBundles (string buildPath, string buildPathExternal = null)
        {
            #if UNITY_EDITOR
            
            if (!Directory.Exists (buildPath))
            {
                Debug.LogWarning ($"Asset bundle build directory doesn't exist: {buildPath}");
                return;
            }

            var manifest = BuildPipeline.BuildAssetBundles (buildPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            OnAfterBuild (manifest, buildPath, buildPathExternal);
            
            #endif
        }
        
        public static void BuildAllAssetBundlesFromList (string buildPath, List<AssetBundleDesc> bundleDefinitions, string buildPathExternal = null)
        {
            #if UNITY_EDITOR
            
            if (bundleDefinitions == null || bundleDefinitions.Count == 0)
            {
                Debug.LogWarning ($"No bundle definitions found, can't proceed with the build");
                return;
            }

            var buildMap = new List<AssetBundleBuild> ();

            for (int i = 0; i < bundleDefinitions.Count; ++i)
            {
                var definition = bundleDefinitions[i];
                if (definition == null)
                {
                    Debug.LogWarning ($"Bundle definition {i} is null");
                    continue;
                }

                if (string.IsNullOrEmpty (definition.name))
                {
                    Debug.LogWarning ($"Bundle definition {i} has no defined name");
                    continue;
                }

                if (!UtilitiesYAML.IsDirectoryNameValid (definition.name, out var errorDescDir))
                {
                    Debug.LogWarning ($"Bundle definition {i} has invalid name {definition.name}: {errorDescDir}");
                    continue;
                }
                
                UtilitiesYAML.PrepareClearDirectory (buildPath, false, true);

                if (definition.files == null || definition.files.Count == 0)
                {
                    Debug.LogWarning ($"Bundle definition {i} has no files");
                    continue;
                }

                var paths = new List<string> ();
                for (int f = 0; f < definition.files.Count; ++f)
                {
                    var file = definition.files[f];
                    if (file == null)
                    {
                        Debug.LogWarning ($"Bundle definition {i} file {f} is null");
                        continue;
                    }

                    // if (!file.enabled)
                    //     continue;

                    if (string.IsNullOrEmpty (file.path))
                    {
                        Debug.LogWarning ($"Bundle definition {i} file {f} has no path");
                        continue;
                    }

                    var filename = Path.GetFileNameWithoutExtension (file.path);
                    if (!UtilitiesYAML.IsFileNameValid (filename, out var errorDescFile))
                    {
                        Debug.LogWarning ($"Bundle definition {i} file {f} has invalid name {definition.name}: {errorDescFile}");
                        continue;
                    }

                    paths.Add (file.path);
                }

                if (paths.Count == 0)
                {
                    Debug.LogWarning ($"Bundle definition {i} has no files");
                    continue;
                }

                buildMap.Add (new AssetBundleBuild
                {
                    assetBundleName = definition.name,
                    assetNames = paths.ToArray ()
                });

                var manifest = BuildPipeline.BuildAssetBundles (buildPath, buildMap.ToArray (), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
                OnAfterBuild (manifest, buildPath, buildPathExternal);
            }
            
            #endif
        }

        private static void OnAfterBuild (AssetBundleManifest manifest, string buildPath, string buildPathExternal = null)
        {
            #if UNITY_EDITOR
            
            if (manifest == null)
            {
                Debug.LogWarning ($"Asset bundle build might have failed: no manifest returned");
                return;
            }

            var bundleNames = manifest.GetAllAssetBundles ();
            if (bundleNames == null || bundleNames.Length == 0)
            {
                Debug.LogWarning ($"Asset bundle build finished with no outputs. Make sure you mark at least one asset into an asset bundle using the bottom right menu in the asset inspector.");
                return;
            }

            Debug.Log ($"Build finished! Bundles found:\n{bundleNames.ToStringFormatted (true, multilinePrefix: "- ")}");

            for (int i = 0; i < bundleNames.Length; ++i)
            {
                var bundleName = bundleNames[i];
                var di = manifest.GetAllDependencies (bundleName);
                Debug.Log ($"Bundle {i} - {bundleName} | {di.Length} total dependencies");
            }

            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh (UnityEditor.ImportAssetOptions.ForceSynchronousImport);

            if (!string.IsNullOrEmpty (buildPathExternal))
            {
                var buildPathAbsolute = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), buildPath);
                Debug.Log ($"Attempting to copy build:\n- Temp folder: {buildPath}\n- Final folder: {buildPathExternal}");

                try
                {
                    UtilitiesYAML.CopyDirectory (buildPathAbsolute, buildPathExternal, true);
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                    throw;
                }
            }
            
            #endif
        }
    }
}
