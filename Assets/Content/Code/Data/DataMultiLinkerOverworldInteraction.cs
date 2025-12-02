using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldInteraction : DataMultiLinker<DataContainerOverworldInteraction>
    {
        public DataMultiLinkerOverworldInteraction ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldInteraction);
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showSteps = true;
            
            [ShowInInspector]
            public static bool showIsolatedEntries = true;
            
            [ShowInInspector]
            public static bool showTagCollections;

            [ShowInInspector]
            public static bool showInheritance = false;
        }

        [ShowInInspector] [HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldInteraction.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldInteraction.Presentation.showTagCollections")]
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
        
        private static List<DataContainerOverworldInteraction> interactionsUpdated = new List<DataContainerOverworldInteraction> ();
        
        public static void ProcessRelated (DataContainerOverworldInteraction origin)
        {
            if (origin == null)
                return;

            interactionsUpdated.Clear ();
            interactionsUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var interaction = GetEntry (childKey);
                    if (interaction != null)
                        interactionsUpdated.Add (interaction);
                }
            }
            
            foreach (var interaction in interactionsUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (interaction != origin)
                    interaction.OnAfterDeserialization (interaction.key);
            }

            foreach (var interaction in interactionsUpdated)
                ProcessRecursiveStart (interaction);

            foreach (var interaction in interactionsUpdated)
                Postprocess (interaction);
        }
        
        public static void ProcessRecursiveStart (DataContainerOverworldInteraction origin)
        {
            if (origin == null)
                return;

            origin.coreProc = null;
            origin.effectsOnExitProc = null;
            origin.checkProc = null;
            origin.tagsProc = null;
            origin.stepsProc = null;

            ProcessRecursive (origin, origin, 0);
        }
        
        public static void Postprocess (DataContainerOverworldInteraction target)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data
        }

        private static void ProcessRecursive (DataContainerOverworldInteraction current, DataContainerOverworldInteraction root, int depth)
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

            // Replace block whole
            /*
            if (current.image != null && root.imageProc == null)
                root.imageProc = current.image;

            if (current.text != null && root.textProc == null)
                root.textProc = current.text;
            */
            
            if (current.core != null && root.coreProc == null)
                root.coreProc = current.core;
            
            if (current.effectsOnExit != null && root.effectsOnExitProc == null)
                root.effectsOnExitProc = current.effectsOnExit;
            
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
            
            if (current.check != null && root.checkProc == null)
            {
                var checkCurrent = current.check;
                var checkProc = root.checkProc;
                
                if (checkProc == null)
                {
                    checkProc = new DataBlockOverworldInteractionCheck ();
                    root.checkProc = checkProc;
                    root.checkProc.repeat = current.check.repeat;
                }
                
                if (checkCurrent.checksGlobal != null && checkProc.checksGlobal == null)
                    checkProc.checksGlobal = checkCurrent.checksGlobal;
                
                if (checkCurrent.checksBase != null && checkProc.checksBase == null)
                    checkProc.checksBase = checkCurrent.checksBase;
                
                if (checkCurrent.checksTarget != null && checkProc.checksTarget == null)
                    checkProc.checksTarget = checkCurrent.checksTarget;
            }
            
            if (current.steps != null && current.steps.Count > 0)
            {
                if (root.stepsProc == null)
                    root.stepsProc = new SortedDictionary<string, DataBlockOverworldInteractionStep> ();

                foreach (var kvp in current.steps)
                {
                    var stepCurrent = kvp.Value;
                    if (stepCurrent == null)
                        continue;

                    var stepKey = kvp.Key;
                    if (!root.stepsProc.TryGetValue (stepKey, out var stepProc))
                    {
                        // Add a clone because generator steps will need to mutate everything
                        stepProc = new DataBlockOverworldInteractionStep ();
                        root.stepsProc.Add (stepKey, stepProc);

                        // Set unserializable fields
                        stepProc.parent = root;
                        stepProc.key = stepCurrent.key;

                        // Set non-reference fields
                        // stepProc.hidden = stepCurrent.hidden;
                    }

                    // Grab references to existing objects for reference fields, where available
                    // This is useful as most of these benefit from ability to live edit mid gameplay

                    if (stepCurrent.image != null && stepProc.image == null)
                        stepProc.image = stepCurrent.image;
                    
                    if (stepCurrent.background != null && stepProc.background == null)
                        stepProc.background = stepCurrent.background;
                    
                    if (stepCurrent.mood != null && stepProc.mood == null)
                        stepProc.mood = stepCurrent.mood;
                    
                    if (stepCurrent.textHeader != null && stepProc.textHeader == null)
                        stepProc.textHeader = stepCurrent.textHeader;
                    
                    if (stepCurrent.text != null && stepProc.text == null)
                        stepProc.text = stepCurrent.text;
                    
                    if (stepCurrent.textReused != null && stepProc.textReused == null)
                        stepProc.textReused = stepCurrent.textReused;
                    
                    if (stepCurrent.properties != null && stepProc.properties == null)
                        stepProc.properties = stepCurrent.properties;
                    
                    if (stepCurrent.effects != null && stepProc.effects == null)
                        stepProc.effects = stepCurrent.effects;
                    
                    if (stepCurrent.optionsReused != null && stepProc.optionsReused == null)
                        stepProc.optionsReused = stepCurrent.optionsReused;
                    
                    // if (stepCurrent.options != null && stepProc.options == null)
                    //    stepProc.options = stepCurrent.options;

                    if (stepCurrent.options != null && stepCurrent.options.Count > 0)
                    {
                        if (stepProc.options == null)
                            stepProc.options = new SortedDictionary<string, DataBlockOverworldInteractionOption> ();
                        
                        foreach (var kvp2 in stepCurrent.options)
                        {
                            var optionCurrent = kvp2.Value;
                            if (optionCurrent == null)
                                continue;

                            var optionKey = kvp2.Key;
                            if (!stepProc.options.TryGetValue (optionKey, out var optionProc))
                            {
                                // Add a clone because generator steps will need to mutate everything
                                optionProc = new DataBlockOverworldInteractionOption ();
                                stepProc.options.Add (optionKey, optionProc);

                                // Set unserializable fields
                                optionProc.parent = root;
                                optionProc.key = optionKey;
                            }

                            DataBlockOverworldInteractionOption optionReused = null;
                            if (optionCurrent.dataReused != null )
                            {
                                var rl = optionCurrent.dataReused;
                                var dataSource = DataMultiLinkerOverworldInteraction.GetEntry (rl.interaction, false);
                                if (dataSource != null && dataSource.steps != null && !string.IsNullOrEmpty (rl.step) && dataSource.steps.TryGetValue (rl.step, out var sr))
                                {
                                    if (sr != null && sr.options != null && !string.IsNullOrEmpty (rl.option) && sr.options.TryGetValue (rl.option, out var or))
                                        optionReused = or;
                                }
                            }

                            bool optionReusedPresent = optionReused != null;

                            // Grab references to existing objects for reference fields, where available
                            // This is useful as most of these benefit from ability to live edit mid gameplay
                            
                            if (optionProc.core == null)
                            {
                                if (optionCurrent.core != null)
                                    optionProc.core = optionCurrent.core;
                                else if (optionReusedPresent && optionReused.core != null)
                                    optionProc.core = optionReused.core;
                            }

                            if (optionProc.core == null)
                            {
                                if (optionCurrent.core != null)
                                    optionProc.core = optionCurrent.core;
                                else if (optionReusedPresent && optionReused.core != null)
                                    optionProc.core = optionReused.core;
                            }
                            
                            if (optionProc.textMain == null)
                            {
                                if (optionCurrent.textMain != null)
                                    optionProc.textMain = optionCurrent.textMain;
                                else if (optionReusedPresent && optionReused.textMain != null)
                                    optionProc.textMain = optionReused.textMain;
                            }
                            
                            if (optionProc.textMainReused == null)
                            {
                                if (optionCurrent.textMainReused != null)
                                    optionProc.textMainReused = optionCurrent.textMainReused;
                                else if (optionReusedPresent && optionReused.textMainReused != null)
                                    optionProc.textMainReused = optionReused.textMainReused;
                            }
                            
                            if (optionProc.textContent == null)
                            {
                                if (optionCurrent.textContent != null)
                                    optionProc.textContent = optionCurrent.textContent;
                                else if (optionReusedPresent && optionReused.textContent != null)
                                    optionProc.textContent = optionReused.textContent;
                            }
                            
                            if (optionProc.textContentReused == null)
                            {
                                if (optionCurrent.textContentReused != null)
                                    optionProc.textContentReused = optionCurrent.textContentReused;
                                else if (optionReusedPresent && optionReused.textContentReused != null)
                                    optionProc.textContentReused = optionReused.textContentReused;
                            }
                            
                            if (optionProc.textEffect == null)
                            {
                                if (optionCurrent.textEffect != null)
                                    optionProc.textEffect = optionCurrent.textEffect;
                                else if (optionReusedPresent && optionReused.textEffect != null)
                                    optionProc.textEffect = optionReused.textEffect;
                            }
                            
                            if (optionProc.textEffectReused == null)
                            {
                                if (optionCurrent.textEffectReused != null)
                                    optionProc.textEffectReused = optionCurrent.textEffectReused;
                                else if (optionReusedPresent && optionReused.textEffectReused != null)
                                    optionProc.textEffectReused = optionReused.textEffectReused;
                            }
                            
                            if (optionProc.check == null)
                            {
                                if (optionCurrent.check != null)
                                    optionProc.check = optionCurrent.check;
                                else if (optionReusedPresent && optionReused.check != null)
                                    optionProc.check = optionReused.check;
                            }
                            
                            /*
                            if (optionProc.mood == null)
                            {
                                if (optionCurrent.mood != null)
                                    optionProc.mood = optionCurrent.mood;
                                else if (optionReusedPresent && optionReused.mood != null)
                                    optionProc.mood = optionReused.mood;
                            }
                            */
                            
                            if (optionProc.checkOtherOptions == null)
                            {
                                if (optionCurrent.checkOtherOptions != null)
                                    optionProc.checkOtherOptions = optionCurrent.checkOtherOptions;
                                else if (optionReusedPresent && optionReused.checkOtherOptions != null)
                                    optionProc.checkOtherOptions = optionReused.checkOtherOptions;
                            }
                            
                            if (optionProc.checksGlobal == null)
                            {
                                if (optionCurrent.checksGlobal != null)
                                    optionProc.checksGlobal = optionCurrent.checksGlobal;
                                else if (optionReusedPresent && optionReused.checksGlobal != null)
                                    optionProc.checksGlobal = optionReused.checksGlobal;
                            }
                            
                            if (optionProc.checksBase == null)
                            {
                                if (optionCurrent.checksBase != null)
                                    optionProc.checksBase = optionCurrent.checksBase;
                                else if (optionReusedPresent && optionReused.checksBase != null)
                                    optionProc.checksBase = optionReused.checksBase;
                            }
                            
                            if (optionProc.checksTarget == null)
                            {
                                if (optionCurrent.checksTarget != null)
                                    optionProc.checksTarget = optionCurrent.checksTarget;
                                else if (optionReusedPresent && optionReused.checksTarget != null)
                                    optionProc.checksTarget = optionReused.checksTarget;
                            }
                            
                            if (optionProc.effects == null)
                            {
                                if (optionCurrent.effects != null)
                                    optionProc.effects = optionCurrent.effects;
                                else if (optionReusedPresent && optionReused.effects != null)
                                    optionProc.effects = optionReused.effects;
                            }
                        }
                    }
                }
            }

            /*
            if (current.options != null && current.options.Count > 0)
            {
                if (root.optionsProc == null)
                    root.optionsProc = new SortedDictionary<string, DataBlockOverworldInteractionOption> ();

                foreach (var kvp in current.options)
                {
                    var optionCurrent = kvp.Value;
                    if (optionCurrent == null)
                        continue;

                    var optionKey = kvp.Key;
                    if (!root.optionsProc.TryGetValue (optionKey, out var optionProc))
                    {
                        // Add a clone because generator steps will need to mutate everything
                        optionProc = new DataBlockOverworldInteractionOption ();
                        root.optionsProc.Add (optionKey, optionProc);

                        // Set unserializable fields
                        optionProc.textHeader = optionCurrent.textHeader;
                        optionProc.textDesc = optionCurrent.textDesc;
                        optionProc.parent = root;

                        // Set non-reference fields
                        optionProc.hidden = optionCurrent.hidden;
                    }

                    // Grab references to existing objects for reference fields, where available
                    // This is useful as most of these benefit from ability to live edit mid gameplay

                    if (optionCurrent.checksBase != null && optionProc.checksBase == null)
                        optionProc.checksBase = optionCurrent.checksBase;
                    
                    if (optionCurrent.checksTarget != null && optionProc.checksTarget == null)
                        optionProc.checksTarget = optionCurrent.checksTarget;
                    
                    if (optionCurrent.functionsBase != null && optionProc.functionsBase == null)
                        optionProc.functionsBase = optionCurrent.functionsBase;
                    
                    if (optionCurrent.functionsTarget != null && optionProc.functionsTarget == null)
                        optionProc.functionsTarget = optionCurrent.functionsTarget;
                    
                    if (optionCurrent.functionsGlobal != null && optionProc.functionsGlobal == null)
                        optionProc.functionsGlobal = optionCurrent.functionsGlobal;
                }
            }
            */
            
            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Interaction {current.key} fails to complete recursive processing in under 20 steps.");
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
                    Debug.LogWarning ($"Interaction {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Interaction {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Interaction {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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
        
        /*
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-10)]
        public void ConvertToEffectGroups ()
        {
            foreach (var kvp in data)
            {
                var interaction = kvp.Value;
                if (interaction.steps == null)
                    continue;

                foreach (var kvp2 in interaction.steps)
                {
                    var step = kvp2.Value;
                    if (step == null || step.options == null)
                        continue;

                    foreach (var kvp3 in step.options)
                    {
                        var option = kvp3.Value;
                        if (option == null)
                            continue;
                        
                        if (option.functionsLocal == null && 
                            option.functionsBase == null && 
                            option.functionsTarget == null && 
                            option.functionsGlobal == null &&
                            option.functionsEventLegacy == null)
                            continue;

                        var effect = new DataBlockOverworldInteractionEffect ();
                        option.effects = new List<DataBlockOverworldInteractionEffect> { effect };

                        effect.functionsLocal = option.functionsLocal;
                        effect.functionsBase = option.functionsBase;
                        effect.functionsTarget = option.functionsTarget;
                        effect.functionsGlobal = option.functionsGlobal;
                        effect.functionsEventLegacy = option.functionsEventLegacy;
                        
                        option.functionsLocal = null;
                        option.functionsBase = null;
                        option.functionsTarget = null;
                        option.functionsGlobal = null;
                        option.functionsEventLegacy = null;
                    }
                }
            }
        }
        */
    }
}


