using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class StopBase : IOverworldEventFunction
    {
        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var baseOverworld = IDUtility.playerBaseOverworld;
            if (baseOverworld == null)
            {
                Debug.LogError ($"StopBase | Event function failed due to missing base ({baseOverworld.ToStringNullCheck ()})");
                return;
            }
            
            OverworldUtility.StopMovement (baseOverworld);
            
            #endif
        }
    }
}