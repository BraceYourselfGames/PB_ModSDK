using System;
using System.Collections.Generic;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionFunctionOwner : ICombatActionExecutionFunction
    {
        public List<ICombatFunctionTargeted> functions;

        
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitCombat == null || unitPersistent == null)
                return;

            if (functions == null)
                return;

            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (unitPersistent);
            }
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatActionFunctionTargetUnit : ICombatActionExecutionFunction
    {
        public List<ICombatFunctionTargeted> functions;
        
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (functions == null)
                return;
            
            
            if (action == null || !action.hasTargetedEntity)
                return;

            var unitTargetCombat = IDUtility.GetCombatEntity (action.targetedEntity.combatID);
            var unitTargetPersistent = IDUtility.GetLinkedPersistentEntity (unitTargetCombat);
            if (unitTargetCombat == null || unitTargetPersistent == null)
                return;

            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (unitTargetPersistent);
            }
            
            #endif
        }
    }
}