using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionCreateAirlift : ICombatActionExecutionFunction
    {
        [ValueDropdown ("@DataMultiLinkerCombatStrike.data.Keys")]
        public string blueprintKey = string.Empty;

        public float angle = 0f;
        
        public void Run (CombatEntity unitCombat, ActionEntity action)
        {
            #if !PB_MODSDK

            if (action == null)
                return;
            
            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            if (unitCombat == null || unitPersistent == null)
                return;

            var startTime = action.startTime.f;
            var dirLanding = Quaternion.Euler (0f, angle, 0f) * Vector3.forward;
            
            PathUtility.GetProjectedTransformAtTime (unitCombat, startTime, out Vector3 pos, out Vector3 dir);
            CombatStrikeHelper.AddStrikeFromAction (blueprintKey, action.id.id, startTime, pos, dirLanding);
            action.isStrikeAttached = true;
            action.ReplaceRetreatAirliftData (angle);
            
            #endif
        }
    }
}