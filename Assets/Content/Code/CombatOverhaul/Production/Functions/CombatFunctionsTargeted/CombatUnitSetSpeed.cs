using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetSpeed : ICombatFunctionTargeted
    {
        public enum Mode
        {
            Set,
            Offset,
            Multiply
        }
        
        public Mode mode = Mode.Set;
        public float speed = 5f;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            var speedCurrent = unitCombat.movementSpeedCurrent.f;
            if (unitPersistent.hasMovementSpeedOverride)
                speedCurrent = unitPersistent.movementSpeedOverride.f;

            var speedModified = 1f;
            if (mode == Mode.Set)
                speedModified = speed;
            else if (mode == Mode.Offset)
                speedModified = speedCurrent + speed;
            else if (mode == Mode.Multiply)
                speedModified = speedCurrent * speed;

            speedModified = Mathf.Clamp (speedModified, 0.01f, 50f);
            Debug.Log ($"Unit {unitPersistent.ToLog ()} speed modified: {speedCurrent} -> {speedModified}");
            
            unitPersistent.ReplaceMovementSpeedOverride (speedModified);
            DataHelperStats.RefreshStatBasedCombatComponents (unitPersistent);
            
            #endif
        }
    }
}