using System.Collections.Generic;
using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddChangesFromSite : ICombatScenarioGenStep
    {
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            if (targetOverworld == null || !targetOverworld.hasDataLinkPointPreset)
            {
                // Debug.LogWarning ($"Skipping scenario changes from site: {targetOverworld.ToLog ()} has no blueprint");
                return;
            }
            
            var preset = targetOverworld.dataLinkPointPreset.data;
            var changes = preset.combatProc?.scenarioChanges;
            if (changes == null)
            {
                // Debug.LogWarning ($"Skipping scenario changes from site: {targetOverworld.ToLog ()} has no change data");
                return;
            }

            bool log = DataShortcuts.sim.logScenarioGeneration;
            if (log)
                Debug.Log ($"Applying scenario changes from site: {targetOverworld.ToLog ()}");
            
            DataBlockScenarioStep stepOnStart = null;
            if (!string.IsNullOrEmpty (scenario.coreProc.stepOnStart) && scenario.stepsProc != null && scenario.stepsProc.TryGetValue (scenario.coreProc.stepOnStart, out var value))
                stepOnStart = value;

            if (changes.functionsOnStart != null && changes.functionsOnStart.Count > 0)
            {
                if (stepOnStart == null)
                    Debug.LogWarning ($"Failed to apply function changes to the starting step: starting step {scenario.coreProc.stepOnStart} not found");
                else
                {
                    if (stepOnStart.functions == null)
                        stepOnStart.functions = new List<ICombatFunction> ();

                    foreach (var function in changes.functionsOnStart)
                    {
                        stepOnStart.functions.Add (function);
                        // if (log)
                            Debug.Log ($"- Added function {function.GetType ().Name} to step {stepOnStart.key}");
                    }
                }
            }
            
            if (changes.compositeOnStart != null)
            {
                bool success = false;
                if (stepOnStart != null && stepOnStart.functions != null)
                {
                    foreach (var function in stepOnStart.functions)
                    {
                        if (function != null && function is CombatCreateUnitComposite functionSpawn && functionSpawn.instanceNameOverride == changes.compositeOnStart.instanceNameOverride)
                        {
                            // if (log)
                            Debug.Log ($"- Modified composite spawn {function.GetType ().Name} function in step {stepOnStart.key} to key {changes.compositeOnStart.blueprintKey}");
                            functionSpawn.blueprintKeys = new List<string> { changes.compositeOnStart.blueprintKey };
                            functionSpawn.tagsUsed = false;
                            functionSpawn.tags = null;
                            success = true;
                            break;
                        }
                    }
                }
                
                if (!success)
                    Debug.LogWarning ($"Failed to apply unit composite spawn changes to the starting step: starting step {scenario.coreProc.stepOnStart} not found or composite spawn function not found");
            }
            
            #endif
        }
    }
}