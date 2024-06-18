using System;
using System.Collections.Generic;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitConditional : ICombatUnitValidationFunction, ICombatActionValidationFunction
    {
        public enum Mode
        {
            All,
            Any,
            None
        }

        public Mode mode = Mode.Any;
        public List<ICombatUnitValidationFunction> functions = new List<ICombatUnitValidationFunction> ();

        public bool IsValid (CombatEntity unitCombat)
        {
            #if !PB_MODSDK
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            return IsValid (unitPersistent);
            #else
            return false;
            #endif
        }
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || functions == null || functions.Count == 0)
                return false;

            bool earlyExit = mode != Mode.All;
            int countTotal = functions.Count;
            int countValid = 0;

            for (int i = 0; i < countTotal; ++i)
            {
                var function = functions[i];
                if (function == null)
                    continue;

                bool valid = function.IsValid (unitPersistent);
                countValid += 1;
                
                if (valid && earlyExit)
                    break;
            }

            if (earlyExit)
            {
                if (mode == Mode.Any)
                    return countValid > 0; // Any
                else
                    return countValid == 0; // None
            }
            else
                return countValid == countTotal; // All

            #else
            return false;
            #endif
        }
    }
}