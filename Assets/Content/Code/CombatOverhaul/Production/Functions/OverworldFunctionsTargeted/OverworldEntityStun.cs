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
    public class OverworldEntityStun : IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;
            
            // Ensure entity is revealed so that name message isn't context-less
            entityOverworld.isPlayerKnown = true;
            entityOverworld.isPlayerRecognized = true;
		    
            var targetName = OverworldUIIntelService.GetEntityName (entityOverworld);
            CIViewOverworldLog.AddMessage ($"{targetName}: {Txt.Get (TextLibs.uiOverworld, "log_ability_drone_stun")}");

            OverworldUtility.StopMovement (entityOverworld);
            OverworldUtility.ForceRecovery (entityOverworld);

            #endif
        }
    }
}