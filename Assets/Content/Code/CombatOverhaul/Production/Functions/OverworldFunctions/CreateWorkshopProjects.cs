using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreateWorkshopProjects : IOverworldActionFunction, IOverworldFunction, ICombatFunction
    {
        [ListDrawerSettings (DefaultExpandedState = true, CustomAddFunction = "@new DataBlockVirtualWorkshopProject ()")]
        public List<DataBlockVirtualWorkshopProject> projects = new List<DataBlockVirtualWorkshopProject> ();
        
        private static List<string> keysFiltered = new List<string> ();
        
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }
        
        public void Run ()
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null || projects == null)
                return;

            bool duplicateProtection = true; // DataShortcuts.sim.salvageWorkshopDuplicateProtection;
            var baseProjects = basePersistent.hasInventoryWorkshopCharges ? basePersistent.inventoryWorkshopCharges.s : null;

            foreach (var description in projects)
            {
                int count = 
                    description.countRandom ? 
                    UnityEngine.Random.Range (description.countMin, description.countMax) : 
                    description.countMin;

                if (description.tagsUsed)
                {
                    var projectsMatchingTags = DataTagUtility.GetKeysWithTags (DataMultiLinkerWorkshopProject.data, description.tags);
                    keysFiltered.Clear ();

                    foreach (var key in projectsMatchingTags)
                    {
                        var project = DataMultiLinkerWorkshopProject.GetEntry (key);
                        if (project == null || project.hidden)
                            continue;
                        
                        // Skip workshop projects already on the base
                        if (duplicateProtection && baseProjects != null && baseProjects.ContainsKey (key))
                            continue;

                        if (!project.rewarded)
                            continue;

                        keysFiltered.Add (key);
                    }
                    
                    if (keysFiltered.Count == 0)
                    {
                        Debug.LogWarning ($"Failed to resolve virtual subsystem group with tags {description.tags.ToStringFormattedKeyValuePairs ()}");
                        continue;
                    }
                    
                    for (int i = 0; i < count; ++i)
                    {
                        var projectKey = keysFiltered.GetRandomEntry ();
                        var project = DataMultiLinkerWorkshopProject.GetEntry (projectKey);
                        if (project == null)
                            continue;
                        
                        EquipmentUtility.ModifyChargesInInventory (basePersistent, projectKey, 1);
                    }
                }
                else
                {
                    var projectKey = description.key;
                    var project = DataMultiLinkerWorkshopProject.GetEntry (projectKey);
                    if (project == null)
                        continue;
                    
                    EquipmentUtility.ModifyChargesInInventory (basePersistent, projectKey, count);
                }
            }
            
            #endif
        }
    }
}