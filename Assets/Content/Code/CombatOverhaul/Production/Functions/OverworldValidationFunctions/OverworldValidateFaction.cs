using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateFaction : DataBlockSubcheckBool, IOverworldEntityValidationFunction
    {
        protected override string GetLabel () => present ? "Should be friendly" : "Should be hostile";
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            bool shouldBeFriendly = present;
            bool isFriendly = !entityPersistent.hasFaction || !string.Equals (entityPersistent.faction.s, Factions.enemy);
            return shouldBeFriendly == isFriendly;

            #else
            return false;
            #endif
        }
    }
}