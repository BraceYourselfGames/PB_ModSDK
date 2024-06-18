using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Game;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldActionUI
    {
        [LabelText ("Color")]
        public Color color;

        [LabelText ("Minified")]
        public bool minified;

        public bool locationFromTarget = false;

        [YamlIgnore]
        [LabelText ("Name")]
        public string textName;
        
        [YamlIgnore]
        [LabelText ("Custom context")]
        public string textContext;
        
        [YamlIgnore]
        [LabelText ("Start msg.")]
        public string textStart;
        
        [YamlIgnore]
        [LabelText ("Cancel msg.")]
        public string textCancel;
        
        [YamlIgnore]
        [LabelText ("End msg. / Desc.")]
        public string textEnd;
        
        [YamlIgnore]
        [HideLabel][TextArea]
        public string textDesc;
    }

    [Serializable]
    public class DataBlockOverActionAudio
    {
        [ValueDropdown("GetAudioKeys")]
        public string onStartEvent;

        [ValueDropdown("GetAudioKeys")]
        public string onCompleteEvent;

        [ValueDropdown("GetAudioKeys")]
        public string onAborted;

        [ValueDropdown("GetAudioKeys")]
        public string onHoverStart;
        
        [ValueDropdown("GetAudioKeys")]
        public string onHoverEnd;
        
        #if UNITY_EDITOR
        private IEnumerable<string> GetAudioKeys () => AudioEvents.GetKeys ();
        #endif
    }

    [Serializable]
    public class DataBlockOverworldActionTime
    {
        public float duration;
        
        // We can extend this in the future, e.g. gating progress similarly to gating entry to events:
        // by narrative memory, by time of day etc.
    }

    public class DataBlockOverworldActionChange
    {
        /*
        [DropdownReference]
        [OnValueChanged ("RefreshParentsFromEdit", true)]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockOverworldActionCall ()")]
        public List<DataBlockOverworldActionCall> calls;
        */
        
        [DropdownReference]
        [OnValueChanged ("RefreshParentsFromEdit", true)]
        public List<IOverworldActionFunction> functions;

        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeGroupAction ()")]
        public List<DataBlockMemoryChangeGroupAction> memoryChanges;
        
        /*
        [DropdownReference]
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockOverworldActionInstanceData ()")]
        public List<DataBlockOverworldActionInstanceData> actionsCreated;
        */
        
        [YamlIgnore, HideInInspector]
        public DataContainerOverworldAction parentAction;

        public void RefreshParentsFromEdit () => 
            RefreshParents (false);
        
        public void RefreshParents (bool onAfterDeserialization)
        {
            /*
            if (calls != null)
            {
                foreach (var call in calls)
                {
                    if (call != null)
                        call.RefreshParents (parentAction, onAfterDeserialization);
                }
            }
            */
        }

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldActionChange () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [Serializable] [LabelWidth (200f)]
    public class DataContainerOverworldAction : DataContainerWithText, IDataContainerTagged, ICustomDataProvider
    {
        [ShowIf ("IsUIVisible")][LabelText ("UI")]
        public DataBlockOverworldActionUI ui;

        [ShowIf("IsAudioVisible")]
        [DropdownReference]
        public DataBlockOverActionAudio audio;

        [DropdownReference]
        public DataBlockOverworldActionTime time;

        [Space (8f)]
        [ShowIf ("IsCoreVisible")]
        public int cost = 0;
        
        [ShowIf ("IsCoreVisible")]
        public int limitPerOwner = 0;
        
        [ShowIf ("IsCoreVisible")]
        public int progressGroup = 0;
        
        [ShowIf ("IsCoreVisible")]
        [ValueDropdown ("@DataMultiLinkerBaseStat.data.Keys"), InlineButtonClear]
        public string progressJobsStat;
        
        [ShowIf ("IsCoreVisible")]
        [ValueDropdown ("@DataMultiLinkerBaseStat.data.Keys"), InlineButtonClear]
        public string progressSpeedStat;

        [ShowIf ("IsCoreVisible")]
		[Tooltip("Require combat participation for victory and defeat flag checks")]
        public bool requireCombatParticipation = false;

        [ShowIf ("IsCoreVisible")]
        public bool completeOnCombatVictory = false;
        
        [ShowIf ("IsCoreVisible")]
        public bool terminateOnCombatDefeat = false;
        
        [ShowIf ("IsCoreVisible")]
        public bool discardOnWorldChange = true;

        [ShowIf ("IsCoreVisible")]
        [LabelText ("Deployment")]
        public bool requiresDeployment;

        [ShowIf ("IsCoreVisible")]
        [PropertyTooltip ("Whether this action can be cancelled from UI")]
        public bool cancellationAllowed;
        
        [ShowIf ("IsCoreVisible")]
        [PropertyTooltip ("Whether resources and charges attached to this action are refunded when action is destroyed due to user input (pressing cancel button in action UI)")]
        public bool refundOnCancellation = true;
        
        [ShowIf ("IsCoreVisible")]
        [PropertyTooltip ("Whether resources and charges attached to this action are refunded when action is destroyed directly - without being compelted or cancelled")]
        public bool refundOnTermination = true;
        
        [ShowIf ("IsCoreVisible")]
        [LabelText ("Refresh on termination")]
        public bool refreshTargetOnTermination = false;

        [ShowIf ("IsCoreVisible")]
        [PropertyTooltip ("Whether this action type can be paused by togging stealthy/fast movement)")]
        public bool haltedByMovementModes = false;
        
        [ShowIf ("IsCoreVisible")]
        [PropertyTooltip ("Whether this action type can be paused by simulation lock countdown)")]
        public bool haltedByLock = true;
        
        [ShowIf ("AreChangesVisible")]
        [PropertyTooltip ("These calls will be invoked when action is created")]
        [DropdownReference (true)]
        public DataBlockOverworldActionChange changesOnStart;
        
        [ShowIf ("AreChangesVisible")]
        [PropertyTooltip ("These calls will be invoked when action is destroyed due to user input (pressing cancel button in action UI)")]
        [DropdownReference (true)]
        public DataBlockOverworldActionChange changesOnCancellation;
        
        [ShowIf ("AreChangesVisible")]
        [PropertyTooltip ("These calls will be invoked when action is destroyed directly - without being completed or cancelled")]
        [DropdownReference (true)]
        public DataBlockOverworldActionChange changesOnTermination;
        
        [ShowIf ("AreChangesVisible")]
        [PropertyTooltip ("These calls will be invoked when action is destroyed due to successful completion (from countdown reaching 0 or direct call)")]
        [DropdownReference (true)]
        public DataBlockOverworldActionChange changesOnCompletion;

        [ShowIf ("AreTagsVisible")]
        [ShowIf ("@DataMultiLinkerOverworldAction.Presentation.showTags")]
        [ValueDropdown ("@DataMultiLinkerOverworldAction.tags")]
        [DropdownReference]
        public HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("IsCustomDataVisible")]
        [DropdownReference]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> customFlags;
        
        [ShowIf ("IsCustomDataVisible")]
        [DropdownReference]
        public SortedDictionary<string, int> customInts;
        
        [ShowIf ("IsCustomDataVisible")]
        [DropdownReference]
        public SortedDictionary<string, float> customFloats;
        
        [ShowIf ("IsCustomDataVisible")]
        [DropdownReference]
        public SortedDictionary<string, string> customStrings;




        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            RefreshParentsInChanges (changesOnStart);
            RefreshParentsInChanges (changesOnCancellation);
            RefreshParentsInChanges (changesOnTermination);
            RefreshParentsInChanges (changesOnCompletion);
        }
        
        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);
            
            FunctionUtility.ReplaceInFunction (typeof (StartAction), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (StartAction)function;
                var spawnData = functionTyped.data;
                if (spawnData != null)
                    FunctionUtility.TryReplaceInString (ref spawnData.key, keyOld, keyNew, context);
            });
            
            FunctionUtility.ReplaceInFunction (typeof (CancelAction), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (CancelAction)function;
                FunctionUtility.TryReplaceInString (ref functionTyped.actionKey, keyOld, keyNew, context);
            });
            
            FunctionUtility.ReplaceInFunction (typeof (TerminateAction), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (TerminateAction)function;
                FunctionUtility.TryReplaceInString (ref functionTyped.actionKey, keyOld, keyNew, context);
            });

            FunctionUtility.ReplaceInFunction (typeof (CompleteAction), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (CompleteAction)function;
                FunctionUtility.TryReplaceInString (ref functionTyped.actionKey, keyOld, keyNew, context);
            });

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
                        OnKeyReplacementInEventFunctions (step.functions, keyOld, keyNew, context);
                        OnKeyReplacementInActionChecks (step.check?.action, keyOld, keyNew, context);
                    }
                }

                if (eventData.options != null)
                {
                    foreach (var kvp2 in eventData.options)
                    {
                        var option = kvp2.Value;
                        var context = $"Event {kvp.Key}, embedded option {kvp2.Key}";
                        OnKeyReplacementInEventFunctions (option.functions, keyOld, keyNew, context);
                        OnKeyReplacementInActionChecks (option.check?.action, keyOld, keyNew, context);
                    }
                }

                if (eventData.actions != null)
                {
                    if (eventData.actions.resetCompletionOnOwner.Contains (keyOld))
                    {
                        Debug.LogWarning ($"Event {kvp.Key}, action link on owner | Replacing action key: {keyOld} -> {keyNew})");
                        eventData.actions.resetCompletionOnOwner.Add (keyOld);
                        eventData.actions.resetCompletionOnOwner.Add (keyNew);
                    }
                    
                    if (eventData.actions.resetCompletionOnTarget.Contains (keyOld))
                    {
                        Debug.LogWarning ($"Event {kvp.Key}, action link on target | Replacing action key: {keyOld} -> {keyNew})");
                        eventData.actions.resetCompletionOnTarget.Add (keyOld);
                        eventData.actions.resetCompletionOnTarget.Add (keyNew);
                    }
                }
            }

            var dataOptions = DataMultiLinkerOverworldEventOption.data;
            foreach (var kvp in dataOptions)
            {
                var option = kvp.Value;
                var context = $"Shared option {kvp.Key}";
                OnKeyReplacementInEventFunctions (option.functions, keyOld, keyNew, context);
                OnKeyReplacementInActionChecks (option.check?.action, keyOld, keyNew, context);
            }

            var dataActions = DataMultiLinkerOverworldAction.data;
            foreach (var kvp in dataActions)
            {
                var action = kvp.Value;
                OnKeyReplacementInActionFunctions (action.changesOnStart?.functions, keyOld, keyNew, $"Action {kvp.Key}, on start");
                OnKeyReplacementInActionFunctions (action.changesOnCancellation?.functions, keyOld, keyNew, $"Action {kvp.Key}, on cancellation");
                OnKeyReplacementInActionFunctions (action.changesOnTermination?.functions, keyOld, keyNew, $"Action {kvp.Key}, on termination");
                OnKeyReplacementInActionFunctions (action.changesOnCompletion?.functions, keyOld, keyNew, $"Action {kvp.Key}, on completion");
            }
        }
        
        private void OnKeyReplacementInEventFunctions (List<IOverworldEventFunction> functions, string keyOld, string keyNew, string context)
        {
            if (functions == null)
                return;
            
            for (int i = 0; i < functions.Count; ++i)
            {
                var function = functions[i];

                if (function is CompleteAction completeAction && completeAction.actionKey == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (CompleteAction) | Replacing action key: {keyOld} -> {keyNew})");
                    completeAction.actionKey = keyNew;
                }
                else if (function is CancelAction cancelAction && cancelAction.actionKey == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (CancelAction) | Replacing action key: {keyOld} -> {keyNew})");
                    cancelAction.actionKey = keyNew;
                }
                else if (function is TerminateAction terminateAction && terminateAction.actionKey == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (TerminateAction) | Replacing action key: {keyOld} -> {keyNew})");
                    terminateAction.actionKey = keyNew;
                }
                else if (function is StartAction startAction && startAction.data != null && startAction.data.key == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (StartAction) | Replacing action key: {keyOld} -> {keyNew})");
                    startAction.data.key = keyNew;
                }
            }
        }

        private void OnKeyReplacementInActionFunctions (List<IOverworldActionFunction> functions, string keyOld, string keyNew, string context)
        {
            if (functions == null)
                return;
            
            for (int i = 0; i < functions.Count; ++i)
            {
                var function = functions[i];

                if (function is CompleteAction completeAction && completeAction.actionKey == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (CompleteAction) | Replacing action key: {keyOld} -> {keyNew})");
                    completeAction.actionKey = keyNew;
                }
                else if (function is CancelAction cancelAction && cancelAction.actionKey == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (CancelAction) | Replacing action key: {keyOld} -> {keyNew})");
                    cancelAction.actionKey = keyNew;
                }
                else if (function is TerminateAction terminateAction && terminateAction.actionKey == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (TerminateAction) | Replacing action key: {keyOld} -> {keyNew})");
                    terminateAction.actionKey = keyNew;
                }
                else if (function is StartAction startAction && startAction.data != null && startAction.data.key == keyOld)
                {
                    Debug.LogWarning ($"{context} | Call {i} (StartAction) | Replacing action key: {keyOld} -> {keyNew})");
                    startAction.data.key = keyNew;
                }
            }
        }
        
        private void OnKeyReplacementInActionChecks (DataBlockOverworldEventCheckAction check, string keyOld, string keyNew, string context)
        {
            if (check == null || check.actions == null)
                return;

            for (int i = 0; i < check.actions.Count; ++i)
            {
                var block = check.actions[i];
                if (block != null && block.key == keyOld)
                {
                    Debug.LogWarning ($"{context} | Action check {i} | Replacing action key: {keyOld} -> {keyNew})");
                    block.key = keyNew;
                }
            }
        }




        private void RefreshParentsInChanges (DataBlockOverworldActionChange change)
        {
            if (change != null)
                change.RefreshParents (true);
        }

        public HashSet<string> GetTags (bool processed)
        {
            return tags;
        }
        
        public bool IsHidden () => false;
        


        public IEnumerable<string> GetFlagKeys () => customFlags?.Keys;
        public IEnumerable<string> GetIntKeys () => customInts?.Keys;
        public IEnumerable<string> GetFloatKeys () => customFloats?.Keys;
        public IEnumerable<string> GetStringKeys () => customStrings?.Keys;

        public string GetKey () => 
            key;

        public bool IsFlagPresent (string key)
        {
            var found = 
                customFlags != null && 
                !string.IsNullOrEmpty (key) && 
                customFlags.ContainsKey (key);
        
            return found;
        }
        
        public bool TryGetInt (string key, out int result, int fallback = default)
        {
            var found = 
                customInts != null && 
                !string.IsNullOrEmpty (key) && 
                customInts.ContainsKey (key);
        
            result = found ? customInts[key] : fallback;
            return found;
        }
        
        public bool TryGetFloat (string key, out float result, float fallback = default)
        {
            var found = 
                customFloats != null && 
                !string.IsNullOrEmpty (key) && 
                customFloats.ContainsKey (key);
        
            result = found ? customFloats[key] : fallback;
            return found;
        }
        
        public bool TryGetString (string key, out string result, string fallback = default)
        {
            var found = 
                customStrings != null && 
                !string.IsNullOrEmpty (key) && 
                customStrings.ContainsKey (key);
        
            result = found ? customStrings[key] : fallback;
            return found;
        }

        public override void ResolveText ()
        {
            if (ui == null)
                return;

            ui.textName = DataManagerText.GetText (TextLibs.overworldEntityActions, $"{key}__1_name", true);
            ui.textContext = DataManagerText.GetText (TextLibs.overworldEntityActions, $"{key}__2_context", true);
            ui.textStart = DataManagerText.GetText (TextLibs.overworldEntityActions, $"{key}__3_start", true);
            ui.textCancel = DataManagerText.GetText (TextLibs.overworldEntityActions, $"{key}__4_cancel", true);
            ui.textEnd = DataManagerText.GetText (TextLibs.overworldEntityActions, $"{key}__5_finish", true);
            ui.textDesc = DataManagerText.GetText (TextLibs.overworldEntityActions, $"{key}__6_desc", true);
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible () || ui == null)
                return;
    
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntityActions, $"{key}__1_name", ui.textName);
            DataManagerText.TryAddingTextToLibrary(TextLibs.overworldEntityActions, $"{key}__2_context", ui.textContext);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntityActions, $"{key}__3_start", ui.textStart);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntityActions, $"{key}__4_cancel", ui.textCancel);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntityActions, $"{key}__5_finish", ui.textEnd);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldEntityActions, $"{key}__6_desc", ui.textDesc);
        }
       
        [ShowInInspector]
        [ShowIf ("@IsCustomDataVisible || AreChangesVisible || IsAudioVisible || AreTagsVisible")]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerOverworldAction () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private static bool IsUIVisible => DataMultiLinkerOverworldAction.Presentation.showUI;
        private static bool IsCoreVisible => DataMultiLinkerOverworldAction.Presentation.showCore;
        private static bool AreChangesVisible => DataMultiLinkerOverworldAction.Presentation.showChanges;
        private static bool AreTagsVisible => DataMultiLinkerOverworldAction.Presentation.showTags;
        private static bool IsCustomDataVisible => DataMultiLinkerOverworldAction.Presentation.showCustomData;
        private static bool IsAudioVisible => DataMultiLinkerOverworldAction.Presentation.showAudio;
        
        public static readonly List<string> progressTypes = new List<string> {ProgressDisplayType.Accumulating, ProgressDisplayType.Diminishing};
        
        #endif
        #endregion
    }

    public static class ProgressDisplayType
    {
        public static string Accumulating = "Accumulating";
        public static string Diminishing = "Diminishing";
    }
}