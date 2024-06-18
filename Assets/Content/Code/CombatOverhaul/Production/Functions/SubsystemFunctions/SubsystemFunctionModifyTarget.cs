using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SubsystemFunctionModifyTarget : ISubsystemFunctionTargeted, ISubsystemFunctionAction
    {
        public List<ICombatFunctionTargeted> functions;

        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile)
        {
            #if !PB_MODSDK

            var targetUnitPersistent = IDUtility.GetLinkedPersistentEntity (targetUnitCombat);
            Run (targetUnitPersistent);
            
            #endif
        }
        
        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action)
        {
            #if !PB_MODSDK

            var targetUnitCombatID = action.hasTargetedEntity ? action.targetedEntity.combatID : IDUtility.invalidID;
            var targetUnitCombat = IDUtility.GetCombatEntity (targetUnitCombatID);
            var targetUnitPersistent = IDUtility.GetLinkedPersistentEntity (targetUnitCombat);
            Run (targetUnitPersistent);
            
            #endif
        }

        private void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || functions == null)
                return;

            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (unitPersistent);
            }
            
            #endif
        }
    }
}