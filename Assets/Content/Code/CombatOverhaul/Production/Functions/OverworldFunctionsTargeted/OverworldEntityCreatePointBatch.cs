using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityProvinceCreatePointBatch : IOverworldTargetedFunction
    {
        public int pointLimit = 3;
        
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"OverworldEntityProvinceCreatePointBatch\", \"GetTags\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

        private IEnumerable<string> GetTags ()
        {
            var data = DataMultiLinkerOverworldPointPreset.data;
            return DataMultiLinkerOverworldPointPreset.tags;
        }

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent == null)
                return;
            
            OverworldPointUtility.GenerateNewPointBatch (provinceKey, pointLimit, filter);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityProvinceCreatePointFiltered : IOverworldTargetedFunction
    {
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"OverworldEntityProvinceCreatePointFiltered\", \"GetTags\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

        private IEnumerable<string> GetTags ()
        {
            var data = DataMultiLinkerOverworldPointPreset.data;
            return DataMultiLinkerOverworldPointPreset.tags;
        }

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent == null)
                return;

            OverworldPointUtility.GenerateNewPoint (null, provinceKey, tagFilterExternal: filter);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityProvinceCreatePoint : IOverworldTargetedFunction
    {
        [ValueDropdown (nameof(GetKeys))]
        public string key;
        
        private IEnumerable<string> GetKeys () => DataMultiLinkerOverworldPointPreset.GetKeys ();
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent == null)
                return;
            
            OverworldPointUtility.GenerateNewPoint (key, provinceKey);

            #endif
        }
    }
}