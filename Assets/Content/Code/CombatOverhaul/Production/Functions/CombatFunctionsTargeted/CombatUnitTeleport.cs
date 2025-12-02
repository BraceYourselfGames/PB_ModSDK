using System;
using System.Collections.Generic;
using PhantomBrigade.Combat.Systems;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitTeleport : ICombatFunctionTargeted
    {
        public TargetFromSource target = new TargetFromSource ();
        public DataBlockAsset effectOnOrigin = new DataBlockAsset ();
        public DataBlockAsset effectOnDestination = new DataBlockAsset ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

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

            var positionCurrent = unitCombat.position.v;
            unitCombat.ForceUnitTransform (position, direction);
            
            var actionsOld = Contexts.sharedInstance.action.GetEntitiesWithActionOwner (unitCombat.id.id);
            if (actionsOld != null)
            {
                foreach (var action in actionsOld)
                {
                    if (action != null && !action.isDestroyed && !action.isDisposed && action.isOnPrimaryTrack)
                        action.isDisposed = true;
                }
            }

            if (effectOnOrigin != null)
                AssetPoolUtility.ActivateInstance (effectOnOrigin.key, positionCurrent, -direction);
            
            if (effectOnDestination != null)
                AssetPoolUtility.ActivateInstance (effectOnDestination.key, position, direction);

            #endif
        }
    }
}