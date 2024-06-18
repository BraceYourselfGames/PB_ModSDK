using System;
using PhantomBrigade.Combat;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitCrash : ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            if (!unitCombat.isCrashable || unitCombat.hasUnitCompositeLink)
                return;

            CombatActionEvent.CrashUnit (unitCombat, null);
            
            #endif
        }
    }
}