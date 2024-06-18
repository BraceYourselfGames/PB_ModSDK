using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitStatusCount : ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        [HideLabel]
        public string key;

        [HideLabel]
        public DataBlockOverworldEventSubcheckInt check = new DataBlockOverworldEventSubcheckInt ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;

            bool statusPresent = UnitStatusUtility.IsStatusPresent (unitCombat, key, out int count);
            bool passed = check != null && check.IsPassed (statusPresent, count);
            return passed;

            #else
            return false;
            #endif
        }
    }
}