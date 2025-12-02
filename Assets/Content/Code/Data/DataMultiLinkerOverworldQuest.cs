using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldQuest : DataMultiLinker<DataContainerOverworldQuest>
    {
        public DataMultiLinkerOverworldQuest ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldQuests);
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showIsolatedEntries = true;
            
            [ShowInInspector]
            public static bool showTagCollections;

            [ShowInInspector]
            public static bool showInheritance = false;
        }

        [ShowInInspector] [HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldQuest.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldQuest.Presentation.showTagCollections")]
        [ShowInInspector] [ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();

        private static StringBuilder sb = new StringBuilder ();

        public static void OnAfterDeserialization ()
        {
            // Process every subsystem recursively first
            foreach (var kvp in data)
                ProcessRecursiveStart (kvp.Value);

            // Fill parents after recursive processing is done on all presets, ensuring lack of cyclical refs etc
            foreach (var kvp1 in data)
            {
                var presetA = kvp1.Value;
                if (presetA == null)
                    continue;

                var key = kvp1.Key;
                
                if (presetA.children != null)
                    presetA.children.Clear ();

                foreach (var kvp2 in data)
                {
                    var presetB = kvp2.Value;
                    if (presetB.parents == null || presetB.parents.Count == 0)
                        continue;

                    foreach (var link in presetB.parents)
                    {
                        if (link.key == key)
                        {
                            if (presetA.children == null)
                                presetA.children = new List<string> ();
                            presetA.children.Add (presetB.key);
                        }
                    }
                }
            }

            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);

            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
        
        private static List<DataContainerOverworldQuest> questsUpdated = new List<DataContainerOverworldQuest> ();
        
        public static void ProcessRelated (DataContainerOverworldQuest origin)
        {
            if (origin == null)
                return;

            questsUpdated.Clear ();
            questsUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var quest = GetEntry (childKey);
                    if (quest != null)
                        questsUpdated.Add (quest);
                }
            }
            
            foreach (var quest in questsUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (quest != origin)
                    quest.OnAfterDeserialization (quest.key);
            }

            foreach (var quest in questsUpdated)
                ProcessRecursiveStart (quest);

            foreach (var quest in questsUpdated)
                Postprocess (quest);
        }
        
        public static void ProcessRecursiveStart (DataContainerOverworldQuest origin)
        {
            if (origin == null)
                return;

            origin.textNameProc = null;
            origin.textDescProc = null;
            origin.coreProc = null;
            origin.tagsProc = null;
            origin.stepsProc = null;
            origin.effectsSharedProc = null;

            ProcessRecursive (origin, origin, 0);
        }
        
        public static void Postprocess (DataContainerOverworldQuest target)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data

            /*
            if (target == null)
                return;

            if (target.stepsProc == null)
                return;
            
            int i = -1;
            foreach (var kvp in target.stepsProc)
            {
                i += 1;
                kvp.Value.index = i;
            }
            */
        }

        private static void ProcessRecursive (DataContainerOverworldQuest current, DataContainerOverworldQuest root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root interaction reference while validating quest hierarchy");
                return;
            }

            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing quest {root.key}");
                return;
            }
            
            /*
            if (current.image != null && root.imageProc == null)
                root.imageProc = current.image;
            */
            
            if (current.textName != null && root.textNameProc == null)
                root.textNameProc = current.textName;
            
            if (current.textDesc != null && root.textDescProc == null)
                root.textDescProc = current.textDesc;
            
            if (current.core != null && root.coreProc == null)
                root.coreProc = current.core;
            
            if (current.tags != null && current.tags.Count > 0)
            {
                if (root.tagsProc == null)
                    root.tagsProc = new HashSet<string> ();

                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProc.Contains (tag))
                        root.tagsProc.Add (tag);
                }
            }

            if (current.steps != null && current.steps.Count > 0)
            {
                if (root.stepsProc == null)
                    root.stepsProc = new SortedDictionary<string, DataBlockOverworldQuestStep> ();

                foreach (var kvp in current.steps)
                {
                    var stepCurrent = kvp.Value;
                    if (stepCurrent == null)
                        continue;

                    var stepKey = kvp.Key;
                    if (!root.stepsProc.TryGetValue (stepKey, out var stepProc))
                    {
                        // Add a clone because generator steps will need to mutate everything
                        stepProc = new DataBlockOverworldQuestStep ();
                        root.stepsProc.Add (stepKey, stepProc);

                        // Set unserializable fields
                        stepProc.parent = root;
                        stepProc.key = stepKey;

                        // Set non-reference fields
                        // stepProc.hidden = stepCurrent.hidden;
                    }

                    // Grab references to existing objects for reference fields, where available
                    // This is useful as most of these benefit from ability to live edit mid gameplay

                    // if (stepCurrent.image != null && stepProc.image == null)
                    //     stepProc.image = stepCurrent.image;
                    
                    if (stepCurrent.index != null && stepProc.index == null)
                        stepProc.index = stepCurrent.index;

                    if (stepCurrent.textName != null && stepProc.textName == null)
                        stepProc.textName = stepCurrent.textName;
                    
                    if (stepCurrent.textDesc != null && stepProc.textDesc == null)
                        stepProc.textDesc = stepCurrent.textDesc;
                    
                    if (stepCurrent.textExit != null && stepProc.textExit == null)
                        stepProc.textExit = stepCurrent.textExit;
                    
                    if (stepCurrent.stepNext != null && stepProc.stepNext == null)
                        stepProc.stepNext = stepCurrent.stepNext;
                    
                    if (stepCurrent.countdownCustom != null && stepProc.countdownCustom == null)
                        stepProc.countdownCustom = stepCurrent.countdownCustom;

                    if (stepCurrent.conditions != null && stepCurrent.conditions.Count > 0)
                    {
                        if (stepProc.conditions == null)
                            stepProc.conditions = new SortedDictionary<string, DataBlockOverworldQuestCondition> ();
                        
                        foreach (var kvp2 in stepCurrent.conditions)
                        {
                            var conditionCurrent = kvp2.Value;
                            if (conditionCurrent == null)
                                continue;

                            var conditionKey = kvp2.Key;
                            if (!stepProc.conditions.TryGetValue (conditionKey, out var conditionProc))
                            {
                                // Add a clone because generator steps will need to mutate everything
                                conditionProc = new DataBlockOverworldQuestCondition ();
                                stepProc.conditions.Add (conditionKey, conditionProc);

                                // Set unserializable fields
                                conditionProc.textDesc = conditionCurrent.textDesc;
                                conditionProc.parent = root;
                                conditionProc.key = conditionKey;

                                // Set non-reference fields
                                conditionProc.priority = conditionCurrent.priority;
                                // conditionProc.color = conditionCurrent.color;
                            }

                            // Grab references to existing objects for reference fields, where available
                            // This is useful as most of these benefit from ability to live edit mid gameplay
                            
                            if (conditionCurrent.filterEventTypes != null && conditionProc.filterEventTypes == null)
                                conditionProc.filterEventTypes = conditionCurrent.filterEventTypes;
                            
                            if (conditionCurrent.completionTarget != null && conditionProc.completionTarget == null)
                                conditionProc.completionTarget = conditionCurrent.completionTarget;
                            
                            if (conditionCurrent.checkValue != null && conditionProc.checkValue == null)
                                conditionProc.checkValue = conditionCurrent.checkValue;

                            if (conditionCurrent.checksBase != null && conditionProc.checksBase == null)
                                conditionProc.checksBase = conditionCurrent.checksBase;
                        }
                    }
                    
                    if (stepCurrent.memoryDisplayed != null && stepProc.memoryDisplayed == null)
                        stepProc.memoryDisplayed = stepCurrent.memoryDisplayed;
                    
                    if (stepCurrent.effectsOnEntry != null && stepProc.effectsOnEntry == null)
                        stepProc.effectsOnEntry = stepCurrent.effectsOnEntry;
                    
                    if (stepCurrent.effectsOnExit != null && stepProc.effectsOnExit == null)
                        stepProc.effectsOnExit = stepCurrent.effectsOnExit;
                    
                    if (stepCurrent.effectsOnEvents != null && stepProc.effectsOnEvents == null)
                        stepProc.effectsOnEvents = stepCurrent.effectsOnEvents;
                }
            }

            if (current.effectsShared != null && current.effectsShared.Count > 0)
            {
                if (root.effectsSharedProc == null)
                    root.effectsSharedProc = new SortedDictionary<string, DataBlockOverworldQuestEffectGroup> ();

                foreach (var kvp in current.effectsShared)
                {
                    var effectCurrent = kvp.Value;
                    if (effectCurrent == null)
                        continue;

                    var effectKey = kvp.Key;
                    if (!root.effectsSharedProc.TryGetValue (effectKey, out var effectProc))
                    {
                        // Add a clone because generator steps will need to mutate everything
                        effectProc = new DataBlockOverworldQuestEffectGroup ();
                        root.effectsSharedProc.Add (effectKey, effectProc);

                        // Set unserializable fields
                        effectProc.key = effectKey;
                    }

                    // Grab references to existing objects for reference fields, where available
                    // This is useful as most of these benefit from ability to live edit mid gameplay
                    
                    if (effectCurrent.checksGlobal != null && effectProc.checksGlobal == null)
                        effectProc.checksGlobal = effectCurrent.checksGlobal;
                    
                    if (effectCurrent.checksBase != null && effectProc.checksBase == null)
                        effectProc.checksBase = effectCurrent.checksBase;

                    if (effectCurrent.functionsGlobal != null && effectProc.functionsGlobal == null)
                        effectProc.functionsGlobal = effectCurrent.functionsGlobal;
                    
                    if (effectCurrent.functionsBase != null && effectProc.functionsBase == null)
                        effectProc.functionsBase = effectCurrent.functionsBase;
                    
                    if (effectCurrent.functionsEventEntity != null && effectProc.functionsEventEntity == null)
                        effectProc.functionsEventEntity = effectCurrent.functionsEventEntity;
                    
                    if (effectCurrent.linkedEffectKeys != null && effectProc.linkedEffectKeys == null)
                        effectProc.linkedEffectKeys = effectCurrent.linkedEffectKeys;
                }
            }

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Quest {current.key} fails to complete recursive processing in under 20 steps.");
                return;
            }

            // No parents further up
            if (current.parents == null || current.parents.Count == 0)
                return;

            for (int i = 0, count = current.parents.Count; i < count; ++i)
            {
                var link = current.parents[i];
                if (link == null || string.IsNullOrEmpty (link.key))
                {
                    Debug.LogWarning ($"Quest {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Quest {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Quest {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
                    continue;
                }
                
                // Append next hierarchy level for easier preview
                if (parent.parents != null && parent.parents.Count > 0)
                {
                    sb.Clear ();
                    for (int i2 = 0, count2 = parent.parents.Count; i2 < count2; ++i2)
                    {
                        if (i2 > 0)
                            sb.Append (", ");

                        var parentOfParent = parent.parents[i2];
                        if (parentOfParent == null || string.IsNullOrEmpty (parentOfParent.key))
                            sb.Append ("—");
                        else
                            sb.Append (parentOfParent.key);
                    }

                    link.hierarchy = sb.ToString ();
                }
                else
                    link.hierarchy = "—";
                
                ProcessRecursive (parent, root, depth + 1);
            }
        }
    }
}


