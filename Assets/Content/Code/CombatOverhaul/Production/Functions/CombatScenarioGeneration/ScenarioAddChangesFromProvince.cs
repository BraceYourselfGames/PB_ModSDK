using System.Collections.Generic;
using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddChangesFromProvince : ICombatScenarioGenStep
    {
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK
            
            bool provinceActiveFound = DataHelperProvince.TryGetProvinceDependenciesActive 
            (
                out var provinceActiveBlueprint, 
                out var provinceActivePersistent, 
                out var provinceActiveOverworld
            );

            if (!provinceActiveFound)
            {
                // Debug.LogWarning ($"Skipping scenario changes from site: {targetOverworld.ToLog ()} has no blueprint");
                return;
            }

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (targetOverworld);
            if (targetPersistent == null)
                return;

            if (provinceActiveBlueprint.scenarioChanges != null)
            {
                var context = $"province BP {provinceActiveBlueprint.key}";
                foreach (var change in provinceActiveBlueprint.scenarioChanges)
                {
                    if (change == null || !change.IsChangeApplicable (scenario, targetPersistent))
                        continue;

                    ApplyChange (scenario, seed, context, change);
                }
            }

            if (provinceActiveOverworld.hasProvinceModifiers)
            {
                var modifiers = provinceActiveOverworld.provinceModifiers.keys;
                foreach (var modifierKey in modifiers)
                {
                    var modifierData = DataMultiLinkerOverworldProvinceModifier.GetEntry (modifierKey, false);
                    if (modifierData == null)
                        continue;
                    
                    if (modifierData.scenarioChanges != null)
                    {
                        var context = $"province modifier {modifierKey}";
                        foreach (var change in modifierData.scenarioChanges)
                        {
                            if (change == null || !change.IsChangeApplicable (scenario, targetPersistent))
                                continue;

                            ApplyChange (scenario, seed, context, change);
                        }
                    }
                    
                    if (!string.IsNullOrEmpty (modifierData.pilotPersistentInjected))
                        ScenarioUtilityGeneration.InsertPersistentPilot (scenario, targetPersistent, modifierData.pilotPersistentInjected);
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