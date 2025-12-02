using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldPointPreset : DataMultiLinker<DataContainerOverworldPointPreset>
    {
        public DataMultiLinkerOverworldPointPreset ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldPoints); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showTagCollections;

            [ShowInInspector]
            public static bool showInheritance = false;
        }

        [ShowInInspector] [HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldPointPreset.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldPointPreset.Presentation.showTagCollections")]
        [ShowInInspector] [ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        private static StringBuilder sb = new StringBuilder ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }

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
        
        private static List<DataContainerOverworldPointPreset> entriesUpdated = new List<DataContainerOverworldPointPreset> ();
        
        public static void ProcessRelated (DataContainerOverworldPointPreset origin)
        {
            if (origin == null)
                return;

            entriesUpdated.Clear ();
            entriesUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var entry = GetEntry (childKey);
                    if (entry != null)
                        entriesUpdated.Add (entry);
                }
            }
            
            foreach (var entry in entriesUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (entry != origin)
                    entry.OnAfterDeserialization (entry.key);
            }

            foreach (var entry in entriesUpdated)
                ProcessRecursiveStart (entry);

            foreach (var entry in entriesUpdated)
                Postprocess (entry);
        }
        
        public static void ProcessRecursiveStart (DataContainerOverworldPointPreset origin)
        {
            if (origin == null)
                return;

            origin.coreProc = null;
            origin.tagsProc = null;
            origin.generationProc = null;
            origin.interactionProc = null;
            origin.effectsOnEventsProc = null;
            origin.combatProc = null;

            ProcessRecursive (origin, origin, 0);
        }
        
        public static void Postprocess (DataContainerOverworldPointPreset target)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data
        }
        
        private static void ProcessRecursive (DataContainerOverworldPointPreset current, DataContainerOverworldPointPreset root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root interaction reference while validating interaction hierarchy");
                return;
            }

            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing interaction {root.key}");
                return;
            }

            if (current.core != null)
            {
                if (root.coreProc == null)
                    root.coreProc = new DataBlockOverworldPointCore ();
                
                if (current.core.textIdentifierGroup != null && root.coreProc.textIdentifierGroup == null)
                    root.coreProc.textIdentifierGroup = current.core.textIdentifierGroup;

                if (current.core.textName != null && root.coreProc.textName == null)
                    root.coreProc.textName = current.core.textName;
                
                if (current.core.textDesc != null && root.coreProc.textDesc == null)
                    root.coreProc.textDesc = current.core.textDesc;
                
                if (current.core.image != null && root.coreProc.image == null)
                    root.coreProc.image = current.core.image;
                
                if (current.core.icon != null && root.coreProc.icon == null)
                    root.coreProc.icon = current.core.icon;
                
                if (current.core.visionColor != null && root.coreProc.visionColor == null)
                    root.coreProc.visionColor = current.core.visionColor;
                
                if (current.core.visual != null && root.coreProc.visual == null)
                    root.coreProc.visual = current.core.visual;
                
                if (current.core.visualLiberated != null && root.coreProc.visualLiberated == null)
                    root.coreProc.visualLiberated = current.core.visualLiberated;
                
                if (current.core.observer != null && root.coreProc.observer == null)
                    root.coreProc.observer = current.core.observer;
                
                if (current.core.movement != null && root.coreProc.movement == null)
                    root.coreProc.movement = current.core.movement;
                
                if (current.core.rangeVision != null && root.coreProc.rangeVision == null)
                    root.coreProc.rangeVision = current.core.rangeVision;
                
                if (current.core.rangeInteraction != null && root.coreProc.rangeInteraction == null)
                    root.coreProc.rangeInteraction = current.core.rangeInteraction;
                
                if (current.core.detection != null && root.coreProc.detection == null)
                    root.coreProc.detection = current.core.detection;
            }

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

            if (current.generation != null)
            {
                if (root.generationProc == null)
                    root.generationProc = new DataBlockOverworldPointGeneration ();
                
                /*
                if (current.generation.tagsChecked != null && root.generationProc.tagsChecked == null)
                    root.generationProc.tagsChecked = current.generation.tagsChecked;

                if (current.generation.spawnLimit != null && root.generationProc.spawnLimit == null)
                    root.generationProc.spawnLimit = current.generation.spawnLimit;
                
                if (current.generation.instanceLimit != null && root.generationProc.instanceLimit == null)
                    root.generationProc.instanceLimit = current.generation.instanceLimit;
                
                if (current.generation.completionLimit != null && root.generationProc.completionLimit == null)
                    root.generationProc.completionLimit = current.generation.completionLimit;
                
                if (current.generation.completionSeparation != null && root.generationProc.completionSeparation == null)
                    root.generationProc.completionSeparation = current.generation.completionSeparation;

                if (current.generation.conditions != null)
                {
                    if (root.generationProc.conditions == null)
                        root.generationProc.conditions = new List<DataBlockOverworldPointGenerationCondition> ();

                    foreach (var check in current.generation.checksBase)
                    {
                        if (check != null)
                            root.generationProc.checksBase.Add (check);
                    }
                }
                */
                
                if (current.generation.spawnPrefs != null && root.generationProc.spawnPrefs == null)
                    root.generationProc.spawnPrefs = current.generation.spawnPrefs;
                
                if (current.generation.checksBase != null)
                {
                    if (root.generationProc.checksBase == null)
                        root.generationProc.checksBase = new List<IOverworldEntityValidationFunction> ();

                    foreach (var check in current.generation.checksBase)
                    {
                        if (check != null)
                            root.generationProc.checksBase.Add (check);
                    }
                }
                
                if (current.generation.functionsOnSpawn != null && root.generationProc.functionsOnSpawn == null)
                    root.generationProc.functionsOnSpawn = current.generation.functionsOnSpawn;
            }

            if (current.interaction != null && root.interactionProc == null)
                root.interactionProc = current.interaction;
            
            if (current.effectsOnEvents != null && root.effectsOnEventsProc == null)
                root.effectsOnEventsProc = current.effectsOnEvents;
            
            if (current.combat != null)
            {
                if (root.combatProc == null)
                    root.combatProc = new DataBlockOverworldPointCombat ();

                if (current.combat.scenarioKeys != null && root.combatProc.scenarioKeys == null)
                    root.combatProc.scenarioKeys = current.combat.scenarioKeys;
                
                if (current.combat.scenarioFilter != null && root.combatProc.scenarioFilter == null)
                    root.combatProc.scenarioFilter = current.combat.scenarioFilter;
                
                if (current.combat.scenarioChanges != null && root.combatProc.scenarioChanges == null)
                    root.combatProc.scenarioChanges = current.combat.scenarioChanges;
                
                if (current.combat.areaKeys != null && root.combatProc.areaKeys == null)
                    root.combatProc.areaKeys = current.combat.areaKeys;
                
                if (current.combat.areaFilter != null && root.combatProc.areaFilter == null)
                    root.combatProc.areaFilter = current.combat.areaFilter;

                if (current.combat.completion != null)
                {
                    if (root.combatProc.completion == null)
                    {
                        root.combatProc.completion = new DataBlockOverworldPointCompletion ();
                        root.combatProc.completion.destroyed = current.combat.completion.destroyed;
                    }
                    
                    if (current.combat.completion.functionsBase != null)
                    {
                        if (root.combatProc.completion.functionsBase == null)
                            root.combatProc.completion.functionsBase = new List<IOverworldTargetedFunction> ();
                        
                        foreach (var function in current.combat.completion.functionsBase)
                        {
                            if (function != null)
                                root.combatProc.completion.functionsBase.Add (function);
                        }
                    }
                    
                    if (current.combat.completion.functionsGlobal != null)
                    {
                        if (root.combatProc.completion.functionsGlobal == null)
                            root.combatProc.completion.functionsGlobal = new List<IOverworldFunction> ();
                        
                        foreach (var function in current.combat.completion.functionsGlobal)
                        {
                            if (function != null)
                                root.combatProc.completion.functionsGlobal.Add (function);
                        }
                    }
                    
                    if (current.combat.completion.functionsTarget != null)
                    {
                        if (root.combatProc.completion.functionsTarget == null)
                            root.combatProc.completion.functionsTarget = new List<IOverworldTargetedFunction> ();
                        
                        foreach (var function in current.combat.completion.functionsTarget)
                        {
                            if (function != null)
                                root.combatProc.completion.functionsTarget.Add (function);
                        }
                    }
                }
                
                if (current.combat.rewards != null && root.combatProc.rewards == null)
                    root.combatProc.rewards = current.combat.rewards;
                
                if (current.combat.salvageBudget != null && root.combatProc.salvageBudget == null)
                    root.combatProc.salvageBudget = current.combat.salvageBudget;
            }

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Point {current.key} fails to complete recursive processing in under 20 steps.");
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
                    Debug.LogWarning ($"Point {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Point {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Point {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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


