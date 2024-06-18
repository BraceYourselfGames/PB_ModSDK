using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TransferTargetRewards : IOverworldEventFunction
    {
        public string rewardKey;
        
        [Range(1, 10)]
        public int rewardCount;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
            var basePersistent = IDUtility.playerBasePersistent;

            if (basePersistent == null || targetPersistent == null)
            {
                Debug.LogError ($"TransferTargetRewards | Event function failed due to missing base ({basePersistent.ToStringNullCheck ()}) or target ({targetPersistent.ToStringNullCheck ()})");
                return;
            }
            
            var blueprint = target.dataLinkOverworldEntityBlueprint.data;
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
            
            var factionBranchData = EquipmentUtility.GetFactionBranchOfEntity (targetPersistent);
            for (int i = 0; i < rewardCount; ++i)
            {
                var block = blocks.GetRandomEntry ();
                EquipmentUtility.PrepareRewardsForSalvage (block, targetPersistent, true, factionBranchData);
            }

            // Directly transfer for now, investigate ability to open salvage UI later
            EquipmentUtility.TransferFullInventory (targetPersistent, basePersistent, true, true);
            
            #endif
        }
    }
}