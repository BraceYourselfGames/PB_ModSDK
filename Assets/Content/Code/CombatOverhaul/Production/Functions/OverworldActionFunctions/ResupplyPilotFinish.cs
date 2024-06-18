using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ResupplyPilotFinish : IOverworldActionFunction
    {
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var playerBasePersistent = IDUtility.playerBasePersistent;
            if (playerBasePersistent == null)
                return;
            
            var nameInternalSafe = UnitUtilities.GetSafePersistentInternalName ("pilot");
            var pilot = UnitUtilities.CreatePilotEntity
            (
                false,
                Factions.player,
                nameInternalSafe,
                null,
                true
            );

            pilot.ReplaceEntityLinkPersistentParent (playerBasePersistent.id.id);
            Debug.LogWarning ($"Added new pilot to player base: {pilot.ToLog ()}");
            
            playerBasePersistent.SetMemoryFloat ("world_tag_new_pilot", 1);
            
            CIViewOverworldRoster.ins.RefreshOptionAvailability ();
            Contexts.sharedInstance.persistent.isPlayerCombatReadinessChecked = true;
            
            GameEventUtility.OnEventObject (GameEventObject.PilotAdded, pilot);
            
            #endif
        }
    }
}