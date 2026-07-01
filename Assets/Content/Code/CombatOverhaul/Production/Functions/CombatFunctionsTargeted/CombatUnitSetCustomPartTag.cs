using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetCustomPartTag : DataBlockSubcheckBool, ICombatFunctionTargeted
    {
        protected override string GetLabel () => present ? "Will be added" : "Will be removed";
    
        [PropertyOrder (-1)]
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        
        [PropertyOrder (-1)]
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (EquipmentCustomTags), false)")]
        public string tag;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            EquipmentUtility.SetCustomPartTag (unitPersistent, socket, tag, present);

            #endif
        }
    }
}