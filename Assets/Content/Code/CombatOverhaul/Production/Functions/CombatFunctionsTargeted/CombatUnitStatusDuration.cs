using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitStatusDuration : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;

        public ValueOperation operation = ValueOperation.Set;
        public float value = 0f;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (!unitCombat.hasUnitStatusStates)
                return;

            var definition = DataMultiLinkerUnitStatus.GetEntry (key);
            if (definition == null || definition.durationFull == null || definition.durationFull.f <= 0f)
                return;

            foreach (var state in unitCombat.unitStatusStates.s)
            {
                if (state.keyHash != definition.keyHash)
                    continue;

                state.durationFull = state.durationFull.ApplyOperation (operation, value);
            }
            
            #endif
        }
    }
}