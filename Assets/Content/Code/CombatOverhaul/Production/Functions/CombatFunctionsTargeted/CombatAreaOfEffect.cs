using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [HideReferenceObjectPicker]
    public class DataBlockIntegrityDamage
    {
        [OnValueChanged ("ValidateValue")]
        public bool normalized;
        
        [OnValueChanged ("ValidateValue")]
        [HideIf ("normalized")]
        public bool leveled;
        
        [OnValueChanged ("ValidateValue")]
        [LabelText ("Value")]
        public float f;

        private void ValidateValue ()
        {
            if (normalized)
                f = Mathf.Clamp01 (f);
            else
                f = Mathf.Max (0f, f);
        }
    }
    
    [Serializable]
    public class CombatAreaOfEffect : CombatAreaOfEffectBase, ICombatFunctionTargeted
    {
        [PropertyOrder (-1)]
        [BoxGroup ("Offset", false)]
        public Vector3 offset;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null || !unitCombat.isUnitTag || !unitCombat.hasPosition)
                return;

            var position = unitCombat.position.v;
            if (unitCombat.hasLocalCenterPoint)
                position += unitCombat.GetCenterOffset ();
            
            if (unitCombat.hasRotation)
                position += unitCombat.rotation.q * offset;

            var direction = (unitCombat.rotation.q * Vector3.forward).FlattenAndNormalize ();
            int level = Mathf.RoundToInt (DataHelperStats.GetAverageUnitLevel (unitPersistent));
            
            RunBase (position, direction, unitPersistent);
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatAreaOfEffectSpatial : CombatAreaOfEffectBase, ICombatFunctionSpatial
    {
        [PropertyOrder (-1)]
        [BoxGroup ("Offset", false)]
        public Vector3 offset;
        
        public void Run (Vector3 position, Vector3 direction)
        {
            #if !PB_MODSDK
            
            RunBase (position, direction, null);
            
            #endif
        }
    }

    [Serializable]
    public class CombatAreaOfEffectFromSource : CombatAreaOfEffectBase, ICombatFunction
    {
        [PropertyOrder (-1)]
        [BoxGroup ("Target", false)]
        public TargetFromSource target = new TargetFromSource ();

        public void Run ()
        {
            #if !PB_MODSDK
            
            var position = Vector3.zero;
            var direction = Vector3.forward;
            
            bool targetFound = ScenarioUtility.GetTarget
            (
                null, 
                target, 
                out var targetPosition, 
                out var targetDirection, 
                out var targetUnitCombat
            );

            if (targetFound)
            {
                position = targetPosition;
                direction = targetDirection.FlattenAndNormalize ();
            }
            
            RunBase (position, direction, null);
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatAreaOfEffectBase
    {
        [BoxGroup ("Spread")]
        [PropertyRange (3f, 60f)]
        public float radius = 10f;
        
        [BoxGroup ("Unit Damage", false)]
        [PropertyRange (0.1f, 16f)]
        public float exponent = 2f;
        
        [BoxGroup ("Unit Damage")]
        public bool dispersed = true;
        
        [DropdownReference (true)]
        public DataBlockIntegrityDamage integrity;
        
        [DropdownReference (true)]
        public DataBlockFloat concussion;
        
        [DropdownReference (true)]
        public DataBlockFloat heat;
        
        [DropdownReference (true)]
        public DataBlockFloat stagger;
        
        [DropdownReference (true)]
        public DataBlockInflictedStatusBuildup statusBuildup;
        
        [LabelText ("Crash Velocity")]
        [DropdownReference (true)]
        public DataBlockFloat crash;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnit targetCheck;

        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsOnHit;

        
        [BoxGroup ("Mask", false)]
        public bool impactUnitSelf = true;
        
        [BoxGroup ("Mask", false)]
        public bool impactUnitAllies = true;
        
        [BoxGroup ("Mask", false)]
        public bool impactUnitHostiles = true;
        
        [BoxGroup ("Mask", false)]
        public bool impactUnitComposites = true;

        [BoxGroup ("Mask")]
        public bool impactProjectiles = false;
        
        [BoxGroup ("Mask")]
        public bool impactProps = true;
        
        
        [BoxGroup ("FX")]
        [ValueDropdown("GetAssetKeys")]
        [InlineButton ("@fxDetonation = null", "-")]
        [LabelText ("Detonation FX")]
        public string fxDetonation;
        
        [BoxGroup ("FX")]
        [PropertyRange (1f, 10f)]
        public float fxDetonationScale = 1f;
        
        [BoxGroup ("FX", false)]
        [ValueDropdown("GetAssetKeys")]
        [InlineButton ("@fxArea = null", "-")]
        [LabelText ("Area FX")]
        public string fxArea;
        
        [BoxGroup ("FX")]
        [ValueDropdown("GetAssetKeys")]
        [InlineButton ("@fxHit = null", "-")]
        [LabelText ("Hit FX")]
        public string fxHit;

        [BoxGroup ("FX")]
        [ValueDropdown ("@AudioEvents.GetKeys ()")]
        public string audioDetonation = null;
        
        
        private IEnumerable<string> GetAssetKeys () => DataMultiLinkerAssetPools.data.Keys;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public CombatAreaOfEffectBase () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion

        public void RunBase (Vector3 position, Vector3 direction, PersistentEntity unitPersistentSource)
        {
            #if !PB_MODSDK
            
            if (!string.IsNullOrEmpty (audioDetonation))
                AudioUtility.CreateAudioEvent (audioDetonation, position);
            
            if (!string.IsNullOrEmpty (fxDetonation))
            {
                AssetPoolUtility.ActivateInstance (fxDetonation, position, direction, Vector3.one * Mathf.Max (1f, fxDetonationScale));
            }

            if (!string.IsNullOrEmpty (fxArea))
            {
                var fxScale = radius * 2f * Vector3.one;
                AssetPoolUtility.ActivateInstance (fxArea, position, direction, fxScale);
            }
            
            if (impactUnitSelf || impactUnitAllies || impactUnitHostiles || impactUnitComposites)
            {
                bool integrityChangeNormalized = integrity != null && integrity.normalized;
                float integrityChange = integrity != null ? integrity.f : 0f;
                float concussionChange = concussion != null ? concussion.f : 0f;
                float heatChange = heat != null ? heat.f : 0f;
                float staggerChange = stagger != null ? stagger.f : 0f;
                float crashVelocity = crash != null ? Mathf.Max (0f, crash.f) : -1f;

                if (integrity != null)
                {
                    if (integrity.normalized)
                        integrityChange = Mathf.Clamp01 (integrityChange);
                    else
                    {
                        integrityChange = Mathf.Max (0f, integrityChange);
                        if (integrity.leveled)
                        {
                            int level = 1;
                            if (unitPersistentSource != null)
                                level = Mathf.RoundToInt (DataHelperStats.GetAverageUnitLevel (unitPersistentSource));
                            else
                            {
                                var sitePersistent = ScenarioUtility.GetCombatSite ();
                                if (sitePersistent != null && sitePersistent.hasCombatUnitLevel)
                                    level = Mathf.Max (1, sitePersistent.combatUnitLevel.i);
                            }

                            if (level > 1)
                            {
                                var stat = DataMultiLinkerUnitStats.GetEntry (UnitStats.weaponDamage, false);
                                if (stat != null && stat.increasePerLevel != null)
                                {
                                    // var levelFinal = Mathf.Max (1, level);
                                    // var levelIncrease = stat.increasePerLevel.f;
                                    // var levelMultiplier = levelFinal - 1;
                                    // integrityChange += integrityChange * levelIncrease * levelMultiplier;
                                    
                                    integrityChange = DataHelperStats.ApplyLevelToStat (integrityChange, level);
                                }
                            }
                        }
                    }
                }
                
                var unitCombatSource = IDUtility.GetLinkedCombatEntity (unitPersistentSource);
                
                string statusBuildupKey = null;
                float statusBuildupAmount = 0f;

                if (statusBuildup != null)
                {
                    statusBuildupKey = statusBuildup.key;
                    statusBuildupAmount = statusBuildup.amount;
                }

                OverlapUtility.OnAreaOfEffectAgainstUnits
                (
                    position, radius, exponent,
                    integrityChange, concussionChange, heatChange, staggerChange,
                    fxHit,
                    null,
                    unitCombatSource,
                    0,
                    0,
                    dispersed,
                    integrityChangeNormalized,
                    impactUnitSelf,
                    impactUnitAllies,
                    impactUnitHostiles,
                    impactUnitComposites,
                    crashVelocity: crashVelocity,
                    targetCheck: targetCheck,
                    functionsOnHit: functionsOnHit,
                    statusBuildupKey: statusBuildupKey,
                    statusBuildupAmount: statusBuildupAmount
                    
                );
            }

            if (impactProjectiles)
                OverlapUtility.OnAreaOfEffectAgainstProjectiles (position, radius, null, fxHit);
            
            if (impactProps)
                OverlapUtility.OnAreaOfEffectAgainstProps (position, radius);
            
            #endif
        }
    }
}