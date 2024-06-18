using System;
using PhantomBrigade.Combat.Systems;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCreateProjectile : ICombatFunction
    {
        [PropertyRange (0f, 10f)]
        public float delay = 0f;
        public string unitNameInternal;
        
        public bool weaponPrimary = true;
        public bool rangeOptimized = true;
        
        public Vector3 positionTargeted;
        public Vector3 originOffset;

        public void Run ()
        {
            #if !PB_MODSDK
            
            var delaySafe = Mathf.Clamp (delay, 0f, 60f);
            if (delaySafe <= 0f)
                RunDelayed ();
            else
                Co.DelayScaled (delaySafe, RunDelayed);
            
            #endif
        }
        
        private void RunDelayed ()
        {
            #if !PB_MODSDK
            
            // Since this might have been delayed, we need some safety checks
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.combat))
                return;
            
            var game = Contexts.sharedInstance.game;
            if (game.isCutsceneInProgress)
                return;
            
            var combat = Contexts.sharedInstance.combat;
            if (combat.isScenarioIntroInProgress)
                return;
            
            var unitSelectedPersistent = IDUtility.GetPersistentEntity (unitNameInternal);
            var unitSelectedCombat = IDUtility.GetLinkedCombatEntity (unitSelectedPersistent);
            if (unitSelectedCombat == null || unitSelectedPersistent == null)
                return;

            var socket = weaponPrimary ? LoadoutSockets.rightEquipment : LoadoutSockets.leftEquipment;
            var part = EquipmentUtility.GetPartInUnit (unitSelectedPersistent, socket);

            if (part == null || !part.hasPrimaryActivationSubsystem)
                return;
                    
            var subsystem = IDUtility.GetEquipmentEntity (part.primaryActivationSubsystem.equipmentID);
            if (subsystem == null)
                return;
            
            var subsystemBlueprint = subsystem?.dataLinkSubsystem.data;
            var activationData = subsystemBlueprint.activationProcessed;
            var projectileData = subsystemBlueprint.projectileProcessed;
                
            if (activationData == null || projectileData == null)
                return;

            float range = 0f;
            if (part.hasEffectivenessData)
            {
                var window = part.effectivenessData.ranges;
                float rangeMin = window.y;
                float rangeMax = window.z;
                range = (rangeMin + rangeMax) * 0.5f;
            }
            else
            {
                float rangeMin = DataHelperStats.GetCachedStatForPart (UnitStats.weaponRangeMin, part);
                float rangeMax = DataHelperStats.GetCachedStatForPart (UnitStats.weaponRangeMax, part);
                range = (rangeMin + rangeMax) * 0.5f;
            }

            if (range < 3f)
                return;

            bool friendly = false;

            var origin = positionTargeted + originOffset;
            var targetDirection = (positionTargeted - origin).normalized;
            var firingPoint = rangeOptimized ? positionTargeted - targetDirection * range : origin;
            var targetPoint = positionTargeted;

            int level = part.level.i;
            var projSpeed = Mathf.Max (1, DataHelperStats.GetCachedStatForPart (UnitStats.weaponProjectileSpeed, part));
            
            var damageDispersed = subsystemBlueprint.IsFlagPresent (PartCustomFlagKeys.DamageDispersed);

            bool primed = true;
            if (projectileData.range != null && projectileData.range.deactivateBeforeRange)
                primed = false;
            else if (projectileData.fuseProximity != null && projectileData.fuseProximity.inertBeforeHit)
                primed = false;
                
            int penetrationCharges = Mathf.FloorToInt (DataHelperStats.GetCachedStatForPart (UnitStats.weaponPenetrationCharges, part));
            int penetrationUnitCost = Mathf.FloorToInt (DataHelperStats.GetCachedStatForPart (UnitStats.weaponPenetrationUnitCost, part));
            int penetrationGeomCost = Mathf.FloorToInt (DataHelperStats.GetCachedStatForPart (UnitStats.weaponPenetrationGeomCost, part));
            float penetrationDamageK = DataHelperStats.GetCachedStatForPart (UnitStats.weaponPenetrationDamageK, part);
            bool penetrationUsed = penetrationCharges > 0;
            var ricochetChance = DataHelperStats.GetCachedStatForPart (UnitStats.weaponProjectileRicochet, part);
            var lifetime = DataHelperStats.GetCachedStatForPart (UnitStats.weaponProjectileLifetime, part);
            
            bool falloffAnimated = projectileData.falloff != null && projectileData.falloff.animated;
            bool fuseProximityUsed = projectileData.fuseProximity != null;
            bool splashDamageUsed = projectileData.splashDamage != null;
            bool splashImpactUsed = projectileData.splashImpact != null;

            string bodyAssetKey = null;
            var bodyAssetScale = Vector3.one;
            bool bodyAssetUsed = false;
            DataBlockColorInterpolated bodyAssetColorOverride = null;
            
            if (projectileData.visual != null && projectileData.visual.body != null)
            {
                var body = projectileData.visual.body;
                bodyAssetScale = projectileData.visual.body.scale;
                bodyAssetKey = !string.IsNullOrEmpty (body.keyEnemy) ? body.keyEnemy : body.key;
                bodyAssetUsed = !string.IsNullOrEmpty (bodyAssetKey);
                bodyAssetColorOverride = friendly || body.colorOverrideEnemy == null ? body.colorOverride : body.colorOverrideEnemy;
            }
            
            var finalDirection = targetDirection;

            var projectile = Contexts.sharedInstance.combat.CreateEntity ();

            projectile.AddDataLinkSubsystemProjectile (projectileData);
            projectile.AddParentPart (part.id.id);
            projectile.AddParentSubsystem (subsystem.id.id);
            projectile.ReplaceScale (bodyAssetScale);
            projectile.ReplaceLevel (level);

            projectile.ReplacePosition (firingPoint);
            projectile.ReplaceRotation (Quaternion.LookRotation (finalDirection));
            projectile.ReplaceFacing (finalDirection);
            projectile.ReplaceTimeToLive (lifetime);
            projectile.ReplaceProjectileCollision (LayerMasks.projectileMask, 0.0f);
            
            projectile.ReplaceSourceEntity (unitSelectedCombat.id.id);
            
            projectile.ReplaceProjectileTargetPosition (targetPoint);

            if (falloffAnimated)
                projectile.isProjectileFalloffAnimated = true;

            if (fuseProximityUsed)
                projectile.isProjectileProximityFuse = true;

            if (splashDamageUsed)
                projectile.isDamageSplash = true;

            if (splashImpactUsed)
            {
                projectile.isImpactSplash = true;
                projectile.ImpactSplashOnDamage = projectileData.splashImpact.triggerOnDamage;
            }

            if (damageDispersed)
                projectile.isDamageDispersed = true;
            
            if (primed)
                projectile.isProjectilePrimed = primed;
            
            if (penetrationUsed)
            {
                projectile.AddProjectilePenetrationInfo 
                (
                    penetrationCharges,
                    penetrationUnitCost,
                    penetrationGeomCost,
                    penetrationDamageK,
                    0,
                    0f,
                    0f,
                    IDUtility.invalidID
                );
            }

            if (bodyAssetUsed)
                AssetPoolUtility.AttachInstance (bodyAssetKey, projectile, true);
            
            var instance = projectile.hasAssetLink ? projectile.assetLink.instance : null;
            bool instancePresent = instance != null;
            
            if (instancePresent)
                instance.UpdateColors (null, bodyAssetColorOverride);
            
            if (projectile.isProjectilePrimed && instancePresent && instance.fxHelperProjectile != null)
                instance.fxHelperProjectile.SetRange (1f);
            
            ScheduledAttackSystem.AddInflictedDamageComponents (part, projectile, null);
            ScheduledAttackSystem.AttachTypeSpecificProjectileData 
            (
                projectile,
                unitSelectedCombat,
                part,
                projectileData,
                firingPoint,
                finalDirection,
                projSpeed,
                targetPoint,
                ricochetChance
            );
            
            #endif
        }
    }
}