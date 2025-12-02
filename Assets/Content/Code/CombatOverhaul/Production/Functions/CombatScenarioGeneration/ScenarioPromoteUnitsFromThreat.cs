using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioPromoteUnitsFromThreat :  ICombatScenarioGenStep
    {
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            if (!DataShortcuts.escalation.unitScalingThreatBased)
            {
                // Debug.Log ($"Scenario {scenario.key} won't be scaled by threat level as this method is disabled");
                return;
            }

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (targetOverworld);
            if (!targetPersistent.hasThreatRatingEscalated || !scenario.coreProc.scalingUsed)
            {
                return;
            }

            int targetThreat = ScenarioUtilityGeneration.GetCombatTargetThreat (targetPersistent);
            var escalationLevel = ScenarioUtilityGeneration.GetCombatTargetEscalationLevel (targetOverworld);
            Debug.Log ($"Scaling scenario {scenario.key} at {targetPersistent.ToLog ()} using threat rating {targetThreat} (modified by escalation level {escalationLevel})");

            int unitGroupLimitTotal = DataShortcuts.escalation.embeddedUnitGroupLimitTotal;
            int unitGroupLimitAdded = DataShortcuts.escalation.embeddedUnitGroupLimitAdded;
            
            foreach (var stepKvp in scenario.stepsProc)
            {
                var step = stepKvp.Value;

                if (step.unitGroups == null)
                    continue;

                var multiplier = 0f;
                if (step.core != null)
                    multiplier = Mathf.Clamp01 (step.core.threatRatingPercentage);

                var targetThreatForStep = multiplier * targetThreat;
                if (targetThreatForStep > 0f)
                    ScenarioUtilityGeneration.PromoteUnitGroups (targetThreatForStep, step.unitGroups, unitGroupLimitTotal, unitGroupLimitAdded, $"Escalating step {stepKvp.Key}");
            }

            // re-run this to set up the new steps we just added
            scenario.OnAfterDeserialization (scenario.key);
            
            #endif
        }
    }
}