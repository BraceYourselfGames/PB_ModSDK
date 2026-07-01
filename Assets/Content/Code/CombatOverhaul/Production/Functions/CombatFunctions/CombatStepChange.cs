using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatStepChange : ICombatFunction
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStepKeys\")")]
        public string stepKey;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            ScenarioUtility.InvokeTransition (stepKey, null);
            
            #endif
        }
    }
}