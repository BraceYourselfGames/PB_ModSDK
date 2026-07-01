using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitActionsOffsetTime : ICombatFunctionTargeted
    {
        [PropertyRange (0f, 5f)]
        public float offset = 0f;
        public bool locked = true;
        public bool primary = true;
        public bool secondary = true;
    
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            var actions = Contexts.sharedInstance.action.GetEntitiesWithActionOwner (unitCombat.id.id);
            if (actions == null || actions.Count == 0)
                return;

            if (offset <= 0f)
                return;
            
            foreach (var action in actions)
            {
                if (action.isDisposed || !action.hasStartTime || action.CompletedAction || action.isDestroyed)
                    continue;
                
                if (action.isLocked && !locked)
                    continue;
                
                var startTime = action.startTime.f;
                
                if (action.isOnPrimaryTrack && primary)
                    action.ReplaceStartTime (startTime + offset);
                else if (action.isOnSecondaryTrack && secondary)
                    action.ReplaceStartTime (startTime + offset);
            }
            
            #endif
        }
    }
}