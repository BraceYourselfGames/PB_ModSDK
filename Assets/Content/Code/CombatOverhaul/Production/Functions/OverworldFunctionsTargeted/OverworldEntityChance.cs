using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityChance : IOverworldTargetedFunction
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;
        
        [BoxGroup, HideLabel]
        public IOverworldTargetedFunction function;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            if (passed && function != null)
                function.Run (entityOverworld);
            
            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityChanceGroup : IOverworldTargetedFunction
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;
        public List<IOverworldTargetedFunction> functions;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = Random.Range (0f, 1f) < chanceFinal;
            if (passed && functions != null)
            {
                foreach (var function in functions)
                {
                    if (function != null)
                        function.Run (entityOverworld);
                }
            }
            
            #endif
        }
    }
}