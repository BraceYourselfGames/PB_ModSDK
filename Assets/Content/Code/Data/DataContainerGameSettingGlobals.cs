using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable] 
    public class DataContainerGameSettingGlobals : DataContainerUnique
    {
        public Dictionary<string, DataBlockOptionGroup> groups;

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            if (groups != null)
            {
                foreach (var kvp in groups)
                {
                    var key = kvp.Key;
                    var group = kvp.Value;
                    group.textName = DataManagerText.GetText (TextLibs.uiSettingGroups, $"{key}__header");
                    // group.textDesc = DataManagerText.GetText (TextLibs.uiSettingGroups, $"{key}__text");
                }
            }
        }
    }
    
    [Serializable]
    public class DataBlockOptionGroup
    {
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel]
        public string textName;
        
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel, TextArea]
        public string textDesc;
        
        public string icon;

        public bool hidden = false;
        public bool audioPaused = true;
        public bool backgroundUsed = true;
    }
    
    public enum SettingMode
    {
        Bool,
        Slider,
        String
    }

    [Serializable]
    public class OptionLevel
    {
        [YamlIgnore, ShowIf (DataEditor.textAttrArg)]
        public string textName;

        [LabelText ("@IsTextLibraryUsed ? \"Text Key\" : \"Custom Text\"")]
        public string textCustom;

        public string valueRaw;

        #if UNITY_EDITOR

        [YamlIgnore][HideInInspector]
        public DataContainerGameSetting parent;

        private bool IsTextLibraryUsed => 
            parent != null && parent.levelsLocalized;

        #endif
    }
}

