using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class WeatherLevel
    {
        [PropertyRange (0f, 1f)]
        public float threshold = 0f;
        
        [PropertyRange (0f, 1f)]
        public float visionMultiplierEnemy = 1f;
        
        [PropertyRange (0f, 1f)]
        public float visionMultiplierPlayer = 1f;

        [ShowInInspector, HideLabel, YamlIgnore, ReadOnly]
        private string textName => Txt.Get (TextLibs.uiOverworld, $"weather_{key}_name");
        
        [ShowInInspector, HideLabel, YamlIgnore, ReadOnly]
        private string textNameNegative => Txt.Get (TextLibs.uiOverworld, $"weather_{key}_name_neg");

        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;

        [HideInInspector, YamlIgnore]
        public string key;
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class ThreatComparisonLevel
    {
        public int threshold = 0;
        
        public string textName;

        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
    }
    
    [Serializable]
    public class DataContainerSettingsOverworld : DataContainerUnique
    {
        #if UNITY_EDITOR
        [FilePath (ParentFolder = "Assets/Resources", Extensions = ".bytes")]
        [OnValueChanged ("ValidateNavGraphAssetPath")]
        #endif
        public string navGraphAssetPath;

        public Vector2 navRestrictionOrigin = new Vector2 (-822f, -615);
        public float navRestrictionRadius = 60f;

        public float simulationStartTime = 12f;

        //public float selectionTimeout = 2f;

        public float minEjectionRange = 50f;
        public float maxEjectionRange = 100f;
        
        public float retreatDurationBase = 24f;
        public float retreatDurationFromDistance = 72f;
        public Vector2 retreatDurationDistances = new Vector2 (300f, 1000f);

        public bool createEnemyEjectionSites = false;
        public bool destroyInRangeOnLiberation = true;
        
        public string entityTagResupplyPoint = "homebase";
        public string entityTagMilitarySpawn = "designation_military";
        public string entityTagWarBase = "war_base";
        
        public List<DataBlockOverworldEventSubcheckTag> entityTagsSquad = new List<DataBlockOverworldEventSubcheckTag> ();

        [DictionaryKeyDropdown(DictionaryKeyDropdownType.ScenarioTag)]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> defaultScenarioTags;

      
        public bool evaluateEventsOverTime = true;
        [FoldoutGroup("Logging")]
        public bool logEventEvaluationOverTimeShort = false;  
        [FoldoutGroup("Logging")]
        public bool logEventEvaluationOverTimeMedium = false;  
        [FoldoutGroup("Logging")]
        public bool logEventEvaluationOverTimeLong = false;
        [FoldoutGroup("Logging")]
        public bool logEventCulling = false;
        [FoldoutGroup("Logging")]
        public bool logEventEvaluationOnContact = false;
        [FoldoutGroup("Logging")]
        public bool logAgentDestinations = false;

        public bool eventOptionDetailsUpdated = false;
        public bool eventOptionDetailsCompact = false;

        public float eventChanceShort = 0.5f;
        public float eventChanceMedium = 0.5f;
        public float eventChanceLong = 0.5f;
        
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurve eventEvaluationCooldownDelta;
        
        [YamlMember (Alias = "eventEvaluationCooldownDelta"), HideInInspector] 
        public AnimationCurveSerialized eventEvaluationCooldownDeltaSerialized;
        
        public float eventEvaluationCooldownShortBase = 1f;
        public float eventEvaluationCooldownShortDelta = 0.1f;
        
        public float eventEvaluationCooldownMediumBase = 24f;
        public float eventEvaluationCooldownMediumDelta = 8f;
        
        public float eventEvaluationCooldownLongBase = 168f;
        public float eventEvaluationCooldownLongDelta = 24f;

        public int eventHistorySize = 100;
        
        [ValueDropdown("@DataMultiLinkerOverworldEvent.data.Keys", IsUniqueList = true), ListDrawerSettings(DraggableItems = false)]
        public List<string> eventRuleChanceOverride;

        [Space (8f)]
        public bool entityEffectsOverTime = false;
        public bool entityEffectsOnEntry = false;

        [Space (8f)]
        public float skyRenderTimeThreshold = .1f;
        public bool DebugCanOrderEnemyUnits;
        public bool DebugEnableLMBScroll;
        public float cameraMovementSpeed = 1f;
        public float zoomDefault = 0.15f;
        public float zoomDistanceDefault = 385f;
        public float cameraRotationYDefault = 57f;
        public float cameraRotationXDefault = 26f;
        public float cameraMovementDelay = 1f;

        public bool deltaTimeProtection = true;
        public float deltaTimeMin = 1 / 240f;
        public float deltaTimeMax = 1 / 15f;
        
        public float overworldTimeProgression = 1 / 60f;
        // public float overlayProvinceStartDistance = 100f;
        
        public float weatherChangeMultiplier = 1f;
        public SortedDictionary<string, WeatherLevel> weatherLevels = new SortedDictionary<string, WeatherLevel> ();
        
        [YamlIgnore, HideInInspector, NonSerialized]
        public List<WeatherLevel> weatherLevelsSorted = new List<WeatherLevel> ();
        
        [Header ("Core animation")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve viewBlipAnimationCurve;
        [YamlMember (Alias = "viewBlipAnimationCurve"), HideInInspector] 
        public AnimationCurveSerialized viewBlipAnimationCurveSerialized;

        public float viewBlipAnimationDuration = 0.25f;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve viewFlareAnimationCurve;
        [YamlMember (Alias = "viewFlareAnimationCurve"), HideInInspector] 
        public AnimationCurveSerialized viewFlareAnimationCurveSerialized;

        public float viewFlareAnimationDuration = 0.25f;
        
        public float pingDuration = 0.1f;
        public float visionAnimationDuration = 0.2f;
        public float movementSpeedMultiplier = 10f;
        public float directionDampSmoothTime = 0.1f;
        public float directionDampMaxSpeed = 10000f;

        public float pathMovementWaypointApproachDistance = 1.0f;
        public float pathMovementLookaheadTime = 1.0f;
        public bool pathVisualEnemy = false;
        
        public float playerRecognitionDelay = 1f;
        public float visionTweenRate = 0.6f;

        public float snowAltitudeBaseline = 55f;
        public float snowAltitudeCamera = 55f;
        public DataBlockCombatBiomeFilter snowAltitudeBiomeFilter;
        
        public bool visionRainEffect = true;
        // public float visionRainMultiplierPlayer = 0.5f;
        // public float visionRainMultiplierEnemy = 0.5f;
        // public float visionRainChangeSpeed = 1f;
        
        [FoldoutGroup("Campaign Autosaving")] 
        public bool saveBeforeCombat = true;
        [FoldoutGroup("Campaign Autosaving")]
        public bool saveAfterCombat = true;
        [FoldoutGroup("Campaign Autosaving")]
        public bool autoSaveAtInterval = true;
        [FoldoutGroup("Campaign Autosaving")]
        public float autoSaveTimerSeconds = 600f;
        [FoldoutGroup("Campaign Autosaving")]
        public int autoSaveSlotCount = 3;  
        
        [FoldoutGroup ("Overworld AI")] 
        public float aiMaxUnitResponseRange = 300;
        [FoldoutGroup ("Overworld AI")] 
        public float aiReinforcementCooldown = 100f;

        [FoldoutGroup("Reinforcements")]
        public float overworldDistancePerReinforcementTurn = 15.0f;
        [FoldoutGroup("Reinforcements")]
        public int reinforcementsFromChasingLimit = 3;
        
        [FoldoutGroup ("Overworld AI/Province Sleep&Wake")]
        public float provinceWakeUpdateInterval = 10f;

        [FoldoutGroup ("Overworld AI/Province Sleep&Wake")]
		public float provinceWakeRadius = 300f;

        public int provinceLevelPerDistance = 1;

        [FoldoutGroup ("Overworld AI/Recovery duration")]
        public float aiRecoveryDuration = 4f;
        
        [FoldoutGroup ("Overworld AI/Recovery duration")]
        public float aiRecoveryDurationStatic = 1f;
        
        [FoldoutGroup ("Overworld AI/Patrols")] 
        public bool aiPatrolRework = true;
        
        [FoldoutGroup ("Overworld AI/Patrols")] 
        public SortedDictionary<string, bool> aiPatrolReworkOriginTags;
        
        [FoldoutGroup ("Overworld AI/Patrols")] 
        public SortedDictionary<string, bool> aiPatrolReworkFallbackTags;

        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiPatrolUpdateInterval = 20f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiSpawnDistanceMin = 40f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public List<int> aiPatrolQuotasPerEscalation = new List<int> { 2, 4, 6, 7 };
        
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiSearchMinRadius = 25f;
        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiSearchMaxRadius = 50f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public bool aiInvestigationEnabled = false;
        
        [FoldoutGroup ("Overworld AI/Convoys")] 
        public bool aiConvoyRedirect = false;
        
        [FoldoutGroup ("Overworld AI/Convoys")] 
        public bool aiConvoyRework = true;
        
        [FoldoutGroup ("Overworld AI/Convoys")] 
        public SortedDictionary<string, bool> aiConvoyReworkSharedTags;
        
        [FoldoutGroup ("Overworld AI/Convoys")] 
        public float aiConvoyUpdateInterval = 20f;
        
        [FoldoutGroup ("Overworld AI/Convoys")] 
        public int aiConvoyTargetPopulation = 3;
        
        // [FoldoutGroup("Overworld AI/Convoys")] 
        // public int aiConvoySpawnLimit = 2;
        
        [FoldoutGroup("Overworld AI/Convoys")] 
        public float aiConvoyExternalJourneyChance = 30f;
        
        [FoldoutGroup("Overworld AI/Convoys")]
        [ValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.tags")]
        public List<string> aiConvoyReassignmentDestinationTags;

        [FoldoutGroup ("Overworld AI/Counter Attacks")]
        public float aiCounterAttackUpdateInterval = 20f;

        [FoldoutGroup("Overworld AI/Counter Attacks")]
        public int aiCounterAttackQuota = 2;
        
        [FoldoutGroup ("Overworld AI/Counter Attacks")]
        public float provinceCounterAttackChance = 10f;
        
        [FoldoutGroup("Overworld Generation")]
        public int seed = 1524376141;
        
        [FoldoutGroup("Overworld Generation")] 
        public bool createNewSeed = false;
        
        [ValueDropdown ("@DataLinkerSettingsProvinces.data.definitionsOfProvinces.keys")]
        [FoldoutGroup("Overworld Generation")] 
        public string startingProvinceName = "province_base";
        
        [ValueDropdown ("@DataLinkerSettingsProvinces.data.definitionsOfProvinces.keys")]
        [FoldoutGroup("Overworld Generation")] 
        public string emptyProvinceName = "province_empty";
        
        [FoldoutGroup("Overworld Effects")]
        public float effectRepairJuiceRecoveryRate= 2f;
        
        [FoldoutGroup("Overworld Effects")]
        public float effectBatteryRecoveryRate = 5f;

        [FoldoutGroup("Overworld Base Ability")]
        public float abilityRainSensorDuration = 15f;
        
        public List<ThreatComparisonLevel> threatComparisonLevels = new List<ThreatComparisonLevel> ();
        
        [YamlIgnore, HideInInspector, NonSerialized]
        public List<ThreatComparisonLevel> threatComparisonLevelsSorted = new List<ThreatComparisonLevel> ();

        #if UNITY_EDITOR

        private const string extensionBytes = ".bytes";

        private void ValidateNavGraphAssetPath ()
        {
            if (string.IsNullOrEmpty (navGraphAssetPath))
                navGraphAssetPath = string.Empty;
            else if (navGraphAssetPath.EndsWith (extensionBytes))
                navGraphAssetPath = navGraphAssetPath.Replace (extensionBytes, string.Empty);
        }

        #endif
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            eventEvaluationCooldownDeltaSerialized = (AnimationCurveSerialized)eventEvaluationCooldownDelta;
            viewBlipAnimationCurveSerialized = (AnimationCurveSerialized)viewBlipAnimationCurve;
            viewFlareAnimationCurveSerialized = (AnimationCurveSerialized)viewFlareAnimationCurve;
        }

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            eventEvaluationCooldownDelta = (AnimationCurve)eventEvaluationCooldownDeltaSerialized;
            viewBlipAnimationCurve = (AnimationCurve)viewBlipAnimationCurveSerialized;
            viewFlareAnimationCurve = (AnimationCurve)viewFlareAnimationCurveSerialized;
            
            weatherLevelsSorted.Clear ();
            if (weatherLevels != null)
            {
                foreach (var kvp in weatherLevels)
                {
                    var key = kvp.Key;
                    var info = kvp.Value;
                    info.key = key;
                    
                    // info.textName = DataManagerText.GetText (TextLibs.overworldOther, $"weather_{key}_name");
                    weatherLevelsSorted.Add (info);
                }
                
                weatherLevelsSorted.Sort ((x, y) => x.threshold.CompareTo (y.threshold));
            }
            
            threatComparisonLevelsSorted.Clear ();
            if (threatComparisonLevels != null)
            {
                foreach (var level in threatComparisonLevels)
                    threatComparisonLevelsSorted.Add (level);

                threatComparisonLevelsSorted.Sort ((x, y) => x.threshold.CompareTo (y.threshold));
            }
        }
    }
}