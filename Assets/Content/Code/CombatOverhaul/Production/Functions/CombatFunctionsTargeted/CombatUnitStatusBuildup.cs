using System;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitStatusBuildup : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;
        public float value = 0f;
        public UnitStatusSource source = UnitStatusSource.Function;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            UnitStatusUtility.OffsetBuildup (unitCombat, key, value, source);
            
            #endif
        }
    }
}