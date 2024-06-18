using System.Collections.Generic;
using System.Linq;
using CustomRendering;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Area
{
    [System.Serializable]
    public class AreaTilesetDatabaseSerialized
    {
        public List<AreaTilesetSerialized> tilesets;

        public AreaTilesetDatabaseSerialized () { }
        public AreaTilesetDatabaseSerialized (AreaTilesetDatabase source)
        {
            if (source == null || source.tilesets == null)
                return;

            tilesets = new List<AreaTilesetSerialized> ();
            foreach (KeyValuePair<int, AreaTileset> kvp in source.tilesets)
            {
                AreaTileset tileset = kvp.Value;
                AreaTilesetSerialized tilesetSerialized = new AreaTilesetSerialized ();

                tilesetSerialized.name = tileset.name;
                tilesetSerialized.id = tileset.id;
                tilesetSerialized.destructible = tileset.destructible;
                tilesetSerialized.hardness = tileset.hardness;
                tilesetSerialized.idOfInterior = tileset.interior != null ? tileset.interior.id : AreaTilesetHelper.idOfNull;

                tilesetSerialized.materialOverlays = tileset.materialOverlays;
                tilesetSerialized.groupIdentifiers = tileset.groupIdentifiers;

                tilesetSerialized.navOverrides = tileset.navOverrides;
                tilesetSerialized.navRestrictions = tileset.navRestrictions;

                tilesetSerialized.sfxNameImpact = tileset.sfxNameImpact;
                tilesetSerialized.fxNameHit = tileset.fxNameHit;
                tilesetSerialized.fxNameStep = tileset.fxNameStep;
                tilesetSerialized.fxNameExplosion = tileset.fxNameExplosion;
                tilesetSerialized.propIDDebrisClumps = tileset.propIDDebrisClumps;
                tilesetSerialized.propIDDebrisPile = tileset.propIDDebrisPile;

				tilesetSerialized.primaryColor = tileset.primaryColor;
				tilesetSerialized.secondaryColor = tileset.secondaryColor;

                tilesets.Add (tilesetSerialized);
                Debug.Log ("AreaTilesetDatabaseSerialized | Adding new tileset | Name: " + tilesetSerialized.name + " | ID: " + tilesetSerialized.id + " | ID of interior: " + tilesetSerialized.idOfInterior);
            }
        }
    }

    [System.Serializable]
    public class AreaTilesetDatabase
    {
        [ListDrawerSettings (DraggableItems = false, HideAddButton = true, HideRemoveButton = true, IsReadOnly = true)]
        [LabelText ("Configurations")]
        public AreaConfigurationData[] configurationDataForBlocks;

        public SortedDictionary<int, AreaTileset> tilesets;

        [HideInInspector]
        public AreaTileset tilesetTerrain;

        [HideInInspector]
        public AreaTileset tilesetFallback;

        [HideInInspector]
        public AreaTileset tilesetRoad;

        public AreaTilesetDatabase () { }
        public AreaTilesetDatabase (AreaTilesetDatabaseSerialized source)
        {
            if (source == null || source.tilesets == null)
                return;

            tilesets = new SortedDictionary<int, AreaTileset> ();

            // Round one - direct tileset loading
            for (int i = source.tilesets.Count - 1; i >= 0; --i)
            {
                AreaTilesetSerialized tilesetSerialized = source.tilesets[i];
                AreaTileset tileset = new AreaTileset ();

                tileset.id = tilesetSerialized.id;
                tileset.name = tilesetSerialized.name;
                tileset.destructible = tilesetSerialized.destructible;
                tileset.hardness = tilesetSerialized.hardness;
                tileset.blocks = new AreaBlockDefinition[256];
                tileset.usedAsInterior = false;

                tileset.materialOverlays = tilesetSerialized.materialOverlays;
                tileset.groupIdentifiers = tilesetSerialized.groupIdentifiers;

                tileset.navOverrides = tilesetSerialized.navOverrides;
                tileset.navRestrictions = tilesetSerialized.navRestrictions;

                tileset.sfxNameImpact = tilesetSerialized.sfxNameImpact;
                tileset.fxNameHit = tilesetSerialized.fxNameHit;
                tileset.fxNameStep = tilesetSerialized.fxNameStep;
                tileset.fxNameExplosion = tilesetSerialized.fxNameExplosion;
                tileset.propIDDebrisClumps = tilesetSerialized.propIDDebrisClumps;
                tileset.propIDDebrisPile = tilesetSerialized.propIDDebrisPile;

                tileset.primaryColor = tilesetSerialized.primaryColor;
                tileset.secondaryColor = tilesetSerialized.secondaryColor;

                if (!tilesets.ContainsKey (tileset.id))
                {
                    // Debug.Log ("AreaTilesetDatabase | Successfully loaded tileset " + tilesetSerialized.name + " with ID " + tilesetSerialized.id);
                    tilesets.Add (tileset.id, tileset);
                }
                else
                {
                    Debug.LogWarning ("AreaTilesetDatabase | Failed to load tileset " + tilesetSerialized.name + " to the library due to collision of its ID with an existing tileset: " + tilesetSerialized.id + " | Removing this tileset from future consideration");
                    source.tilesets.RemoveAt (i);
                }
            }

            // Round two - resolving dependencies
            for (int i = 0; i < source.tilesets.Count; ++i)
            {
                AreaTilesetSerialized tilesetSerialized = source.tilesets[i];
                AreaTileset tileset = tilesets[tilesetSerialized.id];

                if (tilesetSerialized.idOfInterior == AreaTilesetHelper.idOfNull)
                    continue;

                if (tilesets.ContainsKey (tilesetSerialized.idOfInterior))
                {
                    AreaTileset tilesetInterior = tilesets[tilesetSerialized.idOfInterior];
                    tileset.interior = tilesetInterior;
                    tilesetInterior.usedAsInterior = true;
                    // Debug.Log ("AreaTilesetDatabaseSerialized | Binding interior tileset " + tilesetInterior.name + " to tileset " + tileset.name);
                }
                else
                {
                    Debug.LogWarning ("AreaTilesetDatabaseSerialized | Failed to find interior tileset ID " + tilesetSerialized.idOfInterior + " for tileset " + tileset.name);
                }
            }

            if (tilesets.ContainsKey (AreaTilesetHelper.idOfFallback))
            {
                tilesetFallback = tilesets[AreaTilesetHelper.idOfFallback];
                // Debug.Log ("AreaTilesetDatabaseSerialized | Successfully found the fallback tileset named: " + tilesetFallback.name);
            }
            else
                Debug.LogWarning ("AreaTilesetDatabaseSerialized | Failed to find fallback tilset using ID: " + AreaTilesetHelper.idOfFallback);

            if (tilesets.ContainsKey (AreaTilesetHelper.idOfRoad))
            {
                tilesetRoad = tilesets[AreaTilesetHelper.idOfRoad];
                // Debug.Log ("AreaTilesetDatabaseSerialized | Successfully found the road tileset named: " + tilesetRoad.name);
            }
            else
                Debug.LogWarning ("AreaTilesetDatabaseSerialized | Failed to find fallback tilset using ID: " + AreaTilesetHelper.idOfRoad);

            if (tilesets.ContainsKey (AreaTilesetHelper.idOfTerrain))
            {
                tilesetTerrain = tilesets[AreaTilesetHelper.idOfTerrain];
                // Debug.Log ("AreaTilesetDatabaseSerialized | Successfully found the terrain tileset named: " + tilesetTerrain.name);
            }
            else
                Debug.LogWarning ("AreaTilesetDatabaseSerialized | Failed to find fallback tilset using ID: " + AreaTilesetHelper.idOfTerrain);
        }
    }

    [System.Serializable]
    public struct AreaBlockLight
    {
        public float offset;
        public float intensity;
    }

    [System.Serializable]
    public class AreaTilesetSerialized
    {
        public int id;
        public int idOfInterior;
        public string name;

        public bool destructible = false;
        public float hardness = 1;

        public string sfxNameImpact = "impact_bullet_rock";
        public string fxNameHit = "fx_impact_concrete";
        public string fxNameStep = "fx_mech_footstep_concrete";
        public string fxNameExplosion = "fx_environment_explosion";
        public int propIDDebrisPile = 100;
        public List<int> propIDDebrisClumps = new List<int> (new int[] { 103 });

        public Dictionary<int, string> materialOverlays;
        public Dictionary<int, string> groupIdentifiers;
        public List<AreaTilesetNavOverride> navOverrides;
        public Dictionary<int, AreaTilesetNavRestriction> navRestrictions;

		public Vector4 primaryColor;
		public Vector4 secondaryColor;
    }

    [System.Serializable]
    public class AreaTileset
    {
        [BoxGroup ("Core")]
        public int id;

        [BoxGroup ("Core")]
        public string name;

        [BoxGroup ("Core")]
        public bool destructible = false;

        [BoxGroup ("Core")]
        public float hardness = 1;

        [BoxGroup ("FX")]
        public string sfxNameImpact = "impact_bullet_rock";

        [BoxGroup ("FX")]
        public string fxNameHit = "fx_impact_concrete";

        [BoxGroup ("FX")]
        public string fxNameStep = "fx_mech_footstep_concrete";

        [BoxGroup ("FX")]
        public string fxNameExplosion = "fx_environment_explosion";

        [BoxGroup ("FX")]
        public int propIDDebrisPile = 100;

        [BoxGroup ("FX")]
        public List<int> propIDDebrisClumps = new List<int> (new int[] { 103 });

        [FoldoutGroup ("Material Overlays", false)]
        public Dictionary<int, string> materialOverlays;

        [FoldoutGroup ("Subtype Groups", false)]
        public Dictionary<int, string> groupIdentifiers;

        [FoldoutGroup ("Navigation", false)]
        public List<AreaTilesetNavOverride> navOverrides;

        [FoldoutGroup ("Navigation", false)]
        public Dictionary<int, AreaTilesetNavRestriction> navRestrictions;

        [FoldoutGroup ("Blocks", false)]
        public AreaBlockDefinition[] blocks;

        [HideInInspector]
        public AreaTileset interior;
        public bool usedAsInterior;

        public Vector4 primaryColor;
        public Vector4 secondaryColor;
    }

    public static class AreaTilesetHelper
    {
        public const byte assetFamilyBlock = 0;
        public const byte assetFamilyMultiblock = 1;
        public const byte assetFamilyDamage = 2;

        public const int idOfNull = -1;
        public const int idOfFallback = 10;
        public const int idOfForest = 100;
        public const int idOfTerrain = 150;
        public const int idOfRoad = 200;

        private static readonly string configPath = "Configs/Tilesets";
        private static readonly string configName = "tilesets.yaml";

        public static AreaTilesetDatabase database;
        public static AreaTilesetDatabaseSerialized databaseSerialized;

        public static bool log = false;
        private static bool autoloadAttempted = false;

        private static Dictionary<long, AreaDataNavOverride> navOverridesByID = new Dictionary<long, AreaDataNavOverride> ();

        private static List<MaterialSerializationHelper> materialHelpers;
        private static Dictionary<long, InstancedModelContainer> instancedModels;
        private static InstancedMeshRenderer instancedModelFallback;

        public static List<long> instancedIDFailures = new List<long> ();
        private static Dictionary<long, AreaTilesetLight> lightDefinitions;

        /// <summary>
        /// Reference tileset was modeled over particular trimmed subset of 0-255 configuration space, which makes it necessary to make those 1-in-4 selections the basis for configuration data
        /// </summary>

        public static byte[] configurationOrder = new byte[]
        {
        1,
        3,
        5,
        7,
        15,
        16,
        17,
        18,
        19,
        20,
        21,
        22,
        23,
        26,
        27,
        30,
        31,
        48,
        49,
        51,
        52,
        53,
        54,
        55,
        60,
        61,
        63,
        80,
        81,
        82,
        83,
        85,
        87,
        90,
        91,
        95,
        112,
        113,
        114,
        115,
        117,
        119,
        120,
        121,
        122,
        123,
        125,
        127,
        240,
        241,
        243,
        245,
        247
        };




        public static void CheckResources ()
        {
            if (!autoloadAttempted)
            {
                autoloadAttempted = true;
                if (database == null)
                {
                    LoadDatabase ();
                }
            }
        }

        public static bool AreAssetsPresent ()
        {
            return database?.tilesets != null && database.tilesets.Count > 0 && database.tilesetFallback != null;
        }

        public static void ReapplyMaterialOverrides ()
        {
            if (materialHelpers == null)
                return;

            foreach (var data in materialHelpers)
                data.ApplyToTarget ();
        }

        public static AreaTileset GetTileset (int id)
        {
            CheckResources ();
            if (database.tilesets.ContainsKey (id))
                return database.tilesets[id];
            else
                return database.tilesetFallback;
        }

        public static void LoadDatabase ()
        {
            databaseSerialized = UtilitiesYAML.LoadDataFromFile<AreaTilesetDatabaseSerialized> (configPath, configName);
            if (databaseSerialized == null)
                return;

            database = new AreaTilesetDatabase (databaseSerialized);
            CalculateConfigurationData ();

            instancedModels = new Dictionary<long, InstancedModelContainer> ();
            instancedIDFailures = new List<long> ();
            instancedModelFallback = new InstancedMeshRenderer
            {
                mesh = PrimitiveHelper.GetPrimitiveMesh (PrimitiveType.Cube),
                material = PrimitiveHelper.GetDefaultMaterial (),
                instanceLimit = DataLinkerRendering.data.defaultComputeBufferSize,
                subMesh = 0,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true
            };

            lightDefinitions = new Dictionary<long, AreaTilesetLight> ();

            string tilesetTerrainExcepted = "tileset_terrain";
            string pathToTilesetFolder = "Content/Objects/Tilesets";

            ResourceDatabaseContainer resourceDatabase = ResourceDatabaseManager.GetDatabase ();
            if (!ResourceDatabaseManager.IsDatabaseAvailable ())
                return;

            foreach (KeyValuePair<int, AreaTileset> kvp in database.tilesets)
            {
                AreaTileset tileset = kvp.Value;
                if (tileset.name == tilesetTerrainExcepted)
                    continue;

                string pathToTileset = pathToTilesetFolder + "/" + tileset.name;
                if (!resourceDatabase.entries.ContainsKey (pathToTileset))
                {
                    Debug.LogWarning ("ATH | LoadDatabase | Path not found in resource DB: " + pathToTileset);
                    continue;
                }

                // After we retrieve the entry, we do looped search for each tileset sub-asset type separately in a specific order, since some types can depend on others being fully loaded
                ResourceDatabaseEntryRuntime entryForTileset = ResourceDatabaseManager.GetEntryByPath (pathToTileset);
                List<ResourceDatabaseEntryRuntime> entriesForFolders = entryForTileset.GetChildrenOfType (ResourceDatabaseEntrySerialized.Filetype.Folder);
                List<ResourceDatabaseEntryRuntime> entriesForAssets = entryForTileset.GetChildrenOfType (ResourceDatabaseEntrySerialized.Filetype.Asset);

                bool unifiedMaterialDataFound = false;
                for (int i = 0; i < entriesForAssets.Count; ++i)
                {
                    var entry = entriesForAssets[i];
                    if (!string.Equals (entry.name, "materialData"))
                        continue;

                    var data = entry.GetContent<MaterialSerializationHelper> ();
                    if (data == null)
                    {
                        Debug.Log ("Failed to load data even though the name matches");
                        continue;
                    }

                    unifiedMaterialDataFound = true;
                    data.ApplyToTarget ();

                    if (materialHelpers == null)
                        materialHelpers = new List<MaterialSerializationHelper> ();
                    materialHelpers.Add (data);
                    // Debug.Log ("Found unified material data for tileset " + entryForTileset.name);
                }

                for (int c = 0; c < entriesForFolders.Count; ++c)
                {
                    ResourceDatabaseEntryRuntime entry = entriesForFolders[c];
                    if (string.Equals (entry.name, "blocks"))
                        LoadTilesetBlocks (tileset, entry, unifiedMaterialDataFound);
                }
            }

            navOverridesByID = new Dictionary<long, AreaDataNavOverride> ();
            foreach (var kvp in database.tilesets)
            {
                var tileset = kvp.Value;
                if (tileset.navOverrides == null)
                    continue;

                foreach (var navOverrideSource in tileset.navOverrides)
                {
                    var id = GetInstancedModelID
                    (
                        tileset.id,
                        AreaTilesetHelper.assetFamilyBlock,
                        navOverrideSource.config,
                        navOverrideSource.group,
                        navOverrideSource.subtype,
                        true
                    );

                    // Debug.Log ($"Adding nav override {id} for tileset {tileset.name} config {navOverrideSource.config}/group {navOverrideSource.group}/subtype {navOverrideSource.subtype}");
                    if (!navOverridesByID.ContainsKey (id))
                        navOverridesByID.Add (id, new AreaDataNavOverride { pivotIndex = 0, offsetY = navOverrideSource.offsetY });
                }
            }

            // if (Application.isPlaying)
            //     Debug.Log (string.Format ("ATH | Load | Models registered: {0} | Failed: {1}", instancedModels.Count, instancedIDFailures.Count));
        }



        public static Dictionary<byte, byte> configurationCollapseMap;

        public static void CalculateConfigurationData ()
        {
            // Debug.Log ("AreaTilesetHelper | Calculate configurations | Starting the process | Ordered configuration list is sized at: " + configurationOrder.Length);
            database.configurationDataForBlocks = new AreaConfigurationData[256];
            configurationCollapseMap = new Dictionary<byte, byte> (256);

            for (var i = 0; i < configurationOrder.Length; ++i)
            {
                var configurationAsByte = configurationOrder[i];
                var customRotationPossible = TilesetUtility.IsConfigurationRotationPossible (configurationAsByte);
                var customFlippingMode = TilesetUtility.GetConfigurationFlippingAxis (configurationAsByte);

                // Now we need to do 8 transformations of the configuration
                // Essentially, we're creating a lookup table for all 256 configurations possible to encounter
                // (instead of doing an expensive fitting step to determine which of the 57 blocks fits and how it should be rotated/flipped on every spot operation)

                for (var r = 0; r < 8; ++r)
                {
                    // Easy way to get a sequence of 4 non-flipped and 4 flipped transformations
                    var requiredRotation = r % 4;
                    var requiredFlipping = r > 3;

                    // Key is configuration as integer (along the lines of "01011000"), and there are 256 possible configurations
                    // We get it by transforming the configuration through 8 possible scenarios (4 rotations x 2 scale states)
                    var configurationTransformed = TilesetUtility.GetConfigurationTransformed (configurationAsByte, requiredRotation, requiredFlipping);
                    // I have absolutely no idea why it's necessary to overwrite already existing results, but not doing so breaks all rotations on horizontal planes
                    var data = database.configurationDataForBlocks[configurationTransformed];
                    if (data == null)
                    {
                        data = new AreaConfigurationData ();
                        database.configurationDataForBlocks[configurationTransformed] = data;
                    }

                    data.configuration = configurationTransformed;
                    data.configurationAsString = TilesetUtility.GetStringFromConfiguration (data.configuration);
                    data.requiredRotation = requiredRotation;
                    data.requiredFlippingZ = requiredFlipping;
                    data.customRotationPossible = customRotationPossible;
                    data.customFlippingMode = customFlippingMode;

                    if (!configurationCollapseMap.ContainsKey (configurationTransformed))
                    {
                        configurationCollapseMap.Add (configurationTransformed, configurationAsByte);
                    }
                }
            }
        }

        public static void LoadTilesetBlocks (AreaTileset tileset, ResourceDatabaseEntryRuntime folderInfo, bool unifiedMaterialDataFound)
        {
            // First we reset the BlockDefinitions array - and we know for sure that will need the length of 256, since that's the number of possible configurations for a 2x2x2 bool array (or a byte)
            tileset.blocks = new AreaBlockDefinition[256];

            var fileInfoPrefabs = folderInfo.GetChildrenOfType (ResourceDatabaseEntrySerialized.Filetype.Prefab);

            var materialDataContainerFound = false;
            for (var i = 0; i < fileInfoPrefabs.Count; ++i)
            {
                var entry = fileInfoPrefabs[i];
                var prefab = entry.GetContent<GameObject> ();
                if (prefab == null)
                {
                    continue;
                }

                if (!unifiedMaterialDataFound)
                {
                    //bool isMaterialDataContainer = false;
                    if (!materialDataContainerFound)
                    {
                        if (string.Equals (prefab.name, "materialDataContainer"))
                        {
                            materialDataContainerFound = true;
                            //isMaterialDataContainer = true;

                            if (log)
                            {
                                Debug.Log ("ATH | LoadTilesetBlocks | Successfully found the block material data container prefab for tileset " + tileset.name);
                            }

                            var dataContainer = prefab.GetComponent<AreaTilesetContainer> ();
                            if (dataContainer != null)
                            {
                                // Debug.Log ("ATH | LoadTilesetBlocks | Successfully retrieved the data container component");
                                if (dataContainer.materialWithArrays != null && dataContainer.propertyArray != null)
                                {
                                    dataContainer.materialWithArrays.SetVectorArray (Shader.PropertyToID ("_PropertyArray"), dataContainer.propertyArray);
                                    if (log)
                                    {
                                        Debug.Log ("ATH | LoadTilesetBlocks | Successfully retrieved the array-based material and property vectors");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning ("ATH | LoadTilesetBlocks | Failed to retrieve array-based material and/or property vectors for tileset " + tileset.name);
                                }
                            }
                            else
                            {
                                Debug.LogWarning ("ATH | LoadTilesetBlocks | Failed to retrieve the data container component for tileset " + tileset.name);
                            }
                        }
                    }
                }

                var attributes = entry.name.Split ('_');

                // Filename can contain anything in the end, but must start with two blocks - configuration and subtype index (e.g. "01011000_1_window.fbx")

                if (attributes.Length == 2)
                {
                    attributes = new[] { attributes[0], attributes[1], "0" };
                }

                if (attributes.Length < 3)
                {
                    //var attributesDebug = "\"" + string.Join("\", \"", attributes) + ", \"\"";
                    // Debug.LogWarning ("ATH | LoadTilesetBlocks | Tileset: " + tileset.name + " | Filename " + fileInfoPrefab.name + " is incorrectly formatted and can't be parsed, check your tileset export code | Attributes found: " + attributesDebug);
                    continue;
                }

                var group = byte.Parse (attributes[1]);
                var subtype = byte.Parse (attributes[2]);

                // Light suffix
                AreaTilesetLight lightData = null;
                if (attributes.Length >= 4)
                {
                    var lightInfoString = attributes[3];
                    if (lightInfoString.StartsWith ("l"))
                    {
                        var lightIntensity = 1f;
                        var lightOffset = 1f;
                        if (lightInfoString.Length > 1)
                        {
                            var lightInfoStringSplit = lightInfoString.Substring (1, lightInfoString.Length - 1).Split ('-');
                            if (lightInfoStringSplit.Length >= 1 && float.TryParse (lightInfoStringSplit[0], out var lightOffsetParsed))
                            {
                                lightOffset = lightOffsetParsed;
                            }
                            if (lightInfoStringSplit.Length >= 2 && float.TryParse (lightInfoStringSplit[1], out var lightIntensityParsed))
                            {
                                lightIntensity = lightIntensityParsed;
                            }
                        }

                        lightData = new AreaTilesetLight { intensity = lightIntensity, offset = lightOffset };
                        // Debug.Log ($"Light data found: {entry.name} | Offset / intensity: {lightOffset} / {lightIntensity}");
                    }
                }

                // Now that we're sure filename follows the format we need, we extract the properties

                var configuration = TilesetUtility.GetConfigurationFromString (attributes[0]);
                if (log)
                {
                    var customRotationPossible = TilesetUtility.IsConfigurationRotationPossible (configuration);
                    var customFlippingMode = TilesetUtility.GetConfigurationFlippingAxis (configuration);
                    Debug.LogFormat
                    (
                        "ATH | LoadTilesetBlocks | Tileset: {0} | Configuration: {1} | Rotation possible: {2} | Flipping mode: {3}",
                        tileset.name,
                        attributes[0],
                        customRotationPossible,
                        customFlippingMode
                    );
                }

                // Instanced rendering setup
                RegisterInstancedModel (prefab, tileset, assetFamilyBlock, configuration, group, subtype, true, lightData);

                // Now we need to do 8 transformations of the configuration from the filename
                // Essentially, we're creating a lookup table for all 256 configurations possible to encounter
                // (instead of doing an expensive fitting step to determine which of the 57 blocks fits and how it should be rotated/flipped on every spot operation)

                for (var r = 0; r < 8; ++r)
                {
                    // Easy way to get a sequence of 4 non-flipped and 4 flipped transformations
                    var requiredRotation = r % 4;
                    var requiredFlipping = r > 3;

                    // Key is configuration as integer (along the lines of "01011000"), and there are 256 possible configurations
                    // We get it by transforming the configuration through 8 possible scenarios (4 rotations x 2 scale states)
                    var configurationBasedIndex = TilesetUtility.GetConfigurationTransformed (configuration, requiredRotation, requiredFlipping);
                    var block = tileset.blocks[configurationBasedIndex];
                    if (block == null)
                    {
                        // Time to create block definition and add it to the dictionary
                        block = new AreaBlockDefinition ();
                        tileset.blocks[configurationBasedIndex] = block;
                    }

                    // Simple stuff with the subtype array - it has to fit the index we want to write to, and indexes
                    // must be enforced since we don't know the order of files

                    if (block.subtypeGroups == null)
                    {
                        block.subtypeGroups = new SortedDictionary<byte, SortedDictionary<byte, GameObject>> ();
                    }

                    if (!block.subtypeGroups.ContainsKey (group))
                    {
                        block.subtypeGroups.Add (group, new SortedDictionary<byte, GameObject> ());
                    }

                    if (!block.subtypeGroups[group].ContainsKey (subtype))
                    {
                        block.subtypeGroups[group].Add (subtype, prefab);
                    }
                }
            }
        }

        public static void SaveDatabase ()
        {
            if (database != null)
            {
                databaseSerialized = new AreaTilesetDatabaseSerialized (database);
                UtilitiesYAML.SaveDataToFile (configPath, configName, databaseSerialized);
            }
        }

        static readonly List<int> tilesetKeys = new List<int> ();

        public static int OffsetBlockTileset (int tilesetKeyCurrent, bool forward)
        {
            if (database.tilesets.Count == 1)
            {
                // Debug.Log ("ATH | OffsetBlockTileset | No point in offsetting, tileset count is just 1, returning fallback key");
                return database.tilesetFallback.id;
            }

            if (!database.tilesets.ContainsKey (tilesetKeyCurrent))
            {
                Debug.Log ("ATH | OffsetBlockTileset | Tileset " + tilesetKeyCurrent + " not found, resetting current tileset key to fallback");
                tilesetKeyCurrent = database.tilesetFallback.id;
            }

            tilesetKeys.Clear ();
            tilesetKeys.AddRange (database.tilesets.Keys);
            for (var i = tilesetKeys.Count - 1; i >= 0; i -= 1)
            {
                var tileset = database.tilesets[tilesetKeys[i]];
                if (tileset.usedAsInterior)
                {
                    tilesetKeys.RemoveAt (i);
                }
            }

            var tilesetIndexCurrent = tilesetKeys.IndexOf (tilesetKeyCurrent);
            var tilesetIndexOffset = tilesetIndexCurrent.OffsetAndWrap (forward, 0, tilesetKeys.Count - 1);
            var tilesetKeyNew = tilesetKeys[tilesetIndexOffset];

            // Debug.Log ("ATH | OffsetBlockTileset | Current key: " + tilesetKeyCurrent + " | Keys: " + tilesetKeys.Count + " | Index of current key: " + tilesetIndexCurrent + " | Index offset: " + tilesetIndexOffset + " | New key: " + tilesetKeyNew);

            return tilesetKeyNew;
        }

        static readonly List<byte> r_OBG_groupKeys = new List<byte> ();

        public static byte OffsetBlockGroup (AreaBlockDefinition definition, byte groupKeyCurrent, bool forward)
        {
            if (definition.subtypeGroups.Count == 1)
            {
                // Debug.Log ("ATH | OffsetBlockGroup | No point in offsetting, group count is just 1, returning index 0");
                return 0;
            }

            if (!definition.subtypeGroups.ContainsKey (groupKeyCurrent))
            {
                Debug.Log ("ATH | OffsetBlockGroup | Group " + groupKeyCurrent + " not found, resetting current group key to 0");
                groupKeyCurrent = 0;
            }

            r_OBG_groupKeys.Clear ();
            r_OBG_groupKeys.AddRange (definition.subtypeGroups.Keys);
            var groupIndexCurrent = r_OBG_groupKeys.IndexOf (groupKeyCurrent);
            var groupIndexOffset = groupIndexCurrent.OffsetAndWrap (forward, 0, r_OBG_groupKeys.Count - 1);
            var groupKeyNew = r_OBG_groupKeys[groupIndexOffset];

            /*
            Debug.Log
            (
                "ATH | OffsetBlockTileset | Current key: " + groupKeyCurrent +
                " | Keys: " + r_OBG_groupKeys.Count +
                " | Index of current key: " + groupIndexCurrent +
                " | Index offset: " + groupIndexOffset +
                " | New key: " + groupKeyNew
            );
            */

            return groupKeyNew;
        }

        static readonly List<byte> r_OBS_subtypeKeys = new List<byte> ();

        public static byte OffsetBlockSubtype (AreaBlockDefinition definition, byte groupKeyCurrent, byte subtypeKeyCurrent, bool forward)
        {
            if (definition == null || definition.subtypeGroups == null)
            {
                return 0;
            }
            if (!definition.subtypeGroups.ContainsKey (groupKeyCurrent))
            {
                // Debug.Log ("AreaTilesetHelper | OffsetBlockSubtype | Current group key could not be found, switching to 0");
                return 0;
            }

            var subtypes = definition.subtypeGroups[groupKeyCurrent];
            if (subtypes.Count == 1)
            {
                // Debug.Log ("AreaTilesetHelper | OffsetBlockSubtype | No point in offsetting, subtype count is just 1, returning index 0");
                return 0;
            }

            if (!subtypes.ContainsKey (subtypeKeyCurrent))
            {
                Debug.Log ("AreaTilesetHelper | OffsetBlockSubtype | Subtype " + subtypeKeyCurrent + " not found, resetting current subtype key to 0");
                subtypeKeyCurrent = 0;
            }

            r_OBS_subtypeKeys.Clear ();
            r_OBS_subtypeKeys.AddRange (subtypes.Keys);
            var subtypeIndexCurrent = r_OBS_subtypeKeys.IndexOf (subtypeKeyCurrent);
            var subtypeIndexOffset = subtypeIndexCurrent.OffsetAndWrap (forward, 0, r_OBS_subtypeKeys.Count - 1);
            return r_OBS_subtypeKeys[subtypeIndexOffset];
        }

        #if UNITY_EDITOR
        public static byte EnsureSubtypeInGroup (AreaBlockDefinition definition, byte groupKeyCurrent, byte subtypeKeyCurrent)
        {
            if (definition == null || definition.subtypeGroups == null)
            {
                return 0;
            }
            if (!definition.subtypeGroups.ContainsKey (groupKeyCurrent))
            {
                return 0;
            }

            var subtypes = definition.subtypeGroups[groupKeyCurrent];
            if (subtypes.Count == 1)
            {
                return subtypes.Keys.OrderBy(k => k).First ();
            }
            return subtypes.ContainsKey (subtypeKeyCurrent) ? subtypeKeyCurrent : subtypes.Keys.OrderBy(k => k).First ();
        }
        #endif

        [System.Serializable]
        public class InstancedModelContainer
        {
            public InstancedMeshRenderer renderer;
            public long id;
            public AreaTileset tileset;
            public int tilesetID;
            public byte family;
            public byte configuration;
            public byte group;
            public byte subtype;
            public bool verticalFlip;
        }

        private static void RegisterInstancedModel
        (
            GameObject prefab,
            AreaTileset tileset,
            byte family,
            byte configuration,
            byte group,
            byte subtype,
            bool collapse,
            AreaTilesetLight lightData
        )
        {
            if (prefab == null)
                return;

            MeshRenderer mr = prefab.GetComponent<MeshRenderer> ();
            MeshFilter mf = prefab.GetComponent<MeshFilter> ();

            if (mr == null || mf == null)
            {
                Debug.LogWarning (string.Format
                (
                    "ATH | RegisterInstancedModel | Failed to find renderer ({0}) or filter ({1}) on object ({2})",
                    mr.ToStringNullCheck (), mf.ToStringNullCheck (), prefab.name
                ), prefab);
            }

            long instancedModelID = GetInstancedModelID (tileset.id, family, configuration, group, subtype, collapse);
            if (!instancedModels.ContainsKey (instancedModelID))
            {
                //Since we can resize the buffer now, we can initialize everything to the default instance limit and let it resize automatically
                //int limit = configuration == AreaNavUtility.configFloor ? 3072 * 4 : 3072;
                //If we find that production levels require multiple resizes, we can balance the increment or initialize the floor tiles as a multiple of the default limit, clamped to the absolute upper limit - AJ

                InstancedMeshRenderer instancedModel = new InstancedMeshRenderer
                {
                    mesh = mf != null ? mf.sharedMesh : null,
                    material = mr != null ? mr.sharedMaterial : null,
                    instanceLimit = DataLinkerRendering.data.defaultComputeBufferSize,
                    subMesh = 0,
                    castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                    receiveShadows = true,
                    id = instancedModelID
                };

                InstancedModelContainer imc = new InstancedModelContainer ();
                imc.renderer = instancedModel;
                imc.tileset = tileset;
                imc.tilesetID = tileset.id;
                imc.family = family;
                imc.configuration = configuration;
                imc.group = group;
                imc.subtype = subtype;

                instancedModels.Add (instancedModelID, imc);
            }
            else
            {
                Debug.LogWarning
                (
                    string.Format ("ATH | RegisterInstancedModel | Hash collision: {0} | Tileset: {1} | Family: {2} | Config: {3} | Group: {4} | Subtype: {5}",
                    instancedModelID, tileset.id, family, configuration, group, subtype)
                );
            }

            if (lightData != null)
            {
                if (!lightDefinitions.ContainsKey (instancedModelID))
                    lightDefinitions[instancedModelID] = lightData;
            }
        }

        private static long GetInstancedModelID
        (
            int tileset,
            byte family,
            byte configuration,
            byte group,
            byte subtype,
            bool configurationCollapse
        )
        {
            PackIntegerAndBytesToLong pack = new PackIntegerAndBytesToLong
            {
                integer = tileset,
                byte0 = family,
                byte1 = configurationCollapse ? configurationCollapseMap[configuration] : configuration,
                byte2 = group,
                byte3 = subtype
            };

            long instanceRendererID = pack.result;
            return instanceRendererID;
        }

        public static bool GetNavOverrideData
        (
            int tileset,
            byte family,
            byte configuration,
            byte group,
            byte subtype,
            out AreaDataNavOverride navOverride
        )
        {
            navOverride = default;
            var id = GetInstancedModelID (tileset, family, configuration, group, subtype, true);
            if (!navOverridesByID.ContainsKey (id))
                return false;
            else
            {
                navOverride = navOverridesByID[id];
                return true;
            }
        }

        public static bool IsNavigationAllowed
        (
            int tileset,
            byte group,
            byte subtype
        )
        {
            var tilesetData = GetTileset (tileset);
            if (tilesetData == null)
                return false;

            if (tilesetData.navRestrictions == null)
                return true;

            if (!tilesetData.navRestrictions.ContainsKey (group))
                return true;

            var restrictions = tilesetData.navRestrictions[group];

            // If we found the group-keyed restriction entry but there are no specified subtypes then whole group is off limits
            if (restrictions.subtypes == null || restrictions.subtypes.Count == 0)
                return false;

            return !restrictions.subtypes.Contains (subtype);
        }

        public static InstancedMeshRenderer GetInstancedModel
        (
            int tileset,
            byte family,
            byte configuration,
            byte group,
            byte subtype,
            bool checkFailures,
            bool configurationCollapse,
            out bool invalidVariantDetected,
            out bool verticalFlip,
            out AreaTilesetLight lightData
        )
        {
            lightData = null;
            verticalFlip = false;
            var renderer = instancedModelFallback;

            if (tileset == 0)
            {
                // Debug.LogWarning ($"Detected bad tileset ID (0), recovered by using fallback ID {AreaTilesetHelper.idOfFallback}");
                tileset = AreaTilesetHelper.idOfFallback;
                group = 0;
                subtype = 0;
            }

            invalidVariantDetected = false;
            var configurationProcessed = configurationCollapse ? configurationCollapseMap[configuration] : configuration;
            var id = GetInstancedModelID (tileset, family, configurationProcessed, group, subtype, false);
            if (!instancedModels.ContainsKey (id))
            {
                // There is a benign scenario where this might happen - invalid group or subtype ID on a given point; so we'll try a 0-0 variant before bailing
                id = GetInstancedModelID (tileset, family, configurationProcessed, 0, 0, false);
                if (!instancedModels.ContainsKey (id))
                {
                    if (!instancedIDFailures.Contains (id))
                    {
                        instancedIDFailures.Add (id);
                        Debug.LogWarningFormat
                        (
                            "Failed to find mesh ID {0} | Tileset: {1} | Family {2} | Original config: {3} ({4}) | Collapsed config: {5} ({6}) | Group/subtype: {7}/{8} | Total fails: {9}",
                            id, tileset, family,
                            configuration, TilesetUtility.GetStringFromConfiguration (configuration),
                            configurationProcessed, TilesetUtility.GetStringFromConfiguration (configurationProcessed),
                            group, subtype, instancedIDFailures.Count
                        );
                    }

                    if (tileset == 101)
                    {
                        renderer = GetInstancedModel (11, family, configuration, group, subtype, checkFailures, configurationCollapse, out invalidVariantDetected, out verticalFlip, out lightData);
                    }
                    else
                    {
                        renderer = instancedModelFallback;
                        verticalFlip = false;
                    }
                }
                else
                {
                    invalidVariantDetected = true;
                    var container = instancedModels[id];
                    renderer = container.renderer;
                    verticalFlip = container.verticalFlip;
                }
            }
            else
            {
                var container = instancedModels[id];
                renderer = container.renderer;
                verticalFlip = container.verticalFlip;
            }

            if (renderer.mesh == null || renderer.material == null)
            {
                Debug.LogWarningFormat
                (
                    "Detected null mesh ({0}) or material ({1}), swapping for fallback | Fallback mesh {2}, material {3} | Input: {4} / {5}_{6}_{7}",
                    renderer.mesh == null,
                    renderer.material == null,
                    instancedModelFallback.mesh.ToStringNullCheck (),
                    instancedModelFallback.material.ToStringNullCheck (),
                    tileset,
                    configuration,
                    group,
                    subtype
                );
                renderer = instancedModelFallback;
                verticalFlip = false;
            }

            var lightDataFound = lightDefinitions.TryGetValue (id, out lightData);

            return renderer;
        }
    }
}
