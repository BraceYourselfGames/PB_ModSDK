using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Area
{
    public static class TilesetUtility
    {
        public const byte configurationEmpty = byte.MinValue;
        public const byte configurationFull = byte.MaxValue;
        public const byte configurationFloor = 0x0F;
        public const byte configurationTopMask = 0xF0;
        public const byte configurationBitTopSelf = 0x80;
        public const byte configurationBitBottomSelf = 0x08;
        public const byte configurationBitmaskSelf = configurationBitTopSelf | configurationBitBottomSelf;

        public const int blockAssetSize = 3;
        public const int blockAssetRotationShift = 1;
        public const float blockAssetRotationBasis = -90f;

        public const bool blockAssetFlipDifferentAxis = true;
        public static Vector4 blockAssetScaleFlipped = new Vector4
        (
            blockAssetFlipDifferentAxis ? 1f : -1f, 1f,
            blockAssetFlipDifferentAxis ? -1f : 1f, 1f
        );

        public static bool IsConfigurationIdentical (bool[] configA, bool[] configB)
        {
            bool match = true;
            if (configA.Length != configB.Length)
                match = false;
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (configA[i] != configB[i])
                    {
                        match = false;
                        break;
                    }
                }
            }
            return match;
        }

        public static bool IsDamageTemplateIdentical (AreaVolumePointState[] templateA, AreaVolumePointState[] templateB)
        {
            if (templateA == templateB)
            {
                Debug.LogWarning ("AU | IsDamageTemplateIdentical | Comparing exactly the same array object!");
                return true;
            }

            bool match = true;
            if (templateA.Length != templateB.Length)
                match = false;
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (templateA[i] != templateB[i])
                        match = false;
                }
            }
            return match;
        }

        public static bool[] GetConfigurationFromInt (int configurationInt)
        {
            int[] p = new int[8];
            bool[] configuration = new bool[8];
            for (int i = 0; i < 8; ++i)
            {
                p[7 - i] = configurationInt % 2;
                configurationInt = configurationInt / 2;
            }
            for (int i = 0; i < 8; ++i)
            {
                configuration[i] = p[i] == 1 ? true : false;
            }
            return configuration;
        }

        public static int GetIntFromConfiguration (bool[] configuration)
        {
            int configurationInt = 0;
            configurationInt += (configuration[7] ? 1 : 0) * 1;
            configurationInt += (configuration[6] ? 1 : 0) * 10;
            configurationInt += (configuration[5] ? 1 : 0) * 100;
            configurationInt += (configuration[4] ? 1 : 0) * 1000;
            configurationInt += (configuration[3] ? 1 : 0) * 10000;
            configurationInt += (configuration[2] ? 1 : 0) * 100000;
            configurationInt += (configuration[1] ? 1 : 0) * 1000000;
            configurationInt += (configuration[0] ? 1 : 0) * 10000000;
            return configurationInt;
        }

        public static byte GetByteFromConfiguration (bool[] configuration)
        {
            byte configurationByte = 0;
            foreach (bool point in configuration)
            {
                configurationByte <<= 1;
                if (point) configurationByte |= 1;
            }
            return configurationByte;
        }

        public static bool[] GetConfigurationFromByte (byte configurationByte)
        {
            bool[] configuration = new bool[8];
            BitArray configurationBitArray = new BitArray (new byte[] { configurationByte });
            for (int i = 0; i < 8; ++i)
            {
                configuration[i] = configurationBitArray[7 - i];
            }
            return configuration;
        }

        public static void TestByteConversion (bool[] configuration)
        {
            int configurationInt = GetIntFromConfiguration (configuration);
            byte configurationByte = GetByteFromConfiguration (configuration);
            bool[] configurationRestored = GetConfigurationFromByte (configurationByte);
            Debug.Log ("TU | TestByteConversion | Source: " + GetStringFromConfiguration (configuration) + " | Int: " + configurationInt + " | Byte: " + configurationByte.ToString () + " | Restored: " + GetStringFromConfiguration (configurationRestored));
        }

        // Transformations

        // We need to check the rotated and flipped duplicates
        // Bools map to two "floors" of a 2x2x2 3d group of objects this way
        // _____
        // \0   3\
        //  \1___2\
        // _____
        // \4   7\
        //  \5___6\
        //
        // Flipping is just swapping of 0-1, 2-3, 4-5 and 6-7
        // For rotation, since bools make up two circularly mapped "floors", a simple CCW rotation by 90 degrees looks like this:
        // _____        _____
        // \0   3\      \3   2\
        //  \1___2\  ->  \0___1\
        // _____        _____
        // \4   7\      \7   6\
        //  \5___6\  ->  \4___5\

        public static byte first = 1 << 0;
        public static byte zero = 0;

        public static List<byte> GetConfigurationTransformations (byte configuration)
        {
            List<byte> transformations = new List<byte> ();
            for (int i = 0; i < 8; ++i)
            {
                int rotation = i % 4;
                bool flip = i > 3;
                transformations.Add (GetConfigurationTransformed (configuration, rotation, flip));
            }
            return transformations;
        }

        public static byte GetConfigurationTransformed (byte config, int rotation, bool flip)
        {
            var offset = (4 - rotation) % 4;
            int transformed;
            if (!flip)
            {
                transformed = (config & 0x01) << offset
                    | (config & 0x02) >> 1 << ((offset + 1) % 4)
                    | (config & 0x04) >> 2 << ((offset + 2) % 4)
                    | (config & 0x08) >> 3 << ((offset + 3) % 4)
                    | (config & 0x10) >> 4 << (offset + 4)
                    | (config & 0x20) >> 5 << ((offset + 1) % 4 + 4)
                    | (config & 0x40) >> 6 << ((offset + 2) % 4 + 4)
                    | (config & 0x80) >> 7 << ((offset + 3) % 4 + 4);
            }
            else
            {
                transformed = (config & 0x01) << ((offset + 1) % 4)
                    | (config & 0x02) >> 1 << offset
                    | (config & 0x04) >> 2 << ((offset + 3) % 4)
                    | (config & 0x08) >> 3 << ((offset + 2) % 4)
                    | (config & 0x10) >> 4 << ((offset + 5) % 4 + 4)
                    | (config & 0x20) >> 5 << (offset + 4)
                    | (config & 0x40) >> 6 << ((offset + 7) % 4 + 4)
                    | (config & 0x80) >> 7 << ((offset + 6) % 4 + 4);
            }
            return (byte)(transformed & 0xFF);
        }

        private static AreaVolumePointState[] reusedAVPSArray = new AreaVolumePointState[8];

        public static AreaVolumePointState[] GetDamageTemplateTransformed (AreaVolumePointState[] template, int rotation, bool flipHorizontally, bool flipVertically)
        {
            if (reusedAVPSArray.Length != 8)
                reusedAVPSArray = new AreaVolumePointState[8];

            int[] indexes;

            if (!flipHorizontally && !flipVertically) indexes = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            else if (flipHorizontally && !flipVertically) indexes = new int[] { 1, 0, 3, 2, 5, 4, 7, 6 };
            else if (!flipHorizontally && flipVertically) indexes = new int[] { 4, 5, 6, 7, 0, 1, 2, 3 };
            else indexes = new int[] { 5, 4, 7, 6, 1, 0, 3, 2 };

            reusedAVPSArray[rotation] = template[indexes[0]];                 // 0, 1, 2, 3
            reusedAVPSArray[(1 + rotation) % 4] = template[indexes[1]];       // 1, 2, 3, 0
            reusedAVPSArray[(2 + rotation) % 4] = template[indexes[2]];       // 2, 3, 0, 1
            reusedAVPSArray[(3 + rotation) % 4] = template[indexes[3]];       // 3, 0, 1, 2

            reusedAVPSArray[4 + rotation] = template[indexes[4]];             // 4, 5, 6, 7
            reusedAVPSArray[4 + ((1 + rotation) % 4)] = template[indexes[5]]; // 5, 6, 7, 4
            reusedAVPSArray[4 + ((2 + rotation) % 4)] = template[indexes[6]]; // 6, 7, 4, 5
            reusedAVPSArray[4 + ((3 + rotation) % 4)] = template[indexes[7]]; // 7, 4, 5, 6

            return reusedAVPSArray;
        }

        public static byte GetConfigurationFromString (string configurationString)
        {
            if (configurationString.Length != 8)
            {
                Debug.Log ("TilesetUtility | GetConfigurationFromString | Incorrect string length for conversion: " + configurationString);
                return 0;
            }

            var configuration = 0;
            configurationString.CopyTo (0, configurationStringBuffer, 0, 8);
            for (var i = 0; i < 8; ++i)
            {
                if (configurationStringBuffer[i] == '1')
                {
                    configuration |= 1 << (7 - i);
                }

                //It's already zero, don't need to write it
                //else
                //    configuration[i] = false;
            }

            return (byte)(configuration & 0xFF);
        }

        public static string GetStringFromConfiguration (bool[] configuration)
        {
            StringBuilder sb = new StringBuilder ();
            string textTrue = "1";
            string textFalse = "0";
            for (int i = 0; i < configuration.Length; ++i)
                sb.Append (configuration[i] ? textTrue : textFalse);

            return sb.ToString ();
        }

        static readonly char[] configurationStringBuffer = new char[8];
        public static string GetStringFromConfiguration (byte configuration)
        {
            for (var i = 0; i < 8; i += 1)
            {
                if ((configuration & 1 << (7 - i)) != 0)
                {
                    configurationStringBuffer[i] = '1';
                }
                else
                {
                    configurationStringBuffer[i] = '0';
                }
            }
            return new string(configurationStringBuffer);
        }

        public static bool IsConfigurationRotationPossible (bool[] configuration)
        {
            return
            (
                (configuration[0] == configuration[1]) &&
                (configuration[1] == configuration[2]) &&
                (configuration[2] == configuration[3]) &&
                (configuration[4] == configuration[5]) &&
                (configuration[5] == configuration[6]) &&
                (configuration[6] == configuration[7])
            );
        }

        public static bool IsConfigurationRotationPossible (byte configuration) =>
            ((configuration & 0x0F) == 0 | (configuration & 0x0F) == 0x0F) &
            ((configuration & 0xF0) == 0 | (configuration & 0xF0) == 0xF0);


        public static bool IsConfigurationTransformableTo (byte configurationA, byte configurationB)
        {
            for (int r = 0; r < 8; ++r)
            {
                int requiredRotation = r % 4;
                bool requiredFlipping = r > 3;
                byte configurationTransformed = TilesetUtility.GetConfigurationTransformed (configurationA, requiredRotation, requiredFlipping);
                if (configurationA == configurationB)
                {
                    return true;
                }
            }

            return false;
        }

        public static int directionXPos = 0;
        public static int directionZPos = 1;
        public static int directionXNeg = 2;
        public static int directionZNeg = 3;

        public static int directionYPos = 4;
        public static int directionYNeg = 5;

        private static int GetIntFromDirection (PointNeighbourDirection direction)
        {
            int result = 0;
            switch (direction)
            {
                case PointNeighbourDirection.XPos:
                    result = directionXPos;
                    break;
                case PointNeighbourDirection.XNeg:
                    result = directionXNeg;
                    break;
                case PointNeighbourDirection.YPos:
                    result = directionYPos;
                    break;
                case PointNeighbourDirection.YNeg:
                    result = directionYNeg;
                    break;
                case PointNeighbourDirection.ZPos:
                    result = directionZPos;
                    break;
                case PointNeighbourDirection.ZNeg:
                    result = directionZNeg;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static bool IsConfigurationPairSeparated (byte a, byte b, PointNeighbourDirection direction)
        {
            bool[] aDecomposed = TilesetUtility.GetConfigurationFromByte (a);
            bool[] bDecomposed = TilesetUtility.GetConfigurationFromByte (b);

            // bools map to two "floors" of a 2x2x2 3d group of objects this way
            // make sure you compare two configurations using the right direction, otherwise you might get a false negative/positive
            // _____
            // \0   3\
            //  \1___2\

            // direction 0
            // _____
            // \0   3\
            //  \A___B\
            //    _____
            //    \C   D\
            //     \1___2\

            // direction 1
            // _____    _____
            // \0   B\  \D   3\
            //  \1___A\  \C___2\

            // direction 2
            // _____
            // \0   3\
            //  \D___C\
            //    _____
            //    \B   A\
            //     \1___2\

            // direction 3
            // _____    _____
            // \0   C\  \A   3\
            //  \1___D\  \B___2\

            int directionAsInt = GetIntFromDirection (direction);
            if (directionAsInt > 3)
            {
                // Debug.Log ("Direction > 3 from " + direction);
                return false;
            }

            if (!IsConfigurationAtVolumeEdge (aDecomposed) || !IsConfigurationAtVolumeEdge (bDecomposed))
            {
                // Debug.Log ("One of configurations isn't at volume edge");
                return false;
            }

            bool columnA1 = IsColumnEmpty (aDecomposed, (1 + directionAsInt) % 4);
            bool columnA2 = IsColumnEmpty (aDecomposed, (2 + directionAsInt) % 4);
            bool columnB1 = IsColumnEmpty (bDecomposed, (0 + directionAsInt) % 4);
            bool columnB2 = IsColumnEmpty (bDecomposed, (3 + directionAsInt) % 4);

            bool result = columnA1 && columnA2 && columnB1 && columnB2;
            // Debug.Log ("Result: " + result + " | Columns | A1: " + columnA1 + " | A2: " + columnA2 + " | B1: " + columnB1 + " | B2: " + columnB2);

            return result;
        }

        private static bool IsColumnEmpty (bool[] configuration, int i)
        {
            return (configuration[i] == false) && (configuration[i + 4] == false);
        }

        public static bool IsConfigurationAtVolumeEdge (bool[] configuration)
        {
            return
                IsColumnEmpty (configuration, 0) ||
                IsColumnEmpty (configuration, 1) ||
                IsColumnEmpty (configuration, 2) ||
                IsColumnEmpty (configuration, 3);
        }

        public static bool IsConfigurationWalkable (byte configuration)
        {
            return configuration == (byte)15;
        }



        public static bool IsConfigurationWalkable (bool[] configuration)
        {
            return ((configuration[0] == configuration[1] == configuration[2] == configuration[3] == false) && (configuration[4] == configuration[5] == configuration[6] == configuration[7] == true));
        }

        public static bool IsConfigurationFilledOnTop (bool[] configuration)
        {
            return (configuration[0] == true || configuration[1] == true || configuration[2] == true || configuration[3] == true);
        }

        public static int GetConfigurationFlippingAxis (byte configuration)
        {
            if (IsConfigurationRotationPossible (configuration))
            {
                return 2;
            }

            var transformations = GetConfigurationTransformations (configuration);
            if (transformations[1] == transformations[7])
            {
                return 0;
            }
            if (transformations[0] == transformations[4])
            {
                return 2;
            }
            if (transformations[0] == transformations[5])
            {
                return 3;
            }
            if (transformations[0] == transformations[7])
            {
                return 4;
            }
            return -1;
        }

        // Rewrite later to do just one loop with source.Length steps, rotation loop with 90 degree step taken at a time should not be used!

        // Also do pivot calculation
        // Pass the coordinates of the pivot into the transformation
        // Find the index of the original pivot as you encounter the coordinates on transformation
        // Overwrite the index as you transform that point and save out the current coordinates every time
        // Extract the saved coords

        public static bool[] GetBoolArrayTransformed (bool[] source, int rotation, Vector3Int bounds, out Vector3Int boundsRotated, Vector3Int pivot, out Vector3Int pivotRotated)
        {
            int length = source.Length;
            bool[] result = new bool[length];
            System.Array.Copy (source, result, length);

            int pivotRotatedIndexTemp = AreaUtility.GetIndexFromVolumePosition (pivot, bounds, skipBoundsCheck: true);
            Vector3Int boundsRotatedTemp = bounds;
            Debug.Log ("TU | GetBoolArrayTransformed | Starting | Requested rotation: " + rotation + " | Pivot " + pivot.ToString () + " is located at index " + pivotRotatedIndexTemp);

            for (int r = 0; r < rotation; ++r)
            {
                bool[] resultClone = new bool[length];
                System.Array.Copy (result, resultClone, length);

                boundsRotatedTemp = new Vector3Int (boundsRotatedTemp.z, boundsRotatedTemp.y, boundsRotatedTemp.x);
                Debug.Log ("TU | GetBoolArrayTransformed | Bounds (rotated): " + boundsRotatedTemp + " | Bounds (original): " + bounds);

                bool pivotCopiedOnThisRotation = false;

                for (int i = 0; i < result.Length; ++i)
                {
                    int x = i % boundsRotatedTemp.x;
                    int y = i / (boundsRotatedTemp.z * boundsRotatedTemp.x);
                    int z = (i / boundsRotatedTemp.x) % boundsRotatedTemp.z;

                    int indexToCopyFrom = z + (boundsRotatedTemp.x - x - 1) * boundsRotatedTemp.z + y * boundsRotatedTemp.x * boundsRotatedTemp.z;
                    result[i] = resultClone[indexToCopyFrom];

                    if (indexToCopyFrom == pivotRotatedIndexTemp && !pivotCopiedOnThisRotation)
                    {
                        Debug.Log ("TU | GetBoolArrayTransformed | Rotation: " + r + " | Moved the pivot index from " + pivotRotatedIndexTemp + " to " + i);
                        pivotRotatedIndexTemp = i;
                        pivotCopiedOnThisRotation = true;
                    }
                }
            }

            pivotRotated = AreaUtility.GetVolumePositionFromIndex (pivotRotatedIndexTemp, boundsRotatedTemp, log: false);
            boundsRotated = boundsRotatedTemp;
            Debug.Log ("TU | GetBoolArrayTransformed | Final pivot index: " + pivotRotatedIndexTemp + " | Final pivot position " + pivotRotated + " | Final bounds: " + boundsRotated);

            return result;
        }

        public static Vector3 GetPositionTransformed (Vector3 source, int rotation)
        {
            Vector3 result = Vector3.zero;
            switch (rotation)
            {
                case 0:
                    result = new Vector3 (source.x, source.y, source.z);
                    break;
                case 1:
                    result = new Vector3 (-source.z, source.y, source.x);
                    break;
                case 2:
                    result = new Vector3 (-source.x, source.y, -source.z);
                    break;
                case 3:
                    result = new Vector3 (source.z, source.y, -source.x);
                    break;
                default:
                    break;
            }
            return result;
        }

        public static Vector3Int GetVolumePivotTransformed (Vector3Int source, int rotation, Vector3Int bounds)
        {
            Transform pivot = new GameObject ().transform;
            Transform rotator = new GameObject ().transform;
            Transform root = new GameObject ().transform;

            pivot.parent = rotator;
            rotator.parent = root;
            pivot.localPosition = source.ToVector3 ();
            rotator.localRotation = Quaternion.Euler (0f, -90f * rotation, 0f);
            pivot.parent = root;

            if (rotation == 1)
                rotator.localPosition += new Vector3 (bounds.x, 0f, 0f);
            if (rotation == 2)
                rotator.localPosition += new Vector3 (bounds.x, 0f, bounds.z);
            if (rotation == 3)
                rotator.localPosition += new Vector3 (0f, 0f, bounds.x);

            Vector3Int result = new Vector3Int (pivot.localPosition);
            GameObject.DestroyImmediate (root.gameObject);

            return result;
        }

        private static readonly Vector3Int blockBounds = Vector3Int.size2x2x2;

        public static Vector3Int GetVolumePositionFromIndexInBlock (int index)
        {
            if (index == 2) index = 3;
            else if (index == 3) index = 2;
            else if (index == 6) index = 7;
            else if (index == 7) index = 6;

            return new Vector3Int (index % blockBounds.x, index / (blockBounds.z * blockBounds.x), (index / blockBounds.x) % blockBounds.z);
        }
    }





    [System.Serializable]
    public struct TilesetVertexProperties
    {
        public float huePrimary;
        public float saturationPrimary;
        public float brightnessPrimary;
        public float hueSecondary;
        public float saturationSecondary;
        public float brightnessSecondary;
        public float overrideIndex;
        public float damageIntensity;
        private int _hashCode;

        public TilesetVertexProperties (float huePrimary, float saturationPrimary, float brightnessPrimary, float hueSecondary, float saturationSecondary, float brightnessSecondary, float overrideIndex, float damageIntensity)
        {
            this.huePrimary = huePrimary;
            this.saturationPrimary = saturationPrimary;
            this.brightnessPrimary = brightnessPrimary;
            this.hueSecondary = hueSecondary;
            this.saturationSecondary = saturationSecondary;
            this.brightnessSecondary = brightnessSecondary;
            this.overrideIndex = overrideIndex;
            this.damageIntensity = damageIntensity;

            _hashCode =
                huePrimary.GetHashCode () ^
                saturationPrimary.GetHashCode () ^
                brightnessPrimary.GetHashCode () ^
                hueSecondary.GetHashCode () ^
                saturationSecondary.GetHashCode () ^
                brightnessSecondary.GetHashCode () ^
                overrideIndex.GetHashCode () ^
                damageIntensity.GetHashCode ();
        }

        private static TilesetVertexProperties _defaults;

        public static TilesetVertexProperties defaults
        {
            get
            {
                return new TilesetVertexProperties (0.0f, 0.0f, 0.5f, 0.0f, 0.0f, 0.5f, 0f, 0f);
            }
        }

        public override int GetHashCode ()
        {
            return _hashCode;
        }

        public override bool Equals (object obj)
        {
            return obj is TilesetVertexProperties && this == (TilesetVertexProperties)obj;
        }

        public static bool operator == (TilesetVertexProperties a, TilesetVertexProperties b)
        {
            return a.GetHashCode () == b.GetHashCode ();
            // return a.huePrimary == b.huePrimary && a.saturationPrimary == b.saturationPrimary && a.hueSecondary == b.hueSecondary && a.saturationSecondary == b.saturationSecondary && a.emissionIntensity == b.emissionIntensity && a.damageIntensity == b.damageIntensity;
        }
        public static bool operator != (TilesetVertexProperties a, TilesetVertexProperties b)
        {
            return a.GetHashCode () != b.GetHashCode ();
            // return a.huePrimary != b.huePrimary || a.saturationPrimary != b.saturationPrimary || a.hueSecondary != b.hueSecondary || a.saturationSecondary != b.saturationSecondary || a.emissionIntensity != b.emissionIntensity || a.damageIntensity != b.damageIntensity;
        }

        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append ("(Hp: ");
            sb.Append (huePrimary.ToString ("F3"));
            sb.Append ("; Sp: ");
            sb.Append (saturationPrimary.ToString ("F3"));
            sb.Append ("; Bp: ");
            sb.Append (brightnessPrimary.ToString ("F3"));
            sb.Append ("; Hs: ");
            sb.Append (hueSecondary.ToString ("F3"));
            sb.Append ("; Ss: ");
            sb.Append (saturationSecondary.ToString ("F3"));
            sb.Append ("; Bs: ");
            sb.Append (brightnessSecondary.ToString ("F3"));
            sb.Append ("; Ie: ");
            sb.Append (overrideIndex.ToString ("F3"));
            sb.Append ("; Id: ");
            sb.Append (damageIntensity.ToString ("F3"));
            sb.Append (")");
            return sb.ToString ();
        }
    }
}
