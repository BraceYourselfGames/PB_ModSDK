using System;
using System.Collections.Generic;
using Area;
using Entitas;
using PhantomBrigade.Combat;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Combat.Systems;
using PhantomBrigade.Data;
using PhantomBrigade.Functions;
using PhantomBrigade.Game;
using PhantomBrigade.Input.Components;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Components;
using PhantomBrigade.Overworld.Systems;
using QFSW.QC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    public static class ScenarioTags
    {
        public const string BlockFocusEvents = "block_focus_events";
        public const string BlockPersistentPilots = "block_persistent_pilots";
        public const string BlockSaveBefore = "block_save_before";
        public const string BlockSaveAfter = "block_save_after";
        public const string BlockExitOnDefeat = "block_exit_on_defeat";
        public const string BlockTabsSentries = "block_tabs_sentries";
        
        
        public const string TutorialPilotProgression = "tutorial_pilot_progression";
        public const string UnitsNeedSightline = "units_need_sightline";
        public const string UnitsDisposableCounted = "units_disposable_counted";
    }

    public static class ScenarioUnitTags
    {
        public const string ReducedHeat = "flag_reduced_heat";
        public const string ReducedDamageIn = "flag_reduced_damage_in";
        public const string NoHeat = "flag_no_heat";
        public const string NoDamageIn = "flag_no_damage_in";
        public const string NoConcussionIn = "flag_no_concussion_in";
        public const string NoPower = "flag_no_power";
        public const string Unlisted = "flag_unlisted";
        public const string Untargetable = "flag_untargetable";
        public const string Uncollideable = "flag_uncollideable";
        public const string Uncrashable = "flag_uncrashable";
        public const string CompositeUnlisted = "flag_composite_unlisted";
        public const string SpikeOneHit = "flag_spike_one_hit";
        
        public const string ScatterStabilized = "flag_scatter_stabilized";
        public const string AttacksNeedSightline = "flag_attacks_need_sightline";
        public const string MovementSkipped = "movement_skipped";
        public const string MovementAnimMaxed = "movement_anim_maxed";
        
        public const string HideDisposableStatus = "hide_disposable_status";
        public const string RestrictSelection = "restrict_selection";
        public const string AutoEjectOnDestruction = "auto_eject_on_destruction";
        
        public const string FieldsIgnored = "fields_ignored";
        
        public const string InFieldWaterDuringTurn = "in_field_water_during_turn";
        public const string InFieldWater = "in_field_water";
        public const string InFieldLava = "in_field_lava";
        public const string InFieldAcid = "in_field_acid";
        public const string InFieldKill = "in_field_kill";
        
        public const string PilotPersistentUsed = "pilot_persistent_used";
        public const string CustomAudioMissile = "custom_audio_missile";
        public const string CustomGeneratedSquad = "custom_generated_squad";
    }

    public static class ScenarioFieldTypes
    {
        public const string Water = "water";
        public const string Kill = "kill";
        public const string Lava = "lava";
        public const string Acid = "acid";
    }
    
    public static class CombatUIUtility
    {
        public static bool IsFactionFriendly (string faction)
        {
            return !string.Equals (Factions.enemy, faction);
        }
    }

    public static class ScenarioUtility
    {
        private static List<string> keysFilteredArea = new List<string> ();
        
        public static bool IsTimeDay ()
        {
            #if !PB_MODSDK
            bool isDay = TOD_Sky.Instance != null && !TOD_Sky.Instance.IsNight;
            if (Application.isPlaying)
            {
                var overworld = Contexts.sharedInstance.overworld;
                float hour = 8f; // Reasonable default if nothing is found
                if (overworld.hasSimulationTime)
                    hour = overworld.simulationTime.f % 24f;

                var scenarioCurrent = ScenarioUtility.GetCurrentScenario ();
                if (scenarioCurrent != null && scenarioCurrent.coreProc != null && scenarioCurrent.coreProc.timeLocked)
                    hour = scenarioCurrent.coreProc.time % 24f;
                
                var areaCurrent = ScenarioUtility.GetCurrentArea ();
                if (areaCurrent != null && areaCurrent.coreProc != null && areaCurrent.coreProc.timeCustom != null)
                    hour = areaCurrent.coreProc.timeCustom.f % 24f;

                isDay = hour > 5f && hour < 19f;

                // Ensure all the visibility aids are on if visibility is lowered due to precipitation
                if (isDay)
                {
                    var wm = WeatherManager.instance;
                    if (wm != null && wm.precIntensityTarget > 0.7f)
                        isDay = false;
                }
            }

            isTimeCheckedOnce = true;
            isTimeDayLast = isDay;
            return isDay;
            #else
            return true;
            #endif
        }

        public static List<string> GetAreaKeysFromFilter (SortedDictionary<string, bool> filter, bool returnEmpty = true)
        {
            keysFilteredArea.Clear ();
            
            bool filterUsed = filter != null && filter.Count > 0;
            if (filterUsed)
            {
                var areasMatchingTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, filter);
                return areasMatchingTag;
            }
            else
            {
                if (returnEmpty)
                    keysFilteredArea.Add ("unrestricted");
                return keysFilteredArea;
            }
        }
        
        public static SortedDictionary<string, bool> unitGroupTagFilterCombined = new SortedDictionary<string, bool> ();
        public static List<DataContainerCombatUnitGroup> unitGroupsSelected = new List<DataContainerCombatUnitGroup> ();
        public static List<string> unitGroupBranchTagsRemoved = new List<string> ();
        public static string unitGroupBranchTagPrefix = "branch_";
    }
}

