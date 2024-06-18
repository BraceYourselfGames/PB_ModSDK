using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SubsystemFunctionLog : ISubsystemFunctionGeneral, ISubsystemFunctionTargeted, ISubsystemFunctionAction
    {
        public string argument;
        
        public void OnPartEventGeneral (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context)
        {
            #if !PB_MODSDK

            var unitOwner = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;

            Debug.Log ($"{argument}\nContext: {context} | Subsystem: {subsystemBlueprint} | Part: {part.ToLog ()} | Unit owner: {unitOwner.ToLog ()}");
            
            #endif
        }

        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile)
        {
            #if !PB_MODSDK

            var unitOwner = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            Debug.Log ($"{argument}\nContext: {context} | Subsystem: {subsystemBlueprint} | Part: {part.ToLog ()} | Unit owner: {unitOwner.ToLog ()} | Position: {position} | Direction: {direction} | Target position: {targetPosition} | Target unit: {targetUnitCombat.ToLog ()}");
            
            #endif
        }
        
        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action)
        {
            #if !PB_MODSDK

            var unitOwner = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            var actionKey = action != null && action.hasDataKeyAction ? action.dataKeyAction.s : "?";

            Debug.Log ($"{argument}\nContext: {context} | Subsystem: {subsystemBlueprint} | Part: {part.ToLog ()} | Unit owner: {unitOwner.ToLog ()} | Action: {action.ToLog ()} ({actionKey})");
            
            #endif
        }
    }
}