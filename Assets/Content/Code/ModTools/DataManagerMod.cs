using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using PhantomBrigade.ModTools;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    using Area;
    using Data;
    using Mods;

    [ExecuteInEditMode]
    [LabelWidth (160f), HideMonoScript]
    public class DataManagerMod : MonoBehaviour
    {
        #if UNITY_EDITOR

        private static DataManagerMod ins;
        private static bool initialized = false;
        private static bool loadedOnce = false;

        private const string filenameMain = "project";
        private const string extensionYAML = ".yaml";

        private void Update ()
        {
            if (!initialized)
            {
                initialized = true;
                ins = this;
                // gameObject.name = "ModManager";
            }

            if (!loadedOnce)
                LoadAll ();
        }

        [Title ("Mod project manager", TitleAlignment = TitleAlignments.Centered)]
        [ShowInInspector, PropertyOrder (-1)]
        [PropertyTooltip ("The folder where mod projects are stored. If this folder does not exist, it will be created when you add your first mod.")]
        [PropertySpace (0f, 3f)]
        [LabelText ("Source folder"), LabelWidth (160f), ReadOnly, ElidedPath]
        public static string folderPathProjects;

        // Parts providing a way to add a new mod
        // Wrapped in a subclass to separate utility fields like error strings, reduce the need for top level grouping attributes etc.
        #region ModCreation

        [HideReferenceObjectPicker]
        public class ModSetup
        {
            [FoldoutGroup ("New mod", false), HorizontalGroup ("New mod/H")]
            [InfoBoxBottom ("@" + nameof(idError), InfoMessageType.Error, VisibleIf = nameof(isIDErrorVisible), OverlayColor = "#FFCCCC")]
            [OnValueChanged (nameof(ValidateNewID))]
            [HideLabel, SuffixLabel ("Unique ID & folder name", true)]
            public string id;

            [FoldoutGroup ("New mod"), HorizontalGroup ("New mod/H", 80f)]
            [EnableIf (nameof(creationPossible))]
            [Button ("Create", 21)]
            private void CreateMod ()
            {
                ValidateNewID ();
                if (!idValid)
                    return;

                var modData = new DataContainerModData ()
                {
                    key = id,
                    useDefaultWorkingPath = true,
                    metadata = new ModMetadata
                    {
                        id = id,
                        gameVersionMin = "2.0",
                        ver = "0.1",
                        name = "New Mod Name",
                        desc = descDefault
                    }
                };

                SaveMod (modData);
                mods.Add (id, modData);
                modSelectedID = id;
                id = string.Empty;
                idError = null;
                idValid = true;
            }

            private bool creationPossible => !string.IsNullOrEmpty (id) && idValid;
            private bool idValid;
            private string idError;
            private bool isIDErrorVisible => !idValid && !string.IsNullOrEmpty (idError);
            private const string descDefault = "Enter your description here. You can use some BBCode tags here, such as [b]bold[/b], [i]italic[/i] and [u]underlined[/u] text.\n\nYou can also embed more links if the URL field above is not enough:\n- [url=www.google.com][u]Example text[/u][/url]";

            private void ValidateNewID ()
            {
                idValid = ModToolsHelper.ValidateModID (id, null, mods, out idError);
            }
        }

        [ShowInInspector, HideLabel]
        public static readonly ModSetup modSetup = new ModSetup ();

        #endregion



        // Parts backing and displaying the selected mod
        #region ModSelected

        private static readonly SortedDictionary<string, DataContainerModData> mods = new SortedDictionary<string, DataContainerModData> ();

        public static string GetModCountText () => (mods != null ? mods.Count : 0).ToString ();
        public static IEnumerable<string> GetModKeys () => mods?.Keys;
        public static bool IsModSelectionVisible () => modSelected != null;
        public static bool IsModSelectionPossible () => SteamWorkshopHelper.IsUtilityOperationAvailable;
        public static string GetModSelectionTitle () => modSelected != null ? "Selected mod" : "No mod selected";
        public static Color GetSelectedKeyColor ()
        {
            if (DataContainerModData.selectedMod != null && DataContainerModData.selectedMod == modSelected)
                return DataContainerModData.colorSelected;
            return Color.white;
        }

        [Title ("Selected mod")]
        [ShowInInspector, PropertyOrder (21)]
        [ValueDropdown (nameof (GetModKeys))]
        [HideLabel, SuffixLabel ("$" + nameof (GetModCountText)), GUIColor (nameof (GetSelectedKeyColor))]
        [EnableIf (nameof(IsModSelectionPossible))]
        public static string modSelectedID
        {
            get
            {
                return modSelectedIDInternal;
            }
            set
            {
                // Disable config edits on changes to this key
                DataContainerModData.selectedMod = null;
                modSelectedIDInternal = value;
                modOptions.OnSelectionChange ();
            }
        }

        private static string modSelectedIDInternal = null;

        [ShowInInspector, PropertyOrder (24)]
        [ShowIf (nameof(IsModSelectionVisible))]
        [BoxGroup ("ModSelected", false)]
        [HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
        public static DataContainerModData modSelected
        {
            get
            {
                if (!string.IsNullOrEmpty (modSelectedID) && mods != null && mods.TryGetValue (modSelectedID, out var value))
                    return value;
                return null;
            }
            set
            {

            }
        }

        #endregion


        // Parts providing options for selected mod (save/load/delete/rename/duplicate and more)
        // Wrapped in a subclass to separate utility fields like error strings, reduce the need for top level grouping attributes etc.
        #region ModOptions

        [HideReferenceObjectPicker]
        public class ModOptions
        {
            [HorizontalGroup ("Bt1", 0.3333f)]
            [Button (SdfIconType.JournalArrowUp, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Reload")]
            public static void LoadProjectSelected ()
            {
                var id = modSelectedID;
                LoadProject (id);
            }

            public static void LoadProject (string id)
            {
                if (string.IsNullOrEmpty (id))
                {
                    Debug.LogWarning ($"");
                }

                var projectPath = DataPathHelper.GetCombinedCleanPath (folderPathProjects, id);
                var filePath = DataPathHelper.GetCombinedCleanPath (projectPath, filenameMain + extensionYAML);

                var modData = UtilitiesYAML.LoadDataFromFile<DataContainerModData> (filePath, true, false);
                if (modData == null)
                {
                    Debug.LogWarning ($"Can't load project: data not found using ID {id} | Full path: {filePath}");
                    return;
                }

                modData.OnAfterDeserialization (id);

                if (mods.ContainsKey (id))
                {
                    Debug.Log ($"Reloaded project {id} | Full path: {filePath}");
                    mods[id] = modData;
                }
                else
                {
                    Debug.Log ($"Loaded new project {id} | Full path: {filePath}");
                    mods.Add (id, modData);
                }
            }


            [HorizontalGroup ("Bt1", 0.3333f)]
            [Button (SdfIconType.JournalArrowDown, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Save")]
            public static void SaveProjectSelected ()
            {
                var modData = modSelected;
                SaveProject (modData);
            }

            public static void SaveProject (DataContainerModData modData)
            {
                if (modData == null)
                {
                    Debug.LogWarning ($"Can't save project: nothing selected");
                    return;
                }

                var id = modData.key;
                if (!ModToolsHelper.ValidateModID (id, modData, mods, out var idError))
                {
                    if (string.IsNullOrEmpty (idError))
                        Debug.LogWarning ($"Can't save project: invalid ID \"{id}\"");
                    else
                        Debug.LogWarning ($"Can't save project: \"{id}\" {idError}");
                    return;
                }

                Debug.Log ($"Saving mod {id}: {modData.GetModPathProject ()}");
                modData.OnBeforeSerialization ();

                var projectPath = modData.GetModPathProject ();
                var filePath = DataPathHelper.GetCombinedCleanPath (projectPath, filenameMain + extensionYAML);

                UtilitiesYAML.SaveToFile (filePath, modData);
            }


            [HorizontalGroup ("Bt1")]
            [Button (SdfIconType.JournalX, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Delete")]
            [PropertySpace (0f, 3f)]
            public static void DeleteProjectSelected ()
            {
                var modData = modSelected;
                DeleteProject (modData);
            }

            public static void DeleteProject (DataContainerModData modData)
            {
                if (modData == null)
                {
                    Debug.LogWarning ($"Can't delete project: nothing selected | Selected ID: {modSelectedID}");
                    return;
                }

                var projectPath = modData.GetModPathProject ();
                if (!Directory.Exists (projectPath))
                {
                    Debug.LogWarning ($"Can't delete project | {modSelectedID} has no valid path or path doesn't exist: {projectPath}");
                    return;
                }

                var id = modData.key;
                var text = $"Are you sure you'd like to delete this mod project (ID {modData.id}) in its entirety? This operation can not be reverted.";

                text += "\n\nProject folder: " + projectPath;
                text += "\n- Includes project.yaml with this config, metadata.yaml and all mod files.";

                var configPath = modData.GetModPathConfigs ();
                if (Directory.Exists (configPath))
                    text += "\n- Includes the Configs folder storing any changes made to game databases.";

                if (!EditorUtility.DisplayDialog ("Delete Mod Project", text, "Confirm", "Cancel"))
                    return;

                EditorCoroutineUtility.StartCoroutineOwnerless (DeleteProjectFolderIE (projectPath));

                mods.Remove (id);
                modSelectedID = string.Empty;
            }

            [ShowInInspector]
            [PropertyOrder (-1)]
            [FoldoutGroup ("Rename", false), HorizontalGroup ("Rename/H")]
            [InfoBoxBottom ("@" + nameof(idError), InfoMessageType.Error, VisibleIf = nameof(isIDErrorVisible), OverlayColor = "#FFCCCC")]
            [OnValueChanged (nameof(OnIDChange))]
            [HideLabel, SuffixLabel ("New ID", true)]
            public static string idNew;

            private bool idValid;
            private string idError;
            private bool isIDErrorVisible => !idValid && !string.IsNullOrEmpty (idError);

            private void OnIDChange ()
            {
                if (!string.IsNullOrEmpty (idNew) && idNew == modSelectedID)
                {
                    idValid = false;
                    idError = null;
                }
                else
                    idValid = ModToolsHelper.ValidateModID (idNew, modSelected, mods, out idError);
            }

            [PropertyOrder (-1)]
            [FoldoutGroup ("Rename"), HorizontalGroup ("Rename/H", 80f)]
            [EnableIf (nameof (idValid))]
            [Button ("Rename", 21)]
            public static void RenameConfigSelected ()
            {
                RenameConfigSelected (idNew);
            }

            public static void RenameConfigSelected (string idNew)
            {
                var modData = modSelected;
                if (modData == null)
                {
                    Debug.LogWarning ($"Can't rename project: nothing selected");
                    return;
                }

                if (string.Equals (idNew, modData.id))
                {
                    Debug.LogWarning ($"Can't rename project: no new name provided");
                    return;
                }

                if (!ModToolsHelper.ValidateModID (idNew, modData, mods, out var idError))
                {
                    if (string.IsNullOrEmpty (idError))
                        Debug.LogWarning ($"Can't rename project: invalid ID \"{idNew}\"");
                    else
                        Debug.LogWarning ($"Can't rename project: \"{idNew}\" {idError}");
                    return;
                }

                var idOld = modData.key;
                var sourcePath = DataPathHelper.GetCombinedCleanPath (folderPathProjects, idOld);

                if (Directory.Exists (sourcePath))
                {
                    var targetPath = DataPathHelper.GetCombinedCleanPath (folderPathProjects, idNew);
                    if (Directory.Exists (targetPath))
                    {
                        Debug.LogWarning ($"Can't rename project from \"{idOld}\" to \"{idNew}\": directory with the same name discovered on disk | Full path: {sourcePath}");
                        return;
                    }

                    try
                    {
                        Debug.Log ($"Trying to move folder:\n- Source: {sourcePath}\n- Target: {targetPath}");
                        Directory.Move (sourcePath, targetPath);
                    }
                    catch (IOException ioe)
                    {
                        Debug.LogError ("Key not changed -- error while renaming mod project directory: " + ioe.Message);
                        return;
                    }
                }

                mods.Remove (idOld);
                mods.Add (idNew, modData);
                modData.key = idNew;
                modSelectedID = idNew;

                SaveProject (modData);
                LoadProject (modData.id);
            }

            [PropertyOrder (-1)]
            [FoldoutGroup ("Rename"), HorizontalGroup ("Rename/H", 80f)]
            [EnableIf (nameof(idValid))]
            [Button ("Duplicate", 21)]
            public static void DuplicateConfigSelected ()
            {
                DuplicateConfigSelected (idNew);
            }

            public static void DuplicateConfigSelected (string idNew)
            {
                var modData = modSelected;
                if (modData == null)
                {
                    Debug.LogWarning ($"Can't duplicate project: nothing selected");
                    return;
                }

                if (!ModToolsHelper.ValidateModID (idNew, modData, mods, out var idError))
                {
                    if (string.IsNullOrEmpty (idError))
                        Debug.LogWarning ($"Can't duplicate project: invalid ID \"{idNew}\"");
                    else
                        Debug.LogWarning ($"Can't duplicate project: \"{idNew}\" {idError}");
                    return;
                }

                var idOld = modData.key;
                var sourcePath = DataPathHelper.GetCombinedCleanPath (folderPathProjects, idOld);
                var targetPath = DataPathHelper.GetCombinedCleanPath (folderPathProjects, idNew);

                if (!Directory.Exists (sourcePath))
                {
                    Debug.LogWarning ($"Can't duplicate project from ID \"{idOld}\" to \"{idNew}\": source directory not found on disk | Full path: {sourcePath}");
                    return;
                }

                if (Directory.Exists (targetPath))
                {
                    Debug.LogWarning ($"Can't duplicate project from ID \"{idOld}\" to \"{idNew}\": target directory with the same name discovered on disk | Full path: {targetPath}");
                    return;
                }

                UtilitiesYAML.CopyDirectory (sourcePath, targetPath, true);

                var modDataCopy = UtilitiesYAML.CloneThroughYaml (modData);
                modDataCopy.key = idNew;
                modDataCopy.metadata = UtilitiesYAML.CloneThroughYaml (modData.metadata);
                modDataCopy.SyncMetadata ();

                if (modDataCopy.workshop != null)
                    modDataCopy.workshop.publishedID = string.Empty;

                mods.Add (idNew, modDataCopy);
                modSelectedID = idNew;

                SaveProject (modDataCopy);
                LoadProject (modDataCopy.id);
            }

            [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightNeonBlue))]
            [ShowIf (nameof(IsConfigSetupAllowed))]
            [HorizontalGroup ("Bt2", 0.3333f)]
            [Button (SdfIconType.Intersect, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Setup config editing")]
            [PropertyTooltip ("Copy config databases from SDK to project folder. Do this if you want to create config overrides or edit levels.")]
            public static void SetupConfigs ()
            {
                var modData = modSelected;
                if (modData != null)
                    modData.EnableConfigs ();
            }

            [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightNeonGreen))]
            [ShowIf (nameof(IsConfigEntryAllowed))]
            [HorizontalGroup ("Bt2", 0.3333f)]
            [Button (SdfIconType.FileEarmarkTextFill, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Enter config editing")]
            [PropertyTooltip ("Switches all databases to config files copied into your mod project folder, enabling config editing")]
            public static void SelectForEditing ()
            {
                DataContainerModData.selectedMod = modSelected;
                ResetArea ();
                ResetDBs ();
            }

            [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightSelectedMod))]
            [ShowIf (nameof(IsConfigExitAllowed))]
            [HorizontalGroup ("Bt2", 0.3333f)]
            [Button (SdfIconType.FileX, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Exit config editing")]
            [PropertyTooltip ("Disables database editing, switching the editor back to reading backed up canonical Configs from the SDK folder.")]
            public static void DeselectForEditing ()
            {
                DataContainerModData.selectedMod = null;
                ResetArea ();
                ResetDBs ();
            }

            private static bool IsConfigSetupAllowed () => modSelected != null &&
                                                           modSelected.hasProjectFolder &&
                                                           !Directory.Exists (modSelected.GetModPathConfigs ());
            public static bool IsConfigExitAllowed () => DataContainerModData.selectedMod != null;
            public static bool IsConfigEntryAllowed () => DataContainerModData.selectedMod == null &&
                                                          modSelected != null &&
                                                          modSelected.hasProjectFolder &&
                                                          Directory.Exists (modSelected.GetModPathConfigs ());

            // [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightNeonRed))]
            [HorizontalGroup ("Bt2", 0.3333f)]
            [Button (SdfIconType.Boxes, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Export to user")]
            [PropertyTooltip ("Export the mod into the user folder, allowing you to test it the next time you start the game.\n\nBefore the export, the original data will be compared to the data in the mod project folder: only the modified files will be exported. Make sure to check the appropriate metadata fields.")]
            public static void ExportToUserFolder ()
            {
                var modData = modSelected;
                if (modData != null)
                    ModToolsExperimental.GenerateModFiles (modSelected, modData.ExportToUserFolderFinalize);
            }

            // [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightNeonRed))]
            [HorizontalGroup ("Bt2")]
            [Button (SdfIconType.BoxSeam, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Export to archive")]
            [PropertyTooltip ("Package the mod into a .zip file, allowing you to share it with other players.\n\nBefore the export, the original data will be compared to the data in the mod project folder: only the modified files will be exported. Make sure to check the appropriate metadata fields.")]
            public static void ExportToArchive ()
            {
                var modData = modSelected;
                if (modData != null)
                    ModToolsExperimental.GenerateModFiles (modSelected, modData.ExportToArchiveFinalize);
            }
            
            [FoldoutGroup("Utilities")]
            [HorizontalGroup ("Utilities/Bt3")]
            [EnableIf (nameof(IsConfigEntryAllowed))]
            [Button (SdfIconType.FileEarmarkBreakFill, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Reset all configs")]
            [PropertyTooltip ("Replace the Configs folder with the original files from the SDK. Equivalent to setting up config editing for the first time.")]
            public static void ResetConfigs ()
            {
                var modData = modSelected;
                if (modData == null)
                    return;

                var projectPath = modData.GetModPathProject (); 
                if (!EditorUtility.DisplayDialog 
                (
                    "Reset configs from SDK", 
                    $"Are you sure you'd like to replace the Configs folder in the selected mod (ID {modData.id}) with the original files from the SDK? This operation can not be reverted. Back up your changes if you are not sure.\n\nProject folder: \n{projectPath}", 
                    "Confirm", 
                    "Cancel")
                )
                {
                    return;
                }
                
                ModToolsExperimental.CopyConfigsFromSDK (modData);
                DeselectForEditing ();
            }
            
            // [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightNeonRed))]
            [FoldoutGroup("Utilities")]
            [HorizontalGroup ("Utilities/Bt3")]
            [Button (SdfIconType.Box, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Export to source")]
            [PropertyTooltip ("Export the mod files into the mod project folder without taking any additional step (no export to user folder, archive or Workshop).")]
            public static void ExportSimple ()
            {
                var modData = modSelected;
                if (modData != null)
                    ModToolsExperimental.GenerateModFiles (modSelected, null);
            }
            
            [FoldoutGroup("Utilities")]
            [HorizontalGroup ("Utilities/Bt3")]
            [Button (SdfIconType.Files, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Import from mod")]
            [PropertyTooltip ("Import files from an exported mod into this mod project. An inverse of the standard export operations.")]
            public static void ImportFromFolder ()
            {
                var modData = modSelected;
                if (modData == null)
                   return;
                
                var pathProject = modData.GetModPathProject ();
                var pathSelected = EditorUtility.OpenFolderPanel ("Select Folder", pathProject, "");
                if (string.IsNullOrEmpty (pathSelected))
                    return;
                
                var dirSelected = new DirectoryInfo (pathSelected);
                if (!dirSelected.Exists)
                {
                    Debug.LogWarning ($"Can't import, no folder selected");
                    return;
                }
                
                if (!EditorUtility.DisplayDialog 
                (
                    "Import files?", 
                    $"Are you sure you'd like to overwrite the contents of the Configs folder in the selected mod project (ID {modData.id}) with the original files from the SDK?"+ 
                    $"\n\nFrom folder: \n{pathSelected}/ConfigOverrides" + 
                    $"\n\nTo project folder: \n{pathProject}/Configs",
                    "Confirm", 
                    "Cancel")
                )
                {
                    return;
                }

                ModToolsExperimental.CopyConfigsFromExportedMod (modData, pathSelected);
            }
            
            public void OnSelectionChange ()
            {
                // Reset inputs
                idNew = modSelected != null ? modSelected.id : string.Empty;
                OnIDChange ();
            }
        }


        [ShowIf (nameof(IsModSelectionVisible))]
        [ShowInInspector, HideLabel, PropertyOrder (23)]
        [BoxGroup ("BgSelected", false)]
        public static readonly ModOptions modOptions = new ModOptions ();

        #endregion



        [PropertyOrder (-1)]
        [HorizontalGroup ("BgGlobal")]
        [Button (SdfIconType.JournalArrowUp, IconAlignment.LeftOfText, ButtonHeight = 32, Name = "Load all")]
        private static void LoadAll ()
        {
            folderPathProjects = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetUserFolder (), "ModsSource");
            var modsLoaded = UtilitiesYAML.LoadDecomposedDictionary<DataContainerModData>
            (
                folderPathProjects,
                logExceptions: true,
                appendApplicationPath: false,
                directoryMode: true,
                directoryModeFilename: filenameMain,
                forceLowerCase: false
            );

            loadedOnce = true;
            mods.Clear ();

            foreach (var kvp in modsLoaded)
            {
                var id = kvp.Key;
                var mod = kvp.Value;
                mods.Add (id, mod);
            }

            Debug.Log ($"Loaded {mods.Count} mod projects");
            foreach (var kvp in mods)
            {
                var id = kvp.Key;
                var mod = kvp.Value;
                mod.OnAfterDeserialization (id);
            }

            // Debug.LogWarning ($"Discovered project files: {modsLoaded.ToStringFormattedKeyValuePairs (true, toStringOverride: (x) => $"ID: {x.id} / Key: {x.key}")}");
        }

        [PropertyOrder (-1)]
        [HorizontalGroup ("BgGlobal")]
        [Button (SdfIconType.JournalArrowDown, IconAlignment.LeftOfText, ButtonHeight = 32, Name = "Save all")]
        [PropertySpace (0f, 3f)]
        private static void SaveAll ()
        {
            if (mods == null)
                return;

            foreach (var kvp in mods)
            {
                var modData = kvp.Value;
                if (modData != null)
                    SaveMod (modData);
            }
        }

        static void ResetArea ()
        {
            var obj = FindObjectOfType<AreaManager>();
            if (obj == null)
            {
                return;
            }
            obj.UnloadArea (false);
            DataMultiLinkerCombatArea.selectedArea = null;
        }

        static void ResetDBs ()
        {
            var obj = FindObjectOfType<UtilityDatabaseSerialization>();
            if (obj == null)
            {
                Debug.LogError ("Unable to find gameobject for UtilityDatabaseSerialization");
                return;
            }
            obj.ResetLoadedOnce ();

            DataManagerText.ResetLoadedOnce ();
            ItemHelper.LoadVisuals ();
        }

        static IEnumerator DeleteProjectFolderIE (string projectPath)
        {
            var progressID = Progress.Start ("Delete project folder " + Path.GetFileName (projectPath), "Starting...");
            Progress.SetStepLabel(progressID, "items");
            yield return null;

            var directories = GetProjectSubdirectories (projectPath);
            var count = directories.Count;
            for (var i = 0; i < count; i += 1)
            {
                var d = directories[i];
                var n = d.Substring (projectPath.Length + 1);
                Progress.Report (progressID, i, count, "Deleting " + n);
                Directory.Delete (d, true);
                yield return null;
            }
            Progress.Report (progressID, 0.99f, "Deleting " + projectPath);
            Directory.Delete (projectPath, true);
            yield return null;
            Progress.Finish (progressID);
        }

        static List<string> GetProjectSubdirectories (string projectPath)
        {
            var directories = Directory.GetDirectories (projectPath).ToList();
            var i = 0;
            while (i < directories.Count)
            {
                if (Path.GetFileName (directories[i]) != "Configs")
                {
                    i += 1;
                    continue;
                }
                var configsPath = directories[i];
                directories.RemoveAt (i);
                var dpath = Path.Combine (projectPath, "Configs", "DataDecomposed");
                if (Directory.Exists (dpath))
                {
                    directories.AddRange (Directory.GetDirectories (dpath));
                }
                directories.Add (configsPath);
                break;
            }
            return directories;
        }

        public static void SaveMod (DataContainerModData modData)
        {
            ModOptions.SaveProject (modData);
        }

        public static void SelectObject ()
        {
            if (ins != null)
                UnityEditor.Selection.activeObject = ins;
            else
            {
                var obj = GameObject.FindObjectOfType<DataManagerMod> ();
                if (obj != null)
                    UnityEditor.Selection.activeObject = obj;
            }
        }

        public static List<DataContainerModData> GetConfigEditMods () => mods.Values
            .Where (md => md.hasProjectFolder && Directory.Exists (md.GetModPathConfigs ()))
            .ToList ();

        #endif
    }
}
