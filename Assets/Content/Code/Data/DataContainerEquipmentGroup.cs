using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class EquipmentGroupType
    {
        public const string manufacturer = "manufacturer";
        public const string trait = "trait";
        public const string category = "category";
    }
    
    [Serializable]
    public class DataContainerEquipmentGroup : DataContainerWithText
    {
        public int priority;
        
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (EquipmentGroupType), false)")]
        public string type = EquipmentGroupType.category;
        
        [YamlIgnore, HideLabel]
        public string textName;
        
        [YamlIgnore, HideLabel, TextArea]
        public string textDesc;

        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon;
        
        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string iconSmall;

        public bool visibleInName = true;
        public bool visibleInDesc = true;
        public bool visibleAsPerk = false;
        public bool visibleInFilters = true;
        
        public bool parts = true;
        public bool subsystems = false;

        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tagsPartPreset;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tagsSubsystem;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        public string hardpointSubsystem;

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.equipmentGroups, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.equipmentGroups, $"{key}__text");
        }

        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerEquipmentGroup () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentGroups, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentGroups, $"{key}__text", textDesc);
        }

        #endif
    }
}

