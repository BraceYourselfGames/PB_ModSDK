using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotValidateNameInternal : IPilotValidationFunction
    {
        public string nameInternal;
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag || !pilot.hasNameInternal)
                return false;

            bool passed = string.Equals (pilot.nameInternal.s, nameInternal, StringComparison.Ordinal);
            return passed;

            #else
            return false;
            #endif
        }
    }
}