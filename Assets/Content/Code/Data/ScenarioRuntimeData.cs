using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade
{
    public class CombatRewardGroupCollapsed
    {
        public PointRewardType type;
        public SortedDictionary<string, int> rewards;
    }
    
    public class CombatDescription
    {
        // Scenario key and seed used to generate it
        public string scenarioKey;
        public int scenarioSeed;

        [YamlIgnore, FoldoutGroup ("Scenario")]
        public DataContainerScenario scenarioGenerated;

        // Area and biome the scenario takes place in
        public string areaKey;
        public string biomeKey;
        
        // The key of spawn group in area used for player squad
        public string spawnKeyPlayer = null;
        
        // The lookup of scenario step keys to spawn group keys (relative to indexes of unit group slots in a given scenario step)
        public SortedDictionary<string, SortedDictionary<int, CombatSetupStepSlot>> spawnsAndGroupsPerStep = null;
        
        // The lookup of scenario state keys to spawn group keys (relative to indexes of unit group slots in a given scenario step)
        public SortedDictionary<string, SortedDictionary<int, SortedDictionary<int, CombatSetupStepSlot>>> spawnsAndGroupsPerState = null;
        
        // The lookup of which spawn groups in a scenario were already used, and when used, how many points in them were taken
        public SortedDictionary<string, int> spawnPointsUsed = null;
        
        // Mapping of state keys to area location keys
        public SortedDictionary<string, string> stateLocations;
        
        // Mapping of state keys to area volume keys
        public SortedDictionary<string, string> stateVolumes;

        // Rewards
        public SortedDictionary<string, CombatRewardGroupCollapsed> rewardGroupsCollapsed;

        [YamlIgnore]
        public bool ignoreNextReset = false;
        
        #if !PB_MODSDK

        public void ResetForGeneration 
        (
            string scenarioKey, 
            int scenarioSeed, 
            string areaKey, 
            string biomeKey, 
            OverworldEntity targetOverworld
        )
        {
            this.scenarioKey = scenarioKey;
            this.scenarioSeed = scenarioSeed;
                
            this.areaKey = areaKey;
            this.biomeKey = biomeKey;

            spawnKeyPlayer = null;

            if (spawnsAndGroupsPerStep == null)
                spawnsAndGroupsPerStep = new SortedDictionary<string, SortedDictionary<int, CombatSetupStepSlot>> ();
            else
                spawnsAndGroupsPerStep.Clear ();

            if (spawnsAndGroupsPerState == null)
                spawnsAndGroupsPerState = new SortedDictionary<string, SortedDictionary<int, SortedDictionary<int, CombatSetupStepSlot>>> ();
            else
                spawnsAndGroupsPerState.Clear ();

            if (spawnPointsUsed == null)
                spawnPointsUsed = new SortedDictionary<string, int> ();
            else
                spawnPointsUsed.Clear ();

            if (stateLocations == null)
                stateLocations = new SortedDictionary<string, string> ();
            else
                stateLocations.Clear ();

            if (stateVolumes == null)
                stateVolumes = new SortedDictionary<string, string> ();
            else
                stateVolumes.Clear ();

            if (rewardGroupsCollapsed == null)
                rewardGroupsCollapsed = new SortedDictionary<string, CombatRewardGroupCollapsed> ();
            else
                rewardGroupsCollapsed.Clear ();
            
            if (scenarioGenerated == null)
                scenarioGenerated = new DataContainerScenario ();

            int targetOverworldID = targetOverworld.id.id;
            
            scenarioGenerated.Clear ();
            scenarioGenerated.key = $"{scenarioKey}_gen_{targetOverworldID}";
            
            if (scenarioGenerated.parents == null)
                scenarioGenerated.parents = new List<DataContainerScenarioParent> ();
            else
                scenarioGenerated.parents.Clear ();
            scenarioGenerated.parents.Add (new DataContainerScenarioParent { key = scenarioKey });
        }

        public void ProcessOnLoading (OverworldEntity targetOverworld)
        {
            if (targetOverworld == null)
                return;
            
            // The only part of the UpdateCombatDescription that's supposed to run on load
            if (scenarioGenerated == null)
                scenarioGenerated = new DataContainerScenario ();

            int targetOverworldID = targetOverworld.id.id;
            scenarioGenerated.Clear ();
            scenarioGenerated.key = $"{scenarioKey}_gen_{targetOverworldID}";
            
            if (scenarioGenerated.parents == null)
                scenarioGenerated.parents = new List<DataContainerScenarioParent> ();
            else
                scenarioGenerated.parents.Clear ();
            scenarioGenerated.parents.Add (new DataContainerScenarioParent { key = scenarioKey });
            
            // This fills detailed scenario data that can't be saved/loaded
            ScenarioSetupUtility.RegenerateScenario (this, targetOverworld);

            // Another fix to PB-16928 (Scenario data in briefing saves is ignored, combat is randomized on each load)
            // Required because the fix to PB-16879 (Escalation does not correctly apply to scenario squads) has a side effect of triggering on load, resetting everything
            ignoreNextReset = true;
        }
        
        #endif
    }
}

