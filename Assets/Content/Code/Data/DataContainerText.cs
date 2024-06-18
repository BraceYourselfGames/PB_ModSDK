using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    public static class TextLibs
    {
        public const string cutscenesVideo = "cutscenes_video";

        public const string equipmentPartPresets = "equipment_part_presets";
        public const string equipmentSubsystems = "equipment_subsystems";
        public const string equipmentGroups = "equipment_groups";
        public const string equipmentSockets = "equipment_sockets";
        public const string equipmentHardpoints = "equipment_hardpoints";
        
        public const string overworldEntities = "overworld_entities";
        public const string overworldProvinces = "overworld_provinces";
        public const string overworldEvents = "overworld_events";
        public const string overworldEntityActions = "overworld_entity_actions";
        public const string overworldMemories = "overworld_memories";
        public const string overworldResources = "overworld_resources";
        public const string overworldBranches = "overworld_branches";
        public const string overworldOther = "overworld_other";

        public const string baseActions = "base_actions";
        public const string baseUpgrades = "base_upgrades";
        public const string baseStats = "base_stats";
        public const string baseStatsGroups = "base_stats_groups";
        
        public const string scenarioModifiers = "scenario_modifiers";
        public const string scenarioComms = "scenario_comms";
        public const string scenarioEmbedded = "scenario_embedded";
        public const string scenarioStatesGlobal = "scenario_states_global";
        public const string scenarioStatesShared = "scenario_states_shared";
        public const string scenarioStepsShared = "scenario_steps_shared";
        public const string scenarioGroups = "scenario_groups";
        
        public const string combatActions = "combat_actions";
        public const string combatStats = "combat_stats";

        public const string uiHints = "ui_hints";
        public const string uiInputHints = "ui_input_hints";
        
        public const string uiCombat = "ui_combat";

        public const string uiSettings = "ui_settings";
        public const string uiSettingGroups = "ui_setting_groups";
        public const string uiSettingInputAction = "ui_setting_input_action";
        
        public const string uiDifficultySettings = "ui_difficulty_settings";
        public const string uiDifficultySettingGroups = "ui_difficulty_setting_groups";

        public const string uiTutorials = "ui_tutorials";
        public const string uiPause = "ui_pause";

        public const string uiOverworld = "ui_overworld";
        public const string uiBase = "ui_base";
        public const string uiCredits = "ui_credits";

        public const string workshopEmbedded = "workshop_embedded";
        public const string workshopShared = "workshop_shared";
        public const string workshopVariants = "workshop_variants";
        public const string workshopCategories = "workshop_categories";
        
        public const string pilotPersonalities = "pilot_personalities";
        public const string pilotChecks = "pilot_check";
        
        public const string unitRoles = "unit_roles";
        public const string unitGroups = "unit_groups";
        public const string unitChecks = "unit_checks";
        public const string unitBlueprints = "unit_blueprints";
        public const string unitStats = "unit_stats";
        public const string unitPerformanceClasses = "unit_perf_classes";
        public const string unitComposites = "unit_composites";
        public const string unitStatus = "unit_status";
    }

    public static class TextShortcuts
    {
        public static string GetUIHint (string textKey) => DataManagerText.GetText (TextLibs.uiHints, textKey);
        
        public static string GetUICombat (string textKey) => DataManagerText.GetText (TextLibs.uiCombat, textKey);
    }

    public static class TextKeysUIUnitInfo
    {
        public const string pilotMissingCallsign = "pilotMissingCallsign";
        public const string pilotMissingName = "pilotMissingName";
    }
    
    public static class TextErrors
    {

    }

    public static class TextLibraryHelper
    {
        public const string markupTagInputAction = "[ia=";
        public const string markupTagColor = "[col=";
        public const string markupTagEnd = "]";

        public const string tagItalicStart = "[i]";
        public const string tagItalicEnd = "[/i]";
        
        public const string chineseFullStop = "。";
        public const string chineseExclamationMark = "！";
        public const string chineseQuestionMark = "？";

        public const string newlineSequenceLong = "\r\n";
        public const string newlineSequenceShort = "\n";
        public const string newlineSequenceShortDouble = "\n\n";
        public const string newlineSequenceShortTriple = "\n\n\n";

        public static char[] newlineCharacters = new char[]
        {
            '\r',
            '\n'
        };

        public static List<string> defaultLibraries = new List<string>
        {
            "subtitles",
            "ui_options"
        };
        
        private static Type typeLast = null;
        private static List<string> textKeysToRemove = new List<string> ();

        private static Dictionary<string, DataBlockTextEntryMain> entriesMainPreserved = new Dictionary<string, DataBlockTextEntryMain> ();

        public static void OnBeforeTextExport (Type type, string textSectorKey, string prefix = null)
        {
            var sector = DataManagerText.GetLibrarySector (textSectorKey);
            if (sector == null || sector.entries == null)
            {
                Debug.LogWarning ($"Can't run general purpose OnBeforeTextExport for type {type?.Name} due to missing sector {textSectorKey}");
                return;
            }

            typeLast = type;
            textKeysToRemove.Clear ();
            entriesMainPreserved.Clear ();

            bool prefixChecked = !string.IsNullOrEmpty (prefix);
            var entries = sector.entries;
            
            foreach (var kvp in entries)
            {
                var key = kvp.Key;
                if (prefixChecked && !key.StartsWith (prefix))
                    continue;
                
                entriesMainPreserved.Add (key, kvp.Value);
                textKeysToRemove.Add (key);
            }

            foreach (var key in textKeysToRemove)
                entries.Remove (key);
        }
        
        public static void OnAfterTextExport (Type type, string textSectorKey)
        {
            if (typeLast != type)
            {
                Debug.LogWarning ($"Can't run general purpose OnAfterTextExport due to mismatch of last type (last - {typeLast?.Name}, new - {type?.Name}) making it unsafe to rely on shared collections");
                return;
            }

            var sector = DataManagerText.GetLibrarySector (textSectorKey);
            if (sector == null || sector.entries == null)
            {
                Debug.LogWarning ($"Can't run general purpose OnBeforeTextExport for type {type?.Name} due to missing sector {textSectorKey}");
                return;
            }
            
            sector.OnAfterDeserialization (textSectorKey); 
            
            // Restore saved data
            if (entriesMainPreserved.Count == 0)
                return;

            var entries = sector.entries;
            foreach (var kvp in entriesMainPreserved)
            {
                if (entries.ContainsKey (kvp.Key))
                {
                    var entry = entries[kvp.Key];
                    entry.temp = kvp.Value.temp;
                    entry.color = kvp.Value.color;
                    entry.noteDev = kvp.Value.noteDev;
                    entry.noteWriter = kvp.Value.noteWriter;
                }
            }
        }

        public static string RemoveUnwantedNewlines (string text, out int newlinesRemoved)
        {
            newlinesRemoved = 0;
            if (string.IsNullOrEmpty (text))
                return text;

            // Replace \r\n with \n
            text = text.Replace (TextLibraryHelper.newlineSequenceLong, TextLibraryHelper.newlineSequenceShort);
                
            // Remove trailing \r and \n
            text = text.TrimEnd (TextLibraryHelper.newlineCharacters);
            
            var sequenceTriple = TextLibraryHelper.newlineSequenceShortTriple;
            var sequenceDouble = TextLibraryHelper.newlineSequenceShortDouble;
                
            // Iterate until there are no instances where text is never separated by more than two newlines
            // An easy way to do that is to check for matches to `\n\n\n` and replace them with `\n\n` until no matches remain
            while (text.Contains (sequenceTriple))
            {
                text = text.Replace (sequenceTriple, sequenceDouble);
                newlinesRemoved += 1;
                    
                // I like giving while loops a way to exit in case I mess up the changes above and condition stays true
                if (newlinesRemoved > 100)
                {
                    // Debug.LogWarning ($"{key} | Failed to clean string {text} from chained new line characters, while loop got stuck");
                    break;
                }
            }
                
            // if (newlinesRemoved > 0)
            //     Debug.LogWarning ($"{key} | Detected and cleaned {newlinesRemoved} unwanted newlines in text {text}");

            return text;
        }
        
        const char charSpace = ' ';
        const string charsStart = "ABCDEFGHIKLMNPQRSTUVWXYZ";
        const string charsFiller = "abcdefghijklmnpqrstuvwxyz0123456789";
        private const int wordLengthMin = 3;
        private const int wordLengthMax = 12;
        
        private static char[] charBuffer = new char[10];
        private static int charsStartLength = charsStart.Length;
        private static int charsFillerLength = charsFiller.Length;
        
        private const string placeholderPrefix = "[d4ff00][b]";
        private const string placeholderSuffix = "[/b][-]";

        public static string GetRandomString (int length)
        {
            return GetRandomString (length, placeholderPrefix, placeholderSuffix);
        }
        
        public static string GetRandomString (int length, string prefix, string suffix)
        {
            bool prefixUsed = !string.IsNullOrEmpty (prefix);
            int prefixLength = prefixUsed ? prefix.Length : 0;

            bool suffixUsed = !string.IsNullOrEmpty (suffix);
            int suffixLength = suffixUsed ? suffix.Length : 0;
            int lengthTotal = length + prefixLength + suffixLength;

            // Resize buffer if too small
            if (charBuffer.Length < lengthTotal)
                charBuffer = new char[lengthTotal];
            
            if (prefixUsed)
            {
                for (var i = 0; i < prefixLength; ++i)
                    charBuffer[i] = prefix[i];
            }

            int wordLength = Random.Range (wordLengthMin, wordLengthMax);
            int wordIndex = 0;

            // Fill buffer with random characters up to desired length
            for (var i = 0; i < length; ++i)
            {
                int charIndex = i + prefixLength;
                if (wordIndex == wordLength)
                {
                    charBuffer[charIndex] = charSpace;
                    wordLength = Random.Range (wordLengthMin, wordLengthMax);
                    wordIndex = 0;
                }
                else
                {
                    if (i > 0)
                        charBuffer[charIndex] = charsFiller[Random.Range (0, charsFillerLength)];
                    else
                        charBuffer[charIndex] = charsStart[Random.Range (0, charsStartLength)];
                    ++wordIndex;
                }
            }
            
            if (suffixUsed)
            {
                for (var i = 0; i < suffixLength; ++i)
                {
                    int charIndex = i + prefixLength + length;
                    charBuffer[charIndex] = suffix[i];
                }
            }

            // Return subset of buffer of desired length
            return new string (charBuffer, 0, lengthTotal);
        }

        public static string FindAndInsertNewlines (string text)
        {
            text = FindAndInsertNewlines (text, chineseFullStop);
            text = FindAndInsertNewlines (text, chineseExclamationMark);
            text = FindAndInsertNewlines (text, chineseQuestionMark);
            return text;
        }
        
        public static string FindAndInsertNewlines (string text, string filter)
        {
            int loops = 0;
            int startIndex = 0;
            while (text.IndexOf (filter, startIndex, StringComparison.Ordinal) != -1)
            {
                startIndex = text.IndexOf (filter, startIndex, StringComparison.Ordinal);
                // Debug.Log ($"L{loops} | Found the glyph {filter} at index {startIndex}");
                startIndex += 1;
                text = text.Insert (startIndex, "\n");

                loops += 1;
                if (loops > 50)
                {
                    Debug.LogWarning ("Text markup processing seems to be broken, investigate this stack");
                    break;
                }
            }

            return text;
        }

        private const char formatCharOpen = '{';
        private const char formatCharClose = '}';

        public static int GetStringFormatArgumentCount (string input)
        {
            if (string.IsNullOrEmpty (input))
                return 0;

            int balance = 0;
            int argumentCount = 0;

            foreach (var character in input)
            {
                if (character == formatCharOpen)
                {
                    ++argumentCount;
                    ++balance;
                }
                else if (character == formatCharClose)
                {
                    --balance;
                }
            }

            if (balance != 0)
                argumentCount = 0;

            return argumentCount;
        }
    }

    [Serializable]
    public class DataContainerTextLibraryCore : DataContainerUnique
    {
        public bool hidden = false; 
        public int group = 0; 
        public string name = "English";

        public string flag = "icon_flag_english";
        public string font = "default";
        
        public string subtitleBuffer = string.Empty;
        public float subtitleFadeoutTime = 2f;
        public bool insertNewLines;
        public int widthSubtitle = 360;
        public int sizeSubtitle = 16;
        public int sizeHint = 16;
        public int sizeTooltipHeader = 16;
        public int sizeTooltipContent = 12;
        public int spacingTooltipHeader = 2;

        public bool pseudolocMode = false;
        public bool allowItalic = true;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockTextColor
    {
        [HideLabel, ColorUsage (false)]
        public Color value;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockTextNote
    {
        [HideLabel, TextArea (1, 10)]
        public string text;
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockTextEntryMain
    {
        [PropertyOrder (-4)]
        [LabelText ("Cell color"), LabelWidth (100f), HideIf ("@color == null"), InlineButtonClear]
        public DataBlockTextColor color;
        
        [PropertyOrder (-3)]
        [LabelText ("Dev. note"), LabelWidth (100f), HideIf ("@noteDev == null"), InlineButtonClear]
        public DataBlockTextNote noteDev;

        [PropertyOrder (-2)]
        [LabelText ("Writer note"), LabelWidth (100f), HideIf ("@noteWriter == null"), InlineButtonClear]
        public DataBlockTextNote noteWriter;

        [YamlIgnore]
        public string noteCombined
        {
            get
            {
                bool noteDevFound = noteDev != null && !string.IsNullOrEmpty (noteDev.text);
                bool noteWriterFound = noteWriter != null && !string.IsNullOrEmpty (noteWriter.text);
                
                if (noteDevFound && noteWriterFound)
                    return $"{noteDev.text}\n\n{noteWriter.text}";
                else if (noteDevFound)
                    return noteDev.text;
                else if (noteWriterFound)
                    return noteWriter.text;
                else
                    return string.Empty;
            }
        }
        
        [HideInInspector]
        public bool temp = true;
        
        [TextArea (1, 10)][HideLabel][HideIf ("@DataManagerText.showProcessedText")]
        public string text = "";

        [TextArea (1, 10)][HideLabel][YamlIgnore][ShowIf ("@DataManagerText.showProcessedText")][ReadOnly]
        public string textProcessed = "";

        public void OnBeforeSerialization (string key)
        {
            var textModified = TextLibraryHelper.RemoveUnwantedNewlines (text, out int newlinesRemoved);
            if (newlinesRemoved > 0)
            {
                Debug.LogWarning ($"{key} | Detected and cleaned {newlinesRemoved} unwanted newlines in text {text}");
                text = textModified;
            }
        }

        public void OnAfterDeserialization (string key, bool insertNewlines)
        {
            if (string.IsNullOrEmpty (text))
            {
                textProcessed = string.Empty;
                return;
            }
            
            textProcessed = text;

            if (insertNewlines)
                textProcessed = TextLibraryHelper.FindAndInsertNewlines (text);
            
            // Extend this over time whenever we need text processing
            // This can include adding suffixes, colors, tag handling etc.
        }

        #region Editor
        #if UNITY_EDITOR

        private static Color colorFinal = new Color (0.9f, 1f, 0.9f, 0.5f);
        private static Color colorTemp = new Color (1f, 0.7f, 0.7f, 1f);
        private Color GetTempButtonColor => temp ? colorTemp : colorFinal;
        private string GetTempButtonLabel => temp ? "Placeholder" : "Final";
        
        private static Color colorFull = new Color (1f, 1f, 1f, 1f);
        private static Color colorNull = new Color (1f, 1f, 1f, 0.5f);
        private const string bgOptions = "_DefaultHorizontalGroup/Buttons";
        
        [PropertyOrder (-5), HideIf ("temp")]
        [HorizontalGroup (48), ButtonWithIcon (SdfIconType.DashSquareFill, ButtonSizes.Medium, 20, "This text is marked as final"), GUIColor ("colorFinal")]
        private void MarkAsPlaceholder () =>
            temp = true;
        
        [PropertyOrder (-5), ShowIf ("temp")]
        [HorizontalGroup (48), ButtonWithIcon (SdfIconType.CheckSquareFill, ButtonSizes.Medium, 20, "This text is marked as placeholder"), GUIColor ("colorTemp")]
        private void MarkAsFinal () =>
            temp = false;

        [PropertyOrder (-5)]
        [ShowIf ("@color == null")]
        [ButtonGroup (bgOptions), Button ("+ Color", ButtonSizes.Medium), GUIColor ("colorNull")]
        private void OnColorButton () =>
            color = new DataBlockTextColor { value = new Color (0.6f, 0.9f, 0.4f, 1f) };

        [PropertyOrder (-5)]
        [ShowIf ("@noteWriter == null")]
        [ButtonGroup (bgOptions), Button ("+ Writer", ButtonSizes.Medium), GUIColor ("colorNull")]
        private void OnCommentButton () =>
            noteWriter = new DataBlockTextNote { text = "Writer note about the text entered" };
            
                    
        [PropertyOrder (-5)]
        [ShowIf ("@noteDev == null")]
        [ButtonGroup (bgOptions), Button ("+ Developer", ButtonSizes.Medium), GUIColor ("colorNull")]
        private void OnNoteButton () =>
            noteDev = new DataBlockTextNote { text = "Developer note about the context of the entry" };


        #endif
        #endregion
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockTextEntryLocalization
    {
        [TextArea (1, 10)][HideLabel][HideIf ("@DataManagerText.showProcessedText")]
        public string text = "";

        [TextArea (1, 10)][HideLabel][YamlIgnore][ShowIf ("@DataManagerText.showProcessedText")][ReadOnly]
        public string textProcessed = "";
        
        [TextArea (1, 10)][HideInInspector]
        public string textEng = "";

        public virtual void OnBeforeSerialization (string key)
        {
            var textModified = TextLibraryHelper.RemoveUnwantedNewlines (text, out int newlinesRemoved);
            if (newlinesRemoved > 0)
            {
                Debug.LogWarning ($"{key} | Detected and cleaned {newlinesRemoved} unwanted newlines in text {text}");
                text = textModified;
            }
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockTextSectorColumnInfo
    {
        [ValidateInput ("IsStringValid", "$invalidMessage")]
        public string suffix;
        
        [ValidateInput ("IsStringValid", "$invalidMessage")]
        public string suffixDisplayName;

        private bool IsStringValid (string input) => !string.IsNullOrEmpty (input);
        private string invalidMessage = "This field should never be null or empty to ensure correct export";
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockTextSectorSplitInfo
    {
        [ValidateInput ("IsStringValid", "$invalidMessage")]
        public string displayName;
        
        [ValidateInput ("IsStringValid", "$invalidMessage")]
        public string prefix;

        private bool IsStringValid (string input) => !string.IsNullOrEmpty (input);
        private string invalidMessage = "This field should never be null or empty to ensure correct export";
    }
    
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockTextSectorSuffixInfo
    {
        [LabelText ("Suffix / Context")]
        public string suffix;
        
        [TextArea][HideLabel]
        public string info;
    }

    public enum TextSectorExportMode
    {
        None,
        LinePerKeySimple,
        LinePerKeySplit,
        Collection
    }

    [Serializable]
    public class DataContainerTextSectorLocalization : DataContainer
    {
        [HideInInspector]
        public long timeLastSync;
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockTextEntryLocalization> entries = new SortedDictionary<string, DataBlockTextEntryLocalization> ();
        
        public DataBlockTextCollectionLocalized collection;

        [YamlIgnore, HideInInspector]
        public DataContainerTextLocalization parent;

        public override void OnBeforeSerialization ()
        {
            if (entries == null)
                return;

            foreach (var kvp in entries)
            {
                var entry = kvp.Value;
                if (entry != null)
                    entry.OnBeforeSerialization (kvp.Key);
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (entries != null)
            {
                bool parentCoreDefined = parent != null && parent.core != null;
                bool insertNewlines = parentCoreDefined && parent.core.insertNewLines;
                bool allowItalic = parentCoreDefined && parent.core.allowItalic;

                foreach (var kvp in entries)
                {
                    var entry = kvp.Value;
                    if (entry != null)
                    {
                        if (string.IsNullOrEmpty (entry.text))
                        {
                            entry.textProcessed = string.Empty;
                            continue;
                        }
            
                        entry.textProcessed = entry.text;

                        if (insertNewlines)
                            entry.textProcessed = TextLibraryHelper.FindAndInsertNewlines (entry.textProcessed);
                        
                        if (!allowItalic)
                        {
                            entry.textProcessed = entry.textProcessed.Replace (TextLibraryHelper.tagItalicStart, string.Empty);
                            entry.textProcessed = entry.textProcessed.Replace (TextLibraryHelper.tagItalicEnd, string.Empty);
                        }
            
                        // Extend this over time whenever we need text processing
                        // This can include adding suffixes, colors, tag handling etc.
                    }
                }
            }
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataContainerTextSectorMain : DataContainer
    {
        [HideInInspector]
        public long timeLastSync;
        
        public bool localized = false;

        public TextSectorExportMode exportMode = TextSectorExportMode.None;
    
        [LabelText ("Name / Desc.")]
        public string name;
                
        [HideLabel, TextArea]
        public string description;
        
        [LabelText ("Split Metadata"), ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true)]
        public List<DataBlockTextSectorSplitInfo> splits = new List<DataBlockTextSectorSplitInfo> (); 
        
        [PropertyOrder (2)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockTextEntryMain> entries = new SortedDictionary<string, DataBlockTextEntryMain> ();

        [PropertyOrder (1)]
        [YamlIgnore, ShowInInspector, ShowIf ("@entries != null && entries.Count > 0")]
        private SortedDictionary<string, DataBlockTextEntryMain> entriesTemp
        {
            get
            {
                entriesTempInternal.Clear ();
                if (entries != null)
                {
                    foreach (var kvp in entries)
                    {
                        if (kvp.Value.temp)
                            entriesTempInternal.Add (kvp.Key, kvp.Value);
                    }
                }
                return entriesTempInternal;
            }
            set
            {
                entriesTempInternal = value;
            }
        }

        private SortedDictionary<string, DataBlockTextEntryMain> entriesTempInternal = new SortedDictionary<string, DataBlockTextEntryMain> ();
        
        public DataBlockTextCollectionLibrary collection;

        [YamlIgnore, HideInInspector]
        public DataContainerTextLibrary parent;

        public override void OnBeforeSerialization ()
        {
            if (entries == null)
                return;

            foreach (var kvp in entries)
            {
                var entry = kvp.Value;
                if (entry != null)
                    entry.OnBeforeSerialization (kvp.Key);
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (entries != null)
            {
                bool insertNewlines = parent != null && parent.core != null && parent.core.insertNewLines;
                foreach (var kvp in entries)
                {
                    var entry = kvp.Value;
                    if (entry != null)
                        entry.OnAfterDeserialization (kvp.Key, insertNewlines);
                }
            }
        }
        
        [Button ("Mark all placeholder"), ButtonGroup]
        private void MarkAllTemporary () => Mark (true);
        
        [Button ("Mark all final"), ButtonGroup]
        private void MarkAllFinal () => Mark (false);

        private void Mark (bool temp)
        {
            if (entries == null)
                return;

            foreach (var kvp in entries)
            {
                var entry = kvp.Value;
                if (entry == null)
                    continue;

                entry.temp = temp;
            }
        }

        #if UNITY_EDITOR
        
        /*
        private const string bgOptions = "_DefaultHorizontalGroup/Buttons";
        
        [PropertyOrder (-1), HideIf ("temp"), EnableIf ("IsExportAvailable")]
        [HorizontalGroup (48), ButtonWithIcon (SdfIconType.DashSquareFill, ButtonSizes.Medium, 20, "YAML files are currently not saved/loaded"), GUIColor ("colorFinal")]
        private void EnableFileUse ()
        {
            var helper = DataHelperTextPipeline.instance;
            if (helper != null)
                helper.useFiles = true;
        }
        
        [PropertyOrder (-1), ShowIf ("temp"), EnableIf ("IsExportAvailable")]
        [HorizontalGroup (48), ButtonWithIcon (SdfIconType.FileEarmarkCheckFill, ButtonSizes.Medium, 20, "YAML files are currently saved/loaded"), GUIColor ("colorTemp")]
        private void DisableFileUse ()
        {
            var helper = DataHelperTextPipeline.instance;
            if (helper != null)
                helper.useFiles = false;
        }
        */
        
        /*
        [PropertyOrder (-1), EnableIf ("IsExportAvailable")]
        [ButtonWithIcon (SdfIconType.CloudDownload, "Download text from Google Sheets"), ButtonGroup]
        private void ReadFromSheets ()
        {
            var helper = DataHelperTextPipeline.instance;
            if (helper != null)
                helper.DownloadLibrarySector (key);
        }

        [PropertyOrder (-1), EnableIf ("IsExportAvailable")]
        [ButtonWithIcon (SdfIconType.CloudUpload, "Upload text to Google Sheets"), ButtonGroup]
        private void WriteToSheets ()
        {
            var helper = DataHelperTextPipeline.instance;
            if (helper != null)
                helper.UploadLibrarySector (key);
        }
        
        private bool IsExportAvailable => exportMode == TextSectorExportMode.LinePerKeySimple || exportMode == TextSectorExportMode.LinePerKeySplit;
        */
        
        #endif
    }
    
    [Serializable]
    public class DataBlockTextCollectionLibrary : DataContainer
    {
        public bool temp = true;
        
        public DataBlockTextNote noteWriter;
        
        public HashSet<string> tags = new HashSet<string> ();
        
        [ListDrawerSettings (DefaultExpandedState = false, ShowIndexLabels = true)]
        public List<string> entries = new List<string> ();
    }

    [Serializable]
    public class DataBlockTextCollectionLocalized : DataContainer
    {
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> entries = new List<string> ();
        
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> entriesEng = new List<string> ();
    }
    
    
    [Serializable]
    public class DataContainerTextCollectionLocalization : DataContainer
    {
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> entries = new List<string> ();
    }
    
    [Serializable]
    public class DataContainerTextLibrary : DataContainerUnique
    {
        [FoldoutGroup ("Other", false)]
        public DataContainerTextLibraryCore core;

        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [HideReferenceObjectPicker]
        public SortedDictionary<string, DataContainerTextSectorMain> sectors = new SortedDictionary<string, DataContainerTextSectorMain> ();

        [YamlIgnore, NonSerialized, HideInInspector]
        public SortedDictionary<string, DataContainerTextSectorMain> sectorsWithCollections = new SortedDictionary<string, DataContainerTextSectorMain> ();

        public override void OnAfterDeserialization ()
        {
            if (sectors == null)
            {
                Debug.Log ("Text library has no sectors, skipping text processing");
                return;
            }

            if (sectorsWithCollections == null)
                sectorsWithCollections = new SortedDictionary<string, DataContainerTextSectorMain> ();
            sectorsWithCollections.Clear ();

            foreach (var kvp1 in sectors)
            {
                var sector = kvp1.Value;
                if (sector == null)
                {
                    Debug.Log ($"Encountered a null sector under key {kvp1.Key} while processing text library");
                    continue;
                }

                sector.parent = this;
                sector.OnAfterDeserialization (kvp1.Key);
                
                if (sector.collection != null)
                    sectorsWithCollections.Add (kvp1.Key, sector);
            }
        }

        public DataContainerTextSectorMain GetSector (string sectorKey)
        {
            if (sectors == null || string.IsNullOrEmpty (sectorKey) || !sectors.ContainsKey (sectorKey))
                return null;
            else
                return sectors[sectorKey];
        }

        public void TryAddingText (string sectorKey, string textKey, string text)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Can't add text to library: no sector key provided");
                return;
            }
            
            if (string.IsNullOrEmpty (textKey))
            {
                Debug.LogWarning ($"Can't add text to library sector {sectorKey}: no text key provided");
                return;
            }

            var sector = GetSector (sectorKey);
            if (sector == null)
            {
                sector = TryAddingSector (sectorKey);
                if (sector == null)
                    return;
            }
            
            if (text == null)
                text = string.Empty;

            if (sector.entries.ContainsKey (textKey))
                sector.entries[textKey].text = text;
            else
                sector.entries.Add (textKey, new DataBlockTextEntryMain { text = text });
        }
        
        public void TryRemovingText (string sectorKey, string textKey)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Can't delete text from the library: no sector key provided");
                return;
            }
            
            if (string.IsNullOrEmpty (textKey))
            {
                Debug.LogWarning ($"Can't delete text from the library sector {sectorKey}: no text key provided");
                return;
            }

            var sector = GetSector (sectorKey);
            if (sector == null)
            {
                Debug.LogWarning ($"Can't delete text from the library sector {sectorKey}: no such sector exists");
                return;
            }

            if (!sector.entries.ContainsKey (textKey))
            {
                Debug.LogWarning ($"Can't delete text from the library sector/key {sectorKey}/{textKey}: no such text key exists");
                return;
            }
                
            Debug.Log ($"Removed text library sector/key {sectorKey}/{textKey}");
            sector.entries.Remove (textKey);
        }
        
        public DataContainerTextSectorMain TryAddingSector (string sectorKey)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Library sector couldn't be added due to null or empty key");
                return null;
            }

            var sectorExisting = GetSector (sectorKey);
            if (sectorExisting != null)
            {
                Debug.LogWarning ($"Library sector {sectorKey} couldn't be added due to already being present");
                return null;
            }

            Debug.Log ($"Added library sector {sectorKey}");
            var sectorNew = new DataContainerTextSectorMain ();
            sectors[sectorKey] = sectorNew;
            return sectorNew;
        }
    }
    
    [Serializable]
    public class DataContainerTextLocalization : DataContainerUnique
    {
        public DataContainerTextLibraryCore core;

        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [HideReferenceObjectPicker]
        public SortedDictionary<string, DataContainerTextSectorLocalization> sectors = new SortedDictionary<string, DataContainerTextSectorLocalization> ();

        public override void OnAfterDeserialization ()
        {
            bool pseudolocMode = core != null && core.pseudolocMode;
            if (pseudolocMode)
            {
                var libraryData = DataManagerText.libraryData;
                bool pseudolocRestrictedToFinalText = DataShortcuts.debug.pseudolocRestrictedToFinalText;

                sectors = new SortedDictionary<string, DataContainerTextSectorLocalization> ();
                foreach (var kvp in libraryData.sectors)
                {
                    var sectorFromLibrary = kvp.Value;
                    
                    if (pseudolocRestrictedToFinalText && !sectorFromLibrary.localized)
                        continue;

                    var sectorNew = new DataContainerTextSectorLocalization ();
                    sectors.Add(kvp.Key, sectorNew);

                    // Skip all empty collections from library
                    if (sectorFromLibrary.entries != null && sectorFromLibrary.entries.Count != 0)
                    {
                        var entriesFromLibrary = sectorFromLibrary.entries;
                        var entriesNew = new SortedDictionary<string, DataBlockTextEntryLocalization>();
                        sectorNew.entries = entriesNew;

                        foreach (var kvp2 in entriesFromLibrary)
                        {
                            var entryFromLibrary = kvp2.Value;

                            if (pseudolocRestrictedToFinalText && entryFromLibrary.temp)
                                continue;

                            var entryNew = new DataBlockTextEntryLocalization();
                            entryNew.text = PseudoLoc.ApplyPseudoloc (entryFromLibrary.text);
                            sectorNew.entries.Add(kvp2.Key, entryNew);
                        }
                    }
                    
                    var collectionFromLibrary = sectorFromLibrary.collection;
                    if (collectionFromLibrary != null && collectionFromLibrary.entries != null)
                    {
                        var entriesCollectionNew = new List<string> (collectionFromLibrary.entries.Count);
                        sectorNew.collection = new DataBlockTextCollectionLocalized ();
                        sectorNew.collection.entries = entriesCollectionNew;

                        foreach (var entryFromLibrary in collectionFromLibrary.entries)
                        {
                            var entryNew = PseudoLoc.ApplyPseudoloc (entryFromLibrary);
                            entriesCollectionNew.Add (entryNew);
                        }
                    }
                    
                }
            }

            if (sectors != null)
            {
                foreach (var kvp1 in sectors)
                {
                    var sector = kvp1.Value;
                    if (sector == null)
                    {
                        Debug.Log ($"Encountered a null sector under key {kvp1.Key} while processing text localization");
                        continue;
                    }

                    sector.parent = this;
                    sector.OnAfterDeserialization (kvp1.Key);
                }
            }
        }
        
        public DataContainerTextSectorLocalization GetSector (string sectorKey)
        {
            if (sectors == null || string.IsNullOrEmpty (sectorKey) || !sectors.ContainsKey (sectorKey))
                return null;
            else
                return sectors[sectorKey];
        }
        
        public void TryAddingText (string sectorKey, string textKey, string text)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Can't add text to library: no sector key provided");
                return;
            }
            
            if (string.IsNullOrEmpty (textKey))
            {
                Debug.LogWarning ($"Can't add text to library sector {sectorKey}: no text key provided");
                return;
            }

            var sector = GetSector (sectorKey);
            if (sector == null)
            {
                sector = TryAddingSector (sectorKey);
                if (sector == null)
                    return;
            }
            
            if (text == null)
                text = string.Empty;

            if (sector.entries.ContainsKey (textKey))
                sector.entries[textKey].text = text;
            else
                sector.entries.Add (textKey, new DataBlockTextEntryLocalization { text = text });
        }
        
        public void TryRemovingText (string sectorKey, string textKey)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Can't delete text from the library: no sector key provided");
                return;
            }
            
            if (string.IsNullOrEmpty (textKey))
            {
                Debug.LogWarning ($"Can't delete text from the library sector {sectorKey}: no text key provided");
                return;
            }

            var sector = GetSector (sectorKey);
            if (sector == null)
            {
                Debug.LogWarning ($"Can't delete text from the library sector {sectorKey}: no such sector exists");
                return;
            }

            if (!sector.entries.ContainsKey (textKey))
            {
                Debug.LogWarning ($"Can't delete text from the library sector/key {sectorKey}/{textKey}: no such text key exists");
                return;
            }
                
            Debug.Log ($"Removed text library sector/key {sectorKey}/{textKey}");
            sector.entries.Remove (textKey);
        }
        
        public DataContainerTextSectorLocalization TryAddingSector (string sectorKey)
        {
            if (string.IsNullOrEmpty (sectorKey))
            {
                Debug.LogWarning ($"Library sector couldn't be added due to null or empty key");
                return null;
            }

            var sectorExisting = GetSector (sectorKey);
            if (sectorExisting != null)
            {
                Debug.LogWarning ($"Library sector {sectorKey} couldn't be added due to already being present");
                return null;
            }

            Debug.Log ($"Added library sector {sectorKey}");
            var sectorNew = new DataContainerTextSectorLocalization ();
            sectors[sectorKey] = sectorNew;
            return sectorNew;
        }
    }
}

