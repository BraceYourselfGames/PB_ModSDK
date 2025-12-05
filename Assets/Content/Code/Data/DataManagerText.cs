using System;
using System.Collections.Generic;
using System.IO;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Mods;
using Sirenix.OdinInspector;
using UnityEngine;

#if PB_MODSDK
using System.Linq;
using PhantomBrigade.SDK.ModTools;
#endif

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataManagerText : MonoBehaviour
    {
        private const string tgSettings = "Settings";
        private const string tgLib = "Library";
        private const string fgAddText = "_DefaultTabGroup/Library/Add or replace text";
        private const string bgAddText = "_DefaultTabGroup/Library/Add or replace text/Bg";

        private const string tgLoc = "Localization";

        private const string tgUtilities = "Utilities";
        private const string bgUtilitiesLegacy = "_DefaultTabGroup/Utilities/ButtonsLegacy";
        private const string bgUtilitiesOther = "_DefaultTabGroup/Utilities/ButtonsOther";

        [ShowInInspector, FoldoutGroup (tgSettings), PropertyOrder (-10)]
        public static bool log = false;

        [ShowInInspector, FoldoutGroup (tgSettings), PropertyOrder (-10)]
        public static bool showProcessedText = false;

        private static bool libraryLoadedOnce = false;
        private static DataContainerTextLibrary libraryDataInternal;

        [PropertySpace (8f), PropertyOrder (-5)]
        [TabGroup (tgLib), ShowInInspector, OnValueChanged ("OnLibraryModified", true)]
        public static DataContainerTextLibrary libraryData
        {
            get
            {
                if (libraryDataInternal == null && !libraryLoadedOnce)
                    LoadLibrary ();
                return libraryDataInternal;
            }
            set
            {
                libraryDataInternal = value;
            }
        }

        private static bool libraryUnsaved = false;
        private static string libraryPath = "Configs/TextLibrary";

        #if PB_MODSDK

        private static bool librarySourceLoadedOnce = false;
        private static DataContainerTextLibrary librarySourceDataInternal;

        [PropertySpace (8f), PropertyOrder (-5)]
        [TabGroup (tgLib), ShowInInspector]
        public static DataContainerTextLibrary librarySourceData
        {
            get
            {
                if (librarySourceDataInternal == null && !librarySourceLoadedOnce)
                    LoadLibrarySource ();
                return librarySourceDataInternal;
            }
        }

        #endif



        [TabGroup (tgLoc), ShowInInspector, PropertyOrder (-2)]
        private static SortedDictionary<string, string> localizationKeysToPathsBuiltin = new SortedDictionary<string, string> ();

        [TabGroup (tgLoc), ShowInInspector, PropertyOrder (-2)]
        private static SortedDictionary<string, string> localizationKeysToPathsMods = new SortedDictionary<string, string> ();

        [TabGroup (tgLoc), ShowInInspector]
        [ValueDropdown ("@localizationKeysToPathsBuiltin.Keys")]
        public static string localizationKeySelected = "French";

        [TabGroup (tgLoc), ShowInInspector, ReadOnly]
        public static string localizationKeyLast;

        [TabGroup (tgLoc), ShowInInspector, ReadOnly]
        public static string localizationPathLast;

        [TabGroup (tgLoc), ShowInInspector, ReadOnly]
        public static bool localizationPathAppendedLast;


        [PropertySpace (8f), PropertyOrder (1)]
        [TabGroup (tgLoc), ShowInInspector, OnValueChanged ("OnLocalizationModified", true)]
        public static DataContainerTextLocalization localizationDataInternal;



        private static bool localizationUnsaved = false;
        private static string localizationPathBuiltin = "Configs/TextLocalizations/";



        private void OnEnable ()
        {

        }

        private void Update ()
        {

        }

        private void OnLibraryModified () => libraryUnsaved = true;
        private void OnLocalizationModified () => localizationUnsaved = true;

        [PropertyOrder (-10)]
        [TabGroup (tgLib)]
        [ButtonGroup ("_DefaultTabGroup/Library/Buttons"), Button ("Load", ButtonSizes.Large)]
        public static void LoadLibrary ()
        {
            libraryLoadedOnce = true;
            libraryDataInternal = new DataContainerTextLibrary ();

            var pathFull = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), libraryPath);

            #if PB_MODSDK && UNITY_EDITOR

            if (IsModOverrideUsed ())
            {
                var modPath = DataContainerModData.selectedMod.GetModPathProject ();
                pathFull = DataPathHelper.GetCombinedCleanPath (modPath, libraryPath);
                Debug.Log ($"Loading text library from config enabled mod | Path: {pathFull}");
            }

            #endif

            libraryDataInternal.core = UtilitiesYAML.LoadDataFromFile<DataContainerTextLibraryCore>
            (
                pathFull,
                "core.yaml",
                appendApplicationPath: false
            );

            libraryDataInternal.sectors = UtilitiesYAML.LoadDecomposedDictionary<DataContainerTextSectorMain>
            (
                pathFull + "/Sectors",
                appendApplicationPath: false
            );

            libraryDataInternal.OnAfterDeserialization ();

            #if PB_MODSDK && UNITY_EDITOR
            if (IsModOverrideUsed())
            {
                // Check for disk changes that happened offline.
                var modData = DataContainerModData.selectedMod;
                var checksum = modData.textLibrary.LibraryDirectory.Mod.Checksum;
                UpdateChecksums ();
                if (!ConfigChecksums.ChecksumEqual (checksum, modData.textLibrary.LibraryDirectory.Mod.Checksum))
                {
                    Debug.Log ("TextLibrary: offline or format changes detected in mod configs -- refreshing checksums on disk");
                    ModToolsHelper.SaveChecksums (DataContainerModData.selectedMod);
                }
            }
            #endif
        }

        #if PB_MODSDK

        public static void LoadLibrarySource ()
        {
            librarySourceLoadedOnce = true;
            librarySourceDataInternal = new DataContainerTextLibrary ();

            var pathFull = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), libraryPath);

            librarySourceDataInternal.core = UtilitiesYAML.LoadDataFromFile<DataContainerTextLibraryCore>
            (
                pathFull,
                "core.yaml",
                appendApplicationPath: false
            );

            librarySourceDataInternal.sectors = UtilitiesYAML.LoadDecomposedDictionary<DataContainerTextSectorMain>
            (
                pathFull + "/Sectors",
                appendApplicationPath: false
            );

            librarySourceDataInternal.OnAfterDeserialization ();
        }

        public static void ResetLoadedOnce ()
        {
            libraryLoadedOnce = false;
            libraryDataInternal = null;
        }

        private static bool IsModOverrideUsed ()
        {
            #if UNITY_EDITOR
            return DataContainerModData.selectedMod != null &&
                   DataContainerModData.selectedMod.hasProjectFolder &&
                   Directory.Exists (DataContainerModData.selectedMod.GetModPathConfigs ());
            #else
            return false;
            #endif
        }

        #endif

        [PropertyOrder (-10)]
        [TabGroup (tgLib)]
        [ButtonGroup ("_DefaultTabGroup/Library/Buttons"), Button ("@libraryUnsaved ? \"Save data*\" : \"Save data\"", ButtonSizes.Large)]
        [GUIColor ("GetLibrarySaveButtonColor")]
        public static void SaveLibrary ()
        {
            if (libraryDataInternal == null)
            {
                Debug.Log ($"No library data available, can't save");
                return;
            }

            var pathFull = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), libraryPath);

            #if PB_MODSDK && UNITY_EDITOR

            if (IsModOverrideUsed ())
            {
                var modPath = DataContainerModData.selectedMod.GetModPathProject ();
                pathFull = DataPathHelper.GetCombinedCleanPath (modPath, libraryPath);
                Debug.Log ($"Saving text library to config enabled mod | Path: {pathFull}");
            }

            #endif

            if (log)
                Debug.Log ($"Writing main text library to path {pathFull}");

            libraryDataInternal.OnBeforeSerialization ();

            if (libraryDataInternal.core != null)
            {
                UtilitiesYAML.SaveDataToFile
                (
                    pathFull,
                    "core.yaml",
                    libraryDataInternal.core,
                    appendApplicationPath: false
                );
            }

            if (libraryDataInternal.sectors != null)
            {
                UtilitiesYAML.SaveDecomposedDictionary
                (
                    pathFull + "/Sectors",
                    libraryDataInternal.sectors,
                    false,
                    appendApplicationPath: false
                );
            }

            libraryUnsaved = false;

            #if PB_MODSDK && UNITY_EDITOR
            SaveChecksums ();
            #endif
        }

        #if PB_MODSDK && UNITY_EDITOR
        static void SaveChecksums ()
        {
            if (!IsModOverrideUsed ())
            {
                return;
            }
            UpdateChecksums ();
            ModToolsHelper.SaveChecksums (DataContainerModData.selectedMod);
        }

        static void UpdateChecksums ()
        {
            var modData = DataContainerModData.selectedMod;
            var modPath = modData.GetModPathProject ();
            var pathLibrary = DataPathHelper.GetCombinedCleanPath (modPath, libraryPath);
            var pathCore = DataPathHelper.GetCombinedCleanPath (pathLibrary, "core.yaml");
            var modLibraryChecksum = modData.textLibrary.LibraryDirectory.Mod;
            var entryCore = modLibraryChecksum.Entries.SingleOrDefault (e => e.RelativePath.EndsWith ("core.yaml"));
            if (entryCore != null)
            {
                try
                {
                    ((ConfigChecksums.ConfigFile)entryCore).Update (ConfigChecksums.EntrySource.Mod, File.ReadAllText (pathCore));
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat ("Error during checksum operation | mod: {0} | key: {1} | path: {2}", modData.id, "core", pathCore);
                    Debug.LogException (ex);
                }
            }

            var pathSectors = DataPathHelper.GetCombinedCleanPath (pathLibrary, "Sectors");
            var sectorsChecksum = modData.textLibrary.SectorsDirectory;
            var modSectorsChecksums = new ConfigChecksums.ConfigDirectory (ConfigChecksums.EntryType.Directory)
            {
                Source = ConfigChecksums.EntrySource.Mod,
                Locator = sectorsChecksum.Mod.Locator,
                RelativePath = sectorsChecksum.Mod.RelativePath,
            };
            var errorKeys = new List<string> ();
            var entries = new List<(string, string)> ();
            foreach (var key in libraryData.sectors.Keys)
            {
                var fileName = key + ".yaml";
                var filePath = DataPathHelper.GetCombinedCleanPath (pathSectors, fileName);
                try
                {
                    entries.Add ((fileName, File.ReadAllText (filePath)));
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat ("Error during checksum operation | mod: {0} | key: {1} | path: {2}", modData.id, key, filePath);
                    Debug.LogException (ex);
                    errorKeys.Add (key);
                }
            }
            modSectorsChecksums.AddFiles (ConfigChecksums.EntrySource.Mod, entries);
            UpdateChecksumSource (sectorsChecksum.SDK, modSectorsChecksums);
            RemoveErrors (sectorsChecksum.SDK, errorKeys);
            modData.textLibrary.SectorsDirectory = (sectorsChecksum.SDK, modSectorsChecksums);

            modData.checksumsRoot.Patch (modSectorsChecksums);
            ConfigChecksums.UpdateChecksums (ConfigChecksums.EntrySource.Mod, modData.checksumsRoot, modSectorsChecksums);
        }

        static void UpdateChecksumSource (ConfigChecksums.ConfigDirectory sdk, ConfigChecksums.ConfigDirectory mod)
        {
            if (sdk == null)
            {
                return;
            }

            var sdkKeyMap = ModToolsHelper.GetEntriesByKey (sdk, false);
            var modKeyMap = ModToolsHelper.GetEntriesByKey (mod, false);
            foreach (var kvp in modKeyMap)
            {
                if (!sdkKeyMap.TryGetValue (kvp.Key, out var sdkEntry))
                {
                    continue;
                }
                if (!ConfigChecksums.ChecksumEqual (sdkEntry.Checksum, kvp.Value.Checksum))
                {
                    continue;
                }
                kvp.Value.Source = ConfigChecksums.EntrySource.SDK;
            }
        }

        static void RemoveErrors (ConfigChecksums.ConfigDirectory sdk, List<string> errorKeys)
        {
            if (errorKeys.Any ())
            {
                var sdkKeyMap = ModToolsHelper.GetEntriesByKey (sdk, false);
                var errorIndexes = errorKeys
                    .Where (k => sdkKeyMap.ContainsKey (k))
                    .Select (k => -sdkKeyMap[k].Locator.Last ())
                    .ToList ();

                if (sdk != null)
                {
                    errorIndexes.Sort ();
                    foreach (var idx in errorIndexes)
                    {
                        sdk.Entries.RemoveAt (-idx);
                    }
                    sdk.FixLocators ();
                }

                foreach (var key in errorKeys)
                {
                    libraryData.sectors.Remove (key);
                }
            }
        }
        #endif

        public static void MarkLibraryChanged ()
        {
            libraryUnsaved = true;
        }

        public static void MarkLocalizationChanged ()
        {
            libraryUnsaved = true;
        }

        public static DataContainerTextLocalization GetLocalizationData ()
        {
            return localizationDataInternal;
        }

        public static string GetCurrentFontKey ()
        {
            if (localizationDataInternal != null)
                return localizationDataInternal.core.font;
            return libraryDataInternal.core.font;
        }


        [PropertyOrder (-1)]
        [TabGroup (tgLoc)]
        [ButtonGroup ("_DefaultTabGroup/Localization/Buttons"), Button ("Refresh list", ButtonSizes.Large)]
        public static void RefreshLocalizationListBuiltin ()
        {
            localizationKeysToPathsBuiltin.Clear ();

            // Initialize lookup first, provided its possible
            if (!Directory.Exists (localizationPathBuiltin))
            {
                Debug.LogWarning ($"Failed to find built-in localization folder, LoadLocalizationFromKey will fail to load any key | Path: {localizationPathBuiltin}");
                return;
            }

            var directories = Directory.GetDirectories (localizationPathBuiltin);
            if (directories.Length == 0)
            {
                Debug.LogWarning ($"Failed to find any subfolders in main localization folder, LoadDataFromName will fail to load any official localizations | Path: {localizationPathBuiltin}");
                return;
            }

            foreach (var path in directories)
            {
                // Bail if folder is empty
                var files = Directory.GetFiles (path);
                if (files.Length == 0)
                    continue;

                var key = path.Replace (localizationPathBuiltin, string.Empty).Replace ("\\", "/");
                localizationKeysToPathsBuiltin.Add (key, path);
            }
        }

        [PropertyOrder (-1)]
        [TabGroup (tgLoc)]
        [ButtonGroup ("_DefaultTabGroup/Localization/Buttons"), Button ("Load", ButtonSizes.Large)]
        private static void LoadLocalizationSelected ()
        {
            LoadLocalizationFromKey (localizationKeySelected);
        }

        public static void LoadLocalizationFromKey (string localizationKey)
        {
            if (string.IsNullOrEmpty (localizationKey))
            {
                Debug.LogWarning ($"Null or empty localization key provided, can't load localization");
                return;
            }

            // Initialize lookup first, provided its possible
            if (localizationKeysToPathsBuiltin.Count == 0)
                RefreshLocalizationListBuiltin ();

            // If official localization exists, load it
            if (localizationKeysToPathsBuiltin.ContainsKey (localizationKey))
            {
                var path = localizationKeysToPathsBuiltin[localizationKey];
                localizationKeyLast = localizationKey;
                LoadLocalization (path, true);
            }
            else if (localizationKeysToPathsMods.ContainsKey (localizationKey))
            {
                var path = localizationKeysToPathsMods[localizationKey];
                localizationKeyLast = localizationKey;
                LoadLocalization (path, false);
            }
            else
            {
                Debug.LogWarning ($"Failed to find any localizations with key {localizationKey}");
            }
        }

        public static void LoadLocalization (string localizationPath, bool appendApplicationPath)
        {
            localizationPathLast = DataPathHelper.GetCleanPath (localizationPath);
            localizationPathAppendedLast = appendApplicationPath;
            Debug.Log ($"Attempting to load localization using path {localizationPathLast} | Application path appended: {localizationPathAppendedLast}");

            localizationDataInternal = new DataContainerTextLocalization ();

            localizationDataInternal.core = UtilitiesYAML.LoadDataFromFile<DataContainerTextLibraryCore>
                (localizationPath, "core.yaml", appendApplicationPath: appendApplicationPath);

            localizationDataInternal.sectors = UtilitiesYAML.LoadDecomposedDictionary<DataContainerTextSectorLocalization>
                ($"{localizationPath}/Sectors", appendApplicationPath: appendApplicationPath);

            // Only inject mod text at runtime and before post-deserialization code
            if (Application.isPlaying)
                ModManager.ProcessLocalizationEdits (localizationKeyLast, localizationDataInternal.sectors);

            localizationDataInternal.OnAfterDeserialization ();
        }

        [PropertyOrder (-1)]
        [TabGroup (tgLoc)]
        [ShowIf ("@localizationDataInternal != null")]
        [ButtonGroup ("_DefaultTabGroup/Localization/Buttons"), Button ("Unload", ButtonSizes.Large)]
        public static void FlushLocalization ()
        {
            Debug.Log ($"English language selected: main library will be used for all text queries from this point on. If a localization was loaded at this point, it would be unloaded.");
            localizationDataInternal = null;
            localizationKeyLast = "English";
            localizationPathLast = null;
            localizationPathAppendedLast = false;
        }

        [PropertyOrder (-1)]
        [TabGroup (tgLoc)]
        [ButtonGroup ("_DefaultTabGroup/Localization/Buttons"), Button ("@localizationUnsaved ? \"Save data*\" : \"Save data\"", ButtonSizes.Large)]
        [GUIColor ("GetLocalizationSaveButtonColor")]
        public static void SaveLocalizationLoaded ()
        {
            if (localizationDataInternal == null)
            {
                Debug.LogWarning ($"No loaded localization data available, can't save");
                return;
            }

            if (string.IsNullOrEmpty (localizationPathLast))
            {
                Debug.LogWarning ("No last localization path available, can't save");
                return;
            }

            if (log)
                Debug.Log ($"Writing localization {localizationKeyLast} to path {localizationPathLast} | Appended: {localizationPathAppendedLast}");

            localizationDataInternal.OnBeforeSerialization ();

            if (localizationDataInternal.core != null)
                UtilitiesYAML.SaveDataToFile (localizationPathLast, "core.yaml", localizationDataInternal.core, localizationPathAppendedLast);

            if (localizationDataInternal.sectors != null)
                UtilitiesYAML.SaveDecomposedDictionary ($"{localizationPathLast}/Sectors", localizationDataInternal.sectors, false, localizationPathAppendedLast);

            localizationUnsaved = false;
        }



        public static DataContainerTextSectorLocalization GetLocalizationSector (string sectorKey)
        {
            if (localizationDataInternal == null || localizationDataInternal.sectors == null)
                return null;

            return localizationDataInternal.GetSector (sectorKey);
        }

        public static DataContainerTextSectorMain GetLibrarySector (string sectorKey)
        {
            if (libraryData == null || libraryData.sectors == null)
                return null;

            return libraryData.GetSector (sectorKey);
        }

        public static string GetText (string sectorKey, string textKey, bool suppressWarning = false)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found: Null or empty sector key");
                return string.Empty;
            }

            if (string.IsNullOrEmpty (textKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found: Null or empty text key");
                return string.Empty;
            }

            // Only check loc. during play mode
            if (!Application.isPlaying)
                return GetTextFromLibrary (sectorKey, textKey, suppressWarning);

            // If localization is missing entirely, go straight to library as fallback
            if (localizationDataInternal == null)
            {
                // if (!suppressWarning)
                //     Debug.LogWarning ($"Text not found in localization ({sectorKey}/{textKey}): Localization not loaded, falling back to library");
                return GetTextFromLibrary (sectorKey, textKey, suppressWarning);
            }

            var sector = localizationDataInternal.GetSector (sectorKey);
            if (sector == null)
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in localization ({sectorKey}/{textKey}): Sector not present, falling back to library");
                return GetTextFromLibrary (sectorKey, textKey, suppressWarning);
            }

            if (sector.entries == null || !sector.entries.ContainsKey (textKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in localization ({sectorKey}/{textKey}): Sector doesn't contain a given key, falling back to library");
                return GetTextFromLibrary (sectorKey, textKey, suppressWarning);
            }

            var entry = sector.entries[textKey];
            var text = entry.textProcessed;
            bool textEmpty = string.IsNullOrEmpty (text);

            if (textEmpty)
            {
                if (DataShortcuts.debug.textFromLibraryWhenEmpty)
                    return GetTextFromLibrary (sectorKey, textKey, suppressWarning);
                else if (DataShortcuts.debug.textWarningWhenEmpty)
                    return $"[b][ffff88]${textKey}[-][/b]";
                else
                    return string.Empty;
            }

            return entry.textProcessed;
        }

        #if PB_MODSDK

        public static string GetRawTextFromLibrary (bool forceSource, string sectorKey, string textKey, bool suppressWarning = true)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library: Null or empty sector key");
                return string.Empty;
            }

            if (string.IsNullOrEmpty (textKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library: Null or empty text key");
                return string.Empty;
            }

            var libraryDataSelected = forceSource ? librarySourceData : libraryData;
            if (libraryDataSelected == null)
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in ({(forceSource ? "source" : "working")}) library ({sectorKey}/{textKey}): Library not loaded");
                return string.Empty;
            }

            var sector = libraryData.GetSector (sectorKey);
            if (sector == null)
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in ({(forceSource ? "source" : "working")}) library ({sectorKey}/{textKey}): Sector not present");
                return string.Empty;
            }

            if (sector.entries == null || !sector.entries.ContainsKey (textKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in ({(forceSource ? "source" : "working")}) library ({sectorKey}/{textKey}): Sector doesn't contain this key");
                return string.Empty;
            }

            var entry = sector.entries[textKey];
            return entry.text;
        }

        #endif

        private static string GetTextFromLibrary (string sectorKey, string textKey, bool suppressWarning = false)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library: Null or empty sector key");
                return string.Empty;
            }

            if (string.IsNullOrEmpty (textKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library: Null or empty text key");
                return string.Empty;
            }

            if (libraryData == null)
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library ({sectorKey}/{textKey}): Main library not loaded");
                return string.Empty;
            }

            var sector = libraryData.GetSector (sectorKey);
            if (sector == null)
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library ({sectorKey}/{textKey}): Sector not present");
                return string.Empty;
            }

            if (sector.entries == null || !sector.entries.ContainsKey (textKey))
            {
                if (!suppressWarning)
                    Debug.LogWarning ($"Text not found in library ({sectorKey}/{textKey}): Sector doesn't contain a given key");
                return string.Empty;
            }

            var entry = sector.entries[textKey];
            var text = entry.textProcessed;
            bool textEmpty = string.IsNullOrEmpty (text);

            if (textEmpty)
            {
                if (DataShortcuts.debug.textWarningWhenEmpty)
                    return $"[b][ff8888]${textKey}[-][/b]";
                else
                    return string.Empty;
            }

            return entry.textProcessed;
        }

        public static DataBlockTextEntryMain GetTextEntryFromLibrary (string sectorKey, string textKey)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Text not found in library: Null or empty sector key");
                return null;
            }

            if (string.IsNullOrEmpty (textKey))
            {
                Debug.LogWarning ($"Text not found in library: Null or empty text key");
                return null;
            }

            if (libraryData == null)
            {
                Debug.LogWarning ($"Text not found in library ({sectorKey}/{textKey}): Main library not loaded");
                return null;
            }

            var sector = libraryData.GetSector (sectorKey);
            if (sector == null)
            {
                Debug.LogWarning ($"Text not found in library ({sectorKey}/{textKey}): Sector not present");
                return null;
            }

            if (sector.entries == null || !sector.entries.ContainsKey (textKey))
            {
                Debug.LogWarning ($"Text not found in library ({sectorKey}/{textKey}): Sector doesn't contain a given key");
                return null;
            }

            var entry = sector.entries[textKey];
            return entry;
        }

        private static List<string> fallbackKeyList = new List<string> { };

        public static IEnumerable<string> GetLibrarySectorKeys ()
        {
            if (libraryData == null || libraryData.sectors == null)
                return fallbackKeyList;

            return libraryData.sectors.Keys;
        }

        #if UNITY_EDITOR
        public static IEnumerable<string> GetAddTextKeys ()
        {
            var sector = GetLibrarySector (addTextSector);
            if (sector == null || sector.entries == null)
                return null;
            return sector.entries.Keys;
        }
        #endif

        public static IEnumerable<string> GetLocalizationLanguageKeysBuiltin ()
        {
            if (localizationKeysToPathsBuiltin == null)
                return fallbackKeyList;

            return localizationKeysToPathsBuiltin.Keys;
        }

        public static IEnumerable<string> GetLocalizationSectorKeys ()
        {
            if (localizationDataInternal == null || localizationDataInternal.sectors == null)
                return fallbackKeyList;

            return localizationDataInternal.sectors.Keys;
        }

        public static IEnumerable<string> GetLibraryTextKeys (string sectorKey)
        {
            if (libraryData == null || libraryData.sectors == null || string.IsNullOrEmpty (sectorKey) || !libraryData.sectors.ContainsKey (sectorKey))
                return fallbackKeyList;

            var sector = libraryData.sectors[sectorKey];
            if (sector.entries == null)
                return fallbackKeyList;
            return sector.entries.Keys;
        }

        public static void TryAddingTextToLibrary (string sectorKey, string textKey, string text, string note = null)
        {
            if (libraryData == null)
            {
                Debug.LogWarning ($"Can't add text to library: no library loaded");
                return;
            }

            libraryData.TryAddingText (sectorKey, textKey, text, note);
        }

        public static void TryAddingSectorToLibrary (string sectorKey)
        {
            if (libraryData == null)
            {
                Debug.LogWarning ($"Can't add sector to library: no library loaded");
                return;
            }

            libraryData.TryAddingSector (sectorKey);
        }


        public static void ClearLocalizationsFromMods ()
        {
            localizationKeysToPathsMods.Clear ();
        }

        public static void AddLocalizationFromMod (string key, string path)
        {
            if (string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ($"Failed to add localization due to null or empty key");
                return;
            }

            if (localizationKeysToPathsMods.ContainsKey (key))
            {
                Debug.LogWarning ($"Failed to add localization due to key {key} already being registered");
                return;
            }

            if (localizationKeysToPathsMods.ContainsKey (path))
            {
                Debug.LogWarning ($"Failed to add localization due to key {key} already being registered");
                return;
            }

            localizationKeysToPathsMods.Add (key, path);
        }

        #if UNITY_EDITOR

        private static Color colorNormal = new Color (1f, 1f, 1f, 1f);
        private static Color colorUnsaved = Color.HSVToRGB (0.6f, 0.5f, 1f);

        private Color GetLibrarySaveButtonColor => libraryUnsaved ? colorUnsaved : colorNormal;
        private Color GetLocalizationSaveButtonColor => localizationUnsaved ? colorUnsaved : colorNormal;

        private static bool IsAddKeyInvalid ()
        {
            var sector = libraryData != null && !string.IsNullOrEmpty (addTextSector) && libraryData.sectors.ContainsKey (addTextSector) ? libraryData.sectors[addTextSector] : null;
            var keyValid = sector == null || sector.entries == null || (!string.IsNullOrEmpty (addTextKey) && !sector.entries.ContainsKey (addTextKey));
            return !keyValid;
        }

        private static bool IsAddBlockVisible ()
        {
            return libraryData != null;
        }



        [PropertyOrder (-6)]
        [ShowIf ("IsAddBlockVisible")]
        [FoldoutGroup (fgAddText, false)]
        [Button ("Add to library"), ButtonGroup (bgAddText)]
        private static void AddTextToLibrary () => TryAddingTextToLibrary (addTextSector, addTextKey, addTextValue);

        [PropertyOrder (-6)]
        [ShowIf ("IsAddBlockVisible")]
        [FoldoutGroup (fgAddText, false)]
        [Button ("Copy to clipboard"), ButtonGroup (bgAddText)]
        private static void CopyTextSnippet ()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.systemCopyBuffer = addTextCode;
            #endif
        }

        private static bool IsExistingTextVisible => !string.IsNullOrEmpty (addTextSector) && !string.IsNullOrEmpty (addTextKeyDropdown);

        private static string GetExistingText ()
        {
            if (!IsExistingTextVisible)
                return string.Empty;

            return Txt.Get (addTextSector, addTextKeyDropdown, false);
        }

        [PropertyOrder (-6)]
        [ShowIf ("IsAddBlockVisible")]
        [FoldoutGroup (fgAddText)]
        [ShowInInspector]
        [LabelText ("Sector"), LabelWidth (120f), ValueDropdown ("GetLibrarySectorKeys")]
        [InfoBox ("@GetExistingText ()", "IsExistingTextVisible", InfoMessageType = InfoMessageType.None)]
        private static string addTextSector;

        [PropertyOrder (-6)]
        [ShowIf ("IsAddBlockVisible")]
        [FoldoutGroup (fgAddText)]
        [ShowInInspector]
        [LabelText ("Existing Keys"), LabelWidth (120f), ValueDropdown ("GetAddTextKeys")]
        private static string addTextKeyDropdown;

        [PropertyOrder (-6)]
        [ShowIf ("IsAddBlockVisible")]
        [FoldoutGroup (fgAddText)]
        [ShowInInspector]
        [LabelText ("Key / Text"), LabelWidth (120f), InfoBox ("Invalid key (empty or already registered)", "IsAddKeyInvalid", InfoMessageType = InfoMessageType.Warning)]
        private static string addTextKey;

        [PropertyOrder (-6)]
        [ShowIf ("IsAddBlockVisible")]
        [FoldoutGroup (fgAddText)]
        [ShowInInspector]
        [HideLabel, TextArea (1, 10)]
        private static string addTextValue;

        [PropertyOrder (-5)]
        [ShowIf ("@IsAddBlockVisible () && !string.IsNullOrEmpty (addTextSector) && !string.IsNullOrEmpty (addTextKey)")]
        [FoldoutGroup (fgAddText)]
        [ShowInInspector]
        [HideLabel, MultiLineProperty (3), GUIColor (0.625f, 0.825f, 0.8f)]
        private static string addTextCode
        {
            get { return GetCodeSnippet (); }
            set {  }
        }

        private static Type textSectorKeyClassType = typeof (TextLibs);
        private static string GetCodeSnippet ()
        {
            if (string.IsNullOrEmpty (addTextSector) || string.IsNullOrEmpty (addTextKey))
                return string.Empty;

            var fields = FieldReflectionUtility.GetConstantStringFieldNamesValues (textSectorKeyClassType);
            string shortcut = null;

            foreach (var kvp in fields)
            {
                bool match = kvp.Value == addTextSector;
                if (match)
                {
                    shortcut = kvp.Key;
                    break;
                }
            }

            if (shortcut != null)
                return $"Txt.Get (TextLibs.{shortcut}, \"{addTextKey}\")";
            else
                return $"Txt.Get (\"{addTextSector}\", \"{addTextKey}\")\n// Consider adding sector key \"{addTextSector}\" to TextLibs class";
        }

        #endif


        [PropertySpace (8f), PropertyOrder (1)]
        [TabGroup (tgUtilities), ShowInInspector]
        private static DataContainerTextLibraryOld libraryDataOld;

        private static string libraryPathOld = "Configs/Text/English";

        [TabGroup (tgUtilities)]
        [ButtonGroup (bgUtilitiesLegacy), Button ("Load (old)", ButtonSizes.Large)]
        public static void LoadLibraryOld ()
        {
            Debug.Log ($"Loading old library from {libraryPathOld}");

            libraryDataOld = new DataContainerTextLibraryOld ();
            libraryDataOld.core = UtilitiesYAML.LoadDataFromFile<DataContainerTextLibraryCoreOld>
                (libraryPathOld, "core.yaml", appendApplicationPath: true);
            libraryDataOld.specialized = UtilitiesYAML.LoadDataFromFile<DataContainerTextSpecializedOld>
                (libraryPathOld, "specialized.yaml", appendApplicationPath: true);
            libraryDataOld.sectors = UtilitiesYAML.LoadDecomposedDictionary<DataContainerTextSectorOld>
                ($"{libraryPathOld}/Sectors", appendApplicationPath: true);
        }

        [TabGroup (tgUtilities)]
        [ButtonGroup (bgUtilitiesLegacy), Button ("Save (old)", ButtonSizes.Large)]
        public static void SaveLibraryOld ()
        {
            if (libraryDataOld == null)
            {
                Debug.Log ($"No old library data available, can't save");
                return;
            }

            Debug.Log ($"Writing old text library to path {libraryPathOld}");

            if (libraryDataOld.core != null)
                UtilitiesYAML.SaveDataToFile (libraryPathOld, "core.yaml", libraryDataOld.core);

            if (libraryDataOld.specialized != null)
                UtilitiesYAML.SaveDataToFile (libraryPathOld, "specialized.yaml", libraryDataOld.specialized);

            if (libraryDataOld.sectors != null)
                UtilitiesYAML.SaveDecomposedDictionary ($"{libraryPathOld}/Sectors", libraryDataOld.sectors, false);
        }

        [TabGroup (tgUtilities)]
        [ButtonGroup (bgUtilitiesLegacy), Button ("Convert to\nnew format", ButtonSizes.Large)]
        public static void ConvertLibraryOld ()
        {
            if (libraryDataOld == null)
            {
                Debug.Log ($"No old library data available, can't save");
                return;
            }

            libraryData = new DataContainerTextLibrary ();

            var coreOld = libraryDataOld.core;
            var core = new DataContainerTextLibraryCore ();
            libraryData.core = core;

            core.name = "English";
            core.flag = "icon_flag_english";

            core.subtitleBuffer = coreOld.subtitleBuffer;
            core.subtitleFadeoutTime = coreOld.subtitleFadeoutTime;
            core.insertNewLines = coreOld.insertNewLines;
            core.widthSubtitle = coreOld.widthSubtitle;
            core.sizeSubtitle = coreOld.sizeSubtitle;
            core.sizeHint = coreOld.sizeHint;
            core.sizeTooltipHeader = coreOld.sizeTooltipHeader;
            core.sizeTooltipContent = coreOld.sizeTooltipContent;
            core.spacingTooltipHeader = coreOld.spacingTooltipHeader;

            /*
            var specOld = libraryDataOld.specialized;
            var collections = new SortedDictionary<string, DataContainerTextCollectionMain> ();
            libraryData.collections = collections;

            if (specOld.pilotGroups != null)
            {
                if (specOld.pilotGroups.TryGetValue ("player_default", out var group))
                {
                    collections.Add ("pilot_names_family_01", new DataContainerTextCollectionMain
                    {
                        localized = true,
                        tags = new HashSet<string> { "group_pilot", "pilot_names_family" },
                        entries = new List<string> (group.familyNames)
                    });

                    collections.Add ("pilot_names_given_01", new DataContainerTextCollectionMain
                    {
                        localized = true,
                        tags = new HashSet<string> { "group_pilot", "pilot_names_given" },
                        entries = new List<string> (group.givenNames)
                    });

                    collections.Add ("pilot_names_callsigns_01", new DataContainerTextCollectionMain
                    {
                        localized = true,
                        tags = new HashSet<string> { "group_pilot", "pilot_names_callsigns" },
                        entries = new List<string> (group.callsigns)
                    });
                }
            }

            if (specOld.overworldEntityGroups != null)
            {
                var nameGroupData = DataMultiLinkerOverworldNameGroup.data;
                nameGroupData.Clear ();

                int i = 0;
                foreach (var kvp in specOld.overworldEntityGroups)
                {
                    var groupKey = kvp.Key;
                    var group = kvp.Value;

                    if (group.names == null || group.names.Count == 0)
                        continue;

                    nameGroupData.Add (groupKey, new DataContainerOverworldNameGroup
                    {
                        key = groupKey,
                        collectionTag = groupKey,
                        prefixID = group.prefixFromID,
                        suffixID = group.suffixFromID
                    });

                    collections.Add ($"overworld_{groupKey}", new DataContainerTextCollectionMain
                    {
                        localized = true,
                        tags = new HashSet<string> { "group_overworld", groupKey },
                        entries = new List<string> (group.names)
                    });

                    i += 1;
                }

                DataMultiLinkerOverworldNameGroup.SaveData ();
            }
            */

            int sectorCount = 0;
            int entryCount = 0;

            var sectorsOld = libraryDataOld.sectors;
            var sectors = new SortedDictionary<string, DataContainerTextSectorMain> ();
            libraryData.sectors = sectors;

            foreach (var kvp in sectorsOld)
            {
                var sectorOld = kvp.Value;
                var sector = new DataContainerTextSectorMain ();
                sectors.Add (kvp.Key, sector);
                sectorCount += 1;

                sector.name = sectorOld.name;
                sector.description = sectorOld.description;
                sector.exportMode = TextSectorExportMode.None;

                if (sectorOld.exportMode == TextSectorExportModeOld.LinePerGroup)
                    sector.exportMode = TextSectorExportMode.LinePerKeySimple;
                else if (sectorOld.exportMode == TextSectorExportModeOld.LinePerKeySimple)
                    sector.exportMode = TextSectorExportMode.LinePerKeySimple;
                else if (sectorOld.exportMode == TextSectorExportModeOld.LinePerKeySplit)
                    sector.exportMode = TextSectorExportMode.LinePerKeySplit;

                if (sectorOld.splits != null && sectorOld.splits.Count > 0)
                {
                    sector.splits = new List<DataBlockTextSectorSplitInfo> ();
                    foreach (var s in sectorOld.splits)
                        sector.splits.Add (new DataBlockTextSectorSplitInfo { displayName = s.displayName, prefix = s.prefix });
                }

                var entriesOld = sectorOld.entries;
                var entries = new SortedDictionary<string, DataBlockTextEntryMain> ();
                sector.entries = entries;

                foreach (var kvp2 in entriesOld)
                {
                    var entryOld = kvp2.Value;
                    var entry = new DataBlockTextEntryMain ();
                    entries.Add (kvp2.Key, entry);
                    entryCount += 1;

                    entry.temp = entryOld.temp;
                    entry.text = entryOld.text;
                }
            }

            Debug.Log ($"Finished import of legacy text database to new format | Sectors: {sectorCount} | Entries: {entryCount}");
            DataManagerText.SaveLibrary ();
        }

        /*
        [TabGroup (tgUtilities)]
        [Button ("Convert collections to sectors", ButtonSizes.Large)]
        public static void ConvertCollectionsToSectors ()
        {
            if (libraryDataInternal == null || libraryDataInternal.collections == null)
                return;

            foreach (var kvp in libraryDataInternal.collections)
            {
                var colKey = kvp.Key;
                var colOld = kvp.Value;

                var colNew = new DataBlockTextCollection ();
                colNew.tags = colOld.tags;
                colNew.entries = colOld.entries;

                var sector = new DataContainerTextSectorMain ();
                sector.key = $"xc_{colKey}";
                sector.name = $"Col. ({colKey})";
                sector.description = "This is a text collection. In contrast to most text sectors, this has no pairs of keys and text. Instead, it simply contains a list of text. The game randomly selects from this list.";
                sector.localized = colOld.localized;
                sector.collection = colNew;
                sector.exportMode = TextSectorExportMode.Collection;
                sector.parent = libraryDataInternal;
                libraryDataInternal.sectors.Add (sector.key, sector);
            }

            libraryDataInternal.collections = null;
        }
        */
    }

    public static class Txt
    {
        public static string Get (string sectorKey, string textKey, bool suppressWarning = false) =>
            DataManagerText.GetText (sectorKey, textKey, suppressWarning);

        public static string Wrap (this string content, string prefix, string suffix)
        {
            return $"{prefix}{content}{suffix}";
        }
    }
}
