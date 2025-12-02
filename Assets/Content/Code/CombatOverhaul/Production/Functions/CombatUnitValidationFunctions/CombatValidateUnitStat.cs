using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitStat : DataBlockOverworldEventSubcheckFloat, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        public string statKey;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || string.IsNullOrEmpty (statKey))
                return false;
            
            var statCache = unitPersistent.statCacheUnit.totals;
            var statValueFound = statCache.TryGetValue (statKey, out var statValue);
            bool passed = IsPassed (statValueFound, statValue);
            return passed;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitStatNormalized : DataBlockOverworldEventSubcheckFloat, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataHelperStats.GetNormalizedCombatStatKeys")]
        public string statKey;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || string.IsNullOrEmpty (statKey))
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;
            
            var valueNormalized = DataHelperStats.GetNormalizedCombatStat (unitCombat, statKey);
            bool passed = IsPassed (true, valueNormalized);
            return passed;

            #else
            return false;
            #endif
        }
    }
}