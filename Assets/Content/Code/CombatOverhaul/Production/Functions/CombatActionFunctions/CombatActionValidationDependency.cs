using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionValidationDependency : ICombatActionValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string dependencyActionKey;
        
        public bool IsValid (CombatEntity unitCombat)
        {
            #if !PB_MODSDK

            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            var dependencyData = DataMultiLinkerAction.GetEntry (dependencyActionKey);
            if (dependencyData == null)
                return false;

            bool available = DataHelperAction.IsAvailable (dependencyData, unitPersistent);
            return available;

            #else
            return false;
            #endif
        }
    }
}