using System;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitPilotEject : ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            var availableActions = DataHelperAction.GetAvailableActions (unitCombat);
            if (availableActions.Contains("eject"))
            {
                CombatActionEvent.OnEjection (unitCombat, null);
                CombatActionEvent.CrashUnit (unitCombat, null);
            }
            else
                Debug.LogWarning ($"Unit {unitPersistent.ToLog ()}/{unitCombat.ToLog ()} can't eject");
            
            #endif
        }
    }
}