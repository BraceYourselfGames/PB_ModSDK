using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityConceal : IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;
            
            entityOverworld.isIntelLocked = true;
            entityOverworld.isPlayerRecognized = false;

            #endif
        }
    }
}