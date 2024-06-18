using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class VolumeAddOperation : AreaSceneVolumeOperation
    {
        public interface Checks
        {
            bool denyAddBlockTop { get; }
            System.Action onActionDenied { get; set; }
        }

        public bool TryGetTargetBlocks (AreaVolumePoint pointStart, Vector3 direction)
        {
            // Adding a block should happen in the free space around the selected block. The mouse pointer will be on
            // one of the faces of the block so we need to get that neighbor.
            //
            // If that neighbor is in layer 0, the block will be cut in half and won't be complete. This is similar to what happens on the X and Z boundaries
            // but a segment can cover the exposed parts of those half blocks. That can't be done for the top boundary so placing a block on top of layer 1 is blocked.

            if (checks.denyAddBlockTop)
            {
                if (bb.enableVolumeLogging)
                {
                    Debug.Log ("Edit volume denied | op: add | index: " + pointStart.spotIndex);
                }
                checks.onActionDenied?.Invoke ();
                return false;
            }

            if (pointStart.pointState == AreaVolumePointState.Full)
            {
                var (compass, newPoint) = AreaSceneHelper.GetNeighborFromDirection (pointStart, direction);
                if (newPoint == null)
                {
                    Debug.LogErrorFormat ("Edit volume -- neighbor is null | op: add | index: {0} | neighbor: {1}", pointStart.spotIndex, compass);
                    return false;
                }
                if (bb.enableVolumeLogging)
                {
                    Debug.LogFormat ("Edit volume -- use neighbor | op: add | index: {0} --> {1} | neighbor: {2}", pointStart.spotIndex, newPoint.spotIndex, compass);
                }
                pointStart = newPoint;
            }

            if (bb.enableVolumeLogging)
            {
                Debug.Log ("Edit volume -- target block | op: add | index: " + pointStart.spotIndex);
            }

            var points = AreaManager.CollectPointsInBrush (pointStart, AreaManager.editingVolumeBrush);
            pointsToEdit.Clear ();
            pointsToEdit.AddRange (points);
            return true;
        }

        public bool Apply () => TryChangeBlocks () && UpdateBlocks ();

        bool TryChangeBlocks () => IsSelectedTilesetValid(bb.volumeTilesetSelected.id) && AreaSceneHelper.TryChangeBlocks (pointsToEdit, TrySetBlockFull, bb.enableVolumeLogging);

        static bool IsSelectedTilesetValid (int selectedTileset)
        {
            switch (selectedTileset)
            {
                case 0:
                case AreaTilesetHelper.idOfTerrain:
                case AreaTilesetHelper.idOfRoad:
                case AreaTilesetHelper.idOfForest:
                    return false;
                default:
                    return true;
            }
        }

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
            var layerSize = am.boundsFull.x * am.boundsFull.z;
            var terrainModified = false;
            editedIndexes.Clear ();
            modifiedIndexes.Clear ();
            foreach (var point in pointsToEdit)
            {
                if (bb.enableVolumeLogging)
                {
                    Debug.LogFormat ("Update block for point | index: {0} | state: {1} | tileset: {2}", point.spotIndex, point.pointState, point.blockTileset);
                }
                editedIndexes.Add (point.spotIndex);
                var selectedTilesetID = bb.volumeTilesetSelected.id;
                var terrainModifiedPoint = false;
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

                    if (bb.enableVolumeLogging)
                    {
                        Debug.LogFormat
                        (
                            "Update {0} spot in block | index: {1} | state: {2} | configuration: {3} | tileset: {4}",
                            AreaSceneHelper.SpotNeighborDisplay(i, compact: true),
                            spot.spotIndex,
                            spot.pointState,
                            AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration),
                            spot.blockTileset
                        );
                    }

                    if (CheckTilesetOverride (bb, spot, selectedTilesetID))
                    {
                        var changeTerrain = AreaManager.IsPointTerrain (spot);
                        if (bb.enableVolumeLogging)
                        {
                            Debug.LogFormat ("Change tileset | index: {0} | tileset: {1} --> {2} | terrain: {3}", spot.spotIndex, spot.blockTileset, selectedTilesetID, changeTerrain);
                        }
                        AreaManager.ClearTileData (spot);
                        spot.blockTileset = selectedTilesetID;
                        terrainModifiedPoint |= changeTerrain;
                        modifiedIndexes.Add (spot.spotIndex);
                    }

                    terrainModifiedPoint |= UpdateSpotConfiguration (spot, i, bb.enableVolumeLogging);
                }
                if (terrainModifiedPoint)
                {
                    terrainModified = true;
                    VolumeOperationHelper.TrickleDown (point, selectedTilesetID, layerSize,  modifiedIndexes, bb.enableVolumeLogging);
                }
            }

            foreach (var index in modifiedIndexes)
            {
                var spot = am.points[index];

                if (spot.spotConfiguration == TilesetUtility.configurationFull
                    && spot.blockTileset != 0
                    && AreaSceneHelper.IsEnclosed (am, spot))
                {
                    var changeTerrain = AreaManager.IsPointTerrain (spot);
                    if (bb.enableVolumeLogging)
                    {
                        Debug.LogFormat ("Convert spot to interior | index: {0} | tileset: {1} --> 0 | terrain: {2}", spot.spotIndex, spot.blockTileset, changeTerrain);
                    }
                    AreaManager.ClearTileData (spot);
                    terrainModified |= changeTerrain;
                }

                am.RebuildBlock (spot, bb.enableVolumeLogging);
                am.RebuildCollisionForPoint (spot);
            }
            foreach (var index in editedIndexes)
            {
                am.UpdateDamageAroundIndex (index);
            }

            return terrainModified;
        }

        static bool CheckTilesetOverride (AreaSceneBlackboard bb, AreaVolumePoint avp, int selectedID)
        {
            if (selectedID == avp.blockTileset)
            {
                return false;
            }
            if (bb.swapTilesetOnVolumeEdits)
            {
                return true;
            }
            if (avp.blockTileset == 0)
            {
                return true;
            }
            return bb.overrideTerrainAndRoadTilesetsOnVolumeEdits && AreaSceneHelper.VolumePointHasTerrainOrRoadTilesets (avp);
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

            var changeTerrain = false;
            var fullState = (configuration & TilesetUtility.configurationBitTopSelf) == TilesetUtility.configurationBitTopSelf;
            if (fullState && spot.pointState != AreaVolumePointState.Full)
            {
                changeTerrain = AreaManager.IsPointTerrain (spot);
                spot.pointState = AreaVolumePointState.Full;
            }

            spot.spotConfiguration = configuration;
            spot.spotConfigurationWithDamage = configuration;
            modifiedIndexes.Add (spot.spotIndex);
            return changeTerrain;
        }

        public VolumeAddOperation (AreaSceneBlackboard bb, Checks checks)
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
