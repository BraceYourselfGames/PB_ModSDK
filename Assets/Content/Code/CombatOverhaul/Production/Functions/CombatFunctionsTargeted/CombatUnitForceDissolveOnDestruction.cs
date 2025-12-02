using System;
using PhantomBrigade.AI.BT;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitForceDissolveOnDestruction : ICombatFunctionTargeted
    {
        public bool forced = true;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;
            
            if (unitCombat.hasCombatView)
            {
                var uvm = unitCombat.combatView.view.visualManager;
                if (uvm != null)
                    uvm.SetDestructionDissolveForced (forced);
            }
            else
            {
                unitPersistent.SetMemoryFloat ("temp_destruction_dissolve_forced", forced ? 1f : 0f);
            }

            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitForceMovementFactor : ICombatFunctionTargeted
    {
        public float factor = 0f;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;
            
            if (unitCombat.hasTankAnimationView)
            {
                var av = unitCombat.tankAnimationView.view;
                av.movementFactorLocked = factor;
            }
            else
            {
                unitPersistent.SetMemoryFloat ("temp_movement_factor_locked", factor);
            }

            #endif
        }
    }
}