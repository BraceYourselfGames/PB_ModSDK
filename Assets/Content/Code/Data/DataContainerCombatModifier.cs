using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerCombatModifier : DataContainerWithText
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        public Color color;
    
        public bool hidden = false;
        public int priority = 0;

        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel]
        public string textName;
        
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel, TextArea]
        public string textDesc;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.scenarioModifiers, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.scenarioModifiers, $"{key}__text");
        }

        #if UNITY_EDITOR 

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioModifiers, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioModifiers, $"{key}__text", textDesc);
        }

        #endif
    }
}

