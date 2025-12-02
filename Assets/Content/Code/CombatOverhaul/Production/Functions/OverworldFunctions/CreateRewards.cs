using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Linking;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreateRewards : IOverworldFunction
    {
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldReward.data.Keys")]
        public SortedDictionary<string, int> rewards = new SortedDictionary<string, int> ();
        
        public void Run ()
        {
            #if !PB_MODSDK

            EquipmentUtility.PrepareRewardsStandalone (rewards);
            
            #endif
        }
    }
    
    [Serializable]
    public class CreateRewardsFiltered : IOverworldFunction
    {
        public bool randomSingleEntry = false;
        
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldReward.data.Keys")]
        public List<DataBlockOverworldPointReward> rewards = new List<DataBlockOverworldPointReward> ();

        [YamlIgnore, ReadOnly]
        public bool generated = false;
        
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, int> rewardsProcessed = new SortedDictionary<string, int> ();
        
        [YamlIgnore, ReadOnly]
        public static List<string> rewardKeysFiltered = new List<string> ();

        [Button ("Generate")]
        public void Generate ()
        {
            generated = true;
            rewardsProcessed.Clear ();
            
            if (rewards == null || rewards.Count == 0)
                return;

            if (randomSingleEntry)
            {
                var rewardSlot = rewards.GetRandomEntry ();
                AppendSlot (rewardSlot);
            }
            else
            {
                foreach (var rewardSlot in rewards)
                    AppendSlot (rewardSlot);
            }
        }

        private void AppendSlot (DataBlockOverworldPointReward rewardSlot)
        {
            if (rewardSlot == null)
                return;

            var chanceRandom = UnityEngine.Random.Range (0f, 1f);
            if (chanceRandom > rewardSlot.chance)
                return;

            var rewardData = DataMultiLinkerOverworldReward.data;
            rewardKeysFiltered.Clear ();
            if (rewardSlot.tagsUsed)
            {
                var keys = DataTagUtility.GetKeysWithTags (rewardData, rewardSlot.tags);
                if (keys != null && keys.Count > 0)
                    rewardKeysFiltered.AddRange (keys);
            }
            else if (rewardSlot.keys != null && rewardSlot.keys.Count > 0)
                rewardKeysFiltered.AddRange (rewardSlot.keys);

            if (rewardKeysFiltered == null || rewardKeysFiltered.Count == 0)
                return;

            var rewardKeySelected = rewardKeysFiltered.GetRandomEntry ();
            var rewardPreset = DataMultiLinkerOverworldReward.GetEntry (rewardKeySelected);
            if (rewardPreset == null)
                return;

            int count = Mathf.Clamp (rewardSlot.repeats, 1, 10);
            if (rewardsProcessed.ContainsKey (rewardKeySelected))
                rewardsProcessed[rewardKeySelected] += count;
            else
                rewardsProcessed.Add (rewardKeySelected, count);
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (!generated)
                Generate ();

            EquipmentUtility.PrepareRewardsStandalone (rewardsProcessed);
            
            #endif
        }
    }
}