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
        
        public static void GenerateModFiles (DataContainerModData modData, Action onCompletion)
        {
            if (modData == null)
                return;
    
            var dirProject = new DirectoryInfo (modData.GetModPathProject ());
            if (!dirProject.Exists)
                return;
            
            CancelExport ();
            coroutine = EditorCoroutineUtility.StartCoroutineOwnerless (GenerateModFilesIE (modData, onCompletion));
        }

        private static void CancelExport ()
        {
            EditorUtility.ClearProgressBar ();
            if (coroutine != null)
                EditorCoroutineUtility.StopCoroutine (coroutine);
        }

        private static IEnumerator GenerateModFilesIE (DataContainerModData modData, Action onCompletion)
        {
            EditorUtility.DisplayProgressBar ("Exporting mod", "Deleting export folders...", 0f);
            modData.DeleteOutputDirectories ();

            // Set up ConfigOverrides and ConfigEdits
            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", "Generating config files...", 0f);
            yield return GenerateConfigFolders (modData);

            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", "Checking asset bundles...", 0.95f);
            ModToolsHelper.GenerateAssetBundles (modData);
            
            yield return new EditorWaitForSeconds (0.1f);
            EditorUtility.DisplayProgressBar ("Exporting mod", "Checking text edits...", 0.95f);
            ModToolsHelper.GenerateTextEdits (modData);
            
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
            // TODO: For non-yaml content like area files: invalidate and copy entire root folder if any subfile is different

            if (modData == null)
            {
                Debug.Log ($"Can't generate mod config overrides: mod data is null");
                CancelExport ();
            }

            var pathSDK = DataPathHelper.GetApplicationFolder ();
            var pathSDKConfigs = Path.Combine (pathSDK, "Configs");
            DirectoryInfo dirSDKConfigs = new DirectoryInfo (pathSDKConfigs);
            FileInfo[] filesSDK = dirSDKConfigs.GetFiles ("*.yaml", SearchOption.AllDirectories);
            // string[] filesSDK = Directory.GetFiles (pathSDKConfigs, "*.yaml", SearchOption.AllDirectories);
            
            var pathMod = modData.GetModPathProject ();
            var pathModConfigs = Path.Combine (pathMod, "Configs");
            DirectoryInfo dirModConfigs = new DirectoryInfo (pathModConfigs);
            FileInfo[] filesMod = dirModConfigs.GetFiles ("*.yaml", SearchOption.AllDirectories);
            // string[] filesMod = Directory.GetFiles (pathModConfigs, "*.yaml", SearchOption.AllDirectories);
            
            Debug.LogWarning ($"Scanning files...\nSDK ({filesSDK.Length}): {pathSDKConfigs}\nMod ({filesMod.Length}): {pathModConfigs}");
            EditorUtility.DisplayProgressBar ("Exporting mod", $"Scanning mod files ({filesMod.Length})...", 0.25f);
            
            timer.Reset ();
            timer.Start ();
            
            fileRecords.Clear ();
            fileRecordsDeleted.Clear ();
            fileRecordsModified.Clear ();
            fileRecordsAdded.Clear ();
            
            // Check initial set using the mod folder
            for (int i = 0; i < filesMod.Length; i++)
            {
                var fileMod = filesMod[i];
                var pathFull = fileMod.FullName;
                var pathLocal = pathFull.Substring (pathModConfigs.Length + 1);

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
                var pathLocal = pathFull.Substring (pathSDKConfigs.Length + 1);

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
            var configFolderName = "Configs";
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
    }

    #endif
}
