using System;
using System.Collections.Generic;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitSetUntargetable : ICombatFunctionTargeted
    {
        public bool untargetable;

        private static Dictionary<string, bool> tagChanges = new Dictionary<string, bool> ();

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            tagChanges[ScenarioUnitTags.Untargetable] = untargetable;
            ScenarioUtility.ChangeUnitScenarioTags (unitCombat, tagChanges);
            
            #endif
        }
    }
}