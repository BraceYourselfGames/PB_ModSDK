using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerEquipmentGroup : DataMultiLinker<DataContainerEquipmentGroup> 
    {
        public DataMultiLinkerEquipmentGroup ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.equipmentGroups); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        [FoldoutGroup ("Utilities", false)]
        [ShowInInspector]
        public static SortedDictionary<string, List<string>> partPresetLookup = new SortedDictionary<string, List<string>> ();
        
        [FoldoutGroup ("Utilities", false)]
        [ShowInInspector]
        public static SortedDictionary<string, DataContainerEquipmentGroup> dataGenerated = new SortedDictionary<string, DataContainerEquipmentGroup> ();

        [FoldoutGroup ("Utilities", false)]
        [Button]
        public static void RefreshPartLookups ()
        {
            partPresetLookup.Clear ();
            foreach (var kvp in DataMultiLinkerPartPreset.data)
            {
                var preset = kvp.Value;
                if (preset.hidden)
                    continue;
                
                var presetKey = kvp.Key;
                preset.UpdateGroups ();

                foreach (var groupKey in preset.groupFilterKeys)
                {
                    if (!partPresetLookup.ContainsKey (groupKey))
                        partPresetLookup.Add (groupKey, new List<string> { presetKey });
                    else
                        partPresetLookup[groupKey].Add (presetKey);
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [ShowInInspector]
        public static SortedDictionary<string, List<string>> subsystemLookup = new SortedDictionary<string, List<string>> ();

        [FoldoutGroup ("Utilities", false)]
        [Button]
        public static void RefreshSubsystemLookups ()
        {
            subsystemLookup.Clear ();
            
            foreach (var kvp in DataMultiLinkerSubsystem.data)
            {
                var subsystem = kvp.Value;
                if (subsystem.hidden)
                    continue;

                var subsystemKey = kvp.Key;
                subsystem.UpdateGroups ();
                
                foreach (var groupKey in subsystem.groupFilterKeys)
                {
                    if (!subsystemLookup.ContainsKey (groupKey))
                        subsystemLookup.Add (groupKey, new List<string> { subsystemKey });
                    else
                        subsystemLookup[groupKey].Add (subsystemKey);
                }
            }
        }

        public static void OnAfterDeserialization ()
        {
            dataGenerated.Clear ();

            var hardpoints = DataMultiLinkerSubsystemHardpoint.data;
            foreach (var kvp in hardpoints)
            {
                var hardpointKey = kvp.Key;
                var hardpoint = kvp.Value;

                if (!hardpoint.editable || !hardpoint.group)
                    continue;

                var keyForGroup = $"hardpoint_{hardpointKey}";
                var g = new DataContainerEquipmentGroup ();
                g.key = keyForGroup;
                dataGenerated.Add (keyForGroup, g);

                g.priority = 0;
                g.type = "category";
                g.textName = hardpoint.textName;
                g.textDesc = hardpoint.textDesc;
                g.icon = hardpoint.icon;
                g.visibleAsPerk = false;
                g.visibleInName = false;
                g.visibleInFilters = true;
                g.visibleInDesc = false;
                g.parts = false;
                g.subsystems = false;
                g.hardpointSubsystem = hardpointKey;
            }
        }


        #if UNITY_EDITOR
        
        /*
        [FoldoutGroup ("Utilities", false)]
        [Button]
        public static void MigrateFromFeatures ()
        {
            foreach (var kvp in data)
            {
                var group = kvp.Value;
                group.visibleInDesc = group.visibleInInfo;
            }
            
            foreach (var kvp in DataMultiLinkerEquipmentFeature.data)
            {
                var feature = kvp.Value;
                if (feature.hidden)
                    continue;

                var key = $"part_{feature.key}";
                Debug.LogWarning ($"Migrating part feature {feature.key} as {key}");

                if (data.ContainsKey (key))
                {
                    Debug.LogWarning ($"Can't migrate, key already exists");
                    continue;
                }
                
                var group = new DataContainerEquipmentGroup ();
                data.Add (key, group);
                
                group.key = key;
                group.textName = feature.textName;
                group.textDesc = feature.textDesc;
                group.type = feature.type;
                group.icon = feature.icon;
                group.visibleAsPerk = true;
                group.visibleInFilters = feature.key.Contains ("spec");
                group.visibleInName = false;
                group.visibleInDesc = false;
                group.visibleInInfo = false;
                group.priority = group.visibleInFilters ? 20 : 30;
                group.parts = true;
                group.subsystems = false;
                group.tagsSubsystem = new SortedDictionary<string, bool> { { feature.key, true } };
            }
        }
        */

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button]
        public void ConvertOldGroups ()
        {
            var tagConfigs = DataMultiLinkerEquipmentTag.data;
            
            foreach (var kvp in tagConfigs)
            {
                var tag = kvp.Key;
                var config = kvp.Value;
                
                if (config == null || config.blocks == null)
                    continue;

                if (config.blocks.ContainsKey (EquipmentTagContexts.partCategory))
                {
                    var block = config.blocks[EquipmentTagContexts.partCategory];
                    if (block != null)
                    {
                        var key = $"part_{tag}";
                        if (data.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Skipping migration of config {key} as such config already exists");
                            continue;
                        }

                        Debug.Log ($"Migrated part group config {key} for tag {tag} | Name: {block.textShort}\nDesc: {block.textLong}");
                        data.Add (key, new DataContainerPartGroup
                        {
                            key = key,
                            textName = block.textShort,
                            textDesc = block.textLong,
                            icon = block.spriteName?.name,
                            parts = true,
                            subsystems = false,
                            tags = new SortedDictionary<string, bool> { { tag, true } }
                        });
                    }
                }

                if (config.blocks.ContainsKey (EquipmentTagContexts.subsystemCategory))
                {
                    var block = config.blocks[EquipmentTagContexts.subsystemCategory];
                    if (block != null)
                    {
                        var key = $"subsystem_{tag}";
                        if (data.ContainsKey (key))
                        {
                            Debug.LogWarning ($"Skipping migration of config {key} as such config already exists");
                            continue;
                        }

                        Debug.Log ($"Migrated subsystem group config {key} for tag {tag} | Name: {block.textShort}\nDesc: {block.textLong}");
                        data.Add (key, new DataContainerPartGroup
                        {
                            key = key,
                            textName = block.textShort,
                            textDesc = block.textLong,
                            icon = block.spriteName?.name,
                            parts = false,
                            subsystems = true,
                            tags = new SortedDictionary<string, bool> { { tag, true } }
                        });
                    }
                }
            }
        }
        */
        
        /*
        [FoldoutGroup ("Utilities", false)]
        [Button]
        public void ConvertToNewTagCollections ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var config = kvp.Value;

                if (config.tags != null && config.tags.Count > 0)
                    config.tagsSubsystem = config.tags;

                config.tags = null;
            }
        }
        */

        /*
        [FoldoutGroup ("Utilities", false)]
        [ShowInInspector]
        public static List<string> tagsUnrecognized = new List<string> ();

        [FoldoutGroup ("Utilities", false)]
        [Button]
        public void ConvertOldPartPresetText ()
        {
            tagsUnrecognized.Clear ();
            
            var historyKey = nameof (DataContainerSubsystem);
            var history = DataLinkerHistory.data.keyReplacementGroups;
            var historyPartPresets = history.ContainsKey (historyKey) ? history[historyKey].keyReplacements : null;
            var historyUsed = historyPartPresets != null;
            
            var tagConfigs = DataMultiLinkerEquipmentTag.data;
            var partPresets = DataMultiLinkerPartPreset.data;
            var hardpointInternal = "internal_main_equipment"; 
            
            foreach (var kvp in tagConfigs)
            {
                var tagDataKey = kvp.Key;
                var tagData = kvp.Value;
                
                if (tagData == null || tagData.blocks == null)
                    continue;

                if (!tagData.blocks.ContainsKey (EquipmentTagContexts.partModel))
                    continue;

                var block = tagData.blocks[EquipmentTagContexts.partModel];
                if (block == null)
                    continue;

                if (historyUsed)
                {
                    int loops = 0;
                    bool success = true;
                    
                    while (DataLinkerHistory.IsKeyReplacementAvailable (historyKey, tagDataKey, out string keyNew))
                    {
                        Debug.LogWarning ($"Encountered part preset rename, changing key: {tagDataKey} -> {keyNew}");
                        tagDataKey = keyNew;
                        loops += 1;

                        if (loops > 10)
                        {
                            Debug.LogWarning ($"Rename has too many steps, breaking out, history likely has a loop | Origin: {kvp.Key}");
                            success = false;
                            break;
                        }
                    }
                    
                    if (!success)
                        continue;
                }

                DataContainerPartPreset partPreset = null;
                foreach (var kvp2 in partPresets)
                {
                    var partPresetCandidate = kvp2.Value;
                    
                    if (partPresetCandidate.systems == null || partPresetCandidate.systems.Count == 0 || !partPresetCandidate.systems.ContainsKey (hardpointInternal))
                        continue;

                    var link = partPresetCandidate.systems[hardpointInternal];
                    if (link == null || link.tagsUsed || link.fillFromHardpoint || link.blueprints == null || link.blueprints.Count > 1)
                        continue;

                    var blueprintKey = link.blueprints[0];
                    if (blueprintKey == tagDataKey)
                    {
                        partPreset = partPresetCandidate;
                        break;
                    }
                }
                
                if (partPreset == null)
                {
                    tagsUnrecognized.Add (tagDataKey);
                    Debug.LogWarning ($"Failed to find part preset with blueprint {tagDataKey} to transfer UI data to");
                    continue;
                }

                Debug.Log ($"Moving UI data to part preset {partPreset.key} based on blueprint {tagDataKey} | Name: {block.textShort}\nDesc: {block.textLong}");

                if (!string.IsNullOrEmpty (block.textShort))
                {
                    if (partPreset.textName == null)
                        partPreset.textName = new DataBlockEquipmentTextName { s = block.textShort };
                    else
                        partPreset.textName.s = block.textShort;
                }

                if (!string.IsNullOrEmpty (block.textLong))
                {
                    if (partPreset.textDesc == null)
                        partPreset.textDesc = new DataBlockEquipmentTextDesc { s = block.textLong };
                    else
                        partPreset.textDesc.s = block.textLong;
                }
            }
        }
        */

        #endif
    }
    
    public static class DataHelperEquipment
    {
        public static readonly string fallbackPartModelName = Txt.Get(TextLibs.uiBase, "inventory_info_unknown");
        public static readonly string fallbackPartModelDesc = string.Empty; // "No model description available";
        
        public static readonly string fallbackGroupName = Txt.Get(TextLibs.uiBase, "inventory_info_unknown");
        public static readonly string fallbackGroupDesc = "No group description available";
        public static readonly string fallbackGroupIcon = "s_icon_l32_part_arch_all";

        private static StringBuilder sb = new StringBuilder ();

        public static DataContainerPartPreset GetLastInheritor (DataContainerPartPreset partPreset)
        {
            if (partPreset == null)
                return null;
        
            int loops = 0;
            while (partPreset.children != null && partPreset.children.Count > 0)
            {
                var partPresetChild = DataMultiLinkerPartPreset.GetEntry (partPreset.children[0], false);
                if (partPresetChild == null)
                    break;

                partPreset = partPresetChild;
                loops += 1;
                    
                // Just in case
                if (loops > 100)
                    break;
            }

            return partPreset;
        }
        
        public static DataContainerSubsystem GetLastInheritor (DataContainerSubsystem subsystem)
        {
            if (subsystem == null)
                return null;
        
            int loops = 0;
            while (subsystem.children != null && subsystem.children.Count > 0)
            {
                var subsystemChild = DataMultiLinkerSubsystem.GetEntry (subsystem.children[0], false);
                if (subsystemChild == null)
                    break;

                subsystem = subsystemChild;
                loops += 1;
                    
                // Just in case
                if (loops > 100)
                    break;
            }

            return subsystem;
        }

        #if !PB_MODSDK
        public static string GetPartModelName (this EquipmentEntity part, bool appendSuffixes = true)
        {
            if (!part.isPart || !part.hasDataLinkPartPreset)
                return fallbackPartModelName;

            var preset = part.dataLinkPartPreset.data;
            int rating = part.hasRating ? part.rating.i : 1;
            return GetPartModelName (preset, rating, appendSuffixes: appendSuffixes);
        }
        #endif

        public static string GetPartModelName (this DataContainerPartPreset preset, int rating = 1, bool descendToChildren = false, bool appendSuffixes = true)
        {
            if (preset == null)
                return fallbackPartModelName;
            
            if (descendToChildren)
                preset = GetLastInheritor (preset);
            
            string text = null;

            if (preset.textNameProcessed != null && !string.IsNullOrEmpty (preset.textNameProcessed.s))
                text = preset.textNameProcessed.s;

            if (preset.textNameFromRatingProcessed != null)
            {
                foreach (var kvp in preset.textNameFromRatingProcessed)
                {
                    var block = kvp.Value;
                    if (block != null && !string.IsNullOrEmpty (block.s) && block.IsPassed (rating))
                    {
                        text = block.s;
                        break;
                    }
                }
            }
            
            if (string.IsNullOrEmpty (text))
                text = fallbackPartModelName;
            
            if (appendSuffixes)
            {
                var suffixes = DataShortcuts.ui.equipmentRatingSuffixes;
                if (suffixes != null && suffixes.TryGetValue (rating, out var suffix))
                    text += suffix;
            }

            return text;
        }

        #if !PB_MODSDK
        public static string GetPartModelDesc (this EquipmentEntity part)
        {
            var preset = part.dataLinkPartPreset.data;
            int rating = part.hasRating ? part.rating.i : 1;
            return GetPartModelDesc (preset, rating);
        }
        #endif

        public static string GetPartModelDesc (this DataContainerPartPreset preset, int rating = 1, bool descendToChildren = false, bool appendGroupMain = false, bool appendGroupsSecondary = false)
        {
            if (preset == null)
                return fallbackPartModelDesc;
            
            if (descendToChildren)
                preset = GetLastInheritor (preset);
            
            string text = null;

            if (preset.textDescProcessed != null && !string.IsNullOrEmpty (preset.textDescProcessed.s))
                text = preset.textDescProcessed.s;

            if (preset.textDescFromRatingProcessed != null)
            {
                foreach (var kvp in preset.textDescFromRatingProcessed)
                {
                    var block = kvp.Value;
                    if (block != null && !string.IsNullOrEmpty (block.s) && block.IsPassed (rating))
                    {
                        text = block.s;
                        break;
                    }
                }
            }
            
            if (string.IsNullOrEmpty (text))
                text = fallbackPartModelDesc;

            if (appendGroupMain || appendGroupsSecondary)
            {
                sb.Clear ();
                sb.Append (text);

                string groupKeyAppended = null;
                bool groupKeyFiltering = false;

                if (appendGroupMain && !string.IsNullOrEmpty (preset.groupMainKey))
                {
                    var group = DataMultiLinkerEquipmentGroup.GetEntry (preset.groupMainKey, false);
                    if (group != null && !string.IsNullOrEmpty (group.textName) && !string.IsNullOrEmpty (group.textDesc))
                    {
                        if (sb.Length > 0)
                            sb.Append ("\n\n");
                        sb.Append (group.textName);
                        sb.Append ("[aa]: ");
                        sb.Append (group.textDesc);
                        sb.Append ("[ff]");

                        groupKeyAppended = preset.groupMainKey;
                        groupKeyFiltering = true;
                    }
                }

                if (appendGroupsSecondary && preset.groupFilterKeys != null)
                {
                    foreach (var groupKey in preset.groupFilterKeys)
                    {
                        if (groupKeyFiltering && string.Equals (groupKey, groupKeyAppended, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        var group = DataMultiLinkerEquipmentGroup.GetEntry (groupKey, false);
                        if (group == null || !group.visibleInDesc || string.IsNullOrEmpty (group.textName) || string.IsNullOrEmpty (group.textDesc))
                            continue;

                        if (sb.Length > 0)
                            sb.Append ("\n\n");
                        sb.Append (group.textName);
                        sb.Append ("[aa]: ");
                        sb.Append (group.textDesc);
                        sb.Append ("[ff]");
                    }
                }
                
                text = sb.ToString ();
            }

            return text;
        }

        #if !PB_MODSDK
        public static List<string> GetEquipmentGroupKeys (this EquipmentEntity part)
        {
            if (!part.isPart || !part.hasDataLinkPartPreset)
                return null;
            
            var preset = part.dataLinkPartPreset.data;
            return preset.groupFilterKeys;
        }

        public static DataContainerEquipmentGroup GetEquipmentGroup (this EquipmentEntity part)
        {
            if (!part.isPart || !part.hasDataLinkPartPreset)
                return null;
            
            var preset = part.dataLinkPartPreset.data;
            if (string.IsNullOrEmpty (preset.groupMainKey))
                return null;
            
            // This method works if we want filter groups to be the same as name group, but we want more precise control over groups getting into name
            // if (preset.groupFilterKeys == null || preset.groupFilterKeys.Count == 0)
            //     return null;
            // var groupKey = preset.groupFilterKeys.FirstOrDefault ();
            // var group = DataMultiLinkerEquipmentGroup.GetEntry (groupKey);

            var group = DataMultiLinkerEquipmentGroup.GetEntry (preset.groupMainKey);
            return group;
        }
        #endif
        
        private static string partPresetTagBody = "body_part";
        
        public static string GetEquipmentGroupName (DataContainerEquipmentGroup group, DataContainerPartPreset preset = null)
        {
            if (group == null || string.IsNullOrEmpty (group.textName))
                return fallbackGroupName;

            if (preset != null && preset.tagsProcessed != null && preset.tagsProcessed.Contains (partPresetTagBody))
            {
                var sockets = preset.socketsProcessed;
                string socketSuffix = null;
                    
                if (sockets.Contains (LoadoutSockets.corePart))
                    socketSuffix = Txt.Get(TextLibs.uiBase, "inventory_socket_suffix_torso");
                else if (sockets.Contains (LoadoutSockets.secondaryPart))
                    socketSuffix = Txt.Get(TextLibs.uiBase, "inventory_socket_suffix_legs");
                else if (sockets.Contains (LoadoutSockets.leftOptionalPart))
                    socketSuffix = Txt.Get(TextLibs.uiBase, "inventory_socket_suffix_arm");

                if (!string.IsNullOrEmpty (socketSuffix))
                    return $"{group.textName} / {socketSuffix}";
            }
            
            return group.textName;
        }
        
        #if !PB_MODSDK
        public static string GetEquipmentGroupName (this EquipmentEntity entity, DataContainerEquipmentGroup group = null)
        {
            if (group == null)
                group = GetEquipmentGroup (entity);
            
            if (group == null || string.IsNullOrEmpty (group.textName))
                return fallbackGroupName;

            if (entity.hasDataLinkPartPreset)
            {
                var preset = entity.dataLinkPartPreset.data;
                var tags = preset.tagsProcessed;
                if (tags != null && tags.Contains (partPresetTagBody))
                {
                    var sockets = preset.socketsProcessed;
                    string socketSuffix = null;
                    
                    if (sockets.Contains (LoadoutSockets.corePart))
                        socketSuffix = Txt.Get(TextLibs.uiBase, "inventory_socket_suffix_torso");
                    else if (sockets.Contains (LoadoutSockets.secondaryPart))
                        socketSuffix = Txt.Get(TextLibs.uiBase, "inventory_socket_suffix_legs");
                    else if (sockets.Contains (LoadoutSockets.leftOptionalPart))
                        socketSuffix = Txt.Get(TextLibs.uiBase, "inventory_socket_suffix_arm");

                    if (!string.IsNullOrEmpty (socketSuffix))
                        return $"{group.textName} / {socketSuffix}";
                }
            }
            
            return group.textName;
        }
        
        public static string GetEquipmentGroupDesc (this EquipmentEntity entity)
        {
            var group = GetEquipmentGroup (entity);
            if (group == null || string.IsNullOrEmpty (group.textDesc))
                return fallbackGroupDesc;
            return group.textDesc;
        }
        
        public static string GetEquipmentGroupIcon (this EquipmentEntity entity)
        {
            var group = GetEquipmentGroup (entity);
            if (group == null || string.IsNullOrEmpty (group.icon))
                return fallbackGroupIcon;
            return group.icon;
        }
        #endif

        public static Color GetSocketEditorColor (string socket)
        {
            if (string.IsNullOrEmpty (socket) || !socketsToColors.TryGetValue (socket, out var color))
                return new Color (1f, 1f, 1f, 1f);

            return color;
        }
        
        private static Dictionary<string, Color> socketsToColors = new Dictionary<string, Color>
        {
            { LoadoutSockets.back, Color.HSVToRGB (0.00f, 0.15f, 1f) },
            { LoadoutSockets.corePart, Color.HSVToRGB (0.45f, 0.15f, 1f) },
            { LoadoutSockets.leftEquipment, Color.HSVToRGB (0.50f, 0.15f, 1f) },
            { LoadoutSockets.rightEquipment, Color.HSVToRGB (0.55f, 0.15f, 1f) },
            { LoadoutSockets.leftOptionalPart, Color.HSVToRGB (0.60f, 0.15f, 1f) },
            { LoadoutSockets.rightOptionalPart, Color.HSVToRGB (0.65f, 0.15f, 1f) },
            { LoadoutSockets.secondaryPart, Color.HSVToRGB (0.70f, 0.15f, 1f) },
        };
    }
}


