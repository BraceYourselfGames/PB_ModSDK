using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitBlackboardSet : ICombatFunctionTargeted
    {
        public string key = string.Empty;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;
            
            if (string.IsNullOrEmpty (key))
                return;
            
            ScenarioUtility.SaveEntityToGlobalBlackboard (key, unitCombat);
            
            #endif
        }
    }
}