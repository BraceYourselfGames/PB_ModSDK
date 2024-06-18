using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class DataBlockSubcheckBoolStatus : DataBlockSubcheckBool
    {
        protected override string GetLabel () => present ? "Status should be present" : "Status should be absent";
    }
    
    [Serializable]
    public class CombatValidateUnitStatus : DataBlockSubcheckBoolStatus, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        [HideLabel]
        public string key;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;

            bool statusPresent = UnitStatusUtility.IsStatusPresent (unitCombat, key);
            bool passed = statusPresent == present;

            return passed;

            #else
            return false;
            #endif
        }
    }
}