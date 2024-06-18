using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TransferBlueprintRewards : IOverworldEventFunction
    {
        [DictionaryValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.data.Keys")]
        public string blueprintKey;
    
        public string rewardKey;
        
        [Range(1, 10)]
        public int rewardCount;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
            var basePersistent = IDUtility.playerBasePersistent;
            var baseOverworld = IDUtility.playerBaseOverworld;

            if (basePersistent == null || baseOverworld == null)
            {
                Debug.LogError ($"TransferTargetRewards | Event function failed due to missing base");
                return;
            }

            var blueprint = DataMultiLinkerOverworldEntityBlueprint.GetEntry (blueprintKey);
            if (blueprint == null)
            {
                Debug.LogError ($"TransferTargetRewards | Event function failed due to invalid blueprint key ({blueprintKey})");
                return;
            }
            
            var rewards = blueprint.rewardsProcessed;
            if (rewards == null || rewards.blocks == null || string.IsNullOrEmpty (rewardKey) || !rewards.blocks.ContainsKey (rewardKey))
            {
                Debug.LogWarning ($"TransferTargetRewards | Failed to process loot objective capture rewards: host ({target.ToLog ()}) blueprint {blueprint.key} has no rewards dictionary or no entry for key {rewardKey}");
                return;
            }

            var blocks = rewards.blocks[rewardKey];
            if (blocks == null || blocks.Count == 0)
            {
                Debug.LogWarning ($"TransferTargetRewards | Failed to process loot objective capture rewards: host ({target.ToLog ()}) blueprint {blueprint.key} rewards dictionary at key {rewardKey} has null or empty list of blocks");
                return;
            }

            var provinceCurrentBlueprint = DataHelperProvince.GetProvinceBlueprintAtEntity (baseOverworld);
            var factionBranchData = provinceCurrentBlueprint != null ? DataMultiLinkerOverworldFactionBranch.GetEntry (provinceCurrentBlueprint.factionBranch) : null;
            
            for (int i = 0; i < rewardCount; ++i)
            {
                var block = blocks.GetRandomEntry ();
                EquipmentUtility.PrepareRewardsForSalvage (block, basePersistent, true, factionBranchData);
            }
            
            #endif
        }
    }
}