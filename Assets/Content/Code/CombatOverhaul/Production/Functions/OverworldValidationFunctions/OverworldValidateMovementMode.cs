using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateMovementMode : IOverworldValidationFunction
    {
        public DataBlockOverworldEventSubcheckMovementMode check = new DataBlockOverworldEventSubcheckMovementMode ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK
            
            var overworld = Contexts.sharedInstance.overworld;
            var movementModeKey = overworld.hasActiveMovementMode ? overworld.activeMovementMode.key : MovementModes.normal;
            bool required = !check.not;
            bool match = check.modes.Contains (movementModeKey);
            bool movementModeValid = required == match;
            return movementModeValid;

            #else
            return false;
            #endif
        }
    }
}