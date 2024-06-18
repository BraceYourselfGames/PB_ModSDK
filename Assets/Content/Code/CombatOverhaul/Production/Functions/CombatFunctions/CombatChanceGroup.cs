using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatChanceGroup : ICombatFunction
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;
        public List<ICombatFunction> functions;

        public void Run ()
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            if (passed && functions != null)
            {
                foreach (var function in functions)
                {
                    if (function != null)
                        function.Run ();
                }
            }
            
            #endif
        }
    }
}