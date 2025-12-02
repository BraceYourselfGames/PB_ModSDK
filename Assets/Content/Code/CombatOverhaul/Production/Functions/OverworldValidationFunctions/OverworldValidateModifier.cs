using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateModifier : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceModifier.data.Keys")]
        public string key = string.Empty;
        
        protected override string GetLabel () => present ? "Should be present" : "Should be absent";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var provinceActive = DataHelperProvince.GetProvinceOverworldActive ();
            var modifiers = provinceActive != null && provinceActive.hasProvinceModifiers ? provinceActive.provinceModifiers.keys : null;
            var modifierPresent = modifiers != null && !string.IsNullOrEmpty (key) && modifiers.Contains (key);
            return present == modifierPresent;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateModifierTag : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceModifier.GetTags ()")]
        public string tag = string.Empty;
        
        protected override string GetLabel () => present ? "Should be present" : "Should be absent";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var provinceActive = DataHelperProvince.GetProvinceOverworldActive ();
            var modifiers = provinceActive != null && provinceActive.hasProvinceModifiers ? provinceActive.provinceModifiers.keys : null;
            bool tagPresent = false;
            
            if (modifiers != null)
            {
                foreach (var modifierKey in modifiers)
                {
                    var modifierData = DataMultiLinkerOverworldProvinceModifier.GetEntry (modifierKey, false);
                    if (modifierData == null || modifierData.tags == null)
                        continue;

                    foreach (var tagChecked in modifierData.tags)
                    {
                        if (string.Equals (tagChecked, tag, StringComparison.Ordinal))
                        {
                            tagPresent = true;
                            break;
                        }
                    }

                    if (tagPresent)
                        break;
                }
            }

            return present == tagPresent;

            #else
            return false;
            #endif
        }
    }
}