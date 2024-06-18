using System.Collections.Generic;
using System.IO;

using PhantomBrigade.Data;

using Unity.Mathematics;
using UnityEngine;

namespace Area
{
    using Scene;

    [System.Serializable]
    sealed class LevelSnippetCore : LevelSnippetContent
    {
        public const int Priority = -2;
        public const string DisplayText = "Terrain & Structures";
        public static ILevelExtension Create () => new LevelSnippetCore ();

        protected private override int GetPriorityInternal () => Priority;

        public override (bool OK, string ErrorMessage) Deserialize (DirectoryInfo path, LevelData data)
        {
            var channels = new Channels ();
            if (!BinaryDataUtility.LoadFieldsFromBinary (channels, path.FullName + "/"))
            {
                return (false, "Level snippet core chunk unable to load channels -- check console log | path: " + path.FullName);
            }

            var bounds = data.Bounds;
            var expectedSize = bounds.x * bounds.y * bounds.z;
            if (channels.points.Length != expectedSize)
            {
                var msg = string.Format
                (
                    "Level snippet core chunk has fewer points than required by bounds | path: {0} | expected: {1} | actual: {2}",
                    path,
                    expectedSize,
                    channels.points.Length
                );
                return (false, msg);
            }

            var spotCount = channels.spotIndexes.Length;
            if (spotCount > expectedSize)
            {
                var msg = string.Format
                (
                    "Level snippet core chunk has more spots than points in snippet | path: {0} | point count: {1} | spot count: {2}",
                    data,
                    expectedSize,
                    spotCount
                );
                return (false, msg);
            }

            DeserializePointData (channels, data);
            DeserializeSpotData (channels, data);
            return (true, "");
        }

        public override (SerializationResult Result, string ErrorMessage) Serialize (string modID, DirectoryInfo path, LevelData data)
        {
            if (data.Bounds == Vector3Int.size0x0x0)
            {
                return (SerializationResult.Empty, "");
            }
            if (data.Points.Count == 0)
            {
                return (SerializationResult.Empty, "");
            }

            var bounds = data.Bounds;
            var expectedSize = bounds.x * bounds.y * bounds.z;
            if (data.Points.Count != expectedSize)
            {
                var msg = string.Format
                (
                    "Level snippet points serializer detected an inconsistency between bounds and points | path: {0}\nPoint count mismatch | bounds: {1} | expected point count: {2} | actual point count: {3}",
                    path,
                    bounds,
                    expectedSize,
                    data.Points.Count
                );
                return (SerializationResult.Error, msg);
            }

            priority = Priority;
            this.modID = modID;

            var channels = new Channels ();
            SerializePointData (data, channels);
            SerializeSpotData (data, channels);
            return BinaryDataUtility.SaveFieldsToBinary (channels, path.FullName + "/")
                ? (SerializationResult.Success, "")
                : (SerializationResult.Error, "Level snippet core chunk not saved completely -- check console log | path: " + path.FullName);
        }

        void DeserializePointData (Channels channels, LevelData data)
        {
            var points = data.Points;
            points.Clear ();
            foreach (var pt in channels.points)
            {
                var point = new AreaVolumePoint ()
                {
                    pointState = pt ? AreaVolumePointState.Full : AreaVolumePointState.Empty,
                };
                points.Add (point);
            }
        }

        void DeserializeSpotData (Channels channels, LevelData data)
        {
            var points = data.Points;
            for (var i = 0; i < channels.spotIndexes.Length; i += 1)
            {
                var index = channels.spotIndexes[i];
                if (!index.IsValidIndex (points))
                {
                    // XXX is this a fatal error?
                    continue;
                }
                var point = points[index];
                point.blockTileset = channels.spotTilesets[i];
                point.blockGroup = channels.spotGroups[i];
                point.blockSubtype = channels.spotSubtypes[i];
                var offset = channels.spotOffsets[i];
                var sign = -((offset & 0x80) >> 7) & ~0x7F;
                point.terrainOffset = (offset | sign) / WorldSpace.BlockSize;
                var transform = channels.spotTransforms[i];
                point.blockRotation = (byte)(transform % flipShift);
                point.blockFlippedHorizontally = transform >= flipShift;
                var primary = channels.spotMaterialsPrimary[i];
                var secondary = channels.spotMaterialsSecondary[i];
                point.customization = new TilesetVertexProperties
                (
                    primary.x,
                    primary.y,
                    primary.z,
                    secondary.x,
                    secondary.y,
                    secondary.z,
                    primary.w,
                    0f
                );
            }
        }

        void SerializePointData (LevelData data, Channels channels)
        {
            var points = new bool[data.Points.Count];
            for (var i = 0; i < points.Length; i += 1)
            {
                points[i] = data.Points[i].pointState != AreaVolumePointState.Empty;
            }
            channels.points = points;
        }

        void SerializeSpotData (LevelData data, Channels channels)
        {
            var indexes = new List<int> ();
            var tilesets = new List<int> ();
            var groups = new List<byte> ();
            var subtypes = new List<byte> ();
            var transforms = new List<byte> ();
            var offsets = new List<byte> ();
            var primaryMaterials = new List<float4> ();
            var secondaryMaterials = new List<float3> ();

            foreach (var point in data.Points)
            {
                if (!point.spotPresent)
                {
                    continue;
                }
                if (point.pointState == AreaVolumePointState.Empty && point.spotConfiguration == TilesetUtility.configurationEmpty)
                {
                    continue;
                }
                indexes.Add (point.spotIndex);
                tilesets.Add (point.blockTileset);
                groups.Add (point.blockGroup);
                subtypes.Add (point.blockSubtype);
                offsets.Add ((byte)Mathf.RoundToInt (point.terrainOffset * WorldSpace.BlockSize));
                var transform = point.blockRotation;
                if (point.blockFlippedHorizontally)
                {
                    transform += flipShift;
                }
                transforms.Add (transform);
                var customization = point.customization;
                primaryMaterials.Add (new float4 (customization.huePrimary, customization.saturationPrimary, customization.brightnessPrimary, customization.overrideIndex));
                secondaryMaterials.Add (new float3 (customization.hueSecondary, customization.saturationSecondary, customization.brightnessSecondary));
            }

            channels.spotIndexes = indexes.ToArray ();
            channels.spotTilesets = tilesets.ToArray ();
            channels.spotGroups = groups.ToArray ();
            channels.spotSubtypes = subtypes.ToArray ();
            channels.spotTransforms = transforms.ToArray ();
            channels.spotOffsets = offsets.ToArray ();
            channels.spotMaterialsPrimary = primaryMaterials.ToArray ();
            channels.spotMaterialsSecondary = secondaryMaterials.ToArray ();
        }

        (bool OK, string ErrorMessage) ValidateChannelLengths (int spotCount, Channels channels)
        {
            if (spotCount != channels.spotTilesets.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: tilesets | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotTilesets.Length
                );
                return (false, msg);
            }
            if (spotCount != channels.spotGroups.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: groups | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotGroups.Length
                );
                return (false, msg);
            }
            if (spotCount != channels.spotSubtypes.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: subtypes | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotSubtypes.Length
                );
                return (false, msg);
            }
            if (spotCount != channels.spotTransforms.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: transforms | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotTransforms.Length
                );
                return (false, msg);
            }
            if (spotCount != channels.spotOffsets.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: offsets | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotOffsets.Length
                );
                return (false, msg);
            }
            if (spotCount != channels.spotMaterialsPrimary.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: primary materials | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotMaterialsPrimary.Length
                );
                return (false, msg);
            }
            if (spotCount != channels.spotMaterialsSecondary.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet spots chunk -- length mismatch | channel: secondary materials | spot count: {0} | channel count: {1}",
                    spotCount,
                    channels.spotMaterialsSecondary.Length
                );
                return (false, msg);
            }
            return (true, "");
        }

        const byte flipShift = 4;

        sealed class Channels
        {
            [BinaryData] public bool[] points;
            [BinaryData] public int[] spotIndexes;
            [BinaryData] public int[] spotTilesets;
            [BinaryData] public byte[] spotGroups;
            [BinaryData] public byte[] spotSubtypes;
            [BinaryData] public byte[] spotTransforms;
            [BinaryData] public byte[] spotOffsets;
            [BinaryData] public float4[] spotMaterialsPrimary;
            [BinaryData] public float3[] spotMaterialsSecondary;
        }
    }
}
