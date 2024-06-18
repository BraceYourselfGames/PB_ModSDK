using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateMemory : IOverworldValidationFunction
    {
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockOverworldMemoryCheckGroup check = new DataBlockOverworldMemoryCheckGroup ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            if (check == null)
                return false;

            bool passed = check.IsPassed (entityPersistent);
            return passed;

            #else
            return false;
            #endif
        }
    }
}