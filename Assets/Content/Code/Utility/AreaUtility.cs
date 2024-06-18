using System.Collections;
using System.Collections.Generic;

using Area;
using PhantomBrigade.Data;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace UtilityArea
{
    public static class Functions
    {
        #if UNITY_EDITOR

        public static IEnumerator GenerateSpawnsIE (System.Action onUtilityCoroutineEnd)
        {
            var area = DataMultiLinkerCombatArea.selectedArea;
            if (area == null)
            {
                Debug.LogWarning ("No selected area, can't proceed");
                onUtilityCoroutineEnd ();
                yield break;
            }

            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
            {
                Debug.LogWarning ("Failed to find area manager");
                onUtilityCoroutineEnd ();
                yield break;
            }

            var am = sceneHelper.areaManager;
            if (am.points == null || am.points.Count == 0)
            {
                Debug.LogWarning ("Failed to find loaded volume points");
                onUtilityCoroutineEnd ();
                yield break;
            }

            var prefabPath = "Content/Prefabs/Scene Helpers/AreaSpawnPattern";
            var spawnInstance = Object.FindObjectOfType (typeof (AreaSpawnGeneratorHelper), true) as AreaSpawnGeneratorHelper;
            if (spawnInstance == null)
            {
                var spawnPrefab = Resources.Load (prefabPath, typeof (AreaSpawnGeneratorHelper));
                if (spawnPrefab != null)
                {
                    spawnInstance = Object.Instantiate (spawnPrefab) as AreaSpawnGeneratorHelper;
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
                onUtilityCoroutineEnd ();
                yield break;
            }

            var floorNavNodePositions = new List<Vector3> ();

            // If we have no access to nav graph, we just grab flat floor spots
            foreach (var point in am.points)
            {
                if (point.spotConfiguration == AreaNavUtility.configFloor)
                    floorNavNodePositions.Add (point.instancePosition);
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

            /*
            var posNavigationValidation = AreaUtility.GroundPoint (spawnInstance.transformNavigationValidation.position);
            var pathfinder = PrepareNavigation ();
            if (pathfinder == null)
            {
                Debug.LogWarning ($"Failed to find a AstarPath or create one");
                onUtilityCoroutineEnd ();
                yield break;
            }
            yield return new EditorWaitForSeconds (0.1f);
            pathfinder.enabled = true;
            yield return new EditorWaitForSeconds (0.1f);
            var navOK = UpdateNavigation (pathfinder);
            if (!navOK)
            {
                Debug.LogWarning ("Failed to find navigation graph");
                onUtilityCoroutineEnd ();
                yield break;
            }
            */

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
                onUtilityCoroutineEnd ();
            }
        }

        static float WrapAngle (float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        /*
        static AstarPath PrepareNavigation ()
        {
            var pathfinder = Object.FindObjectOfType (typeof (AstarPath)) as AstarPath;
            if (pathfinder == null)
            {
                GameObject pathGO = new GameObject ("_PathfindingService");
                pathfinder = pathGO.AddComponent<AstarPath> ();
                AstarPath.active = pathfinder;

                pathfinder = AstarPath.active;
                pathfinder.scanOnStartup = false;
                pathfinder.showGraphs = false;
                pathfinder.showNavGraphs = false;
                pathfinder.threadCount = ThreadCount.AutomaticLowLoad;
                pathfinder.logPathResults =  PathLog.None;
                pathfinder.heuristic = Heuristic.DiagonalManhattan;
                pathfinder.heuristicScale = 0.33f;
                pathfinder.AwakeForced ();
            }

            pathfinder.enabled = false;
            return pathfinder;
        }

        static bool UpdateNavigation (AstarPath pathfinder)
        {
            var restrictedPathSeeker = Object.FindObjectOfType (typeof (AstarPath)) as Seeker;
            if (restrictedPathSeeker == null)
            {
                var restrictedSeekerObject = new GameObject ("_PathfindingSeekerRestricted");
                restrictedPathSeeker = restrictedSeekerObject.AddComponent<Seeker> ();
                restrictedPathSeeker.traversableTags = DataShortcuts.sim.restrictedPathTags;

                StartEndModifier modifier = new StartEndModifier ();
                modifier.exactEndPoint = DataShortcuts.sim.pathEndPointExactness;
                modifier.exactStartPoint = DataShortcuts.sim.pathStartPointExactness;
                restrictedPathSeeker.startEndModifier = modifier;
            }

            PhantomNavGraph phantomGraph = null;
            if (pathfinder.data.graphs != null)
            {
                for (int i = 0; i <  pathfinder.data.graphs.Length; ++i)
                {
                    if ( pathfinder.data.graphs[i] is PhantomNavGraph graph)
                    {
                        phantomGraph = graph;
                        break;
                    }
                }
            }

            if (phantomGraph == null)
                phantomGraph = pathfinder.data.AddGraph (typeof (PhantomNavGraph)) as PhantomNavGraph;

            if (phantomGraph == null)
            {
                return false;
            }

            phantomGraph.Scan ();
            return true;
        }
        */

        #endif
    }
}
