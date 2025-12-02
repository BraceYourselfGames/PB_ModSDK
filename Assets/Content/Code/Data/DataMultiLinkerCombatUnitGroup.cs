using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatUnitGroup : DataMultiLinker<DataContainerCombatUnitGroup>
    {
        public DataMultiLinkerCombatUnitGroup ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            
            textSectorKeys = new List<string> { TextLibs.unitGroups };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.unitGroups),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.unitGroups)
            );
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector] public static bool showCore = true;
        
            [ShowInInspector] public static bool showText = false;

            [ShowInInspector] public static bool showTags = true;
            
            [ShowInInspector] public static bool showGrades = true;
            
            [ShowInInspector] public static bool showUnits = true;

            [ShowInInspector] public static bool showTagCollections = false;
        }

        [ShowInInspector] [HideLabel] public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerCombatUnitGroup.Presentation.showTagCollections")] [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerCombatUnitGroup.Presentation.showTagCollections")] [ShowInInspector] [ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        [FoldoutGroup ("Utilities", false)]
        [ShowInInspector][ReadOnly]
        public static Dictionary<string, List<string>> gradeUpgradeLookup = new Dictionary<string, List<string>> ();

        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }
        
        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
        
        [Button ("Log custom IDs")][FoldoutGroup ("Utilities", false)]
        public void LogCustomIdentification ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;

                if (container.unitsPerGrade != null)
                {
                    int g = 0;
                    foreach (var block in container.unitsPerGrade)
                    {
                        if (block != null && block.units != null)
                        {
                            int u = 0;
                            foreach (var unit in block.units)
                            {
                                if (unit.custom != null)
                                {
                                    var c = unit.custom;
                                    if (c.id != null)
                                        Debug.Log ($"{key} | Grade {g} | Unit {u} | Custom unit name: {c.id.nameOverride}");
                                    else if (c.idPilot != null)
                                        Debug.Log ($"{key} | Grade {g} | Unit {u} | Custom pilot name: {c.idPilot.nameOverride} / {c.idPilot.callsignOverride}");
                                }
                                ++u;
                            }
                        }
                        ++g;
                    }
                }
            }
        }

        [Button] [FoldoutGroup ("Utilities", false)]
        public void SetDefaultBranch ()
        {
            var prefix = "branch_";
            var branchKeysTrimmed = DataMultiLinkerOverworldFactionBranch.data.Keys.ToList ();
            for (int i = 0; i < branchKeysTrimmed.Count; ++i)
                branchKeysTrimmed[i] = branchKeysTrimmed[i].Replace (prefix, string.Empty);
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;

                bool found = false;
                foreach (var branchKeyTrimmed in branchKeysTrimmed)
                {
                    if (key.StartsWith (branchKeyTrimmed))
                    {
                        Debug.Log ($"{prefix}{branchKeyTrimmed}: {key}");
                        found = true;
                        container.factionBranchDefault = $"{prefix}{branchKeyTrimmed}";
                        break;
                    }
                }

                if (!found)
                    Debug.LogWarning ($"?: {key}");
            }
        }

        [Button][FoldoutGroup ("Utilities", false)]
        public void TrimUnusedCustomizations ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;

                if (container.unitsPerGrade != null)
                {
                    int g = 0;
                    foreach (var block in container.unitsPerGrade)
                    {
                        if (block != null && block.units != null)
                        {
                            int u = 0;
                            foreach (var unit in block.units)
                            {
                                if (unit.custom != null)
                                {
                                    var c = unit.custom;
                                    if 
                                    (
                                        c.name == null &&
                                        c.id == null &&
                                        c.idReused == null &&
                                        c.idPilot == null &&
                                        c.spawn == null &&
                                        c.landing == null &&
                                        c.flags == null &&
                                        c.faction == null &&
                                        c.speed == null &&
                                        c.pilotAppearance == null &&
                                        c.pilotStats == null &&
                                        c.statMultipliers == null &&
                                        c.combatTags == null
                                    )
                                    {
                                        Debug.Log ($"{key} | Grade {g} | Unit {u} | Custom block unused, clearing...");
                                        unit.custom = null;
                                    }
                                }
                                ++u;
                            }
                        }
                        ++g;
                    }
                }
            }
        }

        [Button ("Upgrade unit preset tags")] [FoldoutGroup ("Utilities", false)]
        public void UpgradePresetTags ()
        {
            foreach (var kvp in data)
            {
                var group = kvp.Value;
                if (group == null || group.unitPresets == null)
                {
                    continue;
                }

                foreach (var unitPreset in group.unitPresets)
                {
                    if (unitPreset.Key != "vehicle")
                    {
                        continue;
                    }

                    var presetLink = unitPreset.Value;

                    if (presetLink == null)
                    {
                        continue;
                    }

                    if (presetLink is DataBlockScenarioUnitFilter presetLinkFilter)
                    {
                        if (presetLinkFilter.tags == null)
                        {
                            continue;
                        }

                        presetLinkFilter.tags = new SortedDictionary<string, bool> ();

                        presetLinkFilter.tags.Add ("type_vehicle", true);


                        if (kvp.Key.Contains ("vanguard"))
                        {
                            presetLinkFilter.tags.Add ("role_vanguard", true);
                        }

                        if (kvp.Key.Contains ("guardian"))
                        {
                            presetLinkFilter.tags.Add ("role_guardian", true);
                        }

                        if (kvp.Key.Contains ("sharpshooter"))
                        {
                            presetLinkFilter.tags.Add ("role_sharpshooter", true);
                        }

                        if (kvp.Key.Contains ("support"))
                        {
                            presetLinkFilter.tags.Add ("role_support", true);
                        }

                        if (kvp.Key.Contains ("scout"))
                        {
                            presetLinkFilter.tags.Add ("role_scout", true);
                        }

                        if (kvp.Key.Contains ("striker"))
                        {
                            presetLinkFilter.tags.Add ("role_striker", true);
                        }

                        if (kvp.Key.Contains ("juggernaut"))
                        {
                            presetLinkFilter.tags.Add ("role_juggernaut", true);
                        }

                        if (kvp.Key.Contains ("leader"))
                        {
                            presetLinkFilter.tags.Add ("role_leader", true);
                        }

                        if (kvp.Key.Contains ("experimental"))
                        {
                            presetLinkFilter.tags.Add ("filter_hybrid", true);
                        }

                        if (kvp.Key.Contains ("specops"))
                        {
                            if (!kvp.Key.Contains ("0"))
                            {
                                presetLinkFilter.tags.Add ("filter_hybrid", true);
                            }
                            else
                            {
                                presetLinkFilter.tags.Add ("filter_hybrid", false);
                            }
                        }

                        if (kvp.Key.Contains ("army"))
                        {
                            if (kvp.Key.Contains ("2"))
                            {
                                presetLinkFilter.tags.Add ("filter_hybrid", true);
                            }
                            else
                            {
                                presetLinkFilter.tags.Add ("filter_hybrid", false);
                            }
                        }

                        if (kvp.Key.Contains ("reserves") || kvp.Key.Contains ("training"))
                        {
                            if (!presetLinkFilter.tags.ContainsKey ("filter_hybrid"))
                            {
                                presetLinkFilter.tags.Add ("filter_hybrid", false);
                            }
                        }
                    }
                }
            }
        }

        /*
        [Button ("Upgrade pilot presets")] [FoldoutGroup ("Utilities", false)]
        public void UpgradePilotPresets ()
        {
            foreach (var kvp in data)
            {
                var group = kvp.Value;
                if (group == null || group.units == null || group.unitPresets == null)
                {
                    continue;
                }

                foreach (var units in group.units)
                {
                    var presetName = "enemy_";
                    var tags = new SortedDictionary<string, bool> ();

                    if (units.key == "mech")
                    {
                        if (group.unitPresets["mech"] is DataBlockScenarioUnitFilter presetLinkFilter)
                        {
                            if (presetLinkFilter.tags == null)
                            {
                                continue;
                            }

                            tags = presetLinkFilter.tags;
                        }

                        presetName += "mech_pilot_";
                    }

                    if (units.key == "vehicle")
                    {
                        if (group.unitPresets["vehicle"] is DataBlockScenarioUnitFilter presetLinkFilter)
                        {
                            if (presetLinkFilter.tags == null)
                            {
                                continue;
                            }

                            tags = presetLinkFilter.tags;
                        }

                        presetName += "tank_pilot_";
                    }

                    if (units.quality == 0)
                    {
                        presetName += "r0_";
                    }

                    if (units.quality == 1)
                    {
                        presetName += "r0_";
                    }

                    if (units.quality == 2)
                    {
                        presetName += "r1_";
                    }

                    if (units.quality == 3)
                    {
                        presetName += "r2_";
                    }

                    if (tags.ContainsKey ("role_artillery"))
                    {
                        presetName += "artillery";
                    }
                    else if (tags.ContainsKey ("role_grenadier"))
                    {
                        presetName += "grenadier";
                    }
                    else if (tags.ContainsKey ("role_guardian"))
                    {
                        presetName += "guardian";
                    }
                    else if (tags.ContainsKey ("role_juggernaut"))
                    {
                        presetName += "juggernaut";
                    }
                    else if (tags.ContainsKey ("role_leader"))
                    {
                        presetName += "leader";
                    }
                    else if (tags.ContainsKey ("role_scout"))
                    {
                        presetName += "scout";
                    }
                    else if (tags.ContainsKey ("role_sharpshooter"))
                    {
                        presetName += "sharpshooter";
                    }
                    else if (tags.ContainsKey ("role_striker"))
                    {
                        presetName += "striker";
                    }

                    else if (tags.ContainsKey ("role_support"))
                    {
                        presetName += "support";
                    }
                    else if (tags.ContainsKey ("role_vanguard"))
                    {
                        presetName += "vanguard";
                    }

                    if (units?.custom?.pilot?.presetKey == null)
                    {
                        continue;
                    }

                    units.custom.pilot.presetKey = presetName;
                }
            }
        }
        */

        /*
        [Button ("Fix targeting profile key")] [FoldoutGroup ("Utilities", false)]
        public void FixTargetingKey ()
        {
            foreach (var kvp in data)
            {
                var group = kvp.Value;
                if (group == null || group.units == null || group.unitPresets == null)
                {
                    continue;
                }

                for (var s = 0; s < group.units.Count; ++s)
                {
                    var slot = group.units[s];
                    if (slot == null || string.IsNullOrEmpty (slot.key))
                    {
                        continue;
                    }

                    if (slot.aiBehavior == "Flanker")
                    {
                        slot.aiBehavior = string.Empty;
                        slot.aiTargeting = string.Empty;
                    }
                    else
                    {
                        slot.aiTargeting = slot.aiTargeting.ToLowerInvariant ();
                    }
                }
            }

            var presets = DataMultiLinkerUnitPreset.data;
            foreach (var kvp in presets)
            {
                var preset = kvp.Value;
                preset.aiBehavior = "Flanker";
                preset.aiTargeting = "default";
            }
        }
        */

        /*
        [Button ("Upgrade targeting profile key")] [FoldoutGroup ("Utilities", false)]
        public void UpgradeTargetingKey ()
        {
            foreach (var kvp in data)
            {
                var group = kvp.Value;
                if (group == null || group.units == null || group.unitPresets == null)
                {
                    continue;
                }

                for (var s = 0; s < group.units.Count; ++s)
                {
                    var slot = group.units[s];
                    if (slot == null || string.IsNullOrEmpty (slot.key))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty (slot.aiBehavior))
                    {
                        slot.aiBehavior = "Flanker";
                    }

                    if (string.IsNullOrEmpty (slot.aiTargeting))
                    {
                        if (kvp.Key.Contains ("artillery"))
                        {
                            slot.aiTargeting = "artillery";
                        }

                        if (kvp.Key.Contains ("grenadier"))
                        {
                            slot.aiTargeting = "grenadier";
                        }

                        if (kvp.Key.Contains ("vanguard"))
                        {
                            slot.aiTargeting = "vanguard";
                        }

                        if (kvp.Key.Contains ("guardian"))
                        {
                            slot.aiTargeting = "guardian";
                        }

                        if (kvp.Key.Contains ("sharpshooter"))
                        {
                            slot.aiTargeting = "sharpshooter";
                        }

                        if (kvp.Key.Contains ("support"))
                        {
                            slot.aiTargeting = "support";
                        }

                        if (kvp.Key.Contains ("scout"))
                        {
                            slot.aiTargeting = "scout";
                        }

                        if (kvp.Key.Contains ("striker"))
                        {
                            slot.aiTargeting = "striker";
                        }

                        if (kvp.Key.Contains ("juggernaut"))
                        {
                            slot.aiTargeting = "juggernaut";
                        }

                        if (kvp.Key.Contains ("leader"))
                        {
                            slot.aiTargeting = "leader";
                        }
                    }
                }
            }           
        }
        */
    }
}