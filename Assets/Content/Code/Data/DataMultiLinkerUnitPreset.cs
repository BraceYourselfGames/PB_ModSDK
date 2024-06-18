using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;

using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitPreset : DataMultiLinker<DataContainerUnitPreset>
    {
        [ShowInInspector][PropertyOrder (-1)]
        [FoldoutGroup ("Last generated units")]
        public static List<DataBlockUnitDescriptionDebug> unitsGenerated;

        public DataMultiLinkerUnitPreset ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;

            [ShowInInspector]
            public static bool showEquipment = true;

            [ShowInInspector]
            public static bool showOutput = true;

            [ShowInInspector]
            public static bool showUsage = true;
            
            [ShowInInspector]
            public static bool showFilterPreviews = true;
            
            [ShowInInspector]
            public static bool showTags = true;

            [ShowInInspector]
            public static bool showTagCollections = false;

            [ShowInInspector]
            [ShowIf ("showInheritance")]
            [InfoBox ("Warning: this mode triggers inheritance processing every time inheritable fields are modified. This is useful for instantly previewing changes to things like stat or inherited text, but it can cause data loss if you are not careful. Only currently modified config is protected from being reloaded, save if you switch to another config.", VisibleIf = "autoUpdateInheritance")]
            public static bool autoUpdateInheritance = false;
            
            [ShowInInspector]
            public static bool showInheritance = false;
        }

        [ShowInInspector][HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerUnitPreset.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerUnitPreset.Presentation.showTagCollections")]
        [ShowInInspector][ReadOnly]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitPreset.tags")]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();

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
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
            
            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);

            #if !PB_MODSDK
            DataHelperUnitEquipment.InvalidateLookups ();
            #endif
        }
        
        private static StringBuilder sb = new StringBuilder ();
        private static List<DataContainerUnitPreset> presetsUpdated = new List<DataContainerUnitPreset> ();

        public static void ProcessRelated (DataContainerUnitPreset origin)
        {
            if (origin == null)
                return;

            presetsUpdated.Clear ();
            presetsUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var preset = GetEntry (childKey);
                    if (preset != null)
                        presetsUpdated.Add (preset);
                }
            }
            
            foreach (var preset in presetsUpdated)
            {
                // Avoid localization refresh on origin
                if (preset != origin)
                    preset.OnAfterDeserialization (preset.key);
            }

            foreach (var preset in presetsUpdated)
                ProcessRecursiveStart (preset);
            
            foreach (var preset in presetsUpdated)
                Postprocess (preset);

            // Debug.Log ($"Updated {presetsUpdated.Count} presets: {presetsUpdated.ToStringFormatted (toStringOverride: (a) => a.key)}");
        }
        
        private static void ProcessRecursiveStart (DataContainerUnitPreset origin)
        {
            if (origin == null)
                return;

            origin.blueprintProcessed = null;
            origin.textTypeProc = null;
            origin.liveryProcessed = null;
            origin.aiBehaviorProcessed = null;
            origin.aiTargetingProcessed = null;
            origin.animationOverridesProcessed = null;

            if (origin.functionsProcessed != null)
                origin.functionsProcessed.Clear ();
            
            if (origin.tagsProcessed != null)
                origin.tagsProcessed.Clear ();

            if (origin.partTagPreferencesProcessed != null)
                origin.partTagPreferencesProcessed.Clear ();
            
            if (origin.partsProcessed != null)
                origin.partsProcessed.Clear ();

            if (origin.parents != null)
            {
                foreach (var parent in origin.parents)
                {
                    if (parent != null)
                        parent.hierarchy = string.Empty;
                }
            }

            ProcessRecursive (origin, origin, 0);
        }
        
        private static void ProcessRecursive (DataContainerUnitPreset current, DataContainerUnitPreset root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null part preset step or root unit preset reference while processing part preset hierarchy");
                return;
            }
            
            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered infinite dependency loop at depth level {depth} when processing unit preset {root.key}");
                return;
            }

            if (!string.IsNullOrEmpty (current.blueprint) && string.IsNullOrEmpty (root.blueprintProcessed))
                root.blueprintProcessed = current.blueprint;
            
            if (current.textType != null && !string.IsNullOrEmpty (current.textType.s))
                root.textTypeProc = current.textType;
            
            if (!string.IsNullOrEmpty (current.livery) && string.IsNullOrEmpty (root.liveryProcessed))
                root.liveryProcessed = current.livery;
            
            if (!string.IsNullOrEmpty (current.aiBehavior) && string.IsNullOrEmpty (root.aiBehaviorProcessed))
                root.aiBehaviorProcessed = current.aiBehavior;
            
            if (!string.IsNullOrEmpty (current.aiTargeting) && string.IsNullOrEmpty (root.aiTargetingProcessed))
                root.aiTargetingProcessed = current.aiTargeting;
            
            if (current.animationOverrides != null && root.animationOverridesProcessed == null)
                root.animationOverridesProcessed = current.animationOverrides;

            if (current.tags != null && current.tags.Count > 0)
            {
                if (root.tagsProcessed == null)
                    root.tagsProcessed = new HashSet<string> ();
                
                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProcessed.Contains (tag))
                        root.tagsProcessed.Add (tag);
                }
            }

            if (current.partTagPreferences != null && current.partTagPreferences.Count > 0)
            {
                if (root.partTagPreferencesProcessed == null)
                    root.partTagPreferencesProcessed = new SortedDictionary<string, List<DataBlockPartTagFilter>> ();

                var ptpRoot = root.partTagPreferencesProcessed;
                var ptpCurrent = current.partTagPreferences;
                
                foreach (var kvp in ptpCurrent)
                {
                    var socketTag = kvp.Key;
                    var listCurrent = kvp.Value;
                    
                    if (listCurrent == null || listCurrent.Count == 0)
                        continue;

                    List<DataBlockPartTagFilter> listRoot;
                    if (!ptpRoot.ContainsKey (socketTag))
                    {
                        listRoot = new List<DataBlockPartTagFilter> ();
                        ptpRoot.Add (socketTag, listRoot);
                    }
                    else
                    {
                        listRoot = ptpRoot[socketTag];
                        
                        // If list size doesn't match, it's safer to bail - hard to assume where to additively inject stuff
                        if (listRoot.Count != listCurrent.Count)
                            continue;
                    }

                    for (int i = 0; i < listCurrent.Count; ++i)
                    {
                        var filterCurrent = listCurrent[i];
                        if (filterCurrent == null || filterCurrent.tags == null || filterCurrent.tags.Count == 0)
                            continue;

                        DataBlockPartTagFilter filterRoot;
                        if (listRoot.Count <= i)
                        {
                            filterRoot = new DataBlockPartTagFilter { tags = new SortedDictionary<string, bool> () };
                            listRoot.Add (filterRoot);
                        }
                        else
                            filterRoot = listRoot[i];

                        foreach (var kvp2 in filterCurrent.tags)
                            filterRoot.tags[kvp2.Key] = kvp2.Value;
                    }
                }
            }

            if (current.functions != null && current.functions.Count > 0)
            {
                if (root.functionsProcessed == null)
                    root.functionsProcessed = new List<ICombatFunctionTargeted> ();

                foreach (var function in current.functions)
                {
                    if (function != null)
                        root.functionsProcessed.Add (function);
                }
            }

            if (current.parts != null && current.parts.Count > 0)
            {
                if (root.partsProcessed == null)
                    root.partsProcessed = new SortedDictionary<string, DataBlockUnitPartOverride> ();

                var ptsRoot = root.partsProcessed;
                var ptsCurrent = current.parts;

                foreach (var kvp in ptsCurrent)
                {
                    var socket = kvp.Key;
                    var partOverrideCurrent = kvp.Value;
                    
                    if (partOverrideCurrent == null)
                        continue;
                    
                    if (partOverrideCurrent.livery == null && partOverrideCurrent.preset == null && (partOverrideCurrent.systems == null || partOverrideCurrent.systems.Count == 0))
                        continue;

                    DataBlockUnitPartOverride partOverrideRoot;
                    if (!ptsRoot.ContainsKey (socket))
                    {
                        partOverrideRoot = new DataBlockUnitPartOverride ();
                        ptsRoot.Add (socket, partOverrideRoot);
                    }
                    else
                        partOverrideRoot = ptsRoot[socket];

                    if (partOverrideCurrent.livery != null && partOverrideRoot.livery == null)
                        partOverrideRoot.livery = partOverrideCurrent.livery;
                    
                    if (partOverrideCurrent.rating != null && partOverrideRoot.rating == null)
                        partOverrideRoot.rating = partOverrideCurrent.rating;
                    
                    if (partOverrideCurrent.preset != null && partOverrideRoot.preset == null)
                        partOverrideRoot.preset = partOverrideCurrent.preset;
                    
                    if (partOverrideCurrent.systems != null && partOverrideCurrent.systems.Count > 0)
                    {
                        foreach (var kvp2 in partOverrideCurrent.systems)
                        {
                            var hardpoint = kvp2.Key;
                            var blockCurrent = kvp2.Value;
                            
                            if (blockCurrent == null)
                                continue;
                            
                            DataBlockPresetSubsystem blockRoot = null;
                            if (partOverrideRoot.systems == null || !partOverrideRoot.systems.ContainsKey (hardpoint))
                            {
                                if (partOverrideRoot.systems == null)
                                    partOverrideRoot.systems = new SortedDictionary<string, DataBlockPresetSubsystem> ();
                                
                                blockRoot = new DataBlockPresetSubsystem ();
                                blockRoot.hardpointKey = hardpoint;
                                partOverrideRoot.systems.Add (hardpoint, blockRoot);
                            }
                            else
                                blockRoot = partOverrideRoot.systems[hardpoint];
                            
                            if (!string.IsNullOrEmpty (blockCurrent.livery) && string.IsNullOrEmpty (blockRoot.livery))
                                blockRoot.livery = blockCurrent.livery;

                            if (blockCurrent.flags != null && blockRoot.flags == null)
                                blockRoot.flags = blockCurrent.flags;
                            
                            if (blockCurrent.resolver != null)
                            {
                                var resolverCurrent = blockCurrent.resolver;
                                if (resolverCurrent is DataBlockSubsystemSlotResolverHardpoint)
                                {
                                    // Only fill if root is missing a resolver, no mutations possible
                                    if (blockRoot.resolver == null)
                                        blockRoot.resolver = new DataBlockSubsystemSlotResolverHardpoint ();
                                }
                                else if (resolverCurrent is DataBlockSubsystemSlotResolverKeys resolverCurrentKeys)
                                {
                                    if (blockRoot.resolver == null)
                                    {
                                        // If root resolver is empty, fill it with exact copy
                                        // This has to be split for editor since editor has a special index field used for tools that must be preserved
                                        #if UNITY_EDITOR
                                        blockRoot.resolver = new DataBlockSubsystemSlotResolverKeys { keys = new List<string> (resolverCurrentKeys.keys), index = resolverCurrentKeys.index };
                                        #else
                                        blockRoot.resolver = new DataBlockSubsystemSlotResolverKeys { keys = new List<string> (resolverCurrentKeys.keys) };
                                        #endif
                                    }
                                    else
                                    {
                                        // If root resolver is not empty, we must try to cast it to correct type to mutate it
                                        var resolverRootKeys = blockRoot.resolver as DataBlockSubsystemSlotResolverKeys;
                                        if (resolverRootKeys == null || resolverRootKeys.keys == null)
                                            Debug.LogWarning ($"Failed to mutate subsystem resolver on {root.key} with key-based resolver from {current.key} - inheritance hierarchy likely has resolver type mismatch");
                                        else
                                        {
                                            foreach (var blueprint in resolverCurrentKeys.keys)
                                            {
                                                if (!resolverRootKeys.keys.Contains (blueprint))
                                                    resolverRootKeys.keys.Add (blueprint);
                                            }
                                        }
                                    }
                                }
                                else if (resolverCurrent is DataBlockSubsystemSlotResolverTags resolverCurrentTags)
                                {
                                    if (blockRoot.resolver == null)
                                    {
                                        // If root resolver is empty, fill it with exact copy
                                        blockRoot.resolver = new DataBlockSubsystemSlotResolverTags { filter = new SortedDictionary<string, bool> (resolverCurrentTags.filter) };
                                    }
                                    else
                                    {
                                        // If root resolver is not empty, we must try to cast it to correct type to mutate it
                                        var resolverRootTags = blockRoot.resolver as DataBlockSubsystemSlotResolverTags;
                                        if (resolverRootTags == null || resolverRootTags.filter == null)
                                            Debug.LogWarning ($"Failed to mutate subsystem resolver on {root.key} with tag-based resolver from {current.key} - inheritance hierarchy likely has resolver type mismatch");
                                        else
                                        {
                                            foreach (var kvp3 in resolverCurrentTags.filter)
                                                resolverRootTags.filter[kvp3.Key] = kvp3.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Unit preset {root.key} fails to complete recursive processing in under 20 steps | Current step: {current.key}");
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
                    Debug.LogWarning ($"Unit preset {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Unit preset {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Unit preset {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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
        
        private static void Postprocess (DataContainerUnitPreset preset)
        {
            #if UNITY_EDITOR
            
            preset.RefreshInspectorData ();
            
            #endif
        }
        
        
        
        #if UNITY_EDITOR
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void GenerateOutput ()
        {
            string branchTagPrefix = "branch_";
            foreach (var kvp in data)
            {
                var preset = kvp.Value;
                if (preset.tags == null)
                    continue;
                
                if (preset.key.Contains ("temp_") || preset.key.Contains ("promot_"))
                    continue;

                bool branchFound = false;
                foreach (var tag in preset.tags)
                {
                    if (tag.StartsWith (branchTagPrefix))
                    {
                        branchFound = true;
                        break;
                    }
                }

                if (!branchFound)
                    continue;
                
                if (preset.partTagPreferences != null && preset.partTagPreferences.Count > 0)
                    preset.GenerateOutput ();
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void SetListedFlag ()
        {
            var prefix = "generic_";
            foreach (var kvp in data)
            {
                var preset = kvp.Value;
                preset.listed = preset.key.StartsWith (prefix);
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void DeleteRatingTags ()
        {
            Dictionary<string, int> tagsToRatings = new Dictionary<string, int>
            {
                { "rating_0", 0 },
                { "rating_1", 1 },
                { "rating_2", 2 },
                { "rating_3", 3 },
            };

            foreach (var kvp in data)
            {
                var preset = kvp.Value;
                if (preset.partTagPreferences != null && preset.partTagPreferences.Count > 0)
                {
                    foreach (var kvp2 in preset.partTagPreferences)
                    {
                        var socketTag = kvp2.Key;
                        var partFilters = kvp2.Value;

                        if (partFilters == null)
                            continue;

                        foreach (var partFilter in partFilters)
                        {
                            if (partFilter == null || partFilter.tags == null)
                                continue;

                            foreach (var kvp3 in tagsToRatings)
                            {
                                var tag = kvp3.Key;
                                if (partFilter.tags.ContainsKey (tag))
                                {
                                    Debug.Log ($"{preset.key} | Socket tag {socketTag} preferences | Using rating tag: {tag}");
                                    partFilter.tags.Remove (tag);
                                }
                            }
                        }
                    }
                }

                if (preset.parts != null && preset.parts.Count > 0)
                {
                    foreach (var kvp2 in preset.parts)
                    {
                        var socket = kvp2.Key;
                        var socketOverrideData = kvp2.Value;
                        if (socketOverrideData == null || socketOverrideData.preset == null)
                            continue;

                        if (socketOverrideData.preset is DataBlockPartSlotResolverTags resolverTags)
                        {
                            if (resolverTags.filters != null && resolverTags.filters.Count > 0)
                            {
                                foreach (var partFilterContainer in resolverTags.filters)
                                {
                                    if (partFilterContainer == null || partFilterContainer.tags == null)
                                        continue;
                                    
                                    foreach (var kvp3 in tagsToRatings)
                                    {
                                        var tag = kvp3.Key;
                                        if (partFilterContainer.tags.ContainsKey (tag))
                                        {
                                            Debug.Log ($"{preset.key} | Socket {socket} override | Using rating tag: {tag}");
                                            partFilterContainer.tags.Remove (tag);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void ReplaceRatingInPartKeys ()
        {
            Dictionary<string, int> suffixesToRatings = new Dictionary<string, int>
            {
                { "_r0", 0 },
                { "_r1", 1 },
                { "_r2", 2 },
                { "_r3", 3 },
            };

            Dictionary<string, int> deletionsToRatings = new Dictionary<string, int> ();
            Dictionary<string, string> deletionsToRenames = new Dictionary<string, string> ();

            foreach (var kvp in data)
            {
                var preset = kvp.Value;

                if (preset.parts != null && preset.parts.Count > 0)
                {
                    foreach (var kvp2 in preset.parts)
                    {
                        var socket = kvp2.Key;
                        var socketOverrideData = kvp2.Value;
                        if (socketOverrideData == null || socketOverrideData.preset == null)
                            continue;

                        if (socketOverrideData.preset is DataBlockPartSlotResolverKeys resolverKeys)
                        {
                            if (resolverKeys.keys != null && resolverKeys.keys.Count > 0)
                            {
                                deletionsToRatings.Clear ();
                                deletionsToRenames.Clear ();
                                
                                foreach (var kvp3 in suffixesToRatings)
                                {
                                    var ratingSuffix = kvp3.Key;
                                    foreach (var partPresetKeyOld in resolverKeys.keys)
                                    {
                                        if (partPresetKeyOld.EndsWith (kvp3.Key))
                                        {
                                            deletionsToRatings.Add (partPresetKeyOld, kvp3.Value);
                                            deletionsToRenames.Add (partPresetKeyOld, partPresetKeyOld.Replace (ratingSuffix, string.Empty));
                                        }
                                    }
                                }

                                foreach (var kvp3 in deletionsToRatings)
                                {
                                    var partPresetKeyOld = kvp3.Key;
                                    var partPresetKeyNew = deletionsToRenames[kvp3.Key];
                                    
                                    resolverKeys.keys.Remove (partPresetKeyOld);
                                    resolverKeys.keys.Add (partPresetKeyNew);
                                    
                                    int rating = kvp3.Value;
                                    if (rating != 1)
                                        socketOverrideData.rating = new DataBlockInt { i = kvp3.Value };
                                    Debug.Log ($"{preset.key} | Socket {socket} override | Detected preset using rating suffix, renaming: {partPresetKeyOld} to {partPresetKeyNew} | Rating: { rating }");
                                }
                            }
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogMissingPartKeys ()
        {
            foreach (var kvp in data)
            {
                var preset = kvp.Value;

                if (preset.parts != null && preset.parts.Count > 0)
                {
                    foreach (var kvp2 in preset.parts)
                    {
                        var socket = kvp2.Key;
                        var socketOverrideData = kvp2.Value;
                        if (socketOverrideData == null || socketOverrideData.preset == null)
                            continue;

                        if (socketOverrideData.preset is DataBlockPartSlotResolverKeys resolverKeys)
                        {
                            if (resolverKeys.keys != null && resolverKeys.keys.Count > 0)
                            {
                                foreach (var partPresetKey in resolverKeys.keys)
                                {
                                    var partPreset = DataMultiLinkerPartPreset.GetEntry (partPresetKey, false);
                                    if (partPreset == null)
                                    {
                                        Debug.Log ($"{preset.key} | Socket {socket} override | Bad preset key: {partPresetKey}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        #endif

        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void LogOverrideUse ()
        {
            var list = new List<string> ();
            foreach (var d in data)
            {
               var value = d.Value;
               if (value.parts == null || value.parts.Count == 0)
                   continue;
               
               list.Add ($"{d.Key}: {value.parts.Keys.ToStringFormatted ()}");
            }
            
            Debug.Log ($"Unit presets using part overrides: {list.Count}:\n{list.ToStringFormatted (true)}");
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void RemoveGradeAndEquipmentQualityTags ()
        {
            var suffixes = new HashSet<string>
            {
                "_0",
                "_1",
                "_2"
            };
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;
                
                if (preset == null || preset.tags == null)
                    continue;

                bool suffixFound = false;
                foreach (var suffix in suffixes)
                {
                    if (key.EndsWith (suffix))
                    {
                        suffixFound = true;
                        break;
                    }
                }

                if (!suffixFound)
                {
                    foreach (var tag in UnitGroupDifficulty.tags)
                    {
                        if (preset.tags.Contains (tag))
                        {
                            Debug.Log ($"{preset.key} | Preset contains grade tag {tag} but doesn't adhere to standard naming scheme");
                            break;
                        }
                    }
                    
                    continue;
                }

                foreach (var tag in UnitGroupDifficulty.tags)
                {
                    if (preset.tags.Contains (tag))
                    {
                        preset.tags.Remove (tag);
                        Debug.Log ($"{preset.key} | Removing grade tag {tag}");
                    }
                }

                if (preset.partTagPreferences != null)
                {
                    foreach (var kvp2 in preset.partTagPreferences)
                    {
                        var socketTag = kvp2.Key;
                        var partFilters = kvp2.Value;

                        if (partFilters == null)
                            continue;

                        foreach (var partFilter in partFilters)
                        {
                            if (partFilter == null || partFilter.tags == null)
                                continue;

                            for (int i = 0; i < 4; ++i)
                            {
                                var tag = $"rating_{i}";
                                if (partFilter.tags.ContainsKey (tag))
                                {
                                    partFilter.tags.Remove (tag);
                                    Debug.Log ($"{preset.key} | Removing equipment tag {tag} under socket tag {socketTag}");
                                }
                            }
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void RemoveUnusedTags ()
        {
            var unusedTagPrefixes = new HashSet<string>
            {
                "branch_"
            };
            
            var tagsToRemove = new List<string> ();
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;
                
                if (preset == null || preset.tags == null)
                    continue;
                
                tagsToRemove.Clear ();

                foreach (var tag in preset.tags)
                {
                    foreach (var prefix in unusedTagPrefixes)
                    {
                        if (tag.StartsWith (prefix))
                        {
                            Debug.Log ($"{preset.key} | Removing unused tag {tag}");
                            tagsToRemove.Add (tag);
                            break;
                        }
                    }
                }
                
                foreach (var tag in tagsToRemove)
                    preset.tags.Remove (tag);
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void RemoveUnusedPartFilters ()
        {
            var unusedFilterPrefixes = new HashSet<string>
            {
                "part_melee"
            };
            
            var tagsToRemove = new List<string> ();
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;

                if (preset.partTagPreferences != null)
                {
                    foreach (var kvp2 in preset.partTagPreferences)
                    {
                        var socketTag = kvp2.Key;
                        var partFilters = kvp2.Value;

                        if (partFilters == null)
                            continue;

                        foreach (var partFilter in partFilters)
                        {
                            if (partFilter == null || partFilter.tags == null)
                                continue;
                                
                            tagsToRemove.Clear ();

                            foreach (var kvp3 in partFilter.tags)
                            {
                                var tag = kvp3.Key;
                                
                                foreach (var prefix in unusedFilterPrefixes)
                                {
                                    if (tag.StartsWith (prefix))
                                    {
                                        Debug.Log ($"{preset.key} | Removing unused part filter tag {tag} in socket tag {socketTag}");
                                        tagsToRemove.Add (tag);
                                        break;
                                    }
                                }
                            }
                
                            foreach (var tag in tagsToRemove)
                                partFilter.tags.Remove (tag);

                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void LogManufacturerUse ()
        {
            var socketsInBranches = new SortedDictionary<string, SortedDictionary<string, List<string>>> ();
            // List<string> manufacturerList = null;

            var branchPrefix = "branch_";
            var manufacturerPrefix = "manufacturer_";

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;

                if (preset.tags == null)
                    continue;

                foreach (var presetTag in preset.tags)
                {
                    if (!presetTag.StartsWith (branchPrefix))
                        continue;

                    var branchTag = presetTag;

                    if (preset.partTagPreferences == null)
                        continue;

                    foreach (var kvp2 in preset.partTagPreferences)
                    {
                        var socketTag = kvp2.Key;
                        var partFilters = kvp2.Value;

                        if (partFilters == null)
                            continue;

                        foreach (var partFilter in partFilters)
                        {
                            if (partFilter == null || partFilter.tags == null)
                                continue;
                            
                            foreach (var kvp3 in partFilter.tags)
                            {
                                var partTag = kvp3.Key;
                                if (!partTag.StartsWith (manufacturerPrefix))
                                    continue;

                                var manufacturerTag = partTag;
                                if (!socketsInBranches.ContainsKey (branchTag))
                                    socketsInBranches.Add (branchTag, new SortedDictionary<string, List<string>> ());

                                var manufacturersInSockets = socketsInBranches[branchTag];
                                if (!manufacturersInSockets.ContainsKey (socketTag))
                                    manufacturersInSockets.Add (socketTag, new List<string> ());
                                
                                var manufacturers = manufacturersInSockets[socketTag];
                                if (!manufacturers.Contains (manufacturerTag))
                                    manufacturers.Add (manufacturerTag);
                            }
                        }
                    }
                }
            }

            foreach (var kvp in socketsInBranches)
            {
                var branchTag = kvp.Key;
                var manufacturersInSockets = kvp.Value;
                Debug.Log ($"Branch {branchTag} | Socket tags: {manufacturersInSockets.Count}");

                foreach (var kvp2 in manufacturersInSockets)
                {
                    var socketTag = kvp2.Key;
                    var manufacturers = kvp2.Value;
                    manufacturers.Sort ();
                    Debug.Log ($"- {socketTag}: {manufacturers.ToStringFormatted ()}");
                }
            }
        }

        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void LogInheritedBlueprints ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;

                if (string.IsNullOrEmpty (preset.blueprint))
                {
                    Debug.Log ($"{key} has no direct blueprint | Inherited: {preset.blueprintProcessed}");
                }
            }
        }

        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void ReplacePartFilterTags ()
        {
            // var socketsInBranches = new SortedDictionary<string, SortedDictionary<string, List<string>>> ();
            // List<string> manufacturerList = null;
            
            var tagReplacementMap = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "body", new Dictionary<string, string>
                    {
                        { "manufacturer_01", "mnf_armor_01" },
                        { "manufacturer_02", "mnf_armor_02" },
                        { "manufacturer_03", "mnf_armor_03" }
                    }
                }
            };

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;

                if (preset.partTagPreferences == null)
                    continue;
                
                foreach (var kvp2 in preset.partTagPreferences)
                {
                    var socketTag = kvp2.Key;
                    var partFilters = kvp2.Value;
                    
                    if (partFilters == null)
                        continue;
                    
                    if (!tagReplacementMap.ContainsKey (socketTag))
                        continue;

                    var map = tagReplacementMap[socketTag];

                    foreach (var partFilter in partFilters)
                    {
                        if (partFilter == null || partFilter.tags == null)
                            continue;
                        
                        foreach (var kvp3 in map)
                        {
                            var partTagOld = kvp3.Key;
                            var partTagReplacement = kvp3.Value;
                            
                            if (partFilter.tags.ContainsKey (partTagOld))
                            {
                                bool required = partFilter.tags[partTagOld];
                                partFilter.tags.Remove (partTagOld);
                                partFilter.tags.Add (partTagReplacement, required);
                                Debug.Log ($"{preset.key} | {socketTag} | {partTagOld} -> {partTagReplacement}");
                            }
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void RemoveManufacturerTags ()
        {
            // var socketsInBranches = new SortedDictionary<string, SortedDictionary<string, List<string>>> ();
            // List<string> manufacturerList = null;
            
            var tagPrefix = "mnf_";
            var tagsToRemove = new List<string> ();

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;

                if (preset.partTagPreferences != null)
                {
                    foreach (var kvp2 in preset.partTagPreferences)
                    {
                        var socketTag = kvp2.Key;
                        var partFilters = kvp2.Value;

                        if (partFilters == null)
                            continue;

                        foreach (var partFilter in partFilters)
                        {
                            if (partFilter == null || partFilter.tags == null)
                                continue;
                                
                            tagsToRemove.Clear ();

                            foreach (var kvp3 in partFilter.tags)
                            {
                                var tag = kvp3.Key;
                                if (!tag.StartsWith (tagPrefix))
                                    continue;
                                
                                tagsToRemove.Add (tag);
                                Debug.Log ($"{preset.key} | {socketTag} | Removing tag {tag}");
                            }
                            
                            foreach (var tag in tagsToRemove)
                                partFilter.tags.Remove (tag);
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void MakeTestPresetsUnlisted ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;
                preset.listed = true;

                /*
                if (key.Contains ("test_") || key.Contains ("promo_") || key.Contains ("custom"))
                {
                    preset.listed = false;
                    if (!preset.hidden)
                    {
                        Debug.Log ($"{key}: not hidden");
                        if (key.Contains ("test_") || key.Contains ("promo_"))
                            preset.hidden = true;
                    }
                }
                */
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void MakeTestPresetsBranchIndependent ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var preset = kvp.Value;
                preset.listed = true;
                
                if (key.Contains ("test_") || key.Contains ("promo_") || key.Contains ("custom"))
                {
                    preset.branchIndependent = true;
                }
            }
        }
        
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void ClarifyEmptySlots ()
        {
            UpdatePresetsWithAction (ClarifyEmptySlotsInPreset);
        }
        
        private static void ClarifyEmptySlotsInPreset (DataContainerUnitPreset preset)
        {
            if (preset == null || preset.parts == null)
                return;

            foreach (var kvp in preset.parts)
            {
                var partOverride = kvp.Value;
                if (partOverride == null)
                    continue;

                if (partOverride.livery == null && partOverride.preset == null && partOverride.systems == null)
                {
                    Debug.LogWarning ($"{preset.key} | Clarified clearing intent of socket {kvp.Key}");
                    partOverride.preset = new DataBlockPartSlotResolverClear ();
                }
            }
        }

        private static void UpdatePresetsWithAction (Action<DataContainerUnitPreset> action)
        {
            if (action == null)
                return;
            
            foreach (var kvp in data)
                action.Invoke (kvp.Value);

            foreach (var kvp in DataMultiLinkerScenario.data)
            {
                var scenario = kvp.Value;
                if (scenario.unitPresetsProc == null)
                    continue;
                
                foreach (var kvp2 in scenario.unitPresetsProc)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                        action.Invoke (presetEmbedded.preset);
                }
            }
            
            foreach (var kvp in DataMultiLinkerCombatUnitGroup.data)
            {
                var unitGroup = kvp.Value;
                if (unitGroup.unitPresets == null)
                    continue;
                
                foreach (var kvp2 in unitGroup.unitPresets)
                {
                    if (kvp2.Value is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                        action.Invoke (presetEmbedded.preset);
                }
            }
        }


        /*
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void ReplaceRatingZero ()
        {
            // var socketsInBranches = new SortedDictionary<string, SortedDictionary<string, List<string>>> ();
            // List<string> manufacturerList = null;

            var presetKeyReplacements = new Dictionary<string, string> ();

            foreach (var kvp in data)
            {
                var preset = kvp.Value;
                if (kvp.Key.Contains ("vhc"))
                    continue;

                bool training = false;
                if (preset.partsUpdated != null)
                {
                    foreach (var kvp2 in preset.partsUpdated)
                    {
                        var partOverride = kvp2.Value;
                        if (partOverride == null || partOverride.partPresetKeys == null)
                            continue;

                        presetKeyReplacements.Clear ();
                        foreach (var partPresetKey in partOverride.partPresetKeys)
                        {
                            if (partPresetKey.EndsWith ("_r0"))
                                presetKeyReplacements.Add (partPresetKey, partPresetKey.Replace ("_r0", "_r1"));
                        }

                        foreach (var kvp3 in presetKeyReplacements)
                        {
                            partOverride.partPresetKeys.Remove (kvp3.Key);
                            partOverride.partPresetKeys.Add (kvp3.Value);
                            Debug.LogWarning ($"{preset.key} / {kvp2.Key} | Replacing R0 with R1:\n{kvp3.Key} -> {kvp3.Value}");
                        }
                    }
                }
            }
        }
        */
        
        /*
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void ReplaceRatingZeroWithTrainingFlag ()
        {
            // var socketsInBranches = new SortedDictionary<string, SortedDictionary<string, List<string>>> ();
            // List<string> manufacturerList = null;
            
            var tagRatingZero = "rating_0";
            var tagRatingOne = "rating_1";

            foreach (var kvp in data)
            {
                var preset = kvp.Value;

                bool training = false;
                if (preset.partTagPreferences != null)
                {
                    foreach (var kvp2 in preset.partTagPreferences)
                    {
                        var partFilters = kvp2.Value;
                        if (partFilters == null)
                            continue;

                        foreach (var partFilter in partFilters)
                        {
                            if (partFilter == null || partFilter.tags == null || !partFilter.tags.ContainsKey (tagRatingZero))
                                continue;

                            bool value = partFilter.tags[tagRatingZero];
                            if (!value)
                                continue;
                            
                            partFilter.tags.Remove (tagRatingZero);
                            partFilter.tags.Add (tagRatingOne, true);
                            training = true;
                            break;
                        }
                    }
                }
                
                if (!training)
                    continue;
                
                Debug.LogWarning ($"{preset.key} | Replacing R0 with training flag | Part tag preferences:\n{preset.partTagPreferences.ToStringFormattedKeyValuePairs (true, (a) => a.ToStringFormatted (toStringOverride: (b) => b.tags.ToStringFormattedKeyValuePairs ()))}");
                preset.training = true;
            }
        }
        */

        /*
        
        [Button (ButtonSizes.Large), PropertyOrder (-10)]
        public void Upgrade ()
        {
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                container.subsystemTagPreferences = new SortedDictionary<string, List<DataBlockSubsystemTagPreference>> ();
                
                if (container.subsystemArchetypeOverides != null)
                {
                    Debug.LogWarning ($"Moving {container.subsystemArchetypeOverides.Count} archetype overrides on unit preset {container.key}");
                    foreach (var kvp2 in container.subsystemArchetypeOverides)
                    {
                        var hardpointTag = kvp2.Key;
                        var list = new List<DataBlockSubsystemTagPreference> ();
                        container.subsystemTagPreferences.Add (hardpointTag, list);

                        var archetypes = kvp2.Value;
                        foreach (var archetype in archetypes)
                        {
                            var tagPreferences = new SortedDictionary<string, bool> ();
                            tagPreferences.Add (archetype, true);
                            list.Add (new DataBlockSubsystemTagPreference { tags = tagPreferences });
                        }
                    }
                    
                    container.subsystemArchetypeOverides = null;
                }
            }
        }
        */
    }
}