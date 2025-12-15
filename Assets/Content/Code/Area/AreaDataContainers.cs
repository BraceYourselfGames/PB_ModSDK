using UnityEngine;
using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Unity.Mathematics;

namespace Area
{
    public class AreaDataContainer
    {
        public bool[] points;
        public List<AreaDataSpot> spots;
        public List<AreaDataCustomization> customizations;
        public List<AreaDataIntegrity> integrities;
        public List<AreaDataProp> props;
        public List<AreaDataNavOverride> navOverrides;
        public List<int> indestructibleIndexes;

        public AreaDataContainer (AreaManager am)
        {
            if (am == null || am.points == null || am.points.Count == 0)
            {
                Debug.LogWarning ("Aborted attempt to construct area data container, since we don't have any points loaded");
                return;
            }

            points = new bool[am.points.Count];
            spots = new List<AreaDataSpot> ();
            customizations = new List<AreaDataCustomization> ();
            integrities = new List<AreaDataIntegrity> ();
            indestructibleIndexes = new List<int> ();

            for (int pointIndex = 0; pointIndex < am.points.Count; ++pointIndex)
            {
                AreaVolumePoint pointSource = am.points[pointIndex];
                points[pointIndex] = pointSource.pointState == AreaVolumePointState.Full || pointSource.pointState == AreaVolumePointState.FullDestroyed;

                if
                (
                    pointSource.spotConfiguration != AreaNavUtility.configEmpty &&
                    pointSource.spotConfigurationWithDamage != AreaNavUtility.configFull
                )
                {
                    // 253 (byte) <- -3 (int)
                    // 254 (byte) <- -2 (int)
                    // 255 (byte) <- -1 (int)
                    // 0   (byte) <-  0 (int)
                    // 1   (byte) <-  1 (int)
                    // 2   (byte) <-  2 (int)
                    // 2   (byte) <-  3 (int)

                    float offsetFloat = pointSource.terrainOffset;
                    int offsetInt = Mathf.RoundToInt (offsetFloat * 3f);
                    byte offsetByte = (byte)offsetInt;

                    AreaDataSpot spot = new AreaDataSpot
                    {
                        index = pointIndex,
                        tileset = pointSource.blockTileset,
                        group = pointSource.blockGroup,
                        subtype = pointSource.blockSubtype,
                        rotation = pointSource.blockRotation,
                        flip = pointSource.blockFlippedHorizontally,
                        offset = offsetByte
                    };
                    spots.Add (spot);

                    AreaDataCustomization customization = new AreaDataCustomization
                    {
                        index = pointIndex,
                        h1 = pointSource.customization.huePrimary,
                        s1 = pointSource.customization.saturationPrimary,
                        b1 = pointSource.customization.brightnessPrimary,
                        h2 = pointSource.customization.hueSecondary,
                        s2 = pointSource.customization.saturationSecondary,
                        b2 = pointSource.customization.brightnessSecondary,
                        emission = pointSource.customization.overrideIndex
                    };
                    customizations.Add (customization);
                }

                if
                (
                    pointSource.pointState != AreaVolumePointState.Empty &&
                    pointSource.integrity < 1f
                )
                {
                    AreaDataIntegrity integrity = new AreaDataIntegrity
                    {
                        index = pointIndex,
                        integrity = pointSource.integrity,
                        destructible = pointSource.destructible
                    };
                    integrities.Add (integrity);
                }
                
                if (!pointSource.destructible)
                    indestructibleIndexes.Add (pointIndex);
            }

            props = new List<AreaDataProp> (am.placementsProps.Count);
            for (int i = 0; i < am.placementsProps.Count; ++i)
            {

                AreaPlacementProp propSource = am.placementsProps[i];
                AreaDataProp prop = new AreaDataProp
                {
                    id = propSource.id,
                    pivotIndex = propSource.pivotIndex,
                    rotation = propSource.rotation,
                    flip = propSource.flipped,
                    status = 0,
                    offsetX = propSource.offsetX,
                    offsetZ = propSource.offsetZ,
                    h1 = propSource.hsbPrimary.x,
                    s1 = propSource.hsbPrimary.y,
                    b1 = propSource.hsbPrimary.z,
                    h2 = propSource.hsbSecondary.x,
                    s2 = propSource.hsbSecondary.y,
                    b2 = propSource.hsbSecondary.z
                };
                props.Add (prop);
            }

            navOverrides = new List<AreaDataNavOverride> (am.navOverridesSaved.Count);
            foreach (var kvp in am.navOverridesSaved)
            {
                var navOverride = new AreaDataNavOverride
                {
                    pivotIndex = kvp.Key,
                    offsetY = kvp.Value.offsetY
                };
                navOverrides.Add (navOverride);
            }
        }
        
        public AreaDataContainer (AreaClipboard clipboard)
        {
            if (clipboard == null || clipboard.clipboardPointsSaved == null || clipboard.clipboardPointsSaved.Count == 0)
            {
                Debug.LogWarning ("Aborted attempt to construct area data container, since there is no clipboard data available");
                return;
            }

            points = new bool[clipboard.clipboardPointsSaved.Count];
            spots = new List<AreaDataSpot> ();
            customizations = new List<AreaDataCustomization> ();
            integrities = new List<AreaDataIntegrity> ();
            indestructibleIndexes = new List<int> ();

            for (int pointIndex = 0; pointIndex < clipboard.clipboardPointsSaved.Count; ++pointIndex)
            {
                AreaVolumePoint pointSource = clipboard.clipboardPointsSaved[pointIndex];
                points[pointIndex] = pointSource.pointState == AreaVolumePointState.Full || pointSource.pointState == AreaVolumePointState.FullDestroyed;

                if
                (
                    pointSource.spotConfiguration != AreaNavUtility.configEmpty &&
                    pointSource.spotConfigurationWithDamage != AreaNavUtility.configFull
                )
                {
                    // 253 (byte) <- -3 (int)
                    // 254 (byte) <- -2 (int)
                    // 255 (byte) <- -1 (int)
                    // 0   (byte) <-  0 (int)
                    // 1   (byte) <-  1 (int)
                    // 2   (byte) <-  2 (int)
                    // 2   (byte) <-  3 (int)

                    float offsetFloat = pointSource.terrainOffset;
                    int offsetInt = Mathf.RoundToInt (offsetFloat * 3f);
                    byte offsetByte = (byte)offsetInt;

                    AreaDataSpot spot = new AreaDataSpot
                    {
                        index = pointIndex,
                        tileset = pointSource.blockTileset,
                        group = pointSource.blockGroup,
                        subtype = pointSource.blockSubtype,
                        rotation = pointSource.blockRotation,
                        flip = pointSource.blockFlippedHorizontally,
                        offset = offsetByte
                    };
                    spots.Add (spot);

                    AreaDataCustomization customization = new AreaDataCustomization
                    {
                        index = pointIndex,
                        h1 = pointSource.customization.huePrimary,
                        s1 = pointSource.customization.saturationPrimary,
                        b1 = pointSource.customization.brightnessPrimary,
                        h2 = pointSource.customization.hueSecondary,
                        s2 = pointSource.customization.saturationSecondary,
                        b2 = pointSource.customization.brightnessSecondary,
                        emission = pointSource.customization.overrideIndex
                    };
                    customizations.Add (customization);
                }

                if
                (
                    pointSource.pointState != AreaVolumePointState.Empty &&
                    pointSource.integrity < 1f
                )
                {
                    AreaDataIntegrity integrity = new AreaDataIntegrity
                    {
                        index = pointIndex,
                        integrity = pointSource.integrity,
                        destructible = pointSource.destructible
                    };
                    integrities.Add (integrity);
                }
                
                if (!pointSource.destructible)
                    indestructibleIndexes.Add (pointIndex);
            }

            props = new List<AreaDataProp> (clipboard.clipboardPropsSaved.Count);
            for (int i = 0; i < clipboard.clipboardPropsSaved.Count; ++i)
            {

                AreaPlacementProp propSource = clipboard.clipboardPropsSaved[i];
                AreaDataProp prop = new AreaDataProp
                {
                    id = propSource.id,
                    pivotIndex = propSource.pivotIndex,
                    rotation = propSource.rotation,
                    flip = propSource.flipped,
                    status = 0,
                    offsetX = propSource.offsetX,
                    offsetZ = propSource.offsetZ,
                    h1 = propSource.hsbPrimary.x,
                    s1 = propSource.hsbPrimary.y,
                    b1 = propSource.hsbPrimary.z,
                    h2 = propSource.hsbSecondary.x,
                    s2 = propSource.hsbSecondary.y,
                    b2 = propSource.hsbSecondary.z
                };
                props.Add (prop);
            }

            navOverrides = null;
        }

        public AreaDataContainer (AreaDataContainerSerialized dataCollections)
        {
            if (dataCollections == null || dataCollections.points == null || dataCollections.points.Length == 0)
            {
                Debug.LogWarning ("Aborted attempt to construct area data container {key} from raw data, since we don't have any points loaded");
                return;
            }

            int pointsCount = dataCollections.points.Length;
            points = new bool[pointsCount];
            
            for (int i = 0; i < pointsCount; ++i)
            {
                var pointFull = dataCollections.points[i];
                points[i] = pointFull;
            }

            byte flipThreshold = (byte)3;
            int spotsCount = dataCollections.spotIndexes.Length;
            spots = new List<AreaDataSpot> (spotsCount);
            customizations = new List<AreaDataCustomization> (spotsCount);

            for (int i = 0; i < spotsCount; ++i)
            {
                int index = dataCollections.spotIndexes[i];
                int tileset = dataCollections.spotTilesets[i];
                
                byte group = dataCollections.spotGroups[i];
                byte subtype = dataCollections.spotSubtypes[i];
                byte transform = dataCollections.spotTransforms[i];
                byte offset = dataCollections.spotOffsets[i];

                byte rotation = (byte)(transform % 4);
                bool flip = transform > flipThreshold;
                
                spots.Add (new AreaDataSpot
                {
                    index        = index,
                    tileset      = tileset,
                    group        = group,
                    subtype      = subtype,
                    rotation     = rotation,
                    flip         = flip,
                    offset       = offset
                });

                var materialPrimary = dataCollections.spotMaterialsPrimary[i];
                var materialSecondary = dataCollections.spotMaterialsSecondary[i];
                
                customizations.Add (new AreaDataCustomization
                {
                    index = index,
                    h1 = materialPrimary.x,
                    s1 = materialPrimary.y,
                    b1 = materialPrimary.z,
                    emission = materialPrimary.w,
                    h2 = materialSecondary.x,
                    s2 = materialSecondary.y,
                    b2 = materialSecondary.z
                });
            }
            
            if (dataCollections.indestructibleIndexes != null)
                indestructibleIndexes = new List<int> (dataCollections.indestructibleIndexes);

            int damageCount = dataCollections.damageIndexes.Length;
            integrities = new List<AreaDataIntegrity> ();
            
            for (int i = 0; i < damageCount; ++i)
            {
                var index = dataCollections.damageIndexes[i];
                var value = dataCollections.damageValues[i];
                
                integrities.Add (new AreaDataIntegrity
                {
                    index = index,
                    integrity = value
                });
            }

            int propsCount = dataCollections.propIndexes.Length;
            props = new List<AreaDataProp> (propsCount);
            
            for (int i = 0; i < propsCount; ++i)
            {
                int index = dataCollections.propIndexes[i];
                int id = dataCollections.propIDs[i];
                byte transform = dataCollections.propTransforms[i];
                Vector3 offset = dataCollections.propOffsets[i];
                Vector3 materialPrimary = dataCollections.propMaterialsPrimary[i];
                Vector3 materialSecondary = dataCollections.propMaterialsSecondary[i];
                
                byte rotation = (byte)(transform % 4);
                bool flip = transform > flipThreshold;

                props.Add (new AreaDataProp
                {
                    id = id,
                    pivotIndex = index,
                    rotation = rotation,
                    flip = flip,
                    status = 0,
                    offsetX = offset.x,
                    offsetZ = offset.z,
                    h1 = materialPrimary.x,
                    s1 = materialPrimary.y,
                    b1 = materialPrimary.z,
                    h2 = materialSecondary.x,
                    s2 = materialSecondary.y,
                    b2 = materialSecondary.z
                });
            }

            if (dataCollections.navOverrideIndexes != null && dataCollections.navOverrideOffsets != null)
            {
                int navOverridesCount = dataCollections.navOverrideIndexes.Length;
                navOverrides = new List<AreaDataNavOverride> (navOverridesCount);
                
                // if (navOverridesCount > 0)
                //     Debug.Log ($"Loading nav overrides | Indexes: {dataCollections.navOverrideIndexes.Length} | Offsets: {dataCollections.navOverrideOffsets.Length}");

                for (int i = 0; i < navOverridesCount; ++i)
                {
                    int index = dataCollections.navOverrideIndexes[i];
                    float offset = dataCollections.navOverrideOffsets[i];

                    navOverrides.Add (new AreaDataNavOverride
                    {
                        pivotIndex = index,
                        offsetY = offset
                    });
                }
            }
        }
    }
    
    public class AreaDataContainerSerialized
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

        [BinaryData] public int[] damageIndexes;
        [BinaryData] public float[] damageValues;

        [BinaryData (logIfMissing = false)] public int[] navOverrideIndexes;
        [BinaryData (logIfMissing = false)] public float[] navOverrideOffsets;

        [BinaryData] public int[] propIndexes;
        [BinaryData] public int[] propIDs;
        [BinaryData] public byte[] propTransforms;
        [BinaryData] public float3[] propOffsets;
        [BinaryData] public float3[] propMaterialsPrimary;
        [BinaryData] public float3[] propMaterialsSecondary;
        
        [BinaryData (logIfMissing = false)] public int[] indestructibleIndexes;

        public AreaDataContainerSerialized () { }
        
        public AreaDataContainerSerialized (AreaDataContainer dataUnpacked)
        {
            if (dataUnpacked == null || dataUnpacked.points == null || dataUnpacked.points.Length == 0)
                return;

            int pointCount = dataUnpacked.points.Length;
            byte flipShift = (byte)4;
            
            points = new bool[pointCount];

            var spotIndexesList = new List<int> ();
            var spotTilesetsList = new List<int> ();
            var spotGroupsList = new List<byte> ();
            var spotSubtypesList = new List<byte> ();
            var spotTransformsList = new List<byte> ();
            var spotOffsetsList = new List<byte> ();

            var spotMaterialsPrimaryList = new List<float4> ();
            var spotMaterialsSecondaryList = new List<float3> ();

            var damageIndexesList = new List<int> ();
            var damageValuesList = new List<float> ();

            for (int i = 0; i < dataUnpacked.points.Length; ++i)
            {
                bool pointFull = dataUnpacked.points[i] == true;
                points[i] = pointFull;
            }
            
            for (int i = 0; i < dataUnpacked.spots.Count; ++i)
            {
                var spotSource = dataUnpacked.spots[i];

                byte transformByte = spotSource.rotation;
                if (spotSource.flip)
                    transformByte += flipShift;
                
                spotIndexesList.Add (spotSource.index);
                spotTilesetsList.Add (spotSource.tileset);
                spotGroupsList.Add (spotSource.group);
                spotSubtypesList.Add (spotSource.subtype);
                spotTransformsList.Add (transformByte);
                spotOffsetsList.Add (spotSource.offset);
            }

            if (dataUnpacked.indestructibleIndexes != null)
                indestructibleIndexes = dataUnpacked.indestructibleIndexes.ToArray ();
            else
                indestructibleIndexes = Array.Empty<int> ();

            for (int i = 0; i < dataUnpacked.customizations.Count; ++i)
            {
                var materialSource = dataUnpacked.customizations[i];

                spotMaterialsPrimaryList.Add (new float4
                (
                    materialSource.h1,
                    materialSource.s1,
                    materialSource.b1, 
                    materialSource.emission
                ));
                    
                spotMaterialsSecondaryList.Add (new float3 
                (
                    materialSource.h2,
                    materialSource.s2,
                    materialSource.b2
                ));
            }
            
            for (int i = 0; i < dataUnpacked.integrities.Count; ++i)
            {
                var integritySource = dataUnpacked.integrities[i];
                
                damageIndexesList.Add (integritySource.index);
                damageValuesList.Add (integritySource.integrity);
            }
            
            spotIndexes = spotIndexesList.ToArray ();
            spotTilesets = spotTilesetsList.ToArray ();
            spotGroups = spotGroupsList.ToArray ();
            spotSubtypes = spotSubtypesList.ToArray ();
            spotTransforms = spotTransformsList.ToArray ();
            spotOffsets = spotOffsetsList.ToArray ();

            spotMaterialsPrimary = spotMaterialsPrimaryList.ToArray ();
            spotMaterialsSecondary = spotMaterialsSecondaryList.ToArray ();

            damageIndexes = damageIndexesList.ToArray ();
            damageValues = damageValuesList.ToArray ();
            
            var propIndexesList = new List<int> ();
            var propIDsList = new List<int> ();
            var propTransformsList = new List<byte> ();
            var propOffsetsList = new List<float3> ();
            var propMaterialsPrimaryList = new List<float3> ();
            var propMaterialsSecondaryList = new List<float3> ();

            for (int i = 0; i < dataUnpacked.props.Count; ++i)
            {
                var propSource = dataUnpacked.props[i];
                
                byte transformByte = propSource.rotation;
                if (propSource.flip)
                    transformByte += flipShift;
                
                propIndexesList.Add (propSource.pivotIndex);
                propIDsList.Add (propSource.id);
                propTransformsList.Add (transformByte);
                propOffsetsList.Add (new float3 (propSource.offsetX, 0f, propSource.offsetZ));
                
                propMaterialsPrimaryList.Add (new float3 
                (
                    propSource.h1,
                    propSource.s1,
                    propSource.b1
                ));
                
                propMaterialsSecondaryList.Add (new float3 
                (
                    propSource.h2,
                    propSource.s2,
                    propSource.b2
                ));
            }
            
            var navOverrideIndexList = new List<int> ();
            var navOverrideOffsetList = new List<float> ();
            for (var i = 0; i < dataUnpacked.navOverrides.Count; i += 1)
            {
                var navOverrideSource = dataUnpacked.navOverrides[i];
                navOverrideIndexList.Add (navOverrideSource.pivotIndex);
                navOverrideOffsetList.Add (navOverrideSource.offsetY);
            }
            
            propIndexes = propIndexesList.ToArray ();
            propIDs = propIDsList.ToArray ();
            propTransforms = propTransformsList.ToArray ();
            propOffsets = propOffsetsList.ToArray ();
            propMaterialsPrimary = propMaterialsPrimaryList.ToArray ();
            propMaterialsSecondary = propMaterialsSecondaryList.ToArray ();
            navOverrideIndexes = navOverrideIndexList.ToArray ();
            navOverrideOffsets = navOverrideOffsetList.ToArray ();
        }
    }
}