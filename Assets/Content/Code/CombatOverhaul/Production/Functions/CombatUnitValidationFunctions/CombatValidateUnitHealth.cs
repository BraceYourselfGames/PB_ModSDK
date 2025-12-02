using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitHealthNormalized : DataBlockOverworldEventSubcheckFloat, ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            DataHelperStats.GetUnitEHP (unitPersistent, out float ehpCurrent, out float ehpMax, out float ehpNormalized, out int socketsUsed);
            bool passed = IsPassed (true, ehpNormalized);
            return passed;

            #else
            return false;
            #endif
        }
    }
}