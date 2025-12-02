using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitTagChange : ICombatFunctionTargeted
    {
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tagChanges = new SortedDictionary<string, bool> ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            ScenarioUtility.ChangeUnitScenarioTags (unitCombat, tagChanges);
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitRoleChange : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitRole.data.Keys")]
        public string role;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.hasUnitIdentification)
                return;

            var id = unitPersistent.unitIdentification;
            unitPersistent.ReplaceUnitIdentification (role, id.nameSerial, id.nameLoc, id.nameOverride);
            
            #endif
        }
    }
}