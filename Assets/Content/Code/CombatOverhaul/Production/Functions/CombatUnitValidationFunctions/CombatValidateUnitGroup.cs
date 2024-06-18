using System;
using System.Collections.Generic;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitGroup : ICombatUnitValidationFunction
    {
        public DataBlockOverworldEventSubcheckInt validationsCount;
        public List<ICombatUnitValidationFunction> validations = new List<ICombatUnitValidationFunction> ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || validations == null || validations.Count == 0)
                return false;
            
            int childrenValid = 0;
            int childrenCount = validations.Count;
            
            foreach (var child in validations)
            {
                if (child == null)
                    continue;

                bool childValid = child.IsValid (unitPersistent);
                if (childValid)
                    childrenValid += 1;
            }

            bool valid = validationsCount != null ? validationsCount.IsPassed (true, childrenValid) : childrenCount == childrenValid;
            return valid;

            #else
            return false;
            #endif
        }
    }
}