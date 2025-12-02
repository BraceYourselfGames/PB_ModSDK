using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class UnitValidateFrameDefects : DataBlockOverworldEventSubcheckInt, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        protected override string GetLabel () => "Frame damage";

        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            int defects = unitPersistent.hasUnitFrameDefects ? unitPersistent.unitFrameDefects.i : 0;
            return IsPassed (true, defects);
            
            #else
            return false;
            #endif
        }
    }
    
    public class UnitValidateFrameIntegrity : DataBlockOverworldEventSubcheckFloat, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            float hp = unitPersistent.hasUnitFrameIntegrity ? unitPersistent.unitFrameIntegrity.f : 1f;
            return IsPassed (true, hp);
            
            #else
            return false;
            #endif
        }
    }
    
    public class UnitValidateMemory : DataBlockOverworldMemoryCheckGroup, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return false;

            bool passed = IsPassed (unitPersistent);
            return passed;

            #else
            return false;
            #endif
        }
    }
}