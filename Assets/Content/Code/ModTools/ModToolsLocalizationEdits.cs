using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using PhantomBrigade.SDK.ModTools;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhantomBrigade.ModTools
{
    [Serializable, HideDuplicateReferenceBox]
    public class ModConfigLocEdit
    {
        [OnValueChanged (nameof(UpdateChildren), true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, ModConfigLocEditLanguage> languages = new SortedDictionary<string, ModConfigLocEditLanguage> ();

        public void OnAfterDeserialization ()
        {
            UpdateChildren ();
        }

        public void UpdateChildren ()
        {
            if (languages != null)
            {
                foreach (var kvp in languages)
                {
                    if (kvp.Value != null)
                        kvp.Value.language = kvp.Key;
                }
            }
        }
        
        #region Editor
        #if UNITY_EDITOR

        private bool init = false;
        private IEnumerable<string> GetLanguageKeys ()
        {
            if (!init)
            {
                init = true;
                DataManagerText.RefreshLocalizationListBuiltin ();
            }

            return DataManagerText.GetLocalizationLanguageKeysBuiltin ();
        }
        
        public void SaveToMod (DataContainerModData modData)
        {
            if (modData == null)
            {
                Debug.LogWarning ("Can't save text edits, parent mod not provided");
                return;
            }
            
            if (languages == null || languages.Count == 0)
                return;

            bool anyDataPresent = false;
            foreach (var kvp in languages)
            {
                var language = kvp.Value;
                if (language == null || language.sectors == null)
                    continue;

                foreach (var kvp2 in language.sectors)
                {
                    var sector = kvp2.Value;
                    if (sector == null || sector.text == null)
                        continue;

                    anyDataPresent = true;
                    break;
                }
                
                if (anyDataPresent)
                    break;
            }
            
            if (!anyDataPresent)
                return;

            var rootPath = modData.GetModPathProject ();
            if (!Directory.Exists (rootPath))
            {
                Debug.LogWarning ($"Can't save text edits, mod {modData.id} directory doesn't exist: {rootPath}");
                return;
            }

            var editPath = DataPathHelper.GetCombinedCleanPath (rootPath, "LocalizationEdits");
            UtilitiesYAML.PrepareClearDirectory (editPath, false, false);
            Debug.Log ($"Exporting text edits for mod {modData.id} to {editPath}");

            int e = -1;
            foreach (var kvp in languages)
            {
                var languageKey = kvp.Key;
                var language = kvp.Value;
                if (language == null || language.sectors == null)
                    continue;

                var languagePath = DataPathHelper.GetCombinedCleanPath (editPath, languageKey);
                foreach (var kvp2 in language.sectors)
                {
                    var sectorKey = kvp2.Key;
                    var sector = kvp2.Value;
                    if (sector == null || sector.text == null)
                        continue;

                    e += 1;
                    var sectorPath = DataPathHelper.GetCombinedCleanPath (languagePath, sectorKey) + ".yaml";
                    var editsSaved = new SortedDictionary<string, string> ();
                    var config = new ModLocalizationEdit { edits = editsSaved };

                    foreach (var kvp3 in sector.text)
                    {
                        var textKey = kvp3.Key;
                        var textOverride = kvp3.Value?.text;
                        if (!string.IsNullOrEmpty (textKey) && !string.IsNullOrEmpty (textOverride))
                            editsSaved.Add (textKey, textOverride);
                    }

                    UtilitiesYAML.SaveToFile (sectorPath, config);
                    Debug.Log ($"Text edit {e} {languageKey}/{sectorKey}: \n- {sectorPath}");
                }
            }
        }

        [PropertyOrder (-1)]
        [Button ("Load from files"), ButtonGroup]
        private void LoadFromModSelected ()
        {
            if (DataManagerMod.modSelected != null)
                LoadFromMod (DataManagerMod.modSelected);
        }
        
        [PropertyOrder (-1)]
        [Button ("Apply to Configs"), ButtonGroup]
        private void ApplyToConfigs ()
        {
            if (DataManagerMod.modSelected != null)
                ModTextHelper.ApplyTextChangesToConfigs (DataManagerMod.modSelected);
        }

        // [Button ("Load from folder")]
        public static void LoadFromMod (DataContainerModData modData, bool showWarning = true, string pathCustom = null)
        {
            if (modData == null)
            {
                Debug.LogWarning ("Can't load text edits, parent mod not provided");
                return;
            }

            var rootPath = modData.GetModPathProject ();
            var pathImport = DataPathHelper.GetCombinedCleanPath (rootPath, DataContainerModData.localizationEditsFolderName);
            
            if (!string.IsNullOrEmpty (pathCustom))
                pathImport = DataPathHelper.GetCleanPath (pathCustom);
            
            DirectoryInfo dirImport = new DirectoryInfo (pathImport);
            if (!dirImport.Exists)
            {
                Debug.LogWarning ($"Can't load config edits, import path {pathImport} doesn't exist");
                return;
            }
            
            FileInfo[] filesTextEdits = dirImport.GetFiles ("*.yaml", SearchOption.AllDirectories);
            if (filesTextEdits.Length == 0)
            {
                Debug.LogWarning ($"Can't load config edits, import path {pathImport} contains no .yaml files");
                return;
            }
            
            if (showWarning)
            {
                if (!EditorUtility.DisplayDialog
                    (
                        "Import LocalizationEdits?",
                        $"Would you like to text edits into the selected mod project (ID {modData.id})? The imported edits might overwrite existing edits." +
                        $"\n\nFrom folder: \n{pathImport}",
                        "Import LocalizationEdits",
                        "Cancel")
                   )
                {
                    return;
                }
            }

            for (int i = 0; i < filesTextEdits.Length; ++i)
            {
                var filePath = DataPathHelper.GetCleanPath (filesTextEdits[i].FullName);
                var filePathTrimmed = filePath.Replace (pathImport, string.Empty);

                if (filePathTrimmed.StartsWith ("/"))
                    filePathTrimmed = filePathTrimmed.Substring (1, filePathTrimmed.Length - 1);

                if (filePathTrimmed.EndsWith (".yaml"))
                    filePathTrimmed = filePathTrimmed.Replace (".yaml", string.Empty);
                
                var split = filePathTrimmed.Split ('/');
                if (split.Length != 2)
                {
                    Debug.LogWarning ($"Skipping file {filePathTrimmed}: wrong hierarchy\n{filePath}\n{pathImport}");
                    continue;
                }

                var editsSerialized = UtilitiesYAML.LoadDataFromFile<ModLocalizationEdit> (filePath, appendApplicationPath: false);
                if (editsSerialized == null || editsSerialized.edits == null || editsSerialized.edits.Count == 0)
                {
                    Debug.LogWarning ($"Skipping file {filePathTrimmed}: can't be deserialized");
                    continue;
                }
                
                var languageKey = split[0];
                var sectorKey = split[1];
                Debug.Log ($"Edit {i} | Language: {languageKey} | Sector: {sectorKey} | Edits: {editsSerialized.edits.ToStringFormattedKeys ()}");

                if (modData.textEdits == null)
                    modData.textEdits = new ModConfigLocEdit ();

                if (modData.textEdits.languages == null)
                    modData.textEdits.languages = new SortedDictionary<string, ModConfigLocEditLanguage> ();

                if (!modData.textEdits.languages.ContainsKey (languageKey))
                    modData.textEdits.languages.Add (languageKey, new ModConfigLocEditLanguage ());
                            
                var language = modData.textEdits.languages[languageKey];
                if (language == null)
                {
                    language = new ModConfigLocEditLanguage ();
                    modData.textEdits.languages[languageKey] = language;
                }
                
                if (language.sectors == null)
                    language.sectors = new SortedDictionary<string, ModConfigLocSector> ();
                            
                if (!language.sectors.ContainsKey (sectorKey))
                    language.sectors.Add (sectorKey, new ModConfigLocSector ());
                            
                var sector = language.sectors[sectorKey];
                if (sector.text == null)
                    sector.text = new SortedDictionary<string, ModConfigLocText> ();
                else
                    sector.text.Clear ();

                foreach (var kvp in editsSerialized.edits)
                {
                    if (!string.IsNullOrEmpty (kvp.Key))
                    {
                        var editNew = new ModConfigLocText ();
                        editNew.text = kvp.Value;
                        sector.text.Add (kvp.Key, editNew);
                    }
                }
            }
            
            if (modData.textEdits != null)
                modData.textEdits.UpdateChildren ();
        }

        #endif
        #endregion
    }
    
    [HideDuplicateReferenceBox]
    public class ModConfigLocEditLanguage
    {
        [InfoBox ("$hintLocked", VisibleIf = nameof(IsEntryLocked))]
        [DisableIf (nameof(IsEntryLocked))]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, ModConfigLocSector> sectors = new SortedDictionary<string, ModConfigLocSector> ();

        [YamlIgnore, HideInInspector]
        public string language;
        
        private string hintLocked = "These text edits can't be modified and are generated automatically when you modify text in databases and press \"Save Text\" in their inspectors.";
        private bool IsEntryLocked => !string.IsNullOrEmpty (language) && string.Equals (language, "English", StringComparison.Ordinal);
    }
    
    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class ModConfigLocSector
    {
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, ModConfigLocText> text = new SortedDictionary<string, ModConfigLocText> ();
    }

    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class ModConfigLocText
    {
        [TextArea (1, 10)]
        [SuffixLabel ("$GetSuffix", true)]
        [HideLabel]
        public string text;

        [TextArea (1, 10), GUIColor (nameof(textSourceColor))]
        [SuffixLabel ("Original", true)]
        [ShowIf (nameof (IsOverride))]
        [HideLabel, ReadOnly]
        public string textSource;

        private Color textSourceColor = new Color (0.8f, 0.75f, 0.7f, 1f);
        private bool IsOverride => !string.IsNullOrEmpty (textSource);
        private string GetSuffix => IsOverride ? "Override" : "New";
    }

    public static class ModTextHelper
    {
        public static void GenerateTextChanges (DataContainerModData modData)
        {
            #if UNITY_EDITOR
            bool modConfigsUsed =
                modData != null && 
                modData.hasProjectFolder && 
                Directory.Exists (modData.GetModPathConfigs ());
            
            if (!modConfigsUsed)
                return;
            
            // Load mod library
            DataManagerText.LoadLibrary ();
            
            var sectorsWorking = DataManagerText.libraryData.sectors;
            if (sectorsWorking == null)
            {
                Debug.LogWarning ("Can't generate text edits: mod text library not found");
                return;
            }
            
            // Load library from version appropriate location, without overwriting main library in memory
            var pathSDK = DataPathHelper.GetApplicationFolder ();
            var pathSDKConfigs = DataPathHelper.GetCombinedCleanPath (pathSDK, "Configs");
            
            bool modConfigsVersioned = !string.IsNullOrEmpty (modData.configsVersion?.version);
            if (modConfigsVersioned && !string.Equals (modData.configsVersion.version, ConfigsVersion.versionExpected))
            {
                var ver = modData.configsVersion.version;
                var pathSDKConfigsFolderOld = $"ConfigsOld/{ver}";
                var pathSDKConfigsOld = DataPathHelper.GetCombinedCleanPath (pathSDK, pathSDKConfigsFolderOld);
                DirectoryInfo dirSDKConfigsOld = new DirectoryInfo (pathSDKConfigsOld);
                if (dirSDKConfigsOld.Exists)
                {
                    Debug.LogWarning ($"Mod Configs folder version is {ver} instead of {ConfigsVersion.versionExpected}, falling back to reading the old configs folder {pathSDKConfigsOld}");
                    pathSDKConfigs = pathSDKConfigsOld;
                }
            }
            
            var pathSDKLibrary = DataPathHelper.GetCombinedCleanPath (pathSDKConfigs, "TextLibrary");
            var dirSDKLibrary = new DirectoryInfo (pathSDKLibrary);
            if (!dirSDKLibrary.Exists)
            {
                Debug.Log ($"Can't generate text changes: failed to find SDK text library folder at {pathSDKLibrary}");
                return;
            }
            
            var librarySource = new DataContainerTextLibrary ();
            librarySource.core = UtilitiesYAML.LoadDataFromFile<DataContainerTextLibraryCore> (pathSDKLibrary, "core.yaml", appendApplicationPath: false);
            librarySource.sectors = UtilitiesYAML.LoadDecomposedDictionary<DataContainerTextSectorMain> (pathSDKLibrary + "/Sectors", appendApplicationPath: false);
            librarySource.OnAfterDeserialization ();

            var sectorsSource = librarySource.sectors;
            if (sectorsSource == null)
            {
                Debug.LogWarning ("Can't generate text edits: SDK text library not found");
                return;
            }

            bool rootInitialized = false;
            ModConfigLocEditLanguage languageEn = null;
            
            Debug.Log ($"Iterating over {sectorsSource.Count} text sectors to find text changes...");
            foreach (var kvp in sectorsSource)
            {
                var sectorKey = kvp.Key;
                var sectorSource = kvp.Value;
                if (sectorSource == null || sectorSource.entries == null)
                {
                    Debug.LogWarning ($"Skipping comparison of sector {sectorKey}, no entries in the source text library");
                    continue;
                }
                
                var sectorWorkingFound = sectorsWorking.TryGetValue (sectorKey, out var sectorWorking);
                if (!sectorWorkingFound || sectorWorking.entries == null)
                {
                    Debug.LogWarning ($"Skipping comparison of sector {sectorKey}, it couldn't be found in the mod text library");
                    continue;
                }
                
                int textEditsCount = 0;
                SortedDictionary<string, ModConfigLocText> textEditsCollection = null;
                
                foreach (var kvp2 in sectorWorking.entries)
                {
                    var textKey = kvp2.Key;
                    string textWorking = kvp2.Value.text;

                    string textSource = null;
                    if (sectorSource.entries.TryGetValue (textKey, out var textSourceEntry))
                        textSource = textSourceEntry.text;

                    if (!string.Equals (textWorking, textSource))
                    {
                        textEditsCount += 1;
                        Debug.Log ($"Text change {textEditsCount}: {sectorKey}/{textKey}");
                        
                        // Initialize the English text edits and/or reset them
                        if (textEditsCollection == null)
                        {
                            if (!rootInitialized)
                            {
                                // Root is initialized this late in case we never encounter an actual edit
                                rootInitialized = true;
                                
                                if (modData.textEdits == null)
                                    modData.textEdits = new ModConfigLocEdit ();

                                if (modData.textEdits.languages == null)
                                    modData.textEdits.languages = new SortedDictionary<string, ModConfigLocEditLanguage> ();

                                var languageKey = "English";
                                if (!modData.textEdits.languages.TryGetValue (languageKey, out languageEn))
                                {
                                    languageEn = new ModConfigLocEditLanguage ();
                                    modData.textEdits.languages.Add (languageKey, languageEn);
                                }
                            
                                languageEn = modData.textEdits.languages[languageKey];
                                if (languageEn.sectors == null)
                                    languageEn.sectors = new SortedDictionary<string, ModConfigLocSector> ();
                                else
                                    languageEn.sectors.Clear ();
                            }
                            
                            if (!languageEn.sectors.TryGetValue (sectorKey, out var sector))
                            {
                                sector = new ModConfigLocSector ();
                                languageEn.sectors.Add (sectorKey, sector);
                            }
                            
                            if (sector.text == null)
                                sector.text = new SortedDictionary<string, ModConfigLocText> ();
                            else
                                sector.text.Clear ();

                            textEditsCollection = sector.text;
                            textEditsCollection.Clear ();
                            modData.textEdits.UpdateChildren ();
                        }

                        textEditsCollection[textKey] = new ModConfigLocText
                        {
                            text = textWorking,
                            textSource = textSource
                        };
                    }
                }
            }
            
            if (modData.textEdits != null)
            {
                bool anyDataPresent = false;
                if (modData.textEdits.languages != null)
                {
                    foreach (var kvp in modData.textEdits.languages)
                    {
                        var language1 = kvp.Value;
                        if (language1 == null || language1.sectors == null)
                            continue;

                        foreach (var kvp2 in language1.sectors)
                        {
                            var sector = kvp2.Value;
                            if (sector == null || sector.text == null)
                                continue;

                            anyDataPresent = true;
                            break;
                        }

                        if (anyDataPresent)
                            break;
                    }
                }
                
                if (!anyDataPresent)
                {
                    Debug.Log ($"No text edits detected, removing text edit object...");
                    modData.textEdits = null;
                }
                else
                    modData.textEdits.UpdateChildren ();
            }
            
            // DataManagerMod.SaveMod (mod);
            #endif
        }
        
        public static void ApplyTextChangesToConfigs (DataContainerModData modData)
        {
            #if UNITY_EDITOR
            bool modConfigsUsed =
                modData != null && 
                modData.hasProjectFolder && 
                Directory.Exists (modData.GetModPathConfigs ());
            
            if (!modConfigsUsed)
            {
                Debug.LogWarning ("Can't apply text edits: mod config folder not found");
                return;
            }
            
            if (modData.textEdits == null || modData.textEdits.languages == null)
            {
                Debug.LogWarning ("Can't apply text edits: mod config has no edits");
                return;
            }
            
            modData.RefreshConfigsVersion ();
            DataContainerModData.selectedMod = modData;
            Debug.Log ($"Config editing entered with mod {modData.id}");
            
            DataManagerText.LoadLibrary ();
            
            var sectorsWorking = DataManagerText.libraryData.sectors;
            if (sectorsWorking == null)
            {
                Debug.LogWarning ("Can't apply text edits: mod text library not found");
                return;
            }

            foreach (var kvp in modData.textEdits.languages)
            {
                if (!string.Equals (kvp.Key, "English"))
                {
                    Debug.LogWarning ($"Skipping language {kvp.Key}, forced import into Configs not supported for now...");
                    continue;
                }

                var language = kvp.Value;
                if (language == null || language.sectors == null)
                    continue;

                foreach (var kvp2 in language.sectors)
                {
                    var sectorKey = kvp2.Key;
                    var sectorEdits = kvp2.Value;
                    if (sectorEdits == null || sectorEdits.text == null || sectorEdits.text.Count == 0)
                        continue;

                    foreach (var kvp3 in sectorEdits.text)
                    {
                        var entryKey = kvp3.Key;
                        var entryEdit = kvp3.Value;
                        if (entryEdit == null || entryEdit.text == null)
                            continue;
                        
                        var sectorWorking = sectorsWorking.TryGetValue (sectorKey, out var v) ? v : null;
                        if (sectorWorking == null)
                        {
                            Debug.LogWarning ($"Failed to apply text edit to key {sectorKey}/{entryKey}: sector not found in working library");
                            continue;
                        }

                        var entriesWorking = sectorWorking.entries;
                        if (entriesWorking == null)
                            entriesWorking = new SortedDictionary<string, DataBlockTextEntryMain> ();
                        
                        var entryWorking = entriesWorking.TryGetValue (entryKey, out var v2) ? v2 : null;
                        if (entryWorking == null)
                        {
                            entryWorking = new DataBlockTextEntryMain ();
                            entryWorking.temp = false;
                            entryWorking.text = entryEdit.text;
                        }
                            
                        Debug.LogWarning ($"Applied text edit to key {sectorKey}/{entryKey}:\n{entryEdit.text}");
                        entryWorking.text = entryEdit.text;
                        entriesWorking[entryKey] = entryWorking;
                    }
                }
            }
            
            DataManagerText.SaveLibrary ();
            
            // DataManagerMod.SaveMod (mod);
            #endif
        }
    }
}
