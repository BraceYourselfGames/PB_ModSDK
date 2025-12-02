using System.Collections;
using System.Collections.Generic;
using System.Text;
using Area;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatArea : DataMultiLinker<DataContainerCombatArea>
    {
        public DataMultiLinkerCombatArea ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            LevelContentHelper.Initialize ();
        }

        public override bool IsUsingDirectories () => true;
        public override bool IsDisplayIsolated () => true;
        public override DataContainer GetDisplayIsolatedOverride () => selectedArea;
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showSpawns = true;
            
            [ShowInInspector]
            public static bool showSpawnRaycasts = true;
            
            [ShowInInspector]
            public static bool showSpawnGeneration = false;
            
            [ShowInInspector]
            public static bool showLocations = true;
            
            [ShowInInspector]
            public static bool showVolumes = true;
            
            [ShowInInspector]
            public static bool showFields = true;
            
            [ShowInInspector]
            public static bool showWaypoints = true;

            [ShowInInspector]
            public static bool showTags = true;
            
            [ShowInInspector]
            public static bool showSelections = true;
            
            [ShowInInspector]
            public static bool showSelectionUtils = false;
            
            [ShowInInspector]
            public static bool showInheritance = false;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
            
            [ShowInInspector]
            public static bool showHierarchy = true;
            
            [ShowInInspector]
            public static bool showSelection = false;
        }

        [FoldoutGroup ("View options", false), ShowInInspector, HideLabel]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();

        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showTagCollections")]
        [ShowInInspector]
        private static List<string> keysWithContent = new List<string> ();
        
        
        // [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections")]
        // [ShowInInspector, ReadOnly, BoxGroup ("Selections")]
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        public static DataContainerCombatArea selectedArea;
        
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections && DataMultiLinkerCombatArea.Presentation.showSpawns && DataMultiLinkerCombatArea.selectedSpawnGroup != null")]
        [ShowInInspector, BoxGroup ("Selections")]
        [LabelText ("@selectedSpawnGroup != null ? selectedSpawnGroup.key : string.Empty")]
        public static DataBlockAreaSpawnGroup selectedSpawnGroup;
        
        // [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections && DataMultiLinkerCombatArea.Presentation.showSpawns && DataMultiLinkerCombatArea.selectedSpawnGroup != null")]
        // [ShowInInspector, BoxGroup ("Selections")]
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        public static DataBlockAreaPoint selectedSpawnPoint;

        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        public static DataBlockAreaPoint selectedWaypoint;
        
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections && DataMultiLinkerCombatArea.Presentation.showLocations && DataMultiLinkerCombatArea.selectedLocation != null")]
        [ShowInInspector, BoxGroup ("Selections")]
        [LabelText ("@selectedLocation != null ? selectedLocation.key : string.Empty")]
        public static DataBlockAreaLocationTagged selectedLocation;
        
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections && DataMultiLinkerCombatArea.Presentation.showVolumes && DataMultiLinkerCombatArea.selectedVolume != null")]
        [ShowInInspector, BoxGroup ("Selections")]
        [LabelText ("@selectedVolume != null ? selectedVolume.key : string.Empty")]
        public static DataBlockAreaVolumeTagged selectedVolume;
        
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections && DataMultiLinkerCombatArea.Presentation.showFields && DataMultiLinkerCombatArea.selectedField != null")]
        [ShowInInspector, BoxGroup ("Selections")]
        [LabelText ("@selectedField != null ? selectedField.key : string.Empty")]
        public static DataBlockAreaField selectedField;
        
        [GUIColor (0.85f, 0.95f, 1.0f, 1.0f)]
        [ShowIf ("@DataMultiLinkerCombatArea.Presentation.showSelections && DataMultiLinkerCombatArea.Presentation.showWaypoints && DataMultiLinkerCombatArea.selectedWaypointGroup != null")]
        [ShowInInspector, BoxGroup ("Selections")]
        [LabelText ("@selectedWaypointGroup != null ? selectedWaypointGroup.key : string.Empty")]
        public static DataBlockAreaWaypointGroup selectedWaypointGroup;
        
        private static StringBuilder sb = new StringBuilder ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }

        public static AreaDataCore GetCurrentLevelDataCore ()
        {
            if (selectedArea == null)
                return null;

            if (selectedArea?.contentProc?.channels == null)
                return null;

            return selectedArea.contentProc.core;
        }

        public static AreaDataContainer GetCurrentLevelDataRoot ()
        {
            if (selectedArea == null)
                return null;

            if (selectedArea?.contentProc?.channels == null)
                return null;

            if (selectedArea.contentProc.core == null)
                return null;
            
            if (selectedArea.contentProc.channels.TryGetValue (AreaChannelRoot.alias, out var channel) && channel is AreaChannelRoot channelRoot)
                return channelRoot.content;
            
            return null;
        }
        
        public static string GetCurrentUnpackedLevelPath ()
        {
            var temp = GetCurrentLevelDataRoot ();
            if (temp == null)
                return null;

            return selectedArea.contentProc.pathLast;
        }

        public static List<string> GetKeysWithContent ()
        {
            // In case data isn't loaded, interact with the property getter
            var dataTemp = data;
            return keysWithContent;
        }
        
        
        
        public static void OnAfterDeserialization ()
        {
            // Process every area recursively first
            foreach (var kvp in data)
                ProcessRecursiveStart (kvp.Value);
            
            keysWithContent.Clear ();
            
            // Fill parents after recursive processing is done on all presets, ensuring lack of cyclical refs etc
            foreach (var kvp1 in data)
            {
                var entryA = kvp1.Value;
                if (entryA == null)
                    continue;

                var key = kvp1.Key;
                entryA.children.Clear ();
                
                if (entryA.content != null)
                    keysWithContent.Add (key);
                
                foreach (var kvp2 in data)
                {
                    var entryB = kvp2.Value;
                    if (entryB.parents == null || entryB.parents.Count == 0)
                        continue;

                    foreach (var link in entryB.parents)
                    {
                        if (link.key == key)
                            entryA.children.Add (entryB.key);
                    }
                }
            }
            
            foreach (var kvp1 in data)
                Postprocess (kvp1.Value);
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
        
        private static List<DataContainerCombatArea> areasUpdated = new List<DataContainerCombatArea> ();
        
        public static void ProcessRelated (DataContainerCombatArea origin)
        {
            if (origin == null)
                return;

            areasUpdated.Clear ();
            areasUpdated.Add (origin);
            
            if (origin.children != null)
            {
                foreach (var childKey in origin.children)
                {
                    var composite = GetEntry (childKey);
                    if (composite != null)
                        areasUpdated.Add (composite);
                }
            }
            
            foreach (var scenario in areasUpdated)
            {
                // Avoid localization refresh and other losses on origin
                if (scenario != origin)
                    scenario.OnAfterDeserialization (scenario.key);
            }

            foreach (var composite in areasUpdated)
                ProcessRecursiveStart (composite);

            foreach (var composite in areasUpdated)
                Postprocess (composite);
        }
        
        public static void ProcessRecursiveStart (DataContainerCombatArea origin)
        {
            if (origin == null)
                return;

            origin.imageProc = null;
            origin.coreProc = null;
            origin.atmosphereProc = null;
            origin.tagsProc = null;
            origin.spawnGroupsProc = null;
            origin.locationsProc = null;
            origin.volumesProc = null;
            origin.fieldsProc = null;
            origin.waypointGroupsProc = null;
            origin.briefingGroupsInjectedProc = null;
            origin.introProc = null;
            origin.boundaryProc = null;
            origin.segmentsProc = null;
            origin.biomeFilterProc = null;
            origin.scenarioChangesProc = null;
            origin.contentProc = null;
            origin.OnBeforeProcessing ();
                
            ProcessRecursive (origin, origin, 0);
        }
        
        public static void Postprocess (DataContainerCombatArea area)
        {
            area.positionCenter = area.GetSpawnGroupCenter ("center", false);
        }

        private static void ProcessRecursive (DataContainerCombatArea current, DataContainerCombatArea root, int depth)
        {
            if (current == null || root == null)
            {
                Debug.LogWarning ($"Received null step or root scenario reference while validating scenario hierarchy");
                return;
            }

            if (depth > 0 && current == root)
            {
                Debug.LogWarning ($"Encountered dependency loop at depth level {depth} when processing scenario {root.key}");
                return;
            }
            
            if (current.image != null && root.imageProc == null)
                root.imageProc = current.image;
            
            if (current.core != null && root.coreProc == null)
                root.coreProc = current.core;
            
            if (current.content != null && root.contentProc == null)
                root.contentProc = current.content;
            
            if (current.atmosphere != null && root.atmosphereProc == null)
                root.atmosphereProc = current.atmosphere;
            
            if (current.tags != null && current.tags.Count > 0)
            {
                if (root.tagsProc == null)
                    root.tagsProc = new HashSet<string> ();
                
                foreach (var tag in current.tags)
                {
                    if (!string.IsNullOrEmpty (tag) && !root.tagsProc.Contains (tag))
                        root.tagsProc.Add (tag);
                }
            }

            if (current.spawnGroups != null && current.spawnGroups.Count > 0)
            {
                if (root.spawnGroupsProc == null)
                    root.spawnGroupsProc = new SortedDictionary<string, DataBlockAreaSpawnGroup> ();

                foreach (var kvp in current.spawnGroups)
                {
                    if (kvp.Value != null && !root.spawnGroupsProc.ContainsKey (kvp.Key))
                        root.spawnGroupsProc.Add (kvp.Key, kvp.Value);
                }
            }

            if (current.locations != null && current.locations.Count > 0)
            {
                if (root.locationsProc == null)
                    root.locationsProc = new SortedDictionary<string, DataBlockAreaLocationTagged> ();

                foreach (var kvp in current.locations)
                {
                    if (kvp.Value != null && !root.locationsProc.ContainsKey (kvp.Key))
                        root.locationsProc.Add (kvp.Key, kvp.Value);
                }
            }

            if (current.volumes != null && current.volumes.Count > 0)
            {
                if (root.volumesProc == null)
                    root.volumesProc = new SortedDictionary<string, DataBlockAreaVolumeTagged> ();

                foreach (var kvp in current.volumes)
                {
                    if (kvp.Value != null && !root.volumesProc.ContainsKey (kvp.Key))
                        root.volumesProc.Add (kvp.Key, kvp.Value);
                }
            }

            if (current.fields != null && current.fields.Count > 0)
            {
                if (root.fieldsProc == null)
                    root.fieldsProc = new List<DataBlockAreaField> ();

                foreach (var entry in current.fields)
                {
                    if (entry != null)
                        root.fieldsProc.Add (entry);
                }
            }
            
            if (current.waypointGroups != null && current.waypointGroups.Count > 0)
            {
                if (root.waypointGroupsProc == null)
                    root.waypointGroupsProc = new SortedDictionary<string, DataBlockAreaWaypointGroup> ();

                foreach (var kvp in current.waypointGroups)
                {
                    if (kvp.Value != null && !root.waypointGroupsProc.ContainsKey (kvp.Key))
                        root.waypointGroupsProc.Add (kvp.Key, kvp.Value);
                }
            }
            
            if (current.briefingGroupsInjected != null && current.briefingGroupsInjected.Count > 0)
            {
                if (root.briefingGroupsInjectedProc == null)
                    root.briefingGroupsInjectedProc = new List<DataBlockScenarioBriefingGroup> ();

                foreach (var entry in current.briefingGroupsInjected)
                {
                    if (entry != null)
                        root.briefingGroupsInjectedProc.Add (entry);
                }
            }
            
            if (current.briefingGroupsInjected != null && current.briefingGroupsInjected.Count > 0)
            {
                if (root.briefingGroupsInjectedProc == null)
                    root.briefingGroupsInjectedProc = new List<DataBlockScenarioBriefingGroup> ();

                foreach (var entry in current.briefingGroupsInjected)
                {
                    if (entry != null)
                        root.briefingGroupsInjectedProc.Add (entry);
                }
            }
            
            if (current.intro != null && root.introProc == null)
                root.introProc = current.intro;
            
            if (current.boundary != null && root.boundaryProc == null)
                root.boundaryProc = current.boundary;
            
            if (current.segments != null && current.segments.Count > 0)
            {
                if (root.segmentsProc == null)
                    root.segmentsProc = new List<DataBlockEnvironmentSegment> ();

                foreach (var entry in current.segments)
                {
                    if (entry != null)
                        root.segmentsProc.Add (entry);
                }
            }
            
            if (current.biomeFilter != null && root.biomeFilterProc == null)
                root.biomeFilterProc = current.biomeFilter;
            
            if (current.scenarioChanges != null && current.scenarioChanges.Count > 0)
            {
                if (root.scenarioChangesProc == null)
                    root.scenarioChangesProc = new List<DataBlockProvinceScenarioChange> ();

                foreach (var entry in current.scenarioChanges)
                {
                    if (entry != null)
                        root.scenarioChangesProc.Add (entry);
                }
            }
            

            // Just in case we somehow missed a cyclical dependency
            if (depth > 20)
            {
                Debug.LogWarning ($"Area {current.key} fails to complete recursive processing in under 20 steps.");
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
                    Debug.LogWarning ($"Area {current.key} has null or empty parent link at index {i}!");
                    continue;
                }

                if (link.key == current.key)
                {
                    Debug.LogWarning ($"Area {current.key} has invalid parent key matching itself at index {i}");
                    continue;
                }

                var parent = GetEntry (link.key, false);
                if (parent == null)
                {
                    Debug.LogWarning ($"Area {current.key} has invalid parent key {link.key} at index {i} that can't be found in the database");
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





        #if UNITY_EDITOR
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogUntaggedRoads ()
        {
            foreach (var kvp in data)
            {
                var area = kvp.Value;
                if (area == null || area.spawnGroups == null)
                    continue;

                foreach (var kvp2 in area.spawnGroups)
                {
                    var spawnGroupKey = kvp2.Key;
                    var spawnGroup = kvp2.Value;

                    if (!spawnGroupKey.Contains ("road"))
                        continue;
                    
                    if (spawnGroup.tags == null || spawnGroup.tags.Count == 0)
                    {
                        Debug.Log ($"Spawn {kvp.Key} / {kvp2.Key} has no tags");
                        continue;
                    }

                    if (!spawnGroup.tags.Contains ("context_road"))
                    {
                        Debug.Log ($"Spawn {kvp.Key} / {kvp2.Key} is not tagged with context_road: {spawnGroup.tags.ToStringFormatted ()}");
                    }
                    else if (spawnGroup.tags.Contains ("perimeter_outer"))
                    {
                        Debug.Log ($"Spawn {kvp.Key} / {kvp2.Key} is tagged as outer perimeter: {spawnGroup.tags.ToStringFormatted ()}");
                    }
                    else if (spawnGroup.tags.Count > 2)
                    {
                        Debug.Log ($"Spawn {kvp.Key} / {kvp2.Key} has too many tags: {spawnGroup.tags.ToStringFormatted ()}");
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void UnloadArea ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
                return;
            
            sceneHelper.areaManager.UnloadArea (false);
                        
            if (!Application.isPlaying)
            {
                #if !PB_MODSDK

                if (OverworldSceneHelper.ins != null)
                    OverworldSceneHelper.ins.SetActiveDirectly (true);

                #endif

                if (CombatSceneHelper.ins != null)
                    CombatSceneHelper.ins.DestroyTerrainMeshes ();
            }
        }
        
        private bool IsUtilityOperationAvailable => utilityCoroutine == null;
        private const string utilityCheck = "IsUtilityOperationAvailable";

        private EditorCoroutine utilityCoroutine = null;
        
        [HideIf (utilityCheck)]
        [FoldoutGroup ("Utilities", false)]
        [Button ("Stop coroutine", ButtonSizes.Large), PropertyOrder (-10)]
        public void CancelCoroutine ()
        {
            EditorCoroutineUtility.StopCoroutine (utilityCoroutine);
            OnUtilityCoroutineEnd ();
        }

        private void OnUtilityCoroutineEnd ()
        {
            utilityCoroutine = null;
            EditorUtility.ClearProgressBar ();
        }
        
        private bool IsSpawnGenerationAvailable ()
        {
            return IsUtilityOperationAvailable;
        }
        
        [ShowIf ("IsSpawnGenerationAvailable")]
        [FoldoutGroup ("Utilities", false)]
        [Button ("Generate spawns", ButtonSizes.Large), PropertyOrder (-10)]
        public void GenerateSpawns (bool pathfindingValidation)
        {
            if (pathfindingValidation)
            {
                #if !PB_MODSDK

                if (!Application.isPlaying || !IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.combat))
                {
                    Debug.Log ($"Spawn generation with pathfinding not available outside combat mode at runtime, as it depends on pathfinding service and runtime nav graph");
                    return;
                }

                #else
                
                Debug.LogWarning ($"Spawn generation with pathfinding not available in the SDK, as it depends on pathfinding service and runtime nav graph");
                return;
                
                #endif
            }

            if (EditorUtility.DisplayDialog ("Start test", "Are you sure you'd like to generate spawn points? This process can take a while due to pathfinding needed for each point.", "Confirm", "Cancel"))
                utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (GenerateSpawnsIE (pathfindingValidation));
        }

        #if !PB_MODSDK

        private static bool pathComplete = false;
        private static Pathfinding.Path path = null;

        private static void OnPathComplete (Pathfinding.Path pathReturned)
        {
            path = pathReturned;
            pathComplete = true;
        }

        #endif
        
        public IEnumerator GenerateSpawnsIE (bool navGraphUsed)
        {
            #if PB_MODSDK
            var area = DataMultiLinkerCombatArea.selectedArea;
            #else
            var area = navGraphUsed ? ScenarioUtility.GetCurrentArea () : DataMultiLinkerCombatArea.selectedArea;
            #endif

            if (area == null)
            {
                Debug.LogWarning ("No selected area, can't proceed");
                OnUtilityCoroutineEnd ();
                yield break;
            }

            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
            {
                Debug.LogWarning ("Failed to find area manager");
                OnUtilityCoroutineEnd ();
                yield break;
            }
            
            var am = sceneHelper.areaManager;
            if (am.points == null || am.points.Count == 0)
            {
                Debug.LogWarning ("Failed to find loaded volume points");
                OnUtilityCoroutineEnd ();
                yield break;
            }

            #if !PB_MODSDK

            IAreaNavNode[] navNodes = null;
            if (navGraphUsed)
            {
                var graph = PathfindingGraphManager.GetGraphCombat ();
                navNodes = graph.nodes;

                if (navNodes == null || navNodes.Length == 0)
                {
                    Debug.LogWarning ("Failed to find navigation nodes");
                    OnUtilityCoroutineEnd ();
                    yield break;
                }
            }

            #endif
            
            var prefabPath = "Content/Prefabs/Scene Helpers/AreaSpawnPattern";
            var spawnInstance = FindObjectOfType (typeof (AreaSpawnGeneratorHelper), true) as AreaSpawnGeneratorHelper;
            if (spawnInstance == null)
            {
                var spawnPrefab = Resources.Load (prefabPath, typeof (AreaSpawnGeneratorHelper));
                if (spawnPrefab != null)
                {
                    spawnInstance = Instantiate (spawnPrefab) as AreaSpawnGeneratorHelper;
                    if (spawnInstance != null)
                    {
                        var t = spawnInstance.transform;
                        t.parent = sceneHelper.areaManager.transform;
                        t.rotation = Quaternion.identity;
                        t.localScale = Vector3.one;
                        t.position = new Vector3 (am.boundsFull.x, 0f, am.boundsFull.z) * (TilesetUtility.blockAssetSize * 0.5f);
                    }
                }
            }
            
            if (spawnInstance == null)
            {
                Debug.LogWarning ($"Failed to find a AreaSpawnGeneratorHelper generation helper object or instantiate one using prefab path: {prefabPath}");
                OnUtilityCoroutineEnd ();
                yield break;
            }
            
            var floorNavNodePositions = new List<Vector3> ();
            if (navGraphUsed)
            {
                #if !PB_MODSDK

                // If we are using the nav graph, it's better to fetch nodes directly - they are subject to more conditions, e.g. no proximity to walls
                foreach (var node in navNodes)
                    floorNavNodePositions.Add (node.GetPosition ());

                #endif
            }
            else
            {
                // If we have no access to nav graph, we just grab flat floor spots
                foreach (var point in am.points)
                {
                    if (point.spotConfiguration == AreaNavUtility.configFloor)
                        floorNavNodePositions.Add (point.instancePosition);
                }
            }

            foreach (var link in spawnInstance.links)
            {
                if (link.pointCandidates == null)
                    link.pointCandidates = new List<AreaSpawnGeneratorHelper.SpawnPointCandidate> ();
                else
                    link.pointCandidates.Clear ();

                if (link.pointsFinal == null)
                    link.pointsFinal = new List<AreaSpawnGeneratorHelper.SpawnPointCandidate> ();
                else
                    link.pointsFinal.Clear ();

                link.linkPos = link.transform.position;
                link.linkPosFlat = new Vector2 (link.linkPos.x, link.linkPos.z);
            }
            
            var posNavigationValidation = AreaUtility.GroundPoint (spawnInstance.transformNavigationValidation.position);
            for (int p = 0, pLimit = floorNavNodePositions.Count; p < pLimit; ++p)
            {
                var pointPos = floorNavNodePositions[p];
                var pointPosFlat = new Vector2 (pointPos.x, pointPos.z);

                var progress = p / (float)pLimit;
                EditorUtility.DisplayProgressBar ($"Validating point {p+1}/{pLimit}", $"Area: {am.areaName}; Bounds: {am.boundsFull}", progress);

                float distanceMin = 100000f;
                AreaSpawnGeneratorHelper.SpawnLink linkClosest = null;

                foreach (var link in spawnInstance.links)
                {
                    var distance = Vector2.Distance (link.linkPosFlat, pointPosFlat);
                    if (distance < distanceMin)
                    {
                        distanceMin = distance;
                        linkClosest = link;
                    }
                }
                
                if (linkClosest != null)
                {
                    linkClosest.pointCandidates.Add (new AreaSpawnGeneratorHelper.SpawnPointCandidate
                    {
                        position = pointPos,
                        distance = distanceMin
                    });
                }
            }
            
            float distanceThreshold = spawnInstance.distanceThreshold;
            int pointsPerGroup = spawnInstance.pointsPerGroup;
            
            if (area.spawnGroups == null)
                area.spawnGroups = new SortedDictionary<string, DataBlockAreaSpawnGroup> ();
            else
                area.spawnGroups.Clear ();

            for (int i = 0, limit = spawnInstance.links.Count; i < limit; ++i)
            {
                var link = spawnInstance.links[i];
                if (link.pointCandidates == null || link.pointCandidates.Count == 0)
                    continue;
                
                AreaSpawnGeneratorHelper.SpawnGroupPreset preset = null;
                foreach (var presetCandidate in spawnInstance.presets)
                {
                    if (presetCandidate.key == link.preset)
                    {
                        preset = presetCandidate;
                        break;
                    }
                }

                if (preset == null)
                {
                    Debug.LogWarning ($"Link {link.name} has no matching preset {link.preset}");
                    continue;
                }
                
                // Sort list to make last point the closest to center
                link.pointCandidates.Sort ((x, y) => y.distance.CompareTo (x.distance));
                
                var color = Color.HSVToRGB ((float)i / limit, 1f, 1f);
                var colorDarker = Color.Lerp (color, Color.black, 0.75f);
                var colorDarkerTrs = colorDarker.WithAlpha (0.5f);
                var colorLighter = Color.Lerp (color, Color.white, 0.75f);
                var colorLighterTrs = colorLighter.WithAlpha (0.35f);
                var posOrigin = link.transform.position;

                for (int x = 0, limit2 = link.pointCandidates.Count; x < limit2; ++x)
                {
                    var pointCandidate = link.pointCandidates[x];
                    var pos = pointCandidate.position;
                    var distFactor = (float)x / limit2;
                    
                    Debug.DrawLine (posOrigin, pos, Color.Lerp (colorDarkerTrs, colorLighterTrs, distFactor), 3f);
                    Debug.DrawLine (pos, pos + Vector3.up * 3f, Color.Lerp (colorDarker, colorLighter, distFactor), 3f);
                }
                
                // Begin collecting final points
                int iterations = 0;
                while (true)
                {
                    // For each candidate point, starting from one closest to group origin
                    for (int x = link.pointCandidates.Count - 1; x >= 0; --x)
                    {
                        var pointCandidate = link.pointCandidates[x];
                        var pointCandidateFlat = pointCandidate.position.Flatten2D ();
                        
                        if (link.pointsFinal.Count > 0)
                        {
                            // Check every point already confirmed and eliminate a given candidate from running if it's too close to any 
                            bool deleteDueToProximity = false;
                            foreach (var pointFinal in link.pointsFinal)
                            {
                                var pointFinalFlat = pointFinal.position.Flatten2D ();
                                var distance = Vector2.Distance (pointFinalFlat, pointCandidateFlat);
                                if (distance < distanceThreshold)
                                {
                                    deleteDueToProximity = true;
                                    break;
                                }
                            }

                            // If any finalized point is too close, this candidate is of no use going forward
                            if (deleteDueToProximity)
                            {
                                link.pointCandidates.RemoveAt (x);
                                continue;
                            }
                        }

                        #if !PB_MODSDK

                        if (navGraphUsed)
                        {
                            // Request new path
                            pathComplete = false;

                            var seeker = PathfindingGraphManager.GetSeekerCombat (true);
                            seeker.StartPath (pointCandidate.position, posNavigationValidation, OnPathComplete);
                            
                            int loops = 0;
                            var wait = new WaitForEndOfFrame ();
                            
                            // Wait for path to complete
                            while (!pathComplete)
                            {
                                // Debug.Log ($"Waiting for path for action {a} ({actionDesc.name} of unit {i} ({unitPersistent.nameInternal})");
                                loops += 1;
                                if (loops > 100)
                                {
                                    Debug.LogWarning ($"Timed out waiting for path to compete");
                                    break;
                                }

                                yield return wait;
                            }

                            // Discard if path callback was never fired for some reason
                            if (!pathComplete)
                            {
                                link.pointCandidates.RemoveAt (x);
                                continue;
                            }

                            // Discard if path wasn't generated
                            if (path.error)
                            {
                                link.pointCandidates.RemoveAt (x);
                                continue;
                            }

                            // Discard if path didn't reach intended destination
                            var posLast = path.vectorPath[path.vectorPath.Count - 1];
                            if (Vector3.Distance (posLast, posNavigationValidation) > 3f)
                            {
                                link.pointCandidates.RemoveAt (x);
                                continue;
                            }

                            // Discard if path doesn't contain enough points
                            var pathPoints = path.vectorPath;
                            if (pathPoints.Count < 2)
                            {
                                link.pointCandidates.RemoveAt (x);
                                continue;
                            }
                            
                            for (int p = 1, limit2 = pathPoints.Count; p < limit2; ++p)
                            {
                                var p1 = pathPoints[p - 1];
                                var p2 = pathPoints[p];
                                var navColor = Color.HSVToRGB ((float)p / limit2, 1f, 1f);
                                Debug.DrawLine (p1, p2, navColor, 4f);
                            }

                            UnityEditor.SceneView.RepaintAll ();
                        }

                        #endif

                        // If we're here, a given candidate has no obstacles in joining the final set
                        link.pointsFinal.Add (pointCandidate);
                        break;
                    }

                    // If we collected enough points, break the while loop
                    if (link.pointsFinal.Count >= pointsPerGroup)
                        break;
                    
                    // If we're too far, break just in case
                    iterations += 1;
                    if (iterations > 10000)
                        break;
                }

                var groupKeySuffix = link.suffix;
                var groupKeyFull = $"{link.preset}{groupKeySuffix}";
                
                var sg = new DataBlockAreaSpawnGroup ();
                area.spawnGroups.Add (groupKeyFull, sg);
                
                sg.key = groupKeyFull;
                sg.tags = new HashSet<string> (preset.tags);
                sg.points = new List<DataBlockAreaPoint> ();

                var dir = -link.transform.forward;
                var angle = dir.sqrMagnitude > 0f ? Quaternion.LookRotation (dir, Vector3.up).eulerAngles.y : 0f;
                var angleRounded = Mathf.RoundToInt (Mathf.RoundToInt (angle / 45f) * 45);
                var rotationRounded = new Vector3 (0f, angleRounded, 0f);
                
                for (int p = 0, limit2 = link.pointsFinal.Count; p < limit2; ++p)
                {
                    var point = link.pointsFinal[p];
                    var pos = point.position;

                    Debug.DrawLine (posOrigin, pos, Color.white, 5f);
                    Debug.DrawLine (pos, pos + Vector3.up * 3f, Color.white, 5f);
                    
                    sg.points.Add (new DataBlockAreaPoint
                    {
                        index = p,
                        point = pos,
                        rotation = rotationRounded
                    });
                }
                
                sg.RefreshAveragePosition ();

                foreach (var directionalTag in spawnInstance.tagsDirectional)
                {
                    var arcHalf = directionalTag.arc / 2f;
                    var angleFrom = WrapAngle (directionalTag.angle - arcHalf);
                    var angleTo = WrapAngle (directionalTag.angle + arcHalf);
                    
                    bool match = angle >= angleFrom && angle <= angleTo;
                    if (match)
                    {
                        sg.tags.Add (directionalTag.key);
                    }
                }
                
                Debug.Log ($"Created spawn group {sg.key} with {sg.points.Count} points and following tags:\n{sg.tags.ToStringFormatted (true, multilinePrefix: "- ")}");

                yield return null;
                OnUtilityCoroutineEnd ();
            }
        }
        
        private static float WrapAngle (float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
        
        #endif
        
        // A small rotation utility
        /* public string key;
         public Vector3 targetPoint;
         [Button]
         public void LookAt ()
         {
             if (!data.ContainsKey (key))
                 return;
             
             Vector3 rotation = new Vector3 ();
             Vector3 direction;
             foreach (var spawnGroup in data[key].spawnGroups)
             {
                 foreach (var point in spawnGroup.Value.points)
                 {
                    direction = (targetPoint - point.point).normalized;
                    rotation.y = Quaternion.LookRotation (direction).eulerAngles.y;
                    point.rotation = rotation;
                 }
             }
         }
        */
    }
}


