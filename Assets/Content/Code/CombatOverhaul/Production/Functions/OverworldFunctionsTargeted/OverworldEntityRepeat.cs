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
    public class OverworldEntityRepeat : IOverworldTargetedFunction
    {
        public int repeats = 1;
        
        [BoxGroup, HideLabel]
        public IOverworldTargetedFunction function;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            if (repeats <= 0 || function == null)
                return;

            for (int i = 0; i < repeats; ++i)
                function.Run (entityOverworld);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityRepeatGroup : IOverworldTargetedFunction
    {
        public int repeats = 1;
        
        public List<IOverworldTargetedFunction> functions;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (repeats <= 0 || functions == null)
                return;
            
            for (int i = 0; i < repeats; ++i)
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