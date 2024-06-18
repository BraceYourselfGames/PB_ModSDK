using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Game;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreatePilot : IOverworldFunction, IOverworldActionFunction
    {
        [DropdownReference (true)]
        public PilotIdentification identification;
        
        [DropdownReference (true)]
        public string bio;
        
        [DropdownReference (true)]
        public DataBlockPilotAppearanceOverlay appearanceOverlay;

        [DropdownReference (true)]
        public DataBlockPilotAppearance appearanceCustom;

        [DropdownReference]
        public List<string> appearancePresets;
        
        [DropdownReference]
        public SortedDictionary<string, float> memory;

       
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }

        public void Run ()
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

            if (pilot == null)
                return;
            
            pilot.ReplaceEntityLinkPersistentParent (playerBasePersistent.id.id);
            Debug.LogWarning ($"Added new pilot to player base: {pilot.ToLog ()}");
            
            playerBasePersistent.SetMemoryFloat ("world_tag_new_pilot", 1);

            if (memory != null)
            {
                foreach (var kvp in memory)
                    pilot.SetMemoryFloat (kvp.Key, kvp.Value);
            }
            
            if (appearanceCustom != null)
            {
                var pilotAppearance = pilot.pilotAppearance.data;
                pilotAppearance.CopyFrom (appearanceCustom);
                pilot.ReplacePilotAppearance (pilotAppearance);
            }
            else if (appearancePresets != null && appearancePresets.Count > 0)
            {
                var appearancePresetKey = appearancePresets.GetRandomEntry ();
                var appearancePresetsAll = DataLinkerSettingsPilot.data?.appearancePresets;
                if (appearancePresetsAll != null && !string.IsNullOrEmpty (appearancePresetKey) && appearancePresetsAll.TryGetValue (appearancePresetKey, out var preset))
                {
                    if (preset.appearance != null)
                    {
                        var pilotAppearance = pilot.pilotAppearance.data;
                        pilotAppearance.CopyFrom (appearanceCustom);
                        pilot.ReplacePilotAppearance (pilotAppearance);
                    }
                }
            }

            if (appearanceOverlay != null)
            {
                var pilotAppearance = pilot.pilotAppearance.data;
                pilotAppearance.portrait = appearanceOverlay.key;
                pilotAppearance.portraitVariant = appearanceOverlay.variant;
                pilot.ReplacePilotAppearance (pilotAppearance);
            }

            if (identification != null)
            {
                pilot.ReplacePilotIdentification
                (
                    identification.callsignIndex,
                    identification.callsignOverride,
                    identification.nameIndexPrimary,
                    identification.nameIndexSecondary,
                    identification.nameOverride
                );
            }

            if (!string.IsNullOrEmpty (bio))
                pilot.ReplacePilotBio (bio);

            CIViewOverworldRoster.ins.RefreshOptionAvailability ();
            Contexts.sharedInstance.persistent.isPlayerCombatReadinessChecked = true;
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CreatePilot () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
}