using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Area
{
    using Scene;

    sealed class TerrainRemoveOperation : AreaSceneVolumeOperation
    {
        public interface Checks
        {
            bool denyRemoveBlock { get; }
            bool denyTerrainEdit { get; }
            System.Action onActionDenied { get; set; }
        }

        public bool TryGetTargetBlocks (AreaVolumePoint pointStart, Vector3 direction)
        {
            if (checks.denyRemoveBlock || checks.denyTerrainEdit)
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.Log ("Edit terrain denied | op: remove | index: " + pointStart.spotIndex);
                }
                checks.onActionDenied?.Invoke ();
                return false;
            }

            if (bb.enableTerrainShapeLogging)
            {
                Debug.Log ("Edit terrain target block | op: remove | index: " + pointStart.spotIndex);
            }

            face = AreaSceneHelper.GetCompassFromDirection (direction);
            if (!AreaSceneTerrainShapeMode.CheckBrushPoints (pointStart, pointsToEdit, true, AreaSceneHelper.FreeSpacePolicy.SlopePass, bb.enableTerrainShapeLogging))
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.Log ("Edit terrain target block -- point check failed | op: remove");
                }
                return false;
            }

            if (bb.enableTerrainShapeLogging)
            {
                Debug.Log ("Edit terrain target blocks | op: remove | blocks: " + pointsToEdit.Select (pt => pt.spotIndex).ToStringFormatted ());
            }
            return true;
        }

        public bool Apply () => TryChangeBlocks () && UpdateBlocks ();

        bool TryChangeBlocks () => AreaSceneHelper.TryChangeBlocks (pointsToEdit, TrySetBlockEmpty, bb.enableTerrainShapeLogging);
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
            var spotsModified = false;
            var spots = SpotsByFace.Map[(int)face];
            editedIndexes.Clear ();
            modifiedIndexes.Clear ();
            foreach (var point in pointsToEdit)
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.LogFormat ("Update block for point | index: {0} | state: {1}", point.spotIndex, point.pointState);
                }
                editedIndexes.Add (point.spotIndex);
                ClearTerrainOffset (point);
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

                    if (bb.enableTerrainShapeLogging)
                    {
                        Debug.LogFormat
                        (
                            "Update {0} spot in block {1} | index: {2} | state: {3} | configuration: {4}",
                            AreaSceneHelper.SpotNeighborDisplay(neighborIndex, compact: true),
                            point.spotIndex,
                            spot.spotIndex,
                            spot.pointState,
                            AreaSceneHelper.GetPointConfigurationDisplayString (spot.spotConfiguration)
                        );
                    }

                    if (AreaManager.IsSpotInterior (spot))
                    {
                        ConvertInteriorSpot (spot, bb.enableTerrainShapeLogging);
                        spotsModified = true;
                    }
                    spotsModified |= UpdateSpotConfiguration (spot, neighborIndex, bb.enableTerrainShapeLogging);
                }
            }

            foreach (var index in modifiedIndexes)
            {
                var spot = am.points[index];
                am.RebuildBlock (spot, bb.enableTerrainShapeLogging);
                am.RebuildCollisionForPoint (spot);
            }
            foreach (var index in editedIndexes)
            {
                am.UpdateDamageAroundIndex (index);
            }

            return spotsModified;
        }

        void ClearTerrainOffset (AreaVolumePoint point)
        {
            var neighborUp = point.pointsWithSurroundingSpots[WorldSpace.SpotNeighbor.Up];
            if (neighborUp != null && neighborUp.terrainOffset != 0f)
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.LogFormat ("Remove terrain offset from up neighbor | index: {0} | offset: {1:F2}", neighborUp.spotIndex, neighborUp.terrainOffset);
                }
                neighborUp.terrainOffset = 0f;
            }
        }

        static void ConvertInteriorSpot (AreaVolumePoint spot, bool log)
        {
            if (log)
            {
                Debug.LogFormat ("Interior spot converting to terrain | index: {0} | tileset: 0 --> {1}", spot.spotIndex, AreaTilesetHelper.idOfTerrain);
            }
            AreaManager.ClearTileData (spot);
            spot.blockTileset = AreaTilesetHelper.idOfTerrain;
            modifiedIndexes.Add (spot.spotIndex);
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

            var modified = false;
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

                modified = true;
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

                modified = true;
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
            return modified;
        }

        public TerrainRemoveOperation (AreaSceneBlackboard bb, Checks checks)
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
