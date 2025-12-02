using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatGroupLoop : ICombatFunction
    {
        public int repeats;
        public List<ICombatFunction> functions;

        public void Run ()
        {
            #if !PB_MODSDK
            
            int r = Mathf.Clamp (repeats, 1, 100);
            for (int i = 0; i < r; i++)
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
    
    [Serializable]
    public class CombatGroupLoopByResolvedInt : ICombatFunction
    {
        [BoxGroup ("A", false)]
        public IOverworldIntValueFunction valueFromBase;

        public List<ICombatFunction> functions;

        public void Run ()
        {
            #if !PB_MODSDK

            if (valueFromBase == null)
                return;

            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null)
                return;
            
            int r = Mathf.Clamp (valueFromBase.Resolve (baseOverworld), 1, 100);
            Debug.Log ($"Looping a function {r} times based on resolved value from {valueFromBase.GetType ().Name}");
            
            for (int i = 0; i < r; i++)
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