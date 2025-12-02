using System;
using System.Collections.Generic;
using System.Linq;
using Content.Code.Utility;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using System.Text;
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
#endif


namespace PhantomBrigade.Data
{
    public class DataBlockOverworldQuestParent : DataContainerParent
    {
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerOverworldQuest.data.Keys;
    }

    public class DataBlockOverworldQuestCountdown
    {
        [OnValueChanged (nameof(UpdateCachedColors)), PropertyRange (0f, 1f)]
        public float hueMain = 0f;
        
        [OnValueChanged (nameof(UpdateCachedColors)), PropertyRange (0f, 1f)]
        public float hueShort = 0f;
        
        [OnValueChanged (nameof(UpdateCachedColors)), PropertyRange (0f, 1f)]
        public float hueDelay = 0.56f;
        
        [YamlIgnore, ShowInInspector, ReadOnly, ColorUsage (false)]
        public Color colorMainBody = Color.HSVToRGB (0f, 0.5f, 0.8f).WithAlpha (1f);
        
        [YamlIgnore, ShowInInspector, ReadOnly, ColorUsage (false)]
        public Color colorMainHighlight = Color.HSVToRGB (0f, 0.5f, 1f).WithAlpha (1f);
        
        [YamlIgnore, ShowInInspector, ReadOnly, ColorUsage (false)]
        public Color colorHintNegative = Color.HSVToRGB (0f, 0.5f, 1f).WithAlpha (1f);
        
        [YamlIgnore, ShowInInspector, ReadOnly, ColorUsage (false)]
        public Color colorHintPositive = Color.HSVToRGB (0.56f, 0.5f, 1f).WithAlpha (1f);

        [YamlIgnore, ShowInInspector, ReadOnly, ColorUsage (false)]
        public Color colorShapeOffsetNegative = Color.HSVToRGB (0f, 0.6f, 0.8f).WithAlpha (1f);
        
        [YamlIgnore, ShowInInspector, ReadOnly, ColorUsage (false)]
        public Color colorShapeOffsetPositive = Color.HSVToRGB (0.56f, 0.6f, 0.8f).WithAlpha (1f);
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textHintMain;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textHintShortToEnd;

        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textHintShort;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textHintDelay;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textHintDelayToMax;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textTooltipMain;

        public void UpdateCachedColors ()
        {
            colorMainBody = Color.HSVToRGB (hueMain, 0.5f, 0.8f).WithAlpha (1f);
            colorMainHighlight = Color.HSVToRGB (hueMain, 0.5f, 1f).WithAlpha (1f);
            
            colorHintNegative = Color.HSVToRGB (hueShort, 0.5f, 1f).WithAlpha (1f);
            colorShapeOffsetNegative = Color.HSVToRGB (hueShort, 0.6f, 0.8f).WithAlpha (0.65f);            
            
            colorHintPositive = Color.HSVToRGB (hueDelay, 0.5f, 1f).WithAlpha (1f);
            colorShapeOffsetPositive = Color.HSVToRGB (hueDelay, 0.6f, 0.8f).WithAlpha (0.65f);
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestCountdown () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    // [GUIColor ("@$value.GetColor ()")]
    [HideDuplicateReferenceBox]
    public class DataBlockOverworldQuestStep
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;

        [OnInspectorGUI ("DrawHeaderGUI", false)]
        [DropdownReference (true), InlineProperty]
        public DataBlockInt index;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textName;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textDesc;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textExit;
        
        [DropdownReference (true)]
        public DataBlockOverworldQuestCountdown countdownCustom;
        
        // [DropdownReference (true)]
        // public DataBlockStringNonSerializedLong textCountdown;

        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public HashSet<string> memoryDisplayed;
        
        [DropdownReference, GUIColor ("@GetSectionColor (3)")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockOverworldQuestCondition> conditions;
        
        [DropdownReference, PropertyOrder (5), GUIColor ("@GetSectionColor (5)")]
        [LabelText ("Effects on events")]
        public List<DataBlockOverworldQuestEffectGroupContextual> effectsOnEvents;

        [DropdownReference (true), PropertyOrder (4), GUIColor ("@GetSectionColor (4)")]
        [LabelText ("Effects on entry")]
        public DataBlockOverworldQuestEffectGroup effectsOnEntry;
        
        [DropdownReference (true), PropertyOrder (6), GUIColor ("@GetSectionColor (6)")]
        [LabelText ("Effects on exit")]
        public DataBlockOverworldQuestEffectGroup effectsOnExit;
        
        [DropdownReference (true), PropertyOrder (7), GUIColor ("@GetSectionColor (7)")]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldQuest\", \"GetStepKeys\")")]
        public string stepNext;
        
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataContainerOverworldQuest parent;
        
        #region Editor
        #if UNITY_EDITOR
        
        private Color GetColor ()
        {
            return DataEditor.GetColorFromElementIndex (index != null ? index.i : 0, 1f);
        }
        
        private Color GetSectionColor (int section)
        {
            return Color.HSVToRGB ((float)section / 10f + 0.1f, 0.15f, 1f);
        }
        
        private void DrawHeaderGUI ()
        {
            if (index == null || parent == null || parent.steps == null)
                return;
            
            var rect = UnityEditor.EditorGUILayout.BeginVertical ();
            GUILayout.Label (" ", GUILayout.Height (12));
            UnityEditor.EditorGUILayout.EndVertical ();

            var gc = GUI.color;
            var col = GetColor ();
            var rect2 = rect.Expand (-2);
            
            float progress = Mathf.Clamp01 (index.i / (float)parent.steps.Count);

            GUI.color = Color.Lerp (Color.black, col, 0.5f);
            GUI.DrawTexture (rect, Texture2D.whiteTexture);
                
            GUI.color = col;
            GUI.DrawTexture (new Rect (rect2.x, rect2.y, rect2.width * progress, rect2.height), Texture2D.whiteTexture);
            
            GUI.color = gc;
        }
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestStep () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockOverworldQuestEffectCombatEnd
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsSite = new List<IOverworldTargetedFunction> ();
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestEffectCombatEnd () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockOverworldQuestEffect
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestEffect () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockFilterEventEntity
    {
        public GameEventType eventType;
        public List<IOverworldEntityValidationFunction> checks;
    }

    public class DataBlockOverworldQuestEffectGroupContextual : DataBlockOverworldQuestEffectGroup
    {
        [PropertyOrder (-2)]
        [InfoBox ("$GetEarlyDesc")]
        public bool early = true;
        
        [PropertyOrder (-1)]
        [DropdownReference]
        [LabelText ("Event Type Filter"), LabelWidth (140f)]
        public HashSet<GameEventType> filterEventTypes;

        [PropertyOrder (-1)]
        [DropdownReference (true)]
        [LabelText ("Entity Event Filter"), LabelWidth (140f)]
        public DataBlockFilterEventEntity filterEventEntity;
        
        #region Editor
        #if UNITY_EDITOR

        private string GetEarlyDesc => 
            early ? 
                "This effect will execute before condition evaluation. Useful for effects that should be immediately checked in conditions, such as writing memory on point completion. However, this effect can't be prevented from running on step completion, since completion hasn't been evaluated yet." : 
                "This effect will execute after condition evaluation. Useful for effects such as spawning next batch of targets, which must be prevented from running if condition update determines that a step is over.";
        
        #endif
        #endregion
    }

    [HideDuplicateReferenceBox]
    public class DataBlockOverworldQuestEffectGroup
    {
        [YamlIgnore, HideInInspector]
        public string key;
        
        [PropertyOrder (-10)]
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsEventEntity;

        [DropdownReference (true)]
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldQuest\", \"GetEffectKeys\")")]
        public HashSet<string> linkedEffectKeys;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestEffectGroup () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    
    public class DataBlockOverworldResolvedIntCheck : DataBlockOverworldEventSubcheckInt
    {
        [BoxGroup, HideLabel]
        public IOverworldIntValueFunction resolver;

        public bool IsPassedWithEntity (OverworldEntity entityChecked)
        {
            if (entityChecked == null || resolver == null)
                return false;

            var valueCurrent = resolver.Resolve (entityChecked);
            return base.IsPassed (true, valueCurrent);
        }
    }

    public class DataBlockOverworldResolvedIntThreshold
    {
        [BoxGroup ("Value", false)]
        public IOverworldIntValueFunction value;

        [BoxGroup ("Target", false)]
        public IOverworldIntValueFunction target;
    }
    
    public class DataBlockOverworldQuestCondition
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;

        public int priority = 0;

        [DropdownReference]
        public HashSet<GameEventType> filterEventTypes;
        
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string textDesc;
        
        [DropdownReference (true)]
        public DataBlockInt completionTarget;
        
        [DropdownReference (true)]
        public DataBlockOverworldResolvedIntThreshold checkValue;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataContainerOverworldQuest parent;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestCondition () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockOverworldQuestCore
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldQuest\", \"GetStepKeys\")")]
        public string stepOnStart;
        
        public bool mainType = true;
        public bool recordPointCompletion = true;
        public bool recordPointMemory = true;
    }

    public class DataBlockOverworldQuestEffectLinkValidated
    {
        // Check entity with validation functions here
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldQuestEffectLinkValidated () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [LabelWidth (160f)]
    public class DataContainerOverworldQuest : DataContainerWithText, IDataContainerTagged
    {
        private const string tgCore = "Core";
        private const string tgSteps = "Steps";
        private const string tgShared = "Shared";
        
        [TabGroup (tgCore)]
        [ToggleLeft]
        public bool hidden = false;
        
        [TabGroup (tgCore)]
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockOverworldInteractionParent ()")]
        public List<DataBlockOverworldQuestParent> parents;
        
        [TabGroup (tgCore)]
        [YamlIgnore, ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children;
        
        [TabGroup (tgCore)]
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textName;
        
        [TabGroup (tgCore)]
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockStringNonSerialized textNameProc;
        
        
        [TabGroup (tgCore)]
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textDesc;
        
        [TabGroup (tgCore)]
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockStringNonSerializedLong textDescProc;
       
        
        [TabGroup (tgCore)]
        [DropdownReference (true)]
        public DataBlockOverworldQuestCore core;
        
        [TabGroup (tgCore)]
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldQuestCore coreProc;

        
        [TabGroup (tgCore)]
        [DropdownReference]
        public HashSet<string> tags;
        
        [TabGroup (tgCore)]
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public HashSet<string> tagsProc;

        /*
        [TabGroup (tgSteps)]
        [YamlIgnore, ShowIf ("IsDisplayIsolated"), ShowInInspector]
        [ValueDropdown (nameof(GetStepKeys))]
        private string stepIsolatedKey;
        
        [TabGroup (tgSteps)]
        [YamlIgnore, ShowIf ("IsDisplayIsolated"), ShowInInspector, HideReferenceObjectPicker, HideDuplicateReferenceBox, HideLabel]
        [OnValueChanged ("UpdateSteps", true)]
        private DataBlockOverworldQuestStep stepIsolated
        {
            get
            {
                if (steps == null || steps.Count == 0)
                    return null;

                if (string.IsNullOrEmpty (stepIsolatedKey) || !steps.TryGetValue (stepIsolatedKey, out var v))
                {
                    stepIsolatedKey = steps.Keys.First ();
                    v = steps[stepIsolatedKey];
                }

                return v;
            }
            set
            {
                
            }
        }
        */
        
        [TabGroup (tgSteps), YamlIgnore, HideLabel, ShowIf ("IsDisplayIsolated"), ShowInInspector]
        private DataViewIsolatedDictionary<DataBlockOverworldQuestStep> stepIsolated; 
        
        [TabGroup (tgSteps)]
        [DropdownReference]
        [HideIf ("IsDisplayIsolated")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        [OnValueChanged ("UpdateSteps", true)]
        public SortedDictionary<string, DataBlockOverworldQuestStep> steps;
        
        [TabGroup (tgSteps)]
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockOverworldQuestStep> stepsProc;
        
        /*
        [TabGroup (tgShared)]
        [YamlIgnore, ShowIf ("IsDisplayIsolated"), ShowInInspector]
        [ValueDropdown (nameof(GetEffectKeys))]
        private string effectIsolatedKey;
        
        [TabGroup (tgShared)]
        [YamlIgnore, ShowIf ("IsDisplayIsolated"), ShowInInspector, HideReferenceObjectPicker, HideDuplicateReferenceBox, HideLabel]
        private DataBlockOverworldQuestEffectGroup effectIsolated
        {
            get
            {
                if (effectsShared == null || effectsShared.Count == 0)
                    return null;

                if (string.IsNullOrEmpty (effectIsolatedKey) || !effectsShared.TryGetValue (effectIsolatedKey, out var v))
                {
                    effectIsolatedKey = effectsShared.Keys.First ();
                    v = effectsShared[effectIsolatedKey];
                }

                return v;
            }
            set
            {
                
            }
        }
        */
        
        [TabGroup (tgShared), YamlIgnore, HideLabel, ShowIf ("IsDisplayIsolated"), ShowInInspector]
        private DataViewIsolatedDictionary<DataBlockOverworldQuestEffectGroup> effectIsolated; 
        
        [TabGroup (tgShared)]
        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        [HideIf ("IsDisplayIsolated")]
        public SortedDictionary<string, DataBlockOverworldQuestEffectGroup> effectsShared;
        
        [TabGroup (tgShared)]
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockOverworldQuestEffectGroup> effectsSharedProc;
        
        
        public bool IsHidden () => hidden;
        
        public HashSet<string> GetTags (bool processed) => 
            processed ? tagsProc : tags;
        
        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);
            
            foreach (var kvp in DataMultiLinkerOverworldQuest.data)
            {
                var entry = kvp.Value;
                if (entry.parents != null)
                {
                    for (int i = 0; i < entry.parents.Count; ++i)
                    {
                        var parent = entry.parents[i];
                        if (parent != null && parent.key == keyOld)
                        {
                            Debug.LogWarning ($"Quest {kvp.Key}, parent block {i} | Replacing entity key: {keyOld} -> {keyNew})");
                            parent.key = keyNew;
                        }
                    }
                }
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            UpdateSteps ();

            #if UNITY_EDITOR
            
            stepIsolated = new DataViewIsolatedDictionary<DataBlockOverworldQuestStep> 
            (
                "Steps", 
                () => steps, 
                () => GetStepKeys,
                UpdateSteps
            );
            
            effectIsolated = new DataViewIsolatedDictionary<DataBlockOverworldQuestEffectGroup> 
            (
                "Effects", 
                () => effectsShared, 
                () => GetEffectKeys
            );
            
            #endif
        }

        private void UpdateSteps ()
        {
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        continue;

                    step.key = stepKey;
                    step.parent = this;
                    
                    if (step.conditions != null)
                    {
                        foreach (var kvp2 in step.conditions)
                        {
                            var conditionKey = kvp2.Key;
                            var condition = kvp2.Value;

                            if (condition == null)
                                continue;

                            condition.key = conditionKey;
                            condition.parent = this;
                        }
                    }
                    
                    if (step.countdownCustom != null)
                        step.countdownCustom.UpdateCachedColors ();
                }
            }
        }

        public override void ResolveText ()
        {
            if (textName != null)
                textName.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{key}_a_header");
                    
            if (textDesc != null)
                textDesc.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{key}_a_text");
            
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        continue;

                    var keyPrefix = $"{key}__s_{stepKey}";
                    
                    if (step.textName != null)
                        step.textName.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_a_header");
                    
                    if (step.textDesc != null)
                        step.textDesc.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_a_text");
                    
                    if (step.textExit != null)
                        step.textExit.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_b_exit");
                    
                    // if (step.textCountdown != null)
                    //     step.textCountdown.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_c_countdown");

                    if (step.conditions != null)
                    {
                        foreach (var kvp2 in step.conditions)
                        {
                            var conditionKey = kvp2.Key;
                            var condition = kvp2.Value;

                            if (condition == null)
                                continue;
                            
                            condition.textDesc = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_cd_{conditionKey}_text");
                        }
                    }
                    
                    if (step.countdownCustom != null)
                    {
                        var cc = step.countdownCustom;
                        
                        if (cc.textHintMain != null)
                            cc.textHintMain.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_main");
                        
                        if (cc.textHintShortToEnd != null)
                            cc.textHintShortToEnd.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_short_to_end");
                        
                        if (cc.textHintShort != null)
                            cc.textHintShort.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_short");
                        
                        if (cc.textHintDelay != null)
                            cc.textHintDelay.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_delay");
                        
                        if (cc.textHintDelayToMax != null)
                            cc.textHintDelayToMax.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_delay_to_max");
                        
                        if (cc.textTooltipMain != null)
                            cc.textTooltipMain.s = DataManagerText.GetText (TextLibs.overworldQuests, $"{keyPrefix}_tc_tooltip_main");
                    }
                }
            }
        }
        
        private IEnumerable<string> GetStepKeys => steps != null ? steps.Keys : null;
        private IEnumerable<string> GetEffectKeys => effectsShared != null ? effectsShared.Keys : null;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldQuest () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;
            
            if (textName != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{key}_a_header",  textName.s);
                    
            if (textDesc != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{key}_a_text",  textDesc.s);

            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        continue;

                    var keyPrefix = $"{key}__s_{stepKey}";
                    
                    if (step.textName != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_a_header",  step.textName.s);
                    
                    if (step.textDesc != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_a_text",  step.textDesc.s);
                    
                    if (step.textExit != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_b_exit",  step.textExit.s);
                    
                    // if (step.textCountdown != null)
                    //     DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_c_countdown",  step.textCountdown.s);

                    if (step.conditions != null)
                    {
                        foreach (var kvp2 in step.conditions)
                        {
                            var conditionKey = kvp2.Key;
                            var condition = kvp2.Value;

                            if (condition == null)
                                continue;

                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_cd_{conditionKey}_text",  condition.textDesc);
                        }
                    }

                    if (step.countdownCustom != null)
                    {
                        var cc = step.countdownCustom;
                        
                        if (cc.textHintMain != null)
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_main", cc.textHintMain.s);
                        
                        if (cc.textHintShortToEnd != null)
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_short_to_end", cc.textHintShortToEnd.s);
                        
                        if (cc.textHintShort != null)
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_short", cc.textHintShort.s);
                        
                        if (cc.textHintDelay != null)
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_delay", cc.textHintDelay.s);
                        
                        if (cc.textHintDelayToMax != null)
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_tc_hint_delay_to_max", cc.textHintDelayToMax.s);
                        
                        if (cc.textTooltipMain != null)
                            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldQuests, $"{keyPrefix}_tc_tooltip_main", cc.textTooltipMain.s);
                    }
                }
            }
        }
        
        private bool IsInheritanceVisible => DataMultiLinkerOverworldQuest.Presentation.showInheritance;
        private bool IsDisplayIsolated => DataMultiLinkerOverworldQuest.Presentation.showIsolatedEntries;
        
        #endif
        #endregion
    }
}

