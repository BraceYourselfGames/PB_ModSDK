using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class PhantomAssetTools : OdinEditorWindow
{
	[MenuItem("PB Mod SDK/Item visuals/Asset tools")]
	private static void OpenWindow()
    {
        var instance = (PhantomAssetTools)GetWindow(typeof(PhantomAssetTools), false, "Phantom Asset Tools");
        instance.maxSize = new Vector2(700f, 500f);
        instance.minSize = new Vector2(500f, 300f);

        instance.Show();
    }

    // Material Tools Tab ========================================================================================
    // Batch Create Materials ----------------------------
    [TabGroup("Material Tools")]
    [Title("Batch Create Materials For Armor")]
    [DetailedInfoBox("Creates a Material asset for each selected armor Model asset in batch.",
    "Creates a Material asset for each selected armor Model asset in batch.\n\n" +
    "The script will create a .mat file next to each .fbx file. It also looks for albedo, mseo and normal\n" +
    "textures that follow model_name_albedo.png etc. naming convention and applies them automatically.")]
    [Button(ButtonSizes.Large)]
    [PropertyOrder(0)]
    public void CreateMaterialsForSelectedArmor() { CreateMaterialsForSelectedArmor_Func(); }
    // --------------------------------------------------

    // Batch Apply Materials ----------------------------
    [TabGroup("Material Tools")]
    [Title("Batch Apply Materials")]
    [PropertyOrder(1)]
    [DetailedInfoBox("Applies Material assets of your choice to selected Model assets in batch.",
    "Applies Material assets of your choice to selected Model assets in batch.\n\n" +
    "Make sure you specify the Model's material name you want your material to be applied to.\n" +
    "Select one or more Model assets (FBX files) in the Project tab you wish to edit first.\n\n" + 
    "When Armor Mat is enabled the script will use an existing armor material that is located in the same folder as the armor fbx and has the same name.")]
    [TableList]
    [OnInspectorInit("PopulateMatDataList")]
    public List<ModelMaterialData> ModelMaterialDataList = new List<ModelMaterialData>();

    [TabGroup("Material Tools")]
    [Button(ButtonSizes.Large)]
    [PropertyOrder(2)]
    public void ApplyMaterialsAndReimport() { ApplyMaterialsAndReimport_Func(); }
    // --------------------------------------------------

    // Batch Apply Brightness And Saturation ------------
    [TabGroup("Material Tools")]
    [Title("Batch Apply Brightness And Saturation to Materials")]
    [DetailedInfoBox("Applies brightness and saturation values to selected materials.",
    "Applies proper brightness and saturation values to selected armor and weapon materials.\n\n" +
    "Select several material assets and the script will apply proper brightness and saturation values to your materials" +
    "(proper - as in, based on your material names, it will apply certain value to head armor, certain value to thigh armor. Same for weapons, etc.)\n" +
    "Keep in mind it will only work for mech part shader-based materials that have part_ or wpn_ in their names!\n\n" +
    "You can also select just one folder and the script will recursively find all relevant materials inside.")]
    [Button(ButtonSizes.Large)]
    [PropertyOrder(3)]
    public void ApplyAlbedoBrightnessAndSaturationToSelectedMaterials() { ApplyBrightnessSaturationToMaterials_Func(); }

    // Batch Apply Brightness And Saturation ------------
    [TabGroup("Material Tools")]
    [Title("Fix Zero Stripping Direction Value")]
    [Button(ButtonSizes.Large)]
    [PropertyOrder(4)]
    public void FixZeroStrippingDirectionValue() { FixZeroStrippingDirectionValue_Func(); }
    // --------------------------------------------------

    // Prefab Tools Tab ==========================================================================================
    // Create Item Prefabs -------------------------------
    [TabGroup("Prefab Tools")]
    [Title("Create Item Prefabs")]
    [DetailedInfoBox("Creates Item prefabs out of selected Model assets based on current Prefab Mode.",
    "Creates Item prefabs out of selected Model assets based on current Prefab Mode.\n\n" +
    "Select one or more Model assets (FBX files) in the Project tab first.")]
    [PropertyOrder(3)]
    public PrefabModeEnum PrefabMode;
    
    [TabGroup("Prefab Tools")]
    [ShowIf("PrefabMode", PrefabModeEnum.Weapon)]
    [PropertyOrder(4)]
    public WeaponModeEnum WeaponMode;
    
    [TabGroup("Prefab Tools")]
    [ShowIf("PrefabMode", PrefabModeEnum.Weapon)]
    [PropertyOrder(5)]
    public bool AddFXTransform = true;

    [TabGroup("Prefab Tools")]
    [ShowIf("PrefabMode", PrefabModeEnum.Armor)]
    [PropertyOrder(5)]
    public bool ReplaceBaseSkeleton;

    [TabGroup("Prefab Tools")]
    [Button(ButtonSizes.Large), PropertySpace]
    [PropertyOrder(6)]
    public void CreatePrefabsOutOfSelectedModels() { CreatePrefabsOutOfSelectedModels_Func(); }
    // ===========================================================================================================

    public enum PrefabModeEnum {Armor, Weapon, Prop};

    public enum WeaponModeEnum {Firearm, Melee};


    // Batch Create Materials ----------------------------
    private void CreateMaterialsForSelectedArmor_Func()
    {
        var modelImporters = LoadModelImportersFromSelection();

        foreach (var mi in modelImporters)
        {
            // Grab the folder path of the model in the array
            string modelFolderPath = Path.GetDirectoryName(mi.assetPath);
            string modelName = Path.GetFileNameWithoutExtension(mi.assetPath);

            Material material = new Material(Shader.Find("Hardsurface/Parts/Base (mech)"));

            Texture2D textureAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>( Path.Combine(modelFolderPath, modelName + "_albedo.png") );
            if (textureAlbedo != null) material.SetTexture("_MainTex", textureAlbedo);
            Texture2D textureMSEO = AssetDatabase.LoadAssetAtPath<Texture2D>( Path.Combine(modelFolderPath, modelName + "_mseo.png") );
            if (textureMSEO != null) material.SetTexture("_MSEOTex", textureMSEO);
            Texture2D textureNormal = AssetDatabase.LoadAssetAtPath<Texture2D>( Path.Combine(modelFolderPath, modelName + "_normal.png") );
            if (textureNormal != null) material.SetTexture("_NormalTex", textureNormal);

            // Need to set float as well to reflect changes in the inspector
            material.SetFloat("_UseMSEO", 1f);
            material.EnableKeyword("PART_USE_MSEO");
            material.SetFloat("_UseArrays", 1f);
            material.EnableKeyword("PART_USE_ARRAYS");

            material.SetColor("_ColorPrimary", new Color(0.2473333f, 0.3147878f, 0.371f));
            material.SetColor("_ColorSecondary", new Color(0.493346f, 0.5082307f, 0.5379999f));
            material.SetColor("_ColorTertiary", new Color(0.8380001f, 0.3339957f, 0.136594f));

            material.SetFloat("_AlbedoMaskWear", 0.4f);
            material.SetFloat("_AlbedoMaskWearMultiplier", 1.5f);
            material.SetFloat("_AlbedoMaskWearPower", 2f);

            SetAlbedoBrightAndSatBasedOnAssetName(material, modelName);

            material.SetVector("_SmoothnessMin", new Vector4(0.0f, 0.5f, 0.65f, 0f));
            material.SetVector("_SmoothnessMed", new Vector4(0.0f, 0.5f, 0.65f, 0f));
            material.SetVector("_SmoothnessMax", new Vector4(0.0f, 0.5f, 0.65f, 0f));

            AssetDatabase.CreateAsset(material, Path.Combine(modelFolderPath, modelName + ".mat"));
        }

    }
    // --------------------------------------------------


    // Batch Apply Materials ----------------------------
    private void PopulateMatDataList()
    {
        ModelMaterialData matData;

        if (ModelMaterialDataList.Count > 0)
            return;

        matData = new ModelMaterialData();
        matData.MaterialName = "surface";
        ModelMaterialDataList.Add(matData);

        matData = new ModelMaterialData();
        matData.MaterialName = "decals";
        ModelMaterialDataList.Add(matData);
    }
    private Material matToApply;
    private void ApplyMaterialsAndReimport_Func()
    {
        var modelImporters = LoadModelImportersFromSelection();

        foreach (var mi in modelImporters)
        {
            // Grab the folder path of the model in the array
            string modelFolderPath = Path.GetDirectoryName(mi.assetPath);
            string modelName = Path.GetFileNameWithoutExtension(mi.assetPath);

            using (var so = new SerializedObject(mi))
            {
                // Looks like m_Materials serves as an indexed list of materials present in the model
                var materials = so.FindProperty("m_Materials");
                // Then external objects are the links to material assets in the project (?)
                var externalObjects = so.FindProperty("m_ExternalObjects");

                // Go through list of materials defined in the model
                for (int materialIndex = 0; materialIndex < materials.arraySize; materialIndex ++)
                {
                    var id = materials.GetArrayElementAtIndex(materialIndex);
                    var name = id.FindPropertyRelative("name").stringValue;
                    var type = id.FindPropertyRelative("type").stringValue;
                    var assembly = id.FindPropertyRelative("assembly").stringValue;

                    matToApply = null;

                    // Check the current model material's name against items in the user defined material list in the editor GUI
                    // if a material with matching name found - store a material asset reference and bail out of the loop
                    foreach (var matData in ModelMaterialDataList)
                    {
                        if (name == matData.MaterialName)
                        {
                            // If Use Existing Material is checked - try to load a material with the same name as the model asset, located next to the model
                            if (matData.ArmorMat)
                            {
                                Material mat = AssetDatabase.LoadAssetAtPath<Material>( Path.Combine(modelFolderPath, modelName + ".mat") );
                                if (mat != null) matToApply = mat;
                            }
                            else
                            {
                                matToApply = matData.MaterialAsset;
                            }

                            break;
                        }
                    }

                    // Only update material field with a specified name
                    if (matToApply != null)
                    {
                        SerializedProperty materialProperty = null;

                        // Go through a list of external objects 'linked' to the model (in our case: all material assets assigned within material list)
                        for (int externalObjectIndex = 0; externalObjectIndex < externalObjects.arraySize; externalObjectIndex ++)
                        {
                            var currentSerializedProperty = externalObjects.GetArrayElementAtIndex(externalObjectIndex);
                            var externalName = currentSerializedProperty.FindPropertyRelative("first.name").stringValue;
                            var externalType = currentSerializedProperty.FindPropertyRelative("first.type").stringValue;
                            // If external object's name and type match the name and type of an item in our material list:
                            // populate materialProperty with the material asset reference stored in the "second" field (?)
                            if (externalType == type && externalName == name)
                            {
                                materialProperty = currentSerializedProperty.FindPropertyRelative("second");
                                break;
                            }
                        }

                        // If no external object was found with such name and type - time to create a new one
                        // This is basically creating a new 'link' to the material asset through script
                        if (materialProperty == null)
                        {
                            var lastIndex = externalObjects.arraySize++;
                            var currentSerializedProperty = externalObjects.GetArrayElementAtIndex(lastIndex);
                            currentSerializedProperty.FindPropertyRelative("first.name").stringValue = name;
                            currentSerializedProperty.FindPropertyRelative("first.type").stringValue = type;
                            currentSerializedProperty.FindPropertyRelative("first.assembly").stringValue = assembly;
                            currentSerializedProperty.FindPropertyRelative("second").objectReferenceValue = matToApply;
                        }
                        // If an external object was found - just update its material reference
                        else
                        {
                            materialProperty.objectReferenceValue = matToApply;
                        }
                    }

                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            mi.SaveAndReimport();
        }
    }
    // --------------------------------------------------


    // Create Item Prefabs -------------------------------
    private string partNameSubstringToCheck;
    private Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
    private void CreatePrefabsOutOfSelectedModels_Func()
    {
        var modelImporters = LoadModelImportersFromSelection();
        // Grab the folder path of the first model in the array
        string selectedModelFolderPath = Path.GetDirectoryName(modelImporters[0].assetPath);
        // Ask user where to save the prefabs
        string pathToSaveToFull = EditorUtility.SaveFolderPanel("Choose where to save the prefabs", selectedModelFolderPath, "");
        string pathToSaveTo = pathToSaveToFull.Substring(pathToSaveToFull.IndexOf("Assets/"));

        foreach (var mi in modelImporters)
        {
            // Load model asset from its asset path
            var modelRootGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(mi.assetPath);
            // Instantiate model asset
            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelRootGO);

            if (PrefabMode == PrefabModeEnum.Prop)
            {
                // Create empty GameObject that will serve as a container for our prop
                var prefabRoot = new GameObject();
                prefabRoot.name = modelRootGO.name;
                // Put the prop GO on a "Prop" layer
                prefabRoot.layer = 24;

                // Parent the model asset under prefab root GameObject
                modelInstance.transform.SetParent(prefabRoot.transform, false);

                var areaPropComp = prefabRoot.AddComponent<Area.AreaProp>();
                // Call a built-in function to collect all the model's MeshRenderers in a list
                areaPropComp.FillReferences();

                BoxCollider propBoxCollider = prefabRoot.AddComponent<BoxCollider>();
                Bounds propBounds = GetMaxBounds(modelInstance);
                propBoxCollider.center = propBounds.center;
                propBoxCollider.size = propBounds.size;

                // Call a bult-in function to collect the newly created box collider
                areaPropComp.CollectColliderAndRigidbody();

                // Save the resulting hierarchy as a prefab
                //PrefabUtility.SaveAsPrefabAsset(prefabRoot, Path.Combine(pathToSaveTo, prefabRoot.name + ".prefab"));
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, pathToSaveTo +"/" + prefabRoot.name + ".prefab");
                // Delete the hierarchy in the scene we were working with before saving it into the prefab
                DestroyImmediate(prefabRoot, false);
            }
            else
            {
                // Add ItemVisual component to the root GameObject
                //ItemVisual itemVisualComp = prefabRoot.AddComponent<ItemVisual>();
                ItemVisual itemVisualComp = modelInstance.AddComponent<ItemVisual>();
                // Call a built-in function to collect all the model's MeshRenderers in a list
                itemVisualComp.CollectRenderers();

                // For Weapons - Additionally create holder_fx transform if model asset has "_front" or "_top" in its name
                if (PrefabMode == PrefabModeEnum.Weapon)
                {
                    if (WeaponMode == WeaponModeEnum.Firearm)
                    {
                        partNameSubstringToCheck = "_front";
                    }
                    else if (WeaponMode == WeaponModeEnum.Melee)
                    {
                        partNameSubstringToCheck = "_top";
                    }

                    if (AddFXTransform && modelRootGO.name.Contains(partNameSubstringToCheck))
                    {
                        GameObject weaponFXTransform = new GameObject();
                        weaponFXTransform.name = "holder_fx";
                        weaponFXTransform.transform.SetParent(modelInstance.transform, false);
                        // Put holder_fx on "Editor" layer as a temporary measure to make sure any
                        // colliders attached to it don't interact with other physics objects
                        weaponFXTransform.layer = 15;

                        itemVisualComp.activationUsed = true;
                        itemVisualComp.AddActivationLink(weaponFXTransform.transform);

                        if (WeaponMode == WeaponModeEnum.Melee)
                        {

                            // NOTE: this will be replaced with built-in ItemVisual component logic later

                            // Add box collider to melee's FX transform - it's used to define the location and size of blade trail effect
                            BoxCollider fxMeleeBox = weaponFXTransform.AddComponent<BoxCollider>();                 
                            fxMeleeBox.isTrigger = true;
                            // Look for 'surface' mesh in our model, if one found - position the box collider in the middle on Y
                            // and at the edge of the melee's blade + set box size the same as mesh bounds (user is expected to tweak all of that additoinally)
                            MeshRenderer surfaceMesh = modelInstance.transform.Find("surface").GetComponent<MeshRenderer>();
                            if (surfaceMesh != null)
                            {
                                weaponFXTransform.transform.position = new Vector3(0, surfaceMesh.bounds.center.y, surfaceMesh.bounds.max.z);
                                fxMeleeBox.center = new Vector3(0, 0, -surfaceMesh.bounds.extents.z);
                                fxMeleeBox.size = surfaceMesh.bounds.size;
                            }                            
                        }
                    }

                }
                // For Armor - Control IncludeBase parameter if needed
                else if (PrefabMode == PrefabModeEnum.Armor && ReplaceBaseSkeleton)
                {
                    itemVisualComp.includesBase = true;
                }

                // Save the resulting hierarchy as a prefab
                //PrefabUtility.SaveAsPrefabAsset(modelInstance, Path.Combine(pathToSaveTo, modelRootGO.name + ".prefab"));
                PrefabUtility.SaveAsPrefabAsset(modelInstance, pathToSaveTo +"/" + modelRootGO.name + ".prefab");
                // Delete the hierarchy in the scene we were working with before saving it into the prefab
                DestroyImmediate(modelInstance, false);
            }
        }
    }
    // --------------------------------------------------


    // Batch Apply Brightness And Saturation ------------
    private void ApplyBrightnessSaturationToMaterials_Func()
    {
        var materialAssetPaths = GetArmorAndWeaponMaterialAssetPathsFromSelection();

        if (materialAssetPaths.Count > 0)
        {
            foreach (string assetPath in materialAssetPaths)
            {
                // Double check we found a material asset
                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(Material))
                {
                    Material m = (Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                    if (m != null)
                    {
                        SetAlbedoBrightAndSatBasedOnAssetName (m, Path.GetFileNameWithoutExtension(assetPath));
                        EditorUtility.SetDirty(m);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void FixZeroStrippingDirectionValue_Func()
    {
        var materialAssetPaths = GetArmorAndWeaponMaterialAssetPathsFromSelection();

        Vector3 stripParametersCached;

        if (materialAssetPaths.Count > 0)
        {
            foreach (string assetPath in materialAssetPaths)
            {
                // Double check we found a material asset
                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(Material))
                {
                    Material m = (Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                    if (m != null)
                    {
                        // Implicit conversion from Vector4 to Vector3 (w component is irrelevant)
                        stripParametersCached = m.GetVector("_StripParameters");

                        if (stripParametersCached.magnitude < 0.1f)
                        {
                            m.SetVector("_StripParameters", new Vector4 (0, 1, 0, 1));
                        }
                        EditorUtility.SetDirty(m);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // Collect armor and weapon materials into a list of asset paths based on user's selection
    private List<string> GetArmorAndWeaponMaterialAssetPathsFromSelection()
    {
        var materialAssetPaths = new List<string>();

        var guids = Selection.assetGUIDs;

        // If only one element is selected check if its a folder
        if (guids.Length == 1)
        {
            var selectedGuid = guids[0];
            if (selectedGuid != null)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);
                // If its a folder - do recursive material search
                if (Directory.Exists(assetPath))
                {
                    string[] materialAssetsInFolders = Directory.GetFiles(assetPath, "*.mat", SearchOption.AllDirectories);
                    foreach (string matAssetPath in materialAssetsInFolders)
                    {
                        if (IsMaterialValidByName(matAssetPath))
                            materialAssetPaths.Add(matAssetPath);
                    }
                    
                }
                // Otherwise just operate on this one selected material asset
                else
                {
                    if (IsMaterialValidByName(assetPath))
                        materialAssetPaths.Add(assetPath);
                }
            }
        }
        // If more elements are selected - only get material assets from selection
        else
        {
            foreach (var g in guids)
            {
                if (g != null)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(g);
                    if (IsMaterialValidByName(assetPath))
                        materialAssetPaths.Add(assetPath);
                }
            }
        }

        return materialAssetPaths;
    }

    // Check if material name is relevant (i.e. the material is for armor or weapon part)
    private bool IsMaterialValidByName(string materialPath)
    {
        string materialName = Path.GetFileName(materialPath);
        return ( (materialName.StartsWith("part_") || materialName.StartsWith("wpn_")) && materialName.EndsWith(".mat") );
    }
    // --------------------------------------------------

    private List<ModelImporter> LoadModelImportersFromSelection()
    {
        var modelImporters = new List<ModelImporter>();
        var guids = Selection.assetGUIDs;
        foreach (var g in guids)
        {
            if (g != null)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(g); 
                var assetImporter = AssetImporter.GetAtPath(assetPath);
 
                if (assetImporter is ModelImporter model)
                {
                    modelImporters.Add(model);
                }
            }
        }
        return modelImporters;
    }

    private void SetAlbedoBrightAndSatBasedOnAssetName(Material material, string assetName)
    {
        if (assetName.Contains("_arm"))
        {
            material.SetFloat("_AlbedoBrightness", 1.15f);
            material.SetFloat("_AlbedoSaturation", 1.0f);
        }
        else if (assetName.Contains("_foot"))
        {
            material.SetFloat("_AlbedoBrightness", 0.5f);
            material.SetFloat("_AlbedoSaturation", 0.5f);
        }
        else if (assetName.Contains("_head"))
        {
            material.SetFloat("_AlbedoBrightness", 1.2f);
            material.SetFloat("_AlbedoSaturation", 1.0f);
        }
        else if (assetName.Contains("_leg"))
        {
            material.SetFloat("_AlbedoBrightness", 0.7f);
            material.SetFloat("_AlbedoSaturation", 0.7f);
        }
        else if (assetName.Contains("_pelvis"))
        {
            material.SetFloat("_AlbedoBrightness", 0.75f);
            material.SetFloat("_AlbedoSaturation", 0.85f);
        }
        else if (assetName.Contains("_shoulder"))
        {
            material.SetFloat("_AlbedoBrightness", 1.25f);
            material.SetFloat("_AlbedoSaturation", 1.05f);
        }
        else if (assetName.Contains("_thigh"))
        {
            material.SetFloat("_AlbedoBrightness", 0.8f);
            material.SetFloat("_AlbedoSaturation", 1.0f);
        }
        else if (assetName.Contains("_torso"))
        {
            material.SetFloat("_AlbedoBrightness", 1.0f);
            material.SetFloat("_AlbedoSaturation", 1.15f);
        }
        // Added a case for weapons, they just get more brightness
        else if (assetName.Contains("wpn_"))
        {
            material.SetFloat("_AlbedoBrightness", 1.15f);
        }
    }

	
}

[Serializable]
[HideInInspector]
public class ModelMaterialData
{
    [PropertyTooltip("Uses an existing armor material that is located in the same folder as the model asset and has the same name.")]
    [TableColumnWidth(60, Resizable = false)]
    [HideLabel]
    public bool ArmorMat;

    [AssetsOnly]
    [HideIf("ArmorMat")]
    public Material MaterialAsset;

    public string MaterialName;
}