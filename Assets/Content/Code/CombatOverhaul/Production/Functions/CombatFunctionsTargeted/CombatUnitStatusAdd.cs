using System;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitStatusAdd : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;
        public float durationOverride = -1f;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            UnitStatusUtility.AddStatus (unitCombat, key, UnitStatusSource.Function, durationOverride);
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitStatusAddFromStat : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;
        
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        public string stat;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            var statValue = DataHelperStats.GetCachedStatForUnit (stat, unitPersistent);
            if (statValue <= 0f)
                return;
            
            UnitStatusUtility.AddStatus (unitCombat, key, UnitStatusSource.Function, statValue);
            
            #endif
        }
    }
}