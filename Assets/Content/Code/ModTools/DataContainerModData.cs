using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using PhantomBrigade.ModTools;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    using Data;
    using Mods;
    
    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class ConfigsVersion
    {
        public const string versionExpected = "2.1.0";
        
        [HideInInspector]
        public string version;
        
        [GUIColor ("$" + nameof (GetColor))]
        [InfoBox ("$" + nameof (GetWarningText), InfoMessageType.Warning, VisibleIf = nameof (IsWarningVisible))]
        [HideLabel, SuffixLabel ("Configs version", true), ShowInInspector]
        private string versionVisible
        {
            get => version;
            set { }
        }

        private bool IsWarningVisible ()
        {
            return string.IsNullOrEmpty (version) || version != versionExpected;
        }

        private string GetWarningText ()
        {
            return $"{versionExpected} is the current version of the Configs folder in the SDK. The Configs folder might be outdated or might otherwise not match the current state of the game. This can cause incorrect mod exports and lead to ingame bugs. We recommend resetting configs to the latest state from the SDK.";
        }

        private Color GetColor ()
        {
            return IsWarningVisible () ? ModToolsColors.HighlightNeonSepia : ModToolsColors.HighlightNeonGreen;
        }
    }

    [Serializable, HideDuplicateReferenceBox]
    public class ModAssetBundles
    {
        [InfoBox ("@GetVersionWarningText", InfoMessageType.Error, VisibleIf = "IsVersionWarningVisible")]
        [ShowInInspector]
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new AssetBundleDesc ()")]
        public List<AssetBundleDesc> bundleDefinitions = new List<AssetBundleDesc> ();

        #if UNITY_EDITOR

        private bool IsVersionWarningVisible => !ModToolsHelper.IsUnityVersionSupported ();
        private string GetVersionWarningText => $"Warning! Exported assets will only be loaded by the game if your Editor version exactly matches the engine version of the game.\n\n- Game engine version: {ModToolsHelper.unityVersionExpected}\n- Editor engine version: {Application.unityVersion}";

        #endif
    }

    #if UNITY_EDITOR
    [Serializable, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public sealed class FileReferences
    {
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = nameof(CreateEntry))]
        public List<FileReference> files = new List<FileReference> ();

        FileReference CreateEntry () => new FileReference ();

        public void OnAfterDeserialization (DataContainerModData parent)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    file.OnAfterDeserialization (parent);
                }
            }
        }
    }

    [Serializable, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public sealed class FileReference
    {
        [HorizontalGroup (20f)]
        [PropertySpace (2f)]
        [HideLabel]
        public bool enabled = true;
        
        [HideInInspector]
        public bool relative = false;

        [HorizontalGroup]
        [EnableIf (nameof (enabled)), GUIColor (nameof (GetPathColor))]
        [Sirenix.OdinInspector.FilePath (AbsolutePath = true, RequireExistingPath = false, IncludeFileExtension = true, UseBackslashes = false)]
        [HideLabel, ShowInInspector, InlineButton (nameof(ToggleRelative), "$" + nameof(GetRelativeLabel))]
        public string pathProperty
        {
            get => GetFinalPath ();
            set => UpdatePathFromInput (value);
        }
        
        [ReadOnly, GUIColor(nameof(GetPathColor)), DisplayAsString (TextAlignment.Right)]
        [HideLabel, ShowIf(nameof(relative))]
        public string path;

        [YamlIgnore, HideInInspector]
        public DataContainerModData parent;

        public void OnAfterDeserialization (DataContainerModData parent)
        {
            this.parent = parent;
            UpdatePathFromInput (path);
        }

        private void UpdatePathFromInput (string pathInput)
        {
            if (string.IsNullOrEmpty (pathInput))
                return;
            
            if (relative)
            {
                var pathMod = parent.GetModPathProject ();
                if (!pathInput.StartsWith (pathMod))
                {
                    Debug.LogWarning ($"Relative path invalid, doesn't start in parent directory:\n- Mod: {pathMod}\n- Path: {path}");
                    return;
                }
            
                path = pathInput.Substring (pathMod.Length + 1);
            }
            else
            {
                path = pathInput;
            }
        }

        public string GetFinalPath ()
        {
            if (!relative)
                return path;

            if (parent == null)
                return null;

            var pathMod = parent.GetModPathProject ();
            var pathRelative = DataPathHelper.GetCombinedCleanPath (pathMod, path);
            return pathRelative;
        }

        private bool IsPathValid ()
        {
            var pathFinal = GetFinalPath ();
            return File.Exists (pathFinal);
        }

        private void ToggleRelative ()
        {
            relative = !relative;
            UpdatePathFromInput (path);
        }

        private string GetRelativeLabel ()
        {
            return relative ? " Relative " : " Absolute ";
        }

        private Color GetPathColor () => IsPathValid () ? relative ? ModToolsColors.HighlightNeonGreen : Color.white : ModToolsColors.HighlightNeonRed;
    }
    #endif

    [LabelWidth (100f), BoxGroup ("Root", false)]
    public class ModConfigStatus
    {
        #if UNITY_EDITOR

        public static IEnumerable<string> GetModKeys () => DataManagerMod.GetModKeys ();
        public static bool IsModSelectionPossible () => DataManagerMod.IsModSelectionPossible () && DataContainerModData.selectedMod == null;
        public static Color GetColor () => hasSelectedMod ? colorSelected : colorWarning; // DataManagerMod.GetSelectedKeyColor ();

        private static string GetStatusText ()
        {
            if (DataManagerMod.modSelected == null)
                return "You are viewing the original game data. It is a backup that can not be modified. Select a mod and enter config editing to unlock this inspector.";

            if (DataContainerModData.selectedMod == null)
            {
                if (!DataManagerMod.ModOptions.IsConfigEntryAllowed ())
                    return "You are viewing the original game data. It is a backup that can not be modified. Enter config editing with the currently selected mod to unlock this inspector.";

                return "You are viewing the original game data. It is a backup that can not be modified. Setup config editing with the currently selected mod to unlock this inspector.";
            }

            return "You are viewing a copy of game data from the mod folder. You can freely modify it. It will be compared to original game data on mod export.";
        }

        [ShowInInspector, InfoBox ("$" + nameof (GetStatusText), InfoMessageType.None, VisibleIf = nameof (hasSelectedMod))]
        [ShowIf (nameof (hasSelectedMod))]
        [PropertyTooltip ("$" + nameof (configsPath))]
        [HideLabel, ElidedPath]
        [GUIColor (nameof(colorSelected))]
        public static string configsPath
        {
            get => DataContainerModData.selectedMod?.GetModPathConfigs ();
            set
            {

            }
        }

        [ShowInInspector, InfoBox ("$" + nameof (GetStatusText), InfoMessageType.None, VisibleIf = nameof (hasNoSelectedMod))]
        [ValueDropdown (nameof (GetModKeys))]
        [HideLabel]
        [EnableIf (nameof(IsModSelectionPossible))]
        [GUIColor (nameof(GetColor))]
        public static string modSelectedID
        {
            get
            {
                return DataManagerMod.modSelectedID;
            }
            set
            {
                DataManagerMod.modSelectedID = value;
            }
        }

        [HorizontalGroup (groupHr)]
        [Button (SdfIconType.FileLock2Fill, "Open mod manager", ButtonHeight = 32)]
        [PropertySpace (0f, 12f)]
        [GUIColor (nameof(GetColor))]
        private static void OpenModManager () => DataManagerMod.SelectObject ();


        [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightNeonGreen))]
        [ShowIf (nameof(IsConfigEntryAllowed))]
        [HorizontalGroup (groupHr, 0.3333f)]
        [Button (SdfIconType.FileEarmarkTextFill, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Enter config editing")]
        [PropertyTooltip ("Switches all databases to config files copied into your mod project folder, enabling config editing")]
        public static void SelectForEditing ()
        {
            DataManagerMod.ModOptions.SelectForEditing ();
            EditorCoroutineUtility.StartCoroutineOwnerless (OnConfigEditingToggle ());
        }

        [GUIColor ("@ModToolsColors." + nameof (ModToolsColors.HighlightSelectedMod))]
        [ShowIf (nameof(IsConfigExitAllowed))]
        [HorizontalGroup (groupHr, 0.3333f)]
        [Button (SdfIconType.FileX, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Exit config editing")]
        [PropertyTooltip ("Disables database editing, switching the editor back to reading backed up canonical Configs from the SDK folder.")]
        public static void DeselectForEditing ()
        {
            DataManagerMod.ModOptions.DeselectForEditing ();
            EditorCoroutineUtility.StartCoroutineOwnerless (OnConfigEditingToggle ());
        }

        // Switching config editing mode on or off might not update auto-assigned readonly attributes in the currently visible inspector
        // This coroutine is a quick hack to solve this, switching away from a current selection then returning to it to guarantee a redraw
        private static IEnumerator OnConfigEditingToggle ()
        {
            var selectionStart = Selection.activeGameObject;
            if (selectionStart == null)
                yield break;

            Selection.activeGameObject = null;
            yield return new EditorWaitForSeconds (0.1f);
            Selection.activeGameObject = selectionStart;

        }

        private static bool IsConfigExitAllowed () => DataManagerMod.ModOptions.IsConfigExitAllowed ();
        private static bool IsConfigEntryAllowed () => DataManagerMod.ModOptions.IsConfigEntryAllowed ();

        private static bool hasSelectedMod => DataContainerModData.hasSelectedConfigs;
        private static bool hasNoSelectedMod => !DataContainerModData.hasSelectedConfigs;

        private const string groupHr = "Selected mod";
        private const string groupHrLine = "Selected mod/Line";

        private static Color colorWarning => ModToolsColors.HighlightNeonSepia;
        private static Color colorSelected => DataContainerModData.colorSelected;

        #endif
    }

    [LabelWidth (160f)]
    public class DataContainerModData : DataContainer
    {
        #if !UNITY_EDITOR

        public static DataContainerModData selectedMod;
        public static bool hasSelectedConfigs => false;
        public string GetModPathConfigs () => null;

        #else

        public static DataContainerModData selectedMod;
        public static bool hasSelectedConfigs => selectedMod != null && selectedMod.metadata != null && selectedMod.metadata.isConfigEnabled;

        // Internal ID doubling as directory name for the mod.
        // Backed by .key field in DataContainer, which can be used for this purpose
        // due to lowercase restrictions becoming optional in UtilitiesYAML
        [VerticalGroup (OdinGroup.Name.Project, Order = OdinGroup.Order.Project)]
        [LabelText ("ID")]
        public string id => key;

        // Actual mod data folder (separated from where these YAML configs are saved)
        // The field is here to make it easy to display where mod folders are located.
        [YamlIgnore] // Not serialized, set from DataManagerMod load method
        [VerticalGroup (OdinGroup.Name.Project)]
        [ConditionalSpace (0f, 4f, nameof(spaceAfterWorkingPath))]
        [PropertyTooltip ("$" + nameof(projectPath))]
        [LabelText ("Project path"), ElidedPath]
        public string projectPath;

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.Project)]
        [ShowIf (nameof(showUserModFolder))]
        [ConditionalSpace (0f, 4f, nameof(spaceAfterUserModFolder))]
        [PropertyTooltip ("$" + nameof(userModFolder))]
        [EnableGUI, ElidedPath]
        [InlineButton (nameof(DeleteUserModFolder), "Delete", ShowIf = nameof(showDeleteUserModFolder))]
        public string userModFolder => GetModPathUser ();

        public void EnableConfigs ()
        {
            ModToolsExperimental.CopyConfigsFromSDK (this);
            
            metadata.isConfigEnabled = true;
            metadata.includesConfigOverrides = true;
            
            ModToolsHelper.SaveMod (this);

            if (GUIHelper.CurrentWindow != null)
                GUIHelper.CurrentWindow.Repaint ();
        }

        public void ExportToUserFolderFinalize ()
        {
            var dirPathUser = GetModPathUser ();
            TryExportToFolderShared (dirPathUser, "user Mods folder", true);
        }

        public bool TryExportToFolderShared (string dirPathTarget, string targetDesc, bool targetOpen)
        {
            if (!UtilitiesYAML.IsDirectoryNameValid (id, out var errorDesc))
            {
                Debug.LogWarning ($"Can't export the mod ID {id} to {targetDesc}, ID is not usable as a directory name: {errorDesc}");
                return false;
            }

            var projectPath = GetModPathProject ();
            if (!Directory.Exists (projectPath))
            {
                Debug.LogWarning ($"Can't export the mod ID {id} to {targetDesc}, local project folder doesn't exist. Make sure to save this DB. Expected path: " + projectPath);
                return false;
            }

            if (string.IsNullOrEmpty (dirPathTarget))
            {
                Debug.LogWarning ($"Can't export the mod ID {id}, {targetDesc} folder not provided. Make sure to check the ID and project folder of this mod.");
                return false;
            }

            if (!EditorUtility.DisplayDialog ("Start export", $"Are you sure you'd like to export mod ID {id} to {targetDesc}?\n\nProject folder: \n{projectPath}\n\nTarget folder: \n{dirPathTarget}", "Confirm", "Cancel"))
                return false;

            bool success = true;
            try
            {
                CopyContents (projectPath, dirPathTarget);
                if (targetOpen)
                    Application.OpenURL ("file://" + dirPathTarget);
            }
            catch (Exception e)
            {
                Debug.LogWarning ("Failed to export the mod | Encountered an exception:\n" + e);
                success = false;
            }

            return success;
        }

        public void ExportToArchiveFinalize ()
        {
            if (!UtilitiesYAML.IsDirectoryNameValid (id, out var errorDesc))
            {
                Debug.LogWarning ($"Can't package the mod. ID {id} should be usable as a directory name: {errorDesc}");
                return;
            }

            var projectPath = GetModPathProject ();
            if (!Directory.Exists (projectPath))
            {
                Debug.LogWarning ("Can't package the mod. The local project folder should exist before packaging. Expected path: " + projectPath);
                return;
            }

            var dirPathPkg = DataPathHelper.GetCombinedCleanPath (projectPath, Path.GetFileName (projectPath));
            Directory.CreateDirectory (dirPathPkg);

            try
            {
                var zipPath = dirPathPkg + "Mod.zip";
                if (File.Exists (zipPath))
                {
                    Debug.Log ($"Deleting existing archive...");
                    File.Delete (zipPath);
                }

                CopyContents (projectPath, dirPathPkg);
                ZipFile.CreateFromDirectory (dirPathPkg, zipPath, System.IO.Compression.CompressionLevel.Optimal, true);
                Application.OpenURL ("file://" + projectPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning ("Failed to export the mod | Encountered an exception:\n" + e);
            }

            Directory.Delete (dirPathPkg, true);
        }

        public void DeleteUserModFolder ()
        {
            var dirPathUser = GetModPathUser ();
            if (!Directory.Exists (dirPathUser))
            {
                return;
            }
            if (!EditorUtility.DisplayDialog ("Delete User Mod Folder", $"Are you sure you'd like to delete the user folder for this mod (ID {id}, config {key})\n\nUser folder: \n{dirPathUser}", "Confirm", "Cancel"))
            {
                return;
            }
            try
            {
                Directory.Delete (dirPathUser, true);
            }
            catch (IOException ioe)
            {
                Debug.LogWarning ("Failed to delete user mod folder -- encountered an exception | path: " + dirPathUser + "\n" + ioe);
            }
        }
        
        [ShowIf (nameof(IsConfigsVersionVisible))]
        [HideLabel, YamlIgnore]
        public ConfigsVersion configsVersion;

        public void RefreshConfigsVersion ()
        {
            var pathConfigs = GetModPathConfigs ();
            configsVersion = null;
            
            if (hasProjectFolder && Directory.Exists (pathConfigs))
            {
                var configsVersionPath = DataPathHelper.GetCombinedCleanPath (pathConfigs, "dataVersion.yaml");
                configsVersion = UtilitiesYAML.LoadDataFromFile<ConfigsVersion> (configsVersionPath, false, false);
                if (configsVersion == null)
                    configsVersion = new ConfigsVersion ();
            }
        }

        private bool IsConfigsVersionVisible ()
        {
            return hasProjectFolder && Directory.Exists (GetModPathConfigs ());
        }

        // Metadata is not embedded directly into this config, to ensure there is only
        // one source of truth on disk: metadata.yaml in the mod directory. It's easy to
        // redirect serialization to that separate file via OnAfterD./OnBeforeS. overrides
        [YamlIgnore]
        [FoldoutGroup(OdinGroup.Name.Metadata, true), PropertyOrder (OdinGroup.Order.Metadata), PropertySpace (8f)]
        [EnableIf (nameof(hasProjectFolder))]
        [OnValueChanged (nameof(SyncMetadata), true)]
        [HideLabel]
        public ModMetadata metadata;

        // Optional data needed for Steam Workshop uploads
        [DropdownReference (true), PropertyOrder (199)]
        [GUIColor ("@" + nameof(colorSteam))]
        [OnValueChanged ("OnWorkshopChange", true)]
        public ModWorkshopData workshop;

        public bool HasWorkshop => workshop != null && metadata != null;

        [BoxGroup (OdinGroup.Name.Workshop), PropertyOrder (OdinGroup.Order.Workshop)]
        [EnableIf (nameof(isWorkshopUploadable))]
        [ShowIf (nameof(isWorkshopUsable))]
        [GUIColor ("@" + nameof(colorSteam))]
        [HorizontalGroup (OdinGroup.Name.WorkshopButtons, 0.5f)]
        [Button (SdfIconType.Steam, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Export to Steam Workshop")]
        public void UploadToWorkshop () => SteamWorkshopHelper.UploadToWorkshop (this);

        [BoxGroup (OdinGroup.Name.Workshop), PropertyOrder (OdinGroup.Order.Workshop)]
        [ShowIf (nameof(isWorkshopUsable))]
        [GUIColor ("@" + nameof(colorSteam))]
        [HorizontalGroup (OdinGroup.Name.WorkshopButtons)]
        [Button (SdfIconType.ArrowDownCircle, IconAlignment.LeftEdge, ButtonHeight = 32, Name = "Check local install")]
        public void GetInstallState () => SteamWorkshopHelper.GetInstallState (this);

        [BoxGroup (OdinGroup.Name.Workshop), PropertyOrder (OdinGroup.Order.Workshop)]
        [ShowIf (nameof(isWorkshopBusy))]
        [GUIColor ("@" + nameof(colorSteam))]
        [Button ("Stop workshop operation", ButtonSizes.Large)]
        public void CancelCoroutine () => SteamWorkshopHelper.StopCoroutine ();

        [DropdownReference (true)]
        [PropertyOrder (OdinGroup.Order.ConfigEdits)]
        public ModConfigEditSource configEdits;

        [DropdownReference (true)]
        [PropertyOrder (OdinGroup.Order.TextEdits)]
        public ModConfigLocEdit textEdits;

        [DropdownReference (true)]
        [PropertyOrder (OdinGroup.Order.AssetBundles)]
        public ModAssetBundles assetBundles;

        [DropdownReference (true)]
        [PropertyOrder (OdinGroup.Order.LibraryDLLs)]
        [LabelText ("Library DLLs")]
        public FileReferences libraryDLLs;

        [DropdownReference (true)]
        [PropertyOrder (OdinGroup.Order.ExtraFiles)]
        [LabelText ("Extra Files")]
        public FileReferences extraFiles;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (!string.IsNullOrEmpty (projectPath))
            {
                var filePath = DataPathHelper.GetCombinedCleanPath (projectPath, "metadata.yaml");
                if (File.Exists (filePath))
                {
                    metadata = UtilitiesYAML.LoadDataFromFile<ModMetadata> (filePath, true, false);
                }
            }

            // Loaded metadata might be null or have conflicting ID defined, so an immediate sync is needed
            SyncMetadata ();

            var pathConfigs = GetModPathConfigs ();
            // Debug.Log ($"Loaded mod {key}\n- Project path: {GetModPathProject ()}\n- Configs path: {pathConfigs}");

            metadata.isConfigEnabled = Directory.Exists (pathConfigs);

            configsVersion = null;
            if (metadata.isConfigEnabled)
                RefreshConfigsVersion ();

            if (workshop != null)
                workshop.parent = this;

            if (configEdits != null)
                configEdits.OnAfterDeserialization ();

            if (textEdits != null)
                textEdits.OnAfterDeserialization ();
            
            if (libraryDLLs != null)
                libraryDLLs.OnAfterDeserialization (this);
            
            if (extraFiles != null)
                extraFiles.OnAfterDeserialization (this);
        }

        private void OnWorkshopChange ()
        {
            if (workshop != null)
                workshop.parent = this;
        }

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            SyncMetadata ();
            
            if (string.IsNullOrEmpty (projectPath))
            {
                Debug.LogWarning ("Couldn't save metadata.yaml, project folder path is null or empty!");
                return;
            }

            if (!Directory.Exists (projectPath))
            {
                Debug.LogWarning ("Couldn't save metadata.yaml, project folder doesn't exist: " + projectPath);
                return;
            }

            UtilitiesYAML.SaveToFile (projectPath, "metadata.yaml", metadata);
        }

        public void SyncMetadata ()
        {
            metadata ??= new ModMetadata ();
            metadata.id = id;
            metadata.includesConfigEdits = configEdits != null;
            metadata.includesAssetBundles = assetBundles != null;
            metadata.includesLocalizationEdits = textEdits != null;

            var pathProject = GetModPathProject ();
            metadata.includesTextures = !string.IsNullOrEmpty (pathProject) && Directory.Exists (GetModPathProject () + "/Textures");
            metadata.includesLibraries = libraryDLLs != null && libraryDLLs.files.Count (f => f.enabled) != 0;
        }

        public static readonly string defaultWorkingPathParent = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetUserFolder (), "ModsSource");

        public bool hasProjectFolder => Directory.Exists (GetModPathProject ());
        public const string dirWorkshopTemp = "/WorkshopTemp";

        public string GetModPathProject ()
        {
            return projectPath;
        }

        public string GetModPathWorkshopTemp ()
        {
            return projectPath + dirWorkshopTemp;
        }

        public string GetModPathUser ()
        {
            var projectPath = GetModPathProject ();
            if (string.IsNullOrEmpty (projectPath))
            {
                return null;
            }

            var localPath = DataPathHelper.GetCombinedCleanPath ("Mods", Path.GetFileName (projectPath));
            var dirPath = DataPathHelper.GetCombinedPath (PathParentType.User, localPath);
            return dirPath;
        }

        public string GetModPathConfigs ()
        {
            var projectPath = GetModPathProject ();
            return string.IsNullOrEmpty (projectPath) ? null : DataPathHelper.GetCombinedCleanPath (projectPath, "Configs");
        }

        public void DeleteOutputDirectories ()
        {
            var configOverridesPath = DataPathHelper.GetCombinedCleanPath (GetModPathProject (), overridesFolderName);
            if (Directory.Exists (configOverridesPath))
                Directory.Delete (configOverridesPath, true);
            
            var configEditsPath = DataPathHelper.GetCombinedCleanPath (GetModPathProject (), editsFolderName);
            if (Directory.Exists (configEditsPath))
                Directory.Delete (configEditsPath, true);
            
            var assetBundlesPath = DataPathHelper.GetCombinedCleanPath (GetModPathProject (), assetBundlesFolderName);
            if (Directory.Exists (assetBundlesPath))
                Directory.Delete (assetBundlesPath, true);
        }

        void CopyContents (string projectPath, string destPath)
        {
            OnBeforeSerialization ();
            UtilitiesYAML.PrepareClearDirectory (destPath, false, false);
            UtilitiesYAML.SaveToFile (destPath, "metadata.yaml", metadata);
            CopyFolder (metadata.includesConfigEdits, projectPath, destPath, editsFolderName);
            CopyFolder (metadata.includesConfigOverrides, projectPath, destPath, overridesFolderName);
            CopyFolder (metadata.includesLocalizationEdits, projectPath, destPath, "LocalizationEdits");
            CopyFolder (metadata.includesLocalizations, projectPath, destPath, "Localizations");
            CopyFolder (metadata.includesTextures, projectPath, destPath, "Textures");
            CopyFolder (metadata.includesAssetBundles, projectPath, destPath, "AssetBundles");
            CopyFolder (metadata.includesLibraries, projectPath, destPath, "Libraries");
            CopyExtraFiles (destPath);
        }

        void CopyExtraFiles (string destPath)
        {
            if (extraFiles == null || extraFiles.files.Count == 0)
            {
                return;
            }
            foreach (var fr in extraFiles.files)
            {
                if (!fr.enabled)
                {
                    continue;
                }
                
                var pathSource = fr.GetFinalPath ();
                var fileSource = new FileInfo (pathSource);
                
                if (!fileSource.Exists)
                {
                    Debug.Log ($"External file doesn't exist: {pathSource}");
                    continue;
                }

                var filename = Path.GetFileName (pathSource);
                var pathDest = DataPathHelper.GetCombinedCleanPath (destPath, filename);
                
                if (string.Equals ("metadata.yaml", filename))
                    continue;

                Debug.Log ($"Copying external file...\nFrom: {pathSource}\nTo: {pathDest}");
                File.Copy (pathSource, pathDest, true);
            }
        }

        static void CopyFolder (bool includes, string dirPathProject, string dirPathUser, string folderName)
        {
            if (!includes)
            {
                return;
            }
            var dirFrom = dirPathProject + "/" + folderName;
            var dirTo = dirPathUser + "/" + folderName;
            UtilitiesYAML.CopyDirectory (dirFrom, dirTo, true);
            if (!Directory.Exists (dirTo))
            {
                // Folder for content must exist if metadata says mod has that content, even if the folder is empty.
                // Otherwise the game will display a warning.
                Directory.CreateDirectory (dirTo);
            }
        }
        
        bool spaceAfterWorkingPath => !showUserModFolder;
        bool showUserModFolder => Directory.Exists (userModFolder);
        bool spaceAfterUserModFolder => showUserModFolder;

        bool showEnableConfigs => hasProjectFolder && !Directory.Exists (GetModPathConfigs ());
        bool enablePackagingButtons => metadata != null;
        bool showDeleteUserModFolder => Directory.Exists (GetModPathUser ());

        bool enableWorkshop => enablePackagingButtons && hasProjectFolder && HasWorkshop;
        bool isWorkshopUploadable => isWorkshopUsable && !string.IsNullOrEmpty (workshop?.changes);
        bool isWorkshopUsable => enableWorkshop && SteamWorkshopHelper.IsUtilityOperationAvailable;
        bool isWorkshopBusy => enableWorkshop && !SteamWorkshopHelper.IsUtilityOperationAvailable;

        bool isSelected => selectedMod == this;
        public Color selectButtonColor => isSelected ? colorSelected : Color.white;
        public string selectButtonLabel => isSelected ? "Deselect" : "Select";

        public static readonly Color colorSteam = Color.Lerp (new Color (0.48f, 0.72f, 1f), Color.white, 0.6f);
        public static readonly Color colorSelected = new Color (0.8f, 1f, 0.9f);

        public const string assetBundlesFolderName = "AssetBundles";
        public const string configsFolderName = "Configs";
        public const string overridesFolderName = "ConfigOverrides";
        public const string editsFolderName = "ConfigEdits";
        public const string librariesFolderName = "Libraries";
        public const string localizationEditsFolderName = "LocalizationEdits";

        static class OdinGroup
        {
            public static class Name
            {
                public const string Metadata = nameof(Metadata);
                public const string Project = nameof(Project);
                public const string Workshop = "workshop";
                public const string WorkshopButtons = Workshop + "/Buttons";
            }

            public static class Order
            {
                public const float Project = 0f;
                public const float Metadata = 1f;
                public const float Workshop = 2f;
                public const float ConfigEdits = 3f;
                public const float TextEdits = 4f;
                public const float AssetBundles = 5f;
                public const float LibraryDLLs = 6f;
                public const float ExtraFiles = 7f;
            }
        }

        [HorizontalGroup ("Dropdown")]
        [ShowInInspector, PropertyOrder (100), DisplayAsString, HideLabel, ReadOnly]
        private const string helperTip = " Click the button to the right to add more features";

        [HorizontalGroup ("Dropdown")]
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerModData () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }
}
