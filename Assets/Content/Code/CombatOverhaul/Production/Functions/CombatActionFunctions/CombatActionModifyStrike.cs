using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionModifyStrike : ICombatActionExecutionFunction
    {
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitCombat == null || unitPersistent == null)
                return;

            var startTime = action.startTime.f;
            CombatStrikeHelper.RescheduleStrikeFromAction (action.id.id, startTime);
            
            #endif
        }
    }
}