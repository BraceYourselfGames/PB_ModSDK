using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [LabelWidth (120f)]
    public class DataContainerUnitRole : DataContainerWithText
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;

        [ShowIf ("textUsed")]
        [LabelText ("Name / Desc.")][YamlIgnore]
        public string textName;

        [ShowIf ("textUsed")]
        [HideLabel, TextArea (1, 10)][YamlIgnore]
        public string textDesc;
        
        public bool textUsed = true;
        public bool selectable = true;
        public bool nameOverride = false;

        public override void ResolveText ()
        {
            if (textUsed)
            {
                textName = DataManagerText.GetText (TextLibs.unitRoles, $"{key}__name", suppressWarning: true);
                textDesc = DataManagerText.GetText (TextLibs.unitRoles, $"{key}__text", suppressWarning: true);
            }
        }

        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (textUsed)
            {
                if (!string.IsNullOrEmpty (textName))
                    DataManagerText.TryAddingTextToLibrary (TextLibs.unitRoles, $"{key}__name", textName);

                if (!string.IsNullOrEmpty (textDesc))
                    DataManagerText.TryAddingTextToLibrary (TextLibs.unitRoles, $"{key}__text", textDesc);
            }
        }

        #endif
        #endregion
    }
}

