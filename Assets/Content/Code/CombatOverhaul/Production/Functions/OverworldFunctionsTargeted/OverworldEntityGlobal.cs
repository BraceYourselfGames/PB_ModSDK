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
    public class OverworldEntityGlobal : IOverworldTargetedFunction
    {
        [BoxGroup, HideLabel]
        public IOverworldFunction function;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            if (function != null)
                function.Run ();
            
            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityGlobalGroup : IOverworldTargetedFunction
    {
        public List<IOverworldFunction> functions;

        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            if (functions != null)
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