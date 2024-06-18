using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public static class OverworldTags
    {
        public const string site = "site";
        public const string squad = "squad";
        public const string settlement = "settlement";
    }

    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldEntityBlueprint : DataMultiLinker<DataContainerOverworldEntityBlueprint>
    {
        public DataMultiLinkerOverworldEntityBlueprint ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldEntities); 
        }
    
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [Space (8f)]
            [ShowInInspector][LabelText ("Show UI")]
            public static bool showUI = true;
            
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showVisuals = true;
            
            [ShowInInspector]
            public static bool showMovement = true;
            
            [ShowInInspector]
            public static bool showRanges = true;
            
            [ShowInInspector]
            public static bool showIntel = true;

            [ShowInInspector]
            public static bool showBattery = true;

            [ShowInInspector]
            public static bool showRepairJuice = true;

            [Space (8f)]
            [ShowInInspector]
            public static bool showUnits = true;
            
            [ShowInInspector]
            public static bool showScenarios = true;
            
            [ShowInInspector]
            public static bool showProduction = true;
            
            [ShowInInspector][LabelText ("Show AI")]
            public static bool showAI = true;

            [ShowInInspector]
            public static bool showSpawning = true;
            
            [ShowInInspector]
            public static bool showReinforcements = true;

            [ShowInInspector]
            public static bool showLoot = true;
            
            [ShowInInspector]
            public static bool showRewards = true;

            [ShowInInspector]
            public static bool showEscalation = true;
            
            [ShowInInspector]
            public static bool showThreatLevel = true;
            
            [ShowInInspector]
            public static bool showBattleSite = true;
            
            [Space (8f)]
            [ShowInInspector]
            public static bool showTags = true;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
            
            [ShowInInspector]
            // [ShowIf ("showInheritance")]
            [InfoBox ("Warning: this mode triggers inheritance processing every time inheritable fields are modified. This is useful for instantly previewing changes to things like stat or inherited text, but it can cause data loss if you are not careful. Only currently modified config is protected from being reloaded, save if you switch to another config.", VisibleIf = "autoUpdateInheritance")]
            public static bool autoUpdateInheritance = false;
            
            [ShowInInspector]
            public static bool showInheritance = false;
        }
        
        [ShowInInspector, HideLabel, FoldoutGroup ("View options", false)]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerOverworldEntityBlueprint.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldEntityBlueprint.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        private static StringBuilder sb = new StringBuilder ();
        
        
        /*
        public static void OnAfterDeserialization ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                ProcessRecursive (blueprint, blueprint, 0, new HashSet<string> ());
            }
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
        */

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
        }
        
        private static List<DataContainerOverworldEntityBlueprint> blueprintsUpdated = new List<DataContainerOverworldEntityBlueprint> ();

      
        public static void ProcessRelated (DataContainerOverworldEntityBlueprint origin)
        {
            if (origin == null)
                return;

            blueprintsUpdated.Clear ();
            blueprintsUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var preset = GetEntry (childKey);
                    if (preset != null)
                        blueprintsUpdated.Add (preset);
                }
            }
            
            foreach (var blueprint in blueprintsUpdated)
            {
                // Avoid localization refresh on origin
                if (blueprint != origin)
                    blueprint.OnAfterDeserialization (blueprint.key);
            }

            foreach (var blueprint in blueprintsUpdated)
                ProcessRecursiveStart (blueprint);
            
            foreach (var blueprint in blueprintsUpdated)
                Postprocess (blueprint);
        }
        
        private static void Postprocess (DataContainerOverworldEntityBlueprint blueprint)
        {
            if (blueprint.factionBranchProcessed != null && !string.IsNullOrEmpty (blueprint.factionBranchProcessed.factionBranch))
            {
                //If the processed tags does not contain the faction branch, automatically add it here.
                if (!blueprint.tagsProcessed.Contains (blueprint.factionBranchProcessed.factionBranch))
                    blueprint.tagsProcessed.Add (blueprint.factionBranchProcessed.factionBranch);
            }
            
            #if UNITY_EDITOR
            if (blueprint.areasProcessed != null)
                blueprint.UpdateAreaKeys (true);
            #endif
        }
        
        private static void ProcessRecursiveStart (DataContainerOverworldEntityBlueprint origin)
        {
            if (origin == null)
                return;
            
            origin.tagsProcessed.Clear ();

            if (origin.textNameProcessed != null)
                origin.textNameProcessed.s = string.Empty;

            if (origin.textDescProcessed != null)
                origin.textDescProcessed.s = string.Empty;

            origin.textIdentifierGroupProcessed = null;
            origin.imageProcessed = null;
            origin.iconProcessed = null;
            origin.coreProcessed = null;
            origin.factionBranchProcessed = null;
            origin.visualProcessed = null;
            origin.movementProcessed = null;
            origin.rangesProcessed = null;
            origin.detectionProcessed = null;
            origin.intelProcessed = null;
            origin.unitsProcessed = null;
            origin.scenariosProcessed = null;
            origin.scenarioUnitsProcessed = null;
            origin.scenarioChangesProcessed = null;
            origin.areasProcessed = null;
            origin.productionProcessed = null;
            origin.aiProcessed = null;
            origin.spawningProcessed = null;
            origin.rewardsProcessed = null;
            origin.interactionEffectsProcessed = null;
            origin.salvageBudgetProcessed = null;
            origin.escalationProcessed = null;
            origin.threatLevelProcessed = null;
            origin.battleSiteProcessed = null;

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

        private static void ProcessRecursive (DataContainerOverworldEntityBlueprint current, DataContainerOverworldEntityBlueprint root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null overworld blueprint step or root overworld blueprint reference while processing hierarchy");
                return;
            }

            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Overworld blueprint {root.key} encountered infinite dependency loop at depth level {depth}");
                return;
            }
            
            if (current.tags != null)
            {
                if (!root.tagsProcessed.Contains (current.key))
                    root.tagsProcessed.Add (current.key);

                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProcessed.Contains (tag))
                        root.tagsProcessed.Add (tag);
                }
            }

            if (current.textName != null && !string.IsNullOrEmpty (current.textName.s))
            {
                if (root.textNameProcessed == null)
                    root.textNameProcessed = new DataBlockStringNonSerialized ();

                if (string.IsNullOrEmpty (root.textNameProcessed.s))
                    root.textNameProcessed.s = current.textName.s;
            }

            if (current.textDesc != null && !string.IsNullOrEmpty (current.textDesc.s))
            {
                if (root.textDescProcessed == null)
                    root.textDescProcessed = new DataBlockStringNonSerializedLong ();

                if (string.IsNullOrEmpty (root.textDescProcessed.s))
                    root.textDescProcessed.s = current.textDesc.s;
            }
            
            if (!string.IsNullOrWhiteSpace (current.textIdentifierGroup) && string.IsNullOrEmpty (root.textIdentifierGroupProcessed))
                root.textIdentifierGroupProcessed = current.textIdentifierGroup;
            
            if (!string.IsNullOrWhiteSpace (current.image) && string.IsNullOrEmpty (root.imageProcessed))
                root.imageProcessed = current.image;
            
            if (!string.IsNullOrWhiteSpace (current.icon) && string.IsNullOrEmpty (root.iconProcessed))
                root.iconProcessed = current.icon;

            if (current.core != null && root.coreProcessed == null)
            {
                root.coreProcessed = current.core;
            }
            
            if (current.factionBranch != null && root.factionBranchProcessed == null)
            {
                root.factionBranchProcessed = current.factionBranch;
            }

            if (current.visual != null && root.visualProcessed == null)
            {
                root.visualProcessed = current.visual;
            }
            
            if (current.movement != null && root.movementProcessed == null)
            {
                root.movementProcessed = current.movement;
            }

            if (current.ranges != null && root.rangesProcessed == null)
            {
                root.rangesProcessed = current.ranges;
            }

            if (current.detection != null && root.detectionProcessed == null)
            {
                root.detectionProcessed = current.detection;
            }
            
            if (current.intel != null && root.intelProcessed == null)
            {
                root.intelProcessed = current.intel;
            }

            if (current.units != null && root.unitsProcessed == null)
            {
                root.unitsProcessed = current.units;
            }

            if (current.scenarios != null && current.scenarios.tags != null && current.scenarios.tags.Count > 0)
            {
                if (root.scenariosProcessed == null)
                    root.scenariosProcessed = new DataBlockOverworldEntityScenarios ();
                if (root.scenariosProcessed.tags == null)
                    root.scenariosProcessed.tags = new SortedDictionary<string, bool> ();
                foreach (var kvp in current.scenarios.tags)
                {
                    var tag = kvp.Key;
                    if (!string.IsNullOrEmpty (tag) && !root.scenariosProcessed.tags.ContainsKey (tag))
                        root.scenariosProcessed.tags.Add (tag, kvp.Value);
                }
            }
            
            if (current.scenarioUnits != null && current.scenarioUnits.Count > 0 && root.scenarioUnitsProcessed == null)
                root.scenarioUnitsProcessed = current.scenarioUnits;
            
            if (current.scenarioChanges != null && root.scenarioChangesProcessed == null)
                root.scenarioChangesProcessed = current.scenarioChanges;
            
            if (current.areas != null && current.areas.tags != null && current.areas.tags.Count > 0)
            {
                if (root.areasProcessed == null)
                    root.areasProcessed = new DataBlockOverworldEntityAreas ();
                if (root.areasProcessed.tags == null)
                    root.areasProcessed.tags = new SortedDictionary<string, bool> ();
                
                foreach (var kvp in current.areas.tags)
                {
                    var tag = kvp.Key;
                    if (!string.IsNullOrEmpty (tag) && !root.areasProcessed.tags.ContainsKey (tag))
                        root.areasProcessed.tags.Add (tag, kvp.Value);
                }
            }
            
            if (current.production != null && root.productionProcessed == null)
            {
                root.productionProcessed = current.production;
            }
            
            if (current.ai != null && root.aiProcessed == null)
            {
                root.aiProcessed = current.ai;
            }

            if (current.spawning != null && root.spawningProcessed == null)
            {
                root.spawningProcessed = current.spawning;
            }

            if (current.reinforcements != null && root.reinforcementsProcessed == null)
            {
                root.reinforcementsProcessed = current.reinforcements;
            }

            /*
            if (current.rewards != null && root.rewardsProcessed == null)
            {
                root.rewardsProcessed = current.rewards;
            }
            */
            
            if (current.rewards != null && current.rewards.blocks != null && current.rewards.blocks.Count > 0)
            {
                if (root.rewardsProcessed == null)
                    root.rewardsProcessed = new DataBlockOverworldEntityRewards ();

                if (root.rewardsProcessed.triggersAfterCombat == null && current.rewards.triggersAfterCombat != null)
                    root.rewardsProcessed.triggersAfterCombat = current.rewards.triggersAfterCombat;
                
                if (root.rewardsProcessed.blocks == null)
                    root.rewardsProcessed.blocks = new SortedDictionary<string, List<DataBlockOverworldEntityRewardBlock>> ();

                foreach (var kvp in current.rewards.blocks)
                {
                    if (!root.rewardsProcessed.blocks.ContainsKey (kvp.Key))
                        root.rewardsProcessed.blocks.Add (kvp.Key, kvp.Value);
                }
            }
            
            if (current.interactionEffects != null && root.interactionEffectsProcessed == null)
            {
                root.interactionEffectsProcessed = current.interactionEffects;
            }

            if (current.salvageBudget != null && root.salvageBudgetProcessed == null)
            {
                root.salvageBudgetProcessed = current.salvageBudget;
	        }

            if (current.escalation != null && root.escalationProcessed == null)
            {
                root.escalationProcessed = current.escalation;
            }
            
            if (current.threatLevel != null && root.threatLevelProcessed == null)
            {
                root.threatLevelProcessed = current.threatLevel;
            }
            
            if (current.battleSite != null && root.battleSiteProcessed == null)
            {
                root.battleSiteProcessed = current.battleSite;
            }
            
            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Overworld blueprint {root.key} fails to complete recursive processing in under 20 steps | Current step: {current.key}");
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
                    Debug.LogWarning ($"Overworld blueprint {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Overworld blueprint {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Overworld blueprint {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void LogWarObjectives ()
        {
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                if (container.hidden)
                    continue;

                if (container.escalation != null)
                {
                    var e = container.escalation;
                    Debug.Log ($"{container.key} has escalation/war data | Esc. gain: {e.escalationGain} | Score hit: {e.warScoreDealt} | Objective: {e.warObjectiveCandidate}");

                    if (e.warObjectiveCandidate && kvp.Key.Contains ("squad_"))
                    {
                        e.warObjectiveCandidate = false;
                        Debug.Log ($"{container.key} is a squad, disabling war objective flag");
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void LogEmbeddedUnits ()
        {
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                if (container.hidden)
                    continue;

                if (container.unitsProcessed != null && container.scenarioUnitsProcessed != null)
                {
                    for (int i = 0; i < container.scenarioUnitsProcessed.Count; ++i)
                    {
                        var block = container.scenarioUnitsProcessed[i];
                        if (block == null || block.unitGroups == null || block.unitGroups.Count == 0)
                            continue;
                        
                        Debug.Log ($"{container.key} has embedded units | Variation {i} contains {block.unitGroups.Count} squads");
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void PrintAssetKeys ()
        {
            var dictionary = new SortedDictionary<string, List<string>> ();
            
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                if (container == null || container.visual == null)
                    continue;

                var v = container.visual;

                if (v.visualPrefabs == null || v.visualPrefabs.Count == 0)
                {
                    Debug.LogWarning ($"{container.key} has null or empty visual asset list");
                    continue;
                }
                
                if (v.visualPrefabs.Count > 1)
                {
                    Debug.LogWarning ($"{container.key} has more than one visual asset: {container.visual.visualPrefabs.ToStringFormatted ()}");
                    continue;
                }

                for (int i = 0; i < v.visualPrefabs.Count; ++i)
                {
                    var link = v.visualPrefabs[i];
                    if (link == null || string.IsNullOrEmpty (link.path))
                    {
                        Debug.LogWarning ($"{container.key} has null or empty path in visual asset link {i}");
                        continue;
                    }

                    if (!dictionary.ContainsKey (link.path))
                    {
                        dictionary.Add (link.path, new List<string> { container.key });
                    }
                    else
                    {
                        var list = dictionary[link.path];
                        if (!list.Contains (container.key))
                            list.Add (container.key);
                    }
                }
            }

            var report = dictionary.ToStringFormattedKeyValuePairs (true, (x) => x.ToStringFormatted ());
            Debug.Log ($"Discovered {dictionary.Count} unique visual assets:\n{report}");
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void PrintThreatRatings ()
        {
            var list = new List<DataContainerOverworldEntityBlueprint> ();
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                if (container == null || container.hidden || container.threatLevelProcessed == null)
                    continue;

                list.Add (container);
            }
            
            list.Sort ((x, y) => x.threatLevelProcessed.baseThreatLevel.CompareTo (y.threatLevelProcessed.baseThreatLevel));
            var text = list.ToStringFormatted (true, multilinePrefix: "- ", toStringOverride: (x) => $"{x.key}: {x.threatLevelProcessed.baseThreatLevel}");
            Debug.Log (text);
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void UpgradeRewardEquipmentKeys ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (blueprint.rewards == null || blueprint.rewards.blocks == null)
                    continue;

                var rewards = blueprint.rewards.blocks;
                foreach (var kvp2 in rewards)
                {
                    var blocks = kvp2.Value;
                    if (blocks == null)
                        continue;

                    foreach (var block in blocks)
                    {
                        if (block == null)
                            continue;
                        
                        if (block.parts != null)
                        {
                            foreach (var part in block.parts)
                            {
                                if (part == null || part.tagsUsed)
                                    continue;

                                var keyOld = part.preset;
                                var keyNew = DataMultiLinkerPartPreset.GetUpgradedKey (part.preset, out bool replaced);
                                if (replaced)
                                {
                                    part.preset = keyNew;
                                    Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with part preset key {keyOld} -> {keyNew}");
                                }
                            }
                        }

                        if (block.subsystems != null)
                        {
                            foreach (var subsystem in block.subsystems)
                            {
                                if (subsystem == null || subsystem.tagsUsed)
                                    continue;

                                var keyOld = subsystem.blueprint;
                                var keyNew = DataMultiLinkerSubsystem.GetUpgradedKey (subsystem.blueprint, out bool replaced);
                                if (replaced)
                                {
                                    subsystem.blueprint = keyNew;
                                    Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with subsystem key {keyOld} -> {keyNew}");
                                }
                            }
                        }
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-10)]
        public void UpgradeRewardEquipmentRatings ()
        {
            Dictionary<string, int> partPresetRatingSuffixes = new Dictionary<string, int>
            {
                { "_r1", 1 }, 
                { "_r2", 2 }, 
                { "_r3", 3 }
            };
            Dictionary<int, string> partPresetQualityTables = new Dictionary<int, string>
            {
                { 2, "default_r2_uncommon" }, 
                { 3, "default_r3_rare" }
            };

            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (blueprint.rewards == null || blueprint.rewards.blocks == null)
                    continue;

                var rewards = blueprint.rewards.blocks;
                foreach (var kvp2 in rewards)
                {
                    var blocks = kvp2.Value;
                    if (blocks == null)
                        continue;

                    foreach (var block in blocks)
                    {
                        if (block == null)
                            continue;
                        
                        if (block.parts != null)
                        {
                            foreach (var part in block.parts)
                            {
                                if (part == null || part.tagsUsed)
                                    continue;

                                var keyOld = part.preset;
                                if (!string.IsNullOrEmpty (keyOld))
                                {
                                    foreach (var kvp4 in partPresetRatingSuffixes)
                                    {
                                        var suffix = kvp4.Key;
                                        int rating = kvp4.Value;
                                        
                                        if (part.preset.EndsWith (suffix))
                                        {
                                            part.preset = part.preset.Replace (suffix, string.Empty);
                                            if (partPresetQualityTables.ContainsKey (rating))
                                            {
                                                part.qualityTableKey = partPresetQualityTables[rating];
                                                Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with part preset key without the rating: {keyOld} -> {part.preset} | Quality table added: {part.qualityTableKey}");
                                            }
                                            else
                                            {
                                                Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with part preset key without the rating: {keyOld} -> {part.preset}");
                                            }
                                        }
                                    }
                                }

      
                                var keyNew = DataMultiLinkerPartPreset.GetUpgradedKey (part.preset, out bool replaced);
                                if (replaced)
                                {
                                    part.preset = keyNew;
                                    Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with part preset key {keyOld} -> {keyNew}");
                                }
                            }
                        }

                        if (block.subsystems != null)
                        {
                            foreach (var subsystem in block.subsystems)
                            {
                                if (subsystem == null || subsystem.tagsUsed)
                                    continue;

                                var keyOld = subsystem.blueprint;
                                var keyNew = DataMultiLinkerSubsystem.GetUpgradedKey (subsystem.blueprint, out bool replaced);
                                if (replaced)
                                {
                                    subsystem.blueprint = keyNew;
                                    Debug.Log ($"Updated reward block in overworld entity {blueprint.key} reward {kvp2.Key} with subsystem key {keyOld} -> {keyNew}");
                                }
                            }
                        }
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button ("Print images", ButtonSizes.Large)]
        public void PrintImages ()
        {
            var keysAll = TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEntities);
            var keysUnused = new List<string> ();
            var keysMissing = new List<string> ();
            
            var sb = new StringBuilder ();
            var set = new SortedDictionary<string, List<string>> ();
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (string.IsNullOrEmpty (blueprint.image))
                    continue;
                
                // Debug.Log ($"{blueprint.key}: {blueprint.image}");
                if (!set.ContainsKey (blueprint.image))
                {
                    set.Add (blueprint.image, new List<string> { blueprint.key });
                    if (!keysAll.Contains (blueprint.image))
                        keysMissing.Add (blueprint.image);
                }
                else
                {
                    set[blueprint.image].Add (blueprint.key);
                    set[blueprint.image].Sort ();
                }
            }

            bool started = false;
            foreach (var kvp in set)
            {
                if (started)
                    sb.Append ("\n\n");
                else
                    started = true;
                
                sb.Append (kvp.Key);

                bool vqg = kvp.Key.Contains ("_vqg");
                sb.Append (vqg ? " (VQG)" : " (Normal)");
                
                sb.Append (":\n");
                sb.Append (kvp.Value.ToStringFormatted (true, multilinePrefix: "- "));
            }
            
            foreach (var key in keysAll)
            {
                if (!set.ContainsKey (key))
                    keysUnused.Add (key);
            }

            if (keysUnused.Count > 0)
            {
                sb.Append ("\n\n");
                sb.Append ("Unused images:\n");
                sb.Append (keysUnused.ToStringFormatted (true, multilinePrefix: "- "));
            }
            
            if (keysMissing.Count > 0)
            {
                sb.Append ("\n\n");
                sb.Append ("Missing images:\n");
                sb.Append (keysMissing.ToStringFormatted (true, multilinePrefix: "- "));
            }

            Debug.Log (sb.ToString ());
            GUIUtility.systemCopyBuffer = sb.ToString ();
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print escalation gains", ButtonSizes.Large)]
        public void PrintEscalationGains ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (blueprint.escalation == null)
                    continue;

                var e = blueprint.escalation;
                float gainWar = e.escalationGain * e.escalationGainWarMultiplier;
                float gain = e.escalationGain;
                if (gain <= 0f && gainWar <= 0f)
                    continue;
                
                Debug.Log ($"- {blueprint.key} | Escalation (raid/war): {gain}/{gainWar}");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print effects", ButtonSizes.Large)]
        public void PrintEffects ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (blueprint.interactionEffects == null)
                    continue;
                
                if (blueprint.interactionEffects.effectsOnEntry != null && blueprint.interactionEffects.effectsOnEntry.Count > 0)
                    Debug.Log ($"{blueprint.key} has effects on entry: {blueprint.interactionEffects.effectsOnEntry.ToStringFormatted ()}");
                
                if (blueprint.interactionEffects.effectsOverTime != null && blueprint.interactionEffects.effectsOverTime.Count > 0)
                    Debug.Log ($"{blueprint.key} has effects on entry: {blueprint.interactionEffects.effectsOverTime.ToStringFormatted ()}");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Print loot", ButtonSizes.Large)]
        public void PrintLoot ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (blueprint.rewards == null || blueprint.rewards.blocks == null || blueprint.hidden || blueprint.key.Contains ("debug"))
                    continue;

                foreach (var kvp2 in blueprint.rewards.blocks)
                {
                    var rewardKey = kvp2.Key;
                    var list = kvp2.Value;

                    if (list == null)
                        continue;

                    for (int i = 0; i < list.Count; ++i)
                    {
                        var block = list[i];
                        if (block == null)
                        {
                            Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: null data");
                            continue;
                        }

                        if (block.parts != null)
                        {
                            foreach (var part in block.parts)
                            {
                                if (part.tagsUsed)
                                {
                                    var partKeys = DataTagUtility.GetKeysWithTags (DataMultiLinkerPartPreset.data, part.tags);
                                    if (partKeys.Count == 0)
                                        Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: no parts found with tag filter: {part.tags.ToStringFormattedKeyValuePairs ()}");
                                }
                                else
                                {
                                    var partData = DataMultiLinkerPartPreset.GetEntry (part.preset, false);
                                    if (partData == null)
                                        Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: no part preset found: {part.preset}");
                                }

                                if (part.levelMin < 1)
                                    Debug.Log ($"{blueprint.key} | Reward {rewardKey} index {i}: min level offset too low: {(part.levelRandom ? $"{part.levelMin}-{part.levelMax}" : part.levelMin.ToString())}");
                            }
                        }
                        
                        if (block.subsystems != null)
                        {
                            foreach (var s in block.subsystems)
                            {
                                if (s.tagsUsed)
                                {
                                    var partKeys = DataTagUtility.GetKeysWithTags (DataMultiLinkerSubsystem.data, s.tags);
                                    if (partKeys.Count == 0)
                                        Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: no subsystems found with tag filter: {s.tags.ToStringFormattedKeyValuePairs ()}");
                                }
                                else
                                {
                                    var subsystemData = DataMultiLinkerSubsystem.GetEntry (s.blueprint, false);
                                    if (subsystemData == null)
                                        Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: no subsystem blueprint found: {s.blueprint}");
                                }
                            }
                        }

                        if (block.projects != null)
                        {
                            foreach (var s in block.projects)
                            {
                                if (s.tagsUsed)
                                {
                                    var partKeys = DataTagUtility.GetKeysWithTags (DataMultiLinkerWorkshopProject.data, s.tags);
                                    if (partKeys.Count == 0)
                                        Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: no projects found with tag filter: {s.tags.ToStringFormattedKeyValuePairs ()}");
                                }
                                else
                                {
                                    var projectData = DataMultiLinkerWorkshopProject.GetEntry (s.key, false);
                                    if (projectData == null)
                                        Debug.LogWarning ($"{blueprint.key} | Reward {rewardKey} index {i}: no projects found: {s.key}");
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
        [Button ("Upgrade rewards", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeRewards ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;
                if (blueprint.rewards == null)
                    continue;

                foreach (var kvp2 in blueprint.rewards.blocks)
                {
                    var list = kvp2.Value;
                    if (list == null || list.Count == 0)
                        continue;

                    for (int i = 0, count = list.Count; i < count; ++i)
                    {
                        var block = list[i];
                        if (block == null || block.supplies == null)
                            continue;

                        var s = block.supplies;
                        block.resources = new SortedDictionary<string, DataBlockVirtualResource>
                        {
                            {
                                ResourceKeys.supplies,
                                new DataBlockVirtualResource
                                {
                                    amountRandom = s.amountRandom,
                                    amountMin = s.amountMin,
                                    amountMax = s.amountMax
                                }
                            }
                        };

                        Debug.LogWarning ($"Upgraded {kvp.Key} / {kvp2.Key} / {i} from supplies to resources");
                        block.supplies = null;
                    }
                }
            }
        }
        */

        /*
        [Button ("Upgrade", ButtonSizes.Large), PropertyOrder (-10)]
        public void Upgrade ()
        {
            foreach (var kvp in data)
            {
                var blueprint = kvp.Value;

                if (blueprint.ui != null)
                {
                    blueprint.textName = blueprint.ui.textName;
                    blueprint.textDesc = blueprint.ui.textDesc;
                    blueprint.textIdentifierGroup = blueprint.ui.identificationGroup;
                    blueprint.image = blueprint.ui.image;
                    blueprint.ui = null;
                }

                if (blueprint.units != null && !blueprint.units.unitGroupCustom)
                {
                    var group = DataMultiLinkerOverworldUnitGroups.GetEntry (blueprint.units.unitGroupKey);
                    if (group != null && group.tags != null)
                    {
                        blueprint.units.tags = new SortedDictionary<string, bool> ();
                        foreach (var tag in group.tags)
                            blueprint.units.tags.Add (tag, true);
                    }
                }
                
                var ui = new DataBlockOverworldEntityUI ();
                blueprint.ui = ui;

                ui.image = blueprint.image;
                ui.identificationGroup = blueprint.identificationGroup;
                ui.textName = blueprint.textName;
                ui.textDesc = blueprint.textDesc;
                
                var core = new DataBlockOverworldEntityCore ();
                blueprint.core = core;

                core.inventory = blueprint.inventory;
                core.selectable = blueprint.selectable;
                core.capturable = blueprint.capturable;
                
                core.interactable = blueprint.interactable;
                core.interactionRangeCustom = blueprint.interactionRangeCustom;
                core.interactionRange = blueprint.interactionRange;
                
                core.destroyOnDefeat = blueprint.destroyOnDefeat;
                core.destroyOnLoot = blueprint.destroyOnLoot;
                
                core.detectable = blueprint.detectable;
                core.recognizable = blueprint.recognizable;
                core.observable = blueprint.observable;
                core.trackable = blueprint.trackable;
                core.permanent = blueprint.permanent;
                
                core.factionChangesWithProvince = blueprint.factionChangesWithProvince;
                core.faction = blueprint.faction;
                
                var visual = new DataBlockOverworldEntityVisual ();
                blueprint.visual = visual;

                visual.visualFromSquad = blueprint.visualFromSquad;
                visual.visualPrefabs = blueprint.visualPrefabs;
                
                
                
                blueprint.image = string.Empty;
                blueprint.identificationGroup = string.Empty;
                blueprint.textName = string.Empty;
                blueprint.textDesc = string.Empty;
                
                blueprint.inventory = false;
                blueprint.selectable = false;
                blueprint.capturable = false;
                
                blueprint.interactable = false;
                blueprint.interactionRangeCustom = false;
                blueprint.interactionRange = 0;
                
                blueprint.destroyOnDefeat = false;
                blueprint.destroyOnLoot = false;
                
                blueprint.detectable = false;
                blueprint.recognizable = false;
                blueprint.observable = false;
                blueprint.trackable = false;
                blueprint.permanent = false;
                
                blueprint.factionChangesWithProvince = false;
                blueprint.faction = null;
                
                blueprint.visualFromSquad = false;
                blueprint.visualPrefabs = null;
            }
        }
        */
    }
}
