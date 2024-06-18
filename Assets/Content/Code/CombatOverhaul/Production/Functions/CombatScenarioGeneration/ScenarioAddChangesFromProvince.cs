using System.Collections.Generic;
using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddChangesFromProvince : ICombatScenarioGenStep
    {
        public void Run (DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            var targetPersistent = ScenarioUtility.GetCombatSite ();
            var targetOverworld = IDUtility.GetLinkedOverworldEntity (targetPersistent);
            if (targetOverworld == null || !targetOverworld.hasDataLinkOverworldEntityBlueprint)
            {
                // Debug.LogWarning ($"Skipping scenario changes from site: {targetOverworld.ToLog ()} has no blueprint");
                return;
            }

            var provinceBlueprint = DataHelperProvince.GetProvinceBlueprintAtEntity (targetOverworld);
            if (provinceBlueprint == null || provinceBlueprint.scenarioChanges == null)
                return;

            var changes = provinceBlueprint.scenarioChanges;
            foreach (var change in changes)
            {
                if (change == null || !change.IsChangeApplicable (scenario))
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
            if (log)
                Debug.Log ($"Applying changes to generated scenario {scenario.key} from province {provinceBlueprint.key}");

            DataBlockScenarioStep stepOnStart = null;
            if (!string.IsNullOrEmpty (scenario.coreProc.stepOnStart) && scenario.stepsProc != null && scenario.stepsProc.TryGetValue (scenario.coreProc.stepOnStart, out var value))
                stepOnStart = value;
            
            if (change.functionsOnStart != null && change.functionsOnStart.Count > 0)
            {
                if (stepOnStart == null)
                    Debug.LogWarning ($"Failed to apply function changes to the starting step: starting step {scenario.coreProc.stepOnStart} not found");
                else
                {
                    if (stepOnStart.functions == null)
                        stepOnStart.functions = new List<ICombatFunction> ();

                    foreach (var function in change.functionsOnStart)
                    {
                        stepOnStart.functions.Add (function);
                        // if (log)
                            Debug.Log ($"- Added function {function.GetType ().Name} to step {stepOnStart.key}");
                    }
                }
            }

            if (change.units != null && change.units.Count > 0)
            {
                var context = $"province {provinceBlueprint.key}";
                ScenarioUtilityGeneration.InsertUnitBlocks (scenario, change.units, context);
            }
            
            #endif
        }
    }
}