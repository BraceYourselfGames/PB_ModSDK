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
    public class OverworldEntityProvinceMemory : IOverworldTargetedFunction
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent != null)
                provincePersistent.ApplyEventMemoryChangeFloat (changes);

            #endif
        }
    }

    [Serializable]
    public class OverworldEntityProvinceCapture : IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent != null)
                DataHelperProvince.CaptureProvince (provincePersistent, false);

            #endif
        }
    }
}