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
    public class OverworldEntityEffect : IOverworldTargetedFunction
    {
        public DataBlockAsset asset = new DataBlockAsset ();
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || !entityOverworld.hasPosition || asset == null)
                return;

            AssetPoolUtility.ActivateInstance (asset.key, entityOverworld.position.v, Quaternion.identity, asset.scale);

            #endif
        }
    }
}