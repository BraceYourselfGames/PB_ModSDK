using System;
using PhantomBrigade.Data;
using PhantomBrigade.Linking;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class DestroyTarget : IOverworldActionFunction 
    {
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            Run (target);
            
            #endif
        }
        
        private void Run (OverworldEntity target)
        {
            #if !PB_MODSDK

            OverworldUtility.TryDestroySite (target);
            
            #endif
        }
    }
}