using UnityEngine;
using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;

namespace Area
{
    public static class AreaUtility
    {
        public static readonly Vector3 spotOffset = new Vector3 (-1.5f, 1.5f, -1.5f);
        static readonly Vector3Int invalidVolumePosition = Vector3Int.size1x1x1Neg;
        public const int invalidIndex = -1;

        private static readonly Dictionary<int, int[]> reusedExternalIndexes = new Dictionary<int, int[]> ();
        private static int[] reusedIntegerArray8;

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

        public enum Direction
        {
            XPos = 0,
            XNeg = 1,
            YPos = 2,
            YNeg = 3,
            ZPos = 4,
            ZNeg = 5,
        }

        public static void ClearCache ()
        {
            reusedExternalIndexes.Clear ();
        }

        public static int GetNeighbourIndexFromDirection (int externalStartIndex, Direction direction, Vector3Int boundsArea)
        {
            if (externalStartIndex == invalidIndex)
                return invalidIndex;

            reusedPositionStart = GetVolumePositionFromIndex (externalStartIndex, boundsArea);

            if (direction == Direction.XPos)
                reusedPositionFinal = new Vector3Int (reusedPositionStart.x + 1, reusedPositionStart.y, reusedPositionStart.z);
            else if (direction == Direction.XNeg)
                reusedPositionFinal = new Vector3Int (reusedPositionStart.x - 1, reusedPositionStart.y, reusedPositionStart.z);
            else if (direction == Direction.YPos)
                reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y + 1, reusedPositionStart.z);
            else if (direction == Direction.YNeg)
                reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y - 1, reusedPositionStart.z);
            else if (direction == Direction.ZPos)
                reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y, reusedPositionStart.z + 1);
            else if (direction == Direction.ZNeg)
                reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y, reusedPositionStart.z - 1);
            else
                reusedPositionFinal = invalidVolumePosition;

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
                reusedExternalIndex = GetIndexFromVolumePosition (reusedPositionFinal, boundsArea);

            return reusedExternalIndex;
        }

        public static int[] GetNeighbourIndexesFromDirections (int externalStartIndex, Direction[] directions, Vector3Int boundsArea)
        {
            int size = directions.Length;
            if (!reusedExternalIndexes.ContainsKey (size))
                reusedExternalIndexes.Add (size, new int[size]);

            reusedPositionStart = GetVolumePositionFromIndex (externalStartIndex, boundsArea);
            int[] externalIndexes = reusedExternalIndexes[size];

            for (int i = 0; i < directions.Length; ++i)
            {
                Direction direction = directions[i];

                if (direction == Direction.XPos)
                    reusedPositionFinal = new Vector3Int (reusedPositionStart.x + 1, reusedPositionStart.y, reusedPositionStart.z);
                else if (direction == Direction.XNeg)
                    reusedPositionFinal = new Vector3Int (reusedPositionStart.x - 1, reusedPositionStart.y, reusedPositionStart.z);
                else if (direction == Direction.YPos)
                    reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y + 1, reusedPositionStart.z);
                else if (direction == Direction.YNeg)
                    reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y - 1, reusedPositionStart.z);
                else if (direction == Direction.ZPos)
                    reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y, reusedPositionStart.z + 1);
                else if (direction == Direction.ZNeg)
                    reusedPositionFinal = new Vector3Int (reusedPositionStart.x, reusedPositionStart.y, reusedPositionStart.z - 1);
                else
                    reusedPositionFinal = invalidVolumePosition;

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
                    reusedExternalIndex = GetIndexFromVolumePosition (reusedPositionFinal, boundsArea);

                externalIndexes[i] = reusedExternalIndex;
            }

            return externalIndexes;
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
                            reusedExternalIndex = GetIndexFromVolumePosition (reusedPositionFinal, boundsArea);

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
            int size = boundsNeighbourhood.x * boundsNeighbourhood.y * boundsNeighbourhood.z;
            if (!reusedExternalIndexes.ContainsKey (size))
            {
                // Debug.Log ("Attempting to add new size to reused external indexes: " + size + " | Total size count: " + reusedExternalIndexes.Count);
                reusedExternalIndexes.Add (size, new int[size]);
            }

            reusedPositionStart = GetVolumePositionFromIndex (externalStartIndex, boundsArea);
            int[] externalIndexes = reusedExternalIndexes[size];

            for (int y = 0; y < boundsNeighbourhood.y; ++y)
            {
                for (int z = 0; z < boundsNeighbourhood.z; ++z)
                {
                    for (int x = 0; x < boundsNeighbourhood.x; ++x)
                    {
                        Vector3Int externalFinalPosition = new Vector3Int
                        (
                            reusedPositionStart.x + x + pivotPosition.x,
                            reusedPositionStart.y + y + pivotPosition.y,
                            reusedPositionStart.z + z + pivotPosition.z
                        );

                        int internalIndex = x + boundsNeighbourhood.x * z + boundsNeighbourhood.x * boundsNeighbourhood.z * y;
                        int externalIndex = -1;

                        if
                        (
                            externalFinalPosition.x >= 0 &&
                            externalFinalPosition.y >= 0 &&
                            externalFinalPosition.z >= 0 &&
                            externalFinalPosition.x < boundsArea.x &&
                            externalFinalPosition.y < boundsArea.y &&
                            externalFinalPosition.z < boundsArea.z
                        )
                            externalIndex = GetIndexFromVolumePosition (externalFinalPosition, boundsArea);

                        externalIndexes[internalIndex] = externalIndex;
                    }
                }
            }

            return externalIndexes;
        }



        public static int[] GetNeighbourIndexesIn2x2x2 (int index, Vector3Int pivotPosition, Vector3Int boundsFull)
        {
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

        public static int GetIndexFromInternalPosition (int x, int z, int boundsX, int boundsZ)
        {
            var boundsFit =
                x >= 0 && x < boundsX &&
                z >= 0 && z < boundsZ;
            return boundsFit ? x + z * boundsX : invalidIndex;
        }
        
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

        public static Vector3Int GetInternalSpotPositionFromWorld (Vector3 positionInWorld, Vector3 positionOfVolume)
        {
	        var positionCorrected = Vector3.Scale ((positionInWorld - positionOfVolume), new Vector3 (1f, -1f, 1f)) / TilesetUtility.blockAssetSize;
	        var positionFinal = new Vector3Int (Mathf.FloorToInt (positionCorrected.x), Mathf.FloorToInt (positionCorrected.y), Mathf.FloorToInt (positionCorrected.z));

	        return positionFinal;
        }

        public static Vector3Int GetInternalPointPositionFromWorld (Vector3 positionInWorld, Vector3 positionOfVolume)
        {
	        var positionCorrected = Vector3.Scale ((positionInWorld - positionOfVolume), new Vector3 (1f, -1f, 1f)) / TilesetUtility.blockAssetSize;
	        var positionFinal = new Vector3Int (Mathf.RoundToInt (positionCorrected.x), Mathf.RoundToInt (positionCorrected.y), Mathf.RoundToInt (positionCorrected.z));

	        return positionFinal;
        }

        public static bool GetIsInBounds (Vector3Int pos, Vector3Int bounds) =>
	        pos.x >= 0 &&
	               pos.x < bounds.x &&
	               pos.y >= 0 &&
	               pos.y < bounds.y &&
	               pos.z >= 0 &&
	               pos.z < bounds.z;

        public static int GetIndexFromWorldPosition (Vector3 positionInWorld, Vector3 positionOfVolume, Vector3Int bounds)
        {
            // Debug.Log ("AU | GetIndexFromWorldPosition | PW: " + positionInWorld + " | PC: " + reusedPositionCorrected + " | PF: " + reusedPositionFinal);
            return GetIndexFromVolumePosition (GetInternalPointPositionFromWorld(positionInWorld, positionOfVolume), bounds);
        }

        public static Vector3Int GetVolumePositionFromIndex (int index, Vector3Int bounds, bool log = true)
        {
            if (bounds.x == 0 || bounds.z == 0)
            {
                if (log)
                {
                    Debug.LogError ("Division by zero in bounds " + bounds);
                }
                return invalidVolumePosition;
            }
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
    }
}
