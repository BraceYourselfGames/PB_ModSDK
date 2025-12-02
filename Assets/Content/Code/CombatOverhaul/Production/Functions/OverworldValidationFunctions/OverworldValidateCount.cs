using System;
using System.Collections.Generic;
using Entitas;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldValidateResolvedInt : DataBlockOverworldResolvedIntCheck, IOverworldGlobalValidationFunction
    {
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var baseOverworld = IDUtility.playerBaseOverworld;
            bool passed = IsPassedWithEntity (baseOverworld);
            return passed;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateCount
    {
        protected virtual IEnumerable<string> GetKeys () => null;
        
        protected bool IsCountValid (SortedDictionary<string, DataBlockOverworldEventSubcheckInt> filter, IDictionary<string, int> counts)
        {
            if (filter == null || filter.Count == 0)
                return false;

            foreach (var kvp in filter)
            {
                var check = kvp.Value;
                if (check == null)
                    continue;
                
                var tag = kvp.Key;
                int count = counts != null && counts.TryGetValue (tag, out var value) ? value : 0;
                bool passed = check.IsPassed (true, count);
                if (!passed)
                    return false;
            }

            return true;
        }
        
        protected bool IsCountValid (SortedDictionary<string, DataBlockOverworldEventSubcheckInt> filter, IDictionary<string, float> counts)
        {
            if (filter == null || filter.Count == 0)
                return false;
            
            foreach (var kvp in filter)
            {
                var check = kvp.Value;
                if (check == null)
                    continue;
                
                var tag = kvp.Key;
                int count = counts != null && counts.TryGetValue (tag, out var value) ? Mathf.RoundToInt (value) : 0;
                bool passed = check.IsPassed (true, count);
                if (!passed)
                    return false;
            }

            return true;
        }
    }
    
    [Serializable]
    public class OverworldValidateCompletionPreset : OverworldValidateCount, IOverworldEntityValidationFunction
    {
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"OverworldValidateCompletionPreset\", \"GetKeys\")")]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckInt> filter = new SortedDictionary<string, DataBlockOverworldEventSubcheckInt> ();
        
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerOverworldPointPreset.GetKeys ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var c = overworld.overworldPointCompletions.keys;
            return IsCountValid (filter, c);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateCompletionScenario : OverworldValidateCount, IOverworldEntityValidationFunction
    {
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"OverworldValidateCompletionScenario\", \"GetKeys\")")]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckInt> filter = new SortedDictionary<string, DataBlockOverworldEventSubcheckInt> ();
        
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerScenario.GetKeys ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var c = overworld.overworldScenarioCompletions.keys;
            return IsCountValid (filter, c);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateCompletionArea : OverworldValidateCount, IOverworldEntityValidationFunction
    {
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"OverworldValidateCompletionArea\", \"GetKeys\")")]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckInt> filter = new SortedDictionary<string, DataBlockOverworldEventSubcheckInt> ();
        
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerCombatArea.GetKeys ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var c = overworld.overworldAreaCompletions.keys;
            return IsCountValid (filter, c);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateMemorySimple : OverworldValidateCount, IOverworldEntityValidationFunction
    {
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"OverworldValidateMemorySimple\", \"GetKeys\")")]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckInt> filter = new SortedDictionary<string, DataBlockOverworldEventSubcheckInt> ();
        
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerOverworldMemory.data.Keys;
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null || !entityPersistent.hasEventMemory)
                return false;

            return IsCountValid (filter, entityPersistent.eventMemory.s);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidatePointUseByKey: IOverworldEntityValidationFunction
    {
        [ValueDropdown("GetKeys")]
        public string keyChecked;
        
        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt spawnLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt instanceLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt completionLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt completionSeparation;

        private IEnumerable<string> GetKeys => DataMultiLinkerOverworldPointPreset.data.Keys;

        #if !PB_MODSDK
        public bool IsValidInternal (Dictionary<string, int> spawnsByKey, Dictionary<string, int> completionsByKey, List<PointCompletionRecord> completionRecords)
        {
            
            // Skip presets that can't spawn more than N times
            if (spawnLimit != null)
            {
                int spawnCount = spawnsByKey != null && spawnsByKey.TryGetValue (keyChecked, out var value) ? value : 0; 
                if (spawnCount >= spawnLimit.i)
                {
                    // Debug.Log ($"Disqualified due to spawn count of key {tagChecked} reaching limit {spawnCount}");
                    return false;
                }
            }
            
            // Skip presets that can't spawn more than N times
            if (completionLimit != null)
            {
                int completionCount = completionsByKey != null && completionsByKey.TryGetValue (keyChecked, out var value) ? value : 0; 
                if (completionCount >= completionLimit.i)
                {
                    // Debug.Log ($"Disqualified due to completion count for key {keyChecked} reaching limit {completionCount}");
                    return false;
                }
            }
            
            // Skip presets that can't spawn if they were completed less than N completions ago. 
            // For instance, if set to "2" and we're checking a preset called "base":
            // ... > patrol (3) > cache (2) > city (1) - no matches up to depth 2, can spawn
            // ... > patrol (3) > base (2) > convoy (1) - matched at depth 2, can't spawn
            if (completionSeparation != null && completionRecords != null && completionRecords.Count > 0)
            {
                bool completionTooRecent = false;
                int completionsChecked = 0;
                int completionSeparationMin = completionSeparation.i;
                
                for (int i = completionRecords.Count - 1; i >= 0; --i)
                {
                    var completion = completionRecords[i];
                    completionsChecked += 1;
                    
                    // Bail if we're deep enough
                    if (completionsChecked > completionSeparationMin) 
                        break;

                    // Bail if we find a match before the break above
                    if (string.Equals (completion.presetKey, keyChecked, StringComparison.Ordinal))
                    {
                        completionTooRecent = true;
                        break;
                    }
                }

                if (completionTooRecent)
                {
                    // Debug.Log ($"Disqualified due to another point with same key {tagChecked} being completed too recently, {completionsChecked} encounters ago (below minimum separation of {completionSeparationMin})");
                    return false;
                }
            }

            return true;
        }
        #endif

        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (keyChecked))
                return false;

            var overworld = Contexts.sharedInstance.overworld;
            var spawnsByKey = overworld.overworldPointSpawns.keys;
            var completionsByKey = overworld.overworldPointCompletions.keys;
            var completionRecords = overworld.overworldPointRecords.s;
            
            if (!IsValidInternal (spawnsByKey, completionsByKey, completionRecords))
                return false;
            
            // Skip presets that can't coexist in more than N instances
            if (instanceLimit != null)
            {
                var instances = overworld.GetEntitiesWithDataKeyPointPreset (keyChecked);

                int instanceCount = 0;
                foreach (var entityActive in instances)
                {
                    if (entityActive.isDestroyed)
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
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public OverworldValidatePointUseByKey () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [Serializable]
    public class OverworldValidatePointUseByTag: IOverworldEntityValidationFunction
    {
        [ValueDropdown ("GetTags")]
        public string tagChecked;
        
        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt spawnLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt instanceLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt completionLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt completionSeparation;
        
        private IEnumerable<string> GetTags ()
        { 
            var data = DataMultiLinkerOverworldPointPreset.data;
            return DataMultiLinkerOverworldPointPreset.tags;
        }

        #if !PB_MODSDK
        public bool IsValidInternal (Dictionary<string, int> spawnsByTag, Dictionary<string, int> completionsByTag, List<PointCompletionRecord> completionRecords)
        {
            if (spawnLimit != null)
            {
                int spawnCount = spawnsByTag != null && spawnsByTag.TryGetValue (tagChecked, out var value) ? value : 0; 
                if (spawnCount >= spawnLimit.i)
                {
                    // Debug.Log ($"Disqualified due to spawn count of key {tagChecked} reaching limit {spawnCount}");
                    return false;
                }
            }
            
            if (completionLimit != null)
            {
                int completionCount = completionsByTag != null && completionsByTag.TryGetValue (tagChecked, out var value) ? value : 0; 
                if (completionCount >= completionLimit.i)
                {
                    // Debug.Log ($"Disqualified due to completion count reaching limit {completionCount}");
                    return false;
                }
            }
            
            // Skip presets that can't spawn if they were completed less than N completions ago. 
            // For instance, if set to "2" and we're checking a preset called "base":
            // ... > patrol (3) > cache (2) > city (1) - no matches up to depth 2, can spawn
            // ... > patrol (3) > base (2) > convoy (1) - matched at depth 2, can't spawn
            if (completionSeparation != null && completionRecords != null && completionRecords.Count > 0)
            {
                bool completionTooRecent = false;
                int completionsChecked = 0;
                int completionSeparationMin = completionSeparation.i;
                
                for (int i = completionRecords.Count - 1; i >= 0; --i)
                {
                    var completion = completionRecords[i];
                    completionsChecked += 1;
                    
                    // Bail if we're deep enough
                    if (completionsChecked > completionSeparationMin) 
                        break;
                    
                    // Commented out fast path using direct key comparison
                    // This fast path is not usable for tags and unnecessary since only <10 depth levels will be checked in practice
                    /*
                    // Bail if we find a match before the break above
                    if (string.Equals (completion.presetKey, presetKey, StringComparison.Ordinal))
                    {
                        completionTooRecent = true;
                        break;
                    }
                    */

                    if (string.IsNullOrEmpty (completion.presetKey))
                        continue;

                    var presetCompleted = DataMultiLinkerOverworldPointPreset.GetEntry (completion.presetKey);
                    if (presetCompleted.tagsProc == null || !presetCompleted.tagsProc.Contains (tagChecked))
                        continue;

                    completionTooRecent = true;
                    break;
                }

                if (completionTooRecent)
                {
                    // Debug.Log ($"Disqualified due to another point with same tag {tagChecked} being completed too recently, {completionsChecked} encounters ago (below minimum separation of {completionSeparationMin})");
                    return false;
                }
            }
            
            return true;
        }
        #endif

        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (tagChecked))
                return false;

            var overworld = Contexts.sharedInstance.overworld;
            var spawnsByTag = overworld.overworldPointSpawns.tags;
            var completionsByTag = overworld.overworldPointCompletions.tags;
            var completionRecords = overworld.overworldPointRecords.s;
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
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public OverworldValidatePointUseByTag () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}