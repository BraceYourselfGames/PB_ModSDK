using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldIntValueProvinceCompletionPresetKey : IOverworldIntValueFunction
    {
        [ValueDropdown (nameof(GetKeys))]
        public string key;
        
        private IEnumerable<string> GetKeys () => DataMultiLinkerOverworldPointPreset.GetKeys ();

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provinceOverworld = IDUtility.GetOverworldEntity (provinceKey);
            if (provinceOverworld == null || !provinceOverworld.hasProvincePointCompletions)
                return 0;

            var c = provinceOverworld.provincePointCompletions.keys;
            return c != null && !string.IsNullOrEmpty (key) && c.TryGetValue (key, out var v) ? v : 0;

            #endif
            
            return 0;
        }
    }
    
    [Serializable]
    public class OverworldIntValueProvinceCompletionPresetTag : IOverworldIntValueFunction
    {
        [ValueDropdown (nameof(GetTags))]
        public string tag;
        
        private IEnumerable<string> GetTags ()
        {
            var data = DataMultiLinkerOverworldPointPreset.data;
            return DataMultiLinkerOverworldPointPreset.tags;
        }

        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provinceOverworld = IDUtility.GetOverworldEntity (provinceKey);
            if (provinceOverworld == null || !provinceOverworld.hasProvincePointCompletions)
                return 0;

            var c = provinceOverworld.provincePointCompletions.tags;
            return c != null && !string.IsNullOrEmpty (tag) && c.TryGetValue (tag, out var v) ? v : 0;

            #endif
            
            return 0;
        }
    }
    
    
    [Serializable]
    public class OverworldIntValueProvinceMemory : IOverworldIntValueFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.GetKeys ()")]
        public string key;
        
        public int Resolve (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent == null)
                return 0;

            bool found = provincePersistent.TryGetMemoryRounded (key, out var value);
            return found ? value : 0;

            #endif
            
            return 0;
        }
    }
}