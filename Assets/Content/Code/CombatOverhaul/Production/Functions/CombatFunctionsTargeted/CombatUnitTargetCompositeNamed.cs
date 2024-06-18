using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitTargetCompositeNamed : ICombatFunction
    {
        public string instanceKey;
        public string unitKey;
        public List<ICombatFunctionTargeted> functionsTargeted;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var unitsInComposite = UnitUtilities.GetUnitsInComposite (instanceKey);
            if (unitsInComposite == null || !unitsInComposite.TryGetValue (unitKey, out var unitCombatTarget))
            {
                Debug.LogWarning ($"Can't run targeted functions on composite child {unitKey} - couldn't find a child unit with that key under composite {instanceKey}");
                return;
            }

            var unitPersistentTarget = IDUtility.GetLinkedPersistentEntity (unitCombatTarget);
            foreach (var function in functionsTargeted)
                function.Run (unitPersistentTarget);
            
            #endif
        }
    }
}