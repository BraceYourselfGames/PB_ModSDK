using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum WorkshopProjectType
    {
        Custom = 0,
        Part = 1,
        Subsystem = 2
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataContainerWorkshopCategory : DataContainerWithText
    {
        public int priority;
        public WorkshopProjectType contentType = WorkshopProjectType.Custom;
        
        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;

        [YamlIgnore]
        [TextArea (1, 10)][HideLabel]
        public string textDesc;
        
        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon;
        
        [DropdownReference]
        public SortedDictionary<string, bool> tagFilter = new SortedDictionary<string, bool> ();

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.workshopCategories, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.workshopCategories, $"{key}__text");
        }

        #if UNITY_EDITOR 
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerWorkshopCategory () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

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

