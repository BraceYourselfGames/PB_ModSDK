using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class UnitPerformanceGroups
    {
        public const string weight = "weight";
        public const string speed = "speed";
        public const string dash = "dash";
    }

    public class DataBlockUnitWeightClass
    {
        public int stabilityBonus = 0;
        public float concussionMultiplier = 1.5f;
    }
    
    [Serializable, HideReferenceObjectPicker, LabelWidth (160f)]
    public class DataContainerUnitPerformanceClass : DataContainerWithText
    {
        [YamlIgnore, ReadOnly]
        public int index = 0;

        public string textRange;
        
        [LabelText ("Name")][YamlIgnore]
        public string textName;
        
        [LabelText ("Description")]
        public DataBlockStringNonSerializedLong textDesc;

        [ValueDropdown ("GetGroupKeys")]
        public string group = UnitPerformanceGroups.weight;

        public float threshold = 0f;

        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;

        public DataBlockUnitWeightClass valuesWeight;

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitPerformanceClasses, $"{key}_name");
            
            if (textDesc != null)
                textDesc.s = DataManagerText.GetText (TextLibs.unitPerformanceClasses, $"{key}_desc");
        }
        
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.unitPerformanceClasses, $"{key}_name", textName);
            
            if (textDesc != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.unitPerformanceClasses, $"{key}_desc", textDesc.s);
        }

        private IEnumerable<string> GetGroupKeys => FieldReflectionUtility.GetConstantStringFieldValues (typeof (UnitPerformanceGroups));

        #endif
    }
}