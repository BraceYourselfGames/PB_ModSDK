using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotValidateMemory : DataBlockOverworldMemoryCheckGroup, IPilotValidationFunction
    {
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool passed = IsPassed (pilot);
            return passed;

            #else
            return false;
            #endif
        }
    }
}