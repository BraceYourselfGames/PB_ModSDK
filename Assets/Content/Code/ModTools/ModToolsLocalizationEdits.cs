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
            if (languages == null || languages.Count == 0)
                return;
            
            if (modData == null)
            {
                Debug.LogWarning ("Can't save text edits, parent mod not provided");
                return;
            }

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
        [Button ("Load from files")]
        private void LoadFromModSelected ()
        {
            if (DataManagerMod.modSelected != null)
                LoadFromMod (DataManagerMod.modSelected);
        }

        // [Button ("Load from folder")]
        public void LoadFromMod (DataContainerModData mod)
        {
            if (mod == null)
            {
                Debug.LogWarning ("Can't load text edits, parent mod not provided");
                return;
            }

            var rootPath = mod.GetModPathProject ();
            var editPath = DataPathHelper.GetCombinedCleanPath (rootPath, "LocalizationEdits");

            if (!EditorUtility.DisplayDialog ("Load text edits", $"Are you sure you'd like to load text edits for mod {mod.id}? They might overwrite or replace the text edit you have already entered through the inspector or databases, so only use this option if you are trying to import data from an old non-SDK mod. Path: {editPath}", "Confirm", "Cancel"))
                return;

            if (!Directory.Exists (editPath))
            {
                Debug.LogWarning ($"Can't load text edits, mod {mod.id} doesn't have a text edit folder: {editPath}");
                return;
            }
            
            string[] filePaths = Directory.GetFiles (editPath, "*.yaml", SearchOption.AllDirectories);

            for (int i = 0; i < filePaths.Length; ++i)
            {
                var filePath = DataPathHelper.GetCleanPath (filePaths[i]);
                var filePathTrimmed = filePath.Replace (editPath, string.Empty);

                if (filePathTrimmed.StartsWith ("/"))
                    filePathTrimmed = filePathTrimmed.Substring (1, filePathTrimmed.Length - 1);

                if (filePathTrimmed.EndsWith (".yaml"))
                    filePathTrimmed = filePathTrimmed.Replace (".yaml", string.Empty);
                
                var split = filePathTrimmed.Split ('/');
                if (split.Length != 2)
                {
                    Debug.LogWarning ($"Skipping file {filePathTrimmed}: wrong hierarchy");
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

                if (mod.textEdits == null)
                    mod.textEdits = new ModConfigLocEdit ();

                if (mod.textEdits.languages == null)
                    mod.textEdits.languages = new SortedDictionary<string, ModConfigLocEditLanguage> ();

                if (!mod.textEdits.languages.ContainsKey (languageKey))
                    mod.textEdits.languages.Add (languageKey, new ModConfigLocEditLanguage ());
                            
                var language = mod.textEdits.languages[languageKey];
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
            
            mod.textEdits.UpdateChildren ();
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
        public static void GenerateTextChangesToSectors (List<string> textSectorKeys)
        {
            #if UNITY_EDITOR
            bool modConfigsUsed =
                DataManagerMod.modSelected != null && 
                DataContainerModData.selectedMod != null && 
                DataContainerModData.selectedMod.hasProjectFolder && 
                Directory.Exists (DataContainerModData.selectedMod.GetModPathConfigs ());
            
            if (!modConfigsUsed)
                return;
            
            if (textSectorKeys == null || textSectorKeys.Count == 0)
            {
                Debug.LogWarning ($"Can't process text changes for sectors: no keys received");
                return;
            }
            
            var mod = DataManagerMod.modSelected;
            var languageKey = "English";
            
            foreach (var sectorKey in textSectorKeys)
            {
                var sectorWorkingFound = DataManagerText.libraryData.sectors.TryGetValue (sectorKey, out var sectorWorking);
                if (!sectorWorkingFound || sectorWorking.entries == null)
                {
                    Debug.LogWarning ($"Skipping comparison of sector {sectorKey}, it couldn't be found in the working text library");
                    continue;
                }
                
                var sectorSourceFound = DataManagerText.librarySourceData.sectors.TryGetValue (sectorKey, out var sectorSource);
                if (!sectorSourceFound || sectorSource.entries == null)
                {
                    Debug.LogWarning ($"Skipping comparison of sector {sectorKey}, it couldn't be found in the source text library");
                    continue;
                }
                
                Debug.Log ($"Saving text changes for sectors {textSectorKeys.ToStringFormatted ()}");
                int textEditsCount = 0;
                SortedDictionary<string, ModConfigLocText> textEditsCollection = null;

                foreach (var kvp in sectorWorking.entries)
                {
                    var textKey = kvp.Key;
                    string textWorking = kvp.Value.text;

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
                            if (mod.textEdits == null)
                                mod.textEdits = new ModConfigLocEdit ();

                            if (mod.textEdits.languages == null)
                                mod.textEdits.languages = new SortedDictionary<string, ModConfigLocEditLanguage> ();

                            if (!mod.textEdits.languages.ContainsKey (languageKey))
                                mod.textEdits.languages.Add (languageKey, new ModConfigLocEditLanguage ());
                            
                            var language = mod.textEdits.languages[languageKey];
                            if (language.sectors == null)
                                language.sectors = new SortedDictionary<string, ModConfigLocSector> ();
                            
                            if (!language.sectors.ContainsKey (sectorKey))
                                language.sectors.Add (sectorKey, new ModConfigLocSector ());
                            
                            var sector = language.sectors[sectorKey];
                            if (sector.text == null)
                                sector.text = new SortedDictionary<string, ModConfigLocText> ();
                            else
                                sector.text.Clear ();

                            textEditsCollection = sector.text;
                            textEditsCollection.Clear ();
                            mod.textEdits.UpdateChildren ();
                        }

                        textEditsCollection[textKey] = new ModConfigLocText
                        {
                            text = textWorking,
                            textSource = textSource
                        };
                    }
                }
                
                if 
                (
                    textEditsCount == 0 && 
                    mod.textEdits?.languages != null && 
                    mod.textEdits.languages.TryGetValue (languageKey, out var l) && 
                    l?.sectors != null &&
                    l.sectors.ContainsKey (sectorKey)
                )
                {
                    Debug.Log ($"No text edits found for sector {sectorKey}, deleting the override from the mod config");
                    l.sectors.Remove (sectorKey);
                }
            }
            
            DataManagerMod.SaveMod (mod);
            #endif
        }
    }
}
