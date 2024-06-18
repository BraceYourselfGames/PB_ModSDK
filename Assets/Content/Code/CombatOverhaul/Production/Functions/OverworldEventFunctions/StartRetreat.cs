using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class StartRetreat : IOverworldEventFunction, IOverworldFunction
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            OverworldIndirectFunctions.StartRetreat ();
            
            #endif
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            OverworldIndirectFunctions.StartRetreat ();
            
            #endif
        }
    }
}