using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class TeleportBaseToPosition : IOverworldActionFunction, IOverworldFunction
    {
        public Vector3 position;
        
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
    
    public class TeleportBaseToEntityName : IOverworldFunction
    {
        public string nameInternal;
        public float interactionRangeMultiplier;

        public void Run ()
        {
            #if !PB_MODSDK

            var entityOverworld = IDUtility.GetOverworldEntity (nameInternal);
            if (entityOverworld == null || entityOverworld.isDestroyed || !entityOverworld.hasPosition)
            {
                Debug.LogWarning ($"Failed to teleport base to entity named {nameInternal}, no such entity found");
                return;
            }

            var baseOverworld = IDUtility.playerBaseOverworld; 
            OverworldUtility.OffsetFromInteraction (baseOverworld, entityOverworld, Mathf.Max (0f, interactionRangeMultiplier));
            
            #endif
        }
    }
}