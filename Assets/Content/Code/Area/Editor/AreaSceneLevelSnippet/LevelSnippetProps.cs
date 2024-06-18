using System.Collections.Generic;
using System.IO;

using PhantomBrigade.Data;

using Unity.Mathematics;
using UnityEngine;

namespace Area
{
    [System.Serializable]
    sealed class LevelSnippetProps : LevelSnippetContent
    {
        public const int Priority = -1;
        public const string DisplayText = "Props";
        public static ILevelExtension Create () => new LevelSnippetProps ();

        protected private override int GetPriorityInternal () => Priority;

        public override (bool OK, string ErrorMessage) Deserialize (DirectoryInfo path, LevelData data)
        {
            var channels = new Channels ();
            if (!BinaryDataUtility.LoadFieldsFromBinary (channels, path.FullName + "/"))
            {
                return (false, "Level snippet props chunk unable to load channels -- check console log | path: " + path.FullName);
            }

            var propCount = channels.propIndexes.Length;
            var (ok, errorMessage) = ValidateChannelLengths (propCount, channels);
            if (!ok)
            {
                return (false, errorMessage);
            }

            var points = data.Points;
            for (var i = 0; i < channels.propIndexes.Length; i += 1)
            {
                var index = channels.propIndexes[i];
                if (!index.IsValidIndex (points))
                {
                    Debug.LogWarningFormat
                    (
                        "Level snippet props chunk found prop with index out of bounds -- skipping | prop: {0} | index: {1} | point: {2}",
                        i,
                        index,
                        points.Count
                    );
                    continue;
                }

                var propID = channels.propIDs[i];
                var prototype = AreaAssetHelper.GetPropPrototype (propID);
                if (prototype == null)
                {
                    Debug.LogWarningFormat
                    (
                        "Level snippet props chunk found prop with no matching prototype -- skipping | prop: {0} | index: {1} | prototype ID: {2}",
                        i,
                        index,
                        propID
                    );
                    continue;
                }

                var offset = channels.propOffsets[i];
                var transform = channels.propTransforms[i];
                var primary = channels.propMaterialsPrimary[i];
                var secondary = channels.propMaterialsSecondary[i];
                var placement = new AreaPlacementProp ()
                {
                    id = propID,
                    pivotIndex = index,
                    offsetX = offset.x,
                    offsetZ = offset.z,
                    rotation = (byte)(transform % flipShift),
                    flipped = transform >= flipShift,
                    hsbPrimary = new Vector4 (primary.x, primary.y, primary.z, 0f),
                    hsbSecondary = new Vector4 (secondary.x, secondary.y, secondary.z, 0f),
                    state = new AreaPropState(),
                };
                data.Props.Add (placement);
            }
            return (true, "");
        }

        public override (SerializationResult Result, string ErrorMessage) Serialize (string modID, DirectoryInfo path, LevelData data)
        {
            if (data.Props.Count == 0)
            {
                return (SerializationResult.Empty, "");
            }

            var indexes = new List<int> ();
            var IDs = new List<int> ();
            var transforms = new List<byte> ();
            var offsets = new List<float3> ();
            var primaryMaterials = new List<float3> ();
            var secondaryMaterials = new List<float3> ();

            foreach (var prop in data.Props)
            {
                indexes.Add(prop.pivotIndex);
                IDs.Add (prop.id);
                var transform = prop.rotation;
                if (prop.flipped)
                {
                    transform += flipShift;
                }
                transforms.Add (transform);
                offsets.Add (new float3 (prop.offsetX, 0f, prop.offsetZ));
                var primary = prop.hsbPrimary;
                primaryMaterials.Add (new float3 (primary.x, primary.y, primary.z));
                var secondary = prop.hsbSecondary;
                secondaryMaterials.Add (new float3 (secondary.x, secondary.y, secondary.z));
            }

            priority = Priority;
            this.modID = modID;

            var channels = new Channels ()
            {
                propIndexes = indexes.ToArray (),
                propIDs = IDs.ToArray (),
                propTransforms = transforms.ToArray (),
                propOffsets = offsets.ToArray (),
                propMaterialsPrimary = primaryMaterials.ToArray (),
                propMaterialsSecondary = secondaryMaterials.ToArray (),
            };
            return BinaryDataUtility.SaveFieldsToBinary (channels, path.FullName + "/")
                ? (SerializationResult.Success, "")
                : (SerializationResult.Error, "Level snippet props chunk not saved completely -- check console log | path: " + path.FullName);
        }

        (bool OK, string ErrorMessage) ValidateChannelLengths (int propCount, Channels channels)
        {
            if (propCount != channels.propIDs.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet props chunk -- length mismatch | channel: prop IDs | spot count: {0} | channel count: {1}",
                    propCount,
                    channels.propIDs.Length
                );
                return (false, msg);
            }
            if (propCount != channels.propTransforms.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet props chunk -- length mismatch | channel: transforms | spot count: {0} | channel count: {1}",
                    propCount,
                    channels.propTransforms.Length
                );
                return (false, msg);
            }
            if (propCount != channels.propOffsets.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet props chunk -- length mismatch | channel: offsets | spot count: {0} | channel count: {1}",
                    propCount,
                    channels.propOffsets.Length
                );
                return (false, msg);
            }
            if (propCount != channels.propMaterialsPrimary.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet props chunk -- length mismatch | channel: primary materials | spot count: {0} | channel count: {1}",
                    propCount,
                    channels.propMaterialsPrimary.Length
                );
                return (false, msg);
            }
            if (propCount != channels.propMaterialsSecondary.Length)
            {
                var msg = string.Format
                (
                    "Inconsistent data detected in level snippet props chunk -- length mismatch | channel: secondary materials | spot count: {0} | channel count: {1}",
                    propCount,
                    channels.propMaterialsSecondary.Length
                );
                return (false, msg);
            }
            return (true, "");
        }

        const byte flipShift = 4;

        sealed class Channels
        {
            [BinaryData] public int[] propIndexes;
            [BinaryData] public int[] propIDs;
            [BinaryData] public byte[] propTransforms;
            [BinaryData] public float3[] propOffsets;
            [BinaryData] public float3[] propMaterialsPrimary;
            [BinaryData] public float3[] propMaterialsSecondary;
        }
    }
}
