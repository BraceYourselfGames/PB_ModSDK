using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateMemory : IOverworldEntityValidationFunction
    {
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockOverworldMemoryCheckGroup check = new DataBlockOverworldMemoryCheckGroup ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            if (check == null)
                return false;

            bool passed = check.IsPassed (entityPersistent);
            return passed;

            #else
            return false;
            #endif
        }
    }

    public class OverworldValidateModeDemo : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Should be on" : "Should be off";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK
            return present == SettingUtility.demoMode;
            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateModeTest : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Should be on" : "Should be off";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK
            return present == SettingUtility.testMode;
            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateModeDeveloper : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Should be on" : "Should be off";
        
        public bool IsValid ()
        {
            #if !PB_MODSDK
            return present == DataShortcuts.debug.developerMode;
            #else
            return false;
            #endif
        }
    }
}