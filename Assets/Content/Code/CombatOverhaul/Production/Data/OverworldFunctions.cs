using System.Collections.Generic;
using System;
using PhantomBrigade.Data;
using PhantomBrigade.Functions;
using UnityEngine;

namespace PhantomBrigade.Overworld
{
    public static class FunctionUtility
    {
        public static void TryReplaceInDictionary<T> (IDictionary<string, T> dictionary, string from, string to, string context)
        {
            if (dictionary != null && dictionary.ContainsKey (from))
            {
                Debug.LogWarning ($"{context}: Replacing entity key from {from} -> {to}");
                var value = dictionary[from];
                dictionary.Remove (from);
                dictionary.Add (to, value);
            }
        }
        
        public static void TryReplaceInString (ref string s, string from, string to, string context)
        {
            if (s != null && s == from)
            {
                Debug.LogWarning ($"{context}: Replacing entity key from {from} -> {to}");
                s = to;
            }
        }
        
        public static void ReplaceInFunction (Type functionType, string from, string to, Action<object,string> action)
        {
            if (action == null)
                return;

            if (typeof (IOverworldEventFunction).IsAssignableFrom (functionType))
            {
                void ReplaceInOverworldEventFunctions (List<IOverworldEventFunction> functions, string context)
                {
                    if (functions == null)
                        return;
                    
                    foreach (var function in functions)
                    {
                        if (function != null && function.GetType () == functionType)
                            action.Invoke (function, context);
                    }
                }
                
                var dataEvents = DataMultiLinkerOverworldEvent.data;
                foreach (var kvp in dataEvents)
                {
                    var eventData = kvp.Value;
                    if (eventData == null)
                        continue;

                    if (eventData.steps != null)
                    {
                        foreach (var kvp2 in eventData.steps)
                        {
                            var step = kvp2.Value;
                            ReplaceInOverworldEventFunctions (step?.functions, $"Event {kvp.Key}, step {kvp2.Key}");
                        }
                    }

                    if (eventData.options != null)
                    {
                        foreach (var kvp2 in eventData.options)
                        {
                            var option = kvp2.Value;
                            ReplaceInOverworldEventFunctions (option?.functions, $"Event {kvp.Key}, embedded option {kvp2.Key}");
                        }
                    }
                }
                
                var dataOptions = DataMultiLinkerOverworldEventOption.data;
                foreach (var kvp in dataOptions)
                {
                    var optionData = kvp.Value;
                    ReplaceInOverworldEventFunctions (optionData?.functions, $"Shared option {kvp.Key}");
                }
            }

            if (typeof (IOverworldActionFunction).IsAssignableFrom (functionType))
            {
                void ReplaceInOverworldActionFunctions (DataBlockOverworldActionChange change, string context)
                {
                    if (change == null || change.functions == null)
                        return;
                    
                    foreach (var function in change.functions)
                    {
                        if (function != null && function.GetType () == functionType)
                            action.Invoke (function, context);
                    }
                }
                
                var dataActions = DataMultiLinkerOverworldAction.data;
                foreach (var kvp in dataActions)
                {
                    var actionData = kvp.Value;
                    if (actionData == null)
                        continue;

                    ReplaceInOverworldActionFunctions (actionData.changesOnStart, $"Action {kvp.Key}, on start");
                    ReplaceInOverworldActionFunctions (actionData.changesOnCancellation, $"Action {kvp.Key}, on cancel");
                    ReplaceInOverworldActionFunctions (actionData.changesOnTermination, $"Action {kvp.Key}, on terminate");
                    ReplaceInOverworldActionFunctions (actionData.changesOnCompletion, $"Action {kvp.Key}, on completion");
                }
            }

            if (typeof (IOverworldFunction).IsAssignableFrom (functionType))
            {
                void ReplaceInOverworldFunctions (List<IOverworldFunction> functions, string context)
                {
                    if (functions == null)
                        return;
                    
                    foreach (var function in functions)
                    {
                        if (function != null && function.GetType () == functionType)
                            action.Invoke (function, context);
                    }
                }
                
                var dataTutorials = DataMultiLinkerTutorial.data;
                foreach (var kvp in dataTutorials)
                {
                    var tutorial = kvp.Value;
                    if (tutorial == null)
                        continue;

                    if (tutorial.pages != null)
                    {
                        for (int i = 0, count = tutorial.pages.Count; i < count; ++i)
                        {
                            var page = tutorial.pages[i];
                            ReplaceInOverworldFunctions (page?.effectsOverworld?.functions, $"Tutorial {kvp.Key}, page {i} effects");
                        }
                    }
                    
                    ReplaceInOverworldFunctions (tutorial.effectsOnEnd?.effectsOverworld?.functions, $"Tutorial {kvp.Key}, effects on end");
                }
            }
            
            if (typeof (ICombatFunction).IsAssignableFrom (functionType))
            {
                void ReplaceInCombatFunctions (List<ICombatFunction> functions, string context)
                {
                    if (functions == null)
                        return;
                    
                    foreach (var function in functions)
                    {
                        if (function != null && function.GetType () == functionType)
                            action.Invoke (function, context);
                    }
                }
                
                var dataTutorials = DataMultiLinkerTutorial.data;
                foreach (var kvp in dataTutorials)
                {
                    var tutorial = kvp.Value;
                    if (tutorial == null)
                        continue;

                    if (tutorial.pages != null)
                    {
                        for (int i = 0, count = tutorial.pages.Count; i < count; ++i)
                        {
                            var page = tutorial.pages[i];
                            ReplaceInCombatFunctions (page?.effectsCombat?.functions, $"Tutorial {kvp.Key}, page {i} effects");
                        }
                    }
                    
                    ReplaceInCombatFunctions (tutorial.effectsOnEnd?.effectsCombat?.functions, $"Tutorial {kvp.Key}, effects on end");
                }

                var dataScenarios = DataMultiLinkerScenario.data;
                foreach (var kvp in dataScenarios)
                {
                    var scenario = kvp.Value;
                    if (scenario == null)
                        continue;

                    if (scenario.stepsProc != null)
                    {
                        foreach (var kvp2 in scenario.stepsProc)
                        {
                            var step = kvp2.Value;
                            ReplaceInCombatFunctions (step?.functions, $"Scenario {kvp.Key}, step {kvp2.Key}");
                        }
                    }

                    if (scenario.statesProc != null)
                    {
                        foreach (var kvp2 in scenario.statesProc)
                        {
                            var state = kvp2.Value;
                            if (state != null && state.reactions != null && state.reactions.effectsPerIncrement != null)
                            {
                                foreach (var kvp3 in state.reactions.effectsPerIncrement)
                                {
                                    var effect = kvp3.Value;
                                    ReplaceInCombatFunctions (effect?.functions, $"Scenario {kvp.Key}, state {kvp2.Key}, reaction {kvp3.Key}");
                                }
                            }
                        }
                    }

                    /*
                    if (scenario.customOutcomeReactions != null && scenario.customOutcomeReactions.reactions != null)
                    {
                        for (int i = 0; i < scenario.customOutcomeReactions.reactions.Count; ++i)
                        {
                            var effect = scenario.customOutcomeReactions.reactions[i];
                            if (effect.reaction == null)
                                continue;
                            
                            ReplaceInCombatFunctions (effect?.reaction.functions, $"Scenario {kvp.Key}, outcome reaction {i}");
                        }
                    }
                    */
                }
            }
        }
    }
}