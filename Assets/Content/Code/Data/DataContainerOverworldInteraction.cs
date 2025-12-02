using System;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using PhantomBrigade.Functions;
using PhantomBrigade.Game;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockOverworldInteractionParent : DataContainerParent
    {
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerOverworldInteraction.data.Keys;
    }

    public class DataBlockOverworldInteractionVideo
    {
        public bool loop = false;
        public float contentDelay = 2f;
        
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("UpdatePath")]
        [InlineButton ("UpdatePath", "Update path")]
        public VideoClip clip;
    
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;

        public void OnBeforeSerialization (string context)
        {
            #if UNITY_EDITOR && !PB_MODSDK
            UpdatePath ();
            #endif
        }

        public void OnAfterDeserialization (string context)
        {
            #if !PB_MODSDK
            clip = !string.IsNullOrEmpty (path) ? Resources.Load<VideoClip> (path) : null;
            if (clip == null)
            {
                Debug.LogWarning ($"Failed to load video clip from path [{path}] in interaction background {context}");
                return;
            }
            #endif
        }  
        
        #if UNITY_EDITOR

        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);
        
        public void UpdatePath ()
        {
            if (clip == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (clip);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring (0, fullPath.Length - extension.Length);
            path = fullPath;
        }
        
        #endif
    }
    
    public class DataBlockOverworldInteractionBackground
    {
        public bool immersive;
        public bool focus;
        
        [PropertyRange (0f, 1f), HorizontalGroup]
        [LabelText ("Brightness / Color")]
        public float brightness = 1f;
        
        [HideLabel, HorizontalGroup (0.25f)]
        public Color color = Color.white.WithAlpha (0f);
        
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEvents)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEvents, 256)", false)]
        public string image;

        [DropdownReference (true)]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEvents)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEvents, 256)", false)]
        public string imageSecondary;
        
        [DropdownReference (true)]
        public DataBlockOverworldInteractionVideo video;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldInteractionBackground () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockOverworldInteractionStepProperties
    {
        public bool displayResources = false;
        public DataBlockInt optionLimitRandom = null;
    }
    
    public class DataBlockOverworldInteractionStep
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;
        
        [DropdownReference, HideLabel, HideReferenceObjectPicker] 
        public DataBlockComment comment;
        
        [DropdownReference (true)]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEvents)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEvents, 256)", false)]
        public string image;
        
        [DropdownReference (true)]
        public DataBlockOverworldInteractionBackground background;
        
        [DropdownReference (true)]
        public DataBlockOverworldInteractionMusicMood mood;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textHeader;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong text;
        
        [DropdownReference (true)]
        public DataBlockLocString textReused;
        
        [DropdownReference (true)]
        public DataBlockOverworldInteractionStepProperties properties;
        
        [DropdownReference]
        public List<DataBlockOverworldInteractionEffect> effects;
        
        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockOverworldInteractionOption> options;
        
        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public List<DataBlockOverworldInteractionOptionLink> optionsReused;
        
        [YamlIgnore, HideInInspector, NonSerialized] 
        public DataContainerOverworldInteraction parent;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldInteractionStep () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private IEnumerable<string> GetOptionKeys => options?.Keys;
        
        #endif
        #endregion
    }

    [Flags]
    public enum InteractionOptionEffectSimple
    {
        None = 0,
        Exit = 1,
        DestroyTarget = 2
    }

    public class DataBlockOverworldInteractionOptionCore
    {
        [ToggleLeft]
        public bool hidden = false;

        [ToggleLeft]
        public bool hiddenIfUnavailable = false;
        
        [ToggleLeft]
        public bool textEffectsCombined = false;
        
        [PropertyOrder (-1), HorizontalGroup ("A"), LabelText ("Priority / Tint")]
        public int priority = 0;
        
        [HideLabel, PropertyOrder (-1), HorizontalGroup ("A", 80f)]
        public Color color = Color.white;
        
        [PropertyOrder (-2)]
        public InteractionOptionEffectSimple effect = InteractionOptionEffectSimple.None;
    }
    
    public class DataBlockOverworldInteractionMusicMood
    {
        public EventMusicMoods value = EventMusicMoods.None;
        
        #if UNITY_EDITOR && !PB_MODSDK

        [HideInEditorMode, Button ("Choice sync ►"), ButtonGroup]
        private void TestChoiceSync ()
        {
            var audioEvent = AudioUtility.CreateAudioEvent (AudioEventOverworldNarrativeMusic.MusicEventChoice);
            audioEvent.AddAudioSyncUpdate (AudioEventOverworldNarrativeMusic.MusicEventChoice, (int)value);
        }

        [HideInEditorMode, Button ("Outcome sync ►"), ButtonGroup]
        private void TestOutcomeSync ()
        {
            AudioUtility.CreateAudioSyncUpdate (AudioEventOverworldNarrativeMusic.MusicEventOutcome, (int)value);
        }
        
        [HideInEditorMode, Button ("Full sync ►"), ButtonGroup]
        private void TestFullSync ()
        {
            var audioEvent = AudioUtility.CreateAudioEvent (AudioEventOverworldNarrativeMusic.MusicEventChoice);
            audioEvent.AddAudioSyncUpdate (AudioEventOverworldNarrativeMusic.MusicEventChoice, (int)value);
            AudioUtility.CreateAudioSyncUpdate (AudioEventOverworldNarrativeMusic.MusicEventOutcome, (int)value);
        }
        
        #endif
    }

    public class DataBlockOverworldInteractionEffect
    {
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsTarget;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IInteractionOptionFunction> functionsLocal;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldInteractionEffect () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockOverworldInteractionOptionLink
    {
        [ValueDropdown ("$GetInteractionKeys")]
        [InlineButtonClear]
        [LabelText ("Interaction")]
        public string keyInteraction;
        
        [ValueDropdown ("$GetStepKeys")]
        [LabelText ("Step")]
        public string keyStep = "main";
        
        [LabelText ("Option")]
        [ValueDropdown ("$GetOptionKeys")]
        public string keyOption = "leave";

        [HideInInspector]
        public DataBlockOverworldInteractionOption GetOptionData ()
        {
            if (string.IsNullOrEmpty (keyStep) || string.IsNullOrEmpty (keyOption))
                return null;
                
            bool contained = string.IsNullOrEmpty (keyInteraction);
            var interactionData = contained ? parent : DataMultiLinkerOverworldInteraction.GetEntry (keyInteraction, false);
            if (interactionData == null)
                return null;
                
            var steps = interactionData.stepsProc;
            if (steps == null || !steps.TryGetValue (keyStep, out var stepData))
                return null;

            var options = stepData?.options;
            if (options == null || !options.TryGetValue (keyOption, out var optionData))
                return null;

            return optionData;
        }
        
        [YamlIgnore, HideInInspector, NonSerialized]
        public DataContainerOverworldInteraction parent;
        
        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetInteractionKeys => DataMultiLinkerOverworldInteraction.data.Keys;

        private IEnumerable<string> GetStepKeys ()
        {
            bool contained = string.IsNullOrEmpty (keyInteraction);
            var interactionData = contained ? parent : DataMultiLinkerOverworldInteraction.GetEntry (keyInteraction, false);
            var steps = interactionData?.stepsProc;
            return steps != null ? steps.Keys : null;
        }
        
        private IEnumerable<string> GetOptionKeys ()
        {
            if (string.IsNullOrEmpty (keyStep))
                return null;
            
            bool contained = string.IsNullOrEmpty (keyInteraction);
            var interactionData = contained ? parent : DataMultiLinkerOverworldInteraction.GetEntry (keyInteraction, false);
            var steps = interactionData?.stepsProc;
            if (steps == null || !steps.TryGetValue (keyStep, out var stepData) || stepData?.options == null)
                return null;

            var s = stepData.options;
            return stepData.options.Keys;
        }

        #endif
        #endregion
    }

    public class DataBlockInteractionOptionReused
    {
        [ValueDropdown ("$GetInteractionKeys")]
        public string interaction;
        
        [ValueDropdown ("$GetStepKeys")]
        public string step;
        
        [ValueDropdown ("$GetOptionKeys")]
        public string option;
        
        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetInteractionKeys => DataMultiLinkerOverworldInteraction.data.Keys;

        private IEnumerable<string> GetStepKeys ()
        {
            var interactionData = DataMultiLinkerOverworldInteraction.GetEntry (interaction, false);
            if (interactionData == null || interactionData.steps == null)
                return null;

            return interactionData.steps.Keys;
        }
        
        private IEnumerable<string> GetOptionKeys ()
        {
            var interactionData = DataMultiLinkerOverworldInteraction.GetEntry (interaction, false);
            if (interactionData == null || interactionData.steps == null || string.IsNullOrEmpty (step))
                return null;

            if (!interactionData.steps.TryGetValue (step, out var stepData))
                return null;

            if (stepData == null || stepData.options == null)
                return null;

            return stepData.options.Keys;
        }

        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockStringInteractionEffect : DataBlockStringNonSerializedLong
    {
        [PropertyOrder (-1)]
        [HideLabel, ColorUsage (true, false)]
        public Color color = Color.white;
    }

    public class DataBlockOverworldInteractionOption
    {
        [YamlIgnore, HideInInspector, NonSerialized]
        public string key;

        #if UNITY_EDITOR
        [PropertyOrder (-1)]
        [ShowInInspector, ReadOnly, YamlIgnore, HideLabel, MultiLineProperty]
        private string preview => GetTextPreview ();
        #endif
        
        [GUIColor (0.9f, 1f, 0.8f)]
        [DropdownReference (true)]
        public DataBlockOverworldInteractionOptionCore core;
        
        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockInteractionOptionReused dataReused;

        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockStringNonSerialized textMain;
        
        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockLocString textMainReused;
        
        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockStringNonSerializedLong textContent;
        
        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockLocString textContentReused;
        
        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockStringInteractionEffect textEffect;
        
        [DropdownReference (true), OnValueChanged ("OnTextRelatedChange", true)]
        public DataBlockLocString textEffectReused;
        
        // [DropdownReference (true)]
        // public DataBlockOverworldInteractionMusicMood mood;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventCheck check;
        
        [DropdownReference]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataBlockOverworldInteractionStep\", \"GetOptionKeys\")")]
        public SortedDictionary<string, bool> checkOtherOptions;
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksTarget;
        
        [DropdownReference, OnValueChanged ("OnTextRelatedChange", true)]
        public List<DataBlockOverworldInteractionEffect> effects;
        
        [YamlIgnore, HideInInspector, NonSerialized]
        public DataContainerOverworldInteraction parent;

        public bool AreEffectsPresent ()
        {
            return effects != null && effects.Count > 0;
        }
        
        #region Editor
        #if UNITY_EDITOR

        [HideDuplicateReferenceBox, HideReferenceObjectPicker, BoxGroup ("S", false), GUIColor (0.8f, 0.8f, 0.8f, 1f)]
        [ShowInInspector, ShowIf ("GetStepLinkShortcut")]
        private InteractionOptionEnterStep stepLink
        {
            get
            {
                return GetStepLinkShortcut ();
            }
            set
            {
                
            }
        }

        private InteractionOptionEnterStep GetStepLinkShortcut ()
        {
            if (effects == null)
                return null;

            foreach (var effect in effects)
            {
                if (effect == null || effect.functionsLocal == null)
                    continue;

                foreach (var f in effect.functionsLocal)
                {
                    if (f is InteractionOptionEnterStep fs)
                        return fs;
                }
            }
                
            return null;
        }
        
        private IEnumerable<string> GetStepKeys () => parent?.steps != null ? parent.steps.Keys : null;

        [Button, HideIf ("GetStepLinkShortcut")]
        private void AddStepLink ([ValueDropdown ("GetStepKeys")] string stepKey)
        {
            if (effects == null)
                effects = new List<DataBlockOverworldInteractionEffect> ();

            DataBlockOverworldInteractionEffect effect = null;
            foreach (var effectCandidate in effects)
            {
                if (effectCandidate != null)
                {
                    effect = effectCandidate;
                    break;
                }
            }

            if (effect == null)
            {
                effect = new DataBlockOverworldInteractionEffect ();
                effects.Add (effect);
            }

            if (effect.functionsLocal == null)
                effect.functionsLocal = new List<IInteractionOptionFunction> ();
            
            effect.functionsLocal.Add (new InteractionOptionEnterStep
            {
                step = stepKey
            });
        }

        private void OnTextRelatedChange ()
        {
            previewNeedsChange = true;
            previewTextLast = null;
        }

        private static List<string> textFromFunctionsList = new List<string> ();
        private static StringBuilder sb = new StringBuilder ();

        private string GetTextPreview ()
        {
            if (!previewNeedsChange && !string.IsNullOrEmpty (previewTextLast))
                return previewTextLast;

            previewNeedsChange = false;
            sb.Clear ();

            string textPreviewHeader = null;
            if (!string.IsNullOrEmpty (textMain?.s))
                textPreviewHeader = textMain.s;
            else if (textMainReused != null)
                textPreviewHeader = textMainReused.text;

            if (!string.IsNullOrEmpty (textPreviewHeader))
                sb.Append (textPreviewHeader.ToUpperInvariant ());
            else
                sb.Append ("???");
            
            string textPreviewContent = null;
            if (!string.IsNullOrEmpty (textContent?.s))
                textPreviewContent = textContent.s;
            else if (textContentReused != null)
                textPreviewContent = textContentReused.text;
            
            if (!string.IsNullOrEmpty (textPreviewContent))
            {
                sb.Append ("\n");
                sb.Append (textPreviewContent);
            }
            
            if (effects != null && effects.Count > 0)
            {
                string textPreviewEffect = null;
                if (!string.IsNullOrEmpty (textEffect?.s))
                    textPreviewEffect = textEffect.s;
                else if (textEffectReused != null)
                    textPreviewEffect = textEffectReused.text;

                bool appendGeneratedText = string.IsNullOrEmpty (textPreviewEffect) || (core != null && core.textEffectsCombined);
                if (appendGeneratedText)
                {
                    textFromFunctionsList.Clear ();
                    foreach (var effect in effects)
                    {
                        if (effect == null)
                            continue;

                        if (effect.functionsGlobal != null)
                        {
                            foreach (var function in effect.functionsGlobal)
                            {
                                if (function == null)
                                    continue;

                                if (function is CreateRewards f1 && f1.rewards != null && f1.rewards.Count > 0)
                                {
                                    var prefix = Txt.Get (TextLibs.uiOverworld, "int_sh_effect_prefix_salvage");
                                    DataContainerOverworldReward.AppendRewardGroupsCollapsedToList (f1.rewards, textFromFunctionsList, false, prefix);
                                }
                                else if (function is CreateRewardsFiltered f2 && f2.rewards != null && f2.rewards.Count > 0)
                                {
                                    f2.Generate ();
                                    var prefix = Txt.Get (TextLibs.uiOverworld, "int_sh_effect_prefix_salvage");
                                    DataContainerOverworldReward.AppendRewardGroupsCollapsedToList (f2.rewardsProcessed, textFromFunctionsList, false, prefix);
                                }
                                else if (function is IFunctionLocalizedText functionLoc)
                                {
                                    var textFunction = functionLoc.GetLocalizedText ();
                                    textFromFunctionsList.Add (textFunction);
                                }
                            }
                        }

                        if (textFromFunctionsList.Count > 0)
                        {
                            var textFormatted = textFromFunctionsList.ToStringFormatted (true, multilinePrefix: "   ");
                            if (!string.IsNullOrEmpty (textPreviewEffect))
                                textPreviewEffect = $"{textPreviewEffect}\n{textFormatted}";
                            else
                                textPreviewEffect = textFormatted;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty (textPreviewEffect))
                {
                    sb.Append ("\n   ");
                    sb.Append (textPreviewEffect);
                }
            }

            previewTextLast = sb.ToString ();
            return previewTextLast;
        }
        
        private string previewTextLast = null;
        private bool previewNeedsChange = false;
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldInteractionOption () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public enum OverworldInteractionRepeat
    {
        Enabled,
        UniquePerCampaign,
        UniquePerProvince
    }
    
    public class DataBlockOverworldInteractionCheck
    {
        public OverworldInteractionRepeat repeat = OverworldInteractionRepeat.Enabled;
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksTarget;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldInteractionCheck () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockOverworldInteractionMusicCombat
    {
        public enum CombatMusicType
        {
            LinearCustom,
            LinearStandard,
            Dynamic
        }
        
        public CombatMusicType type = CombatMusicType.LinearStandard;
        
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (AudioCombatMusicState), false)")]
        public string stateCustomTrack = string.Empty;

        [PropertyRange (0f, 1f)]
        public float executionState = 0f;
        
        [PropertyRange (0f, 1f)]
        public float intensity = 0f;

        private void Apply ()
        {
            #if !PB_MODSDK
            AudioUtility.CreateAudioEvent (AudioEventMusic.MusicOverworldStop); 
            
            if (type == CombatMusicType.LinearCustom && !string.IsNullOrEmpty (stateCustomTrack))
            {
                AudioUtility.CreateAudioEvent (AudioEventMusic.MusicCombatLinear);
                AudioUtility.CreateAudioStateEvent (AudioStateMusic.CombatMusicLinearState, stateCustomTrack);
            
                // Fade music to planning mode (value of 0), and start out at initial intensity
                AudioUtility.CreateAudioSyncUpdate (AudioSyncMusic.CombatMusicExecutionState, executionState);
                AudioUtility.CreateAudioSyncUpdate (AudioSyncMusic.CombatMusicLinearIntensity, intensity);
            }
            else if (type == CombatMusicType.Dynamic)
            {
                // There is only one dynamic track for now so we'll trigger than
                AudioUtility.CreateAudioEvent (AudioEventMusic.Dynamic01);
                
                // Set music into neutral uncertain mode
                var mood = AudioStateMusic.CombatDynamicPrefixUncertainty;

                AudioUtility.CreateAudioStateEvent (AudioStateGroupMusic.CombatDynamic, $"{mood}{AudioStateMusic.CombatDynamicStart}");
                AudioUtility.CreateAudioStateEvent (AudioStateGroupMusic.CombatDynamic, $"{mood}{AudioStateMusic.CombatDynamicPlanning}");
                
                AudioUtility.CreateAudioSyncUpdate (AudioSyncMusic.CombatMusicExecutionState, executionState);
                AudioUtility.CreateAudioSyncUpdate (AudioSyncs.MusicCombatCinematicIntensity, intensity);
            }
            else
            {
                int startingProgress = DataShortcuts.music.startingMusicProgress;
                var threshold = ReactiveMusicUtility.GetThresholdByProgress(startingProgress);
                var mood = threshold.IsValid ? threshold.mood : AudioStateMusic.CombatStateNeutral;

                AudioUtility.CreateAudioEvent (AudioEventMusic.MusicCombatLinear);
                AudioUtility.CreateAudioStateEvent (AudioStateMusic.CombatMusicLinearState, mood);
                
                AudioUtility.CreateAudioSyncUpdate (AudioSyncMusic.CombatMusicExecutionState, executionState);
                AudioUtility.CreateAudioSyncUpdate (AudioSyncMusic.CombatMusicLinearIntensity, intensity); 
            }
            #endif
        }
    }

    public class DataBlockOverworldInteractionCore
    {
        [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldInteraction\", \"GetStepKeys\")")]
        public string stepOnStart;
        public int priority = 0;

        [DropdownReference (true)]
        public DataBlockOverworldInteractionMusicMood moodOnStart;
        
        // [DropdownReference (true)]
        // public DataBlockOverworldInteractionMusicCombat combatMusicOnStart;
        
        [DropdownReference (true)]
        public string devSkipStep;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldInteractionCore () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [LabelWidth (160f)]
    public class DataContainerOverworldInteraction : DataContainerWithText, IDataContainerTagged
    {

        
        [ShowIf ("showCore")]
        [ToggleLeft]
        public bool hidden = false;

        [ShowIf ("showCore")]
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockOverworldInteractionParent ()")]
        public List<DataBlockOverworldInteractionParent> parents;
        
        [ShowIf ("showCore")]
        [YamlIgnore, ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children;

        
        [ShowIf ("showCore")]
        [DropdownReference (true)]
        public DataBlockOverworldInteractionCore core;
        
        [ShowIf ("@showCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldInteractionCore coreProc;
        
        
        [ShowIf ("showCore")]
        [DropdownReference]
        public List<DataBlockOverworldInteractionEffect> effectsOnExit;
        
        [ShowIf ("@showCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockOverworldInteractionEffect> effectsOnExitProc;

        
        [ShowIf ("showCore")]
        [DropdownReference]
        public HashSet<string> tags;
        
        [ShowIf ("@showCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public HashSet<string> tagsProc;
        
        
        [ShowIf ("showCore")]
        [DropdownReference (true)]
        public DataBlockOverworldInteractionCheck check;
        
        [ShowIf ("@showCore && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldInteractionCheck checkProc;
        
        
        [ShowIf ("@showSteps && IsDisplayIsolated")]
        [YamlIgnore, HideLabel, ShowInInspector]
        private DataViewIsolatedDictionary<DataBlockOverworldInteractionStep> stepIsolated; 
        
        [ShowIf ("@showSteps && !IsDisplayIsolated")]
        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockOverworldInteractionStep> steps;
        
        [ShowIf ("@showSteps && IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockOverworldInteractionStep> stepsProc;
        
        
        public bool IsHidden () => hidden;
        
        public HashSet<string> GetTags (bool processed) => 
            processed ? tagsProc : tags;

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);
            
            foreach (var kvp in DataMultiLinkerOverworldInteraction.data)
            {
                var entry = kvp.Value;
                if (entry.parents != null)
                {
                    for (int i = 0; i < entry.parents.Count; ++i)
                    {
                        var parent = entry.parents[i];
                        if (parent != null && parent.key == keyOld)
                        {
                            Debug.LogWarning ($"Interaction {kvp.Key}, parent block {i} | Replacing entity key: {keyOld} -> {keyNew})");
                            parent.key = keyNew;
                        }
                    }
                }
            }
            
            foreach (var kvp in DataMultiLinkerOverworldPointPreset.data)
            {
                var entry = kvp.Value;
                if (entry != null && entry.interaction != null)
                {
                    if (entry.interaction.keys != null)
                    {
                        if (entry.interaction.keys.Contains (keyOld))
                        {
                            entry.interaction.keys.Remove (keyOld);
                            entry.interaction.keys.Add (keyNew);
                            Debug.LogWarning ($"Point {kvp.Key} interaction key replaced: {keyOld} -> {keyNew})");
                        }
                    }
                }
            }
        }

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();

            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var step = kvp.Value;
                    if (step == null)
                        continue;

                    if (step.background != null)
                    {
                        if (step.background.video != null)
                            step.background.video.OnBeforeSerialization (key);
                    }
                }
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            #if UNITY_EDITOR
            
            stepIsolated = new DataViewIsolatedDictionary<DataBlockOverworldInteractionStep> 
            (
                "Steps", 
                () => steps, 
                () => GetStepKeys
            );
            
            #endif
            
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        continue;

                    step.key = stepKey;

                    if (step.background != null)
                    {
                        if (step.background.video != null)
                            step.background.video.OnAfterDeserialization (key);
                    }

                    if (step.optionsReused != null)
                    {
                        foreach (var option in step.optionsReused)
                        {
                            if (option != null)
                                option.parent = this;
                        }
                    }
                    
                    if (step.options != null)
                    {
                        foreach (var kvp2 in step.options)
                        {
                            var optionKey = kvp2.Key;
                            var option = kvp2.Value;

                            if (option == null)
                                continue;

                            option.key = optionKey;
                            option.parent = this;
                            
                            if (option.check != null)
                                Debug.LogWarning ($"Interaction {key} option {optionKey} has a legacy check");
                        }
                    }
                }
            }
        }

        public override void ResolveText ()
        {
            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        continue;

                    var keyPrefix = $"{key}__s_{stepKey}";
                    
                    if (step.textHeader != null)
                        step.textHeader.s = DataManagerText.GetText (TextLibs.overworldInteraction, $"{keyPrefix}_header");
                    
                    if (step.text != null)
                        step.text.s = DataManagerText.GetText (TextLibs.overworldInteraction, $"{keyPrefix}_main");

                    if (step.options != null)
                    {
                        foreach (var kvp2 in step.options)
                        {
                            var optionKey = kvp2.Key;
                            var option = kvp2.Value;

                            if (option == null)
                                continue;
                            
                            // option.textHeader = DataManagerText.GetText (TextLibs.overworldInteraction, $"{keyPrefix}_op_{optionKey}_header");
                            // option.textDesc = DataManagerText.GetText (TextLibs.overworldInteraction, $"{keyPrefix}_op_{optionKey}_text");
                            
                            if (option.textMain != null)
                                option.textMain.s = DataManagerText.GetText (TextLibs.overworldInteraction, $"{keyPrefix}_op_{optionKey}_header");
                            
                            if (option.textContent != null)
                                option.textContent.s = DataManagerText.GetText (TextLibs.overworldInteraction, $"{keyPrefix}_op_{optionKey}_text");
                        }
                    }
                }
            }
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldInteraction () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private bool showCore => DataMultiLinkerOverworldInteraction.Presentation.showCore;
        private bool showSteps => DataMultiLinkerOverworldInteraction.Presentation.showSteps;
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (steps != null)
            {
                foreach (var kvp in steps)
                {
                    var stepKey = kvp.Key;
                    var step = kvp.Value;

                    if (step == null)
                        continue;

                    var keyPrefix = $"{key}__s_{stepKey}";
                    
                    if (step.textHeader != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.overworldInteraction, $"{keyPrefix}_header",  step.textHeader.s);
                    
                    if (step.text != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.overworldInteraction, $"{keyPrefix}_main",  step.text.s);

                    if (step.options != null)
                    {
                        foreach (var kvp2 in step.options)
                        {
                            var optionKey = kvp2.Key;
                            var option = kvp2.Value;

                            if (option == null)
                                continue;
                            
                            if (option.textMain != null)
                                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldInteraction, $"{keyPrefix}_op_{optionKey}_header",  option.textMain.s);
                            
                            if (option.textContent != null)
                                DataManagerText.TryAddingTextToLibrary (TextLibs.overworldInteraction, $"{keyPrefix}_op_{optionKey}_text",  option.textContent.s);
                        }
                    }
                }
            }
        }
        
        private bool IsDisplayIsolated => DataMultiLinkerOverworldInteraction.Presentation.showIsolatedEntries;
        private bool IsInheritanceVisible => DataMultiLinkerOverworldInteraction.Presentation.showInheritance;
        private IEnumerable<string> GetStepKeys => steps != null ? steps.Keys : null;

        #if !PB_MODSDK
        [HideInEditorMode, Button ("Test (checked)"), ButtonGroup ("Test"), PropertyOrder (-1)]
        private void TestChecked ()
        {
            OverworldInteractionUtility.StartInteraction (this, TryGetSelectedEntity (), true);
        }
        
        [HideInEditorMode, Button ("Test (forced)"), ButtonGroup ("Test"), PropertyOrder (-1)]
        private void TestForced ()
        {
            OverworldInteractionUtility.StartInteraction (this, TryGetSelectedEntity (), true);
        }
        
        [HideInEditorMode, Button ("Test (review)"), ButtonGroup ("Test"), PropertyOrder (-1)]
        private void TestReview ()
        {
            OverworldInteractionUtility.StartInteraction (this, IDUtility.playerBaseOverworld, false, true, true);
        }
        
        private OverworldEntity TryGetSelectedEntity ()
        {
            var overworld = Contexts.sharedInstance.overworld;
            if (!overworld.hasSelectedEntity)
                return IDUtility.playerBaseOverworld;
			
            var selectionOverworld = IDUtility.GetOverworldEntity (overworld.selectedEntity.id);
            return selectionOverworld;
        }
        #endif
        
        #endif
        #endregion
    }
}

