using System;
using PhantomBrigade.Combat;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitDestroy : ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            CombatActionEvent.OnDestruction (unitCombat, false);
            
            #endif
        }
    }
}