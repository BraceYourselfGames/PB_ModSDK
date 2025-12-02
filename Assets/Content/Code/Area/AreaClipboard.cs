using System.Collections.Generic;
using UnityEngine;

namespace Area
{
    public class AreaClipboard
    {
        public string name;
        public readonly List<AreaVolumePoint> clipboardPointsSaved = new List<AreaVolumePoint> ();
        public readonly List<AreaPlacementProp> clipboardPropsSaved = new List<AreaPlacementProp> ();
        public Vector3Int clipboardBoundsSaved;
        public Vector3Int clipboardDirection;

        public bool IsValid => clipboardPointsSaved.Count != 0;

        public void Reset ()
        {
            clipboardPointsSaved.Clear ();
            clipboardPropsSaved.Clear ();
            clipboardBoundsSaved = Vector3Int.size0x0x0;
            clipboardDirection = new Vector3Int (1, 0, 0);
        }

        public void CopyFromArea (AreaManager am, Vector3Int origin, Vector3Int size, bool log = false)
        {
            var volumeLength = size.x * size.y * size.z;
            if (log)
            {
                Debug.LogFormat ("Clipboard copy: {0}/{1}/{2}", origin, size, volumeLength);
            }
            Reset ();
            clipboardBoundsSaved = size;
            for (var i = 0; i < volumeLength; i += 1)
            {
                clipboardPointsSaved.Add (new AreaVolumePoint ());

                var clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);
                var sourcePointPosition = clipboardPointPosition + origin;
                var sourcePointIndex = AreaUtility.GetIndexFromVolumePosition (sourcePointPosition, am.boundsFull);
                var sourcePoint = am.points[sourcePointIndex];
                var clipboardPoint = new AreaVolumePoint ();

                clipboardPointsSaved[i] = clipboardPoint;
                clipboardPoint.pointState = sourcePoint.pointState;
                clipboardPoint.pointPositionIndex = clipboardPointPosition;
                #if PB_MODSDK
                // Reindex points so that things are consistent for snippets.
                // The area manager does not change spot indexes on paste. It simply
                // applies the properties from the clipboard point to the area point.
                clipboardPoint.spotIndex = i;
                #endif
                clipboardPoint.spotConfiguration = sourcePoint.spotConfiguration;
                clipboardPoint.spotConfigurationWithDamage = sourcePoint.spotConfigurationWithDamage;
                clipboardPoint.blockFlippedHorizontally = sourcePoint.blockFlippedHorizontally;
                clipboardPoint.blockGroup = sourcePoint.blockGroup;
                clipboardPoint.blockRotation = sourcePoint.blockRotation;
                clipboardPoint.blockSubtype = sourcePoint.blockSubtype;
                clipboardPoint.blockTileset = sourcePoint.blockTileset;
                clipboardPoint.customization = sourcePoint.customization;
                clipboardPoint.terrainOffset = sourcePoint.terrainOffset;

                //Check if we're on the far edge of the copy bounds - spot data there is outside the copy box
                var isEdgePoint = clipboardPointPosition.x + 1 >= clipboardBoundsSaved.x ||
                    clipboardPointPosition.y + 1 >= clipboardBoundsSaved.y ||
                    clipboardPointPosition.z + 1 >= clipboardBoundsSaved.z;

                if (isEdgePoint || !am.indexesOccupiedByProps.TryGetValue (sourcePointIndex, out var props))
                {
                    continue;
                }

                //Copy props
                foreach (var prop in props)
                {
                    var clone = prop.SimpleClone ();
                    #if PB_MODSDK
                    // Reindex props to match points so that things are consistent for snippets.
                    // Pivot index gets overwritten by the area manager on paste with an index
                    // calculated from the pasted position.
                    clone.pivotIndex = i;
                    #endif
                    clone.clipboardPosition = clipboardPointPosition;
                    clipboardPropsSaved.Add (clone);
                }
            }
        }

        public void Rotate (bool clockwise)
        {
            var oldBounds = clipboardBoundsSaved;

            //Shift tracks the fact that we will have to move the copy volume origin (since coords can't be negative)
            var xShift = 0;
            var zShift = 0;
            var rotateV = clockwise ? new Vector3Int (0, 0, -1) : new Vector3Int (0, 0, 1);
            var antiV = rotateV * -1;
            clipboardBoundsSaved = RotateXZByVector (clipboardBoundsSaved, rotateV);
            if (clipboardBoundsSaved.x < 0)
            {
                clipboardBoundsSaved.x = Mathf.Abs (clipboardBoundsSaved.x);
                xShift = clipboardBoundsSaved.x - 1;
            }

            if (clipboardBoundsSaved.z < 0)
            {
                clipboardBoundsSaved.z = Mathf.Abs (clipboardBoundsSaved.z);
                zShift = clipboardBoundsSaved.z - 1;
            }

            clipboardDirection = RotateXZByVector (clipboardDirection, rotateV);

            //We need to remap the point order inside the clipboard array to maintain the same XZY axis ordering
            var newList = new List<AreaVolumePoint> (new AreaVolumePoint[clipboardPointsSaved.Count]);
            for (var i = 0; i < newList.Count; i += 1)
            {
                var clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);
                clipboardPointPosition.x -= xShift;
                clipboardPointPosition.z -= zShift;

                var oldCoord = RotateXZByVector (clipboardPointPosition, antiV);
                var oldIndex = AreaUtility.GetIndexFromVolumePosition (oldCoord, oldBounds);

                newList[i] = clipboardPointsSaved[oldIndex];
                newList[i].pointPositionIndex = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);
            }

            //Move the spot data around since after the rotation, it's effectively offset
            for (var i = 0; i < newList.Count; i += 1)
            {
                var clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);

                if (clockwise)
                {
                    clipboardPointPosition.z += 1;
                }
                else
                {
                    clipboardPointPosition.x += 1;
                }

                if (clipboardPointPosition.x >= clipboardBoundsSaved.x || clipboardPointPosition.z >= clipboardBoundsSaved.z)
                {
                    continue;
                }

                var spotShiftIndex = AreaUtility.GetIndexFromVolumePosition (clipboardPointPosition, clipboardBoundsSaved);
                CopySpotData (newList[spotShiftIndex], newList[i]);

                newList[i].blockRotation = RotateByte (newList[i].blockRotation, clockwise);
            }

            //Fix up spot configuration data
            for (var i = 0; i < newList.Count; i += 1)
            {
                var clipboardPointPosition = AreaUtility.GetVolumePositionFromIndex (i, clipboardBoundsSaved);

                byte configByte = 0;
                byte configByteDmg = 0;
                var anyDestroyed = false;

                for (var j = 0; j < 8; j += 1)
                {
                    var jRemapped = AreaUtility.configurationIndexRemapping[j];
                    var xOffset = jRemapped % 2;
                    var zOffset = (jRemapped / 2) % 2;
                    var yOffset = jRemapped / 4;
                    var offsetPos = clipboardPointPosition + new Vector3Int (xOffset, yOffset, zOffset);

                    if (offsetPos.x >= clipboardBoundsSaved.x || offsetPos.y >= clipboardBoundsSaved.y || offsetPos.z >= clipboardBoundsSaved.z)
                    {
                        continue;
                    }

                    var offsetIndex = AreaUtility.GetIndexFromVolumePosition (offsetPos, clipboardBoundsSaved);
                    var state = newList[offsetIndex].pointState;
                    if (state != AreaVolumePointState.Empty)
                    {
                        configByte |= (byte)(1 << (7 - j));
                    }
                    if (state == AreaVolumePointState.Full)
                    {
                        configByteDmg |= (byte)(1 << (7 - j));
                    }
                    if (state == AreaVolumePointState.FullDestroyed)
                    {
                        anyDestroyed = true;
                    }
                }

                newList[i].spotHasDamagedPoints = anyDestroyed;
                newList[i].spotConfiguration = configByte;
                newList[i].spotConfigurationWithDamage = anyDestroyed ? configByteDmg : configByte;
            }

            clipboardPointsSaved.Clear ();
            clipboardPointsSaved.AddRange (newList);

            //Rotate the copied props
            for (var i = 0; i < clipboardPropsSaved.Count; i += 1)
            {
                var prop = clipboardPropsSaved[i];

                prop.clipboardPosition = RotateXZByVector (prop.clipboardPosition, rotateV);
                prop.clipboardPosition.x += xShift;
                prop.clipboardPosition.z += zShift;

                //Since props are on spots, not points, we have to compensate for the spots being shifted
                if (clockwise)
                {
                    prop.clipboardPosition.z -= 1;
                }
                else
                {
                    prop.clipboardPosition.x -= 1;
                }
                prop.rotation = RotateByte (prop.rotation, clockwise);
            }
        }

        static void CopySpotData (AreaVolumePoint from, AreaVolumePoint to)
        {
            //to.spotConfiguration = from.spotConfiguration;
            //to.spotConfigurationWithDamage = from.spotConfigurationWithDamage;
            to.blockFlippedHorizontally = from.blockFlippedHorizontally;
            to.blockGroup = from.blockGroup;
            to.blockRotation = from.blockRotation;
            to.blockSubtype = from.blockSubtype;
            to.blockTileset = from.blockTileset;
            to.customization = from.customization;
        }

        static byte RotateByte (byte value, bool rotateClockwise) => (byte)((value + (rotateClockwise ? 3 : 1)) % 4);

        // Rotates v1 by v2, treating them as 2d vectors and ignoring Y axis
        static Vector3Int RotateXZByVector (Vector3Int v1, Vector3Int v2) =>
            new Vector3Int
            (
                v1.x * v2.x - v1.z * v2.z,
                v1.y,
                v1.x * v2.z + v1.z * v2.x
            );
    }
}
