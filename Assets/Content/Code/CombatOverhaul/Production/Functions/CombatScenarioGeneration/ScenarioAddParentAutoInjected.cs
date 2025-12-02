using PhantomBrigade.Functions;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddParentInjected : ICombatScenarioGenStep
    {
        private static Dictionary<string, List<string>> injectionCandidatesPerGroup = new Dictionary<string, List<string>> ();
        
        public void Run (OverworldEntity targetOverworld, DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null)
                return;

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (targetOverworld);
            if (targetPersistent == null || targetOverworld == null || !targetOverworld.hasDataLinkPointPreset)
            {
                // Debug.LogWarning ($"Skipping scenario changes from site: {targetOverworld.ToLog ()} has no blueprint");
                return;
            }
            
            if (scenario.tagsProc == null || scenario.tagsProc.Count == 0)
                return;

            injectionCandidatesPerGroup.Clear ();
            var scenariosAll = DataMultiLinkerScenario.data;
            var scenarioTags = scenario.tagsProc;
            
            foreach (var kvp in scenariosAll)
            {
                var scenarioCandidate = kvp.Value;
                if (scenarioCandidate.hidden || scenarioCandidate.generationInjection == null)
                    continue;

                var gi = scenarioCandidate.generationInjection;
                if (!gi.enabled || string.IsNullOrEmpty (gi.group))
                    continue;
                
                bool injectionPossible = gi.IsInjectionPossible (scenario.tagsProc, targetPersistent);
                if (!injectionPossible)
                    continue;

                if (!injectionCandidatesPerGroup.ContainsKey (gi.group))
                    injectionCandidatesPerGroup.Add (gi.group, new List<string> { scenarioCandidate.key });
                else
                    injectionCandidatesPerGroup[gi.group].Add (scenarioCandidate.key);
            }

            if (injectionCandidatesPerGroup.Count == 0)
                return;
            
            bool log = DataShortcuts.sim.logScenarioGeneration;
            if (log)
            {
                var report = injectionCandidatesPerGroup.ToStringFormattedKeyValuePairs (toStringOverride: x => x.ToStringFormatted ());
                Debug.Log ($"Scenario {scenario.key} has {injectionCandidatesPerGroup.Count} compatible injection groups:\n{report}");
            }

            foreach (var kvp in injectionCandidatesPerGroup)
            {
                var group = kvp.Key;
                var candidateKeys = kvp.Value;
                if (candidateKeys == null || candidateKeys.Count == 0)
                    continue;
                
                var candidateKey = candidateKeys.GetRandomEntry ();
                TryInjectingParent (scenario, group, candidateKey);
            }

            #endif
        }
        
        private void TryInjectingParent (DataContainerScenario scenario, string group, string parentKey)
        {
            #if !PB_MODSDK

            if (string.IsNullOrEmpty (parentKey))
                return;
                
            bool log = DataShortcuts.sim.logScenarioGeneration;
            if (scenario.parents != null)
            {
                bool present = false;
                foreach (var parent in scenario.parents)
                {
                    if (parent != null && parent.key.Equals (parentKey))
                    {
                        if (log)
                            Debug.LogWarning ($"Skipping scenario parent {parentKey} from injection group {group} on generated scenario {scenario.key}: parent already present");

                        present = true;
                        break;
                    }
                }

                if (present)
                    return;
            }
            
            if (log)
                Debug.Log ($"Adding scenario parent {parentKey} from injection group {group} to generated scenario {scenario.key}");
            
            if (scenario.parents == null)
                scenario.parents = new List<DataContainerScenarioParent> ();
            
            scenario.parents.Add (new DataContainerScenarioParent { key = parentKey });
            
            #endif
        }
    }
}