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

        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
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
        [TextArea (1, 10), HideLabel]
        public string text;
    }

    public static class ModTextHelper
    {
        public static void GenerateTextChangesToSectors (List<string> textSectorKeys)
        {
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
            
            Debug.Log ($"Saving text changes for sectors {textSectorKeys.ToStringFormatted ()}");
            int textEditsCount = 0;
            SortedDictionary<string, ModConfigLocText> textEditsCollection = null;
            
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

                        textEditsCollection[textKey] = new ModConfigLocText { text = textWorking };
                    }
                }
            }

            if (textEditsCount == 0 && mod.textEdits?.languages != null && mod.textEdits.languages.ContainsKey (languageKey))
            {
                Debug.Log ("No text edits found, deleting the English library overrides from the mod config");
                mod.textEdits.languages.Remove (languageKey);
            }
            
            Debug.Log ($"Saving mod {mod.id} in {mod.GetModPathProject ()}");
            DataManagerMod.SaveMod (mod);
        }
    }
}
