using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public static class DataHelperUnitEquipment
    {
        private static List<string> fallbackStringList = new List<string> ();

        private static bool initialized = false;
        
        public static void InvalidateLookups ()
        {
            initialized = false;
            CheckSetup ();
        }
        
        public static void CheckSetup ()
        {
            if (!initialized)
                Setup ();
        }
        
        public static void Setup ()
        {
            initialized = true;
            subsystemsForHardpoint = new Dictionary<string, List<string>> ();
            subsystemsForHardpointAndEmpty = new Dictionary<string, List<string>> ();

            var subsystemBlueprints = DataMultiLinkerSubsystem.data;
            if (subsystemBlueprints != null)
            {
                foreach (var kvp in subsystemBlueprints)
                {
                    if (kvp.Value.hardpointsProcessed == null)
                        continue;

                    foreach (string hardpoint in kvp.Value.hardpointsProcessed)
                    {
                        if (string.IsNullOrEmpty (hardpoint))
                            continue;
                        
                        if (!subsystemsForHardpoint.ContainsKey (hardpoint))
                        {
                            subsystemsForHardpoint.Add (hardpoint, new List<string> ());
                            subsystemsForHardpointAndEmpty.Add (hardpoint, new List<string> ());
                            subsystemsForHardpointAndEmpty[hardpoint].Add (string.Empty);
                        }
                        
                        if (!subsystemsForHardpoint[hardpoint].Contains (kvp.Key))
                        {
                            subsystemsForHardpoint[hardpoint].Add (kvp.Key);
                            subsystemsForHardpointAndEmpty[hardpoint].Add (kvp.Key);
                        }
                    }
                }
            }

            partPresetsForSocket = new Dictionary<string, List<string>> ();
            partPresetsForSocketAndEmpty = new Dictionary<string, List<string>> ();
            
            socketDataSorted.Clear ();
            var sockets = DataMultiLinkerPartSocket.data;
            
            foreach (var kvp in sockets)
            {
                var socket = kvp.Key;
                var data = kvp.Value;
                
                socketDataSorted.Add (data);
                partPresetsForSocket.Add (socket, new List<string> ());
                partPresetsForSocketAndEmpty.Add (socket, new List<string> ());
                partPresetsForSocketAndEmpty[socket].Add (string.Empty);
            }
            
            socketDataSorted.Sort ((a, b) => a.priority.CompareTo (b.priority));
            socketKeysSorted.Clear ();
            socketKeysSortedWithEmpty.Clear ();
            socketKeysSortedWithEmpty.Add (string.Empty);
            socketKeysSortedBody.Clear ();
            socketKeysSetBody.Clear ();
            
            foreach (var data in socketDataSorted)
            {
                socketKeysSorted.Add (data.key);
                socketKeysSortedWithEmpty.Add (data.key);

                if (data.tags != null && data.tags.Contains (socketTagBody))
                {
                    socketKeysSortedBody.Add (data.key);
                    socketKeysSetBody.Add (data.key);
                }
            }

            var partPresets = DataMultiLinkerPartPreset.data;
            foreach (var kvp in partPresets)
            {
                var partPreset = kvp.Value;
                if (partPreset.socketsProcessed == null || partPreset.hidden)
                    continue;

                foreach (var socket in partPreset.socketsProcessed)
                {
                    if (string.IsNullOrEmpty (socket))
                        continue;

                    if (partPresetsForSocket.ContainsKey (socket))
                    {
                        var partPresetsList = partPresetsForSocket[socket];
                        if (!partPresetsList.Contains (partPreset.key))
                            partPresetsList.Add (partPreset.key);
                    }
                    
                    if (partPresetsForSocketAndEmpty.ContainsKey (socket))
                    {
                        var partPresetsList = partPresetsForSocketAndEmpty[socket];
                        if (!partPresetsList.Contains (partPreset.key))
                            partPresetsList.Add (partPreset.key);
                    }
                }
            }
            
            classTags = new List<string> ();
            classTags.Add (UnitClassKeys.mech);
            classTags.Add (UnitClassKeys.tank);
            classTags.Add (UnitClassKeys.turret);
            classTags.Add (UnitClassKeys.system);
        }
        

        public static List<string> GetFactions ()
        {
            return Factions.GetList();
        }

        private static List<DataContainerPartSocket> socketDataSorted = new List<DataContainerPartSocket> ();
        
        private static List<string> socketKeysSorted = new List<string> ();
        private static List<string> socketKeysSortedWithEmpty = new List<string> ();
        private static List<string> socketKeysSortedBody = new List<string> ();
        private static HashSet<string> socketKeysSetBody = new HashSet<string> ();

        public static string socketTagBody = "body";
        public static string socketBack = "back";

        public static List<string> GetSockets ()
        {
            CheckSetup ();
            return socketKeysSorted;
        }
        
        public static List<string> GetSocketsBody ()
        {
            CheckSetup ();
            return socketKeysSortedBody;
        }
        
        public static HashSet<string> GetSocketsBodySet ()
        {
            CheckSetup ();
            return socketKeysSetBody;
        }
        
        public static List<string> GetSocketsWithEmpty ()
        {
            CheckSetup ();
            return socketKeysSortedWithEmpty;
        }
        
        
        private static List<string> classTags;

        public static List<string> GetClassTags ()
        {
            CheckSetup ();
            if (classTags == null)
                return fallbackStringList;
            return classTags;
        }
        
        
        
        
        private static Dictionary<string, List<string>> subsystemsForHardpoint;
        private static Dictionary<string, List<string>> subsystemsForHardpointAndEmpty;

        public static List<string> GetSubsystemsForHardpoint (string hardpoint, bool includeEmpty = false)
        {
            CheckSetup ();
            if (string.IsNullOrEmpty (hardpoint))
            {
                Debug.LogWarning ($"Bad request for subsystems for hardpoint: null or empty hardpoint key");
                return fallbackStringList;
            }
            
            var subsystems = includeEmpty ? subsystemsForHardpointAndEmpty : subsystemsForHardpoint;
            if (subsystems == null)
            {
                Debug.LogWarning ($"Failed to find subsystem lists for hardpoint {hardpoint}");
                return fallbackStringList;
            }
            
            if (!subsystems.ContainsKey (hardpoint))
            {
                Debug.LogWarning ($"Failed to find any subsystems utilizing hardpoint {hardpoint}");
                return fallbackStringList;
            }
            
            return subsystems[hardpoint];
        }
        
        
        

        private static Dictionary<string, List<string>> partPresetsForSocket;
        private static Dictionary<string, List<string>> partPresetsForSocketAndEmpty;
        
        public static List<string> GetPartPresetsForSocket (string socket, bool includeEmpty = false)
        {
            CheckSetup ();
            var presets = includeEmpty ? partPresetsForSocketAndEmpty : partPresetsForSocket;
            if (string.IsNullOrEmpty (socket) || presets == null || !presets.ContainsKey (socket))
                return fallbackStringList;
            return presets[socket];
        }
        
        private static List<string> keysReturned = new List<string> ();

        public static List<string> FilterPartPresetsForSocket (string socket, SortedDictionary<string, bool> filter)
        {
            bool socketChecked = !string.IsNullOrEmpty (socket);
            var partPresetsFromTags = DataTagUtility.GetContainersWithTags 
            (
                DataMultiLinkerPartPreset.data, 
                filter
            );

            keysReturned.Clear ();
            if (partPresetsFromTags.Count == 0)
                return keysReturned;

            // Filtering out presets based on factors not covered by tagging: hidden flag, missing or mismatched socket
            foreach (var entry in partPresetsFromTags)
            {
                var presetFromTag = entry as DataContainerPartPreset;
                if (presetFromTag == null || presetFromTag.hidden || presetFromTag.socketsProcessed == null)
                    continue;

                if (socketChecked && !presetFromTag.socketsProcessed.Contains (socket))
                    continue;

                keysReturned.Add (presetFromTag.key);
            }

            return keysReturned;
        }

        private static SortedDictionary<string, bool> filterCombinedTemp = new SortedDictionary<string, bool> ();
        
        public static List<string> FilterPartPresetsForSocketGroup 
        (
            string socketGroup, 
            SortedDictionary<string, bool> filter, 
            string unitBlueprintKey = null,
            string factionBranchKey = null
        )
        {
            var socketData = DataMultiLinkerPartSocket.data;
            var tagsMap = DataMultiLinkerPartSocket.tagsMap;
            
            bool socketGroupChecked = !string.IsNullOrEmpty (socketGroup);
            HashSet<string> socketsInGroup = null;
            socketGroupChecked = socketGroupChecked && tagsMap.TryGetValue (socketGroup, out socketsInGroup);
            if (socketsInGroup == null)
                socketGroupChecked = false;
            
            var filterFinal = filter;
            SortedDictionary<string, bool> filterFromBlueprint = null;
            SortedDictionary<string, bool> filterFromFaction = null;

            if (!string.IsNullOrEmpty (unitBlueprintKey))
            {
                var unitBlueprint = DataMultiLinkerUnitBlueprint.GetEntry (unitBlueprintKey, false);
                if (unitBlueprint != null)
                {
                    filterFromBlueprint = 
                        unitBlueprint.partPresetFilter != null && 
                        unitBlueprint.partPresetFilter.Count > 0 ? 
                        unitBlueprint.partPresetFilter : 
                        null;
                    
                    if (socketGroupChecked && !string.IsNullOrEmpty (factionBranchKey) && !string.IsNullOrEmpty (unitBlueprint.classTag))
                    {
                        var factionData = DataMultiLinkerOverworldFactionBranch.GetEntry (factionBranchKey, false);
                        if (factionData != null)
                        {
                            var filtersFromFactionAll = 
                                factionData.unitPartFilters != null && 
                                factionData.unitPartFilters.ContainsKey (unitBlueprint.classTag) ? 
                                factionData.unitPartFilters[unitBlueprint.classTag].filters : 
                                null;

                            // Find a match to socket tag, fill the mutable list
                            if (filtersFromFactionAll != null && filtersFromFactionAll.TryGetValue (socketGroup, out var filterFromFactionList))
                            {
                                if (filterFromFactionList != null && filterFromFactionList.Count > 0)
                                    filterFromFaction = filterFromFactionList[0].tags;
                            }
                        }
                    }
                }
            }

            if (filterFromBlueprint != null || filterFromFaction != null)
            {
                filterFinal = filterCombinedTemp;
                filterFinal.Clear ();
                
                if (filterFromBlueprint != null)
                {
                    foreach (var kvp1 in filterFromBlueprint)
                    {
                        string tag = kvp1.Key;
                        bool required = kvp1.Value;
                        filterFinal[tag] = required;
                    }
                }
                
                if (filterFromFaction != null)
                {
                    foreach (var kvp1 in filterFromFaction)
                    {
                        string tag = kvp1.Key;
                        bool required = kvp1.Value;
                        filterFinal[tag] = required;
                    }
                }
                
                if (filter != null)
                {
                    foreach (var kvp1 in filter)
                    {
                        string tag = kvp1.Key;
                        bool required = kvp1.Value;
                        filterFinal[tag] = required;
                    }
                }
            }

            var partPresetsFromTags = DataTagUtility.GetContainersWithTags 
            (
                DataMultiLinkerPartPreset.data, 
                filterFinal
            );

            keysReturned.Clear ();
            if (partPresetsFromTags.Count == 0)
                return keysReturned;

            // Filtering out presets based on factors not covered by tagging: hidden flag, missing or mismatched socket
            foreach (var entry in partPresetsFromTags)
            {
                var presetFromTag = entry as DataContainerPartPreset;
                if (presetFromTag == null || presetFromTag.hidden || presetFromTag.socketsProcessed == null)
                    continue;

                if (socketGroupChecked && tagsMap != null)
                {
                    bool socketInGroup = false;
                    foreach (var socket in presetFromTag.socketsProcessed)
                    {
                        if (!string.IsNullOrEmpty (socket) && socketsInGroup.Contains (socket))
                        {
                            socketInGroup = true;
                            break;
                        }
                    }
                    
                    if (!socketInGroup)
                        continue;
                }

                keysReturned.Add (presetFromTag.key);
            }

            return keysReturned;
        }
    }
}

