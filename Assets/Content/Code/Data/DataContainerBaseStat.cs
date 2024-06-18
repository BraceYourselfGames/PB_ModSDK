using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [LabelWidth (120f)]
    public class DataContainerBaseStat : DataContainerWithText
    {
        [ValueDropdown ("GetGroupKeys")]
        public string group;
        
        public int priority;
        
        [LabelText ("Name / Desc.")][YamlIgnore]
        public string textName;

        [HideLabel, TextArea (1, 10)][YamlIgnore]
        public string textDesc;
        
        public string textSuffix;

        public float valueStart = 0f;
        public string valueFormat = "0.##";
        public bool positive = true;

        [DropdownReference (true)]
        public DataBlockFloat min;
        
        [DropdownReference (true)]
        public DataBlockFloat max;
        
        [DropdownReference (true)]
        public DataBlockFloat valueMultiplierVisual;
        
        [DropdownReference]
        public List<DataBlockBasePartDependency> partDependencies;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.baseStats, $"{key}__name");
            textDesc = DataManagerText.GetText (TextLibs.baseStats, $"{key}__text");
            
        }

        #region Editor
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.baseStats, $"{key}__name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.baseStats, $"{key}__text", textDesc);
        }
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerBaseStat () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private IEnumerable<string> GetGroupKeys => DataMultiLinkerBaseStatGroup.data.Keys;

        #endif

        #endregion
    }
}

