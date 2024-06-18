using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreateOverworldEntity : IOverworldEventFunction, IOverworldEventFunctionEarly, IOverworldActionFunction, IOverworldFunction
    {
        public bool Early () => true; 

        [BoxGroup]
        public DataBlockOverworldEventSpawnData spawnData = new DataBlockOverworldEventSpawnData ();

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            OverworldIndirectFunctions.CreateOverworldEntity 
            (
                IDUtility.playerBaseOverworld, 
                target, 
                spawnData, 
                $"event {eventData.key}"
            );
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            var blueprintKey = source.dataKeyOverworldAction.s;
            var ownerOverworld = IDUtility.GetOverworldActionOwner (source);
            var targetOverworld = IDUtility.GetOverworldActionOverworldTarget (source);
            
            OverworldIndirectFunctions.CreateOverworldEntity 
            (
                ownerOverworld, 
                targetOverworld, 
                spawnData,
                $"action {blueprintKey}"
            );
            
            #endif
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            OverworldIndirectFunctions.CreateOverworldEntity 
            (
                IDUtility.playerBaseOverworld,
                null, 
                spawnData,
                $"general function"
            );
            
            #endif
        }
    }
}