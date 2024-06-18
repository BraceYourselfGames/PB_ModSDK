using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitValueSimpleConstant : ICombatUnitValueResolver
    {
        public float value;
        
        public float Resolve (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            return value;
            
            #else
            return 0f;
            #endif
        }
    }
}