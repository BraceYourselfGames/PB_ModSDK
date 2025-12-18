using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using PhantomBrigade.ModTools;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    #if UNITY_EDITOR

    using Data;

    public static class ModToolsHelper
    {
        public const string unityVersionExpectedMajor = "2020.3";
        public const string unityVersionExpected = "2020.3.49f1";

        public static bool IsUnityVersionSupported (bool strict = true)
        {
            var unityVersion = Application.unityVersion;
            bool match =
                strict ?
                string.Equals (unityVersion, unityVersionExpected, StringComparison.Ordinal) :
                unityVersion.Contains (unityVersionExpectedMajor, StringComparison.Ordinal);

            return match;
        }

        public static bool ValidateModID (string id, DataContainerModData modSource, IDictionary<string, DataContainerModData> mods, out string errorDesc)
        {
            if (string.IsNullOrWhiteSpace (id))
            {
                errorDesc = "Mod ID should not be is null, empty or whitespace";
                return false;
            }
            if (!UtilitiesYAML.IsDirectoryNameValid (id, out errorDesc))
            {
                return false;
            }
            if (id.Any (char.IsWhiteSpace))
            {
                errorDesc = "Mod ID should not have any whitespace characters";
                return false;
            }
            if (id.Any (c => char.IsPunctuation (c) && (c != '_' && c != '.')))
            {
                errorDesc = "Mod ID should not contain any punctuation except . and _";
                return false;
            }
            if (char.IsPunctuation (id[0]) || char.IsPunctuation (id[id.Length - 1]))
            {
                errorDesc = "Mod ID can not start or end with punctuation";
                return false;
            }

            if (mods != null)
            {
                foreach (var kvp in mods)
                {
                    var idOther = kvp.Key;
                    var modOther = kvp.Value;

                    // If this request originates from a registered mod, skip comparison to that mod
                    if (modSource != null && modSource == modOther)
                        continue;

                    if (string.Equals (id, idOther))
                    {
                        errorDesc = "Mod ID can't match an already registered mod";
                        return false;
                    }
                }
            }

            errorDesc = "";
            return true;
        }

        public static void SaveMod (DataContainerModData modData) => DataManagerMod.SaveMod (modData);

        public static void CopyConfigDB (DirectoryInfo source, string dest)
        {
            foreach (var f in source.EnumerateFiles ())
            {
                var destPath = DataPathHelper.GetCombinedCleanPath (dest, f.Name);
                f.CopyTo (destPath, true);
            }
            foreach (var d in source.EnumerateDirectories ())
            {
                var destPath = DataPathHelper.GetCombinedCleanPath (dest, d.Name);
                Directory.CreateDirectory (destPath);
                CopyConfigDB (d, destPath);
            }
        }

        public static void GenerateAssetBundles (DataContainerModData modData)
        {
            if (modData?.assetBundles?.bundleDefinitions == null || modData.assetBundles.bundleDefinitions.Count == 0)
                return;

            if (modData.metadata == null || !modData.metadata.includesAssetBundles)
            {
                Debug.LogWarning ("Skipping building asset bundles due to metadata specifying `includesAssetBundles: false`");
                return;
            }

            var buildPathFinal = Path.Combine (modData.GetModPathProject (), DataContainerModData.assetBundlesFolderName);
            var buildPathTemp = "Temp/AssetBundleBuilds";

            if (!IsUnityVersionSupported (true))
            {
                Debug.LogError ($"Warning! Exported assets will only be loaded by the game if your Editor version exactly matches the engine version of the game. Game engine version: {unityVersionExpected}. Editor engine version: {Application.unityVersion}");
                // return; // Probably best not to return on this so that folks can catch additional errors and learn how the whole process executes
            }

            Debug.Log ($"Building asset bundles | Unity version: {Application.unityVersion}:\n- Temp folder:{buildPathTemp}\n- Final folder: {buildPathFinal}");
            ModToolsAssetBundles.BuildAllAssetBundlesFromList (buildPathTemp, modData.assetBundles.bundleDefinitions, buildPathFinal);
        }
        
        public static void GenerateLibraries (DataContainerModData modData)
        {
            if (modData == null)
            {
                return;
            }

            if (modData.metadata == null)
            {
                return;
            }
            if (!modData.metadata.includesLibraries)
            {
                return;
            }
            if (modData.libraryDLLs == null)
            {
                return;
            }
            if (modData.libraryDLLs.files.Count == 0)
            {
                return;
            }

            var pathLibraries = DataPathHelper.GetCombinedCleanPath (modData.GetModPathProject (), DataContainerModData.librariesFolderName);
            if (!Directory.Exists (pathLibraries))
            {
                Directory.CreateDirectory (pathLibraries);
            }
            foreach (var dll in modData.libraryDLLs.files)
            {
                if (dll == null || string.IsNullOrEmpty (dll.path) || !dll.enabled)
                    continue;
                
                var pathSource = dll.GetFinalPath ();
                var fileSource = new FileInfo (pathSource);
                
                if (!fileSource.Exists)
                {
                    Debug.Log ($"External library file doesn't exist: {pathSource}");
                    continue;
                }
                
                var filename = Path.GetFileName (pathSource);
                var pathDest = DataPathHelper.GetCombinedCleanPath (pathLibraries, filename);
                
                File.Copy (pathSource, pathDest, true);
                Debug.Log ($"Copying external DLL into Libraries...\nFrom: {pathSource}\nTo: {pathDest}");
            }
        }
    }

    #endif
}
