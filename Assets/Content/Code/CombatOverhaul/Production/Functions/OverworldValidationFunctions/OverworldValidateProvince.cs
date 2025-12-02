using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateProvinceMemory : IOverworldEntityValidationFunction
    {
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockOverworldMemoryCheckGroup check = new DataBlockOverworldMemoryCheckGroup ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK
            
            if (check == null)
                return false;
            
            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provincePersistent = IDUtility.GetPersistentEntity (provinceKey);
            if (provincePersistent == null)
                return false;

            bool passed = check.IsPassed (entityPersistent);
            return passed;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateProvincePointUseByKey : OverworldValidatePointUseByKey
    {
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (keyChecked))
                return false;
            
            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provinceOverworld = IDUtility.GetOverworldEntity (provinceKey);
            if (provinceOverworld == null)
                return false;

            var spawnsByKey = provinceOverworld.hasProvincePointSpawns ? provinceOverworld.provincePointSpawns.keys : null;
            var completionsByKey = provinceOverworld.hasProvincePointCompletions ? provinceOverworld.provincePointCompletions.keys : null;
            var completionRecords = provinceOverworld.hasProvincePointRecords ? provinceOverworld.provincePointRecords.s : null;
            
            if (!IsValidInternal (spawnsByKey, completionsByKey, completionRecords))
                return false;

            // Skip presets that can't coexist in more than N instances
            if (instanceLimit != null)
            {
                var overworld = Contexts.sharedInstance.overworld;
                var instances = overworld.GetEntitiesWithDataKeyPointPreset (keyChecked);

                int instanceCount = 0;
                foreach (var entityActive in instances)
                {
                    if (entityActive.isDestroyed)
                        continue;

                    var provinceKeyOnEntity = DataHelperProvince.GetProvinceKeyAtEntity (entityActive);
                    if (!string.Equals (provinceKey, provinceKeyOnEntity, StringComparison.Ordinal))
                        continue;

                    var entityActivePersistent = IDUtility.GetLinkedPersistentEntity (entityActive);
                    if (entityActivePersistent != null && entityActivePersistent.hasFaction && string.Equals (entityActivePersistent.faction.s, Factions.player, StringComparison.Ordinal))
                        continue;
                    
                    instanceCount += 1;
                }

                if (instanceCount >= instanceLimit.i)
                {
                    // Debug.Log ($"Disqualified due to instance count for key {keyChecked} reaching limit {instanceCount}");
                    return false;
                }
            }
            
            return true;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateProvincePointUseByTag : OverworldValidatePointUseByTag
    {
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (tagChecked))
                return false;
            
            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            var provinceKey = DataHelperProvince.GetProvinceKeyAtEntity (entityOverworld);
            var provinceOverworld = IDUtility.GetOverworldEntity (provinceKey);
            if (provinceOverworld == null)
                return false;

            var spawnsByTag = provinceOverworld.hasProvincePointSpawns ? provinceOverworld.provincePointSpawns.tags : null;
            var completionsByTag = provinceOverworld.hasProvincePointCompletions ? provinceOverworld.provincePointCompletions.tags : null;
            var completionRecords = provinceOverworld.hasProvincePointRecords ? provinceOverworld.provincePointRecords.s : null;
            
            if (!IsValidInternal (spawnsByTag, completionsByTag, completionRecords))
                return false;
            
            // Skip presets that can't coexist in more than N instances
            if (instanceLimit != null)
            {
                int instanceCountTagged = 0;
                var entities = OverworldPointUtility.GetActivePoints (false, false);
                foreach (var entityActive in entities)
                {
                    if (entityActive.isDestroyed)
                        continue;

                    var provinceKeyOnEntity = DataHelperProvince.GetProvinceKeyAtEntity (entityActive);
                    if (!string.Equals (provinceKey, provinceKeyOnEntity, StringComparison.Ordinal))
                        continue;

                    var presetActive = entityActive.dataLinkPointPreset.data;
                    if (presetActive.tagsProc == null || !presetActive.tagsProc.Contains (tagChecked))
                        continue;
                    
                    var entityActivePersistent = IDUtility.GetLinkedPersistentEntity (entityActive);
                    if (entityActivePersistent != null && entityActivePersistent.hasFaction && string.Equals (entityActivePersistent.faction.s, Factions.player, StringComparison.Ordinal))
                        continue;

                    instanceCountTagged += 1;
                }

                if (instanceCountTagged >= instanceLimit.i)
                {
                    // Debug.Log ($"Disqualified due to instance count of tag {tagChecked} reaching limit {instanceCountTagged}");
                    return false;
                }
            }
            
            return true;

            #else
            return false;
            #endif
        }
    }
}