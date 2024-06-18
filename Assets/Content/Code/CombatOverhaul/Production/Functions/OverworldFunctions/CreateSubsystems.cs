using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreateSubsystems : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction
    {
        public bool log = false;
        
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockVirtualSubsystems ()")]
        public List<DataBlockVirtualSubsystems> subsystems = new List<DataBlockVirtualSubsystems> ();
        
        private static List<string> keysFiltered = new List<string> ();

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            Run ();
        }
        
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }
        
        public void Run ()
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null || subsystems == null)
                return;
            
            foreach (var description in subsystems)
            {
                int count = description.countRandom ? UnityEngine.Random.Range (description.countMin, description.countMax + 1) : description.countMin;
                if (description.tagsUsed)
                {
                    var subsystemsMatchingTags = DataTagUtility.GetKeysWithTags (DataMultiLinkerSubsystem.data, description.tags, returnAllOnEmptyFilter: true);
                    keysFiltered.Clear ();
                    
                    foreach (var subsystemCandidateKey in subsystemsMatchingTags)
                    {
                        var blueprint = DataMultiLinkerSubsystem.GetEntry (subsystemCandidateKey);
                        if (blueprint == null || blueprint.hidden)
                            continue;
                        
                        if (description.ratingRange != null)
                        {
                            if (blueprint.rating < description.ratingRange.min || blueprint.rating > description.ratingRange.max)
                                continue;
                        }
                        
                        if (blueprint.tagsProcessed != null && blueprint.tagsProcessed.Contains (EquipmentTags.incompatible))
                            continue;

                        if (blueprint.hardpointsProcessed == null)
                            continue;

                        bool hardpointValid = true;
                        foreach (var hardpoint in blueprint.hardpointsProcessed)
                        {
                            var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpoint);
                            if (hardpointInfo == null || !hardpointInfo.exposed || !hardpointInfo.editable)
                            {
                                hardpointValid = false;
                                break;
                            }
                        }
                        
                        if (!hardpointValid)
                            continue;

                        keysFiltered.Add (subsystemCandidateKey);
                    }
                    
                    if (keysFiltered.Count == 0)
                    {
                        Debug.LogWarning ($"Failed to resolve virtual subsystem group with tags {description.tags.ToStringFormattedKeyValuePairs ()}");
                        continue;
                    }
                    
                    for (int i = 0; i < count; ++i)
                    {
                        var blueprintName = keysFiltered.GetRandomEntry ();
                        var subsystem = UnitUtilities.CreateSubsystemEntity (blueprintName);
                        if (subsystem != null)
                        {
                            subsystem.ReplaceSalvageSelection (false);
                            EquipmentUtility.AttachSubsystemToInventory (subsystem, basePersistent, true, log);
                        }
                    }
                }
                else
                {
                    var blueprint = DataMultiLinkerSubsystem.GetEntry (description.blueprint);
                    if (blueprint == null)
                        continue;

                    if (blueprint.tagsProcessed != null && blueprint.tagsProcessed.Contains (EquipmentTags.incompatible))
                        continue;

                    if (blueprint.hardpointsProcessed == null)
                        continue;

                    bool hardpointValid = true;
                    foreach (var hardpoint in blueprint.hardpointsProcessed)
                    {
                        var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpoint);
                        if (hardpointInfo == null || !hardpointInfo.exposed || !hardpointInfo.editable)
                        {
                            hardpointValid = false;
                            break;
                        }
                    }
                        
                    if (!hardpointValid)
                        continue;
                    
                    for (int i = 0; i < count; ++i)
                    {
                        var blueprintName = description.blueprint;
                        var subsystem = UnitUtilities.CreateSubsystemEntity (blueprintName);
                        if (subsystem != null)
                        {
                            subsystem.ReplaceSalvageSelection (false);
                            EquipmentUtility.AttachSubsystemToInventory (subsystem, basePersistent, true, log);
                        }
                    }
                }
            }
            
            #endif
        }
    }
}