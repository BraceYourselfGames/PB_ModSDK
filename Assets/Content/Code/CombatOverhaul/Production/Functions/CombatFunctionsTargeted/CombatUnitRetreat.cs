using System;
using PhantomBrigade.Combat;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitRetreat : ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            CombatActionEvent.OnRetreatApplication (unitCombat);
            
            #endif
        }
    }
}