using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public static class ScenarioUnitTags
    {
        public const string ReducedHeat = "flag_reduced_heat";
        public const string ReducedDamageIn = "flag_reduced_damage_in";
        public const string NoHeat = "flag_no_heat";
        public const string NoDamageIn = "flag_no_damage_in";
        public const string NoConcussionIn = "flag_no_concussion_in";
        public const string NoPower = "flag_no_power";
        public const string Untargetable = "flag_untargetable";
        public const string Uncollideable = "flag_uncollideable";
        
        public const string HideDisposableStatus = "hide_disposable_status";
        public const string RestrictSelection = "restrict_selection";
        public const string AutoEjectOnDestruction = "auto_eject_on_destruction";
        
        public const string InFieldWater = "in_field_water";
        public const string InFieldLava = "in_field_lava";
        public const string InFieldAcid = "in_field_acid";
    }

    public static class ScenarioFieldTypes
    {
        public const string Water = "water";
        public const string Lava = "lava";
        public const string Acid = "acid";
    }
    
    public static class CombatUIUtility
    {
        public static bool IsFactionFriendly (string faction)
        {
            return !string.Equals (Factions.enemy, faction);
        }
    }
    
    public static class ScenarioUtility
    {
         private static SortedDictionary<string, bool> tagFilterCombined = new SortedDictionary<string, bool> ();
        private static List<DataContainerCombatArea> areasFiltered = new List<DataContainerCombatArea> ();
        private static List<string> keysFiltered = new List<string> ();
        private static List<string> keysFilteredArea = new List<string> ();
        private static SortedDictionary<string, bool> areaRequirementsCombined = new SortedDictionary<string, bool> ();
        
        public static bool predictionEnabled = true;

        private const string areaTagContextPrefix = "context_";
        private static string areaKeySelectedLast = null;
        private static Dictionary<string, string> areaSelectionsPerContext = new Dictionary<string, string> ();

        public static string GetAreaKeyBiased (SortedDictionary<string, bool> filterFromScenario, SortedDictionary<string, bool> filterFromSite)
        {
            if (filterFromScenario == null)
            {
                Debug.LogWarning ($"Area selection | Failed to find any combat areas, no filter from scenario provided");
                return null;
            }

            areaRequirementsCombined.Clear ();
            string tagContext = null;

            if (filterFromScenario != null)
            {
                foreach (var kvp in filterFromScenario)
                {
                    var tag = kvp.Key;
                    areaRequirementsCombined.Add (tag, kvp.Value);

                    if (kvp.Value && tag.StartsWith (areaTagContextPrefix))
                        tagContext = tag;
                }
            }

            if (filterFromSite != null)
            {
                foreach (var kvp in filterFromSite)
                {
                    if (!areaRequirementsCombined.ContainsKey (kvp.Key))
                    {
                        var tag = kvp.Key;
                        areaRequirementsCombined.Add (tag, kvp.Value);

                        if (kvp.Value && tag.StartsWith (areaTagContextPrefix))
                            tagContext = tag;
                    }
                }
            }
            
            bool contextUsed = !string.IsNullOrEmpty (tagContext);
            var areaCandidates = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, areaRequirementsCombined);
            var areaCandidatesCount = areaCandidates.Count;
            
            if (areaCandidatesCount == 0)
            {
                Debug.LogWarning ($"Area selection | Failed to find any combat areas matching combined tag requirements: \n{areaRequirementsCombined.ToStringFormattedKeys (true, multilinePrefix: "- ")}");
                return null;
            }

            if (areaCandidatesCount == 1)
            {
                var keySelected = areaCandidates[0];
                areaKeySelectedLast = keySelected;
                if (contextUsed)
                    areaSelectionsPerContext[tagContext] = keySelected;
                
                Debug.LogWarning ($"Area selection | Selected area: {keySelected} | No other candidates | Combined tag requirements: \n{areaRequirementsCombined.ToStringFormattedKeys (true, multilinePrefix: "- ")}");
                return keySelected;
            }

            if (!string.IsNullOrEmpty (areaKeySelectedLast) && areaCandidates.Contains (areaKeySelectedLast))
            {
                Debug.LogWarning ($"Area selection | Removing last selected area from candidates: {areaKeySelectedLast}");
                areaCandidates.Remove (areaKeySelectedLast);
                areaCandidatesCount -= 1;
            }

            if (areaCandidatesCount > 1 && contextUsed && areaSelectionsPerContext.TryGetValue (tagContext, out var keyUsedLast) && areaCandidates.Contains (keyUsedLast))
            {
                Debug.LogWarning ($"Area selection | Context tag: {tagContext} | Removing last selected area for this context from candidates: {keyUsedLast}");
                areaCandidates.Remove (keyUsedLast);
                areaCandidatesCount -= 1;
            }

            int randomIndex = UnityEngine.Random.Range (0, areaCandidatesCount);
            var keySelectedRandom = areaCandidates[randomIndex];
            areaKeySelectedLast = keySelectedRandom;
            if (contextUsed)
                areaSelectionsPerContext[tagContext] = keySelectedRandom;
            
            Debug.Log ($"Area selection | Selected area: {keySelectedRandom} | Candidates:\n{areaCandidates.ToStringFormatted (true, multilinePrefix: "- ")}\nCombined tag requirements: \n{areaRequirementsCombined.ToStringFormattedKeys (true, multilinePrefix: "- ")}");
            return keySelectedRandom;
        }

        public static List<string> GetAreaKeysFromSite (DataContainerOverworldEntityBlueprint blueprint)
        {
            keysFilteredArea.Clear ();
            
            if (blueprint == null)
                return keysFilteredArea;

            bool filterUsed = blueprint.areasProcessed != null && blueprint.areasProcessed.tags != null && blueprint.areasProcessed.tags.Count > 0;
            if (filterUsed)
            {
                var areasMatchingTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, blueprint.areasProcessed.tags);
                return areasMatchingTag;
            }
            else
            {
                keysFilteredArea.Add ("unrestricted");
                return keysFilteredArea;
            }
        }
        
        public static List<string> GetAreaKeysFromFilter (SortedDictionary<string, bool> filter, bool returnEmpty = true)
        {
            keysFilteredArea.Clear ();
            
            bool filterUsed = filter != null && filter.Count > 0;
            if (filterUsed)
            {
                var areasMatchingTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, filter);
                return areasMatchingTag;
            }
            else
            {
                if (returnEmpty)
                    keysFilteredArea.Add ("unrestricted");
                return keysFilteredArea;
            }
        }

        public static List<string> GetAreaKeysFromSiteAndScenario (DataContainerOverworldEntityBlueprint blueprint, DataContainerScenario scenario)
        {
            keysFilteredArea.Clear ();
            
            if (scenario == null || blueprint == null)
                return keysFilteredArea;

            if (!scenario.areasProc.tagFilterUsed)
            {
                if (scenario.areasProc.keys != null)
                    keysFilteredArea.AddRange (scenario.areasProc.keys);
                return keysFilteredArea;
            }

            areaRequirementsCombined.Clear ();
                
            // Size, shape tags etc
            foreach (var kvp2 in scenario.areasProc.tagFilter)
                areaRequirementsCombined.Add (kvp2.Key, kvp2.Value);
                
            SortedDictionary<string, bool> areaRequirementsFromSite = null;
            string areaKeyUnpacked = null;

            if (blueprint.areasProcessed != null && blueprint.areasProcessed.tags != null && blueprint.areasProcessed.tags.Count > 0)
            {
                foreach (var kvp2 in blueprint.areasProcessed.tags)
                {
                    if (!areaRequirementsCombined.ContainsKey (kvp2.Key))
                        areaRequirementsCombined.Add (kvp2.Key, kvp2.Value);
                }
            }

            var areasMatchingTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, areaRequirementsCombined);
            keysFilteredArea.AddRange (areasMatchingTag);
            return keysFilteredArea;
        }
        
        public static List<string> GetScenarioKeysFromSite (DataContainerOverworldEntityBlueprint blueprint, bool returnEmpty = true)
        {
            keysFiltered.Clear ();
            
            if (blueprint == null)
                return keysFiltered;

            bool filterUsed = blueprint.scenariosProcessed != null && blueprint.scenariosProcessed.tags != null && blueprint.scenariosProcessed.tags.Count > 0;
            if (filterUsed)
            {
                var scenariosMatchingTag = DataTagUtility.GetContainersWithTags (DataMultiLinkerScenario.data, blueprint.scenariosProcessed.tags);
                foreach (var scenarioCandidateObj in scenariosMatchingTag)
                {
                    var scenarioCandidate = scenarioCandidateObj as DataContainerScenario;
                    if (scenarioCandidate == null || scenarioCandidate.hidden)
                        continue;
                    
                    keysFiltered.Add (scenarioCandidate.key);
                }
                
                return keysFiltered;
            }
            else
            {
                if (returnEmpty)
                    keysFiltered.Add ("unrestricted");
                return keysFiltered;
            }
        }
    }
}