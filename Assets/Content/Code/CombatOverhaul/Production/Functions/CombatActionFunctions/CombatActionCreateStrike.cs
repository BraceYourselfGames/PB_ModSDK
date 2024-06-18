using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionCreateStrike : ICombatActionExecutionFunction
    {
        [ValueDropdown ("@DataMultiLinkerCombatStrike.data.Keys")]
        public string blueprintKey = string.Empty;
        
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitCombat == null || unitPersistent == null)
                return;

            var startTime = action.startTime.f;
            PathUtility.GetProjectedTransformAtTime (unitCombat, startTime, out Vector3 pos, out Vector3 dir);
            CombatStrikeHelper.AddStrikeFromAction (blueprintKey, action.id.id, startTime, pos, dir);
            action.isStrikeAttached = true;
            
            #endif
        }
    }
}