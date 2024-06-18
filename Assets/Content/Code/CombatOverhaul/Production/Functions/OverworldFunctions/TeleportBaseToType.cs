using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TeleportBaseToType : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.data.Keys")]
        public string blueprintKey;

        public bool contactRange;
        
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            OverworldUtility.TeleportBaseToType (blueprintKey, contactRange);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            OverworldUtility.TeleportBaseToType (blueprintKey, contactRange);
            
            #endif
        }

        public void Run ()
        {
            #if !PB_MODSDK
            
            OverworldUtility.TeleportBaseToType (blueprintKey, contactRange);
            
            #endif
        }
    }
}