using System.Collections.Generic;
using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddChangesFromGenerator : ICombatScenarioGenStep
    {
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed, bool standaloneMode)
        {
            #if !PB_MODSDK
            
            if (!standaloneMode)
                return;

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (targetOverworld);
            if (targetPersistent == null || targetOverworld == null || !targetOverworld.hasCombatGeneratorKey)
                return;

            var generator = DataMultiLinkerScenarioGenerator.GetEntry (targetOverworld.combatGeneratorKey.generator);
            if (generator == null)
                return;
            
            // Apply changes, nothing complex needed
            if (generator.scenarioChangesProc != null && generator.scenarioChangesProc.Count > 0)
            {
                var context = $"generator BP change {generator.key}";
                foreach (var change in generator.scenarioChangesProc)
                {
                    if (change == null || !change.IsChangeApplicable (scenario, targetPersistent))
                        continue;

                    ApplyChange (scenario, seed, context, change);
                }
            }

            #endif
        }

        private void ApplyChange (DataContainerScenario scenario, int seed, string context, DataBlockProvinceScenarioChange change)
        {
            #if !PB_MODSDK

            bool log = DataShortcuts.sim.logScenarioGeneration;
            if (log)
                Debug.Log ($"Applying changes to generated scenario {scenario.key} from {context}");

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
                ScenarioUtilityGeneration.InsertUnitBlocks (scenario, change.units, context);
            
            #endif
        }
    }
}