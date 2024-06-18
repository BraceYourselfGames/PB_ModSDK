using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyMemoryCombat : ICombatFunction
    {
        [HideLabel, HideReferenceObjectPicker]
        public DataBlockMemoryChangeGroupScenario data = new DataBlockMemoryChangeGroupScenario ();

        public void Run ()
        {
            #if !PB_MODSDK
            
            ScenarioUtility.ApplyMemoryChangeForScenario (data);
            
            #endif
        }
    }
}