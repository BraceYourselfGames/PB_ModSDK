using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhantomBrigade.Functions.Equipment;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPartPreset : DataMultiLinker<DataContainerPartPreset>
    {
        public DataMultiLinkerPartPreset ()
        {
            textSectorKeys = new List<string> { TextLibs.equipmentPartPresets };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.equipmentPartPresets),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.equipmentPartPresets)
            );
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showUI = true;
            
            [ShowInInspector]
            public static bool showSockets = true;
        
            [ShowInInspector]
            public static bool showSystems = true;
            
            [ShowInInspector]
            public static bool showGeneration = true;
            
            [ShowInInspector]
            public static bool showComments = true;
            
            [ShowInInspector]
            public static bool showTags = true;

            [ShowInInspector]
            public static bool showTagCollections = false;
            
            [ShowInInspector]
            public static bool showPredictedEffect = false;
            
            [ShowInInspector]
            public static bool showHierarchy = false;
            
            [ShowInInspector]
            public static bool showVisuals = false;
            
            [ShowInInspector]
            public static bool showWorkshop = false;
            
            [ShowInInspector]
            [ShowIf ("showInheritance")]
            [InfoBox ("Warning: this mode triggers inheritance processing every time inheritable fields are modified. This is useful for instantly previewing changes to things like stat or inherited text, but it can cause data loss if you are not careful. Only currently modified config is protected from being reloaded, save if you switch to another config.", VisibleIf = "autoUpdateInheritance")]
            public static bool autoUpdateInheritance = false;
            
            [ShowInInspector]
            public static bool showInheritance = false;

            [ShowInInspector]
            public static bool logUpdates = false;

            [ShowInInspector]
            public static int debugGenerationLevel
            {
                get => EquipmentUtility.debugGenerationLevel;
                set => EquipmentUtility.debugGenerationLevel = value;
            }
            
            [ShowInInspector]
            public static int debugGenerationRating
            {
                get => EquipmentUtility.debugGenerationRating;
                set => EquipmentUtility.debugGenerationRating = value;
            }
        }

        [FoldoutGroup ("View", false), ShowInInspector, HideLabel]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerPartPreset.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerPartPreset.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        [ShowIf ("@DataMultiLinkerPartPreset.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> qualityMap = new Dictionary<string, HashSet<string>> ();
        
        [ShowIf ("@DataMultiLinkerPartPreset.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, string> subsystemFixedUsageMap = new Dictionary<string, string> ();
        
        [ShowIf ("@DataMultiLinkerPartPreset.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static HashSet<string> keysWorkshopUnlockable = new HashSet<string> ();

        private static StringBuilder sb = new StringBuilder ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }

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
            
            subsystemFixedUsageMap.Clear ();
            keysWorkshopUnlockable.Clear ();
            
            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
        }
        
        public void OnDestroy ()
        {
            #if UNITY_EDITOR
            DestroyVisual ();
            #endif
        }

        private static List<DataContainerPartPreset> presetsUpdated = new List<DataContainerPartPreset> ();

        public static void ProcessRelated (DataContainerPartPreset origin)
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

        private static void Postprocess (DataContainerPartPreset preset)
        {
            preset.UpdateGroups ();
            preset.SortGenSteps ();

            if (preset.genSteps != null)
            {
                foreach (var genStep in preset.genSteps)
                {
                    if (genStep == null)
                        continue;

                    if (genStep is AddHardpoints genStepHardpoint)
                    {
                        if (genStepHardpoint.subsystemsInitial != null && genStepHardpoint.subsystemsInitial.Count == 1)
                        {
                            var subsystemKey = genStepHardpoint.subsystemsInitial[0];
                            if (!string.IsNullOrEmpty (subsystemKey))
                                subsystemFixedUsageMap[subsystemKey] = preset.key;
                        }
                    }
                }
            }

            if (!preset.hidden && preset.workshopInfoProc != null)
            {
                if (preset.tagsProcessed == null || !preset.tagsProcessed.Contains (EquipmentTags.incompatible))
                    keysWorkshopUnlockable.Add (preset.key);
            }
            
            #if !PB_MODSDK
            if (Application.isPlaying && IDUtility.IsGameLoaded ())
            {
                var equipment = Contexts.sharedInstance.equipment;
                var entities = equipment.GetEntitiesWithDataKeyPartPreset (preset.key);
                if (entities != null && entities.Count > 0)
                {
                    Debug.Log ($"Replacing reference to preset {preset.key} on {entities.Count} parts. It is recommended to restart the game to fully apply the changes.");
                    foreach (var entity in entities)
                        entity.ReplaceDataLinkPartPreset (preset);
                }
            }
            #endif
        }
        
        private static void ProcessRecursiveStart (DataContainerPartPreset origin)
        {
            if (origin == null)
                return;
            
            origin.socketsProcessed.Clear ();
            origin.tagsProcessed.Clear ();

            if (origin.genStepsProcessed != null)
                origin.genStepsProcessed.Clear ();
            
            if (origin.textNameProcessed != null)
                origin.textNameProcessed.s = string.Empty;

            if (origin.textDescProcessed != null)
                origin.textDescProcessed.s = string.Empty;

            if (origin.parents != null)
            {
                foreach (var parent in origin.parents)
                {
                    if (parent != null)
                        parent.hierarchy = string.Empty;
                }
            }

            origin.ratingRangeProcessed = null;
            origin.workshopInfoProc = null;

            ProcessRecursive (origin, origin, 0);
        }

        private static void ProcessRecursive (DataContainerPartPreset current, DataContainerPartPreset root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null part preset step or root part preset reference while processing part preset hierarchy");
                return;
            }
            
            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered infinite dependency loop at depth level {depth} when processing part preset {root.key}");
                return;
            }

            if (current.textName != null && !string.IsNullOrEmpty (current.textName.s))
            {
                if (root.textNameProcessed == null)
                    root.textNameProcessed = new DataBlockEquipmentTextName ();
                
                if (string.IsNullOrEmpty (root.textNameProcessed.s))
                    root.textNameProcessed.s = current.textName.s;
                else
                {
                    var s = root.textNameProcessed.s;
                    if (current.textName.split)
                    {
                        if (s.EndsWith (")"))
                            s = s.Substring (0, s.Length - 1) + $", {current.textName.s})";
                        else
                            s = $"{s} ({current.textName.s})";
                    }
                    else
                        s = $"{current.textName.s}{s}";
                    
                    root.textNameProcessed.s = s;
                }
            }

            if (current.textDesc != null && !string.IsNullOrEmpty (current.textDesc.s))
            {
                if (root.textDescProcessed == null)
                    root.textDescProcessed = new DataBlockEquipmentTextDesc ();
                
                if (string.IsNullOrEmpty (root.textDescProcessed.s))
                    root.textDescProcessed.s = current.textDesc.s;
                else
                {
                    if (current.textDesc.split)
                        root.textDescProcessed.s = $"{root.textDescProcessed.s}\n\n{current.textDesc.s}";
                    else
                        root.textDescProcessed.s = $"{root.textDescProcessed.s} {current.textDesc.s}";
                }
            }

            if (current.textNameFromRating != null && root.textNameFromRatingProcessed == null)
            {
                root.textNameFromRatingProcessed = current.textNameFromRating;
            }
            
            if (current.textDescFromRating != null && root.textDescFromRatingProcessed == null)
            {
                root.textDescFromRatingProcessed = current.textDescFromRating;
            }
            
            if (current.ratingRange != null && root.ratingRangeProcessed == null)
            {
                root.ratingRangeProcessed = current.ratingRange;
            }
            
            if (current.workshopInfo != null)
            {
                if (root.workshopInfoProc == null)
                    root.workshopInfoProc = new SortedDictionary<int, WorkshopItemData> ();
                
                foreach (var kvp in current.workshopInfo)
                {
                    int rating = kvp.Key;
                    var workshopInfoCurrent = kvp.Value;
                    if (workshopInfoCurrent == null)
                        continue;
                    
                    var workshopInfoRoot = root.workshopInfoProc.TryGetValue (rating, out var w) ? w : null;
                    if (workshopInfoRoot == null)
                    {
                        workshopInfoRoot = new WorkshopItemData ();
                        root.workshopInfoProc[rating] = workshopInfoRoot;
                    }
                    
                    if (workshopInfoCurrent.progressLimit != null && workshopInfoRoot.progressLimit == null)
                        workshopInfoRoot.progressLimit = workshopInfoCurrent.progressLimit;
                    
                    if (workshopInfoCurrent.inputResourcesViaStats != null && workshopInfoRoot.inputResourcesViaStats == null)
                        workshopInfoRoot.inputResourcesViaStats = workshopInfoCurrent.inputResourcesViaStats;
                    
                    if (workshopInfoCurrent.inputResources != null && workshopInfoRoot.inputResources == null)
                        workshopInfoRoot.inputResources = workshopInfoCurrent.inputResources;
                    
                    if (workshopInfoCurrent.basePartRequirements != null && workshopInfoRoot.basePartRequirements == null)
                        workshopInfoRoot.basePartRequirements = workshopInfoCurrent.basePartRequirements;
                }
            }

            if (current.sockets != null)
            {
                foreach (var socket in current.sockets)
                {
                    if (!string.IsNullOrEmpty (socket) && !root.socketsProcessed.Contains (socket))
                        root.socketsProcessed.Add (socket);
                }
            }
            
            if (current.tags != null)
            {
                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProcessed.Contains (tag))
                        root.tagsProcessed.Add (tag);
                }
            }

            if (current.genSteps != null && current.genSteps.Count > 0)
            {
                if (root.genStepsProcessed == null)
                    root.genStepsProcessed = new List<IPartGenStep> (4);
                
                root.genStepsProcessed.AddRange (current.genSteps);
                // if (!root.hidden && root.key.Contains ("elbrus"))
                //     Debug.Log ($"D{depth} - {current.key}:\n" + root.genStepsProcessed.ToStringFormatted (true, toStringOverride: (a) => $"- {a.GetType ().Name} ({(a is PartGenStepTargeted at ? ((PartGenStepTargeted)a).hardpointsTargeted.ToStringFormatted () : "untargeted")}): {a.GetPriority ()}"));
            }

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Part preset {root.key} fails to complete recursive processing in under 20 steps | Current step: {current.key}");
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
                    Debug.LogWarning ($"Part preset {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Part preset {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Part preset {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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

        private static StringBuilder sbDesc = new StringBuilder ();
        private static string fallbackString = string.Empty;

        public static bool IsSubsystemUsed (DataContainerSubsystem subsystem)
        {
            if (subsystem == null)
                return false;

            if (subsystemFixedUsageMap == null || string.IsNullOrEmpty (subsystem.key))
                return false;

            if (subsystemFixedUsageMap.TryGetValue (subsystem.key, out var partKey))
            {
                var preset = GetEntry (partKey, false);
                return preset != null;
            }
            
            return false;
        }

        public static string TryGetFixedUsageDescription (DataContainerSubsystem subsystem)
        {
            if (subsystem == null)
                return null;

            if (subsystemFixedUsageMap == null || string.IsNullOrEmpty (subsystem.key))
                return null;

            if (subsystemFixedUsageMap.TryGetValue (subsystem.key, out var partKey))
            {
                var preset = GetEntry (partKey, false);
                sbDesc.Clear ();
                sbDesc.Append (partKey);
                
                if (preset != null)
                {
                    if (preset.textNameProcessed != null && !string.IsNullOrEmpty (preset.textNameProcessed.s))
                    {
                        sbDesc.Append (" (");
                        sbDesc.Append (preset.textNameProcessed.s);
                        sbDesc.Append (")");
                    }
                }
                else
                    sbDesc.Append (" (?)");

                return sbDesc.ToString ();
            }
            else
                return null;
        }
        
        #if UNITY_EDITOR

        private static GameObject visualHolder;
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        private void DestroyVisual ()
        {
            if (visualHolder != null)
            {
                Debug.Log ("Cleaning up visual helper for DataMultiLinkerPartPreset");
                UtilityGameObjects.ClearChildren (visualHolder.transform);
                DestroyImmediate (visualHolder);
            }
        }

        private static HashSet<string> collectedVisuals = new HashSet<string> ();
        private static Dictionary<string, DataBlockSubsystemAttachment> collectedAttachments = new Dictionary<string, DataBlockSubsystemAttachment> ();

        public static void VisualizeObject (DataContainerPartPreset partPreset, bool useProcessedSystems, bool logMaterialWarnings = false)
        {
            if (partPreset == null || partPreset.genStepsProcessed == null)
                return;

            collectedVisuals.Clear ();
            collectedAttachments.Clear ();
            
            foreach (var step in partPreset.genStepsProcessed)
            {
                if (step is AddHardpoints stepHardpoint)
                {
                    if (stepHardpoint.subsystemsInitial != null)
                    {
                        foreach (var subsystemKey in stepHardpoint.subsystemsInitial)
                        {
                            var subsystem = DataMultiLinkerSubsystem.GetEntry (subsystemKey, false);
                            if (subsystem == null)
                                continue;

                            if (subsystem.visualsProcessed != null && subsystem.visualsProcessed.Count > 0)
                            {
                                foreach (var visualKey in subsystem.visualsProcessed)
                                {
                                    if (!string.IsNullOrEmpty (visualKey) && !collectedVisuals.Contains (visualKey))
                                        collectedVisuals.Add (visualKey);
                                }
                            }

                            if (subsystem.attachmentsProcessed != null && subsystem.attachmentsProcessed.Count > 0)
                            {
                                foreach (var kvp in subsystem.attachmentsProcessed)
                                    collectedAttachments[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
            }

            if (collectedVisuals.Count == 0 && collectedAttachments.Count == 0)
                return;
                
            if (visualHolder == null)
                visualHolder = GameObject.Find ("DataPreviewHolder");
            if (visualHolder == null)
            {
                visualHolder = new GameObject ("DataPreviewHolder");
                // visualHolder.hideFlags = HideFlags.HideAndDontSave;
                visualHolder.transform.position = new Vector3 (0f, 200f, 0f);
            }

            UtilityGameObjects.ClearChildren (visualHolder.transform);

            if (collectedVisuals != null && collectedVisuals.Count > 0)
            {
                foreach (var key in collectedVisuals)
                {
                    var visualPrefab = ItemHelper.GetVisual (key);
                    if (visualPrefab == null)
                    {
                        Debug.LogWarning ($"Failed to fetch visual prefab named {key}");
                        continue;
                    }

                    var visualInstance = Instantiate (visualPrefab.gameObject).GetComponent<ItemVisual> ();
                    var t = visualInstance.transform;
                    t.name = visualPrefab.name;
                    t.parent = visualHolder.transform;
                    t.localPosition = (visualInstance.customTransform ? visualInstance.customPosition : Vector3.zero);
                    t.localRotation = (visualInstance.customTransform ? Quaternion.Euler (visualInstance.customRotation) : Quaternion.identity);
                    t.localScale = Vector3.one;

                    if (logMaterialWarnings)
                    {
                        var renderers = visualInstance.GetComponentsInChildren<Renderer> (true);
                        if (renderers != null && renderers.Length > 0)
                        {
                            for (int r = 0; r < renderers.Length; r++)
                            {
                                var renderer = renderers[r];
                                var sm = renderer.sharedMaterials;
                                if (sm == null || sm.Length == 0)
                                    continue;

                                for (int i = 0; i < sm.Length; i++)
                                {
                                    var mat = sm[i];
                                    if (mat == null)
                                        Debug.LogWarning ($"Material {i} on renderer {renderer.name} under visual {key} is null");
                                }
                            }
                        }
                    }
                }
            }
            
            if (collectedAttachments != null && collectedAttachments.Count > 0)
            {
                foreach (var kvp in collectedAttachments)
                {
                    var block = kvp.Value;
                    if (block == null)
                        continue;
                
                    var visualPrefab = ItemHelper.GetVisual (block.key);
                    if (visualPrefab == null)
                    {
                        Debug.LogWarning ($"Failed to fetch visual prefab named {block.key}");
                        continue;
                    }

                    var visualInstance = Instantiate (visualPrefab.gameObject).GetComponent<ItemVisual> ();
                    var t = visualInstance.transform;
                    t.name = visualPrefab.name;
                    t.parent = visualHolder.transform;
                    t.localPosition = (visualInstance.customTransform ? visualInstance.customPosition : Vector3.zero) + block.position;
                    t.localRotation = (visualInstance.customTransform ? Quaternion.Euler (visualInstance.customRotation) : Quaternion.identity) * Quaternion.Euler (block.rotation);
                    t.localScale = block.scale;
                    
                    if (logMaterialWarnings)
                    {
                        var renderers = visualInstance.GetComponentsInChildren<Renderer> (true);
                        if (renderers != null && renderers.Length > 0)
                        {
                            for (int r = 0; r < renderers.Length; r++)
                            {
                                var renderer = renderers[r];
                                var sm = renderer.sharedMaterials;
                                if (sm == null || sm.Length == 0)
                                    continue;

                                for (int i = 0; i < sm.Length; i++)
                                {
                                    var mat = sm[i];
                                    if (mat == null)
                                        Debug.LogWarning ($"Material {i} on renderer {renderer.name} under visual {block.key} is null");
                                }
                            }
                        }
                    }
                }
            }
            
            if (UnityEditor.SceneView.lastActiveSceneView != null)
            {
                Bounds bounds = new Bounds (visualHolder.transform.position, Vector3.zero);
                MeshRenderer[] renderers = visualHolder.GetComponentsInChildren<MeshRenderer> ();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var mr = renderers[i];
                    bounds.Encapsulate (mr.bounds);
                }

                if (renderers.Length != 0)
                    UnityEditor.SceneView.lastActiveSceneView.Frame (bounds);
                else
                    UnityEditor.SceneView.lastActiveSceneView.LookAt (visualHolder.transform.position);
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void ReplacePartTags ()
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
                
                if (preset.tags == null)
                    continue;

                foreach (var kvp2 in tagReplacementMap)
                {
                    var prefix = kvp2.Key;
                    if (!key.StartsWith (prefix))
                        continue;

                    var map = kvp2.Value;
                    
                    foreach (var kvp3 in map)
                    {
                        var partTagOld = kvp3.Key;
                        var partTagReplacement = kvp3.Value;

                        if (preset.tags.Contains (partTagOld))
                        {
                            preset.tags.Remove (partTagOld);
                            preset.tags.Add (partTagReplacement);
                            Debug.Log ($"{preset.key} | {partTagOld} -> {partTagReplacement}");
                        }
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void SortTags ()
        {
            foreach (var kvp in data)
            {
                var presetKey = kvp.Key;
                var preset = kvp.Value;

                if (preset.tags == null)
                    continue;

                var tagsList = preset.tags.ToList ();
                tagsList.Sort ();
                preset.tags = new HashSet<string> (tagsList);
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void RenameArmorConsistently ()
        {
            var keysToRename = new Dictionary<string, string> ();
            
            foreach (var kvp in data)
            {
                var presetRootKey = kvp.Key;
                var presetRoot = kvp.Value;
                
                if (!presetRootKey.StartsWith ("body_set_"))
                    continue;

                foreach (var presetAreaKey in presetRoot.children)
                {
                    var presetArea = GetEntry (presetAreaKey);
                    if (presetArea == null)
                        continue;
                    
                    var presetAreaKeySplit = presetAreaKey.Split ('_');
                    var area = presetAreaKeySplit[1];
                    var presetAreaKeyModified = $"{presetRootKey}_{area}";
                    
                    Debug.Log ($"Area part preset: {presetAreaKey} -> {presetAreaKeyModified}\nRoot: {presetRootKey}");
                    keysToRename.Add (presetAreaKey, presetAreaKeyModified);

                    foreach (var presetRatingKey in presetArea.children)
                    {
                        var presetRating = GetEntry (presetRatingKey);
                        if (presetRating == null)
                            continue;
                        
                        var presetRatingKeySplit = presetRatingKey.Split ('_');
                        var rating = presetRatingKeySplit[presetRatingKeySplit.Length - 1];
                        var presetRatingKeyModified = $"{presetRootKey}_{area}_{rating}";
                        
                        Debug.Log ($"Rating part preset: {presetRatingKey} -> {presetRatingKeyModified}\nRoot: {presetRootKey}");
                        keysToRename.Add (presetRatingKey, presetRatingKeyModified);
                    }
                }
            }

            foreach (var kvp in keysToRename)
            {
                var keyOld = kvp.Key;
                var keyNew = kvp.Value;
                ReplaceKey (keyOld, keyNew);
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void RenameArmorParents ()
        {
            foreach (var kvp in data)
            {
                var presetKey = kvp.Key;
                var preset = kvp.Value;
                
                if (!presetKey.StartsWith ("body_set_"))
                    continue;
                
                if (!presetKey.EndsWith ("_r0") && !presetKey.EndsWith ("_r1") && !presetKey.EndsWith ("_r2") && !presetKey.EndsWith ("_r3"))
                    continue;

                var link = preset.parents[0];
                var parentKeySplit = link.key.Split ('_');
                var area = parentKeySplit[1];
                var parentKeyModified = link.key.Replace ($"_{area}_", "_") + $"_{area}";
                Debug.Log ($"Replacing part preset {presetKey} parent: {link.key} -> {parentKeyModified}");
                link.key = parentKeyModified;
            }
        }
        
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void CreateBodyInternals ()
        {
            foreach (var kvp in data)
            {
                var presetRootKey = kvp.Key;
                var presetRoot = kvp.Value;
                
                if (!presetRootKey.StartsWith ("body_set_"))
                    continue;
                
                if (!presetRootKey.EndsWith ("_arm") && !presetRootKey.EndsWith ("_bottom") && !presetRootKey.EndsWith ("_top"))
                    continue;

                var presetRootKeySplit = presetRootKey.Split ('_');
                var area = presetRootKeySplit[presetRootKeySplit.Length - 1];
                var subsystemInternalKey = presetRootKey.Replace ("body_set_", "internal_main_");
                Debug.Log ($"Creating internal subsystem {subsystemInternalKey} for part preset {presetRootKey}");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void SeparateHighRatingParents ()
        {
            var parentKeyPatternToReplace = "body_flags_open";

            foreach (var kvp in data)
            {
                var presetKey = kvp.Key;
                var preset = kvp.Value;

                if (!presetKey.StartsWith ("body_set_"))
                    continue;

                if (!presetKey.EndsWith ("_r2") && !presetKey.EndsWith ("_r3"))
                    continue;
                
                if (preset.parents == null || preset.parents.Count == 0)
                    continue;

                DataBlockPartPresetParent parentBlockToEdit = null;
                foreach (var parentBlock in preset.parents)
                {
                    var parentKey = parentBlock.key;
                    if (parentKey.Contains (parentKeyPatternToReplace))
                        parentBlockToEdit = parentBlock;
                }
                
                if (parentBlockToEdit == null)
                    continue;

                var presetKeySplit = presetKey.Split ('_');
                var rating = presetKeySplit[presetKeySplit.Length - 1];
                var area = presetKeySplit[presetKeySplit.Length - 2];
                var parentKeyReplacement = $"body_flags_{rating}_{area}";
                
                Debug.Log ($"{presetKey} | Replacing parent: {parentBlockToEdit.key} -> {parentKeyReplacement}");
                parentBlockToEdit.key = parentKeyReplacement;
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void UpdateBodyParents ()
        {
            foreach (var kvp in data)
            {
                var presetKey = kvp.Key;
                var preset = kvp.Value;
                
                if (!presetKey.StartsWith ("body_set") || presetKey.Contains ("_flags_"))
                    continue;

                if (presetKey.EndsWith ("_arm") || presetKey.EndsWith ("_bottom") || presetKey.EndsWith ("_top"))
                {
                    var presetKeySplit = presetKey.Split ('_');
                    var area = presetKeySplit[presetKeySplit.Length - 1];
                    var parentKey = $"body_flags_fused_{area}";
                    preset.parents.Insert (0, new DataBlockPartPresetParent { key = parentKey });
                    Debug.Log ($"Updating preset {presetKey} with fused flag parent {parentKey}");
                }
                
                if (presetKey.EndsWith ("_r2") || presetKey.EndsWith ("_r3"))
                {
                    var presetKeySplit = presetKey.Split ('_');
                    var area = presetKeySplit[presetKeySplit.Length - 2];
                    var parentKey = $"body_flags_open_{area}";
                    preset.parents.Insert (0, new DataBlockPartPresetParent { key = parentKey });
                    Debug.Log ($"Updating preset {presetKey} with open flag parent {parentKey}");
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogHiddenAndVisible ()
        {
            var lh = new List<DataContainerPartPreset> ();
            var lv = new List<DataContainerPartPreset> ();
            
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (c.hidden)
                {
                    lh.Add (c);
                }
                else
                {
                    lv.Add (c);
                }
            }

            foreach (var c in lh)
            {
                int countR = c.genSteps != null ? c.genSteps.Count : 0;
                int countP = c.genStepsProcessed != null ? c.genStepsProcessed.Count : 0;
                Debug.Log ($"{c.key} | Hidden | Generator steps (R/P): {countR}/{countP}");
            }
            
            foreach (var c in lv)
            {
                int countR = c.genSteps != null ? c.genSteps.Count : 0;
                int countP = c.genStepsProcessed != null ? c.genStepsProcessed.Count : 0;
                if (countP > 0)
                    Debug.Log ($"{c.key} | Visible | Generator steps (R/P): {countR}/{countP}");
                else
                    Debug.LogWarning ($"{c.key} | Visible | Generator steps (R/P): {countR}/{countP} | Can't be generated!");
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogSubsystemRatingChecks ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (c == null || c.genSteps == null)
                    continue;

                for (int i = 0; i < c.genSteps.Count; ++i)
                {
                    var step = c.genSteps[i];
                    if (step is TrimSystemsByRating stepTrim)
                    {
                        Debug.Log ($"{c.key} | Step {i} trims subsystem rating | {(stepTrim.relative ? "Relative" : "Absolute")}, R{stepTrim.ratingMin}-{stepTrim.ratingMax}");
                    }
                }
            }
        }

        #endif
    }
}