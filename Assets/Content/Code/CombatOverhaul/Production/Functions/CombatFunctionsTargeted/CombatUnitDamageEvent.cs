using System;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    public class DataBlockInflictedStatusBuildup
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key;
        
        [PropertyRange (0f, 1f)]
        public float amount = 1f;
    }
    
    [LabelWidth (160f)]
    [Serializable]
    public class CombatUnitDamageEvent : ICombatFunctionTargeted
    {
        [BoxGroup ("A")]
        public float delay = 0f;
        
        [BoxGroup ("A", false)]
        public bool splash = true;
        
        [BoxGroup ("A")]
        public bool dispersed = true;
        
        [BoxGroup ("A")]
        public bool sourceInternal = true;

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

        public void Run (PersistentEntity unitPersistent)
        {
            CreateDamageEvent (unitPersistent);
        }

        public CombatEntity CreateDamageEvent (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return null;
            
            var level = Mathf.RoundToInt (DataHelperStats.GetAverageUnitLevel (unitPersistent));
            level = Mathf.Clamp (level, 1, DataShortcuts.sim.partLevelLimit);
            
            float integrityChange = integrity != null ? integrity.f : 0f;
            float concussionChange = concussion != null ? concussion.f : 0f;
            float heatChange = heat != null ? heat.f : 0f;
            float staggerChange = stagger != null ? stagger.f : 0f;
            
            if (integrity != null)
            {
                if (integrity.normalized)
                    integrityChange = Mathf.Clamp01 (integrityChange);
                else
                {
                    integrityChange = Mathf.Max (0f, integrityChange);
                    if (integrity.leveled && level > 1)
                    {
                        var stat = DataMultiLinkerUnitStats.GetEntry (UnitStats.weaponDamage, false);
                        if (stat != null && stat.increasePerLevel != null)
                        {
                            integrityChange = DataHelperStats.ApplyLevelToStat (integrityChange, level);
                            
                            // var levelFinal = Mathf.Max (1, level);
                            // var levelIncrease = stat.increasePerLevel.f;
                            // var levelMultiplier = levelFinal - 1;
                            // integrityChange += integrityChange * levelIncrease * levelMultiplier;
                        }
                    }
                }
            }
        
            bool integrityPresent = !integrityChange.RoughlyEqual (0f);
            bool concussionPresent = !concussionChange.RoughlyEqual (0f);
            bool heatPresent = !heatChange.RoughlyEqual (0f);
            bool staggerPresent = !staggerChange.RoughlyEqual (0f);

            var group = splash ? DamageEventGroup.Splash : DamageEventGroup.ContactStandard;
            var de = CombatUtilities.CreateDamageEvent (group, unitCombat.id.id);

            var facing = Quaternion.Euler (0f, Random.Range (-180f, 180f), 0f) * Vector3.forward;
            var pos = unitCombat.GetCenterPoint ();

            de.AddFacing (facing);
            de.AddPosition (pos);
                
            de.isDamageSplash = splash;
            de.isDamageDispersed = dispersed;
            de.AddLevel (level);
            
            if (delay > 0f)
                de.AddDelay (delay);

            if (integrityPresent)
            {
                de.AddInflictedDamage (integrityChange);
                de.isInflictedDamageNormalized = integrity != null ? integrity.normalized : false;
            }

            if (concussionPresent)
                de.AddInflictedConcussion (concussionChange);

            if (heatPresent)
                de.AddInflictedHeat (heatChange);
                
            if (staggerPresent)
                de.AddInflictedStagger (staggerChange);

            if (sourceInternal)
                de.isDamageEventInternal = true;
            
            if (statusBuildup != null)
                de.AddInflictedBuildup (statusBuildup.key, statusBuildup.amount);

            return de;
            
            #else
            return null;
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatUnitDamageEvent () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}