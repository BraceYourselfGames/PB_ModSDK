using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitLiveryPreset : DataMultiLinker<DataContainerUnitLiveryPreset>
    {
        public DataMultiLinkerUnitLiveryPreset ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showProcessing = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [HideInEditorMode]
        [FoldoutGroup ("Utilities", false)]
        [PropertyOrder (-2), InlineButton ("SaveFromCustomization", "Save")]
        public string savedPresetName = "";
        
        public static void OnAfterDeserialization ()
        {
            foreach (var kvp in data)
            {
                var preset = kvp.Value;
                ProcessRecursive (preset, preset, 0);
            }
        }
        
        private static void ProcessRecursive (DataContainerUnitLiveryPreset current, DataContainerUnitLiveryPreset root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null current preset step or root preset reference while processing livery preset hierarchy");
                return;
            }
            
            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered infinite dependency loop at depth level {depth} when processing livery preset {root.key}");
                return;
            }

            // If current object has root node and origin object doesn't, we can fill this
            if (root.nodeProcessed == null && current.node != null)
            {
                root.nodeProcessed = new DataBlockUnitLiveryPresetNode ();
                root.nodeProcessed.livery = current.node.livery;
                root.nodeProcessed.RefreshPreview ();
            }

            // If current object has sockets collection, we can check for nested overrides there 
            if (current.sockets != null)
            {
                foreach (var kvp1 in current.sockets)
                {
                    var socket = kvp1.Key;
                    var currentSocketInfo = kvp1.Value;
                    if (currentSocketInfo == null)
                        continue;

                    // If current object has a node inside a socket, we need to fetch (if it exists, otherwise add) root object socket info
                    // Then we need to fill its node with copied values from current object socket info
                    DataBlockUnitLiveryPresetSocket rootSocketInfo = null;
                    if (currentSocketInfo.node != null)
                    {
                        rootSocketInfo = GetOrCreateRootSocketInfo (root, socket);
                        rootSocketInfo.node = new DataBlockUnitLiveryPresetNode ();
                        rootSocketInfo.node.livery = currentSocketInfo.node.livery;
                        rootSocketInfo.node.RefreshPreview ();
                    }
                    
                    // If current sub-object has hardpoints collection, we can check for nested overrides there
                    if (currentSocketInfo.hardpoints != null)
                    {
                        foreach (var kvp2 in currentSocketInfo.hardpoints)
                        {
                            var hardpoint = kvp2.Key;
                            var currentHardpointInfo = kvp2.Value;
                            if (currentHardpointInfo == null)
                                continue;
                            
                            // If current object has a node inside a hardpoint, we need to fetch (if it exists, otherwise add) root object socket info
                            // Next, we need to fetch (if it exists, otherwise add) root object hardpoint info
                            // Then we need to fill its node with copied values from current object socket info
                            DataBlockUnitLiveryPresetNode rootHardpointInfo = null;
                            rootSocketInfo = GetOrCreateRootSocketInfo (root, socket);
                            rootHardpointInfo = GetOrCreateRootHardpointInfo (rootSocketInfo, hardpoint);
                            rootHardpointInfo.livery = currentHardpointInfo.livery;
                            rootHardpointInfo.RefreshPreview ();
                        }
                    }
                }
            }
            
            if (string.IsNullOrEmpty (current.parent))
                return;
            
            if (depth > 0)
                root.parentHierarchy += $"←  {current.parent}  ";

            if (current.key == current.parent)
            {
                Debug.LogWarning ($"Livery preset {current.key} references itself as a parent, which is invalid configuration - overriding");
                current.parent = null;
                return;
            }

            var parent = GetEntry (current.parent);
            if (parent == null)
            {
                Debug.LogWarning ($"Failed to find parent {current.parent} for livery preset {current.key}");
                return;
            }

            ProcessRecursive (parent, root, depth + 1);
        }

        private static DataBlockUnitLiveryPresetSocket GetOrCreateRootSocketInfo (DataContainerUnitLiveryPreset root, string socket)
        {
            if (root.socketsProcessed == null)
                root.socketsProcessed = new Dictionary<string, DataBlockUnitLiveryPresetSocket> ();
        
            DataBlockUnitLiveryPresetSocket rootSocketInfo = null;
            if (root.socketsProcessed.ContainsKey (socket))
                rootSocketInfo = root.socketsProcessed[socket];
            else
            {
                rootSocketInfo = new DataBlockUnitLiveryPresetSocket ();
                root.socketsProcessed.Add (socket, rootSocketInfo);
            }
            return rootSocketInfo;
        }
        
        private static DataBlockUnitLiveryPresetNode GetOrCreateRootHardpointInfo (DataBlockUnitLiveryPresetSocket rootSocketInfo, string hardpoint)
        {
            if (rootSocketInfo.hardpoints == null)
                rootSocketInfo.hardpoints = new Dictionary<string, DataBlockUnitLiveryPresetNode> ();
            
            DataBlockUnitLiveryPresetNode rootHardpointInfo = null;
            if (rootSocketInfo.hardpoints.ContainsKey (hardpoint))
                rootHardpointInfo = rootSocketInfo.hardpoints[hardpoint];
            else
            {
                rootHardpointInfo = new DataBlockUnitLiveryPresetNode ();
                rootSocketInfo.hardpoints.Add (hardpoint, rootHardpointInfo);
            }
            return rootHardpointInfo;
        }

        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void GeneratePresets ()
        {
            var branches = new Dictionary<string, HashSet<string>>
            {
                { "inv_reserves", new HashSet<string> { LoadoutSockets.leftOptionalPart, LoadoutSockets.leftEquipment } },
                { "inv_army", new HashSet<string> { LoadoutSockets.leftOptionalPart, LoadoutSockets.leftEquipment } },
                { "inv_exp", new HashSet<string> { LoadoutSockets.leftOptionalPart, LoadoutSockets.leftEquipment } },
                { "inv_specops", new HashSet<string> { LoadoutSockets.leftOptionalPart, LoadoutSockets.leftEquipment } }
            };
            
            var grades = new HashSet<string>
            {
                "g1",
                "g2",
                "g3"
            };

            var roles = new HashSet<string>
            {
                "attacker",
                "berserker",
                "charger",
                "defender",
                "ranger"
            };

            foreach (var kvp1 in branches)
            {
                var branch = kvp1.Key;
                var sockets = kvp1.Value;
                
                foreach (var grade in grades)
                {
                    var keyBranchGrade = $"{branch}_{grade}";
                    if (!data.ContainsKey (keyBranchGrade))
                    {
                        var containerBranchGrade = new DataContainerUnitLiveryPreset ();
                        containerBranchGrade.name = $"{branch.FirstLetterToUpperCase ()} {grade.ToUpperInvariant ()}";
                        data.Add (keyBranchGrade, containerBranchGrade);
                        Debug.Log ($"Added branch/grade preset {keyBranchGrade}");

                        containerBranchGrade.node = new DataBlockUnitLiveryPresetNode ();
                        containerBranchGrade.node.livery = $"{branch}_{grade}";
                    }
                    else
                        Debug.LogWarning ($"Skipped existing branch/grade preset {keyBranchGrade}");
                    
                    foreach (var role in roles)
                    {
                        var keyRole = $"{branch}_{grade}_{role}";
                        if (!data.ContainsKey (keyRole))
                        {
                            var containerRole = new DataContainerUnitLiveryPreset ();
                            containerRole.name = $"{branch.FirstLetterToUpperCase ()} {grade.ToUpperInvariant ()} {role.FirstLetterToUpperCase ()}";
                            containerRole.parent = keyBranchGrade;
                            containerRole.sockets = new Dictionary<string, DataBlockUnitLiveryPresetSocket> ();
                            data.Add (keyRole, containerRole);
                            Debug.Log ($"Added branch/grade/role preset {keyRole}");

                            var roleLivery = $"inv_role_{role}";
                            foreach (var socket in sockets)
                            {
                                var socketData = new DataBlockUnitLiveryPresetSocket ();
                                socketData.node = new DataBlockUnitLiveryPresetNode ();
                                socketData.node.livery = roleLivery;
                                containerRole.sockets.Add (socket, socketData);
                            }
                        }
                        else
                            Debug.LogWarning ($"Skipped existing branch/grade/role preset {keyRole}");
                    }
                }
            }

            GenerateLiveryNames ();
        }
        
        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void GenerateLiveries ()
        {
            var branches = new HashSet<string>
            {
                "inv_reserves", 
                "inv_army",
                "inv_exp",
                "inv_specops"
            };
            
            var grades = new HashSet<string>
            {
                "g1",
                "g2",
                "g3"
            };

            var roles = new HashSet<string>
            {
                "attacker",
                "berserker",
                "charger",
                "defender",
                "ranger"
            };
            
            var dataLiveries = DataMultiLinkerEquipmentLivery.data;

            foreach (var role in roles)
            {
                var keyRole = $"inv_role_{role}";
                if (!dataLiveries.ContainsKey (keyRole))
                {
                    var container = new DataContainerEquipmentLivery ();
                    container.rating = 1;
                    container.textName = $"Invader {role.FirstLetterToUpperCase ()}";
                    container.source = "Invasion forces";
                    dataLiveries.Add (keyRole, container);
                    Debug.Log ($"Added role livery {keyRole}");
                }
                else
                    Debug.LogWarning ($"Skipped role livery {keyRole}");
            }

            foreach (var branch in branches)
            {
                foreach (var grade in grades)
                {
                    var keyBranchGrade = $"{branch}_{grade}";
                    if (!dataLiveries.ContainsKey (keyBranchGrade))
                    {
                        var container = new DataContainerEquipmentLivery ();
                        container.rating = 1;
                        container.textName = $"{branch.FirstLetterToUpperCase ()} {grade.ToUpperInvariant ()}";
                        container.source = "Invasion forces";
                        dataLiveries.Add (keyBranchGrade, container);
                        Debug.Log ($"Added branch/grade livery {keyBranchGrade}");
                    }
                    else
                        Debug.LogWarning ($"Skipped existing branch/grade livery {keyBranchGrade}");
                }
            }
        }

        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void GenerateLiveryPresetNames ()
        {
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                var split = container.key.Split ('_');

                if (split.Length == 3)
                {
                    var branch = split[1].FirstLetterToUpperCase ();
                    var grade = split[2].ToUpperInvariant ();
                    container.name = $"{branch} {grade}";
                }
                else if (split.Length == 4)
                {
                    var branch = split[1].FirstLetterToUpperCase ();
                    var grade = split[2].ToUpperInvariant ();
                    var role = split[3].FirstLetterToUpperCase ();
                    container.name = $"{branch} {grade} {role}";
                }
            }
        }
        
        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void GenerateLiveryNames ()
        {
            foreach (var kvp in DataMultiLinkerEquipmentLivery.data)
            {
                var key = kvp.Key;
                if (!key.StartsWith ("inv"))
                    continue;
                
                var container = kvp.Value;
                var split = container.key.Split ('_');

                if (split.Length == 3)
                {
                    var branch = split[1].FirstLetterToUpperCase ();
                    var grade = split[2].ToUpperInvariant ();
                    container.textName = $"{branch} {grade}";
                    container.source = branch;
                    container.rating = 1;
                }
                else if (split.Length == 4)
                {
                    var branch = split[1].FirstLetterToUpperCase ();
                    var grade = split[2].ToUpperInvariant ();
                    var role = split[3].FirstLetterToUpperCase ();
                    container.textName = $"{branch} {grade} {role}";
                    container.source = branch;
                    container.rating = 1;
                }
            }
        }

        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void AssignToUnitGroupsFromQuality () => AssignToUnitGroups (true);
        
        [PropertyOrder (-1)]
        [FoldoutGroup ("Utilities", false)]
        [Button]
        private void AssignToUnitGroupsFromGrade () => AssignToUnitGroups (false);
        
        private void AssignToUnitGroups (bool gradeFromQuality)
        {
            var branchToPrefix = new Dictionary<string, string>
            {
                { "army", "inv_army" },
                { "experimental", "inv_exp" },
                { "reserves", "inv_reserves" },
                { "specops", "inv_specops" },
                { "trainingarmy", "inv_army" },
                { "trainingreserves", "inv_reserves" }
            };
            
            var qualityToGrade = new Dictionary<int, string>
            {
                { 0, "g1" },
                { 1, "g1" },
                { 2, "g2" },
                { 3, "g3" }
            };
            
            var gradeToGrade = new Dictionary<int, string>
            {
                { 0, "g1" },
                { 1, "g2" },
                { 2, "g3" },
            };
            
            foreach (var kvp in DataMultiLinkerCombatUnitGroup.data)
            {
                var key = kvp.Key;
                var container = kvp.Value;
                if (container.unitsPerGrade == null)
                    continue;
                
                var split = container.key.Split ('_');
                if (split.Length != 2)
                    continue;

                var branch = split[0];
                if (!branchToPrefix.ContainsKey (branch))
                    continue;

                var prefix = branchToPrefix[branch];
                var role = split[1];

                if (gradeFromQuality)
                {
                    for (int i = 0; i < container.unitsPerGrade.Count; ++i)
                    {
                        var block = container.unitsPerGrade[i];
                        if (block == null || block.units == null)
                            continue;

                        block.liveryPreset = string.Empty;
                        
                        int a = 0;
                        foreach (var unitResolver in block.units)
                        {
                            if (unitResolver == null)
                                continue;

                            var quality = 1;//unitResolver.quality;
                            var grade = qualityToGrade.ContainsKey (quality) ? qualityToGrade[quality] : "g1";

                            if(!string.IsNullOrEmpty(unitResolver.qualityTableKey) && DataMultiLinkerQualityTable.data.TryGetValue(unitResolver.qualityTableKey, out var qualityTable))
	                            grade = qualityTable.liveryGrade;

                            var liveryPresetKey = $"{prefix}_{grade}_{role}";
                            unitResolver.liveryPreset = liveryPresetKey;
                            unitResolver.RefreshLiveryPresetPreview ();
                            
                            Debug.Log ($"{key} / Grade: {i} / Unit: {a} / Quality: {quality} / Livery preset: {liveryPresetKey}");
                            ++a;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < container.unitsPerGrade.Count; ++i)
                    {
                        var block = container.unitsPerGrade[i];
                        if (block == null)
                            continue;
                        
                        var grade = gradeToGrade.ContainsKey (i) ? gradeToGrade[i] : "g1";
                        var liveryPresetKey = $"{prefix}_{grade}_{role}";
                        block.liveryPreset = liveryPresetKey;
                        block.RefreshLiveryPresetPreview ();
                        Debug.Log ($"{key} / Grade: {i} / Livery preset: {liveryPresetKey}");

                        foreach (var unitResolver in block.units)
                        {
                            if (unitResolver == null)
                                continue;

                            unitResolver.liveryPreset = string.Empty;
                        }
                    }
                }
            }
        }
    }
}


