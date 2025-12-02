using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldModifierAdd : IOverworldFunction
    {
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceModifier.data.Keys")]
        public string key = string.Empty;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var provinceActive = DataHelperProvince.GetProvinceOverworldActive ();
            if (provinceActive != null)
                DataHelperProvince.TryAddingModifierToProvince (provinceActive, key);

            #endif
        }
    }
    
    public class OverworldModifierRemove : IOverworldFunction
    {
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceModifier.data.Keys")]
        public string key = string.Empty;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var provinceActive = DataHelperProvince.GetProvinceOverworldActive ();
            if (provinceActive != null)
                DataHelperProvince.TryRemovingModifierFromProvince (provinceActive, key);

            #endif
        }
    }
}