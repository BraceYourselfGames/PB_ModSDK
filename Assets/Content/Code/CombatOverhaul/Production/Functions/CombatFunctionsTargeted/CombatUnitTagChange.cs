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
}