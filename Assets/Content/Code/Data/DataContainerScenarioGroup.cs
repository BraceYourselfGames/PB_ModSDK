using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerScenarioGroup : DataContainerWithText
    {
        public bool hidden;
        public int priority;
        public int priorityBriefing;
        public bool tinted = false;
        
        [PropertyRange (0, 360), HorizontalGroup]
        public int hue = 0;

        [YamlIgnore, ShowInInspector, HorizontalGroup (32f), HideLabel]
        private Color color => Color.HSVToRGB (Mathf.Clamp01 (hue / 360f), tinted ? 0.75f : 0.25f, tinted ? 1f : 0.5f);

        [YamlIgnore, HideLabel]
        public string textName;
        
        [YamlIgnore, HideLabel, TextArea]
        public string textDesc;

        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon;
        
        [PropertyOrder (2)]
        [DictionaryKeyDropdown ("@DataMultiLinkerScenario.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.scenarioGroups, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.scenarioGroups, $"{key}__text");
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioGroups, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioGroups, $"{key}__text", textDesc);
        }

        [Button, PropertyOrder (1)]
        private void InjectNameToTags ()
        {
            if (tags == null)
                tags = new SortedDictionary<string, bool> ();
            
            tags[key] = true;
        }

        #endif
    }
}

