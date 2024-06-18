using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class CharacterVisualsData : MonoBehaviour
{
    const string validation = "UpdateVisuals";

    [OnValueChanged (validation)]
    public SkinnedMeshRenderer charSkinnedMeshRenderer;

    [OnValueChanged (validation)]
    public bool CharacterHairUsed = true;
    
    [OnValueChanged (validation)]
    public Transform CharacterHairHolder;
    
    [OnValueChanged (validation)]
    public bool CharacterAccessoriesUsed = true;
    
    [OnValueChanged (validation)]
    public Transform CharacterAccessoriesHolder;

    [OnValueChanged (validation)]
    public bool CharacterUsesFemaleBody;

    // Character hair and facial hair parameters
    [OnValueChanged (validation), Header ("Hair"), Space (10)]
    public CharacterHairData CharacterHairDataParameters;

    [OnValueChanged (validation)]
    public Color HairColor = new Color (0.3f, 0.26f, 0.23f);

    [OnValueChanged (validation), Header ("Facial Hair")]
    public CharacterHairData CharacterFacialHairDataParameters;

    [OnValueChanged (validation)]
    public Color FacialHairColor = new Color (0.3f, 0.26f, 0.23f);

    [OnValueChanged (validation), Header ("Eyebrows")]
    public Color EyebrowsColor = new Color (0.29f, 0.216f, 0.16f, 1.0f);

    [OnValueChanged (validation), Range (0.0f, 1.0f)]
    public float EyebrowsThickness = 0.5f;

    [OnValueChanged (validation), Range (0.0f, 3.0f)]
    public float EyebrowsStyle = 0.0f;

    // Character skin parameters
    [OnValueChanged (validation), Header ("Skin")]
    public CharacterSkinTexturesData CharacterSkinTextures;

    [OnValueChanged (validation)]
    public Color SkinTint = new Color (1.0f, 1.0f, 1.0f);

    [OnValueChanged (validation), Range (0.0f, 3.0f)]
    public float SkinSmoothnessIntensity = 1.8f;

    [OnValueChanged (validation)]
    public Color SkinRedness = new Color (1.0f, 0.0f, 0.0f, 0.05f);

    [OnValueChanged (validation)]
    public Color SkinDarkening = new Color (0.5f, 0.4f, 0.3f, 0.05f);

    // Character lips parameters
    [OnValueChanged (validation), Header ("Lips")]
    public Color LipsTint = new Color (1.0f, 1.0f, 1.0f, 0.0f);

    // Character eye material parameters
    [OnValueChanged (validation), Header ("Eyes")]
    public Color EyeballColorTint = new Color (1.0f, 1.0f, 1.0f);

    [OnValueChanged (validation)]
    public Color EyeIrisColor = new Color (0.4f, 0.4f, 0.45f, 0.0f);

    [OnValueChanged (validation)]
    public Color EyeIrisColorSecondary = new Color (0.8f, 0.6f, 0.45f, 0.0f);

    // Character accessories control
    [Header ("Accessories")]
    [OnValueChanged (validation)]
    public List<CharacterAccessory> AccessoriesList = new List<CharacterAccessory> ();

    public List<CharacterAccessoryLink> AccessoryLinks = new List<CharacterAccessoryLink> ();
    private Dictionary<string, CharacterAccessoryLink> AccessoryLinksLookup = new Dictionary<string, CharacterAccessoryLink> ();

    // Character clothing parameters
    [Header ("Clothing")]
    [OnValueChanged (validation), Range (0.0f, 0.49f)]
    public float UpperBodyHideAreasForClothing = 0.0f;

    [OnValueChanged (validation), Range (0.0f, 0.49f)]
    public float LowerBodyHideAreasForClothing = 0.0f;

    [OnValueChanged (validation)]
    public CharacterSkinTexturesData CharacterBaseClothingTextures;

    [OnValueChanged (validation), Space (5)]
    [Tooltip ("List all clothing mesh renderers here, so the script can apply body blendshapes to them too.")]
    public List<SkinnedMeshRenderer> clothingSkinnedMeshRenderers = new List<SkinnedMeshRenderer> ();

    // Character blendshape parameters
    [Header ("Blendshapes")]
    [Tooltip ("It is recommended to keep this set to TRUE. Blendshapes never hold their old indices upon mesh reimport, so it is safer to clean them up and set them through the script.")]
    [OnValueChanged (validation)]
    public bool ClearBlendshapeWeights = true;

    [InfoBox ("This component statically sets listed blendshapes' weight, because Unity's renderer blendshape list becomes a mess after several FBX reimports with all the settings scrambled.", InfoMessageType.Info)]
    [OnValueChanged (validation)]
    public List<ControlledBlendshape> ControlledBlendshapesList = new List<ControlledBlendshape> ();

    // Character fake cockpit lighting toggle + list of renderers populated automatically
    [Header ("List Of Renderers for Fake Cockpit Lighting")]
    [NonSerialized, ShowInInspector, ReadOnly]
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<Renderer> renderers;

    [NonSerialized]
    private bool initialized = false;

    // Cached private string to see if character mesh reference has been changed
    // Used in blendshape list population
    [NonSerialized]
    private SkinnedMeshRenderer charSkinnedMeshRendererCached;

    [NonSerialized]
    private Mesh charSharedMesh;

    [NonSerialized]
    private int blendShapeIndex = -1;

    [NonSerialized]
    private MaterialPropertyBlock _propBlock;

    [NonSerialized]
    private MaterialPropertyBlock _propBlockHair;

    [NonSerialized]
    private MaterialPropertyBlock _propBlockAcc;

    [NonSerialized]
    private string charMaterialName;

    [NonSerialized]
    private MeshFilter hairMeshFilter;

    [NonSerialized]
    private MeshRenderer hairMeshRenderer;

    [NonSerialized]
    private GameObject accessoryPrefab;

    [NonSerialized]
    private MeshRenderer accessoryMeshRenderer;

    [NonSerialized, ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
    private Dictionary<int, Material> materialCopies = new Dictionary<int, Material> ();

    [NonSerialized]
    private bool characterWearingHat;

    #if UNITY_EDITOR

    // Public to allow other editor utilities to make use of this once lookup is built
    [NonSerialized, ShowInInspector]
    [ListDrawerSettings (DefaultExpandedState = false, IsReadOnly = true)]
    public List<string> blendshapeNames;

    private List<string> GetListOfBlendshapeNames ()
    {
        if (charSkinnedMeshRenderer)
        {
            if (charSkinnedMeshRendererCached != charSkinnedMeshRenderer)
            {
                if (blendshapeNames == null)
                    blendshapeNames = new List<string> ();
                else
                    blendshapeNames.Clear ();

                for (int i = 0; i < charSkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    blendshapeNames.Add (charSkinnedMeshRenderer.sharedMesh.GetBlendShapeName (i));

                blendshapeNames.Sort ();
                charSkinnedMeshRendererCached = charSkinnedMeshRenderer;
            }
        }
        else
        {
            if (blendshapeNames.Count > 0)
                blendshapeNames.Clear ();
        }

        return blendshapeNames;
    }
    #endif

    // Accessories control
    [Serializable]
    public class CharacterAccessory
    {
        [OnValueChanged ("UpdateVisuals")]
        public GameObject AccessoryMesh;

        [OnValueChanged ("UpdateVisuals")]
        public Vector3 HSVOffsetsPrimary = new Vector3 (0.0f, 0.5f, 0.5f);

        [OnValueChanged ("UpdateVisuals")]
        public Vector3 HSVOffsetsSecondary = new Vector3 (0.0f, 0.5f, 0.5f);

        // Optional opacity boost for accessory variants with transparent visors
        [OnValueChanged ("UpdateVisuals")]
        public float GlassOpacityOverride = 0.0f;

        [OnValueChanged ("UpdateVisuals")]
        public string HolderKey;

        #if UNITY_EDITOR
        [HideInInspector, NonSerialized]
        public CharacterVisualsData obj;

        private void UpdateVisuals () => obj.UpdateVisuals ();
        #endif
    }

    // Accessories control
    [Serializable]
    public class CharacterAccessoryLink
    {
        public string Key;
        public Transform Holder;
    }

    // Blendshapes control
    [Serializable]
    public class ControlledBlendshape
    {
        [HideInInspector, NonSerialized]
        public CharacterVisualsData obj;

        // Odin attributes and method calls are only needed for in-editor drop-down list to work
        // For runtime, we only need the BlendshapeName string value that is serialized
        [OnValueChanged ("UpdateVisuals"), ValueDropdown ("GetBlendshapeNames")]
        public string BlendshapeName = "";

        [OnValueChanged ("UpdateVisuals"), Range (-100, 100)]
        public int BlendshapeInfluence = 0;

        #if UNITY_EDITOR
        private List<string> GetBlendshapeNames () => obj != null ? obj.GetListOfBlendshapeNames () : null;
        #endif
        
        private void SnapValue () => BlendshapeInfluence = Mathf.RoundToInt (BlendshapeInfluence / 10f) * 10;

        private void UpdateVisuals ()
        {
            SnapValue ();
            if (obj != null)
                obj.UpdateVisuals ();
        }
    }

    private bool BaseClothingForHeadExists ()
    {
        // Checking only for albedo texture is enough to determine if user needs a clothing layer to be applied
        if (CharacterBaseClothingTextures == null)
            return false;

        var variant = CharacterUsesFemaleBody ? CharacterBaseClothingTextures.variantF : CharacterBaseClothingTextures.variantM;
        return variant != null && variant.HeadSkinTextureAlbedo != null;
    }

    private bool BaseClothingForBodyExists ()
    {
        if (CharacterBaseClothingTextures == null)
            return false;

        var variant = CharacterUsesFemaleBody ? CharacterBaseClothingTextures.variantF : CharacterBaseClothingTextures.variantM;
        return variant != null && variant.BodySkinTextureAlbedo != null;
    }

    private static readonly int ID_SkinTex = Shader.PropertyToID ("_SkinTex");
    private static readonly int ID_SkinTexBlso = Shader.PropertyToID ("_SkinTexBLSO");
    private static readonly int ID_Bump = Shader.PropertyToID ("_Bump");
    private static readonly int ID_TintColor = Shader.PropertyToID ("_TintColor");
    private static readonly int ID_SkinTexSmoothIntensity = Shader.PropertyToID ("_SkinTexSmoothIntensity");
    private static readonly int ID_SkinTexSpotsRednessColor = Shader.PropertyToID ("_SkinTexSpotsRednessColor");
    private static readonly int ID_SkinTexSpotsDarknessColor = Shader.PropertyToID ("_SkinTexSpotsDarknessColor");
    private static readonly int ID_HairCapTex = Shader.PropertyToID ("_HairCapTex");
    private static readonly int ID_HairCapIntensity = Shader.PropertyToID ("_HairCapIntensity");
    private static readonly int ID_HairCapColor = Shader.PropertyToID ("_HairCapColor");
    private static readonly int ID_HairCapTexSecondary = Shader.PropertyToID ("_HairCapTexSecondary");
    private static readonly int ID_HairCapIntensitySecondary = Shader.PropertyToID ("_HairCapIntensitySecondary");
    private static readonly int ID_HairCapColorSecondary = Shader.PropertyToID ("_HairCapColorSecondary");
    private static readonly int ID_EyebrowsColor = Shader.PropertyToID ("_EyebrowsColor");
    private static readonly int ID_EyebrowsThickness = Shader.PropertyToID ("_EyebrowsThickness");
    private static readonly int ID_EyebrowsStyleChoice = Shader.PropertyToID ("_EyebrowsStyleChoice");
    private static readonly int ID_LipsTintColor = Shader.PropertyToID ("_LipsTintColor");
    private static readonly int ID_UseClothing = Shader.PropertyToID ("_UseClothing");
    private static readonly int ID_SkinTexClothing = Shader.PropertyToID ("_SkinTexClothing");
    private static readonly int ID_MseoTexClothing = Shader.PropertyToID ("_MSEOTexClothing");
    private static readonly int ID_NormalTexClothing = Shader.PropertyToID ("_NormalTexClothing");
    private static readonly int ID_HideUpperBodyForClothing = Shader.PropertyToID ("_HideUpperBodyForClothing");
    private static readonly int ID_HideLowerBodyForClothing = Shader.PropertyToID ("_HideLowerBodyForClothing");
    private static readonly int ID_IrisColor = Shader.PropertyToID ("_EyeIrisColor");
    private static readonly int ID_IrisColorSecondary = Shader.PropertyToID ("_EyeIrisColorSecondary");
    private static readonly int ID_EyeBallColorTint = Shader.PropertyToID ("_EyeBallColorTint");
    private static readonly int ID_HsbOffsetsPrimary = Shader.PropertyToID ("_HSBOffsetsPrimary");
    private static readonly int ID_HsbOffsetsSecondary = Shader.PropertyToID ("_HSBOffsetsSecondary");
    private static readonly int ID_GlassOpacityOverride = Shader.PropertyToID ("_GlassOpacityOverride");
    private static readonly int ID_FemaleHairSwitch = Shader.PropertyToID ("_FemaleHair");
    private static readonly int ID_HairColor = Shader.PropertyToID ("_HairColor");
    private static readonly int ID_Cutoff = Shader.PropertyToID ("_Cutoff");
    private static readonly int ID_HairSmoothness = Shader.PropertyToID ("_HairSmoothness");
    private static readonly int ID_HairPbrSmoothness = Shader.PropertyToID ("_HairPBRSmoothness");
    private static readonly int ID_HairMainSpecularity = Shader.PropertyToID ("_HairMainSpecularity");
    private static readonly int ID_HairTangentshift = Shader.PropertyToID ("_HairTangentshift");
    private static readonly int ID_HairAOIntensity = Shader.PropertyToID ("_HairAOIntensity");
    private static readonly int ID_HairOutlineIntensity = Shader.PropertyToID ("_HairOutlineIntensity");
    private static readonly int ID_HairGradientIntensity = Shader.PropertyToID ("_HairGradientIntensity");
    private static readonly int ID_HairGradientUseVColor = Shader.PropertyToID ("_HairGradientUseVColor");
    private static readonly int ID_HairGradientPower = Shader.PropertyToID ("_HairGradientPower");
    private static readonly int ID_ShadowPassOpacityCut = Shader.PropertyToID ("_ShadowPassOpacityCut");
    private static readonly int ID_OpacityAmplifyCoef = Shader.PropertyToID ("_OpacityAmplifyCoef");
    private static readonly int ID_NormalAmplifyCoef = Shader.PropertyToID ("_NormalAmplifyCoef");
    private static readonly int ID_OpacityAmplifyDistance = Shader.PropertyToID ("_OpacityAmplifyDistance");
    private static readonly int ID_OpacityAmplifyDistanceOffset = Shader.PropertyToID ("_OpacityAmplifyDistanceOffset");
    private static readonly int ID_OpacityBlueNoiseBlendFactor = Shader.PropertyToID ("_OpacityBlueNoiseBlendFactor");
    private static readonly int ID_HairInertiaMultiplier = Shader.PropertyToID ("_HairInertiaMultiplier");
    private static readonly int ID_HairGravityTiltStrengthTop = Shader.PropertyToID ("_HairGravityTiltStrengthTop");
    private static readonly int ID_HairGravityTiltStrengthBottom = Shader.PropertyToID ("_HairGravityTiltStrengthBottom");
    private static readonly int ID_HairGravityTiltVerticalGradientOffset = Shader.PropertyToID ("_HairGravityTiltVerticalGradientOffset");

    // Main function to apply all the parameters
    [Button ("Apply Settings Manually")]
    public void UpdateVisuals ()
    {
        if (renderers == null)
            renderers = new List<Renderer> ();
        else
            renderers.Clear ();

        if (AccessoryLinksLookup == null)
            AccessoryLinksLookup = new Dictionary<string, CharacterAccessoryLink> ();
        else
            AccessoryLinksLookup.Clear ();

        foreach (var link in AccessoryLinks)
        {
            if (!AccessoryLinksLookup.ContainsKey (link.Key))
                AccessoryLinksLookup.Add (link.Key, link);
        }

        if (charSkinnedMeshRenderer)
        {
            renderers.Add (charSkinnedMeshRenderer);

            // Init material property block once
            if (_propBlock == null)
                _propBlock = new MaterialPropertyBlock ();
            else
                _propBlock.Clear ();

            // Character material control
            var charMaterials = charSkinnedMeshRenderer.sharedMaterials;
            for (int i = 0; i < charMaterials.Length; i++)
            {
                var currentCharMat = charMaterials[i];
                charMaterialName = currentCharMat.name;
                // ============================ Head Material ============================
                if (charMaterialName.Substring (charMaterialName.Length - 4) == "Head")
                {
                    charSkinnedMeshRenderer.GetPropertyBlock (_propBlock, i);

                    // Skin textures - head
                    if (CharacterSkinTextures != null)
                    {
                        var variant = CharacterUsesFemaleBody ? CharacterSkinTextures.variantF : CharacterSkinTextures.variantM;
                        _propBlock.SetTexture (ID_SkinTex, variant.HeadSkinTextureAlbedo);
                        _propBlock.SetTexture (ID_SkinTexBlso, variant.HeadSkinTextureRDSO);
                        _propBlock.SetTexture (ID_Bump, variant.HeadSkinTextureNormal);
                    }

                    // Skin parameters - head
                    _propBlock.SetColor (ID_TintColor, SkinTint);
                    _propBlock.SetFloat (ID_SkinTexSmoothIntensity, SkinSmoothnessIntensity);
                    _propBlock.SetColor (ID_SkinTexSpotsRednessColor, SkinRedness);
                    _propBlock.SetColor (ID_SkinTexSpotsDarknessColor, SkinDarkening);

                    // Hair - head
                    if (CharacterHairUsed && CharacterHairDataParameters != null)
                    {
                        // All these parameters are not meant to be exposed for player\customization,
                        // they are artist-defined parameters specific to a mesh\hairstyle
                        _propBlock.SetTexture (ID_HairCapTex, CharacterHairDataParameters.HairCapTex);
                        _propBlock.SetFloat (ID_HairCapIntensity, CharacterHairDataParameters.HairCapIntensity);
                        _propBlock.SetColor (ID_HairCapColor, HairColor * CharacterHairDataParameters.HairCapBrightness);
                    }
                    else
                    {
                        _propBlock.SetTexture (ID_HairCapTex, Texture2D.blackTexture);
                        _propBlock.SetFloat (ID_HairCapIntensity, 0);
                    }

                    // Facial hair - head
                    if (CharacterHairUsed && CharacterFacialHairDataParameters != null)
                    {
                        _propBlock.SetTexture (ID_HairCapTexSecondary, CharacterFacialHairDataParameters.HairCapTex);
                        _propBlock.SetFloat (ID_HairCapIntensitySecondary, CharacterFacialHairDataParameters.HairCapIntensity);
                        _propBlock.SetColor (ID_HairCapColorSecondary, FacialHairColor * CharacterFacialHairDataParameters.HairCapBrightness);
                    }
                    else
                    {
                        _propBlock.SetTexture (ID_HairCapTexSecondary, Texture2D.blackTexture);
                        _propBlock.SetFloat (ID_HairCapIntensitySecondary, 0);
                    }

                    // Eyebrows - head
                    _propBlock.SetColor (ID_EyebrowsColor, EyebrowsColor);
                    _propBlock.SetFloat (ID_EyebrowsThickness, EyebrowsThickness);
                    _propBlock.SetFloat (ID_EyebrowsStyleChoice, EyebrowsStyle);

                    // Lips - head
                    _propBlock.SetColor (ID_LipsTintColor, LipsTint);

                    // Base clothing layer - head
                    if (BaseClothingForHeadExists ())
                    {
                        var variant = CharacterUsesFemaleBody ? CharacterBaseClothingTextures.variantF : CharacterBaseClothingTextures.variantM;
                        _propBlock.SetFloat (ID_UseClothing, 1);
                        _propBlock.SetTexture (ID_SkinTexClothing, variant.HeadSkinTextureAlbedo);
                        _propBlock.SetTexture (ID_MseoTexClothing, variant.HeadSkinTextureRDSO);
                        _propBlock.SetTexture (ID_NormalTexClothing, variant.HeadSkinTextureNormal);
                    }
                    else
                    {
                        _propBlock.SetFloat (ID_UseClothing, 0);
                        _propBlock.SetTexture (ID_SkinTexClothing, Texture2D.blackTexture);
                        _propBlock.SetTexture (ID_MseoTexClothing, Texture2D.blackTexture);
                        _propBlock.SetTexture (ID_NormalTexClothing, Texture2D.blackTexture);
                    }

                    charSkinnedMeshRenderer.SetPropertyBlock (_propBlock, i);
                }
                // ============================ Body Material ============================
                else if ((charMaterialName.Substring (charMaterialName.Length - 4) == "Body") || (charMaterialName.Substring (charMaterialName.Length - 10) == "Body_Pilot"))
                {
                    charSkinnedMeshRenderer.GetPropertyBlock (_propBlock, i);

                    // Skin textures - body
                    if (CharacterSkinTextures != null)
                    {
                        var variant = CharacterUsesFemaleBody ? CharacterSkinTextures.variantF : CharacterSkinTextures.variantM;
                        _propBlock.SetTexture (ID_SkinTex, variant.BodySkinTextureAlbedo);
                        _propBlock.SetTexture (ID_SkinTexBlso, variant.BodySkinTextureRDSO);
                        _propBlock.SetTexture (ID_Bump, variant.BodySkinTextureNormal);
                    }

                    // Skin parameters - body
                    _propBlock.SetColor (ID_TintColor, SkinTint);
                    _propBlock.SetFloat (ID_SkinTexSmoothIntensity, SkinSmoothnessIntensity);
                    // Clothing - body
                    _propBlock.SetFloat (ID_HideUpperBodyForClothing, UpperBodyHideAreasForClothing);
                    _propBlock.SetFloat (ID_HideLowerBodyForClothing, LowerBodyHideAreasForClothing);

                    // Base clothing layer - body
                    if (BaseClothingForBodyExists ())
                    {
                        var variant = CharacterUsesFemaleBody ? CharacterBaseClothingTextures.variantF : CharacterBaseClothingTextures.variantM;
                        _propBlock.SetFloat (ID_UseClothing, 1);
                        _propBlock.SetTexture (ID_SkinTexClothing, variant.BodySkinTextureAlbedo);
                        _propBlock.SetTexture (ID_MseoTexClothing, variant.BodySkinTextureRDSO);
                        _propBlock.SetTexture (ID_NormalTexClothing, variant.BodySkinTextureNormal);
                    }
                    else
                    {
                        _propBlock.SetFloat (ID_UseClothing, 0);
                        _propBlock.SetTexture (ID_SkinTexClothing, Texture2D.blackTexture);
                        _propBlock.SetTexture (ID_MseoTexClothing, Texture2D.blackTexture);
                        _propBlock.SetTexture (ID_NormalTexClothing, Texture2D.blackTexture);
                    }

                    charSkinnedMeshRenderer.SetPropertyBlock (_propBlock, i);
                }
                // ============================ Eye Material ============================
                else if (charMaterialName.Substring (charMaterialName.Length - 7) == "EyeBall")
                {
                    charSkinnedMeshRenderer.GetPropertyBlock (_propBlock, i);

                    // Skin parameters
                    _propBlock.SetColor (ID_TintColor, SkinTint);
                    // Eye parameters
                    _propBlock.SetColor (ID_IrisColor, EyeIrisColor);
                    _propBlock.SetColor (ID_IrisColorSecondary, EyeIrisColorSecondary);
                    _propBlock.SetColor (ID_EyeBallColorTint, EyeballColorTint);
                    charSkinnedMeshRenderer.SetPropertyBlock (_propBlock, i);
                }
            }

            // ============================ Accessories Control ============================
            var accessoryTransform = CharacterAccessoriesHolder != null ? CharacterAccessoriesHolder : CharacterHairHolder;
            if (CharacterAccessoriesUsed && accessoryTransform != null)
            {
                // Clear all accessories spawned earlier
                accessoryTransform.DestroyChildren ();
                // Spawn new accessories and parent them to hair_holder transform
                if (AccessoriesList.Count > 0)
                {
                    // Init material property block once
                    if (_propBlockAcc == null)
                        _propBlockAcc = new MaterialPropertyBlock ();
                    else
                        _propBlockAcc.Clear ();

                    accessoryPrefab = null;
                    accessoryMeshRenderer = null;
                    characterWearingHat = false;

                    foreach (CharacterAccessory charAccessory in AccessoriesList)
                    {
                        #if UNITY_EDITOR
                        charAccessory.obj = this;
                        #endif

                        if (charAccessory.AccessoryMesh != null)
                        {
                            accessoryPrefab = Instantiate (charAccessory.AccessoryMesh, accessoryTransform, false);
                            accessoryPrefab.transform.SetLocalTransformationToZero ();
                            
                            if (AccessoryLinksLookup.TryGetValue (charAccessory.HolderKey, out var HolderLink) && HolderLink.Holder != null)
                            {
                                accessoryPrefab.transform.parent = HolderLink.Holder;
                                accessoryPrefab.transform.SetLocalTransformationToZero ();
                                accessoryPrefab.transform.parent = accessoryTransform;
                            }

                            // Make sure the instantiated objects are not saved when previewing things in-editor
                            accessoryPrefab.hideFlags = HideFlags.DontSaveInEditor;
                            // Assign this object to the same layer as the parent object. This means all pilot-in-the-cockpit stuff will get the right "Pilot" layer.
                            // This will ensure hats are rendered in cutscenes, while getting correctly culled in combat.
                            accessoryPrefab.layer = gameObject.layer;

                            accessoryMeshRenderer = accessoryPrefab.GetComponent<MeshRenderer> ();
                            renderers.Add (accessoryMeshRenderer);

                            // Support for multiple accessory materials to make it future-proof (helmets can have a separate glass material etc.)
                            var accMaterials = accessoryMeshRenderer.sharedMaterials;
                            for (int i = 0; i < accMaterials.Length; i++)
                            {
                                accessoryMeshRenderer.GetPropertyBlock (_propBlockAcc, i);
                                _propBlockAcc.SetVector (ID_HsbOffsetsPrimary, charAccessory.HSVOffsetsPrimary);
                                _propBlockAcc.SetVector (ID_HsbOffsetsSecondary, charAccessory.HSVOffsetsSecondary);
                                // Optional opacity boost for accessory variants that have transparent visors
                                // (if we want to override the default value and make them opaque)
                                _propBlockAcc.SetFloat (ID_GlassOpacityOverride, charAccessory.GlassOpacityOverride);
                                // Use female head morph switch - accessory (stored in UV1.xy and UV2.x)
                                _propBlockAcc.SetFloat (ID_FemaleHairSwitch, CharacterUsesFemaleBody ? 1 : 0);
                                accessoryMeshRenderer.SetPropertyBlock (_propBlockAcc, i);
                            }

                            // Indicate that our character wears a hat - need to adjust hair mesh if true
                            if (accessoryPrefab.name.StartsWith ("Hat_") || accessoryPrefab.name.StartsWith ("Helmet_"))
                            {
                                characterWearingHat = true;
                            }
                        }
                    }
                }
            }

            // ============================ Hair Mesh ============================
            // CharacterHairHolder transform has MeshFilter and MeshRenderer components already created within character's prefab
            if (CharacterHairUsed && CharacterHairHolder != null)
            {
                hairMeshFilter = CharacterHairHolder.GetComponent<MeshFilter> ();
                hairMeshRenderer = null;

                if (CharacterHairDataParameters != null && CharacterHairDataParameters.HairMesh != null)
                {
                    // Init material property block once
                    //if (_propBlockHair == null)
                    //    _propBlockHair = new MaterialPropertyBlock ();
                    //else
                    //    _propBlockHair.Clear ();

                    // Assign hair mesh from data to our character's hair mesh
                    hairMeshFilter.sharedMesh = CharacterHairDataParameters.HairMesh.GetComponent<MeshFilter> ().sharedMesh;

                    hairMeshRenderer = CharacterHairHolder.GetComponent<MeshRenderer> ();
                    renderers.Add (hairMeshRenderer);

                    // Fetch material from hair asset
                    var sharedMaterial = CharacterHairDataParameters.HairMesh.GetComponent<MeshRenderer> ().sharedMaterial;

                    // Temporary hack required due to MaterialPropertyBlocks not working on this renderer for some reason
                    // - Fetch ID of shared material
                    // - Create a copy if this material hasn't already been copied
                    // - Assign that copy to the shared material slot of the local renderer
                    // - Instead of using a MaterialPropertyBlock, set properties directly on the material

                    int sharedMaterialID = sharedMaterial.GetInstanceID ();
                    Material material = null;

                    if (materialCopies.ContainsKey (sharedMaterialID) && materialCopies[sharedMaterialID] != null)
                        material = materialCopies[sharedMaterialID];
                    else
                    {
                        material = new Material (sharedMaterial.shader);
                        material.CopyPropertiesFromMaterial (sharedMaterial);
                        materialCopies[sharedMaterialID] = material;
                    }

                    // Update renderer material
                    hairMeshRenderer.sharedMaterial = material;

                    // Update copy material
                    material.SetColor (ID_HairColor, HairColor);

                    // All these parameters are not meant to be exposed for player\customization,
                    // they are artist-defined parameters specific to a mesh\hairstyle
                    material.SetFloat (ID_Cutoff, CharacterHairDataParameters.AlphaTestCutoff);
                    material.SetFloat (ID_HairSmoothness, CharacterHairDataParameters.HairSmoothness);
                    material.SetFloat (ID_HairPbrSmoothness, CharacterHairDataParameters.HairPBRSmoothness);
                    material.SetFloat (ID_HairMainSpecularity, CharacterHairDataParameters.HairMainSpecularity);
                    material.SetFloat (ID_HairTangentshift, CharacterHairDataParameters.HairTangentShift);
                    material.SetFloat (ID_HairAOIntensity, CharacterHairDataParameters.HairAOIntensity);
                    material.SetFloat (ID_HairOutlineIntensity, CharacterHairDataParameters.HairOutlineIntensity);
                    material.SetFloat (ID_HairGradientIntensity, CharacterHairDataParameters.HairGradientIntensity);
                    material.SetFloat (ID_HairGradientUseVColor, CharacterHairDataParameters.HairGradientUseVColor);
                    material.SetFloat (ID_HairGradientPower, CharacterHairDataParameters.HairGradientPower);
                    material.SetFloat (ID_ShadowPassOpacityCut, CharacterHairDataParameters.ShadowPassOpacityCut);
                    material.SetFloat (ID_OpacityAmplifyCoef, CharacterHairDataParameters.OpacityAmplifyCoef);
                    material.SetFloat (ID_NormalAmplifyCoef, CharacterHairDataParameters.NormalAmplifyCoef);
                    material.SetFloat (ID_OpacityAmplifyDistance, CharacterHairDataParameters.OpacityAmplifyDistance);
                    material.SetFloat (ID_OpacityAmplifyDistanceOffset, CharacterHairDataParameters.OpacityAmplifyDistanceOffset);
                    material.SetFloat (ID_OpacityBlueNoiseBlendFactor, CharacterHairDataParameters.OpacityBlueNoiseBlendFactor);
                    material.SetFloat (ID_HairInertiaMultiplier, CharacterHairDataParameters.HairInertiaMultiplier);
                    material.SetFloat (ID_HairGravityTiltStrengthTop, CharacterHairDataParameters.HairGravityTiltStrengthTop);
                    material.SetFloat (ID_HairGravityTiltStrengthBottom, CharacterHairDataParameters.HairGravityTiltStrengthBottom);
                    material.SetFloat (ID_HairGravityTiltVerticalGradientOffset, CharacterHairDataParameters.HairGravityTiltVerticalGradientOffset);

                    // Use female head morph switch - hair (stored in UV1.xy and UV2.x)
                    material.SetFloat (ID_FemaleHairSwitch, CharacterUsesFemaleBody ? 1 : 0);

                    // Temporarily commented out until we can find why MaterialPropertyBlock doesn't work
                    /*
                    hairRenderer.GetPropertyBlock (_propBlockHair);
                    _propBlockHair.SetColor(ID_HairColor, HairColor);

                    // All these parameters are not meant to be exposed for player\customization,
                    // they are artist-defined parameters specific to a mesh\hairstyle
                    _propBlockHair.SetFloat(ID_Cutoff, CharacterHairDataParameters.AlphaTestCutoff);
                    _propBlockHair.SetFloat(ID_HairSmoothness, CharacterHairDataParameters.HairSmoothness);
                    _propBlockHair.SetFloat(ID_HairPbrSmoothness,  CharacterHairDataParameters.HairPBRSmoothness);
                    _propBlockHair.SetFloat(ID_HairMainSpecularity, CharacterHairDataParameters.HairMainSpecularity);
                    _propBlockHair.SetFloat(ID_HairTangentshift,  CharacterHairDataParameters.HairTangentShift);
                    _propBlockHair.SetFloat(ID_HairAOIntensity,  CharacterHairDataParameters.HairAOIntensity);
                    _propBlockHair.SetFloat(ID_HairOutlineIntensity, CharacterHairDataParameters.HairOutlineIntensity);
                    _propBlockHair.SetFloat(ID_HairGradientIntensity, CharacterHairDataParameters.HairGradientIntensity);
                    _propBlockHair.SetFloat(ID_ShadowPassOpacityCut,  CharacterHairDataParameters.ShadowPassOpacityCut);
                    _propBlockHair.SetFloat(ID_OpacityAmplifyCoef,  CharacterHairDataParameters.OpacityAmplifyCoef);
                    _propBlockHair.SetFloat(ID_NormalAmplifyCoef,  CharacterHairDataParameters.NormalAmplifyCoef);
                    _propBlockHair.SetFloat(ID_OpacityAmplifyDistance,  CharacterHairDataParameters.OpacityAmplifyDistance);
                    _propBlockHair.SetFloat(ID_OpacityAmplifyDistanceOffset,  CharacterHairDataParameters.OpacityAmplifyDistanceOffset);
                    _propBlockHair.SetFloat(ID_OpacityBlueNoiseBlendFactor,  CharacterHairDataParameters.OpacityBlueNoiseBlendFactor);
                    
                    // // Use female head morph switch - hair (stored in UV1.xy and UV2.x)
                    _propBlockHair.SetFloat(ID_FemaleHairSwitch, CharacterUsesFemaleBody ? 1 : 0);

                    hairRenderer.SetPropertyBlock(_propBlockHair);
                    */
                }
                else
                {
                    hairMeshFilter.sharedMesh = null;
                    hairMeshFilter.mesh = null;
                }
            }

            // ============================ Blendshapes Control ============================
            charSharedMesh = charSkinnedMeshRenderer.sharedMesh;

            if (charSharedMesh)
            {
                // Clear all blendshape weights first (useful to clean up wrong blendshape weights whenever mesh was reimported)
                if (ClearBlendshapeWeights)
                {
                    for (int i = 0; i < charSharedMesh.blendShapeCount; i++)
                    {
                        charSkinnedMeshRenderer.SetBlendShapeWeight (i, 0);
                    }
                }

                if (ControlledBlendshapesList.Count > 0)
                {
                    foreach (ControlledBlendshape cntrlBlendshape in ControlledBlendshapesList)
                    {
                        cntrlBlendshape.obj = this;
                        // Make sure blendshape name is not empty, Unity editor will crash on GetBlendShapeIndex otherwise!
                        // (The name can be empty when the user has just added a new list entry)
                        if (!string.IsNullOrEmpty (cntrlBlendshape.BlendshapeName))
                        {
                            blendShapeIndex = charSharedMesh.GetBlendShapeIndex (cntrlBlendshape.BlendshapeName);
                            if (blendShapeIndex != -1)
                            {
                                charSkinnedMeshRenderer.SetBlendShapeWeight (blendShapeIndex, (float)cntrlBlendshape.BlendshapeInfluence * 0.8f);
                            }
                            else
                            {
                                Debug.LogWarning ("Blendshape with the name: " + cntrlBlendshape.BlendshapeName + " does not exist in the character mesh");
                            }

                            // Additional pass to apply blendshapes to clothing meshes (if there are any)
                            if (clothingSkinnedMeshRenderers.Count > 0)
                            {
                                foreach (SkinnedMeshRenderer clothingMesh in clothingSkinnedMeshRenderers)
                                {
                                    if (clothingMesh)
                                    {
                                        blendShapeIndex = clothingMesh.sharedMesh.GetBlendShapeIndex (cntrlBlendshape.BlendshapeName);
                                        if (blendShapeIndex != -1)
                                        {
                                            clothingMesh.SetBlendShapeWeight (blendShapeIndex, (float)cntrlBlendshape.BlendshapeInfluence * 0.8f);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            initialized = true;
        }
    }

    // Use Awake for runtime
    void Awake ()
    {
        UpdateVisuals ();
    }

    // Utilize OnEnable call only in-editor to avoid frame hitches inbetween cinematic shots
    // when lots of characters become visible and need their settings applied
    // (Still useful in-editor to make sure visuals get updated when editing the cutscenes etc.)
    void OnEnable ()
    {
        if (Application.isPlaying)
            return;

        if (!initialized)
            UpdateVisuals ();
    }
}