using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitActionsDiscard : ICombatFunctionTargeted
    {
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
            
            foreach (var action in actions)
            {
                if (action.isDisposed)
                    continue;
                
                if (action.isLocked && !locked)
                    continue;
                
                if (action.isOnPrimaryTrack && primary)
                    action.isDisposed = true;
                else if (action.isOnSecondaryTrack && secondary)
                    action.isDisposed = true;
            }
            
            #endif
        }
    }
}