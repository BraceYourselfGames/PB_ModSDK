using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitStatusDurationReset : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;

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

                state.durationFull = definition.durationFull.f;
            }
            
            #endif
        }
    }
    
    [Serializable]
    public class CombatUnitStatusCountdownReset : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (!unitCombat.hasUnitStatusStates)
                return;

            var definition = DataMultiLinkerUnitStatus.GetEntry (key);
            if (definition == null || definition.durationFull == null || definition.durationFull.f <= 0f)
                return;

            var combat = Contexts.sharedInstance.combat;
            var time = combat.hasSimulationTime ? combat.simulationTime.f : 0f;

            foreach (var state in unitCombat.unitStatusStates.s)
            {
                if (state.keyHash != definition.keyHash)
                    continue;

                state.timeOnStart = time;
            }
            
            #endif
        }
    }
}