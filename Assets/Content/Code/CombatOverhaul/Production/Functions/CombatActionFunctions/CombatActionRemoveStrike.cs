using System;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionRemoveStrike : ICombatActionExecutionFunction
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

            CombatStrikeHelper.RemoveStrikeFromAction (action.id.id);
            
            #endif
        }
    }
}