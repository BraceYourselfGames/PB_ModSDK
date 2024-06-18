using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddRequestedHomeGuard : ICombatScenarioGenStep
    {
        private StringBuilder sb = new StringBuilder ();
    
        public void Run (DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            var modifiersActive = ScenarioUtility.GetCombatModifiers ();
            if (!modifiersActive.Contains (CombatModifierKeys.CallHomeGuard))
                return;

            var reinforcementConfigKey = "home_guard";
            var reinforcementConfig = DataMultiLinkerCombatReinforcement.GetEntry (reinforcementConfigKey);
            if (reinforcementConfig == null)
            {
                Debug.LogWarning ($"Tried to generate supporting unit group due to combat modifier, but failed to find reinforcement config {reinforcementConfigKey}");
                return;
            }
            
            var basePersistent = ScenarioUtility.GetCombatSite ();
            basePersistent.TryGetMemoryRounded (EventMemoryInt.World_Counter_Resistance_Reputation, out int reputation);
            
            int grade = reputation;
            
            // If we're in friendly province, it's nice to get a little boost to reputation
            bool provinceFriendly = OverworldUtility.IsCurrentProvinceFriendly ();
            if (provinceFriendly)
                grade += 1;
            
            var maxReputation = DataLinkerSettingsEscalation.data.maxReputationWithHomeGuard;
            if (reputation + 1 >= maxReputation)
                AchievementHelper.UnlockAchievement (Achievement.FullHelp);

            var sourceUnitGroups = reinforcementConfig.unitGroups;
            var generatedUnitGroups = new List<DataBlockScenarioUnitGroup> ();
            
            // Ensure the right version of HG arrives to the fight
            foreach (var unitGroup in sourceUnitGroups)
            {
                var unitGroupCopy = UtilitiesYAML.CloneThroughYaml (unitGroup);
                unitGroupCopy.baseGrade = Mathf.Clamp (grade, 0, 3);
                unitGroupCopy.maxGrade = unitGroupCopy.baseGrade;
                generatedUnitGroups.Add (unitGroupCopy);
            }

            // Fixed to 1 for now
            int turn = 1;
            
            // Ensure state collection is present in the scenario
            if (scenario.statesProc == null)
                scenario.statesProc = new SortedDictionary<string, DataBlockScenarioState> ();
            
            var stateKey = $"auto_homeguard_t{turn}";
            var state = new DataBlockScenarioState
            {
                textNameKey = "reinforcements_friendly_header",
                textDescKey = "reinforcements_friendly_text",
                evaluationContext = ScenarioStateRefreshContext.OnExecutionEnd,
                startInScope = true,
                turn = new DataBlockScenarioSubcheckTurn ()
                {
                    check = IntCheckMode.GreaterEqual,
                    relative = false,
                    value = turn
                },
                reactions = new DataBlockScenarioStateReactions ()
                {
                    triggerLimit = 1,
                    expectedValue = true,
                    scopeRemovalOnLimit = true,
                    effectsPerIncrement = new Dictionary<int, DataBlockScenarioStateReaction> ()
                    {
                        {
                            1, 
                            new DataBlockScenarioStateReaction ()
                            {
                                unitGroups = generatedUnitGroups,
                                commsOnStart = new List<DataBlockScenarioCommLink>
                                {
                                    new DataBlockScenarioCommLink
                                    {
                                        time = 0,
                                        key = "radio_home_guard_active"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            scenario.statesProc.Add (stateKey, state);
            
            #endif
        }
    }
}