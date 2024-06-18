using System;
using System.Collections.Generic;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerWorkshopVariants : DataContainerWithText
    {
        public SortedDictionary<string, DataBlockWorkshopProjectVariant> variants = new SortedDictionary<string, DataBlockWorkshopProjectVariant> ();
        
        public override void ResolveText ()
        {
            if (variants == null)
                return;

            foreach (var kvp in variants)
            {
                var variantKey = kvp.Key;
                var variant = kvp.Value;
                
                if (variant == null || string.IsNullOrEmpty (variantKey))
                    continue;
                
                variant.textName = DataManagerText.GetText (TextLibs.workshopVariants, $"{key}__{variantKey}_name");
                variant.textDesc = DataManagerText.GetText (TextLibs.workshopVariants, $"{key}__{variantKey}_text");
            }
            
            
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;
            
            if (variants == null)
                return;

            foreach (var kvp in variants)
            {
                var variantKey = kvp.Key;
                var variant = kvp.Value;
                
                if (variant == null || string.IsNullOrEmpty (variantKey))
                    continue;
                
                DataManagerText.TryAddingTextToLibrary (TextLibs.workshopVariants, $"{key}__{variantKey}_name", variant.textName);
                DataManagerText.TryAddingTextToLibrary (TextLibs.workshopVariants, $"{key}__{variantKey}_text", variant.textDesc);
            }
        }
        
        #endif
    }
}

