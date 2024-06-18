using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitsApplyChanges : ICombatFunction
    {
        [BoxGroup ("Filter", false)]
        public DataBlockScenarioSubcheckUnitFilter unitFilter = new DataBlockScenarioSubcheckUnitFilter ();
        
        [BoxGroup ("Change", false)]
        public DataBlockScenarioUnitChange change = new DataBlockScenarioUnitChange ();
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (unitFilter == null)
                return;
            
            var unitTargets = unitFilter.GetFilteredUnitsUsingSettings ();
            if (unitTargets == null || unitTargets.Count == 0)
                return;

            Debug.Log ($"Applying a change to {unitTargets.Count} units");
            
            foreach (var unitTargetCombat in unitTargets)
            {
                var unitTargetPersistent = IDUtility.GetLinkedPersistentEntity (unitTargetCombat);
                Debug.Log ($"Applying a change to {unitTargetCombat.ToLog ()} / {unitTargetPersistent.ToLog ()}");
                
                ScenarioUtility.ApplyUnitChangeIsolated (unitTargetPersistent, change);
            }
            
            #endif
        }
    }
}