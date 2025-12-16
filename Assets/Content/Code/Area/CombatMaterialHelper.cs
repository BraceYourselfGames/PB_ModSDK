using System;
using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

public class CombatMaterialHelper : MonoBehaviour
{
    [Serializable]
    public class TerrainMaterialLayer
    {
        [PreviewField, HideLabel]
        public Texture2D texAlbedoSmoothness;
        
        [PreviewField, HideLabel]
        public Texture2D texNormalHeight;
    }
    
    [Serializable]
    public class TerrainPreset
    {
        // Primary (e.g. grass)
        [Min (0)]
        public int indexRed;
        
        // Secondary (e.g. mossy rock)
        [Min (0)]
        public int indexGreen;
        
        // Tertiary (e.g. rock)
        [Min (0)]
        public int indexBlue;
        
        // Decal (e.g. leaves)
        [Min (0)]
        public int indexAlpha;
    }

    public enum TextureSize
    {
        x256 = 256,
        x512 = 512,
        x1024 = 1024,
        x2048 = 2048
    }
    
    [BoxGroup ("Presets")]
    public Vector2 parametersScaleBase = new Vector2 (100f, 10f);
    
    [BoxGroup ("Presets")]
    public Vector2 parametersScaleCombat = new Vector2 (300f, 12f);

    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 parametersScale;
    
    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 parametersBlendContrast;
    
    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 parametersBlendOffset;
    
    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Texture2D texSplat;
    
    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Texture2D texDistant;
    
    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Texture2D texSlope;

    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Texture2D texWeather;

    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Texture2D texGradient;
    
    [BoxGroup ("Properties")]
    [OnValueChanged ("ApplyOther")]
    public Color slopeTint;
    
    [BoxGroup ("Details")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 hsbDetail1;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail1AlbedoSmoothness;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail1NormalHeight;
    
    [BoxGroup ("Details")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 hsbDetail2;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail2AlbedoSmoothness;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail2NormalHeight;
    
    [BoxGroup ("Details")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 hsbDetail3;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail3AlbedoSmoothness;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail3NormalHeight;
    
    [BoxGroup ("Details")]
    [OnValueChanged ("ApplyOther")]
    public Vector4 hsbDetail4;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail4AlbedoSmoothness;
    
    [BoxGroup ("Details")]
    public Texture2D texDetail4NormalHeight;

    [BoxGroup ("Vegetation")]
    [Tooltip ("RGB - primary tint, A - tint amount")]
    public Color vegetationColorPrimary;
    
    [BoxGroup ("Vegetation")]
    [Tooltip ("RGB - secondary tint, A - foliage amount")]
    public Color vegetationColorSecondary;

    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals")]
    [PropertyRange (0f, 1f)]
    public float rainIntensity = 0f;
    
    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals")]
    [PropertyRange (0f, 1f)]
    public float snowSurfaceIntensity = 0f;
    
    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals")]
    [PropertyRange (0f, 1f)]
    public float snowSurfaceIntensityFromSnowFall = 0.15f;
    
    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals")]
    [PropertyRange (0f, 1f)]
    public float snowFallIntensity = 0f;

    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals")]
    [PropertyTooltip ("X: height, Y: shadow size, Z: glow size, W: inversion")]
    public Vector4 sliceInputs = new Vector4 (10f, 6f, 0f);

    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals")]
    [PropertyTooltip ("RGB: tint, A: intensity")]
    [ColorUsage (true, true)]
    public Color sliceColor = new Color (1f, 1f, 1f, 1f);
    
    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals"), NonSerialized, ShowInInspector]
    public bool sliceShadingEnabled = false;
    
    [BoxGroup ("Globals")]
    [OnValueChanged ("ApplyGlobals"), NonSerialized, ShowInInspector]
    public bool sliceCutoffEnabled = false;
    
    [BoxGroup ("Isolines")]
    [OnValueChanged ("ApplyIsoline")]
    public Color isolineColorIdle = new Color (0f, 0.5f, 1f, 0f);
    
    [BoxGroup ("Isolines")]
    [OnValueChanged ("ApplyIsoline")]
    public Color isolineColorFull = new Color (0f, 0.5f, 1f, 0.25f);

    [BoxGroup ("Isolines")]
    [OnValueChanged ("ApplyIsoline")]
    [PropertyRange (0f, 1f)]
    public float isolineIntensity = 0f;
    
    [BoxGroup ("Isolines")]
    [OnValueChanged ("ApplyIsoline")]
    [PropertyRange (0f, 1f)]
    public float isolineAnimationLength = 0.5f;

    private static readonly int propertyID_TexSplat = Shader.PropertyToID ("_CombatTerrainTexSplat");
    private static readonly int propertyID_TexGradient = Shader.PropertyToID ("_CombatTerrainTexGradient");
    private static readonly int propertyID_TexSlope = Shader.PropertyToID ("_CombatTerrainTexSlope");
    private static readonly int propertyID_TexWeather = Shader.PropertyToID ("_CombatTerrainTexWeather");
    private static readonly int propertyID_TexDistant = Shader.PropertyToID ("_CombatTerrainTexDistant");
    private static readonly int propertyID_SliceInputs = Shader.PropertyToID ("_GlobalEnvironmentSliceInputs");
    private static readonly int propertyID_SliceColor = Shader.PropertyToID ("_GlobalEnvironmentSliceColor");
    
    private static readonly int propertyID_ParamsScale = Shader.PropertyToID ("_CombatTerrainParamsScale");
    private static readonly int propertyID_ParamsBlendContrast = Shader.PropertyToID ("_CombatTerrainParamsBlendContrast");
    private static readonly int propertyID_ParamsBlendOffset = Shader.PropertyToID ("_CombatTerrainParamsBlendOffset");
    
    private static readonly int propertyID_CombatTerrain1HSB = Shader.PropertyToID ("_CombatTerrainDetail1HSB");
    private static readonly int propertyID_CombatTerrainTex1AS = Shader.PropertyToID ("_CombatTerrainTexDetail1AH");
    private static readonly int propertyID_CombatTerrainTex1NH = Shader.PropertyToID ("_CombatTerrainTexDetail1NH");
    
    private static readonly int propertyID_CombatTerrain2HSB = Shader.PropertyToID ("_CombatTerrainDetail2HSB");
    private static readonly int propertyID_CombatTerrainTex2AS = Shader.PropertyToID ("_CombatTerrainTexDetail2AH");
    private static readonly int propertyID_CombatTerrainTex2NH = Shader.PropertyToID ("_CombatTerrainTexDetail2NH");
    
    private static readonly int propertyID_CombatTerrain3HSB = Shader.PropertyToID ("_CombatTerrainDetail3HSB");
    private static readonly int propertyID_CombatTerrainTex3AS = Shader.PropertyToID ("_CombatTerrainTexDetail3AH");
    private static readonly int propertyID_CombatTerrainTex3NH = Shader.PropertyToID ("_CombatTerrainTexDetail3NH");
    
    private static readonly int propertyID_CombatTerrain4HSB = Shader.PropertyToID ("_CombatTerrainDetail4HSB");
    private static readonly int propertyID_CombatTerrainTex4AS = Shader.PropertyToID ("_CombatTerrainTexDetail4AH");
    private static readonly int propertyID_CombatTerrainTex4NH = Shader.PropertyToID ("_CombatTerrainTexDetail4NH");
    
    private static readonly int propertyID_CombatVegetationColor1 = Shader.PropertyToID ("_CombatVegetationColor1");
    private static readonly int propertyID_CombatVegetationColor2 = Shader.PropertyToID ("_CombatVegetationColor2");
    
    private static readonly int propertyID_WeatherParameters = Shader.PropertyToID ("_WeatherParameters");
    private static readonly int propertyID_SlopeTint = Shader.PropertyToID ("_TintSide");
    
    private static readonly int propertyID_IsolineColor = Shader.PropertyToID ("_GlobalIsolineColor");
    
    private static readonly string keywordSliceShading = "_USE_SLICE_SHADING";
    private static readonly string keywordSliceCutoff = "_USE_SLICE_CUTOFF";
    
    private void Awake ()
    {
        ApplyAll ();
    }
    
    public void SetupSlicingForLevelEditor (bool sliceEnabled, int sliceDepth)
    {
        sliceCutoffEnabled = sliceEnabled;
        sliceShadingEnabled = sliceEnabled;
        sliceInputs = new Vector4 (-sliceDepth * 3f, 1.5f, 2f, 0f);
        // sliceColor = new Color (1f, 1f, 1f, 1f);
        ApplyGlobals ();
    }
    
    [Button ("Apply all", ButtonSizes.Medium)]
    public void ApplyAll ()
    {
        ApplyTexDetails ();
        ApplyOther ();
    }

    [ButtonGroup]
    [Button ("Apply texture arrays", ButtonSizes.Medium)]
    public void ApplyTexDetails ()
    {
        if (texDetail1AlbedoSmoothness != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex1AS, texDetail1AlbedoSmoothness);
        
        if (texDetail1NormalHeight != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex1NH, texDetail1NormalHeight);
        
        if (texDetail2AlbedoSmoothness != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex2AS, texDetail2AlbedoSmoothness);
        
        if (texDetail2NormalHeight != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex2NH, texDetail2NormalHeight);
        
        if (texDetail3AlbedoSmoothness != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex3AS, texDetail3AlbedoSmoothness);
        
        if (texDetail3NormalHeight != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex3NH, texDetail3NormalHeight);
        
        if (texDetail4AlbedoSmoothness != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex4AS, texDetail4AlbedoSmoothness);
        
        if (texDetail4NormalHeight != null)
            Shader.SetGlobalTexture (propertyID_CombatTerrainTex4NH, texDetail4NormalHeight);
    }

    [ButtonGroup]
    [Button ("Apply other", ButtonSizes.Medium)]
    public void ApplyOther ()
    {
        hsbDetail1 = new Vector4 (Mathf.Clamp01 (hsbDetail1.x), Mathf.Clamp01 (hsbDetail1.y), Mathf.Clamp01 (hsbDetail1.z), Mathf.Clamp01 (hsbDetail1.w));
        hsbDetail2 = new Vector4 (Mathf.Clamp01 (hsbDetail2.x), Mathf.Clamp01 (hsbDetail2.y), Mathf.Clamp01 (hsbDetail2.z), Mathf.Clamp01 (hsbDetail2.w));
        hsbDetail3 = new Vector4 (Mathf.Clamp01 (hsbDetail3.x), Mathf.Clamp01 (hsbDetail3.y), Mathf.Clamp01 (hsbDetail3.z), Mathf.Clamp01 (hsbDetail3.w));
        hsbDetail4 = new Vector4 (Mathf.Clamp01 (hsbDetail4.x), Mathf.Clamp01 (hsbDetail4.y), Mathf.Clamp01 (hsbDetail4.z), Mathf.Clamp01 (hsbDetail4.w));
        
        Shader.SetGlobalVector (propertyID_ParamsScale, parametersScale);
        Shader.SetGlobalVector (propertyID_ParamsBlendContrast, parametersBlendContrast);
        Shader.SetGlobalVector (propertyID_ParamsBlendOffset, parametersBlendOffset);
        
        Shader.SetGlobalVector (propertyID_CombatTerrain1HSB, hsbDetail1);
        Shader.SetGlobalVector (propertyID_CombatTerrain2HSB, hsbDetail2);
        Shader.SetGlobalVector (propertyID_CombatTerrain3HSB, hsbDetail3);
        Shader.SetGlobalVector (propertyID_CombatTerrain4HSB, hsbDetail4);
        Shader.SetGlobalColor (propertyID_SlopeTint, slopeTint);
        
        if (texSplat != null)
            Shader.SetGlobalTexture (propertyID_TexSplat, texSplat);
        
        if (texDistant != null)
            Shader.SetGlobalTexture (propertyID_TexDistant, texDistant);
        
        if (texSlope != null)
            Shader.SetGlobalTexture (propertyID_TexSlope, texSlope);

        if (texWeather != null)
            Shader.SetGlobalTexture (propertyID_TexWeather, texWeather);
    }

    private void ApplyIsoline ()
    {
        isolineIntensity = Mathf.Clamp01 (isolineIntensity);
        var color = Color.Lerp (isolineColorIdle, isolineColorFull, isolineIntensity);
        Shader.SetGlobalColor (propertyID_IsolineColor, color);
    }
    
    [ButtonGroup]
    [Button ("Apply globals", ButtonSizes.Medium)]
    public void ApplyGlobals ()
    {
        var weatherParameters = new Vector4 (rainIntensity, snowSurfaceIntensity, snowFallIntensity, 0f);
        Shader.SetGlobalVector (propertyID_WeatherParameters, weatherParameters);
        
        // X: fade height, Y: shadow size, Z: color size, W: inversion (0 = cut from above, 1 = cut from below)
        Shader.SetGlobalVector (propertyID_SliceInputs, sliceInputs);
        Shader.SetGlobalColor (propertyID_SliceColor, sliceColor);

        // Enable shadows/glows via finalgbuffer function
        UpdateKeyword (keywordSliceShading, sliceShadingEnabled);
        
        // Enable cutoff in surface functions
        UpdateKeyword (keywordSliceCutoff, sliceCutoffEnabled);
    }
    
    private void UpdateKeyword (string keyword, bool keywordEnabled)
    {
        bool keywordActive = Shader.IsKeywordEnabled (keyword);
        if (keywordEnabled != keywordActive)
        {
            if (keywordEnabled)
            {
                Shader.EnableKeyword (keyword);
                // Debug.Log ($"Shader keyword enabled: {keyword} | Confirmation: {Shader.IsKeywordEnabled (keyword)}");
            }
            else
            {
                Shader.DisableKeyword (keyword);
                // Debug.Log ($"Shader keyword disabled: {keyword} | Confirmation: {Shader.IsKeywordEnabled (keyword)}");
            }
        }
    }

    public static DataContainerCombatBiome biomeLast = null;

    public void ApplyBiomeLast ()
    {
        if (biomeLast == null)
            biomeLast = DataMultiLinkerCombatBiome.GetEntry ("mossy_neutral");
            
        if (biomeLast != null)
            ApplyBiome (biomeLast);
    }
    
    public void ApplyBiomeIfLast (DataContainerCombatBiome biome)
    {
        if (biome != null && biome == biomeLast)
            ApplyBiome (biome);
    }

    public void ApplyBiome (DataContainerCombatBiome biome, bool forceBaseScale = false)
    {
        if (biome == null)
            return;

        bool inBase = forceBaseScale;
        biomeLast = biome;

        var parametersScaleXY = inBase ? parametersScaleBase : parametersScaleCombat;
        parametersScale = new Vector4 (parametersScaleXY.x, parametersScaleXY.y, biome.gradientRange.x, biome.gradientRange.y);
        
        var core = DataMultiLinkerCombatArea.GetCurrentLevelDataCore ();
        if (core != null && core.bounds.y > 2)
        {
            float from = -core.bounds.y * 3f + 1.5f;
            from += core.gradientOffsetBottom * 3f;
            
            float to = -1.5f;
            to += core.gradientOffsetTop * 3f;
            to = Mathf.Max (from + 3f, to);
            
            parametersScale = new Vector4 (parametersScale.x, parametersScale.y, from, to);
        }

        Shader.SetGlobalVector (propertyID_ParamsScale, parametersScale);

        texSplat = DataLinkerCombatBiomes.GetTextureSplat (biome.textureSplat);
        if (texSplat != null)
            Shader.SetGlobalTexture (propertyID_TexSplat, texSplat);
        
        texDistant = DataLinkerCombatBiomes.GetTextureDistant (biome.textureDistant);
        if (texDistant != null)
            Shader.SetGlobalTexture (propertyID_TexDistant, texDistant);
        
        texSlope = DataLinkerCombatBiomes.GetTextureSlope (biome.textureSlope);
        if (texSlope != null)
            Shader.SetGlobalTexture (propertyID_TexSlope, texSlope);
        
        texGradient = biome.gradientTexture;
        if (texGradient != null)
            Shader.SetGlobalTexture (propertyID_TexGradient, texGradient);

        var s1 = biome.slot1;
        var s2 = biome.slot2;
        var s3 = biome.slot3;
        var s4 = biome.slot4;

        hsbDetail1 = s1.hsv;
        hsbDetail2 = s2.hsv;
        hsbDetail3 = s3.hsv;
        hsbDetail4 = s4.hsv;

        Shader.SetGlobalVector (propertyID_CombatTerrain1HSB, hsbDetail1);
        Shader.SetGlobalVector (propertyID_CombatTerrain2HSB, hsbDetail2);
        Shader.SetGlobalVector (propertyID_CombatTerrain3HSB, hsbDetail3);
        Shader.SetGlobalVector (propertyID_CombatTerrain4HSB, hsbDetail4);
        
        slopeTint = biome.slopeColor;
        Shader.SetGlobalColor (propertyID_SlopeTint, slopeTint);

        vegetationColorPrimary = biome.vegetationColorPrimary;
        vegetationColorSecondary = biome.vegetationColorSecondary;
        Shader.SetGlobalColor (propertyID_CombatVegetationColor1, vegetationColorPrimary);
        Shader.SetGlobalColor (propertyID_CombatVegetationColor2, vegetationColorSecondary);

        snowSurfaceIntensity = biome.weatherSnowAmount;

        if (snowFallIntensity > 0f)
        {
            // Calculate how much snow should there be on the ground during snowfall
            // Use snow intensity by 10x as a multiplier, to ensure surface doesn't jump to snow-covered instantly but is reliably present at most snowfalls
            var mul = Mathf.Clamp01 (snowFallIntensity * 10f);
            var snowSurfaceIntensityBaseline = snowSurfaceIntensityFromSnowFall * mul;
            
            // Use max operation instead of clamped addition
            // Some biomes might already define precisely tuned values like 80% that shouldn't be pushed higher
            snowSurfaceIntensity = Mathf.Max (snowSurfaceIntensity, snowSurfaceIntensityBaseline);
        }
        
        var weatherParameters = new Vector4 (rainIntensity, snowSurfaceIntensity, snowFallIntensity, 0f);
        Shader.SetGlobalVector (propertyID_WeatherParameters, weatherParameters);

        parametersBlendContrast = new Vector4
        (
            s1.blending.x,
            s2.blending.x,
            s3.blending.x,
            s4.blending.x
        );
        
        Shader.SetGlobalVector (propertyID_ParamsBlendContrast, parametersBlendContrast);

        parametersBlendOffset = new Vector4
        (
            s1.blending.y,
            s2.blending.y,
            s3.blending.y,
            s4.blending.y
        );
        
        Shader.SetGlobalVector (propertyID_ParamsBlendOffset, parametersBlendOffset);
        
        #if !PB_MODSDK
        
        var l1 = DataLinkerCombatBiomes.GetLayerData (s1.layerKey);
        if (l1 != null)
        {
            texDetail1AlbedoSmoothness = l1.albedoSmoothness?.texture;
            if (texDetail1AlbedoSmoothness != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex1AS, texDetail1AlbedoSmoothness);
            
            texDetail1NormalHeight = l1.normalHeight?.texture;
            if (texDetail1NormalHeight != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex1NH, texDetail1NormalHeight);
        }
        
        var l2 = DataLinkerCombatBiomes.GetLayerData (s2.layerKey);
        if (l2 != null)
        {
            texDetail2AlbedoSmoothness = l2.albedoSmoothness?.texture;
            if (texDetail2AlbedoSmoothness != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex2AS, texDetail2AlbedoSmoothness);
            
            texDetail2NormalHeight = l2.normalHeight?.texture;
            if (texDetail2NormalHeight != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex2NH, texDetail2NormalHeight);
        }
        
        var l3 = DataLinkerCombatBiomes.GetLayerData (s3.layerKey);
        if (l3 != null)
        {
            texDetail3AlbedoSmoothness = l3.albedoSmoothness?.texture;
            if (texDetail3AlbedoSmoothness != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex3AS, texDetail3AlbedoSmoothness);
            
            texDetail3NormalHeight = l3.normalHeight?.texture;
            if (texDetail3NormalHeight != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex3NH, texDetail3NormalHeight);
        }
        
        var l4 = DataLinkerCombatBiomes.GetLayerData (s4.layerKey);
        if (l4 != null)
        {
            texDetail4AlbedoSmoothness = l4.albedoSmoothness?.texture;
            if (texDetail4AlbedoSmoothness != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex4AS, texDetail4AlbedoSmoothness);
            
            texDetail4NormalHeight = l4.normalHeight?.texture;
            if (texDetail4NormalHeight != null)
                Shader.SetGlobalTexture (propertyID_CombatTerrainTex4NH, texDetail4NormalHeight);
        }

        #endif
    }

    [BoxGroup ("Isolines"), Button, HideInEditorMode]
    public void SetIsolineActive (bool active, bool instant)
    {
        isolineIntensity = Mathf.Clamp01 (active ? 1f : 0f);
        ApplyIsoline ();
    }

    private void AnimateIsoline (float value)
    {
        isolineIntensity = Mathf.Clamp01 (value);
        ApplyIsoline ();
    }

    #if PB_MODSDK
    public void CacheSliceSettings ()
    {
        cachedSliceCutoffEnabled = sliceCutoffEnabled;
        cachedSliceShadingEnabled = sliceShadingEnabled;
        cachedSliceInputs = sliceInputs;
        cachedSliceColor = sliceColor;
    }

    public void RestoreSliceSettings ()
    {
        sliceCutoffEnabled = cachedSliceCutoffEnabled;
        sliceShadingEnabled = cachedSliceShadingEnabled;
        sliceInputs = cachedSliceInputs;
        sliceColor = cachedSliceColor;
        ApplyGlobals ();
    }

    public void SetupSlicingForLayerMode (bool sliceEnabled, float sliceDepth, HSBColor hsb)
    {
        sliceCutoffEnabled = sliceEnabled;
        sliceShadingEnabled = sliceEnabled;
        sliceInputs = new Vector4 (sliceDepth, 0f, 0f, 0f);
        sliceColor = hsb.ToColor ();
        ApplyGlobals ();
    }

    static bool cachedSliceCutoffEnabled;
    static bool cachedSliceShadingEnabled;
    static Vector4 cachedSliceInputs;
    static Color cachedSliceColor;
    #endif
}
