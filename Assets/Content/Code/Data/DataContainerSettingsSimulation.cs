using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][Searchable]
    public class DataContainerSettingsSimulation : DataContainerUnique
    {
        //How much of a bonus does a unit get from having a shield equipped
        public int passivelyShieldedStabilityBonus = 1;
        //How much of a bonus does a unit get from having a shield actively deployed
        public int activelyShieldedStabilityBonus = 1;
        //How much of a penalty to stability does a unit get from being a vehicle
        public int minorUnitStabilityPenalty = 1;
        //How much of a penalty to stability does a unit get from standing still
        public int idleUnitStabilityPenalty = 2;
        //Stability that is used for units who are considered completely unstable
        public int minimumStability = -10;
        //Stability of units that are considered unable to be destabilized
        public int maxStability = 10;
        //How much does the level affect incoming / outgoing concussion damage
        public float concussionLevelOffsetScalar = 0.25f;
        //What is the absolute lowest that concussion damage can be lowered to
        public float concussionLevelMinScalar = 0.1f;
        //What is the absolute highest that concussion damage can be scaled to
        public float concussionLevelMaxScalar = 2.0f;
        
        //How much does the level affect incoming / outgoing buildup damage
        public float buildupLevelOffsetScalar = 0.25f;
        //What is the absolute lowest that buildup damage can be lowered to
        public float buildupLevelMinScalar = 0.5f;
        //What is the absolute highest that buildup damage can be scaled to
        public float buildupLevelMaxScalar = 2.0f;

        public float unitPhysicsMassMin = 20f;
        public float unitPhysicsMassMax = 60f;
        public float unitGameMassMin = 20f;
        public float unitGameMassMax = 60f;

        [Header ("Repair")]
        public float repairRate = 0.09f;
        public float repairResourceDrainRate = 1.44f;

        public float repairCombatBarrierLimit = 2.5f;
        public float repairCombatBarrierCost = 16f;
        public bool combatReactionLights = true;
        public bool combatNightLights = true;
        public bool combatAccentLights = true;
        public bool combatIntro = false;

        [Header ("Gameplay")]
        public bool noDamageFriendly = false;
        public bool noDamageEnemy = false;
        public bool noHeatModeFriendly = false;
        public bool noHeatModeEnemy = false;

        public bool unlimitedBatteryMode = false;
        public bool unlimitedRepairMode = false;
        public bool readGuidedScatterStat = false;
        public bool estimatedStatsIncludeRandomSystems = false;
        
        public bool editorAutoSelectOverworld = false;
        public bool editorAutoSelectCombat = false;
        
        [PropertyRange (0, 3), ShowIf ("editorAutoSelectCombat")]
        public int editorAutoSelectCombatMode = 0;
        
        [Space (8f)]
        public int costDismantlePartIntact = 5;
        public int costDismantlePartDamaged = 10;
        public int costTakePartIntact = 10;
        public int costTakePartDamaged = 20;
        public int costDismantleSubsystemIntact = 5;
        public int costDismantleSubsystemDamaged = 10;
        public int costTakeSubsystemIntact = 10;
        public int costTakeSubsystemDamaged = 20;
        public int costTakeMechFrame = 80;
        

        public int salvageBudgetBaseDefeat = 60;
        public bool salvageWorkshopDuplicateProtection = true;
        public bool workshopChargeSpending = true;
        public int workshopLevelBias = 2;
        public bool dismantlingRewardsUsed = false;
        public bool liveryUnlocks = false;

        /*
        public int salvageRecoverBaseCost = 10;
        public int salvageDismantleBaseCost = 5;
        public float salvageDamageMultiplier = 2f;
        public float salvageFriendlyMultiplier = 0.6f;
        public float salvagePartMultiplier = 1f;
        public float salvageSubsystemMultiplier = 1f;
        */

        [Space (8f)]
        public float shotAimDeviationLimit = 10f;
        public float shotScatterXScalar = 0.6f;
        public float shotScatterYScalar = 1.0f;
        public float defaultRotationSpeed = 1f;
        public float mximumProjectileSpeed = 500f;
        public float stateChangeDelay = 1f;

        public bool targetRangeRestrictions = true;
        public bool targetRangeRestrictionsFlat = false;
        public bool targetSelectionFromPaths = false;
        public int targetTrackingIterations = 3;
        public float targetVelocityModifier = 0.5f;
        public float targetVelocityModifierPlayer = 1f;
        
        public bool logEquipmentGeneration = false;
        public bool logEquipmentLoading = false;
        public bool logScenarioGeneration = false;
        public bool logScenarioUpdates = false;
        public bool logCombatRequests = false;
        public bool logCombatDamage = false;
        public bool logCombatActions = false;
        public bool logCombatTargeting = false;
        public bool logCombatAreaOfEffect = false;
        public bool logCombatUnitStatus = false;
        public bool logCompositeLinks = false;
        public bool logCompositeBehavior = false;
        public bool logProjectileStatus = false;

        public bool partLevelUsed = true;
        public bool partLevelExponential = false;
        public int partLevelLimit = 99;
        public float partLevelIncrease = 0.25f;
        
        public bool partFiringCancellable = false;
        public bool partLevelUpgradeable = false;
        public bool partLevelUpgradeLimitFromWorkshop = true;
        public int partLevelUpgradeLimit = 3;
        public float partLevelCostIncrease = 1.75f;
        public SortedDictionary<int, SortedDictionary<string, int>> partLevelCostsByRating;
        
        public bool partEventsLog = false;

        public int activeSquadLimit = 4;

        [Header("Missile Scrambling")]
        public float scrambleDuration = 2.1f;
        public float scrambleInnerRadius = 20f;
        public float scrambleOuterRadius = 40f;
        public float scrambleDelayVisual = 0.2f;
        public bool scrambleOffsetUsed = true;
        public float scrambleOffsetTime = 0.1f;
        public float scrambleOffsetVertical = 18f;
        public float scrambleOffsetLateral = 18f;

        [Header("Power and Speed")]
        //How much mass can one unit of power lift?
        public float powerToMassRatio = 1;
        //How much of a speed penalty do we get per unit of underpowered mass
        public float massToSpeedRatio = 0.5f;
        public bool speedLimitUltraHeavy = true;
        
        public float thrusterPowerToMassRatio = 1;
        public float massToDashRatio = 5;
        public float dashAngleLimit = 30f;
        public float dashWindowMin = 9f;
        public float dashWindowMaxOffset = 3f;
        public float dashDistanceDistributionMin;
        public float dashDistanceDistributionMax;
        public bool logDashDistanceCalculation = true;
        
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer dashDistanceRemap;
        
        [YamlMember (Alias = "dashDistanceRemap"), HideInInspector] 
        public AnimationCurveSerialized dashDistanceRemapSerialized;
        
        [Header ("Heat")]
        //How often does the overheating damage get checked/applied?
        public float overheatTickInterval = 0.1f;          
        //How much time does it take to reset from "overheating" to "normal"? Must be larger than overheatTickInterval
        public float overheatTimestampEraseTime = 0.5f;
        public float overheatVFXInterval = 0.3f;
        public float overheatDamagePerHeatUnit = 0.001f;

        public float overheatDuration = 0.5f;
        public float overheatDamagePerTick = 0.001f;
        public float overheatBuildupPerTick = 0.1f;
        
        [Header("Heat Dissipation (Modified by Rain Intensity)")]
        [Range(0, 1), Tooltip("Minimum Rain intensity to start the effect")]
        public float rainMinIntensityThreshold = 0.25f;
        [Range(0, 1), Tooltip("Rain intensity at which the effect reaches its maximum")]
        public float rainMaxPotencyThreshold = 0.75f;
        [Range(1, 3), Tooltip("Multiplier for heat dissipation rate at maximum potency")]
        public float rainMaxDissipationMultiplier = 2f;

        [Header ("Fields")]
        public bool fieldStatusFromWater = false;
        public bool fieldStatusImmediateWater = false;
        public float fieldDissipationMultiplierWater = 1.5f;
        public float fieldDissipationMultiplierLava = 0.5f;
        
        [Header("Time")]
        public int timeScaleSteps = 1;
        public float timeScaleMain = 0.6f;
        public float timeScaleSlow = 0.2f;
        public float timeScaleEaseOutTime = 0.3f;
        public float timeScaleMinEaseOut = 0.05f;
        public float timeScaleEaseOutSpeed = 0.5f;
        public float timeScaleEaseInSpeed = 0.5f;
        //public float simulationPrewarmTime = 0.5f;
        public float skyLatitude = 45f;
        
        [Space (4f)]
        public int turnLength = 5;
        public float maxActionTimePlacement = 4.5f;
        public float paintToSecondsScalar = 0.15f;
        public float minActionLengthEquipment = 0.6f;
        public float predictionTimeAnimationSpeedLimit = 100f;
        public float predictionTimeAnimationDuration = 0.1f;
        public float predictionFadeoutThreshold = 5f;
        
        [Space (4f)]
        public bool timelineAdjustmentPatchUsed = true;
        public bool timelineAdjustmentPatchLog = true;
        public bool timelineAdjustmentPatchOnPlacement = false;

        [Header ("Damage")]
        public bool meleeDamageOnFriendlies = false;
        public bool meleeDamageOnPilotsMissing = false;
        public bool meleeDamageOnPilotsKnockedOut = false;
        
        public bool ricochetOnDash = true;
        public float ricochetOnDashDamageScalar = 0.3f;
        public bool ricochetOnMelee = true;
        public bool ricochetOnEjected = true;
        
        public float minimumEnvironmentalDamageFalloffThreshold = 0.2f;
        public bool environmentCollisionDebug = false;
        public float environmentDamageFalloffPower = 2f;
        
        [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
        public string environmentExplosionAsset = string.Empty;
        [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
        public string environmentFireAsset = string.Empty;
        [PropertyRange (0f, 1f)]
        public float environmentFireChance = 0.5f;
        
        public float falloffRicochetThreshold = 0.1f;
        //Random noise added to ricochet reflection vectors
        public float ricochetScatterDegrees = 10f;
        public float ricochetVelocityDecay = 0.33f;
        //How much additional gravity is added to a projectile
        public float ricochetGravityScalar = 2f;
        //What is the split of damage between the hit object and the projectile? (Applies only to units)
        public float ricochetDamageScalar = 1f;
        //Should props cause projectiles to ricochet
        public bool ricochetOnPropHit = false;
        //How is damage is lost after ricocheting off the environment? 1 - 0.33 == 66% of damage retained
        public float environmentRicochetScalar = 0.33f;
        //How is damage is lost after ricocheting off a prop? 1 - 0.33 == 66% of damage retained
        public float propRicochetScalar = 0.33f;
        //percent of damage lost on penetrating environment 1 - 0.20 == 80% of damage retained
        public float environmentPenetrationDamageLoss = 0.20f;
        //percent of damage lost on penetrating units 1 - 0.25 == 75% of damage retained
        public float unitPenetrationDamageLoss = 0.25f;
        //percent of damage lost on penetrating props 1 - 0.20 == 80% of damage retained
        public float propPenetrationDamageLoss = 0.20f;
        
        public float beamVulnerabilityDecayPerSec = 1.0f;
        public float beamVulnerabilityDecayPauseOnDamage = 0.1f;
        public float beamTicksPerSecond = 30;

        public float turningLimitLengthAdjustmentK = 1.05f;

        public float shieldBeamReflectionArc = 180.0f;

        [Tooltip("How close the beams have to be to create a singularity?")]
        public float beamSingularityGenerationDistance = 3.0f;
        [Tooltip("Damage radius of a singularity")]
        public float beamSingularityRadius = 10.0f;
        [Tooltip("Singularity damage to geometry")]
        public float beamSingularityAreaDamage = 7.0f;
        [Tooltip("Singularity damage multiplier, relative to beam damage")]
        public float beamSingularityDamageMultiplier = 0.5f;

        [Tooltip("How much beam damage do shields negate?")]
        public float beamReflectionDamageNegation = 1.0f;
        [Tooltip("How much beam damage gets kept with each reflect")]
        public float beamReflectionDamageAttenuationK = 1.0f;

        public float compositeAreaOfEffectDecay = 0.5f;
		public float penetrationUnitDistance = 3f;
        
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer impactDamageFalloff;
        
        [YamlMember (Alias = "impactDamageFalloff"), HideInInspector] 
        public AnimationCurveSerialized impactDamageFalloffSerialized;
        
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer damageCoverageApproximation;
        
        [YamlMember (Alias = "damageCoverageApproximation"), HideInInspector] 
        public AnimationCurveSerialized damageCoverageApproximationSerialized;

        public float shieldVsHeatDamageMultiplier = 0.3f;
        public float shieldVsStaggerDamageMultiplier = 0.0f;
        public float shieldVsConcussionDamageMultiplier = 0.0f;

        public bool concussionScalingFromWeight = true;
        public bool concussionScalingFromWeightBody = true;
        public bool unifiedWeightClasses = true;

        public bool staggerUsed = false;
        public float staggerDebuffDecay = 0.2f;
        public float staggerLevelOffsetMultiplier = 0.25f;
        public float staggerLevelMinMultiplier = 0.1f;
        public float staggerLevelMaxMultiplier = 2.0f;

        public float heatLevelOffsetMultiplier = 0.25f;
        public float heatLevelMinMultiplier = 0.1f;
        public float heatLevelMaxMultiplier = 2.0f;

        [Header("Pilot")]
        //How much extra damage do we take to concussion when flanked?
        public float concussionScalarFlanking = 1.25f;

        //How quickly does fatigue reduce in overworld simulation time? 1 hour restores 1 fatigue
        public float healthRecoveryRate = 0f;
        
        //How much concussion damage can a pilot take in one fight before they're unconscious?
        public float concussionOffset = 28f;
        public float concussionOffsetVariation = 5f;

        //When a pilot is created, they start with this health
        public float pilotHealthBase = 100f;
        public float pilotHealthBaseVariation = 25f;
        public float pilotHealthDeficitVariationEnemy = 0.2f;
        
        [Header ("Resources")]
        [Tooltip("How long can we start regenerating after damage?")]
        public float barrierRegenerationDelay = 5.0f;

        [Header("Pilot Upkeep")]
        [Tooltip("How frequent is the upkeep (Hour)")]
        public float pilotUpkeepFrequency = 24f;
        [Tooltip("Per-pilot cost for pilot upkeep")]
        public float pilotUpkeepCost = 1f;

        [Header ("Physics")]
        public bool allowPhysicsSubstepping = true;
        public float minimumSubstepLength = 0.001f;
        public float maximumStepLength = 0.06f;
        public float defaultPhysicsStep = 0.03f;
        public float preWarmStep = 0.03f;
        public Vector3 gravityForce = Vector3.down * 9.7f;

        [Header ("Crashing")]
        public float crashingGroundCastRadius = 0.5f;
        public float crashingGroundRayOffset = 1f;
        public float crashingGroundRayThreshold = 1.25f;
        public float crashingGroundDecelerationRate = 5f;
        public float crashingSlopeGroundingThreshold = 0.9f;
        public bool crashingGroundOnEnd = false;

        public float crashingDuration = 5.0f;
        public float crashingAngularVelocityMax = 10f;
        public float crashingDelayOnHeatDeath = 0f;

        public float verticalVelocityLimit = 10f;
        public float verticalVelocityGain = 1f;
        public float verticalVelocityDamageThreshold = 1f;

        //public string onCrashActionKey = "Crashing";
        //How much damage the environment takes from units
        public float environmentalImpactDamage = 100f;
        public float environmentalInactiveImpactDamageModifier = 0.5f;
        public float environmentalDebugDamageLow = 25f;
        public float environmentalDebugDamageHigh = 100f;
        public float environmentalDebugDamageShift = -0.5f;
        
        //What portion of the unit's total health is lost to crashing into the environment
        public float mechEnvironmentDamageScalar = 0.05f;
        public float unitCollisionDamageScalar = 0.05f;
        public float vehicleEnvironmentDamageScalar = 0.1f;
        public float crashingImpactCoefficient = 0.75f;
        public float crashingSeparationThreshold = 3f;
        public float crashingSeparationSpeed = 5f;
        public float crashingVelocityLimit = 30f;
        public float tankAngularDamping = 1;
        public float tankLinearDamping = 1;
        public float mechAngularDamping = 1;
        public float mechLinearDamping = 1;

        public float environmentalDamageCooldown = 0.3f;
        public float unitCrashDamageCooldown = 1f;

        [Header ("Melee")]
        public bool meleeFallbackVariantsUsed = false;
        public bool meleeRangeFromDash = true;
        public bool meleeProjectileCollision = false;
        public float meleeCollisionRadius = 3f;

        [Header ("Pathfinding")]
        public float pathSegmentLengthLimit = 9;
        [PropertyRange (0f, 1f)]
        public float pathRelaxationFactor = 0.5f;
        public int pathSubdivisionLevel = 4;
        public int restrictedPathTags = 1; // binary: 1000 0000 ..
        public int openPathTags = 3; // binary: 1100 0000 ..s
        public StartEndModifier.Exactness pathEndPointExactness = StartEndModifier.Exactness.ClosestOnNode;
        public StartEndModifier.Exactness pathStartPointExactness = StartEndModifier.Exactness.ClosestOnNode;

        [Header ("Tags")]
        [OnValueChanged ("SortSpawnTags")]
        public HashSet<string> combatSpawnTags = new HashSet<string> ();
        
        [OnValueChanged ("SortLocationTags")]
        public HashSet<string> combatLocationTags = new HashSet<string> ();
        
        [OnValueChanged ("SortScenarioRewards")]
        public HashSet<string> scenarioRewardKeys = new HashSet<string> ();
        
        [OnValueChanged ("SortScenarioUnitTags")]
        public HashSet<string> scenarioUnitTags = new HashSet<string> ();

        [OnValueChanged ("SortVolumeTags")]
        public HashSet<string> combatVolumeTags = new HashSet<string> ();

        public List<DataBlockDismantlingReward> dismantlingRewardsParts = new List<DataBlockDismantlingReward> ();
        public List<DataBlockDismantlingReward> dismantlingRewardsSubsystems = new List<DataBlockDismantlingReward> ();

        [Header("SLG Features Flag")] 
        public bool displayCombatStats = false;
        public bool salvageMaxSupplyFeature = false;
        public bool inventoryRangeSelection = false;
        public bool scrollableEventText = false;
        public bool autoResolution = false;
        public float pathfindingMaxSearchRange = 400f;
        public float pathfindingPushbackDistance = 15f;
        public float saveMaximumLimit = 60;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            if (damageCoverageApproximation != null)
                damageCoverageApproximationSerialized = (AnimationCurveSerialized) damageCoverageApproximation.curve;
          
            if (impactDamageFalloff != null)
                impactDamageFalloffSerialized = (AnimationCurveSerialized) impactDamageFalloff.curve;
            
            if (dashDistanceRemap != null)
                dashDistanceRemapSerialized = (AnimationCurveSerialized) dashDistanceRemap.curve;
        }

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            
            if (damageCoverageApproximationSerialized != null)
                damageCoverageApproximation = new AnimationCurveContainer ((AnimationCurve) damageCoverageApproximationSerialized);
            
            if (impactDamageFalloffSerialized != null)
                impactDamageFalloff = new AnimationCurveContainer ((AnimationCurve) impactDamageFalloffSerialized);
            
            if (dashDistanceRemapSerialized != null)
                dashDistanceRemap = new AnimationCurveContainer ((AnimationCurve) dashDistanceRemapSerialized);
            
            SortSpawnTags ();
            SortLocationTags ();
            SortScenarioRewards ();
            SortScenarioUnitTags ();
            SortVolumeTags ();
        }

        private void SortSpawnTags () => UtilityCollections.Sort (ref combatSpawnTags);
        private void SortLocationTags () => UtilityCollections.Sort (ref combatLocationTags);
        private void SortScenarioRewards () => UtilityCollections.Sort (ref scenarioRewardKeys);
        private void SortScenarioUnitTags () => UtilityCollections.Sort (ref scenarioUnitTags);
        private void SortVolumeTags () => UtilityCollections.Sort (ref combatVolumeTags);
    }

    public class DataBlockDismantlingReward
    {
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags = new SortedDictionary<string, bool> ();
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> resources = new SortedDictionary<string, int> ();
    }
}

