using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker]
    public class DataContainerWorkshopCategory : DataContainerWithText
    {
        public int priority;
        
        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;

        [YamlIgnore]
        [TextArea (1, 10)][HideLabel]
        public string textDesc;
        
        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon;
        
        [ValueDropdown ("@DataMultiLinkerWorkshopProject.tags")]
        public HashSet<string> tags = new HashSet<string> ();

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.workshopCategories, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.workshopCategories, $"{key}__text");
        }

        #if UNITY_EDITOR 

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.workshopCategories, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.workshopCategories, $"{key}__text", textDesc);
        }

        #endif
    }
}

