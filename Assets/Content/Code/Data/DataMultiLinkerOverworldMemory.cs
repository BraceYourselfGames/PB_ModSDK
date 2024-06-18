using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldMemory : DataMultiLinker<DataContainerOverworldMemory>
    {
        public DataMultiLinkerOverworldMemory ()
        {
            textSectorKeys = new List<string> { TextLibs.overworldMemories };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.overworldMemories),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.overworldMemories)
            );
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showLookups = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector]
        public static HashSet<string> keysInt = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static HashSet<string> keysFloat = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static HashSet<string> keysPersistedOnGeneration = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, string> keysOldToNew = new Dictionary<string, string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public static List<string> keysEditableSorted = new List<string> ();

        public static void OnAfterDeserialization ()
        {
            keysFloat.Clear ();
            keysInt.Clear ();
            keysPersistedOnGeneration.Clear ();
            keysOldToNew.Clear ();
            keysEditableSorted.Clear ();

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var config = kvp.Value;

                if (config == null)
                    continue;
                
                keysEditableSorted.Add (key);

                if (config.type == OverworldMemoryType.Int)
                    keysInt.Add (key);
                
                else if (config.type == OverworldMemoryType.Float)
                    keysFloat.Add (key);

                if (!config.discardOnWorldChange) 
                    keysPersistedOnGeneration.Add (key);

                if (!string.IsNullOrEmpty (config.keyOld))
                {
                    if (!keysOldToNew.ContainsKey (config.keyOld))
                        keysOldToNew.Add (config.keyOld, key);
                    else
                        Debug.LogWarning ($"Detected a conflict in overworld memory key renaming history: old key {config.keyOld} is already registered from config {keysOldToNew[config.keyOld]}, but config {key} also claims the same old key");
                }
            }

            #if UNITY_EDITOR
            
            var keysFeatures = FieldReflectionUtility.GetConstantStringFieldValues (typeof (EventMemoryFeatureFlag));
            if (keysFeatures != null)
            {
                foreach (var key in keysFeatures)
                {
                    if (!keysEditableSorted.Contains (key))
                        keysEditableSorted.Add (key);
                }
            }
            
            #endif
            
            keysEditableSorted.Sort ();
        }
        
        
        
        
        private static StringBuilder sb = new StringBuilder ();
        
        private static string GetFixedKey (string input)
        {
            string keyFixed = string.Concat (input.Select ((x,i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
            keyFixed = keyFixed.ToLowerInvariant ();
            keyFixed = keyFixed.Replace ("/", "");

            sb.Clear ();
            foreach (var element in keyFixed.ToCharArray ())
            {
                if (sb.Length > 0)
                {
                    var charLast = sb[sb.Length - 1];
                    if (charLast == '_' && element == '_')
                    {
                        Debug.LogWarning ($"Skipping character {sb.Length} ({sb[sb.Length - 1]}) in {keyFixed}");
                        continue;
                    }
                }
                    
                sb.Append (element);
            }
            keyFixed = sb.ToString ();
            return keyFixed;
        }

        private static void LogMemoryCheckGroup (string context, DataBlockOverworldMemoryCheckGroup group)
        {
            if (group == null)
                return;
            
            var text = group.GetStringAndWarning (out bool warning);
            if (warning)
                Debug.LogWarning ($"{context}\n{text}");
            else
                Debug.Log ($"{context}\n{text}");
        }

        [FoldoutGroup ("Utilities", false)]
        [Button]
        private static void LogPersistedOnWorldReset ()
        {
            var listPreserved = new List<string> ();
            var listDiscarded = new List<string> ();
            
            foreach (var kvp in data)
            {
                var memory = kvp.Value;
                var report = memory.ui != null && !string.IsNullOrEmpty (memory.ui.textName) ? $"{kvp.Key} ({memory.ui.textName})" : kvp.Key;
                
                if (memory.discardOnWorldChange)
                    listDiscarded.Add (report);
                else
                    listPreserved.Add (report);
            }
            
            Debug.Log ($"Preserved memories ({listPreserved.Count}):\n{listPreserved.ToStringFormatted (true, multilinePrefix: "- ")}");
            Debug.Log ($"Discarded memories ({listDiscarded.Count}):\n{listDiscarded.ToStringFormatted (true, multilinePrefix: "- ")}");
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private static void LogMemoryChecks ()
        {
            /*
            foreach (var kvp in DataMultiLinkerScenario.data)
            {
                var scenario = kvp.Value;
                foreach (var kvp2 in scenario.stepsProc)
                {
                    var step = kvp2.Value;
                    if (step.unitGroups == null) 
                        continue;

                    for (int i = 0, count = step.unitGroups.Count; i < count; ++i)
                    {
                        var slot = step.unitGroups[i];
                        if (slot.check == null)
                            continue;
                        
                        if (slot.check.provinceEventMemory != null) 
                            LogMemoryCheckGroup ($"Scenario {kvp.Key} / Step {kvp2.Key} / Slot {i}: province event memory check", slot.check.provinceEventMemory);

                        if (slot.check.selfEventMemory != null)
                            LogMemoryCheckGroup ($"Scenario {kvp.Key} / Step {kvp2.Key} / Slot {i}: self event memory check", slot.check.selfEventMemory);
                        
                        if (slot.check.targetEventMemory != null)
                            LogMemoryCheckGroup ($"Scenario {kvp.Key} / Step {kvp2.Key} / Slot {i}: target event memory check", slot.check.targetEventMemory);
                    }
                }
            }
            */
            
            foreach (var kvp in DataMultiLinkerPilotCheck.data)
            {
                var pilotCheck = kvp.Value;
                if (pilotCheck.check == null || pilotCheck.check.eventMemory == null)
                    continue;
                
                LogMemoryCheckGroup ($"Pilot check {kvp.Key}: event memory check", pilotCheck.check.eventMemory);
            }

            foreach (var kvp in DataMultiLinkerOverworldEvent.data)
            {
                var eventData = kvp.Value;
                
                if (eventData.actorsPilots != null)
                {
                    foreach (var kvp2 in eventData.actorsPilots)
                    {
                        var slot = kvp2.Value;
                        if (slot?.check?.eventMemory != null)
                            LogMemoryCheckGroup ($"Event {kvp.Key} / Pilot actor {kvp.Key}: event memory check", slot.check.eventMemory);
                    }
                }
                
                if (eventData.actorsUnits != null)
                {
                    foreach (var kvp2 in eventData.actorsUnits)
                    {
                        var slot = kvp2.Value;
                        if (slot?.check?.eventMemory != null)
                            LogMemoryCheckGroup ($"Event {kvp.Key} / Unit actor {kvp.Key}: event memory check", slot.check.eventMemory);
                    }
                }
                
                if (eventData.actorsSites != null)
                {
                    foreach (var kvp2 in eventData.actorsSites)
                    {
                        var slot = kvp2.Value;
                        if (slot?.check?.eventMemory != null)
                            LogMemoryCheckGroup ($"Event {kvp.Key} / World actor {kvp.Key}: event memory check", slot.check.eventMemory);
                    }
                }

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var step = kvp2.Value;
                        if (step.check != null)
                        {
                            if (step.check.self?.eventMemory != null)
                                LogMemoryCheckGroup ($"Event {kvp.Key} / Step {kvp2.Key}: self event memory check", step.check.self.eventMemory);

                            if (step.check.target?.eventMemory != null)
                                LogMemoryCheckGroup ($"Event {kvp.Key} / Step {kvp2.Key}: target event memory check", step.check.target.eventMemory);

                            if (step.check.province?.eventMemory != null)
                                LogMemoryCheckGroup ($"Event {kvp.Key} / Step {kvp2.Key}: province event memory check", step.check.province.eventMemory);
                        }
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        if (option.check != null)
                        {
                            if (option.check.self?.eventMemory != null)
                                LogMemoryCheckGroup ($"Event {kvp.Key} / Embedded option {kvp2.Key} / Check: self event memory check", option.check.self.eventMemory);

                            if (option.check.target?.eventMemory != null)
                                LogMemoryCheckGroup ($"Event {kvp.Key} / Embedded option {kvp2.Key} / Check: target event memory check", option.check.target.eventMemory);

                            if (option.check.province?.eventMemory != null)
                                LogMemoryCheckGroup ($"Event {kvp.Key} / Embedded option {kvp2.Key} / Check: province event memory check", option.check.province.eventMemory);
                        }
                    }
                }
            }
            
            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var option = kvp.Value;
                if (option.check != null)
                {
                    if (option.check.self?.eventMemory != null)
                        LogMemoryCheckGroup ($"Option {kvp.Key}: self event memory check", option.check.self.eventMemory);

                    if (option.check.target?.eventMemory != null)
                        LogMemoryCheckGroup ($"Option {kvp.Key}: target event memory check", option.check.target.eventMemory);

                    if (option.check.province?.eventMemory != null)
                        LogMemoryCheckGroup ($"Option {kvp.Key}: province event memory check", option.check.province.eventMemory);
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button]
        public void PrintUnusedMemory ()
        {
            List<string> keyList = new List<string> (data.Keys);
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                foreach (var eventKVP in DataMultiLinkerOverworldEvent.data)
                {
                    var eventValue = eventKVP.Value;
                    if (eventValue?.steps != null)
                    {
                        foreach (var step in eventValue?.steps)
                        {
                            if (step.Value?.check?.province?.eventMemory?.checks != null)
                            {
                                foreach (var check in step.Value?.check?.province?.eventMemory?.checks)
                                {
                                    if (check.key == key)
                                    {
                                        RemoveKey (keyList, key);
                                    }
                                }

                                if (step.Value?.check?.self?.eventMemory?.checks != null)
                                {
                                    foreach (var check in step.Value?.check?.self?.eventMemory?.checks)
                                    {
                                        if (check.key == key)
                                        {
                                            RemoveKey (keyList, key);
                                        }
                                    }

                                    if (step.Value?.check?.target?.eventMemory?.checks != null)
                                    {
                                        foreach (var check in step.Value?.check?.target?.eventMemory?.checks)
                                        {
                                            if (check.key == key)
                                            {
                                                RemoveKey (keyList, key);
                                            }
                                        }
                                    }
                                }
                            }

                            if (step.Value?.memoryChanges != null)
                                foreach (var changes in step.Value?.memoryChanges)
                                {
                                    if (changes?.changes != null)
                                    {
                                        foreach (var change in changes?.changes)
                                        {
                                            if (change.key == key)
                                            {
                                                RemoveKey (keyList, key);
                                            }
                                        }
                                    }
                                }
                        }
                    }

                    if (eventValue?.options != null)
                    {
                        foreach (var option in eventValue?.options)
                        {
                            if (option.Value?.check?.province?.eventMemory?.checks != null)
                            {
                                foreach (var check in option.Value?.check?.province?.eventMemory?.checks)
                                {
                                    if (check.key == key)
                                    {
                                        RemoveKey (keyList, key);
                                    }
                                }

                                if (option.Value?.check?.self?.eventMemory?.checks != null)
                                {
                                    foreach (var check in option.Value?.check?.self?.eventMemory?.checks)
                                    {
                                        if (check.key == key)
                                        {
                                            RemoveKey (keyList, key);
                                        }
                                    }

                                    if (option.Value?.check?.target?.eventMemory?.checks != null)
                                        foreach (var check in option.Value?.check?.target?.eventMemory?.checks)
                                        {
                                            if (check.key == key)
                                            {
                                                RemoveKey (keyList, key);
                                            }
                                        }
                                }
                            }

                            if (option.Value?.memoryChanges != null)
                            {
                                foreach (var changes in option.Value?.memoryChanges)
                                {
                                    if (changes?.changes != null)
                                    {
                                        foreach (var change in changes?.changes)
                                        {
                                            if (change.key == key)
                                            {
                                                RemoveKey (keyList, key);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var key in keyList)
            {
                Debug.Log (key);
            }
        }

        public void RemoveKey(List<string> keyList, string key)
        {
            if (keyList.Contains(key))
                keyList.Remove(key);
        }
        
        

        /*
        private static void UpgradeFloatChanges (string context, ref List<DataBlockMemoryChangeFloat> changesOld, ref List<DataBlockMemoryChangeFloat> changesNew)
        {
            if (changesNew == null)
                changesNew = new List<DataBlockMemoryChangeFloat> ();
            
            foreach (var changeOld in changesOld)
            {
                var changeNew = new DataBlockMemoryChangeFloat
                {
                    change = changeOld.change,
                    key = GetFixedKey (changeOld.key),
                    value = changeOld.value
                };
                                    
                changesNew.Add (changeNew);
                Debug.Log ($"{context} | Float change upgraded:\n{changeNew}");
            }

            // Clear old collection, or later removal of this field might lead to data loss due to YAML anchors
            changesOld = null;
        }
        
        private static void UpgradeIntChanges (string context, ref List<DataBlockMemoryChangeInt> changesOld, ref List<DataBlockMemoryChangeFloat> changesNew)
        {
            if (changesNew == null)
                changesNew = new List<DataBlockMemoryChangeFloat> ();
            
            foreach (var changeOld in changesOld)
            {
                // Map old integer specific change types to equivalent float changes
                var changeType = DictionaryEntryChangeFloat.Add;
                if (changeOld.change == DictionaryEntryChangeInt.Bool)
                    changeType = changeOld.value == 1 ? DictionaryEntryChangeFloat.Add : DictionaryEntryChangeFloat.Remove;
                else if (changeOld.change == DictionaryEntryChangeInt.Offset)
                    changeType = DictionaryEntryChangeFloat.Offset;
                else if (changeOld.change == DictionaryEntryChangeInt.Set)
                    changeType = DictionaryEntryChangeFloat.Set;

                var changeNew = new DataBlockMemoryChangeFloat
                {
                    change = changeType,
                    key = GetFixedKey (changeOld.key),
                    value = changeOld.value
                };
                                    
                changesNew.Add (changeNew);
                Debug.Log ($"{context} | Int change upgraded:\n{changeNew}");
            }
            
            // Clear old collection, or later removal of this field might lead to data loss due to YAML anchors
            changesOld = null;
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private static void UpgradeMemoryChanges ()
        {
            foreach (var kvp in DataMultiLinkerScenario.data)
            {
                var scenario = kvp.Value;
                foreach (var kvp2 in scenario.states)
                {
                    var state = kvp2.Value;
                    if (state.reactions == null || state.reactions.effectsPerIncrement == null) 
                        continue;

                    foreach (var kvp3 in state.reactions.effectsPerIncrement)
                    {
                        var effect = kvp3.Value;
                        if (effect == null)
                            continue;

                        if (effect.memoryChangesFloat != null && effect.memoryChangesFloat.Count > 0)
                        {
                            foreach (var changeGroupOld in effect.memoryChangesFloat)
                            {
                                var changeGroupNew = new DataBlockMemoryChangeGroupScenario ()
                                {
                                    context = changeGroupOld.context,
                                    provinceKey = changeGroupOld.provinceKey,
                                    changes = new List<DataBlockMemoryChangeFloat> ()
                                };

                                if (effect.memoryChanges == null)
                                    effect.memoryChanges = new List<DataBlockMemoryChangeGroupScenario> ();
                                effect.memoryChanges.Add (changeGroupNew);
                                UpgradeFloatChanges ($"Scenario {kvp.Key} | State {kvp2.Key} | Effect {kvp3.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                            }
                        }
                        
                        if (effect.memoryChangesInt != null && effect.memoryChangesInt.Count > 0)
                        {
                            foreach (var changeGroupOld in effect.memoryChangesInt)
                            {
                                var changeGroupNew = new DataBlockMemoryChangeGroupScenario ()
                                {
                                    context = changeGroupOld.context,
                                    provinceKey = changeGroupOld.provinceKey,
                                    changes = new List<DataBlockMemoryChangeFloat> ()
                                };

                                if (effect.memoryChanges == null)
                                    effect.memoryChanges = new List<DataBlockMemoryChangeGroupScenario> ();
                                effect.memoryChanges.Add (changeGroupNew);
                                UpgradeIntChanges ($"Scenario {kvp.Key} | State {kvp2.Key} | Effect {kvp3.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                            }
                        }
                    }
                }
            }

            foreach (var kvp in DataMultiLinkerOverworldEvent.data)
            {
                var eventData = kvp.Value;
                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var step = kvp2.Value;
                        if (step == null)
                            continue;
                        
                        if (step.memoryChangesFloat != null && step.memoryChangesFloat.Count > 0)
                        {
                            foreach (var changeGroupOld in step.memoryChangesFloat)
                            {
                                var changeGroupNew = new DataBlockMemoryChangeGroupEvent ()
                                {
                                    context = changeGroupOld.context,
                                    provinceKey = changeGroupOld.provinceKey,
                                    changes = new List<DataBlockMemoryChangeFloat> ()
                                };

                                if (step.memoryChanges == null)
                                    step.memoryChanges = new List<DataBlockMemoryChangeGroupEvent> ();
                                step.memoryChanges.Add (changeGroupNew);
                                UpgradeFloatChanges ($"Event {kvp.Key} | Step {kvp2.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                            }
                        }
                        
                        if (step.memoryChangesInt != null && step.memoryChangesInt.Count > 0)
                        {
                            foreach (var changeGroupOld in step.memoryChangesInt)
                            {
                                var changeGroupNew = new DataBlockMemoryChangeGroupEvent ()
                                {
                                    context = changeGroupOld.context,
                                    provinceKey = changeGroupOld.provinceKey,
                                    changes = new List<DataBlockMemoryChangeFloat> ()
                                };

                                if (step.memoryChanges == null)
                                    step.memoryChanges = new List<DataBlockMemoryChangeGroupEvent> ();
                                step.memoryChanges.Add (changeGroupNew);
                                UpgradeIntChanges ($"Event {kvp.Key} | Step {kvp2.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                            }
                        }
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        if (option == null)
                            continue;
                        
                        if (option.memoryChangesFloat != null && option.memoryChangesFloat.Count > 0)
                        {
                            foreach (var changeGroupOld in option.memoryChangesFloat)
                            {
                                var changeGroupNew = new DataBlockMemoryChangeGroupEvent ()
                                {
                                    context = changeGroupOld.context,
                                    provinceKey = changeGroupOld.provinceKey,
                                    changes = new List<DataBlockMemoryChangeFloat> ()
                                };

                                if (option.memoryChanges == null)
                                    option.memoryChanges = new List<DataBlockMemoryChangeGroupEvent> ();
                                option.memoryChanges.Add (changeGroupNew);
                                UpgradeFloatChanges ($"Event {kvp.Key} | Embedded option {kvp2.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                            }
                        }
                        
                        if (option.memoryChangesInt != null && option.memoryChangesInt.Count > 0)
                        {
                            foreach (var changeGroupOld in option.memoryChangesInt)
                            {
                                var changeGroupNew = new DataBlockMemoryChangeGroupEvent ()
                                {
                                    context = changeGroupOld.context,
                                    provinceKey = changeGroupOld.provinceKey,
                                    changes = new List<DataBlockMemoryChangeFloat> ()
                                };

                                if (option.memoryChanges == null)
                                    option.memoryChanges = new List<DataBlockMemoryChangeGroupEvent> ();
                                option.memoryChanges.Add (changeGroupNew);
                                UpgradeIntChanges ($"Event {kvp.Key} | Embedded option {kvp2.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                            }
                        }
                    }
                }
                
                if (eventData.customSpawnData != null)
                {
                    foreach (var kvp2 in eventData.customSpawnData)
                    {
                        var spawnData = kvp2.Value;
                        if (spawnData == null)
                            continue;
                        
                        if (spawnData.memoryChangesFloat != null && spawnData.memoryChangesFloat.Count > 0)
                            UpgradeFloatChanges ($"Event {kvp.Key} | Spawn data {kvp2.Key}", ref spawnData.memoryChangesFloat, ref spawnData.memoryChanges);

                        if (spawnData.memoryChangesInt != null && spawnData.memoryChangesInt.Count > 0)
                            UpgradeIntChanges ($"Event {kvp.Key} | Spawn data {kvp2.Key}", ref spawnData.memoryChangesInt, ref spawnData.memoryChanges);
                    }
                }
            }
            
            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var option = kvp.Value;
                if (option == null)
                    continue;
                        
                if (option.memoryChangesFloat != null && option.memoryChangesFloat.Count > 0)
                {
                    foreach (var changeGroupOld in option.memoryChangesFloat)
                    {
                        var changeGroupNew = new DataBlockMemoryChangeGroupEvent ()
                        {
                            context = changeGroupOld.context,
                            provinceKey = changeGroupOld.provinceKey,
                            changes = new List<DataBlockMemoryChangeFloat> ()
                        };

                        if (option.memoryChanges == null)
                            option.memoryChanges = new List<DataBlockMemoryChangeGroupEvent> ();
                        option.memoryChanges.Add (changeGroupNew);
                        UpgradeFloatChanges ($"Shared option {kvp.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                    }
                }
                        
                if (option.memoryChangesInt != null && option.memoryChangesInt.Count > 0)
                {
                    foreach (var changeGroupOld in option.memoryChangesInt)
                    {
                        var changeGroupNew = new DataBlockMemoryChangeGroupEvent ()
                        {
                            context = changeGroupOld.context,
                            provinceKey = changeGroupOld.provinceKey,
                            changes = new List<DataBlockMemoryChangeFloat> ()
                        };

                        if (option.memoryChanges == null)
                            option.memoryChanges = new List<DataBlockMemoryChangeGroupEvent> ();
                        option.memoryChanges.Add (changeGroupNew);
                        UpgradeIntChanges ($"Shared option {kvp.Key}", ref changeGroupOld.changes, ref changeGroupNew.changes);
                    }
                }
            }

            foreach (var kvp in DataMultiLinkerOverworldAction.data)
            {
                var action = kvp.Value;
                if (action == null)
                    continue;
                
                if (action.customSpawnData != null)
                {
                    foreach (var kvp2 in action.customSpawnData)
                    {
                        var spawnData = kvp2.Value;
                        if (spawnData == null)
                            continue;
                        
                        if (spawnData.memoryChangesFloat != null && spawnData.memoryChangesFloat.Count > 0)
                            UpgradeFloatChanges ($"Action {kvp.Key} | Spawn data {kvp2.Key}", ref spawnData.memoryChangesFloat, ref spawnData.memoryChanges);

                        if (spawnData.memoryChangesInt != null && spawnData.memoryChangesInt.Count > 0)
                            UpgradeIntChanges ($"Action {kvp.Key} | Spawn data {kvp2.Key}", ref spawnData.memoryChangesInt, ref spawnData.memoryChanges);
                    }
                }
            }
        }
        */
        
        /*
        private static void UpgradeMemoryCheckGroup (string context, DataBlockOverworldMemoryCheckGroup group)
        {
            if (group == null) 
                return;

            if (group.checks == null)
                group.checks = new List<DataBlockOverworldMemoryCheck> ();
            else
                group.checks.Clear ();

            int intsAdded = 0;
            int floatsAdded = 0;
                
            // Extract old int checks
            if (group.eventMemoryInt != null && group.eventMemoryInt.Count > 0)
            {
                group.method = group.eventMemoryMethodInt; 
                foreach (var checkOld in group.eventMemoryInt)
                {
                    if (checkOld == null) 
                        continue;
                    
                    bool presenceDesired = checkOld.check != IntCheckMode.Bool || checkOld.value > 0;
                    
                    var valueCheck = MemoryValueCheckMode.NoValueCheck;
                    if (checkOld.check == IntCheckMode.Less)
                        valueCheck = MemoryValueCheckMode.Less;
                    else if (checkOld.check == IntCheckMode.LessEqual)
                        valueCheck = MemoryValueCheckMode.LessEqual;
                    else if (checkOld.check == IntCheckMode.Equal)
                        valueCheck = MemoryValueCheckMode.Equal;
                    else if (checkOld.check == IntCheckMode.GreaterEqual)
                        valueCheck = MemoryValueCheckMode.GreaterEqual;
                    else if (checkOld.check == IntCheckMode.Greater)
                        valueCheck = MemoryValueCheckMode.Greater;

                    intsAdded += 1;
                    var checkNew = new DataBlockOverworldMemoryCheck
                    {
                        key = checkOld.key,
                        presenceDesired = presenceDesired,
                        valueCheck = valueCheck,
                        value = checkOld.value,
                    };
                    
                    Debug.Log ($" ┌ New check (from int): {checkNew.ToString ()}");
                    group.checks.Add (checkNew);
                }
                
                group.eventMemoryInt = null;
            }
            
            // Extract old float checks
            if (group.eventMemoryFloat != null && group.eventMemoryFloat.Count > 0)
            {
                group.method = group.eventMemoryMethodFloat;
                foreach (var checkOld in group.eventMemoryFloat)
                {
                    if (checkOld == null)
                        continue;

                    var valueCheck = MemoryValueCheckMode.NoValueCheck;
                    if (checkOld.check == FloatCheckMode.Less)
                        valueCheck = MemoryValueCheckMode.Less;
                    else if (checkOld.check == FloatCheckMode.RoughlyEqual)
                        valueCheck = MemoryValueCheckMode.Equal;
                    else if (checkOld.check == FloatCheckMode.Greater)
                        valueCheck = MemoryValueCheckMode.Greater;

                    floatsAdded += 1;
                    var checkNew = new DataBlockOverworldMemoryCheck
                    {
                        key = checkOld.key,
                        presenceDesired = true,
                        valueCheck = valueCheck,
                        value = checkOld.value,
                    };
                    
                    Debug.Log ($" ┌ New check (from float): {checkNew.ToString ()}");
                    group.checks.Add (checkNew);
                }

                group.eventMemoryFloat = null;
            }

            // Upgrade keys
            foreach (var check in group.checks)
            {
                if (check == null || string.IsNullOrEmpty (check.key))
                    continue;
                
                if (keysOldToNew.ContainsKey (check.key))
                    check.key = keysOldToNew[check.key];
                else
                {
                    var keyNew = GetFixedKey (check.key);
                    if (data.ContainsKey (keyNew))
                    {
                        Debug.LogWarning ($" ┌ Forcing upgrade of previously unregistered old key: {check.key} | Using new config created right before: {keyNew}");
                    }
                    else
                    {
                        var config = new DataContainerOverworldMemory ();
                        data.Add (keyNew, config);
                
                        config.key = keyNew;
                        config.type = OverworldMemoryType.Int;
                        config.discardOnWorldChange = true;
                        config.keyOld = check.key;

                        if (keyNew.StartsWith ("world"))
                            config.host = OverworldMemoryHost.World;
                        else if (keyNew.StartsWith ("province"))
                            config.host = OverworldMemoryHost.Province;
                        else if (keyNew.StartsWith ("unit"))
                            config.host = OverworldMemoryHost.Unit;
                        else if (keyNew.StartsWith ("pilot"))
                            config.host = OverworldMemoryHost.Pilot;
                    
                        Debug.LogWarning ($" ┌ Forcing upgrade of previously unregistered old key {check.key} | Registering new config {keyNew} | Host: {config.host}");
                    }
                    
                    check.key = keyNew;
                }
            }
            
            var text = group.GetStringAndWarning (out bool warning);
            if (warning)
                Debug.LogWarning ($"{context} | Upgraded {intsAdded}-I, {floatsAdded}-F\n{text}");
            else
                Debug.Log ($"{context} | Upgraded {intsAdded}-I, {floatsAdded}-F\n{text}");
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private static void UpgradeMemoryChecks () 
        {
            foreach (var kvp in DataMultiLinkerScenario.data)
            {
                var scenario = kvp.Value;
                foreach (var kvp2 in scenario.steps)
                {
                    var step = kvp2.Value;
                    if (step.unitGroups == null)
                        continue;

                    for (int i = 0, count = step.unitGroups.Count; i < count; ++i)
                    {
                        var slot = step.unitGroups[i];
                        if (slot.check == null)
                            continue;
                        
                        if (slot.check.provinceEventMemory != null)
                            UpgradeMemoryCheckGroup ($"Scenario {kvp.Key} / Step {kvp2.Key} / Slot {i}: province event memory check", slot.check.provinceEventMemory);

                        if (slot.check.selfEventMemory != null)
                            UpgradeMemoryCheckGroup ($"Scenario {kvp.Key} / Step {kvp2.Key} / Slot {i}: self event memory check", slot.check.selfEventMemory);
                        
                        if (slot.check.targetEventMemory != null)
                            UpgradeMemoryCheckGroup ($"Scenario {kvp.Key} / Step {kvp2.Key} / Slot {i}: target event memory check", slot.check.targetEventMemory);
                    }
                }
            }
            
            foreach (var kvp in DataMultiLinkerPilotCheck.data)
            {
                var pilotCheck = kvp.Value;
                if (pilotCheck.check == null || pilotCheck.check.eventMemory == null)
                    continue;
                
                UpgradeMemoryCheckGroup ($"Pilot check {kvp.Key}: event memory check", pilotCheck.check.eventMemory);
            }

            foreach (var kvp in DataMultiLinkerOverworldEvent.data)
            {
                var eventData = kvp.Value;
                
                if (eventData.actorPilots != null)
                {
                    for (int i = 0, count = eventData.actorPilots.Count; i < count; ++i)
                    {
                        var slot = eventData.actorPilots[i];
                        if (slot.check?.eventMemory != null)
                            UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Pilot actor {i}: event memory check", slot.check.eventMemory);
                    }
                }
                
                if (eventData.actorUnits != null)
                {
                    for (int i = 0, count = eventData.actorUnits.Count; i < count; ++i)
                    {
                        var slot = eventData.actorUnits[i];
                        if (slot.check?.eventMemory != null)
                            UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Unit actor {i}: event memory check", slot.check.eventMemory);
                    }
                }
                
                if (eventData.actorsWorld != null)
                {
                    for (int i = 0, count = eventData.actorsWorld.Count; i < count; ++i)
                    {
                        var slot = eventData.actorsWorld[i];
                        if (slot.check?.eventMemory != null)
                            UpgradeMemoryCheckGroup ($"Event {kvp.Key} / World actor {i}: event memory check", slot.check.eventMemory);
                    }
                }

                if (eventData.steps != null)
                {
                    foreach (var kvp2 in eventData.steps)
                    {
                        var step = kvp2.Value;
                        if (step.check != null)
                        {
                            if (step.check.self?.eventMemory != null)
                                UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Step {kvp2.Key}: self event memory check", step.check.self.eventMemory);

                            if (step.check.target?.eventMemory != null)
                                UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Step {kvp2.Key}: target event memory check", step.check.target.eventMemory);

                            if (step.check.province?.eventMemory != null)
                                UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Step {kvp2.Key}: province event memory check", step.check.province.eventMemory);
                        }
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        if (option.check != null)
                        {
                            if (option.check.self?.eventMemory != null)
                                UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Embedded option {kvp2.Key} / Check: self event memory check", option.check.self.eventMemory);

                            if (option.check.target?.eventMemory != null)
                                UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Embedded option {kvp2.Key} / Check: target event memory check", option.check.target.eventMemory);

                            if (option.check.province?.eventMemory != null)
                                UpgradeMemoryCheckGroup ($"Event {kvp.Key} / Embedded option {kvp2.Key} / Check: province event memory check", option.check.province.eventMemory);
                        }
                    }
                }
            }
            
            foreach (var kvp in DataMultiLinkerOverworldEventOption.data)
            {
                var option = kvp.Value;
                if (option.check != null)
                {
                    if (option.check.self?.eventMemory != null)
                        UpgradeMemoryCheckGroup ($"Option {kvp.Key}: self event memory check", option.check.self.eventMemory);

                    if (option.check.target?.eventMemory != null)
                        UpgradeMemoryCheckGroup ($"Option {kvp.Key}: target event memory check", option.check.target.eventMemory);

                    if (option.check.province?.eventMemory != null)
                        UpgradeMemoryCheckGroup ($"Option {kvp.Key}: province event memory check", option.check.province.eventMemory);
                }
            }
        }
        */
        
        /*
        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void RenameMemoryKeys ()
        {
            var renameHistory = new SortedDictionary<string, string> ();
            
            var intKeys = DataLinkerSettingsOverworld.data.eventMemoryKeysInt;
            foreach (var keyOld in intKeys)
            {
                var keyNew = GetFixedKey (keyOld);
                if (!renameHistory.ContainsKey (keyOld))
                    renameHistory.Add (keyOld, keyNew);
            }

            intKeys.Clear ();
            foreach (var kvp in renameHistory)
                intKeys.Add (kvp.Value);
            
            Debug.LogWarning ($"Full rename history (int):\n{renameHistory.ToStringFormattedKeyValuePairs (true)}");
            renameHistory.Clear ();
            
            var floatKeys = DataLinkerSettingsOverworld.data.eventMemoryKeysFloat;
            foreach (var keyOld in floatKeys)
            {
                var keyNew = GetFixedKey (keyOld);
                if (!renameHistory.ContainsKey (keyOld))
                    renameHistory.Add (keyOld, keyNew);
            }

            floatKeys.Clear ();
            foreach (var kvp in renameHistory)
                floatKeys.Add (kvp.Value);

            Debug.LogWarning ($"Full rename history (float):\n{renameHistory.ToStringFormattedKeyValuePairs (true)}");
        }
        */

        /*
        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void GenerateMemoryConfigs ()
        {
            var ovc = DataShortcuts.overworld;
            var keysSpecial = new HashSet<string>
            {
                "Province/Tag/Ignore",
                "Province/LiberationAcknowledged",
                "World/Tag/Assassination/Victory",
                "World/Tag/Assassination/Defeat",
                "World/Tag/Cat/Present",
                "World/Tag/Deserter/Present",
                "World/Tag/Defector/Present",
                "World/Tag/Dog/Present",
                "World/Tag/RecruitPilot/NewPilotOnboard",
                "Province/Tag/Core/Tutorial1",
                "Province/Tag/Core/Tutorial2",
                "Province/Tag/ForeignTerritory",
                "World/Counter/Battlefield",
                "World/Counter/BlackMarketInfluence",
                "World/Counter/Cat/Bond",
                "World/Counter/Dog/Bond",
                "World/Counter/Reputation/Guerrillas",
                "Province/Tag/Tutorial/NewProvince",
                "Province/Tag/Tutorial/TransportAttacked",
                "World/Tag/Tutorial/FortificationCaptured",
                "World/Tag/Tutorial/GotPerk",
                "World/Tag/Tutorial/GotReactor",
                "World/Tag/Tutorial/Target",
                "World/Tag/Tutorial/UnitDefeated",
                "World/Tag/Tutorial/Victory",
                "World/Auto_TimeInDanger",
                "World/Auto_TimeInProvince",
                "World/Auto_TimeOfDay",
                "World/Auto_TimeSinceCombat",
                "World/Auto_TimeTotal",
                "World/Counter/Auto_CombatEncounters",
                "World/Counter/Auto_CombatVictoryStreak",
                "World/Counter/Auto_CombatVictoryThreatRecord",
                "World/Counter/Auto_PilotsLost"
            };

            DataShortcuts.overworld.eventMemoryKeysPreserved = keysSpecial;

            var renameHistory = new SortedDictionary<string, string> ();

            var keysSpecialFixed = new HashSet<string> ();
            foreach (var key in keysSpecial)
            {
                string keyFixed = GetFixedKey (key);
                keysSpecialFixed.Add (keyFixed);
            }

            void AddConfig (string keyOld, OverworldMemoryType type)
            {
                var keyNew = GetFixedKey (keyOld);
                if (!renameHistory.ContainsKey (keyOld))
                    renameHistory.Add (keyOld, keyNew);
                
                if (data.ContainsKey (keyNew))
                    return;
                
                var config = new DataContainerOverworldMemory ();
                data.Add (keyNew, config);
                
                config.key = keyNew;
                config.type = type;
                config.discardOnWorldChange = !keysSpecialFixed.Contains (keyNew);
                config.keyOld = keyOld;

                if (keyNew.StartsWith ("world"))
                    config.host = OverworldMemoryHost.World;
                else if (keyNew.StartsWith ("province"))
                    config.host = OverworldMemoryHost.Province;
                else if (keyNew.StartsWith ("unit"))
                    config.host = OverworldMemoryHost.Unit;
                else if (keyNew.StartsWith ("pilot"))
                    config.host = OverworldMemoryHost.Pilot;
                
                Debug.Log ($"{keyOld} -> {keyNew}\nType: {config.type} | Host: {config.host} | Discard: {config.discardOnWorldChange}");
            }
            
            foreach (var key in ovc.eventMemoryKeysFloat)
            {
                AddConfig (key, OverworldMemoryType.Float);
            }
            
            foreach (var key in ovc.eventMemoryKeysInt)
            {
                AddConfig (key, OverworldMemoryType.Int);
            }
            
            Debug.LogWarning ($"Full rename history:\n{renameHistory.ToStringFormattedKeyValuePairs (true)}");
        }
        */
    }
}