using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ChangeSelfView : IOverworldEventFunction, IOverworldActionFunction
    {
        public string assetKey;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var self = IDUtility.playerBaseOverworld;
            OverworldIndirectFunctions.ChangeTargetView (self, assetKey);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var self = IDUtility.GetOverworldActionOwner (source);
            OverworldIndirectFunctions.ChangeTargetView (self, assetKey);
            
            #endif
        }
    }
}