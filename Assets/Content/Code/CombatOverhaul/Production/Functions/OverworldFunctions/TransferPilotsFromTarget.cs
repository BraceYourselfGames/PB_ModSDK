using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TransferPilotsFromTarget : IOverworldEventFunction, IOverworldActionFunction
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            OverworldIndirectFunctions.TransferPilotsFromTarget (target);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            OverworldIndirectFunctions.TransferPilotsFromTarget (target);
            
            #endif
        }
    }
}