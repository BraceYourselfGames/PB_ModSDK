using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TeleportBaseToPosition : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction
    {
        public Vector3 position;
        
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            OverworldUtility.TeleportBaseToPosition (position);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            OverworldUtility.TeleportBaseToPosition (position);
            
            #endif
        }

        public void Run ()
        {
            #if !PB_MODSDK
            
            OverworldUtility.TeleportBaseToPosition (position);
            
            #endif
        }
    }
}