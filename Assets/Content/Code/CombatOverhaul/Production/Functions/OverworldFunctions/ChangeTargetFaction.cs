using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ChangeTargetFaction : IOverworldEventFunction, IOverworldActionFunction
    {
        [ValueDropdown ("@Factions.GetList ()")]
        public string faction;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            Run (target);
            
            #endif
        }

        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var target = IDUtility.GetOverworldActionOverworldTarget (source);
            Run (target);
            
            #endif
        }

        private void Run (OverworldEntity target)
        {
            #if !PB_MODSDK

            if (target == null)
            {
                Debug.LogError ($"ChangeTargetFaction | Event function failed due to missing target");
                return;
            }

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (target);
            if (entityPersistent == null)
            {
                Debug.LogError ($"ChangeTargetFaction | Event function failed due to missing persistent entity ({target.ToStringNullCheck ()})");
                return;
            }

            var factionNew = faction == Factions.player ? Factions.player : Factions.enemy;
            entityPersistent.ReplaceFaction (factionNew);

            // Special case for the capital battle: The site is forcibly changed to friendly as a result of the event outcome, call CheckForSimpleCapture here to run the logic to try and capture the capital province.
            if (factionNew == Factions.player && entityPersistent.hasProvinceOfOrigin)
                OverworldCombatOutcomeProcessingSystem.CheckForSimpleCapture(Game.CombatOutcome.Victory, entityPersistent);
            
            #endif
        }
    }
}