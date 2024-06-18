using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class StunTarget : IOverworldEventFunction, IOverworldActionFunction
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            Run (target);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            Run (target);
            
            #endif
        }
        
        public void Run (OverworldEntity target)
        {
            #if !PB_MODSDK

            if (target == null)
            {
                Debug.LogError ($"StunTarget | Event function failed due to missing target");
                return;
            }
            
            OverworldUtility.ForceRecovery (target);
            
            #endif
        }
    }
}