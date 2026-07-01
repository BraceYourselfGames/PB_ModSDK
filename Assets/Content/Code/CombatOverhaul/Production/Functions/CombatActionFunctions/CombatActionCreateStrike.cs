using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionCreateStrike : ICombatActionExecutionFunction
    {
        [ValueDropdown ("@DataMultiLinkerCombatStrike.data.Keys")]
        public string blueprintKey = string.Empty;

        public bool targetFromOwner = false;
        public bool directionFromOrigin = false;
        public Vector3 targetOffset = Vector3.zero;
        
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitCombat == null || unitPersistent == null)
                return;

            var startTime = action.startTime.f;
            PathUtility.GetProjectedTransformAtTime (unitCombat, startTime, out Vector3 originPos, out Vector3 originDir);

            var targetPos = Vector3.zero;
            var targetDir = Quaternion.Euler (0f, 45f, 0f) * Vector3.forward;
            bool targetFound = false;
            
            if (action.hasTargetedEntity)
            {
                var unitTargetCombat = IDUtility.GetCombatEntity (action.targetedEntity.combatID);
                if (unitTargetCombat != null)
                {
                    PathUtility.GetProjectedTransformAtTime (unitTargetCombat, startTime, out var targetUnitPos, out var targetUnitDir);
                    targetFound = true;
                    targetPos = targetUnitPos;
                }
            }
            else if (action.hasTargetedPoint)
            {
                targetPos = action.targetedPoint.v;
                targetFound = true;
            }

            if (targetFromOwner)
            {
                targetPos = originPos;
                targetDir = originDir.FlattenAndNormalize ();
                targetFound = true;
            }

            if (!targetFound)
                return;
            
            if (!targetFromOwner && directionFromOrigin && originPos != targetPos)
                targetDir = (targetPos - originPos).FlattenAndNormalize ();

            if (targetOffset != Vector3.zero && targetDir != Vector3.zero)
            {
                var targetRot = Quaternion.LookRotation (targetDir, Vector3.up);
                targetPos += targetRot * targetOffset;
            }

            CombatStrikeHelper.AddStrikeFromAction (blueprintKey, action.id.id, startTime, targetPos, targetDir);
            action.isStrikeAttached = true;
            
            #endif
        }
    }
}