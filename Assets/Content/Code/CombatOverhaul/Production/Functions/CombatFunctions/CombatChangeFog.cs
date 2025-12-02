using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatChangeFog : ICombatFunction
    {
        public bool fogChangeInstant;
        
        [PropertyRange (0f, 1f)]
        public float fogIntensityTarget = 0.33f;

        [PropertyRange (0.1f, 10f)]
        [HideIf ("fogChangeInstant")]
        public float fogProgressionSpeed = 0.2f;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (fogChangeInstant)
                PostprocessingHelper.SetFogTarget (fogIntensityTarget, true);
            else
            {
                PostprocessingHelper.SetFogProgressionSpeed (fogProgressionSpeed);
                PostprocessingHelper.SetFogTarget (fogIntensityTarget, false);
            }
            
            #endif
        }
    }

    public class CombatReserveReward : ICombatFunction
    {
        [InfoBox ("Note: these keys are for collapsed reward dictionary entries on point presets, not for actual reward DB entries")]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioRewardKeys")]
        public SortedDictionary<string, int> rewards = new SortedDictionary<string, int> ();
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (rewards != null && rewards.Count > 0)
            {
                var persistent = Contexts.sharedInstance.persistent;
                var rewardsCurrent = persistent.combatScenarioStateRewardsDelayed.s;
                
                foreach (var kvp2 in rewards)
                {
                    if (rewardsCurrent.TryGetValue (kvp2.Key, out int countCurrent))
                        rewardsCurrent[kvp2.Key] = kvp2.Value + countCurrent;
                    else
                        rewardsCurrent.Add (kvp2.Key, kvp2.Value);
                    
                    Debug.Log ($"Adding reward {kvp2.Key} (x{kvp2.Value})");
                }
                        
                persistent.ReplaceCombatScenarioStateRewardsDelayed (rewardsCurrent);
            }
            
            #endif
        }
    }
    
    public class CombatReserveRewardExact : ICombatFunction
    {
        public string groupKey;
        
        [InfoBox ("Note: these keys are for the specific reward DB entries, and not for the collapsed reward dictionary on point presets")]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldReward.data.Keys")]
        public SortedDictionary<string, int> rewards = new SortedDictionary<string, int> ();
        
        public void Run ()
        {
            #if !PB_MODSDK

            if (rewards == null || rewards.Count == 0)
                return;
            
            if (string.IsNullOrEmpty (groupKey))
                return;

            var persistent = Contexts.sharedInstance.persistent;
            var cd = ScenarioUtility.GetCurrentCombatDescription ();
            if (cd == null)
                return;
            
            if (cd.rewardGroupsCollapsed == null)
                cd.rewardGroupsCollapsed = new SortedDictionary<string, CombatRewardGroupCollapsed> ();
            
            if (!cd.rewardGroupsCollapsed.TryGetValue (groupKey, out var group))
            {
                group = new CombatRewardGroupCollapsed ();
                cd.rewardGroupsCollapsed.Add (groupKey, group);
            }
            
            if (group.rewards == null)
                group.rewards = new SortedDictionary<string, int> ();
                
            group.type = PointRewardType.CombatVictory;

            foreach (var kvp2 in rewards)
            {
                var rewardKey = kvp2.Key;
                int rewardCount = kvp2.Value;
                
                if (group.rewards.TryGetValue (rewardKey, out var rewardCountCurrent))
                    group.rewards[rewardKey] = kvp2.Value + rewardCountCurrent;
                else
                    group.rewards.Add (rewardKey, rewardCount);
                
                Debug.Log ($"Adding reward {kvp2.Key} (x{kvp2.Value}) to reward group {groupKey}");
            }

            #endif
        }
    }
}