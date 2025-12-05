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
    }

    [Serializable, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public sealed class FileReference
    {
        [HorizontalGroup (20f)]
        [PropertySpace (2f)]
        [HideLabel]
        public bool enabled = true;

        [HorizontalGroup]
        [EnableIf (nameof(enabled))]
        [Sirenix.OdinInspector.FilePath (AbsolutePath = true, IncludeFileExtension = true, RequireExistingPath = true, UseBackslashes = false)]
        [HideLabel]
        public string path;
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

        [VerticalGroup (OdinGroup.Name.Project)]
        // [ShowIf (nameof(hasProjectFolder))]
        [PropertyTooltip ("Use the default assigned folder for this mod's project files.\nUncheck if you want change the folder used to store the project files.")]
        [OnValueChanged (nameof(InitializeWorkingPath))]
        [LabelText ("Default project path")]
        public bool useDefaultWorkingPath = true;

        // Actual mod data folder (separated from where these YAML configs are saved)
        // The field is here to make it easy to display where mod folders are located.
        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.Project)]
        [ShowIf (nameof (showDefaultPath))]
        [ConditionalSpace (0f, 4f, nameof (spaceAfterWorkingPath))]
        [PropertyTooltip ("$" + nameof (displayWorkingPath))]
        [HideLabel, ElidedPath]
        public string displayWorkingPath
        {
            get => GetModPathProject ();
            set { }
        }

        // Let the modder override where the project files are stored for this particular mod.
        [VerticalGroup (OdinGroup.Name.Project)]
        [ShowIf (nameof(showWorkingPath))]
        [ConditionalSpace (0f, 4f, nameof(spaceAfterWorkingPath))]
        [PropertyTooltip ("$" + nameof(workingPath))]
        [LabelText ("Custom project path"), ElidedPath]
        public string workingPath;

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
            EditorCoroutineUtility.StartCoroutineOwnerless (EnableConfigsIE (GUIHelper.CurrentWindow));
        }

        IEnumerator EnableConfigsIE (EditorWindow inspectorWindow)
        {
            var root = new DirectoryInfo (DataPathHelper.GetApplicationFolder ());
            var dest = GetModPathConfigs ();
            Directory.CreateDirectory (dest);
            yield return ModToolsHelper.CopyConfigsIE (root, dest);
            metadata.isConfigEnabled = true;
            metadata.includesConfigOverrides = true;
            ModToolsHelper.SaveMod (this);
            if (inspectorWindow != null)
            {
                inspectorWindow.Repaint ();
            }
        }

        public void ExportToUserFolder ()
        {
            var (result, upgrade) = ModToolsHelper.EnsureModChecksums (this);
            switch (result)
            {
                case EnsureResult.Error:
                    EditorUtility.DisplayDialog ("Config Editing Unavailable", "A technical error is preventing you from exporting this mod. Please check the Unity log console for details.", "Dismiss");
                    return;
                case EnsureResult.Break:
                    Debug.Log ("Cancelled export to user folder: " + id);
                    return;
            }

            if (upgrade == null)
            {
                ModToolsHelper.GenerateModFiles (this, ExportToUserFolderFinalize);
                return;
            }
            EditorCoroutineUtility.StartCoroutineOwnerless (UpgradeAndContinue ());
            return;

            IEnumerator UpgradeAndContinue ()
            {
                yield return upgrade ();
                ModToolsHelper.GenerateModFiles (this, ExportToUserFolderFinalize);
            }
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

        public void ExportToArchive ()
        {
            var (result, upgrade) = ModToolsHelper.EnsureModChecksums (this);
            switch (result)
            {
                case EnsureResult.Error:
                    EditorUtility.DisplayDialog ("Config Editing Unavailable", "A technical error is preventing you from exporting this mod. Please check the Unity log console for details.", "Dismiss");
                    return;
                case EnsureResult.Break:
                    Debug.Log ("Cancelled export to archive: " + id);
                    return;
            }

            if (upgrade == null)
            {
                ModToolsHelper.GenerateModFiles (this, ExportToArchiveFinalize);
                return;
            }
            EditorCoroutineUtility.StartCoroutineOwnerless (UpgradeAndContinue ());
            return;

            IEnumerator UpgradeAndContinue ()
            {
                yield return upgrade ();
                ModToolsHelper.GenerateModFiles (this, ExportToArchiveFinalize);
            }
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

            var projectPath = GetModPathProject ();
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

            metadata.isConfigEnabled = Directory.Exists (GetModPathConfigs ());

            if (workshop != null)
                workshop.parent = this;

            if (configEdits != null)
                configEdits.OnAfterDeserialization ();

            if (textEdits != null)
                textEdits.OnAfterDeserialization ();
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

            var projectPath = GetModPathProject ();
            if (string.IsNullOrEmpty (projectPath))
            {
                Debug.LogWarning ("Couldn't save metadata.yaml, project folder path is null or empty!");
                return;
            }

            if (!Directory.Exists (projectPath))
            {
                if (useDefaultWorkingPath)
                {
                    // If a user is using the default project path, the path would be inside PhantomBrigade user folder.
                    // It should be ok to auto-create one: if something goes wrong with the operation, the consequences might be limited
                    UtilitiesYAML.PrepareClearDirectory (projectPath, true, false);
                    Debug.Log ("Created mod project folder: " + projectPath);
                }
                else
                {
                    // I'm fairly uncomfortable with the idea of ever automatically creating folder outside of PB user folder.
                    // Someone unfamiliar with the tools can accidentally create folders in unintended places or worse.
                    // If a user is using a custom path, they likely used a picker UI and directory already exists (e.g. for a Git repo).
                    Debug.LogWarning ("Couldn't save metadata.yaml, project folder doesn't exist: " + projectPath);
                    return;
                }
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
            if (!useDefaultWorkingPath && !string.IsNullOrEmpty (workingPath))
            {
                return workingPath;
            }

            return defaultWorkingPath;
        }

        public string GetModPathWorkshopTemp ()
        {
            if (!useDefaultWorkingPath && !string.IsNullOrEmpty (workingPath))
            {
                return workingPath + dirWorkshopTemp;
            }

            return defaultWorkingPath + dirWorkshopTemp;
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

        public bool LoadChecksums (SDKChecksumData sdkChecksumData, bool errorOnUpgrade = true)
        {
            if (sdkChecksumData == null)
            {
                Debug.LogWarning ("Missing checksum data for SDK. Build the data manually by clicking the Create checksums for SDK config DBs button in the DataModel game object.");
                return false;
            }

            var checksumsDeserializer = new ConfigChecksums.Deserializer (new DirectoryInfo (GetModPathProject ()));
            var result = checksumsDeserializer.Load ();
            switch (result.Code)
            {
                case ConfigChecksums.Deserializer.ResultCode.Upgrade:
                    if (errorOnUpgrade)
                    {
                        Debug.LogWarning ("Mod checksums: " + result.ErrorMessage);
                        return false;
                    }
                    break;
                case ConfigChecksums.Deserializer.ResultCode.Error:
                    Debug.LogError (result.ErrorMessage);
                    return false;
            }
            dataVersion = result.DataVersion;
            originChecksum = result.OriginChecksum;
            checksumsRoot = result.Root;
            multiLinkerChecksumMap = result.MultiLinkerMap.Keys
                .Select (k =>
                {
                    sdkChecksumData.MultiLinkerChecksumMap.TryGetValue (k, out var sdk);
                    return new
                    {
                        Key = k,
                        Pair = (sdk, result.MultiLinkerMap[k]),
                    };
                })
                .ToDictionary (x => x.Key, x => x.Pair);
            linkerChecksumMap = result.LinkerMap.Keys
                .Select(k =>
                {
                    sdkChecksumData.LinkerChecksumMap.TryGetValue (k, out var sdk);
                    return new
                    {
                        Key = k,
                        Pair = (sdk, result.LinkerMap[k]),
                    };
                })
                .ToDictionary(x => x.Key, x => x.Pair);

            return true;
        }

        public void UnloadChecksums ()
        {
            linkerChecksumMap = null;
            multiLinkerChecksumMap = null;
            checksumsRoot = null;
            originChecksum = default;
        }

        public void DeleteOutputDirectories ()
        {
            var configOverridesPath = DataPathHelper.GetCombinedCleanPath (GetModPathProject (), overridesFolderName);
            if (Directory.Exists (configOverridesPath))
            {
                Directory.Delete (configOverridesPath, true);
            }
            var configEditsPath = DataPathHelper.GetCombinedCleanPath (GetModPathProject (), editsFolderName);
            if (Directory.Exists (configEditsPath))
            {
                Directory.Delete (configEditsPath, true);
            }
            var assetBundlesPath = DataPathHelper.GetCombinedCleanPath (GetModPathProject (), assetBundlesFolderName);
            if (Directory.Exists (assetBundlesPath))
            {
                Directory.Delete (assetBundlesPath, true);
            }
        }

        public void DeleteConfigOverride (ConfigChecksums.ConfigEntry entry)
        {
            switch (entry)
            {
                case ConfigChecksums.ConfigFile cfgFile:
                    DeleteConfigOverrideFile (cfgFile);
                    break;
                case ConfigChecksums.ConfigDirectory cfgDir:
                    DeleteConfigOverrideDirectory (cfgDir);
                    break;
            }
        }

        [HideInInspector]
        [YamlIgnore]
        public byte dataVersion;
        [HideInInspector]
        [YamlIgnore]
        public ConfigChecksums.Checksum originChecksum;
        [HideInInspector]
        [YamlIgnore]
        public ConfigChecksums.ConfigDirectory checksumsRoot;
        [HideInInspector]
        [YamlIgnore]
        public Dictionary<Type, (ConfigChecksums.ConfigDirectory SDK, ConfigChecksums.ConfigDirectory Mod)> multiLinkerChecksumMap;
        [HideInInspector]
        [YamlIgnore]
        public Dictionary<Type, (ConfigChecksums.ConfigFile SDK, ConfigChecksums.ConfigFile Mod)> linkerChecksumMap;

        public string defaultWorkingPath => DataPathHelper.GetCombinedCleanPath (defaultWorkingPathParent, id);

        private void InitializeWorkingPath ()
        {
            if (useDefaultWorkingPath || !string.IsNullOrEmpty (workingPath))
            {
                return;
            }

            workingPath = defaultWorkingPath;
            Debug.Log ($"Set working path to {workingPath}");
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
                if (!File.Exists (fr.path))
                {
                    continue;
                }

                var pathDest = DataPathHelper.GetCombinedCleanPath (destPath, Path.GetFileName (fr.path));
                // Do not overwrite existing files. This is to prevent accidentally overwriting metadata.yaml.
                File.Copy (fr.path, pathDest);
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

        void DeleteConfigOverrideFile (ConfigChecksums.ConfigFile cfgFile)
        {
            var projectPath = GetModPathProject ();
            var overridePath = Path.Combine (projectPath, overridesFolderName, cfgFile.RelativePath);
            if (!File.Exists (overridePath))
            {
                return;
            }
            File.Delete (overridePath);
            CleanUpOverrideHierarchy (projectPath, overridePath);
        }

        void DeleteConfigOverrideDirectory (ConfigChecksums.ConfigDirectory cfgDir)
        {
            var projectPath = GetModPathProject ();
            var overridePath = Path.Combine (projectPath, overridesFolderName, cfgDir.RelativePath);
            if (!Directory.Exists (overridePath))
            {
                return;
            }
            Directory.Delete (overridePath, true);
            CleanUpOverrideHierarchy (projectPath, overridePath);
        }

        void CleanUpOverrideHierarchy(string projectPath, string overridePath)
        {
            overridePath = Path.GetDirectoryName (overridePath);
            while (overridePath != projectPath && Directory.Exists (overridePath))
            {
                if (Directory.GetFileSystemEntries (overridePath).Length != 0)
                {
                    break;
                }
                Directory.Delete (overridePath);
                overridePath = Path.GetDirectoryName (overridePath);
            }
        }

        bool showDefaultPath => hasProjectFolder && useDefaultWorkingPath;
        bool showWorkingPath => hasProjectFolder && !useDefaultWorkingPath;
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
        public const string overridesFolderName = "ConfigOverrides";
        public const string editsFolderName = "ConfigEdits";
        public const string librariesFolderName = "Libraries";

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
