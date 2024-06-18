using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitActionsCreate : ICombatFunctionTargeted
    {
        public bool discardExisting = true;

        [ListDrawerSettings (DefaultExpandedState = true)]
        public List<DataBlockScenarioUnitChangeAction> actions = new List<DataBlockScenarioUnitChangeAction> ();
    
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;
            
            if (discardExisting)
            {
                var actionsOld = Contexts.sharedInstance.action.GetEntitiesWithActionOwner (unitCombat.id.id);
                if (actionsOld != null)
                {
                    foreach (var action in actionsOld)
                    {
                        if (action != null && !action.isDestroyed && !action.isDisposed)
                            action.isDisposed = true;
                    }
                }
            }

            if (actions == null || actions.Count == 0)
                return;

            // Debug.Log ($"Creating {actions.Count} actions for unit {unitPersistent.ToLog ()}: {actions.ToStringFormatted (toStringOverride: (x) => x.key)}");
            Co.Run (ScenarioUtility.CreateUnitActionsIE (unitCombat, unitPersistent, actions));
            
            #endif
        }
    }
}