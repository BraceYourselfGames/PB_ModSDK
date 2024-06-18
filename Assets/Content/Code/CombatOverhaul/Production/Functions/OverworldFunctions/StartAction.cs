using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class StartAction : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunctionLog, IOverworldEventFunctionEarly
    {
        public bool Early () => true; 
        
        public DataBlockOverworldActionInstanceData data = new DataBlockOverworldActionInstanceData ();

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var baseOverworld = IDUtility.playerBaseOverworld;
            OverworldActionUtility.InstantiateOverworldActionFromData (baseOverworld, target, data);
            
            #endif
        }

        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var ownerOverworld = IDUtility.GetOverworldActionOwner (source);
            var targetOverworld = IDUtility.GetOverworldActionOverworldTarget (source);
            OverworldActionUtility.InstantiateOverworldActionFromData (ownerOverworld, targetOverworld, data);
            
            #endif
        }

        public string ToLog ()
        {
            if (data == null)
                return "null";
            
            if (data.target == ActionTargetProvider.None)
                return $"{data.key} on {data.owner}";
            
            return $"{data.key} on {data.owner} targeting {data.target}";
        }
    }
}