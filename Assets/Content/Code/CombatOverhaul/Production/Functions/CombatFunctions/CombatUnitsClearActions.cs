using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitsClearActions : ICombatFunction
    {
        [BoxGroup ("Filter", false)]
        public DataBlockScenarioSubcheckUnitFilter unitFilter = new DataBlockScenarioSubcheckUnitFilter ();

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (unitFilter == null)
                return;
            
            var unitTargets = unitFilter.GetFilteredUnitsUsingSettings ();
            if (unitTargets == null || unitTargets.Count == 0)
                return;

            var actionContext = Contexts.sharedInstance.action;
            foreach (var unitTargetCombat in unitTargets)
            {
                var actions = actionContext.GetEntitiesWithActionOwner (unitTargetCombat.id.id);
                foreach (var action in actions)
                {
                    if (action.isDisposed || action.isLocked)
                        continue;
                    action.isDisposed = true;
                }
            }
            
            #endif
        }
    }
}