using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum AnimationFilterPositionMode
    {
        None,
        SmoothDamp,
        CriticalSpring
    }
    
    public enum AnimationFilterRotationMode
    {
        None,
        SmoothDamp
    }

    [Serializable]
    public class DataBlockTransformSequence
    {
        [FoldoutGroup ("Capture", false), YamlIgnore]
        [InlineButton ("Capture")]
        public Transform source;
        
        [FoldoutGroup ("Capture"), YamlIgnore]
        public string sourceChildFilter;
        
        [FoldoutGroup ("Capture"), YamlIgnore]
        public float sourceTimeScale;
        
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<DataBlockTransformSnapshot> snapshots;

        #if UNITY_EDITOR
        
        public static DataBlockTransformSequence selection;

        [Button ("Select for editing"), PropertyOrder (-1)]
        private void Select ()
        {
            selection = this;
        }
        
        private void Capture ()
        {
            if (source == null || string.IsNullOrEmpty (sourceChildFilter))
                return;

            if (snapshots == null)
                snapshots = new List<DataBlockTransformSnapshot> ();
            else
                snapshots.Clear ();
            
            var childCount = source.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                var child = source.GetChild (i);
                if (!child.name.StartsWith (sourceChildFilter))
                    continue;

                var nameSplit = child.name.Split ('_');
                if (nameSplit.Length < 2)
                    continue;

                var nameSectionLast = nameSplit[nameSplit.Length - 1];
                bool timeParsed = int.TryParse (nameSectionLast, out int timeExtracted);
                if (!timeParsed)
                    continue;

                float timeFinal = timeExtracted * sourceTimeScale;
                snapshots.Add (new DataBlockTransformSnapshot
                {
                    position = child.position,
                    rotation = child.rotation,
                    time = timeFinal,
                });
            }
            
            snapshots.Sort ((x, y) => x.time.CompareTo (y.time));
        }
        
        #endif
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockTransformSnapshot
    {
        [ShowInInspector, ReadOnly, HorizontalGroup, YamlIgnore]
        [LabelText ("Frame/CT/CS")]
        public int frame => Mathf.RoundToInt (time * 60);

        [ShowInInspector, ReadOnly, HorizontalGroup, YamlIgnore]
        [HideLabel]
        public float curveTimeLast = 0;
        
        [ShowInInspector, ReadOnly, HorizontalGroup, YamlIgnore]
        [HideLabel]
        public float curveSampleLast = 0;

        public float time;
        public Vector3 position;
        public Quaternion rotation;
    }
    
    [Serializable, Searchable] 
    public class DataContainerSettingsAnimation : DataContainerUnique
    {
        [Header ("Core curves")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve stopIntensityPreDeceleration;
        [YamlMember (Alias = "stopIntensityPreDeceleration"), HideInInspector] 
        public AnimationCurveSerialized stopIntensityPreDecelerationSerialized;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve stopIntensityPostDeceleration;
        [YamlMember (Alias = "stopIntensityPostDeceleration"), HideInInspector]  
        public AnimationCurveSerialized stopIntensityPostDecelerationSerialized;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve stopIntensityBlend;
        [YamlMember (Alias = "stopIntensityBlend"), HideInInspector] 
        public AnimationCurveSerialized stopIntensityBlendSerialized;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve idleIntensityBlend;
        [YamlMember (Alias = "idleIntensityBlend"), HideInInspector] 
        public AnimationCurveSerialized idleIntensityBlendSerialized;
        
        [Header ("Listener curves")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve listenerPosition;
        [YamlMember (Alias = "listenerPosition"), HideInInspector] 
        public AnimationCurveSerialized listenerPositionSerialized;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve listenerPositionCurveCombat;
        [YamlMember (Alias = "listenerPositionCurveCombat"), HideInInspector] 
        public AnimationCurveSerialized listenerPositionCurveCombatSerialized;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve listenerPositionCurveOverworld;
        [YamlMember (Alias = "listenerPositionCurveOverworld"), HideInInspector] 
        public AnimationCurveSerialized listenerPositionCurveOverworldSerialized;

        [Header ("Core data")]
        public float animLayerBlendSpeed = 10f;
        public float braceAnimLayerBlendSpeed = 2f;
        public float slidingSpeedThreshold = 8f;
        public float slidingActionLengthThreshold = 1f; //transition to sliding animation is a second long
        public float minNormalizedSpeedQuickTurn = 0.0f;
        public float maxNormalizedSpeedQuickTurn = 0.5f;
        public float movementSpeedDampSmoothTime = 0.1f;
        public float movementSpeedDampMaxSpeed = 10000f;

        public float minViableSpeedRatio = 0.5f;
        public float landingDelayRange = 0.25f;
        public bool pilotPersonalityUsed = false;
        public bool fineTuneAimIkUsed = false;

        [PropertySpace (4f)]
        public bool secondaryEquipmentFacingForcedAny = false;
        public bool secondaryEquipmentFacingForcedArms = true;
        public bool secondaryEquipmentFacingSmooth = true;
        public float secondaryEquipmentFacingThreshold = 45f;
        
        [FoldoutGroup ("Shockwaves")]
        public SortedDictionary<string, DataBlockResourceTexture> shockwaveTextures;
        
        [FoldoutGroup ("Shockwaves")]
        public SortedDictionary<string, DataContainerResourceAsset<ShockwaveVisualPreset>> shockwavePresets;
        
        [Header ("Dash")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve timeRemapDash;
        [YamlMember (Alias = "timeRemapDash"), HideInInspector] 
        public AnimationCurveSerialized timeRemapDashSerialized;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve timePreviewDash;
        [YamlMember (Alias = "timePreviewDash"), HideInInspector] 
        public AnimationCurveSerialized timePreviewDashSerialized;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve dashTrajectoryFlat;
        [YamlMember (Alias = "dashTrajectoryFlat"), HideInInspector] 
        public AnimationCurveSerialized dashTrajectoryFlatSerialized;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve dashTrajectoryAscent;
        [YamlMember (Alias = "dashTrajectoryAscent"), HideInInspector] 
        public AnimationCurveSerialized dashTrajectoryAscentSerialized;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve dashTrajectoryDescent;
        [YamlMember (Alias = "dashTrajectoryDescent"), HideInInspector] 
        public AnimationCurveSerialized dashTrajectoryDescentSerialized;

        public float dashVFXShutdownOffset = 0.1f;
        public Vector2 dashPreviewPoseWindow = new Vector2 (0f, 1f);
        
        [Header ("Fallback Melee")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve timeRemapMeleeFallback;
        [YamlMember (Alias = "timeRemapMeleeFallback"), HideInInspector] 
        public AnimationCurveSerialized timeRemapMeleeFallbackSerialized;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve timePreviewMeleeFallback;
        [YamlMember (Alias = "timePreviewMeleeFallback"), HideInInspector] 
        public AnimationCurveSerialized timePreviewMeleeFallbackSerialized;

        public float timePreviewSpeed = 0.5f;
        public float timePreviewRemap = 0.5f;
        public Vector4 timePreviewFactors = new Vector4 (1f, 1f, 1f, 1f);

        [Header ("Standard Melee")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve timeRemapMeleeStandard;
        [YamlMember (Alias = "timeRemapMeleeStandard"), HideInInspector] 
        public AnimationCurveSerialized timeRemapMeleeStandardSerialized;
        
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve timePreviewMeleeStandard;
        [YamlMember (Alias = "timePreviewMeleeStandard"), HideInInspector] 
        public AnimationCurveSerialized timePreviewMeleeStandardSerialized;

        // Start and end of a lunge for the mech animation system
        public Vector2 meleeAnimationTimingsStandard = new Vector2 (0.37f, 0.49f);
        public Vector2 meleeAnimationTimingsFallback = new Vector2 (0.37f, 0.49f);
        
        // Start and end of a jump for the projection holograms
        public Vector2 meleePoseTimingsStandard = new Vector2 (0.37f, 0.49f);
        public Vector2 meleePoseTimingsFallback = new Vector2 (0.37f, 0.49f);
        
        // Start and end of prediction time interval (normalized) shown while painting
        public Vector2 meleePredictionRangeStandard = new Vector2 (0.485f, 0.75f);
        public Vector2 meleePredictionRangeFallback = new Vector2 (0.485f, 0.75f);
        
        // Start and end of thruster firing effects
        public Vector2 meleeEffectTimingsStandard = new Vector2 (0.41f, 0.49f);
        public Vector2 meleeEffectTimingsFallback = new Vector2 (0.25f, 0.75f);

        public Vector3 meleeStandardCollisionSize = new Vector3 (2f, 6f, 3f);

        public float meleeShockwaveHeightOffset = 2.5f;

        [Header ("Pilots")]
        public float hipDropThreshold = -0.1f; 
        public float poseBlendSpeed = 1f; 
        public float lookAheadBlendSpeed = 1f;

        [Header ("Melee Overrides")]
        public bool meleeCollisionVisualization = true;
        public bool meleeCollisionInterpolation = true;
        
        public bool meleeCollisionSizeOverride = false;
        [ShowIf ("meleeCollisionSizeOverride")]
        public Vector3 meleeCollisionSize = new Vector3 (2f, 6f, 3f);
        public Vector3 meleeCollisionSizeFallback = new Vector3 (3f, 7f, 3f);

        public bool meleeCollisionOffsetOverride = false;
        [ShowIf ("meleeCollisionOffsetOverride")]
        public Vector3 meleeCollisionOffset = new Vector3 (0f, 0f, 0f); 
        public Vector3 meleeCollisionOffsetFallback = new Vector3 (0f, 0f, 0f);
        
        [Header ("Tank Animation")]
        public float tiltLateralInputDeadzone = 5f;
        public float tiltLateralInputLimit = 15f;
        public float tiltLateralDampSmoothTime = 0.1f;
        public float tiltLateralDampMaxSpeed = 10000f;
        public float tiltParallelDampSmoothTime = 0.1f;
        public float tiltParallelDampMaxSpeed = 10000f;
        public float tiltParallelDecaySpeed = 1f;

        public float movementFactorNegative = 0f;
        public float movementFactorPositive = 1f;
        public float movementFactorDampSmoothTime = 0.1f;
        public float movementFactorDampMaxSpeed = 10000f;

        [Header ("Mech Animation")]
        public float paramDampenValue = 0.1f;
        public float fastMechTransitionThreshold = 16f;
        public float mechWalkSpeed = 3.5f;
        public float mechJogSpeed = 9f;
        public float mechRunSpeed = 16f;
        public float mechSprintSpeed = 28f;

        [Header ("Banking")]
        public float bankingAngularRange = 180f;
        public float bankingDampSmoothTime = 0.1f;
        public float bankingDampMaxSpeed = 10000f;
        public float bankingMultiplier = 1f;
        public float stationaryRotateSpeedValidMin = -1f;
        public float stationaryRotateSpeedValidMax = 1f; 
        public float rotSpeedThreshold = 0.1f;
        public float maxStationTurningAngleWithoutStepping = 5f;
        public float mechBankingBlendSpeed = 20f;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker]
        public AnimationCurve bankingSpeedWarp;
        [YamlMember (Alias = "bankingSpeedWarp"), HideInInspector]
        public AnimationCurveSerialized bankingSpeedWarpSerialized;

        [Header ("Blend Data")]
        public float valuePopThreshold = 2f;
        public float animIdleToIdlePoseBlend = 4f;
        public float ikAimConstraintBlendOutroBuffer = 2f;
        public float iKAimConstraintIntroDelay = 0.1f;

         [Header ("IK Aiming")]
        public float safeRotateDistanceThreshold = 10f; // threshold distance for animating rotation to target of action, this stops spinning of the upperbody when the mech is directly above its target
        public float rotSpeedChangeDistanceBasedTarget = 300f;
        public float closePointTargetDistance = 150f;

        [Header ("Vertical Aiming")]
        public float aimVerticalMaxAngle = 45f;
        public float aimVerticalMinAngle = -45f;
        public float aimVerticalHeadMultiplier = 0.5f;
        public float aimVerticalTorsoMultiplier = 0.75f;
        public float aimVerticalBlendOut = 5f; 
        public float vertAngleBlendPilot = 5f;
        public float aimMechOriginHeight = 5f; 
        public float aimMechAngleDivisor = 90f; 

        [Header ("IK Setup")]
        public bool ikUpperBody = true;
        public bool ikFeetOn = false;
        public bool ikFeetAdvancedOn = false;
        public bool ikFootLockStepStationaryTurning = false;
        public float maxIKPoleWeightSupport = 0.75f;

        [Header ("IKBlends")]
        public float armStraightLength = 2.92f;
        public float upperIKBlendBound = 1.0f;
        public float lowerIKBlendBound = 0.8f;
        public float mechIkBlendSpeed = 20f; 
        public float ikBlendIntroBuffer = 0.1f; // should be at the least slightly less than anim prep time as the targeting updates on the frame of anim prep time before start of current action
        public float ikBlendIntroBufferHeavy = 0.1f;
        public float ikBlendIntroBufferExcessive = 0.0f;
        public float mechIkBlendDeltaIntro = 0.25f;
        public float mechIkBlendDeltaIntroHeavy = 0.25f;
        public float mechIkBlendDeltaIntroExcessive = 0.25f;
        public float mechIkBlendDeltaOutro = 0.25f;
        public float mechIKBlendDeltaHolster = 5f;
        public float mechIkBlendDeltaOutroHeavy = 0.25f;
        public float mechIkBlendDeltaOutroExcessive = 0.25f; 
        public float armIKBlendMultiplier = 1f;
        public bool mechIKMaintainRotation = false;
        public float ikRUpcomingEquipmentActivationWindow = 0.05f;
        public float ikLUpcomingEquipmentActivationWindow = 0.15f;
        public float ikTorsoUpcomingEquipmentActivationWindow = 0.15f;
        public float pvValueMultiplier = 0.8f;

        [Header ("IKTorso")]
        public float torsoIKSwitchTransformRotationDelta = 0.2f;
        public float torsoIKMaxRotThreshold = 180.0f;
        public float twistTorsoMax = 1.0f;
        public float twistTimeThreshold = 1.0f;

        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker]
        public AnimationCurve timeTorsoTwistImpulse;
        [YamlMember (Alias = "timeTorsoTwistImpulse"), HideInInspector]
        public AnimationCurveSerialized timeTorsoTwistImpulseSerialized;

        [Header ("IKFeet")]
        public float footPlantedHeightThreshold = 0.985f; //height of foot joint is 0.9744954
        public float ikThreshold = 0.15f;
        public float mechIKFootBlendSpeed = 20f;
        public float ikFeetWeightBlendSpeed = 10f; 
        public float distanceFootReduction = 1.5f;
        public float legHyperExtensionThreshold = 0.1f;
        public float maxGroundHitRayCast = 4f;
        public float footFloorPenetrationCheckThreshold = 0.25f;

        [Header ("Weapon Joint Drift Debug")]
        public float shieldDriftDebugSqrThreshold = 1.5f;

        [Header ("Action Buffer Lengths")]
        public float slidingStartThreshold = 0.99f;
        public float slidingEndThreshold = 0.75f;
        public float animationSlidingPrep = 0.66f;
        public float animationLengthFiringPrep = 0.33f; //this needs to be maximum the outro buffer of an action, otherwise upcoming action will take precedent of current action for chained actions. Forcing current to exit early

        public int rotationFixTemp = 0;
        public float animationLengthMovementStop = 0.8f;
        public float animationLengthMovementStopFastMechs = 0.333f; //20 frames at 30fps
        public float animationLengthMovementStart = 0.8f;
        public float animationLengthMovementStartRun = 0.6f;
        public float animationLengthMovementStartSprint = 0.5f;
        public float animationLengthDashAnticipation = 0.5f; 
        public float animationStopLimitFast = 0.5f;
        public float animationStopLimitSlow = (75f-18f)/30f;
        public float futureActionFastSwitchBuffer = 0.5f;
        public float endCurrentActionPoseBuffer = 0.5f;
        public float transitionToSlidingThreshold = 0.1f;
        public float transitionOutOfDashThreshold = 0.3f;
        public float targetedActionBuffer = 0.25f;
        public float targetedActionRootDelay = 0.083f;
        public float holsterCheckBuffer = 2f;
        public float quickHolsterBuffer = 1.3f; // is the outro animation + the long holster animation, not just the length of the long holster animation
        public float immediateHolsterBuffer = 0.7f;
        public float heavyUnholsterWindow = 0.5f;
        public float sideUnholsterWindow = 0.5f;
        public float jumpSpeedThreshold = 7.5f;
        public float dashToMeleeTransitionMaxLength = 0.35f;
        public float animationFiringIntroDuration = 0.27f;
        public float movementFollowMeleeThreshold = 0.1f;
        public float chainedActionThreshold = 1f;
        public float timeBetweenChainedActionsThreshold = 1f; 
        public float shortActionThreshold = 2f;

        [Header ("Melee Buffer")]
        public float dashToMeleeTransitionTriggerTime = 1f;
        public float upcomingMeleeThreshold = 2f;

        public AnimationFilterPositionMode filterPositionMode = AnimationFilterPositionMode.SmoothDamp;
        public bool filterPositionDuringRemap = true;
        public float filterPositionInputSmoothDamp = 0.1f;
        public float filterPositionLimitSmoothDamp = 10000f;
        public float filterPositionInputCriticalSpring = 50f;
        public float filterPositionLimitCriticalSpring = 10000f;
        public bool filterPositionCatchup = true;
        
        public AnimationFilterRotationMode filterRotationMode = AnimationFilterRotationMode.SmoothDamp;
        public bool filterRotationDuringRemap = true;
        public float filterRotationInputSmoothDamp = 0.1f;
        public float filterRotationLimitSmoothDamp = 10000f;
        public float filterFadeDuration = 0.05f;

        public float collisionWarningThreshold = 9f;

        public float crashingGravityForce = 9f;
        
        public float authoredGetUpAnimationLength = 2.333f;
        public float authoredConcussionAnimationLength = 4.167f;

        public float idleBreakCoolDownMin = 0.5f;
        public float idleBreakCoolDownMax = 1.5f;

        [Header ("Recoil")]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurve weaponDraw;
        [YamlMember (Alias = "weaponDraw"), HideInInspector] 
        public AnimationCurveSerialized weaponDrawSerialized;
        public bool recoilUsed = true;
        public float recoilDecaySpeed = 4f;
        public float recoilChangeSpeed = 40f;

        public float torsoRecoilMult = 50f;
        public float torsoZAxisRecoilMult = 0.5f;
        public float torsoZAxisBraceRecoilMult = -0.2f;
        public float braceRotRecoilMult = 5f;
        
        public float recoilTranslationDecayRate = 0.25f;

        public float maxSimSpeed = 0.6f;
        public float maxRateOfFire = 1.25f;//this is rate of fire in relation to deltaTime

        public float maxAdditiveOffsetMag = 10f;
        public float recoilResetRotBlendOut = 0.25f;

        [Header ("Impacts")]
        public float impactAdditionScalar;
        public float impactDecaySpeed;
        public float impactDecaySpeedFast;
        public float hitRandomSpreadUpper = 6;
        public float hitRandomSpreadLower = -5;
        public float hitReactUpperBodyMagMultiplier = 0.5f;
        public float hitReactMagMultiplier = -2f;
        public float hitReactAnimationDuration = 0.1f;
        public float hitReactAnimationSpeedLimit = 500f;
        public float damageHitDecayPilot = 10f;

         [Header("FX")]
        public float fxTimeToLive = 5f;
        public Vector3 firingCenterOffset = Vector3.up * 1.75f;
        public float fxStartDisposalAt = 0.9f;
        public float fxFadeSpeed = 4f;
        public float fxFadeDistance = 3f;

        [Header ("Movement overrides")]
        public bool debugMelee = false;
        public bool debugChains = false;
        
        public bool dashNonlinearMode = true; 
        public float meleeSpeedThresholdMedium = 10f;
        public float meleeSpeedThresholdLight = 22f;
        public bool flipMeleeSide = true;

        [Header("Audio")] 
        public float turretAudioAngularVelocityThreshold = 10f;
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            stopIntensityPreDecelerationSerialized = (AnimationCurveSerialized)stopIntensityPreDeceleration;
            stopIntensityPostDecelerationSerialized = (AnimationCurveSerialized)stopIntensityPostDeceleration;
            stopIntensityBlendSerialized = (AnimationCurveSerialized)stopIntensityBlend;
            idleIntensityBlendSerialized = (AnimationCurveSerialized)idleIntensityBlend;
            weaponDrawSerialized = (AnimationCurveSerialized)weaponDraw;
            
            listenerPositionSerialized = (AnimationCurveSerialized)listenerPosition;
            listenerPositionCurveOverworldSerialized = (AnimationCurveSerialized)listenerPositionCurveOverworld;
            listenerPositionCurveCombatSerialized = (AnimationCurveSerialized)listenerPositionCurveCombat;

            timeRemapDashSerialized = (AnimationCurveSerialized)timeRemapDash;
            timePreviewDashSerialized = (AnimationCurveSerialized)timePreviewDash;
            
            timeRemapMeleeStandardSerialized = (AnimationCurveSerialized)timeRemapMeleeStandard;
            timePreviewMeleeStandardSerialized = (AnimationCurveSerialized)timePreviewMeleeStandard;
            
            timeRemapMeleeFallbackSerialized = (AnimationCurveSerialized)timeRemapMeleeFallback;
            timePreviewMeleeFallbackSerialized = (AnimationCurveSerialized)timePreviewMeleeFallback;
            
            dashTrajectoryFlatSerialized = (AnimationCurveSerialized)dashTrajectoryFlat;
            dashTrajectoryAscentSerialized = (AnimationCurveSerialized)dashTrajectoryAscent;
            dashTrajectoryDescentSerialized = (AnimationCurveSerialized)dashTrajectoryDescent;

            bankingSpeedWarpSerialized = (AnimationCurveSerialized)bankingSpeedWarp;
            timeTorsoTwistImpulseSerialized = (AnimationCurveSerialized)timeTorsoTwistImpulse;
            
            if (shockwaveTextures != null)
            {
                foreach (var kvp in shockwaveTextures)
                    kvp.Value.OnBeforeSerialization ();
            }
            
            if (shockwavePresets != null)
            {
                foreach (var kvp in shockwavePresets)
                    kvp.Value.OnBeforeSerialization ();
            }
        }

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            stopIntensityPreDeceleration = (AnimationCurve)stopIntensityPreDecelerationSerialized;
            stopIntensityPostDeceleration = (AnimationCurve)stopIntensityPostDecelerationSerialized;
            stopIntensityBlend = (AnimationCurve)stopIntensityBlendSerialized;
            idleIntensityBlend = (AnimationCurve)idleIntensityBlendSerialized;
            weaponDraw = (AnimationCurve)weaponDrawSerialized;

            listenerPosition = (AnimationCurve)listenerPositionSerialized;
            listenerPositionCurveOverworld = (AnimationCurve)listenerPositionCurveOverworldSerialized;
            listenerPositionCurveCombat = (AnimationCurve)listenerPositionCurveCombatSerialized;
            
            timeRemapDash = (AnimationCurve)timeRemapDashSerialized;
            timePreviewDash = (AnimationCurve)timePreviewDashSerialized;
            
            timeRemapMeleeStandard = (AnimationCurve)timeRemapMeleeStandardSerialized;
            timePreviewMeleeStandard = (AnimationCurve)timePreviewMeleeStandardSerialized;
            
            timeRemapMeleeFallback = (AnimationCurve)timeRemapMeleeFallbackSerialized;
            timePreviewMeleeFallback = (AnimationCurve)timePreviewMeleeFallbackSerialized;
            
            dashTrajectoryFlat = (AnimationCurve)dashTrajectoryFlatSerialized;
            dashTrajectoryAscent = (AnimationCurve)dashTrajectoryAscentSerialized;
            dashTrajectoryDescent = (AnimationCurve)dashTrajectoryDescentSerialized;

            bankingSpeedWarp = (AnimationCurve)bankingSpeedWarpSerialized;
            timeTorsoTwistImpulse = (AnimationCurve)timeTorsoTwistImpulseSerialized;
            
            if (shockwaveTextures != null)
            {
                foreach (var kvp in shockwaveTextures)
                    kvp.Value.OnAfterDeserialization ();
            }
            
            if (shockwavePresets != null)
            {
                foreach (var kvp in shockwavePresets)
                    kvp.Value.OnAfterDeserialization (kvp.Key);
            }
        }
        
        public ShockwaveVisualPreset GetShockwavePreset (string key)
        {
            #if !PB_MODSDK
            if (shockwavePresets == null || string.IsNullOrEmpty (key))
                return null;

            bool found = shockwavePresets.TryGetValue (key, out var value);
            return found ? value.asset : null;
            #else
            return null;
            #endif
        }
    }

    
    
    
    [Serializable]
    public class AnimationCurveSerialized
    {
        public WrapMode modePostWrap;
        public WrapMode modePreWrap;
        public KeyframeSerialized[] keys;

        public static explicit operator AnimationCurveSerialized (AnimationCurve source) => source.ToSerializedFormat ();
        public static explicit operator AnimationCurve (AnimationCurveSerialized source) => source.ToRuntimeFormat ();
    }

    public static class GradientUtility
    {
        public static Gradient GetCopy (this Gradient source)
        {
            if (source == null)
                return null;
            
            var gradient = new Gradient ();
            
            var colorCount = source.colorKeys.Length;
            var colorKeys = new GradientColorKey[colorCount];
            for (int i = 0; i < colorCount; ++i)
            {
                var sourceKey = source.colorKeys[i];
                colorKeys[i] = new GradientColorKey
                {
                    color = sourceKey.color,
                    time = sourceKey.time
                };
            }

            var alphaCount = source.alphaKeys.Length;
            var alphaKeys = new GradientAlphaKey[alphaCount];
            for (int i = 0; i < source.alphaKeys.Length; ++i)
            {
                var sourceKey = source.alphaKeys[i];
                alphaKeys[i] = new GradientAlphaKey
                {
                    alpha = sourceKey.alpha,
                    time = sourceKey.time
                };
            }

            gradient.SetKeys (colorKeys, alphaKeys);
            return gradient;
        }
        
        public static Gradient GetCopyWithColor (this Gradient source, Color color, bool retainFirstColor = false)
        {
            if (source == null)
                return null;
            
            var gradient = new Gradient ();
            
            var colorCount = source.colorKeys.Length;
            var colorKeys = new GradientColorKey[colorCount];
            for (int i = 0; i < colorCount; ++i)
            {
                var sourceKey = source.colorKeys[i];
                colorKeys[i] = new GradientColorKey
                {
                    color = retainFirstColor && i == 0 ? sourceKey.color : color,
                    time = sourceKey.time
                };
            }

            var alphaCount = source.alphaKeys.Length;
            var alphaKeys = new GradientAlphaKey[alphaCount];
            for (int i = 0; i < source.alphaKeys.Length; ++i)
            {
                var sourceKey = source.alphaKeys[i];
                alphaKeys[i] = new GradientAlphaKey
                {
                    alpha = sourceKey.alpha,
                    time = sourceKey.time
                };
            }

            gradient.SetKeys (colorKeys, alphaKeys);
            return gradient;
        }
    }

    [Serializable]
    public struct KeyframeSerialized
    {
        // public float time;
        // public float value;
        // public float inTangent;
        // public float outTangent;
        // public float inWeight;
        // public float outWeight;
        // public int tangentMode;
        // public int weightedMode;
        
        [YamlMember (Alias = "tv_tg")]
        public Vector4 timeValueTangents;
        [YamlMember (Alias = "w")]
        public Vector2 weights;
        [YamlMember (Alias = "m")]
        public Vector2Int modes;

        public KeyframeSerialized (Keyframe source)
        {
            // time = source.time;
            // value = source.value;
            // inTangent = source.inTangent;
            // outTangent = source.outTangent;
            // inWeight = source.inWeight;
            // outWeight = source.outWeight;
            // tangentMode = source.tangentMode;
            // weightedMode = (int)source.weightedMode;

            timeValueTangents = new Vector4 (source.time, source.value, source.inTangent, source.outTangent);
            weights = new Vector2 (source.inWeight, source.outWeight);
            modes = new Vector2Int (source.tangentMode, (int)source.weightedMode);
        }
    }
    

    
    
    
    
    public static class AnimationCurveSerializedUtility
    {
        public static AnimationCurve ToRuntimeFormat (this AnimationCurveSerialized source)
        {
            if (source == null)
            {
                Debug.LogWarning ("Failed to deserialize animation curve due to serialized representation being null");
                return new AnimationCurve (new Keyframe (0f, 1f), new Keyframe (1f, 0f));
            }
            
            var keys = new Keyframe[source.keys.Length];
            for (int i = 0; i < source.keys.Length; ++i)
            {
                var keyframe = source.keys[i];

                float time = keyframe.timeValueTangents.x; // keyframe.time;
                float value = keyframe.timeValueTangents.y; // keyframe.value;
                float inTangent = keyframe.timeValueTangents.z; // keyframe.inTangent;
                float outTangent = keyframe.timeValueTangents.w; // keyframe.outTangent;
                float inWeight = keyframe.weights.x; // keyframe.inWeight;
                float outWeight = keyframe.weights.y; // keyframe.outWeight;

                keys[i] = new Keyframe 
                (
                    time,
                    value,
                    inTangent,
                    outTangent,
                    inWeight,
                    outWeight
                );
                
                keys[i].tangentMode = keyframe.modes.x; // keyframe.tangentMode;
                keys[i].weightedMode = (WeightedMode)keyframe.modes.y; // (WeightedMode)keyframe.weightedMode;
            }
            
            var result = new AnimationCurve (keys);
            result.postWrapMode = source.modePostWrap;
            result.preWrapMode = source.modePreWrap;
            
            return result;
        }
        
        public static AnimationCurveSerialized ToSerializedFormat (this AnimationCurve source)
        {
            if (source == null)
            {
                Debug.LogWarning ("Failed to create serialized animation curve due to source being null!");
                return null;
            }

            var result = new AnimationCurveSerialized ();
            result.modePostWrap = source.postWrapMode;
            result.modePreWrap = source.preWrapMode;
            result.keys = new KeyframeSerialized[source.keys.Length];
            
            for (int i = 0; i < source.keys.Length; ++i)
                result.keys[i] = new KeyframeSerialized (source.keys[i]);
            return result;
        }
    }
        
    [Serializable]
    public class GradientSerialized
    {
        public GradientMode mode;
        public GradientAlphaKey[] alphaKeys;
        public GradientColorKey[] colorKeys;

        public static explicit operator GradientSerialized (Gradient source) => source.ToSerializedFormat ();
        public static explicit operator Gradient (GradientSerialized source) => source.ToRuntimeFormat ();
    }
    
    public static class GradientSerializedUtility
    {
        public static Gradient ToRuntimeFormat (this GradientSerialized source)
        {
            if (source == null)
            {
                Debug.LogWarning ("Failed to deserialize gradient due to serialized representation being null");
                return new Gradient ();
            }
            
            var alphaKeys = new GradientAlphaKey[source.alphaKeys.Length];
            for (int i = 0; i < source.alphaKeys.Length; ++i)
                alphaKeys[i] = source.alphaKeys[i];

            var colorKeys = new GradientColorKey[source.colorKeys.Length];
            for (int i = 0; i < source.colorKeys.Length; ++i)
                colorKeys[i] = source.colorKeys[i];

            return new Gradient
            {
                mode = source.mode,
                alphaKeys = alphaKeys,
                colorKeys = colorKeys
            };
        }
        
        public static GradientSerialized ToSerializedFormat (this Gradient source)
        {
            if (source == null)
            {
                Debug.LogWarning ("Failed to create serialized gradient due to source being null");
                return null;
            }

            var alphaKeys = new GradientAlphaKey[source.alphaKeys.Length];
            for (int i = 0; i < source.alphaKeys.Length; ++i)
                alphaKeys[i] = source.alphaKeys[i];

            var colorKeys = new GradientColorKey[source.colorKeys.Length];
            for (int i = 0; i < source.colorKeys.Length; ++i)
                colorKeys[i] = source.colorKeys[i];
            
            return new GradientSerialized
            {
                mode = source.mode,
                alphaKeys = alphaKeys,
                colorKeys = colorKeys
            };
        }
    }

    public static class UtilityAnimationCurve
    {
        public static void ApplyToCurve (ref AnimationCurve target, bool linear, List<Keyframe> keyframes)
        {
            if (keyframes == null || keyframes.Count < 2)
                return;

            int keyframeCount = keyframes.Count;
            target = new AnimationCurve (keyframes.ToArray ());

            if (linear)
            {
                #if UNITY_EDITOR
                AnimationUtility.SetKeyRightTangentMode (target, 0, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyLeftTangentMode (target, keyframeCount - 1, AnimationUtility.TangentMode.Linear);
                
                if (keyframeCount > 2)
                {
                    for (int i = 1, iLimit = keyframeCount - 1; i < iLimit; ++i)
                    {
                        AnimationUtility.SetKeyBroken (target, i, true);
                        AnimationUtility.SetKeyRightTangentMode (target, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode (target, i, AnimationUtility.TangentMode.Linear);
                    }
                }
                #endif
            }
        }
    }

#if UNITY_EDITOR
    public static class AnimationCurvePresets
    {
        
        
        public static AnimationCurve fallback01
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 0f),
                    new Keyframe (1f, 1f), 
                });
                return result;
            }
        }
        
        public static AnimationCurve fallback01Linear
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 0f),
                    new Keyframe (1f, 1f), 
                });
                #if UNITY_EDITOR
                AnimationUtility.SetKeyRightTangentMode (result, 0, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyLeftTangentMode (result, 1, AnimationUtility.TangentMode.Linear);
                #else
                Debug.LogWarning ("0-1 fallback curve with linear tangents failed to fully initialize due to missing AnimationUtility API");
                #endif
                return result;
            }
        }
        
        public static AnimationCurve fallback10
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 1f),
                    new Keyframe (1f, 0f), 
                });
                return result;
            }
        }
        
        public static AnimationCurve fallback10Linear
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 1f),
                    new Keyframe (1f, 0f), 
                });
                #if UNITY_EDITOR
                AnimationUtility.SetKeyRightTangentMode (result, 0, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyLeftTangentMode (result, 1, AnimationUtility.TangentMode.Linear);
                #else
                Debug.LogWarning ("1-0 fallback curve with linear tangents failed to fully initialize due to missing AnimationUtility API");
                #endif
                return result;
            }
        }
        
        public static AnimationCurve fallback11
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 1f),
                    new Keyframe (1f, 1f), 
                });
                return result;
            }
        }
        
        public static AnimationCurve fallback00
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 0f),
                    new Keyframe (1f, 0f), 
                });
                return result;
            }
        }
        
        public static AnimationCurve fallback010
        {
            get
            {
                var result = new AnimationCurve (new []
                {
                    new Keyframe (0f, 0f),
                    new Keyframe (0.5f, 1f), 
                    new Keyframe (1f, 0f), 
                });
                return result;
            }
        }
    }
#endif
}

