using PhantomBrigade.Functions;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddParentFromProvince : ICombatScenarioGenStep
    {
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK
            
            if (targetOverworld == null || !targetOverworld.hasDataLinkPointPreset)
            {
                // Debug.LogWarning ($"Skipping scenario changes from site: {targetOverworld.ToLog ()} has no blueprint");
                return;
            }

            var provinceBlueprint = DataHelperProvince.GetProvinceBlueprintAtEntity (targetOverworld);
            if (provinceBlueprint == null || provinceBlueprint.scenarioChanges == null)
                return;
            
            var targetPersistent = IDUtility.GetLinkedPersistentEntity (targetOverworld);
            if (targetPersistent == null)
                return;

            var changes = provinceBlueprint.scenarioChanges;
            foreach (var change in changes)
            {
                if (change == null || !change.IsChangeApplicable (scenario, targetPersistent))
                    continue;

                ApplyChange (scenario, seed, provinceBlueprint, change);
            }
            
            #endif
        }
        
        private void ApplyChange (DataContainerScenario scenario, int seed, DataContainerOverworldProvinceBlueprint provinceBlueprint, DataBlockProvinceScenarioChange change)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (change.parentKey))
                return;
                
            bool log = DataShortcuts.sim.logScenarioGeneration;
            if (scenario.parents != null)
            {
                bool present = false;
                foreach (var parent in scenario.parents)
                {
                    if (parent != null && parent.key.Equals (change.parentKey))
                    {
                        if (log)
                            Debug.LogWarning ($"Skipping scenario parent {change.parentKey} from province {provinceBlueprint.key} on generated scenario {scenario.key}: parent already present");

                        present = true;
                        break;
                    }
                }

                if (present)
                    return;
            }
            
            if (log)
                Debug.Log ($"Adding scenario parent {change.parentKey} from province {provinceBlueprint.key} to generated scenario {scenario.key}");
            
            if (scenario.parents == null)
                scenario.parents = new List<DataContainerScenarioParent> ();
            
            scenario.parents.Add (new DataContainerScenarioParent { key = change.parentKey });
            
            #endif
        }
    }
}