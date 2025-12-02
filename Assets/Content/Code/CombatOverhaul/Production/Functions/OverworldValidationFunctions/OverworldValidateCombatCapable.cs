using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateCombatCapable : IOverworldEntityValidationFunction
    {
        public bool baseShouldBeReady = true;
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            if (!entityPersistent.hasFaction || !string.Equals (entityPersistent.faction.s, Factions.enemy))
                return false;

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            if (entityOverworld == null || !entityOverworld.hasCombatDescriptionLink)
                return false;

            var persistent = Contexts.sharedInstance.persistent;
            bool ready = persistent.isPlayerCombatReady;
            if (ready != baseShouldBeReady)
                return false;

            return true;

            #else
            return false;
            #endif
        }
    }
}