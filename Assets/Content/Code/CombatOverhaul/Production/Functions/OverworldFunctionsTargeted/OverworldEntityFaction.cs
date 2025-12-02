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
    public class OverworldEntityFaction : IOverworldTargetedFunction
    {
        public bool friendly;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;
            
            entityPersistent.ReplaceFaction (friendly ? Factions.player : Factions.enemy);

            #endif
        }
    }
}