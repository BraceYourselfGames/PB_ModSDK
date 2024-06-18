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
        
        // Whether check for features is performed at all.
        // Old saves won't have that flag and will receive positive result on all feature checks
        public const string Check = "feature_check";
        
        // Whether base is unlocked at all
        public const string BaseAvailable = "feature_base_available";
        public const string BaseWorkshop = "feature_base_workshop";
        public const string BaseUpgrades = "feature_base_upgrades";
        public const string BaseAbilities = "feature_base_abilities";

        // Whether navigation is unlocked
        // Value has meaning: 1 is nav to mountain base, 2 is nav within the province 0, 3 is full nav
        public const string NavigationLevel = "feature_nav";
        
        public const string OverworldContest = "feature_overworld_contest";
        public const string OverworldSalvageFrames = "feature_overworld_salvage_frames";

        public const string CombatUI = "feature_combat_ui";
        public const string CombatUnitSelection = "feature_combat_unit_selection";
        public const string CombatScenarioStatus = "feature_combat_scenario_status";
        public const string CombatActionHotbar = "feature_combat_action_hotbar";
        public const string CombatActionDrag = "feature_combat_action_drag";
        public const string CombatUnitClasses = "feature_combat_unit_classes";
        public const string CombatUnitLoadout = "feature_combat_unit_loadout";
        public const string CombatPilotIdentifiers = "feature_combat_pilot_identifiers";
        public const string CombatPilotHealth = "feature_combat_pilot_health";
        
        public const string CombatReplay = "feature_combat_replay";
        
        // Whether prediction is unlocked
        // Value has meaning: 0 is bootable but not yet unlocked, 1 is unlocked fully with timeline visible
        public const string CombatPrediction = "feature_combat_prediction";

        public static Dictionary<string, int> featureValuesLast = new Dictionary<string, int> ();
        
    }
    
    public static class EventMemoryInt
    {
        public const string World_Counter_Resistance_Reputation = "world_reputation_guard";
        public const string World_Left_Tutorial_Province = "world_left_tutorial_province";
    }
    
    public static class EventMemoryIntAutofilled
    {
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
        
        public const string World_Counter_Auto_WorkshopBuildPart = "world_auto_workshop_build_part";
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

        public const string World_Auto_War = "world_auto_war";

        public const string Province_Auto_EscalationLevel = "province_auto_escalation_level";
        public const string Province_Auto_Defeat = "province_auto_war_defeat";
        public const string Province_Auto_Victory = "province_auto_war_victory";
        public const string Province_Auto_AtWar = "province_auto_war";
        public const string Province_Auto_Visited = "province_auto_visited";
        
        public const string Pilot_Counter_Auto_CombatEncounters = "pilot_auto_combat_encounters";
        public const string Pilot_Counter_Auto_CombatVictory = "pilot_auto_combat_victory";
        public const string Pilot_Counter_Auto_CombatDefeat = "pilot_auto_combat_defeat";
        public const string Pilot_Counter_Auto_CombatKills = "pilot_auto_combat_kills";
        public const string Pilot_Counter_Auto_CombatTakedowns = "pilot_auto_combat_takedowns";
        
        public const string Pilot_Tag_Auto_OriginatingProvince = "pilot_auto_originating_province";
        public const string Pilot_Tag_Auto_OriginatingEntitiy = "pilot_auto_originating_entity";

        public const string Pilot_Internal_Bio_Index = "pilot_internal_bio_index";
        
        public const string World_Internal_Cta_Workshop = "world_internal_cta_workshop";
        public const string World_Internal_Cta_Upgrades = "world_internal_cta_upgrades";
        
        public const string Combat_Auto_LevelDestruction = "combat_sc_level_destruction";
        public const string Combat_Auto_LevelDestructionPercentage = "combat_sc_level_destruction_percentage";
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
    
    public sealed class AgentAIState
    {
        public enum StateDescription
        {
            Calm = 0,
            Investigating = 1,
            Alerted = 2
        }

        public StateDescription state;
    }
    
    public sealed class PlayerDetectionStatus
    {
        public enum DetectionState
        {
            NotDetected,
            InProgress,
            Detected
        }

        public DetectionState state;
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

    [Serializable][HideReferenceObjectPicker]
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
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckFloatRange
    {
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
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckIntRange
    {
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
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckInt
    {
        [HorizontalGroup]
        [OnValueChanged ("OnCheckModeChange")]
        [HideLabel]
        public IntCheckMode check = IntCheckMode.Less;

        [HorizontalGroup (0.2f)]
        [HideLabel]
        [ShowIf ("IsValueVisible")]
        public int value;

        public bool IsPassed (bool valuePresent, int valueCurrent)
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

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckAction
    {
        [HideInInspector]
        public bool actionDesired;
        
        public OverworldEntitySource owner = OverworldEntitySource.EventSelf;
        
        [ShowIf ("actionTargetChecked")]
        [LabelText ("Target Compared")]
        public OverworldEntitySource actionTargetComparison = OverworldEntitySource.EventTarget;

        [LabelText ("Target Check")]
        public bool actionTargetChecked = false;

        public bool tagsUsed = false;
        
        [HideIf ("tagsUsed")]
        [ValueDropdown ("GetActionKeys")]
        public string key;
        
        [ShowIf ("tagsUsed")]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldAction.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;

        #region Editor
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetActionKeys => DataMultiLinkerOverworldAction.data.Keys;
        
        [Button ("@GetInversionLabel (actionDesired)"), GUIColor ("@GetInversionColor (actionDesired)"), PropertyOrder (-1)]
        private void Invert () => actionDesired = !actionDesired;
        private string GetInversionLabel (bool value) => value ? "Required" : "Prohibited";
        private Color GetInversionColor (bool value) => Color.HSVToRGB (value ? 0.55f : 0f, 0.5f, 1f);
        
        #endif
        #endregion
        
    }




    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckFaction
    {
        public bool hostileCheck;
        
        [ShowIf ("hostileCheck")]
        public bool hostile;
        
        [HideIf ("hostileCheck")]
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public HashSet<string> factions = new HashSet<string> ();

        public override string ToString()
        {
	        if(hostileCheck)
				return $"hostile == {hostile}";
            else
	        {
                string result = "";

                foreach (var f in factions)
                {
					if(result.Length > 0)
						result += ", ";

	                result += f;
                }

                return result;
	        }
        }
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckProvinceDefender
    {
        public bool defender;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckProvinceAccess
    {
        public bool accessible;
        public bool partially;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckProvinceHope
    {
        public List<DataBlockOverworldEventSubcheckInt> checks;
    }
    
    public static class MovementModes
    {
        public const string normal = "normal";
        public const string fast = "fast";
        public const string stealth = "stealth";
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckMovementMode
    {
        [ValueDropdown ("GetKeys")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public HashSet<string> modes = new HashSet<string> { MovementModes.normal };
        
        [HideInInspector]
        public bool not;

        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        private IEnumerable<string> GetKeys () => FieldReflectionUtility.GetConstantStringFieldValues (typeof (MovementModes));
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldEventSubcheckWeather
    {
        [OnValueChanged ("CheckValue")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public Vector2 range;
        
        [HideInInspector]
        public bool not;

        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        private void CheckValue () => range = new Vector2 (Mathf.Clamp01 (range.x), Mathf.Clamp (range.y, range.x, 1f));
        #endif
    }

    public class DataBlockOverworldEventCheckSelf
    {
        [ShowIf ("@tags != null && tags.Count > 0")]
        public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;
        
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<DataBlockOverworldEventSubcheckTag> tags;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> baseParts;

        [DropdownReference (true)]
        public DataBlockOverworldMemoryCheckGroup eventMemory;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckCombatReady combatReady;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool movement;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckMovementMode movementMode;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool deployment;

        [DropdownReference]
        public Dictionary<string, DataBlockOverworldEventSubcheckInt> resources;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt pilotsAvailable;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt unitsAvailable;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckPilots pilots;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool units;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt levelDeltaTarget;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt levelDeltaProvince;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckWeather weather;

        #if !PB_MODSDK
        
        private static StringBuilder sb1 = new StringBuilder ();
        private static StringBuilder sb2 = new StringBuilder ();
        private static List<PersistentEntity> pilotsTemp = new List<PersistentEntity> ();

        public bool IsValid (OverworldEntity selfOverworld, bool log = false)
        {
            if (log)
            {
                sb1.Clear ();
                sb2.Clear ();
            }

            // Fetching self entities

            bool selfValid = true;
            bool selfPresent = selfOverworld != null;

            var selfPersistent = selfPresent ? IDUtility.GetLinkedPersistentEntity (selfOverworld) : null;
            var selfBlueprint = selfPresent && selfOverworld.hasDataLinkOverworldEntityBlueprint ? selfOverworld.dataLinkOverworldEntityBlueprint.data : null;

            // Fetching province entities (they might be useful even before province block, e.g. for level delta comparison
            


            if (selfPresent && selfPersistent != null && selfPersistent.isOverworldTag && selfBlueprint != null)
            {
                bool tagsValid = true;
                if (tags != null && tags.Count > 0)
                {
                    int tagMatches = 0;
                    var tagsOnBlueprint = selfBlueprint.tagsProcessed;
                    foreach (var tagRequirement in tags)
                    {
                        bool required = !tagRequirement.not;
                        if (tagsOnBlueprint.Contains (tagRequirement.tag) == required)
                        {
                            tagMatches += 1;
                            if (tagsMethod == EntityCheckMethod.RequireOne)
                                break;
                        }
                    }
                
                    if (tagsMethod == EntityCheckMethod.RequireOne)
                        tagsValid = tagMatches > 0;

                    else if (tagsMethod == EntityCheckMethod.RequireAll)
                        tagsValid = tagMatches == tags.Count;
                    
                    if (log && !tagsValid)
                    {
                        sb2.Append ("\n- Base has incompatible tags | Conditions ");
                        sb2.Append (tagsMethod == EntityCheckMethod.RequireOne ? "(one match required):" : "(all required):");
                        
                        foreach (var tagRequirement in tags)
                        {
                            bool required = !tagRequirement.not;
                            bool present = tagsOnBlueprint.Contains (tagRequirement.tag);
                            
                            if (required != present)
                            {
                                sb2.Append ("\n  - ");
                                sb2.Append (tagRequirement.tag);
                                sb2.Append (required ? " must be present" : " must be absent");
                            }
                        }
                    }
                }

                bool basePartsValid = true;
                if (baseParts != null && baseParts.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var partsInstalled = overworld.hasBasePartsInstalled ? overworld.basePartsInstalled.s : null;
                    bool partsInstalledFound = partsInstalled != null && partsInstalled.Count > 0;
                
                    foreach (var kvp in baseParts)
                    {
                        var partKey = kvp.Key;
                        var countRequired = kvp.Value;
                        int countCurrent = partsInstalledFound && partsInstalled.ContainsKey (partKey) ? partsInstalled[partKey] : 0;

                        if (countCurrent < countRequired)
                        {
                            basePartsValid = false;
                            break;
                        }
                    }
                }
                
                bool memoryValid = true;
                if (eventMemory != null)
                {
                    memoryValid = eventMemory.IsPassed (selfPersistent);
                    if (log && !memoryValid)
                    {
                        sb2.Append ("\n- Base has incompatible memories | ");
                        sb2.Append (eventMemory.ToStringWithComparison (selfPersistent));
                    }
                }
                
                bool combatReadyValid = true;
                if (combatReady != null)
                {
                    bool ready = Contexts.sharedInstance.persistent.isPlayerCombatReady;
                    combatReadyValid = ready == combatReady.ready;
                    if (log && !combatReadyValid)
                    {
                        sb2.Append ("\n- Base should ");
                        sb2.Append (combatReady.ready ? "be ready for combat" : "not be ready for combat");
                    }
                }
                
                bool movementValid = true;
                if (movement != null)
                {
                    movementValid = selfOverworld.hasPath == movement.present;
                    if (log && !movementValid)
                    {
                        sb2.Append ("\n- Base should be ");
                        sb2.Append (movement.present ? "moving" : "stopped");
                    }
                }
                
                bool movementModeValid = true;
                if (movementMode != null && movementMode.modes != null && movementMode.modes.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var movementModeKey = overworld.hasActiveMovementMode ? overworld.activeMovementMode.key : MovementModes.normal;
                    bool required = !movementMode.not;
                    bool match = movementMode.modes.Contains (movementModeKey);
                    movementModeValid = required == match;

                    if (log && !movementModeValid)
                    {
                        sb2.Append ("\n- Base should be in movement mode ");
                        sb2.Append (movementMode.modes.ToStringFormatted (appendBrackets: false));
                        sb2.Append (", currently in ");
                        sb2.Append (movementModeKey);
                    }
                }

                bool weatherValid = true;
                if (weather != null)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var weatherColor = overworld.hasPlayerWeather ? overworld.playerWeather.c : Color.black;

                    bool required = !weather.not;
                    bool match = weatherColor.b >= weather.range.x && weatherColor.b <= weather.range.y;
                    weatherValid = required == match;
                    
                    if (log && !weatherValid)
                    {
                        sb2.Append ("\n- Base weather should be ");
                        sb2.Append (required ? "in range of" : "outside range of");
                        sb2.Append (weather.range.x.ToString ("0.##"));
                        sb2.Append (" - ");
                        sb2.Append (weather.range.y.ToString ("0.##"));
                        sb2.Append (", currently ");
                        sb2.Append (weatherColor.b.ToString ("0.##"));
                    }
                }
                
                bool deploymentValid = true;
                if (deployment != null)
                {
                    deploymentValid = selfOverworld.isDeployed == deployment.present;
                    if (log && !deploymentValid)
                    {
                        sb2.Append ("\n- Base should ");
                        sb2.Append (deployment.present ? "be deployed" : "not be deployed");
                    }
                }
                
                bool resourcesValid = true;
                if (resources != null && resources.Count > 0)
                {
                    var resourcesOnBase = selfPersistent.hasInventoryResources ? selfPersistent.inventoryResources.s : null;
                    foreach (var kvp in resources)
                    {
                        var key = kvp.Key;
                        var amountCurrent = resourcesOnBase != null && resourcesOnBase.ContainsKey (key) ? resourcesOnBase[key] : 0f;
                        var amountCurrentRound = Mathf.RoundToInt (amountCurrent);
                        
                        bool passed = kvp.Value.IsPassed (true, amountCurrentRound);
                        if (!passed)
                        {
                            resourcesValid = false;
                            
                            if (log)
                            {
                                sb2.Append ($"\n- Base failed a resource check\n  - ");
                                sb2.Append (key);
                                sb2.Append (" (");
                                sb2.Append (amountCurrent.ToString ("0.##"));
                                sb2.Append ("): ");
                                sb2.Append (kvp.Value.ToString ());
                            }
                            
                            break;
                        }
                    }
                }
                
                bool pilotsAvailableValid = true;
                if (pilotsAvailable != null)
                {
                    var persistent = Contexts.sharedInstance.persistent;
                    var pilotCountReady = persistent.hasPlayerCombatStats ? persistent.playerCombatStats.pilotCountReady : 0;
                    pilotsAvailableValid = pilotsAvailable.IsPassed (true, pilotCountReady);
                    
                    if (log && !pilotsAvailableValid)
                    {
                        sb2.Append ("\n- Base failed check for available pilots (");
                        sb2.Append (pilotCountReady);
                        sb2.Append (": ");
                        sb2.Append (pilotsAvailable);
                    }
                }
                
                bool unitsAvailableValid = true;
                if (unitsAvailable != null)
                {
                    var persistent = Contexts.sharedInstance.persistent;
                    var unitCountReady = persistent.hasPlayerCombatStats ? persistent.playerCombatStats.unitCountReady : 0;
                    unitsAvailableValid = unitsAvailable.IsPassed (true, unitCountReady);
                    
                    if (log && !unitsAvailableValid)
                    {
                        sb2.Append ("\n- Base failed check for available units (");
                        sb2.Append (unitCountReady);
                        sb2.Append (": ");
                        sb2.Append (unitsAvailable);
                    }
                }
                
                bool pilotsValid = true;
                if (pilots != null)
                {
                    pilotsTemp.Clear ();
                    var pilotCandidates = Contexts.sharedInstance.persistent.GetEntitiesWithEntityLinkPersistentParent (selfPersistent.id.id);

                    foreach (var entity in pilotCandidates)
                    {
                        // Only grab pilots attached to the base, no point supporting unit attached pilots as they'll be deprecated soon
                        if (!entity.isPilotTag || !entity.hasPilotIdentification || !entity.hasPilotHealth)
                            continue;
                
                        // Skip dead or dying pilots
                        if (entity.isDeceased || entity.pilotHealth.f <= 0f)
                            continue;
                        
                        pilotsTemp.Add (entity);
                    }

                    bool selfPilotsPresent = pilotsTemp.Count > 0;
                    bool selfPilotsPresentMatch = selfPilotsPresent == pilots.present;
                    bool selfPilotsFactionMatch = true;
                
                    if (selfPilotsPresent && pilots.factionChecked && pilotsTemp.Count > 0)
                    {
                        foreach (var pilot in pilotsTemp)
                        {
                            if (pilot.faction.s != pilots.faction)
                            {
                                selfPilotsFactionMatch = false;
                                break;
                            }
                        }
                    }

                    pilotsValid = selfPilotsPresentMatch && selfPilotsFactionMatch;
                    
                    if (log && !pilotsValid)
                    {
                        sb2.Append ("\n- Base failed check for presence of pilots: ");
                        sb2.Append (pilots.present ? "must be present" : "must be absent");
                    
                        if (pilots.factionChecked)
                        {
                            sb2.Append (pilots.factionInverted ? ", must not have faction " : ", must have faction ");
                            sb2.Append (pilots.faction);
                        }
                    }
                }

                bool unitsValid = true;
                if (units != null)
                {
                    var unitsLinked = Contexts.sharedInstance.persistent.GetEntitiesWithEntityLinkPersistentParent (selfPersistent.id.id);
                    var unitCount = 0;

                    foreach (var unitCandidate in unitsLinked)
                    {
                        if (unitCandidate.isUnitTag && !unitCandidate.isDestroyed && !unitCandidate.isWrecked)
                            unitCount += 1;
                    }
                    
                    var unitsPresent = unitCount > 0;
                    unitsValid = units.present == unitsPresent;
                    
                    if (log && !unitsValid)
                    {
                        sb2.Append ("\n- Base failed check for presence of units: ");
                        sb2.Append (units.present ? "must be present" : "must be absent");
                    }
                }

                int levelPlayerBase = 1;
                if (levelDeltaTarget != null || levelDeltaProvince != null)
                {
                    var levelRange = OverworldStatResolver.ResolveUnitsLevelRange (null, selfOverworld);
                    levelPlayerBase = Mathf.RoundToInt ((levelRange.x + levelRange.y) * 0.5f);
                }

                selfValid =
                    tagsValid &&
                    basePartsValid &&
                    memoryValid &&
                    combatReadyValid &&
                    movementValid &&
                    movementModeValid &&
                    weatherValid &&
                    deploymentValid &&
                    resourcesValid &&
                    pilotsAvailableValid &&
                    unitsAvailableValid &&
                    pilotsValid &&
                    unitsValid;
            }

            return selfValid;
        }
        
        #endif

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventCheckSelf () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockOverworldEventCheckActors
    {
        [DropdownReference]
        public Dictionary<string, bool> actorsWorldPresent;
        
        [DropdownReference]
        public Dictionary<string, bool> actorsUnitsPresent;
        
        [DropdownReference]
        public Dictionary<string, bool> actorsPilotsPresent;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventCheckActors () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockOverworldEventCheckTarget
    {
        [ShowIf ("@tags != null && tags.Count > 0")]
        public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;
        
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<DataBlockOverworldEventSubcheckTag> tags;
        
        [DropdownReference (true)]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public DataBlockOverworldMemoryCheckGroup eventMemory;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckAI ai;

        [DropdownReference]
        public Dictionary<string, DataBlockOverworldEventSubcheckInt> resources;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckPilots pilots;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool units;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFaction faction;

        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckBool resupplyPoint;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventCheckTarget () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public enum ProvincePositionProvider
    {
        Self,
        Target
    }
    
    public class DataBlockOverworldEventCheckProvince
    {
        public ProvincePositionProvider positionProvider = ProvincePositionProvider.Self;
        
        [DropdownReference (true)]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public DataBlockOverworldMemoryCheckGroup eventMemory;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckFaction faction;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckProvinceAccess access;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldEventCheckProvince () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public enum OverworldEntitySource
    {
        EventSelf,
        EventTarget
    }
    
    public class DataBlockOverworldEventCheckAction
    {
        [ShowIf ("@actions != null && actions.Count > 0")]
        public EntityCheckMethod actionMethod = EntityCheckMethod.RequireAll;
        
        [EnableIf ("actions")]
        [HorizontalGroup ("actions")]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<DataBlockOverworldEventSubcheckAction> actions;

        #region Editor
        #if UNITY_EDITOR

        [Button ("@DataEditor.GetToggleLabel (actions)"), HorizontalGroup ("actions", DataEditor.toggleButtonWidth)]
        private void ToggleActions () => 
            actions = actions == null ? new List<DataBlockOverworldEventSubcheckAction> () : null;
        
        #endif
        #endregion

    }




    public class DataBlockOverworldEventCheck
    {
        [YamlIgnore, HideInEditorMode, ToggleLeft]
        public bool forceLog = false;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventCheckSelf self;

        [DropdownReference (true)]
        public DataBlockOverworldEventCheckTarget target;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventCheckProvince province;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventCheckAction action;

        [DropdownReference (true)]
        public DataBlockOverworldEventCheckActors actors;
        
        
        [HideInInspector, YamlIgnore]
        public DataBlockOverworldEventStep parentStep;

        [HideInInspector, YamlIgnore]
        public DataContainerOverworldEventOption parentOption;
        
        public void RefreshParents (DataBlockOverworldEventStep parentStep = null, DataContainerOverworldEventOption parentOption = null)
        {
            this.parentStep = parentStep;
            this.parentOption = parentOption;
        }
        
        #if !PB_MODSDK
        
        private static StringBuilder sb1 = new StringBuilder ();
        private static StringBuilder sb2 = new StringBuilder ();
        private static List<PersistentEntity> pilotsTemp = new List<PersistentEntity> ();
        private static int actionOwnerIDLast = IDUtility.invalidID;
        private static HashSet<OverworldActionEntity> actionsFromOwnerLast;

        public bool IsPassed (OverworldEntity selfOverworld, OverworldEntity targetOverworld, bool log = false)
        {
			if (forceLog)
				log = true;

            if (log)
            {
                sb1.Clear ();
                sb2.Clear ();
            }

            // Fetching self entities

            bool selfValid = true;
            bool selfPresent = selfOverworld != null;
            bool selfChecked = self != null;
            
            var selfPersistent = selfPresent ? IDUtility.GetLinkedPersistentEntity (selfOverworld) : null;
            var selfBlueprint = selfPresent && selfOverworld.hasDataLinkOverworldEntityBlueprint ? selfOverworld.dataLinkOverworldEntityBlueprint.data : null;

            // Fetching target entities (they might be useful even before target block, e.g. for level delta comparison)
            bool targetPresent = targetOverworld != null && !targetOverworld.isDestroyed;
            bool targetChecked = target != null;
            
            bool targetValid = targetChecked == false || targetOverworld != null;
            
            var targetPersistent = targetPresent ? IDUtility.GetLinkedPersistentEntity (targetOverworld) : null;
            var targetBlueprint = targetPresent && targetOverworld.hasDataLinkOverworldEntityBlueprint ? targetOverworld.dataLinkOverworldEntityBlueprint.data : null;

            // Fetching province entities (they might be useful even before province block, e.g. for level delta comparison
            
            bool provinceValid = true;
            bool provinceChecked = province != null;
            
            OverworldEntity provinceOverworldSelf = null;
            PersistentEntity provincePersistentSelf = null;
            bool provincePresentSelf = false;
            
            if ((provinceChecked && province.positionProvider == ProvincePositionProvider.Self) || (selfChecked && self.levelDeltaProvince != null))
            {
                bool provincePositionFound = false;
                var provincePosition = Vector3.zero;
                string provinceKey = selfPresent ? DataHelperProvince.GetProvinceKeyAtEntity (selfOverworld) : DataHelperProvince.GetProvinceKeyAtPositionExpensive (provincePosition);

                provinceOverworldSelf = provinceKey != null ? IDUtility.GetOverworldEntity (provinceKey) : null;
                provincePersistentSelf = provinceOverworldSelf != null ? IDUtility.GetLinkedPersistentEntity (provinceOverworldSelf) : null;
                provincePresentSelf = provinceOverworldSelf != null && provincePersistentSelf != null;
            }
            
            OverworldEntity provinceOverworldTarget = null;
            PersistentEntity provincePersistentTarget = null;
            bool provincePresentTarget = false;
            
            if (provinceChecked && province.positionProvider == ProvincePositionProvider.Target)
            {
                bool provincePositionFound = false;
                var provincePosition = Vector3.zero;
                string provinceKey = targetPresent ? DataHelperProvince.GetProvinceKeyAtEntity (selfOverworld) : DataHelperProvince.GetProvinceKeyAtPositionExpensive (provincePosition);

                provinceOverworldTarget = provinceKey != null ? IDUtility.GetOverworldEntity (provinceKey) : null;
                provincePersistentTarget = provinceOverworldTarget != null ? IDUtility.GetLinkedPersistentEntity (provinceOverworldTarget) : null;
                provincePresentTarget = provinceOverworldTarget != null && provincePersistentTarget != null;
            }

            if (selfPresent && selfChecked && selfPersistent != null && selfPersistent.isOverworldTag && selfBlueprint != null)
            {
                bool tagsValid = true;
                if (self.tags != null && self.tags.Count > 0)
                {
                    int tagMatches = 0;
                    var tags = selfBlueprint.tagsProcessed;
                    foreach (var tagRequirement in self.tags)
                    {
                        bool required = !tagRequirement.not;
                        if (tags.Contains (tagRequirement.tag) == required)
                        {
                            tagMatches += 1;
                            if (self.tagsMethod == EntityCheckMethod.RequireOne)
                                break;
                        }
                    }
                
                    if (self.tagsMethod == EntityCheckMethod.RequireOne)
                        tagsValid = tagMatches > 0;

                    else if (self.tagsMethod == EntityCheckMethod.RequireAll)
                        tagsValid = tagMatches == self.tags.Count;
                    
                    if (log && !tagsValid)
                    {
                        sb2.Append ("\n- Base has incompatible tags | Conditions ");
                        sb2.Append (self.tagsMethod == EntityCheckMethod.RequireOne ? "(one match required):" : "(all required):");
                        
                        foreach (var tagRequirement in self.tags)
                        {
                            bool required = !tagRequirement.not;
                            bool present = tags.Contains (tagRequirement.tag);
                            
                            if (required != present)
                            {
                                sb2.Append ("\n  - ");
                                sb2.Append (tagRequirement.tag);
                                sb2.Append (required ? " must be present" : " must be absent");
                            }
                        }
                    }
                }

                bool basePartsValid = true;
                if (self.baseParts != null && self.baseParts.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var partsInstalled = overworld.hasBasePartsInstalled ? overworld.basePartsInstalled.s : null;
                    bool partsInstalledFound = partsInstalled != null && partsInstalled.Count > 0;
                
                    foreach (var kvp in self.baseParts)
                    {
                        var partKey = kvp.Key;
                        var countRequired = kvp.Value;
                        int countCurrent = partsInstalledFound && partsInstalled.ContainsKey (partKey) ? partsInstalled[partKey] : 0;

                        if (countCurrent < countRequired)
                        {
                            basePartsValid = false;
                            break;
                        }
                    }
                }
                
                bool memoryValid = true;
                if (self.eventMemory != null)
                {
                    memoryValid = self.eventMemory.IsPassed (selfPersistent);
                    if (log && !memoryValid)
                    {
                        sb2.Append ("\n- Base has incompatible memories | ");
                        sb2.Append (self.eventMemory.ToStringWithComparison (selfPersistent));
                    }
                }
                
                bool combatReadyValid = true;
                if (self.combatReady != null)
                {
                    bool ready = Contexts.sharedInstance.persistent.isPlayerCombatReady;
                    combatReadyValid = ready == self.combatReady.ready;
                    if (log && !combatReadyValid)
                    {
                        sb2.Append ("\n- Base should ");
                        sb2.Append (self.combatReady.ready ? "be ready for combat" : "not be ready for combat");
                    }
                }
                
                bool movementValid = true;
                if (self.movement != null)
                {
                    movementValid = selfOverworld.hasPath == self.movement.present;
                    if (log && !movementValid)
                    {
                        sb2.Append ("\n- Base should be ");
                        sb2.Append (self.movement.present ? "moving" : "stopped");
                    }
                }
                
                bool movementModeValid = true;
                if (self.movementMode != null && self.movementMode.modes != null && self.movementMode.modes.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var movementModeKey = overworld.hasActiveMovementMode ? overworld.activeMovementMode.key : MovementModes.normal;
                    bool required = !self.movementMode.not;
                    bool match = self.movementMode.modes.Contains (movementModeKey);
                    movementModeValid = required == match;

                    if (log && !movementModeValid)
                    {
                        sb2.Append ("\n- Base should be in movement mode ");
                        sb2.Append (self.movementMode.modes.ToStringFormatted (appendBrackets: false));
                        sb2.Append (", currently in ");
                        sb2.Append (movementModeKey);
                    }
                }

                bool weatherValid = true;
                if (self.weather != null)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var weatherColor = overworld.hasPlayerWeather ? overworld.playerWeather.c : Color.black;

                    bool required = !self.weather.not;
                    bool match = weatherColor.b >= self.weather.range.x && weatherColor.b <= self.weather.range.y;
                    weatherValid = required == match;
                    
                    if (log && !weatherValid)
                    {
                        sb2.Append ("\n- Base weather should be ");
                        sb2.Append (required ? "in range of" : "outside range of");
                        sb2.Append (self.weather.range.x.ToString ("0.##"));
                        sb2.Append (" - ");
                        sb2.Append (self.weather.range.y.ToString ("0.##"));
                        sb2.Append (", currently ");
                        sb2.Append (weatherColor.b.ToString ("0.##"));
                    }
                }
                
                bool deploymentValid = true;
                if (self.deployment != null)
                {
                    deploymentValid = selfOverworld.isDeployed == self.deployment.present;
                    if (log && !deploymentValid)
                    {
                        sb2.Append ("\n- Base should ");
                        sb2.Append (self.deployment.present ? "be deployed" : "not be deployed");
                    }
                }
                
                bool resourcesValid = true;
                if (self.resources != null && self.resources.Count > 0)
                {
                    var resources = selfPersistent.hasInventoryResources ? selfPersistent.inventoryResources.s : null;
                    foreach (var kvp in self.resources)
                    {
                        var key = kvp.Key;
                        var amountCurrent = resources != null && resources.ContainsKey (key) ? resources[key] : 0f;
                        var amountCurrentRound = Mathf.RoundToInt (amountCurrent);
                        
                        bool passed = kvp.Value.IsPassed (true, amountCurrentRound);
                        if (!passed)
                        {
                            resourcesValid = false;
                            
                            if (log)
                            {
                                sb2.Append ($"\n- Base failed a resource check\n  - ");
                                sb2.Append (key);
                                sb2.Append (" (");
                                sb2.Append (amountCurrent.ToString ("0.##"));
                                sb2.Append ("): ");
                                sb2.Append (kvp.Value.ToString ());
                            }
                            
                            break;
                        }
                    }
                }
                
                bool pilotsAvailableValid = true;
                if (self.pilotsAvailable != null)
                {
                    var persistent = Contexts.sharedInstance.persistent;
                    var pilotCountReady = persistent.hasPlayerCombatStats ? persistent.playerCombatStats.pilotCountReady : 0;
                    pilotsAvailableValid = self.pilotsAvailable.IsPassed (true, pilotCountReady);
                    
                    if (log && !pilotsAvailableValid)
                    {
                        sb2.Append ("\n- Base failed check for available pilots (");
                        sb2.Append (pilotCountReady);
                        sb2.Append (": ");
                        sb2.Append (self.pilotsAvailable);
                    }
                }
                
                bool unitsAvailableValid = true;
                if (self.unitsAvailable != null)
                {
                    var persistent = Contexts.sharedInstance.persistent;
                    var unitCountReady = persistent.hasPlayerCombatStats ? persistent.playerCombatStats.unitCountReady : 0;
                    unitsAvailableValid = self.unitsAvailable.IsPassed (true, unitCountReady);
                    
                    if (log && !unitsAvailableValid)
                    {
                        sb2.Append ("\n- Base failed check for available units (");
                        sb2.Append (unitCountReady);
                        sb2.Append (": ");
                        sb2.Append (self.unitsAvailable);
                    }
                }
                
                bool pilotsValid = true;
                if (self.pilots != null)
                {
                    pilotsTemp.Clear ();
                    var pilotCandidates = Contexts.sharedInstance.persistent.GetEntitiesWithEntityLinkPersistentParent (selfPersistent.id.id);

                    foreach (var entity in pilotCandidates)
                    {
                        // Only grab pilots attached to the base, no point supporting unit attached pilots as they'll be deprecated soon
                        if (!entity.isPilotTag || !entity.hasPilotIdentification || !entity.hasPilotHealth)
                            continue;
                
                        // Skip dead or dying pilots
                        if (entity.isDeceased || entity.pilotHealth.f <= 0f)
                            continue;
                        
                        pilotsTemp.Add (entity);
                    }

                    bool selfPilotsPresent = pilotsTemp.Count > 0;
                    bool selfPilotsPresentMatch = selfPilotsPresent == self.pilots.present;
                    bool selfPilotsFactionMatch = true;
                
                    if (selfPilotsPresent && self.pilots.factionChecked && pilotsTemp.Count > 0)
                    {
                        foreach (var pilot in pilotsTemp)
                        {
                            if (pilot.faction.s != self.pilots.faction)
                            {
                                selfPilotsFactionMatch = false;
                                break;
                            }
                        }
                    }

                    pilotsValid = selfPilotsPresentMatch && selfPilotsFactionMatch;
                    
                    if (log && !pilotsValid)
                    {
                        sb2.Append ("\n- Base failed check for presence of pilots: ");
                        sb2.Append (self.pilots.present ? "must be present" : "must be absent");
                    
                        if (self.pilots.factionChecked)
                        {
                            sb2.Append (self.pilots.factionInverted ? ", must not have faction " : ", must have faction ");
                            sb2.Append (self.pilots.faction);
                        }
                    }
                }

                bool unitsValid = true;
                if (self.units != null)
                {
                    var units = Contexts.sharedInstance.persistent.GetEntitiesWithEntityLinkPersistentParent (selfPersistent.id.id);
                    var unitCount = 0;

                    foreach (var unitCandidate in units)
                    {
                        if (unitCandidate.isUnitTag && !unitCandidate.isDestroyed && !unitCandidate.isWrecked)
                            unitCount += 1;
                    }
                    
                    var unitsPresent = unitCount > 0;
                    unitsValid = self.units.present == unitsPresent;
                    
                    if (log && !unitsValid)
                    {
                        sb2.Append ("\n- Base failed check for presence of units: ");
                        sb2.Append (self.units.present ? "must be present" : "must be absent");
                    }
                }

                int levelPlayerBase = 1;
                if (self.levelDeltaTarget != null || self.levelDeltaProvince != null)
                {
                    var levelRange = OverworldStatResolver.ResolveUnitsLevelRange (null, selfOverworld);
                    levelPlayerBase = Mathf.RoundToInt ((levelRange.x + levelRange.y) * 0.5f);
                }
                
                bool levelDeltaTargetValid = true;
                if (self.levelDeltaTarget != null)
                {
                    if (!targetPresent)
                    {
                        levelDeltaTargetValid = false;
                        if (log)
                            sb2.Append ("\n- Base failed check for level delta with target, target wasn't found");
                    }
                    else
                    {
                        int levelTarget = targetPersistent.hasCombatUnitLevel ? targetPersistent.combatUnitLevel.i : 1;
                        int levelDelta = levelTarget - levelPlayerBase;
                        levelDeltaTargetValid = self.levelDeltaTarget.IsPassed (true, levelDelta);
                        
                        if (log && !levelDeltaTargetValid)
                        {
                            sb2.Append ("\n- Base failed check for level difference with target (");
                            sb2.Append (levelDelta);
                            sb2.Append ("): ");
                            sb2.Append (self.levelDeltaTarget);
                        }
                    }
                }
                
                bool levelDeltaProvinceValid = true;
                if (self.levelDeltaProvince != null)
                {
                    if (!provincePresentSelf)
                        levelDeltaProvinceValid = false;
                    else
                    {
                        var levelProvince = provinceOverworldSelf.provinceLevel.level;
                        int levelDelta = levelProvince - levelPlayerBase;
                        levelDeltaProvinceValid = self.levelDeltaProvince.IsPassed (true, levelDelta);
                        
                        if (log && !levelDeltaTargetValid)
                        {
                            sb2.Append ("\n- Base failed check for threat difference with target (");
                            sb2.Append (levelDelta);
                            sb2.Append ("): ");
                            sb2.Append (self.levelDeltaProvince);
                        }
                    }
                }
                
                selfValid = 
                    tagsValid &&
                    basePartsValid && 
                    memoryValid && 
                    combatReadyValid &&
                    movementValid &&
                    movementModeValid &&
                    weatherValid && 
                    deploymentValid &&
                    resourcesValid && 
                    pilotsAvailableValid && 
                    unitsAvailableValid && 
                    pilotsValid &&
                    unitsValid &&
                    levelDeltaTargetValid &&
                    levelDeltaProvinceValid;
            }
            else if (selfChecked && log)
            {
                sb2.Append ("\n- Base checks skipped");
                // sb2.Append ($"\n- Base check skipped | Present: {selfPresent} | Checked: {selfChecked} | Persistent entity: {selfPersistent.ToStringNullCheck ()} | Correct tag: {selfPersistent?.isOverworldTag} | Blueprint: {selfBlueprint.ToStringNullCheck ()}");
            }
            

            
            if (targetPresent && targetChecked && targetPersistent != null && targetPersistent.isOverworldTag && targetBlueprint != null)
            {
                bool tagsValid = true;
                if (target.tags != null && target.tags.Count > 0)
                {
                    int tagMatches = 0;
                    var tags = targetBlueprint.tagsProcessed;
                    foreach (var tagRequirement in target.tags)
                    {
                        bool required = !tagRequirement.not;
                        if (tags.Contains (tagRequirement.tag) == required)
                        {
                            tagMatches += 1;
                            if (target.tagsMethod == EntityCheckMethod.RequireOne)
                                break;
                        }
                    }
                    
                    if (target.tagsMethod == EntityCheckMethod.RequireOne)
                        tagsValid = tagMatches > 0;

                    else if (target.tagsMethod == EntityCheckMethod.RequireAll)
                        tagsValid = tagMatches == target.tags.Count;
                    
                    if (log && !tagsValid)
                    {
                        sb2.Append ("\n- Target has incompatible tags | Conditions ");
                        sb2.Append (target.tagsMethod == EntityCheckMethod.RequireOne ? "(one match required):" : "(all required):");
                        
                        foreach (var tagRequirement in target.tags)
                        {
                            bool required = !tagRequirement.not;
                            bool present = tags.Contains (tagRequirement.tag);
                            
                            if (required != present)
                            {
                                sb2.Append ("\n  - ");
                                sb2.Append (tagRequirement.tag);
                                sb2.Append (required ? " must be present" : " must be absent");
                            }
                        }
                    }
                }

                bool memoryValid = true;
                if (target.eventMemory != null)
                {
                    memoryValid = target.eventMemory.IsPassed (targetPersistent);
                    if (log && !memoryValid)
                    {
                        sb2.Append ("\n- Target has incompatible memories | ");
                        sb2.Append (target.eventMemory.ToStringWithComparison (targetPersistent));
                    }
                }
                
                bool aiValid = true;
                if (target.ai != null)
                {
                    var detectionCurrent = targetOverworld.hasPlayerDetectionStatus ? targetOverworld.playerDetectionStatus.state : PlayerDetectionStatus.DetectionState.NotDetected;
                    var stateCurrent = targetOverworld.hasAgentAIState ? targetOverworld.agentAIState.state : AgentAIState.StateDescription.Calm;
                    
                    if (target.ai.states != null)
                    {
                        foreach (var kvp in target.ai.states)
                        {
                            var state = kvp.Key;
                            bool required = kvp.Value;
                            bool match = state == stateCurrent;
                            if (match != required)
                            {
                                aiValid = false;
                                
                                if (log)
                                {
                                    sb2.Append ("\n- Target is in wrong AI state (");
                                    sb2.Append (stateCurrent);
                                    sb2.Append ("): ");
                                    sb2.Append (required ? "must be in " : "must not be in ");
                                    sb2.Append (state);
                                }
                                
                                break;
                            }
                        }
                    }
                    
                    if (aiValid && target.ai.detection != null)
                    {
                        foreach (var kvp in target.ai.detection)
                        {
                            var detection = kvp.Key;
                            bool required = kvp.Value;
                            bool match = detection == detectionCurrent;
                            if (match != required)
                            {
                                aiValid = false;
                                
                                if (log)
                                {
                                    sb2.Append ("\n- Target is in wrong AI detection stage (");
                                    sb2.Append (detectionCurrent);
                                    sb2.Append ("): ");
                                    sb2.Append (required ? "must be in " : "must not be in ");
                                    sb2.Append (detection);
                                }
                                
                                break;
                            }
                        }
                    }
                }
                
                bool resourcesValid = true;
                if (target.resources != null && target.resources.Count > 0)
                {
                    var resources = targetPersistent.hasInventoryResources ? targetPersistent.inventoryResources.s : null;
                    foreach (var kvp in target.resources)
                    {
                        var key = kvp.Key;
                        var amountCurrent = resources != null && resources.ContainsKey (key) ? resources[key] : 0f;
                        var amountCurrentRound = Mathf.RoundToInt (amountCurrent);
                        
                        bool passed = kvp.Value.IsPassed (true, amountCurrentRound);
                        if (!passed)
                        {
                            resourcesValid = false;

                            if (log)
                            {
                                sb2.Append ($"\n- Target failed a resource check\n  - ");
                                sb2.Append (key);
                                sb2.Append (" (");
                                sb2.Append (amountCurrent.ToString ("0.##"));
                                sb2.Append ("): ");
                                sb2.Append (kvp.Value.ToString ());
                            }
                            
                            break;
                        }
                    }
                }
                
                bool pilotsValid = true;
                if (target.pilots != null)
                {
                    pilotsTemp.Clear ();
                    var pilotCandidates = Contexts.sharedInstance.persistent.GetEntitiesWithEntityLinkPersistentParent (targetPersistent.id.id);

                    foreach (var entity in pilotCandidates)
                    {
                        if (entity.isPilotTag)
                            pilotsTemp.Add (entity);
                        
                        if (entity.isUnitTag)
                        {
                            var pilot = IDUtility.GetLinkedPilot (entity);
                            if (pilot != null)
                                pilotsTemp.Add (pilot);
                        }
                    }

                    bool targetPilotsPresent = pilotsTemp.Count > 0 || targetPersistent.hasVirtualPilotGroup;
                    bool targetPilotsPresentMatch = targetPilotsPresent == target.pilots.present;
                    bool targetPilotsFactionMatch = true;
                    
                    if (targetPilotsPresent && target.pilots.factionChecked)
                    {
                        if (pilotsTemp.Count > 0)
                        {
                            foreach (var pilot in pilotsTemp)
                            {
                                if (pilot.faction.s != target.pilots.faction)
                                {
                                    targetPilotsFactionMatch = false;
                                    break;
                                }
                            }
                        }

                        if (targetPersistent.hasVirtualPilotGroup)
                        {
                            var descriptions = targetPersistent.virtualPilotGroup.descriptions;
                            foreach (var description in descriptions)
                            {
                                if (description.faction != target.pilots.faction)
                                {
                                    targetPilotsFactionMatch = false;
                                    break;
                                }
                            }
                        }
                    }

                    pilotsValid = targetPilotsPresentMatch && targetPilotsFactionMatch;
                    if (log && !pilotsValid)
                    {
                        sb2.Append ("\n- Target failed check for presence of pilots: ");
                        sb2.Append (target.pilots.present ? "must be present" : "must be absent");
                    
                        if (target.pilots.factionChecked)
                        {
                            sb2.Append (target.pilots.factionInverted ? ", must not have faction " : ", must have faction ");
                            sb2.Append (target.pilots.faction);
                        }
                    }
                }

                bool unitsValid = true;
                if (target.units != null)
                {
                    bool unitsPresent = targetPersistent.hasCombatUnits && targetPersistent.hasCombatUnitLevel;
                    if (target.units.present)
                    {
                        // Since this check is generally used to ask "is there a combat capable garrison?", 
                        // I've added some flag checks to block stunned/deployed/recovering enemies from being cleared for combat
                        unitsPresent = unitsPresent && !targetOverworld.hasInteractionCooldown && !targetOverworld.hasRecoveryCooldown && !targetOverworld.isDeployed;
                        unitsValid = unitsPresent;
                    }
                    else
                    {
                        unitsValid = !unitsPresent;
                    }
                    
                    if (log && !unitsValid)
                    {
                        sb2.Append ("\n- Target failed check for garrison: ");
                        sb2.Append (target.units.present ? "must be present" : "must be absent");
                    }
                }
                
                bool factionValid = true;
                if (target.faction != null)
                {
                    var hostile = !OverworldUtility.IsFriendlyFaction (selfPersistent, targetPersistent);
                    if (!target.faction.hostileCheck)
                    {
                        factionValid = target.faction.factions != null && target.faction.factions.Contains (targetPersistent.faction.s);
                        if (log && !factionValid)
                        {
                            sb2.Append ("\n- Target has incompatible faction (");
                            sb2.Append (targetPersistent.faction.s);
                            sb2.Append ("): must be ");
                            sb2.Append (target.faction.factions.ToStringFormatted (appendBrackets: false));
                        }
                    }
                    else
                    {
                        factionValid = target.faction.hostile == hostile;
                        if (log && !factionValid)
                        {
                            sb2.Append ("\n- Target should ");
                            sb2.Append (target.faction.hostile ? "be hostile" : "be friendly");
                        }
                    }
                }

                bool resupplyPointValid = true;
                if (target.resupplyPoint != null)
                {
                    bool isResupplySite = OverworldUtility.IsResupplyAvailableAt (targetPersistent, targetOverworld);
                    bool shouldBeResupplySite = target.resupplyPoint.present;
                    resupplyPointValid = isResupplySite == shouldBeResupplySite;
                    
                    if (log && !resupplyPointValid)
                    {
                        sb2.Append ("\n- Target should ");
                        sb2.Append (target.resupplyPoint.present ? "be a resupply point" : "not be a resupply point");
                    }
                }

                targetValid =
                    tagsValid && 
                    memoryValid && 
                    aiValid && 
                    resourcesValid &&  
                    pilotsValid && 
                    unitsValid && 
                    factionValid && 
                    resupplyPointValid;
            }
            else if (targetChecked && log)
            {
                sb2.Append ("\n- Target check skipped");
                // sb2.Append ($"\n- Target check skipped, possible reasons:\n- Present: {targetPresent} | Persistent entity: {targetPersistent.ToStringNullCheck ()} | Correct tag: {targetPersistent?.isOverworldTag} | Blueprint: {targetBlueprint.ToStringNullCheck ()}");
            }
            
            

            if (provinceChecked)
            {
                var provincePosition = Vector3.zero;
                OverworldEntity provinceOverworld = null;
                string provinceKey = null;
                
                if (province.positionProvider == ProvincePositionProvider.Self)
                {
                    provinceKey = selfPresent ? DataHelperProvince.GetProvinceKeyAtEntity (selfOverworld) : DataHelperProvince.GetProvinceKeyAtPositionExpensive (provincePosition);
                    provinceOverworld = provinceKey != null ? IDUtility.GetOverworldEntity (provinceKey) : null;
                }
                else if (province.positionProvider == ProvincePositionProvider.Target)
                {
                    provinceKey = targetPresent ? DataHelperProvince.GetProvinceKeyAtEntity (targetOverworld) : DataHelperProvince.GetProvinceKeyAtPositionExpensive (provincePosition);
                    provinceOverworld = provinceKey != null ? IDUtility.GetOverworldEntity (provinceKey) : null;
                }
                
                
                var provincePersistent = provinceOverworld != null ? IDUtility.GetLinkedPersistentEntity (provinceOverworld) : null;
                var provincePresent = provinceOverworld != null && provincePersistent != null;

                if (provincePresent)
                {
                    bool provinceMemoryValid = true;
                    if (province.eventMemory != null)
                    {
                        provinceMemoryValid = province.eventMemory.IsPassed (provincePersistent);
                        if (log && !provinceMemoryValid)
                        {
                            sb2.Append ("\n- Province (");
                            sb2.Append (province.positionProvider == ProvincePositionProvider.Self ? "at base" : "at target");
                            sb2.Append (") has incompatible memories | ");
                            sb2.Append (province.eventMemory.ToStringWithComparison (provincePersistent));
                        }
                    }
                    
                    bool provinceAccessValid = true;
                    if (province.access != null)
                    {
                        var provinceFaction = provincePersistent.faction.s;
                        var provinceBlueprint = provinceOverworld.dataLinkOverworldProvince.data;
                        var provinceFriendly = OverworldUtility.IsFriendlyFaction (selfPersistent, provincePersistent);
                        bool provinceHasToBeAccessible = province.access.accessible;

                        if (!provinceFriendly)
                        {
                            // If a province isn't friendly, we need to verify whether it's accessible first
                            bool provinceAccessible = true;
                            int provinceNeighboursSupporting = 0;

                            foreach (var kvp in provinceBlueprint.neighbourData)
                            {
                                var provinceNeighbourName = kvp.Key;
                                var provinceNeighbour = IDUtility.GetOverworldEntity (provinceNeighbourName);
                                if (provinceNeighbour == null)
                                {
                                    Debug.Log ($"Failed to find a province with name {provinceNeighbourName} while checking neighbours of province {provinceKey}");
                                    continue;
                                }

                                var provinceNeighbourPersistent = IDUtility.GetLinkedPersistentEntity (provinceNeighbour);
                                bool provinceNeighbourSupport = provinceNeighbourPersistent.faction.s == provinceFaction;
                                
                                if (!provinceNeighbourSupport)
                                    continue;
                                
                                provinceNeighboursSupporting += 1;

                                // If our test mandates that a province has to be accessible, and not protected in any way,
                                // then encountering a supporting neighbour meant the check has failed
                                if (!province.access.partially)
                                {
                                    provinceAccessible = false;
                                    if (log)
                                        Debug.Log ($"As partial accessibility is not permitted by the check, this neighbour makes the province inaccessible");
                                    break;
                                }
                            }

                            // If our test mandates that a province can be partially accessible but every neighbour around it is of same faction,
                            // then this means the check has failed
                            if (province.access.partially && provinceNeighboursSupporting == provinceBlueprint.neighbourData.Count && !provinceBlueprint.alwaysVulnerable)
                            {
                                if (log)
                                    Debug.Log ($"While partial accessibility is permitted, every single neighbour is has same faction, so province can not be accessed");
                                provinceAccessible = false;
                            }

                            // Final result of a check is based on whether we actually wanted a province to be accessible or not
                            provinceAccessValid = provinceAccessible == provinceHasToBeAccessible;
                            
                            if (log && !provinceAccessValid)
                            {
                                sb2.Append ("\n- Province (");
                                sb2.Append (province.positionProvider == ProvincePositionProvider.Self ? "at base" : "at target");
                                sb2.Append (") must be");
                                sb2.Append (provinceHasToBeAccessible ? "possible to contest" : "impossible to contest");
                            }
                        }
                        else
                        {
                            // If a province is friendly, we just have to check whether we wanted access or not - we know we can access it
                            provinceAccessValid = provinceHasToBeAccessible;
                            
                            if (log && !provinceAccessValid)
                            {
                                sb2.Append ("\n- Province (");
                                sb2.Append (province.positionProvider == ProvincePositionProvider.Self ? "at base" : "at target");
                                sb2.Append (") must be");
                                sb2.Append (provinceHasToBeAccessible ? "possible to contest" : "impossible to contest");
                            }
                        }
                    }

                    bool provinceFactionValid = true;
                    if (province.faction != null)
                    {
                        if (!province.faction.hostileCheck)
                        {
                            provinceFactionValid = province.faction.factions != null && province.faction.factions.Contains (provincePersistent.faction.s);
                            
                            if (log && !provinceFactionValid)
                            {
                                sb2.Append ("\n- Province (");
                                sb2.Append (province.positionProvider == ProvincePositionProvider.Self ? "at base" : "at target");
                                sb2.Append (") has incompatible faction (");
                                sb2.Append (provincePersistent.faction.s);
                                sb2.Append ("): must be ");
                                sb2.Append (province.faction.factions.ToStringFormatted (appendBrackets: false));
                            }
                        }
                        else
                        {
                            var provinceHostile = !OverworldUtility.IsFriendlyFaction (selfPersistent, provincePersistent);
                            provinceFactionValid = province.faction.hostile == provinceHostile;
                            
                            if (log && !provinceFactionValid)
                            {
                                sb2.Append ("\n- Province (");
                                sb2.Append (province.positionProvider == ProvincePositionProvider.Self ? "at base" : "at target");
                                sb2.Append (") should ");
                                sb2.Append (province.faction.hostile ? "be hostile" : "be friendly");
                            }
                        }
                    }
                    
                    provinceValid =
                        provinceMemoryValid &&
                        provinceAccessValid &&
                        provinceFactionValid;
                }
                else if (log)
                {
                    sb2.Append ($"\n- Province ({province.positionProvider}) | Check skipped, failed to find entity | Position : {provincePosition} | Key: {provinceKey}");
                }
            }

            /*
            if (log)
            {
                sb1.Clear ();
                sb2.Clear ();
            }
            */
            
            bool actionValid = true;
            bool actionChecked = action != null;

            if (actionChecked)
            {
                bool actionListValid = true;
                if (action.actions != null && action.actions.Count > 0)
                {
                    int actionListMatches = 0;
                    foreach (var actionQuery in action.actions)
                    {
                        if (!actionQuery.tagsUsed)
                        {
                            if (string.IsNullOrEmpty (actionQuery.key))
                                continue;
                        }
                        else
                        {
                            if (actionQuery.tags == null || actionQuery.tags.Count == 0)
                                continue;
                        }

                        OverworldEntity actionOwner = null;
                        if (actionQuery.owner == OverworldEntitySource.EventSelf)
                            actionOwner = selfOverworld;
                        else if (actionQuery.owner == OverworldEntitySource.EventTarget)
                            actionOwner = targetOverworld;
                        
                        if (actionOwner == null || actionOwner.isDestroyed)
                            continue;
                        
                        var actionTargetChecked = actionQuery.actionTargetChecked;
                        OverworldEntity actionTargetComparison = null;
                        if (actionTargetChecked)
                        {
                            if (actionQuery.actionTargetComparison == OverworldEntitySource.EventSelf)
                                actionTargetComparison = selfOverworld;
                            else if (actionQuery.actionTargetComparison == OverworldEntitySource.EventTarget)
                                actionTargetComparison = targetOverworld;
                        }

                        bool actionFound = false;
                        HashSet<OverworldActionEntity> actionsFromOwner = null;
                        if (actionOwner.id.id == actionOwnerIDLast && actionsFromOwnerLast != null)
                            actionsFromOwner = actionsFromOwnerLast;
                        else
                        {
                            actionsFromOwner = Contexts.sharedInstance.overworldAction.GetEntitiesWithOverworldOwner (actionOwner.id.id);
                            actionsFromOwnerLast = actionsFromOwner;
                        }

                        foreach (var actionFromOwner in actionsFromOwner)
                        {
                            if (actionFromOwner.isDestroyed)
                                continue;
                            
                            if (!actionQuery.tagsUsed)
                            {
                                if (actionFromOwner.dataKeyOverworldAction.s != actionQuery.key)
                                    continue;
                            }
                            else
                            {
                                var actionBlueprint = actionFromOwner.dataLinkOverworldAction.data;
                                bool tagMatch = true;
                                
                                if (actionBlueprint.tags != null)
                                {
                                    foreach (var kvp in actionQuery.tags)
                                    {
                                        var tag = kvp.Key;
                                        bool required = kvp.Value;
                                        bool found = actionBlueprint.tags.Contains (tag);
                                        if (found != required)
                                        {
                                            tagMatch = false;
                                            break;
                                        }
                                    }
                                }
                                
                                if (!tagMatch)
                                    continue;
                            }

                            if (actionTargetChecked)
                            {
                                if (!actionFromOwner.hasOverworldTarget)
                                    continue;

                                var actionTarget = IDUtility.GetOverworldActionOverworldTarget (actionFromOwner);
                                if (actionTarget != actionTargetComparison)
                                    continue;
                            }

                            actionFound = true;
                            break;
                        }
                        
                        bool desired = actionQuery.actionDesired;
                        if (desired == actionFound)
                        {
                            ++actionListMatches;
                            if (action.actionMethod == EntityCheckMethod.RequireOne)
                                break;
                        }
                    }
                
                    if (action.actionMethod == EntityCheckMethod.RequireOne)
                        actionListValid = actionListMatches > 0;

                    else if (action.actionMethod == EntityCheckMethod.RequireAll)
                        actionListValid = actionListMatches == action.actions.Count;
                    
                    // Clear this cache so that we don't rely on it later when actions might've disappeared
                    actionOwnerIDLast = IDUtility.invalidID;
                    actionsFromOwnerLast = null;

                    // TODO: Extend with more details
                    if (log && !actionListValid)
                    {
                        sb2.Append ($"\n- Action checks failed (");
                        sb2.Append (action.actionMethod == EntityCheckMethod.RequireAll ? "all conditions mandatory" : "one condition sufficient");
                        sb2.Append ("): ");

                        foreach (var subcheck in action.actions)
                        {
                            sb2.Append ("\n  - ");
                            if (subcheck.tagsUsed)
                            {
                                sb2.Append ("Action with tags: ");
                                sb2.Append (subcheck.tags.ToStringFormattedKeyValuePairs ());
                            }
                            else
                            {
                                sb2.Append ("Action ");
                                sb2.Append (subcheck.key);
                            }

                            sb2.Append ("\n    - ");
                            sb2.Append (subcheck.owner == OverworldEntitySource.EventSelf ? "Owned by the base" : "Owned by the contact");
                        
                            if (subcheck.actionTargetChecked)
                            {
                                sb2.Append ("\n    - ");
                                sb2.Append (subcheck.actionTargetComparison == OverworldEntitySource.EventSelf ? "Targeting the base" : "Targeting the contact");
                            }
                        
                            sb2.Append ("\n    - ");
                            sb2.Append (subcheck.actionDesired ? "Should exist" : "Should not exist");
                        }
                    }
                }

                actionValid = actionListValid;
            }

            bool valid = selfValid && targetValid && provinceValid && actionValid;
            
            // To optimize, we never check actors unless everything above is valid
            if (valid && actors != null)
            {
                bool actorsWorldValid = true;
                if (actors.actorsWorldPresent != null && actors.actorsWorldPresent.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var ids = OverworldEventUtility.actorIDsWorld; // overworld.hasEventInProgressActorWorld ? overworld.eventInProgressActorWorld.persistentIDs : null;
                    actorsWorldValid = OverworldEventUtility.AreActorChecksPassed (actors.actorsWorldPresent, ids, sb2, "Site");
                }
                
                bool actorsUnitsValid = true;
                if (actors.actorsUnitsPresent != null && actors.actorsUnitsPresent.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var ids = OverworldEventUtility.actorIDsUnits; // overworld.hasEventInProgressActorUnits ? overworld.eventInProgressActorUnits.persistentIDs : null;
                    actorsUnitsValid = OverworldEventUtility.AreActorChecksPassed (actors.actorsUnitsPresent, ids, sb2, "Unit");
                }
                
                bool actorsPilotsValid = true;
                if (actors.actorsPilotsPresent != null && actors.actorsPilotsPresent.Count > 0)
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var ids = OverworldEventUtility.actorIDsPilots; // overworld.hasEventInProgressActorPilots ? overworld.eventInProgressActorPilots.persistentIDs : null;
                    actorsPilotsValid = OverworldEventUtility.AreActorChecksPassed (actors.actorsPilotsPresent, ids, sb2, "Pilot");
                }
                
                valid = valid && actorsWorldValid && actorsUnitsValid && actorsPilotsValid;
            }

            if (log && !valid)
            {
                sb1.Append ($"Check failed ");
                
                if (parentOption != null)
                {
                    if (parentOption.parent != null)
                        sb1.Append ($"for option {parentOption.key} in event {parentOption.parent.key}:");
                    else
                        sb1.Append ($"for option {parentOption.key} with unknown parent:");
                }
                else if (parentStep != null)
                {
                    if (parentStep.parent != null)
                        sb1.Append ($"for step {parentStep.key} in event {parentStep.parent.key}:");
                    else
                        sb1.Append ($"for step {parentStep.key} with unknown parent:");
                }
                else
                    sb1.Append ($"in unknown context");

                sb1.Append (sb2.ToString ());
                
                DebugHelper.LogWarning (sb1.ToString ());
            }

            return valid;
        }

        public string GetLog()
        {
			return sb1.ToString ();
        }

        #endif
    }
}