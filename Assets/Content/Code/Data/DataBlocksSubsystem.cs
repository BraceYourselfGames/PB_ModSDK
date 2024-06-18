
using System;
using System.Collections.Generic;
using PhantomBrigade.Game;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class PartCustomFlagKeys
    {
        public const string MeleeDamageDispersed = "melee_damage_dispersed";
        public const string MeleeDamageSplash = "melee_damage_splash";
        public const string MeleeImpactCrash = "melee_impact_crash";
        public const string DamageDispersed = "damage_dispersed";
        public const string ReactionLosCheck = "reaction_los_check";
        public const string VisualBaseHidden = "vis_base_hidden";
        public const string SalvageExempt = "salvage_exempt";

        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartCustomFlagKeys));
            return keys;
        }
    }

    public static class PartCustomFloatKeys
    {
        /*
        public const string MeleeDurationStrike = "melee_duration_strike";
        public const string MeleeDurationDashOut = "melee_duration_dash_out";
        public const string MeleeDurationDashAlign = "melee_duration_dash_align";
        public const string MeleeDurationDashFull = "melee_duration_dash_full";
        
        public const string MeleeTimeImpact = "melee_time_impact";
        public const string MeleeDistanceIn = "melee_distance_in";
        public const string MeleeDistanceOut = "melee_distance_out";
        public const string MeleeDistanceOffset = "melee_distance_offset";

        public const string MeleeBladeLength = "melee_blade_length";
        public const string DistanceCancelThreshold = "distance_cancel_threshold";
        */
        
        public const string ReactionAngleThreshold = "reaction_angle_threshold";
        public const string ReactionRotationSpeed = "reaction_rotation_speed";

        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartCustomFloatKeys));
            return keys;
        }
    }
    
    public static class PartCustomVectorKeys
    {
        public const string MeleeCollisionSize = "melee_hit_size";
        public const string MeleeCollisionOffset = "melee_hit_offset";

        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartCustomVectorKeys));
            return keys;
        }
    }

    public static class PartCustomIntKeys
    {
        public const string MeleeAnimationVariant = "melee_animation_variant";
        
        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartCustomIntKeys));
            return keys;
        }
    }

    public static class PartCustomStringKeys
    {
        public const string MeleeTrajectory = "melee_trajectory";

        private static List<string> keys;
        public static List<string> GetKeys ()
        {
            if (keys == null) keys = FieldReflectionUtility.GetConstantStringFieldValues (typeof (PartCustomStringKeys));
            return keys;
        }
    }
    
    [Serializable] 
    public class DataBlockPartCustom
    {
        [DropdownReference]
        [ValueDropdown ("@PartCustomFlagKeys.GetKeys ()")]
        public HashSet<string> flags;
        
        [DropdownReference]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.PartCustomInt)]
        public SortedDictionary<string, int> ints;
        
        [DropdownReference]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.PartCustomFloat)]
        public SortedDictionary<string, float> floats;
        
        [DropdownReference]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.PartCustomVector)]
        public SortedDictionary<string, Vector3> vectors;
        
        [DropdownReference]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.PartCustomString)]
        public SortedDictionary<string, string> strings;

        
        
        public bool IsFlagPresent (string key)
        {
            var found = !string.IsNullOrEmpty (key) && flags != null && flags.Contains (key);
            return found;
        }

        public bool TryGetInt (string key, out int result)
        {
            var found = !string.IsNullOrEmpty (key) && ints != null && ints.ContainsKey (key);
            result = found ? ints[key] : default;
            return found;
        }
        
        public bool TryGetFloat (string key, out float result)
        {
            var found = !string.IsNullOrEmpty (key) && floats != null && floats.ContainsKey (key);
            result = found ? floats[key] : default;
            return found;
        }
        
        public bool TryGetVector (string key, out Vector3 result)
        {
            var found = !string.IsNullOrEmpty (key) && vectors != null && vectors.ContainsKey (key);
            result = found ? vectors[key] : default;
            return found;
        }
        
        public bool TryGetString (string key, out string result)
        {
            var found = !string.IsNullOrEmpty (key) && strings != null && strings.ContainsKey (key);
            result = found ? strings[key] : default;
            return found;
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPartCustom () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockSubsystemActivation_V2
    {
        [DropdownReference (true)]
        public DataBlockSubsystemActivationVisual visual;
        
        [DropdownReference (true)]
        public DataBlockSubsystemActivationAudio audio;
        
        [DropdownReference (true)]
        public DataBlockSubsystemActivationLight light;
        
        [DropdownReference (true)]
        public DataBlockSubsystemActivationRecoil recoil;
        
        [DropdownReference (true)]
        public DataBlockSubsystemActivationTiming timing;
        
        [DropdownReference (true)]
        public DataBlockSubsystemActivationHitReaction hitReaction;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockSubsystemActivation_V2 () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockTransformOverride
    {
        public bool root;
        public Vector3 position;
        public Vector3 rotation;
    }
    
    public class DataBlockSubsystemActivationVisual
    {
        [DropdownReference (true)]
        public DataBlockAssetFactionBased local = new DataBlockAssetFactionBased ();
        
        [DropdownReference (true)]
        public DataBlockAssetFactionBased root;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        public string localSocketOverride;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        public string localHardpointOverride;

        [DropdownReference (true)]
        public DataBlockTransformOverride localTransformOverride;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockSubsystemActivationVisual () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockSubsystemActivationLight
    {
        public bool shared = true;
        
        [ShowIf ("shared")]
        [ValueDropdown("GetLightKeys"), InlineButtonClear]
        public string key = "default";
        
        [HideIf ("shared")]
        public DataContainerEquipmentLight custom;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetLightKeys () => DataMultiLinkerEquipmentLight.data.Keys;
        #endif
    }
    
    public class DataBlockSubsystemActivationAudio
    {
        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        [PropertyTooltip ("Audio event posted on first instance of activation (like first bullet)")]
        public string onActivationFirst = string.Empty;
        
        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        [PropertyTooltip ("Audio event posted on instance of activation that's not first nor last (like second bullet)")]
        public string onActivationMid = string.Empty;
        
        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        [PropertyTooltip ("Audio event posted on last instance of activation (like last bullet)")]
        public string onActivationLast = string.Empty;

        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        [PropertyTooltip ("Audio event posted on start of activation window (like start of attack action)")]
        public string onActivationStart = string.Empty;
        
        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        [PropertyTooltip ("Audio event posted on end of activation window (like end of attack action)")]
        public string onActivationEnd = string.Empty;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetAudioKeys () => AudioEvents.GetKeys ();
        #endif
    }

    public class DataBlockSubsystemActivationRecoil
    {
        [ValueDropdown ("GetKeysRecoil"), InlineButtonClear]
        public string key;

        #if UNITY_EDITOR
        private IEnumerable<string> GetKeysRecoil () => DataMultiLinkerEquipmentRecoil.data.Keys;
        #endif
    }
    
    public class DataBlockSubsystemActivationTiming
    {
        public float timeFrom = 0f;
        public float timeTo = 1f;
        public float exponent = 1f;
    }

    public class DataBlockSubsystemActivationHitReaction
    {
        public float force = 1f;
    }
    
    



    
    public class DataBlockSubsystemProjectileDistribution
    {
        [PropertyRange (1, 10), PropertyTooltip ("Number of random samples controlling the distribution curve. Check anydice.com to visualize!")]
        public int samples = 1;
    }
    
    public class DataBlockSubsystemProjectileRange
    {
        public bool deactivateBeforeRange;
        public bool destroyAfterRange;
        
        [ShowIf ("destroyAfterRange")]
        [PropertyTooltip ("Some projectiles don't travel in a straight line so deactivating them exactly at range is problematic. Set it to e.g. 1.1-1.25x for missiles traveling in an arc.")]
        [LabelText ("Range Multiplier")]
        [PropertyRange (1f, 2f)]
        public float destroyAfterRangeMultiplier = 1f;
    }
    
    public class DataBlockSubsystemProjectileFragmentation
    {
        [PropertyRange (1, 50)]
        public int count;

        // public DataBlockSubsystemProjectileFragmentationDelayed delayed;
    }

    public class DataBlockRotationToTarget
    {
        [PropertyRange (1f, 180f)]
        public float rotationLimit = 90f;
    }

    public class DataBlockSubsystemProjectileFragmentationDelayed
    {
        [PropertyRange (1, 50)]
        public int count = 2;
        
        [PropertyRange (0.01f, 5f)]
        public float time = 0.5f;
        
        [PropertyRange (0f, 270f)]
        public float angle = 30;
        
        [PropertyRange (0f, 270f)]
        public float angleMin = 0;
        
        [PropertyRange (0f, 1f)]
        public float scatterUniformity = 0f;
        
        [PropertyRange (0, 1000)]
        public float addedSpeed = 0;

        public int generationLimit = 1;

        public bool lifetimeReset = false;
        
        public bool damageSplit = true;
        
        [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys")]
        public string fxKey = "fx_projectile_pop";

        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        [InlineButtonClear]
        public string subsystemOverride;

        [HideIf ("@string.IsNullOrEmpty (subsystemOverride)")]
        public bool subsystemOverridesDamage = false;
        
        public DataBlockScenarioSubcheckUnitFilter targetUnitFiltered;
        
        [InlineButtonClear]
        public DataBlockRotationToTarget rotationToTarget;
    }
    
    public class DataBlockSubsystemProjectileVisual
    {
        [DropdownReference (true)]
        public DataBlockAssetProjectile body = new DataBlockAssetProjectile ();
        
        [DropdownReference (true)]
        public DataBlockAssetFactionBased impact;
        
        [DropdownReference (true)]
        public DataBlockAssetFactionBased deactivation;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockSubsystemProjectileVisual () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockAssetFactionBased
    {
        [ValueDropdown("GetAssetKeys")]
        public string key;
        
        [DropdownReference]
        [ValueDropdown("GetAssetKeys")]
        public string keyEnemy;

        [OnValueChanged ("OnScaleChanged")]
        public Vector3 scale = new Vector3 (1, 1, 1);
        
        [DropdownReference (true)]
        public DataBlockFloat01 hueOffset;
        
        [DropdownReference (true)]
        public DataBlockFloat01 hueOffsetEnemy;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetAssetKeys () => DataMultiLinkerAssetPools.data.Keys;
        private void OnScaleChanged ()
        {
            scale = new Vector3
            (
                Mathf.Clamp (scale.x, 0.5f, 10f),
                Mathf.Clamp (scale.y, 0.5f, 10f),
                Mathf.Clamp (scale.z, 0.5f, 10f)
            );
        }
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockAssetFactionBased () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        #endif
    }
    
    public class DataBlockAssetProjectile
    {
        [ValueDropdown("GetAssetKeys")]
        public string key;
        
        [DropdownReference]
        [ValueDropdown("GetAssetKeys")]
        public string keyEnemy;

        [OnValueChanged ("OnScaleChanged")]
        public Vector3 scale = new Vector3 (1, 1, 1);

        [DropdownReference (true)]
        public DataBlockColorInterpolated colorOverride;
        
        [DropdownReference (true)]
        public DataBlockColorInterpolated colorOverrideEnemy;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetAssetKeys () => DataMultiLinkerAssetPools.data.Keys;
        private void OnScaleChanged ()
        {
            scale = new Vector3
            (
                Mathf.Clamp (scale.x, 0.5f, 10f),
                Mathf.Clamp (scale.y, 0.5f, 10f),
                Mathf.Clamp (scale.z, 0.5f, 10f)
            );
        }
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockAssetProjectile () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        #endif
    }
    
    public class DataBlockAsset
    {
        [PropertyOrder (-10)]
        [ValueDropdown("GetAssetKeys")]
        public string key = string.Empty;
        
        [HideInInspector]
        [OnValueChanged ("OnScaleChanged")]
        [InlineButton ("@SetScaleUniform (1)", "1")]
        public Vector3 scale = new Vector3 (1, 1, 1);
        
        [ShowIf ("IsScaleXYZVisible")]
        [YamlIgnore, ShowInInspector, PropertyOrder (0)]
        [LabelText ("Scale (XYZ)")]
        public Vector3 scaleXYZ
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }
        
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetAssetKeys () => DataMultiLinkerAssetPools.data.Keys;
        
        private void OnScaleChanged ()
        {
            /*
            scale = new Vector3
            (
                Mathf.Clamp (scale.x, 0.05f, 25f),
                Mathf.Clamp (scale.y, 0.05f, 25f),
                Mathf.Clamp (scale.z, 0.05f, 25f)
            );
            */
        }

        private void SetScaleUniform (float value)
        {
            scale = Vector3.one * value;
            OnScaleChanged ();
        }

        protected virtual bool IsScaleXYZVisible () => true;

        #endif
    }
    
    public class DataBlockProjectileAssetSize
    {
        [OnValueChanged ("OnScaleChanged")]
        public Vector3 scale = new Vector3 (1, 1, 1);
        
        #if UNITY_EDITOR
        private void OnScaleChanged ()
        {
            scale = new Vector3
            (
                Mathf.Clamp (scale.x, 0.5f, 5f),
                Mathf.Clamp (scale.y, 0.5f, 5f),
                Mathf.Clamp (scale.z, 0.5f, 5f)
            );
        }
        #endif
    }
    
    
    
    public class DataBlockSubsystemProjectileAudio
    {
        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        public string onImpact;
        
        [ValueDropdown("GetAudioKeys"), InlineButtonClear]
        public string onDeactivation;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetAudioKeys () => AudioEvents.GetKeys ();
        #endif
    }
    
    public class DataBlockSubsystemStatusBuildup
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key;

        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys"), InlineButtonClear]
        public string statMultiplier;
                
        [PropertyRange (0f, 1f)]
        public float amount = 1f;
    }
    
    public class DataBlockSubsystemProjectile_V2
    {
        [Tooltip ("What range of projectiles in a volley should receive guidance visualizations (gizmos showing all the control outputs, velocity compensation, predicted target position etc.). Set to [0,0] to only show first projectile, to [0, 2] to show first 3 projectiles, and so on.")]
        [LabelText ("Debug Projectile Indexes")]
        public Vector2Int debugIndexRange;

        [DropdownReference (true)]
        public DataBlockSubsystemProjectileVisual visual;
        
        [DropdownReference (true)]
        public DataBlockSubsystemProjectileAudio audio;

        [DropdownReference (true)]
        public DataBlockFloat damageDelay;
        
        [DropdownReference (true)]
        public DataBlockSubsystemProjectileDistribution distribution;
        
        [DropdownReference (true)]
        public DataBlockSubsystemProjectileRange range;
        
        [DropdownReference (true)]
        public DataBlockSubsystemProjectileFragmentation fragmentation;
        
        [DropdownReference (true)]
        public DataBlockSubsystemProjectileFragmentationDelayed fragmentationDelayed;

        [OnValueChanged ("TryUpdatingEffectiveness", true)]
        [DropdownReference (true)]
        public DataBlockProjectileDamageFalloff falloff;
        
        [OnValueChanged ("TryUpdatingEffectiveness", true)]
        [DropdownReference (true)]
        public DataBlockProjectileDamageFalloffGlobal falloffGlobal;

        [DropdownReference (true)]
        public DataBlockProjectileAnimationFade animationFade;

        [DropdownReference (true)]
        public DataBlockProjectileBallistics ballistics;
        
        [DropdownReference (true)]
        public DataBlockProjectileProximityFuse fuseProximity; 
        
        [DropdownReference (true)]
        public DataBlockProjectileHitResponse hitResponse; 
        
        [DropdownReference (true)]
        public DataBlockProjectileSplashDamage splashDamage;
        
        [DropdownReference (true)]
        public DataBlockProjectileSplashImpact splashImpact;
        
        [DropdownReference (true)]
        public DataBlockGuidanceData guidanceData;
        
        [DropdownReference (true)]
        public DataBlockProjectileAudioGuidance guidanceAudio;
        
        [DropdownReference (true)]
        public DataBlockSubsystemStatusBuildup statusBuildup;
        
        [DropdownReference (true)]
        public DataBlockUITrajectory uiTrajectory;
        
        [DropdownReference (true)]
        public DataBlockFloat uiSpeedAverage;
        
        [PropertyTooltip ("This block alters the maximum permitted cross section of a target in coverage calculation, increasing it by 1 meter for every 0.1 reduction. Default is 1.0, which keeps the calculated target at 2.5m. Value of 0.8 expands it to 4.5m, etc. Useful for decreasing the weight of the coverage calculation in the optimum range band.")]
        [OnValueChanged ("TryUpdatingEffectiveness", true)]
        [DropdownReference (true)]
        [LabelText ("UI - Coverage Weight")]
        public DataBlockFloat01 uiCoverageWeight;
        
        [PropertyTooltip ("This block alters what % of effectiveness is tested by the game to determine the optimum band. The default is 0.8. Setting this to 0.5 will make the game display the optimum band around 50% zone instead, etc.")]
        [OnValueChanged ("TryUpdatingEffectiveness", true)]
        [DropdownReference (true)]
        [LabelText ("UI - Optimum Threshold")]
        public DataBlockFloat01 uiOptimumThreshold;

        [YamlIgnore, HideInInspector]
        public DataContainerSubsystem parentSubsystem;

        public void OnBeforeSerialization ()
        {
            if (falloff != null)
                falloff.OnBeforeSerialization ();
            
            if (uiTrajectory != null)
                uiTrajectory.OnBeforeSerialization ();

            if (guidanceData != null)
            {
                var gp = guidanceData;
                {
                    if (gp.inputTargetPointLateral != null && gp.inputTargetPointLateral is DataBlockGuidanceInputCurve c0)
                        c0.OnBeforeSerialization ();
                    
                    if (gp.inputTargetHeight != null && gp.inputTargetHeight is DataBlockGuidanceInputCurve c1)
                        c1.OnBeforeSerialization ();

                    if (gp.inputTargetBlend != null && gp.inputTargetBlend is DataBlockGuidanceInputCurve c2)
                        c2.OnBeforeSerialization ();

                    if (gp.inputTargetUpdate != null && gp.inputTargetUpdate is DataBlockGuidanceInputCurve c3)
                        c3.OnBeforeSerialization ();
                    
                    if (gp.inputTargetOffset != null && gp.inputTargetOffset is DataBlockGuidanceInputCurve c4)
                        c4.OnBeforeSerialization ();

                    if (gp.inputSteering != null && gp.inputSteering is DataBlockGuidanceInputCurve c5)
                        c5.OnBeforeSerialization ();

                    if (gp.inputThrottle != null && gp.inputThrottle is DataBlockGuidanceInputCurve c6)
                        c6.OnBeforeSerialization ();
                    
                    if (gp.inputThrottleDotPower != null && gp.inputThrottleDotPower is DataBlockGuidanceInputCurve c7)
                        c7.OnBeforeSerialization ();
                }
            }
        }

        public void OnAfterDeserialization (DataContainerSubsystem parentSubsystem)
        {
            this.parentSubsystem = parentSubsystem;
            
            if (falloff != null)
                falloff.OnAfterDeserialization ();
            
            if (uiTrajectory != null)
                uiTrajectory.OnAfterDeserialization ();

            if (guidanceData != null)
            {
                var gp = guidanceData;
                {
                    if (gp.inputTargetPointLateral != null && gp.inputTargetPointLateral is DataBlockGuidanceInputCurve c0)
                        c0.OnAfterDeserialization ();
                    
                    if (gp.inputTargetHeight != null && gp.inputTargetHeight is DataBlockGuidanceInputCurve c1)
                        c1.OnAfterDeserialization ();

                    if (gp.inputTargetBlend != null && gp.inputTargetBlend is DataBlockGuidanceInputCurve c2)
                        c2.OnAfterDeserialization ();

                    if (gp.inputTargetUpdate != null && gp.inputTargetUpdate is DataBlockGuidanceInputCurve c3)
                        c3.OnAfterDeserialization ();
                    
                    if (gp.inputTargetOffset != null && gp.inputTargetOffset is DataBlockGuidanceInputCurve c4)
                        c4.OnAfterDeserialization ();

                    if (gp.inputSteering != null && gp.inputSteering is DataBlockGuidanceInputCurve c5)
                        c5.OnAfterDeserialization ();

                    if (gp.inputThrottle != null && gp.inputThrottle is DataBlockGuidanceInputCurve c6)
                        c6.OnAfterDeserialization ();
                    
                    if (gp.inputThrottleDotPower != null && gp.inputThrottleDotPower is DataBlockGuidanceInputCurve c7)
                        c7.OnAfterDeserialization ();
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockSubsystemProjectile_V2 () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private void TryUpdatingEffectiveness ()
        {
            #if !PB_MODSDK
            DataHelperStats.RefreshPartEffectivenessFromProjectileConfig (falloff, falloffGlobal);
            #endif
        }
        
        #endif
        #endregion
    }
    


    
    public class DataBlockUITrajectory
    {
        public float scale = 20f;
        public float fixedAreaDistance = 30f;
        
        [PropertyRange (0f, 1f)]
        public float fixedAreaTime = 0f;

        // Arrival times for 50m, 100m, 150m
        public Vector3 arrivalTimes = new Vector3 (1f, 2f, 3f);
        
        [HideLabel]
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer curve = new AnimationCurveContainer ();
        
        [YamlMember (Alias = "curve"), HideInInspector] 
        public AnimationCurveSerialized curveSerialized;
        
        public void OnBeforeSerialization ()
        {
            if (curve != null)
                curveSerialized = (AnimationCurveSerialized) curve.curve;
        }

        public void OnAfterDeserialization ()
        {
            if (curveSerialized != null)
                curve = new AnimationCurveContainer ((AnimationCurve) curveSerialized);
        }

        public float GetArrivalTime (float distance)
        {
            if (distance <= 0f)
                return 0f;
            
            if (distance < 50f)
            {
                var interpolant = distance / 50f;
                return Mathf.Lerp (0f, arrivalTimes.x, interpolant);
            }
            
            if (distance < 100f)
            {
                var interpolant = (distance - 50f) / 50f;
                return Mathf.Lerp (arrivalTimes.x, arrivalTimes.y, interpolant);
            }
            
            if (distance < 150f)
            {
                var interpolant = (distance - 100f) / 50f;
                return Mathf.Lerp (arrivalTimes.y, arrivalTimes.z, interpolant);
            }

            var distancePastRecords = distance - 150f;
            var timeInLastInterval = arrivalTimes.z - arrivalTimes.y;
            if (timeInLastInterval < 0f)
                return arrivalTimes.z;

            var speedMultiplier = 1f / timeInLastInterval;
            var speed = 50f / speedMultiplier;
            return arrivalTimes.z + distancePastRecords / speed;
        }

        public Vector3 GetProjectedPosition (float projProgress, float distanceToTarget, Vector3 firingPos, Vector3 targetPos)
        {
            var projProgressDistance = projProgress * distanceToTarget;
            
            float fixedAreaDistance = Mathf.Max (1f, this.fixedAreaDistance);
            float fixedAreaTime = Mathf.Clamp01 (this.fixedAreaTime);
            float distanceWithoutFixedArea = distanceToTarget - fixedAreaDistance;
            
            // Interpolant for part of the curve meant to have fixed length (e.g. projectile ascent phase)
            var interpolantFixed = projProgressDistance / fixedAreaDistance;
            interpolantFixed = Mathf.Clamp01 (interpolantFixed.RemapToRange (0f, 1f, 0f, fixedAreaTime));
                                
            // Interpolant for remainder of the curve evenly scaled over full distance
            var interpolantRemainder = (projProgressDistance - fixedAreaDistance) / distanceWithoutFixedArea;
            interpolantRemainder = Mathf.Clamp01 (interpolantRemainder.RemapToRange (0f, 1f, fixedAreaTime, 1f));
                        
            // Simple switch between two interpolants. TODO: Make it a smooth blend
            var interpolantFinal = projProgressDistance < fixedAreaDistance ? interpolantFixed : interpolantRemainder;

            // Base position
            var posInterpolated = Vector3.Lerp (firingPos, targetPos, interpolantFinal);
                        
            // Height sample
            var sample = curve.curve.Evaluate (interpolantFinal);
            var sampleScaled = sample * scale;

            // Final position; distance accumulation for next step
            var projPos = new Vector3 (posInterpolated.x, posInterpolated.y + sampleScaled, posInterpolated.z);
            return projPos;
        }
    }

    public class DataBlockProjectileDamageFalloff
    {
        public bool animated = true;
    
        [HideLabel]
        [YamlIgnore, HideReferenceObjectPicker]
        public AnimationCurveContainer curve = new AnimationCurveContainer ();
        
        [YamlMember (Alias = "damageFalloff"), HideInInspector] 
        public AnimationCurveSerialized curveSerialized;
        
        public void OnBeforeSerialization ()
        {
            if (curve != null)
                curveSerialized = (AnimationCurveSerialized) curve.curve;
        }

        public void OnAfterDeserialization ()
        {
            if (curveSerialized != null)
                curve = new AnimationCurveContainer ((AnimationCurve) curveSerialized);
        }
    }
    
    public class DataBlockProjectileDamageFalloffGlobal
    {
        public bool rangeStartFromStat = false;
        
        [LabelText ("@rangeStartFromStat ? \"Offset From Max Range\" : \"Start Range\"")]
        public float rangeStart = 50f;
        
        [PropertyRange (1f, 100f)]
        public float gradientSize = 50f;

        [PropertyRange (0.1f, 8f)]
        public float exponent = 1f;

        public float GetDamageMultiplier (float rangeMaxStat, float distance)
        {
            float falloffGlobalInterval = Mathf.Max (1f, gradientSize);
            float falloffGlobalDistanceStart = rangeStartFromStat ? rangeMaxStat + rangeStart : rangeStart;
            
            // Shift distance to local space of global falloff (0 if you're at starting distance), get multiplier for damage
            float distShiftedToStart = distance - falloffGlobalDistanceStart;
            float distFactor = Mathf.Clamp01 (distShiftedToStart / falloffGlobalInterval);
            
            if (!exponent.RoughlyEqual (1f))
                distFactor = Mathf.Pow (distFactor, exponent);

            float multiplier = 1f - distFactor;
            return multiplier;
        }
    }

    public class DataBlockProjectileBallistics
    {
        public float lateralSpeed = 10f;
        public float arcPeak = 10f;
    }

    public class DataBlockProjectileHitResponse
    {
        public bool unitHitDestruction = true;
        public bool propHitDestruction = false;
        public bool envHitDestruction = true;
        public bool envHitTrigger = true;
        
        [PropertyRange (0f, 1f)]
        public float envHitAlignmentAmount = 0f;
    }
    
    public class DataBlockProjectileProximityFuse
    {
        [Tooltip ("How close a target has to be for fuse to detonate. Configure this considering splash damage range - it's not ideal to detonate exactly at range, as that would inflict no damage")]
        public float distance = 3f;
        public bool targetExclusive = false;
        public bool inertBeforeHit = false;
        public bool triggerWithoutEntity = false;
    }

    public class DataBlockProjectileSplashDamage
    {
        [Tooltip ("How damage is applied within splash range. 1 is linear falloff - further out, less damage. High values approach X-Com style flat damage anywhere in the volume. ~0.45 is ultra-realistic square falloff. 2 is good default.")]
        public float exponent = 2f;
        
        [ValueDropdown("GetAssetKeys")]
        [InlineButton ("@fxDetonation = null", "-")]
        [LabelText ("Detonation FX")]
        public string fxDetonation;
        public float fxDetonationScale = 1f;
        
        [ValueDropdown("GetAssetKeys")]
        [InlineButton ("@fxArea = null", "-")]
        [LabelText ("Area FX")]
        public string fxArea;
        
        [ValueDropdown("GetAssetKeys")]
        [InlineButton ("@fxHit = null", "-")]
        [LabelText ("Hit FX")]
        public string fxHit;
        
        private IEnumerable<string> GetAssetKeys () => DataMultiLinkerAssetPools.data.Keys;
    }
    
    public class DataBlockProjectileSplashImpact
    {
        [Tooltip ("How damage is applied within splash range. 1 is linear falloff - further out, less damage. High values approach X-Com style flat damage anywhere in the volume. ~0.45 is ultra-realistic square falloff. 2 is good default.")]
        public float exponent = 2f;
        public bool triggerOnDamage;
    }
    
    public class DataBlockProjectileAudioGuidance
    {       
        [ValueDropdown ("GetAudioKeys")]
        [InlineButton ("@soundOnLaunch = null", "-")]
        public string soundOnLaunch;
        
        [ValueDropdown ("GetAudioKeys")]
        [InlineButton ("@soundOnPriming = null", "-")]
        public string soundOnPriming;
        
        [ValueDropdown ("GetAudioKeys")]
        [InlineButton ("@soundOnExpiration = null", "-")]
        public string soundOnExpiration;
        
        public Vector2 syncProximityRange = new Vector2 (12f, 24f);
        
        [PropertyRange (1f, 4f)]
        public float syncPowerAngularVelocity = 1f;
        
        [PropertyRange (0f, 1f)]
        public float syncSize = 1f;
        
        
        
        private IEnumerable<string> GetAudioKeys () => AudioEvents.GetKeys ();
    }
    
    public class DataBlockProjectileAnimation
    {
        [LabelText ("Reveal Duration")]
        public float durationReveal = 0.25f;
        
        [LabelText ("Finish Duration")]
        public float durationFinish = 0.25f;
    }
    
    public class DataBlockProjectileAnimationFade
    {
        public float speed = 4f;
        public float distance = 3f;
    }



    public interface IDataBlockGuidanceInput
    {
        float Evaluate (float timeNormalized);
    }
    
    public class DataBlockGuidanceInputConstant : IDataBlockGuidanceInput
    {
        [PropertyRange (0f, 1f)]
        public float value = 0f;

        public float Evaluate (float timeNormalized)
        {
            return value;
        }
    }
    
    public class DataBlockGuidanceInputLinear : IDataBlockGuidanceInput
    {
        [HorizontalGroup ("A"), LabelText ("Value (From/To)")]
        [MinValue (0f), MaxValue (1f)]
        public float valueFrom = 0f;

        [HorizontalGroup ("A", 0.3f), HideLabel]
        [MinValue (0f), MaxValue (1f)]
        public float valueTo = 1f;
        
        [MinMaxSlider (0f, 1f)]
        public Vector2 timeRange = new Vector2 (0f, 1f);

        public float Evaluate (float timeNormalized)
        {
            if (timeNormalized <= timeRange.x)
                return valueFrom;

            if (timeNormalized >= timeRange.y)
                return valueTo;

            var interpolant = timeNormalized.RemapTo01 (timeRange.x, timeRange.y);
            return Mathf.Lerp (valueFrom, valueTo, interpolant);
        }
    }
    
    public class DataBlockGuidanceInputCurve : IDataBlockGuidanceInput
    {
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker] 
        public AnimationCurveContainer curve = new AnimationCurveContainer ();
        
        [YamlMember (Alias = "curve"), HideInInspector] 
        public AnimationCurveSerialized curveSerialized;

        public float Evaluate (float timeNormalized)
        {
            return curve.GetCurveSample (timeNormalized);
        }
        
        public void OnBeforeSerialization ()
        {
            if (curve?.curve != null)
                curveSerialized = (AnimationCurveSerialized) curve.curve;
        }

        public void OnAfterDeserialization ()
        {
            if (curveSerialized != null)
                curve = new AnimationCurveContainer ((AnimationCurve) curveSerialized);
        }
    }
    
    public class DataBlockGuidanceVelocityCompensation
    {
        [Tooltip ("Range of distances (start/end) over which velocity compensation fades in. For instance, if set to [20,60], compensation would be absent further than 50 meters away and would have full force closer than 20 meters away.")]
        public Vector2 rangeLimit = new Vector2 (20, 60);
        
        [Tooltip ("How far into the future the compensation is allowed to predict. Prevents unusable results when approach speed is fairly low.")]
        [PropertyRange (0, 1)]
        public float timeLimit = 1;

        [Tooltip ("The number of iterations used by velocity compensation. For most projectiles, 1 iteration should be enough, but for very precise cases like smart bullets, more iteration might be useful. The gist is that compensation involves calculating the time it takes to reach target and projecting target position into the future using that time and target velocity. Changing target position that way changes distance to target, which changes time to reach target, which means compensation can be refined. This can be repeated over and over.")]
        [PropertyRange (0, 3)]
        public int iterations = 1;

        [Tooltip ("Compensation for target velocity. Helps makes the projectile point not at the target, but at a place where target would be by the time projectile arrives. I recommend leaving this at 1, as it is quite precise and almost never aligns with projectile approach, which means it's unlikely to ever cause panic on final approach like ~1 values for self velocity compensation.")]
        [LabelText ("Vel. Comp. Multiplier (Target)")]
        [PropertyRange (0f, 1f)]
        public float targetVelocityCompensation = 1f;
        
        [Tooltip ("Compensation for velocity of the guided projectile. Helps makes the projectile point not at the target, but at a place where target would be by the time projectile arrives. It's quite important to compensate for it, as projectiles typically travel at velocities far exceeding the units - missing this can make the projectile blind to the fact that a tank is moving well past it. Avoid setting it to 1 to prevent projectiles from panicking close to targets, though - or projectiles might panic and think that they will miss the target on very last part of the approach. Values around 0.8 seem to be a happy medium.")]
        [LabelText ("Vel. Comp. Multiplier (Self)")]
        [PropertyRange (0f, 1f)]
        public float selfVelocityCompensation = 1f;
        
        [Tooltip ("Work in progress feature, leave at 0 at all times for the time being. Will eventually project self velocity vector onto a plane relevant for steering to prevent the problem of predicted position running behind the projectile")]
        [LabelText ("Vel. Comp. Projection (Self)")]
        [PropertyRange (0f, 1f)]
        public float selfVelocityProjection = 0f;
    }
    
    public class DataBlockGuidanceDirectionCheck
    {
        [LabelText ("View Check Distance")]
        public float distance = 50f;
        
        [LabelText ("View Dot Threshold")]
        [PropertyRange (-1f, 1f)]
        public float dotThreshold = -1f;
    }

    public enum LateralOffsetMode
    {
        Left,
        Right,
        Interleaved
    }

    [LabelWidth (220f)]
    public class DataBlockGuidanceData
    {
        [FoldoutGroup (fgRigidbody, false)]
        [Tooltip ("Best to keep this around 1 for maximum physics stability, but it can be useful to reduce it if inertia is too much of a problem. High drag and low mass can be an interesting underwater torpedo-like combination.")]
        public float rigidbodyMass = 1f;
        
        [FoldoutGroup (fgRigidbody)]
        [Tooltip ("How fast the projectile is bleeding energy when aligned to velocity vector. Maximum speed of a projectile during sustained burn depends on a combination of drag and acceleration force. This is similar to any falling body reaching terminal velocity - maximum speed where drag and gravity balance each other out.")]
        [PropertyRange (0f, 10f)]
        public float rigidbodyDrag = 0.5f;
        
        [FoldoutGroup (fgRigidbody)]
        [Tooltip ("How fast the projectile is bleeding energy when perpendicular to velocity vector. Maximum speed of a projectile during sustained burn depends on a combination of drag and acceleration force. This is similar to any falling body reaching terminal velocity - maximum speed where drag and gravity balance each other out.")]
        [PropertyRange (0f, 10f)]
        public float rigidbodyDriftDrag = 0.5f;
        
        [FoldoutGroup (fgRigidbody)]
        [Tooltip ("How fast the projectile is bleeding angular velocity. Useful if you're seeing projectiles struggling to stop turns they initiate.")]
        [PropertyRange (0f, 10f)]
        public float rigidbodyAngularDrag = 1.5f;
        
        [FoldoutGroup (fgRigidbody)]
        [Tooltip ("How fast the misaligned velocity is converted to aligned velocity.")]
        [PropertyRange (0f, 10f)]
        public float rigidbodyLift = 0f;
        
        [FoldoutGroup (fgRigidbody)]
        [PropertyTooltip ("Maximum speed given mass and drag")]
        [ShowInInspector]
        public float rigidbodySpeedLimit => GetSpeedLimit ();
        
        
        [FoldoutGroup (fgForces, false)]
        [Tooltip ("How much force can be used to yaw the projectile (horizontal rotation)")]
        [LabelText ("Yaw Force")]
        public float driverSteeringForce = 250f;
        
        [FoldoutGroup (fgForces)]
        [Tooltip ("How much force can be used to pitch the projectile (vertical rotation)")]
        [LabelText ("Pitch Force")]
        public float driverPitchForce = 200f;
        
        [FoldoutGroup (fgForces)]
        [Tooltip ("How much force can be used to push the projectile forward (engine thrust)")]
        [LabelText ("Thrust Force")]
        public float driverAccelerationForce = 100f;
        
        [FoldoutGroup (fgForces)]
        [Tooltip ("How low can engine be throttled (setting it above 0 can prevent projectiles from turning on the spot)")]
        [LabelText ("Thrust Minimum")]
        [PropertyRange (0.1f, 1f)]
        public float driverAccelerationMin = 0.1f;

        [FoldoutGroup (fgInputs, false)]
        [ShowIf ("@inputTargetHeight != null || inputTargetBlend != null || inputTargetUpdate != null || inputSteering != null || inputThrottle != null")]
        [LabelText ("Progress From Distance")]
        public bool inputProgressFromTarget = true;
        
        [FoldoutGroup (fgInputs)]
        [ShowIf ("@inputTargetHeight != null")]
        [Tooltip ("The peak of the trajectory relative to firing point")]
        [LabelText ("Target Height Scale")]
        [MinValue (0f)]
        public float inputTargetHeightScale = 30f;

        [FoldoutGroup (fgInputs)]
        [ShowIf ("@inputTargetOffset != null")]
        [Tooltip ("Random radius would be raised to power entered here. Useful for clustering output: power of 2 would shift most points towards the center, power of 0.5 would shift them away from center. Value of 1 is perfectly uniform distribution.")]
        [LabelText ("Target Offset Power")]
        [MinValue (0.1f), MaxValue (16f)]
        public float inputTargetOffsetPower = 1f;
        
        [FoldoutGroup (fgInputs)]
        [ShowIf ("@inputTargetOffset != null")]
        [Tooltip ("...")]
        [LabelText ("Throttle Dot Multiplier Power")]
        [MinValue (0.1f), MaxValue (16f)]
        public float inputThrottleDotPowerScale = 1f;

        [FoldoutGroup (fgInputs)]
        [ShowIf ("@inputTargetPointLateral != null")]
        [Tooltip ("...")]
        [LabelText ("Lateral Offset Mode")]
        [MinValue (0.1f), MaxValue (16f)]
        public LateralOffsetMode inputTargetPointLateralMode = LateralOffsetMode.Interleaved;

        [FoldoutGroup (fgPID)]
        public SimplePIDSettings steeringPID = new SimplePIDSettings ();
        
        [FoldoutGroup (fgPID)]
        public SimplePIDSettings pitchPID = new SimplePIDSettings ();
        
        [Tooltip ("Whether target point is towards target (0.5) or to the left (0) or right (1). Useful for making the missile vary the horizontal component of its trajectory without using target blend.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputTargetPointLateral;
        
        [Tooltip ("Whether target height equals start height (0) or start + ceiling height (1). Useful for making the missile vary the vertical component of its trajectory without using target blend.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputTargetHeight;
        
        [Tooltip ("Whether the projectile follows target height (0) or target point (1). Useful for making the missile track an independent path at a start then switch to tracking a unit towards the end.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputTargetBlend;
        
        [Tooltip ("Whether the projectile receives target position updates. If this value is below 0.5, the projectile ceases to receive updates about the target position.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputTargetUpdate;
        
        [Tooltip ("Whether the projectile tracks exact position of target unit or a point randomly offset by it (by separately defined radius and distribution). Useful for making projectiles track a wide spread of points at a start before converging at the end; or making a volley less precise, etc.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputTargetOffset;
        
        [Tooltip ("Whether the projectile is capable of applying full steering forces (1) or can't steer at all (0). Useful for making projectiles course correct more or less over their lifetime, e.g. for missiles that precisely guide for most of their life then accelerate without guidance at the end.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputSteering;
        
        [Tooltip ("Whether the projectile is capable of applying full thrust (1) or can't thrust at all (0). Useful for making projectiles run out of thrust towards the end, or making them accelerate on final approach, etc.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputThrottle;
        
        [Tooltip ("Power applied to dot product of facing direction vs target direction, which is then used as throttle multiplier. Set to 1 for throttle proportional to alignment, to 16 for throttle to fire only on nigh perfect alignment etc.")]
        [DropdownReference (true)]
        public IDataBlockGuidanceInput inputThrottleDotPower;

        [Tooltip ("Special feature that makes the projectile lose ability to steer if target goes out of certain dot product cone while within a certain distance. Useful for producing cinematic misses, where projectile doesn't turn sideways on near misses and instead jousts past target before regaining ability to course correct.")]
        [DropdownReference (true)]
        public DataBlockGuidanceDirectionCheck directionCheck;
        
        [Tooltip ("How well the projectile predicts future position of a unit")]
        [DropdownReference (true)]
        public DataBlockGuidanceVelocityCompensation velocityCompensation;
        

        private const string fgForces = "Forces";
        private const string fgInputs = "Inputs";
        private const string fgPID = "PID";
        private const string fgRigidbody = "Rigidbody";
        
        private float GetSpeedLimit ()
        {
            var F = (driverAccelerationForce * rigidbodyMass) / (1 / rigidbodyDrag - driverAccelerationMin);
            return F;
        }
        

        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockGuidanceData () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }

	public class DataBlockSubsystemBeam
    {
		public float maxDistance;
		public float damageRadius;
		public float secondsToFullDamage;
        public float foldoutSpeed;

        public bool visualByFaction;
        
        [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
        [InlineButton ("@visualName = null", "-")]
        public string visualName;
        
        [ShowIf ("visualByFaction")]
        [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
        [InlineButton ("@visualNameEnemy = null", "-")]
        public string visualNameEnemy;
    }
    
    public class DataBlockSubsystemBeam_V2
    {
        // public float maxDistance; // to stat weaponRangeMax
        // public float damageRadius; // to stat weaponDamageRadius
        // public float secondsToFullDamage; // to stat weaponDamageBuildup
        // public float foldoutSpeed; // to stat weaponProjectileSpeed

        public float reflectionAngleLimit = 180f;

        [DropdownReference (true)]
        public DataBlockAssetProjectile body;
        
        [DropdownReference (true)]
        public DataBlockAssetFactionBased impact;
        
        [DropdownReference (true)]
        public DataBlockAssetFactionBased reflection;
        
        [DropdownReference (true)]
        public DataBlockSubsystemStatusBuildup statusBuildup;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockSubsystemBeam_V2 () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}