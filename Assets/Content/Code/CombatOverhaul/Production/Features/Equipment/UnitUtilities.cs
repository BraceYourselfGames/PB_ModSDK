using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
using PhantomBrigade.Functions.Equipment;

namespace PhantomBrigade.Data
{
    public static class UnitUtilities
    {
        private static Dictionary<string, GeneratedHardpoint> partGenerationLayout = new Dictionary<string, GeneratedHardpoint> ();
        private static SortedDictionary<string, bool> partFilterBody = new SortedDictionary<string, bool> ();
        private static Dictionary<string, DataBlockSubsystemTagFilter> subsystemTagPreferencesCollapsed = new Dictionary<string, DataBlockSubsystemTagFilter> ();
        private static Dictionary<string, DataBlockPartTagFilter> partTagPreferencesCollapsed = new Dictionary<string, DataBlockPartTagFilter> ();

        private static List<string> subsystemsFilteredByHardpoint = new List<string> ();
        // private static List<string> presetsFiltered = new List<string> ();
        private static Dictionary<string, DataBlockSavedPart> unitDescription;
        
        // private static SortedDictionary<string, bool> partTagPreferenceForSocket = new SortedDictionary<string, bool> ();

        public static Dictionary<string, DataBlockSavedPart> CreatePersistentUnitDescription 
        (
            DataContainerUnitPreset preset, 
            int level = 1, 
            bool reuseCollections = true, 
            bool saveForDebugging = false,
            DataContainerOverworldFactionBranch factionData = null,
            DataContainerUnitLiveryPreset liveryPreset = null,
            string equipmentQualityTableKey = null,
            int rating = -1,
            bool acceptLowerQuality = false
        )
        {
            bool logExtended = DataShortcuts.sim.logEquipmentGeneration;
            if (preset == null)
            {
                Debug.LogWarning ($"Failed to create  unit description due to null preset");
                return null;
            }
            
            var unitBlueprint = DataMultiLinkerUnitBlueprint.GetEntry (preset.blueprintProcessed);
            if (unitBlueprint == null)
            {
                Debug.LogWarning ($"Failed to create unit description due to unknown blueprint {preset.blueprintProcessed} in preset {preset.key}");
                return null;
            }

            var partFilterFromBlueprint = 
                unitBlueprint.partPresetFilter != null && 
                unitBlueprint.partPresetFilter.Count > 0 ? 
                unitBlueprint.partPresetFilter : 
                null;
            
            var partFiltersFromFaction = 
                !string.IsNullOrEmpty (unitBlueprint.classTag) &&
                factionData != null &&
                factionData.unitPartFilters != null && 
                factionData.unitPartFilters.ContainsKey (unitBlueprint.classTag) ? 
                factionData.unitPartFilters[unitBlueprint.classTag].filters : 
                null;
            
            // Just in case passed in rating is weird
            if (rating != -1)
                rating = Mathf.Max (rating, 0);

            // Drop rating to 0 for training factions
            if (factionData != null && factionData.training)
                rating = 0;
            
            // Drop rating to 1 in absence of quality table if fixed rating wasn't provided
            if (string.IsNullOrEmpty (equipmentQualityTableKey) && rating == -1)
                rating = 1;

            if (logExtended)
                Debug.Log ($"Creating unit description from preset {preset.key} | Part filter from blueprint: {partFilterFromBlueprint.ToStringNullCheck ()} | Equipment quality argument: {rating} | Equipment quality table key: {equipmentQualityTableKey} | Part filter from faction: {partFilterFromBlueprint.ToStringNullCheck ()} | Rating: {rating}");

            var description = reuseCollections ? unitDescription : new Dictionary<string, DataBlockSavedPart> ();
            if (reuseCollections)
                description.Clear ();

            partFilterBody.Clear ();
            partTagPreferencesCollapsed.Clear ();
            bool partTagPreferencesEnforced = false;

            if (preset.partTagPreferencesProcessed != null && preset.partTagPreferencesProcessed.Count > 0)
            {
                foreach (var kvp in preset.partTagPreferencesProcessed)
                {
                    var socketTag = kvp.Key;
                    var partTagPreferences = kvp.Value;
                    
                    if (partTagPreferences == null || partTagPreferences.Count == 0)
                        continue;

                    partTagPreferencesEnforced = true;
                    partTagPreferencesCollapsed.Add (socketTag, partTagPreferences.GetRandomEntry ());
                }
            }

            var liveryOnUnit = preset.liveryProcessed;
            if (!string.IsNullOrEmpty (liveryOnUnit))
            {
                var liveryOnUnitData = DataMultiLinkerEquipmentLivery.GetEntry (liveryOnUnit, false);
                if (liveryOnUnitData == null)
                    liveryOnUnit = null;
            }

            if (liveryPreset != null && liveryPreset.nodeProcessed != null)
                liveryOnUnit = liveryPreset.nodeProcessed.livery;

            bool bodyTagPinningUsed = !unitBlueprint.bodyTagPinningExempted && !preset.bodyTagPinningExempted;
            bool bodyTagPinnedFound = false;
            string bodyTagPinned = null;

            foreach (var partEntry in unitBlueprint.sockets)
            {
                // We need to fetch socket data to get access to its tags - both unit preset driven filters and faction driven filters are keyed
                // not by specific socket keys, but by tags grouping socket keys together. No point defining 4 filters when 1 can cover all body parts etc.
                var socket = partEntry.Key;
                var socketData = DataMultiLinkerPartSocket.GetEntry (socket);

                var presetSettingsOnBlueprint = partEntry.Value;
                var partPresetKey = presetSettingsOnBlueprint.presetDefault;
                bool partPresetFiltered = presetSettingsOnBlueprint.presetFiltered;

                // Since we access socket override data in multiple places, it's useful to resolve it once
                DataBlockUnitPartOverride socketOverrideData = null;
                if (preset.partsProcessed != null && preset.partsProcessed.ContainsKey (socket))
                {
                    var socketOverrideDataCandidate = preset.partsProcessed[socket];
                    if (socketOverrideDataCandidate != null)
                        socketOverrideData = socketOverrideDataCandidate;
                }

                // I'd prefer not to manage two places where we merge filters, so I handle them separately until they go into GetPartPresetKey
                SortedDictionary<string, bool> partFilterFromPresetOverrides = null;
                SortedDictionary<string, bool> partFilterFromPresetPrefs = null;
                
                // We need to check part overrides to fetch one of the filter sets or skip filtering process
                if (socketOverrideData != null && socketOverrideData.preset != null)
                {
                    // If the socket override instructs that part should be missing, there is no point continuing
                    if (socketOverrideData.preset is DataBlockPartSlotResolverClear)
                        continue;

                    // If the socket overrides reference a specific part, then we can stop right there - no need to filter anything
                    if (socketOverrideData.preset is DataBlockPartSlotResolverKeys resolverKeys)
                    {
                        partPresetFiltered = false;
                        partPresetKey = null;
                        if (resolverKeys != null && resolverKeys.keys.Count > 0)
                            partPresetKey = resolverKeys.keys.GetRandomEntry ();
                    }

                    // If tag mode is used, then we don't skip filtering and just save the filter for later merge
                    else if (socketOverrideData.preset is DataBlockPartSlotResolverTags resolverTags)
                    {
                        if (resolverTags.filters != null && resolverTags.filters.Count > 0)
                        {
                            var partFilterContainer = resolverTags.filters.GetRandomEntry ();
                            if (partFilterContainer != null && partFilterContainer.tags != null && partFilterContainer.tags.Count > 0)
                                partFilterFromPresetOverrides = partFilterContainer.tags;
                        }
                    }
                }

                // Local var for rating, accounting for rating possibly changing to fall back after failed filtering
                int ratingResolved = rating;
                
                var qualityTable = equipmentQualityTableKey != null ? DataMultiLinkerQualityTable.GetEntry (equipmentQualityTableKey, false) : null;
                if (qualityTable != null)
                {
                    ratingResolved = qualityTable.RollRandomQuality ();
                    if (logExtended)
                        Debug.Log ($"Socket {socket} | Rating changed to {ratingResolved} based on quality table {equipmentQualityTableKey}");
                }

                if (socketOverrideData != null && socketOverrideData.rating != null)
                {
                    ratingResolved = socketOverrideData.rating.i;
                    if (logExtended)
                        Debug.Log ($"Socket {socket} | Rating changed to {ratingResolved} based on rating override");
                }
                
                // Ensure rating is sensible
                int ratingResolvedClamped = Mathf.Clamp (ratingResolved, 0, 4);
                if (ratingResolvedClamped != ratingResolved)
                {
                    if (logExtended)
                        Debug.Log ($"Socket {socket} | Rating changed to {ratingResolvedClamped} due to value {ratingResolved} being out of 0-4 limit");
                    ratingResolved = ratingResolvedClamped;
                }
                
                // We continue to compiling dependencies for filtering only if we haven't encountered direct part preset override
                if (partPresetFiltered)
                {
                    // Next we pick up filter based on socket tag (e.g. "body") - similar to filter attached to socket key that could've been picked up higher
                    foreach (var kvp in partTagPreferencesCollapsed)
                    {
                        var socketTag = kvp.Key;
                        if (socketData.tags.Contains (socketTag))
                        {
                            var partFilter = kvp.Value != null ? kvp.Value.tags : null;
                            if (partFilter != null && partFilter.Count > 0)
                                partFilterFromPresetPrefs = partFilter;
                        }
                    }

                    // Faction-driven part filters might be impossible to fulfill and aren't meant to be locked in stone, defining descending set of filters to try
                    // Due to that, we want to maintain a list we can modify, with refs to filters from faction config - instead of directly using faction config
                    partFilterStackFromFaction.Clear ();

                    // Not all factions define preferences for a given unit type, though
                    if (!preset.branchIndependent && partFiltersFromFaction != null)
                    {
                        // Find a match to socket tag, fill the mutable list
                        foreach (var kvp in partFiltersFromFaction)
                        {
                            var socketTag = kvp.Key;
                            if (socketData.tags.Contains (socketTag))
                            {
                                var partFilterStack = kvp.Value != null ? kvp.Value : null;
                                if (partFilterStack != null && partFilterStack.Count > 0)
                                {
                                    foreach (var partFilterContainer in partFilterStack)
                                    {
                                        if (partFilterContainer != null && partFilterContainer.tags != null && partFilterContainer.tags.Count > 0)
                                            partFilterStackFromFaction.Add (partFilterContainer.tags);
                                    }
                                }
                            }
                        }
                    }

                    partPresetKey = GetPartPresetKey 
                    (
                        preset.key,
                        socket,
                        partFilterFromBlueprint,
                        partFilterFromPresetPrefs,
                        partFilterFromPresetOverrides,
                        socketData.body ? partFilterBody : null,
                        ratingResolved,
                        logExtended,
                        0
                    );
                }
                
                // If part preset name is null or empty, there is no point in continuing (no log because all cases where this could be unexpected already log)
                if (string.IsNullOrEmpty (partPresetKey))
                    continue;

                // Next we need to fetch preset data
                var partPreset = DataMultiLinkerPartPreset.GetEntry (partPresetKey, false);
                if (partPreset == null)
                {
                    Debug.LogWarning ($"Unit preset {preset.key} / socket {socket} | Failed to find part preset data for key {partPresetKey}");
                    continue;
                }

                if (partPreset.genStepsProcessed == null || partPreset.genStepsProcessed.Count == 0)
                {
                    Debug.LogWarning ($"Unit preset {preset.key} / socket {socket} | Failed to generate part {partPresetKey}: no generation steps defined");
                    continue;
                }

                if (logExtended)
                    Debug.Log ($"Unit preset {preset.key} | Resolved part to {socket}: {partPresetKey}");

                // If we're in a body socket and pinned armor tag was not found yet
                if (bodyTagPinningUsed && !bodyTagPinnedFound && socketData.body)
                {
                    if (partPreset.tagsProcessed != null)
                    {
                        foreach (var tag in partPreset.tagsProcessed)
                        {
                            if (tag != null && tag.Contains (tagPrefixArmor))
                            {
                                if (logExtended)
                                    Debug.Log ($"Unit preset {preset.key} | Pinned armor tag to {tag} (from {partPresetKey} in {socket})");
                                
                                bodyTagPinnedFound = true;
                                bodyTagPinned = tag;
                                partFilterBody[bodyTagPinned] = true;
                                break;
                            }
                        }
                    }
                }

                // At this point we know we're going to be dealing with a concrete part, no matter what.
                // Time to consider how given part can be customized, e.g. in terms of livery
                string partLiveryName = liveryOnUnit;

                // Time to go back to overrides:
                // it is possible that livery was modified even if part wasn't modified from there, which is a totally valid way to define a unit
                if (socketOverrideData != null && socketOverrideData.livery != null)
                {
                    var liveryOnPartDesc = socketOverrideData.livery.livery;
                    if (!string.IsNullOrEmpty (liveryOnPartDesc))
                    {
                        // If we encounter invalid livery key, clear the string
                        var liveryData = DataMultiLinkerEquipmentLivery.GetEntry (liveryOnPartDesc);
                        if (liveryData != null)
                            partLiveryName = liveryOnPartDesc;
                    }
                    else
                    {
                        // Reset livery to unit livery if override was on and key was deliberately left empty
                        partLiveryName = liveryOnUnit;
                    }
                }

                var liveryOverridePerSocket = 
                    liveryPreset != null && liveryPreset.socketsProcessed != null && liveryPreset.socketsProcessed.ContainsKey (socket) ? 
                    liveryPreset.socketsProcessed[socket] : 
                    null;
                
                if (liveryOverridePerSocket != null && liveryOverridePerSocket.node != null)
                    partLiveryName = liveryOverridePerSocket.node.livery;

                // Part layouts are mutable at runtime and part presets are just construction blueprints, not lifetime contracts:
                // therefore, we need to set up independent collections for available hardpoints and compatible sockets

                bool subsystemOverridesPresent =
                    socketOverrideData != null &&
                    socketOverrideData.systems != null &&
                    socketOverrideData.systems.Count > 0;

                partGenerationLayout.Clear ();
                
                foreach (var step in partPreset.genStepsProcessed)
                {
                    if (step != null)
                        step.Run (partPreset, partGenerationLayout, ratingResolved, false);
                }
                
                if (logExtended)
                    Debug.Log ($"Socket {socket} | L{level} | R{ratingResolved} | Part preset {preset.key} generated | Steps: {partPreset.genStepsProcessed.Count}\n{EquipmentGenUtility.GetLayoutDescription (partGenerationLayout)}");

                // Add all hardpoints in layout returned from part generation to hardpoint list
                var hardpoints = new HashSet<string> ();
                foreach (var kvp in partGenerationLayout)
                {
                    var hardpoint = kvp.Key;

                    var hardpointInfo = DataMultiLinkerSubsystemHardpoint.GetEntry (hardpoint, false);
                    if (hardpointInfo == null)
                    {
                        Debug.LogWarning ($"Unit preset {preset.key} | Skipping unknown hardpoint {socket}/{hardpoint} in unit preset {preset.key}");
                        continue;
                    }
                    
                    hardpoints.Add (hardpoint);
                }
                
                // Just in case where part preset doesn't contain some hardpoint but we want to inject something on top of standard layout
                if (subsystemOverridesPresent)
                {
                    foreach (var kvp in socketOverrideData.systems)
                    {
                        var hardpoint = kvp.Key;
                        if (!hardpoints.Contains (hardpoint))
                            hardpoints.Add (hardpoint);
                    }
                }
                
                var sockets = new HashSet<string> ();
                foreach (var s in partPreset.socketsProcessed)
                    sockets.Add (s);
                
                // Finally, it's time to compile final list of subsystems
                // We can start the process by iterating through full set of hardpoints defined by part blueprint
                
                var subsystemsCompiled = new Dictionary<string, DataBlockSavedSubsystem> ();
                foreach (var kvp in partGenerationLayout)
                {
                    var hardpoint = kvp.Key;
                    var hardpointGenerated = kvp.Value;
                    var subsystemCandidates = hardpointGenerated.subsystemCandidates;
                    bool subsystemFused = hardpointGenerated.fused;
                    
                    var subsystemBlueprintFromLayout = subsystemCandidates.Count > 0 ? subsystemCandidates.GetRandomEntry () : null;
                    var subsystemBlueprintKey = subsystemBlueprintFromLayout != null ? subsystemBlueprintFromLayout.key : null;
                    var subsystemBlueprintSource = "part preset";

                    // Next check if override coming from unit preset itself is present and use it accordingly
                    bool subsystemOverrideFound = subsystemOverridesPresent && socketOverrideData.systems.ContainsKey (hardpoint);
                    if (subsystemOverrideFound)
                    {
                        var subsystemOverrideData = socketOverrideData.systems[hardpoint];
                        subsystemBlueprintKey = subsystemOverrideData.GetBlueprint ();
                        subsystemBlueprintSource = "subsystem override in unit preset";
                        subsystemFused = subsystemOverrideData.flags == null || subsystemOverrideData.flags.fused;
                    }

                    if (logExtended)
                        Debug.Log ($"Unit preset: {preset.key} | Resolved subsystem in {socket}/{hardpoint} | Blueprint: {subsystemBlueprintKey} | Source: {subsystemBlueprintSource}");
                    
                    // If we arrived here with no subsystem name, no point proceeding
                    if (string.IsNullOrEmpty (subsystemBlueprintKey))
                        continue;

                    string subsystemLiveryName = null;
                    var liveryOverridePerHardpoint = 
                        liveryOverridePerSocket != null && liveryOverridePerSocket.hardpoints != null && liveryOverridePerSocket.hardpoints.ContainsKey (hardpoint) ? 
                        liveryOverridePerSocket.hardpoints[hardpoint] : 
                        null;
                    
                    if (liveryOverridePerHardpoint != null)
                        subsystemLiveryName = liveryOverridePerHardpoint.livery;

                    // Creating subsystem description - pretty similar to the data game receives from places like savegames
                    var subsystemDesc = new DataBlockSavedSubsystem ();
                    subsystemDesc.blueprint = subsystemBlueprintKey;
                    subsystemDesc.fused = subsystemFused;
                    subsystemDesc.destroyed = false;
                    subsystemDesc.livery = subsystemLiveryName;
                    subsystemsCompiled.Add (hardpoint, subsystemDesc);
                }

                if (subsystemOverridesPresent)
                {
                    foreach (var kvp in socketOverrideData.systems)
                    {
                        var hardpoint = kvp.Key;
                        if (subsystemsCompiled.ContainsKey (hardpoint))
                            continue;
                        
                        var subsystemOverrideData = kvp.Value;
                        var subsystemBlueprintKey = subsystemOverrideData.GetBlueprint ();
                        var subsystemFused = subsystemOverrideData.flags == null || subsystemOverrideData.flags.fused;
                        
                        string subsystemLiveryName = null;
                        var liveryOverridePerHardpoint = 
                            liveryOverridePerSocket != null && liveryOverridePerSocket.hardpoints != null && liveryOverridePerSocket.hardpoints.ContainsKey (hardpoint) ? 
                                liveryOverridePerSocket.hardpoints[hardpoint] : 
                                null;
                    
                        if (liveryOverridePerHardpoint != null)
                            subsystemLiveryName = liveryOverridePerHardpoint.livery;

                        var subsystemDesc = new DataBlockSavedSubsystem ();
                        subsystemDesc.blueprint = subsystemBlueprintKey;
                        subsystemDesc.fused = subsystemFused;
                        subsystemDesc.destroyed = false;
                        subsystemDesc.livery = subsystemLiveryName;
                        subsystemsCompiled.Add (hardpoint, subsystemDesc);
                    }
                }
                
                var block = new DataBlockSavedPart ();
                description.Add (socket, block);
                
                block.version = EquipmentUtility.generationVersionExpected;
                block.serial = -1;
                block.level = level;
                block.rating = ratingResolved;
                block.preset = partPresetKey;
                block.integrity = 1f;
                block.barrier = 1f;
                block.sockets = sockets;
                block.hardpoints = hardpoints;
                block.livery = partLiveryName;
                block.salvageable = false;
                block.inventoryAdded = false;
                block.systems = subsystemsCompiled;
                
                EquipmentGenUtility.ReturnTempGenerationData (partGenerationLayout);

                if (logExtended)
                    Debug.Log ($"Socket {socket} | L{level} | R{ratingResolved} | Subsystems: {subsystemsCompiled.Count}\n{block.systems.ToStringFormattedKeyValuePairs (true, multilinePrefix: "- ", toStringOverride: (x) => x.blueprint)}");

                #if UNITY_EDITOR
                block.SetInspectorData (socket, true, false, true);
                #endif
            }

            if (saveForDebugging)
            {
                if (DataMultiLinkerUnitPreset.unitsGenerated != null)
                    DataMultiLinkerUnitPreset.unitsGenerated.Add (new DataBlockUnitDescriptionDebug
                    {
                        preset = preset?.key, 
                        factionDataSource = factionData != null ? factionData.key : null,
                        qualityTableKey = equipmentQualityTableKey,
                        quality = rating,
                        description = description
                    });
            }

            return description;
        }

        private const string tagPrefixArmor = "armor_";
        private static List<SortedDictionary<string, bool>> partFilterStackFromFaction = new List<SortedDictionary<string, bool>> ();
        private static SortedDictionary<string, bool> partFilterCombined = new SortedDictionary<string, bool> ();
        private static List<string> partPresetsFiltered = new List<string> ();
        private static List<string> partFilterKeysToRemove = new List<string> ();

        private static void TrimConflictsWithPrefixAndValue 
        (
            SortedDictionary<string, bool> filterModified, 
            SortedDictionary<string, bool> filterIncoming, 
            string prefix,
            bool expectedValue
        )
        {
            bool conflictFound = false;
            foreach (var kvp in filterIncoming)
            {
                var tag = kvp.Key;
                bool required = kvp.Value;
                
                if (required != expectedValue || !tag.StartsWith (prefix))
                    continue;

                conflictFound = true;
                break;
            }
            
            if (!conflictFound)
                return;
        
            partFilterKeysToRemove.Clear ();
            
            foreach (var kvp in filterModified)
            {
                var tag = kvp.Key;
                bool required = kvp.Value;
                
                if (required != expectedValue || !tag.StartsWith (prefix))
                    continue;
                
                partFilterKeysToRemove.Add (tag);
            }

            foreach (var tag in partFilterKeysToRemove)
                filterModified.Remove (tag);
        }

        private static string GetPartPresetKey
        (
            string unitPresetKey,
            string socketKey,
            SortedDictionary<string, bool> partFilterFromBlueprint,
            SortedDictionary<string, bool> partFilterFromPresetPrefs,
            SortedDictionary<string, bool> partFilterFromPresetOverrides,
            SortedDictionary<string, bool> partFilterCustom,
            int rating,
            bool logExtended,
            int depth
        )
        {
            string partPresetKey = null;
            partFilterCombined.Clear ();
            partPresetsFiltered.Clear ();

            bool ratingChecked = rating >= 0;

            // Unit blueprints can apply additional constraints: for instance, block matches for certain tags
            if (partFilterFromBlueprint != null && partFilterFromBlueprint.Count > 0)
            {
                foreach (var kvp1 in partFilterFromBlueprint)
                {
                    string tag = kvp1.Key;
                    bool required = kvp1.Value;

                    partFilterCombined.Add (tag, required);
                }
            }

            // See if the stack of filters from faction still contains anything
            if (partFilterStackFromFaction.Count > 0)
            {
                var factionTagRequirementsTop = partFilterStackFromFaction[0];
                if (factionTagRequirementsTop != null && factionTagRequirementsTop.Count > 0)
                {
                    foreach (var kvp in factionTagRequirementsTop)
                    {
                        var tag = kvp.Key;
                        var required = kvp.Value;
                        
                        // Since this is happening after unit filter addition, we need to check whether tag is already added
                        if (partFilterCombined.ContainsKey (tag))
                            partFilterCombined[tag] = required;
                        else
                            partFilterCombined.Add (kvp.Key, kvp.Value);
                    }
                }
                
                // Pop the top entry from the stack - if we fail to select a part, we'll be able to do another pass with the next filter
                // tagFilterStackFromFaction.RemoveAt (0);
            }

            // Tag filter from socket tag based preset collection is merged next, to allow overriding of merge results above
            if (partFilterFromPresetPrefs != null && partFilterFromPresetPrefs.Count > 0)
            {
                // If this filter has rating requirement, we want to avoid conflict with auto-injected tag
                TrimConflictsWithPrefixAndValue (partFilterCombined, partFilterFromPresetPrefs, UnitEquipmentQuality.tagPrefix, true);
            
                foreach (var kvp in partFilterFromPresetPrefs)
                {
                    string tag = kvp.Key;
                    bool required = kvp.Value;

                    if (partFilterCombined.ContainsKey (tag))
                        partFilterCombined[tag] = required;
                    else
                        partFilterCombined.Add (kvp.Key, kvp.Value);
                }
            }
            
            // Tag filter from socket key based preset overrides is merged last, to give them a chance to override things at last possible moment
            if (partFilterFromPresetOverrides != null && partFilterFromPresetOverrides.Count > 0)
            {
                // If this filter has rating requirement, we want to avoid conflict with auto-injected tag
                TrimConflictsWithPrefixAndValue (partFilterCombined, partFilterFromPresetOverrides, UnitEquipmentQuality.tagPrefix, true);
                
                foreach (var kvp in partFilterFromPresetOverrides)
                {
                    string tag = kvp.Key;
                    bool required = kvp.Value;

                    if (partFilterCombined.ContainsKey (tag))
                        partFilterCombined[tag] = required;
                    else
                        partFilterCombined.Add (kvp.Key, kvp.Value);
                }
            }
            
            // Tag filter from socket key based preset overrides is merged last, to give them a chance to override things at last possible moment
            if (partFilterCustom != null && partFilterCustom.Count > 0)
            {
                foreach (var kvp in partFilterCustom)
                {
                    string tag = kvp.Key;
                    bool required = kvp.Value;

                    if (partFilterCombined.ContainsKey (tag))
                        partFilterCombined[tag] = required;
                    else
                        partFilterCombined.Add (kvp.Key, kvp.Value);
                }
            }

            if (partFilterCombined.Count > 0)
            {
                // Now get a random part preset key using a given filter
                var partPresetsFromTags = DataTagUtility.GetContainersWithTags 
                (
                    DataMultiLinkerPartPreset.data, 
                    partFilterCombined
                );
                
                if (partPresetsFromTags.Count > 0)
                {
                    string presetFromTagNameLast = null;
                    
                    // Filtering out presets based on factors not covered by tagging: hidden flag, missing or mismatched socket
                    foreach (var entry in partPresetsFromTags)
                    {
                        var presetFromTag = entry as DataContainerPartPreset;
                        if (presetFromTag == null || presetFromTag.hidden || presetFromTag.socketsProcessed == null)
                            continue;
                        
                        // Discard candidates that have constrained rating
                        if (ratingChecked)
                        {
                            if (presetFromTag.ratingRangeProcessed != null)
                            {
                                if (!presetFromTag.ratingRangeProcessed.IsPassed (rating))
                                    continue;
                            }
                        }

                        // Useful for logging socket based rejections
                        presetFromTagNameLast = presetFromTag.key;
                        
                        if (!presetFromTag.socketsProcessed.Contains (socketKey))
                            continue;

                        partPresetsFiltered.Add (presetFromTag.key);
                    }

                    if (partPresetsFiltered.Count > 0)
                    {
                        partPresetKey = partPresetsFiltered.GetRandomEntry ();
                        if (depth > 0 || logExtended)
                            Debug.Log ($"Unit preset {unitPresetKey} / socket {socketKey} | Depth: {depth} | Selected part preset: {partPresetKey} | Quality: {rating} | Candidates after final trim ({partPresetsFiltered.Count}): {partPresetsFiltered.TSF ()} | Filter: {partFilterCombined.TSFKVP ()}");
                    }
                    else
                        Debug.LogWarning ($"Unit preset {unitPresetKey} / socket {socketKey} | Depth: {depth} | No part presets matched required socket despite some being successfully filtered | Quality: {rating} | Candidates before final trim ({partPresetsFromTags.Count}): {partPresetsFromTags.TSF ()} | Last preset evaluated: {presetFromTagNameLast} | Filter: {partFilterCombined.TSFKVP ()}");
                }
                else
                    Debug.LogWarning ($"Unit preset {unitPresetKey} / socket {socketKey} | Depth: {depth} | No presets were found using filter {partFilterCombined.TSFKVP ()}");
            }
            else
                Debug.LogWarning ($"Unit preset {unitPresetKey} / socket {socketKey} | Depth: {depth} | Failed to combine a tag filter | Unit blueprint filter: {partFilterFromBlueprint.TSFKVP ()}");

            // If we're not successful at selecting a part and there are more filters remaining, we can try again
            if (partPresetKey == null && partFilterStackFromFaction.Count > 1)
            {
                // Pop the top entry from the stack
                partFilterStackFromFaction.RemoveAt (0);
                
                if (logExtended)
                    Debug.LogWarning ($"Unit preset {unitPresetKey} / socket {socketKey} | Depth: {depth} | Attempting to select a part using next available faction filter");
                
                partPresetKey = GetPartPresetKey 
                (
                    unitPresetKey, 
                    socketKey,
                    partFilterFromBlueprint, 
                    partFilterFromPresetPrefs,
                    partFilterFromPresetOverrides,
                    partFilterCustom,
                    rating,
                    logExtended,
                    depth + 1
                );
            }

            return partPresetKey;
        }
    }
}