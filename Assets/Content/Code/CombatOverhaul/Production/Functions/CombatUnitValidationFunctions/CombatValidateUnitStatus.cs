using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitStatusCount : ICombatUnitValidationFunction, ICombatActionValidationFunction
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
            return IsValid (unitCombat);

            #else
            return false;
            #endif
        }
        
        public bool IsValid (CombatEntity unitCombat)
        {
            #if !PB_MODSDK

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