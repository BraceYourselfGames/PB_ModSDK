using UnityEngine;

namespace Area
{
    public static class ClipboardCopyOperation
    {
        static Vector3Int clipboardOriginStashed;
        static Vector3Int clipboardBoundsRequestedStashed;

        public static void StashClipboardOriginAndBounds (AreaManager am)
        {
            clipboardOriginStashed = am.clipboardOrigin;
            clipboardBoundsRequestedStashed = am.clipboardBoundsRequested;
        }

        public static void RestoreClipboardOriginAndBounds (AreaManager am)
        {
            am.clipboardOrigin = clipboardOriginStashed;
            am.clipboardBoundsRequested = clipboardBoundsRequestedStashed;
        }

        public static void ShrinkwrapSource (AreaManager am)
        {
            var shrinkBounds = GetShrinkwrapBounds (am, am.clipboardOrigin, am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);
            am.clipboardOrigin.y = shrinkBounds.topY;
            am.clipboardBoundsRequested.y = shrinkBounds.bottomY - shrinkBounds.topY + 1;
        }

        public static void ShrinkwrapTarget (AreaManager am)
        {
            var shrinkBounds = GetShrinkwrapBounds (am, am.targetOrigin, am.targetOrigin + am.clipboard.clipboardBoundsSaved + Vector3Int.size1x1x1Neg);
            am.targetOrigin.y = shrinkBounds.bottomY - am.clipboard.clipboardBoundsSaved.y + 1;
        }

		static (int topY, int bottomY) GetShrinkwrapBounds(AreaManager am, Vector3Int cornerA, Vector3Int cornerB)
		{
			cornerA.y = 0;
            cornerB.y = am.boundsFull.y - 1;

			var topY = int.MaxValue;
			var bottomY = int.MaxValue;
            for (var y = cornerA.y; y <= cornerB.y; y += 1)
            {
                var allFull = true;
                var allEmpty = true;
                for (var z = cornerA.z; z <= cornerB.z; z += 1)
                {
                    for (var x = cornerA.x; x <= cornerB.x; x += 1)
                    {
                        var coord = new Vector3Int (x, y, z);
                        var sourcePointIndex = AreaUtility.GetIndexFromVolumePosition (coord, am.boundsFull);
                        var sourcePoint = am.points[sourcePointIndex];
                        if (sourcePoint.pointState != AreaVolumePointState.Empty)
                        {
                            allEmpty = false;
                        }
                        if (sourcePoint.pointState != AreaVolumePointState.Full)
                        {
                            allFull = false;
                        }
                        if (!allFull && !allEmpty)
                        {
                            break;
                        }
                    }

                    if (!allFull && !allEmpty)
                    {
                        break;
                    }
                }

                if (!allEmpty)
                {
                    topY = Mathf.Min (y - 1, topY);
                }
                if (allFull)
                {
                    bottomY = Mathf.Min (y, bottomY);
                }
            }

            if (topY < cornerA.y || topY > cornerB.y)
            {
                topY = cornerA.y;
            }
            if (bottomY < cornerA.y || bottomY > cornerB.y)
            {
                bottomY = cornerB.y;
            }
            return (topY, bottomY);
		}

        public static void CopyVolume (AreaManager am, bool log = false)
        {
            var cornerA = am.clipboardOrigin;
            var cornerB = am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg;
            var indexA = AreaUtility.GetIndexFromVolumePosition (cornerA, am.boundsFull);
            var indexB = AreaUtility.GetIndexFromVolumePosition (cornerB, am.boundsFull);
            if (log)
            {
                Debug.LogFormat ("Copy request | origin: {0}/{1} | size: {2} | diag: {3}/{4}", indexA, cornerA, am.clipboardBoundsRequested, indexB, cornerB);
            }

            if (indexA == -1 || indexB == -1)
            {
                Debug.LogWarningFormat (
                    "CopyVolume | Failed to copy due to specified corners {0} and {1} falling outside of source level bounds {2}",
                    cornerA,
                    cornerB,
                    am.boundsFull
                );
                return;
            }

            // Bounds are limits, like array sizes - so they will be bigger by 1 than just pure position difference
            // For example, a set of points [(0,0);(1,0);(0,1);(1;1)] has bounds of 2x2, not 1x1!
            var size = cornerB - cornerA + Vector3Int.size1x1x1;
            if (size.x < 2 || size.y < 2 || size.z < 2)
            {
                Debug.LogWarningFormat (
                    "CopyVolume | Failed to copy due to specified corners not creating valid clip bounds | B - A: {0} - {1} = {2}",
                    cornerB,
                    cornerA,
                    cornerB - cornerA
                );
                return;
            }

            am.clipboard.CopyFromArea(am, cornerA, size, log);
	    }
    }
}
