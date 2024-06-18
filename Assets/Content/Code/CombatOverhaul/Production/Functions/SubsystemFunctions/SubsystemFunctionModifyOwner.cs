using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class SubsystemFunctionModifyOwner : ISubsystemFunctionGeneral, ISubsystemFunctionTargeted, ISubsystemFunctionAction
    {
        public List<ICombatFunctionTargeted> functions;
        
        public void OnPartEventGeneral (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context)
        {
            #if !PB_MODSDK

            var unitOwner = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            Run (unitOwner);
            
            #endif
        }

        public void OnPartEventTargeted (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, Vector3 position, Vector3 direction, Vector3 targetPosition, CombatEntity targetUnitCombat, CombatEntity projectile)
        {
            #if !PB_MODSDK

            var unitOwner = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            Run (unitOwner);
            
            #endif
        }
        
        public void OnPartEventAction (EquipmentEntity part, DataContainerSubsystem subsystemBlueprint, string context, ActionEntity action)
        {
            #if !PB_MODSDK

            var unitOwner = part != null && part.hasPartParentUnit ? IDUtility.GetPersistentEntity (part.partParentUnit.persistentID) : null;
            Run (unitOwner);
            
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