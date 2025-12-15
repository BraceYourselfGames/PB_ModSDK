using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PhantomBrigade;
using PhantomBrigade.Data;

namespace Area
{
    public static class AreaUtility
    {
        public const float pointColliderInflation = 0.1f;
        public static readonly Vector3 spotOffset = new Vector3 (-1f, 1f, -1f) * TilesetUtility.blockAssetHalfSize;
        public const int invalidIndex = -1;

        private static Dictionary<int, int[]> reusedExternalIndexes = new Dictionary<int, int[]> ();
        private static int[] reusedIntegerArray8 = new int[8];

        private static int reusedInteger = 0;
        private static int reusedExternalIndex = -1;
        private static int reusedInternalIndex = -1;

        private static Vector3Int reusedPositionStart;
        private static Vector3Int reusedPositionFinal;

        private static Vector3 reusedPositionCorrected;

        private static AreaVolumePoint reusedAreaVolumePoint;

        public static string ToLog (this AreaVolumePoint point)
        {
            if (point != null)
                return $"({point.pointState}, I-{point.spotIndex} {point.pointPositionIndex}, C-{point.spotConfiguration})";
            else
                return "(null)";
        }

        public static void ClearCache ()
        {
            reusedExternalIndexes.Clear ();
        }

        public static void GetNeighbourIndexesInXxYxZ (int externalStartIndex, Vector3Int boundsNeighbourhood, Vector3Int pivotPosition, Vector3Int boundsArea, int[] indexes)
        {
            for (int y = 0; y < boundsNeighbourhood.y; ++y)
            {
                for (int z = 0; z < boundsNeighbourhood.z; ++z)
                {
                    for (int x = 0; x < boundsNeighbourhood.x; ++x)
                    {
                        reusedPositionStart = GetVolumePositionFromIndex (externalStartIndex, boundsArea);
                        reusedPositionFinal = new Vector3Int (reusedPositionStart.x + x + pivotPosition.x, reusedPositionStart.y + y + pivotPosition.y, reusedPositionStart.z + z + pivotPosition.z);

                        reusedInternalIndex = x + boundsNeighbourhood.x * z + boundsNeighbourhood.x * boundsNeighbourhood.z * y;
                        reusedExternalIndex = invalidIndex;

                        if
                        (
                            reusedPositionFinal.x >= 0 &&
                            reusedPositionFinal.y >= 0 &&
                            reusedPositionFinal.z >= 0 &&
                            reusedPositionFinal.x < boundsArea.x &&
                            reusedPositionFinal.y < boundsArea.y &&
                            reusedPositionFinal.z < boundsArea.z
                        )
                            reusedExternalIndex = GetIndexFromInternalPosition (reusedPositionFinal, boundsArea);

                        indexes[reusedInternalIndex] = reusedExternalIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Used to fetch indexes in a volume neighbouring a given point. Warning: always copy an array received from this method if your method depends on multiple results at once, since this method is reusing arrays and returns same reference for same given neighbourhood size.
        /// </summary>
        /// <param name="externalStartIndex">The origin point of the operation</param>
        /// <param name="boundsNeighbourhood">The bounds of the neighbourhood to return</param>
        /// <param name="pivotPosition">The pivot/offset of the returned set, useful since by default indexes are returned with origin in a corner. For example, for a 3x3x3 neighbourhood, -1,-1,-1 pivot puts the origin into the center of returned set.</param>
        /// <param name="boundsArea">Full bounds of the scene involved in the operation</param>
        /// <returns></returns>

        public static int[] GetNeighbourIndexesInXxYxZ (int externalStartIndex, Vector3Int boundsNeighbourhood, Vector3Int pivotPosition, Vector3Int boundsArea)
        {
            var size = boundsNeighbourhood.x * boundsNeighbourhood.y * boundsNeighbourhood.z;
            if (!reusedExternalIndexes.ContainsKey (size))
            {
                // Debug.Log ("Attempting to add new size to reused external indexes: " + size + " | Total size count: " + reusedExternalIndexes.Count);
                reusedExternalIndexes.Add (size, new int[size]);
            }

            reusedPositionStart = GetVolumePositionFromIndex (externalStartIndex, boundsArea);
            var externalIndexes = reusedExternalIndexes[size];

            for (var y = 0; y < boundsNeighbourhood.y; y += 1)
            {
                for (var z = 0; z < boundsNeighbourhood.z; z += 1)
                {
                    for (var x = 0; x < boundsNeighbourhood.x; x += 1)
                    {
                        var externalFinalPosition = new Vector3Int
                        (
                            reusedPositionStart.x + x + pivotPosition.x,
                            reusedPositionStart.y + y + pivotPosition.y,
                            reusedPositionStart.z + z + pivotPosition.z
                        );
                        var internalIndex = x + boundsNeighbourhood.x * z + boundsNeighbourhood.x * boundsNeighbourhood.z * y;
                        externalIndexes[internalIndex] = GetIndexFromInternalPosition (externalFinalPosition, boundsArea);
                    }
                }
            }

            return externalIndexes;
        }



        public static int[] GetNeighbourIndexesIn2x2x2 (int index, Vector3Int pivotPosition, Vector3Int boundsFull)
        {
            if (reusedIntegerArray8 == null || reusedIntegerArray8.Length != 8)
                reusedIntegerArray8 = new int[8];

            reusedIntegerArray8 = GetNeighbourIndexesInXxYxZ (index, Vector3Int.size2x2x2, pivotPosition, boundsFull);

            reusedInteger = reusedIntegerArray8[2];
            reusedIntegerArray8[2] = reusedIntegerArray8[3];
            reusedIntegerArray8[3] = reusedInteger;

            reusedInteger = reusedIntegerArray8[6];
            reusedIntegerArray8[6] = reusedIntegerArray8[7];
            reusedIntegerArray8[7] = reusedInteger;

            // reusedIntegerArray8[0] = index - (boundsArea.x * boundsArea.z) - boundsArea.x - 1 + shift;
            // reusedIntegerArray8[1] = index - (boundsArea.x * boundsArea.z) - boundsArea.x + shift;
            // reusedIntegerArray8[2] = index - (boundsArea.x * boundsArea.z) + shift;
            // reusedIntegerArray8[3] = index - (boundsArea.x * boundsArea.z) - 1 + shift;
            // reusedIntegerArray8[4] = index - boundsArea.x - 1 + shift;
            // reusedIntegerArray8[5] = index - boundsArea.x + shift;
            // reusedIntegerArray8[6] = index + shift;
            // reusedIntegerArray8[7] = index - 1 + shift;

            return reusedIntegerArray8;
        }

        public static void RearrangeNeighbourArrayIn2x2x2 (AreaVolumePoint[] neighbours)
        {
            if (neighbours == null || neighbours.Length != 8)
                return;

            reusedAreaVolumePoint = neighbours[2];
            neighbours[2] = neighbours[3];
            neighbours[3] = reusedAreaVolumePoint;

            reusedAreaVolumePoint = neighbours[6];
            neighbours[6] = neighbours[7];
            neighbours[7] = reusedAreaVolumePoint;
        }

        public static int GetIndexFromVolumePosition (Vector3Int position, Vector3Int bounds, bool skipBoundsCheck = false) =>
            skipBoundsCheck || GetIsInBounds (position, bounds)
                ? position.x + position.z * bounds.x + position.y * bounds.x * bounds.z
                : invalidIndex;

        public static int GetIndexFromInternalPosition (Vector3Int position, Vector3Int bounds)
        {
            // Debug.Log ("AU | GetIndexFromInternalPosition | " + position);
            if (GetIsInBounds (position, bounds))
            {
                return position.x
                     + position.z * bounds.x
                     + position.y * bounds.x * bounds.z;
            }
            return invalidIndex;
        }

        public static int GetIndexFromInternalPosition (int x, int z, int boundsX, int boundsZ)
        {
            bool boundsFit =
                x >= 0 && x < boundsX &&
                z >= 0 && z < boundsZ;

            // Debug.Log ("AU | GetIndexFromInternalPosition | " + position);
            if (boundsFit)
            {
                return
                (
                    x +
                    z * boundsX
                );
            }
            else
                return invalidIndex;
        }

        public static Vector3Int GetInternalPointPositionFromWorld (Vector3 positionInWorld, Vector3 positionOfVolume)
        {
	        var positionCorrected = Vector3.Scale ((positionInWorld - positionOfVolume), new Vector3 (1f, -1f, 1f)) / TilesetUtility.blockAssetSize;
	        var positionFinal = new Vector3Int (Mathf.RoundToInt (positionCorrected.x), Mathf.RoundToInt (positionCorrected.y), Mathf.RoundToInt (positionCorrected.z));

	        return positionFinal;
        }

        public static bool GetIsInBounds (Vector3Int pos, Vector3Int bounds)
        {
	        return pos.x >= 0 &
	               pos.x < bounds.x &
	               pos.y >= 0 &
	               pos.y < bounds.y &
	               pos.z >= 0 &
	               pos.z < bounds.z;
        }
        public static int GetIndexFromWorldPosition (Vector3 positionInWorld, Vector3 positionOfVolume, Vector3Int bounds)
        {
            // Debug.Log ("AU | GetIndexFromWorldPosition | PW: " + positionInWorld + " | PC: " + reusedPositionCorrected + " | PF: " + reusedPositionFinal);
            return GetIndexFromInternalPosition (GetInternalPointPositionFromWorld(positionInWorld, positionOfVolume), bounds);
        }

        public static Vector3Int GetVolumePositionFromIndex (int index, Vector3Int bounds)
        {
            if (bounds.x == 0 || bounds.z == 0)
                Debug.LogError ("Division by zero in bounds " + bounds);
            return new Vector3Int (index % bounds.x, index / (bounds.z * bounds.x), (index / bounds.x) % bounds.z);
        }

        public static Vector3 GetLocalPositionFromGridPosition (Vector3Int volumePosition)
        {
            return new Vector3 (volumePosition.x, -volumePosition.y, volumePosition.z) * TilesetUtility.blockAssetSize;
        }



        public static AreaVolumePointConfiguration Transform (this AreaVolumePointConfiguration c, int plane, int rotation)
        {
            // Debug.Log ("AU | Transform | " + plane + " | " + rotation);

            // See layout_helper.skp for illustrated transformations
            // This is a shortcut to getting configurations after rotations to planes and within planes
            // Couldn't figure an elegant way to calculate this, but this should be enough for most purposes

            //                                                   0          1          2          3          4          5          6          7

            // Horizontal plane
            if (plane == 0)
            {
                if (rotation == 0)
                    return new AreaVolumePointConfiguration (c.corner0, c.corner1, c.corner2, c.corner3, c.corner4, c.corner5, c.corner6, c.corner7);
                else if (rotation == 1)
                    return new AreaVolumePointConfiguration (c.corner3, c.corner0, c.corner1, c.corner2, c.corner7, c.corner4, c.corner5, c.corner6);
                else if (rotation == 2)
                    return new AreaVolumePointConfiguration (c.corner2, c.corner3, c.corner0, c.corner1, c.corner6, c.corner7, c.corner4, c.corner5);
                else
                    return new AreaVolumePointConfiguration (c.corner1, c.corner2, c.corner3, c.corner0, c.corner5, c.corner6, c.corner7, c.corner4);
            }
            // Vertical plane A
            else if (plane == 1)
            {
                if (rotation == 0)
                    return new AreaVolumePointConfiguration (c.corner3, c.corner2, c.corner6, c.corner7, c.corner0, c.corner1, c.corner5, c.corner4);
                else if (rotation == 1)
                    return new AreaVolumePointConfiguration (c.corner2, c.corner1, c.corner5, c.corner6, c.corner3, c.corner0, c.corner4, c.corner7);
                else if (rotation == 2)
                    return new AreaVolumePointConfiguration (c.corner1, c.corner0, c.corner4, c.corner5, c.corner2, c.corner3, c.corner7, c.corner6);
                else
                    return new AreaVolumePointConfiguration (c.corner0, c.corner3, c.corner7, c.corner4, c.corner1, c.corner2, c.corner6, c.corner5);
            }
            // Vertical plane B
            else if (plane == 2)
            {
                if (rotation == 0)
                    return new AreaVolumePointConfiguration (c.corner4, c.corner0, c.corner3, c.corner7, c.corner5, c.corner1, c.corner2, c.corner6);
                else if (rotation == 1)
                    return new AreaVolumePointConfiguration (c.corner7, c.corner3, c.corner2, c.corner6, c.corner4, c.corner0, c.corner1, c.corner5);
                else if (rotation == 2)
                    return new AreaVolumePointConfiguration (c.corner6, c.corner2, c.corner1, c.corner5, c.corner7, c.corner3, c.corner0, c.corner4);
                else
                    return new AreaVolumePointConfiguration (c.corner5, c.corner1, c.corner0, c.corner4, c.corner6, c.corner2, c.corner3, c.corner7);
            }
            // Vertical plane C
            else if (plane == 3)
            {
                if (rotation == 0)
                    return new AreaVolumePointConfiguration (c.corner4, c.corner5, c.corner1, c.corner0, c.corner7, c.corner6, c.corner2, c.corner3);
                else if (rotation == 1)
                    return new AreaVolumePointConfiguration (c.corner7, c.corner4, c.corner0, c.corner3, c.corner6, c.corner5, c.corner1, c.corner2);
                else if (rotation == 2)
                    return new AreaVolumePointConfiguration (c.corner6, c.corner7, c.corner3, c.corner2, c.corner5, c.corner4, c.corner0, c.corner1);
                else
                    return new AreaVolumePointConfiguration (c.corner5, c.corner6, c.corner2, c.corner1, c.corner4, c.corner7, c.corner3, c.corner0);
            }
            // Vertical plane D
            else if (plane == 4)
            {
                if (rotation == 0)
                    return new AreaVolumePointConfiguration (c.corner1, c.corner5, c.corner6, c.corner2, c.corner0, c.corner4, c.corner7, c.corner3);
                else if (rotation == 1)
                    return new AreaVolumePointConfiguration (c.corner0, c.corner4, c.corner5, c.corner1, c.corner3, c.corner7, c.corner6, c.corner2);
                else if (rotation == 2)
                    return new AreaVolumePointConfiguration (c.corner3, c.corner7, c.corner4, c.corner0, c.corner2, c.corner6, c.corner5, c.corner1);
                else
                    return new AreaVolumePointConfiguration (c.corner2, c.corner6, c.corner7, c.corner3, c.corner1, c.corner5, c.corner4, c.corner0);
            }
            // Upside down plane
            else
            {
                if (rotation == 0)
                    return new AreaVolumePointConfiguration (c.corner7, c.corner6, c.corner5, c.corner4, c.corner3, c.corner2, c.corner1, c.corner0);
                else if (rotation == 1)
                    return new AreaVolumePointConfiguration (c.corner6, c.corner5, c.corner4, c.corner7, c.corner2, c.corner1, c.corner0, c.corner3);
                else if (rotation == 2)
                    return new AreaVolumePointConfiguration (c.corner5, c.corner4, c.corner7, c.corner6, c.corner1, c.corner0, c.corner3, c.corner2);
                else
                    return new AreaVolumePointConfiguration (c.corner4, c.corner7, c.corner6, c.corner5, c.corner0, c.corner3, c.corner2, c.corner1);
            }
        }

        public static bool IsNeighbor (this AreaVolumePoint origin, AreaVolumePoint neighbour)
        {
            return
                neighbour.pointPositionIndex.x >= (origin.pointPositionIndex.x - 1) &&
                neighbour.pointPositionIndex.x <= (origin.pointPositionIndex.x + 1) &&
                neighbour.pointPositionIndex.y >= (origin.pointPositionIndex.y - 1) &&
                neighbour.pointPositionIndex.y <= (origin.pointPositionIndex.y + 1) &&
                neighbour.pointPositionIndex.z >= (origin.pointPositionIndex.z - 1) &&
                neighbour.pointPositionIndex.z <= (origin.pointPositionIndex.z + 1);
        }

        public static AreaVolumePoint GetRemainsPointFromRubblePoint (AreaVolumePoint pointStart)
        {
            bool spotAIsSuitable = pointStart.pointsWithSurroundingSpots[0] != null && ValidatePointForRemains (pointStart.pointsWithSurroundingSpots[0]);
            bool spotBIsSuitable = pointStart.pointsWithSurroundingSpots[1] != null && ValidatePointForRemains (pointStart.pointsWithSurroundingSpots[1]);
            bool spotCIsSuitable = pointStart.pointsWithSurroundingSpots[2] != null && ValidatePointForRemains (pointStart.pointsWithSurroundingSpots[2]);
            bool spotDIsSuitable = pointStart.pointsWithSurroundingSpots[3] != null && ValidatePointForRemains (pointStart.pointsWithSurroundingSpots[3]);

            if (spotAIsSuitable && spotBIsSuitable && spotCIsSuitable && spotDIsSuitable)
                return pointStart.pointsWithSurroundingSpots[0];

            else
                return null;
        }

        private static bool ValidatePointForRemains (AreaVolumePoint point)
        {
            if (point.spotPresent)
                return false;

            return point.spotConfigurationWithDamage == AreaNavUtility.configFloor;

            /*
            for (int i = 0; i < AreaNavUtility.maskFloorsAll.Length; ++i)
            {
                if (AreaNavUtility.maskFloorsAll[i] == point.spotConfigurationWithDamage)
                    return true;
            }

            return false;*/
        }

        public static AreaVolumePoint GetRubblePointBelow (Vector3 positionStart, List<AreaVolumePoint> points, Vector3Int bounds)
        {
            AreaVolumePoint result = null;

            int index = AreaUtility.GetIndexFromWorldPosition (positionStart, Vector3.zero, bounds);
            if (index.IsValidIndex (points))
            {
                AreaVolumePoint closestPointToStart = points[index];
                result = GetRubblePointBelow (closestPointToStart, 0);
            }

            return result;
        }

        public static AreaVolumePoint GetRubblePointBelow (AreaVolumePoint pointStart, int depth)
        {
            AreaVolumePoint result = null;
            if (pointStart.pointsInSpot != null)
            {
                AreaVolumePoint pointBelow = pointStart.pointsInSpot[4];
                if (pointBelow != null)
                {
                    /*
                    if (pointBelow.state != AreaVolumePointState.Full)
                        result = GetRubblePointBelow (pointBelow, depth + 1);
                    else
                        result = pointBelow;
                    */

                    if (pointBelow.pointState == AreaVolumePointState.Full && pointBelow.IsSurroundedByFullPoints ())
                        result = pointBelow;
                    else
                        result = GetRubblePointBelow (pointBelow, depth + 1);
                }
            }
            return result;
        }

        public static void RecheckColumnForRubble (AreaVolumePoint pointCurrent, bool destroyedPointPresent)
        {
            if (!destroyedPointPresent && pointCurrent.instancesRubble != null)
            {
                for (int i = 0; i < pointCurrent.instancesRubble.Count; ++i)
                    pointCurrent.instancesRubble[i].SetActive (false);
                pointCurrent.instancesRubble = null;
            }

            if (pointCurrent.pointState == AreaVolumePointState.Full)
                destroyedPointPresent = false;

            if (pointCurrent.pointsInSpot != null)
            {
                AreaVolumePoint pointBelow = pointCurrent.pointsInSpot[4];
                if (pointBelow != null)
                    RecheckColumnForRubble (pointBelow, destroyedPointPresent);
            }
        }

        public static int[] configurationIndexRemapping = new int[] { 0, 1, 3, 2, 4, 5, 7, 6 };

        public static Vector3 GetPropOffsetAsVector (AreaPropPrototypeData prototype, float offsetX, float offsetZ, int rotation, Quaternion rootRotation)
        {
	        if (prototype.prefab == null)
				return Vector3.zero;

			switch(prototype.prefab.compatibility)
			{
				case AreaProp.Compatibility.Floor:
					return GetPropOffsetAsVector (offsetX, offsetZ, rotation);
				case AreaProp.Compatibility.WallStraightMiddle:
					return	(rootRotation * Vector3.right) * offsetX +
							(rootRotation * Vector3.up) * offsetZ;
				case AreaProp.Compatibility.WallStraightBottomToFloor:
					return GetPropOffsetAsVector (0.0f, offsetZ, rotation);
				case AreaProp.Compatibility.WallStraightTopToFloor:
					return GetPropOffsetAsVector (0.0f, offsetZ, rotation);
			}

			return Vector3.zero;
        }

        public static Vector3 GetPropOffsetAsVector (AreaPlacementProp placement, Quaternion rootRotation)
        {
            return GetPropOffsetAsVector(placement.prototype, placement.offsetX, placement.offsetZ, placement.rotation, rootRotation);
        }

        public static Vector3 GetPropOffsetAsVector (float offsetX, float offsetZ, int rotation)
        {
	        Vector3 offset = new Vector3 (Mathf.Clamp (offsetX, -1.5f, 1.5f), 0f, Mathf.Clamp (offsetZ, -1.5f, 1.5f));
	        Vector3 offsetRotated = Quaternion.Euler (0f, -90f * rotation, 0f) * offset;
            return offsetRotated;
        }

        public static Vector3 SnapPointToGrid (Vector3 point)
        {
            var gridStep = 3f;
            var gridOffset = 1.5f;

            var clampedPoint = new Vector3
            (
                Mathf.Round ((point.x - gridOffset) / gridStep) * gridStep + gridOffset,
                Mathf.Round ((point.y + gridOffset) / gridStep) * gridStep - gridOffset,
                Mathf.Round ((point.z - gridOffset) / gridStep) * gridStep + gridOffset
            );

            return clampedPoint;
        }

        public static Vector3 GroundPoint (Vector3 point)
        {
            var groundingRayOrigin = point + Vector3.up * 100f;
            var groundingRay = new Ray (groundingRayOrigin, Vector3.down);

            if (Physics.Raycast (groundingRay, out var groundingHit, 200f, LayerMasks.environmentMask))
                return groundingHit.point;
            return point;
        }

        #if !PB_MODSDK
        public static void SaveIntegrityToList (AreaManager am, List<DataBlockAreaIntegrity> integrities)
        {
            if (am == null || am.points == null || integrities == null)
                return;

            for (int pointIndex = 0; pointIndex < am.points.Count; ++pointIndex)
            {
                AreaVolumePoint pointSource = am.points[pointIndex];
                if
                (
                    pointSource.pointState != AreaVolumePointState.Empty &&
                    pointSource.integrity < 1f
                )
                {
                    var integrity = new DataBlockAreaIntegrity
                    {
                        i = pointIndex,
                        v = pointSource.integrity
                    };

                    integrities.Add (integrity);
                }
            }
        }
        #endif

        public static float GetNearestFloorHeight (float y) =>
            Mathf.Round ((y + TilesetUtility.blockAssetHalfSize) / TilesetUtility.blockAssetSize)
                * TilesetUtility.blockAssetSize
                - TilesetUtility.blockAssetHalfSize;


        #if !PB_MODSDK
        private static readonly List<AreaVolumePoint> pointOriginsForStructureRescan = new List<AreaVolumePoint> ();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryAddingScanOrigin (AreaVolumePoint p)
        {
            if (p == null)
                return;
            if (p.pointState == AreaVolumePointState.Full & !p.simulatedChunkPresent & !p.indestructibleAny)
                pointOriginsForStructureRescan.Add (p);
        }

        public static void ScanNeighborsDisconnectedOnDestruction
        (
            AreaVolumePoint pointDestroyed,
            List<AreaVolumePoint> points,
            bool scheduleSimulation = true,
            List<AreaVolumePoint> pointsToDestroy = null
        )
        {
            if (points == null)
                return;

            var pointsCount = points.Count;
            if (pointsCount == 0)
                return;

            pointOriginsForStructureRescan.Clear ();
            TryAddingScanOrigin (pointDestroyed.pointsInSpot[WorldSpace.Compass.Down]);
            TryAddingScanOrigin (pointDestroyed.pointsInSpot[WorldSpace.Compass.East]);
            TryAddingScanOrigin (pointDestroyed.pointsWithSurroundingSpots[WorldSpace.Compass.West]);
            TryAddingScanOrigin (pointDestroyed.pointsInSpot[WorldSpace.Compass.North]);
            TryAddingScanOrigin (pointDestroyed.pointsWithSurroundingSpots[WorldSpace.Compass.South]);
            TryAddingScanOrigin (pointDestroyed.pointsWithSurroundingSpots[WorldSpace.Compass.Up]);

            var log = DataShortcuts.sim.debugCombatStructureAnalysis;
            if (log)
                Debug.Log ($"After damage at point {pointDestroyed.spotIndex}, there are {pointOriginsForStructureRescan.Count} surrounding points requiring structure connection check");

            if (pointOriginsForStructureRescan.Count == 0)
                return;

            var countDisconnected = 0;
            foreach (var pointOrigin in pointOriginsForStructureRescan)
            {
                var pointsIsolated = BuildIsolatedPointListFromOrigin (pointOrigin, points, false, pointsToDestroy);
                if (pointsIsolated.Count == 0)
                    continue;

                countDisconnected += 1;
                if (log)
                {
                    Debug.Log ($"New isolated set discovered | Origin: {pointOrigin.spotIndex} | Points: {pointsIsolated.Count} | Detaching...");
                    #if UNITY_EDITOR
                    DebugExtensions.DrawCube (pointOrigin.pointPositionLocal, Vector3.one * 1.5f, Color.white, 5f);
                    foreach (var pointIsolated in pointsIsolated)
                    {
                        Debug.DrawLine (pointIsolated.pointPositionLocal, pointIsolated.pointPositionLocal + new Vector3 (0f, 1f, 0f), Color.red, 5f);
                    }
                    #endif
                }

                if (scheduleSimulation)
                    AreaManager.OnIsolatedStructureDiscovery (pointsIsolated);
            }

            if (countDisconnected != 0 & log)
                Debug.Log ($"Out of {pointOriginsForStructureRescan.Count} surrounding points checked, {countDisconnected} points were confirmed disconnected");
        }



        public static int structureAnalysisCounter = 0;
        private static bool debugReachability = false;

        private static int pointStackIterations = 0;
        private static int pointStackSize = 0;
        private static AreaVolumePoint[] pointStack = null;
        private static readonly HashSet<int> pointsVisited = new HashSet<int> ();
        private static readonly List<AreaVolumePoint> pointsAccumulated = new List<AreaVolumePoint> ();
        private static AreaVolumePoint pointIndestructibleFirst = null;

        public enum ScanResult
        {
            StopIndestructible = 0,
            StopDestroyed,
            Stop,
            SavedForNextStep,
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ScanResult ScanNeighborStructure (AreaVolumePoint pointStart, AreaVolumePoint pointNeighbour)
        {
            if (pointNeighbour == null)
                return ScanResult.Stop;
            if (pointNeighbour.pointState == AreaVolumePointState.Empty)
                return ScanResult.Stop;
            if (pointsVisited.Contains (pointNeighbour.spotIndex))
                return ScanResult.Stop;
            if (pointNeighbour.pointState == AreaVolumePointState.FullDestroyed)
                return ScanResult.StopDestroyed;

            if (!pointNeighbour.simulatedChunkPresent & pointNeighbour.indestructibleAny)
            {
                pointIndestructibleFirst = pointNeighbour;
                return ScanResult.StopIndestructible;
            }

            pointStack[pointStackSize] = pointNeighbour;
            pointStackSize += 1;

            #if UNITY_EDITOR
            if (debugReachability)
            {
                var progress = (float)pointStackIterations;
                var hue = (progress * 0.0002f) % 1f;
                var lum = Mathf.Clamp (1f - progress * 0.0001f, 0.2f, 0.95f);
                var sat = 1f - Mathf.Max (0f, 0.8f - progress * 0.0002f);
                Debug.DrawLine (pointStart.pointPositionLocal, pointNeighbour.pointPositionLocal, Color.HSVToRGB (hue, sat, lum), 5f);
            }
            #endif

            return ScanResult.SavedForNextStep;
        }

        public static List<AreaVolumePoint> BuildIsolatedPointListFromOrigin
        (
            AreaVolumePoint pointOrigin,
            List<AreaVolumePoint> points,
            bool debug,
            List<AreaVolumePoint> pointsToDestroy
        )
        {
            pointsAccumulated.Clear ();
            if (points == null)
                return pointsAccumulated;

            var count = points.Count;
            if (count == 0)
                return pointsAccumulated;

            if (pointOrigin == null)
                return pointsAccumulated;
            if (pointOrigin.simulationRequested | pointOrigin.simulatedChunkPresent)
                return pointsAccumulated;

            if (pointStack == null || pointStack.Length < count)
                pointStack = new AreaVolumePoint[count];

            debugReachability = debug && DataShortcuts.sim.debugCombatStructureAnalysis;
            if (debugReachability)
            {
                Debug.Log ($"Checking if structure rescan is needed on change at point {pointOrigin.spotIndex}");
                #if UNITY_EDITOR
                Debug.DrawLine (pointOrigin.pointPositionLocal, pointOrigin.pointPositionLocal + new Vector3 (3f, 3f, 3f), Color.yellow, 5f);
                #endif
            }

            var countDestroyInitial = pointsToDestroy != null ? pointsToDestroy.Count : 0;
            pointsVisited.Clear ();
            pointIndestructibleFirst = null;
            pointStack[0] = pointOrigin;
            pointStackSize = 1;

            const int iterationLimit = 1000000;
            for (pointStackIterations = 0; pointStackSize > 0 & pointStackIterations < iterationLimit; pointStackIterations += 1)
            {
                pointStackSize -= 1;
                var point = pointStack[pointStackSize];

                int pointIndex = point.spotIndex;
                if (pointsVisited.Contains (pointIndex))
                    continue;

                pointsVisited.Add (pointIndex);
                pointsAccumulated.Add (point);

                // Start by attempting to add Y+ point to stack (TryAddingToStack returns a bool to confirm if this succeeds)
                var pointVerticalStart = point;
                var pointVerticalNext = pointVerticalStart.pointsInSpot[WorldSpace.Compass.Down];
                var scanResult = ScanNeighborStructure (pointVerticalStart, pointVerticalNext);

                // If that succeeds, keep trying to go down and expand stack while possible
                while (scanResult == ScanResult.SavedForNextStep)
                {
                    pointVerticalStart = pointVerticalNext;
                    pointVerticalNext = pointVerticalStart.pointsInSpot[WorldSpace.Compass.Down];
                    scanResult = ScanNeighborStructure (pointVerticalStart, pointVerticalNext);
                }

                if (scanResult == ScanResult.StopIndestructible)
                    break;

                if (ScanNeighborStructure (point, point.pointsInSpot[WorldSpace.Compass.East]) == ScanResult.StopIndestructible)
                    break;
                if (ScanNeighborStructure (point, point.pointsInSpot[WorldSpace.Compass.North]) == ScanResult.StopIndestructible)
                    break;
                if (ScanNeighborStructure (point, point.pointsWithSurroundingSpots[WorldSpace.Compass.West]) == ScanResult.StopIndestructible)
                    break;
                if (ScanNeighborStructure (point, point.pointsWithSurroundingSpots[WorldSpace.Compass.South]) == ScanResult.StopIndestructible)
                    break;
                if (ScanNeighborStructure (point, point.pointsWithSurroundingSpots[WorldSpace.Compass.Up]) == ScanResult.StopIndestructible)
                    break;

                if (pointsToDestroy == null)
                    continue;

                if (scanResult != ScanResult.StopDestroyed | pointVerticalStart != point)
                    continue;

                var forceDestroy = false;
                for (var i = WorldSpace.Compass.East; !forceDestroy & i <= WorldSpace.Compass.NorthEast; i += 1)
                {
                    var pointNeighbour = pointVerticalNext.pointsInSpot[i];
                    if (pointNeighbour == null || pointNeighbour.pointState != AreaVolumePointState.Full)
                        continue;
                    forceDestroy = true;
                }
                for (var i = WorldSpace.Compass.SouthWest; !forceDestroy & i <= WorldSpace.Compass.West; i += 1)
                {
                    var pointNeighbour = pointVerticalNext.pointsWithSurroundingSpots[i];
                    if (pointNeighbour == null || pointNeighbour.pointState != AreaVolumePointState.Full)
                        continue;
                    forceDestroy = true;
                }

                if (!forceDestroy)
                    continue;

                pointsAccumulated.RemoveAt (pointsAccumulated.Count - 1);
                pointsToDestroy.Add (point);
            }

            // Stop going if this point already belongs to an older sim chunk
            if (pointIndestructibleFirst != null)
            {
                if (debugReachability)
                {
                    var point = pointIndestructibleFirst;
                    Debug.Log ($"Point {pointOrigin.spotIndex} is connected to indestructible point {point.spotIndex} within {pointStackIterations} iterations");
                    #if UNITY_EDITOR
                    Debug.DrawLine (point.pointPositionLocal, point.instancePosition + new Vector3 (-3f, -3f, -3f), Color.cyan, 5f);
                    #endif
                }
                pointsAccumulated.Clear ();
                if (pointsToDestroy != null && pointsToDestroy.Count > countDestroyInitial)
                    pointsToDestroy.RemoveRange (countDestroyInitial, pointsToDestroy.Count - countDestroyInitial);
            }
            else if (debugReachability)
                Debug.Log ($"Point {pointOrigin.spotIndex} is not connected to any indestructible points - no matches after {pointStackIterations} iterations");

            return pointsAccumulated;
        }

        #endif



        /*
        public static bool IsStructureRescanOnPointChange (List<AreaVolumePoint> points, AreaVolumePoint pointOrigin)
        {
            if (points == null || points.Count == 0)
                return false;

            if (pointOrigin == null)
                return false;

            int pointLoopLimitConservative = points.Count + 1000;
            int iterationIndex = 0;
            int uniqueVisitID = 0;

            structureScanIndex += 1;

            if (debugStructureAnalysis)
            {
                Debug.Log ($"Checking if structure rescan is needed on change at point {pointOrigin.spotIndex}");
                Debug.DrawLine (pointOrigin.pointPositionLocal, pointOrigin.pointPositionLocal + new Vector3 (0f, 12f, 0f), Color.yellow, 5f);
            }

            structurePointsQueue.Clear ();
            var q = structurePointsQueue;
            q.Clear ();
            q.Enqueue (pointOrigin);

            while (q.Count > 0)
            {
                var point = q.Dequeue ();

                if (q.Count > pointLoopLimitConservative)
                {
                    Debug.LogWarning ($"Failed to exit structure analysis algorithm | Remaining queue: {q.Count} | Iterations: {iterationIndex}");
                    break;
                }

                // Stop going if this point already belongs to an older sim chunk
                if (point.simulatedChunkPresent)
                    continue;

                // Stop going if you hit a point that's already been reached
                if (point.structuralValue != structureValueDefault)
                    continue;

                // If this is an indestructible point, break - we found a link to an indestructible point, we're done
                if (point.indestructibleIndirect || !point.destructible)
                {
                    Debug.Log ($"Structure rescan is not needed on change at point {pointOrigin.spotIndex}: it is connected to indestructible point {point.spotIndex} within {iterationIndex} iterations");
                    return false;
                }

                // This point is part of the connected sequence, give it the generation index
                point.structuralValue = iterationIndex;
                point.structuralParent = point.structuralParentCandidate;

                if (point.structuralParent != null)
                    point.structuralGroup = point.structuralParent.structuralGroup;

                // spotPoints: 1 (X+) 2 (Z+) 4 (Y+)
                // spotsAroundThisPoint: 3 (Y-), 5 (Z-), 6 (X-)

                var pointYPos = point.pointsInSpot[4];
                if (TryLinkingStructureToNeighbor (pointYPos, point))
                    q.Enqueue (pointYPos);

                var pointXPos = point.pointsInSpot[1];
                if (TryLinkingStructureToNeighbor (pointXPos, point))
                    q.Enqueue (pointXPos);

                var pointXNeg = point.pointsWithSurroundingSpots[6];
                if (TryLinkingStructureToNeighbor (pointXNeg, point))
                    q.Enqueue (pointXNeg);

                var pointZPos = point.pointsInSpot[2];
                if (TryLinkingStructureToNeighbor (pointZPos, point))
                    q.Enqueue (pointZPos);

                var pointZNeg = point.pointsWithSurroundingSpots[5];
                if (TryLinkingStructureToNeighbor (pointZNeg, point))
                    q.Enqueue (pointZNeg);

                var pointYNeg = point.pointsWithSurroundingSpots[3];
                if (TryLinkingStructureToNeighbor (pointYNeg, point))
                    q.Enqueue (pointYNeg);

                iterationIndex++;
            }

            // If we're here, we failed to find an indestructible point reachable from origin: full rescan might be needed
            Debug.Log ($"Structure rescan might be needed on change at point {pointOrigin.spotIndex}: didn't find a connected indestructible point after {iterationIndex} iterations");
            return true;
        }
        */

        /*
        public static void UpdateStructureAnalysis (List<AreaVolumePoint> points)
        {
            if (points == null || points.Count == 0)
                return;

            debugReachability = DataShortcuts.sim.debugCombatStructureAnalysis;
            structureAnalysisCounter += 1;

            if (debugReachability)
                Debug.Log ($"Updating structural analysis | Run {structureAnalysisCounter}");

            int pointsCount = points.Count;
            int pointLoopLimitConservative = points.Count + 1000;

            // Go over every single point in the level and clear temp values used for structural analysis - unless those points are already detached
            for (int i = 0; i < pointsCount; ++i)
            {
                var point = points[i];
                if (point.simulatedChunkPresent)
                    continue;

                point.structuralGroup = 0;
                point.structuralValue = structureValueDefault;
                point.structuralParentCandidate = null;
                point.structuralParent = null;
            }

            var pointOrigin = SelectOriginPointForStructure (points);
            if (pointOrigin == null)
                return;

            // Try to iterate from origin point and see how many points get marked up
            var pointsGatheredFirst = GatherConnectedPointsFromOrigin
            (
                pointOrigin,
                pointLoopLimitConservative,
                out int iterationsPassed,
                out int pointsIndestructible
            );

            structurePointsIsolated.Clear ();
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                if (point == null)
                    continue;

                if (!point.destructible || point.indestructibleIndirect)
                    continue;

                if (point.simulatedChunkPresent || point.structuralValue != structureValueDefault || point.pointState != AreaVolumePointState.Full)
                    continue;

                structurePointsIsolated.Add (point);
            }

            Debug.Log ($"Selected point {pointOrigin.spotIndex}/{pointsCount} as indestructible origin | Origin connects to {pointsGatheredFirst.Count} points | Iterations: {iterationsPassed} | Indestructible points reached: {pointsIndestructible} | {structurePointsIsolated.Count} destructible points not checked yet.");
        }

        private static AreaVolumePoint SelectOriginPointForStructure (List<AreaVolumePoint> points)
        {
            // But for exotic cases like levels without edges, we can start elsewhere
            int pointsCount = points.Count;
            for (int i = pointsCount - 1; i >= 0; --i)
            {
                var point = points[i];

                // Points that are already detached or points that are empty are not suitable origins
                if (point.simulatedChunkPresent || point.pointState != AreaVolumePointState.Full)
                    continue;

                // Points that are destructible are not suitable
                if (!point.indestructibleIndirect && point.destructible)
                    continue;

                return point;
            }

            return null;
        }

        private static List<AreaVolumePoint> pointsGathered = new List<AreaVolumePoint> ();

        private static List<AreaVolumePoint> GatherConnectedPointsFromOrigin
        (
            AreaVolumePoint pointOrigin,
            int limit,
            out int iterationIndex,
            out int pointsIndestructible
        )
        {
            iterationIndex = 0;
            pointsIndestructible = 0;
            pointsGathered.Clear ();

            if (pointOrigin == null)
                return null;

            if (debugReachability)
            {
                DebugExtensions.DrawCube (pointOrigin.pointPositionLocal, Vector3.one, Color.white, 5f);
                Debug.DrawLine (pointOrigin.pointPositionLocal, pointOrigin.pointPositionLocal + new Vector3 (0f, 12f, 0f), Color.white, 5f);
            }

            var q = structurePointsQueue;
            q.Clear ();
            q.Enqueue (pointOrigin);

            while (q.Count > 0)
            {
                var point = q.Dequeue ();

                if (q.Count > limit)
                {
                    Debug.LogWarning ($"Failed to exit structure analysis algorithm | Remaining queue: {q.Count} | Iterations: {iterationIndex}");
                    break;
                }

                // Stop going if this point already belongs to an older sim chunk
                if (point.simulatedChunkPresent)
                    continue;

                // Stop going if you hit a point that's already been reached
                if (point.structuralValue != structureValueDefault)
                    continue;

                // If this is an indestructible point, do not cancel iteration, but register that fact
                if (point.indestructibleIndirect || !point.destructible)
                    pointsIndestructible += 1;

                // This point is part of the connected sequence, give it the generation index
                point.structuralValue = iterationIndex;
                point.structuralParent = point.structuralParentCandidate;

                if (point.structuralParent != null)
                    point.structuralGroup = point.structuralParent.structuralGroup;

                pointsGathered.Add (point);

                // spotPoints: 1 (X+) 2 (Z+) 4 (Y+)
                // spotsAroundThisPoint: 3 (Y-), 5 (Z-), 6 (X-)

                var pointYPos = point.pointsInSpot[4];
                if (TryLinkingStructureToNeighbor (pointYPos, point))
                    q.Enqueue (pointYPos);

                var pointXPos = point.pointsInSpot[1];
                if (TryLinkingStructureToNeighbor (pointXPos, point))
                    q.Enqueue (pointXPos);

                var pointXNeg = point.pointsWithSurroundingSpots[6];
                if (TryLinkingStructureToNeighbor (pointXNeg, point))
                    q.Enqueue (pointXNeg);

                var pointZPos = point.pointsInSpot[2];
                if (TryLinkingStructureToNeighbor (pointZPos, point))
                    q.Enqueue (pointZPos);

                var pointZNeg = point.pointsWithSurroundingSpots[5];
                if (TryLinkingStructureToNeighbor (pointZNeg, point))
                    q.Enqueue (pointZNeg);

                var pointYNeg = point.pointsWithSurroundingSpots[3];
                if (TryLinkingStructureToNeighbor (pointYNeg, point))
                    q.Enqueue (pointYNeg);

                iterationIndex++;
            }

            return pointsGathered;
        }

        private static bool TryLinkingStructureToNeighbor (AreaVolumePoint pointNeighbor, AreaVolumePoint pointParentCandidate)
        {
            if (pointNeighbor == null || pointNeighbor.simulatedChunkPresent)
                return false;

            bool linkable = pointNeighbor.structuralValue == structureValueDefault && pointNeighbor.pointState == AreaVolumePointState.Full;
            pointNeighbor.structuralParentCandidate = pointParentCandidate;

            if (debugReachability)
            {
                var hue = (pointParentCandidate.structuralValue * 0.0002f) % 1f;
                var lum = Mathf.Clamp (1f - pointParentCandidate.structuralValue * 0.0001f, 0.2f, 0.95f);
                var sat = 1f - Mathf.Max (0f, 0.8f - pointParentCandidate.structuralValue * 0.0002f);
                Debug.DrawLine (pointNeighbor.pointPositionLocal, pointParentCandidate.pointPositionLocal, Color.HSVToRGB (hue, sat, lum), 5f);
            }

            return linkable;
        }

        */
    }

    public class PointData
    {
        public static int boundsX;
        public static int boundsZ;
        public static int layerXZ;
        public static readonly (int x, int y, int z)[] offsets = new []
        {
            (1, 0, 0),
            (0, 0, -1),
            (-1, 0, 0),
            (0, 0, 1),
            (0, 1, 0),
            (0, -1, 0),
            (1, 0, 1),
            (1, 0, -1),
            (-1, 0, 1),
            (-1, 0, -1),
        };

        public static void SetBounds (Vector3Int bounds)
        {
            boundsX = bounds.x;
            boundsZ = bounds.z;
            layerXZ = bounds.x * bounds.z;
        }

        public static AreaVolumePoint GetNeighbourPoint (int indexPoint, int direction, List<AreaVolumePoint> points)
        {
            var index = indexPoint;
            var offset = offsets[direction];

            // Row and column checks to prevent unintended wrap around.
            var x = index % boundsX + offset.x;
            var z = index / boundsX % boundsZ + offset.z;
            if (x < 0 | z < 0)
                return null;
            if (x >= boundsX | z >= boundsZ)
                return null;

            index += offset.y * layerXZ;
            index += offset.z * boundsX;
            index += offset.x;
            if (!index.IsValidIndex (points))
                return null;

            var point = points[index];
            if (!point.spotPresent)
                return null;
            return point;
        }
    }
}
