using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitCheck : ICombatUnitValidationFunction
    {
        [HideLabel, HideReferenceObjectPicker]
        public DataBlockScenarioSubcheckUnit check = new DataBlockScenarioSubcheckUnit ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            bool match = ScenarioUtility.IsUnitMatchingCheck (unitPersistent, unitCombat, check, true, true, false);
            return match;

            #else
            return false;
            #endif
        }
    }
}