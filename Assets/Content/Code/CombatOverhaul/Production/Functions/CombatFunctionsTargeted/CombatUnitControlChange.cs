using System;
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
}