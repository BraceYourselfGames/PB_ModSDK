using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class UnitValidateNameInternal : ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        public string nameInternal;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag || !unitPersistent.hasNameInternal)
                return false;

            bool passed = string.Equals (unitPersistent.nameInternal.s, nameInternal, StringComparison.Ordinal);
            return passed;

            #else
            return false;
            #endif
        }
    }
}