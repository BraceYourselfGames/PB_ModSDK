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

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldBranches, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldBranches, $"{key}__text", textDesc);
        }
        
        #endif
        #endregion
    }
}

