using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockFactionPartFilters
    {
        [DictionaryDrawerSettings (KeyLabel = "Socket tag", ValueLabel = "Filters (by priority)")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.SocketTag)]
        public SortedDictionary<string, List<DataBlockPartTagFilter>> filters;
    }

    public class DataBlockCodexUnlock
    {
        [ValueDropdown("@DataMultiLinkerCodex.GetKeys ()")]
        public string key;

        public void Run ()
        {
            #if !PB_MODSDK
            
            CodexUtility.TryUnlockEntry (key);
            
            #endif
        }
    }

    [Serializable]
    public class DataContainerOverworldFactionBranch : DataContainerWithText
    {
        [YamlIgnore]
        [LabelText ("Header / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [TextArea][HideLabel]
        public string textDesc;

        public string liveryPrefix;

        public bool training;
        
        [DropdownReference]
        public List<DataBlockCodexUnlock> codexUnlocksBriefing;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.UnitGroupTag)]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> unitGroupTags;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.UnitGroupTag)]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> unitPresetTags;

        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, KeyLabel = "Unit class", ValueLabel = "Filters")]
        [DictionaryKeyDropdown ("@DataHelperUnitEquipment.GetClassTags ()")]
        public SortedDictionary<string, DataBlockFactionPartFilters> unitPartFilters;

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.overworldBranches, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.overworldBranches, $"{key}__text");
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            if (unitPartFilters != null)
            {
                foreach (var kvp1 in unitPartFilters)
                {
                    var pf = kvp1.Value;
                    if (pf == null || pf.filters == null)
                        continue;

                    var partTagPreferences = pf.filters;
                    foreach (var kvp in partTagPreferences)
                    {
                        if (kvp.Value != null)
                        {
                            foreach (var entry in kvp.Value)
                            {
                                if (entry != null)
                                {
                                    entry.parentSocketGroup = kvp.Key;
                                    entry.parentFactionBranch = key;
                                    entry.parentUnitBlueprint = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldBranches, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldBranches, $"{key}__text", textDesc);
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldFactionBranch () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}

