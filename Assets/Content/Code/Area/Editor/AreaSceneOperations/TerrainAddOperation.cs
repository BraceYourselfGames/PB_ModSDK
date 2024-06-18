using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Area
{
    sealed class TerrainAddOperation : AreaSceneVolumeOperation
    {
        public interface Checks
        {
            bool denyAddBlockTop { get; }
            bool denyTerrainEdit { get; }
            System.Action onActionDenied { get; set; }
        }

        public bool TryGetTargetBlocks (AreaVolumePoint pointStart, Vector3 direction)
        {
            if (checks.denyAddBlockTop || checks.denyTerrainEdit)
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.Log ("Edit terrain denied | op: add | index: " + pointStart.spotIndex);
                }
                checks.onActionDenied?.Invoke ();
                return false;
            }

            var upSpots = true;
            if (pointStart.pointState == AreaVolumePointState.Full)
            {
                var (compass, newPoint) = AreaSceneHelper.GetNeighborFromDirection (pointStart, direction);
                if (newPoint == null)
                {
                    Debug.LogErrorFormat ("Edit terrain -- neighbor is null | op: add | index: {0} | neighbor: {1}", pointStart.spotIndex, compass);
                    return false;
                }
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.LogFormat ("Edit terrain -- use neighbor | op: add | index: {0} --> {1} | neighbor: {2}", pointStart.spotIndex, newPoint.spotIndex, compass);
                }
                pointStart = newPoint;
                upSpots = false;
            }
            else if (bb.enableTerrainShapeLogging)
            {
                Debug.Log ("Edit terrain -- target block | op: add | index: " + pointStart.spotIndex);
            }

            if (!AreaSceneTerrainShapeMode.CheckBrushPoints (pointStart, pointsToEdit, upSpots, AreaSceneHelper.FreeSpacePolicy.LookDownPass, bb.enableTerrainShapeLogging))
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.Log ("Edit terrain target block -- point check failed | op: add");
                }
                return false;
            }

            if (bb.enableTerrainShapeLogging)
            {
                Debug.Log ("Edit terrain target blocks | op: add | blocks (" + pointsToEdit.Count + "): " + pointsToEdit.Select (pt => pt.spotIndex).ToStringFormatted ());
            }
            return true;
        }

        public bool Apply () => TryChangeBlocks () && UpdateBlocks ();

        bool TryChangeBlocks () => AreaSceneHelper.TryChangeBlocks (pointsToEdit, TrySetBlockFull, bb.enableTerrainShapeLogging);
        static bool TrySetBlockFull (AreaVolumePoint point, bool log)
        {
            if (point.pointState != AreaVolumePointState.Empty)
            {
                return false;
            }
            if (log)
            {
                Debug.Log ("Change point state to full | index: " + point.spotIndex);
            }
            return true;
        }

        bool UpdateBlocks ()
        {
            var am = bb.am;
            var spotsModified = false;
            editedIndexes.Clear ();
            modifiedIndexes.Clear ();
            foreach (var point in pointsToEdit)
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.LogFormat ("Update block for point | index: {0} | state: {1} | offset: {2:F2}", point.spotIndex, point.pointState, point.terrainOffset);
                }
                editedIndexes.Add (point.spotIndex);
                point.terrainOffset = 0f;
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

                    if (bb.enableTerrainShapeLogging)
                    {
                        Debug.LogFormat
                        (
                            "Update {0} spot in block {1} | index: {2} | state: {3} | configuration: {4}",
                            AreaSceneHelper.SpotNeighborDisplay(i, compact: true),
                            point.spotIndex,
                            spot.spotIndex,
                            spot.pointState,
                            AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration)
                        );
                    }

                    spotsModified |= UpdateSpotConfiguration (spot, i, bb.enableTerrainShapeLogging);
                    if (!AreaManager.IsPointTerrain (spot))
                    {
                        spot.blockTileset = AreaTilesetHelper.idOfTerrain;
                        modifiedIndexes.Add (spot.spotIndex);
                        spotsModified = true;
                    }
                }
            }

            foreach (var index in modifiedIndexes)
            {
                var spot = am.points[index];

                if (spot.spotConfiguration == TilesetUtility.configurationFull
                    && spot.blockTileset != 0
                    && AreaSceneHelper.IsEnclosed (am, spot))
                {
                    if (bb.enableTerrainShapeLogging)
                    {
                        Debug.LogFormat ("Convert spot to interior | index: {0} | tileset: {1} --> 0", spot.spotIndex, spot.blockTileset);
                    }
                    AreaManager.ClearTileData (spot);
                    spot.terrainOffset = 0f;
                    spotsModified = true;
                }

                am.RebuildBlock (spot, bb.enableTerrainShapeLogging);
                am.RebuildCollisionForPoint (spot);
            }
            foreach (var index in editedIndexes)
            {
                am.UpdateDamageAroundIndex (index);
            }

            return spotsModified;
        }

        static bool UpdateSpotConfiguration (AreaVolumePoint spot, int neighborIndex, bool log)
        {
            var configuration = spot.spotConfiguration;
            configuration |= AreaSceneHelper.spotNeighborMasks[neighborIndex];
            if (configuration == spot.spotConfiguration)
            {
                return false;
            }

            if (log)
            {
                Debug.LogFormat
                (
                    "Spot configuration modified | index: {0} | configuration: {1} --> {2}",
                    spot.spotIndex,
                    AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration),
                    AreaSceneHelper.GetPointConfigurationDisplayString (configuration)
                );
            }

            var modified = false;
            var fullState = (configuration & TilesetUtility.configurationBitTopSelf) == TilesetUtility.configurationBitTopSelf;
            if (fullState && spot.pointState != AreaVolumePointState.Full)
            {
                modified = true;
                spot.pointState = AreaVolumePointState.Full;
            }

            spot.spotConfiguration = configuration;
            modifiedIndexes.Add (spot.spotIndex);
            return modified;
        }

        public TerrainAddOperation (AreaSceneBlackboard bb, Checks checks)
        {
            this.bb = bb;
            this.checks = checks;
        }

        readonly AreaSceneBlackboard bb;
        readonly Checks checks;
        readonly List<AreaVolumePoint> pointsToEdit = new List<AreaVolumePoint> ();

        static readonly List<int> editedIndexes = new List<int> ();
        static readonly List<int> modifiedIndexes = new List<int> ();
    }
}
