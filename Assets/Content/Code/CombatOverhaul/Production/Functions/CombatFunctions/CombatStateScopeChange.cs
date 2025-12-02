using System;
using System.Collections.Generic;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class CombatStateScopeChange : ICombatFunction
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [HorizontalGroup]
        public string key;

        [HideLabel, HorizontalGroup (32f), ToggleLeft]
        public bool scoped;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (key))
                return;
            
            var combat = Contexts.sharedInstance.combat;
            var stateScope = combat.hasScenarioStateScope ? combat.scenarioStateScope.s : null;
            if (stateScope == null)
                return;
            
            var stateDefinition = ScenarioUtility.GetStateDefinition (key);
            if (stateDefinition == null)
            {
                Debug.LogWarning ($"Failed to apply state value change using key {key} - definition not found");
                return;
            }
            
            var stateShouldBeInScope = scoped;
            var stateCurrentlyInScope = stateScope.ContainsKey (key);

            if (stateCurrentlyInScope == stateShouldBeInScope)
                return;

            if (stateShouldBeInScope)
            {
                Debug.Log ($"State {key} is now in scope");
                
                var step = ScenarioUtility.GetCurrentStep ();
                var startTurn = combat.currentTurn.i;
                var startTime = combat.simulationTime.f;
                
                stateScope.Add (key, new ScenarioStateScopeMetadata ()
                {
                    entryStepKey = step.key, 
                    entryTurn = startTurn, 
                    entrySimulationTime = startTime,
                    entryRealTime = TimeCustom.unscaledTime
                });
            }
            else
            {
                Debug.Log ($"State {key} has left scope");
                stateScope.Remove (key);
            }
            
            combat.ReplaceScenarioStateScope (stateScope);
            
            #endif
        }
    }
    
    public class CombatStateScopeChangeGroup : ICombatFunction
    {
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerScenario\", \"GetStateKeys\")")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> stateScopeChanges = new SortedDictionary<string, bool> ();
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (stateScopeChanges == null)
                return;
            
            var combat = Contexts.sharedInstance.combat;
            var stateScope = combat.hasScenarioStateScope ? combat.scenarioStateScope.s : null;
            if (stateScope == null)
                return;

            bool scopeChanged = false;
            foreach (var kvp in stateScopeChanges)
            {
                var stateKey = kvp.Key;
                var stateDefinition = !string.IsNullOrEmpty(stateKey) ? ScenarioUtility.GetStateDefinition (stateKey) : null;
                if (stateDefinition == null)
                    continue;
                
                var stateShouldBeInScope = kvp.Value;
                var stateCurrentlyInScope = stateScope.ContainsKey (stateKey);

                if (stateCurrentlyInScope == stateShouldBeInScope)
                    return;

                scopeChanged = true;
                if (stateShouldBeInScope)
                {
                    Debug.Log ($"State {stateKey} is now in scope");
                
                    var step = ScenarioUtility.GetCurrentStep ();
                    var startTurn = combat.currentTurn.i;
                    var startTime = combat.simulationTime.f;
                
                    stateScope.Add (stateKey, new ScenarioStateScopeMetadata ()
                    {
                        entryStepKey = step.key, 
                        entryTurn = startTurn, 
                        entrySimulationTime = startTime,
                        entryRealTime = TimeCustom.unscaledTime
                    });
                }
                else
                {
                    Debug.Log ($"State {stateKey} has left scope");
                    stateScope.Remove (stateKey);
                }
            }
            
            if (scopeChanged)
                combat.ReplaceScenarioStateScope (stateScope);
            
            #endif
        }
    }
}

