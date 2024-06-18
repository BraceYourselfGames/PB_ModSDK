using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetSpeedRotationPrimary : ICombatFunctionTargeted
    {
        public enum Mode
        {
            Set,
            Offset,
            Multiply
        }
        
        public Mode mode = Mode.Set;
        [LabelText ("Speed (R/S)")]
        public float speed = 0.5f;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;
            
            var speedCurrent = DataShortcuts.anim.filterRotationLimitSmoothDamp;
            if (unitCombat.hasRotationSpeedPrimaryOverride)
                speedCurrent = unitCombat.rotationSpeedPrimaryOverride.f;
            else if (unitCombat.hasUnitCompositeLink)
            {
                var compositeBlueprint = DataMultiLinkerUnitComposite.GetEntry (unitCombat.unitCompositeLink.blueprintKey, false);
                if (compositeBlueprint != null && compositeBlueprint.coreProcessed != null)
                    speedCurrent = compositeBlueprint.coreProcessed.speedRotationPrimary;
            }

            var speedModified = 1f;
            if (mode == Mode.Set)
                speedModified = speed;
            else if (mode == Mode.Offset)
                speedModified = speedCurrent + speed;
            else if (mode == Mode.Multiply)
                speedModified = speedCurrent * speed;

            speedModified = Mathf.Clamp (speedModified, 0.01f, 50f);
            Debug.Log ($"Unit {unitPersistent.ToLog ()} primary rotation speed modified (r/s): {speedCurrent} -> {speedModified}");
            
            unitCombat.ReplaceRotationSpeedPrimaryOverride (speedModified);
            
            #endif
        }
    }
}