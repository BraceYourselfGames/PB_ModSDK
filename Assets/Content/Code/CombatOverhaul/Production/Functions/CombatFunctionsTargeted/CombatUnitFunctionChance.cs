using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitFunctionChance : ICombatFunctionTargeted
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;
        
        [BoxGroup, HideLabel]
        public ICombatFunctionTargeted function;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            if (passed && function != null)
                function.Run (unitPersistent);
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitFunctionChanceGroup : ICombatFunctionTargeted
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;
        public List<ICombatFunctionTargeted> functions;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            if (passed && functions != null)
            {
                foreach (var function in functions)
                {
                    if (function != null)
                        function.Run (unitPersistent);
                }
            }
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitFunctionChanceFromDifficulty : ICombatFunctionTargeted
    {
        public string key;
        
        [BoxGroup, HideLabel]
        public ICombatFunctionTargeted function;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (DifficultyUtility.GetMultiplier (key));
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            if (passed && function != null)
                function.Run (unitPersistent);
            
            #endif
        }
    }
}