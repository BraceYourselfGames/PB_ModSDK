using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatStateValueChange : ICombatFunction
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [HorizontalGroup]
        public string key;

        [HideLabel, HorizontalGroup (32f), ToggleLeft]
        public bool value;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (key))
                return;
            
            var combat = Contexts.sharedInstance.combat;
            var stateValues = combat.hasScenarioStateValues ? combat.scenarioStateValues.s : null;
            if (stateValues == null)
                return;
            
            var stateDefinition = ScenarioUtility.GetStateDefinition (key);
            if (stateDefinition == null || stateDefinition.evaluated)
            {
                Debug.LogWarning ($"Failed to apply state value change using key {key} - definition not found or prohibits manual control");
                return;
            }

            Debug.Log ($"Modified state {key} to value {value}");
            stateValues[key] = value;
            combat.ReplaceScenarioStateValues (stateValues);
            
            var scenario = ScenarioUtility.GetCurrentScenarioAndStep (out var stepCurrent);
            if (scenario == null || stepCurrent == null)
                return;

            if (stepCurrent.transitions != null)
            {
                var transitionMode = stepCurrent.transitions.transitionMode;
                if (transitionMode == ScenarioTransitionEvaluation.OnStateChange)
                {
                    Debug.Log ($"Step {stepCurrent.key} requires transition evaluation on any state change, triggering it...");
                    combat.isScenarioTransitionRefresh = true;
                }
            }
            
            #endif
        }
    }
}