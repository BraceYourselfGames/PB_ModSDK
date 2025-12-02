using System;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityChangeView : IOverworldTargetedFunction
    {
        public string assetKey;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            OverworldUtility.ChangeTargetView (entityOverworld, assetKey);

            #endif
        }
    }
}