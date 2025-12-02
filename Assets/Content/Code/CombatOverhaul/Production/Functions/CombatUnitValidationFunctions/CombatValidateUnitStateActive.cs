using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitStateActive : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        protected override string GetLabel () => present ? "Unit should be active" : "Unit should be inactive";
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            
            bool active = ScenarioUtility.IsUnitActive (unitPersistent, unitCombat, false, true, true);
            bool validated = active == present;
            return validated;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitStateMobile : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        protected override string GetLabel () => present ? "Unit should be mobile" : "Unit should not be mobile";
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            
            bool active = ScenarioUtility.IsUnitActive (unitPersistent, unitCombat, false, true, true);
            bool mobile = active && !CombatUtilities.IsUnitImmobilized (unitPersistent);
            bool validated = mobile == present;
            return validated;

            #else
            return false;
            #endif
        }
    }
}