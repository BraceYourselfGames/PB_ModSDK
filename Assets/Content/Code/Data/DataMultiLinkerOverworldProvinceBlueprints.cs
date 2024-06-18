using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldProvinceBlueprints : DataMultiLinker<DataContainerOverworldProvinceBlueprint>
    {
        public DataMultiLinkerOverworldProvinceBlueprints ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldProvinces);
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;

            [ShowInInspector]
            public static bool showNeighbours = true;

            [ShowInInspector]
            public static bool showBorder = true;

            [ShowInInspector]
            public static bool showEntities = true;

            [ShowInInspector]
            public static bool showSpawns = true;

            [ShowInInspector]
            public static bool showSpawnKeys = true;

            [ShowInInspector]
            public static bool showLinksActorSearch = false;

            [ShowInInspector]
            public static bool showLinksConvoy = false;

            [ShowInInspector]
            public static bool showLinksCounterAttacks = false;

            [ShowInInspector]
            public static bool showLinksWar = false;

            [ShowInInspector]
            public static bool showObjectivesDecisive = false;

            [ShowInInspector]
            public static string spawnGroupHighlighted = "obj_decisive";
        }

        [ShowInInspector, ShowIf ("@selection != null"), HideLabel, BoxGroup ("Selected"), PropertyOrder (90)]
        public static DataContainerOverworldProvinceBlueprint selection;

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldAction.Presentation.showTagCollections")]
        public static HashSet<string> spawnGroupKeys;

        public static void OnAfterDeserialization ()
        {
            if (spawnGroupKeys == null)
                spawnGroupKeys = new HashSet<string> ();
            else
                spawnGroupKeys.Clear ();

            var queue = new Queue<(DataContainerOverworldProvinceBlueprint blueprint, int depth)> ();
            var visitedSet = new HashSet<DataContainerOverworldProvinceBlueprint> ();

            foreach (var kvp in data)
            {
                var container = kvp.Value;

                if (container.spawns != null)
                {
                    foreach (var kvp2 in container.spawns)
                    {
                        var spawnGroupKey = kvp2.Key;
                        if (!spawnGroupKeys.Contains (spawnGroupKey))
                            spawnGroupKeys.Add (spawnGroupKey);
                    }
                }

                if (container.neighbourAdjacencyForActors == null)
                    container.neighbourAdjacencyForActors = new Dictionary<string, int> ();
                else
                    container.neighbourAdjacencyForActors.Clear ();

                queue.Clear ();
                visitedSet.Clear ();
                queue.Enqueue ((kvp.Value, 0));

                while (queue.Count > 0)
                {
                    (DataContainerOverworldProvinceBlueprint blueprint, int depth) nextItem = queue.Dequeue ();
                    if (visitedSet.Contains (nextItem.blueprint))
						continue;

                    container.neighbourAdjacencyForActors.Add (nextItem.blueprint.key, nextItem.depth);
                    visitedSet.Add (nextItem.blueprint);

                    foreach (var kvp2 in nextItem.blueprint.neighbourData)
                    {
                        var neighborKey = kvp2.Key;
                        if (string.IsNullOrEmpty (neighborKey) || !data.ContainsKey (neighborKey))
                            continue;

                        var block = kvp2.Value;
                        if (!block.allowActorSearch)
                            continue;

                        var neighbourBlueprint = data[neighborKey];
	                    queue.Enqueue ((neighbourBlueprint, nextItem.depth + 1));
					}
                }


            }
        }

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button ("Upgrade neighbor data"), PropertyOrder (-2)]
        public void UpgradeNeighborData ()
        {
            foreach (var kvp in data)
            {
                var p = kvp.Value;

                if (p.neighbours == null)
                    continue;

                if (p.neighbourData == null)
                    p.neighbourData = new SortedDictionary<string, DataBlockOverworldProvinceNeighbor> ();

                foreach (var neighbourName in p.neighbours)
                {
                    if (string.IsNullOrEmpty (neighbourName))
                        continue;

                    var pn = GetEntry (neighbourName);
                    if (pn == null)
                        continue;

                    p.neighbourData.Add (neighbourName, new DataBlockOverworldProvinceNeighbor
                    {
                        allowActorSearch = true,
                        allowConvoys = true,
                        allowCounterAttacks = true,
                        allowWar = true
                    });
                }

                p.neighbours = null;
            }
        }
        */

        [FoldoutGroup ("Utilities", false)]
        [Button ("Clear decisive objectives"), PropertyOrder (-2)]
        public void ClearDecisiveObjectives ()
        {
            var lookup = new Dictionary<string, List<string>> ();
            foreach (var kvp in data)
            {
                var p = kvp.Value;
                if (p == null)
                    continue;

                p.warObjectivesDecisiveRequired = 0;
                p.warObjectivesDecisive = null;
            }

            foreach (var kvp in lookup)
                kvp.Value.Sort();

            Debug.Log (lookup.ToStringFormattedKeyValuePairs (true, toStringOverride: (x) => x.ToStringFormatted (true, multilinePrefix: "- ")));
        }

        [FoldoutGroup ("Utilities", false)]
        [Button ("Log branches"), PropertyOrder (-2)]
        public void LogBranches ()
        {
            var lookup = new Dictionary<string, List<string>> ();
            foreach (var kvp in data)
            {
                var p = kvp.Value;
                if (p == null || string.IsNullOrEmpty (p.factionBranch))
                    continue;

                if (!lookup.ContainsKey (p.factionBranch))
                    lookup.Add (p.factionBranch, new List<string> ());

                lookup[p.factionBranch].Add (kvp.Key);
            }

            foreach (var kvp in lookup)
                kvp.Value.Sort();

            Debug.Log (lookup.ToStringFormattedKeyValuePairs (true, toStringOverride: (x) => x.ToStringFormatted (true, multilinePrefix: "- ")));
        }

        [Button (ButtonSizes.Large), ButtonGroup ("A"), PropertyOrder (-1)]
        public void Ground ()
        {
            foreach (var kvp in data)
            {
                var container = kvp.Value;

                if (container.border != null)
                {
                    for (int i = 0; i < container.border.Count; ++i)
                    {
                        var point = container.border[i];
                        var groundingRayOrigin = new Vector3 (point.x, 200f, point.z);
                        var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                        if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                        {
                            Debug.DrawLine (hit.point, point, Color.green, 10f);
                            container.border[i] = hit.point;
                        }
                    }
                }

                if (container.spawns != null)
                {
                    foreach (var kvp2 in container.spawns)
                    {
                        var spawns = kvp2.Value;
                        if (spawns == null || spawns.Count == 0)
                            continue;

                        for (int i = 0; i < spawns.Count; ++i)
                        {
                            var spawn = spawns[i];
                            if (spawn == null)
                                continue;

                            var groundingRayOrigin = new Vector3 (spawn.position.x, 200f, spawn.position.z);
                            var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                            if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                            {
                                Debug.DrawLine (hit.point, spawn.position, Color.red, 10f);
                                spawn.position = hit.point;
                            }
                        }
                    }
                }
            }
        }
    }
}
