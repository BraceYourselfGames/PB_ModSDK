using System;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitValueSimpleRandom : ICombatUnitValueResolver
    {
        public float valueMin;
        public float valueMax;
        
        public float Resolve (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            return Random.Range (valueMin, valueMax);
            
            #else
            return 0f;
            #endif
        }
    }
}