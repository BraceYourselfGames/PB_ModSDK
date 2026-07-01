using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Input.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockScenarioHintUnitLink
    {
        // Narrow down which unit gets evaluated
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataShortcuts.sim.scenarioUnitTags")]
        public SortedDictionary<string, bool> tags;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckConstraintUnit name;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckConstraintFaction faction;
        
        [DropdownReference (true)]
        public DataBlockScenarioSubcheckUnitState state;
        
        [DropdownReference (true)] 
        public DataBlockOverworldEventSubcheckBool selection;

        [DropdownReference] 
        public DataBlockOverworldEventSubcheckInt actionTotalCount;

        [BoxGroup ("actionTypeCountsGroup", false)]
        [ShowIf ("@actionCounts != null && actionCounts.Count > 1")]
        [LabelText ("Every Check Required")]
        public bool actionCountsRequireAll = true;

        [DropdownReference]
        [BoxGroup ("actionTypeCountsGroup")]
        [DictionaryKeyDropdown ("@DataMultiLinkerAction.data.Keys")]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckInt> actionCounts;
        
        [DropdownReference]
        public List<ICombatUnitValidationFunction> checks;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioHintUnitLink () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [LabelWidth (180f)]
    public class DataBlockScenarioHintConditional
    {
        public bool hidden = false;
        
        [OnValueChanged ("OnDataChange", true)]
        [HideLabel, BoxGroup ("UI", false)]
        public DataBlockTutorialHint data = new DataBlockTutorialHint ();
        
        [BoxGroup (boxGroupCore, false)]
        public CombatUIModes inputMode;

        [BoxGroup (boxGroupCore, false)]
        public InputHintMode inputController = InputHintMode.All;
        
        [DropdownReference (true)]
        public DataBlockScenarioHintUnitLink unitLink;
        
        [DropdownReference (true)] 
        public DataBlockOverworldEventSubcheckBool actionContextMenu;
        
        [DropdownReference (true)] 
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public string actionSelection;
        
        [DropdownReference (true)] 
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public List<string> actionsBlocked;
        
        [DropdownReference (true)] 
        [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
        public List<string> actionsUnblocked;

        private const string boxGroupCore = "boxGroupCore";
        private const string boxGroupMenu = "boxGroupMenu";
        private const string boxGroupUnit = "boxGroupUnit";
        private const string boxGroupAction = "boxGroupAction";
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioHintConditional () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #if !PB_MODSDK
        private void OnDataChange ()
        {
            if (Application.isPlaying && IDUtility.IsGameState (GameStates.combat))
                ScenarioUtility.RecheckConditionalHints (true);
        }
        #endif

        #endif
        #endregion
    }
    
    public class DataBlockViewTutorialEffects
    {
        public InputHintModeSimplified controller = InputHintModeSimplified.All;
        public CINavDir inputDirection = CINavDir.Forward;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsOverworld;
        
        [DropdownReference]
        public List<ICombatFunction> functionsCombat;
        
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockViewTutorialEffects () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }

    [Serializable][LabelWidth (180f)]
    public class DataBlockViewTutorialEffectsOverworld
    {
        public List<IOverworldFunction> functions = new List<IOverworldFunction> ();
    }

    [Serializable][LabelWidth (180f)]
    public class DataBlockViewTutorialEffectsCombat
    {
        public List<ICombatFunction> functions = new List<ICombatFunction> ();
    }

    [Serializable][LabelWidth (180f)]
    public class DataBlockViewTutorialPage
    {
        [OnValueChanged (onChange)]
        [LabelText ("Foreground Offset")]
        public int depthForeground;
        
        [OnValueChanged (onChange)]
        [LabelText ("Background Offset")]
        public int depthBackground;
        
        [OnValueChanged (onChange)]
        [LabelText ("Skippable")]
        public bool skippable = true;
        
        [OnValueChanged (onChange)]
        [LabelText ("Camera Input")]
        public bool cameraInputPermitted = true;
        
        [OnValueChanged (onChange)]
        [LabelText ("Nav. Focus")]
        public bool navigationFocus = true;

        [OnValueChanged (onChange)]
        [ToggleGroup ("backgroundUsed", groupTitle: "Background", CollapseOthersOnExpand = false)]
        public bool backgroundUsed;
        
        [OnValueChanged (onChange)]
        [ToggleGroup ("backgroundUsed")]
        public bool backgroundBlur;
        
        [OnValueChanged (onChange)]
        [ToggleGroup ("backgroundUsed")]
        public Color backgroundColor;

        [DropdownReference, BoxGroup ("Center", false)]
        public DataBlockTutorialCenter center;
        
        [DropdownReference, BoxGroup ("Hint", false)]
        [OnValueChanged (onChange, true)]
        public DataBlockTutorialHint hint;

        [DropdownReference, BoxGroup ("Combat effects", false)]
        public DataBlockViewTutorialEffectsOverworld effectsOverworld;
        
        [DropdownReference, BoxGroup ("Combat effects", false)]
        public DataBlockViewTutorialEffectsCombat effectsCombat;

        [YamlIgnore, HideInInspector]
        public string id;
        
        [YamlIgnore, ShowInInspector, ReadOnly, PropertyOrder (-1)]
        public int index;

        [YamlIgnore, HideInInspector, NonSerialized]
        public DataContainerTutorial parent;
        
        private const string onChange = "OnChange";
        
        public void OnChange ()
        {
            #if !PB_MODSDK
            if (!Application.isPlaying || CIViewTutorial.ins == null || !CIViewTutorial.ins.IsEntered ())
                return;

            if (CIViewTutorial.ins.pageIDLast != id)
                return;

            CIViewTutorial.ins.Refresh (this, true);
            #endif
        }
        
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockViewTutorialPage () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }

    public class DataBlockTutorialHintWorldAnchor
    {
        public string entityName;
        
        public string stateName;
        
        [ShowIf ("@string.IsNullOrEmpty (entityName)")]
        public Vector3 position;

        public bool entityPrediction;
    }
    
    public class DataBlockTutorialHintArrowCustom
    {
        public Vector2 offset;
        public float rotation;
    }
    
    public class DataBlockTutorialHintHighlight
    {
        public Vector4 offsets;
    }

    [Serializable] [LabelWidth (180f)]
    public class DataBlockTutorialCenter
    {
        [OnValueChanged ("OnChange")]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEvents)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEvents, 128)", false)]
        [InlineButtonClear]
        public string textImage = null;

        [DropdownReference (true)]
        public DataBlockFloat01 hue;
        
        // [DropdownReference (true)]
        // public DataBlockLocString textHeaderReused;

        // [DropdownReference (true)]
        // public DataBlockLocString textContentReused;

        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockTextTrimodal textHeaderLink = new DataBlockTextTrimodal ();
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockTextTrimodal textContentLink = new DataBlockTextTrimodal ();
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockTextTrimodal textContentLinkController;
        
        [DropdownReference (true)]
        public DataBlockTextInputActions textInputActions;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerInputHint.data.Keys")]
        [LabelText ("Input Hint Appendix")]
        public string textInputHintAppendix;
        
        [YamlIgnore, HideInInspector]
        public DataBlockViewTutorialPage parent;
        
        #if UNITY_EDITOR

        private void OnChange ()
        {
            if (parent != null)
                parent.OnChange ();
        }

        [ShowInInspector, PropertyOrder (2)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockTutorialCenter () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
    }

    [Serializable]
    public class DataBlockTextInputActions
    {
        public bool spriteWrap = true;

        [ValueDropdown ("@DataMultiLinkerInputAction.data.Keys")]
        [ListDrawerSettings (ShowIndexLabels = true), InlineButton ("@actions.Add (string.Empty)", "Add")]
        public List<string> actions = new List<string> ();
    }

    [Serializable][LabelWidth (180f)]
    public class DataBlockTutorialHint
    {
        [OnValueChanged (onChange)]
        [HideLabel, ColorUsage (showAlpha: true)]
        public Color color = Color.cyan.WithAlpha (1f);

        [OnValueChanged (onChange)]
        public CIViewTutorialHint.FrameLocation frameLocation = CIViewTutorialHint.FrameLocation.TopLeft;
        
        [OnValueChanged (onChange)]
        public CIViewTutorialHint.FrameGradientMode frameGradientMode = CIViewTutorialHint.FrameGradientMode.None;
        
        [OnValueChanged (onChange)]
        [HorizontalGroup ("Position")]
        [LabelText ("Frame Position")]
        public int framePositionX = 0;
        
        [OnValueChanged (onChange)]
        [HorizontalGroup ("Position")]
        [HideLabel]
        public int framePositionY = 0;
        
        [OnValueChanged (onChange)]
        [HorizontalGroup ("Size")]
        [LabelText ("Frame Size")]
        public int frameSizeX = 512;
        
        [OnValueChanged (onChange)]
        [HorizontalGroup ("Size")]
        [HideLabel]
        public int frameSizeY = 256;

        [OnValueChanged (onChange)]
        public bool frameBlocksInput = true;

        [OnValueChanged (onChange)]
        public bool frameBoundary = true;

        [OnValueChanged (onChange)]
        public CIViewTutorialHint.TextLocation textLocation = CIViewTutorialHint.TextLocation.BottomLeft;
        
        [OnValueChanged (onChange)]
        public CIViewTutorialHint.ButtonLocation buttonLocation = CIViewTutorialHint.ButtonLocation.None;
        
        [OnValueChanged (onChange)]
        public int textWidth = 256;

        [PropertyOrder (1)]
        [DropdownReference (true)]
        [OnValueChanged (onChange, true)]
        public DataBlockTutorialHintWorldAnchor worldAnchor;
        
        [PropertyOrder (1)]
        [DropdownReference (true)]
        [OnValueChanged (onChange, true)]
        public DataBlockTutorialHintArrowCustom arrowCustom;
        
        [PropertyOrder (1)]
        [DropdownReference (true)]
        [OnValueChanged (onChange, true)]
        public DataBlockTutorialHintHighlight frameHighlight;
        
        // [PropertyOrder (1)]
        // [DropdownReference (true)]
        // public DataBlockLocString textReused;
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockTextTrimodal textLink;
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockTextTrimodal textLinkController;
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockTextInputActions textInputActions;
        
        [DropdownReference (true)]
        [OnValueChanged ("OnChange", true)]
        [ValueDropdown ("@DataMultiLinkerInputHint.data.Keys")]
        [LabelText ("Input Hint Appendix")]
        public string textInputHintAppendix;
        
        [DropdownReference, BoxGroup ("Combat effects", false)]
        public List<DataBlockViewTutorialEffects> effectsOnInput;

        [YamlIgnore, HideInInspector]
        public string id;
        
        [YamlIgnore, HideInInspector]
        public DataBlockViewTutorialPage parent;

        private const string onChange = "OnChange";
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (2)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockTutorialHint () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private void OnChange ()
        {
            if (parent != null)
                parent.OnChange ();
        }
        
        #endif
    }

    public class DataBlockTutorialEffectsOnEnd
    {
        [DropdownReference, BoxGroup ("Combat effects", false)]
        public DataBlockViewTutorialEffectsOverworld effectsOverworld;
        
        [DropdownReference, BoxGroup ("Combat effects", false)]
        public DataBlockViewTutorialEffectsCombat effectsCombat;
        
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockTutorialEffectsOnEnd () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
    }

    public static class TutorialKeys
    {
        public const string OverworldIntro = "overworld_intro";
        public const string OverworldSalvage = "overworld_salvage";
        public const string OverworldPilotProgression = "overworld_pilot_progression";
        public const string OverworldPilotRecruit = "overworld_pilot_recruit";
        
        public const string OverworldResupply = "overworld_resupply";
        public const string OverworldContest = "overworld_contest";
        public const string OverworldOpen = "overworld_open";
        public const string OverworldBoss = "overworld_boss";
        public const string OverworldGeneralOverview = "overworld_general_overview";
        public const string OverworldCamp = "overworld_camp";
        public const string OverworldCapitalCamp = "overworld_capital_camp";
        
        public const string BaseIntroduction = "base_introduction";
        public const string BaseBriefing = "base_briefing";
        public const string BaseBriefingDemolition = "base_briefing_demolition";
        public const string BaseWorkshop = "base_workshop";
        
        public const string CombatIntro1 = "combat_intro_01";
        public const string CombatIntro2 = "combat_intro_02";
        public const string CombatIntro3 = "combat_intro_03";
        public const string CombatIntro4 = "combat_intro_04";
        
        public const string CombatBarrier = "combat_barrier";
    }

    [Serializable][LabelWidth (180f)]
    public class DataContainerTutorial : DataContainerWithText
    {
        public int depthBackground;
        public int depthForeground;
        
        [OnValueChanged ("RefreshParentsInPages")]
        public List<DataBlockViewTutorialPage> pages;

        public DataBlockTutorialEffectsOnEnd effectsOnEnd;

        public override void OnAfterDeserialization (string key)
        {
            // Set key manually so that it is in place for RefreshParentsInPages
            this.key = key;
            
            // Run this method first, before base implementation invokes ResolveText, which depends on IDs set here
            RefreshParentsInPages ();
            
            // Now it's safe to run this
            base.OnAfterDeserialization (key);
        }

        private void RefreshParentsInPages ()
        {
            if (pages != null)
            {
                for (int i = 0, count = pages.Count; i < count; ++i)
                {
                    var page = pages[i];
                    if (page == null)
                        continue;

                    page.index = i;
                    page.parent = this;
                    page.id = $"{key}__p{i:D2}";
                    
                    if (page.center != null)
                        page.center.parent = page;

                    if (page.hint != null)
                    {
                        page.hint.parent = page;
                        page.hint.id = page.id;
                    }
                }
            }
        }

        public override void ResolveText ()
        {
            // Ensure double underscore separator follows the key
            // textName = DataManagerText.GetText (TextLibs.uiTutorials, $"{key}__0c_name");

            if (pages != null)
            {
                for (int i = 0; i < pages.Count; ++i)
                {
                    var page = pages[i];
                    if (page == null)
                        continue;

                    if (page.center != null)
                    {
                        /*
                        if (page.center.textHeaderLink == null)
                        {
                            page.center.textHeaderLink = new DataBlockTextTrimodal { mode = DataBlockTextTrimodal.Mode.LocUnique };
                            Debug.Log ($"Created tutorial text link: center header {page.id}");
                        }
                        
                        if (page.center.textHeaderReused != null)
                        {
                            page.center.textHeaderLink = new DataBlockTextTrimodal
                            {
                                mode = DataBlockTextTrimodal.Mode.LocReused,
                                textSector = page.center.textHeaderReused.textSector,
                                textKey = page.center.textHeaderReused.textKey
                            };
                            
                            Debug.Log ($"Created reused tutorial text link: center header {page.id}");
                        }
                        
                        if (page.center.textContentLink == null)
                        {
                            page.center.textContentLink = new DataBlockTextTrimodal { mode = DataBlockTextTrimodal.Mode.LocUnique };
                            Debug.Log ($"Created tutorial text link: center content {page.id}");
                        }
                        
                        if (page.center.textContentReused != null)
                        {
                            page.center.textContentLink = new DataBlockTextTrimodal
                            {
                                mode = DataBlockTextTrimodal.Mode.LocReused,
                                textSector = page.center.textContentReused.textSector,
                                textKey = page.center.textContentReused.textKey
                            };
                            
                            Debug.Log ($"Created reused tutorial text link: center content {page.id}");
                        }
                        */
                        
                        if (page.center.textHeaderLink != null)
                            page.center.textHeaderLink.ResolveText (TextLibs.uiTutorials, $"{page.id}_header");

                        if (page.center.textContentLink != null)
                            page.center.textContentLink.ResolveText (TextLibs.uiTutorials, $"{page.id}_text");
                        
                        if (page.center.textContentLinkController != null)
                            page.center.textContentLinkController.ResolveText (TextLibs.uiTutorials, $"{page.id}_text_ctrl");
                    }

                    if (page.hint != null)
                    {
                        /*
                        if (page.hint.textLink == null)
                        {
                            page.hint.textLink = new DataBlockTextTrimodal { mode = DataBlockTextTrimodal.Mode.LocUnique };
                            Debug.Log ($"Created tutorial text link: hint {page.id}");
                        }
                        
                        if (page.hint.textReused != null)
                        {
                            page.hint.textLink = new DataBlockTextTrimodal
                            {
                                mode = DataBlockTextTrimodal.Mode.LocReused,
                                textSector = page.hint.textReused.textSector,
                                textKey = page.hint.textReused.textKey
                            };
                            
                            Debug.Log ($"Created reused tutorial text link: hint {page.id}");
                        }
                        */
                        
                        if (page.hint.textLink != null)
                            page.hint.textLink.ResolveText (TextLibs.uiTutorials, $"{page.id}_hint");
                        
                        if (page.hint.textLinkController != null)
                            page.hint.textLinkController.ResolveText (TextLibs.uiTutorials, $"{page.id}_hint_ctrl");
                    }
                }
            }
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            // Ensure double underscore separator follows the key
            // DataManagerText.TryAddingTextToLibrary (TextLibs.uiTutorials, $"{key}__0c_name", textName);
            
            if (pages != null)
            {
                for (int i = 0; i < pages.Count; ++i)
                {
                    var page = pages[i];
                    if (page == null)
                        continue;

                    if (page.center != null)
                    {
                        if (page.center.textHeaderLink != null)
                            page.center.textHeaderLink.SaveText (TextLibs.uiTutorials, $"{page.id}_header");
                        
                        if (page.center.textContentLink != null)
                            page.center.textContentLink.SaveText (TextLibs.uiTutorials, $"{page.id}_text");
                        
                        if (page.center.textContentLinkController != null)
                            page.center.textContentLinkController.SaveText (TextLibs.uiTutorials, $"{page.id}_text_ctrl");
                    }
                    
                    if (page.hint != null)
                    {
                        if (page.hint.textLink != null)
                            page.hint.textLink.SaveText (TextLibs.uiTutorials, $"{page.id}_hint");
                        
                        if (page.hint.textLinkController != null)
                            page.hint.textLinkController.SaveText (TextLibs.uiTutorials, $"{page.id}_hint_ctrl");
                    }
                }
            }
        }

        [HideInEditorMode]
        [Button (ButtonSizes.Medium), PropertyOrder (-1)]
        private void Test ()
        {
            #if !PB_MODSDK
            if (CIViewTutorial.ins != null)
                CIViewTutorial.ins.OnTutorialStartFromKey (key, true);
            #endif
        }
        
        #endif
    }
}

