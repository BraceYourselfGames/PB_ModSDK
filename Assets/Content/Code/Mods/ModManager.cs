using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Area;
using HarmonyLib;
using Content.Code.Utility;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Mods
{
    public class ModManager : MonoBehaviour
    {
        [ShowInInspector]
        public static ModConfig config;

        [ShowInInspector]
        public static int loadedModCountMax = 0;

        [ShowInInspector]
        private static int modFoldersLocal = 0;

        [ShowInInspector]
        private static int modFoldersWorkshop = 0;

        [ShowInInspector]
        public static List<ModMetadataPreloaded> metadataPreloadListFull = new List<ModMetadataPreloaded> ();

        [ShowInInspector]
        public static List<ModMetadataPreloaded> metadataPreloadList = new List<ModMetadataPreloaded> ();

        [ShowInInspector]
        public static List<string> idsOccupied = new List<string> ();

        [ShowInInspector]
        public static List<ModLoadedData> loadedMods = new List<ModLoadedData> ();

        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout, KeyLabel = "ID", ValueLabel = "Loaded Data")]
        public static Dictionary<string, ModLoadedData> loadedModsLookup = new Dictionary<string, ModLoadedData> ();

        private static Type typeModLink = typeof (ModLink);
        private static FieldInfo fieldInfoModLinkInstance = typeof (ModLink).GetField ("instance", BindingFlags.Public | BindingFlags.Static);

        private const char editSeparator = ':';

        public const string modSourceLocal = "Local";
        public const string modSourceWorkshop = "Workshop";



        public static bool AreModsAvailable ()
        {
            bool modsAvailable = metadataPreloadList != null && metadataPreloadList.Count > 0;
            return modsAvailable;
        }

        public static bool AreModsActive ()
        {
            bool modsEnabled = loadedMods != null && loadedMods.Count > 0;
            return modsEnabled;
        }

        public static bool AreModsActive (out int count)
        {
            count = 0;
            bool modsEnabled = loadedModCountMax > 0 || (loadedMods != null && loadedMods.Count > 0);
            if (modsEnabled)
                count = loadedMods.Count;

            return modsEnabled;
        }



        [Button (ButtonSizes.Large), PropertyOrder (-1)]
        public static void LoadEverything (bool inGameStartup)
        {
            ModUtilities.Initialize ();
            PreloadModList ();
            LoadMods (inGameStartup);
        }

        [Button (ButtonSizes.Medium), ButtonGroup ("A"), PropertyOrder (-1)]
        public static void SaveConfig ()
        {
            if (config == null)
                config = new ModConfig ();

            var pathSettings = DataPathHelper.GetSettingsFolder ();
            UtilitiesYAML.SaveDataToFile<ModConfig> (pathSettings, "mods.yaml", config, false);
        }

        [Button (ButtonSizes.Medium), ButtonGroup ("A"), PropertyOrder (-1)]
        public static void PreloadModList ()
        {
            config = null;
            metadataPreloadListFull.Clear ();
            metadataPreloadList.Clear ();
            idsOccupied.Clear ();
            queuedDialogs.Clear ();

            PreloadModListCollect ();
            PreloadModListSort ();
            PreloadModListFinalize ();
        }

        private static void PreloadModListCollect ()
        {
            var pathSettings = DataPathHelper.GetSettingsFolder ();
            bool configNotFound = false;

            try
            {
                config = UtilitiesYAML.LoadDataFromFile<ModConfig> (pathSettings, "mods.yaml", false, false);
            }
            catch (Exception e)
            {
                PostWarning ($"Encountered an exception while reading mod config from {pathSettings}:\n{e}");
                return;
            }

            if (config == null)
            {
                config = new ModConfig ();
                Debug.LogWarning ($"Mod manager | Mod list config not found, creating one...");
                SaveConfig ();
            }

            var pathModsUser = DataPathHelper.GetModsFolder (ModFolderType.User);

            try
            {
                if (!Directory.Exists (pathModsUser))
                {
                    Debug.Log ($"Mod manager | User mod folder ({pathModsUser}) doesn't exist, creating one");
                    Directory.CreateDirectory (pathModsUser);
                }
                else
                {
                    var directoryList = UtilitiesYAML.GetDirectoryList (pathModsUser, false);
                    modFoldersLocal = directoryList.Count;

                    foreach (var directoryName in directoryList)
                    {
                        var pathMod = $"{pathModsUser}{directoryName}/";
                        var metadata = UtilitiesYAML.LoadDataFromFile<ModMetadata> (pathMod, "metadata.yaml", true, false);

                        if (metadata != null && !string.IsNullOrEmpty (metadata.id))
                        {
                            // Fill utility fields on metadata
                            metadata.OnAfterDeserialization (directoryName, pathMod);
                            metadataPreloadListFull.Add (new ModMetadataPreloaded
                            {
                                metadata = metadata,
                                directoryName = directoryName,
                                pathFull = pathMod,
                                source = modSourceLocal
                            });
                        }
                        else
                            PostWarning ($"Could not find metadata.yaml (or found no ID within) in the user mod folder {pathMod}");
                    }
                }

                var pathModsWorkshop = DataPathHelper.GetModsFolder (ModFolderType.Workshop);
                if (string.IsNullOrEmpty (pathModsWorkshop) || !Directory.Exists (pathModsWorkshop))
                {
                    Debug.Log ($"Mod manager | Workshop mod folder ({pathModsWorkshop}) doesn't exist");
                    // Do not create a dir., it's up to Steam to manage it
                }
                else
                {
                    var directoryList = UtilitiesYAML.GetDirectoryList (pathModsWorkshop, false);
                    modFoldersWorkshop = directoryList.Count;

                    foreach (var directoryName in directoryList)
                    {
                        var pathMod = $"{pathModsWorkshop}{directoryName}/";
                        var metadata = UtilitiesYAML.LoadDataFromFile<ModMetadata> (pathMod, "metadata.yaml", true, false);

                        if (metadata != null && !string.IsNullOrEmpty (metadata.id))
                        {
                            // Fill utility fields on metadata
                            metadata.OnAfterDeserialization (directoryName, pathMod);
                            metadataPreloadListFull.Add (new ModMetadataPreloaded
                            {
                                metadata = metadata,
                                directoryName = directoryName,
                                pathFull = pathMod,
                                source = modSourceWorkshop
                            });
                        }
                        else
                            PostWarning ($"Could not find metadata.yaml (or found no ID within) in the Workshop mod folder {pathMod}");
                    }
                }
            }
            catch (Exception e)
            {
                PostWarning ($"Encountered an exception while reading mod metadata:\n{e}");
                return;
            }
        }

        private static void PreloadModListSort ()
        {
            if (metadataPreloadListFull != null)
                metadataPreloadListFull.Sort (ComparePreloadedMods);
        }

        private static void PreloadModListFinalize ()
        {
            int configOverridesCount = config != null && config.overrides != null ? config.overrides.Count : 0;
            bool configOverridesChecked = config?.overrides != null && configOverridesCount > 0;

            for (int i = 0, iLimit = metadataPreloadListFull.Count; i < iLimit; ++i)
            {
                var metadataPreload = metadataPreloadListFull[i];
                var id = metadataPreload.metadata.id;
                if (idsOccupied.Contains (id))
                {
                    PostWarning ($"Skipping mod folder {metadataPreload.pathFull} containing mod with ID {id}: another mod with the same ID was already registered first.");
                    continue;
                }

                if (configOverridesChecked)
                {
                    bool disabled = false;
                    for (int o = 0; o < configOverridesCount; ++o)
                    {
                        var ovr = config.overrides[o];
                        if (ovr == null)
                            continue;

                        bool match = string.Equals (ovr.id, id, StringComparison.OrdinalIgnoreCase);
                        if (match && !ovr.enabled)
                        {
                            Debug.LogWarning ($"Mod manager | Skipping preload for mod folder {i} with ID {id} already present in the list: {metadataPreload.pathFull}");
                            disabled = true;
                            break;
                        }
                    }

                    if (disabled)
                        continue;
                }

                idsOccupied.Add (id);
                metadataPreloadList.Add (metadataPreload);
            }

            var sb = new StringBuilder ();
            for (int i = 0; i < metadataPreloadList.Count; ++i)
            {
                var metadataPreload = metadataPreloadList[i];
                sb.Append ("\n- ");
                sb.Append (i);
                sb.Append (" (");
                sb.Append (metadataPreload.source);
                sb.Append ("): ");
                sb.Append (metadataPreload.directoryName);
            }

            string report = sb.ToString ();
            Debug.Log ($"Mod manager | User mod folders: {modFoldersLocal} | Workshop mod folders: {modFoldersWorkshop} | Total metadata entries: {metadataPreloadListFull.Count} | Loaded mods: {metadataPreloadList.Count}\n{report}");
        }

        private static int ComparePreloadedMods (ModMetadataPreloaded node1, ModMetadataPreloaded node2)
        {
            // Mods with higher priority value go first. For example, between mod A with priority -100 and mod B with priority 50, mod B would load first
            int priorityComparison = node2.metadata.priority.CompareTo (node1.metadata.priority);
            if (priorityComparison != 0)
                return priorityComparison;

            // Mods with alphanumerically higher ID go first
            int idComparison = string.Compare (node1.metadata.id, node2.metadata.id, StringComparison.OrdinalIgnoreCase);
            if (idComparison != 0)
                return idComparison;

            // User mods go first, workshop mods last
            int sourceComparison = node1.source.CompareTo (node2.source);
            if (sourceComparison != 0)
                return sourceComparison;

            // Mods with alphanumerically higher dir. name go first
            int dirComparison = string.Compare (node1.directoryName, node2.directoryName, StringComparison.OrdinalIgnoreCase);
            return dirComparison;
        }

        [Button (ButtonSizes.Medium), ButtonGroup ("B"), PropertyOrder (-1)]
        public static void SaveMetadata ()
        {
            /*
            if (metadataLookup == null || metadataLookup.Count == 0)
                return;

            var pathMods = DataPathHelper.GetModsFolder (config.loadFromApplicationPath);
            foreach (var kvp in metadataLookup)
            {
                var directoryName = kvp.Key;
                var metadata = kvp.Value;
                if (metadata == null)
                    continue;

                var pathMetadata = $"{pathMods}{directoryName}/metadata.yaml";
                UtilitiesYAML.SaveToFile (pathMetadata, metadata);
            }
            */
        }

        public static void LoadModsOutsideStartup () => LoadMods (false);

        [Button (ButtonSizes.Medium), ButtonGroup ("C"), PropertyOrder (-1)]
        public static void LoadMods (bool inGameStartup)
        {
            try
            {
                loadedMods.Clear ();
                loadedModsLookup.Clear ();

                if (Application.isPlaying)
                    DataManagerText.ClearLocalizationsFromMods ();

                if (metadataPreloadList == null || metadataPreloadList.Count == 0)
                {
                    Debug.LogWarning ($"Mod manager | Skipping mod loading as preloaded mod list is null or empty");
                    LoadModsFinish ();
                    return;
                }

                // Record peak mod count for purposes of permanently flagging modded sessions
                loadedModCountMax = Mathf.Max (loadedModCountMax, metadataPreloadList.Count);

                var info = BuildInfoHelper.GetBuildInfo ();
                bool versionChecked = false;
                Version versionCurrent = null;

                if (string.IsNullOrEmpty (info))
                    Debug.LogWarning ($"Mod manager | Failed to find build info");
                else
                {
                    var s = info.Split ('-');
                    if (BuildInfoHelper.indexNumber.IsValidIndex (s))
                    {
                        var versionText = s[BuildInfoHelper.indexNumber];
                        versionChecked = TryParseVersionString (versionText, out int versionMajor, out int versionMinor, out int versionPatch);
                        if (versionChecked)
                            versionCurrent = new Version (versionMajor, versionMinor, versionPatch);
                    }
                }

                for (int i = 0, count = metadataPreloadList.Count; i < count; ++i)
                {
                    var metadataPreload = metadataPreloadList[i];
                    LoadMod (metadataPreload, i, versionChecked, versionCurrent, inGameStartup);
                }

                #if !PB_MODSDK

                if (Application.isPlaying)
                {
                    // Refresh UI to notify user about loaded mods
                    if (CIViewInternalBuildInfo.ins != null)
                        CIViewInternalBuildInfo.ins.Refresh ();

                    DataManagerText.RefreshLocalizationOptionLevels ();

                    // if (loadedMods.Count > 0)
                    //     AchievementHelper.UnlockAchievement (Achievement.ModInstalled);
                }

                #endif
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat ("Mod manager | Encountered an exception while loading mods\n{0}", e);
            }

            LoadModsFinish ();
        }

        private static void LoadModsFinish ()
        {
            // We want to reload both of these at this point no matter whether mods were loaded or not
            TextureManager.Load ();
            ItemHelper.LoadVisuals ();

            #if !PB_MODSDK

            CIViewPauseFooter.ins.RefreshModDisplay ();
            Co.DelayFrames (5, CheckQueuedDialogs);

            #endif
        }

        public static void LoadMod (ModMetadataPreloaded metadataPreload, int i, bool versionChecked, Version versionCurrent, bool inGameStartup)
        {
            string pathMod = string.Empty;
            try
            {
                if (metadataPreload == null || string.IsNullOrEmpty (metadataPreload.pathFull))
                {
                    PostWarning ($"Skipping mod {i}: null or empty entry or directory path");
                    return;
                }

                if (metadataPreload.metadata == null)
                    return;

                var metadata = metadataPreload.metadata;
                string id = metadata.id;

                pathMod = metadataPreload.pathFull;
                if (!Directory.Exists (pathMod))
                {
                    PostWarning ($"Skipping mod {i} ({id}), directory doesn't exist: {pathMod}");
                    return;
                }

                if (loadedModsLookup.ContainsKey (id))
                {
                    PostWarning ($"Skipping mod {i} ({id}): another mod with the same ID was already loaded first");
                    return;
                }

                // Skipping mods that declare version requirements not satisfied by this version of the game
                if (versionChecked)
                {
                    if (metadata.gameVersionMinPacked != null)
                    {
                        bool versionCurrentLower = versionCurrent < metadata.gameVersionMinPacked;
                        if (versionCurrentLower)
                        {
                            PostWarning ($"Skipping mod {i} ({id}): game version {versionCurrent} is lower than {metadata.gameVersionMinPacked}");
                            return;
                        }
                    }

                    if (metadata.gameVersionMaxPacked != null)
                    {
                        bool versionCurrentHigher = versionCurrent > metadata.gameVersionMaxPacked;
                        if (versionCurrentHigher)
                        {
                            PostWarning ($"Skipping mod {i} ({id}): game version {versionCurrent} is higher than {metadata.gameVersionMaxPacked}");
                            return;
                        }
                    }
                }

                var loadedData = new ModLoadedData ();
                loadedData.metadata = metadata;
                loadedModsLookup.Add (metadata.id, loadedData);

                Debug.Log ($"Mod manager | Loading mod {i} ({id}) | Name: {metadata.name} | Version: {metadata.ver} | Description: {metadata.desc}");

                // Libraries are always loaded first in case they include new types configs depend on
                if (metadata.includesLibraries)
                {
                    if (inGameStartup)
                        TryLoadingLibraries (metadata.id, pathMod + "Libraries/", loadedData);
                    else
                        Debug.LogWarning ($"Mod manager | Skipping library loading for {metadata.id} as this load is not occurring during game startup");
                }

                if (metadata.includesConfigOverrides)
                    TryLoadingConfigOverrides (metadata.id, pathMod + "ConfigOverrides/", loadedData);

                if (metadata.includesConfigEdits)
                    TryLoadingConfigEdits (metadata.id, pathMod + "ConfigEdits/", loadedData);

                if (metadata.includesConfigTrees)
                    TryLoadingConfigTrees (metadata.id, pathMod + "ConfigTrees/", loadedData);

                if (metadata.includesTextures)
                    TryLoadingTextures (metadata.id, pathMod + "Textures/", loadedData);

                if (metadata.includesLocalizationEdits)
                    TryLoadingLocalizationEdits (metadata.id, pathMod + "LocalizationEdits/", loadedData);

                if (metadata.includesLocalizations)
                    TryLoadingLocalizations (metadata.id, pathMod + "Localizations/", loadedData);

                if (metadata.includesAssetBundles)
                    TryLoadingAssetBundles (metadata.id, pathMod + "AssetBundles/", loadedData);

                loadedMods.Add (loadedData);
            }
            catch (Exception e)
            {
                Debug.LogWarning ($"Mod manager | Encountered exception loading mod {i} from {pathMod}: {e}");
            }
        }

        public static void TryLoadingConfigOverrides (string id, string pathConfigOverrides, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathConfigOverrides))
            {
                PostWarning ($"Mod {id} | Failed to find {pathConfigOverrides} subfolder expected based on metadata");
                return;
            }

            var list = Directory.EnumerateFiles (pathConfigOverrides, "*.yaml", SearchOption.AllDirectories).ToList ();
            loadedData.configOverrides = new List<ModConfigOverride> (list.Count);

            foreach (var path in list)
            {
                var pathFull = path.Replace ("\\", "/");
                var filename = Path.GetFileName (pathFull);
                var key = Path.GetFileNameWithoutExtension (pathFull);

                var pathTrimmed = pathFull.Replace (pathConfigOverrides, string.Empty).Replace (".yaml", string.Empty);
                var typeName = DataPathUtility.GetDataTypeFromPath (pathTrimmed);
                if (typeName == null)
                {
                    pathTrimmed = pathTrimmed.Replace (key, string.Empty);
                    typeName = DataPathUtility.GetDataTypeFromPath (pathTrimmed);
                }

                var type = FieldReflectionUtility.GetTypeByName (typeName);
                if (type == null)
                {
                    PostWarning ($"Mod {id} | Encountered config override of unknown target type {typeName} at path {pathFull}");
                    continue;
                }

                var configOverride = new ModConfigOverride
                {
                    pathFull = pathFull,
                    pathTrimmed = pathTrimmed,
                    filename = filename,
                    key = key,
                    typeName = typeName,
                    type = type
                };

                loadedData.configOverrides.Add (configOverride);

                var containerObject = UtilitiesYAML.ReadFromFile (type, pathFull, false);
                if (containerObject == null)
                    continue;

                configOverride.containerObject = containerObject;
            }
        }

        public static void TryLoadingConfigEdits (string id, string pathConfigEdits, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathConfigEdits))
            {
                PostWarning ($"Mod {id} | Failed to find {pathConfigEdits} subfolder expected based on metadata");
                return;
            }

            var list = Directory.EnumerateFiles (pathConfigEdits, "*.yaml", SearchOption.AllDirectories).ToList ();
            loadedData.configEdits = new List<ModConfigEditLoaded> (list.Count);

            foreach (var path in list)
            {
                var pathFull = path.Replace ("\\", "/");
                var filename = Path.GetFileName (pathFull);
                var key = Path.GetFileNameWithoutExtension (pathFull);

                var pathTrimmed = pathFull.Replace (pathConfigEdits, string.Empty).Replace (".yaml", string.Empty);
                var typeName = DataPathUtility.GetDataTypeFromPath (pathTrimmed);
                if (typeName == null)
                {
                    pathTrimmed = pathTrimmed.Replace (key, string.Empty);
                    typeName = DataPathUtility.GetDataTypeFromPath (pathTrimmed);
                }

                var type = FieldReflectionUtility.GetTypeByName (typeName);
                if (type == null)
                {
                    PostWarning ($"Mod {id} | Located config edit of unknown target type {typeName} at path {pathFull}");
                    continue;
                }

                var configEdit = new ModConfigEditLoaded
                {
                    pathFull = pathFull,
                    pathTrimmed = pathTrimmed,
                    filename = filename,
                    key = key,
                    typeName = typeName,
                    type = type
                };

                loadedData.configEdits.Add (configEdit);

                var dataSerialized = UtilitiesYAML.ReadFromFile<ModConfigEditSerialized> (pathFull, false);
                if (dataSerialized == null)
                    continue;

                configEdit.data = new ModConfigEdit ();
                configEdit.data.removed = dataSerialized.removed;

                if (dataSerialized.edits != null)
                {
                    configEdit.data.edits = new List<ModConfigEditStep> (dataSerialized.edits.Count);
                    for (int i = 0; i < dataSerialized.edits.Count; ++i)
                    {
                        var editString = dataSerialized.edits[i];
                        var split = editString.Split (editSeparator);
                        if (split.Length != 2)
                        {
                            PostWarning ($"Mod {id} | Config edit line {i} has invalid number of elements:\n{editString}\n\nFile:\n{pathFull}");
                            continue;
                        }

                        var editPath = split[0];
                        var editValue = split[1];
                        if (editValue.StartsWith (" "))
                            editValue = editValue.Substring (1, editValue.Length - 1);

                        if (string.IsNullOrEmpty (editPath))
                        {
                            PostWarning ($"Mod {id} | Config edit line {i} has empty edit path:\n{editString}\n\nFile:\n{pathFull}");
                            continue;
                        }

                        configEdit.data.edits.Add (new ModConfigEditStep { path = editPath, value = editValue });
                    }
                }
            }
        }



        public static void TryLoadingConfigTrees (string id, string pathConfigTrees, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathConfigTrees))
            {
                PostWarning ($"Mod {id} | Failed to find {pathConfigTrees} subfolder expected based on metadata");
                return;
            }

            var list = Directory.EnumerateFiles (pathConfigTrees, "*.yaml", SearchOption.AllDirectories).ToList ();
            loadedData.configEdits = new List<ModConfigEditLoaded> (list.Count);

            foreach (var path in list)
            {
                var pathFull = path.Replace ("\\", "/");
                var filename = Path.GetFileName (pathFull);
                var key = Path.GetFileNameWithoutExtension (pathFull);

                var pathTrimmed = pathFull.Replace (pathConfigTrees, string.Empty).Replace (".yaml", string.Empty);
                var typeName = DataPathUtility.GetDataTypeFromPath (pathTrimmed);
                if (typeName == null)
                {
                    pathTrimmed = pathTrimmed.Replace (key, string.Empty);
                    typeName = DataPathUtility.GetDataTypeFromPath (pathTrimmed);
                }

                var type = FieldReflectionUtility.GetTypeByName (typeName);
                if (type == null)
                {
                    PostWarning ($"Mod {id} | Located config tree of unknown target type {typeName} at path {pathFull}");
                    continue;
                }

                var configTree = new ModConfigTreeLoaded
                {
                    pathFull = pathFull,
                    pathTrimmed = pathTrimmed,
                    filename = filename,
                    key = key,
                    typeName = typeName,
                    type = type
                };

                loadedData.configTrees.Add (configTree);

                var data = UtilitiesYAML.ReadFromFile<ModConfigTree> (pathFull, false);
                configTree.data = data;
            }
        }

        public static void TryLoadingAssetBundles (string id, string pathAssetBundles, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathAssetBundles))
            {
                PostWarning ($"Mod {id} | Failed to find {pathAssetBundles} subfolder expected based on metadata");
                return;
            }

            // Bail if folder is empty
            var files = Directory.GetFiles (pathAssetBundles);
            if (files.Length == 0)
            {
                PostWarning ($"Mod {id} | Failed to load asset bundles from {pathAssetBundles} subfolder: no files found");
                return;
            }

            Debug.Log ($"Mod {id} | Discovered {files.Length} in the asset bundle folder {pathAssetBundles}");
            var manifestExtension = ".manifest";

            for (int i = 0; i < files.Length; ++i)
            {
                var file = files[i];
                if (!file.EndsWith (manifestExtension))
                    continue;

                AssetBundle assetBundle = AssetBundle.LoadFromFile (file.Replace (manifestExtension, string.Empty));
                if (assetBundle == null)
                {
                    PostWarning ($"Mod {id} | Failed to load an AssetBundle from file {file}");
                    continue;
                }

                if (loadedData.assetBundles == null)
                    loadedData.assetBundles = new List<AssetBundle> ();

                loadedData.assetBundles.Add (assetBundle);
                Debug.Log ($"Mod {id} | Loaded asset bundle {assetBundle.name} from manifest file: {file} | Assets:\n{assetBundle.GetAllAssetNames ().ToStringFormatted (true, multilinePrefix: "- ")}");
            }
        }

        public static UnityEngine.Object GetAsset (string modID, string assetBundleName, string assetName, bool log = true)
        {
            var mods = ModManager.loadedMods;
            if (mods == null || string.IsNullOrEmpty (modID))
            {
                if (log)
                    DebugHelper.LogWarning ($"GetAsset failed: no loaded mods or invalid mod ID string");
                return null;
            }

            ModLoadedData loadedData = null;
            foreach (var loadedDataCandidate in mods)
            {
                if (loadedDataCandidate == null || loadedDataCandidate.metadata == null || !string.Equals (loadedDataCandidate.metadata.id, modID))
                    continue;

                loadedData = loadedDataCandidate;
                break;
            }

            if (loadedData == null)
            {
                if (log)
                    DebugHelper.LogWarning ($"GetAsset failed: no loaded mods with ID {modID}");
                return null;
            }

            return loadedData.GetAsset (assetBundleName, assetName);
        }

        public static void TryLoadingLevels (string id, string pathLevels, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathLevels))
            {
                PostWarning ($"Mod {id} | Failed to find {pathLevels} subfolder expected based on metadata");
                return;
            }

            loadedData.levels = new Dictionary<string, ModLevelLoaded> ();

            var directoryList = UtilitiesYAML.GetDirectoryList (pathLevels);

            for (int i = 0, iLimit = directoryList.Count; i < iLimit; ++i)
            {
                var directory = directoryList[i];
                var pathCombined = $"{pathLevels}/{directory}/";

                var dataCore = UtilitiesYAML.LoadDataFromFile<AreaDataCore> (pathCombined, "core.yaml", false, false);
                if (dataCore == null)
                {
                    PostWarning ($"Mod {id} | Loading level directory {i + 1}/{iLimit}: {directory} | Failed to find core.yaml");
                    continue;
                }

                Debug.Log ($"Mod {id} | Loading level directory {i + 1}/{iLimit}: {directory} | Loaded core data | Bounds: {dataCore.bounds} | Full path:\n{pathCombined}");
                loadedData.levels.Add (directory, new ModLevelLoaded
                {
                    pathCombined = pathCombined
                });
            }
        }

        public static void TryLoadingTextures (string id, string pathTextures, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathTextures))
            {
                PostWarning ($"Mod {id} | Failed to find {pathTextures} subfolder expected based on metadata");
                return;
            }

            loadedData.textureFolders = new List<ModTextureFolder> ();
            TryLoadingTexturesRecursive (id, pathTextures, pathTextures, loadedData);
        }

        private static void TryLoadingTexturesRecursive (string id, string pathDirectoryRoot, string pathDirectoryParent, ModLoadedData loadedData)
        {
            var directories = Directory.GetDirectories (pathDirectoryParent);

            // Not the bottom-most folder, continue
            if (directories.Length > 0)
            {
                foreach (var pathDirectoryChild in directories)
                    TryLoadingTexturesRecursive (id, pathDirectoryRoot, pathDirectoryChild, loadedData);
            }
            else
            {
                // Bail if folder is empty
                var files = Directory.GetFiles (pathDirectoryParent);
                if (files.Length == 0)
                    return;

                // Extract suffix
                var pathFull = pathDirectoryParent.Replace ("\\", "/");
                var pathTrimmed = pathDirectoryParent.Replace (pathDirectoryRoot, string.Empty).Replace ("\\", "/");
                Debug.Log ($"Mod {id} | Discovered texture folder with {files.Length} files: {pathTrimmed}");

                loadedData.textureFolders.Add (new ModTextureFolder
                {
                    pathFull = pathFull,
                    pathTrimmed = pathTrimmed
                });
            }
        }

        public static void TryLoadingLocalizations (string id, string pathLocalizations, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathLocalizations))
            {
                PostWarning ($"Mod {id} | Failed to find {pathLocalizations} subfolder expected based on metadata");
                return;
            }

            var directories = Directory.GetDirectories (pathLocalizations);
            if (directories.Length == 0)
            {
                PostWarning ($"Mod {id} | Failed to find any localizations in Localizations subfolder");
                return;
            }

            foreach (var path in directories)
            {
                // Bail if folder is empty
                var files = Directory.GetFiles (path);
                if (files.Length == 0)
                {
                    PostWarning ($"Mod {id} | Failed to find any files in localization folder {path}");
                    continue;
                }

                var key = path.Replace (pathLocalizations, string.Empty).Replace ("\\", "/");
                Debug.LogWarning ($"Mod {id} | Registered localization at {path} with key {key}");
                DataManagerText.AddLocalizationFromMod (key, path);
            }
        }

        public static void TryLoadingLocalizationEdits (string id, string pathLocalizationEdits, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (pathLocalizationEdits))
            {
                PostWarning ($"Mod {id} | Failed to find {pathLocalizationEdits} subfolder expected based on metadata");
                return;
            }

            loadedData.localizationEdits = new Dictionary<string, List<ModLocalizationEditLoaded>> ();
            var listDirectories = Directory.EnumerateDirectories(pathLocalizationEdits);

            foreach (var pathPerLanguage in listDirectories)
            {
                var languageName = Path.GetFileName (pathPerLanguage);
                if (string.IsNullOrEmpty (languageName))
                    continue;

                var list = Directory.EnumerateFiles (pathLocalizationEdits, "*.yaml", SearchOption.AllDirectories).ToList ();
                var languageEdits = new List<ModLocalizationEditLoaded> (list.Count);
                loadedData.localizationEdits.Add (languageName, languageEdits);

                foreach (var path in list)
                {
                    var pathFull = path.Replace ("\\", "/");
                    var filename = Path.GetFileName (pathFull);
                    var key = Path.GetFileNameWithoutExtension (pathFull);

                    var pathTrimmed = pathFull.Replace (pathLocalizationEdits, string.Empty).Replace (".yaml", string.Empty);
                    if (!pathTrimmed.StartsWith (languageName))
                        continue;

                    var localizationEdit = new ModLocalizationEditLoaded
                    {
                        pathFull = pathFull,
                        pathTrimmed = pathTrimmed,
                        filename = filename,
                        key = key
                    };

                    languageEdits.Add (localizationEdit);

                    var dataSerialized = UtilitiesYAML.ReadFromFile<ModLocalizationEdit> (pathFull, false);
                    if (dataSerialized == null)
                        continue;

                    localizationEdit.data = dataSerialized;
                    Debug.LogWarning ($"Mod {id} | Registered localization edit for language {languageName} sector {key}");
                }
            }
        }

        public static void TryLoadingLibraries (string id, string path, ModLoadedData loadedData)
        {
            if (loadedData == null)
                return;

            if (!Directory.Exists (path))
            {
                PostWarning ($"Mod {id} | Failed to find {path} subfolder expected based on metadata");
                return;
            }

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo (path);
                if (!directoryInfo.Exists)
                {
                    PostWarning ($"Mod {id} | Failed to find directory at path {path}");
                    return;
                }

                List<Assembly> assemblyList = new List<Assembly> ();
                foreach (FileInfo file in directoryInfo.GetFiles ())
                {
                    if (file.Name.ToLower ().EndsWith (".dll"))
                    {
                        Debug.Log (string.Format ("Loading mod .dll: {0}", file.Name));
                        Assembly assembly = Assembly.LoadFrom (file.FullName);
                        if (assembly != null)
                            assemblyList.Add (assembly);
                    }
                }

                if (assemblyList.Count == 0)
                {
                    PostWarning ($"Mod {id} | Failed to load any code assemblies from path {path}");
                    return;
                }

                Type linkTypeFound = null;
                loadedData.assemblies = new List<Assembly> ();
                loadedData.assemblyNames = new List<string> ();

                foreach (Assembly assembly in assemblyList)
                {
                    var types = assembly.GetTypes ();
                    foreach (Type type in types)
                    {
                        if (!typeModLink.IsAssignableFrom (type))
                            continue;

                        if (linkTypeFound != null)
                        {
                            PostWarning ($"Mod {id} | Located more than one class inheriting from ModLink in the loaded code assembly, skipping type {linkTypeFound.Name}");
                            continue;
                        }

                        linkTypeFound = type;
                    }

                    loadedData.assemblies.Add (assembly);
                    loadedData.assemblyNames.Add (assembly.FullName);
                }

                object linkObject = linkTypeFound != null ? Activator.CreateInstance (linkTypeFound) : null;
                var linkTyped = linkObject != null ? linkObject as ModLink : null;

                if (linkTyped == null)
                {
                    PostWarning ($"Mod {id} | Failed to locate a class inheriting from ModLink type despite the mod metadata declaring it includes compliant libraries");
                    return;
                }

                Debug.Log ($"Mod {id} | Located ModLink of type {linkTypeFound.Name}, assigned metadata and singleton reference to it");

                // Set singleton value - this can't be done through this object reference so we have to use reflection
                fieldInfoModLinkInstance.SetValue (null, linkObject);

                // Fill metadata - this has to be done early, as OnLoad method might use it
                linkTyped.metadata = loadedData.metadata;

                // Creating patcher - this has to be done here and not inside OnLoad to allow for detecting changes
                var harmonyInstance = new Harmony (id);

                // Do operations shared between all library mods that need to run prior to OnLoad
                OnLibraryModPreload (loadedData);

                // Running this method once instance, metadata and patcher are in place
                // Note that mods can override its implementation, switching from PatchAll to more fine grained operations
                linkTyped.OnLoad (harmonyInstance);

                // Detecting changes for debugging
                var patchedMethods = harmonyInstance.GetPatchedMethods ();
                // var patchedMethodsPicked = patchedMethods.Where (method => harmonyInstance.GetPatchInfo (method).Owners.Contains (id)).ToList ();

                loadedData.patchedMethods = patchedMethods.ToList ();
                loadedData.patchedMethodNames = new List<string> ( loadedData.patchedMethods.Count);

                foreach (var methodBase in loadedData.patchedMethods)
                {
                    var fullName = $"{methodBase.DeclaringType?.Name}.{methodBase.Name}";
                    loadedData.patchedMethodNames.Add (fullName);
                }

                // Register tag mappings etc.
                OnLibraryModLoaded (loadedData);
            }
            catch (Exception ex)
            {
                Debug.LogError ($"Mod {id} | Exception while loading mod at path {path}");
                Debug.LogException (ex);
            }
        }

        public static void OnLibraryModPreload (ModLoadedData loadedData)
        {
            // Stub in case we need future functionality for all mods that fires before OnLoad
        }

        public static void OnLibraryModLoaded (ModLoadedData loadedData)
        {
            if (loadedData == null || loadedData.assemblies == null || loadedData.assemblies.Count == 0)
                return;

            // Fetch the tag mapping collection to detect if we need to rebuild the deserializer
            var tagMappings = UtilitiesYAML.GetTagMappings ();
            var tagCountCurrent = tagMappings.Count;

            // For every loaded assembly, add hinted types to the mappings (only the main assembly types would be in otherwise)
            foreach (var assembly in loadedData.assemblies)
                UtilitiesYAML.AddTagMappingsHintedInAssembly (assembly);

            if (tagCountCurrent == tagMappings.Count)
            {
                // No change so skip re-initializing the YAML deserializer.
                return;
            }

            // Rebuild the deserializer so that if there are config files as well in the mod they can use
            // the new tag mappings. We have to null out the existing deserializer to get past a guard in
            // SetupYAMLReader().

            UtilitiesYAML.RebuildDeserializer ();
        }

        public static void ProcessLibraryEdits (SortedDictionary<string, DataContainerTextSectorMain> sectors)
        {
            if (loadedMods == null || sectors == null || sectors.Count == 0)
                return;

            var languageName = "English";
            for (int i = 0; i < loadedMods.Count; ++i)
            {
                var loadedData = loadedMods[i];
                if (loadedData == null || loadedData.metadata == null || !loadedData.metadata.includesLocalizationEdits || loadedData.localizationEdits == null)
                    continue;

                var modID = loadedData.metadata.id;
                if (!loadedData.localizationEdits.ContainsKey (languageName))
                {
                    Debug.LogWarning ($"Mod {i} ({modID}) | Applying text library edits | {languageName} is not covered by the mod");
                    continue;
                }

                var edits = loadedData.localizationEdits[languageName];
                if (edits == null || edits.Count == 0)
                    continue;

                foreach (var edit in edits)
                {
                    if (edit == null || edit.data == null || edit.data.edits == null || edit.data.edits.Count == 0)
                        continue;

                    var sectorKey = edit.key;
                    if (string.IsNullOrEmpty (sectorKey) || !sectors.ContainsKey (sectorKey))
                    {
                        Debug.LogWarning ($"Mod {i} ({modID}) | Applying text library edits | Target sector {sectorKey} not found");
                        continue;
                    }

                    var sectorData = sectors[sectorKey];
                    var entries = sectorData.entries;

                    foreach (var kvp in edit.data.edits)
                    {
                        var key = kvp.Key;
                        var text = kvp.Value;

                        if (entries.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) | Applying text library edits | Replacing text {sectorKey}/{key}:\n-{text}");
                            entries[key].text = text;
                        }
                        else
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) | Applying text library edits | Adding text {sectorKey}/{key}:\n-{text}");
                            entries.Add (key, new DataBlockTextEntryMain { text = text });
                        }
                    }
                }
            }
        }

        public static void ProcessLocalizationEdits (string languageName, SortedDictionary<string, DataContainerTextSectorLocalization> sectors)
        {
            if (loadedMods == null || sectors == null || sectors.Count == 0)
                return;

            for (int i = 0; i < loadedMods.Count; ++i)
            {
                var loadedData = loadedMods[i];
                if (loadedData == null || loadedData.metadata == null || !loadedData.metadata.includesLocalizationEdits || loadedData.localizationEdits == null)
                    continue;

                var modID = loadedData.metadata.id;
                if (!loadedData.localizationEdits.ContainsKey (languageName))
                {
                    Debug.LogWarning ($"Mod {i} ({modID}) | Failed to apply localization edits: current language {languageName} is not covered by the mod");
                    continue;
                }

                var edits = loadedData.localizationEdits[languageName];
                if (edits == null || edits.Count == 0)
                    continue;

                foreach (var edit in edits)
                {
                    if (edit == null || edit.data == null || edit.data.edits == null || edit.data.edits.Count == 0)
                        continue;

                    var sectorKey = edit.key;
                    if (string.IsNullOrEmpty (sectorKey) || !sectors.ContainsKey (sectorKey))
                    {
                        Debug.LogWarning ($"Mod {i} ({modID}) | Failed to apply localization edit to language {languageName} sector {sectorKey}: no such sector found");
                        continue;
                    }

                    var sectorData = sectors[sectorKey];
                    var entries = sectorData.entries;

                    foreach (var kvp in edit.data.edits)
                    {
                        var key = kvp.Key;
                        var text = kvp.Value;

                        if (entries.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) | Replacing {languageName} text in sector {sectorKey} key {key}");
                            entries[key].text = text;
                        }
                        else
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) | Adding {languageName} text in sector {sectorKey} key {key}");
                            entries.Add (key, new DataBlockTextEntryLocalization { text = text });
                        }
                    }
                }
            }
        }

        private const string dataPathPrefixConfigs = "Configs/";
        private const string dataPathPrefixConfigOverrides = "ConfigOverrides/";

        public static void ProcessConfigModsForMultiLinker<T> (Type dataType, SortedDictionary<string, T> dataInternal, string dataPath) where T : DataContainer, new()
        {
            if (dataInternal == null || dataType == null)
                return;

            if (loadedMods == null)
                return;

            var dataTypeName = dataType.Name;
            var pathRoot = dataPath.Replace (dataPathPrefixConfigs, dataPathPrefixConfigOverrides);

            for (int i = 0; i < loadedMods.Count; ++i)
            {
                var loadedData = loadedMods[i];
                if (loadedData == null || loadedData.metadata == null)
                    continue;

                var modID = loadedData.metadata.id;
                var pathFull = loadedData.metadata.path + pathRoot;

                if (loadedData.metadata.includesConfigOverrides && loadedData.configOverrides != null && loadedData.configOverrides.Count > 0)
                {
                    for (int c = 0; c < loadedData.configOverrides.Count; ++c)
                    {
                        var configOverride = loadedData.configOverrides[c];
                        if (configOverride == null || configOverride.containerObject == null)
                            continue;

                        if (configOverride.type != dataType)
                            continue;

                        var container = configOverride.containerObject as T;
                        if (container == null)
                            continue;

                        var key = configOverride.key;
                        container.path = pathFull;

                        if (dataInternal.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) replaces config {key} of type {dataTypeName}");
                            dataInternal[key] = container;
                        }
                        else
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) injects additional config {key} of type {dataTypeName}");
                            dataInternal.Add (key, container);
                        }
                    }
                }

                if (loadedData.metadata.includesConfigEdits && loadedData.configEdits != null && loadedData.configEdits.Count > 0)
                {
                    for (int c = 0; c < loadedData.configEdits.Count; ++c)
                    {
                        var configEdit = loadedData.configEdits[c];
                        if (configEdit == null || configEdit.data == null)
                            continue;

                        if (configEdit.type != dataType)
                            continue;

                        var key = configEdit.key;
                        if (!dataInternal.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) attempts to edit config {key} of type {dataTypeName}, which doesn't exist");
                            continue;
                        }

                        var container = dataInternal[key];
                        var editData = configEdit.data;

                        if (editData.removed)
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) removes config {key} of type {dataTypeName}");
                            dataInternal.Remove (key);
                            continue;
                        }

                        if (editData.edits != null && editData.edits.Count > 0)
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) edits config {key} of type {dataTypeName}");

                            int f = 0;
                            foreach (var step in editData.edits)
                            {
                                var filename = container.key;
                                ModUtilities.ProcessFieldEdit (container, filename, step.path, step.value, i, modID, dataTypeName);
                                f += 1;
                            }
                        }
                    }
                }

                if (loadedData.metadata.includesConfigTrees && loadedData.configTrees != null && loadedData.configTrees.Count > 0)
                {
                    for (int c = 0; c < loadedData.configTrees.Count; ++c)
                    {
                        var configTree = loadedData.configTrees[c];
                        if (configTree == null || configTree.data == null)
                            continue;

                        if (configTree.type != dataType)
                            continue;

                        var key = configTree.key;
                        if (!dataInternal.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) attempts to apply edit tree to config {key} of type {dataTypeName}, which doesn't exist");
                            continue;
                        }

                        var container = dataInternal[key];
                        var editData = configTree.data;
                    }
                }
            }
        }

        public static T ProcessConfigModsForLinker<T> (Type dataType, T dataInternal) where T : DataContainerUnique
        {
            if (dataInternal == null || dataType == null)
                return dataInternal;

            if (loadedMods == null)
                return dataInternal;

            var dataTypeName = dataType.Name;

            for (int i = 0; i < loadedMods.Count; ++i)
            {
                var loadedData = loadedMods[i];
                if (loadedData == null || loadedData.metadata == null)
                    continue;

                var modID = loadedData.metadata.id;
                if (loadedData.metadata.includesConfigOverrides && loadedData.configOverrides != null && loadedData.configOverrides.Count > 0)
                {
                    for (int c = 0; c < loadedData.configOverrides.Count; ++c)
                    {
                        var configOverride = loadedData.configOverrides[c];
                        if (configOverride == null || configOverride.containerObject == null)
                            continue;

                        if (configOverride.type != dataType)
                            continue;

                        var container = configOverride.containerObject as T;
                        if (container == null)
                            continue;

                        var key = configOverride.key;
                        Debug.LogWarning ($"Mod {i} ({modID}) replaces global config {key} of type {dataTypeName}");
                        dataInternal = container;
                    }
                }

                if (loadedData.metadata.includesConfigEdits && loadedData.configEdits != null && loadedData.configEdits.Count > 0)
                {
                    for (int c = 0; c < loadedData.configEdits.Count; ++c)
                    {
                        var configEdit = loadedData.configEdits[c];
                        if (configEdit == null || configEdit.data == null)
                            continue;

                        if (configEdit.type != dataType)
                            continue;

                        var editData = configEdit.data;
                        if (editData.removed)
                        {
                            Debug.LogWarning ($"Mod {i} ({modID}) requests removal of global config {configEdit.key} of type {dataTypeName}, which is not allowed");
                            continue;
                        }

                        if (editData.edits != null && editData.edits.Count > 0)
                        {
                            // Debug.Log ($"Mod {i} ({modID}) edits global config {configEdit.key} of type {dataTypeName}");

                            int f = 0;
                            foreach (var step in editData.edits)
                            {
                                var filename = configEdit.key;
                                ModUtilities.ProcessFieldEdit (dataInternal, filename, step.path, step.value, i, modID, dataTypeName);
                            }
                        }
                    }
                }
            }

            return dataInternal;
        }

        public static bool TryParseVersionString (string input, out int versionMajor, out int versionMinor, out int versionPatch, bool maxVersionContext = false)
        {
            versionMajor = 0;
            versionMinor = 0;
            versionPatch = 0;

            var versionTextSplit = input.Split ('.');
            if (versionTextSplit.Length == 3)
            {
                bool versionMajorParsed = int.TryParse (versionTextSplit[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out versionMajor);
                bool versionMinorParsed = int.TryParse (versionTextSplit[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out versionMinor);
                bool versionPatchParsed = int.TryParse (versionTextSplit[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out versionPatch);
                if (versionMajorParsed && versionMinorParsed && versionPatchParsed)
                    return true;
            }
            else if (versionTextSplit.Length == 2)
            {
                bool versionMajorParsed = int.TryParse (versionTextSplit[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out versionMajor);
                bool versionMinorParsed = int.TryParse (versionTextSplit[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out versionMinor);
                versionPatch = maxVersionContext ? 100 : 0;
                if (versionMajorParsed && versionMinorParsed)
                    return true;
            }

            return false;
        }



        public class QueuedDialog
        {
            public string message;
            public bool critical;

            public QueuedDialog (string message, bool critical = false)
            {
                this.message = message;
                this.critical = critical;
            }
        }

        public static bool IsLoadingIssueDetected ()
        {
            return queuedDialogs != null && queuedDialogs.Count > 0;
        }

        private static List<QueuedDialog> queuedDialogs = new List<QueuedDialog> ();

        private static void PostWarning (string message, bool critical = false)
        {
            #if !PB_MODSDK
            queuedDialogs.Add (new QueuedDialog (message, critical));
            #endif

            Debug.LogWarning ($"MM | {message}");
        }

        private static void CheckQueuedDialogs ()
        {
            #if !PB_MODSDK

            if (queuedDialogs == null || queuedDialogs.Count == 0)
                return;

            var dialog = queuedDialogs[0];

            string header = "Mod manager";
            float hue = 0.08f;
            System.Action callback = CheckQueuedDialogs;

            if (dialog.critical)
            {
                hue = 0f;
                callback = ExitToDesktop;
            }

            queuedDialogs.RemoveAt (0);
            CIViewDialogConfirmation.ins.Open (header, dialog.message, callback, null, showCancel: false, hue: hue);

            #endif
        }

        #if !PB_MODSDK

        private static void ExitToDesktop ()
        {
            #if !UNITY_EDITOR
            Co.Delay (0.1f, () => Application.Quit ());
            #endif
        }

        #endif
    }
}
