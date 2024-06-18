using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitStatusGroup : ICombatUnitValidationFunction
    {
        public enum StatusCondition
        {
            NonePresent,
            OnePresent,
            AllPresent
        }

        public StatusCondition condition = StatusCondition.NonePresent;
        
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        [HideLabel]
        public HashSet<string> keys;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;

            if (keys == null || keys.Count == 0)
                return false;

            int countChecked = keys.Count;
            int countPresent = 0;

            if (unitCombat.hasUnitStatusStates)
            {
                foreach (var key in keys)
                {
                    bool statusPresent = UnitStatusUtility.IsStatusPresent (unitCombat, key);
                    if (statusPresent)
                        countPresent += 1;
                }
            }

            bool passed = false;

            if (condition == StatusCondition.NonePresent)
                passed = countPresent == 0;
            
            else if (condition == StatusCondition.OnePresent)
                passed = countPresent >= 1;
            
            else if (condition == StatusCondition.AllPresent)
                passed = countPresent == countChecked;

            return passed;

            #else
            return false;
            #endif
        }
    }
}