using System;
using System.Collections.Generic;
using System.Text;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using PhantomBrigade.Functions.Equipment;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Equipment]
    public sealed class DataKeyPartPreset : IComponent
    {
        [EntityIndex]
        public string s;
    }
    
    [Equipment]
    public sealed class DataLinkPartPreset : IComponent
    {
        public DataContainerPartPreset data;
    }

    
    [HideReferenceObjectPicker]
    public class DataBlockSubsystemSlotFlags
    {
        public bool fused = false;
    }
    
    public class DataBlockSubsystemSlotResolver
    {
        
    }
    
    public class DataBlockSubsystemSlotResolverHardpoint : DataBlockSubsystemSlotResolver
    {
        
    }
    
    public class DataBlockSubsystemSlotResolverKeys : DataBlockSubsystemSlotResolver
    {
        #if UNITY_EDITOR
        [YamlIgnore, HideInInspector]
        [PropertyRange (0, "@keys != null ? (keys.Count - 1) : 0")]
        [LabelText ("Key Index (Preview)")]
        public int index = 0;
        #endif
        
        [PropertyTooltip ("A random subsystem will be picked from this list of specific keys")]
        [ValueDropdown ("@AttributeExpressionUtility.GetStringsFromParentMethod ($property, \"GetBlueprintNames\", 2)")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public List<string> keys = new List<string> ();
    }
    
    public class DataBlockSubsystemSlotResolverTags : DataBlockSubsystemSlotResolver
    {
        [PropertyTooltip ("A random subsystem will be picked from a list of all subsystems matching these tag requirements (if any exist)")]
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();
    }
    
    public class DataBlockSubsystemSlotResolverRules : DataBlockSubsystemSlotResolver
    {
        
    }

    [Flags]
    public enum SubsystemSlotRating
    {
        None = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 4
    }

    [Serializable][HideReferenceObjectPicker] 
    public class DataBlockPresetSubsystem
    {
        [DropdownReference (true)]
        [ValueDropdown ("GetLiveryNames")]
        public string livery;
        
        [DropdownReference (true)]
        public DataBlockSubsystemSlotFlags flags;

        [DropdownReference (true)]
        [LabelText ("@GetResolverText ()")]
        public DataBlockSubsystemSlotResolver resolver;

        [YamlIgnore, HideInInspector]
        public string hardpointKey;

        public string GetSpecificBlueprint ()
        {
            #if UNITY_EDITOR
            if (resolver is DataBlockSubsystemSlotResolverKeys resolverKeys && resolverKeys.keys != null && resolverKeys.keys.Count > 1)
            {
                var index = resolverKeys.index.IsValidIndex (resolverKeys.keys) ? resolverKeys.index : 0;
                return resolverKeys.keys[index];
            }
            else
                return GetBlueprint ();
            #else
            return GetBlueprint ();
            #endif
        }
        
        private static List<string> subsystemsKeysFiltered = new List<string> ();
        private static List<DataContainerSubsystem> subsystemsFiltered = new List<DataContainerSubsystem> ();
        
        public string GetBlueprint ()
        {
            var result = string.Empty;
            
            if (string.IsNullOrEmpty (hardpointKey) || resolver == null)
                return result;

            if (resolver is DataBlockSubsystemSlotResolverHardpoint)
            {
                var list = DataMultiLinkerSubsystem.GetSubsystemKeysWithHardpoint (hardpointKey);
                if (list == null || list.Count == 0)
                {
                    Debug.LogWarning ($"Failed to find a subsystem for hardpoint {hardpointKey} using any blueprint cleared for that hardpoint\nNot a single subsystem using such hardpoint was found");
                    return string.Empty;
                }
                
                result = list.GetRandomEntry ();
            }
            else if (resolver is DataBlockSubsystemSlotResolverKeys resolverKeys)
            {
                var keys = resolverKeys.keys;
                if (keys == null || keys.Count == 0)
                {
                    Debug.LogWarning ($"Failed to find a subsystem for hardpoint {hardpointKey} using a blueprint list as it was null or empty");
                    return result;
                }
                    
                result = keys.GetRandomEntry ();
            }
            else if (resolver is DataBlockSubsystemSlotResolverTags resolverTags)
            {
                var filter = resolverTags.filter;
                if (filter == null || filter.Count == 0)
                {
                    Debug.LogWarning ($"Failed to find a subsystem for hardpoint {hardpointKey} using tag filter: null or empty tag filter provided");
                    return result;
                }

                var subsystemsFromTags = DataTagUtility.GetContainersWithTags (DataMultiLinkerSubsystem.data, resolverTags.filter);
                if (subsystemsFromTags.Count > 0)
                {
                    subsystemsKeysFiltered.Clear ();
                    foreach (var entry in subsystemsFromTags)
                    {
                        var subsystemFromTag = entry as DataContainerSubsystem;
                        if (subsystemFromTag.hardpointsProcessed.Contains (hardpointKey))
                            subsystemsKeysFiltered.Add (subsystemFromTag.key);
                    }
                        
                    if (subsystemsKeysFiltered.Count > 0)
                        result = subsystemsKeysFiltered.GetRandomEntry ();
                    else
                        Debug.LogWarning ($"Failed to find a subsystem for hardpoint {hardpointKey} using following tag filter (filtering by hardpoint left nothing:\n{filter.ToStringFormattedKeyValuePairs ()}");
                }
                else
                    Debug.LogWarning ($"Failed to find a subsystem for hardpoint {hardpointKey} using following tag filter (no matches discovered):\n{filter.ToStringFormattedKeyValuePairs ()}");
            }
            else if (resolver is DataBlockSubsystemSlotResolverRules resolverRules)
            {
                /*
                var rules = resolverRules.rules;
                if (rules == null || rules.Count == 0)
                {
                    Debug.LogWarning ($"Failed to find a subsystem for hardpoint {hardpointKey} using rules filter: null or empty rules list provided");
                    return result;
                }

                // Start with a set of subsystems that are eligible for a given hardpoint.
                // That immediately cuts off systems that would never qualify, giving us a small precomputed set
                var subsystemsForHardpoint = DataMultiLinkerSubsystem.GetSubsystemsWithHardpoint (hardpointKey);
                
                // Copy to our own reused collection, as we'll be modifying that list and would corrupt precomputed set if we don't copy
                subsystemsFiltered.Clear ();
                subsystemsFiltered.AddRange (subsystemsForHardpoint);
                
                // Start iterating over rules and trimming stuff
                */
            }

            return result;
        }

        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPresetSubsystem () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private System.Action callbackOnResolverChange;

        // private bool IsBlueprintIndexVisible =>
        //     DataMultiLinkerPartPreset.Presentation.showPredictedStats && !fillFromHardpoint && blueprints != null && blueprints.Count > 1;
        
        private IEnumerable<string> GetLiveryNames () => DataMultiLinkerEquipmentLivery.data.Keys;
        
        private IEnumerable<string> GetBlueprintNames ()
        {
            if (string.IsNullOrEmpty (hardpointKey))
                return DataMultiLinkerSubsystem.data.Keys;
            else
                return DataHelperUnitEquipment.GetSubsystemsForHardpoint (hardpointKey);
        }

        public void SetInspectorData (System.Action callbackOnResolverChange)
        {
            this.callbackOnResolverChange = callbackOnResolverChange;
        }

        private void OnResolverChange ()
        {
            callbackOnResolverChange?.Invoke ();
        }

        private static string resolverLabelHardpoint = "Fill From Hardpoint";
        private static string resolverLabelKeys = "Fill From Keys";
        private static string resolverLabelTags = "Fill From Tags";
        private static string resolverLabelRules = "Fill From Rules";

        private string GetResolverText ()
        {
            if (resolver == null)
                return null;

            if (resolver is DataBlockSubsystemSlotResolverHardpoint)
                return resolverLabelHardpoint;

            if (resolver is DataBlockSubsystemSlotResolverKeys)
                return resolverLabelKeys;

            if (resolver is DataBlockSubsystemSlotResolverTags)
                return resolverLabelTags;
            
            if (resolver is DataBlockSubsystemSlotResolverRules)
                return resolverLabelRules;

            return null;
        }
                
        #endif
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockEquipmentTextName
    {
        [ToggleLeft]
        public bool split = true;
        
        [YamlIgnore]
        [HideLabel]
        public string s;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockEquipmentTextDesc
    {
        [ToggleLeft]
        public bool split = true;
        
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string s;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockEquipmentTextFromHardpoint
    {
        public bool suffix = true;
        
        [InlineButtonClear]
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        public string hardpointOverride = string.Empty;
    }
    

    [HideReferenceObjectPicker]
    public class DataBlockPartPresetParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        [SuffixLabel ("@hierarchyProperty"), HideLabel]
        public string key;

        [YamlIgnore, ReadOnly, HideInInspector]
        private string hierarchyProperty => DataMultiLinkerPartPreset.Presentation.showHierarchy ? hierarchy : string.Empty;
        
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

            bool present = DataMultiLinkerPartPreset.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }
        
        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockPartGenerationStep
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [DropdownReference]
        public List<IPartGenCheck> checks;
        
        [BoxGroup, HideLabel]
        public IPartGenStep step;

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPartGenerationStep () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataBlockFromRatingName : DataBlockFromRating
    {

        [YamlIgnore]
        [HideLabel]
        public string s;
    }
    
    public class DataBlockFromRatingDesc : DataBlockFromRating
    {
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string s;
    }
    
    public class DataBlockFromRating
    {
        [LabelText ("Min/Max Rating"), HorizontalGroup]
        public int min;
        
        [HideLabel, HorizontalGroup (0.4f)]
        public int max;
        
        public bool IsPassed (int rating)
        {
            if (rating >= min && rating <= max)
                return true;

            return false;
        }
    }

    public class WorkshopItemData
    {
        public int progressRequired => progressLimit != null ? Mathf.Max (1, progressLimit.i) : 1;
        
        [DropdownReference (true)]
        public DataBlockInt progressLimit;
        
        [DropdownReference (true)]
        public DataBlockFloat inputResourcesViaStats;

        [DropdownReference, ListDrawerSettings (CustomAddFunction = "@new DataBlockResourceCost ()")]
        public List<DataBlockResourceCost> inputResources;
        
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> basePartRequirements;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public WorkshopItemData () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [LabelWidth (180f)]
    public class DataContainerPartPreset : DataContainerWithText, IDataContainerTagged
    {
        [YamlIgnore, ReadOnly]
        [ShowIf ("@IsUIVisible && !string.IsNullOrEmpty (groupMainKey)")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        [LabelText ("Group (Main)")]
        public string groupMainKey;
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("@IsCoreVisible && groupFilterKeys != null && groupFilterKeys.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        [LabelText ("Groups (Filtering)")]
        public List<string> groupFilterKeys;
        
        [ShowIf ("IsCoreVisible"), ToggleLeft]
        public bool hidden = false;

        [ShowIf ("IsCoreVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockPartPresetParent ()")]
        [DropdownReference]
        public List<DataBlockPartPresetParent> parents = new List<DataBlockPartPresetParent> ();
        
        [ShowIf ("IsCoreVisible")]
        [YamlIgnore, LabelText ("Children"), ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public List<string> children = new List<string> ();

        [ShowIf ("IsCoreVisible")]
        [ValueDropdown ("GetLiveryNames")]
        [DropdownReference]
        public string livery = null;
        
        
        [ShowIf ("IsCoreVisible")]
        [ValueDropdown ("GetLiveryNames")]
        [DropdownReference]
        public DataBlockFromRating ratingRange;
        
        [ShowIf ("@IsCoreVisible && IsInheritanceVisible && ratingRange != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockFromRating ratingRangeProcessed;
        
        
        [ShowIf ("IsCoreVisible")]
        [DropdownReference (true), HideLabel] 
        public DataBlockComment comment;

        [ShowIf ("IsUIVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockEquipmentTextName textName;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible && textNameProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockEquipmentTextName textNameProcessed;
        
        [ShowIf ("IsUIVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public DataBlockEquipmentTextDesc textDesc;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible && textDescProcessed != null")]
        [YamlIgnore, ReadOnly]
        public DataBlockEquipmentTextDesc textDescProcessed;

        [ShowIf ("IsUIVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public SortedDictionary<string, DataBlockFromRatingName> textNameFromRating;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible && textNameFromRatingProcessed != null")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockFromRatingName> textNameFromRatingProcessed;

        [ShowIf ("IsUIVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference (true)]
        public SortedDictionary<string, DataBlockFromRatingDesc> textDescFromRating;
        
        [ShowIf ("@IsUIVisible && IsInheritanceVisible && textDescFromRatingProcessed != null")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockFromRatingDesc> textDescFromRatingProcessed;

        
        [ShowIf ("AreSocketsVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        [ListDrawerSettings (ShowPaging = false, DefaultExpandedState = true)]
        [DropdownReference]
        public HashSet<string> sockets = new HashSet<string> ();
        
        [ShowIf ("@AreSocketsVisible && IsInheritanceVisible")]
        [ValueDropdown ("@DataMultiLinkerPartSocket.data.Keys")]
        [ListDrawerSettings (ShowPaging = false, DefaultExpandedState = true)]
        [YamlIgnore, ReadOnly]
        public HashSet<string> socketsProcessed = new HashSet<string> ();

        
        [ShowIf ("AreTagsVisible")] 
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [ValueDropdown ("@DataMultiLinkerPartPreset.tags")]
        [DropdownReference]
        public HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@AreTagsVisible && IsInheritanceVisible")]
        [ValueDropdown ("@DataMultiLinkerPartPreset.tags")]
        [ReadOnly, YamlIgnore]
        public HashSet<string> tagsProcessed = new HashSet<string> ();
        
        
        [PropertyOrder (20)]
        [ShowIf ("IsGenVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference]
        public SortedDictionary<int, WorkshopItemData> workshopInfo;
        
        [PropertyOrder (20)]
        [ShowIf ("@IsInheritanceVisible")]
        [ReadOnly, YamlIgnore]
        [HideDuplicateReferenceBox]
        public SortedDictionary<int, WorkshopItemData> workshopInfoProc;
        
        
        [PropertyOrder (20), ShowInInspector]
        [ShowIf ("IsGenVisible")]
        [OnValueChanged ("OnFullRefreshRequired", true)]
        [DropdownReference]
        public List<IPartGenStep> genSteps;
        
        [PropertyOrder (20), ShowInInspector]
        [ShowIf ("@IsGenVisible && IsInheritanceVisible")]
        [ReadOnly, YamlIgnore]
        [HideDuplicateReferenceBox]
        public List<IPartGenStep> genStepsProcessed;


        public HashSet<string> GetTags (bool processed)
        {
            return processed ? tagsProcessed : tags;
        }
        
        public bool IsHidden () => hidden;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);
            
            var partPresets = DataMultiLinkerPartPreset.data;
            foreach (var kvp in partPresets)
            {
                var partPreset = kvp.Value;
                if (partPreset.parents == null || partPreset.parents.Count == 0) 
                    continue;

                foreach (var link in partPreset.parents)
                {
                    if (link.key == keyOld)
                    {
                        link.key = keyNew;
                        Debug.Log ($"Updated parent in part preset {kvp.Key} with key: {keyOld} -> {keyNew}");
                    }
                }
            }

            var unitPresets = DataMultiLinkerUnitPreset.data;
            foreach (var kvp in unitPresets)
            {
                var unitPreset = kvp.Value;
                if (unitPreset.parts == null) 
                    continue;

                foreach (var kvp2 in unitPreset.parts)
                {
                    var partOverride = kvp2.Value;
                    if (partOverride.preset == null)
                        continue;

                    if (partOverride.preset is DataBlockPartSlotResolverKeys resolverKeys)
                    {
                        var keys = resolverKeys.keys;
                        if (keys != null && keys.Contains (keyOld))
                        {
                            keys.Remove (keyOld);
                            keys.Add (keyNew);
                            Debug.Log ($"Updated unit preset {unitPreset.key} with part preset key {keyOld} -> {keyNew}");
                        }
                    }
                }
            }
            
            var unitBlueprints = DataMultiLinkerUnitBlueprint.data;
            foreach (var kvp in unitBlueprints)
            {
                var unitBlueprint = kvp.Value;
                if (unitBlueprint.sockets == null)
                    continue;

                foreach (var kvp2 in unitBlueprint.sockets)
                {
                    var value = kvp2.Value;
                    if (value.presetDefault != keyOld)
                        continue;

                    value.presetDefault = keyNew;
                    Debug.Log ($"Updated unit blueprint {unitBlueprint.key} socket {kvp2.Key} with part preset key {keyOld} -> {keyNew}");
                }
            }
            
            var unitGroups = DataMultiLinkerCombatUnitGroup.data;
            foreach (var kvp in unitGroups)
            {
                var unitGroup = kvp.Value;
                if (unitGroup.unitPresets == null)
                    continue;
                
                foreach (var kvp2 in unitGroup.unitPresets)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                    {
                        if (presetEmbedded.preset == null)
                            continue;

                        var parts = presetEmbedded.preset.parts;
                        if (parts == null)
                            continue;
                        
                        foreach (var kvp3 in parts)
                        {
                            var partOverride = kvp3.Value;
                            if (partOverride.preset == null)
                                continue;

                            if (partOverride.preset is DataBlockPartSlotResolverKeys resolverKeys)
                            {
                                var keys = resolverKeys.keys;
                                if (keys != null && keys.Contains (keyOld))
                                {
                                    keys.Remove (keyOld);
                                    keys.Add (keyNew);
                                    Debug.Log ($"Updated unit preset {kvp2.Key} in unit group {unitGroup.key} with part preset key {keyOld} -> {keyNew}");
                                }
                            }
                        }
                    }
                }
            }

            var rewards = DataMultiLinkerOverworldReward.data;
            foreach (var kvp in rewards)
            {
                var reward = kvp.Value;
                if (reward.parts == null)
                    continue;
                
                foreach (var part in reward.parts)
                {
                    if (part == null || part.tagsUsed)
                        continue;

                    if (part.keys != null && part.keys.Contains (keyOld))
                    {
                        part.keys.Remove (keyOld);
                        part.keys.Add (keyNew);
                        Debug.Log ($"Updated reward {reward.key} reward with part preset key {keyOld} -> {keyNew}");
                    }
                }
            }
        }

        private static List<DataContainerEquipmentGroup> groupsFound = new List<DataContainerEquipmentGroup> ();
        private static HashSet<string> tagCacheTemporary = new HashSet<string> ();

        public void UpdateGroups ()
        {
            if (hidden)
                return;

            bool groupsPresent = groupFilterKeys != null;
            if (groupsPresent)
                groupFilterKeys.Clear ();
            
            groupsFound.Clear ();
            tagCacheTemporary.Clear ();

            if (genStepsProcessed != null)
            {
                foreach (var step in genStepsProcessed)
                {
                    if (step is AddHardpoints stepHardpoint && stepHardpoint.subsystemsInitial != null && stepHardpoint.subsystemsInitial.Count > 0)
                    {
                        foreach (var subsystemKey in stepHardpoint.subsystemsInitial)
                        {
                            var subsystem = DataMultiLinkerSubsystem.GetEntry (subsystemKey, false);
                            if (subsystem == null || subsystem.tagsProcessed == null)
                                continue;
                            
                            foreach (var tag in subsystem.tagsProcessed)
                            {
                                if (!string.IsNullOrEmpty (tag) && !tagCacheTemporary.Contains (tag))
                                    tagCacheTemporary.Add (tag);
                            }
                        }
                    }
                }
            }

            foreach (var kvp in DataMultiLinkerEquipmentGroup.data)
            {
                var group = kvp.Value;
                if (!group.parts)
                    continue;
                
                if (group.tagsSubsystem != null && group.tagsSubsystem.Count > 0)
                {
                    bool match = true;
                    foreach (var kvp2 in group.tagsSubsystem)
                    {
                        var tag = kvp2.Key;
                        var required = kvp2.Value;
                        var present = tagCacheTemporary.Contains (tag);

                        if (required != present)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                }
                
                if (group.tagsPartPreset != null && group.tagsPartPreset.Count > 0)
                {
                    bool match = true;
                    foreach (var kvp2 in group.tagsPartPreset)
                    {
                        var tag = kvp2.Key;
                        var required = kvp2.Value;
                        var present = tagsProcessed.Contains (tag);

                        if (required != present)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                }
                
                groupsFound.Add (group);
            }

            int groupsCount = groupsFound.Count;
            if (groupsCount == 0)
                return;
            
            if (groupsCount > 1)
                groupsFound.Sort ((x, y) => x.priority.CompareTo (y.priority));

            foreach (var group in groupsFound)
            {
                if (group.visibleInName && string.IsNullOrEmpty (groupMainKey))
                    groupMainKey = group.key;
                
                if (group.visibleInFilters || group.visibleAsPerk)
                {
                    if (!groupsPresent)
                    {
                        groupsPresent = true;
                        groupFilterKeys = new List<string> ();
                    }
                    
                    groupFilterKeys.Add (group.key);
                }
            }
        }

        public void SortGenSteps ()
        {
            // SortGenSteps (genSteps);
            SortGenSteps (genStepsProcessed);
        }
        
        private void SortGenSteps (List<IPartGenStep> steps)
        {
            if (steps == null || steps.Count <= 1)
                return;

            steps.Sort (CompareGenStepsForSorting); 
            
            for (int index = steps.Count - 1; index >= 0; index -= 1)
            {
                if (steps[index] != null)
                    break;
                
                steps.RemoveAt (index);
            }
        }
        
        private int CompareGenStepsForSorting (IPartGenStep step1, IPartGenStep step2)
        {
            if (step1 == null && step2 == null) { return 0; }
            if (step1 == null) { return 1; }
            if (step2 == null) { return -1; }
            return step1.GetPriority().CompareTo(step2.GetPriority());
        }

        #if !PB_MODSDK
        public void IssueToPlayer (int level, int rating)
        {
	        if (!Application.isPlaying || !Contexts.sharedInstance.persistent.hasDataKeySave)
		        return;

	        var playerBasePersistent = IDUtility.playerBasePersistent;
	        if (playerBasePersistent == null)
		        return;
	        
	        var partEntity = UnitUtilities.CreatePartEntityFromPreset (this, level: level, rating: rating);
	        if (partEntity == null)
		        return;
            
	        var liveryKey = DataMultiLinkerEquipmentLivery.data.GetRandomKey ();
	        partEntity.AddDataKeyEquipmentLivery (liveryKey);
	        EquipmentUtility.AttachPartToInventory (partEntity, playerBasePersistent, true, true);
            
            if (CIViewBaseCustomizationNav.ins.IsEntered ())
                CIViewBaseCustomizationNav.ins.RefreshForInventory (playerBasePersistent);

            if (CIViewBaseCustomizationSelector.ins.IsEntered ())
                CIViewBaseCustomizationSelector.ins.RedrawAnyEquipmentList ();
        }
        #endif
        
        public override void ResolveText ()
        {
            if (textName != null)
                textName.s = DataManagerText.GetText (TextLibs.equipmentPartPresets, $"{key}__name");
            
            if (textDesc != null)
                textDesc.s = DataManagerText.GetText (TextLibs.equipmentPartPresets, $"{key}__text");
            
            if (textNameFromRating != null)
            {
                foreach (var kvp in textNameFromRating)
                {
                    var block = kvp.Value;
                    if (block != null)
                        block.s = DataManagerText.GetText (TextLibs.equipmentPartPresets, $"{key}__name_{kvp.Key}");
                }
            }
            
            if (textDescFromRating != null)
            {
                foreach (var kvp in textDescFromRating)
                {
                    var block = kvp.Value;
                    if (block != null)
                        block.s = DataManagerText.GetText (TextLibs.equipmentPartPresets, $"{key}__text_{kvp.Key}");
                }
            }
        }
        
        public string GenerateToText (int rating, bool log)
        {
            if (genStepsProcessed == null || genStepsProcessed.Count == 0)
                return "No generator defined";

            Dictionary<string, GeneratedHardpoint> layout = new Dictionary<string, GeneratedHardpoint> ();
            foreach (var step in genStepsProcessed)
            {
                if (step != null)
                    step.Run (this, layout, rating, log);
            }

            var sb = new StringBuilder ();
            sb.Append ("Generated part preset: ");
            sb.Append (key);
            sb.Append (" | Rating: ");
            sb.Append (rating);
            sb.Append (" | Steps: ");
            sb.Append (genStepsProcessed.Count);

            sb.Append ("\n");
            sb.Append (EquipmentGenUtility.GetLayoutDescription (layout));

            EquipmentGenUtility.ReturnTempGenerationData (layout);

            var output = sb.ToString ();
            return output;
        }
        
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (textName != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentPartPresets, $"{key}__name", textName.s);
            
            if (textDesc != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentPartPresets, $"{key}__text", textDesc.s);
            
            if (textNameFromRating != null)
            {
                foreach (var kvp in textNameFromRating)
                {
                    var block = kvp.Value;
                    if (block != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentPartPresets, $"{key}__name_{kvp.Key}", block.s);
                }
            }
            
            if (textDescFromRating != null)
            {
                foreach (var kvp in textDescFromRating)
                {
                    var block = kvp.Value;
                    if (block != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.equipmentPartPresets, $"{key}__text_{kvp.Key}", block.s);
                }
            }
        }
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerPartPreset () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        private bool IsCoreVisible => DataMultiLinkerPartPreset.Presentation.showCore;
        private bool IsUIVisible => DataMultiLinkerPartPreset.Presentation.showUI;
        private bool AreSocketsVisible => DataMultiLinkerPartPreset.Presentation.showSockets;
        private bool AreSystemsVisible => DataMultiLinkerPartPreset.Presentation.showSystems;
        private bool AreTagsVisible => DataMultiLinkerPartPreset.Presentation.showTags;
        private bool AreAnalyticsVisible => DataMultiLinkerPartPreset.Presentation.showPredictedEffect;
        private bool IsGenVisible => DataMultiLinkerPartPreset.Presentation.showGeneration;
        private bool IsInheritanceVisible => DataMultiLinkerPartPreset.Presentation.showInheritance;
        private bool AreVisualsVisible => DataMultiLinkerPartPreset.Presentation.showVisuals;
        private bool IsWorkshopVisible => DataMultiLinkerPartPreset.Presentation.showWorkshop;
        private bool IsApplicationPlaying => Application.isPlaying;
        
        private bool systemsMisconfigured = false;
        private string systemsErrorReport = string.Empty;
        private StringBuilder systemsErrorBuilder = new StringBuilder ();
        
        private IEnumerable<string> GetLiveryNames () => DataMultiLinkerEquipmentLivery.data.Keys;
        
        [ShowIf ("AreVisualsVisible")]
        [Button ("Visualize (isolated)"), ButtonGroup ("Vis"), HideInPlayMode, PropertyOrder (-20)]
        public void VisualizeAndFocusIsolated () => 
            DataMultiLinkerPartPreset.VisualizeObject (this, false);
        
        [ShowIf ("AreVisualsVisible")]
        [Button ("Visualize (processed)"), ButtonGroup ("Vis"), HideInPlayMode, PropertyOrder (-20)]
        public void VisualizeAndFocusProcessed () => 
            DataMultiLinkerPartPreset.VisualizeObject (this, true);

        #if !PB_MODSDK
        [ShowIf ("@IsCoreVisible && !hidden && IsApplicationPlaying")]
        [Button ("Spawn R0"), ButtonGroup ("Spawn"), HideInEditorMode, PropertyOrder (-20)] 
        private void SpawnR0 () => IssueToPlayer (EquipmentUtility.debugGenerationLevel, 0);
        
        [ShowIf ("@IsCoreVisible && !hidden && IsApplicationPlaying")]
        [Button ("Spawn R1"), ButtonGroup ("Spawn"), HideInEditorMode, PropertyOrder (-20)] 
        private void SpawnR1 () => IssueToPlayer (EquipmentUtility.debugGenerationLevel, 1);
        
        [ShowIf ("@IsCoreVisible && !hidden && IsApplicationPlaying")]
        [Button ("Spawn R2"), ButtonGroup ("Spawn"), HideInEditorMode, PropertyOrder (-20)] 
        private void SpawnR2 () => IssueToPlayer (EquipmentUtility.debugGenerationLevel, 2);
        
        [ShowIf ("@IsCoreVisible && !hidden && IsApplicationPlaying")]
        [Button ("Spawn R3"), ButtonGroup ("Spawn"), HideInEditorMode, PropertyOrder (-20)] 
        private void SpawnR3 () => IssueToPlayer (EquipmentUtility.debugGenerationLevel, 3);
        #endif

        private void OnFullRefreshRequired ()
        {
            if (DataMultiLinkerPartPreset.Presentation.autoUpdateInheritance)
                DataMultiLinkerPartPreset.ProcessRelated (this);
        }

        private bool IsGenerationPossible => genStepsProcessed != null && genStepsProcessed.Count > 0;

        [YamlIgnore]
        private List<string> genStepsHardpointKeys = new List<string> ();

        private IEnumerable<string> GetGeneratedHardpointKeys ()
        {
            genStepsHardpointKeys.Clear ();

            if (genStepsProcessed != null)
            {
                foreach (var step in genStepsProcessed)
                {
                    if (step is AddHardpoints stepHardpoint)
                    {
                        if (stepHardpoint.hardpointsTargeted != null)
                        {
                            foreach (var hardpointKey in stepHardpoint.hardpointsTargeted)
                            {
                                if (!genStepsHardpointKeys.Contains (hardpointKey))
                                    genStepsHardpointKeys.Add (hardpointKey);
                            }
                        }
                    }
                }
            }

            return genStepsHardpointKeys;
        }

        [ShowIf ("@!hidden && IsGenVisible && IsGenerationPossible && !hidden")]
        [Button ("Generate To Text"), PropertyOrder (19)]
        private void GenerateToLog (int rating, bool log)
        {
            generatedOutput = GenerateToText (rating, log);
            Debug.Log (generatedOutput);
        }

        [ShowIf ("@!hidden && IsGenVisible && IsGenerationPossible && !string.IsNullOrEmpty (generatedOutput)")]
        [Button ("Generate"), PropertyOrder (20)]
        [ShowInInspector, InlineButtonClear, BoxGroup ("Generated", false), HideLabel, DisplayAsString (false)]
        private string generatedOutput;
        
        [ShowIf ("@!hidden && IsWorkshopVisible")]
        [Button ("Generate workshop project"), PropertyOrder (-2)]
        private void ToWorkshop ()
        {
            var data = DataMultiLinkerWorkshopProject.data;
            var keyWorkshop = key.Replace ("wpn_", "prt_item_");

            if (data.ContainsKey (keyWorkshop))
            {
                Debug.LogWarning ($"Key already present!");
                return;
            }
            
            var p = new DataContainerWorkshopProject ();
            p.hidden = false;
            p.textSourceName = new DataBlockWorkshopTextSourceName { key = key, source = WorkshopTextSource.Part };
            p.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = groupMainKey, source = WorkshopTextSource.Group };
            p.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = key, source = WorkshopTextSource.Part };
            
            p.tags = new HashSet<string> { "group_item" };
            p.icon = DataMultiLinkerEquipmentGroup.GetEntry (groupMainKey)?.icon;
            p.duration = new DataBlockFloat { f = 1f };
            p.inputResources = new List<DataBlockResourceCost>
            {
                new DataBlockResourceCost { key = ResourceKeys.supplies, amount = 1 },
                new DataBlockResourceCost { key = ResourceKeys.componentsR2, amount = 1 },
                new DataBlockResourceCost { key = ResourceKeys.componentsR3, amount = 1 }
            };

            p.outputParts = new List<DataBlockWorkshopPart> { new DataBlockWorkshopPart { count = 1, key = key, tags = null } };
            p.variantLinkSecondary = new DataBlockWorkshopVariantLink { key = "prt_item_ratings_01" };
            
            data.Add (keyWorkshop, p);
            
            var linker = GameObject.FindObjectOfType<DataMultiLinkerWorkshopProject> ();
            if (linker != null)
                linker.ApplyFilter ();
        }

        #endif
    }
}

