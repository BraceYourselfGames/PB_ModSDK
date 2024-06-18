using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionAddTargetWarning : ICombatActionExecutionFunction
    {
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;

            action.isTargetWarningUsed = true;
            
            #endif
        }
    }
}