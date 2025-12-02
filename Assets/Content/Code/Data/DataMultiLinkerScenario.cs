using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Combat.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerScenario : DataMultiLinker<DataContainerScenario>
    {
        public DataMultiLinkerScenario ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.scenarioEmbedded); 
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            public static bool tabCore = false;
            public static bool tabSteps = true;
            public static bool tabStates = false;
            public static bool tabOther = false;
            
            private const string foldoutGroup = "Advanced options";
            private Color GetTabColor (bool open) => Color.white.WithAlpha (open ? 0.5f : 1f);
            
            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabCore)")]
            [Button ("Core", ButtonSizes.Large), ButtonGroup]
            public void SetTabCore () => tabCore = !tabCore;

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabSteps)")]
            [Button ("Steps", ButtonSizes.Large), ButtonGroup]
            public void SetTabSteps () => tabSteps = !tabSteps;

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabStates)")]
            [Button ("States", ButtonSizes.Large), ButtonGroup]
            public void SetTabStates () => tabStates = !tabStates;

            [PropertyOrder (-10), GUIColor ("@GetTabColor (tabOther)")]
            [Button ("Other", ButtonSizes.Large), ButtonGroup]
            public void SetTabOther () => tabOther = !tabOther;

            [ShowInInspector, FoldoutGroup (foldoutGroup, false)]
            public static bool tabsVisiblePerConfig = true;

            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showHierarchy = true;
            
            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showStepsIsolated = true;
            
            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showStepBlocks = true;

            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showUnits = true;

            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showStats = false;

            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showTagCollections;
            
            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showInheritance = false;
            
            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showIsolatedEntries = true;
            
            [ShowInInspector, FoldoutGroup (foldoutGroup)]
            public static bool showUtilityData = false;
            
            // [ShowInInspector]
            // [ShowIf ("showInheritance")]
            // [InfoBox ("Warning: this mode triggers inheritance processing every time inheritable fields are modified. This is useful for instantly previewing changes to things like stat or inherited text, but it can cause data loss if you are not careful. Only currently modified config is protected from being reloaded, save if you switch to another config.", VisibleIf = "autoUpdateInheritance")]
            // public static bool autoUpdateInheritance = false;
        }

        [ShowInInspector][HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerScenario.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerScenario.Presentation.showTagCollections")]
        [ShowInInspector][ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static List<string> textKeysStateHeader = new List<string> ();
        public static List<string> textKeysStateDescription = new List<string> ();

        public static List<string> textKeysStepHeader = new List<string> ();
        public static List<string> textKeysStepDescription = new List<string> ();

        public static DataContainerScenario selectedScenario;
        public static DataBlockScenarioStep selectedStep;
        private DataBlockScenarioUnitGroup blockScenarioUnitGroup;
        
        #if !PB_MODSDK
        [LabelWidth (180f)]
        public class ScenarioStatePreview
        {
            [ShowInInspector][FoldoutGroup ("Generation")]
            public string scenarioKeyLast
            {
                get
                {
                    var scenario = ScenarioUtility.GetCurrentScenario (false);
                    return scenario != null ? scenario.key : null;
                }
                set
                {
                    
                }
            }
            
            [ShowInInspector][FoldoutGroup ("Generation")]
            public CombatDescription combatDescription
            {
                get => ScenarioUtility.GetCurrentCombatDescription ();
                set
                {
                    
                }
            }
            
            [ShowInInspector][FoldoutGroup ("Generation")]
            public DataContainerScenario scenarioCurrent
            {
                get => ScenarioUtility.GetCurrentScenario (false);
                set
                {
                    
                }
            }
            
            [ShowInInspector][BoxGroup ("Time")]
            public string executionActive { get => cmb.isScenarioAllowingExecution.ToString (); }
            
            [ShowInInspector][BoxGroup ("Time")]
            public string turn { get => cmb.hasCurrentTurn ? cmb.currentTurn.i.ToString () : "—"; }
            
            [ShowInInspector][BoxGroup ("Time")]
            public string timeSim { get => cmb.hasSimulationTime ? cmb.simulationTime.f.ToString("F2") : "—"; }
            
            [ShowInInspector][BoxGroup ("Time")]
            public string timeReal { get => TimeCustom.unscaledTime.ToString("F2"); }
            
            [ShowInInspector][BoxGroup ("Time")]
            public string stepCurrent { get => cmb.hasScenarioStepCurrent ? cmb.scenarioStepCurrent.s : "—"; }

            [ShowInInspector][BoxGroup ("Time")]
            public string stepStartTimeReal { get => cmb.hasRealTimeAtStepStart ? cmb.realTimeAtStepStart.f.ToString("F2") : "—"; }
            
            [ShowInInspector]
            public Dictionary<string, ScenarioStepStartInfo> startInfo
            {
                get => cmb.hasScenarioStepsStartInfo ? cmb.scenarioStepsStartInfo.lookup : null; 
                set => cmb.ReplaceScenarioStepsStartInfo (value);
            }

            [ShowInInspector]
            public Dictionary<string, ScenarioStateScopeMetadata> scope
            {
                get => cmb.hasScenarioStateScope ? cmb.scenarioStateScope.s : null; 
                set => cmb.ReplaceScenarioStateScope (value);
            }

            [ShowInInspector]
            [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
            public SortedDictionary<string, bool> state
            {
                get => cmb.hasScenarioStateValues ? cmb.scenarioStateValues.s : null; 
                set => cmb.ReplaceScenarioStateValues (value);
            }
            
            [ShowInInspector]
            public SortedDictionary<string, int> stateTriggerCounts
            {
                get => cmb.hasScenarioStateTriggerCounts ? cmb.scenarioStateTriggerCounts.s : null; 
                set => cmb.ReplaceScenarioStateTriggerCounts (value);
            }

            private CombatContext cmb => Contexts.sharedInstance.combat;

            [Button ("Force state update")]
            private void ForceStateUpdate () => cmb.ReplaceScenarioStateRefresh ((int)ScenarioStateRefreshContext.None);
            
            [Button ("Force transition update")]
            private void ForceTransitionUpdate () => cmb.isScenarioTransitionRefresh = true;
            
            [Button ("Replace state scope")]
            private void ForceScope () { if (cmb.hasScenarioStateScope) cmb.ReplaceScenarioStateScope (cmb.scenarioStateScope.s); }

            [Button ("Replace state values")]
            private void ForceValues () { if (cmb.hasScenarioStateValues) cmb.ReplaceScenarioStateValues (cmb.scenarioStateValues.s); }
        }

        [HideLabel, HideReferenceObjectPicker, ShowInInspector, HideInEditorMode]
        [ShowIf ("IsCombatActive"), FoldoutGroup ("Combat State", false)]
        public static ScenarioStatePreview preview = new ScenarioStatePreview ();
        
        #endif

        private bool IsCombatActive => Application.isPlaying;
        private static StringBuilder sb = new StringBuilder ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }

        public static void OnAfterDeserialization ()
        {
            var textKeysStatesShared = DataManagerText.GetLibraryTextKeys (TextLibs.scenarioStatesShared);
            var textKeyStateSuffixHeader = "_header";
            var textKeyStateSuffixDescription = "_text";
            
            textKeysStateHeader.Clear ();
            textKeysStateDescription.Clear ();
            
            if (textKeysStatesShared != null)
            {
                foreach (var key in textKeysStatesShared)
                {
                    if (key.EndsWith (textKeyStateSuffixHeader))
                        textKeysStateHeader.Add (key);
                    else if (key.EndsWith (textKeyStateSuffixDescription))
                        textKeysStateDescription.Add (key);
                }
            }
            
            var textKeysStepsShared = DataManagerText.GetLibraryTextKeys (TextLibs.scenarioStepsShared);
            var textKeyStepSuffixHeader = "_header";
            var textKeyStepSuffixDescription = "_text";
            
            textKeysStepHeader.Clear ();
            textKeysStepDescription.Clear ();
            
            if (textKeysStepsShared != null)
            {
                foreach (var key in textKeysStepsShared)
                {
                    if (key.EndsWith (textKeyStepSuffixHeader))
                        textKeysStepHeader.Add (key);
                    else if (key.EndsWith (textKeyStepSuffixDescription))
                        textKeysStepDescription.Add (key);
                }
            }
            
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
                presetA.children.Clear ();
                
                foreach (var kvp2 in data)
                {
                    var presetB = kvp2.Value;
                    if (presetB.parents == null || presetB.parents.Count == 0)
                        continue;

                    foreach (var link in presetB.parents)
                    {
                        if (link.key == key)
                            presetA.children.Add (presetB.key);
                    }
                }
            }
            
            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
        
        private static List<DataContainerScenario> scenariosUpdated = new List<DataContainerScenario> ();

        public static void ProcessRelated (DataContainerScenario origin)
        {
            if (origin == null)
                return;

            scenariosUpdated.Clear ();
            scenariosUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var composite = GetEntry (childKey);
                    if (composite != null)
                        scenariosUpdated.Add (composite);
                }
            }
            
            foreach (var scenario in scenariosUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (scenario != origin)
                    scenario.OnAfterDeserialization (scenario.key);
            }

            foreach (var composite in scenariosUpdated)
                ProcessRecursiveStart (composite);

            foreach (var composite in scenariosUpdated)
                Postprocess (composite);
        }
        
        public static void ProcessRecursiveStart (DataContainerScenario origin)
        {
            if (origin == null)
                return;

            origin.coreProc = null;
            origin.areasProc = null;
            origin.entryProc = null;
            origin.tagsProc = null;
            origin.statesProc = null;
            origin.stepsProc = null;
            origin.unitPresetsProc = null;
            origin.unitGeneratorsProc = null;
                
            ProcessRecursive (origin, origin, 0);
        }

        public static void Postprocess (DataContainerScenario scenario)
        {
            // Add any code that requires fully filled processed fields here, e.g. procgen text based on merged data
            scenario.RefreshNonSerializedDataProcessed ();

            if (scenario.stepsProc == null || scenario.stepsProc.Count == 0)
            {
                if (scenario.generationInjection == null)
                    Debug.LogWarning ($"Scenario {scenario.key} has no processed steps!");
            }
            else
            {
                foreach (var kvp in scenario.stepsProc)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        Debug.LogWarning ($"Scenario {scenario.key} step {stepKey} is null!");
                    else if (step.core == null)
                        Debug.LogWarning ($"Scenario {scenario.key} step {stepKey} has no core data!");
                }
            }
        }
        
        private static void ProcessRecursive (DataContainerScenario current, DataContainerScenario root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root scenario reference while validating scenario hierarchy");
                return;
            }
            
            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing scenario {root.key}");
                return;
            }
            
            // Replace block whole
            if (current.core != null && root.coreProc == null)
                root.coreProc = current.core;
            
            if (current.areas != null && root.areasProc == null)
                root.areasProc = current.areas;
            
            if (current.entry != null && root.entryProc == null)
                root.entryProc = current.entry;

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
            
            if (current.states != null && current.states.Count > 0)
            {
                if (root.statesProc == null)
                    root.statesProc = new SortedDictionary<string, DataBlockScenarioState> ();
                
                foreach (var kvp in current.states)
                {
                    var stateCurrent = kvp.Value;
                    if (stateCurrent == null)
                        continue;

                    var stateKey = kvp.Key;
                    if (!root.statesProc.TryGetValue (stateKey, out var stateProc))
                    {
                        // Add a clone because generator steps will need to mutate everything
                        stateProc = new DataBlockScenarioState ();
                        root.statesProc.Add (stateKey, stateProc);
                        
                        // Set unserializable fields
                        stateProc.textName = stateCurrent.textName;
                        stateProc.textDesc = stateCurrent.textDesc;
                        stateProc.parentScenario = root;
                        stateProc.key = stateKey;
                        
                        // Set non-reference fields
                        stateProc.evaluated = stateCurrent.evaluated;
                        stateProc.evaluationContext = stateCurrent.evaluationContext;
                        stateProc.visible = stateCurrent.visible;
                        stateProc.startInScope = stateCurrent.startInScope;
                        stateProc.priorityGeneration = stateCurrent.priorityGeneration;
                        stateProc.priority = stateCurrent.priority;
                        stateProc.priorityDisplay = stateCurrent.priorityDisplay;
                        stateProc.mood = stateCurrent.mood;
                    }

                    // Grab references to existing objects for reference fields, where available
                    // This is useful as most of these benefit from ability to live edit mid gameplay
                    
                    if (!string.IsNullOrEmpty (stateCurrent.textNameKey) && string.IsNullOrEmpty (stateProc.textNameKey))
                        stateProc.textNameKey = stateCurrent.textNameKey;
                    
                    if (!string.IsNullOrEmpty (stateCurrent.textDescKey) && string.IsNullOrEmpty (stateProc.textDescKey))
                        stateProc.textDescKey = stateCurrent.textDescKey;
                    
                    if (stateCurrent.textOnCompletion != null && stateProc.textOnCompletion == null)
                        stateProc.textOnCompletion = stateCurrent.textOnCompletion;
                    
                    if (stateCurrent.ui != null && stateProc.ui == null)
                        stateProc.ui = stateCurrent.ui;
                    
                    if (stateCurrent.evaluationOnOutcome != null && stateProc.evaluationOnOutcome == null)
                        stateProc.evaluationOnOutcome = stateCurrent.evaluationOnOutcome;
                    
                    if (stateCurrent.turn != null && stateProc.turn == null)
                        stateProc.turn = stateCurrent.turn;
                    
                    if (stateCurrent.turnModulus != null && stateProc.turnModulus == null)
                        stateProc.turnModulus = stateCurrent.turnModulus;
                    
                    if (stateCurrent.unitChecks != null && stateProc.unitChecks == null)
                        stateProc.unitChecks = stateCurrent.unitChecks;
                    
                    if (stateCurrent.unitCheckLinked != null && stateProc.unitCheckLinked == null)
                        stateProc.unitCheckLinked = stateCurrent.unitCheckLinked;
                    
                    if (stateCurrent.location != null && stateProc.location == null)
                        stateProc.location = stateCurrent.location;
                    
                    if (stateCurrent.locationRetreat != null && stateProc.locationRetreat == null)
                        stateProc.locationRetreat = stateCurrent.locationRetreat;
                    
                    if (stateCurrent.volume != null && stateProc.volume == null)
                        stateProc.volume = stateCurrent.volume;
                    
                    if (stateCurrent.stateValues != null && stateProc.stateValues == null)
                        stateProc.stateValues = stateCurrent.stateValues;
                    
                    if (stateCurrent.memoryBase != null && stateProc.memoryBase == null)
                        stateProc.memoryBase = stateCurrent.memoryBase;
                    
                    if (stateCurrent.functions != null && stateProc.functions == null)
                        stateProc.functions = stateCurrent.functions;

                    if (stateCurrent.reactions != null)
                    {
                        // Now to an exception modified by generators: reactions can't be grabbed as a reference to an existing object,
                        // as parts of their contents are to be modified by generators. Not doing that would let generated changes
                        // leak into source blueprint data.
                        // stateProcRoot.reactions = stateCurrent.reactions;

                        var reactionsCurrent = stateCurrent.reactions;
                        DataBlockScenarioStateReactions reactionsProc = stateProc.reactions;
                        
                        // If processed reaction block doesn't exist, create it
                        if (reactionsProc == null)
                        {
                            reactionsProc = new DataBlockScenarioStateReactions ();
                            stateProc.reactions = reactionsProc;
                            
                            // Set non-reference fields
                            reactionsProc.expectedValue = reactionsCurrent.expectedValue;
                            reactionsProc.scopeRemovalOnLimit = reactionsCurrent.scopeRemovalOnLimit;
                            reactionsProc.triggerLimit = reactionsCurrent.triggerLimit;
                            reactionsProc.triggerIncrement = reactionsCurrent.triggerIncrement;
                        }

                        if (reactionsCurrent.effectsPerIncrement != null)
                        {
                            var effectsCurrent = reactionsCurrent.effectsPerIncrement;
                            var effectsProc = reactionsProc.effectsPerIncrement;
                            
                            // If processed effects collection doesn't exist, create it
                            if (effectsProc == null)
                            {
                                effectsProc = new Dictionary<int, DataBlockScenarioStateReaction> ();
                                reactionsProc.effectsPerIncrement = effectsProc;
                            }

                            // Iterate over effects dictionary at the current hierarchy level
                            foreach (var kvp2 in effectsCurrent)
                            {
                                var reactionCurrentKey = kvp2.Key;
                                var reactionCurrent = kvp2.Value;

                                DataBlockScenarioStateReaction reactionProc = null;
                                var reactionProcFound = effectsProc.TryGetValue (reactionCurrentKey, out reactionProc);
                                
                                if (!reactionProcFound || reactionProc == null)
                                {
                                    reactionProc = new DataBlockScenarioStateReaction ();
                                    effectsProc[reactionCurrentKey] = reactionProc;
                                }
                                
                                // Grab references to existing objects for reference fields, where available
                                // This is useful as most of these benefit from ability to live edit mid gameplay
                                if (reactionCurrent.tags != null && reactionProc.tags == null)
                                    reactionProc.tags = reactionCurrent.tags;

                                if (reactionCurrent.executionOnOutcome != null && reactionProc.executionOnOutcome == null)
                                    reactionProc.executionOnOutcome = reactionCurrent.executionOnOutcome;
                                
                                if (reactionCurrent.stepTransition != null && reactionProc.stepTransition == null)
                                    reactionProc.stepTransition = reactionCurrent.stepTransition;
                                
                                if (reactionCurrent.commsOnStart != null && reactionProc.commsOnStart == null)
                                    reactionProc.commsOnStart = reactionCurrent.commsOnStart;
                                
                                if (reactionCurrent.memoryChanges != null && reactionProc.memoryChanges == null)
                                    reactionProc.memoryChanges = reactionCurrent.memoryChanges;
                                
                                if (reactionCurrent.stateScopeChanges != null && reactionProc.stateScopeChanges == null)
                                    reactionProc.stateScopeChanges = reactionCurrent.stateScopeChanges;
                                
                                if (reactionCurrent.stateValueChanges != null && reactionProc.stateValueChanges == null)
                                    reactionProc.stateValueChanges = reactionCurrent.stateValueChanges;
                                
                                if (reactionCurrent.memoryDisplayChanges != null && reactionProc.memoryDisplayChanges == null)
                                    reactionProc.memoryDisplayChanges = reactionCurrent.memoryDisplayChanges;
                                
                                if (reactionCurrent.unitTagDisplayChanges != null && reactionProc.unitTagDisplayChanges == null)
                                    reactionProc.unitTagDisplayChanges = reactionCurrent.unitTagDisplayChanges;
                                
                                if (reactionCurrent.rewards != null && reactionProc.rewards == null)
                                    reactionProc.rewards = reactionCurrent.rewards;
                                
                                if (reactionCurrent.functions != null && reactionProc.functions == null)
                                    reactionProc.functions = reactionCurrent.functions;
                                
                                if (reactionCurrent.functionsPerUnit != null && reactionProc.functionsPerUnit == null)
                                    reactionProc.functionsPerUnit = reactionCurrent.functionsPerUnit;
                                
                                if (reactionCurrent.outcome != null && reactionProc.outcome == null)
                                    reactionProc.outcome = reactionCurrent.outcome;
                                
                                if (reactionCurrent.unitGeneratorKeys != null && reactionProc.unitGeneratorKeys == null)
                                    reactionProc.unitGeneratorKeys = reactionCurrent.unitGeneratorKeys;
                                
                                // Exception modified by generators: unit groups
                                // This field shouldn't be grabbed by reference and should always be copied
                                if (reactionCurrent.unitGroups != null && reactionProc.unitGroups == null)
                                {
                                    reactionProc.unitGroups = UtilitiesYAML.CloneThroughYaml (reactionCurrent.unitGroups);

                                    TransferNonSerializedDataToClones (reactionCurrent.unitGroups, reactionProc.unitGroups);
                                }
                            }
                        }
                    }
                }
            }
            
            if (current.steps != null && current.steps.Count > 0)
            {
                if (root.stepsProc == null)
                    root.stepsProc = new SortedDictionary<string, DataBlockScenarioStep> ();
                
                foreach (var kvp in current.steps)
                {
                    var stepCurrent = kvp.Value;
                    if (stepCurrent == null)
                        continue;

                    var stepKey = kvp.Key;
                    if (!root.stepsProc.TryGetValue (stepKey, out var stepProc))
                    {
                        // Add a clone because generator steps will need to mutate everything
                        stepProc = new DataBlockScenarioStep ();
                        root.stepsProc.Add (stepKey, stepProc);
                        
                        // Set unserializable fields
                        stepProc.parentScenario = root;
                        stepProc.key = stepKey;
                    }
                    
                    // Exception modified by generators: unit groups
                    // This field shouldn't be grabbed by reference and should always be copied
                    if (stepCurrent.unitGroups != null && stepProc.unitGroups == null)
                    {
                        stepProc.unitGroups = UtilitiesYAML.CloneThroughYaml (stepCurrent.unitGroups);
                        
                        // Transfer loc hidden in unit customization
                        TransferNonSerializedDataToClones (stepCurrent.unitGroups, stepProc.unitGroups);
                    }
                    
                    if (stepCurrent.unitGeneratorKeys != null && stepProc.unitGeneratorKeys == null)
                        stepProc.unitGeneratorKeys = stepCurrent.unitGeneratorKeys;
                    
                    if (stepCurrent.functions != null && stepProc.functions == null)
                        stepProc.functions = UtilitiesYAML.CloneThroughYaml (stepCurrent.functions);
                    
                    // Grab references to existing objects for reference fields, where available
                    // This is useful as most of these benefit from ability to live edit mid gameplay

                    if (stepCurrent.core != null && stepProc.core == null)
                        stepProc.core = stepCurrent.core;
                    
                    if (stepCurrent.transitions != null && stepProc.transitions == null)
                        stepProc.transitions = stepCurrent.transitions;
                    
                    if (stepCurrent.tags != null && stepProc.tags == null)
                        stepProc.tags = stepCurrent.tags;
                    
                    if (stepCurrent.stateScopeChanges != null && stepProc.stateScopeChanges == null)
                        stepProc.stateScopeChanges = stepCurrent.stateScopeChanges;
                    
                    if (stepCurrent.stateValueChanges != null && stepProc.stateValueChanges == null)
                        stepProc.stateValueChanges = stepCurrent.stateValueChanges;
                    
                    if (stepCurrent.memoryDisplayChanges != null && stepProc.memoryDisplayChanges == null)
                        stepProc.memoryDisplayChanges = stepCurrent.memoryDisplayChanges;
                    
                    if (stepCurrent.unitTagDisplayChanges != null && stepProc.unitTagDisplayChanges == null)
                        stepProc.unitTagDisplayChanges = stepCurrent.unitTagDisplayChanges;
                    
                    if (stepCurrent.unitChanges != null && stepProc.unitChanges == null)
                        stepProc.unitChanges = stepCurrent.unitChanges;
                    
                    if (stepCurrent.retreat != null && stepProc.retreat == null)
                        stepProc.retreat = stepCurrent.retreat;
                    
                    if (stepCurrent.actionRestrictions != null && stepProc.actionRestrictions == null)
                        stepProc.actionRestrictions = stepCurrent.actionRestrictions;
                    
                    if (stepCurrent.cutsceneVideoOnStart != null && stepProc.cutsceneVideoOnStart == null)
                        stepProc.cutsceneVideoOnStart = stepCurrent.cutsceneVideoOnStart;
                    
                    if (stepCurrent.atmosphereOnStart != null && stepProc.atmosphereOnStart == null)
                        stepProc.atmosphereOnStart = stepCurrent.atmosphereOnStart;
                    
                    if (stepCurrent.hintsConditional != null && stepProc.hintsConditional == null)
                        stepProc.hintsConditional = stepCurrent.hintsConditional;
                    
                    if (stepCurrent.unitSelection != null && stepProc.unitSelection == null)
                        stepProc.unitSelection = stepCurrent.unitSelection;
                    
                    if (stepCurrent.camera != null && stepProc.camera == null)
                        stepProc.camera = stepCurrent.camera;
                    
                    if (stepCurrent.musicMood != null && stepProc.musicMood == null)
                        stepProc.musicMood = stepCurrent.musicMood;
                    
                    if (stepCurrent.musicIntensity != null && stepProc.musicIntensity == null)
                        stepProc.musicIntensity = stepCurrent.musicIntensity;
                    
                    if (stepCurrent.musicReactive != null && stepProc.musicReactive == null)
                        stepProc.musicReactive = stepCurrent.musicReactive;
                    
                    if (stepCurrent.commsOnStart != null && stepProc.commsOnStart == null)
                        stepProc.commsOnStart = stepCurrent.commsOnStart;
                    
                    if (stepCurrent.audioEventsOnStart != null && stepProc.audioEventsOnStart == null)
                        stepProc.audioEventsOnStart = stepCurrent.audioEventsOnStart;
                    
                    if (stepCurrent.audioStatesOnStart != null && stepProc.audioStatesOnStart == null)
                        stepProc.audioStatesOnStart = stepCurrent.audioStatesOnStart;
                    
                    if (stepCurrent.audioSyncsOnStart != null && stepProc.audioSyncsOnStart == null)
                        stepProc.audioSyncsOnStart = stepCurrent.audioSyncsOnStart;
                    
                    // if (stepCurrent.functions != null && stepProc.functions == null)
                    //     stepProc.functions = stepCurrent.functions;

                    if (stepCurrent.outcome != null && stepProc.outcome == null)
                        stepProc.outcome = stepCurrent.outcome;
                }
            }
            
            if (current.unitPresets != null && current.unitPresets.Count > 0)
            {
                if (root.unitPresetsProc == null)
                    root.unitPresetsProc = new SortedDictionary<string, DataBlockScenarioUnit> ();
                
                foreach (var kvp in current.unitPresets)
                {
                    var unitCurrent = kvp.Value;
                    if (unitCurrent == null)
                        continue;

                    var unitKey = kvp.Key;
                    if (!root.unitPresetsProc.ContainsKey (unitKey))
                        root.unitPresetsProc.Add (unitKey, unitCurrent);
                }
            }
            
            if (current.unitGenerators != null && current.unitGenerators.Count > 0)
            {
                if (root.unitGeneratorsProc == null)
                    root.unitGeneratorsProc = new SortedDictionary<string, DataBlockScenarioUnitGenerationNode> ();
                
                foreach (var kvp in current.unitGenerators)
                {
                    var unitGeneratorCurrent = kvp.Value;
                    if (unitGeneratorCurrent == null)
                        continue;

                    var unitGeneratorKey = kvp.Key;
                    if (!root.unitGeneratorsProc.ContainsKey (unitGeneratorKey))
                        root.unitGeneratorsProc.Add (unitGeneratorKey, unitGeneratorCurrent);
                }
            }

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Scenario {current.key} fails to complete recursive processing in under 20 steps.");
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
                    Debug.LogWarning ($"Scenario {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Scenario {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Scenario {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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

        private static void TransferNonSerializedDataToClones (List<DataBlockScenarioUnitGroup> groupsSource, List<DataBlockScenarioUnitGroup> groupsCloned)
        {
            if (groupsSource == null || groupsCloned == null || groupsSource.Count != groupsCloned.Count)
                return;
            
            // Set unserializable fields
            for (int i = 0, iLimit = groupsSource.Count; i < iLimit; ++i)
            {
                var groupSource = groupsSource[i];
                if (groupSource == null)
                    continue;

                if (groupSource is DataBlockScenarioUnitGroupEmbedded groupEmbeddedSource && groupEmbeddedSource.units != null && groupEmbeddedSource.units.Count > 0)
                {
                    var groupEmbedded = groupsCloned[i];
                    var groupEmbeddedCloned = groupEmbedded as DataBlockScenarioUnitGroupEmbedded;

                    if (groupEmbeddedCloned != null && groupEmbeddedCloned.units != null && groupEmbeddedCloned.units.Count == groupEmbeddedSource.units.Count)
                    {
                        for (int u = 0, uLimit = groupEmbeddedSource.units.Count; u < uLimit; ++u)
                        {
                            var unitSource = groupEmbeddedSource.units[u];
                            var unitCloned = groupEmbeddedCloned.units[u];

                            if (unitSource.custom != null)
                            {
                                if (unitSource.custom.id != null)
                                    unitCloned.custom.id.nameOverride = unitSource.custom.id.nameOverride;

                                if (unitSource.custom.idPilot != null)
                                {
                                    unitCloned.custom.idPilot.callsignOverride = unitSource.custom.idPilot.callsignOverride;
                                    unitCloned.custom.idPilot.nameOverride = unitSource.custom.idPilot.nameOverride;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /*
        [FoldoutGroup ("Upgrade methods", false)]
        [Button ("Upgrade state distance filters", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeStateDistanceFilters ()
        {
            foreach (var kvp in data)
            {
                var s = kvp.Value;
                if (s.steps == null)
                    continue;

                foreach (var kvp2 in s.states)
                {
                    var state = kvp2.Value;
                    if (state == null || state.location == null || state.location.locationProvider == null)
                        continue;

                    if (state.location.locationProvider is DataBlockAreaLocationTagFilter p)
                    {
                        if (p.filterDistanceState == null)
                            continue;

                        if (p.filters == null)
                            p.filters = new List<ICombatPositionValidationFunction> ();

                        p.filters.Add (new CombatPositionValidationByStateDistance
                        {
                            stateKey = p.filterDistanceState.key,
                            checkDistance = new DataBlockOverworldEventSubcheckFloat
                            {
                                check = p.filterDistanceState.check,
                                value = p.filterDistanceState.value
                            }
                        });

                        p.filterDistanceState = null;
                    }
                }
            }
        }
        */

        private static SortedDictionary<string, bool> areaRequirementsCombined = new SortedDictionary<string, bool> ();


        [HideReferenceObjectPicker]
        private class DuplicateTextInstance
        {
            public string scenario;
            public string blockKey;

            public override string ToString () { return $"{scenario} / {blockKey}"; }
        }

        private class DuplicateTextGroup
        {
            public bool replaceWithFallback = false;
        
            [HideIf ("replaceWithFallback")]
            public string key;
            
            [TextArea (1, 10), HideLabel]
            public string text;
            public List<DuplicateTextInstance> instances = new List<DuplicateTextInstance> ();
        }
        
        [FoldoutGroup ("Upgrade methods", false)]
        [ShowInInspector, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, HideAddButton = true)]
        private static List<DuplicateTextGroup> headerDuplicationSteps = new List<DuplicateTextGroup> ();
        
        [FoldoutGroup ("Upgrade methods", false)]
        [ShowInInspector, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, HideAddButton = true)]
        private static List<DuplicateTextGroup> contentDuplicationSteps = new List<DuplicateTextGroup> ();
        
        [FoldoutGroup ("Upgrade methods", false)]
        [ShowInInspector, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, HideAddButton = true)]
        private static List<DuplicateTextGroup> headerDuplicationStates = new List<DuplicateTextGroup> ();
        
        [FoldoutGroup ("Upgrade methods", false)]
        [ShowInInspector, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, HideAddButton = true)]
        private static List<DuplicateTextGroup> contentDuplicationStates = new List<DuplicateTextGroup> ();

        [FoldoutGroup ("Upgrade methods", false)]
        [Button ("Clear state text keys", ButtonSizes.Large), PropertyOrder (-10)]
        public void ClearStateTextKeys ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;

                if (scenario.statesProc != null)
                {
                    foreach (var kvp2 in scenario.statesProc)
                    {
                        var state = kvp2.Value;
                        if (state == null)
                            continue;

                        state.textNameKey = string.Empty;
                        state.textDescKey = string.Empty;
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print execution locks", ButtonSizes.Large), PropertyOrder (-10)]
        private static void PrintExecutionLocks ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario == null || scenario.stepsProc == null)
                    continue;

                foreach (var kvp2 in scenario.stepsProc)
                {
                    var step = kvp2.Value;
                    if (step.core != null && !step.core.executionAllowed)
                        Debug.LogWarning ($"{kvp.Key} / {kvp2.Key}: execution not allowed");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print hints", ButtonSizes.Large), PropertyOrder (-10)]
        private static void PrintHints ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario == null || scenario.stepsProc == null)
                    continue;

                foreach (var kvp2 in scenario.stepsProc)
                {
                    var step = kvp2.Value;
                    if (step.hintsConditional != null)
                    {
                        for (int i = 0; i < step.hintsConditional.Count; ++i)
                        {
                            var hint = step.hintsConditional[i];
                            Debug.LogWarning ($"{kvp.Key} / {kvp2.Key}: Hint {i + 1}\n- {hint.data?.text}");
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print dynamic music", ButtonSizes.Large), PropertyOrder (-10)]
        private static void PrintDynamicMusic ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario.coreProc.musicDynamic)
                {
                    Debug.Log ($"Scenario {scenario.key} has dynamic music");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log usage", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogUsage ()
        {
            var sb = new StringBuilder ();
            
            var presets = DataMultiLinkerOverworldPointPreset.data;
            foreach (var kvp in presets)
            {
                var preset = kvp.Value;
                if (preset.hidden)
                    continue;

                var c = preset.combatProc;
                if (c == null || c.scenarioFilter == null)
                    continue;
                
                c.scenarioFilter.Refresh ();
                var scenarioKeys = c.scenarioFilter.GetFilteredKeys (DataMultiLinkerScenario.data);
                var areaKeys = c.areaFilter != null ? c.areaFilter.GetFilteredKeys (DataMultiLinkerCombatArea.data) : null;

                sb.Append ("\n\n");
                sb.Append (preset.key);
                
                int i = 0;
                foreach (var scenarioKey in scenarioKeys)
                {
                    var scenario = DataMultiLinkerScenario.GetEntry (scenarioKey, false);
                    if (scenario == null || scenario.hidden)
                        continue;

                    sb.Append ($"\n\n{scenario.key}");
                    
                    i += 1;
                    
                    if (scenario.areasProc.tagFilterUsed)
                    {
                        sb.Append ($" (random level): ");
                        areaRequirementsCombined.Clear ();

                        var areasMatchingTag = DataTagUtility.GetKeysWithTags (DataMultiLinkerCombatArea.data, scenario.areasProc.tagFilter);
                        foreach (var areaKey in areasMatchingTag)
                            sb.Append ($"\n- {areaKey}");
                    }
                    else
                    {
                        sb.Append ($" (fixed level): ");
                        sb.Append ($"\n- {scenario.areasProc.keys.ToStringFormatted ()}");
                    }
                }
            }

            var report = sb.ToString ();
            Debug.Log (report);
            GUIUtility.systemCopyBuffer = report;
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Tag for unit injection", ButtonSizes.Large), PropertyOrder (-10)]
        private static void TagForUnitInjection ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario == null)
                    continue;

                if (scenario.stepsProc != null)
                {
                    foreach (var kvp2 in scenario.stepsProc)
                    {
                        var step = kvp2.Value;
                        if (kvp2.Key == scenario.coreProc.stepOnStart)
                            step.tags = new HashSet<string> { ScenarioBlockTags.Start };
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log unit checks in states", ButtonSizes.Large), PropertyOrder (-10)]
        private static void LogUnitChecks ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario == null)
                    continue;

                if (scenario.statesProc != null)
                {
                    foreach (var kvp2 in scenario.statesProc)
                    {
                        var state = kvp2.Value;
                        if (state == null || state.unitChecks == null || state.unitChecks.Count == 0)
                            continue;

                        if (state.unitChecks.Count == 1)
                            Debug.Log ($"Scenario {scenario.key} state {kvp2.Key} has 1 unit check");
                        else
                            Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} has {state.unitChecks.Count} unit checks");
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button ("Log embedded units", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogEmbeddedUnits ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario.stepsProc != null)
                {
                    foreach (var kvp2 in scenario.stepsProc)
                    {
                        var step = kvp2.Value;
                        if (step.unitGroups == null)
                            continue;

                        for (int i = 0, count = step.unitGroups.Count; i < count; ++i)
                        {
                            var unitGroupResolver = step.unitGroups[i];
                            if (unitGroupResolver is DataBlockScenarioUnitGroupEmbedded unitGroupResolverEmbedded)
                                Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} has embedded unit group {i}");
                        }
                    }
                }

                if (scenario.statesProc != null)
                {
                    foreach (var kvp2 in scenario.statesProc)
                    {
                        var state = kvp2.Value;
                        if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                            continue;

                        foreach (var kvp3 in state.reactions.effectsPerIncrement)
                        {
                            var effect = kvp3.Value;
                            if (effect == null || effect.unitGroups == null)
                                continue;

                            for (int i = 0, count = effect.unitGroups.Count; i < count; ++i)
                            {
                                var unitGroupResolver = effect.unitGroups[i];
                                if (unitGroupResolver is DataBlockScenarioUnitGroupEmbedded unitGroupResolverEmbedded)
                                    Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} has embedded unit group {i}");
                            }
                        }
                    }
                }

                if (scenario.unitPresetsProc != null)
                {
                    Debug.Log ($"Scenario {scenario.key} has {scenario.unitPresetsProc.Count} embedded units");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Simplify unit preset links", ButtonSizes.Large), PropertyOrder (-10)]
        public void SimplifyUnitPresetLinks ()
        {
            var unitPresetsEmbeddedRemoved = new HashSet<string> ();
            var unitPresetsEmbeddedUnique = new HashSet<string> ();
            
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                var unitPresetsEmbedded = scenario.unitPresets;
                
                unitPresetsEmbeddedRemoved.Clear ();
                unitPresetsEmbeddedUnique.Clear ();
                
                if (scenario.steps != null)
                {
                    foreach (var kvp2 in scenario.steps)
                    {
                        var step = kvp2.Value;
                        if (step.unitGroups == null)
                            continue;

                        for (int i = 0, count = step.unitGroups.Count; i < count; ++i)
                        {
                            var unitGroupResolver = step.unitGroups[i];
                            if (unitGroupResolver is DataBlockScenarioUnitGroupEmbedded unitGroupResolverEmbedded)
                            {
                                var units = unitGroupResolverEmbedded.units;
                                if (units == null || units.Count == 0)
                                {
                                    Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} has embedded unit group {i}: empty");
                                    continue;
                                }

                                int u = -1;
                                foreach (var unitSlot in units)
                                {
                                    u += 1;
                                    if (unitSlot == null)
                                    {
                                        Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} is null");
                                        continue;
                                    }
                                    
                                    if (unitSlot.keyExternal)
                                        continue;

                                    var unitPresetLink = unitPresetsEmbedded != null && unitPresetsEmbedded.TryGetValue (unitSlot.key, out var l) ? l : null;
                                    if (unitPresetLink == null)
                                    {
                                        Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} with internal key {unitSlot.key} has no link data");
                                        continue;
                                    }

                                    if (unitPresetLink is DataBlockScenarioUnitPresetLink unitPresetLinkKey)
                                    {
                                        Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} can drop redundant internal link data at key {unitSlot.key} and directly link to preset {unitPresetLinkKey.preset}");
                                        unitPresetsEmbeddedRemoved.Add (unitSlot.key);
                                        unitSlot.keyExternal = true;
                                        unitSlot.key = unitPresetLinkKey.preset;
                                    }
                                    else if (unitPresetLink is DataBlockScenarioUnitPresetEmbedded unitPresetLinkEmbedded)
                                    {
                                        Debug.LogError ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} with internal key {unitSlot.key} is an illegal embedded preset");
                                    }
                                }
                            }
                        }
                    }
                }

                if (scenario.states != null)
                {
                    foreach (var kvp2 in scenario.states)
                    {
                        var state = kvp2.Value;
                        if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                            continue;

                        foreach (var kvp3 in state.reactions.effectsPerIncrement)
                        {
                            var effect = kvp3.Value;
                            if (effect == null || effect.unitGroups == null)
                                continue;

                            for (int i = 0, count = effect.unitGroups.Count; i < count; ++i)
                            {
                                var unitGroupResolver = effect.unitGroups[i];
                                if (unitGroupResolver is DataBlockScenarioUnitGroupEmbedded unitGroupResolverEmbedded)
                                {
                                    var units = unitGroupResolverEmbedded.units;
                                    if (units == null || units.Count == 0)
                                    {
                                        Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} has embedded unit group {i}: empty");
                                        continue;
                                    }

                                    int u = -1;
                                    foreach (var unitSlot in units)
                                    {
                                        u += 1;
                                        if (unitSlot == null)
                                        {
                                            Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} is null");
                                            continue;
                                        }
                                        
                                        if (unitSlot.keyExternal)
                                            continue;

                                        var unitPresetLink = unitPresetsEmbedded != null && unitPresetsEmbedded.TryGetValue (unitSlot.key, out var l) ? l : null;
                                        if (unitPresetLink == null)
                                        {
                                            Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} with internal key {unitSlot.key} has no link data");
                                            continue;
                                        }

                                        if (unitPresetLink is DataBlockScenarioUnitPresetLink unitPresetLinkKey)
                                        {
                                            Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} can drop redundant internal link data at key {unitSlot.key} and directly link to preset {unitPresetLinkKey.preset}");
                                            unitPresetsEmbeddedRemoved.Add (unitSlot.key);
                                            unitSlot.keyExternal = true;
                                            unitSlot.key = unitPresetLinkKey.preset;
                                        }
                                        else if (unitPresetLink is DataBlockScenarioUnitPresetEmbedded unitPresetLinkEmbedded)
                                        {
                                            Debug.LogError ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} with internal key {unitSlot.key} is an illegal embedded preset");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (unitPresetsEmbeddedRemoved.Count > 0)
                {
                    Debug.Log ($"Scenario {scenario.key} no longer needs {unitPresetsEmbeddedRemoved.Count}/{unitPresetsEmbedded.Count} embedded unit presets: {unitPresetsEmbeddedRemoved.ToStringFormatted ()}");
                    foreach (var unitPresetKey in unitPresetsEmbeddedRemoved)
                        unitPresetsEmbedded.Remove (unitPresetKey);
                    
                    if (unitPresetsEmbedded.Count > 0)
                        Debug.Log ($"Scenario {scenario.key} has the following embedded presets left: {unitPresetsEmbedded.ToStringFormattedKeys ()}");
                    else
                        Debug.Log ($"Scenario {scenario.key} has no more internal unit presets!");
                }

                if (scenario.unitPresets != null && scenario.unitPresets.Count == 0)
                    scenario.unitPresets = null;
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log broken unit preset links", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogBrokenUnitPresetLinks ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                var unitLinks = scenario.unitPresets;
                
                if (scenario.steps != null)
                {
                    foreach (var kvp2 in scenario.steps)
                    {
                        var step = kvp2.Value;
                        if (step.unitGroups == null)
                            continue;

                        for (int i = 0, count = step.unitGroups.Count; i < count; ++i)
                        {
                            var unitGroupResolver = step.unitGroups[i];
                            if (unitGroupResolver is DataBlockScenarioUnitGroupEmbedded unitGroupResolverEmbedded)
                            {
                                var units = unitGroupResolverEmbedded.units;
                                if (units == null || units.Count == 0)
                                {
                                    Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} has embedded unit group {i}: empty");
                                    continue;
                                }

                                int u = -1;
                                foreach (var unitSlot in units)
                                {
                                    u += 1;
                                    if (unitSlot == null)
                                    {
                                        Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} is null");
                                        continue;
                                    }
                                    
                                    if (unitSlot.keyExternal)
                                    {
                                        var unitPreset = DataMultiLinkerUnitPreset.GetEntry (unitSlot.key, false);
                                        if (unitPreset == null)
                                            Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} has invalid unit preset (external) key {unitSlot.key}");
                                    }
                                    else
                                    {
                                        var unitPresetLink = unitLinks != null && unitLinks.TryGetValue (unitSlot.key, out var l) ? l : null;
                                        if (unitPresetLink == null)
                                            Debug.LogWarning ($"Scenario {scenario.key} step {kvp2.Key} embedded unit group {i} unit {u} has invalid internal unit key {unitSlot.key}");
                                    }
                                }
                            }
                        }
                    }
                }

                if (scenario.states != null)
                {
                    foreach (var kvp2 in scenario.states)
                    {
                        var state = kvp2.Value;
                        if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                            continue;

                        foreach (var kvp3 in state.reactions.effectsPerIncrement)
                        {
                            var effect = kvp3.Value;
                            if (effect == null || effect.unitGroups == null)
                                continue;

                            for (int i = 0, count = effect.unitGroups.Count; i < count; ++i)
                            {
                                var unitGroupResolver = effect.unitGroups[i];
                                if (unitGroupResolver is DataBlockScenarioUnitGroupEmbedded unitGroupResolverEmbedded)
                                {
                                    var units = unitGroupResolverEmbedded.units;
                                    if (units == null || units.Count == 0)
                                    {
                                        Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} has embedded unit group {i}: empty");
                                        continue;
                                    }

                                    int u = -1;
                                    foreach (var unitSlot in units)
                                    {
                                        u += 1;
                                        if (unitSlot == null)
                                        {
                                            Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} is null");
                                            continue;
                                        }
                                    
                                        if (unitSlot.keyExternal)
                                        {
                                            var unitPreset = DataMultiLinkerUnitPreset.GetEntry (unitSlot.key, false);
                                            if (unitPreset == null)
                                                Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} has invalid unit preset (external) key {unitSlot.key}");
                                        }
                                        else
                                        {
                                            var unitPresetLink = unitLinks != null && unitLinks.TryGetValue (unitSlot.key, out var l) ? l : null;
                                            if (unitPresetLink == null)
                                                Debug.LogWarning ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} embedded unit group {i} unit {u} has invalid internal unit key {unitSlot.key}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (scenario.unitPresets != null && scenario.unitPresets.Count == 0)
                    scenario.unitPresets = null;
            }
        }

        private static SortedDictionary<string, bool> tagsCombined = new SortedDictionary<string, bool> ();
        
        private void LogAmbiguousUnitFilter (string prefix, DataBlockScenarioUnitGroup unitGroupResolver)
        {
            if (unitGroupResolver is DataBlockScenarioUnitGroupFilter unitGroupResolverFilter)
            {
                if (unitGroupResolverFilter.tags == null || unitGroupResolverFilter.tags.Count == 0)
                {
                    Debug.LogError ($"{prefix} has no tag filter");
                    return;
                }
                                
                var units = unitGroupResolverFilter.groupsFiltered;
                if (units == null || units.Count == 0)
                {
                    Debug.LogError ($"{prefix} failed to find any units");
                    return;
                }

                bool allNegative = true;
                foreach (var kvp3 in unitGroupResolverFilter.tags)
                {
                    if (kvp3.Value)
                    {
                        allNegative = false;
                        break;
                    }
                }

                if (allNegative)
                {
                    var factionBranchKeyExternal = unitGroupResolverFilter.branchKeyDebug;
                    var tagFilterFinal = unitGroupResolverFilter.GetTagFilterFinalized (factionBranchKeyExternal, out bool tagFilterWasModified);
                    var unitGroupsFound = unitGroupResolverFilter.GetFilteredUnitGroups (tagFilterFinal, tagFilterWasModified);
                    
                    Debug.LogWarning ($"{prefix} has an all-negative tag filter ({(tagFilterWasModified ? "modified by branch" : "no branch")}):\n{tagFilterFinal.ToStringFormattedKeyValuePairs (true, multilinePrefix: "- ")}\nFiltered units:\n{unitGroupsFound.ToStringFormatted (true, multilinePrefix: "- ")}");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log ambiguous unit filters", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogAmbiguousUnitFilters ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario.coreProc == null)
                {
                    Debug.Log ($"Scenario {scenario.key} has no processed core data");
                    continue;
                }
                
                if (!scenario.coreProc.externalBranchUsed)
                {
                    Debug.Log ($"Scenario {scenario.key} does not use an external branch");
                }
                
                if (scenario.steps != null)
                {
                    foreach (var kvp2 in scenario.steps)
                    {
                        var step = kvp2.Value;
                        if (step.unitGroups == null)
                            continue;

                        for (int i = 0, count = step.unitGroups.Count; i < count; ++i)
                        {
                            var unitGroupResolver = step.unitGroups[i];
                            LogAmbiguousUnitFilter ($"Scenario {scenario.key} step {step.key} unit group {i}", unitGroupResolver);
                        }
                    }
                }

                if (scenario.states != null)
                {
                    foreach (var kvp2 in scenario.states)
                    {
                        var state = kvp2.Value;
                        if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                            continue;

                        foreach (var kvp3 in state.reactions.effectsPerIncrement)
                        {
                            var effect = kvp3.Value;
                            if (effect == null || effect.unitGroups == null)
                                continue;

                            for (int i = 0, count = effect.unitGroups.Count; i < count; ++i)
                            {
                                var unitGroupResolver = effect.unitGroups[i];
                                LogAmbiguousUnitFilter ($"Scenario {scenario.key} state {kvp2.Key} reaction {kvp3.Key} unit group {i}", unitGroupResolver);
                            }
                        }
                    }
                }
            }
        }
        
        /*
        [FoldoutGroup ("Utilities", false)]
        [Button ("Fix unit changes", ButtonSizes.Large), PropertyOrder (-10)]
        public void FixUnitChanges ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario.steps != null)
                {
                    foreach (var kvp2 in scenario.steps)
                    {
                        var step = kvp2.Value;
                        if (step.unitChanges == null)
                            continue;

                        for (int i = 0, count = step.unitChanges.Count; i < count; ++i)
                        {
                            var change = step.unitChanges[i];
                            if (change.actions != null)
                            {
                                foreach (var action in change.actions)
                                {
                                    action.target = new TargetFromSource
                                    {
                                        type = action.targetSource,
                                        name = action.targetEntityName,
                                        center = action.targetCenter,
                                        mod = new TargetModification
                                        {
                                            offsetGlobalMin = action.targetPosition
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
        }
        */
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print comms", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogComms ()
        {
            int countTotal = 0;
            string commKeyFilter = null; // "generic";
            bool commKeyFilterUsed = !string.IsNullOrEmpty (commKeyFilter);
            var commKeysUsed = new HashSet<string> ();

            string scenarioKeyFilter = filter;
            bool scenarioKeyFilterUsed = !string.IsNullOrEmpty (scenarioKeyFilter);
            
            foreach (var kvp in data)
            {
                if (scenarioKeyFilterUsed && !kvp.Key.Contains (scenarioKeyFilter))
                    continue;
                
                var scenario = kvp.Value;
                int countLocal = 0;

                if (scenario.statesProc != null)
                {
                    foreach (var kvp2 in scenario.statesProc)
                    {
                        var state = kvp2.Value;
                        if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                            continue;

                        foreach (var kvp3 in state.reactions.effectsPerIncrement)
                        {
                            var effect = kvp3.Value;
                            if (effect == null || effect.commsOnStart == null)
                                continue;

                            for (int i = 0, count = effect.commsOnStart.Count; i < count; ++i)
                            {
                                var commLink = effect.commsOnStart[i];
                                if (commLink == null)
                                    continue;
                                
                                if (!string.IsNullOrEmpty (commLink.key) && !commKeysUsed.Contains (commLink.key))
                                    commKeysUsed.Add (commLink.key);
                                
                                if (commKeyFilterUsed && !commLink.key.Contains (commKeyFilter))
                                    continue;
                                
                                countTotal += 1;
                                countLocal += 1;
                                
                                var commData = DataMultiLinkerCombatComms.GetEntry (commLink.key);
                                if (commData != null)
                                    Debug.Log ($"Filter {filter} / {countLocal:D2} / Scenario {kvp.Key} / State {kvp2.Key} / Reaction {kvp3.Key} / Message {i} / {(commLink.hidden ? "Hidden" : "Active")} | T: {commLink.time} / Key: {commLink.key}\n{commData.textContent.ToStringFormatted (true, multilinePrefix: "- ")}");
                                else
                                    Debug.LogWarning ($"Filter {filter} / {countLocal:D2} / Scenario {kvp.Key} / State {kvp2.Key} / Reaction {kvp3.Key} / Message {i} / {(commLink.hidden ? "Hidden" : "Active")} | T: {commLink.time} / Key: {commLink.key}\n- No data found!");
                                
                                // commLink.hidden = true;
                            }
                        }
                    }
                }


                if (scenario.stepsProc != null)
                {
                    foreach (var kvp2 in scenario.stepsProc)
                    {
                        var step = kvp2.Value;
                        if (step == null || step.commsOnStart == null)
                            continue;

                        for (int i = 0, count = step.commsOnStart.Count; i < count; ++i)
                        {
                            var commLink = step.commsOnStart[i];
                            if (commLink == null)
                                continue;
                            
                            if (!string.IsNullOrEmpty (commLink.key) && !commKeysUsed.Contains (commLink.key))
                                commKeysUsed.Add (commLink.key);
                            
                            if (commKeyFilterUsed && !commLink.key.Contains (commKeyFilter))
                                continue;

                            countTotal += 1;
                            countLocal += 1;
                            
                            var commData = DataMultiLinkerCombatComms.GetEntry (commLink.key);
                            if (commData != null)
                                Debug.Log ($"Filter {filter} / {countLocal:D2} / Scenario {kvp.Key} / Step {kvp2.Key} / Message {i} / {(commLink.hidden ? "Hidden" : "Active")} / T: {commLink.time} / Key: {commLink.key}\n{commData.textContent.ToStringFormatted (true, multilinePrefix: "- ")}");
                            else
                                Debug.LogWarning ($"Filter {filter} / {countLocal:D2} / Scenario {kvp.Key} / Step {kvp2.Key} / Message {i} / {(commLink.hidden ? "Hidden" : "Active")} / T: {commLink.time} / Key: {commLink.key}\n- No data found!");

                            // commLink.hidden = true;
                        }
                    }
                }
                
                if (countLocal > 0)
                    Debug.Log ($"Scenario {scenario.key} has comms: {countLocal} | Filter: {(commKeyFilterUsed ? commKeyFilter : "-")}");
            }

            var commsData = DataMultiLinkerCombatComms.data;
            if (countTotal > 0)
                Debug.Log ($"Total comms found: {countTotal} | Filter: {(commKeyFilterUsed ? commKeyFilter : "-")} | Coverage: {commKeysUsed.Count}/{commsData.Count}");

            var sb = new StringBuilder ();
            sb.Append ("Unused comms:");
            foreach (var kvp in commsData)
            {
                var commsKey = kvp.Key;
                if (!commKeysUsed.Contains (commsKey))
                {
                    sb.Append ($"\n- {commsKey}"); //\n{kvp.Value.textContent.ToStringFormatted (true, multilinePrefix: "- ")}");
                }
            }
            
            Debug.Log (sb.ToString ());
        }
        
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log reactions on outcomes", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogReactionsONOutcome ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario == null || scenario.statesProc == null)
                    continue;

                foreach (var kvp2 in scenario.statesProc)
                {
                    var state = kvp2.Value;
                    if (state == null || state.reactions == null || state.reactions.effectsPerIncrement == null)
                        continue;

                    var r = state.reactions;
                    foreach (var kvp3 in r.effectsPerIncrement)
                    {
                        var effect = kvp3.Value;
                        if (effect.executionOnOutcome == null)
                            continue;

                        var m = effect.executionOnOutcome;
                        var report = $"Scenario {scenario.key}, state {state.key}, reaction {kvp3.Key} triggered on exit";
                        if (m.outcomeVictory)
                        {
                            if (m.caseEarly && m.caseTotal)
                                report += " | Any victory";
                            else if (m.caseEarly)
                                report += " | Early/retreat victory";
                            else if (m.caseTotal)
                                report += " | Total victory";
                        }
                        if (m.outcomeDefeat)
                        {
                            if (m.caseEarly && m.caseTotal)
                                report += " | Any defeat";
                            else if (m.caseEarly)
                                report += " | Early/retreat defeat";
                            else if (m.caseTotal)
                                report += " | Total defeat";
                        }
                        
                        Debug.Log (report);
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log state evaluation contexts", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogStateEvaluationContexts ()
        {
            foreach (var kvp in data)
            {
                var scenario = kvp.Value;
                if (scenario == null || scenario.statesProc == null)
                    continue;

                foreach (var kvp2 in scenario.statesProc)
                {
                    var state = kvp2.Value;
                    if (state == null || !state.evaluated)
                        continue;

                    if (state.evaluationContext != ScenarioStateRefreshContext.OnExecutionEnd)
                    {
                        var report = $"Scenario {scenario.key}, state {state.key} is evaluated on: {state.evaluationContext}";
                        if (state.evaluationOnOutcome != null)
                            report += $" | End of combat {(state.evaluationOnOutcome.present ? "required" : "prohibited")}";
                        Debug.LogWarning (report);
                    }
                }
            }
        }
        
        #if UNITY_EDITOR

        public static IEnumerable<string> GetSelectedScenarioStateKeys ()
        {
            if (selectedScenario == null || selectedScenario.statesProc == null)
                return null;
            return selectedScenario.statesProc.Keys;
        }

        #endif
    }
}