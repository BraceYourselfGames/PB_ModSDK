using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class ScenarioAddParentInjected : ICombatScenarioGenStep
    {
        private static Dictionary<string, List<string>> injectionCandidatesPerGroup = new Dictionary<string, List<string>> ();
        
        public void Run (DataContainerScenario scenario, int seed)
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null)
                return;
            
            var targetPersistent = ScenarioUtility.GetCombatSite ();
            var targetOverworld = IDUtility.GetLinkedOverworldEntity (targetPersistent);
            if (targetOverworld == null || !targetOverworld.hasDataLinkOverworldEntityBlueprint)
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

                if (gi.tagFilter != null && gi.tagFilter.Count > 0)
                {
                    bool match = true;
                    foreach (var kvp2 in gi.tagFilter)
                    {
                        var tag = kvp2.Key;
                        bool required = kvp2.Value;
                        bool present = scenarioTags.Contains (tag);
                        if (required != present)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                }
                
                if (gi.functionsBase != null)
                {
                    bool match = true;
                    foreach (var function in gi.functionsBase)
                    {
                        if (function != null)
                        {
                            bool valid = function.IsValid (basePersistent);
                            if (!valid)
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                    
                    if (!match)
                        continue;
                }

                if (gi.functionsSite != null)
                {
                    bool match = true;
                    foreach (var function in gi.functionsSite)
                    {
                        if (function != null)
                        {
                            bool valid = function.IsValid (targetPersistent);
                            if (!valid)
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                    
                    if (!match)
                        continue;
                }

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