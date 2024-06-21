using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldEventOptionLink
    {
        [PropertyOrder (-1), HorizontalGroup (80f), ToggleLeft]
        public bool shared;

        [PropertyOrder (-2), HorizontalGroup]
        [ValueDropdown ("GetKeys"), HideLabel]
        public string key;

        [DisplayAsString (true), BoxGroup]
        [ShowIf ("@IsTextVisible && GetOption (false) != null")]
        [ShowInInspector, YamlIgnore]
        [HideLabel]
        public string textHeader
        {
            get
            {
                var option = GetOption ();
                return option?.textHeader;
            }
            set
            {
                var option = GetOption (true);
                if (option != null)
                    option.textHeader = value;
            }
        }

        [DisplayAsString (true), BoxGroup]
        [ShowIf ("@IsTextVisible && GetOption (false) != null")]
        [ShowInInspector, YamlIgnore]
        [HideLabel]
        public string textContent
        {
            get
            {
                var option = GetOption ();
                return option?.textContent;
            }
            set
            {
                var option = GetOption (true);
                if (option != null)
                    option.textContent = value;
            }
        }

        [HideInInspector, YamlIgnore]
        public DataContainerOverworldEvent parentEvent;

        [HideInInspector, YamlIgnore]
        public DataBlockOverworldEventStep parentStep;

        private IEnumerable<string> GetKeys ()
        {
            if (shared)
                return DataMultiLinkerOverworldEventOption.data.Keys;
            else
                return parentEvent != null && parentEvent.options != null ? parentEvent.options.Keys : null;
        }

        public DataContainerOverworldEventOption GetOption (bool markModifiedIfFound = false)
        {
            if (shared)
            {
                var option = DataMultiLinkerOverworldEventOption.GetEntry (key, false);
                if (markModifiedIfFound && option != null)
                {
                    #if PB_MODSDK
                    DataMultiLinkerOverworldEventOption.unsavedChangesPossible = DataContainerModData.hasSelectedConfigs;
                    #else
                    DataMultiLinkerOverworldEventOption.unsavedChangesPossible = true;
                    #endif
                }
                return option;
            }
            else
            {
                var options = parentEvent != null && parentEvent.options != null ? parentEvent.options : null;
                var option = !string.IsNullOrEmpty (key) && options != null && options.ContainsKey (key) ? options[key] : null;
                if (markModifiedIfFound && option != null)
                {
                    #if PB_MODSDK
                    DataMultiLinkerOverworldEvent.unsavedChangesPossible = DataContainerModData.hasSelectedConfigs;
                    #else
                    DataMultiLinkerOverworldEvent.unsavedChangesPossible = true;
                    #endif
                }
                return option;
            }
        }

        private bool IsTextVisible => DataMultiLinkerOverworldEvent.Presentation.showStepOptionsText;
    }

    public static class TextVariantHelper
    {
        public static Dictionary<string, int> pronounsToMemoryValues = new Dictionary<string, int>
        {
            { pronoun1, 1 },
            { pronoun2, 2 },
            { pronoun3, 3 }
        };

        public static List<string> pronouns = new List<string>
        {
            pronoun1,
            pronoun2,
            pronoun3
        };

        public const string memoryKey = "pronoun";

        public const string pronoun1 = "she";
        public const string pronoun2 = "he";
        public const string pronoun3 = "they";

        public static void GeneratePilotTextVariants (ref SortedDictionary<string, DataBlockEventTextVariant> textVariants)
        {
            if (textVariants == null)
                textVariants = new SortedDictionary<string, DataBlockEventTextVariant> ();
            else
                textVariants.Clear ();

            textVariants.Add
            (
                $"pronoun_{pronoun1}",
                new DataBlockEventTextVariant
                {
                    generated = true,
                    checkForActorPronouns = new DataBlockOverworldEventPronounCheck { pronoun = pronoun1 }
                }
            );

            textVariants.Add
            (
                $"pronoun_{pronoun2}",
                new DataBlockEventTextVariant
                {
                    generated = true,
                    checkForActorPronouns = new DataBlockOverworldEventPronounCheck { pronoun = pronoun2 }
                }
            );

            // This collection could technically include another variant for pronoun3.
            // However, it's more efficient to just use root text of a step for "they" case. Root level text is always present in localization DB,
            // and not using it for one of the cases will leave a ton of cells unused and create confusion during editing.
        }
    }

    public class DataBlockOverworldEventPronounCheck
    {
        // Which actor slot do we check? An event can have multiple actors, this can't be assumed
        public string actorKey = "pilot_0";

        // What value should the given actor match for the check to succeed
        // For readability and since the exact numerical value we use might change over time, here this is saved as a text key
        [ValueDropdown ("@TextVariantHelper.pronouns")]
        public string pronoun = TextVariantHelper.pronoun1;

        public bool IsPassed ()
        {
            return false;
        }
    }

    [HideReferenceObjectPicker]
    public class DataBlockEventTextVariant
    {
        [HideInInspector]
        public bool generated = false;

        [LabelText ("Header / Content")]
        [YamlIgnore]
        public string textHeader = "PLACEHOLDER";

        [TextArea (1, 3)][HideLabel]
        [YamlIgnore]
        public string textContent = "PLACEHOLDER";

        // Lightweight check for the most common text variant case - make variant linked to gender of specific pilot actor
        [HideReferenceObjectPicker, InlineButtonClear, BoxGroup ("A", false)]
        [LabelText ("Condition (Pronouns)")]
        [HideIf ("generated")]
        public DataBlockOverworldEventPronounCheck checkForActorPronouns;

        // Fully featured check block can be reused here
        // Allows for completely freeform text variants, for example:
        // - if player base has too few units, civilians are surprised and phrase their request for help differently
        // - if the time of day is night, we phrase the description of how we spotted the enemy differently
        [HideReferenceObjectPicker, InlineButtonClear, BoxGroup ("B", false)]
        [LabelText ("Condition (General)")]
        [HideIf ("generated")]
        public DataBlockOverworldEventCheck check;
    }

    public enum EventMusicMoods
    {
        None = 0,
        Restorative = 1,
        Positive = 2,
        ResearchInvestigating = 3,
        ConstructionSalvage = 4,
        Negative = 5,
        Neutral = 6,
        Warning = 7,
        Standard = 8,
    }

    public class DataBlockOverworldEventStep
    {
        [HideInInspector]
        public bool reviewed = false;

        [ShowIf ("@IsCoreVisible && AreImagesVisible")]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEvents)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEvents, 128)", false)]
        [HideLabel]
        public string image = "event_civilian_meeting";

        [ShowIf ("IsCoreVisible")]
        [HideLabel]// [LabelText ("Header / Content")]
        [YamlIgnore]
        public string textName = "PLACEHOLDER";

        [ShowIf ("IsCoreVisible")]
        [TextArea (1, 10)][HideLabel]
        [YamlIgnore]
        public string textDesc = "PLACEHOLDER";

        [ShowIf ("AreTextVariantsVisible")]
        [DropdownReference]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockEventTextVariant> textVariants;

        [YamlIgnore]
        [ShowIf ("AreTextVariantsGeneratedVisible")]
        [DictionaryDrawerSettings (IsReadOnly = true, DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockEventTextVariant> textVariantsGenerated;

        [ShowIf ("AreOptionsVisible")]
        [ListDrawerSettings (ShowPaging = false, CustomAddFunction = "AddOption")]
        public List<DataBlockOverworldEventOptionLink> options = new List<DataBlockOverworldEventOptionLink> ()
        {
            new DataBlockOverworldEventOptionLink
            {
                shared = false,
                key = "intel",
                textHeader = null,
                textContent = null,
                parentEvent = null,
                parentStep = null
            },
            new DataBlockOverworldEventOptionLink
            {
                shared = false,
                key = "leave",
                textHeader = null,
                textContent = null,
                parentEvent = null,
                parentStep = null
            }
        };
        
        [ShowIf ("@IsTabWriting || IsCoreVisible")]
        public bool resourceDisplay = true;
        
        [ShowIf ("@IsTabWriting || IsCoreVisible")]
        public bool colorCustom = false;

        [ShowIf ("@IsTabWriting || IsCoreVisible")]
        [ValueDropdown ("GetColorKeys"), GUIColor ("GetColorPreview")]
        public string colorKey = DataKeysEventColor.Negative;

        [ShowIf ("@(IsTabWriting || IsCoreVisible) && colorCustom")]
        [ColorUsage (showAlpha: false)]
        public Color color = Color.white;

        [ShowIf ("@IsTabWriting && IsCoreVisible")]
        public EventMusicMoods eventMood = EventMusicMoods.Negative;

        [HideInInspector]
        [ShowIf ("@!IsTabWriting && IsCoreVisible")]
        public int priority;

        [ShowIf ("@!IsTabWriting && IsCheckVisible")]
        [DropdownReference (true)]
        public DataBlockOverworldEventCheck check = new DataBlockOverworldEventCheck
        {
            forceLog = false,
            self = new DataBlockOverworldEventCheckSelf (),
            target = new DataBlockOverworldEventCheckTarget (),
            province = null,
            action = null,
            actors = null,
            parentStep = null,
            parentOption = null
        };

        [ShowIf ("@!IsTabWriting && AreEffectsVisible")]
        [DropdownReference (true)]
        public DataBlockOverworldEventActorRefresh actorRefresh;

        [ShowIf ("@!IsTabWriting && AreEffectsVisible")]
        [DropdownReference (true)]
        public DataBlockOverworldEventHopeChange hopeChange;

        [ShowIf ("@!IsTabWriting && AreEffectsVisible")]
        [DropdownReference (true)]
        public DataBlockOverworldEventWarScoreChange warScoreChange;

        [ShowIf ("@!IsTabWriting && AreEffectsVisible")]
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true)]
        public List<DataBlockMemoryChangeGroupEvent> memoryChanges;

        /*
        [ShowIf ("@!IsTabWriting && AreEffectsVisible")]
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true)]
        public List<DataBlockOverworldActionInstanceData> actionsCreated;
        */

        [ShowIf ("@!IsTabWriting && AreEffectsVisible")]
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true)]
        public List<IOverworldEventFunction> functions;


        [HideInInspector, YamlIgnore]
        public string key;

        [HideInInspector, YamlIgnore]
        public DataContainerOverworldEvent parent;

        private const string foldoutGroupEffects = "Effects";
        private Color colorFallback = Color.white.WithAlpha (1f);

        private Color GetColorPreview ()
        {
            if (!colorCustom)
            {
                var colorInfo = DataMultiLinkerUIColor.GetEntry (colorKey);
                if (colorInfo != null && colorInfo.colorCache != null)
                    return colorInfo.colorCache.colorHover;
                else
                    return colorFallback;
            }
            else
                return colorFallback;
        }

        #region Editor
        #if UNITY_EDITOR

        private bool IsTabWriting => parent != null && parent.IsTabWriting;
        private bool IsTabSteps => parent != null && parent.IsTabSteps;
        private bool AreImagesVisible => DataMultiLinkerOverworldEvent.Presentation.showStepImages;

        private static bool IsCoreVisible => DataMultiLinkerOverworldEvent.Presentation.showStepCore;
        private static bool IsCheckVisible => DataMultiLinkerOverworldEvent.Presentation.showStepCheck;
        private static bool AreOptionsVisible => DataMultiLinkerOverworldEvent.Presentation.showStepOptions;
        private static bool AreEffectsVisible => DataMultiLinkerOverworldEvent.Presentation.showStepEffects;

        private bool AreTextVariantsVisible => (IsTabWriting || IsTabSteps) && DataMultiLinkerOverworldEvent.Presentation.showTextVariants;
        private bool AreTextVariantsGeneratedVisible => (IsTabWriting || IsTabSteps) && DataMultiLinkerOverworldEvent.Presentation.showTextVariantsGenerated && textVariantsGenerated != null && textVariantsGenerated.Count > 0;

        [ShowInInspector]
        [ShowIf ("@!IsTabWriting")]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldEventStep () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private IEnumerable<string> GetOptionKeysShared () =>
            DataMultiLinkerOverworldEventOption.data.Keys;

        private IEnumerable<string> GetColorKeys () =>
            DataMultiLinkerUIColor.data.Keys;

        private IEnumerable<string> GetOptionKeysCustom () =>
            parent != null && parent.options != null ? parent.options.Keys : null;

        private void AddOption () =>
            options.Add (new DataBlockOverworldEventOptionLink ());

        #endif
        #endregion

    }



    public class DataBlockOverworldEventOptionInjection
    {
        [DropdownReference (true)]
        public DataBlockBoolCheck optionCombatPresent;

        [DropdownReference (true)]
        public DataBlockBoolCheck optionExitPresent;

        [DropdownReference]
        public SortedDictionary<string, bool> optionKeysFilter;

        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerOverworldEvent.data.Keys")]
        public HashSet<string> eventKeysCompatible;

        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerOverworldEvent.data.Keys")]
        public HashSet<string> eventKeysBlocked;

        [DropdownReference (true)]
        public DataBlockOverworldEventCheck check;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldEventOptionInjection () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockOverworldEventActorRefresh
    {
        public bool refreshWorld = true;
        public bool refreshUnits = true;
        public bool refreshPilots = true;
    }

    public class DataBlockOverworldEventHopeChange
    {
        public int offset = 0;
    }

    public class DataBlockOverworldEventWarScoreChange
    {
        public enum Faction
        {
            Player,
            Enemy
        }

        public Faction faction;
        public float offset;
    }

    public class DataBlockResourceChange
    {
        [ValueDropdown ("GetResourceKeys")]
        public string key;

        [Tooltip ("If true, a change subtracting resources will be checked before being allowed to proceed")]
        public bool check = true;

        [ShowIf ("check")]
        [Tooltip ("If true, and used in overworld option, option will be hidden instead of grayed out if a change isn't possible")]
        public bool checkStrict = false;

        [Tooltip ("If true, the value below will be added to current amount of a given resource, if false, then value below will replace current amount")]
        public bool offset = true;

        public int value = 0;

        #region EDITOR
        #if UNITY_EDITOR

        private IEnumerable<string> GetResourceKeys =>
            DataMultiLinkerResource.data?.Keys;

        #endif
        #endregion

        public override string ToString ()
        {
            string keyText = !string.IsNullOrEmpty (key) ? key : "[no key]";
            string checkText = check ? checkStrict ? "(checked strictly)" : "(checked)" : "(unchecked)";

            if (offset)
            {
                return $"{keyText}: Offset by {value} {checkText}";
            }
            else
            {
                return $"{keyText}: Set to {value} {checkText}";
            }
        }
    }

    [HideReferenceObjectPicker, LabelWidth (200f)]
    public class DataBlockOverworldEventCombat
    {
        [PropertyTooltip ("Whether it's possible to quit from this combat opportunity. If set to false, briefing will not offer an option to leave. This must always be consistent with options offered in current event step.")]
        public bool optional = true;

        public bool scenarioTagsFromTarget = false;

        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerScenario.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> scenarioTags;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldEventCombat () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
}
