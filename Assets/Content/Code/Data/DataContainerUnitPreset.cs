using System;
using System.Collections.Generic;
using System.Linq;
using Entitas;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
#endif

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker][LabelWidth (180f)]
    public class DataBlockVirtualResource
    {
        [HorizontalGroup ("A")]
        [LabelText ("@amountRandom ? \"Amount (min/max)\" : \"Amount\"")]
        public int amountMin = 1;
        
        [HorizontalGroup ("A", 0.33f)]
        [ShowIf ("amountRandom")]
        [HideLabel]
        public int amountMax = 1;
        
        [HorizontalGroup ("A", 18f)]
        [HideLabel]
        public bool amountRandom = false;
    }
    
    [Serializable][HideReferenceObjectPicker][LabelWidth (180f)]
    public class DataBlockVirtualEntity
    {
        [HorizontalGroup ("A")]
        [LabelText ("@countRandom ? \"Count (min/max)\" : \"Count\"")]
        public int countMin = 1;
        
        [HorizontalGroup ("A", 0.33f)]
        [ShowIf ("countRandom")]
        [HideLabel]
        public int countMax = 1;
        
        [HorizontalGroup ("A", 18f)]
        [HideLabel]
        public bool countRandom = false;
        
        public string ToStringCount () =>
            countRandom && countMax != countMin ? $"{countMin}-{countMax}" : countMin.ToString ();
    }
    
    public class DataBlockVirtualPilots : DataBlockVirtualEntity
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetFactions ()")]
        public string faction = Factions.player;
        
        public DataBlockVirtualPilots () { }
        
        public DataBlockVirtualPilots (DataBlockVirtualPilots source)
        {
            faction = source.faction;
        }
    }

    public class DataBlockVirtualParts : DataBlockVirtualEntity
    {
        [HorizontalGroup ("B")]
        [LabelText ("@levelRandom ? \"Level (min/max)\" : \"Level\"")]
        public int levelMin = 1;
        
        [HorizontalGroup ("B", 0.33f)]
        [ShowIf ("levelRandom")]
        [HideLabel]
        public int levelMax = 1;
        
        [HorizontalGroup ("B", 18f)]
        [HideLabel]
        public bool levelRandom = false;

	    [GUIColor ("GetQualityTableColor")]
	    [ValueDropdown ("GetQualityTableKeys")]
	    public string qualityTableKey;

	    private IEnumerable<string> GetQualityTableKeys => DataMultiLinkerQualityTable.data.Keys;
        private Color GetQualityTableColor => !string.IsNullOrEmpty(qualityTableKey) && DataMultiLinkerQualityTable.data.TryGetValue(qualityTableKey, out var v) ? v.uiColor : Color.white;

	    [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string preset;
        
        public bool tagsUsed = true;
        
        [ShowIf ("tagsUsed")]
        [DictionaryKeyDropdown("@DataMultiLinkerPartPreset.tags")]
        public SortedDictionary<string, bool> tags;

        public override string ToString ()
        {
            if (tagsUsed)
                return $"X{ToStringCount ()}: {tags.ToStringFormattedKeyValuePairs ()}";
            else
                return $"X{ToStringCount ()}: {preset}";
        }
        
        public string ToStringExpanded (bool multiline = false, string linePrefix = null)
        {
            if (tagsUsed)
            {
                var partsFiltered = DataTagUtility.GetKeysWithTags (DataMultiLinkerPartPreset.data, tags, limit: 6);
                return $"[ff]{ToStringCount ()} random:{(multiline ? "\n" : " ")}{partsFiltered.ToStringFormatted (multiline, multilinePrefix: linePrefix)}";
            }
            else
                return $"[ff]{ToStringCount ()} fixed: {preset}";
        }

        public DataBlockVirtualParts () { }

        public DataBlockVirtualParts (DataBlockVirtualParts source)
        {
            countMin = source.countMin;
            countMax = source.countMax;
            countRandom = source.countRandom;

            levelMin = source.levelMin;
            levelMax = source.levelMax;
            levelRandom = source.levelRandom;
            
            preset = source.preset;
            tagsUsed = source.tagsUsed;
            tags = tagsUsed ? new SortedDictionary<string, bool> () : null;

            qualityTableKey = source.qualityTableKey;
            
            if (tagsUsed && source.tags != null)
            {
                foreach (var kvp in source.tags)
                    tags.Add (kvp.Key, kvp.Value);
            }
        }
    }
    
    public class DataBlockVirtualSubsystems : DataBlockVirtualEntity
    {
        [InlineButtonClear]
        public DataBlockRangeInt ratingRange;
        
        [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        public string blueprint;
        
        public bool tagsUsed = true;
        
        [ShowIf ("tagsUsed")]
        [DictionaryKeyDropdown("@DataMultiLinkerSubsystem.tags")]
        public SortedDictionary<string, bool> tags;

        public override string ToString ()
        {
            if (tagsUsed)
                return $"X{ToStringCount ()}: {tags.ToStringFormattedKeyValuePairs ()}";
            else
                return $"X{ToStringCount ()}: {blueprint}";
        }
        
        public string ToStringExpanded (bool multiline = false, string linePrefix = null)
        {
            if (tagsUsed)
            {
                var subsystemsFiltered = DataTagUtility.GetKeysWithTags (DataMultiLinkerSubsystem.data, tags, limit: 6);
                return $"[ff]{ToStringCount ()} random:[aa]{(multiline ? "\n" : " ")}{subsystemsFiltered.ToStringFormatted (multiline, multilinePrefix: linePrefix)}[ff]";
            }
            else
                return $"[ff]{ToStringCount ()} fixed:[aa] {blueprint}[ff]";
        }
        
        public DataBlockVirtualSubsystems () { }

        public DataBlockVirtualSubsystems (DataBlockVirtualSubsystems source)
        {
            countMin = source.countMin;
            countMax = source.countMax;
            countRandom = source.countRandom;

            blueprint = source.blueprint;
            tagsUsed = source.tagsUsed;
            tags = tagsUsed ? new SortedDictionary<string, bool> () : null;
            if (tagsUsed && source.tags != null)
            {
                foreach (var kvp in source.tags)
                    tags.Add (kvp.Key, kvp.Value);
            }
        }
    }

    [LabelWidth (120f)]
    public class DataBlockVirtualWorkshopProject
    {
        [HorizontalGroup ("A"), PropertyOrder (-2)]
        [LabelText ("@countRandom ? \"Count (min/max)\" : \"Count\"")]
        public int countMin = 1;
        
        [HorizontalGroup ("A", 0.33f), PropertyOrder (-2)]
        [ShowIf ("countRandom")]
        [HideLabel]
        public int countMax = 1;
        
        [HorizontalGroup ("A", 18f), PropertyOrder (-2)]
        [HideLabel]
        public bool countRandom = false;

        [HideInInspector]
        public bool tagsUsed = false;
        
        [HorizontalGroup, HideLabel]
        [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerWorkshopProject.data.Keys")]
        public string key;
        
        [ShowIf ("tagsUsed")]
        [HorizontalGroup, DictionaryKeyDropdown ("@DataMultiLinkerWorkshopProject.tags")]
        public SortedDictionary<string, bool> tags;
        
        public string ToStringCount () =>
            countRandom && countMax != countMin ? $"{countMin}-{countMax}" : countMin.ToString ();

        public override string ToString ()
        {
            if (tagsUsed)
                return $"X{ToStringCount ()}: {tags.ToStringFormattedKeyValuePairs ()}";
            else
                return $"X{ToStringCount ()}: {key}";
        }
        
        public string ToStringExpanded (bool multiline = false, string linePrefix = null)
        {
            if (tagsUsed)
            {
                var subsystemsFiltered = DataTagUtility.GetKeysWithTags (DataMultiLinkerWorkshopProject.data, tags, limit: 6);
                return $"[ff]{ToStringCount ()} random:{(multiline ? "\n" : " ")}{subsystemsFiltered.ToStringFormatted (multiline, multilinePrefix: linePrefix)}";
            }
            else
                return $"[ff]{ToStringCount ()} fixed: {key}";
        }
        
        #region Editor
        #if UNITY_EDITOR

        [HorizontalGroup (20f), PropertyOrder (-1)]
        [Button (" ")]
        private void SwitchMode ()
        {
            tagsUsed = !tagsUsed;
            if (tagsUsed && tags == null)
                tags = new SortedDictionary<string, bool> { { string.Empty, true } };
        }

        #endif
        #endregion
    }
    
    
    
    
    
    
    
    
    
    

    [HideReferenceObjectPicker]
    public class DataBlockPartTagFilter
    {
        [HideLabel]
        [DictionaryDrawerSettings (KeyLabel = "Tag", ValueLabel = "Required", KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        [DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.tags")]
        public SortedDictionary<string, bool> tags;

        [YamlIgnore, HideInInspector]
        public string parentSocketGroup;
        
        [YamlIgnore, HideInInspector]
        public string parentUnitBlueprint;
        
        [YamlIgnore, HideInInspector]
        public string parentFactionBranch;

        private static bool AreFilterPreviewsVisible => DataMultiLinkerUnitPreset.Presentation.showFilterPreviews;

        [ShowIf ("AreFilterPreviewsVisible"), ShowInInspector, DisplayAsString (false)][HideLabel]
        private string filterPreview
        {
            get
            {
                var keysFromTags = DataHelperUnitEquipment.FilterPartPresetsForSocketGroup (parentSocketGroup, tags, parentUnitBlueprint, parentFactionBranch);
                var r1 = keysFromTags.ToStringFormatted (true, multilinePrefix: "- ");
                var report = $"{keysFromTags.Count} filtered parts:\n{r1}";
                return report;
            }
        }
    }

    [HideReferenceObjectPicker]
    public class DataBlockSubsystemTagFilter
    {
        [HideLabel]
        [DictionaryDrawerSettings (KeyLabel = "Tag", ValueLabel = "Required")]
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        public SortedDictionary<string, bool> tags;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockUnitPresetParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        [SuffixLabel ("@hierarchy"), HideLabel]
        public string key;
        
        [YamlIgnore, ReadOnly, HideInInspector]
        public string hierarchy;


        #region Editor
        #if UNITY_EDITOR

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;
        
        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            bool present = DataMultiLinkerUnitPreset.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }
        
        #endif
        #endregion
    }
    
    public class DataBlockUnitAnimationOverrides
    {
        public float primaryRotationSpeed;
        public float secondaryRotationSpeed;
        public float secondaryYawLimit;
        public float secondaryPitchLimit;
    }

    // Presets don't form an authoritative link with entities
    // This component is simply used to inspect where units came from and doesn't dictate their content
    
    [Persistent]
    public sealed class DataKeyUnitPreset : IComponent
    {
        public string s;
    }
    
    [Combat]
    public sealed class DataLinkUnitProximityEffect : IComponent
    {
        public float updateTimeLast;
        public int triggerCount;
        public DataBlockUnitPresetProximityEffect data;
    }

    public class DataBlockUnitPresetOption
    {
        [PropertyTooltip ("A random subsystem will be picked from this list of specific keys")]
        [ValueDropdown ("GetPresetKeys")]
        [OnValueChanged ("Sort")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true)]
        public List<string> keys = new List<string> ();
        
        [YamlIgnore, HideInInspector]
        public string socket;

        private IEnumerable<string> GetPresetKeys ()
        {
            if (!string.IsNullOrEmpty (socket))
                return DataHelperUnitEquipment.GetPartPresetsForSocket (socket, true);
            else
                return DataMultiLinkerPartPreset.data.Keys;
        }

        private void Sort ()
        {
            if (keys != null)
                keys.Sort ();
        }
    }

    public class DataBlockUnitPresetProximityEffect
    {
        public float distance = 3f;
        public bool triggerBreaksIteration = true;
        
        [DropdownReference (true)]
        public DataBlockFloat updateInterval;

        [DropdownReference (true)]
        public DataBlockInt triggerLimit;
        
        [DropdownReference]
        public List<ICombatUnitValidationFunction> checksUnit;
        
        [DropdownReference]
        public List<ICombatUnitValidationFunction> checksUnitTarget;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsUnit;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsUnitTarget;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitPresetProximityEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockUnitPresetEffects
    {
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnSpawn;
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnArrival;
        
        [DropdownReference]
        public DataBlockUnitPresetProximityEffect effectProximity;

        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnEjection;  
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnDestruction;
        
        #region Editor
        #if UNITY_EDITOR
        
            [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitPresetEffects () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    [LabelWidth (180f)]
    public class DataContainerUnitPreset : DataContainerWithText, IDataContainerTagged
    {
        [OnInspectorGUI ("DrawHeaderGUI", false)]
        [ShowIf ("IsCoreVisible")]
        public bool hidden = false;

        [ShowIf ("IsCoreVisible")] 
        public bool listed = true;
        
        [ShowIf ("IsCoreVisible")] 
        public bool branchIndependent = false;
        
        [ShowIf ("IsCoreVisible")] 
        public bool salvageExempted = false;
        
        [ShowIf ("IsCoreVisible")] 
        public bool bodyTagPinningExempted = false;
        
        [ShowIf ("IsCoreVisible")]
        [DropdownReference]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockUnitPresetParent ()")]
        public List<DataBlockUnitPresetParent> parents = new List<DataBlockUnitPresetParent> ();
        
        [ShowIf ("@IsCoreVisible && children != null && children.Count > 0")]
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        public List<string> children = new List<string> ();

        
        [DropdownReference (true)]
        [ShowIf ("IsCoreVisible")] 
        [ValueDropdown ("GetUnitBlueprints")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public string blueprint;
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("IsCoreProcessedVisible")]
        public string blueprintProcessed;
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textType;
        
        [YamlIgnore, ReadOnly]
        [HideLabel]
        [ShowIf ("IsCoreProcessedVisible")]
        public DataBlockStringNonSerialized textTypeProc;
        

        [DropdownReference (true)]
        [ShowIf ("IsCoreVisible")] 
        [ValueDropdown ("GetLiveryNames")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public string livery;
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("IsCoreProcessedVisible")]
        public string liveryProcessed;
        
        
        [DropdownReference (true)]
        [ShowIf ("IsCoreVisible")]
        [LabelText ("Default AI Behavior")]
        [ValueDropdown ("GetAIBehaviorKeys")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public string aiBehavior = "Flanker";
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("IsCoreProcessedVisible")]
        public string aiBehaviorProcessed;
        
        
        [DropdownReference (true)]
        [ShowIf ("IsCoreVisible")]
        [LabelText ("Default AI Targeting")]
        [ValueDropdown ("GetAITargetingProfileKeys")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public string aiTargeting = "default";
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("IsCoreProcessedVisible")]
        public string aiTargetingProcessed;
        

        [DropdownReference]
        [ShowIf ("AreTagsVisible")] 
        [ValueDropdown ("@DataMultiLinkerUnitPreset.tags")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public HashSet<string> tags;
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("AreTagsProcessedVisible")]
        public HashSet<string> tagsProcessed;

        
        [PropertyOrder (1), DropdownReference]
        [ShowIf ("IsEquipmentVisible")] 
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.SocketTag)]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public SortedDictionary<string, List<DataBlockPartTagFilter>> partTagPreferences;

        [PropertyOrder (1), YamlIgnore, ReadOnly]
        [ShowIf ("IsEquipmentProcessedVisible")]
        public SortedDictionary<string, List<DataBlockPartTagFilter>> partTagPreferencesProcessed;
        
        
        [PropertyOrder (10), DropdownReference]
        [ShowIf ("@IsEquipmentVisible")]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Socket)]
        [DictionaryDrawerSettings (KeyLabel = "Socket")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public SortedDictionary<string, DataBlockUnitPartOverride> parts;
        

        [PropertyOrder (11), YamlIgnore, ReadOnly]
        [ShowIf ("IsEquipmentProcessedVisible")]
        public SortedDictionary<string, DataBlockUnitPartOverride> partsProcessed;


        [ShowIf ("@IsOutputVisible && output != null")]
        [PropertyOrder (21)]
        [YamlIgnore]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        [FoldoutGroup ("Testing", false)]
        public Dictionary<string, DataBlockSavedPart> output;
        
        
        [PropertyOrder (15), DropdownReference (true)]
        [ShowIf ("IsCoreVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public DataBlockUnitAnimationOverrides animationOverrides;
        
        [PropertyOrder (15), YamlIgnore, ReadOnly]
        [ShowIf ("IsCoreProcessedVisible")]
        public DataBlockUnitAnimationOverrides animationOverridesProcessed;
        
        
        [PropertyOrder (16), DropdownReference]
        [ShowIf ("IsCoreVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        public DataBlockUnitPresetEffects effects;
        
        [PropertyOrder (16), YamlIgnore, ReadOnly]
        [ShowIf ("IsCoreProcessedVisible")]
        public DataBlockUnitPresetEffects effectsProc;
        
        
        #if UNITY_EDITOR

        [ShowIf ("@IsUsageVisible && unitGroupsFiltering != null && unitGroupsFiltering.Count > 0")]
        [PropertyOrder (22)]
        [LabelText ("Unit Groups / Tag matching")]
        [YamlIgnore, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<LinkToUnitGroup> unitGroupsFiltering;
        
        [ShowIf ("@IsUsageVisible && unitGroupsLinking != null && unitGroupsLinking.Count > 0")]
        [PropertyOrder (23)]
        [LabelText ("Unit Groups / Directly linking")]
        [YamlIgnore, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<LinkToUnitGroup> unitGroupsLinking;
        
        #endif

        public HashSet<string> GetTags (bool processed)
        {
            return processed ? tagsProcessed : tags;
        }
        
        public bool IsHidden () => hidden;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            #if UNITY_EDITOR
            RefreshInspectorData ();
            ResetUnitGroupRegister ();

            #endif
        }
        
        public override void ResolveText ()
        {
            if (textType != null)
                textType.s = DataManagerText.GetText (TextLibs.unitPresets, $"{key}__type");
        }

        public void OnAfterDeserializationEmbedded ()
        {
            blueprintProcessed = blueprint;
            liveryProcessed = livery;
            aiBehaviorProcessed = aiBehavior;
            aiTargetingProcessed = aiTargeting;
            tagsProcessed = tags;
            partTagPreferencesProcessed = partTagPreferences;
            partsProcessed = parts;
            animationOverridesProcessed = animationOverrides;
            effectsProc = effects;
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            var scenarios = DataMultiLinkerScenario.data;
            foreach (var kvp in scenarios)
            {
                var scenario = kvp.Value;
                if (scenario.unitPresetsProc == null)
                    continue;
                
                foreach (var kvp2 in scenario.unitPresetsProc)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetLink presetLink)
                    {
                        if (presetLink.preset != keyOld)
                            continue;

                        presetLink.preset = keyNew;
                        Debug.Log ($"Updated unit preset {kvp2.Key} in scenario {scenario.key} with new preset key {keyNew}");
                    }
                }
            }

            var unitGroups1 = DataMultiLinkerCombatUnitGroup.data;
            foreach (var kvp in unitGroups1)
            {
                var group = kvp.Value;
                if (group.unitPresets == null)
                    continue;
                
                foreach (var kvp2 in group.unitPresets)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetLink presetLink)
                    {
                        if (presetLink.preset != keyOld)
                            continue;

                        presetLink.preset = keyNew;
                        Debug.Log ($"Updated unit preset {kvp2.Key} in shared unit group {group.key} with new preset key {keyNew}");
                    }
                }
            }
        }
        
        public void OnFullRefreshRequired ()
        {
            if (DataMultiLinkerUnitPreset.Presentation.autoUpdateInheritance)
                DataMultiLinkerUnitPreset.ProcessRelated (this);
            else
            {
                #if UNITY_EDITOR
                OnPartsChanged ();
                #endif
            }
        }

        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (19)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerUnitPreset () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (textType != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.unitPresets, $"{key}__type", textType.s);
        }

        private static bool IsCoreVisible => DataMultiLinkerUnitPreset.Presentation.showCore;
        private static bool IsCoreProcessedVisible => DataMultiLinkerUnitPreset.Presentation.showCore && DataMultiLinkerUnitPreset.Presentation.showInheritance;

        private static bool AreTagsVisible => DataMultiLinkerUnitPreset.Presentation.showTags;
        private static bool AreTagsProcessedVisible => DataMultiLinkerUnitPreset.Presentation.showTags && DataMultiLinkerUnitPreset.Presentation.showInheritance;

        private static bool IsEquipmentVisible => DataMultiLinkerUnitPreset.Presentation.showEquipment;
        private static bool IsEquipmentProcessedVisible => DataMultiLinkerUnitPreset.Presentation.showEquipment && DataMultiLinkerUnitPreset.Presentation.showInheritance;

        private static bool IsOutputVisible => DataMultiLinkerUnitPreset.Presentation.showOutput;
        private static bool IsUsageVisible => DataMultiLinkerUnitPreset.Presentation.showUsage;
        private static bool AreFilterPreviewsVisible => DataMultiLinkerUnitPreset.Presentation.showFilterPreviews;
        
        private static IEnumerable<string> GetUnitBlueprints => DataMultiLinkerUnitBlueprint.data.Keys;
        private static IEnumerable<string> GetLiveryNames => DataMultiLinkerEquipmentLivery.data.Keys;
        private static IEnumerable<string> GetAIBehaviorKeys => DataShortcuts.ai.unitBehaviors;
        private static IEnumerable<string> GetAITargetingProfileKeys => DataMultiLinkerAITargetingProfile.data.Keys;
        
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
        private Color GetElementColorFull (int index) => Color.HSVToRGB (Mathf.Abs (0.6f - (float)index / 12f) % 1f, 0.25f, 1f);
        
        private static GUIStyle miniLabelStyleLeft;
        private static GUIStyle miniLabelStyleRight;
        private static GUIStyle boldLabelStyleCenter;

        private void DrawHeaderGUI ()
        {
            if (hidden)
                return;
            
            if (miniLabelStyleLeft == null)
            {
                miniLabelStyleLeft = new GUIStyle (EditorStyles.miniLabel);
                miniLabelStyleLeft.richText = true;
            }
            
            if (miniLabelStyleRight == null)
            {
                miniLabelStyleRight = new GUIStyle (EditorStyles.miniLabel);
                miniLabelStyleRight.richText = true;
                miniLabelStyleRight.alignment = TextAnchor.MiddleRight;
            }
            
            if (boldLabelStyleCenter == null)
            {
                boldLabelStyleCenter = new GUIStyle (EditorStyles.boldLabel);
                boldLabelStyleCenter.alignment = TextAnchor.MiddleCenter;
            }
            
            var rect = UnityEditor.EditorGUILayout.BeginVertical ();
            GUILayout.Label (" ", GUILayout.Height (18));
            UnityEditor.EditorGUILayout.EndVertical ();

            string textWeight = null;
            string textRange = null;
            string textMid = null;
            Color col = Color.white.WithAlpha (0.1f);

            int indexWeight = 0;
            int indexRange = 0;

            if (tagsProcessed != null)
            {
                if (tagsProcessed.Contains ("range_long"))
                {
                    textRange = "•••  long range";
                    indexRange = 3;
                }
                else if (tagsProcessed.Contains ("range_medium"))
                {
                    textRange = "••<color=black>•</color>  medium range";
                    indexRange = 2;
                }
                else if (tagsProcessed.Contains ("range_short"))
                {
                    textRange = "•<color=black>••</color>  short range";
                    indexRange = 1;
                }
                else 
                    textRange = "<color=black>•••</color>  unknown range";
                
                if (tagsProcessed.Contains ("weight_heavy"))
                {
                    textWeight = "heavy weight  •••";
                    indexWeight = 3;
                }
                else if (tagsProcessed.Contains ("weight_medium"))
                {
                    textWeight = "medium weight  ••<color=black>•</color>";
                    indexWeight = 2;
                }
                else if (tagsProcessed.Contains ("weight_light"))
                {
                    textWeight = "light weight  •<color=black>••</color>";
                    indexWeight = 1;
                }
                else 
                    textWeight = "unknown weight  <color=black>•••</color>";
            }
            
            if (indexRange == 1)
            {
                if (indexWeight == 1)
                {
                    textMid = "CHR";
                    col = Color.HSVToRGB (0.1f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                else if (indexWeight == 3)
                {
                    textMid = "DEF";
                    col = Color.HSVToRGB (0.25f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                        
            }
            else if (indexRange == 2)
            {
                if (indexWeight == 2)
                {
                    textMid = "ATK";
                    col = Color.HSVToRGB (0f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
            }
            else if (indexRange == 3)
            {
                if (indexWeight == 1)
                {
                    textMid = "RAN";
                    col = Color.HSVToRGB (0.5f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                else if (indexWeight == 3)
                {
                    textMid = "BRS";
                    col = Color.HSVToRGB (0.4f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
            }

            var gc = GUI.color;
            GUI.color = col;
            GUI.DrawTexture (rect.Expand (3), Texture2D.whiteTexture);
            GUI.color = gc;
            
            GUI.Label (rect.AlignLeft (120).AddX (8), textRange, miniLabelStyleLeft);
            GUI.Label (rect.AlignRight (120, true).AddX (-8), textWeight, miniLabelStyleRight);
            
            if (!string.IsNullOrEmpty (textMid))
                GUI.Label (rect, textMid, boldLabelStyleCenter);
            
            GUILayout.Space (8f);
        }
        
        [YamlIgnore]
        [PropertyOrder (20), ShowInInspector, ShowIf ("IsOutputVisible")]
        [FoldoutGroup ("Testing", false)]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        [InlineButtonClear]
        public static string factionBranch; // "branch_army";
        
        [YamlIgnore]
        [PropertyOrder (20), ShowInInspector, ShowIf ("IsOutputVisible")]
        [FoldoutGroup ("Testing", false)]
        [ValueDropdown ("@UnitEquipmentQuality.text")]
        [LabelText ("Quality (Fixed)"), InlineButtonClear]
        public static string equipmentQuality;
        
        [YamlIgnore]
        [PropertyOrder (20), ShowInInspector, ShowIf ("IsOutputVisible")]
        [FoldoutGroup ("Testing", false)]
        [ValueDropdown ("@DataMultiLinkerQualityTable.data.Keys")]
        [LabelText ("Quality (Table)"), InlineButtonClear]
        public static string equipmentQualityTableKey;
        
        [PropertyOrder (20), ShowIf ("IsOutputVisible")]
        [Button ("Test output", ButtonSizes.Large)]
        [FoldoutGroup ("Testing", false)]
        public void GenerateOutput ()
        {
            var factionData = DataMultiLinkerOverworldFactionBranch.GetEntry (factionBranch, false);
            int equipmentQualityInt = -1;
            
            for (int i = 0; i < UnitEquipmentQuality.text.Length; ++i)
            {
                var text = UnitEquipmentQuality.text[i];
                if (text == equipmentQuality)
                {
                    equipmentQualityInt = i;
                    break;
                }
            }
            
            output = UnitUtilities.CreatePersistentUnitDescription 
            (
                this, 
                1, 
                false, 
                false, 
                factionData, 
                null, 
                rating: equipmentQualityInt,
                equipmentQualityTableKey: equipmentQualityTableKey
            );
        }
        
        public struct LinkToUnitGroup
        {
            [HideLabel]
            [InlineButton ("TryOpeningUnitGroup", "Open")]
            public string key;

            private void TryOpeningUnitGroup ()
            {
                if (string.IsNullOrEmpty (key) || !DataMultiLinkerCombatUnitGroup.data.ContainsKey (key))
                    return;
            
                var linker = GameObject.FindObjectOfType<DataMultiLinkerCombatUnitGroup> ();
                if (linker == null)
                    return;

                UnityEditor.Selection.activeGameObject = linker.gameObject;
                linker.filter = key;
                linker.filterUsed = true;
                linker.ApplyFilter ();
            }
        }
        
        public void ResetUnitGroupRegister ()
        {
            if (unitGroupsFiltering == null)
                unitGroupsFiltering = new List<LinkToUnitGroup> ();
            else
                unitGroupsFiltering.Clear ();
            
            if (unitGroupsLinking == null)
                unitGroupsLinking = new List<LinkToUnitGroup> ();
            else
                unitGroupsLinking.Clear ();
        }

        public void OnUnitGroupFilterDetection (string unitGroupKey)
        {
            if (unitGroupsFiltering == null)
                return;

            foreach (var link in unitGroupsFiltering)
            {
                if (link.key == unitGroupKey)
                    return;
            }
            
            unitGroupsFiltering.Add (new LinkToUnitGroup { key = unitGroupKey });
        }
        
        public void OnUnitGroupLinkDetection (string unitGroupKey)
        {
            if (unitGroupsLinking == null)
                return;

            foreach (var link in unitGroupsLinking)
            {
                if (link.key == unitGroupKey)
                    return;
            }
            
            unitGroupsLinking.Add (new LinkToUnitGroup { key = unitGroupKey });
        }

        private void OnPartsChanged ()
        {
            RefreshInspectorData ();
        }

        public void RefreshInspectorData ()
        {
            if (parts != null)
            {
                foreach (var kvp in parts)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.socket = kvp.Key;
                        if (kvp.Value.preset != null && kvp.Value.preset is DataBlockPartSlotResolverTags tf)
                        {
                            var socketData = DataMultiLinkerPartSocket.GetEntry (kvp.Key, false);
                            if (tf.filters != null && socketData != null && socketData.tags != null && socketData.tags.Count > 0)
                            {
                                var socketTag = socketData.tags.First ();
                                foreach (var filter in tf.filters)
                                {
                                    if (filter != null)
                                    {
                                        filter.parentSocketGroup = socketTag;
                                        filter.parentFactionBranch = null;
                                        filter.parentUnitBlueprint = blueprintProcessed;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            if (partTagPreferences != null)
            {
                foreach (var kvp in partTagPreferences)
                {
                    if (kvp.Value != null)
                    {
                        foreach (var entry in kvp.Value)
                        {
                            if (entry != null)
                            {
                                entry.parentSocketGroup = kvp.Key;
                                entry.parentFactionBranch = null;
                                entry.parentUnitBlueprint = blueprintProcessed;
                            }
                        }
                    }
                }
            }

            /*
            if (!previewInitialized)
            {
                previewInitialized = true;

                previewCore = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.corePart, text = "Core" };
                previewS2 = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.leftOptionalPart, text = "Left arm" };
                previewS3 = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.rightOptionalPart, text = "Right arm" };
                previewS4 = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.leftEquipment, text = "Left wpn." };
                previewS5 = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.rightEquipment, text = "Right wpn." };
                previewS6 = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.corePart, text = "Legs" };
                previewBack = new DataBlockPartPreviewNew { preset = this, socket = LoadoutSockets.back, text = "Back" };
            }
            */
        }
        
        
        
        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.back)")]
        [ButtonGroup ("S"), Button ("+ Back"), GUIColor ("@GetSocketColor (LoadoutSockets.back)")]
        private void AddSocket0 () => AddSocket (LoadoutSockets.back);
        
        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.corePart)")]
        [ButtonGroup ("S"), Button ("+ Core"), GUIColor ("@GetSocketColor (LoadoutSockets.corePart)")]
        private void AddSocket1 () => AddSocket (LoadoutSockets.corePart);
        
        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.leftEquipment)")]
        [ButtonGroup ("S"), Button ("+ Left wpn."), GUIColor ("@GetSocketColor (LoadoutSockets.leftEquipment)")]
        private void AddSocket4 () => AddSocket (LoadoutSockets.leftEquipment);
        
        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.rightEquipment)")]
        [ButtonGroup ("S"), Button ("+ Right wpn."), GUIColor ("@GetSocketColor (LoadoutSockets.rightEquipment)")]
        private void AddSocket5 () => AddSocket (LoadoutSockets.rightEquipment);
        
        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.leftOptionalPart)")]
        [ButtonGroup ("S"), Button ("+ Left arm"), GUIColor ("@GetSocketColor (LoadoutSockets.leftOptionalPart)")]
        private void AddSocket2 () => AddSocket (LoadoutSockets.leftOptionalPart);
        
        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.rightOptionalPart)")]
        [ButtonGroup ("S"), Button ("+ Right arm"), GUIColor ("@GetSocketColor (LoadoutSockets.rightOptionalPart)")]
        private void AddSocket3 () => AddSocket (LoadoutSockets.rightOptionalPart);

        [ShowIf ("@IsSocketButtonVisible (LoadoutSockets.secondaryPart)")]
        [ButtonGroup ("S"), Button ("+ Legs"), GUIColor ("@GetSocketColor (LoadoutSockets.secondaryPart)")]
        private void AddSocket6 () => AddSocket (LoadoutSockets.secondaryPart);
        
        private Color GetSocketColor (string socket) => DataHelperEquipment.GetSocketEditorColor (socket);

        private bool IsSocketButtonVisible (string socket)
        {
            if (parts == null || string.IsNullOrEmpty (socket))
                return false;

            return !parts.ContainsKey (socket);
        }

        private void AddSocket (string socket)
        {
            if (parts == null || string.IsNullOrEmpty (socket))
                return;

            if (!parts.ContainsKey (socket))
            {
                parts.Add (socket, new DataBlockUnitPartOverride
                {
                    socket = socket, preset = new DataBlockPartSlotResolverKeys
                    {
                        keys = new List<string> { "preset_key" }
                    }
                });
            }
        }

        /*
        private const string bgPartsPreview = "partsPreview";
        private bool IsLayoutVisible => previewInitialized && IsEquipmentVisible && !ShowDictionaries;
        private bool previewInitialized = false;
        
        [GUIColor ("@GetElementColorFull (0)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("SCore", false)]
        private DataBlockPartPreviewNew previewCore;

        [GUIColor ("@GetElementColorFull (2)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("S2", false)]
        private DataBlockPartPreviewNew previewS2;
        
        [GUIColor ("@GetElementColorFull (3)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("S3", false)]
        private DataBlockPartPreviewNew previewS3;
        
        [GUIColor ("@GetElementColorFull (4)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("S4", false)]
        private DataBlockPartPreviewNew previewS4;
        
        [GUIColor ("@GetElementColorFull (5)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("S5", false)]
        private DataBlockPartPreviewNew previewS5;
        
        [GUIColor ("@GetElementColorFull (6)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("S6", false)]
        private DataBlockPartPreviewNew previewS6;
        
        [GUIColor ("@GetElementColorFull (1)")]
        [PropertyOrder (10), YamlIgnore, ShowInInspector, ShowIf ("IsLayoutVisible"), HideLabel, HideReferenceObjectPicker, BoxGroup ("SBack", false)]
        private DataBlockPartPreviewNew previewBack;
        */

        #endif
    }

    /*
    public class DataBlockPartPreviewNew
    {
        [HideInInspector]
        public DataContainerUnitPreset preset;
        
        [HideInInspector]
        public string socket;
        
        [DisplayAsString (TextAlignment.Left), HideLabel][InlineButton ("RemoveSocket", label: "-", ShowIf = "IsPreviewVisible")]
        public string text;
        
        [ShowInInspector, InlineProperty, ShowIf ("IsPreviewVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        private DataBlockUnitPartOverride part
        {
            get => preset.parts.TryGetValue (LoadoutSockets.rightOptionalPart, out var v) ? v : null;
            set
            {
                preset.parts[LoadoutSockets.rightOptionalPart] = value;
            }
        }

        [HideIf ("IsPreviewVisible")]
        [Button (" ", ButtonSizes.Gigantic), ButtonIconSdf (SdfIconType.Plus, 48), PropertyTooltip ("Add socket")]
        private void AddSocket ()
        {
            if (preset == null)
                return;

            if (preset.parts == null)
                preset.parts = new SortedDictionary<string, DataBlockUnitPartOverride> ();
            
            if (!preset.parts.ContainsKey (socket))
                preset.parts.Add (socket, new DataBlockUnitPartOverride { socket = socket });
            
            preset.OnFullRefreshRequired ();
        }
        
        // [ShowIf ("IsPreviewVisible")]
        // [Button (" ", ButtonSizes.Medium), ButtonIconSdf (SdfIconType.X, 24), PropertyTooltip ("Remove socket")]
        private void RemoveSocket ()
        {
            if (preset == null || preset.parts == null || !preset.parts.ContainsKey (socket))
                return;

            preset.parts.Remove (socket);
            preset.OnFullRefreshRequired ();
        }

        private bool IsPreviewVisible () => preset != null && preset.parts != null && preset.parts.ContainsKey (socket);
        private void OnFullRefreshRequired () => preset.OnFullRefreshRequired ();
    }
    */


    public class DataBlockPartSlotResolver
    {
        
    }
    
    public class DataBlockPartSlotResolverClear : DataBlockPartSlotResolver
    {
        
    }

    public class DataBlockPartSlotResolverKeys : DataBlockPartSlotResolver
    {
        [PropertyTooltip ("A random subsystem will be picked from this list of specific keys")]
        [ValueDropdown ("@AttributeExpressionUtility.GetStringsFromParentMethod ($property, \"GetPresetKeys\", 2)")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        [LabelText ("Part Presets")]
        public List<string> keys = new List<string> ();
    }
    
    public class DataBlockPartSlotResolverTags : DataBlockPartSlotResolver
    {
        public List<DataBlockPartTagFilter> filters = new List<DataBlockPartTagFilter> { new DataBlockPartTagFilter { tags = new SortedDictionary<string, bool> () } };

        // [PropertyTooltip ("A random subsystem will be picked from a list of all subsystems matching these tag requirements (if any exist)")]
        // [DictionaryKeyDropdown ("@DataMultiLinkerPartPreset.tags")]
        // [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        // public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();
    }
    
    
    
    
    [Serializable, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class DataBlockUnitPartOverride
    {
        [DropdownReference (true), GUIColor ("GetColor"), InlineProperty]
        [HideLabel]
        public DataBlockUnitLiveryPresetNode livery;

        [DropdownReference (true), GUIColor ("GetColor"), InlineProperty]
        [HideLabel]
        public DataBlockPartSlotResolver preset;
        
        [DropdownReference (true), GUIColor ("GetColor"), InlineProperty]
        [HideLabel]
        public DataBlockInt rating;
        
        [DropdownReference, GUIColor ("GetColor")]
        [ListDrawerSettings (ShowPaging = false, DefaultExpandedState = true)]
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Hardpoint)]
        [DictionaryDrawerSettings (KeyLabel = "Hardpoint")]
        [HideLabel]
        public SortedDictionary<string, DataBlockPresetSubsystem> systems;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitPartOverride () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        [YamlIgnore, HideInInspector] // [ShowInInspector, PropertyOrder (-1), ReadOnly, HideLabel, DisplayAsString (TextAlignment.Left)]
        public string socket;

        private IEnumerable<string> GetPresetKeys ()
        {
            if (!string.IsNullOrEmpty (socket))
                return DataHelperUnitEquipment.GetPartPresetsForSocket (socket, true);
            else
                return DataMultiLinkerPartPreset.data.Keys;
        }

        private Color GetColor () => DataHelperEquipment.GetSocketEditorColor (socket);
        
        #endif
        #endregion
    }
}

