using UnityEngine;

namespace Area
{
    using Scene;

    sealed class ChangeTerrainOffsetOperation : AreaSceneVolumeOperation
    {
        public interface Checks
        {
            bool denyTerrainEdit { get; }
            System.Action onActionDenied { get; set; }
        }

        public bool TryGetTargetBlocks (AreaVolumePoint pointStart, Vector3 direction)
        {
            // Terrain modifications require the target block to be above the terrain so get the up neighbor in that case. The up spot neighbors of the target block
            // should all have the terrain tileset.

            if (checks.denyTerrainEdit)
            {
                if (bb.enableTerrainShapeLogging)
                {
                    Debug.Log ("Edit terrain denied | op: change terrain offset | index: " + pointStart.spotIndex);
                }
                checks.onActionDenied?.Invoke ();
                return false;
            }

            var (compass, newPoint) = AreaSceneHelper.GetNeighborFromDirection (pointStart, direction);
            if (newPoint == null)
            {
                Debug.LogErrorFormat ("Edit volume -- neighbor is null | op: change terrain offset | index: {0} | neighbor: {1}", pointStart.spotIndex, compass);
                return false;
            }

            if (bb.enableTerrainShapeLogging)
            {
                Debug.LogFormat ("Edit volume -- use neighbor | op: change terrain offset | index: {0} --> {1} | neighbor: {2}", pointStart.spotIndex, newPoint.spotIndex, compass);
            }
            point = newPoint;
            return true;
        }

        public bool Apply ()
        {
            if (point.pointState != AreaVolumePointState.Empty)
            {
                return false;
            }
            switch (offsetDirection)
            {
                case OffsetDirection.Up:
                    if (point.terrainOffset > maxOffset || point.terrainOffset.RoughlyEqual (maxOffset))
                    {
                        return false;
                    }
                    point.terrainOffset = Mathf.RoundToInt (point.terrainOffset * WorldSpace.BlockSize + 1f) / WorldSpace.BlockSize;
                    if (bb.enableTerrainShapeLogging)
                    {
                        Debug.LogFormat
                        (
                            "Point {0} ({1}, {2}) now has offset {3}",
                            point.spotIndex,
                            point.pointPositionIndex,
                            point.pointState,
                            point.terrainOffset
                        );
                    }
                    return true;
                case OffsetDirection.Down:
                    if (point.terrainOffset < minOffset || point.terrainOffset.RoughlyEqual (minOffset))
                    {
                        return false;
                    }
                    point.terrainOffset = Mathf.RoundToInt (point.terrainOffset * WorldSpace.BlockSize - 1f) / WorldSpace.BlockSize;
                    if (bb.enableTerrainShapeLogging)
                    {
                        Debug.LogFormat
                        (
                            "Point {0} ({1}, {2}) now has offset {3}",
                            point.spotIndex,
                            point.pointPositionIndex,
                            point.pointState,
                            point.terrainOffset
                        );
                    }
                    return true;
            }
            return false;
        }

        public ChangeTerrainOffsetOperation (AreaSceneBlackboard bb, Checks checks, OffsetDirection direction)
        {
            this.bb = bb;
            this.checks = checks;
            offsetDirection = direction;
        }

        readonly AreaSceneBlackboard bb;
        readonly Checks checks;
        readonly OffsetDirection offsetDirection;
        AreaVolumePoint point;

        public enum OffsetDirection
        {
            Down = 0,
            Up
        }

        const float maxOffset = 1f;
        const float minOffset = -1f;
    }
}
