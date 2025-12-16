using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using PhantomBrigade.Data;
using PhantomBrigade.ModTools;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    #if UNITY_EDITOR

    public static class ModToolsExperimental
    {
        public enum FileStatus
        {
            Unknown = 0,
            Unmodified = 1,
            Modified = 2,
            Deleted = 3,
            Added = 4,
        }
        
        public class FileRecord
        {
            public string pathLocal;
            public FileStatus status;
            
            public FileInfo fileSDK;
            public FileInfo fileMod;

            public override string ToString ()
            {
                return pathLocal;
            }
        }

        private static Dictionary<string, FileRecord> fileRecords = new Dictionary<string, FileRecord> ();
        private static List<FileRecord> fileRecordsDeleted = new List<FileRecord> ();
        private static List<FileRecord> fileRecordsModified = new List<FileRecord> ();
        private static List<FileRecord> fileRecordsAdded = new List<FileRecord> ();
        private static ModConfigEditSource configEditsTemp = null;
        
        private static System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch ();
        private static readonly byte[] bufferA = new byte[64 * 1024];
        private static readonly byte[] bufferB = new byte[64 * 1024];
        private static EditorCoroutine coroutine = null;
        
        private static string pathPrefixDataUnique = "Data";
        private static string pathPrefixDataDecomposed = "DataDecomposed";

        private static List<string> pathExtensionBlocklist = new List<string>
        {
            ".blend",
            ".png"
        };

        private static List<string> pathPrefixesDirBased = new List<string>
        {
            "DataDecomposed/Combat/Areas"
        };

        private static HashSet<string> pathsToDirsInvalidatedFull = new HashSet<string> ();
        private static List<string> pathsToDirsInvalidatedLocal = new List<string> ();
        
        public static void GenerateModFiles (DataContainerModData modData, Action onCompletion)
        {
            if (modData == null || !modData.hasProjectFolder)
                return;
            
            CancelExport ();
            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless (GenerateModFilesIE (modData, onCompletion));
        }

        private static void CancelExport ()
        {
            DataContainerModData.selectedMod = null;
            EditorUtility.ClearProgressBar ();
            
            if (coroutine != null)
                EditorCoroutineUtility.StopCoroutine (coroutine);
        }

        private static IEnumerator GenerateModFilesIE (DataContainerModData modData, Action onCompletion)
        {
            DataContainerModData.selectedMod = modData;
            Debug.Log ($"Exporting mod {modData.id} | Has project folder: {modData.hasProjectFolder} | Path configs: {modData.GetModPathConfigs ()}");
            
            EditorUtility.DisplayProgressBar ("Exporting mod", "Deleting export folders...", 0f);
            modData.DeleteOutputDirectories ();

            bool modConfigsUsed = Directory.Exists (modData.GetModPathConfigs ());
            if (!modConfigsUsed)
                Debug.LogWarning ("Skipping config processing, the mod is not using configs...");
            else
            {
                // Set up ConfigOverrides and ConfigEdits
                yield return new EditorWaitForSeconds (0.1f);
                EditorUtility.DisplayProgressBar ("Exporting mod", "Generating config files...", 0f);
                yield return GenerateConfigFolders (modData);
                
                yield return new EditorWaitForSeconds (0.1f);
                EditorUtility.DisplayProgressBar ("Exporting mod", "Checking text edits...", 0.95f);
                DataManagerText.LoadLibrary ();
                ModTextHelper.GenerateTextChanges (modData);
                if (modData.textEdits != null)
                    modData.textEdits.SaveToMod (modData);
            }
            
            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", "Checking asset bundles...", 0.95f);
            ModToolsHelper.GenerateAssetBundles (modData);
            
            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", "Checking libraries...", 0.95f);
            ModToolsHelper.GenerateLibraries (modData);

            if (onCompletion != null)
            {
                yield return new EditorWaitForSeconds (0.1f);
                onCompletion.Invoke ();
                EditorUtility.DisplayProgressBar ("Exporting mod", "Executing final steps...", 1f);
                yield return new EditorWaitForSeconds (0.35f);
            }

            CancelExport ();
        }
        
        static bool IsFileEqual (FileInfo a, FileInfo b)
        {
            if (a == null || b == null)
                return false;
            
            if (a.Length != b.Length) 
                return false;
            
            if (a.FullName == b.FullName) 
                return true;

            using (var fa = a.OpenRead())
            using (var fb = b.OpenRead())
            {
                int ra, rb;
                do
                {
                    ra = fa.Read (bufferA, 0, bufferA.Length);
                    rb = fb.Read (bufferB, 0, bufferB.Length);
                    if (ra != rb) 
                        return false;

                    for (int i = 0; i < ra; i++)
                    {
                        if (bufferA[i] != bufferB[i]) 
                            return false;
                    }
                }
                while (ra > 0);
            }

            return true;
        }

        private static IEnumerator GenerateConfigFolders (DataContainerModData modData)
        {
            if (modData == null)
            {
                Debug.Log ($"Can't generate mod config files: mod data is null");
                CancelExport ();
                yield break;
            }
            
            var pathSDK = DataPathHelper.GetApplicationFolder ();
            var pathSDKConfigs = Path.Combine (pathSDK, "Configs");
            
            bool modConfigsVersioned = !string.IsNullOrEmpty (modData.configsVersion?.version);
            if (modConfigsVersioned && !string.Equals (modData.configsVersion.version, ConfigsVersion.versionExpected))
            {
                var ver = modData.configsVersion.version;
                var pathSDKConfigsFolderOld = $"ConfigsOld/{ver}";
                var pathSDKConfigsOld = Path.Combine (pathSDK, pathSDKConfigsFolderOld);
                DirectoryInfo dirSDKConfigsOld = new DirectoryInfo (pathSDKConfigsOld);
                if (dirSDKConfigsOld.Exists)
                {
                    Debug.LogWarning ($"Mod Configs folder version is {ver} instead of {ConfigsVersion.versionExpected}, falling back to reading the old configs folder {pathSDKConfigsOld}");
                    pathSDKConfigs = pathSDKConfigsOld;
                }
            }
            
            DirectoryInfo dirSDKConfigs = new DirectoryInfo (pathSDKConfigs);
            if (!dirSDKConfigs.Exists)
            {
                Debug.Log ($"Can't generate mod config files: SDK configs folder at path {pathSDKConfigs} is not found");
                CancelExport ();
                yield break;
            }

            var pathMod = modData.GetModPathProject ();
            var pathModConfigs = Path.Combine (pathMod, "Configs");

            DirectoryInfo dirModConfigs = new DirectoryInfo (pathModConfigs);
            if (!dirModConfigs.Exists)
            {
                Debug.Log ($"Can't generate mod config files: mod configs folder at path {pathModConfigs} is not found");
                CancelExport ();
                yield break;
            }
            
            // For resolving local paths
            int pathSDKConfigsLength = pathSDKConfigs.Length;
            int pathModConfigsLength = pathModConfigs.Length;
            
            // Files we'll iterate on
            FileInfo[] filesSDK = dirSDKConfigs.GetFiles ("*", SearchOption.AllDirectories);
            FileInfo[] filesMod = dirModConfigs.GetFiles ("*", SearchOption.AllDirectories);
            
            Debug.LogWarning ($"Scanning files...\nSDK ({filesSDK.Length}): {pathSDKConfigs}\nMod ({filesMod.Length}): {pathModConfigs}");
            EditorUtility.DisplayProgressBar ("Exporting mod", $"Scanning mod files ({filesMod.Length})...", 0.25f);
            
            timer.Reset ();
            timer.Start ();
            
            fileRecords.Clear ();
            fileRecordsDeleted.Clear ();
            fileRecordsModified.Clear ();
            fileRecordsAdded.Clear ();
            
            pathsToDirsInvalidatedFull.Clear ();
            pathsToDirsInvalidatedLocal.Clear ();

            int pathExtensionBlocklistSize = pathExtensionBlocklist.Count;
            
            // Check initial set using the mod folder
            for (int i = 0; i < filesMod.Length; i++)
            {
                var fileMod = filesMod[i];
                var pathFull = fileMod.FullName;
                var pathLocal = DataPathHelper.GetCleanPath (pathFull.Substring (pathModConfigsLength + 1));
                if (!pathLocal.StartsWith (pathPrefixDataDecomposed) || !pathLocal.StartsWith (pathPrefixDataUnique))
                    continue;

                var extension = Path.GetExtension (pathLocal);
                bool extensionBlocked = false;
                for (int e = 0; e < pathExtensionBlocklistSize; ++e)
                {
                    if (string.Equals (extension, pathExtensionBlocklist[e]))
                    {
                        extensionBlocked = true;
                        break;
                    }
                }
                
                if (extensionBlocked)
                    continue;
                
                var fc = new FileRecord ();
                fc.pathLocal = pathLocal;
                fc.status = FileStatus.Unknown;
                fc.fileMod = fileMod;
                fileRecords.Add (pathLocal, fc);
            }
            
            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", $"Scanning SDK files ({filesMod.Length})...", 0.35f);
            
            // Check for deletions or modifications by iterating over the SDK folder
            for (int i = 0; i < filesSDK.Length; i++)
            {
                var fileSDK = filesSDK[i];
                var pathFull = fileSDK.FullName;
                var pathLocal = DataPathHelper.GetCleanPath (pathFull.Substring (pathSDKConfigsLength + 1));
                if (!pathLocal.StartsWith (pathPrefixDataDecomposed) || !pathLocal.StartsWith (pathPrefixDataUnique))
                    continue;
                
                var extension = Path.GetExtension (pathLocal);
                bool extensionBlocked = false;
                for (int e = 0; e < pathExtensionBlocklistSize; ++e)
                {
                    if (string.Equals (extension, pathExtensionBlocklist[e]))
                    {
                        extensionBlocked = true;
                        break;
                    }
                }
                
                if (extensionBlocked)
                    continue;
                
                FileRecord fc = null;
                if (!fileRecords.TryGetValue (pathLocal, out fc))
                {
                    // File is present in SDK but not present in mods
                    fc = new FileRecord ();
                    fc.pathLocal = pathLocal;
                    fc.status = FileStatus.Deleted;
                    fc.fileSDK = fileSDK;
                    fileRecords.Add (pathLocal, fc);
                    fileRecordsDeleted.Add (fc);
                    continue;
                }
                
                // File is present in mod and in SDK
                fc.fileSDK = fileSDK;
                
                // Next, compare
                bool fileEqual = IsFileEqual (fc.fileSDK, fc.fileMod);
                fc.status = fileEqual ? FileStatus.Unmodified : FileStatus.Modified;
                
                if (!fileEqual)
                    fileRecordsModified.Add (fc);
            }
            
            // Check for additions by iterating over records and looking for missing mod SDK file
            foreach (var kvp in fileRecords)
            {
                var fc = kvp.Value;
                if (fc.fileSDK == null)
                {
                    fc.status = FileStatus.Added;
                    fileRecordsAdded.Add (fc); // Just for nice logging, not used for copying
                }
            }
            
            // Iterate over the directory based DBs to identify if any have modified files
            // Any modified file should lead to entire directory for a given data key being exported
            int pathPrefixesCount = pathPrefixesDirBased.Count;
            foreach (var kvp in fileRecords)
            {
                var fc = kvp.Value;
                if (fc.status == FileStatus.Added || fc.status == FileStatus.Modified)
                {
                    for (int i = 0; i < pathPrefixesCount; i++)
                    {
                        var prefixDatabase = pathPrefixesDirBased[i];
                        if (!fc.pathLocal.StartsWith (prefixDatabase))
                            continue;

                        var pathDirFull = fc.fileMod.DirectoryName;
                        if (pathDirFull == null || pathsToDirsInvalidatedFull.Contains (pathDirFull))
                            continue;
                        
                        var pathDirLocal = DataPathHelper.GetCleanPath (pathDirFull.Substring (pathModConfigsLength + 1));
                        pathsToDirsInvalidatedFull.Add (pathDirFull); // Makes rejections above easy without prettifying each candidate path
                        pathsToDirsInvalidatedLocal.Add (pathDirLocal); // Makes invalidations in next loop easier (same format as fc.pathLocal)
                        Debug.Log ($"File {fc.fileMod.Name} invalidates directory under dir-based DB: {pathDirLocal}");
                        break;
                    }
                }
            }
            
            // Apply identified invalidations
            int pathsToDirsInvalidationCount = pathsToDirsInvalidatedLocal.Count;
            foreach (var kvp in fileRecords)
            {
                var fc = kvp.Value;
                if (fc.status != FileStatus.Unmodified)
                    continue;
                
                for (int i = 0; i < pathsToDirsInvalidationCount; i++)
                {
                    var prefixDataKey = pathsToDirsInvalidatedLocal[i];
                    if (!fc.pathLocal.StartsWith (prefixDataKey))
                        continue;
                    
                    Debug.Log ($"File {fc.fileMod.Name} invalidated based on path prefix: {prefixDataKey}");
                    fc.status = FileStatus.Modified;
                    break;
                }
            }
            
            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", $"Processing detected added, modified and deleted files...", 0.45f);

            Debug.Log ($"Finished scanning files in {timer.Elapsed.Seconds}s");
            Debug.Log ($"Added files ({fileRecordsAdded.Count}):\n{fileRecordsAdded.ToStringMultilineDash ()}");
            Debug.Log ($"Modified files ({fileRecordsModified.Count}):\n{fileRecordsModified.ToStringMultilineDash ()}");
            Debug.Log ($"Deleted files ({fileRecordsDeleted.Count}):\n{fileRecordsDeleted.ToStringMultilineDash ()}");
            
            // Create a separate edits object to avoid stomping on user data
            if (modData.configEdits != null)
                configEditsTemp = UtilitiesYAML.CloneThroughYaml (modData.configEdits);
            else
                configEditsTemp = new ModConfigEditSource ();
            configEditsTemp.OnAfterDeserialization ();

            var dataTypeCollection = typeof (DataContainer);
            var configFolderName = DataContainerModData.configsFolderName;
            var overridesFolderName = DataContainerModData.overridesFolderName;

            // Final iteration (not strictly necessary, but it feels nicer to neatly log everything before the steps below
            foreach (var kvp in fileRecords)
            {
                var fc = kvp.Value;
                
                if (fc.status == FileStatus.Modified || fc.status == FileStatus.Added)
                {
                    try
                    {
                        var fileMod = fc.fileMod;
                        var pathDest = fileMod.FullName.Replace (configFolderName, overridesFolderName);
                        
                        string pathDirName = Path.GetDirectoryName (pathDest);
                        if (pathDirName != null)
                            Directory.CreateDirectory (pathDirName);
                        
                        fileMod.CopyTo (pathDest, true);
                        Debug.Log ($"Copied file ({fc.status}): {fc.pathLocal}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Debug.LogError ($"Error at {fc.pathLocal}: Access denied. Ensure you have permissions to access the files and directories. {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError ($"Error at {fc.pathLocal}: {ex.Message}");
                    }
                }

                if (fc.status == FileStatus.Deleted)
                {
                    var filePathTrimmed = DataPathHelper.GetCleanPath (fc.pathLocal);
                    
                    if (filePathTrimmed.StartsWith ("/"))
                        filePathTrimmed = filePathTrimmed.Substring (1, filePathTrimmed.Length - 1);

                    if (filePathTrimmed.EndsWith (".yaml"))
                        filePathTrimmed = filePathTrimmed.Replace (".yaml", string.Empty);

                    var fileName = Path.GetFileNameWithoutExtension (fc.pathLocal);
                    
                    var typeName = DataPathUtility.GetDataTypeFromPath (filePathTrimmed);
                    if (typeName == null)
                    {
                        filePathTrimmed = filePathTrimmed.Replace (fileName, string.Empty);
                        typeName = DataPathUtility.GetDataTypeFromPath (filePathTrimmed);
                    }

                    var dataType = FieldReflectionUtility.GetTypeByName (typeName);
                    if (dataType == null)
                    {
                        Debug.LogWarning ($"Deleted file {fc.pathLocal} | Unknown target type {typeName}, ignoring...");
                        continue;
                    }

                    bool isCollection = dataTypeCollection.IsAssignableFrom (dataType);
                    if (!isCollection)
                    {
                        Debug.LogWarning ($"Deleted file {fc.pathLocal} | Type {typeName} is not a collection config, removal record can't be created, ignoring");
                        continue;
                    }
                    
                    var component = UtilityDatabaseSerialization.GetComponentForDataType (dataType);
                    if (component == null)
                    {
                        Debug.LogWarning ($"Deleted file {fc.pathLocal} | Failed to find a data component for data type {dataType.Name}, ignoring...");
                        continue;
                    }
                    
                    var componentTypeName = component.GetType ().Name;
                    
                    ModConfigEditMultiLinker multiLinkerEdits = null;
                    if (configEditsTemp.dataMultiLinkers == null)
                        configEditsTemp.dataMultiLinkers = new List<ModConfigEditMultiLinker> ();
                    else
                    {
                        foreach (var candidate in configEditsTemp.dataMultiLinkers)
                        {
                            if (candidate != null && string.Equals (candidate.type, componentTypeName))
                                multiLinkerEdits = candidate;
                        }
                    }
                    
                    if (multiLinkerEdits == null)
                    {
                        multiLinkerEdits = new ModConfigEditMultiLinker { type = component.GetType ().Name };
                        configEditsTemp.dataMultiLinkers.Add (multiLinkerEdits);
                    }
                    
                    ModConfigEditSourceFileMultiLinker fileEdits = null;
                    if (multiLinkerEdits.edits == null)
                        multiLinkerEdits.edits = new List<ModConfigEditSourceFileMultiLinker> ();
                    else
                    {
                        foreach (var candidate in multiLinkerEdits.edits)
                        {
                            if (candidate != null && string.Equals (candidate.key, fileName))
                                fileEdits = candidate;
                        }
                    }
                
                    if (fileEdits == null)
                    {
                        fileEdits = new ModConfigEditSourceFileMultiLinker { key = fileName };
                        multiLinkerEdits.edits.Add (fileEdits);
                    }
                    
                    Debug.Log ($"Deleted file {fc.pathLocal} | File name: {fileName}\n- Data component: {componentTypeName}\n- Data type ({dataTypeCollection.Name}): {dataType.Name}");
                    fileEdits.removed = true;
                    fileEdits.edits = new List<ModConfigEditSourceLine> ();
                }
            }
            
            if (configEditsTemp.dataLinkers != null || configEditsTemp.dataMultiLinkers != null)
            {
                yield return new EditorWaitForSeconds (0.1f);
                EditorUtility.DisplayProgressBar ("Exporting mod", $"Saving config edits...", 0.55f);
                configEditsTemp.SaveToMod (modData);
            }
        }

        public static void CopyConfigsFromSDK (DataContainerModData modData)
        {
            if (modData == null)
            {
                Debug.Log ($"Can't generate mod config overrides: mod data is null");
                return;
            }
            
            var pathSDK = DataPathHelper.GetApplicationFolder ();
            var pathSDKConfigs = Path.Combine (pathSDK, "Configs");
            DirectoryInfo dirSDKConfigs = new DirectoryInfo (pathSDKConfigs);
            
            var pathMod = modData.GetModPathProject ();
            var pathModConfigs = Path.Combine (pathMod, "Configs");
            DirectoryInfo dirModConfigs = new DirectoryInfo (pathModConfigs);
            
            if (dirModConfigs.Exists)
            {
                Debug.Log ($"Deleting the Configs folder in the mod project: {pathModConfigs}");
                dirModConfigs.Delete (true);
            }
            
            try
            {
                UtilitiesYAML.CopyDirectory (pathSDKConfigs, pathModConfigs, true);
            }
            catch (Exception e)
            {
                Debug.LogException (e);
            }

            var configsVersion = new ConfigsVersion { version = ConfigsVersion.versionExpected };
            var configsVersionPath = DataPathHelper.GetCombinedCleanPath (pathModConfigs, "dataVersion.yaml");
            UtilitiesYAML.SaveToFile (configsVersionPath, configsVersion);
            modData.configsVersion = configsVersion;
        }
        
        public static void CopyConfigsFromExportedMod (DataContainerModData modData, string pathSelected)
        {
            if (modData == null)
            {
                Debug.Log ($"Can't import files: mod data is null");
                return;
            }
            
            if (string.IsNullOrEmpty (pathSelected))
            {
                Debug.Log ($"Can't import files: no import path provided");
                return;
            }
            
            var dirSelected = new DirectoryInfo (pathSelected);
            if (!dirSelected.Exists)
            {
                Debug.LogWarning ($"Can't import, no folder selected");
                return;
            }


            var pathMod = modData.GetModPathProject ();
            var pathModConfigs = Path.Combine (pathMod, DataContainerModData.configsFolderName);
            DirectoryInfo dirModConfigs = new DirectoryInfo (pathModConfigs);
            
            if (!dirModConfigs.Exists)
            {
                Debug.Log ($"Can't import files: mod doesn't contain a Configs folder.\n- {pathModConfigs}");
                return;
            }
            
            var pathImportConfigOverrides = Path.Combine (pathSelected, DataContainerModData.overridesFolderName);
            DirectoryInfo dirImportConfigOverrides = new DirectoryInfo (pathImportConfigOverrides);
            if (dirImportConfigOverrides.Exists && EditorUtility.DisplayDialog 
            (
                "Import ConfigOverrides?", 
                $"Discovered the ConfigOverrides folder in the selected import folder. Would you like to copy its contents over the Configs folder in the selected mod (ID {modData.id})? The imported files might overwrite existing files."+ 
                $"\n\nFrom folder: \n{pathSelected}/ConfigOverrides" + 
                $"\n\nTo project folder: \n{pathMod}/Configs",
                "Import ConfigOverrides", 
                "Skip")
            )
            {
                FileInfo[] filesConfigOverrides = dirImportConfigOverrides.GetFiles ("*", SearchOption.AllDirectories);
                Debug.Log ($"Copying configs from selected folder ConfigOverrides to mod folder Configs. Potential files: {filesConfigOverrides.Length}\nMod: {pathModConfigs}\nSource: {pathImportConfigOverrides}");
            
                try
                {
                    foreach (var file in filesConfigOverrides)
                    {
                        var pathFull = file.FullName;
                        var pathLocal = pathFull.Substring (pathImportConfigOverrides.Length + 1);
                        var pathDest = Path.Combine (pathModConfigs, pathLocal);
                    
                        string pathDirName = Path.GetDirectoryName (pathDest);
                        if (pathDirName != null)
                            Directory.CreateDirectory (pathDirName);
                        
                        file.CopyTo (pathDest, true);
                        Debug.Log ($"Copied to Configs: {file.Name}\nLocal path: {pathLocal}\nFrom: {pathFull}\nTo: {pathDest}"); 
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                }
            }
            
            var pathImportConfigEdits = Path.Combine (pathSelected, DataContainerModData.editsFolderName);
            DirectoryInfo dirImportConfigEdits = new DirectoryInfo (pathImportConfigEdits);
            if (dirImportConfigEdits.Exists && EditorUtility.DisplayDialog 
            (
                "Import ConfigEdits?", 
                $"Discovered the ConfigEdits folder in the selected import folder. Would you like to load its contents into the selected mod project (ID {modData.id})? The imported edits might overwrite existing edits."+ 
                $"\n\nFrom folder: \n{pathSelected}/ConfigEdits" + 
                $"\n\nTo project metadata: \n(Edits are stored in the project metadata, no per-edit files used until export)",
                "Import ConfigEdits", 
                "Skip")
            )
            {
                try
                {
                    ModConfigEditSource.LoadFromMod (modData, false, pathImportConfigEdits);
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                }
            }
            
            var pathImportTextEdits = Path.Combine (pathSelected, DataContainerModData.localizationEditsFolderName);
            DirectoryInfo dirImportTextEdits = new DirectoryInfo (pathImportTextEdits);
            if (dirImportTextEdits.Exists && EditorUtility.DisplayDialog 
            (
                "Import LocalizationEdits?", 
                $"Discovered the LocalizationEdits (text modifications) folder in the selected import folder. Would you like to load its contents into the selected mod project (ID {modData.id})? The imported text edits might overwrite existing text edits."+ 
                $"\n\nFrom folder: \n{pathSelected}/LocalizationEdits" + 
                $"\n\nTo project metadata: \n(Edits are stored in the project metadata, no per-edit files used until export)",
                "Import LocalizationEdits", 
                "Skip")
            )
            {
                try
                {
                    ModConfigLocEdit.LoadFromMod (modData, false, pathImportTextEdits);
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                }
            }

            bool textEditsPresent = dirImportTextEdits.Exists && modData.textEdits?.languages != null && modData.textEdits.languages.Count > 0;
            if (EditorUtility.DisplayDialog
            (
                "Apply text edits to Configs?",
                $"The mod has imported text edits. Would you like to apply them directly to Configs so that they show up in data editors?",
                "Apply to Configs",
                "Skip")
            )
            {
                ModTextHelper.ApplyTextChangesToConfigs (modData);
            }
        }
    }

    #endif
}
