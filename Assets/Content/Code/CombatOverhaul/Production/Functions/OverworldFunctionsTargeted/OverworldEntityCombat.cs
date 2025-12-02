using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityCombat : IOverworldTargetedFunction
    {
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            if (overworld.hasInteractionState)
                overworld.RemoveInteractionState ();
            
            ScenarioSetupUtility.EnterCombat (entityOverworld);

            #endif
        }
    }
}