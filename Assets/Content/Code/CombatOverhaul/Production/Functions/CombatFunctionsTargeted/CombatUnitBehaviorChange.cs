using System;
using PhantomBrigade.AI.BT;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitBehaviorChange : ICombatFunctionTargeted
    {
        public string behaviourName;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            var aiAgent = IDUtility.GetLinkedAIAgent (unitCombat);

            if (aiAgent == null)
            {
                Debug.LogWarning ($"Failed to set AI behavior on unit {unitPersistent.ToLog ()}: no combat unit or AI entity");
                return;
            }

            var behaviorTree = BTDataUtility.LoadBehavior (behaviourName);
            if (behaviorTree != null)
                aiAgent.ReplaceAgentBehaviorTree (behaviorTree);
            
            #endif
        }
    }
}