using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
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
    
    [HideReferenceObjectPicker]
    public class QuestLiberationSpawnVariant
    {
        [PropertyRange (0, 3)]
        [LabelText ("Escalation"), SuffixLabel ("$GetEscalationSuffix")]
        [GUIColor ("GetColor")]
        public int escalationValue = 0;
		
        [LabelText ("+ Progress")]
        public int progressMemoryValue = 1;
        
        [LabelText ("- Countdown")]
        public Vector2 countdownOffset = new Vector2 (0f, 0f);
        
        #if UNITY_EDITOR
        
        private Color GetColor () => new HSBColor (0.3f - escalationValue * 0.1f, 0.5f, 1f).ToColor ();

        private static List<string> escalationStrings = new List<string> ()
        {
            "★   ",
            "★★  ",
            "★★★ ",
            "★★★★"
        };

        private string GetEscalationSuffix ()
        {
            #if !PB_MODSDK
            if (escalationStrings == null || escalationStrings.Count == 0 || !escalationValue.IsValidIndex (escalationStrings))
                return "?";
            return escalationStrings[escalationValue];
            #else 
            return null;
            #endif
        }
        
        #endif
    }

    public class QuestLiberationSpawnVariants
    {
        [HideLabel, HideReferenceObjectPicker] 
        public DataBlockComment comment = new DataBlockComment ();

        public int limit = 1;
        public List<QuestLiberationSpawnVariant> variants = new List<QuestLiberationSpawnVariant> ();
    }
    
    public class DataBlockOverworldProvinceFilter : DataBlockFilterLinked<DataContainerOverworldProvinceBlueprint>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerOverworldProvinceBlueprints.GetTags ();
        public override SortedDictionary<string, DataContainerOverworldProvinceBlueprint> GetData () => DataMultiLinkerOverworldProvinceBlueprints.data;
    }
    
    public class DataBlockOverworldModifierFilter : DataBlockFilterLinked<DataContainerOverworldProvinceModifier>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerOverworldProvinceModifier.GetTags ();
        public override SortedDictionary<string, DataContainerOverworldProvinceModifier> GetData () => DataMultiLinkerOverworldProvinceModifier.data;
    }
    
    [Serializable]
    public class DataContainerSettingsOverworld : DataContainerUnique
    {
        public int pointLimit = 3;
        public float pointCountdownDisplayThreshold = 6f;
        public Vector2 pointSpawnRangeDefault = new Vector2 (75f, 150f);
        public float pointSeparationDefault = 40f;
        
        #if UNITY_EDITOR
        [FilePath (ParentFolder = "Assets/Resources", Extensions = ".bytes")]
        [OnValueChanged ("ValidateNavGraphAssetPath")]
        #endif
        public string navGraphAssetPath;

        public Vector2 navRestrictionOrigin = new Vector2 (-822f, -615);
        public float navRestrictionRadius = 60f;

        public float simulationStartTime = 12f;
        
        public bool workshopStripsUnfusedSystems = true;
            
        [DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public SortedDictionary<string, int> partsForWorkshopLevel = new SortedDictionary<string, int> ();

        [DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        public SortedDictionary<string, int> pilotRecruitCostsRookie = new SortedDictionary<string, int> { { ResourceKeys.supplies, 100 } };
        public SortedDictionary<string, int> pilotRecruitCostsPerLevel = new SortedDictionary<string, int> { { ResourceKeys.supplies, 100 } };

        public int pilotRecruitPromotionHistory = 2;
        public float pilotRecruitRefreshInterval = 24f;
        public DataBlockRangeInt pilotRecruitCountBase = new DataBlockRangeInt { min = 3, max = 4 };
        public DataBlockRangeInt pilotRecruitCountLimited = new DataBlockRangeInt { min = 2, max = 2 };
        
        public bool destroyInRangeOnLiberation = true;

        [FoldoutGroup("Logging")]
        public bool logWorkshopLeveling = false;
        
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurve eventEvaluationCooldownDelta;
        
        [YamlMember (Alias = "eventEvaluationCooldownDelta"), HideInInspector] 
        public AnimationCurveSerialized eventEvaluationCooldownDeltaSerialized;

        [Space (8f)]
        public bool entityEffectsOverTime = false;
        public bool entityEffectsOnEntry = false;

        [Space (8f)]
        public float skyRenderTimeThreshold = .1f;
        public bool debugMovementOnSelections;
        public float zoomDefault = 0.15f;
        public float zoomDistanceDefault = 385f;
        public float cameraRotationYDefault = 57f;
        public float cameraRotationXDefault = 26f;

        public bool deltaTimeProtection = true;
        public float deltaTimeMin = 1 / 240f;
        public float deltaTimeMax = 1 / 15f;
        
        public float overworldTimeProgression = 1 / 60f;
        // public float overlayProvinceStartDistance = 100f;
        
        public float weatherChangeMultiplier = 1f;
        public SortedDictionary<string, WeatherLevel> weatherLevels = new SortedDictionary<string, WeatherLevel> ();

        public float[] skyBestTimesOfDay;
        
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
        public float movementSpeedMultiplier = 10f;
        public float movementSpeedMultiplierOverdrive = 3f;
        public float movementPredictedTimeMultiplier = 1.3f;
        public bool movementPredictedTimeDisplay = true;
        public float movementDistanceDisplayScale = 0.2f;
        
        public float directionDampSmoothTime = 0.1f;
        public float directionDampMaxSpeed = 10000f;

        public float overdriveHeatBuildupSpeed = 1f;
        public float overdriveHeatDissipationSpeed = 0.5f;
        public float overdriveHeatDamageFrequency = 1f;
        public float overdriveHeatDamageLimit = 0.25f;
        public float overdriveHeatDamagePerHit = 0.1f;
        
        public float pathMovementWaypointApproachDistance = 1.0f;
        public float pathMovementLookaheadTime = 1.0f;
        public bool pathVisualEnemy = false;
        
        public bool positionUpdatesUnconditional = true;
        public float playerRecognitionDelay = 1f;
        public bool visionClippingDisabled = true;
        public float visionTweenRate = 0.6f;
        
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
        
        [FoldoutGroup ("Overworld AI/Recovery duration")]
        public float aiRecoveryDuration = 4f;
        
        [FoldoutGroup ("Overworld AI/Recovery duration")]
        public float aiRecoveryDurationStatic = 1f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiPatrolUpdateInterval = 20f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiSearchMinRadius = 25f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public float aiSearchMaxRadius = 50f;
        
        [FoldoutGroup ("Overworld AI/Patrols")]
        public bool aiInvestigationEnabled = false;

        [FoldoutGroup("Overworld Generation")]
        public int seed = 1524376141;
        
        [FoldoutGroup("Overworld Generation")] 
        public bool createNewSeed = false;

        [FoldoutGroup("Overworld Base Ability")]
        public float abilityRainSensorDuration = 15f;

        public float travelDurationRealtime = 3f;
        public float travelDurationSim = 12f;

        public int campCostBase = 1;
        public int campCostIncreased = 2;
        
        public float timeSkipDurationRealtime = 3;
        public float timeSkipDurationCamp = 12;
        public float timeSkipDurationResupply = 24;
        public float timeSkipDurationRetreat = 32;
        public bool tutorialOnQuickStart = true;
        public bool campaignProgressTopRight = false;

        public SortedDictionary<string, QuestLiberationSpawnVariants> liberationSpawnVariants = new SortedDictionary<string, QuestLiberationSpawnVariants> ();
        
        public List<string> threatEscalationStrings = new List<string> ();
        
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

            #if UNITY_EDITOR && !PB_MODSDK
            // EDITOR ONLY - QoL feature to allow for easy preview of the 'best time of day hours' in editor
            // Pass temporary float array to TOD Sky system, as PB -> TOD communication is easy, but TOD -> PB is not (TOD classes are compiled before PB)
            TOD_TemporaryStaticParameters.skyBestTimesOfDayTemp = skyBestTimesOfDay;
            #endif
        }

        public float GetBestTimeOfDayHour (float hour)
        {
			float hourNormalized = (hour % 24.0f) / 24.0f;
			return skyBestTimesOfDay [Mathf.RoundToInt (hourNormalized * (skyBestTimesOfDay.Length - 1))];
        }
        
        public IEnumerable<string> GetLiberationSpawnVariantKeys ()
        {
            if (liberationSpawnVariants == null)
                return null;

            return liberationSpawnVariants.Keys;
        }
    }
}