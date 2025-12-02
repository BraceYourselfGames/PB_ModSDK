using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateCountdownExpiration : DataBlockSubcheckBool, IOverworldEntityValidationFunction
    {
        protected override string GetLabel () => present ? "Should be expired" : "Should not be expired";
    
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            bool expired = false;
            
            if (overworld.hasOverworldResetCountdown)
            {
                float timeCurrent = overworld.hasSimulationTime ? overworld.simulationTime.f : 0f;
                var timer = overworld.overworldResetCountdown;
                if (timeCurrent > (timer.startTime + timer.duration))
                    expired = true;
            }

            return present == expired;

            #else
            return false;
            #endif
        }
    }
}