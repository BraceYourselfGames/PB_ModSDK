using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioPromoteUnitsFromEscalationLevel : ICombatScenarioGenStep
    {
        public void Run (DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            if (!DataShortcuts.escalation.unitScalingLevelBased)
            {
                Debug.Log ($"Scenario {scenario.key} won't be scaled by escalation level as this method is disabled");
                return;
            }

            var escalationLevel = ScenarioUtilityGeneration.GetCombatTargetEscalationLevel ();
            if (escalationLevel <= 0)
            {
                Debug.Log ($"Scenario {scenario.key} won't be scaled by escalation level as it is at 0");
                return;
            }

            var target = ScenarioUtility.GetCombatSite ();
            Debug.Log ($"Scaling scenario {scenario.key} at {target.ToLog ()} using escalation level {escalationLevel}");
            
            foreach (var stepKvp in scenario.stepsProc)
            {
                var step = stepKvp.Value;
                if (step == null || step.unitGroups == null)
                    continue;

                PromoteUnitGroups (escalationLevel, step.unitGroups, $"Escalating step {stepKvp.Key}");
            }
            
            foreach (var stateKvp in scenario.statesProc)
            {
                var state = stateKvp.Value;
                if (state.reactions == null || state.reactions.effectsPerIncrement == null || state.reactions.effectsPerIncrement.Count == 0)
                    continue;

                foreach (var kvpReaction in state.reactions.effectsPerIncrement)
                {
                    var reaction = kvpReaction.Value;
                    if (reaction == null || reaction.unitGroups == null)
                        continue;
                    
                    PromoteUnitGroups (escalationLevel, reaction.unitGroups, $"Escalating state {stateKvp.Key} reaction {kvpReaction.Key}");
                }
            }

            // re-run this to set up the new steps we just added
            scenario.OnAfterDeserialization (scenario.key);
            
            #endif
        }

        public static void PromoteUnitGroups (int escalationLevel, List<DataBlockScenarioUnitGroup> unitGroups, string context)
        {
            #if !PB_MODSDK

            // Just in case, bail early if there is nothing to modify - this makes it unnecessary to check/bail mid-cloning
            if (unitGroups == null || unitGroups.Count <= 0)
                return;
            
            // Determine what to do depending on escalation level
            int gradeIndexMin = 0;
            int gradeIndexMax = 0;
            float gradeProportion = 0f;
            int cloneCount = 0;
            
            // G0 - Common/Easy
            // G1 - Veteran/Medium
            // G2 - Elite/Hard
            
            // 0
            // Do nothing
            
            // 1
            // Set 50% of unit groups to min (G1, Gmax)
            // - If there is 1 unit group, G0
            // - If there are 2 unit groups, G1+G0
            // - If there are 3 unit groups, G1+G0+G0
            // - If there are 4 unit groups, G1+G1+G0+G0, etc.
            
            // 2
            // Set 50% of unit groups to min (G2, Gmax)
            // Set remaining unit groups to min (G1, Gmax)
            // - If there is 1 unit group, G1
            // - If there are 2 unit groups, G2+G1
            // - If there are 3 unit groups, G2+G1+G1
            // - If there are 4 unit groups, G2+G2+G1+G1, etc.
            
            // 3
            // Set 100% of unit groups to min (G2, Gmax)
            // - If there is 1 unit group, G2
            // - If there are 2 unit groups, G2+G2
            // - If there are 3 unit groups, G2+G2+G2
            // - If there are 4 unit groups, G2+G2+G2+G2, etc.

            if (escalationLevel <= 1)
            {
                gradeIndexMin = 0;
                gradeIndexMax = 1;
                gradeProportion = 0.5f;
                cloneCount = 0;
            }
            else if (escalationLevel == 2)
            {
                gradeIndexMin = 1;
                gradeIndexMax = 2;
                gradeProportion = 0.5f;
                cloneCount = 0;
            }
            if (escalationLevel == 3)
            {
                gradeIndexMin = 2;
                gradeIndexMax = 2;
                gradeProportion = 1f;
                cloneCount = 0;
            }
            
            float fractionalIndex = 0f;
            for (int i = 0, limit = unitGroups.Count; i < limit; ++i)
            {
                fractionalIndex += gradeProportion;
                bool fractionReached = fractionalIndex >= 1f;
                
                if (fractionReached)
                    fractionalIndex -= 1f;
                
                var group = unitGroups[i];
                int gradeIndexCurrent = group.baseGrade;

                int gradeIndexEscalated;
                if (gradeProportion >= 1f)
                    gradeIndexEscalated = gradeIndexMax;
                else if (gradeProportion <= 0f)
                    gradeIndexEscalated = gradeIndexMin;
                else
                    gradeIndexEscalated = fractionReached ? gradeIndexMax : gradeIndexMin;
                
                int gradeIndexFinal = Mathf.Min (gradeIndexEscalated, group.maxGrade);
                if (gradeIndexFinal != gradeIndexCurrent)
                {
                    Debug.Log ($"{context} | Changing group {i} ({group.origin}) grade from {gradeIndexCurrent} to {gradeIndexFinal}/{group.maxGrade} | Grades from escalation level {escalationLevel}: {gradeIndexMin}-{gradeIndexMax} | Fractional index: {fractionalIndex:0.##} | Proportion: {gradeProportion:0.##} | Fraction reached: {fractionReached}\n- {group.GetDescription ()}");
                    group.baseGrade = gradeIndexEscalated;
                }
            }
            
            /*
            for (int i = 0, limit = unitGroups.Count; i < limit; ++i)
            {
                var group = unitGroups[i];
                var gradeIndexCurrent = group.baseGrade;

                var gradeIndexEscalated = gradeIndexMin;
                if (gradeProportion >= 1f)
                    gradeIndexEscalated = gradeIndexMax;
                else if (gradeProportion.RoughlyEqual (0.5f))
                    gradeIndexEscalated = i % 2 > 0 ? gradeIndexMax : gradeIndexMin;
                else if (gradeProportion.RoughlyEqual (0.33f))
                    gradeIndexEscalated = i % 3 > 1 ? gradeIndexMax : gradeIndexMin;
                else if (gradeProportion.RoughlyEqual (0.66f))
                    gradeIndexEscalated = i % 3 > 0 ? gradeIndexMax : gradeIndexMin;
                else if (gradeProportion.RoughlyEqual (0.25f))
                    gradeIndexEscalated = i % 4 > 2 ? gradeIndexMax : gradeIndexMin;
                else if (gradeProportion.RoughlyEqual (0.75f))
                    gradeIndexEscalated = i % 4 > 0 ? gradeIndexMax : gradeIndexMin;
                
                var gradeIndexFinal = Mathf.Min (gradeIndexEscalated, group.maxGrade);
                if (gradeIndexFinal != gradeIndexCurrent)
                {
                    Debug.Log ($"{context} | Changing group {i} grade from {gradeIndexCurrent} to {gradeIndexFinal} | Grade from escalation: {gradeIndexEscalated} | Limit: {group.maxGrade}");
                    group.baseGrade = gradeIndexEscalated;
                }
            }
            */
            
            #endif
        }
    }
}