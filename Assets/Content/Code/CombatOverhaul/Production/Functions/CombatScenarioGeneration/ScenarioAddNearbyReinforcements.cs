using System.Collections.Generic;
using PhantomBrigade.Functions;

namespace PhantomBrigade.Data
{
    public class ScenarioAddNearbyReinforcements : ICombatScenarioGenStep
    {
        private List<OverworldEntity> reinforcementProviderListModified = new List<OverworldEntity> ();
    
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            /*
            // If this is 0, no point continuing
            var reinforcementProviderLimit = ScenarioUtilityGeneration.GetReinforcementProviderLimit (targetOverworld);
            if (reinforcementProviderLimit <= 0)
                return;

            var sourceOverworld = IDUtility.playerBaseOverworld;
            if (sourceOverworld == null || !sourceOverworld.hasPosition)
                return;

            // The method excepts entities with CombatParticipant component, but just in case, let's pass excepted entity directly
            // This will keep this implementation more resilient to future changes, e.g. if combat participants get marked after generator runs
            var reinforcementProviderList = OverworldUtility.GetReinforcementProviderList (targetOverworld, false, true);
            if (reinforcementProviderList == null || reinforcementProviderList.Count == 0)
                return;
            
            if (!scenario.coreProc.reinforcementsUsed)
            {
                Debug.Log ($"Skipping reinforcements on target {targetOverworld.ToLog ()} based on scenario {scenario.key} preferences");
                return;
            }
            
            bool targetAllowsReinforcements =
                targetOverworld != null &&
                targetOverworld.hasDataLinkOverworldEntityBlueprint &&
                targetOverworld.dataLinkOverworldEntityBlueprint.data.tagsProcessed != null &&
                !targetOverworld.dataLinkOverworldEntityBlueprint.data.tagsProcessed.Contains ("feature_no_reinforcements");

            if (!targetAllowsReinforcements)
            {
                Debug.Log ($"Skipping reinforcements on target {targetOverworld.ToLog ()} based on blueprint tag");
                return;
            }
            
            // Copy returned results from utility into a separate list, as the utility reuses same object to avoid allocating new list every time
            // We want to make modifications to that list, so we should do them on an independent object
            reinforcementProviderListModified.Clear ();
            reinforcementProviderListModified.AddRange (reinforcementProviderList);

            var reinforcementTurns = new HashSet<int> ();
            var basePosition = sourceOverworld.position.v;
            
            reinforcementProviderListModified.Sort ((a, b) => 
                Vector3.Distance (a.position.v, basePosition).CompareTo (Vector3.Distance (b.position.v, basePosition)));

            int reinforcementProviderCount = reinforcementProviderListModified.Count;
            if (reinforcementProviderLimit < reinforcementProviderCount)
            {
                reinforcementProviderListModified.RemoveRange (reinforcementProviderLimit, reinforcementProviderCount - reinforcementProviderLimit);
                Debug.Log ($"Cutting the list of eligible reinforcement providers from {reinforcementProviderCount} to {reinforcementProviderLimit}");
            }

            var persistentIDs = new List<int> ();
            foreach (var providerOverworld in reinforcementProviderListModified)
            {
                var providerPersistent = IDUtility.GetLinkedPersistentEntity (providerOverworld);
                if (providerPersistent == null)
                    continue;

                var blueprint = providerOverworld.dataLinkOverworldEntityBlueprint.data;
                var reinforcementBlock = blueprint.reinforcementsProcessed;
                
                if (reinforcementBlock == null)
                {
                    Debug.Log ($"Couldn't find any reinforcement data on blueprint {blueprint.key} of entity {providerOverworld.ToLog ()}");
                    continue;
                }

                float threatMultiplier = Mathf.Clamp01 (reinforcementBlock.threatMultiplier);
                var reinforcementKeys = reinforcementBlock.reinforcementKeys;
                var reinforcementKey = reinforcementKeys?.GetRandomEntry ();
                var reinforcementData = DataMultiLinkerCombatReinforcement.GetEntry (reinforcementKey);
                
                if (reinforcementData == null || reinforcementData.unitGroups == null || reinforcementData.unitGroups.Count == 0)
                {
                    Debug.Log ($"Couldn't find any reinforcement data on blueprint {blueprint.key} of entity {providerOverworld.ToLog ()}");
                    continue;
                }
                
                // figure out what turn the wave should arrive at
                var distance = Vector3.Distance (providerOverworld.position.v, basePosition);
                var turn = Mathf.RoundToInt (distance / DataShortcuts.overworld.overworldDistancePerReinforcementTurn) + 1;
                while (reinforcementTurns.Contains (turn))
                {
                    turn++;
                }

                if (turn > 10)
                {
                    Debug.Log ($"Skipping enemy reinforcement from entity {providerOverworld.ToLog ()} due to turn being too high: {turn}");
                    continue;
                }

                // generate a new unit group for this wave
                var sourceUnitGroups = reinforcementData.unitGroups;
                var generatedUnitGroups = new List<DataBlockScenarioUnitGroup> ();
                
                foreach (var unitGroup in sourceUnitGroups)
                    generatedUnitGroups.Add (UtilitiesYAML.CloneThroughYaml (unitGroup));

                int unitGroupLimitTotal = DataShortcuts.escalation.reinforcementUnitGroupLimitTotal;
                int unitGroupLimitAdded = DataShortcuts.escalation.reinforcementUnitGroupLimitAdded;
                
                // Don't get threat from entity, which relates to original scenario - old implementation
                // int threat = context.GetTargetThreat (providerPersistent);
                
                // Get base threat from units we start with - alternative proposal
                // float threat = IncreaseUnitGradeWithEscalation.ThreatRating (generatedUnitGroups);
                
                // Get base threat from entity, but don't trust final calculated value from overworld, multiply it first, then modify by escalation
                float threat = providerPersistent.hasThreatRatingBase ? providerPersistent.threatRatingBase.f : 0f;
                threat *= threatMultiplier;
                
                // Get escalation, modify the base threat by it
                var provinceOfOrigin = providerOverworld.hasProvinceSpawnOwner ? IDUtility.GetOverworldEntity (providerOverworld.provinceSpawnOwner.provinceNameInternal) : null;
                int escalationLevel = 0;
                if (provinceOfOrigin != null)
                {
                    escalationLevel = provinceOfOrigin.hasProvinceEscalationLevel ? provinceOfOrigin.provinceEscalationLevel.level : 0;
                    threat += escalationLevel * DataShortcuts.escalation.threatIncreasePerLevel;
                }

                ScenarioUtilityGeneration.PromoteUnitGroups (threat, generatedUnitGroups, unitGroupLimitTotal, unitGroupLimitAdded, "Neighbor");

                // Ensure state collection is present in the scenario
                if (scenario.statesProc == null)
                    scenario.statesProc = new SortedDictionary<string, DataBlockScenarioState> ();
                
                // The state key must use the internal name of the entity and never its ID.
                // This is very important, exactly like using internal name of primary host in saved combat setup.
                // Doing that ensures that generated state name remains stable across sessions, even after game is reloaded.
                // With IDs, one session might churn through a lot and give a patrol ID 1000, and another session might give it ID 250.
                // With internal names, save/load system guarantees that same entity will always have same name.
                // The reason why we care about the state name in this context is because entities that are indirect participants
                // remember the state name in the generated scenario that depends on them. This allows for side effects after combat.
                // As players can reload in briefing (past generation) very often, it's critical that entity and scenario remain in sync here.
                var stateKey = $"auto_rnf_t{turn}_{providerOverworld.nameInternal.s}";
                
                scenario.statesProc.Add (stateKey, new DataBlockScenarioState ()
                {
                    textNameKey = "reinforcements_hostile_header",
                    textDescKey = "reinforcements_hostile_text",
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
                        effectsPerIncrement = new Dictionary<int, DataBlockScenarioStateReaction> ()
                        {
                            {1, new DataBlockScenarioStateReaction () {unitGroups = generatedUnitGroups}}
                        },
                        triggerLimit = 1,
                        expectedValue = true,
                        scopeRemovalOnLimit = true
                    }
                });

                persistentIDs.Add (providerPersistent.id.id);
                reinforcementTurns.Add (turn);
                providerPersistent.ReplaceCombatParticipantIndirect (stateKey, false);
                
                if (DataShortcuts.sim.logCombatRequests)
                    Debug.Log ($"Adding enemy reinforcement state from entity {providerOverworld.ToLog ()} | Turn: {turn} | Unit groups: {generatedUnitGroups.Count} | Threat: {threat}");
            }
            
            */
            
            #endif
        }
    }
}