using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockGameSettingHeader
    {
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel]
        public string textHeader;
    }
    
    [Serializable]
    public class DataContainerGameSetting : DataContainerWithText
    {
        public bool hidden = false;
        public bool instant = false;
        public int priority = 0;
        
        public SettingMode mode = SettingMode.String;
        
        [ValueDropdown ("@DataLinkerGameSettingGlobals.data.groups.Keys")]
        public string group;

        public DataBlockGameSettingHeader header;
        
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel, Multiline (2)]
        public string textName;
        
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel, TextArea]
        public string textDesc;

        public string valueDefault;
        
        [ShowIf ("IsSlider")]
        public string valueMin;
        
        [ShowIf ("IsSlider")]
        public string valueMax;

        [ShowIf ("IsSlider")]
        public string valueSuffix;
        
        [ShowIf ("IsSlider")]
        public string valueFormat = "F0";
        
        [ShowIf ("IsSlider")]
        public float valueVisualMultiplier = 1f;

        [ShowIf ("AreLevelsAvailable")]
        [PropertyTooltip ("If levels aren't used for input, values will be directly parsed from config file, otherwise, config file value will be interpreted as key to levels collection")]
        public bool levelsUsedForInput = true;
        
        [ShowIf ("AreLevelsAvailable")]
        [PropertyTooltip ("Whether to show level selector in UI. This is separate from whether levels are used as inputs: you could have 1920x1080 as value in config file and still have levels for easy in-UI selections")]
        public bool levelsUsedForUI = true;
        
        [ShowIf ("AreLevelsAvailable")]
        public bool levelsLocalized = true;

        [DropdownReference]
        public string entKey;

        public SortedDictionary<string, OptionLevel> levels;
        
        public SortedDictionary<string, string> upgrades;
        

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            #if UNITY_EDITOR
            if (levels != null)
            {
                foreach (var kvp in levels)
                {
                    var level = kvp.Value;
                    level.parent = this;
                }
            }
            #endif
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.uiSettings, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.uiSettings, $"{key}__text");
            
            if (header != null)
                header.textHeader = DataManagerText.GetText (TextLibs.uiSettings, $"{key}__prefix");

            if (levels != null)
            {
                foreach (var kvp in levels)
                {
                    var levelKey = kvp.Key;
                    var level = kvp.Value;

                    if (levelsLocalized)
                    {
                        level.textName = DataManagerText.GetText
                        (
                            TextLibs.uiSettings,
                            !string.IsNullOrEmpty (level.textCustom) ? $"{key}__lv_{level.textCustom}" : $"{key}__lv_{levelKey}"
                        );
                    }
                    else
                    {
                        level.textName = level.textCustom;
                    }
                }
            }
        }

        #if UNITY_EDITOR 

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.uiSettings, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.uiSettings, $"{key}__text", textDesc);
            
            if (header != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.uiSettings, $"{key}__prefix", header.textHeader);

            if (levels != null)
            {
                foreach (var kvp in levels)
                {
                    var levelKey = kvp.Key;
                    var level = kvp.Value;

                    if (levelsLocalized)
                    {
                        DataManagerText.TryAddingTextToLibrary 
                        (
                            TextLibs.uiSettings, 
                            !string.IsNullOrEmpty (level.textCustom) ? $"{key}__lv_{level.textCustom}" : $"{key}__lv_{levelKey}", 
                            level.textName
                        );
                    }
                }
            }
        }

        private bool IsSlider => mode == SettingMode.Slider;
        private bool AreLevelsAvailable => levels != null && levels.Count > 0;
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerGameSetting () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }
}

