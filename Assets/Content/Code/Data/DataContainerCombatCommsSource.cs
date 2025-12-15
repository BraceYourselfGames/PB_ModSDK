using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataContainerCombatCommsSource : DataContainerWithText
    {
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.CombatComms)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.CombatComms, 128)", false)]
        [HideLabel]
        public string image;

        public bool audioEnemy = false;

        public Color color = new Color (1f, 1f, 1f, 1f);
        
        [YamlIgnore]
        public string text;

        public override void ResolveText ()
        {
            text = DataManagerText.GetText (TextLibs.scenarioComms, $"src_{key}_header");
        }

        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioComms, $"src_{key}_header", text);
        }

        #endif
    }
}

