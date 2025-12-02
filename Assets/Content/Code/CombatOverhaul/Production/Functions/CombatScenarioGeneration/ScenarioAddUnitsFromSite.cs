using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddUnitsInternallyGenerated : ICombatScenarioGenStep
    {
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK
            
            if (scenario == null)
                return;
            
            if (scenario.stepsProc != null)
            {
                foreach (var kvp in scenario.stepsProc)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null || step.unitGeneratorKeys == null || step.unitGeneratorKeys.Count == 0)
                        continue;

                    if (DataShortcuts.sim.logScenarioGeneration)
                        Debug.Log ($"Generating unit blocks with in scenario {scenario.key} step: {stepKey}");

                    step.UpdateUnitGroupsGenerated (targetOverworld);

                    if (step.unitGroupsGenerated != null && step.unitGroupsGenerated.Count > 0)
                    {
                        // Debug.Log ($"Scenario {scenario.key} step {stepKey} has {step.unitGroupsGenerated.Count} generated units");
                        ScenarioUtilityGeneration.InsertUnitGroupCopies (step.unitGroupsGenerated, ref step.unitGroups);
                    }
                }
            }

            if (scenario.statesProc != null)
            {
                foreach (var kvp in scenario.statesProc)
                {
                    var stateKey = kvp.Key;
                    var state = kvp.Value;

                    // Skip missing states or states that can never include units
                    if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                        continue;

                    foreach (var kvp2 in state.reactions.effectsPerIncrement)
                    {
                        int reactionIndex = kvp2.Key;
                        var reaction = kvp2.Value;

                        if (reaction == null || reaction.unitGeneratorKeys == null || reaction.unitGeneratorKeys.Count == 0)
                            continue;

                        if (DataShortcuts.sim.logScenarioGeneration)
                            Debug.Log ($"Generating unit blocks with in scenario {scenario.key} state {stateKey} reaction {reactionIndex}");

                        reaction.UpdateUnitGroupsGenerated (targetOverworld);

                        if (reaction.unitGroupsGenerated != null && reaction.unitGroupsGenerated.Count > 0)
                        {
                            Debug.Log ($"Scenario {scenario.key} state {stateKey} reaction {reactionIndex} has {reaction.unitGroupsGenerated.Count} generated units");
                            ScenarioUtilityGeneration.InsertUnitGroupCopies (reaction.unitGroupsGenerated, ref reaction.unitGroups);
                        }
                    }
                }
            }

            #endif
        }
    }
}