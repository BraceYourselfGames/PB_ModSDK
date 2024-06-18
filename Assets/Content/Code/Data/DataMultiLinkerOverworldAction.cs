using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldAction : DataMultiLinker<DataContainerOverworldAction>
    {
        public DataMultiLinkerOverworldAction ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldEntityActions); 
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showUI = true;

            [ShowInInspector]
            public static bool showAudio = true;

            [ShowInInspector]
            public static bool showCore = true;

            [ShowInInspector]
            public static bool showChanges = true;

            [ShowInInspector]
            public static bool showCustomData = true;

            [ShowInInspector]
            public static bool showTags = true;

            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldAction.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldAction.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();

        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }

        [FoldoutGroup ("Utilities", false)]
        [Button]
        private static void LogPersistedOnWorldReset ()
        {
            var listPreserved = new List<string> ();
            var listDiscarded = new List<string> ();
            
            foreach (var kvp in data)
            {
                var action = kvp.Value;
                var report = action.ui != null && !string.IsNullOrEmpty (action.ui.textName) ? $"{kvp.Key} ({action.ui.textName})" : kvp.Key;
                
                if (action.discardOnWorldChange)
                    listDiscarded.Add (report);
                else
                    listPreserved.Add (report);
            }
            
            Debug.Log ($"Preserved actions ({listPreserved.Count}):\n{listPreserved.ToStringFormatted (true, multilinePrefix: "- ")}");
            Debug.Log ($"Discarded actions ({listDiscarded.Count}):\n{listDiscarded.ToStringFormatted (true, multilinePrefix: "- ")}");
        }

        /*
        [Button, FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        public void PrintTargetActions ()
        {
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
                        var context = $"Event {kvp.Key}, step {kvp2.Key}";
                        PrintTargetActions (step.actionsCreated, context);
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        var context = $"Event {kvp.Key}, embedded option {kvp2.Key}";
                        PrintTargetActions (option.actionsCreated, context);
                    }
                }
            }

            var dataOptions = DataMultiLinkerOverworldEventOption.data;
            foreach (var kvp in dataOptions)
            {
                var option = kvp.Value;
                var context = $"Shared option {kvp.Key}";
                PrintTargetActions (option.actionsCreated, context);
            }

            var dataActions = DataMultiLinkerOverworldAction.data;
            foreach (var kvp in dataActions)
            {
                var action = kvp.Value;
                PrintTargetActions (action.changesOnStart?.actionsCreated, $"Action {kvp.Key}, on start");
                PrintTargetActions (action.changesOnCancellation?.actionsCreated, $"Action {kvp.Key}, on cancellation");
                PrintTargetActions (action.changesOnTermination?.actionsCreated, $"Action {kvp.Key}, on termination");
                PrintTargetActions (action.changesOnCompletion?.actionsCreated, $"Action {kvp.Key}, on completion");
            }
        }

        private void PrintTargetActions (List<DataBlockOverworldActionInstanceData> actionsCreated, string context)
        {
            if (actionsCreated == null || actionsCreated.Count == 0)
                return;

            for (int i = 0; i < actionsCreated.Count; ++i)
            {
                var block = actionsCreated[i];
                if (block == null)
                    continue;

                if (block.owner != ActionOwnerProvider.Target)
                    continue;
                
                Debug.LogWarning ($"{context} | Action creation {i} | Owned by: Target | Key: {block.key}");
            }
        }
        */
        
        /*
        [FoldoutGroup ("Utilities", false)]
        [Button (ButtonSizes.Large), PropertyOrder (-10)]
        public static void LogFunctionCoverage ()
        {
            var classesAvailable = new Dictionary<string, Type> ();
            foreach (var type in Assembly.GetExecutingAssembly ().DefinedTypes)
            {
                if (typeof (IOverworldActionFunction).IsAssignableFrom (type))
                    classesAvailable[type.Name] = type;
            }

            var functionKeysOld = FieldReflectionUtility.GetConstantStringFieldNames (typeof (OverworldActionFunctionKeys));
            var functionKeysMissing = new Dictionary<string, List<string>> ();
            var functionKeysImplemented = new Dictionary<string, List<string>> ();
            int functionKeysCovered = 0;

            foreach (var functionKey in functionKeysOld)
            {
                bool implemented = classesAvailable.ContainsKey (functionKey);
                if (implemented)
                    functionKeysCovered += 1;
                
                var dict = implemented ? functionKeysImplemented : functionKeysMissing;
                var list = new List<string> ();
                dict.Add (functionKey, list);
                
                foreach (var kvp in data)
                {
                    var action = kvp.Value;
                    TryFindFunction (action.changesOnStart, functionKey, $"{kvp.Key} (on start)", list);
                    TryFindFunction (action.changesOnCancellation, functionKey, $"{kvp.Key} (on cancellation)", list);
                    TryFindFunction (action.changesOnTermination, functionKey, $"{kvp.Key} (on termination)", list);
                    TryFindFunction (action.changesOnCompletion, functionKey, $"{kvp.Key} (on completion)", list);
                }
            }
            
            if (functionKeysMissing.Count == 0)
                Debug.Log ($"All {functionKeysOld.Count} overworld action functions covered");
            else
            {
                var textImplemented = functionKeysImplemented.ToStringFormatted (true, toStringOverride: x => $"{x.Key}:\n{x.Value.ToStringFormatted (true, multilinePrefix: "- ")}");
                var textMissing = functionKeysMissing.ToStringFormatted (true, toStringOverride: x => $"{x.Key}:\n{x.Value.ToStringFormatted (true, multilinePrefix: "- ")}");
                Debug.LogWarning ($"{functionKeysMissing.Count}/{functionKeysOld.Count} overworld action functions:\n\nMissing:\n{textMissing}\n\nImplemented:\n{textImplemented}");
            }
        }

        private static void TryFindFunction (DataBlockOverworldActionChange changes, string callKey, string context, List<string> usages)
        {
            if (changes == null)
                return;

            if (changes.calls != null)
            {
                foreach (var call in changes.calls)
                {
                    if (call == null || string.IsNullOrEmpty (call.key))
                        continue;
                    
                    if (!string.Equals (call.key, callKey))
                        continue;

                    var text = $"{context}";
                    if (call.arg1 != null)
                        text += $"\n   - A1 ({DumpArgType (call.arg1)}): {DumpArg (call.arg1)}";
                    if (call.arg2 != null)
                        text += $"\n   - A2 ({DumpArgType (call.arg2)}): {DumpArg (call.arg2)}";
                    
                    usages.Add (text);
                }
            }
        }
        
        static string DumpArgType (ActionCallArg arg)
        {
            return arg switch
            {
                ActionCallArgFloat f => "float",
                ActionCallArgInt i => "int",
                ActionCallArgString s => "string",
                _ => ""
            };
        }

        static string DumpArg (ActionCallArg arg)
        {
            return arg switch
            {
                ActionCallArgFloat f => f.value.ToString ("0.##"),
                ActionCallArgInt i => i.value.ToString (),
                ActionCallArgString s => s.value.ToString (),
                _ => ""
            };
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button (ButtonSizes.Large), PropertyOrder (-10)]
        public static void UpgradeEventFunctions ()
        {
            var functionKey = "StartOverworldEvent";
            foreach (var kvp in data)
            {
                var action = kvp.Value;
                UpgradeEventFunctionsInBlock (action.changesOnStart, functionKey, $"{kvp.Key} (on start)");
                UpgradeEventFunctionsInBlock (action.changesOnCancellation, functionKey, $"{kvp.Key} (on cancellation)");
                UpgradeEventFunctionsInBlock (action.changesOnTermination, functionKey, $"{kvp.Key} (on termination)");
                UpgradeEventFunctionsInBlock (action.changesOnCompletion, functionKey, $"{kvp.Key} (on completion)");
            }
        }

        private static void UpgradeEventFunctionsInBlock (DataBlockOverworldActionChange changes, string callKey, string context)
        {
            if (changes == null)
                return;

            if (changes.calls != null)
            {
                foreach (var call in changes.calls)
                {
                    if (call == null || string.IsNullOrEmpty (call.key))
                        continue;
                    
                    if (!string.Equals (call.key, callKey))
                        continue;

                    var eventName = call.arg1 != null ? (call.arg1 as ActionCallArgString)?.value : null;
                    var text = $"- Updating {callKey} ({eventName}) in {context}";

                    Debug.Log (text);

                    if (changes.functions == null)
                        changes.functions = new List<IOverworldActionFunction> ();
                    
                    changes.functions.Add (new StartOverworldEvent { eventKeys = new List<string> { eventName } });
                }
            }
        }
        */

        /*
        [Button, FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        public void UpdateChanges ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                
                if (c.callsOnStart != null && c.callsOnStart.Count > 0)
                    c.changesOnStart = new DataBlockOverworldActionChange { calls = c.callsOnStart };
                c.callsOnStart = null;
                
                if (c.callsOnCancellation != null && c.callsOnCancellation.Count > 0)
                    c.changesOnCancellation = new DataBlockOverworldActionChange { calls = c.callsOnCancellation };
                c.callsOnCancellation = null;
                
                if (c.callsOnTermination != null && c.callsOnTermination.Count > 0)
                    c.changesOnTermination = new DataBlockOverworldActionChange { calls = c.callsOnTermination };
                c.callsOnTermination = null;
                
                if (c.callsOnCompletion != null && c.callsOnCompletion.Count > 0)
                    c.changesOnCompletion = new DataBlockOverworldActionChange { calls = c.callsOnCompletion };
                c.callsOnCompletion = null;
            }
        }
        */
        
        /*
        [FoldoutGroup ("Utilities", false)]
        [Button ("Upgrade action instantiation calls", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeActionCalls ()
        {
            foreach (var kvp in DataMultiLinkerOverworldAction.data)
            {
                var action = kvp.Value;
                
                if (action.changesOnStart != null)
                    UpgradeActionCalls (action.changesOnStart.calls, ref action.changesOnStart.actionsCreated, $"Action (start) {action.key}", null);
                
                if (action.changesOnCancellation != null)
                    UpgradeActionCalls (action.changesOnCancellation.calls, ref action.changesOnCancellation.actionsCreated, $"Action (cancellation) {action.key}", null);
                
                if (action.changesOnTermination != null)
                    UpgradeActionCalls (action.changesOnTermination.calls, ref action.changesOnTermination.actionsCreated, $"Action (termination) {action.key}", null);
                
                if (action.changesOnCompletion != null)
                    UpgradeActionCalls (action.changesOnCompletion.calls, ref action.changesOnCompletion.actionsCreated, $"Action (completion) {action.key}", null);
            }
        }
        
        private void UpgradeActionCalls (List<DataBlockOverworldActionCall> calls, ref List<DataBlockOverworldActionInstanceData> actionsCreated, string context, DataContainerOverworldEvent eventData)
        {
            if (calls == null || calls.Count == 0)
                return;

            for (int i = calls.Count - 1; i >= 0; --i)
            {
                var call = calls[i];
                if (call == null)
                    continue;

                bool dataBased = call.key == OverworldActionFunctionKeys.CreateActionFromData;
                if (!dataBased)
                    continue;

                if (eventData == null)
                {
                    Debug.LogWarning ($"{context} | Invalid call ({i}, {call.key}) to create action - requires event for payload, isn't used from event");
                    continue;
                }
                    
                var dataKey = call.arg1 is ActionCallArgString arg1s ? arg1s.value : null;
                if (string.IsNullOrEmpty (dataKey))
                {
                    Debug.LogWarning ($"{context} | Invalid call ({i}, {call.key}) to create action - null or empty data key argument");
                    continue;
                }
            
                var instanceDataFound = eventData.TryGetActionData (dataKey, out var instanceData);
                if (!instanceDataFound)
                {
                    Debug.LogWarning ($"{context} | Invalid call ({i}, {call.key}) to create action - nothing found using data key argument {dataKey}");
                    continue;
                }
                
                if (actionsCreated == null)
                    actionsCreated = new List<DataBlockOverworldActionInstanceData> ();
                    
                Debug.LogWarning ($"{context} | Moved call ({i}, {call.key}) to action creation block | Action key: {instanceData.key}");
                calls.RemoveAt (i);
                eventData.customActionData.Remove (dataKey);
                actionsCreated.Add (instanceData);
            }
        }
        */
    }
}