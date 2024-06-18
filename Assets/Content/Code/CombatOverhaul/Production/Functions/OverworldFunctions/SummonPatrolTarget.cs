using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld.Systems;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SummonPatrolTarget : IOverworldEventFunction, IOverworldActionFunction
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            Run (target);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            Run (target);
            
            #endif
        }
        
        private void Run (OverworldEntity target)
        {
            #if !PB_MODSDK

            if (target == null)
            {
                Debug.LogError ($"Summon Patrol Target | Event function failed due to missing target");
                return;
            }
            
            var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
            if (targetPersistent == null || targetPersistent == IDUtility.playerBasePersistent)
            {
                Debug.LogError ($"Summon Patrol Target | Event function failed due to missing persistent counterpart or due to match with player base ({target.ToStringNullCheck ()})");
                return;
            }

            CommanderUtil.SendIntel (AIIntel.investigationRequest, target.position.v, null, target);
            
            #endif
        }
    }
}