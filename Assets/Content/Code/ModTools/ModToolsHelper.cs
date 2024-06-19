using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using PhantomBrigade.ModTools;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    #if UNITY_EDITOR

    using Data;
    using Mods;
    using DataMultiLinkerPairMap = IReadOnlyDictionary<Type, (ConfigChecksums.ConfigDirectory SDK, ConfigChecksums.ConfigDirectory Mod)>;

    public sealed class SDKChecksumData
    {
        public ConfigChecksums.ConfigDirectory ChecksumsRoot;
        public Dictionary<Type, ConfigChecksums.ConfigDirectory> MultiLinkerChecksumMap;
        public Dictionary<Type, ConfigChecksums.ConfigFile> LinkerChecksumMap;
    }

    public enum EnsureResult
    {
        Error = 0,
        Continue,
        Break,
    }

    public static class ModToolsHelper
    {
        public const string unityVersionExpectedMajor = "2020.3";
        public const string unityVersionExpected = "2020.3.34f1";

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

        public static void EnsureSDKChecksums ()
        {
            var sdkDirectory = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
            if (!UtilityDatabaseSerialization.checksumsLoaded && !ConfigChecksums.ChecksumsExist (sdkDirectory))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless (BuildSDKChecksums(sdkDirectory));
                return;
            }
            if (!UtilityDatabaseSerialization.checksumsLoaded)
            {
                UtilityDatabaseSerialization.LoadSDKChecksums ();
                if (!UtilityDatabaseSerialization.checksumsLoaded)
                {
                    Debug.LogWarning ("Failed to load checksums for SDK configs. Try to build them manually by clicking the Create checksums for SDK config DBs button in the DataModel game object.");
                    return;
                }
            }
            if (!UtilityDatabaseSerialization.AnySDKConfigsChecksumChanges ())
            {
                return;
            }
            Debug.Log ("Detected changes to the SDK configs. Starting rebuild of checksums.");
            EditorCoroutineUtility.StartCoroutineOwnerless (BuildSDKChecksums (sdkDirectory));
        }

        public static (bool, ConfigChecksums.Checksum) LoadSDKChecksums (bool force = false)
        {
            if (!force && DataManagerMod.sdkChecksumData != null)
            {
                return (true, DataManagerMod.sdkChecksumData.ChecksumsRoot.Checksum);
            }

            var sdkRootDirectory = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
            var checksumsDeserializer = new ConfigChecksums.Deserializer (sdkRootDirectory);
            try
            {
                var result = checksumsDeserializer.Load ();
                if (!result.OK)
                {
                    Debug.LogError (result.ErrorMessage);
                    return (false, default);
                }
                DataManagerMod.sdkChecksumData = new SDKChecksumData ()
                {
                    ChecksumsRoot = result.Root,
                    MultiLinkerChecksumMap = result.MultiLinkerMap,
                    LinkerChecksumMap = result.LinkerMap,
                };
                return (true, DataManagerMod.sdkChecksumData.ChecksumsRoot.Checksum);
            }
            catch (Exception ex)
            {
                Debug.LogError ("Failed to load SDK configs checksums: " + sdkRootDirectory.FullName);
                Debug.LogException (ex);
                return (false, default);
            }
        }

        public static EnsureResult EnsureModChecksums (DataContainerModData modData, bool showDialogs = false)
        {
            if (DataManagerMod.sdkChecksumData == null || DataManagerMod.sdkChecksumData.ChecksumsRoot == null)
            {
                Debug.LogWarning ("Missing checksums for SDK configs. Go to the DataModel game object and click the button to create the SDK checksums.");
                return EnsureResult.Error;
            }
            if (modData.checksumsRoot != null)
            {
                return EnsureResult.Continue;
            }

            var dirSDK = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
            var dirProject = new DirectoryInfo (modData.GetModPathProject ());
            if (ConfigChecksums.ChecksumsExist (dirProject))
            {
                if (!modData.LoadChecksums (DataManagerMod.sdkChecksumData))
                {
                    return EnsureResult.Error;
                }
                if (ConfigChecksums.ChecksumEqual (modData.originChecksum, DataManagerMod.sdkChecksumData.ChecksumsRoot.Checksum))
                {
                    return EnsureResult.Continue;
                }

                if (showDialogs
                    && !EditorUtility.DisplayDialog
                    (
                        "Config editing", "The config DBs in this mod are outdated and should be refreshed from the SDK configs. This may take a while. Proceed?"
                        + "\n\nProject: " + modData.id
                        + "\nFolder: " + dirProject.FullName,
                        "Proceed",
                        "Cancel"
                    ))
                {
                    modData.UnloadChecksums ();
                    return EnsureResult.Break;
                }
                EditorCoroutineUtility.StartCoroutineOwnerless (UpdateModConfigsIE (dirSDK, modData));
                return EnsureResult.Continue;
            }

            if (showDialogs
                && !EditorUtility.DisplayDialog
                (
                    "Config editing", "This mod project does not appear to have any checksums. Checksums allow the SDK to detect the changes you make to the config DBs and correctly export those changes. It may take a while to build the checksums. Proceed?"
                    + "\n\nProject: " + modData.id
                    + "\nFolder: " + dirProject.FullName,
                    "Proceed",
                    "Cancel"
                ))
            {
                return EnsureResult.Break;
            }

            if (!ConfigChecksums.CopyChecksumsFile (dirSDK, dirProject))
            {
                Debug.LogWarning ("Checksums for mod " + modData.id + " do not exist.");
                return EnsureResult.Error;
            }

            if (!modData.LoadChecksums (DataManagerMod.sdkChecksumData))
            {
                return EnsureResult.Error;
            }
            EditorCoroutineUtility.StartCoroutineOwnerless (UpdateModConfigsIE (dirSDK, modData));
            return EnsureResult.Continue;
        }

        public static bool HasChanges (DataContainerModData modData)
        {
            if (!UtilityDatabaseSerialization.checksumsLoaded)
            {
                if (!ConfigChecksums.ChecksumsExist (new DirectoryInfo (DataPathHelper.GetApplicationFolder ())))
                {
                    Debug.LogWarning ("Checksums for SDK do not exist. Build them by clicking the Create checksums for SDK config DBs button in the DataModel game object");
                }
                return false;
            }
            if (EnsureModChecksums (modData) != EnsureResult.Continue)
            {
                return false;
            }
            if (modData.checksumsRoot == null)
            {
                Debug.LogWarningFormat ("Checksums for mod {0}/{1} do not exist.", modData.key, modData.id);
                return false;
            }
            var sdkRootChecksum = DataManagerMod.sdkChecksumData.ChecksumsRoot.Checksum;
            var modRootChecksum = modData.checksumsRoot.Checksum;
            return !ConfigChecksums.ChecksumEqual (sdkRootChecksum, modRootChecksum);
        }

        public static bool HasChanges (DataContainerModData modData, Type containerType)
        {
            if (DataManagerMod.sdkChecksumData == null || DataManagerMod.sdkChecksumData.ChecksumsRoot == null)
            {
                return false;
            }
            if (modData.multiLinkerChecksumMap == null)
            {
                return false;
            }
            var (ok, pair) = GetChecksumEntryForContainer (modData, containerType);
            return ok && !ConfigChecksums.ChecksumEqual (pair.SDK, pair.Mod);
        }

        public static IDictionary<string, ConfigChecksums.ConfigEntry> GetEntriesByFileName (ConfigChecksums.ConfigDirectory configDir, bool directoryMode) =>
            configDir != null
                ? configDir.Entries
                    .Where (entry => directoryMode ? entry is ConfigChecksums.ConfigDirectory : entry is ConfigChecksums.ConfigFile)
                    .Select (entry => new
                    {
                        Key = Path.GetFileName (entry.RelativePath),
                        Entry = entry,
                    })
                    .ToDictionary (x => x.Key, x => x.Entry)
                : (IDictionary<string, ConfigChecksums.ConfigEntry>)emptyMap;

        public static IDictionary<string, ConfigChecksums.ConfigEntry> GetEntriesByKey (ConfigChecksums.ConfigDirectory configDir, bool directoryMode) =>
            configDir != null
                ? configDir.Entries
                    .Where (entry => directoryMode ? entry is ConfigChecksums.ConfigDirectory : entry is ConfigChecksums.ConfigFile)
                    .Select (entry => new
                    {
                        Key = directoryMode ? Path.GetFileName (entry.RelativePath) : Path.GetFileNameWithoutExtension (entry.RelativePath),
                        Entry = entry,
                    })
                    .ToDictionary (x => x.Key, x => x.Entry)
                : (IDictionary<string, ConfigChecksums.ConfigEntry>)emptyMap;

        public static void CopyConfigDB (DirectoryInfo source, string dest)
        {
            foreach (var f in source.EnumerateFiles ())
            {
                var destPath = Path.Combine (dest, f.Name);
                f.CopyTo (destPath, true);
            }
            foreach (var d in source.EnumerateDirectories ())
            {
                var destPath = Path.Combine (dest, d.Name);
                Directory.CreateDirectory (destPath);
                CopyConfigDB (d, destPath);
            }
        }

        public static IEnumerator CopyConfigsIE (DirectoryInfo root, string dest)
        {
            yield return null;

            var source = new DirectoryInfo (Path.Combine (root.FullName, dirNameConfigs));
            var (topLevel, dataDecomposed) = GetConfigSubdirectories (source);
            copyCount = 1 + topLevel.Count + dataDecomposed.Count;
            copyItem = 0;

            buildChecksums = !buildStarted && !ConfigChecksums.ChecksumsExist(source.Parent);
            if (buildChecksums)
            {
                checksums = new ConfigChecksums.Serializer (source, ConfigChecksums.EntrySource.SDK);
            }

            ReportCopyProgress (dest);
            foreach (var file in source.EnumerateFiles ().OrderBy(fi => fi.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                var destPath = Path.Combine (dest, file.Name);
                file.CopyTo (destPath);
                if (buildChecksums)
                {
                    checksums.AddFile (file);
                }
                yield return null;
            }
            copyItem += 1;

            foreach (var d in topLevel)
            {
                var destPath = Path.Combine (dest, d.FullName.Substring (source.FullName.Length + 1));
                yield return CopyConfigSubdirectoryIE (d, destPath, true, source.FullName.Length + 1);
            }

            var trim = dataDecomposed.First ().FullName.Length + 1;
            dataDecomposed.First().Create();
            foreach (var d in dataDecomposed.Skip(1))
            {
                var destPath = Path.Combine (dest, ConfigChecksums.DataDecomposedDirectoryName, d.FullName.Substring(trim));
                yield return CopyConfigSubdirectoryIE (d, destPath, true, source.FullName.Length + 1);
            }

            if (buildChecksums)
            {
                checksums.Save ();
                checksums = null;
                buildChecksums = false;
            }
            else if (buildStarted)
            {
                while (buildStarted)
                {
                    yield return null;
                }
            }
            ConfigChecksums.CopyChecksumsFile (source.Parent, new DirectoryInfo (Path.GetDirectoryName (dest)));

            EditorUtility.ClearProgressBar ();
        }

        public static IEnumerator UpdateModConfigsIE (DirectoryInfo sdkRoot, DataContainerModData modData)
        {
            var dirSDKConfigs = new DirectoryInfo (Path.Combine (sdkRoot.FullName, dirNameConfigs));
            var dirModConfigs = new DirectoryInfo (modData.GetModPathConfigs ());
            yield return UpdateModConfigsLinkerIE (dirSDKConfigs, dirModConfigs, modData);
            yield return UpdateModConfigsMultilinkerIE (dirSDKConfigs, dirModConfigs, modData);
            yield return SyncModChecksums (modData, DataManagerMod.sdkChecksumData.ChecksumsRoot.Checksum);
        }

        public static IEnumerator DeleteModConfigsIE (string configsPath)
        {
            yield return null;

            var directories = Directory.GetDirectories (configsPath).ToList ();
            for (var i = 0; i < directories.Count; i += 1)
            {
                var dpath = directories[i];
                if (Path.GetFileName (dpath) == ConfigChecksums.DataDecomposedDirectoryName)
                {
                    var subdirectories = Directory.GetDirectories (dpath).SelectMany (Directory.GetDirectories).ToArray ();
                    directories.InsertRange (i, subdirectories);
                    break;
                }
            }
            directories.Add (configsPath);

            var overridesPath = Path.Combine (DataContainerModData.selectedMod.GetModPathProject (), DataContainerModData.overridesFolderName);
            if (Directory.Exists (overridesPath))
            {
                var overrides = Directory.GetDirectories (overridesPath);
                for (var i = 0; i < overrides.Length; i += 1)
                {
                    directories.AddRange (Directory.GetDirectories (overrides[i]));
                }
                directories.Add (overridesPath);
            }

            var count = directories.Count;
            for (var i = 0; i < count; i += 1)
            {
                var pct = 1f * i / count;
                var textHeader = string.Format ("Preparing configs {0:P0}", pct);
                EditorUtility.DisplayProgressBar (textHeader, "Preparing " + new string('.', i % 8 + 1), pct);
                Directory.Delete (directories[i], true);
                yield return null;
            }

            EditorUtility.ClearProgressBar ();
        }

        public static void SaveChecksums (DataContainerModData modData)
        {
            var source = new DirectoryInfo (modData.GetModPathConfigs ());
            var serializer = new ConfigChecksums.Serializer (source, modData.checksumsRoot, modData.originChecksum);
            serializer.Save ();
        }

        public static IEnumerator ChecksumSDKConfigsIE (DirectoryInfo root)
        {
            var source = new DirectoryInfo (Path.Combine (root.FullName, dirNameConfigs));
            var (topLevel, dataDecomposed) = GetConfigSubdirectories (source);
            checksums = new ConfigChecksums.Serializer (source, ConfigChecksums.EntrySource.SDK);
            copyCount = 1 + topLevel.Count + dataDecomposed.Count;
            copyItem = 0;

            ReportChecksumProgress (source.FullName);
            foreach (var file in source.EnumerateFiles ().OrderBy(fi => fi.FullName, StringComparer.InvariantCultureIgnoreCase))
            {
                checksums.AddFile (file);
                yield return null;
            }
            copyItem += 1;
            var trim = source.FullName.Length + 1;
            foreach (var d in topLevel)
            {
                yield return ChecksumSubdirectoryIE (d, trim);
            }
            trim = dataDecomposed.First ().FullName.Length + 1;
            foreach (var d in dataDecomposed.Skip (1))
            {
                yield return ChecksumSubdirectoryIE (d, trim);
            }
            checksums.Save ();
        }

        public static IEnumerator UpdateAllModConfigsIE (ConfigChecksums.Checksum checksumSDKConfigsRoot)
        {
            var dirSDK = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
            var mods = DataManagerMod.GetConfigEditMods ();
            if (!mods.Any ())
            {
                yield break;
            }
            foreach (var modData in mods)
            {
                var dirProject = new DirectoryInfo (modData.GetModPathProject ());
                if (ConfigChecksums.ChecksumsExist (dirProject))
                {
                    if (!modData.LoadChecksums (DataManagerMod.sdkChecksumData))
                    {
                        continue;
                    }
                    if (ConfigChecksums.ChecksumEqual (modData.originChecksum, checksumSDKConfigsRoot))
                    {
                        continue;
                    }
                    yield return UpdateModConfigsIE (dirSDK, modData);
                    continue;
                }

                if (!ConfigChecksums.CopyChecksumsFile (dirSDK, dirProject))
                {
                    Debug.LogWarning ("Checksums for mod " + modData.id + " do not exist.");
                    continue;
                }

                if (!modData.LoadChecksums (DataManagerMod.sdkChecksumData))
                {
                    continue;
                }
                yield return UpdateModConfigsIE (dirSDK, modData);
            }
        }

        static (bool, (ConfigChecksums.ConfigEntry SDK, ConfigChecksums.ConfigEntry Mod)) GetChecksumEntryForContainer (DataContainerModData modData, Type containerType) =>
            modData.multiLinkerChecksumMap.TryGetValue (containerType, out var configDir)
                ? (true, configDir)
                : modData.linkerChecksumMap.TryGetValue (containerType, out var configFile)
                    ? (true, configFile)
                    : (false, ((ConfigChecksums.ConfigEntry)null, (ConfigChecksums.ConfigEntry)null));

        static IEnumerator BuildSDKChecksums (DirectoryInfo sdkDirectory)
        {
            if (buildStarted)
            {
                yield break;
            }

            buildStarted = true;
            var source = new DirectoryInfo (Path.Combine (sdkDirectory.FullName, dirNameConfigs));
            var serializer = new ConfigChecksums.Serializer (source, ConfigChecksums.EntrySource.SDK);
            yield return ChecksumConfigsQuietIE (source, serializer);
            UtilityDatabaseSerialization.LoadSDKChecksums ();
            buildStarted = false;
        }

        static IEnumerator ChecksumConfigsQuietIE (DirectoryInfo source, ConfigChecksums.Serializer serializer)
        {
            var progressID = Progress.Start ("Checksum configs " + Path.GetFileName (source.FullName), "Starting...");
            Progress.SetStepLabel(progressID, "items");
            yield return null;

            var (topLevel, dataDecomposed) = GetConfigSubdirectories (source);
            checksums = serializer;
            copyCount = 1 + topLevel.Count + dataDecomposed.Count;
            copyItem = 0;

            Progress.Report (progressID, copyItem, copyCount, source.Name);
            foreach (var file in source.EnumerateFiles ().OrderBy(fi => fi.FullName, StringComparer.InvariantCultureIgnoreCase))
            {
                checksums.AddFile (file);
                yield return null;
            }
            copyItem += 1;
            var trim = source.FullName.Length + 1;
            foreach (var d in topLevel)
            {
                yield return ChecksumSubdirectoryQuietIE (d, trim, progressID);
            }
            trim = dataDecomposed.First ().FullName.Length + 1;
            foreach (var d in dataDecomposed.Skip (1))
            {
                yield return ChecksumSubdirectoryQuietIE (d, trim, progressID);
            }
            checksums.Save ();
            yield return null;
            Progress.Finish (progressID);
        }

        static (List<DirectoryInfo> TopLevel, List<DirectoryInfo> DataDecomposed)
            GetConfigSubdirectories (DirectoryInfo source)
        {
            var topLevel = source
                .GetDirectories ()
                .ToList ();
            var dataDecomposed = new List<DirectoryInfo> ();
            for (var i = topLevel.Count - 1; i >= 0; i -= 1)
            {
                var d = topLevel[i];
                switch (d.Name)
                {
                    case ConfigChecksums.DataDecomposedDirectoryName:
                        dataDecomposed.Add (d);
                        dataDecomposed.AddRange (d.GetDirectories ("*", SearchOption.AllDirectories));
                        topLevel.RemoveAt (i);
                        break;
                    default:
                        topLevel.InsertRange (i + 1, d.GetDirectories ("*", SearchOption.AllDirectories));
                        break;
                }
            }
            topLevel.Sort (OrderByFullName);
            dataDecomposed.Sort (OrderByFullName);
            return (topLevel, dataDecomposed);
        }

        static IEnumerator CopyConfigIE (DirectoryInfo source, string dest)
        {
            foreach (var f in source.EnumerateFiles ())
            {
                var destPath = Path.Combine (dest, f.Name);
                f.CopyTo (destPath, true);
                yield return null;
            }
            foreach (var d in source.EnumerateDirectories ())
            {
                var destPath = Path.Combine (dest, d.Name);
                Directory.CreateDirectory (destPath);
                CopyConfigDB (d, destPath);
            }
        }

        static int OrderByFullName (DirectoryInfo lhs, DirectoryInfo rhs) => StringComparer.InvariantCultureIgnoreCase.Compare (lhs.FullName, rhs.FullName);

        static void ReportCopyProgress (string pathname)
        {
            var pct = 1f * copyItem / copyCount;
            var textHeader = string.Format ("Copying configs {0}/{1} - {2:P0}", copyItem, copyCount, pct);
            EditorUtility.DisplayProgressBar (textHeader, "Copying " + pathname, pct);
        }

        static void ReportChecksumProgress (string pathname)
        {
            var pct = 1f * copyItem / copyCount;
            var textHeader = string.Format ("Checksum configs {0}/{1} - {2:P0}", copyItem, copyCount, pct);
            EditorUtility.DisplayProgressBar (textHeader, "Copying " + pathname, pct);
        }

        static IEnumerator CopyConfigSubdirectoryIE (DirectoryInfo source, string dest, bool report, int trim)
        {
            var relpath = source.FullName.Substring (trim);
            if (report)
            {
                ReportCopyProgress (relpath);
                copyItem += 1;
            }
            if (buildChecksums)
            {
                checksums.PushBranch (source.FullName);
            }

            Directory.CreateDirectory (dest);
            foreach (var file in source.EnumerateFiles ().OrderBy(fi => fi.FullName, StringComparer.InvariantCultureIgnoreCase))
            {
                var filePath = Path.Combine (dest, file.Name);
                file.CopyTo (filePath);
                if (buildChecksums)
                {
                    checksums.AddFile (file);
                }
            }
            yield return null;
        }

        static IEnumerator ChecksumSubdirectoryIE (DirectoryInfo d, int trim)
        {
            var relpath = d.FullName.Substring (trim);
            ReportChecksumProgress (relpath);
            copyItem += 1;
            checksums.PushBranch (d.FullName);
            foreach (var file in d.EnumerateFiles ().OrderBy(fi => fi.FullName, StringComparer.InvariantCultureIgnoreCase))
            {
                checksums.AddFile (file);
            }
            yield return null;
        }

        static IEnumerator ChecksumSubdirectoryQuietIE (DirectoryInfo d, int trim, int progressID)
        {
            var relpath = d.FullName.Substring (trim);
            Progress.Report (progressID, copyItem, copyCount, relpath);
            copyItem += 1;
            checksums.PushBranch (d.FullName);
            foreach (var file in d.EnumerateFiles ().OrderBy(fi => fi.FullName, StringComparer.InvariantCultureIgnoreCase))
            {
                checksums.AddFile (file);
            }
            yield return null;
        }

        public static void GenerateModFiles (DataContainerModData modData, Action onCompletion)
        {
            if (modData == null)
            {
                Debug.Log ($"Can't generate mod files: mod data is null");
                return;
            }

            modData.DeleteOutputDirectories ();

            GenerateAssetBundles (modData);
            #if UNITY_EDITOR
            GenerateLibraries (modData);
            #endif
            var (ok, uds) = CanGenerateConfigOverrides (modData);
            if (!ok)
            {
                GenerateConfigEdits (modData);
                onCompletion?.Invoke ();
                return;
            }

            GenerateConfigOverrides (modData, uds, () =>
            {
                GenerateConfigEdits (modData);
                onCompletion?.Invoke();
            });
        }

        static void GenerateConfigEdits (DataContainerModData modData)
        {
            if (modData?.configEdits == null)
                return;

            modData.configEdits.SaveToMod (modData);
        }

        static void GenerateAssetBundles (DataContainerModData modData)
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

            if (IsUnityVersionSupported (true))
            {
                Debug.LogError ($"Warning! Exported assets will only be loaded by the game if your Editor version exactly matches the engine version of the game. Game engine version: {unityVersionExpected}. Editor engine version: {Application.unityVersion}");
                // return; // Probably best not to return on this so that folks can catch additional errors and learn how the whole process executes
            }

            Debug.Log ($"Building asset bundles:\n- Temp folder:{buildPathTemp}\n- Final folder: {buildPathFinal}");
            ModToolsAssetBundles.BuildAllAssetBundlesFromList (buildPathTemp, modData.assetBundles.bundleDefinitions, buildPathFinal);
        }

        #if UNITY_EDITOR
        static void GenerateLibraries (DataContainerModData modData)
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
                if (!dll.enabled)
                {
                    continue;
                }
                if (!File.Exists (dll.path))
                {
                    continue;
                }

                var pathDest = DataPathHelper.GetCombinedCleanPath (pathLibraries, Path.GetFileName (dll.path));
                File.Copy (dll.path, pathDest, true);
            }
        }
        #endif

        static (bool, UtilityDatabaseSerialization) CanGenerateConfigOverrides (DataContainerModData modData)
        {
            if (!modData.metadata.includesConfigOverrides)
            {
                return (false, default);
            }

            var uds = UnityEngine.Object.FindObjectOfType<UtilityDatabaseSerialization> ();
            if (uds == null)
            {
                Debug.LogWarning ("Failed to get gameobject for type " + nameof(UtilityDatabaseSerialization));
                return (false, default);
            }

            if (!modData.LoadChecksums (DataManagerMod.sdkChecksumData))
            {
                Debug.LogWarning ("Failed to load checksums for mod " + modData.id);
                return (false, default);
            }

            return (true, uds);
        }

        static void GenerateConfigOverrides (DataContainerModData modData, UtilityDatabaseSerialization uds, Action onCompletion)
        {
            var multiLinkerTypeMap = uds.FindAllMultiLinkers ();
            var multiLinkerRemoves = multiLinkerTypeMap
                .Select (kvp => FindRemoved (modData, modData.multiLinkerChecksumMap, kvp.Key, kvp.Value))
                .Where (pr => pr.Removed.Count != 0)
                .ToList ();

            if (multiLinkerRemoves.Count != 0)
            {
                modData.configEdits ??= new ModConfigEditSource ();
                var configEdits = modData.configEdits;
                foreach (var removed in multiLinkerRemoves)
                {
                    var mce = configEdits.AddMultiLinkerEdit (removed.MultiLinkerTypeName);
                    if (mce == null)
                    {
                        continue;
                    }
                    foreach (var key in removed.Removed)
                    {
                        mce.AddFileEdit (key, removed: true);
                    }
                }
                DataManagerMod.ModOptions.SaveProject (modData);
            }

            if (DataContainerModData.selectedMod == modData)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless (SaveAndGenerateModFilesIE (modData, uds, multiLinkerTypeMap, onCompletion));
                return;
            }
            EditorCoroutineUtility.StartCoroutineOwnerless (GenerateModFilesIE (modData, multiLinkerTypeMap, onCompletion));
        }

        static IEnumerator SaveAndGenerateModFilesIE (DataContainerModData modData, UtilityDatabaseSerialization uds, Dictionary<Type, IDataMultiLinker> multiLinkerTypeMap, Action onCompletion)
        {
            yield return uds.SaveAllChildrenInheritingOpenGenericTypeIE (typeof(DataLinker<>), forceLoad: false);
            yield return uds.SaveAllChildrenInheritingOpenGenericTypeIE (typeof(DataMultiLinker<>), forceLoad: false);
            yield return GenerateModFilesIE (modData, multiLinkerTypeMap, onCompletion);
        }

        static IEnumerator GenerateModFilesIE (DataContainerModData modData, Dictionary<Type, IDataMultiLinker> multiLinkerTypeMap, Action onCompletion)
        {
            yield return null;

            var linkerPairs = modData.linkerChecksumMap.Values.Where (e => e.SDK == null || !ConfigChecksums.ChecksumEqual (e.SDK, e.Mod)).ToList ();
            var multiLinkerPairs = modData.multiLinkerChecksumMap
                .Where (kvp => kvp.Value.SDK == null || !ConfigChecksums.ChecksumEqual (kvp.Value.SDK, kvp.Value.Mod))
                .ToDictionary (kvp => kvp.Key, kvp => kvp.Value);
            var total = linkerPairs.Count + multiLinkerPairs.Count;
            var item = 0;

            var sourcePathPrefix = modData.GetModPathConfigs ();
            var destPathPrefix = Path.Combine (modData.GetModPathProject (), DataContainerModData.overridesFolderName);
            foreach (var pair in linkerPairs)
            {
                var pct = 1f * item / total;
                var textHeader = string.Format ("Generate configs {0}/{1} - {2:P0}", item, total, pct);
                EditorUtility.DisplayProgressBar (textHeader, pair.Mod.RelativePath, pct);
                item += 1;
                var sourcePath = Path.Combine (sourcePathPrefix, pair.Mod.RelativePath);
                var destPath = Path.Combine (destPathPrefix, pair.Mod.RelativePath);
                Directory.CreateDirectory (Path.GetDirectoryName (destPath));
                if (pair.SDK == null)
                {
                    // XXX remove debug lines
                    Debug.Log ("New: " + pair.Mod.RelativePath);
                    File.Copy (sourcePath, destPath, true);
                }
                else if (!ConfigChecksums.ChecksumEqual(pair.SDK, pair.Mod))
                {
                    // XXX generate ConfigEdit when that's implemented
                    // XXX remove debug lines
                    Debug.LogFormat ("SDK {0} | {1:x16}{2:x16}", pair.SDK.RelativePath, pair.SDK.Checksum.HalfSum2, pair.SDK.Checksum.HalfSum1);
                    Debug.LogFormat ("Mod {0} | {1:x16}{2:x16} | {3}", pair.Mod.RelativePath, pair.Mod.Checksum.HalfSum2, pair.Mod.Checksum.HalfSum1, pair.Mod.Source);
                    File.Copy (sourcePath, destPath, true);
                }
                yield return null;
            }
            foreach (var kvp in multiLinkerPairs)
            {
                var pair = kvp.Value;
                var pct = 1f * item / total;
                var textHeader = string.Format ("Generate configs {0}/{1} - {2:P0}", item, total, pct);
                EditorUtility.DisplayProgressBar (textHeader, pair.Mod.RelativePath, pct);
                item += 1;
                if (!multiLinkerTypeMap.TryGetValue (kvp.Key, out var dml) || !dml.IsUsingDirectories ())
                {
                    yield return CopyChangedFilesIE (sourcePathPrefix, destPathPrefix, pair);
                    continue;
                }
                yield return GenerateModFilesIE (sourcePathPrefix, destPathPrefix, pair);
            }

            if (onCompletion != null)
            {
                yield return new EditorWaitForSeconds (0.1f);
                onCompletion.Invoke ();
                EditorUtility.DisplayProgressBar ("Saving files - 99%", "Creating exported files...", 1f);
                yield return new EditorWaitForSeconds (0.35f);
            }

            EditorUtility.ClearProgressBar ();
        }

        static IEnumerator CopyChangedFilesIE (string sourcePathPrefix, string destPathPrefix, (ConfigChecksums.ConfigDirectory SDK, ConfigChecksums.ConfigDirectory Mod) pair)
        {
            var dirPath = Path.Combine (destPathPrefix, pair.Mod.RelativePath);
            Directory.CreateDirectory (dirPath);
            var sourceFiles = pair.SDK != null
                ? pair.SDK.Entries.OfType<ConfigChecksums.ConfigFile> ().ToDictionary (e => Path.GetFileName (e.RelativePath), e => e)
                : new Dictionary<string, ConfigChecksums.ConfigFile> ();
            foreach (var entry in pair.Mod.Entries.OfType<ConfigChecksums.ConfigFile> ())
            {
                var sourcePath = Path.Combine (sourcePathPrefix, entry.RelativePath);
                var destPath = Path.Combine (destPathPrefix, entry.RelativePath);
                if (sourceFiles.TryGetValue (Path.GetFileName (entry.RelativePath), out var sdk) && !ConfigChecksums.ChecksumEqual (sdk, entry))
                {
                    // XXX generate ConfigEdit when that's implemented
                    // XXX remove debug lines
                    Debug.LogFormat ("SDK {0} | {1:x16}{2:x16}", sdk.RelativePath, sdk.Checksum.HalfSum2, sdk.Checksum.HalfSum1);
                    Debug.LogFormat ("Mod {0} | {1:x16}{2:x16} | {3}", entry.RelativePath, entry.Checksum.HalfSum2, entry.Checksum.HalfSum1, entry.Source);
                    File.Copy (sourcePath, destPath, true);
                    yield return null;
                }
                else if (sdk == null)
                {
                    // XXX remove debug lines
                    Debug.Log ("New: " + pair.Mod.RelativePath);
                    File.Copy (sourcePath, destPath, true);
                    yield return null;
                }
            }
        }

        static IEnumerator GenerateModFilesIE (string sourcePathPrefix, string destPathPrefix, (ConfigChecksums.ConfigDirectory SDK, ConfigChecksums.ConfigDirectory Mod) pair)
        {
            var pairs = ChangedDirectoryEntries (pair.SDK, pair.Mod);
            foreach (var pr in pairs)
            {
                yield return WalkModDirectoryIE (sourcePathPrefix, destPathPrefix, pr.Mod);
            }
        }

        static IEnumerator WalkModDirectoryIE (string sourcePathPrefix, string destPathPrefix, ConfigChecksums.ConfigDirectory mod)
        {
            var dirPath = Path.Combine (destPathPrefix, mod.RelativePath);
            Directory.CreateDirectory (dirPath);
            foreach (var entry in mod.Entries.OfType<ConfigChecksums.ConfigFile> ())
            {
                var sourcePath = Path.Combine (sourcePathPrefix, entry.RelativePath);
                var destPath = Path.Combine (destPathPrefix, entry.RelativePath);
                File.Copy (sourcePath, destPath, true);
                yield return null;
            }
            foreach (var entry in mod.Entries.OfType<ConfigChecksums.ConfigDirectory> ())
            {
                yield return WalkModDirectoryIE (sourcePathPrefix, destPathPrefix, entry);
            }
        }

        static IEnumerator GenerateModRemovedIE (string path, HashSet<string> removed)
        {
            foreach (var key in removed)
            {
                UtilitiesYAML.SaveToFile (path, key, removedEntry);
                yield return null;
            }
        }

        static List<(ConfigChecksums.ConfigDirectory SDK, ConfigChecksums.ConfigDirectory Mod)> ChangedDirectoryEntries (ConfigChecksums.ConfigDirectory sdk, ConfigChecksums.ConfigDirectory mod)
        {
            var sdkMap = GetEntriesByFileName (sdk, true);
            var modMap = GetEntriesByFileName (mod, true);
            return modMap
                .Where(kvp => kvp.Value is ConfigChecksums.ConfigDirectory)
                .Select(kvp =>
                {
                    sdkMap.TryGetValue (kvp.Key, out var s);
                    return ((ConfigChecksums.ConfigDirectory)s, (ConfigChecksums.ConfigDirectory)kvp.Value);
                })
                .Where (x => x.Item1 == null || !ConfigChecksums.ChecksumEqual (x.Item1, x.Item2))
                .ToList ();
        }

        static (string MultiLinkerTypeName, HashSet<string> Removed) FindRemoved (DataContainerModData modData, DataMultiLinkerPairMap multiLinkerPairs, Type t, IDataMultiLinker dml)
        {
            var sdkSet = new HashSet<string> (dml.SDKKeys);
            if (multiLinkerPairs.TryGetValue (t, out var pair))
            {
                var modEntryMap = GetEntriesByKey (pair.Mod, dml.IsUsingDirectories ());
                var modSet = new HashSet<string> (modEntryMap.Keys);
                sdkSet.ExceptWith (modSet);
            }
            return (dml.GetType().Name, sdkSet);
        }

        static IEnumerator UpdateModConfigsLinkerIE (DirectoryInfo dirSDKConfigs, DirectoryInfo dirModConfigs, DataContainerModData modData)
        {
            var total = DataManagerMod.sdkChecksumData.LinkerChecksumMap.Count;
            var item = 0;
            foreach (var kvp in DataManagerMod.sdkChecksumData.LinkerChecksumMap)
            {
                item = ReportUpdateProgress (total, item, kvp.Value.RelativePath);
                if (!modData.linkerChecksumMap.TryGetValue (kvp.Key, out var pair))
                {
                    item = ReportUpdateProgress (total, item, kvp.Value.RelativePath);
                    var filePathSDKNew = Path.Combine (dirSDKConfigs.FullName, kvp.Value.RelativePath);
                    var filePathModNew = Path.Combine (dirModConfigs.FullName, kvp.Value.RelativePath);
                    File.Copy (filePathSDKNew, filePathModNew, true);
                    yield return null;
                    continue;
                }
                if (pair.Mod.Source == ConfigChecksums.EntrySource.Mod)
                {
                    continue;
                }

                var filePathMod = Path.Combine (dirModConfigs.FullName, pair.Mod.RelativePath);
                pair.Mod.Update (ConfigChecksums.EntrySource.SDK, File.ReadAllBytes(filePathMod));
                if (ConfigChecksums.ChecksumEqual (kvp.Value.Checksum, pair.Mod.Checksum))
                {
                    pair.Mod.Source = ConfigChecksums.EntrySource.SDK;
                    continue;
                }
                var filePathSDK = Path.Combine (dirSDKConfigs.FullName, kvp.Value.RelativePath);
                File.Copy (filePathSDK, filePathMod, true);
                yield return null;
            }
            foreach (var kvp in modData.linkerChecksumMap)
            {
                if (DataManagerMod.sdkChecksumData.LinkerChecksumMap.ContainsKey (kvp.Key))
                {
                    continue;
                }
                if (kvp.Value.Mod.Source == ConfigChecksums.EntrySource.Mod)
                {
                    continue;
                }
                var fileMod = new FileInfo (Path.Combine (dirModConfigs.FullName, kvp.Value.Mod.RelativePath));
                fileMod.Delete ();
            }

            EditorUtility.ClearProgressBar ();
        }

        static int ReportUpdateProgress (int total, int item, string relpath)
        {
            var pct = 1f * item / total;
            var textHeader = string.Format ("Update configs {0}/{1} - {2:P0}", item, total, pct);
            EditorUtility.DisplayProgressBar (textHeader, relpath, pct);
            return item + 1;
        }

        static IEnumerator UpdateModConfigsMultilinkerIE (DirectoryInfo dirSDKConfigs, DirectoryInfo dirModConfigs, DataContainerModData modData)
        {
            // XXX How to handle when an entire DataMultiLinker<T> type is removed from the game? Delete the directory from the mod configs?
            // XXX What if the mod has changes in that DB?

            var total = DataManagerMod.sdkChecksumData.MultiLinkerChecksumMap.Count;
            var item = 0;
            foreach (var kvp in DataManagerMod.sdkChecksumData.MultiLinkerChecksumMap)
            {
                var dml = UtilityDatabaseSerialization.GetMultiLinkerForContainer (kvp.Key);
                if (dml == null)
                {
                    Debug.LogWarning ("No multilinker for type: " + kvp.Key.Name);
                    item += 1;
                    continue;
                }

                item = ReportUpdateProgress (total, item, kvp.Value.RelativePath);
                if (!modData.multiLinkerChecksumMap.TryGetValue (kvp.Key, out var pair))
                {
                    var source = new DirectoryInfo (Path.Combine (dirSDKConfigs.FullName, kvp.Value.RelativePath));
                    var pathDest = Path.Combine (dirModConfigs.FullName, kvp.Value.RelativePath);
                    yield return CopyConfigIE (source, pathDest);
                    continue;
                }

                // Make sure we're working with current checksums.
                dml.LoadDataLocal ();

                var directoryMode = dml.IsUsingDirectories ();
                var entriesSDK = GetEntriesByKey (kvp.Value, directoryMode);
                var entriesMod = GetEntriesByKey (pair.Mod, directoryMode);
                foreach (var entrySDK in entriesSDK)
                {
                    if (!entriesMod.TryGetValue (entrySDK.Key, out var entryMod))
                    {
                        if (directoryMode)
                        {
                            var source = new DirectoryInfo (Path.Combine (dirSDKConfigs.FullName, entrySDK.Value.RelativePath));
                            var pathDest = Path.Combine (dirModConfigs.FullName, entrySDK.Value.RelativePath);
                            yield return CopyConfigIE (source, pathDest);
                        }
                        else
                        {
                            var filePathSDKNew = Path.Combine (dirSDKConfigs.FullName, entrySDK.Value.RelativePath);
                            var filePathModNew = Path.Combine (dirModConfigs.FullName, entrySDK.Value.RelativePath);
                            File.Copy (filePathSDKNew, filePathModNew, true);
                        }
                        continue;
                    }
                    if (entryMod.Source == ConfigChecksums.EntrySource.Mod)
                    {
                        continue;
                    }
                    if (ConfigChecksums.ChecksumEqual (entrySDK.Value.Checksum, entryMod.Checksum))
                    {
                        entryMod.Source = ConfigChecksums.EntrySource.SDK;
                        continue;
                    }
                    if (directoryMode)
                    {
                        var dirEntryMod = new DirectoryInfo (Path.Combine (dirModConfigs.FullName, entryMod.RelativePath));
                        dirEntryMod.Delete (true);
                        dirEntryMod.Create ();
                        var source = new DirectoryInfo (Path.Combine (dirSDKConfigs.FullName, entrySDK.Value.RelativePath));
                        yield return CopyConfigIE (source, dirEntryMod.FullName);
                        continue;
                    }
                    var filePathSDK = Path.Combine (dirSDKConfigs.FullName, entrySDK.Value.RelativePath);
                    var filePathMod = Path.Combine (dirModConfigs.FullName, entryMod.RelativePath);
                    File.Copy (filePathSDK, filePathMod, true);
                }
                foreach (var entryMod in entriesMod)
                {
                    if (entryMod.Value.Source == ConfigChecksums.EntrySource.Mod)
                    {
                        continue;
                    }
                    if (entriesSDK.ContainsKey (entryMod.Key))
                    {
                        continue;
                    }
                    if (directoryMode)
                    {
                        var dirEntry = new DirectoryInfo (Path.Combine (dirModConfigs.FullName, entryMod.Value.RelativePath));
                        dirEntry.Delete (true);
                        yield return null;
                        continue;
                    }
                    var fileEntry = new FileInfo (Path.Combine (dirModConfigs.FullName, entryMod.Value.RelativePath));
                    fileEntry.Delete ();
                }
            }

            EditorUtility.ClearProgressBar ();
        }

        static IEnumerator SyncModChecksums (DataContainerModData modData, ConfigChecksums.Checksum originChecksum)
        {
            var progress = 0f;
            EditorUtility.DisplayProgressBar ("Updating mod checksums", "Working...", progress);
            yield return null;
            var source = new DirectoryInfo (modData.GetModPathConfigs ());
            yield return SyncConfigDirectory (source, modData.checksumsRoot, progress);
            modData.checksumsRoot.FixLocators ();
            var serializer = new ConfigChecksums.Serializer (source, modData.checksumsRoot, originChecksum);
            serializer.Save ();
            modData.LoadChecksums (DataManagerMod.sdkChecksumData);
            EditorUtility.ClearProgressBar ();
        }

        static IEnumerator SyncConfigDirectory (DirectoryInfo source, ConfigChecksums.ConfigDirectory configDirectory, float progress)
        {
            progress += 0.1f;
            if (progress > 1f)
            {
                progress = 0f;
            }
            EditorUtility.DisplayProgressBar ("Updating mod checksums", "Working...", progress);
            yield return null;

            var dir = new DirectoryInfo (Path.Combine (source.FullName, configDirectory.RelativePath));
            var files = new HashSet<string> (dir.EnumerateFiles ().Select (fi => fi.Name));
            var fileEntries = configDirectory.Entries.OfType<ConfigChecksums.ConfigFile> ().ToDictionary (entry => Path.GetFileName (entry.RelativePath), entry => entry.Locator.Last ());
            var removed = new List<int> ();
            foreach (var kvp in fileEntries)
            {
                if (!files.Contains (kvp.Key))
                {
                    removed.Add (-kvp.Value);
                    continue;
                }
                var entry = (ConfigChecksums.ConfigFile)configDirectory.Entries[kvp.Value];
                var filePath = Path.Combine (source.FullName, entry.RelativePath);
                entry.Update (entry.Source, File.ReadAllBytes (filePath));
            }

            var subdirs = dir.EnumerateDirectories ().ToDictionary (di => di.Name, di => di);
            var dirEntries = configDirectory.Entries.OfType<ConfigChecksums.ConfigDirectory> ().ToDictionary (entry => Path.GetFileName (entry.RelativePath), entry => entry.Locator.Last ());
            foreach (var kvp in dirEntries)
            {
                if (!subdirs.ContainsKey (kvp.Key))
                {
                    removed.Add (-kvp.Value);
                    continue;
                }

                progress += 0.1f;
                if (progress > 1f)
                {
                    progress = 0f;
                }
                EditorUtility.DisplayProgressBar ("Updating mod checksums", "Working...", progress);
                yield return null;

                var entry = (ConfigChecksums.ConfigDirectory)configDirectory.Entries[kvp.Value];
                yield return SyncConfigDirectory (source, entry, progress);
            }

            foreach (var kvp in subdirs)
            {
                if (dirEntries.ContainsKey (kvp.Key))
                {
                    continue;
                }

                progress += 0.1f;
                if (progress > 1f)
                {
                    progress = 0f;
                }
                EditorUtility.DisplayProgressBar ("Updating mod checksums", "Working...", progress);
                yield return null;

                var entry = new ConfigChecksums.ConfigDirectory (ConfigChecksums.EntryType.Directory)
                {
                    Source = ConfigChecksums.EntrySource.SDK,
                    Locator = configDirectory.Locator.Append (0).ToArray (),
                    RelativePath = Path.Combine (configDirectory.RelativePath, kvp.Key),
                };
                yield return SyncConfigDirectory (source, entry, progress);
            }

            foreach (var n in files)
            {
                if (fileEntries.ContainsKey (n))
                {
                    continue;
                }
                var entry = new ConfigChecksums.ConfigFile ()
                {
                    Source = ConfigChecksums.EntrySource.SDK,
                    Locator = configDirectory.Locator.Append (0).ToArray (),
                    RelativePath = Path.Combine (configDirectory.RelativePath, n),
                };
                var filePath = Path.Combine (source.FullName, entry.RelativePath);
                entry.Update (ConfigChecksums.EntrySource.SDK, File.ReadAllBytes (filePath));
                configDirectory.Entries.Add (entry);
            }

            removed.Sort ();
            foreach (var i in removed)
            {
                configDirectory.Entries.RemoveAt (-i);
            }

            configDirectory.ComputeChecksum ();
        }

        static int copyItem;
        static int copyCount;

        static bool buildStarted;
        static bool buildChecksums;
        static ConfigChecksums.Serializer checksums;

        static readonly ReadOnlyDictionary<string, ConfigChecksums.ConfigEntry> emptyMap = new ReadOnlyDictionary<string, ConfigChecksums.ConfigEntry> (new Dictionary<string, ConfigChecksums.ConfigEntry> ());
        static readonly ModConfigEditSerialized removedEntry = new ModConfigEditSerialized () { removed = true, };

        const string dirNameConfigs = "Configs";
    }

    #endif
}
