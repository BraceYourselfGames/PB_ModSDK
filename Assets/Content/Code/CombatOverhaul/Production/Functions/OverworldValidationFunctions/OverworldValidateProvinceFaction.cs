using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateProvinceFaction : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Should be friendly" : "Should be hostile";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            bool provinceActiveFound = DataHelperProvince.TryGetProvinceDependenciesActive 
            (
                out var provinceActiveBlueprint, 
                out var provinceActivePersistent, 
                out var provinceActiveOverworld
            );
            
            if (!provinceActiveFound)
                return false;

            bool shouldBeFriendly = present;
            bool isFriendly = !provinceActivePersistent.hasFaction || !string.Equals (provinceActivePersistent.faction.s, Factions.enemy);
            return shouldBeFriendly == isFriendly;

            #else
            return false;
            #endif
        }
    }
}