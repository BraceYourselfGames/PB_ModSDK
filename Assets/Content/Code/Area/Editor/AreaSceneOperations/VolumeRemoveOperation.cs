using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class VolumeRemoveOperation : AreaSceneVolumeOperation
    {
        public interface Checks
        {
            bool denyRemoveBlock { get; }
            System.Action onActionDenied { get; set; }
        }

        public bool TryGetTargetBlocks (AreaVolumePoint pointStart, Vector3 direction)
        {
            if (checks.denyRemoveBlock)
            {
                if (bb.enableVolumeLogging)
                {
                    Debug.Log ("Edit volume denied | op: remove | index: " + pointStart.spotIndex);
                }
                checks.onActionDenied?.Invoke ();
                return false;
            }
            if (bb.enableVolumeLogging)
            {
                Debug.Log ("Edit volume target block | op: remove | index: " + pointStart.spotIndex);
            }
            face = AreaSceneHelper.GetCompassFromDirection (direction);
            var points = AreaManager.CollectPointsInBrush (pointStart, AreaManager.editingVolumeBrush);
            pointsToEdit.Clear ();
            pointsToEdit.AddRange (points);
            return true;
        }

        public bool Apply () => TryChangeBlocks () && UpdateBlocks ();

        bool TryChangeBlocks () => AreaSceneHelper.TryChangeBlocks (pointsToEdit, TrySetBlockEmpty, bb.enableVolumeLogging);
        static bool TrySetBlockEmpty (AreaVolumePoint point, bool log)
        {
            if (point.pointState != AreaVolumePointState.Full)
            {
                return false;
            }
            if (log)
            {
                Debug.Log ("Change point state to empty | index: " + point.spotIndex);
            }
            return true;
        }

        bool UpdateBlocks ()
        {
            var am = bb.am;
            var layerSize = am.boundsFull.x * am.boundsFull.z;
            var terrainModified = false;
            var spots = SpotsByFace.Map[(int)face];
            editedIndexes.Clear ();
            modifiedIndexes.Clear ();
            foreach (var point in pointsToEdit)
            {
                if (bb.enableVolumeLogging)
                {
                    Debug.LogFormat ("Update block for point | index: {0} | state: {1} | tileset: {2}", point.spotIndex, point.pointState, point.blockTileset);
                }
                editedIndexes.Add (point.spotIndex);
                var terrainModifiedPoint = false;
                for (var i = 0; i < spots.Length; i += 1)
                {
                    var neighborIndex = spots[i];
                    var spot = point.pointsWithSurroundingSpots[neighborIndex];
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
                            AreaSceneHelper.SpotNeighborDisplay(neighborIndex, compact: true),
                            spot.spotIndex,
                            spot.pointState,
                            AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration),
                            spot.blockTileset
                        );
                    }

                    if (bb.swapTilesetOnVolumeEdits)
                    {
                        terrainModifiedPoint |= ChangeTileset (spot);
                    }
                    else if (AreaManager.IsSpotInterior (spot))
                    {
                        var faceSpot = i < SpotsByFace.FaceSpotStartIndex ? point.pointsWithSurroundingSpots[spots[i + SpotsByFace.FaceSpotStartIndex]] : null;
                        terrainModifiedPoint |= ConvertInteriorSpot (spot, faceSpot, bb.enableVolumeLogging);
                    }
                    else if (bb.overrideTerrainAndRoadTilesetsOnVolumeEdits && AreaManager.IsPointTerrain (spot))
                    {
                        terrainModifiedPoint |= ChangeTileset (spot);
                    }
                    terrainModifiedPoint |= UpdateSpotConfiguration (spot, neighborIndex, bb.enableVolumeLogging);
                }
                if (terrainModifiedPoint)
                {
                    terrainModified = true;
                    VolumeOperationHelper.TrickleDown (point, bb.volumeTilesetSelected.id, layerSize,  modifiedIndexes, bb.enableVolumeLogging);
                }
            }

            foreach (var index in modifiedIndexes)
            {
                var spot = am.points[index];
                am.RebuildBlock (spot, bb.enableVolumeLogging);
                am.RebuildCollisionForPoint (spot);
            }
            foreach (var index in editedIndexes)
            {
                am.UpdateDamageAroundIndex (index);
            }

            return terrainModified;
        }

        bool ChangeTileset (AreaVolumePoint spot)
        {
            if (spot.blockTileset == bb.volumeTilesetSelected.id)
            {
                return false;
            }

            var selectedID = bb.volumeTilesetSelected.id;
            var changeTerrain = AreaManager.IsPointTerrain (spot) || selectedID == AreaTilesetHelper.idOfTerrain;
            if (bb.enableVolumeLogging)
            {
                Debug.LogFormat ("Change tileset | index: {0} | tileset: {1} --> {2} | terrain: {3}", spot.spotIndex, spot.blockTileset, selectedID, changeTerrain);
            }
            AreaManager.ClearTileData (spot);
            spot.blockTileset = selectedID;
            modifiedIndexes.Add (spot.spotIndex);
            return changeTerrain;
        }

        static bool ConvertInteriorSpot (AreaVolumePoint spot, AreaVolumePoint faceSpot, bool log)
        {
            var isTerrain = AreaManager.IsPointTerrain (faceSpot);
            var tilesetID = faceSpot == null || faceSpot.blockTileset == 0 || isTerrain ? AreaTilesetHelper.idOfFallback : faceSpot.blockTileset;
            if (log)
            {
                Debug.LogFormat
                (
                    isTerrain
                        ? "Interior spot modified under terrain | index: {0} | tileset: 0 --> {1}"
                        : "Interior spot modified | index: {0} | tileset: 0 --> {1}",
                    spot.spotIndex,
                    tilesetID
                );
            }
            AreaManager.ClearTileData (spot);
            spot.blockTileset = tilesetID;
            modifiedIndexes.Add (spot.spotIndex);
            return isTerrain;
        }

        static bool UpdateSpotConfiguration (AreaVolumePoint spot, int neighborIndex, bool log)
        {
            var configuration = spot.spotConfiguration;
            var spotMask = AreaSceneHelper.spotNeighborMasks[neighborIndex];
            configuration &= (byte)~spotMask;
            if (configuration == spot.spotConfiguration)
            {
                return false;
            }

            var changeTerrain = false;
            var clearTileset = configuration == TilesetUtility.configurationEmpty && spot.blockTileset != 0;
            if (clearTileset)
            {
                if (log)
                {
                    Debug.LogFormat
                    (
                        "Spot is empty -- clearing tileset | index: {0} | configuration: {1} --> {2} | tileset: {3} --> 0",
                        spot.spotIndex,
                        AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration),
                        AreaSceneHelper.GetPointConfigurationDisplayString (configuration),
                        spot.blockTileset
                    );
                }

                changeTerrain = AreaManager.IsPointTerrain (spot);
                AreaManager.ClearTileData (spot);
            }

            var emptyState = (configuration & TilesetUtility.configurationBitTopSelf) == 0;
            if (emptyState && spot.pointState != AreaVolumePointState.Empty)
            {
                if (log)
                {
                    Debug.LogFormat
                    (
                        "Spot change state | index: {0} | state: {1} --> {2} | configuration: {3} --> {4}",
                        spot.spotIndex,
                        spot.pointState,
                        AreaVolumePointState.Empty,
                        AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration),
                        AreaSceneHelper.GetPointConfigurationDisplayString (configuration)
                    );
                }

                changeTerrain = changeTerrain || AreaManager.IsPointTerrain (spot);
                spot.pointState = AreaVolumePointState.Empty;
            }

            if (log && !clearTileset && !emptyState)
            {
                Debug.LogFormat
                (
                    "Spot configuration modified | index: {0} | configuration: {1} --> {2}",
                    spot.spotIndex,
                    AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration),
                    AreaSceneHelper.GetPointConfigurationDisplayString (configuration)
                );
            }

            spot.spotConfiguration = configuration;
            spot.spotConfigurationWithDamage = configuration;
            modifiedIndexes.Add (spot.spotIndex);
            return changeTerrain;
        }

        public VolumeRemoveOperation (AreaSceneBlackboard bb, Checks checks)
        {
            this.bb = bb;
            this.checks = checks;
        }

        readonly AreaSceneBlackboard bb;
        readonly Checks checks;
        readonly List<AreaVolumePoint> pointsToEdit = new List<AreaVolumePoint> ();
        WorldSpace.Compass face;

        static readonly List<int> editedIndexes = new List<int> ();
        static readonly List<int> modifiedIndexes = new List<int> ();

    }
}
