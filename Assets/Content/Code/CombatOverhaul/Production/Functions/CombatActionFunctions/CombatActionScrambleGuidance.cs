using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitScrambleGuidance : ICombatFunctionTargeted
    {
        [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys")]
        public string fxKey = "fx_thruster_flash";

        public Vector3 offset;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            if (!unitCombat.hasPosition)
				return;

            var myId = unitCombat.id.id;
            var scrambleOrigin = unitCombat.GetCenterPoint ();

            var group = Contexts.sharedInstance.combat.GetGroup (CombatMatcher.ProjectileTargetEntity);

            var sim = DataShortcuts.sim;
            float scrambleTime = Mathf.Max (0.1f, sim.scrambleDuration);
            float scrambleMinR = Mathf.Max (1f, sim.scrambleInnerRadius);
            float scrambleMaxR = Mathf.Max (2f , sim.scrambleOuterRadius);

            int scrambleCount = 0;
            bool showGuidanceData = DataShortcuts.uiDebug.showGuidanceData;
            bool log = DataShortcuts.debug.debugWeapons;
            
            var timeCurrent = Contexts.sharedInstance.combat.simulationTime.f;
            var scrambleOffsetDir = unitCombat.hasVelocityDirection ? -unitCombat.velocityDirection.v.FlattenAndNormalize () : unitCombat.rotation.q * Vector3.back;
            
            var offsetRotated = Quaternion.LookRotation (scrambleOffsetDir) * offset;
            var fxPos = scrambleOrigin + offsetRotated;
            
            float scrambleMinRSqr = scrambleMinR * scrambleMinR;
            float scrambleMaxRSqr = scrambleMaxR * scrambleMaxR;
            float scrambleDelayVisual = sim.scrambleDelayVisual;
            
            AssetPoolUtility.ActivateInstance (fxKey, fxPos, scrambleOffsetDir);
            
            foreach (var ent in group)
            {
				var tgtId = ent.projectileTargetEntity.combatID;
                if (tgtId != myId)
	                continue;

				if (!ent.hasPosition)
					continue;
                
                var projectilePos = ent.position.v;
                var delta = projectilePos - scrambleOrigin;
                var distSqr = delta.sqrMagnitude;

                if (distSqr > scrambleMaxRSqr)
                {
                    Debug.DrawLine (projectilePos, scrambleOrigin, Color.red, 1f);
                    continue;
                }
                
                var dist = Mathf.Sqrt (distSqr);

                float effectStrength = 1f;
                if (dist > scrambleMinR)
					effectStrength = 1f - (dist - scrambleMinR) / (scrambleMaxR - scrambleMinR);
                
                scrambleCount += 1;
                float duration = scrambleTime * effectStrength;

                ent.ReplaceProjectileGuidanceSuspended (timeCurrent, duration, scrambleOffsetDir);

                var directionToTarget = (scrambleOrigin - projectilePos).normalized;
                var posOffset = projectilePos + ent.rotation.q * Vector3.forward;
                var delay = scrambleDelayVisual * Mathf.Clamp01 (dist / scrambleMaxR);
                if (delay < 0.03f)
                    delay = 0f;

                if (ent.hasVelocity)
                    posOffset += ent.velocity.v * (delay + 0.05f);
                
                AssetPoolUtility.ActivateInstance ("fx_projectile_guidance_scramble", posOffset, directionToTarget, delay: delay);
                
                Debug.DrawLine (projectilePos, scrambleOrigin, Color.Lerp (Color.red, Color.green, Mathf.Clamp01 (effectStrength)), 1f);
                
                if (showGuidanceData)
                    ShapesDrawer.DrawLineHighlighted (projectilePos, scrambleOrigin, Color.Lerp (DataShortcuts.uiDebug.colorDefaultBlue, DataShortcuts.uiDebug.colorDefaultRed, Mathf.Clamp01 (effectStrength)), duration: duration);
                
                if (log)
                    Debug.Log ($"Scrambling projectile {ent.ToLog ()} from unit {unitCombat.ToLog ()} | Effect strength: {effectStrength:0.##} | Time: {duration:0.##} | Distance: {Mathf.Sqrt (distSqr):0.##}");
            }
            
            if (scrambleCount > 0 && CombatUIUtility.IsUnitFriendly (unitCombat))
                AchievementHelper.UnlockAchievement(AchievementKeys.ScrambleMissile);
            
            #endif
        }
    }
}