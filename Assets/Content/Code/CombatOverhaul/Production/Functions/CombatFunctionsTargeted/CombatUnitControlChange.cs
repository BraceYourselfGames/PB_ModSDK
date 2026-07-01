using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitControlChange : ICombatFunctionTargeted
    {
        [LabelText ("AI")]
        public bool ai = true;
        public bool player = false;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            unitCombat.isAIControllable = ai;
            unitCombat.isPlayerControllable = player;
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitHiddenDetectable : ICombatFunctionTargeted
    {
        public int controlDelay = 0;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            unitCombat.isHidden = true;
            unitCombat.isHiddenDetectable = true;

            if (controlDelay > 0)
            {
                unitPersistent.SetMemoryFloat (EventMemoryInt.UnitAIControlCountdown, controlDelay + 1);
                unitCombat.isAIControllable = false;
            }

            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitHidden : ICombatFunctionTargeted
    {
        public bool hidden = true;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            unitCombat.isHidden = hidden;

            #endif
        }
    }
}