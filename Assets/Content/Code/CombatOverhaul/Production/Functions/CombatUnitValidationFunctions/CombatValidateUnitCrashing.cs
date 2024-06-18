using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitCrashing : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;
            
            return unitCombat.isCrashing == present;

            #else
            return false;
            #endif
        }
    }
}