using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Area
{
    [ExecuteInEditMode]
    public class TilesetGenerator : MonoBehaviour
    {
#if UNITY_EDITOR

        [Serializable]
        public class LightSourceDefinition
        {
            public string blockName = string.Empty;
            public Vector3 localPosition = Vector3.zero;
            public Vector3 localRotation = Vector3.zero;
            public Color color = Color.white;
            public float range = 3f;
            public float intensity = 1f;
            public LightType type = LightType.Point;
            public float angle = 90f;
        }

        [Serializable]
        public class PrefabPreferences
        {
            public GameObject prefab = null;
            public bool load = true;

            public PrefabPreferences (GameObject prefab, bool load)
            {
                this.prefab = prefab;
                this.load = load;
            }
        }

        public enum FileGenerationMode
        {
            Blocks = 0,
            DamageEdges = 1,
            Multiblocks = 2
        }

        public List<byte> configurationsByte = new List<byte> ();
        public string folderTilesetImport;
        public string folderTilesetExport;
        public float exportProgress = 0f;
        // [HideInInspector] public TilesetVertexProperties vertexPropertiesDefault = new TilesetVertexProperties (0.1f, 0.75f, 0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);

        public bool exportBlocks = false;
        public bool exportDamage = false;
        public bool exportMultiblocks = false;
        public bool logMaterialReplacement = false;

        public List<PrefabPreferences> assetsBlocks = new List<PrefabPreferences> ();
        public List<PrefabPreferences> assetsDamage = new List<PrefabPreferences> ();
        public List<PrefabPreferences> assetsMultiblocks = new List<PrefabPreferences> ();

        public List<AssetProcessorMaterialReplacement> materialReplacements = new List<AssetProcessorMaterialReplacement> ();

        public Transform lightSourceHelper;
        public List<LightSourceDefinition> lightSourceDefinitions = new List<LightSourceDefinition> ();
        public Material materialReplacementEmpty = null;
        public Material materialTemplateForBlocks;
        public Material materialTemplateForMultiblocks;

        public static string texArraySourceShaderName = "Hardsurface/Environment/Block (standard)";
        public static string texArrayMaterialShaderName = "Hardsurface/Environment/Block (array-based)";



        private void Awake ()
        {
            InitialLoad ();
        }

        private void OnEnable ()
        {
            InitialLoad ();
        }

        private void InitialLoad ()
        {
            if (Utilities.isPlaymodeChanging)
                return;

            if (configurationsByte == null || configurationsByte.Count == 0)
            {
                GenerateConfigurations ();
            }

            if (assetsBlocks == null || assetsBlocks.Count == 0)
            {
                LoadAssets ();
            }
        }

        private void OnDestroy ()
        {
            DestroyImmediate (GetHolderTilesetsCells ().gameObject);
            DestroyImmediate (GetHolderTilesetsMergedBlocks ().gameObject);
            DestroyImmediate (GetHolderTilesetsMergedDamage ().gameObject);
            DestroyImmediate (GetHolderTilesetsMergedMultiBlocks ().gameObject);
            DestroyImmediate (GetHolderTilesetsSplitBlocks ().gameObject);
            DestroyImmediate (GetHolderTilesetsSplitDamage ().gameObject);
            DestroyImmediate (GetHolderTilesetsSplitMultiBlocks ().gameObject);
        }

        public void RebuildEverything ()
        {
            if (lightSourceHelper != null && lightSourceHelper.parent != null)
                lightSourceHelper.parent = null;

            InstantiateAndCheckModels ();
            ReplaceMaterials ();
            MergeEverything ();
            GenerateLights ();
            SaveEverything ();
            ResourceDatabaseManager.RebuildDatabase ();
        }



        // Generation
        public void GenerateConfigurations ()
        {
            configurationsByte.Clear ();

            // The number of results is predictable, it's 2^N - in our case we need 2x2x2 3d group of objects, so N is 8
            int loopCount = (int)Math.Pow (2, 8);

            // All we need is to fill them - to do that, we convert every integer to binary and then to a bool list
            for (int i = 0; i < loopCount; ++i)
            {
                configurationsByte.Add ((byte)i);
            }

            // Since we have to transform every configuration with rotation and flipping to determine whether it has duplicates, we can't do removal in a simple back-ordered for loop and a temporary index list is required
            List<int> unnecessaryIndexes = new List<int> ();
            for (int i = 0; i < configurationsByte.Count; ++i)
            {
                // Since configuration might have already been marked for removal, we need to check for that, as that will mean that transforming and checking it is pointless - another one yielding exactly the same transformations was already passed
                if (unnecessaryIndexes.Contains (i))
                {
                    continue;
                }

                // This gives us 8 transformations - 4 horizontal rotations, and 4 flipped horizontal rotations of our 2x2x2 true/false "cube"
                List<byte> transformations = TilesetUtility.GetConfigurationTransformations (configurationsByte[i]);
                for (int t = 0; t < transformations.Count; ++t)
                {
                    for (int c = 0; c < configurationsByte.Count; ++c)
                    {
                        // Since this is the second loop through the configuration collection, we have so avoid a case where a configuration is removed after a check against itself. We also do another check for unnecessary indexes list
                        if (c == i || unnecessaryIndexes.Contains (c))
                        {
                            continue;
                        }

                        // If there is a match, we don't need one of those configurations
                        if (configurationsByte[c] == transformations[t])
                        {
                            unnecessaryIndexes.Add (c);
                        }
                    }
                }
            }

            // This ensures that we are removing unnecessary indexes in the right order to keep every index valid as we progress through the removals
            unnecessaryIndexes.Sort ();
            for (int i = unnecessaryIndexes.Count - 1; i >= 0; i--)
            {
                configurationsByte.RemoveAt (unnecessaryIndexes[i]);
            }
        }

        [ContextMenu ("Enumerate replacements")]
        private void EnumerateReplacements ()
        {
            var list = new List<AssetProcessorMaterialReplacement> ();
            for (int i = 0; i < materialReplacements.Count; ++i)
            {
                var replacement = materialReplacements[i];
                list.Add (replacement);
            }

            list.Sort (delegate (AssetProcessorMaterialReplacement i1, AssetProcessorMaterialReplacement i2)
            { return i1.textureName.CompareTo (i2.textureName); });

            for (int i = 0; i < materialReplacements.Count; ++i)
            {
                var replacement = list[i];
                if (replacement.replacementMaterial != null)
                    Debug.Log (i + " | Filter: " + replacement.textureName + "\nPath: " + AssetDatabase.GetAssetPath (replacement.replacementMaterial), replacement.replacementMaterial);
            }
        }

        // Tileset loading

        public void LoadAssets ()
        {
            // First we import all our subtype meshes
            if (folderTilesetImport.EndsWith ("/")) folderTilesetImport = folderTilesetImport.TrimEnd ('/');
            if (!AssetDatabase.IsValidFolder (folderTilesetImport))
                return;

            Debug.Log ("TG | LoadAssets | Folder is valid");

            List<PrefabPreferences> assetsBlocksNew = new List<PrefabPreferences> ();
            List<PrefabPreferences> assetsDamageNew = new List<PrefabPreferences> ();
            List<PrefabPreferences> assetsMultiblocksNew = new List<PrefabPreferences> ();

            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo (folderTilesetImport);
            System.IO.FileInfo[] filesSKP = dirInfo.GetFiles ("*.skp");
            System.IO.FileInfo[] filesFBX = dirInfo.GetFiles ("*.fbx");

            List<System.IO.FileInfo> files = new List<System.IO.FileInfo> ();
            for (int i = 0; i < filesSKP.Length; ++i)
                files.Add (filesSKP[i]);
            for (int i = 0; i < filesFBX.Length; ++i)
                files.Add (filesFBX[i]);

            for (int i = 0; i < files.Count; ++i)
            {
                System.IO.FileInfo fileInfo = files[i];
                string fullPath = fileInfo.FullName.Replace (@"\", "/");
                string assetPath = "Assets" + fullPath.Replace (Application.dataPath, "");
                string[] fileNameSplit = fileInfo.Name.Substring (0, fileInfo.Name.Length - 4).Split ('_');

                bool fileContainsBlocks = string.Equals (fileNameSplit[fileNameSplit.Length - 1], "blocks");
                bool fileContainsDamage = string.Equals (fileNameSplit[fileNameSplit.Length - 1], "damage");
                bool fileContainsMultiBlocks = string.Equals (fileNameSplit[fileNameSplit.Length - 1], "multiblocks");

                Debug.Log ("TG | LoadAssets | Handling file " + fullPath + " | Suffix: " + fileNameSplit[fileNameSplit.Length - 1]);

                if (fileNameSplit.Length < 2 || (!fileContainsBlocks && !fileContainsDamage && !fileContainsMultiBlocks))
                    continue;

                Debug.Log (assetPath);
                GameObject asset = AssetDatabase.LoadAssetAtPath (assetPath, typeof (GameObject)) as GameObject;
                if (asset == null)
                    continue;

                if (fileContainsBlocks)
                    assetsBlocksNew.Add (new PrefabPreferences (asset, true));
                else if (fileContainsDamage)
                    assetsDamageNew.Add (new PrefabPreferences (asset, true));
                else
                    assetsMultiblocksNew.Add (new PrefabPreferences (asset, true));
            }

            // Sorting them just in case
            assetsBlocksNew.Sort (delegate (PrefabPreferences i1, PrefabPreferences i2) { return i1.prefab.name.CompareTo (i2.prefab.name); });
            assetsDamageNew.Sort (delegate (PrefabPreferences i1, PrefabPreferences i2) { return i1.prefab.name.CompareTo (i2.prefab.name); });
            assetsMultiblocksNew.Sort (delegate (PrefabPreferences i1, PrefabPreferences i2) { return i1.prefab.name.CompareTo (i2.prefab.name); });
            Debug.Log ("TG | LoadAssets | Tileset files imported (blocks/damage/multiblock sets): " + assetsBlocksNew.Count + " / " + assetsDamageNew.Count + " / " + assetsMultiblocksNew.Count);

            if (assetsBlocksNew.Count == assetsBlocks.Count)
            {
                for (int i = 0; i < assetsBlocks.Count; ++i)
                    assetsBlocksNew[i].load = assetsBlocks[i].load;
            }

            if (assetsDamageNew.Count == assetsDamage.Count)
            {
                for (int i = 0; i < assetsDamage.Count; ++i)
                    assetsDamageNew[i].load = assetsDamage[i].load;
            }

            if (assetsMultiblocksNew.Count == assetsMultiblocks.Count)
            {
                for (int i = 0; i < assetsMultiblocks.Count; ++i)
                    assetsMultiblocksNew[i].load = assetsMultiblocks[i].load;
            }

            assetsBlocks = assetsBlocksNew;
            assetsDamage = assetsDamageNew;
            assetsMultiblocks = assetsMultiblocksNew;
        }

        public void InstantiateAndCheckModels ()
        {
            UtilityGameObjects.ClearChildren (GetHolderTilesetsCells ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsSplitBlocks ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsSplitDamage ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsSplitMultiBlocks ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsMergedBlocks ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsMergedDamage ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsMergedMultiBlocks ());

            if (exportBlocks)
            {
                for (int indexOfTileset = 0; indexOfTileset < assetsBlocks.Count; ++indexOfTileset)
                {
                    if (!assetsBlocks[indexOfTileset].load)
                        continue;

                    // We must check whether our tileset holder contains all configurations
                    bool[] tilesetCompletenessChecklist = new bool[configurationsByte.Count];

                    // Keep track
                    GameObject tilesetHolder = new GameObject ();
                    tilesetHolder.transform.parent = GetHolderTilesetsSplitBlocks ();
                    tilesetHolder.transform.localPosition = Vector3.zero;
                    tilesetHolder.transform.localRotation = Quaternion.identity;
                    tilesetHolder.transform.localScale = Vector3.one;

                    // Removing the suffix, leaving only tileset identifier
                    string tilesetAssetName = assetsBlocks[indexOfTileset].prefab.name;
                    string[] tilesetAssetNameSplit = tilesetAssetName.Split ('_');
                    tilesetHolder.name = tilesetAssetName.Substring (0, tilesetAssetName.Length - (tilesetAssetNameSplit[tilesetAssetNameSplit.Length - 1].Length + 1));

                    Debug.Log ("TG | InstantiateAndCheckModels | Loading the tileset holder with block objects: " + tilesetHolder.name);

                    // Since we don't need any meshes for 00000000 and 11111111 configurations, we can immediately mark those as checked
                    tilesetCompletenessChecklist[0] = true;
                    tilesetCompletenessChecklist[tilesetCompletenessChecklist.Length - 1] = true;
                    Dictionary<byte, int> configurationCounter = new Dictionary<byte, int> ();

                    for (int indexOfBlock = 0; indexOfBlock < assetsBlocks[indexOfTileset].prefab.transform.childCount; ++indexOfBlock)
                    {
                        Transform blockSourceTransform = assetsBlocks[indexOfTileset].prefab.transform.GetChild (indexOfBlock);
                        string[] blockNameSplit = blockSourceTransform.name.Split ('_');

                        // If an object was incorrectly named, it will be rooted out at this point
                        int parsedConfiguration = 0;
                        int parsedSubtype = 0;
                        int parsedGroup = 0;

                        if (blockNameSplit.Length == 2)
                        {
                            blockNameSplit = new string[] { blockNameSplit[0], blockNameSplit[1], "0" };
                        }

                        if (blockNameSplit.Length < 3 || blockNameSplit[0].Length != 8 || !int.TryParse (blockNameSplit[0], out parsedConfiguration) || !int.TryParse (blockNameSplit[1], out parsedGroup) || !int.TryParse (blockNameSplit[2], out parsedSubtype))
                        {
                            Debug.LogWarning ("TG | InstantiateAndCheckModels | Skipping the child GameObject number " + indexOfBlock + " due to failed check on the name: " + blockSourceTransform.name);
                            continue;
                        }

                        // Next we need to extract the configuration byte and determine the configuration index
                        int configurationIndex = -1;
                        byte configurationByte = TilesetUtility.GetConfigurationFromString (blockNameSplit[0]);
                        for (int j = 0; j < configurationsByte.Count; ++j)
                        {
                            if (configurationsByte[j] == configurationByte)
                            {
                                configurationIndex = j;
                                tilesetCompletenessChecklist[j] = true;
                            }
                        }

                        // If index is -1, that means we never found a match (which is pretty weird and shouldn't happen when checking proper template-referenced models against a proper configuration list)
                        if (configurationIndex == -1)
                        {
                            Debug.LogWarning ("TG | InstantiateAndCheckModels | Skipping the child GameObject number " + indexOfBlock + " with name " + blockSourceTransform.name + " | Looked for matching byte " + configurationByte.ToString ("000") + " and integer " + TilesetUtility.GetIntFromConfiguration (TilesetUtility.GetConfigurationFromByte (configurationByte)).ToString ("00000000") + "was not found");
                            continue;
                        }

                        // Counter is used to position blocks vertically, like in the source model
                        if (configurationCounter.ContainsKey (configurationByte))
                            configurationCounter[configurationByte] += 1;
                        else
                            configurationCounter.Add (configurationByte, 0);

                        // This will produce an 8x8 grid of blocks spaced evenly and offset by tileset index
                        float blockSpacing = TilesetUtility.blockAssetSize * 1.5f;
                        Vector3 blockPosition = new Vector3
                        (
                            blockSpacing * ((float)configurationIndex % 8f),
                            blockSpacing * (float)configurationCounter[configurationByte],
                            blockSpacing * Mathf.FloorToInt ((float)configurationIndex / 8f)
                        );

                        // Space tilesets on X
                        blockPosition += new Vector3 (indexOfTileset * blockSpacing * 10f, 20f, 0f);

                        GameObject blockHolder = new GameObject ();
                        blockHolder.name = blockNameSplit[0] + "_" + blockNameSplit[1] + "_" + blockNameSplit[2];
                        blockHolder.transform.parent = tilesetHolder.transform;
                        blockHolder.transform.localPosition = blockPosition;
                        blockHolder.transform.localRotation = Quaternion.identity;
                        blockHolder.transform.localScale = Vector3.one;

                        GameObject blockCopy = Instantiate (blockSourceTransform.gameObject);
                        blockCopy.transform.parent = blockHolder.transform;
                        blockCopy.transform.localPosition = Vector3.zero;
                        blockCopy.transform.localRotation = Quaternion.Euler (0f, 180f, 0f);
                        blockCopy.transform.localScale = Vector3.one;

                        GameObject cellPrefab = (GameObject)Resources.Load ("Editor/AreaVisuals/block_cell", typeof (GameObject));
                        if (cellPrefab != null)
                        {
                            GameObject cellObject = Instantiate (cellPrefab);
                            cellObject.hideFlags = HideFlags.HideAndDontSave;
                            cellObject.layer = 15;
                            cellObject.transform.parent = GetHolderTilesetsCells ();
                            cellObject.transform.localScale = Vector3.one;
                            cellObject.transform.position = blockCopy.transform.position;
                        }
                    }

                    // Validation

                    bool valid = true;
                    int missingConfigurationCount = 0;
                    for (int j = 0; j < tilesetCompletenessChecklist.Length; ++j)
                    {
                        if (!tilesetCompletenessChecklist[j])
                        {
                            Debug.LogWarning ("TG | InstantiateAndCheckModels | Configuration index " + j + ", linked to byte " + configurationsByte[j] + ", which can be represented as " + TilesetUtility.GetStringFromConfiguration (TilesetUtility.GetConfigurationFromByte (configurationsByte[j])) + " was not covered by the currently imported tileset!");
                            missingConfigurationCount += 1;
                            valid = false;
                        }
                    }

                    if (!valid)
                    {
                        Debug.LogWarning ("TG | InstantiateAndCheckModels | The tileset is incomplete | Missing configurations: " + missingConfigurationCount);
                        // DestroyImmediate (tilesetHolder);
                    }
                }
            }

            if (exportDamage)
            {
                for (int i = 0; i < assetsDamage.Count; ++i)
                {
                    if (!assetsDamage[i].load)
                        continue;

                    // Keep track
                    GameObject tilesetHolder = new GameObject ();
                    tilesetHolder.transform.parent = GetHolderTilesetsSplitDamage ();
                    tilesetHolder.transform.localPosition = Vector3.zero;
                    tilesetHolder.transform.localRotation = Quaternion.identity;
                    tilesetHolder.transform.localScale = Vector3.one;

                    // Removing the suffix, leaving only tileset identifier
                    string tilesetAssetName = assetsDamage[i].prefab.name;
                    string[] tilesetAssetNameSplit = tilesetAssetName.Split ('_');
                    tilesetHolder.name = tilesetAssetName.Substring (0, tilesetAssetName.Length - (tilesetAssetNameSplit[tilesetAssetNameSplit.Length - 1].Length + 1));

                    Debug.Log ("TG | InstantiateAndCheckModels | Loading the tileset holder with damage objects: " + tilesetHolder.name);

                    for (int b = 0; b < assetsDamage[i].prefab.transform.childCount; ++b)
                    {
                        Transform damageSourceTransform = assetsDamage[i].prefab.transform.GetChild (b);
                        string[] damageNameSplit = damageSourceTransform.name.Split ('_');

                        // If an object was incorrectly named, it will be rooted out at this point
                        int parsedConfigurationCount = 0;
                        int parsedSubtype = 0;

                        if (damageNameSplit.Length < 5 || damageNameSplit[2].Length != 8 || !int.TryParse (damageNameSplit[1], out parsedConfigurationCount) || !int.TryParse (damageNameSplit[parsedConfigurationCount + 2], out parsedSubtype))
                        {
                            Debug.LogWarning ("TG | InstantiateAndCheckModels | Skipping the child GameObject number " + b + " due to failed check on the name: " + damageSourceTransform.name);
                            continue;
                        }

                        // This will produce an 8x8 grid of blocks spaced evenly and offset by tileset index
                        float blockSpacing = TilesetUtility.blockAssetSize * 1.5f;
                        Vector3 damagePosition = new Vector3 (blockSpacing * b - blockSpacing * (int)((float)b / 8f) * 8f, blockSpacing * parsedSubtype, blockSpacing * (int)((float)b / 8f)) + new Vector3 (i * blockSpacing * 10f, 0f, 0f);

                        GameObject damageHolder = new GameObject ();
                        damageHolder.name = damageSourceTransform.name;
                        damageHolder.transform.parent = tilesetHolder.transform;
                        damageHolder.transform.localPosition = damagePosition;
                        damageHolder.transform.localRotation = Quaternion.identity;
                        damageHolder.transform.localScale = Vector3.one;

                        GameObject damageCopy = Instantiate (damageSourceTransform.gameObject);
                        damageCopy.transform.parent = damageHolder.transform;
                        damageCopy.transform.localPosition = Vector3.zero;
                        damageCopy.transform.localRotation = Quaternion.Euler (0f, 180f, 0f);
                        damageCopy.transform.localScale = Vector3.one;
                    }
                }
            }

            if (exportMultiblocks)
            {
                multiblockBounds = new Dictionary<Transform, Bounds> ();
                multiblockReusedMeshData = new List<ReusedMeshData> ();
                int totalElementCount = 0;

                for (int i = 0; i < assetsMultiblocks.Count; ++i)
                {
                    if (!assetsMultiblocks[i].load)
                        continue;

                    Transform multiblockHolderSource = assetsMultiblocks[i].prefab.transform;
                    if (multiblockHolderSource.childCount == 0)
                        continue;

                    Transform multiblockHolderInstance = new GameObject ().transform;
                    multiblockHolderInstance.parent = GetHolderTilesetsSplitMultiBlocks ();
                    multiblockHolderInstance.name = multiblockHolderSource.name.Replace ("_multiblocks", string.Empty);
                    multiblockHolderInstance.SetLocalTransformationToZero ();

                    for (int b = 0; b < multiblockHolderSource.childCount; ++b)
                    {
                        Transform multiblockSource = multiblockHolderSource.GetChild (b);
                        if (multiblockSource.childCount == 0)
                            continue;

                        Transform multiblockInstance = new GameObject ().transform;
                        multiblockInstance.parent = multiblockHolderInstance;
                        multiblockInstance.name = multiblockSource.name;
                        multiblockInstance.SetLocalTransformationToZero ();
                        multiblockInstance.localPosition += new Vector3 (0f, 0f, 9f * b);

                        for (int a = 0; a < multiblockSource.childCount; ++a)
                        {
                            Transform multiblockElementSource = multiblockSource.GetChild (a);
                            Transform multiblockElementInstance = Instantiate (multiblockElementSource.gameObject).transform;

                            multiblockElementInstance.parent = multiblockInstance;
                            multiblockElementInstance.name = multiblockElementSource.name;
                            multiblockElementInstance.localPosition = multiblockElementSource.localPosition;
                            multiblockElementInstance.localRotation = multiblockElementSource.localRotation;
                            multiblockElementInstance.localScale = multiblockElementSource.localScale;

                            totalElementCount += 1;
                            string nameToCheck = multiblockElementInstance.name.Contains (" ") ? multiblockElementInstance.name.Split (' ')[0] : multiblockElementInstance.name;
                            bool found = false;

                            multiblockElementInstance.name = "element_" + a.ToString ("00");

                            for (int x = 0; x < multiblockReusedMeshData.Count; ++x)
                            {
                                ReusedMeshData rmd = multiblockReusedMeshData[x];
                                if (string.Equals (nameToCheck, rmd.nameMask))
                                {
                                    Debug.Log ("TG | InstantiateAndCheckModels | Found a reused component named " + nameToCheck + " matching RMD " + x);
                                    rmd.splitHoldersWithName.Add (multiblockElementInstance);
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                Debug.Log ("TG | InstantiateAndCheckModels | Creating RMD " + multiblockReusedMeshData.Count + " for so far unique component " + nameToCheck);
                                ReusedMeshData rmd = new ReusedMeshData ();
                                rmd.nameMask = nameToCheck;
                                rmd.splitHoldersWithName = new List<Transform> ();
                                rmd.splitHoldersWithName.Add (multiblockElementInstance);
                                multiblockReusedMeshData.Add (rmd);
                            }
                        }

                        Bounds bounds = new Bounds (multiblockInstance.position, Vector3.zero);
                        List<MeshRenderer> renderers = new List<MeshRenderer> (multiblockInstance.GetComponentsInChildren<MeshRenderer> ());
                        for (int r = 0; r < renderers.Count; ++r)
                            bounds.Encapsulate (renderers[r].bounds);

                        bounds.extents *= 2f;
                        bounds.extents = new Vector3 (Mathf.Max (3f, bounds.extents.x), Mathf.Max (3f, bounds.extents.y), Mathf.Max (3f, bounds.extents.z));
                        bounds.center -= multiblockInstance.position;

                        // Debug.Log ("TG | Bounds: " + bounds + " | Renderers: " + renderers.Count);
                        multiblockBounds.Add (multiblockInstance, bounds);

                        /*
                        string[] multiBlockNameSplit = multiBlockSourceTransform.name.Split ('_');
                        string[] multiBlockBoundsSplit = multiBlockNameSplit[0].Split ('x');

                        // If an object was incorrectly named, it will be rooted out at this point
                        int parsedBoundsX = 0;
                        int parsedBoundsY = 0;
                        int parsedBoundsZ = 0;

                        if (multiBlockBoundsSplit.Length < 3 || !int.TryParse (multiBlockBoundsSplit[0], out parsedBoundsX) || !int.TryParse (multiBlockBoundsSplit[1], out parsedBoundsY) || !int.TryParse (multiBlockBoundsSplit[2], out parsedBoundsZ))
                        {
                            Debug.LogWarning ("TG | LoadAssets | Skipping the multi-block candidate GameObject number " + b + " due to failed check on the name: " + multiBlockSourceTransform.name);
                            continue;
                        }

                        Vector3 multiBlockPosition = new Vector3 (positionX, 0f, 0f);
                        positionX += parsedBoundsX * TilesetUtility.blockAssetSize + 1.5f;

                        GameObject multiBlockHolder = new GameObject ();
                        multiBlockHolder.name = multiBlockSourceTransform.name;
                        multiBlockHolder.transform.parent = tilesetHolder.transform;
                        multiBlockHolder.transform.localPosition = multiBlockPosition;
                        multiBlockHolder.transform.localRotation = Quaternion.identity;
                        multiBlockHolder.transform.localScale = Vector3.one;

                        GameObject multiBlockCopy = Instantiate (multiBlockSourceTransform.gameObject);
                        multiBlockCopy.transform.parent = multiBlockHolder.transform;
                        multiBlockCopy.transform.localPosition = Vector3.zero;
                        multiBlockCopy.transform.localRotation = Quaternion.Euler (0f, 180f, 0f);
                        multiBlockCopy.transform.localScale = Vector3.one;
                        */
                    }
                }

                Debug.Log ("TG | InstantiateAndCheckModels | Total element count: " + totalElementCount + " | Total mesh count from reuse: " + multiblockReusedMeshData.Count);
            }
        }

        // If it's already merged, get merged object from library
        // 

        public List<ReusedMeshData> multiblockReusedMeshData;
        public class ReusedMeshData
        {
            public string nameMask;
            public List<Transform> splitHoldersWithName;
            public Mesh mergedMesh;
        }

        public Dictionary<string, List<MeshRenderer>> multiblockMeshMap;
        public Dictionary<Transform, Bounds> multiblockBounds;

        private void OnDrawGizmosSelected ()
        {
            if (multiblockBounds == null)
                return;

            foreach (KeyValuePair<Transform, Bounds> kvp in multiblockBounds)
            {
                if (kvp.Key == null)
                    continue;

                Gizmos.color = new Color (1f, 0.3f, 0.2f, 0.5f);
                Gizmos.DrawCube (kvp.Key.TransformPoint (kvp.Value.center), kvp.Value.extents);
            }
        }

        public void ReplaceMaterials ()
        {
            if (exportBlocks)
                ReplaceMaterialsInChildren (GetHolderTilesetsSplitBlocks ());

            if (exportDamage)
                ReplaceMaterialsInChildren (GetHolderTilesetsSplitDamage ());

            if (exportMultiblocks)
                ReplaceMaterialsInChildren (GetHolderTilesetsSplitMultiBlocks ());
        }




        // Materials

        public void ReplaceMaterialsInChildren (Transform parent)
        {
            ReplaceMateriaInTarget (parent);
            for (int i = 0; i < parent.childCount; ++i)
            {
                Transform child = parent.GetChild (i);
                ReplaceMaterialsInChildren (child);
            }
        }

        public void ReplaceMateriaInTarget (Transform target)
        {
            MeshRenderer mr = target.GetComponent<MeshRenderer> ();
            if (mr == null)
                return;

            Material[] materialsCopy = new Material[mr.sharedMaterials.Length];
            for (int i = 0; i < mr.sharedMaterials.Length; ++i)
            {
                if (mr.sharedMaterials[i] == null)
                {
                    Debug.LogWarning ("TG | ReplaceMateriaInTarget | Encountered a null reference in the materials array of " + target.name);
                    materialsCopy[i] = materialReplacements[0].replacementMaterial;
                    continue;
                }

                materialsCopy[i] = mr.sharedMaterials[i];
                if (materialsCopy[i].mainTexture == null)
                {
                    materialsCopy[i] = materialReplacementEmpty;
                }
                else
                {
                    for (int m = 0; m < materialReplacements.Count; ++m)
                    {
                        // if (string.Equals (materialsCopy[i].name, materialReplacements[m].undesiredMaterials[u].name))
                        var texNameFilter = materialReplacements[m].textureName;
                        var texName = materialsCopy[i].mainTexture.name;
                        
                        if (texName.Contains (texNameFilter))
                        {
                            if (logMaterialReplacement)
                                Debug.Log ($"TG | ReplaceMaterialInTarget | Found match for filter {texNameFilter} in material {materialsCopy[i].name} using texture {texName}");
                            materialsCopy[i] = materialReplacements[m].replacementMaterial;
                            break;
                        }
                    }
                }
            }

            mr.materials = materialsCopy;
        }



        // Merging

        public void MergeEverything ()
        {
            if (exportBlocks)
            {
                MergeObjectsInHolder (GetHolderTilesetsSplitBlocks (), GetHolderTilesetsMergedBlocks (), FileGenerationMode.Blocks);
                DestroyImmediate (GetHolderTilesetsSplitBlocks ().gameObject);
            }

            if (exportDamage)
            {
                MergeObjectsInHolder (GetHolderTilesetsSplitDamage (), GetHolderTilesetsMergedDamage (), FileGenerationMode.DamageEdges);
                DestroyImmediate (GetHolderTilesetsSplitDamage ().gameObject);
            }

            if (exportMultiblocks)
            {
                MergeObjectsInHolder (GetHolderTilesetsSplitMultiBlocks (), GetHolderTilesetsMergedMultiBlocks (), FileGenerationMode.Multiblocks);
                DestroyImmediate (GetHolderTilesetsSplitMultiBlocks ().gameObject);
            }
        }

        public void MergeObjectsInHolder (Transform holderSplit, Transform holderMerged, FileGenerationMode mode)
        {
            UtilityGameObjects.ClearChildren (holderMerged);

            // I'm too lazy to figure out a proper way to compensate the offsets so yeah, reparenting does the trick
            Vector3 positionOldSource = holderSplit.position;
            Vector3 positionOldDestination = holderMerged.position;
            holderSplit.position = holderMerged.position = Vector3.zero;

            if (mode == FileGenerationMode.Blocks)
            {
                for (int i = 0; i < holderSplit.childCount; ++i)
                {
                    Material materialGenerated = null;
                    List<Material> materialsUsedForGeneration = null;
                    string materialAssetPath = folderTilesetExport + "/" + holderSplit.GetChild (i).name + "/blocks/";

                    AttemptArrayMaterialGeneration (holderSplit.GetChild (i), materialAssetPath, materialTemplateForBlocks, out materialGenerated, out materialsUsedForGeneration);
                    MergeChildren (holderSplit, i, holderMerged, materialGenerated, materialsUsedForGeneration);
                }
            }
            else if (mode == FileGenerationMode.DamageEdges)
            {
                for (int i = 0; i < holderSplit.childCount; ++i)
                    MergeChildren (holderSplit, i, holderMerged);
            }

            else if (mode == FileGenerationMode.Multiblocks)
            {
                for (int i = 0; i < holderSplit.childCount; ++i)
                {
                    Transform groupSplit = holderSplit.GetChild (i);
                    Vector3 groupSplitPositionOld = groupSplit.position;
                    groupSplit.position = Vector3.zero;

                    Transform groupMerged = new GameObject ().transform;
                    groupMerged.name = groupSplit.name;
                    groupMerged.transform.parent = holderMerged;
                    groupMerged.position = Vector3.zero;

                    Material materialGenerated = null;
                    List<Material> materialsUsedForGeneration = null;
                    string materialAssetPath = folderTilesetExport + "/" + groupSplit.name + "/multiblocks/";

                    AttemptArrayMaterialGeneration (groupSplit, materialAssetPath, materialTemplateForMultiblocks, out materialGenerated, out materialsUsedForGeneration);
                    for (int b = 0; b < groupSplit.childCount; ++b)
                        MergeChildren (groupSplit, b, groupMerged, materialGenerated, materialsUsedForGeneration);

                    /*
                    for (int b = 0; b < groupSplit.childCount; ++b)
                    {
                        Transform multiblockSplit = groupSplit.GetChild (b);
                        Vector3 multiblockSplitPositionOld = groupSplit.position;
                        multiblockSplit.position = Vector3.zero;

                        Transform multiblockMerged = new GameObject ().transform;
                        multiblockMerged.name = multiblockSplit.name;
                        multiblockMerged.transform.parent = groupMerged;
                        multiblockMerged.position = Vector3.zero;

                        for (int a = 0; a < multiblockSplit.childCount; ++a)
                        {
                            MergeObject (multiblockSplit.GetChild (a), multiblockMerged);
                        }

                        multiblockSplit.position = multiblockSplitPositionOld;
                        multiblockMerged.localPosition = multiblockSplit.localPosition;
                    }
                    */

                    groupSplit.position = groupSplitPositionOld;
                    groupMerged.localPosition = groupSplit.localPosition;
                }
            }

            // Restoring the old positions
            holderSplit.transform.position = positionOldSource;
            holderMerged.transform.position = positionOldDestination;
        }




        public void AttemptArrayMaterialGeneration (Transform parent, string pathForMaterialAssets, Material materialTemplate, out Material materialGenerated, out List<Material> materialsUsedForGeneration)
        {
            Debug.Log ("TG | AttemptArrayMaterialGeneration | Started on parent: " + parent.name + " | Path: " + pathForMaterialAssets);
            MeshFilter[] filters = parent.GetComponentsInChildren<MeshFilter> ();
            List<Material> materialsForArrayCollapse = new List<Material> ();
            List<Material> materialsWithCustomShaders = new List<Material> ();

            for (int f = 0; f < filters.Length; ++f)
            {
                MeshFilter filter = filters[f];
                Material[] sharedMaterials = filter.gameObject.GetComponent<MeshRenderer> ().sharedMaterials;
                for (int m = 0; m < sharedMaterials.Length; ++m)
                {
                    Material sharedMaterial = sharedMaterials[m];
                    if (string.Equals (sharedMaterial.shader.name, texArraySourceShaderName))
                    {
                        if (!materialsForArrayCollapse.Contains (sharedMaterial))
                        {
                            Debug.Log ("TG | AttemptArrayMaterialGeneration | Material " + sharedMaterial.name + " has array-compatible shader, collecting it with index " + materialsForArrayCollapse.Count);
                            materialsForArrayCollapse.Add (sharedMaterial);
                        }
                    }
                    else
                    {
                        if (!materialsWithCustomShaders.Contains (sharedMaterial))
                        {
                            Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Material " + sharedMaterial.name + " is using a custom shader, skipping it: " + sharedMaterial.shader.name);
                            materialsWithCustomShaders.Add (sharedMaterial);
                        }
                    }
                }
            }

            Debug.Log ("TG | AttemptArrayMaterialGeneration | Materials found suitable for array collapse: " + materialsForArrayCollapse.Count + " | Materials with custom shaders: " + materialsWithCustomShaders.Count);
            if (materialsForArrayCollapse.Count <= 1)
            {
                Debug.Log ("TG | AttemptArrayMaterialGeneration | Cancelling array collapse operation scheduled for this holder due to number of materials being too low: " + materialsForArrayCollapse.Count);
                materialGenerated = null;
                materialsUsedForGeneration = null;
                return;
            }

            Texture2DArray textureArrayAH = null;
            Texture2DArray textureArrayMSEO = null;

            for (int m = 0; m < materialsForArrayCollapse.Count; ++m)
            {
                Material material = materialsForArrayCollapse[m];
                Texture2D textureAH = material.GetTexture ("_MainTex") as Texture2D;
                Texture2D textureMSEO = material.GetTexture ("_MSEO") as Texture2D;

                if (textureArrayAH == null)
                {
                    Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Creating AH texture array with size of " + materialsForArrayCollapse.Count + ", format " + textureAH.format + " and dimensions " + textureAH.width + "x" + textureAH.height);
                    textureArrayAH = new Texture2DArray (textureAH.width, textureAH.height, materialsForArrayCollapse.Count, textureAH.format, true);
                }

                if (textureArrayMSEO == null)
                {
                    Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Creating MSEO texture array with size of " + materialsForArrayCollapse.Count + ", format " + textureMSEO.format + " and dimensions " + textureMSEO.width + "x" + textureMSEO.height);
                    textureArrayMSEO = new Texture2DArray (textureMSEO.width, textureMSEO.height, materialsForArrayCollapse.Count, textureMSEO.format, true);
                }

                if (textureAH != null)
                {
                    if (textureAH.width == textureArrayAH.width && textureAH.height == textureArrayAH.height)
                        Graphics.CopyTexture (textureAH, 0, textureArrayAH, m);
                    else
                        Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Skipping array write for AH texture " + m + " due to mismatched dimensions: " + textureAH.width + "x" + textureAH.height);
                }
                else
                    Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Skipping array write for missing AH texture " + m);

                if (textureMSEO != null)
                {
                    if (textureMSEO.width == textureArrayMSEO.width && textureMSEO.height == textureArrayMSEO.height)
                        Graphics.CopyTexture (textureMSEO, 0, textureArrayMSEO, m);
                    else
                        Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Skipping array write for MSEO texture " + m + " due to mismatched dimensions: " + textureMSEO.width + "x" + textureMSEO.height);
                }
                else
                    Debug.LogWarning ("TG | AttemptArrayMaterialGeneration | Skipping array write for missing MSEO texture " + m);
            }

            Texture2DArray textureArrayAHFromAsset = null;
            if (textureArrayAH != null)
            {
                textureArrayAH.Apply (false, false);
                textureArrayAHFromAsset = UtilityAssetDatabase.CreateAssetSafely (textureArrayAH, pathForMaterialAssets + "materialTexArrayAH.asset");
                textureArrayAHFromAsset.Apply (false, true);
            }

            Texture2DArray textureArrayMSEOFromAsset = null;
            if (textureArrayMSEO != null)
            {
                textureArrayMSEO.Apply (false, false);
                textureArrayMSEOFromAsset = UtilityAssetDatabase.CreateAssetSafely (textureArrayMSEO, pathForMaterialAssets + "materialTexArrayMSEO.asset");
                textureArrayMSEOFromAsset.Apply (false, true);
            }

            Vector4[] propertyArray = new Vector4[10];
            for (int m = 0; m < propertyArray.Length; ++m)
                propertyArray[m] = Vector4.zero;

            for (int m = 0; m < materialsForArrayCollapse.Count; ++m)
            {
                Material material = materialsForArrayCollapse[m];
                float smoothnessMin = material.HasProperty ("_SmoothnessMin") ? material.GetFloat (Shader.PropertyToID ("_SmoothnessMin")) : 0f;
                float smoothnessMed = material.HasProperty ("_SmoothnessMed") ? material.GetFloat (Shader.PropertyToID ("_SmoothnessMed")) : 0f;
                float smoothnessMax = material.HasProperty ("_SmoothnessMax") ? material.GetFloat (Shader.PropertyToID ("_SmoothnessMax")) : 0f;
                float worldSpaceUVOverride = material.HasProperty ("_WorldSpaceUVOverride") ? material.GetFloat (Shader.PropertyToID ("_WorldSpaceUVOverride")) : 0f;

                Vector4 properties = new Vector4 (smoothnessMin, smoothnessMed, smoothnessMax, worldSpaceUVOverride);
                Debug.Log ("TG | AttemptArrayMaterialGeneration | Setting array element " + m + " from material " + material.name + " with vector " + properties);
                propertyArray[m] = properties;
            }

            Material materialWithArrays = new Material (materialTemplate);
            Material materialFromAsset = UtilityAssetDatabase.CreateAssetSafely (materialWithArrays, pathForMaterialAssets + "materialWithArrays.mat");

            materialFromAsset.SetVectorArray (Shader.PropertyToID ("_PropertyArray"), propertyArray);
            if (textureArrayAHFromAsset != null)
                materialFromAsset.SetTexture (Shader.PropertyToID ("_TexArrayAH"), textureArrayAHFromAsset);
            if (textureArrayMSEOFromAsset != null)
                materialFromAsset.SetTexture (Shader.PropertyToID ("_TexArrayMSEO"), textureArrayMSEOFromAsset);

            GameObject tilesetDataObject = new GameObject ();
            tilesetDataObject.name = "materialDataContainer";
            tilesetDataObject.transform.parent = GetHolderTilesetsCells ();

            AreaTilesetContainer container = tilesetDataObject.AddComponent<AreaTilesetContainer> ();
            container.materialWithArrays = materialFromAsset;
            container.propertyArray = propertyArray;

            tilesetDataObject = PrefabUtility.CreatePrefab (pathForMaterialAssets + "materialDataContainer.prefab", tilesetDataObject, ReplacePrefabOptions.Default);

            materialGenerated = materialFromAsset;
            materialsUsedForGeneration = materialsForArrayCollapse;

            Debug.Log ("TG | AttemptArrayMaterialGeneration | Successfully finished material generation for " + parent.name + " using " + materialsForArrayCollapse.Count + " materials");
        }




        public void MergeChildren (Transform topHolderSplit, int childIndex, Transform topHolderMerged, Material materialGenerated = null, List<Material> materialsUsedForGeneration = null)
        {
            Transform parentOfSplitBlocks = topHolderSplit.GetChild (childIndex);
            Vector3 positionOfSplitBlocks = parentOfSplitBlocks.position;
            Transform parentOfMergedBlocks = new GameObject ().transform;

            parentOfMergedBlocks.name = parentOfSplitBlocks.name;
            parentOfMergedBlocks.transform.parent = topHolderMerged;
            parentOfSplitBlocks.position = parentOfMergedBlocks.position = Vector3.zero;

            Debug.Log ("TG | MergeChildren | Started merge of children in object " + parentOfSplitBlocks.name + " | Generated array material: " + materialGenerated.ToStringNullCheck ());

            if (materialGenerated != null && materialsUsedForGeneration != null)
            {
                for (int b = 0; b < parentOfSplitBlocks.childCount; ++b)
                    MergeObject (parentOfSplitBlocks.GetChild (b), parentOfMergedBlocks, materialGenerated, materialsUsedForGeneration);
            }
            else
            {
                for (int b = 0; b < parentOfSplitBlocks.childCount; ++b)
                    MergeObject (parentOfSplitBlocks.GetChild (b), parentOfMergedBlocks);
            }

            parentOfSplitBlocks.position = positionOfSplitBlocks;
            parentOfMergedBlocks.localPosition = parentOfSplitBlocks.localPosition;

        }

        public void MergeObject (Transform objectSliced, Transform objectParent, Material materialGenerated = null, List<Material> materialsUsedForGeneration = null)
        {
            if (objectSliced == null)
            {
                Debug.LogWarning ("CM | Sliced block transform is null");
                return;
            }

            // Another lazy compensation for offsets through reparenting
            Transform destinationParent = objectParent.parent;
            Vector3 holderMergedLocalPosition = objectParent.localPosition;
            Vector3 blockSlicedLocalPosition = objectSliced.localPosition;
            objectParent.localPosition = objectSliced.localPosition = Vector3.zero;

            GameObject blockSemiMerged = CombineMeshes.CombineChildren (objectSliced, objectParent);

            MeshFilter[] filters = blockSemiMerged.GetComponentsInChildren<MeshFilter> ();
            CombineInstance[] instances = new CombineInstance[filters.Length];
            List<Material> materials = new List<Material> ();

            for (int i = 0; i < filters.Length; ++i)
            {
                instances[i].mesh = filters[i].sharedMesh;
                instances[i].transform = filters[i].transform.worldToLocalMatrix;

                Material material = filters[i].gameObject.GetComponent<MeshRenderer> ().sharedMaterial;
                if (!materials.Contains (material))
                    materials.Add (material);
            }

            Mesh mesh = new Mesh ();
            mesh.CombineMeshes (instances, false);

            // Time to bake materials into vertex colors

            MeshFilter filter = blockSemiMerged.AddComponent<MeshFilter> ();
            MeshRenderer renderer = blockSemiMerged.AddComponent<MeshRenderer> ();
            renderer.sharedMaterials = materials.ToArray ();

            if (materialGenerated != null && materialsUsedForGeneration != null)
            {
                Vector2[] uv2 = new Vector2[mesh.vertexCount];
                Material[] materialsInMergedMesh = renderer.sharedMaterials;

                Debug.Log ("TG | MergeObject | " + blockSemiMerged.name + " | Received override list sized at: " + materialsUsedForGeneration.Count + " | Materials in merged mesh: " + materialsInMergedMesh.Length + " | Submeshes: " + mesh.subMeshCount);

                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; ++submeshIndex)
                {
                    int[] trianglesOfSubmesh = mesh.GetTriangles (submeshIndex);
                    Material materialForSubmesh = materialsInMergedMesh[submeshIndex];
                    Vector2 uv2ForSubmesh = new Vector2 (0f, 0f);

                    if (materialsUsedForGeneration.Contains (materialForSubmesh))
                    {
                        int materialIndex = materialsUsedForGeneration.IndexOf (materialForSubmesh);
                        uv2ForSubmesh = new Vector2 (materialIndex, 0f); // + 0.5f is added to move index to the middle of texture array index range
                        Debug.Log ("TG | MergeObject | " + blockSemiMerged.name + " | Successfully found tex-array index " + materialIndex + " for material named " + materialForSubmesh.name + " | Resulting UV2: " + uv2ForSubmesh);
                    }
                    else
                    {
                        Debug.LogWarning ("TG | MergeObject | " + blockSemiMerged.name + " | Material named " + materialForSubmesh + " was not found in tex-array index dictionary");
                    }

                    for (int t = 0; t < trianglesOfSubmesh.Length; ++t)
                        uv2[trianglesOfSubmesh[t]] = uv2ForSubmesh;
                }

                mesh.uv2 = uv2;

                Vector3 center = mesh.bounds.center;
                Vector3 extents = mesh.bounds.extents;
                if (center.x != 0f || center.z != 0f)
                {
                    float maxOnX = Mathf.Abs (center.x) + extents.x;
                    float maxOnZ = Mathf.Abs (center.z) + extents.z;
                    center = new Vector3 (0f, center.y, 0f);
                    extents = new Vector3 (Mathf.Max (1.5f, maxOnX), extents.y, Mathf.Max (1.5f, maxOnZ));
                    Bounds bounds = new Bounds (center, extents * 2f);
                    Debug.Log ("TG | MergeObject | Mesh has a non-centered bounds origin: " + mesh.bounds + " | New bounds: " + bounds, filter.gameObject);
                    mesh.bounds = bounds;
                }
                else
                {
                    Debug.Log ("TG | MergeMesh | Mesh has compliant centered bounds origin");
                }



                // List<int> trianglesForArraySubmesh = new List<int> ();
                Dictionary<Material, List<int>> materialTrianglePairs = new Dictionary<Material, List<int>> ();

                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; ++submeshIndex)
                {
                    int[] trianglesOfSubmesh = mesh.GetTriangles (submeshIndex);
                    Material materialForSubmesh = materialsInMergedMesh[submeshIndex];

                    if (string.Equals (materialForSubmesh.shader.name, texArraySourceShaderName))
                    {
                        // trianglesForArraySubmesh.AddRange (trianglesOfSubmesh);
                        Debug.Log ("TG | MergeObject | Encountered a standard non-merged tileset shader in material for submesh " + submeshIndex + ", proceeding normally");
                        if (!materialTrianglePairs.ContainsKey (materialGenerated))
                            materialTrianglePairs.Add (materialGenerated, new List<int> ());
                        materialTrianglePairs[materialGenerated].AddRange (trianglesOfSubmesh);
                    }
                    else
                    {
                        Debug.LogWarning ("TG | MergeObject | Encountered a nonstandard shader in the material mapped to submesh " + submeshIndex + " called " + materialForSubmesh.name + ": " + materialForSubmesh.shader.name);
                        if (!materialTrianglePairs.ContainsKey (materialForSubmesh))
                            materialTrianglePairs.Add (materialForSubmesh, new List<int> ());
                        materialTrianglePairs[materialForSubmesh].AddRange (trianglesOfSubmesh);
                    }
                }

                Debug.Log ("TG | MergeObject | Preparing to assign " + materialTrianglePairs.Count + " material-submesh pairs to the merged mesh");
                int submeshCounter = 0;
                List<Material> materialsFinal = new List<Material> ();
                foreach (KeyValuePair<Material, List<int>> kvp in materialTrianglePairs)
                {
                    mesh.SetTriangles (kvp.Value.ToArray (), submeshCounter);
                    materialsFinal.Add (kvp.Key);
                    submeshCounter += 1;
                }

                mesh.subMeshCount = submeshCounter;
                renderer.sharedMaterials = materialsFinal.ToArray ();

                // mesh.SetTriangles (trianglesForArraySubmesh, 0);
                // mesh.subMeshCount = 1;
                // renderer.sharedMaterials = new Material[] { materialOverride };
            }

            filter.sharedMesh = mesh;

            UtilityGameObjects.ClearChildren (blockSemiMerged.transform);
            objectParent.parent = destinationParent;
            objectParent.localPosition = holderMergedLocalPosition;
            objectSliced.localPosition = blockSemiMerged.transform.localPosition = blockSlicedLocalPosition;
            blockSemiMerged.transform.localRotation = Quaternion.identity;
        }




        // Saving

        public void SaveEverything ()
        {
            Debug.Log ("TG | SaveAssets | Saving");

            if (folderTilesetExport.EndsWith ("/"))
                folderTilesetImport = folderTilesetImport.TrimEnd ('/');

            if (exportBlocks)
                SaveObjectsInHolder (GetHolderTilesetsMergedBlocks (), "blocks", FileGenerationMode.Blocks);

            if (exportDamage)
                SaveObjectsInHolder (GetHolderTilesetsMergedDamage (), "damage", FileGenerationMode.DamageEdges);

            if (exportMultiblocks)
                SaveObjectsInHolder (GetHolderTilesetsMergedMultiBlocks (), "multiblocks", FileGenerationMode.Multiblocks);

            ResourceDatabaseManager.RebuildDatabase ();
            AreaTilesetHelper.LoadDatabase ();

            AreaManager[] ams = FindObjectsOfType<AreaManager> ();
            for (int i = 0; i < ams.Length; ++i)
                ams[i].RebuildEverything ();

            EditorUtility.ClearProgressBar ();

            DestroyImmediate (GetHolderTilesetsMergedBlocks ().gameObject);
            DestroyImmediate (GetHolderTilesetsMergedDamage ().gameObject);
            DestroyImmediate (GetHolderTilesetsMergedMultiBlocks ().gameObject);
            DestroyImmediate (GetHolderTilesetsCells ().gameObject);
        }

        public void SaveObjectsInHolder (Transform holderMerged, string subfolderName, FileGenerationMode fileGenerationMode)
        {
            Debug.Log ("TG | SaveObjectsInHolder | Holder name: " + holderMerged.name + " | Subfolder name: " + subfolderName + " | Holder child count: " + holderMerged.childCount);

            string progressBarHeader1 = "Saving " + subfolderName;
            string progressBarHeader2 = "Loading " + subfolderName;
            
            float progressBarPercentage = 0.0f;
            EditorUtility.DisplayProgressBar (progressBarHeader1, "Starting...", progressBarPercentage);

            if (fileGenerationMode == FileGenerationMode.Blocks || fileGenerationMode == FileGenerationMode.DamageEdges)
            {
                for (int i = 0; i < holderMerged.childCount; ++i)
                {
                    Transform tilesetMerged = holderMerged.GetChild (i);

                    for (int b = 0; b < tilesetMerged.childCount; ++b)
                    {
                        Debug.Log ("TG | SaveObjectsInHolder | Saving object: " + tilesetMerged.GetChild (b).gameObject.name);

                        progressBarPercentage = Mathf.Min (1f, (float)(b + 1) / (float)tilesetMerged.childCount);
                        string progressDesc = (int)(progressBarPercentage * 100f) + "% done | Processing object: " + tilesetMerged.GetChild (b).gameObject.name;
                        EditorUtility.DisplayProgressBar (progressBarHeader1, progressDesc, progressBarPercentage);

                        SaveObjectMesh (tilesetMerged.GetChild (b).gameObject, folderTilesetExport + "/" + tilesetMerged.name + "/" + subfolderName);
                    }
                }
                
                for (int i = 0; i < holderMerged.childCount; ++i)
                {
                    Transform tilesetMerged = holderMerged.GetChild (i);

                    for (int b = 0; b < tilesetMerged.childCount; ++b)
                    {
                        Debug.Log ("TG | SaveObjectsInHolder | Loading object: " + tilesetMerged.GetChild (b).gameObject.name);

                        progressBarPercentage = Mathf.Min (1f, (float)(b + 1) / (float)tilesetMerged.childCount);
                        string progressDesc = (int)(progressBarPercentage * 100f) + "% done | Processing object: " + tilesetMerged.GetChild (b).gameObject.name;
                        EditorUtility.DisplayProgressBar (progressBarHeader2, progressDesc, progressBarPercentage);

                        AssignSavedMeshToObject (tilesetMerged.GetChild (b).gameObject, folderTilesetExport + "/" + tilesetMerged.name + "/" + subfolderName);
                    }
                }
            }

            exportProgress = 0f;
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();

            EditorUtility.ClearProgressBar ();
        }

        private Vector3Int RoundToGrid (Transform localParent, Vector3 localPosition)
        {
            localPosition -= new Vector3 (1.5f, -1.5f, 1.5f);
            localPosition /= 3f;
            return new Vector3Int
            (
                Mathf.RoundToInt (localPosition.x),
                Mathf.RoundToInt (localPosition.y),
                Mathf.RoundToInt (localPosition.z)
            );
        }
        
        public void SaveObjectMesh (GameObject objectMerged, string exportPath)
        {
            Debug.Log ("TG | SaveObjectMesh | Exporting asset to path: " + exportPath);

            if (objectMerged == null)
            {
                Debug.LogWarning ("TG | SaveObjectMesh | Source transform is null");
                return;
            }

            MeshFilter filter = objectMerged.GetComponent<MeshFilter> ();
            UtilityAssetDatabase.CreateAssetSafely (filter.sharedMesh, exportPath + "/" + objectMerged.name + ".asset", false);
        }
        
        public void AssignSavedMeshToObject (GameObject objectMerged, string exportPath)
        {
            if (objectMerged == null)
            {
                Debug.LogWarning ("TG | AssignSavedMeshToObject | Source transform is null");
                return;
            }

            MeshFilter filter = objectMerged.GetComponent<MeshFilter> ();
            Mesh meshFromAsset = AssetDatabase.LoadAssetAtPath (exportPath + "/" + objectMerged.name + ".asset", typeof (Mesh)) as Mesh;

            if (meshFromAsset != null)
            {
                Debug.Log ("TG | AssignSavedMeshToObject | Mesh " + meshFromAsset.name + " added to object " + objectMerged.name);
                filter.sharedMesh = meshFromAsset;
            }
            else
                Debug.LogWarning ("TG | AssignSavedMeshToObject | Failed to load mesh " + exportPath + "/" + objectMerged.name + ".asset");

            objectMerged = PrefabUtility.CreatePrefab (exportPath + "/" + objectMerged.name + ".prefab", objectMerged, ReplacePrefabOptions.Default);
            filter = objectMerged.GetComponent<MeshFilter> ();
            
            if (filter.sharedMesh == null)
                Debug.Log ("TG | AssignSavedMeshToObject | Mesh missing from object prefab " + objectMerged.name);
        }

        public void SaveObject (GameObject objectMerged, string exportPath, FileGenerationMode fileGenerationMode)
        {
            Debug.Log ("TG | SaveObject | Exporting asset to path: " + exportPath);

            if (objectMerged == null)
            {
                Debug.LogWarning ("TG | SaveObject | Source transform is null");
                return;
            }

            MeshFilter filter = objectMerged.GetComponent<MeshFilter> ();

            Mesh meshFromAsset = UtilityAssetDatabase.CreateAssetSafely (filter.sharedMesh, exportPath + "/" + objectMerged.name + ".asset");
            if (meshFromAsset != null)
            {
                Debug.Log ("TG | SaveObject | Mesh " + meshFromAsset.name + " added to object " + objectMerged.name);
                filter.sharedMesh = meshFromAsset;
            }
            else
                Debug.LogWarning ("TG | SaveObject | Failed to load mesh " + exportPath + "/" + objectMerged.name + ".asset");

            objectMerged = PrefabUtility.CreatePrefab (exportPath + "/" + objectMerged.name + ".prefab", objectMerged, ReplacePrefabOptions.Default);
            filter = objectMerged.GetComponent<MeshFilter> ();
            if (filter.sharedMesh == null)
            {
                Debug.Log ("TG | SaveObject | Mesh missing from object prefab " + objectMerged.name);
            }

            if (fileGenerationMode == FileGenerationMode.Multiblocks)
            {
                string[] nameSplit = objectMerged.name.Split ('_');
                string[] boundsSplit = nameSplit[0].Split ('x');
                string[] pivotSplit = nameSplit[1].Split (',');

                int boundsX = 0;
                int boundsY = 0;
                int boundsZ = 0;
                int pivotX = 0;
                int pivotY = 0;
                int pivotZ = 0;

                if
                (
                    boundsSplit.Length != 3 ||
                    !int.TryParse (boundsSplit[0], out boundsX) ||
                    !int.TryParse (boundsSplit[1], out boundsY) ||
                    !int.TryParse (boundsSplit[2], out boundsZ) ||
                    !int.TryParse (pivotSplit[0], out pivotX) ||
                    !int.TryParse (pivotSplit[1], out pivotY) ||
                    !int.TryParse (pivotSplit[2], out pivotZ)
                )
                {
                    Debug.LogWarning ("TG | LoadAssets | Skipping the creation of multi-block definition SO for " + objectMerged.name + " due to failed check on the name syntax");
                    return;
                }

                TilesetMultiBlockDefinition mbd = ScriptableObject.CreateInstance<TilesetMultiBlockDefinition> ();
                mbd.bounds = new Vector3Int (boundsX, boundsY, boundsZ);
                mbd.pivotPosition = new Vector3Int (pivotX, pivotY, pivotZ);
                mbd.prefab = objectMerged;
                mbd.name = mbd.prefab.name;

                string mbdPath = exportPath + "/" + objectMerged.name + "_mbd.asset";
                TilesetMultiBlockDefinition mbdExisting = AssetDatabase.LoadAssetAtPath<TilesetMultiBlockDefinition> (mbdPath);
                if (mbdExisting != null && mbdExisting.volume.Length == mbd.bounds.x * mbd.bounds.y * mbd.bounds.z)
                {
                    Debug.Log ("TG | LoadAssets | Multiblock definition named " + mbd.name + " already exists, copying volume information from it...");
                    mbd.volume = new bool[mbdExisting.volume.Length];
                    System.Array.Copy (mbdExisting.volume, mbd.volume, mbdExisting.volume.Length);

                    // mbd.volume = mbdExisting.volume.Clone () as bool[];
                }

                UtilityAssetDatabase.CreateAssetSafely (mbd, exportPath + "/" + objectMerged.name + "_mbd.asset");
                // AssetDatabase.CreateAsset (mbd, exportPath + "/" + objectMerged.name + "_mbd.asset");
            }
        }


        public void PlaceEverything ()
        {
            AreaTilesetHelper.LoadDatabase ();
            int tilesetIndex = 0;
            foreach (KeyValuePair<int, AreaTileset> kvp in AreaTilesetHelper.database.tilesets)
            {
                AreaTileset tileset = kvp.Value;

                if (exportBlocks && tileset.blocks != null)
                {
                    int blockIndex = 0;
                    for (int i = 0; i < tileset.blocks.Length; ++i)
                    {
                        AreaBlockDefinition block = tileset.blocks[i];
                        if (block != null)
                        {
                            int subtypeIndex = 0;
                            foreach (KeyValuePair<byte, SortedDictionary<byte, GameObject>> kvpGroup in block.subtypeGroups)
                            {
                                foreach (KeyValuePair<byte, GameObject> kvpSubtype in kvpGroup.Value)
                                {
                                    GameObject prefabInstance = PrefabUtility.InstantiatePrefab (kvpSubtype.Value) as GameObject;

                                    float blockSpacing = TilesetUtility.blockAssetSize * 1.5f;
                                    Vector3 blockPosition = new Vector3
                                    (
                                        blockSpacing * ((float)blockIndex % 8f),
                                        blockSpacing * (float)subtypeIndex,
                                        blockSpacing * Mathf.FloorToInt ((float)blockIndex / 8f)
                                    );

                                    prefabInstance.transform.parent = GetHolderTilesetsMergedBlocks ();
                                    prefabInstance.transform.localPosition = blockPosition + new Vector3 (0f, 12f, 0f);

                                    subtypeIndex += 1;
                                }
                            }

                            blockIndex += 1;
                        }
                    }
                }

                tilesetIndex += 1;
            }
        }


        // Holders

        [HideInInspector] public Transform holderTilesetsCells;
        [HideInInspector] public Transform holderTilesetsSplitBlocks;
        [HideInInspector] public Transform holderTilesetsSplitDamage;
        [HideInInspector] public Transform holderTilesetsSplitMultiBlocks;
        [HideInInspector] public Transform holderTilesetsMergedBlocks;
        [HideInInspector] public Transform holderTilesetsMergedDamage;
        [HideInInspector] public Transform holderTilesetsMergedMultiBlocks;

        [HideInInspector] public string holderTilesetsCellsName = "GeneratorHolder_Cells";
        [HideInInspector] public string holderTilesetsSplitBlocksName = "GeneratorHolder_SplitBlocks";
        [HideInInspector] public string holderTilesetsSplitDamageName = "GeneratorHolder_SplitDamage";
        [HideInInspector] public string holderTilesetsSplitMultiBlocksName = "GeneratorHolder_SplitMultiblocks";
        [HideInInspector] public string holderTilesetsMergedBlocksName = "GeneratorHolder_MergedBlocks";
        [HideInInspector] public string holderTilesetsMergedDamageName = "GeneratorHolder_MergedDamage";
        [HideInInspector] public string holderTilesetsMergedMultiBlocksName = "GeneratorHolder_MergedMultiBlocks";

        public Transform GetHolderTilesetsCells () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsCells, holderTilesetsCellsName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsSplitBlocks () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsSplitBlocks, holderTilesetsSplitBlocksName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsSplitDamage () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsSplitDamage, holderTilesetsSplitDamageName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsSplitMultiBlocks () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsSplitMultiBlocks, holderTilesetsSplitMultiBlocksName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsMergedBlocks () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsMergedBlocks, holderTilesetsMergedBlocksName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsMergedDamage () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsMergedDamage, holderTilesetsMergedDamageName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsMergedMultiBlocks () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsMergedMultiBlocks, holderTilesetsMergedMultiBlocksName, HideFlags.DontSave, transform.localPosition); }

        // Deco


        private string blockFramePrefabPath = "Assets/Content/Objects/TilesetTest/EditorAssets/spot_frame";
        private GameObject blockFramePrefab = null;

        public GameObject GetBlockFramePrefab ()
        {
            if (blockFramePrefab == null)
                blockFramePrefab = AssetDatabase.LoadAssetAtPath (blockFramePrefabPath, typeof (GameObject)) as GameObject;
            return blockFramePrefab;
        }



        public void AddLightSourceDefinition ()
        {
            if (lightSourceHelper == null)
            {
                GameObject go = GameObject.Find ("LightSourceHelper");
                if (go != null)
                    lightSourceHelper = go.transform;
                if (lightSourceHelper.parent == null)
                    return;
            }

            Light light = lightSourceHelper.GetComponent<Light> ();
            if (light == null)
                return;

            LightSourceDefinition ld = new LightSourceDefinition ();
            ld.intensity = light.intensity;
            ld.range = light.range;
            ld.color = light.color;
            ld.localPosition = lightSourceHelper.localPosition;
            ld.localRotation = lightSourceHelper.localRotation.eulerAngles;
            ld.blockName = lightSourceHelper.parent.name;
            ld.type = light.type;
            ld.angle = light.spotAngle;

            lightSourceDefinitions.Add (ld);
        }

        public void GenerateLights ()
        {
            if (exportBlocks)
                GenerateLights (GetHolderTilesetsMergedBlocks ().GetChild (0));

            if (exportMultiblocks)
                GenerateLights (GetHolderTilesetsMergedMultiBlocks ().GetChild (0));
        }

        public void GenerateLights (Transform t)
        {
            for (int s = 0; s < lightSourceDefinitions.Count; ++s)
            {
                LightSourceDefinition lsd = lightSourceDefinitions[s];
                for (int i = 0; i < t.childCount; ++i)
                {
                    Transform block = t.GetChild (i);
                    if (string.Equals (block.name, lsd.blockName))
                    {
                        GameObject lightObject = new GameObject ();
                        lightObject.name = "light";
                        lightObject.transform.parent = block;
                        lightObject.transform.localPosition = lsd.localPosition;
                        lightObject.transform.localRotation = Quaternion.Euler (lsd.localRotation);

                        Light lightComponent = lightObject.AddComponent<Light> ();
                        lightComponent.color = lsd.color;
                        lightComponent.range = lsd.range;
                        lightComponent.intensity = lsd.intensity;
                        lightComponent.type = lsd.type;
                        lightComponent.spotAngle = lsd.angle;

                        Debug.Log ("Created light for " + block.name);
                        break;
                    }
                }
            }
        }

#endif
    }
}