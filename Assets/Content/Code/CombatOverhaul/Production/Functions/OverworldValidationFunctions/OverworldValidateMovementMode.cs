using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateMovementMode : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Base should be in fast mode" : "Base should be in normal mode";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK
            
            var overworld = Contexts.sharedInstance.overworld;
            var movementModeKey = overworld.hasActiveMovementMode ? overworld.activeMovementMode.key : MovementModes.normal;
            bool fast = string.Equals (movementModeKey, MovementModes.fast);
            bool movementModeValid = present == fast;
            return movementModeValid;

            #else
            return false;
            #endif
        }
    }
}