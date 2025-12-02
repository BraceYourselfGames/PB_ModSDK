using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Game
{
    public static class AudioEventUITitle
    {
        public const string GeneralHoverOn = "ui_title_general_hover_on";
        public const string GeneralHoverOff = "ui_title_general_hover_off";
        public const string GeneralSelect = "ui_title_general_select";

        public const string GeneralPauseOpen = "ui_general_pause_open";
        public const string GeneralPauseClose = "ui_general_pause_close";

        public const string TitleConfirmIn = "ui_title_confirm_in";
        public const string TitleConfirmOut = "ui_title_confirm_out";
        
        public const string TitleHoverMinor = "ui_title_hover_minor";
        public const string TitleSelectMinor = "ui_title_select_minor";

        public const string TitleOptionToggle = "ui_title_option_toggle";
        public const string TitleApply = "ui_title_apply";
        public const string TitleStartGame = "ui_title_startgame";
        
        //2.0
        public static string TitleAppear = "ui_title_appear";
        public static string TitleButtonAppear = "ui_title_button_appear";
        
        public static string NewGameIntroText = "ui_new_game_intro_text";
        
        public const string CreditsStart = "credits_start"; 
        public const string CreditsStop = "credits_stop"; 
    }

    public enum EventMusicMoods
    {
        None = 0,
        Restorative = 1,
        Positive = 2,
        ResearchInvestigating = 3,
        ConstructionSalvage = 4,
        Negative = 5,
        Neutral = 6,
        Warning = 7,
        Standard = 8,
        Sad = 9
    }

    public static class AudioEventOverworldNarrativeMusic
    {
        public const string MusicEventStart = "music_event_start";
        public const string MusicEventChoice = "music_event_choice";
        public const string MusicEventOutcome = "music_event_outcome";
        public const string MusicEventChoiceFinal = "music_event_choice_final";
    }

    public static class AudioEventUIBase
    {
        // all buttons in the different crawler sections, like create pilot, destroy, clone etc.
        public const string GeneralButtonHoverOn = "ui_crawler_general_button_hover_on"; 
        public const string GeneralButtonHoverOff = "ui_crawler_general_button_hover_off";
        public const string GeneralButtonSelect = "ui_crawler_general_button_select";

        // all smaller buttons like pilot names, debug toggles, inventory items, mech limbs
        public const string MinorButtonHoverOn = "ui_crawler_minor_button_hover_on";
        public const string MinorButtonHoverOff = "ui_crawler_minor_button_hover_off";
        public const string MinorButtonSelect = "ui_crawler_minor_button_select";
        public const string CrawlerTransitionToMap = "ui_crawler_transition_to_map";
        public const string CrawlerMissionBriefingOpen = "ui_crawler_mission_briefing_open";
        public const string CrawlerMissionBriefingClose = "ui_crawler_mission_briefing_close";
        
        //Rarity customizations
        public const string PartEquipCommon = "ui_customize_part_equip_common";
        public const string PartEquipUncommon = "ui_customize_part_equip_uncommon";
        public const string PartEquipRare = "ui_customize_part_equip_rare";

        // selecting a unit from the list of mechs
        public const string MechUnitSelect = "ui_crawler_mech_unit_select";
        
        // big edit mech circle button
        public const string EditMechHoverOn = "ui_crawler_edit_mech_hover_on";
        public const string EditMechHoverOff = "ui_crawler_edit_mech_hover_off";
        public const string EditMechSelect = "ui_crawler_edit_mech_select";
        public const string EditButtonBack = "ui_crawler_edit_mech_back";
        public const string ChangeBackgroundIn = "ui_crawler_change_background_in";
        public const string ChangeBackgroundOut = "ui_crawler_change_background_out";
        public const string DeployHoverOn = "ui_crawler_deploy_hover_on";
        
        // play once every time the button pulses red
        public const string DeployHoverPulse = "ui_crawler_deploy_hover_pulse";
        public const string DeployHoverOff = "ui_crawler_deploy_hover_off";
        public const string DeployConfirm = "ui_crawler_deploy_confirm";
        public const string DeployBlocked = "ui_crawler_deploy_blocked";
        
        // ambient tweaks
        public const string ViewSelectSquad = "ui_crawler_view_select_squad"; // ?
        
        public const string ViewSelectPilots = "ui_crawler_view_select_pilots";
        public const string ViewSelectUnits = "ui_crawler_view_select_units";
        public const string ViewSelectInventory = "ui_crawler_view_select_inventory";
        public const string ViewSelectWorkshop = "ui_crawler_view_select_workshop";
        public const string ViewSelectBriefing = "ui_crawler_view_select_briefing";
        public const string ViewSelectUpgrades = "ui_crawler_view_select_upgrades";
        
        // Workshop
        public const string WorkshopArmsIntro = "workshop_fabrication_arms_intro";
        public const string WorkshopArmsOutro = "workshop_fabrication_arms_outro";

        //Inventory UI
        public const string ContextScrapHoldStart = "ui_crawler_scrap_hold_start";
        public const string ContextScrapHoldFinish = "ui_crawler_scrap_hold_finish";
        
        public const string ContextScrapDetachHoldStart = "ui_crawler_scrap_detach_hold_start";
        public const string ContextScrapDetachHoldFinish = "ui_crawler_scrap_detach_hold_finish";
        
        public const string ContextDetachHoldStart = "ui_crawler_detach_hold_start";
        public const string ContextDetachHoldFinish = "ui_crawler_detach_hold_finish";
        

        
        //Pilot and Unit Equip
        public const string UnitEquipHoldStart = "ui_crawler_unit_equip_hold_start";
        public const string UnitEquipHoldFinish = "ui_crawler_unit_equip_hold_finish";
        
        public const string UnitUnequipHoldStart = "ui_crawler_unit_unequip_hold_start";
        public const string UnitUnequipHoldFinish = "ui_crawler_unit_unequip_hold_finish";

        //Workshop UI
        public const string WorkshopProjectBuildHoldStart = "ui_workshop_project_build_hold_start";
        public const string WorkshopProjectBuildHoldFinish = "ui_workshop_project_build_hold_finish";
        
        //Upgrade UI
        public const string UpgradeHoverOn = "ui_upgrade_hover_on";
        public const string UpgradeHoverOff = "ui_upgrade_hover_off";
        public const string UpgradeInstallHoldStart = "ui_upgrade_hold_start";
        public const string UpgradeInstallHoldFinish = "ui_upgrade_hold_finish";
        public const string UpgradeRemoveHoldStart = "ui_upgrade_remove_hold_start";
        public const string UpgradeRemoveHoldFinish = "ui_upgrade_remove_hold_finish";
        
        public const string UpgradeInstallWorkshop = "ui_upgrade_workshop";
        public const string UpgradeInstallReactor = "ui_upgrade_reactor";
        public const string UpgradeInstallMovement = "ui_upgrade_movement";
        public const string UpgradeInstallSensors = "ui_upgrade_sensors";
        public const string UpgradeInstallUnits = "ui_upgrade_units";
        public const string UpgradeInvalid = "ui_upgrade_invalid";
        
        public const string UpgradeStatsHoverOn = "ui_upgrade_stats_hover_on";
        public const string UpgradeStatsHoverOff = "ui_upgrade_stats_hover_off";
        public const string UpgradeStatsExpand = "ui_upgrade_stats_expand";
        public const string UpgradeStatsCollapse = "ui_upgrade_stats_collapse";
        
        public const string PilotRecruitConfirm = "ui_crawler_recruit_confirm";
        public const string PilotPrestigeConfirm = "ui_crawler_pilot_prestige_confirm";
        public const string PilotDismissConfirm = "ui_crawler_pilot_dismiss_confirm";
        
        //2.0
        public const string ScrapWorkshopConfirm = "ui_crawler_scrap_workshop_confirm";
    }
    
    public static class AudioEventUICustomization
    {
        public const string SocketPhysicalHover = "ui_customize_socket_physical_hover"; // for physical in world hover over mech sockets (not hardpoints)
        public const string SocketHoverOn = "ui_customize_socket_hover_on"; // menu hovers
        public const string SocketHoverOff = "ui_customize_socket_hover_off"; 
        public const string SocketSelect = "ui_customize_socket_select"; 
        public const string SocketBack = "ui_customize_socket_back"; 
        
        public const string HardpointHoverOn = "ui_customize_hardpoint_hover_on"; 
        public const string HardpointHoverOff = "ui_customize_hardpoint_hover_off"; 
        public const string HardpointSelect = "ui_customize_hardpoint_select"; 
        public const string HardpointBack = "ui_customize_hardpoint_back"; 
        
        public const string StatsHoverOn = "ui_customize_stats_hover_on"; 
        public const string StatsHoverOff = "ui_customize_stats_hover_off";
    }
    
    public static class AudioEventUIOverworld
    {

        public static string GetEventForTimer(float timescale)
        {
            int timeStep = Mathf.RoundToInt(timescale);

            switch (timeStep)
            {
                case 0:
                    return UIMapPause;
                case 1:
                    return UIMapGameSpeed1x;
                case 2:
                    return UIMapGameSpeed2x;
                case 3:
                    return UIMapGameSpeed3x;
            }

            return UIMapPause;
        }

        //Roster, resupply, repair
        public static string UIMapRepairPause = "ui_map_repair_pause";
        public static string UIMapRepairResume = "ui_map_repair_resume";
        public static string UIMapRepairComplete = "ui_map_repair_complete";
        public static string UIMapRepairEmpty = "ui_map_repair_empty";
        
        public static string UIMapResupplyOptionsAppear = "ui_map_resupply_options_appear";
        public static string UIMapResupplyOptionsDisappear = "ui_map_resupply_options_disappear";
        public static string UIMapResupplyOptionsConfirm = "ui_map_resupply_confirm";
        
        public static string UIMapEscalationFriendly = "ui_map_escalation_friendly";
        public static string UIMapEscalationHostileLevel0 = "ui_map_escalation_hostile_level_0";
        public static string UIMapEscalationHostileLevel1 = "ui_map_escalation_hostile_level_1";
        public static string UIMapEscalationHostileLevel2 = "ui_map_escalation_hostile_level_2";
        public static string UIMapEscalationHostileLevel3 = "ui_map_escalation_hostile_level_3";
        
        public static string UIMapContestButtonPopup = "ui_map_contest_button_popup";
        public static string UIMapContestButtonPress = "ui_map_contest_button_press";
        public static string UIMapContestButtonSitePopup = "ui_map_contest_battle_site_popup";
        public static string UIMapContestButtonSiteTimeout = "ui_map_contest_battle_site_timeout";
        
        public static string UIMapContestObjectivePopup = "ui_map_contest_objective_popup";
        public static string UIMapContestWarWin = "ui_map_contest_war_win";
        public static string UIMapContestWarLose = "ui_map_contest_war_lose";
        
        public static string UIMapContestRecaptureStart = "ui_map_contest_recapture_start";
        public static string UIMapContestSiteRecaptureStart = "ui_map_contest_site_recapture_start";
        public static string UIMapContestSiteRecaptureFinish = "ui_map_contest_site_recapture_finish";

        public static string UIMapActionComplete = "ui_map_action_complete";
        
        //Top Time Gadget
        public static string UIMapGameSpeed1x = "ui_map_toggle_speed_1x";
        public static string UIMapGameSpeed2x = "ui_map_toggle_speed_2x";
        public static string UIMapGameSpeed3x = "ui_map_toggle_speed_3x";
        public static string UIMapPause = "ui_map_pause";
        
        //World Selection of crawler
        public static string UIMapCrawlerHoverOn = "ui_map_crawler_hover_on";
        public static string UIMapCrawlerHoverOff = "ui_map_crawler_hover_off";
        public static string UIMapCrawlerSelect = "ui_map_crawler_select";
        public static string UIMapCrawlerDeselect = "ui_map_crawler_deselect";
        public static string UIMapCrawlerMoveConfirm = "ui_map_crawler_move_confirm";
        
        //Sensor pings
        public static string UIMapSensorUnidentified = "ui_map_sensor_unidentified";
        public static string UIMapSensorIdentified = "ui_map_sensor_unidentified";
        public static string UIMapSensorInRange = "ui_map_sensor_in_range";
        public const string UIMapScannerSweep = "ui_map_scanner_sweep";
        
        //AI states
        public const string UIMapDetectionInvestigating = "ui_map_detection_investigating";
        public const string UIMapDetectionPassiveSite = "ui_map_detection_alerted_non_aggressive_site";
        public const string UIMapDetectionPassiveSquad = "ui_map_detection_alerted_non_aggressive";
        public const string UIMapDetectionAggressive = "ui_map_detection_alerted_aggressive";
        public const string UIMapDetectionBoss = "ui_map_detection_alerted_boss";
        
        //On hover of entities
        public static string UIMapUnitHoverOn = "ui_map_unit_hover_on";
        public static string UIMapUnitHoverOff = "ui_map_unit_hover_off";
        public static string UIMapUnitSelect = "ui_map_unit_select";
        public static string UIMapUnitDeselect = "ui_map_unit_deselect";
        
        //Territory UI
        public static string MapTransitionToCrawler = "ui_map_transition_to_crawler";
        public static string MapTerritoryHoverOn = "ui_map_territory_hover_on";
        public static string MapTerritoryHoverOff = "ui_map_territory_hover_off";
        
        //General Event UI
        public static string MapEventWindowOpen = "ui_map_event_window_open";
        public static string MapEventWindowClose = "ui_map_event_window_close";
        public static string MapEventOptionSelect = "ui_map_event_option_select";
        public const string UIMapEventOptionHoverOn = "ui_map_event_option_hover_on";
        public const string UIMapEventOptionHoverOff = "ui_map_event_option_hover_off";
        
        //Specific Events
        public const string UIMapSmokeScreenUse = "ui_map_smoke_screen_use";
        public const string UIMapSuitUp = "ui_map_suit_up";
        
        //Movement
        public const string UIMovementModeNormal = "ui_map_movement_mode_normal";
        public const string UIMovementModeStealth = "ui_map_movement_mode_stealth";
        public const string UIMovementModeOverdrive = "ui_map_movement_mode_overdrive";
        public const string UIMovementStop = "ui_map_movement_mode_stop";
        public const string UIMovementPowerDepleted = "ui_map_movement_mode_power_depleted";
        public const string UIMovementPowerFull = "ui_map_movement_mode_power_full";

        public const string UIMapGeneralHoverOn = "ui_map_general_hover_on";
        public const string UIMapGeneralHoverOff = "ui_map_general_hover_off";

        //Roster
        public const string CampPrepareToggleOn = "ui_map_camp_prepare_toggle_on";
        public const string CampPrepareToggleOff = "ui_map_camp_prepare_toggle_off";
        public const string CampConfirm = "ui_map_camp_confirm";
        public const string OverdriveDamage = "ui_map_overdrive_damage";
        
        public const string CampAmbienceStart = "ambience_camping_start";
        public const string CampAmbienceStop = "ambience_camping_stop";
        
        //Region
        public const string CurrentRegionAppear = "ui_map_current_region_appear";
        
        // 2.0
        public static string CampEffectNeutral = "ui_camp_effect_neutral"; // missing
        public static string CampEffectRoll = "ui_camp_effect_roll"; // missing
        public static string CampEffectNegative = "ui_camp_effect_negative"; // missing
        public static string CampEffectPositive = "ui_camp_effect_positive"; // missing
        public static string CampEffectFinal = "ui_camp_effect_final"; // missing
        
        public static string MapAreaHoverOn = "ui_worldmap_area_hover_on"; // missing
        public static string MapAreaHoverOff = "ui_worldmap_area_hover_off"; // missing
        public static string MapAreaSelect = "ui_worldmap_area_select"; // missing
        public static string MapTravelConfirm = "ui_map_travel_confirm"; // missing
        public static string MapTravelContinue = "ui_map_travel_continue"; // missing
        
        public static string MapEnemyRegroupHoverOn = "ui_map_enemyRegroup_hover_on";
        public static string MapEnemyRegroupHoverOff = "ui_map_enemyRegroup_hover_off";
        
        public static string CrawlerTravelLoopStart = "crawler_travel_loop_start"; // missing
        public static string CrawlerTravelLoopStop = "crawler_travel_loop_stop"; // missing

        public static string CodexEntryUnlocked = "ui_map_codex_unlocked"; // "codex_entry_unlocked"; // missing
        public static string CodexEntryRead = "ui_map_codex_read"; // "codex_entry_read"; // missing
    }

    public static class AudioEventUISalvage
    {
        public const string UISalvageChoiceRecover = "ui_salvage_choice_recover";
        public const string UISalvageChoiceDismantle = "ui_salvage_choice_dismantle";
        public const string UISalvageChoiceSkip = "ui_salvage_choice_skip";

        public const string UISalvageChangeUnit = "ui_salvage_change_unit";

        public const string UISalvageCostIncrease = "ui_salvage_cost_increase";
        public const string UISalvageCostDecrease = "ui_salvage_cost_decrease";
        public const string UISalvageCostOverBudgetIncrease = "ui_salvage_cost_over_budget_increase";
        public const string UISalvageCostOverBudgetDecrease = "ui_salvage_cost_over_budget_decrease";
        
        public const string UISalvageCostOverBudget = "ui_salvage_budget_max";
        public const string UISalvageCostUnderBudget = "ui_salvage_budget_max_return";

        public const string UISalvageSelectWhite = "ui_salvage_select_white";
        public const string UISalvageSelectGreen = "ui_salvage_select_green";
        public const string UISalvageSelectBlue = "ui_salvage_select_blue";
        public const string UISalvageSelectOrange = "ui_salvage_select_orange";

        public const string UISalvageConfirm = "ui_salvage_confirm";
        public const string UISalvageEntry = "ui_salvage_entry";
        public const string UISalvageExit = "ui_salvage_exit";
    }
    
    public static class AudioEventUICombat
    {
        public const string UIReplayHoverOn = "ui_combat_replay_button_hover_on";
        public const string UIReplayHoverOff = "ui_combat_replay_button_hover_off";
        public const string UIReplayStart = "ui_combat_replay_start";
        public const string UIReplayStop = "ui_combat_replay_stop"; 
        
        public const string PredictionActivate = "ui_combat_prediction_activate";
        public const string PredictionHoverOn = "ui_start_locked_hover_on";
        public const string PredictionHoverOff = "ui_start_locked_hover_off";
        
        public const string InvadedIntro = "ui_invaded";
        
        public static string UICombatDebriefingVictory = "ui_combat_debriefing_victory";
        
        public const string DebriefingResultIn = "ui_combat_debriefing_result_in";
        public const string DebriefingResultCount = "ui_combat_debriefing_result_count";
        
        public const string DebriefingUnitsDefeatedStart = "ui_combat_debriefing_units_defeated_start";
        public const string DebriefingUnitsDefeatedStop = "ui_combat_debriefing_units_defeated_stop";
        
        public const string Reward1 = "ui_combat_reward_1";
        public const string Reward2 = "ui_combat_reward_2";
        public const string Reward3 = "ui_combat_reward_3";
        public const string Reward4 = "ui_combat_reward_4";
    }

    public static class AudioEventGeneral
    {
        public const string StopAll = "stop_all";
        public const string TableauStart = "tableau_start";
        public const string StopMainMenuAudio = "stop_main_menu_audio";
        
        public const string TitleRoadmapOpen = "ui_title_roadmap_open";
        public const string TitleRoadmapClose = "ui_title_roadmap_close";
        
        public const string TitleBootTransitionStart = "ui_title_boot_flow_transition";
        public const string TitleBootTransitionMid = "ui_title_boot_flow_arrows_appear";
        
        public const string SeedRandomToggleOn = "ui_title_seed_random_toggle_on";
        public const string SeedRandomToggleOff = "ui_title_seed_random_toggle_off";
        public const string SeedGenerate = "ui_title_seed_generate";
        
        public const string GeneralTextInput = "ui_general_text_input"; 
        public const string GeneralTextDelete = "ui_general_text_delete"; 
    }
    
    public static class AudioEventMusic
    {
        // Move this to configs later
        public const string World = "music_worldmap";
        public const string Customization = "music_worldmap_customize"; // Triggered automatically
        public const string Dynamic01 = "music_combat_cinematic_01";
        public const string MusicCombatLinear = "music_combat_linear";
        public const string MainMenu = "music_menu";
        
        public const string MusicOverworldStop = "music_overworld_stop";
    }

    public static class AudioSyncMusic
    {
        public const string CombatMusicExecutionState = "music_combat_linear_state";
        public const string CombatMusicLinearIntensity = "music_combat_linear_intensity";
        public const string GameOver = "gameover";
        
        //salvage music
        public const string MusicWorldmapSalvage = "music_worldmap_salvage";
        public const string MusicWorldmapSalvageMood = "music_worldmap_salvage_mood";
        public const string MusicWorldmapSalvageState = "music_worldmap_salvage_state";
    }

    public static class AudioStateGroupMusic
    {
        public const string CombatDynamic = "music_cinematic_state";

        public const string General = "general";
    }
    
    public static class AudioStateMusic
    {
        //Option State
        public const string Paused = "paused";
        public const string UnPaused = "unpaused";

        //State Group Update
        public const string CombatMusicLinearState = "music_linear_state";

        //Individual States
        public const string CombatStateDire = "dire";
        public const string CombatStateNegative = "negative";
        public const string CombatStateNeutral = "neutral";
        public const string CombatStatePositive = "positive";

        public const string CombatDynamicPrefixOptimism = "optimism";
        public const string CombatDynamicPrefixUncertainty = "uncertainty";
        public const string CombatDynamicPrefixFear = "fear";
        
        public const string CombatDynamicStart = "_start";
        public const string CombatDynamicExecution = "_cine";
        public const string CombatDynamicPlanning = "_turns";
    }

    public static class AudioEventAmbientGeneral
    {
        public const string Base = "ambience_base"; 
        public const string Crawler = "ambience_crawler"; 
        public const string Hangar = "ambience_hanger"; 
        public const string SituationRoom = "ambience_situationroom"; 
        public const string World = "ambience_worldmap";
        public const string Stop = "stop_ambience";
        public const string StopCrawler = "stop_crawler_audio";

        public const string MainMenu = "ambience_title";

        public const string TransitionBaseShort = "transition_base_short"; 
        public const string TransitionBaseSkip = "transition_base_skip"; 
        public const string TransitionBaseStart = "transition_base_start"; 
        public const string TransitionBaseStop = "transition_base_stop";
    }
    
    public static class AudioEventAmbientCombat
    {
        public const string City = "ambience_city"; 
        public const string Settlement = "ambience_settlement";
        public const string Forest = "ambience_forest";
        public const string Bunker = "ambience_bunker";

        public const string Smoke = "camera_movement";
        public const string SmokeStop = "camera_movement_stop";
        
        public const string Rain = "ambience_rain";
        public const string RainStop = "ambience_rain_stop";
        
        public const string RainMetal = "ambience_rain_metal";
        public const string RainMetalStop = "ambience_rain_metal_stop";
    }
    
    
    
    
    public static class MusicStates
    {
        public const string CinematicState = "music_cinematic_state";

        public const string BaseStart = "base";

        public const string UncertainStart = "uncertainty_start";
        public const string UncertainTurn = "uncertainty_turns";
        public const string UncertainCine = "uncertainty_cine";
        public const string UncertainExit = "uncertainty_exit";

        public const string FearStart = "fear_start";
        public const string FearTurn = "fear_turns";
        public const string FearCine = "fear_cine";
        public const string FearExit = "fear_exit";

        public const string OptimismCine = "optimism_cine";
        public const string OptimismTurn = "optimism_turns";

        public const string StingWin = "sting_win";
        public const string TutorialCutscene01 = "tutorial_cutscene_01";
        public const string TutorialCutscene02 = "tutorial_cutscene_02";
        public const string TutorialCutscene03 = "tutorial_cutscene_03";
    }

    public static class AudioSyncs
    {
        public const string TimeScale = "time_scale";
        public const string TimeScrub = "time_scrub";
        public const string CameraZoom = "camera_zoom";
        public const string CameraSpeed = "camera_speed";
        
        public const string VolumeMain = "volume_main";
        public const string VolumeSFX = "volume_sfx";
        public const string VolumeVO = "volume_vo";
        public const string VolumeMusic = "volume_music";
        
        public const string ImpactPower = "impact_power";
        public const string Velocity = "velocity";

        public const string MusicTimeOfDay = "time_of_day";
        public const string MusicState = "state";

        public const string Weight = "weight";

        public const string Hope = "hope";
        public const string RegionFriendly = "region_friendly";
        public const string OverheatWarning = "warning_overheat";

        public const string WorldmapCrawlerSpeed = "worldmap_crawler_speed";
        
        public const string MusicWorldMapWar = "music_worldmap_war";
        public const string MusicWorldMapDanger = "music_worldmap_danger";
        public const string MusicCombatCinematicIntensity = "music_combat_cinematic_intensity";

        public const string BriefingRoom = "briefingroom";
        public const string BriefingRoomActive = "briefingroomactive";

        public const string TankElevation = "tank_elevation";

        public const string SmokeStrength = "smoke_strength";
        public const string RainStrength = "rain_strength";

        public const string AudioMono = "audio_mono";
        public const string AudioCompressed = "audio_compressed";
        
        public const string MovementModeNormal = "movement_mode_normal";
        public const string MovementModeStealth = "movement_mode_stealth";
        public const string MovementModeOverdrive = "movement_mode_overdrive";
        public const string MovementModeStop = "movement_mode_stop";

        public const string ReplayEntered = "replay";
        public const string ReplayTime = "replay_time";

        public const string WarStrength = "war_strength";
    }
    
    public static class AudioSyncsLinked
    {
        public const string Velocity = "velocity";
        public const string AngularVelocity = "angular_velocity";
        
        public const string WeaponMissileSeekerFuel = "missile_fuel";
        public const string WeaponMissileSeekerAngularVelocity = "missile_angular_velocity";
        public const string WeaponMissileSeekerTargetProximity = "missile_proximity_to_unit";
        public const string WeaponMissileSeekerSize = "missile_size";

        public const string TankWeapon = "tank_weapon";
        public const string TankElevation = "tank_elevation";
    }

    public static class AudioEventsLinked
    {
        public const string TankMovement = "tank_movement";
        public const string TankMovementStop = "tank_movement_stop";

        public const string TankElevatedMovement = "tank_elevated_movement";
        public const string TankElevatedMovementStop = "tank_elevated_movement_stop";
        
        public const string MechSliding = "mech_slide";
        public const string MechSlidingStop = "mech_slide_stop";
        
        public const string AirdropMech = "mech_spawn_airdrop";
        public const string AirdropTank = "tank_spawn_airdrop";
    }

    public static class AudioCombatMusicState
    {
        public const string Boss = "boss";
        public const string BossBig = "boss_big";
        public const string Waves = "waves";
        
        public const string Dire = "dire";
        public const string Negative = "negative";
        public const string Neutral = "neutral";
        public const string Positive = "positive";
        public const string Chill = "chill";
    }

    public static class AudioEvents
    {
        //Overworld events
        public const string WorldmapCrawlerStart = "worldmap_crawler_start";
        public const string WorldmapCrawlerStop = "worldmap_crawler_stop";
        
        public const string WorldmapMechsStart = "worldmap_mechs_start";
        public const string WorldmapMechsStop = "worldmap_mechs_stop";
        
        public const string WorldmapCrawlerDeployStart = "worldmap_crawler_deploy_start";
        public const string WorldmapCrawlerDeployStop = "worldmap_crawler_deploy_stop";

        //New Weapon events -- Class one, single activations
        public const string WeaponHandgunHeavy = "weapon_handgun_heavy";
        public const string WeaponHandgunSemiAuto = "weapon_handgun_semiauto";
        public const string WeaponHandgunAutomatic = "weapon_handgun_automatic";

        public const string WeaponMachinegunMedium = "weapon_machinegun_medium";
        
        public const string WeaponShotgunAutomatic = "weapon_shotgun_automatic";
        public const string WeaponShotgunHeavy = "weapon_shotgun_heavy";
        public const string WeaponShotgunSemiAuto = "weapon_shotgun_semiauto";

        public const string WeaponRifleSniper = "weapon_rifle_sniper";
        public const string WeaponRifleMarksman = "weapon_rifle_marksman";
        public const string WeaponTankCannon = "weapon_tank_cannon";

        public const string WeaponEnergyShotgunDragon = "weapon_energy_shotgun_dragon";

        public const string WeaponEnergyShotgunMisfire = "weapon_energy_shotgun_misfire";

        public const string WeaponEnergyMarksmanThermite = "weapon_energy_marksman_thermite";

        public const string WeaponEnergyCannonCharge = "weapon_energy_cannon_charge";
        
        public const string WeaponEnergySniperCurve = "weapon_energy_sniper_curve";
        public const string WeaponEnergySniperCurveLoop = "weapon_energy_sniper_curve_loop";
        public const string WeaponEnergySniperCurveLoopStop = "weapon_energy_sniper_curve_loop_stop";

        
        //New Weapon events -- Class two, high rate of fire weapons
        public const string WeaponMachinegunSubFirst = "weapon_machinegun_sub_first";
        public const string WeaponMachinegunSubMid = "weapon_machinegun_sub_mid";
        public const string WeaponMachinegunSubMidFast = "weapon_machinegun_sub_mid_fast";
        public const string WeaponMachinegunSubLast = "weapon_machinegun_sub_last";

        public const string WeaponRifleAssaultFirst = "weapon_rifle_assault_first";
        public const string WeaponRifleAssaultMid = "weapon_rifle_assault_mid";
        public const string WeaponRifleAssaultLast = "weapon_rifle_assault_last";

        public const string WeaponTankAAFirst = "weapon_tank_aa_first";
        public const string WeaponTankAAMid = "weapon_tank_aa_mid";
        public const string WeaponTankAALast = "weapon_tank_aa_last";

        public const string WeaponEnergyAssaultPulseFirst = "weapon_energy_assault_pulse_first";
        public const string WeaponEnergyAssaultPulseMid = "weapon_energy_assault_pulse_mid";
        public const string WeaponEnergyAssaultPulseLast = "weapon_energy_assault_pulse_last";

        //Stopping all weapon audio on crashes and similar cases
        public const string WeaponLoopStop = "weapon_loop_stop";

        //New Weapon events -- Class three, Looping weapons
        public const string WeaponMachinegunHeavyStart = "weapon_machinegun_heavy";
        public const string WeaponMachinegunHeavyStop = "weapon_machinegun_heavy_stop";
        public const string WeaponChaingunHeavyStart = "weapon_chaingun_heavy";
        public const string WeaponChaingunHeavyStop = "weapon_chaingun_heavy_stop";

        public const string WeaponEnergyBeamStart = "weapon_energy_beam";
        public const string WeaponEnergyBeamStartStop = "weapon_energy_beam_stop";

        public const string WeaponEnergyBeamUltraHeavyStart = "weapon_energy_beam_ultra_heavy";
        public const string WeaponEnergyBeamUltraHeavyStartStop = "weapon_energy_beam_ultra_heavy_stop";
        
        //New Weapon events -- Class four, Looping missiles
        public const string WeaponMissileSeekerFire = "weapon_missile_seeker_fire";
        public const string WeaponMissileSeekerFireShort = "weapon_missile_seeker_fire_short";
        public const string WeaponMissileSeekerPrime = "weapon_missile_seeker_prime";
        public const string WeaponMissileSeekerExplosion = "weapon_missile_seeker_explode";
        
        //Audio Animation Events
        public const string MechConcussed ="mech_move_concussed";
        public const string MechOverheat = "mech_move_overheat";
        public const string MechSliding = "mech_move_sliding";
        public const string MechStopToMove = "mech_move_stoptomove";
        public const string MechRunToSlide = "mech_move_runtoslide";
        public const string MechSlideToRun = "mech_move_slidetorun";
        public const string MechSlideToStop = "mech_move_slidetostop";
        public const string MechWalkJogRunStop = "mech_move_walkjogruntostop";
        public const string MechSuddenStop = "mech_move_suddenstop";
        public const string MechJumpUp = "mech_move_jumpup";
        public const string MechJumpDown = "mech_move_jumpdown";
        public const string MechJumpOverObstacle = "mech_move_jumpoverobstacle";
        public const string MechJumpOverDitch = "mech_move_jumpoverditch";
        public const string MechGetUpKnee = "mech_move_getupknee";
        public const string MechGetUpProneSupine = "mech_move_getuppronesupine";
        public const string MechDash = "mech_move_dash";
        public const string MechDashEnd = "mech_move_dash_end";
        public const string MechMelee = "mech_move_melee";
        public const string MechStorageOutOf = "mech_move_storage_out_of";
        public const string MechStorageInto = "mech_move_storage_into";
        public const string MechSpawnAirDrop = "mech_spawn_airdrop";
        public const string MechUseEquipmentIntro = "mech_use_equipment_intro";
        public const string MechUseEquipmentOutro = "mech_use_equipment_outro";
        public const string MechSlidingStop = "mech_move_sliding_stop";
        public const string FabricatorArmIntro = "workshop_fabrication_arms_intro";
        public const string FabricatorArmLoop = "workshop_fabrication_arms_loop_os";
        public const string FabricatorArmOutro = "workshop_fabrication_arms_outro";
        public const string MechStopToSprint = "mech_move_stoptosprint";
        public const string MechSprintToStop = "mech_move_sprinttostop";
        public const string MechMeleeStrike = "mech_move_melee_strike";
        public const string MechOverheatStart = "mech_overheat_start";
        public const string MechOverheatLoop = "mech_overheat_loop";
        public const string MechOverheatStop = "mech_overheat_stop"; 
        
        //Boss events
        public const string BossSpiderStep = "boss_spider_step";
        public const string BossSpiderPhaseChange1 = "boss_spider_phase_change_1";
        public const string BossSpiderPhaseChange2 = "boss_spider_phase_change_2";
        public const string BossSpiderWeaponLauncher = "boss_spider_weapon_launcher_fire";
        public const string BossSpiderWeaponCannon = "boss_spider_weapon_main_cannon";
        public const string BossSpiderWeaponBeam = "boss_spider_weapon_beam";
        public const string BossSpiderWeaponBeamEnd = "boss_spider_weapon_beam_end";
        public const string BossTargetingAlert = "ui_boss_target_unit";
        public const string BossTargetingAlertHover = "ui_boss_target_unit_hover";
        
        //Status effects
        public const string StatusActiveDefenseShot = "weapon_active_defense_shot";
        public const string StatusBurningLoopStart = "status_burning_loop";
        public const string StatusBurningLoopStop = "status_burning_loop_stop";
        public const string StatusBurningWaterCool = "status_burning_water_cool";
        public const string StatusVentingLoopStart = "status_venting_loop";
        public const string StatusVentingLoopStop = "status_venting_loop_stop";
        public const string StatusVentingIgnition = "status_venting_burning";
        public const string StatusChargedLoopStart = "status_charged_loop";
        public const string StatusChargedLoopStop = "status_charged_loop_stop";
        public const string StatusChargedDischarge = "status_charged_discharge";
        public const string StatusMeltdownLoopStart = "status_meltdown_loop";
        public const string StatusMeltdownLoopStop = "status_meltdown_loop_stop";
        public const string StatusStabilizeSuccess = "status_stabilize";
        public const string LightningStrike = "lightning_strike";

        //Tank weapon rotate 
        public const string TankWeaponRotateLoopStart = "tank_weapon_rotate_loop_start";
        public const string TankWeaponRotateLoopStop = "tank_weapon_rotate_loop_stop";
        
        //Damage, impacts
        public const string ImpactCrit = "impact_crit";
        
        public const string ImpactBuilding = "impact_bullet_rock";
        public const string ImpactMechBulletSmall = "impact_bullet_mech_small";
        public const string ImpactMechBulletLarge = "impact_bullet_mech_large";
        public const string ImpactMetal = "impact_metal";
        public const string ImpactShield = "impact_bullet_shield";
        public const string Ricochet = "ricochet";

        public const string ImpactMechCrash = "impact_mech_crash";
        public const string ImpactMechGround = "impact_mech_ground";
        public const string ImpactMechMelee = "impact_mech_melee";

        public const string BuildingCrumble = "building_crumble";
        public const string BuildingExplosionLarge = "building_explosion_large";

        public const string ExplosionPart = "explosion_part";
        public const string ExplosionUnit = "explosion_unit";
        
        public const string MechFootfall = "mech_foot";
        public const string MechFootfallWater = "mech_foot_water";
        public const string MechWaterEntry = "mech_step_water_in";
        public const string MechWaterExit = "mech_step_water_out";
        
        public const string MechEjection = "mech_eject";

        public const string CutsceneSFXIntro = "cut_tutorial_intro";
        public const string CutsceneSFXMid = "cut_tutorial_mid";
        public const string CutsceneSFXEnd = "cut_tutorial_end";
        public const string DialogSkip = "dialog_skip";
        public const string CutsceneSkip = "cutscene_skip";
        
        public const string ExplosionUnitTank = "tank_destruction_full";
        public const string ExplosionUnitMech = "mech_destruction_full";
        
        public const string ExplosionEnergyGeneral = "weapon_energy_explosion_gen";

        //UI
        public const string TimeScrub = "time_scrub";
        public const string UISelect = "ui_select";
        public const string UISelectAction = "ui_select_action";
        public const string UIStart = "ui_start";
        
        public const string UIStartHoverOff = "ui_start_hover_off";
        public const string UIStartHoverOn = "ui_start_hover_on";

        public const string UITargetSwitched = "ui_target_switched";
        public const string UITargetMode = "ui_target_mode";
        public const string UIEventLog = "ui_action_log";

        public const string UICommOpen = "ui_comm_open";
        public const string UICommClose = "ui_comm_close";

        public const string UICancel = "ui_cancel";
        
        public const string UITimeSlowdown = "ui_time_slowdown";
                
        public const string UIUnitActionHoverOff = "ui_unit_action_hover_off";
        public const string UIUnitActionHoverOn = "ui_unit_action_hover_on";

        public const string UIUnitHoverOn = "ui_unit_hover_on";
        public const string UIUnitHoverOff = "ui_unit_hover_off";

        public const string UIUnitDeselect = "ui_unit_deselect";
        public const string UIUnitSelect = "ui_unit_select";
        public const string UIUnitSelectAction = "ui_unit_select_action";
        
        public const string UIExecuteLockedHoverOn = "ui_start_locked_hover_on";
        public const string UIExecuteLockedHoverOff = "ui_start_locked_hover_off";
            
        public const string UIActionConfirmed = "ui_unit_action_confirmed";
        public const string UIPredictionActivate = "ui_prediction_activate";
            
        public const string UIObjectiveFulfilled = "ui_unit_action_objective_fulfilled";
        public const string UIObjectiveHint2D = "ui_unit_action_objective_invalid_2d";
        public const string UIObjectiveHint3D = "ui_unit_action_objective_invalid_3d";
        public const string UIObjectiveInvalidated = "ui_unit_action_objective_invalidated";
        public const string UIObjectiveStepFinished = "ui_unit_action_objective_fulfilled_all";
        
        public const string UIHitPipEnemy = "ui_hit_pip_enemy";
        public const string UIHitPipFriendly = "ui_hit_pip_friendly";

        public const string UIOverheatLoopStart = "ui_warning_overheat_loop_start";
        public const string UIOverheatLoopEnd = "ui_warning_overheat_loop_end";

        public const string UICombatWin = "music_combat_sting_linear_win";
        public const string UICombatLose = "music_combat_sting_linear_lose";
        public const string UICombatWinRetreated = "music_combat_sting_linear_retreat_objsuccess";
        public const string UICombatLoseRetreated = "music_combat_sting_linear_retreat_objfailed";

        public const string UIOverworldActionCancelled = "ui_cancel";
        public const string UIOverworldActionHoverOn = "ui_unit_action_hover_on";
        public const string UIOverworldActionHoverOff = "ui_unit_action_hover_off";

        public const string UITitleLoadGame = "ui_title_loadgame";
        
        public const string UICustomizeLiveryPaint = "ui_customize_livery_paint";
        public const string UICustomizeLiveryPaintRemove = "ui_customize_livery_paint_remove";
        public const string UICustomizePartEquip = "ui_customize_part_equip";
        public const string UICustomizePartUnequip = "ui_customize_part_unequip";

        public const string UINewWave = "ui_new_wave";
        
        public const string UICommsSkipVO = "vo_skip";
        public const string UICommsOpen = "ui_comms_open";
        public const string UICommsTextLoopStart = "ui_comms_text_loop_start";
        public const string UICommsTextLoopStop = "ui_comms_text_loop_stop";
        public const string UICommsClose = "ui_comms_close";

        public const string UIOrderAvailable = "ui_order_available";

        //Prop destruction
        public const string PropDestroyCar = "destructible_car"; //any vehicles
        public const string PropDestroyVehicleLarge = "destructible_vehicle_large";
        public const string PropDestroyElectric = "destructible_electric"; //not sure if theres anything for this yet, but was intended for power poles
        public const string PropDestroyWood = "destructible_wood"; //also not sure if this is needed, was intended for wooden piles, but can also be used for fences
        public const string PropDestroyTree = "destructible_tree";
        public const string PropDestroyPlastic = "destructible_plastic";
        public const string PropDestroyMixed = "destructible_mixed";
        public const string PropDestroyGenericSmall = "destructible_generic_small";
        public const string PropDestroyGenericVegetation = "destructible_generic_vegetation";
        public const string PropDestroyMetalSmall = "destructible_metal_small";

        //Prop drone
        public const string PropDroneFlyIn = "prop_drone_fly_in";
        public const string PropDroneFlyOut = "prop_drone_fly_out";
        
        //2.0
        public static string JetFlyby = "jet_flyby"; // missing
        
        public static string WeaponMissileCruiseStart = "weapon_missile_cruise_start";
        public static string WeaponMissileCruiseStop = "weapon_missile_cruise_stop";
        public static string WeaponMissileCruiseExplode = "weapon_missile_cruise_explode";
        
        public static string UICombatIntro1 = "ui_combat_intro_1";
        public static string UICombatIntro2 = "ui_combat_intro_2";
        public static string UICombatIntro3 = "ui_combat_intro_3";
        public static string UICombatIntroStop = "ui_combat_intro_stop";
        
        public static string BossBoatDroneLaunch = "boss_boat_drone_launch";
        public static string CutsceneBaseLaserFire = "cutscene_base_laser_fire";
        
        public static string UICombatCapacitorChargeLoss = "ui_combat_capacitor_charge_lose";
        public static string UICombatCapacitorChargeGain = "ui_combat_capacitor_charge_gain";
        public static string UICombatCapacitorChargeFull = "ui_combat_capacitor_charge_full";
        
        public static string SatDoorExplosion = "door_satellite_explosion";
        public static string SatAntennaDestruction = "satellite_destroyed";
        
        public static string TrainCombatMovementLoopStart = "train_combat_movement_loop";
        public static string TrainCombatMovementLoopStop = "train_combat_movement_loop_stop";
        public static string TrainCombatEngineExplode = "train_combat_engine_explode";
        
        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null)
            {
	            keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (AudioEvents));
                keys.Add("");
            }
            return keys;
        }
    }
}
