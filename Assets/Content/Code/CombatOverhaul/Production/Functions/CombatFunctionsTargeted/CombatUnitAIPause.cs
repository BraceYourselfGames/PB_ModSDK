using System;
using PhantomBrigade.AI.BT;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitAIPause : ICombatFunctionTargeted
    {
        public bool discardActions = true;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (!unitCombat.isAIControllable)
                return;
            
            unitCombat.isAIControllable = false;
            unitCombat.isAIControllableNextTurn = true;
            
            if (discardActions)
            {
                var actions = Contexts.sharedInstance.action.GetEntitiesWithActionOwner (unitCombat.id.id);
                foreach (var action in actions)
                {
                    if (!action.isDisposed && !action.isLocked)
                        action.isDisposed = true;
                }
                
                Debug.Log ($"Pausing AI on unit {unitCombat.ToLog ()} with {actions.Count} actions discarded");

            }
            else
            {
                Debug.Log ($"Pausing AI on unit {unitCombat.ToLog ()}");
            }

            #endif
        }
    }
}