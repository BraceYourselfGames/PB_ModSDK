using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitPilotEffect : ICombatFunctionTargeted
    {
        public List<IPilotTargetedFunction> functions = new List<IPilotTargetedFunction> ();
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null || functions == null)
                return;

            var pilot = IDUtility.GetLinkedPilot (unitPersistent);
            if (pilot == null)
                return;

            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (pilot, unitPersistent);
            }
            
            #endif
        }
    }
}