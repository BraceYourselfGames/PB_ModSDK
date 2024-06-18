using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    interface AreaSceneOperation
    {
        public bool Apply ();
    }

    interface AreaSceneVolumeOperation : AreaSceneOperation
    {
        bool TryGetTargetBlocks (AreaVolumePoint pointStart, Vector3 direction);
    }

    static class VolumeOperationHelper
    {
        public static void TrickleDown (AreaVolumePoint point, int selectedTilesetID, int layerSize, List<int> modifiedIndexes, bool log)
        {
            trickleDownExcludes.Clear ();
            point = point.pointsInSpot[WorldSpace.PointNeighbor.Down];
            while (point != null)
            {
                if (log)
                {
                    Debug.Log ("Trickle down block | index: " + point.spotIndex);
                }
                var changed = false;
                for (var i = point.pointsWithSurroundingSpots.Length - 1; i >= 0; i -= 1)
                {
                    var spot = point.pointsWithSurroundingSpots[i];
                    if (spot == null)
                    {
                        continue;
                    }
                    if (!spot.spotPresent)
                    {
                        continue;
                    }
                    if (!AreaManager.IsPointTerrain (spot))
                    {
                        continue;
                    }
                    var layerIndex = spot.spotIndex - spot.pointPositionIndex.y * layerSize;
                    if (trickleDownExcludes.Contains (layerIndex))
                    {
                        continue;
                    }
                    if ((spot.spotConfiguration & TilesetUtility.configurationTopMask) == 0)
                    {
                        trickleDownExcludes.Add (layerIndex);
                        continue;
                    }
                    if (log)
                    {
                        Debug.LogFormat
                        (
                            "Trickle down change terrain spot | index: {0} ({1}) | tileset: {2} --> {3}",
                            spot.spotIndex,
                            AreaSceneHelper.SpotNeighborDisplay(i, compact: true),
                            spot.blockTileset,
                            selectedTilesetID);
                    }
                    AreaManager.ClearTileData (spot);
                    spot.blockTileset = selectedTilesetID;
                    modifiedIndexes.Add (spot.spotIndex);
                    changed = true;
                }
                if (!changed)
                {
                    return;
                }
                point = point.pointsInSpot[WorldSpace.PointNeighbor.Down];
            }
        }

        static readonly HashSet<int> trickleDownExcludes = new HashSet<int> ();
    }
}
