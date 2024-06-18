using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Area
{
    [ExecuteInEditMode]
    public class TilesetProcessor : MonoBehaviour
    {
        #if UNITY_EDITOR

        [Serializable]
        public class FileSettings
        {
            public bool load = true;
            public GameObject asset = null;

            [LabelText ("Fallback material")]
            public Material materialFallback;

            public FileSettings (GameObject asset, bool load)
            {
                this.asset = asset;
                this.load = load;
            }
        }

        [Serializable]
        public class TilesetProcessorData
        {
            public string name;

            public bool loadBlocks;

            private const string fgOther = "Other";
            private const string fgAssets = "Assets";

            [FoldoutGroup (fgAssets, false)]
            public GameObject assetBlocks;

            [FoldoutGroup (fgOther, false)]
            public Material materialFallback;
            
            [FoldoutGroup (fgOther)]
            public MaterialSerializationHelper materialHelper;
            
            [FoldoutGroup (fgOther)]
            public Transform holderRootSplit;
            
            [FoldoutGroup (fgOther)]
            public Transform holderBlocksSplit;

            [FoldoutGroup (fgOther)]
            public Transform holderRootMerged;
            
            [FoldoutGroup (fgOther)]
            public Transform holderBlocksMerged;
        }

        [Header ("Core"), FolderPath, LabelText ("Import folder")]
        public string folderTilesetImport;

        [Header ("Core"), FolderPath, LabelText ("Export folder")]
        public string folderTilesetExport;

        public List<TilesetProcessorData> tilesets;

        [Header ("Materials"), FolderPath, LabelText ("Materials folder")]
        public string folderMaterials;

        [LabelText ("Replacements"), ListDrawerSettings (ShowPaging = false)]
        public List<AssetProcessorMaterialReplacement> materialReplacements = new List<AssetProcessorMaterialReplacement> ();

        [LabelText ("Template")]
        public Material materialTemplate;

        [LabelText ("Fallback")]
        public Material materialFallbackDefault;

        [Header ("Other"), LabelText ("Spacing")]
        public float blockSpacing = TilesetUtility.blockAssetSize * 1.5f;

        private static string texArraySourceShaderName = "Hardsurface/Environment/Block (standard)";
        //private static string texArrayMaterialShaderName = "Hardsurface/Environment/Block (array-based)";

        private List<byte> configurationBytes = new List<byte> ();
        //private float exportProgress = 0f;

        private GameObject cellPrefab;




        private void Awake ()
        {
            CheckInitialization ();
            LoadAssets ();
        }

        private void OnEnable ()
        {
            CheckInitialization ();
            LoadAssets ();
        }

        private void OnDestroy ()
        {
            DestroyImmediate (GetHolderTilesetsCells ().gameObject);
            DestroyImmediate (GetHolderTilesetsMerged ().gameObject);
            DestroyImmediate (GetHolderTilesetsSplit ().gameObject);
        }




        private void CheckInitialization ()
        {
            if (cellPrefab == null)
            {
                cellPrefab = (GameObject)Resources.Load ("Editor/AreaVisuals/block_cell", typeof (GameObject));
            }

            if (configurationBytes == null || configurationBytes.Count == 0)
            {
                configurationBytes = new List<byte> ();

                // The number of results is predictable, it's 2^N - in our case we need 2x2x2 3d group of objects, so N is 8
                int loopCount = (int)Math.Pow (2, 8);

                // All we need is to fill them - to do that, we convert every integer to binary and then to a bool list
                for (int i = 0; i < loopCount; ++i)
                {
                    configurationBytes.Add ((byte)i);
                }

                // Since we have to transform every configuration with rotation and flipping to determine whether it has duplicates, we can't do removal in a simple back-ordered for loop and a temporary index list is required
                List<int> unnecessaryIndexes = new List<int> ();
                for (int i = 0; i < configurationBytes.Count; ++i)
                {
                    // Since configuration might have already been marked for removal, we need to check for that, as that will mean that transforming and checking it is pointless - another one yielding exactly the same transformations was already passed
                    if (unnecessaryIndexes.Contains (i))
                    {
                        continue;
                    }

                    // This gives us 8 transformations - 4 horizontal rotations, and 4 flipped horizontal rotations of our 2x2x2 true/false "cube"
                    List<byte> transformations = TilesetUtility.GetConfigurationTransformations (configurationBytes[i]);
                    for (int t = 0; t < transformations.Count; ++t)
                    {
                        for (int c = 0; c < configurationBytes.Count; ++c)
                        {
                            // Since this is the second loop through the configuration collection, we have so avoid a case where a configuration is removed after a check against itself. We also do another check for unnecessary indexes list
                            if (c == i || unnecessaryIndexes.Contains (c))
                            {
                                continue;
                            }

                            // If there is a match, we don't need one of those configurations
                            if (configurationBytes[c] == transformations[t])
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
                    configurationBytes.RemoveAt (unnecessaryIndexes[i]);
                }
            }
        }

        public void RebuildEverything ()
        {
            InstantiateAndCheckModels ();
            // ReplaceMaterials ();
            // MergeEverything ();
            // SaveEverything ();
            ResourceDatabaseManager.RebuildDatabase ();
        }

        


        [Button ("Find material replacements", ButtonSizes.Medium)]
        public void FindMaterialReplacements ()
        {
            if (folderMaterials.EndsWith ("/")) folderMaterials = folderMaterials.TrimEnd ('/');
            if (!AssetDatabase.IsValidFolder (folderMaterials))
                return;

            materialReplacements = new List<AssetProcessorMaterialReplacement> ();
            var dirInfo = new System.IO.DirectoryInfo (folderMaterials);
            var files = new List<System.IO.FileInfo> (dirInfo.GetFiles ("*.mat"));

            for (int i = 0; i < files.Count; ++i)
            {
                var fileInfo = files[i];
                string fullPath = fileInfo.FullName.Replace (@"\", "/");
                string assetPath = "Assets" + fullPath.Replace (Application.dataPath, "");

                var material = AssetDatabase.LoadAssetAtPath<Material> (assetPath);
                if (material == null)
                    continue;

                if (!material.HasProperty ("_MainTex"))
                {
                    Debug.Log ("Skipping material " + i + " due to its shader not containing a property called _MainTex");
                    continue;
                }

                var texture = material.GetTexture ("_MainTex");
                if (texture == null)
                {
                    Debug.Log ("Skipping material " + i + " due to it not referencing a texture");
                    continue;
                }

                string textureName = texture.name.EndsWith ("_ah") ? texture.name.TrimEnd ("_ah") : texture.name;
                var replacement = new AssetProcessorMaterialReplacement ();
                replacement.replacementMaterial = material;
                replacement.textureName = textureName;
                materialReplacements.Add (replacement);

                Debug.Log ("Successfully found a replacement: " + material.name + " with texture filter " + textureName);
            }

            materialReplacements.Sort (delegate (AssetProcessorMaterialReplacement i1, AssetProcessorMaterialReplacement i2) 
            { return i1.textureName.CompareTo (i2.textureName); });
        }




        // Tileset loading

        [Button ("Load assets", ButtonSizes.Medium)]
        public void LoadAssets ()
        {
            CheckInitialization ();

            // First we import all our subtype meshes
            if (folderTilesetImport.EndsWith ("/")) folderTilesetImport = folderTilesetImport.TrimEnd ('/');
            if (!AssetDatabase.IsValidFolder (folderTilesetImport))
                return;

            var tilesetLookupOld = new Dictionary<string, TilesetProcessorData> ();
            for (int i = 0; i < tilesets.Count; ++i)
            {
                var tileset = tilesets[i];
                if (!tilesetLookupOld.ContainsKey (tileset.name))
                    tilesetLookupOld.Add (tileset.name, tileset);
            }

            tilesets = new List<TilesetProcessorData> ();
            var tilesetLookup = new Dictionary<string, TilesetProcessorData> ();

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

                if (fileNameSplit.Length != 3)
                {
                    Debug.LogWarning (i + " | Skipping file with invalid name: " + fullPath + "\nAsset path: " + assetPath);
                    continue;
                }

                string tilesetName = fileNameSplit[1];
                string suffix = fileNameSplit[2];

                bool fileContainsBlocks = string.Equals (suffix, "blocks");
                if (!fileContainsBlocks)
                {
                    Debug.LogWarning (i + " | Skipping file with invalid name: " + fullPath + "\nTileset: " + tilesetName + " | Asset path: " + assetPath);
                    continue;
                }

                Debug.Log (i + " | Loading file: " + fullPath + "\nTileset: " + tilesetName + " | Asset path: " + assetPath + " | Suffix: " + fileNameSplit[fileNameSplit.Length - 1]);

                GameObject asset = AssetDatabase.LoadAssetAtPath (assetPath, typeof (GameObject)) as GameObject;
                if (asset == null)
                    continue;

                if (!tilesetLookup.ContainsKey (tilesetName))
                {
                    var tilesetNew = new TilesetProcessorData ();
                    tilesetNew.name = tilesetName;
                    tilesetNew.materialFallback = materialFallbackDefault;

                    if (tilesetLookupOld.ContainsKey (tilesetName))
                    {
                        var tilesetOld = tilesetLookupOld[tilesetName];
                        tilesetNew.loadBlocks = tilesetOld.loadBlocks;
                        tilesetNew.materialFallback = tilesetOld.materialFallback;
                    }

                    tilesetLookup.Add (tilesetName, tilesetNew);
                    tilesets.Add (tilesetNew);
                }

                var tileset = tilesetLookup[tilesetName];

                if (fileContainsBlocks)
                {
                    if (tileset.assetBlocks == null)
                        tileset.assetBlocks = asset;
                    else
                        Debug.LogWarning ("Tileset " + tilesetName + " already contains blocks file, skipping this asset");
                }
            }

            tilesets.Sort (delegate (TilesetProcessorData i1, TilesetProcessorData i2) { return i1.name.CompareTo (i2.name); });
        }




        // [ResponsiveButtonGroup ("Process", DefaultButtonSize = ButtonSizes.Medium, UniformLayout = true)]
        [Button ("Instantiate and check", ButtonSizes.Medium)]
        public void InstantiateAndCheckModels ()
        {
            UtilityGameObjects.ClearChildren (GetHolderTilesetsCells ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsSplit ());
            UtilityGameObjects.ClearChildren (GetHolderTilesetsMerged ());

            GetHolderTilesetsSplit ().gameObject.SetActive (true);
            GetHolderTilesetsMerged ().gameObject.SetActive (false);

            for (int t = 0; t < tilesets.Count; ++t)
            {
                var tileset = tilesets[t];

                bool useBlocks = tileset.loadBlocks && tileset.assetBlocks != null;

                if (!useBlocks)
                    continue;
                
                var holderRoot = new GameObject (tileset.name).transform;
                tileset.holderRootSplit = holderRoot;
                holderRoot.parent = GetHolderTilesetsSplit ();
                holderRoot.SetLocalTransformationToZero ();

                if (useBlocks)
                {
                    var holderBlocks = new GameObject (tileset.name + "_blocks").transform;
                    tileset.holderBlocksSplit = holderBlocks;
                    holderBlocks.parent = holderRoot;
                    holderBlocks.SetLocalTransformationToZero ();

                    // We must check whether our tileset holder contains all configurations
                    bool[] tilesetCompletenessChecklist = new bool[configurationBytes.Count];

                    // Since we don't need any meshes for 00000000 and 11111111 configurations, we can immediately mark those as checked
                    tilesetCompletenessChecklist[0] = true;
                    tilesetCompletenessChecklist[tilesetCompletenessChecklist.Length - 1] = true;
                    Dictionary<byte, int> configurationCounter = new Dictionary<byte, int> ();

                    for (int i = 0; i < tileset.assetBlocks.transform.childCount; ++i)
                    {
                        Transform block = tileset.assetBlocks.transform.GetChild (i);
                        string[] blockNameSplit = block.name.Split ('_');

                        // Backwards compatibiliy with older naming scheme
                        if (blockNameSplit.Length == 2)
                        {
                            blockNameSplit = new string[] { blockNameSplit[0], blockNameSplit[1], "0" };
                        }

                        // Weed out bad names
                        if 
                        (
                            blockNameSplit.Length < 3 || 
                            blockNameSplit[0].Length != 8 || 
                            !int.TryParse (blockNameSplit[0], out int parsedConfiguration) || 
                            !int.TryParse (blockNameSplit[1], out int parsedGroup) || 
                            !int.TryParse (blockNameSplit[2], out int parsedSubtype)
                        )
                        {
                            Debug.LogWarning ("Tileset " + t + ": " + tileset.name + " | Skipping the block " + i + " (" + block.name + ") due to invalid name");
                            continue;
                        }

                        // Next we need to extract the configuration byte and determine the configuration index
                        int configurationIndex = -1;
                        byte configurationByte = TilesetUtility.GetConfigurationFromString (blockNameSplit[0]);
                        for (int j = 0; j < configurationBytes.Count; ++j)
                        {
                            if (configurationBytes[j] == configurationByte)
                            {
                                configurationIndex = j;
                                tilesetCompletenessChecklist[j] = true;
                            }
                        }

                        // If index is -1, that means we never found a match (which is pretty weird and shouldn't happen when checking proper template-referenced models against a proper configuration list)
                        if (configurationIndex == -1)
                        {
                            Debug.LogWarning ("Tileset " + t + ": " + tileset.name + " | Skipping the block " + i + " (" + block.name + ") due to configuration " + 
                                parsedConfiguration + " (" + configurationByte + ") not being found");
                            continue;
                        }

                        // Counter is used to position blocks vertically, like in the source model
                        if (configurationCounter.ContainsKey (configurationByte))
                            configurationCounter[configurationByte] += 1;
                        else
                            configurationCounter.Add (configurationByte, 0);

                        // This will produce an 8x8 grid of blocks spaced evenly and offset by tileset index
                        Vector3 blockPosition = new Vector3
                        (
                            blockSpacing * ((float)configurationIndex % 8f),
                            blockSpacing * (float)configurationCounter[configurationByte],
                            blockSpacing * Mathf.FloorToInt ((float)configurationIndex / 8f)
                        );

                        // Space tilesets on X
                        blockPosition += new Vector3 (t * blockSpacing * 10f, 20f, 0f);

                        var blockEnvelopeName = blockNameSplit[0] + "_" + blockNameSplit[1] + "_" + blockNameSplit[2];
                        if (blockNameSplit.Length == 4)
                            blockEnvelopeName += $"_{blockNameSplit[3]}";
                        
                        var blockEnvelope = new GameObject (blockEnvelopeName).transform;
                        blockEnvelope.parent = tileset.holderBlocksSplit;
                        blockEnvelope.SetLocalTransformationToZero ();
                        blockEnvelope.localPosition = blockPosition;

                        var blockCopy = Instantiate (block.gameObject).transform;
                        blockCopy.parent = blockEnvelope.transform;
                        blockCopy.SetLocalTransformationToZero ();
                        blockCopy.localRotation = Quaternion.Euler (0f, 180f, 0f);
                        
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

                    bool valid = true;
                    int missingConfigurationCount = 0;
                    for (int j = 0; j < tilesetCompletenessChecklist.Length; ++j)
                    {
                        if (!tilesetCompletenessChecklist[j])
                        {
                            Debug.LogWarning ("Configuration index " + j + ", linked to byte " + configurationBytes[j] + ", which can be represented as " + TilesetUtility.GetStringFromConfiguration (TilesetUtility.GetConfigurationFromByte (configurationBytes[j])) + " is missing");
                            missingConfigurationCount += 1;
                            valid = false;
                        }
                    }

                    if (!valid)
                    {
                        Debug.LogWarning ("Tileset " + t + ": " + tileset.name + " | Is missing a number of configurations: " + missingConfigurationCount);
                        // DestroyImmediate (tilesetHolder);
                    }
                }
            }
        }




        private int replacementCounter = 0;

        // [ResponsiveButtonGroup ("Process", DefaultButtonSize = ButtonSizes.Medium, UniformLayout = true)]
        [Button ("Replace materials", ButtonSizes.Medium)]
        private void ReplaceMaterials ()
        {
            for (int t = 0; t < tilesets.Count; ++t)
            {
                var tileset = tilesets[t];

                bool useBlocks = tileset.loadBlocks && tileset.holderBlocksSplit;

                if (!useBlocks)
                    continue;

                replacementCounter = 0;

                if (useBlocks)
                {
                    ReplaceMaterialsInChildren (tileset.holderBlocksSplit, tileset.materialFallback);
                }

                Debug.Log ("Replaced materials on " + replacementCounter + " objects");
            }
        }

        public void ReplaceMaterialsInChildren (Transform parent, Material fallback)
        {
            MeshRenderer mr = parent.GetComponent<MeshRenderer> ();
            if (mr != null)
            {
                Material[] materialsCopy = new Material[mr.sharedMaterials.Length];
                for (int i = 0; i < mr.sharedMaterials.Length; ++i)
                {
                    if (mr.sharedMaterials[i] == null)
                    {
                        Debug.LogWarning ("ReplaceMateriaInTarget | Encountered a null reference in the materials array of " + parent.name);
                        materialsCopy[i] = materialReplacements[0].replacementMaterial;
                        continue;
                    }

                    materialsCopy[i] = mr.sharedMaterials[i];
                    if (materialsCopy[i].mainTexture == null)
                    {
                        materialsCopy[i] = fallback;
                    }
                    else
                    {
                        for (int m = 0; m < materialReplacements.Count; ++m)
                        {
                            // if (string.Equals (materialsCopy[i].name, materialReplacements[m].undesiredMaterials[u].name))
                            if (materialsCopy[i].mainTexture.name.Contains (materialReplacements[m].textureName))
                            {
                                // Debug.Log ("TG | ReplaceMateriaInTarget | Found match in " + target.name + " on " + maps[m].undesiredMaterialNames[u]);
                                materialsCopy[i] = materialReplacements[m].replacementMaterial;
                                break;
                            }
                        }
                    }
                }

                mr.materials = materialsCopy;
                replacementCounter += 1;
            }

            for (int i = 0; i < parent.childCount; ++i)
            {
                Transform child = parent.GetChild (i);
                ReplaceMaterialsInChildren (child, fallback);
            }
        }




        // [ResponsiveButtonGroup ("Process", DefaultButtonSize = ButtonSizes.Medium, UniformLayout = true)]
        [Button ("Generate shared materials", ButtonSizes.Medium)]
        private void GenerateSharedMaterials ()
        {
            for (int t = 0; t < tilesets.Count; ++t)
            {
                var tileset = tilesets[t];

                if (tileset.holderRootSplit == null || (!tileset.loadBlocks))
                    continue;

                MeshFilter[] filters = tileset.holderRootSplit.GetComponentsInChildren<MeshFilter> (true);
                List<Material> materialsForArrayCollapse = new List<Material> ();
                List<Material> materialsWithCustomShaders = new List<Material> ();

                for (int f = 0; f < filters.Length; ++f)
                {
                    var filter = filters[f];
                    var sharedMaterials = filter.gameObject.GetComponent<MeshRenderer> ().sharedMaterials;

                    for (int m = 0; m < sharedMaterials.Length; ++m)
                    {
                        var sharedMaterial = sharedMaterials[m];

                        if (sharedMaterial == null)
                        {
                            Debug.LogError ($"Shared material {m} on filter {f} ({filter.gameObject.name}) is null", filter.gameObject);
                            continue;
                        }

                        if (sharedMaterial.shader == null)
                        {
                            Debug.LogError ($"Shared material {m} on filter {f} ({filter.gameObject.name}) has null shader", filter.gameObject);
                            continue;
                        }

                        if (string.Equals (sharedMaterial.shader.name, texArraySourceShaderName))
                        {
                            if (!materialsForArrayCollapse.Contains (sharedMaterial))
                            {
                                Debug.Log ("Tileset " + t + ": " + tileset.name + " | Material " + sharedMaterial.name + 
                                    " has array-compatible shader, adding it | Materials collected so far: " + materialsForArrayCollapse.Count);
                                materialsForArrayCollapse.Add (sharedMaterial);
                            }
                        }
                        else
                        {
                            if (!materialsWithCustomShaders.Contains (sharedMaterial))
                            {
                                Debug.LogWarning ("Tileset " + t + ": " + tileset.name + " | Material " + sharedMaterial.name + " us using a custom shader, skipping it");
                                materialsWithCustomShaders.Add (sharedMaterial);
                            }
                        }
                    }
                }

                if (materialsForArrayCollapse.Count <= 1)
                {
                    Debug.LogWarning ("Tileset " + t + ": " + tileset.name + " | Cancelling array collapse operation scheduled for this holder due to number of materials being too low");
                    return;
                }

                Debug.Log ("Tileset " + t + ": " + tileset.name + " | Proceeding to generate material " + 
                    " | Materials found suitable for array collapse: " + materialsForArrayCollapse.Count + 
                    " | Materials with custom shaders: " + materialsWithCustomShaders.Count);

                // We delay instantitation of these objects until we encounter a texture - so that we can copy its format
                Texture2DArray textureArrayAH = null;
                Texture2DArray textureArrayMSEO = null;

                for (int m = 0; m < materialsForArrayCollapse.Count; ++m)
                {
                    var material = materialsForArrayCollapse[m];
                    var textureAH = material.GetTexture ("_MainTex") as Texture2D;
                    var textureMSEO = material.GetTexture ("_MSEO") as Texture2D;

                    if (textureArrayAH == null)
                    {
                        Debug.Log ("Creating AH texture array with size of " + materialsForArrayCollapse.Count + ", format " + textureAH.format + " and dimensions " + textureAH.width + "x" + textureAH.height);
                        textureArrayAH = new Texture2DArray (textureAH.width, textureAH.height, materialsForArrayCollapse.Count, textureAH.format, true);
                    }

                    if (textureArrayMSEO == null)
                    {
                        Debug.Log ("Creating MSEO texture array with size of " + materialsForArrayCollapse.Count + ", format " + textureMSEO.format + " and dimensions " + textureMSEO.width + "x" + textureMSEO.height);
                        textureArrayMSEO = new Texture2DArray (textureMSEO.width, textureMSEO.height, materialsForArrayCollapse.Count, textureMSEO.format, true);
                    }

                    if (textureAH != null)
                    {
                        if (textureAH.width == textureArrayAH.width && textureAH.height == textureArrayAH.height)
                            Graphics.CopyTexture (textureAH, 0, textureArrayAH, m);
                        else
                            Debug.LogWarning ("Skipping array write for AH texture " + m + " due to mismatched dimensions: " + textureAH.width + "x" + textureAH.height);
                    }
                    else
                        Debug.LogWarning ("Skipping array write for missing AH texture " + m);

                    if (textureMSEO != null)
                    {
                        if (textureMSEO.width == textureArrayMSEO.width && textureMSEO.height == textureArrayMSEO.height)
                            Graphics.CopyTexture (textureMSEO, 0, textureArrayMSEO, m);
                        else
                            Debug.LogWarning ("Skipping array write for MSEO texture " + m + " due to mismatched dimensions: " + textureMSEO.width + "x" + textureMSEO.height);
                    }
                    else
                        Debug.LogWarning ("Skipping array write for missing MSEO texture " + m);
                }

                string materialAssetPath = folderTilesetExport + "/tileset_" + tileset.name + "/";

                Texture2DArray textureArrayAHFromAsset = null;
                if (textureArrayAH != null)
                {
                    textureArrayAH.Apply (false, false);
                    textureArrayAHFromAsset = UtilityAssetDatabase.CreateAssetSafely (textureArrayAH, materialAssetPath + "materialTexArrayAH.asset");
                    textureArrayAHFromAsset.Apply (false, true);
                }

                Texture2DArray textureArrayMSEOFromAsset = null;
                if (textureArrayMSEO != null)
                {
                    textureArrayMSEO.Apply (false, false);
                    textureArrayMSEOFromAsset = UtilityAssetDatabase.CreateAssetSafely (textureArrayMSEO, materialAssetPath + "materialTexArrayMSEO.asset");
                    textureArrayMSEOFromAsset.Apply (false, true);
                }

                var materialWithArrays = new Material (materialTemplate);
                var materialFromAsset = UtilityAssetDatabase.CreateAssetSafely (materialWithArrays, materialAssetPath + "materialWithArrays.mat");

                if (textureArrayAHFromAsset != null)
                    materialFromAsset.SetTexture (Shader.PropertyToID ("_TexArrayAH"), textureArrayAHFromAsset);
                if (textureArrayMSEOFromAsset != null)
                    materialFromAsset.SetTexture (Shader.PropertyToID ("_TexArrayMSEO"), textureArrayMSEOFromAsset);

                var cnt = new MaterialSerializationHelper.VectorArrayContainer ();
                cnt.sourceNameX = "_SmoothnessMin";
                cnt.sourceNameY = "_SmoothnessMed";
                cnt.sourceNameZ = "_SmoothnessMax";
                cnt.sourceNameW = "_WorldSpaceUVOverride";
                cnt.targetName = "_PropertyArray";

                tileset.materialHelper = ScriptableObject.CreateInstance<MaterialSerializationHelper> ();
                tileset.materialHelper.sourceMaterials = materialsForArrayCollapse;
                tileset.materialHelper.targetMaterial = materialFromAsset;
                tileset.materialHelper.vectorArrays = new List<MaterialSerializationHelper.VectorArrayContainer> ();
                tileset.materialHelper.vectorArrays.Add (cnt);
                tileset.materialHelper.FillFromSources ();
                tileset.materialHelper.ApplyToTarget ();

                AssetDatabase.CreateAsset (tileset.materialHelper, materialAssetPath + "materialData.asset");
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh (ImportAssetOptions.ForceSynchronousImport);

                Debug.Log ("Successfully created a combined material from " + materialsForArrayCollapse.Count + " source materials | Helper data generated", tileset.materialHelper);
            }
        }




        // [ResponsiveButtonGroup ("Process", DefaultButtonSize = ButtonSizes.Medium, UniformLayout = true)]
        [Button ("Merge objects", ButtonSizes.Medium)]
        private void MergeObjects ()
        {
            UtilityGameObjects.ClearChildren (GetHolderTilesetsMerged ());

            GetHolderTilesetsSplit ().gameObject.SetActive (true);
            GetHolderTilesetsMerged ().gameObject.SetActive (true);

            for (int t = 0; t < tilesets.Count; ++t)
            {
                var tileset = tilesets[t];

                bool useBlocks = tileset.loadBlocks && tileset.holderBlocksSplit;
                if (!useBlocks)
                    continue;

                if (tileset.holderRootMerged == null)
                {
                    tileset.holderRootMerged = new GameObject (tileset.name).transform;
                    tileset.holderRootMerged.parent = GetHolderTilesetsMerged ();
                    tileset.holderRootMerged.SetLocalTransformationToZero ();
                }

                if (useBlocks)
                {
                    if (tileset.holderBlocksMerged == null)
                    {
                        tileset.holderBlocksMerged = new GameObject (tileset.name + "_blocks").transform;
                        tileset.holderBlocksMerged.parent = tileset.holderRootMerged;
                        tileset.holderBlocksMerged.SetLocalTransformationToZero ();
                    }

                    MergeChildren (tileset.holderBlocksSplit, tileset.holderBlocksMerged, tileset.materialHelper);
                }
            }

            GetHolderTilesetsSplit ().gameObject.SetActive (false);
        }

        private void MergeChildren (Transform holderSplit, Transform holderMerged, MaterialSerializationHelper materialHelper)
        {
            Debug.Log ("Started merge of children in object " + holderSplit.name);

            for (int b = 0; b < holderSplit.childCount; ++b)
                MergeObject (holderSplit.GetChild (b), holderMerged, materialHelper);
        }

        public void MergeObject (Transform objectSplit, Transform holderMerged, MaterialSerializationHelper materialHelper)
        {
            // Lazy compensation for offsets through reparenting
            Transform holderMergedParent = holderMerged.parent;
            Vector3 holderMergedLocalPosition = holderMerged.localPosition;
            Vector3 objectSplitLocalPosition = objectSplit.localPosition;
            holderMerged.position = objectSplit.position = Vector3.zero;

            var blockSemiMerged = CombineMeshes.CombineChildren (objectSplit, holderMerged);
            var filters = blockSemiMerged.GetComponentsInChildren<MeshFilter> ();
            var combineInstances = new CombineInstance[filters.Length];
            var materials = new List<Material> ();

            for (int i = 0; i < filters.Length; ++i)
            {
                combineInstances[i].mesh = filters[i].sharedMesh;
                combineInstances[i].transform = filters[i].transform.worldToLocalMatrix;

                // That old part is a bit weird: recheck if this has any effect, and if it does, if this should use .sharedMaterials instead!
                Material material = filters[i].gameObject.GetComponent<MeshRenderer> ().sharedMaterial;
                if (!materials.Contains (material))
                    materials.Add (material);
            }

            Mesh mesh = new Mesh ();
            mesh.CombineMeshes (combineInstances, false);

            var filter = blockSemiMerged.AddComponent<MeshFilter> ();
            var renderer = blockSemiMerged.AddComponent<MeshRenderer> ();
            renderer.sharedMaterials = materials.ToArray ();

            // Time to bake materials into vertex colors
            if (materialHelper != null)
            {
                Vector2[] uv2 = new Vector2[mesh.vertexCount];
                Material[] materialsInMergedMesh = renderer.sharedMaterials;

                Debug.Log ("Writing data to mesh for object " + blockSemiMerged.name + " Source materials: " + materialHelper.sourceMaterials.Count +
                    " | Materials in merged mesh: " + materialsInMergedMesh.Length + " | Submeshes in merged mesh: " + mesh.subMeshCount);

                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; ++submeshIndex)
                {
                    int[] trianglesOfSubmesh = mesh.GetTriangles (submeshIndex);
                    Material materialForSubmesh = materialsInMergedMesh[submeshIndex];
                    Vector2 uv2ForSubmesh = new Vector2 (0f, 0f);

                    if (materialHelper.sourceMaterials.Contains (materialForSubmesh))
                    {
                        int materialIndex = materialHelper.sourceMaterials.IndexOf (materialForSubmesh);
                        uv2ForSubmesh = new Vector2 (materialIndex, 0f); // + 0.5f is added to move index to the middle of texture array index range
                        Debug.Log ("TG | MergeObject | " + blockSemiMerged.name + " | Successfully found tex-array index " + materialIndex + " for material named " + materialForSubmesh.name + " | Resulting UV2: " + uv2ForSubmesh);
                    }
                    else
                        Debug.LogWarning ("Object " + blockSemiMerged.name + " | Material named " + materialForSubmesh + " was not found in tex-array index dictionary");

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
                    Debug.LogWarning ("Object " + blockSemiMerged.name + " had non-centered bounds origin: " + mesh.bounds + " | New bounds: " + bounds, filter.gameObject);
                    mesh.bounds = bounds;
                }

                // List<int> trianglesForArraySubmesh = new List<int> ();
                var materialTrianglePairs = new Dictionary<Material, List<int>> ();
                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; ++submeshIndex)
                {
                    int[] trianglesOfSubmesh = mesh.GetTriangles (submeshIndex);
                    var materialForSubmesh = materialsInMergedMesh[submeshIndex];

                    if (string.Equals (materialForSubmesh.shader.name, texArraySourceShaderName))
                    {
                        // trianglesForArraySubmesh.AddRange (trianglesOfSubmesh);
                        Debug.Log ("Encountered a standard non-merged tileset shader in material for submesh " + submeshIndex + ", proceeding normally");
                        if (!materialTrianglePairs.ContainsKey (materialHelper.targetMaterial))
                            materialTrianglePairs.Add (materialHelper.targetMaterial, new List<int> ());
                        materialTrianglePairs[materialHelper.targetMaterial].AddRange (trianglesOfSubmesh);
                    }
                    else
                    {
                        Debug.LogWarning ("Encountered a nonstandard shader in the material mapped to submesh " + submeshIndex + " called " + materialForSubmesh.name + ": " + materialForSubmesh.shader.name);
                        if (!materialTrianglePairs.ContainsKey (materialForSubmesh))
                            materialTrianglePairs.Add (materialForSubmesh, new List<int> ());
                        materialTrianglePairs[materialForSubmesh].AddRange (trianglesOfSubmesh);
                    }
                }

                Debug.Log ("Preparing to assign " + materialTrianglePairs.Count + " material-submesh pairs to the merged mesh");
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
            holderMerged.localPosition = holderMergedLocalPosition;
            objectSplit.localPosition = blockSemiMerged.transform.localPosition = objectSplitLocalPosition;
            blockSemiMerged.transform.localRotation = Quaternion.identity;
        }



        [Button ("Save objects", ButtonSizes.Medium)]
        public void SaveEverything ()
        {
            Debug.Log ("Saving objects");

            if (folderTilesetExport.EndsWith ("/"))
                folderTilesetImport = folderTilesetImport.TrimEnd ('/');

            for (int t = 0; t < tilesets.Count; ++t)
            {
                var tileset = tilesets[t];

                bool useBlocks = tileset.loadBlocks && tileset.holderBlocksMerged;
                if (!useBlocks)
                    continue;

                if (useBlocks)
                {
                    SaveObjectsInHolder (tileset.holderBlocksMerged, tileset.name, "blocks", false);
                }
            }

            ResourceDatabaseManager.RebuildDatabase ();
            AreaTilesetHelper.LoadDatabase ();

            AreaManager[] ams = FindObjectsOfType<AreaManager> ();
            for (int i = 0; i < ams.Length; ++i)
                ams[i].RebuildEverything ();

            EditorUtility.ClearProgressBar ();
            UtilityGameObjects.ClearChildren (GetHolderTilesetsCells ());
        }

        private void SaveObjectsInHolder (Transform holderMerged, string tilesetName, string subfolderName, bool multiblockMode)
        {
            AssetDatabase.StartAssetEditing ();
        
            Debug.Log ("Saving assets from holder " + holderMerged.name + " | Child count: " + holderMerged.childCount);

            string progressBarHeader = "Saving " + subfolderName;
            float progressBarPercentage = 0.0f;
            EditorUtility.DisplayProgressBar (progressBarHeader, "Starting...", progressBarPercentage);

            for (int b = 0; b < holderMerged.childCount; ++b)
            {
                var child = holderMerged.GetChild (b).gameObject;
                string pathShared = string.Format ("{0}/tileset_{1}/{2}/", folderTilesetExport, tilesetName, subfolderName);

                progressBarPercentage = Mathf.Min (1f, (float)(b + 1) / (float)holderMerged.childCount);
                string progressDesc = (int)(progressBarPercentage * 100f) + "% done | Processing object: " + child.name;
                EditorUtility.DisplayProgressBar (progressBarHeader, progressDesc, progressBarPercentage);
                SaveObject (child, pathShared);
            }
            
            AssetDatabase.StopAssetEditing ();

            //exportProgress = 0f;
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh (ImportAssetOptions.ForceSynchronousImport);

            EditorUtility.ClearProgressBar ();
        }

        private void SaveObject (GameObject target, string exportPath)
        {
            var  filter = target.GetComponent<MeshFilter> ();
            string meshPath = exportPath + target.name + ".asset";
            var  meshFromAsset = UtilityAssetDatabase.CreateAssetSafely (filter.sharedMesh, meshPath);

            if (meshFromAsset != null)
                filter.sharedMesh = meshFromAsset;
            else
                Debug.LogWarning ("Failed to load mesh " + exportPath + target.name + ".asset");

            string prefabPath = exportPath + target.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset (target, prefabPath);
        }


        /*
        

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

        public void SaveObject (GameObject objectMerged, string exportPath, FileGenerationMode fileGenerationMode)
        {
            Debug.Log ("TG | SaveBlock | Exporting asset to path: " + exportPath);

            if (objectMerged == null)
            {
                Debug.LogWarning ("TG | SaveBlock | Source transform is null");
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

                if (exportMultiblocks && tileset.multiblocksList != null)
                {
                    for (int i = 0; i < tileset.multiblocksList.Count; ++i)
                    {
                        AreaMultiblock multiblock = tileset.multiblocksList[i];
                        GameObject prefabInstance = PrefabUtility.InstantiatePrefab (multiblock.gameObject) as GameObject;

                        prefabInstance.transform.parent = GetHolderTilesetsMergedMultiBlocks ();
                        prefabInstance.transform.localPosition = new Vector3 (tilesetIndex * 24f, 0f, i * 12f);
                    }
                }

                tilesetIndex += 1;
            }
        }


        // Deco


        private string blockFramePrefabPath = "Assets/Content/Objects/TilesetTest/EditorAssets/spot_frame";
        private GameObject blockFramePrefab = null;

        public GameObject GetBlockFramePrefab ()
        {
            if (blockFramePrefab == null)
                blockFramePrefab = AssetDatabase.LoadAssetAtPath (blockFramePrefabPath, typeof (GameObject)) as GameObject;
            return blockFramePrefab;
        }

        */

        // Holders

        [HideInInspector] public Transform holderTilesetsCells;
        [HideInInspector] public Transform holderTilesetsSplit;
        [HideInInspector] public Transform holderTilesetsMerged;

        [HideInInspector] public string holderTilesetsCellsName = "GeneratorHolder_Cells";
        [HideInInspector] public string holderTilesetsSplitName = "GeneratorHolder_SplitBlocks";
        [HideInInspector] public string holderTilesetsMergedName = "GeneratorHolder_MergedBlocks";

        public Transform GetHolderTilesetsCells () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsCells, holderTilesetsCellsName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsSplit () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsSplit, holderTilesetsSplitName, HideFlags.DontSave, transform.localPosition); }
        public Transform GetHolderTilesetsMerged () { return UtilityGameObjects.GetTransformSafely (ref holderTilesetsMerged, holderTilesetsMergedName, HideFlags.DontSave, transform.localPosition); }

        #endif
    }
}