using UnityEngine;

[System.Serializable]
public class ShaderGlobalConfig
{
    // Used in mech shader for patterns and damage
    public float globalUnitRampSize = 1f;
    public float globalTriplanarScale = 2f;

    // Used in environment triplanar effects
    public float globalEnvironmentDetailScale = 4f;
    public float globalEnvironmentDetailContrast = 0.5f;
    public float globalEnvironmentRampScale = 1.1f;
    public float globalEnvironmentRampInfluence = 1f;
    public float globalEnvironmentDamageOffset = 2f;

    // Height based AO gradient size (in meters)
    public float globalEnvironmentAOSize = 0.5f;

    // Height based AO gradient shift for gradient start (in meters)
    public float globalEnvironmentAOShift = 0.25f;

    // On-off switch for height based AO
    public float globalEnvironmentAOToggle = 1f;

    // Environment block saturation
    public float globalEnvironmentColorSaturation = 0f;

    // Tree billboards
    public float treeBillboardStart = 50;
    public float treeBillboardFadeLength = 10;
    public float treeBillboardFadeOut = 5;

    // Background detail

    public float backgroundSwapDistanceStart = 20f;
    public float backgroundSwapDistanceEnd = 100f;
    public float backgroundGrassScale = 12f;
    public float backgroundGrassScaleAtDistance = 0.5f;
    public float backgroundAsphaltScale = 12f;
    public float backgroundAsphaltScaleAtDistance = 0.5f;
    
    public float heightGradientBottom = -27f;
    public float heightGradientTop = 0f;
    public float brightnessOffsetBottom = -0.5f;
    public float brightnessOffsetTop = 0.5f;

    // Play Mode toggle

    public float globalPlayMode = 0.0f;
}

public static class ShaderGlobalHelper
{
    public static readonly string configFilePath = "Configs";
    public static readonly string configFileName = "shaderGlobals.yaml";
    public static ShaderGlobalConfig config;

    public const int shaderFamilyID_Blocks = 0;
    public const int shaderFamilyID_Props = 1;

    public static bool debug = false;
    public static Texture globalUnitDetailTex;
    public static Texture globalUnitDetailTexNew;
    public static Texture globalUnitDamageTex;
    public static Texture globalUnitDamageTexNew;
    public static Texture globalUnitDamageTexNewSecondary;
    public static Texture globalUnitRampTex;
    public static Texture globalUnitIridescenceTex;

    public static Texture globalDetailTexture;
    public static Texture globalDamageTexture;
    public static Texture globalRampBurnTexture;
    public static Texture globalRampTerraceTexture;

    public static Texture globalBackgroundDetailTexture;
    public static Texture globalBackgroundGrassTexture;
    public static Texture globalBackgroundAsphaltTexture;

    public static Area.AreaManager areaManager;
    private static bool globalsAssigned = false;

    public static void CheckGlobals ()
    {
        if (globalsAssigned)
            return;

        globalsAssigned = true;
        UpdateGlobals ();
    }

    public static void UpdateGlobals ()
    {
        if (config == null)
            LoadConfig ();

        if (config != null)
        {
            config.globalTriplanarScale = Mathf.Clamp (config.globalTriplanarScale, 0.25f, 8f);
            config.globalEnvironmentDetailScale = Mathf.Clamp (config.globalEnvironmentDetailScale, 0.25f, 8f);
            config.globalEnvironmentDetailContrast = Mathf.Clamp01 (config.globalEnvironmentDetailContrast);
            config.globalEnvironmentRampScale = Mathf.Clamp (config.globalEnvironmentRampScale, 0.25f, 8f);
            config.globalEnvironmentRampInfluence = Mathf.Clamp01 (config.globalEnvironmentRampInfluence);
            config.globalEnvironmentDamageOffset = Mathf.Clamp (config.globalEnvironmentDamageOffset, 1f, 8f);

            config.globalEnvironmentAOToggle = Mathf.Clamp01 (config.globalEnvironmentAOToggle);
            config.globalEnvironmentColorSaturation = Mathf.Clamp01 (config.globalEnvironmentColorSaturation);

            if (areaManager == null)
                areaManager = GameObject.FindObjectOfType<Area.AreaManager> ();

            if (areaManager != null)
                SetOcclusionShift (areaManager.boundsFull.y * 3f + 4.5f);

            Shader.SetGlobalFloat ("_GlobalUnitRampSize", config.globalUnitRampSize);
            Shader.SetGlobalFloat ("_GlobalTriplanarScale", config.globalTriplanarScale);
            Shader.SetGlobalFloat ("_GlobalEnvironmentDetailScale", config.globalEnvironmentDetailScale);
            Shader.SetGlobalFloat ("_GlobalEnvironmentDetailContrast", config.globalEnvironmentDetailContrast);
            Shader.SetGlobalFloat ("_GlobalEnvironmentRampScale", config.globalEnvironmentRampScale);
            Shader.SetGlobalFloat ("_GlobalEnvironmentRampInfluence", config.globalEnvironmentRampInfluence);
            Shader.SetGlobalFloat ("_GlobalEnvironmentDamageOffset", config.globalEnvironmentDamageOffset);
            Shader.SetGlobalVector ("_GlobalEnvironmentAmbientSettings", new Vector4
            (
                config.globalEnvironmentAOSize, 
                config.globalEnvironmentAOShift, 
                config.globalEnvironmentAOToggle, 
                config.globalEnvironmentColorSaturation
            ));

            Vector4 propertiesTrees = new Vector4 
            (
                config.treeBillboardStart + config.treeBillboardFadeLength, 
                config.treeBillboardFadeLength, 
                config.treeBillboardFadeOut, 
                0
            );
            
            Shader.SetGlobalVector ("_AfsTerrainTrees", propertiesTrees);

            Shader.SetGlobalVector ("_GlobalBackgroundSwapSettings", new Vector4 
            (
                config.backgroundSwapDistanceStart, 
                config.backgroundSwapDistanceEnd, 
                0, 
                0
            ));
            
            Shader.SetGlobalVector ("_GlobalBackgroundSizeSettings", new Vector4 
            (
                config.backgroundGrassScale, 
                config.backgroundGrassScaleAtDistance, 
                config.backgroundAsphaltScale, 
                config.backgroundAsphaltScaleAtDistance
            ));
            
            Shader.SetGlobalVector ("_GlobalHeightGradientData", new Vector4 
            (
                config.heightGradientBottom, 
                config.heightGradientTop, 
                config.brightnessOffsetBottom, 
                config.brightnessOffsetTop
            ));

            if (Application.isPlaying)
            {
                config.globalPlayMode = 1.0f;
            }
            else
            {
                config.globalPlayMode = 0.0f;
            }
            Shader.SetGlobalFloat ("_GlobalPlayMode", config.globalPlayMode);
        }
        else
            Debug.Log ("SH | UpdateGlobals | Config is null, aborting...");

        SetTexture (globalUnitDetailTex, "_GlobalUnitDetailTex", "Detail texture for units");
        SetTexture (globalUnitDetailTexNew, "_GlobalUnitDetailTexNew", "Detail tiling texture for big units");
        SetTexture (globalUnitDamageTex, "_GlobalUnitDamageTex", "Damage mask texture for units");
        SetTexture (globalUnitDamageTexNew, "_GlobalUnitDamageTexNew", "Generic unit damage mask texture - updated");
        SetTexture (globalUnitDamageTexNewSecondary, "_GlobalUnitDamageTexNewSecondary", "Secondary generic unit damage mask texture");
        SetTexture (globalUnitRampTex, "_GlobalUnitRampTex", "Damage ramp texture for units");
        SetTexture (globalUnitIridescenceTex, "_GlobalUnitIridescenceTex", "Iridescence ramp texture for units");
        
        SetTexture (globalDetailTexture, "_GlobalDetailTex", "Generic detail texture");
        SetTexture (globalDamageTexture, "_GlobalDamageTex", "Generic damage mask texture");
        SetTexture (globalRampBurnTexture, "_GlobalRampBurnTex", "Generic damage ramp texture");
        SetTexture (globalRampTerraceTexture, "_GlobalRampTerraceTexture", "Generic height based terrace texture");

        SetTexture (globalBackgroundDetailTexture, "_GlobalBackgroundDetailTex", "Background detail texture");
        SetTexture (globalBackgroundGrassTexture, "_GlobalBackgroundGrassTex", "Background detail texture");
        SetTexture (globalBackgroundAsphaltTexture, "_GlobalBackgroundAsphaltTex", "Background asphalt texture");
    }

    private static void SetTexture (Texture texture, string property, string description)
    {
        if (texture != null)
            Shader.SetGlobalTexture (property, texture);
        else if (debug)
            Debug.Log ($"SH | UpdateGlobals | {description} is null, aborting shader global setting...");
    }

    public static void LoadConfig ()
    {
        config = UtilitiesYAML.LoadDataFromFile<ShaderGlobalConfig> (configFilePath, configFileName);
    }

    public static void SaveConfig ()
    {
        UtilitiesYAML.SaveDataToFile (configFilePath, configFileName, config);
    }

    public static void SetOcclusionShift (float shift)
    {
        if (config == null)
            return;

        config.globalEnvironmentAOShift = shift;
        Shader.SetGlobalVector ("_GlobalEnvironmentAmbientSettings", new Vector4 (config.globalEnvironmentAOSize, config.globalEnvironmentAOShift, config.globalEnvironmentAOToggle, config.globalEnvironmentColorSaturation));
    }
}

[ExecuteInEditMode]
public class ShaderHelper : MonoBehaviour
{
    public Texture globalUnitDetailTex;
    public Texture globalUnitDetailTexNew;
    public Texture globalUnitDamageTex;
    public Texture globalUnitDamageTexNew;
    public Texture globalUnitDamageTexNewSecondary;
    public Texture globalUnitRampTex;
    public Texture globalUnitIridescenceTex;
    
    public Texture globalDetailTexture;
    public Texture globalDamageTexture;
    public Texture globalRampBurnTexture;
    public Texture globalRampTerraceTexture;

    public Texture globalBackgroundDetailTexture;
    public Texture globalBackgroundGrassTexture;
    public Texture globalBackgroundAsphaltTexture;
    private static int propertyID_GlobalUnscaledTime = Shader.PropertyToID ("_GlobalUnscaledTime");
    private static int propertyID_GlobalFrameCount = Shader.PropertyToID ("_GlobalFrameCount");

    private void OnEnable ()
    {
        if (Utilities.isPlaymodeChanging)
            return;

        LoadTextures ();
        ShaderGlobalHelper.LoadConfig ();
        ShaderGlobalHelper.UpdateGlobals ();
    }

    public void LoadTextures ()
    {
        ShaderGlobalHelper.globalUnitDetailTex = globalUnitDetailTex;
        ShaderGlobalHelper.globalUnitDetailTexNew = globalUnitDetailTexNew;
        ShaderGlobalHelper.globalUnitDamageTex = globalUnitDamageTex;
        ShaderGlobalHelper.globalUnitRampTex = globalUnitRampTex;
        ShaderGlobalHelper.globalUnitIridescenceTex = globalUnitIridescenceTex;

        ShaderGlobalHelper.globalDetailTexture = globalDetailTexture;
        ShaderGlobalHelper.globalDamageTexture = globalDamageTexture;
        ShaderGlobalHelper.globalUnitDamageTexNew = globalUnitDamageTexNew;
        ShaderGlobalHelper.globalUnitDamageTexNewSecondary = globalUnitDamageTexNewSecondary;
        ShaderGlobalHelper.globalRampBurnTexture = globalRampBurnTexture;
        ShaderGlobalHelper.globalRampTerraceTexture = globalRampTerraceTexture;

        ShaderGlobalHelper.globalBackgroundDetailTexture = globalBackgroundDetailTexture;
        ShaderGlobalHelper.globalBackgroundGrassTexture = globalBackgroundGrassTexture;
        ShaderGlobalHelper.globalBackgroundAsphaltTexture = globalBackgroundAsphaltTexture;
    }

    public void Update ()
    {
        float unscaledTime = Time.unscaledTime;

        Shader.SetGlobalVector (propertyID_GlobalUnscaledTime, new Vector4
                        (
                            unscaledTime / 20f,
                            unscaledTime,
                            unscaledTime * 2f,
                            unscaledTime * 3f
                        ));
        Shader.SetGlobalFloat (propertyID_GlobalFrameCount, Time.frameCount);
    }
}
