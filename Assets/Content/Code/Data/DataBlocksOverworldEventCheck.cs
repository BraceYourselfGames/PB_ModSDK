using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class EventMemoryFeatureFlag
    {
        // Every key starts with this
        public const string KeyPrefix = "feature_";
        public const string KeyPrefixCampaign = "campaign_";
        
        // Whether check for features is performed at all.
        // Old saves won't have that flag and will receive positive result on all feature checks
        public const string Check = "feature_check";
        
        // Whether base is unlocked at all
        public const string BaseAvailable = "feature_base_available";
        public const string BaseWorkshop = "feature_base_workshop";
        public const string BaseUpgrades = "feature_base_upgrades";
        public const string BaseRoster = "feature_base_roster";
        public const string BaseRosterAdvanced = "feature_base_roster_advanced";
        public const string BaseRosterDismissal = "feature_base_roster_dismissal";

        // Whether navigation is unlocked
        // Value has meaning: 1 is nav to mountain base, 2 is nav within the province 0, 3 is full nav
        public const string NavigationLevel = "feature_nav";

        public const string CombatUI = "feature_combat_ui";
        public const string CombatUnitSelection = "feature_combat_unit_selection";
        public const string CombatScenarioStatus = "feature_combat_scenario_status";
        public const string CombatActionHotbar = "feature_combat_action_hotbar";
        public const string CombatActionDrag = "feature_combat_action_drag";
        public const string CombatUnitClasses = "feature_combat_unit_classes";
        public const string CombatUnitLoadout = "feature_combat_unit_loadout";
        
        public const string CombatPilotIdentifiers = "feature_combat_pilot_identifiers";
        public const string CombatPilotHealth = "feature_combat_pilot_health";
        
        public const string PilotProgression = "feature_pilot_progression";

        
        public const string CombatReplay = "feature_combat_replay";
        public const string CombatEventLog = "feature_combat_event_log";
        
        // Whether prediction is unlocked
        // Value has meaning: 0 is bootable but not yet unlocked, 1 is unlocked fully with timeline visible
        public const string CombatPrediction = "feature_combat_prediction";

        public static Dictionary<string, int> featureValuesLast = new Dictionary<string, int> ();
        
    }
    
    public static class EventMemoryInt
    {
        public const string World_Counter_Resistance_Reputation = "world_reputation_guard";
        public const string World_Left_Tutorial_Province = "world_left_tutorial_province";
        
        public const string BaseVisualEarly = "base_visual_early";

        public const string UnitWeightOverride = "unit_weight_class_override";
    }
    
    public static class EventMemoryIntAutofilled
    {
        public const string World_Counter_Auto_SquadSizeLast = "world_auto_squad_size_last";
        
        public const string World_Counter_Auto_CombatEncounters = "world_auto_combat_encounters";
        public const string World_Counter_Auto_CombatVictory = "world_auto_combat_victory";
        public const string World_Counter_Auto_CombatDefeat = "world_auto_combat_defeat";

        public const string World_Counter_Auto_CombatFriendlyPilotDeath = "world_auto_combat_friendly_pilot_death";
        public const string World_Counter_Auto_CombatFriendlyUnitLoss = "world_auto_combat_friendly_unit_loss";
        public const string World_Counter_Auto_CombatEnemyPilotDeath = "world_auto_combat_enemy_pilot_death";
        public const string World_Counter_Auto_CombatEnemyUnitLoss = "world_auto_combat_enemy_unit_loss";

        public const string World_Counter_Auto_CombatVictoryStreak = "world_auto_combat_victory_streak";
        public const string World_Counter_Auto_CombatVictoryLevelRecord = "world_auto_combat_victory_level_record";
        public const string World_Counter_Auto_CombatVictoryThreatRecord = "world_auto_combat_victory_threat_record";
        public const string World_Counter_Auto_CombatVictoryThreatScaledRecord = "world_auto_combat_victory_threat_scaled_record";
        
        public const string World_Counter_Auto_WorkshopBuildSystem = "world_auto_workshop_build_system";
        public const string World_Counter_Auto_WorkshopBuildPart = "world_auto_workshop_build_part";
        public const string World_Counter_Auto_WorkshopBuildItem = "world_auto_workshop_build_item";
        public const string World_Counter_Auto_WorkshopBuildUnit = "world_auto_workshop_build_unit";
        public const string World_Counter_Auto_SalvageRecovered = "world_auto_salvage_recovered";
        public const string World_Counter_Auto_SalvageDismantled = "world_auto_salvage_dismantled";
        
        public const string World_Counter_Auto_EventsTotal = "world_auto_events_total";
        
        public const string World_Counter_Auto_ProvinceVisited = "world_auto_province_visited";
        public const string World_Counter_Auto_ProvinceWar = "world_auto_province_war";
        public const string World_Counter_Auto_ProvinceWarVictory = "world_auto_province_war_victory";
        public const string World_Counter_Auto_ProvinceWarDefeat = "world_counter_auto_province_war_defeat";
        public const string World_Counter_Auto_ProvinceWarTotal = "world_counter_auto_province_war_total";
        
        public const string World_Counter_Auto_ProvinceWarObjectivesCreated = "world_auto_province_war_objectives_created";
        public const string World_Counter_Auto_ProvinceWarObjectivesWon = "world_auto_province_war_objectives_won";
        public const string World_Counter_Auto_ProvinceWarBasesCreated = "world_auto_province_war_bases_created";
        public const string World_Counter_Auto_ProvinceWarBattlesCreated = "world_auto_province_war_battles_created";
        public const string World_Counter_Auto_ProvinceWarBattlesWon = "world_auto_province_war_battles_won";
        public const string World_Counter_Auto_ProvinceWarBattlesLost = "world_auto_province_war_battles_lost";
        public const string World_Counter_Auto_ProvinceWarCombatEntered = "world_auto_province_war_combat_entered";
        public const string World_Counter_Auto_ProvinceWarCombatWon = "world_auto_province_war_combat_won";
        
        public const string World_Counter_Auto_CombatEncountersInLoop = "world_auto_combat_encounters_loop";
        public const string World_Counter_Auto_CombatVictoryInLoop = "world_auto_combat_victory_loop";
        public const string World_Counter_Auto_CombatDefeatInLoop = "world_auto_combat_defeat_loop";
        public const string World_Counter_Auto_CombatFriendlyPilotDeathInLoop = "world_auto_combat_friendly_pilot_death_loop";
        public const string World_Counter_Auto_CombatFriendlyUnitLossInLoop = "world_auto_combat_friendly_unit_loss_loop";
        public const string World_Counter_Auto_CombatEnemyPilotDeathInLoop = "world_auto_combat_enemy_pilot_death_loop";
        public const string World_Counter_Auto_CombatEnemyUnitLossInLoop = "world_auto_combat_enemy_unit_loss_loop";
        public const string World_Counter_Auto_CombatPilotSurvivalStreak = "world_auto_combat_pilot_survival_streak";

        public const string World_Tag_Auto_CombatEntered = "world_auto_combat_entered";
        public const string World_Tag_Auto_WarBattleSite = "world_auto_war_battlesite";
        public const string World_Tag_Auto_WarObjective = "world_auto_war_objective";
        public const string World_Tag_Auto_WarDefeat = "world_auto_war_defeat";
        public const string World_Tag_Auto_WarVictory = "world_auto_war_victory";
        
        public const string World_Tag_Auto_CountdownExpired = "world_auto_countdown_expired";
        
        public const string World_Auto_RestCompleted = "world_auto_rest_completed";
        public const string World_Auto_War = "world_auto_war";

        public const string Province_Auto_EscalationLevel = "province_auto_escalation_level";
        public const string Province_Auto_Defeat = "province_auto_war_defeat";
        public const string Province_Auto_Victory = "province_auto_war_victory";
        public const string Province_Auto_AtWar = "province_auto_war";
        public const string Province_Auto_Visited = "province_auto_visited";
        
        public const string World_Auto_Salvage_Multiplier = "world_auto_salvage_multiplier";
        public const string World_Auto_Salvage_Multiplier_Consumable = "world_auto_salvage_multiplier_consumable";
        public const string World_Auto_Salvage_Offset = "world_auto_salvage_offset";
        public const string World_Auto_Salvage_Offset_Consumable = "world_auto_salvage_offset_consumable";
        
        public const string Pilot_Counter_Auto_CombatEncounters = "pilot_auto_combat_encounters";
        public const string Pilot_Counter_Auto_CombatVictory = "pilot_auto_combat_victory";
        public const string Pilot_Counter_Auto_CombatDefeat = "pilot_auto_combat_defeat";
        public const string Pilot_Counter_Auto_CombatKnockouts = "pilot_auto_combat_knockouts";
        public const string Pilot_Counter_Auto_CombatTakedowns = "pilot_auto_combat_takedowns";

        public const string Pilot_Tag_Auto_OriginatingProvince = "pilot_auto_originating_province";

        public const string Pilot_Internal_Bio_Index = "pilot_internal_bio_index";
        
        public const string World_Internal_Cta_Workshop = "world_internal_cta_workshop";
        public const string World_Internal_Cta_Upgrades = "world_internal_cta_upgrades";
        
        public const string Combat_Auto_LevelDestruction = "combat_sc_level_destruction";
        public const string Combat_Auto_LevelDestructionPercentage = "combat_sc_level_destruction_percentage";
        
        public const string Campaign_Progress_Total = "campaign_progress_total";
        public const string Campaign_Progress_Repeatable = "campaign_progress_repeatable";
        public const string Campaign_Countdown_Offset = "campaign_countdown_offset";
        public const string Campaign_Countdown_BaseLength = "campaign_countdown_base_length";
    }
    
    public static class EventMemoryFloatAutofilled
    {
        public const string World_Auto_TimeOfDay = "world_auto_time_of_day";
        public const string World_Auto_TimeTotal = "world_auto_time_total";
        
        public const string World_Auto_TimeInProvince = "world_auto_time_in_province";
        public const string World_Auto_TimeInDanger = "world_auto_time_in_danger";
        public const string World_Auto_TimeSinceCombat = "world_auto_time_since_combat";
        
        public const string World_Auto_WeatherFull = "world_auto_weather_full";
        public const string World_Auto_WeatherRain = "world_auto_weather_rain";
        
        public const string World_Auto_WarScorePlayer = "world_auto_war_score_player";
        public const string World_Auto_WarScoreEnemy = "world_auto_war_score_enemy";

        public const string World_Auto_Hope = "world_auto_hope";
    }
    
    
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckTag
    {
        [ValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.tags")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public string tag;
        
        [HideInInspector]
        public bool not;

        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckCombatReady
    {
        public bool ready;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckAI
    {
        public SortedDictionary<AgentAIState.StateDescription, bool> states = new SortedDictionary<AgentAIState.StateDescription, bool> ();
        public SortedDictionary<PlayerDetectionStatus.DetectionState, bool> detection = new SortedDictionary<PlayerDetectionStatus.DetectionState, bool> ();
    }


    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckPilots
    {
        public bool present;
        public bool factionChecked;
        
        [ShowIf ("factionChecked")]
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public string faction;
        
        [HideInInspector]
        public bool factionInverted;
        
        #if UNITY_EDITOR
        private void Invert () => factionInverted = !factionInverted;
        private string GetInversionLabel () => factionInverted ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (factionInverted ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }
    
    [Serializable]
    public class DataBlockSubcheckBool
    {
        // [ToggleLeft, LabelText ("@GetLabel ()")]
        [HideInInspector]
        public bool present;
        
        public override string ToString () => $"expected {present}";
        protected virtual string GetLabel () => present ? "Should be true" : "Should be false";
        
        #region Editor
        #if UNITY_EDITOR
        
        [PropertyOrder (-1)]
        [Button ("@GetLabel ()"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue () => present = !present;
        private Color GetBoolColor => Color.HSVToRGB (present ? 0.55f : 0f, 0.5f, 1f);
        
        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckBool : DataBlockSubcheckBool
    {
        
    }


    
    public enum FloatCheckMode
    {
        Less,
        RoughlyEqual,
        Greater,
        RoughlyNonEqual
    }

    [Serializable]
    public class DataBlockOverworldEventSubcheckFloat
    {
        [HideLabel]
        [HorizontalGroup]
        public FloatCheckMode check = FloatCheckMode.Less;

        [HorizontalGroup (0.2f)]
        [HideLabel]
        public float value;

        public bool IsPassed (bool valuePresent, float valueCurrent)
        {
            switch (check)
            {
                case FloatCheckMode.Less:
                {
                    if (!valuePresent || valueCurrent < value)
                        return true;
                    break;
                }

                case FloatCheckMode.RoughlyEqual:
                {
                    if (valuePresent && valueCurrent.RoughlyEqual (value))
                        return true;
                    break;
                }

                case FloatCheckMode.Greater:
                {
                    if (valuePresent && valueCurrent > value)
                        return true;
                    break;
                }
                    
                case FloatCheckMode.RoughlyNonEqual:
                {
                    if (!valuePresent || !valueCurrent.RoughlyEqual (value))
                        return true;
                    break;
                }
            }

            return false;
        }

        public override string ToString ()
        {
            return GetCheckDescription ();
        }
        
        public string GetCheckDescription ()
        {
            switch (check)
            {
                case FloatCheckMode.Less:               return $"less than {value:0.##}";
                case FloatCheckMode.RoughlyEqual:       return $"equal to {value:0.##}";
                case FloatCheckMode.Greater:            return $"greater than {value:0.##}";
                case FloatCheckMode.RoughlyNonEqual:    return $"not equal to {value:0.##}";
                default: return "?";
            }
        }
    }
    
    [Serializable]
    public class DataBlockOverworldEventSubcheckFloatRange
    {
        [HideInInspector]
        public bool required;
        
        public float min;
        public float max;

        public bool IsPassed (float valueCurrent)
        {
            bool inRange = valueCurrent >= min && valueCurrent <= max;
            return required ? inRange : !inRange;
        }

        public override string ToString ()
        {
            return GetCheckDescription ();
        }
        
        public string GetCheckDescription ()
        {
            return required ? $"in range {min:0.##}-{max:0.##}" : $"outside range {min:0.##}-{max:0.##}";
        }
        
        #if UNITY_EDITOR

        [PropertyOrder (-1)]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue ()
        {
            required = !required;
        }
        
        private string GetBoolLabel => required ? "Should be in range" : "Should be outside range";
        private Color GetBoolColor => Color.HSVToRGB (required ? 0.55f : 0f, 0.5f, 1f);
        
        #endif
    }
    
    [Serializable]
    public class DataBlockOverworldEventSubcheckIntRange
    {
        [HideInInspector]
        public bool required;
        
        public int min;
        public int max;

        public bool IsPassed (float valueCurrent)
        {
            bool inRange = valueCurrent >= min && valueCurrent <= max;
            return required ? inRange : !inRange;
        }

        public override string ToString ()
        {
            return GetCheckDescription ();
        }
        
        public string GetCheckDescription ()
        {
            return required ? $"in range {min}-{max}" : $"outside range {min}-{max}";
        }
        
        #if UNITY_EDITOR

        [PropertyOrder (-1)]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue ()
        {
            required = !required;
        }
        
        private string GetBoolLabel => required ? "Should be in range" : "Should be outside range";
        private Color GetBoolColor => Color.HSVToRGB (required ? 0.55f : 0f, 0.5f, 1f);
        
        #endif
    }
    
    
    
    public enum IntCheckMode
    {
        Bool,
        Less,
        LessEqual,
        Equal,
        GreaterEqual,
        Greater,
        NonEqual
    }
    
    [Serializable]
    public class DataBlockOverworldEventSubcheckInt
    {
        [LabelText ("$GetLabel"), LabelWidth (100f)]
        [HorizontalGroup]
        [OnValueChanged ("OnCheckModeChange")]
        public IntCheckMode check = IntCheckMode.Less;

        [HorizontalGroup (0.2f)]
        [HideLabel]
        [ShowIf ("IsValueVisible")]
        public int value;

        public virtual bool IsPassed (bool valuePresent, int valueCurrent)
        {
            switch (check)
            {
                case IntCheckMode.Bool:
                {
                    bool valueExpected = value > 0;
                    if (valuePresent == valueExpected)
                        return true;
                    break;
                }
                
                case IntCheckMode.Less:
                {
                    if (!valuePresent || valueCurrent < value)
                        return true;
                    break;
                }
                    
                case IntCheckMode.LessEqual:
                {
                    if (!valuePresent || valueCurrent <= value)
                        return true;
                    break;
                }
                    
                case IntCheckMode.Equal:
                {
                    if (valuePresent && valueCurrent == value)
                        return true;
                    break;
                }
                    
                case IntCheckMode.GreaterEqual:
                {
                    if (valuePresent && valueCurrent >= value)
                        return true;
                    break;
                }
                    
                case IntCheckMode.Greater:
                {
                    if (valuePresent && valueCurrent > value)
                        return true;
                    break;
                }
                    
                case IntCheckMode.NonEqual:
                {
                    if (!valuePresent || valueCurrent != value)
                        return true;
                    break;
                }
            }

            return false;
        }

	    public override string ToString()
        {
	        string op = "";

            switch (check)
            {
                case IntCheckMode.Bool:               return $"equal to {(value > 0? "True" : "False" )}";
                case IntCheckMode.Less:               return $"less than {value}";
                case IntCheckMode.LessEqual:          return $"less or equal to {value}";
                case IntCheckMode.Equal:              return $"equal to {value}";
                case IntCheckMode.GreaterEqual:       return $"greater or equal to {value}";
                case IntCheckMode.Greater:            return $"greater than {value}";
                case IntCheckMode.NonEqual:           return $"not equal to {value}";
            }

            return $"{op} {value}";
        }
        
        protected virtual string GetLabel ()
        {
            return "Check";
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [HorizontalGroup (0.2f)]
        [HideIf ("IsValueVisible")]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue ()
        {
            if (value == 0)
                value = 1;
            else
                value = 0;
        }

        private void OnCheckModeChange ()
        {
            if (check == IntCheckMode.Bool)
                value = value > 0 ? 1 : 0;
        }
        
        private bool IsValueVisible => check != IntCheckMode.Bool;
        private string GetBoolLabel => value > 0 ? "True" : "False";
        private Color GetBoolColor => Color.HSVToRGB (value > 0 ? 0.55f : 0f, 0.5f, 1f);
        
        #endif
        #endregion
    }

    
    
    
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckMemoryInt : DataBlockOverworldEventSubcheckInt
    {
        [ValueDropdown ("GetKeys")]
        public string key;

        [YamlIgnore, HideInInspector]
        public DataBlockOverworldMemoryCheckGroup parent;

        public override string ToString()
        {
	        return $"{key} {base.ToString()}";
        }

        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetKeys => DataMultiLinkerOverworldMemory.data.Keys;

        #endif
        #endregion
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckMemoryFloat : DataBlockOverworldEventSubcheckFloat
    {
        [ValueDropdown ("GetKeys")]
        public string key;

        [YamlIgnore, HideInInspector]
        public DataBlockOverworldMemoryCheckGroup parent;

        public override string ToString()
        {
	        return $"{key} {base.ToString()}";
        }

        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetKeys => DataMultiLinkerOverworldMemory.data.Keys;

        #endif
        #endregion
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckFloatKeyGeneric : DataBlockOverworldEventSubcheckFloat
    {
        public string key;

        public override string ToString ()
        {
            return $"{GetCheckDescription ()} (to {key})";
        }
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckFloatForStates : DataBlockOverworldEventSubcheckFloat
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        public string key;
        
        public override string ToString ()
        {
            return $"{GetCheckDescription ()} (to state {key})";
        }
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckFloatForRetreats : DataBlockOverworldEventSubcheckFloat
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetRetreatKeys\")")]
        public string key;
        
        public override string ToString ()
        {
            return $"{GetCheckDescription ()} (to retreat {key})";
        }
    }




    public class DataBlockOverworldEventCheck
    {
        
    }
}