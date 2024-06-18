Shader "Hardsurface/Parts/Base (mech)"
{
	Properties
	{
        [Toggle (PART_USE_TRIPLANAR)]
        _UseTriplanar ("Use Triplanar", Float) = 0

        [Toggle (PART_USE_MSEO)]
        _UseMSEO ("Use MSEO", Float) = 0

        [Toggle (PART_USE_ARRAYS)]
        _UseArrays ("Use Arrays", Float) = 0

        [Space (10)]
        [Header (Livery Color Customization)]
        [Space (5)]
		_ColorBackground ("Color (background)", Color) = (0.25,0.25,0.25,1)
		_ColorPrimary ("Color (primary, VC-R)", Color) = (0.7,0.5,0.5,1)
		_ColorSecondary ("Color (secondary, VC-G)", Color) = (0.5,0.7,0.5,1)
		_ColorTertiary ("Color (tertiary, VC-B)", Color) = (0.25,0.25,0.25,1)

        [Space (10)]
        [Header (Main Textures)]
        [Space (5)]
		[NoScaleOffset] _MainTex ("Main map", 2D) = "white" {}
        [NoScaleOffset] _MSEOTex ("MSEO map", 2D) = "white" {}
        [NoScaleOffset] _NormalTex ("Normal map", 2D) = "bump" {}

        [Space (10)]
        [Header (Livery Pattern)]
        [Space (5)]
        [NoScaleOffset] _PaintTex ("Livery Pattern map", 2D) = "gray" {}
        _liveryPatternIntensity ("Livery Pattern intensity", Vector) = (0.0, 0.0, 0.0, 0.0)
        _PaintTiling ("Livery Pattern tiling", Range (0.1, 30)) = 1

        [Space (10)]
        [Header (Surface Details)]
        [Space (5)]
        [Toggle (PART_USE_DETAIL_TEX)]
        _UseDetailTex ("Use Detail Texture", Float) = 0
        [HideIfDisabled(PART_USE_DETAIL_TEX)] _DetailTexAlbedoIntensity ("Detail Albedo Intensity", Range (0.01, 1)) = 0.5
        [HideIfDisabled(PART_USE_DETAIL_TEX)] _DetailTexNormalIntensity ("Detail Normals Intensity", Range (0.01, 1)) = 0.5
		[HideIfDisabled(PART_USE_DETAIL_TEX)] _DetailTexTiling ("Detail tiling multiplier", Range (0.1, 30)) = 1

        [Space (10)]
        [Header (Fresnel Effects)]
        [Space (5)]
		[NoScaleOffset] _FresnelTex ("Fresnel map", 2D) = "white" {}

        [Space (10)]
        [Header (Albedo Settings)]
        [Space (5)]
        _AlbedoMaskWear ("Albedo mask wear", Range (0, 1)) = 0
        _AlbedoMaskWearMultiplier ("Albedo mask wear multiplier", Range (1, 4)) = 1
        _AlbedoMaskWearPower ("Albedo mask wear power", Range (1, 4)) = 1

        _AlbedoBrightness ("Albedo Brightness", Range (0.25, 2)) = 1
        _AlbedoSaturation ("Albedo Saturation", Range (0.25, 2)) = 1

        [Space (10)]
        [Header (Metalness)]
        [Space (5)]
		_Metalness ("Metalness (primary,secondary,tertiary)", Vector) = (0, 0, 0, 0)

        [Space (10)]
        [Header (Smoothness)]
        [Space (5)]
		_SmoothnessMin ("Smoothness (primary)", Vector) = (0.0, 0.5, 1.0, 0.0)
		_SmoothnessMed ("Smoothness (secondary)", Vector) = (0.0, 0.5, 1.0, 0.0)
		_SmoothnessMax ("Smoothness (tertiary)", Vector) = (0.0, 0.5, 1.0, 0.0)

        [Space (10)]
        [Header (Emission)]
        [Space (5)]
		_EmissionIntensity ("Emission Intensity", Range (0, 10)) = 3
        _EmissionColorBoost ("Emission Color Boost", Range (0, 1)) = 0
        _EmissionColorSaturation ("Emission Color Saturation", Range (0, 5)) = 1

        _LocalHighlightIntensityTweak ("Optional highlight intensity tweak", Range (0, 1)) = 1

        [Space (10)]
        [Header (Occlusion)]
        [Space (5)]
		_AlbedoOcclusionIntensity ("Albedo occlusion overlay", Range (0, 1)) = 1

        [Space (10)]
        [Header (Damage Effects)]
        [Space (5)]
		_Damage ("Damage (actual, critical, invisibility, alpha influence)", Vector) = (0, 0, 0, 1)
        _DamageMapScale ("Damage map scale", Range (0.25, 4)) = 1
        _DestructionAreaAddition ("Destruction area addition", Range (0, 4)) = 1.5
        _DestructionAreaMultiplier ("Destruction area multiplier", Range (-2, 0)) = -1.5
        _DestructionAreaPosition ("Destruction area position (XYZ)", Vector) = (0, 0, 0, 1)
        _StripParameters ("Stripping direction (XYZ)", Vector) = (0, 1, 0, 1)

        [Space (10)]
        [Header (Overheating)]
        [Space (5)]
        [HDR] _OverheatColor ("Overheat color", Color) = (0, 0, 0, 0)

        [Space (10)]
        [Header (Pixel Overlay)]
        [Space (5)]
		_PixelOverlayIntensity ("Pixel overlay intensity", Range(0, 1)) = 0
		[HDR] _PixelOverlayColor ("Pixel overlay color", Color) = (1, 1, 1, 1)
		_PixelOverlayUVScale ("Pixel overlay UV scale", Float) = 0.8
		_PixelOverlayDotScale ("Pixel overlay dot scale", Float) = 0.4 

        [Space (10)]
        [Header (Debug)]
        [Space (5)]
		_DebugVertexRGB ("Debug vertex color (RGB)", Range (0, 1)) = 0
		_DebugVertexA ("Debug vertex color (A)", Range (0, 1)) = 0
        
        _ArrayOverrideIndex ("Array test index", Range (-1,17)) = 6
        _ArrayOverrideMode ("Array test mode", Range (0,1)) = 0
        // _ArrayIndexFinder ("Array index highlight", Range (-1,30)) = 6
        
		//[HideInInspector] _Cutoff ("Alpha cutoff", Range (0,1)) = 0.5

        // [Header (Globals)]
        // _GlobalUnitDetailScale ("Detail map scale", Range (0.5, 16)) = 4
        // _GlobalUnitDamageScale ("Damage map scale", Range (0.5, 16)) = 4
        // _GlobalUnitRampSize ("Ramp size", Range (0.0, 1.0)) = 0.3
        // _GlobalUnitDetailOffset ("Detail map offset (XYZ)", Vector) = (0, 0, 0, 0)
        // _GlobalUnitDamageOffset ("Damage map offset (XYZ)", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		LOD 200

		ZWrite On
		ZTest LEqual
		Blend One Zero
		Cull Off

		CGPROGRAM
		#pragma surface surf Standard vertex:vert exclude_path:prepass addshadow noforwardadd keepalpha // alphatest:_Cutoff
		#pragma target 5.0
        #pragma shader_feature_local PART_USE_TRIPLANAR
        #pragma shader_feature_local PART_USE_MSEO
        #pragma shader_feature_local PART_USE_ARRAYS
        #pragma shader_feature_local PART_USE_DETAIL_TEX

        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Cginc/HardsurfacePartFunctions.cginc"
        #include "Cginc/MechPartUtility.cginc"

		struct Input
		{
            float2 texcoord_uv1 : TEXCOORD0;
            float2 texcoord_uv2 : TEXCOORD1;
			float4 color : COLOR;
			float3 localPos;
			float3 localNormal;
            float3 worldNormal;
			float4 screenPos;
			float3 viewDir;
			float destructionProximity;
            float3 worldCameraDir;
            float facingSign : VFACE;
			float3 thisVertexPreSkinning;
            INTERNAL_DATA
		};

		float4 _ColorBackground;
		float4 _ColorPrimary;
		float4 _ColorSecondary;
		float4 _ColorTertiary;
		float4 _liveryPatternIntensity;

        float _DamageMapScale;

		sampler2D _MainTex;
        sampler2D _MSEOTex;
		sampler2D _PaintTex;
		float _PaintTiling;
		sampler2D _FresnelTex;
		sampler2D _NormalTex;

        float _DetailTexAlbedoIntensity;
        float _DetailTexNormalIntensity;
		float _DetailTexTiling;

        float _AlbedoMaskWear;
        float _AlbedoMaskWearMultiplier;
        float _AlbedoMaskWearPower;

        float _AlbedoBrightness;
        float _AlbedoSaturation;

        uniform float3 TOD_AmbientColor;

	    float3 _SmoothnessMin;
		float3 _SmoothnessMed;
		float3 _SmoothnessMax;
		float3 _Metalness;

		float _EmissionIntensity;
        float _EmissionColorBoost;
        float _EmissionColorSaturation;

		float _DebugVertexRGB;
		float _DebugVertexA;

		float _SmoothnessMaskAdjustmentFactor;
		float _AlbedoOcclusionIntensity;

        float _DestructionAreaAddition;
        float _DestructionAreaMultiplier;
        float4 _DestructionAreaPosition;

        float4 _OverheatColor;

        float4 _StripParameters;
        // float _ArrayIndexFinder;

		float _PixelOverlayIntensity;
		float4 _PixelOverlayColor;
		float _PixelOverlayUVScale;
		float _PixelOverlayDotScale;

        // Local variable to optionally tweak the highlight intensity per material if needed (DON'T tweak on armor or weapons for visual consistency)
		float _LocalHighlightIntensityTweak;
        // Global variable to turn highlight on and off depending on game's state (in-combat or somewhere else)
        float _MechOverheadLightIntensity;
        
		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			o.localNormal = v.normal;
			o.localPos = v.vertex;

            o.texcoord_uv1 = v.texcoord;
            o.texcoord_uv2 = float2 (lerp (v.texcoord1.x, _ArrayOverrideIndex, _ArrayOverrideMode) + 0.5, 0); 

            // World space direction from the camera towards given object space vertex position
            o.worldCameraDir = -normalize (WorldSpaceViewDir (v.vertex));
            
			o.thisVertexPreSkinning = float3 (v.texcoord1.y, v.texcoord2.x, v.texcoord2.y);

            #if PART_USE_ARRAYS
                float arrayIndex = o.texcoord_uv2.x;
                float4 arrayData_damage = _ArrayForDamage[arrayIndex];
                float4 damageInput = arrayData_damage;
            #else
                float4 damageInput = _Damage;
            #endif

            float3 vertexDistorted;

            ApplyDamageVert
            (
                damageInput,
                v.vertex,
                o.thisVertexPreSkinning,
                v.normal,
                _DestructionAreaPosition,
                _DestructionAreaMultiplier,
                _DestructionAreaAddition,
				_StripParameters,
                o.destructionProximity,
                vertexDistorted
            );

            v.vertex.xyz = vertexDistorted;
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
            float3 normalTest = float3 (0, 0, 1);
            float3 normal = UnpackNormal(tex2D (_NormalTex, IN.texcoord_uv1));
            float backsideFactor = saturate (IN.facingSign);
            float3 normalClean = normal;

            // Fresnel effect mask based on per-pixel normal, creates a higher quality highlight
            float3 worldNormal = WorldNormalVector (IN, normalClean);
            float fresnel = pow(1.0 + dot(IN.worldCameraDir, worldNormal), 2) / 2;

            fixed4 main = tex2D (_MainTex, IN.texcoord_uv1);
            fixed3 mainGrayscale = RGBToGrayscale (main.xyz);
            fixed4 liveryPattern = tex2D (_PaintTex, IN.texcoord_uv1 * _PaintTiling * 0.5f);

            #if PART_USE_DETAIL_TEX
                float4 detailTex = tex2D (_GlobalUnitDetailTexNew, IN.texcoord_uv1 * _DetailTexTiling);

                float detailAlbedo = lerp (0.5f, detailTex.r, _DetailTexAlbedoIntensity);
                main.rgb = Overlay (main.rgb, detailAlbedo);

                float3 detailNormal = float3 (((detailTex.gb - 0.5) * 2), 0.0f) * _DetailTexNormalIntensity;
                normalClean = normalize (normalClean + detailNormal);
            #endif

            #if PART_USE_ARRAYS

                float arrayIndex = IN.texcoord_uv2.x;
                // arrayIndex = arrayIndex + 0.5; // always use an index between two integers, to guarantee sampling of correct slice

                float4 arrayData_colorPrimary = _ArrayForColorPrimary[arrayIndex];
                float4 arrayData_colorSecondary = _ArrayForColorSecondary[arrayIndex];
                float4 arrayData_colorTertiary = _ArrayForColorTertiary[arrayIndex];
                float4 arrayData_smoothnessPrimary = _ArrayForSmoothnessPrimary[arrayIndex];
                float4 arrayData_smoothnessSecondary = _ArrayForSmoothnessSecondary[arrayIndex];
                float4 arrayData_smoothnessTertiary = _ArrayForSmoothnessTertiary[arrayIndex];
                float4 arrayData_metalness = _ArrayForMetalness[arrayIndex];
                float4 arrayData_effect = _ArrayForEffect[arrayIndex];
                float4 arrayData_damage = _ArrayForDamage[arrayIndex];

            #endif

            #if PART_USE_MSEO

                fixed4 mseo = tex2D (_MSEOTex, IN.texcoord_uv1);
                fixed metalnessSample = mseo.x;
                fixed smoothnessSample = mseo.y;
                fixed emissionSample = mseo.z;
                fixed occlusionSample = mseo.w;

                #if PART_USE_ARRAYS
                float metalnessClean = SelectMaskedValueFromVC (IN.color, metalnessSample, float4 (arrayData_metalness.xyz, 0));
                #else
                float metalnessClean = SelectMaskedValueFromVC (IN.color, metalnessSample, float4 (_Metalness.xyz, 0));
                #endif

            #else

                fixed smoothnessSample = mainGrayscale;
                fixed emissionSample = 0;
                fixed occlusionSample = main.w;

                #if PART_USE_ARRAYS
                    float metalnessClean = SelectValueFromVC (IN.color, float4 (arrayData_metalness.xyz, 0));
                #else
                    float metalnessClean = SelectValueFromVC (IN.color, float4 (_Metalness.xyz, 0));
                #endif

            #endif

            float4 damageSample = 0.0f;
            float4 damageSampleSecondary = 0.0f;

            #if PART_USE_ARRAYS
                float4 damageInput = arrayData_damage;
            #else
                float4 damageInput = _Damage;
            #endif
            
            #if PART_USE_TRIPLANAR

                half3 triblend = saturate (pow (IN.localNormal, 4));
                triblend /= max (dot (triblend, half3(1, 1, 1)), 0.0001);

                float detailScale = 0.25;
                float4 detailSampleX = tex2D (_GlobalUnitDetailTex, IN.thisVertexPreSkinning.yz * detailScale + _GlobalUnitDetailOffset.yz);
                float4 detailSampleY = tex2D (_GlobalUnitDetailTex, IN.thisVertexPreSkinning.xz * detailScale + _GlobalUnitDetailOffset.xz);
                float4 detailSampleZ = tex2D (_GlobalUnitDetailTex, IN.thisVertexPreSkinning.xy * detailScale + _GlobalUnitDetailOffset.xy);
                float4 detailSample = detailSampleX.xyzw * triblend.x + detailSampleY.xyzw * triblend.y + detailSampleZ.xyzw * triblend.z;

                if ((damageInput.x + damageInput.y) > 0.0f)
                {
                    float damageScale = (1 / _DamageMapScale);
                    float4 damageSampleX = tex2D (_GlobalUnitDamageTex, IN.thisVertexPreSkinning.yz * damageScale + _GlobalUnitDamageOffset.yz);
                    float4 damageSampleY = tex2D (_GlobalUnitDamageTex, IN.thisVertexPreSkinning.xz * damageScale + _GlobalUnitDamageOffset.xz);
                    float4 damageSampleZ = tex2D (_GlobalUnitDamageTex, IN.thisVertexPreSkinning.xy * damageScale + _GlobalUnitDamageOffset.xy);

                    damageSample = damageSampleX.xyzw * triblend.x + damageSampleY.xyzw * triblend.y + damageSampleZ.xyzw * triblend.z;

                    float4 damageSampleSecondaryX = tex2D (_GlobalUnitDamageTexNewSecondary, IN.thisVertexPreSkinning.yz * damageScale + _GlobalUnitDamageOffset.yz);
                    float4 damageSampleSecondaryY = tex2D (_GlobalUnitDamageTexNewSecondary, IN.thisVertexPreSkinning.xz * damageScale + _GlobalUnitDamageOffset.xz);
                    float4 damageSampleSecondaryZ = tex2D (_GlobalUnitDamageTexNewSecondary, IN.thisVertexPreSkinning.xy * damageScale + _GlobalUnitDamageOffset.xy);

                    damageSampleSecondary = damageSampleSecondaryX.xyzw * triblend.x + damageSampleSecondaryY.xyzw * triblend.y + damageSampleSecondaryZ.xyzw * triblend.z;
                }
            #else

                float detailScale = 1;
                float4 detailSample = tex2D (_GlobalUnitDetailTex, IN.texcoord_uv1 * detailScale + _GlobalUnitDetailOffset.xy);

                if ((damageInput.x + damageInput.y) > 0.0f)
                {
                    float damageScale = _DamageMapScale;
                    damageSample = tex2D (_GlobalUnitDamageTexNew, IN.texcoord_uv1 * damageScale + _GlobalUnitDamageOffset.xy);
                    damageSampleSecondary = tex2D (_GlobalUnitDamageTexNewSecondary, IN.texcoord_uv1 * damageScale + _GlobalUnitDamageOffset.xy);
                }

            #endif

            fixed detailNoise = detailSample.z;
            fixed detailPattern = detailSample.w;

            float albedoColorMaskHigh = pow (saturate (abs ((mainGrayscale * 2 - 1) * _AlbedoMaskWearMultiplier)), _AlbedoMaskWearPower);
            float albedoColorMaskLow = pow (saturate (abs ((1 - occlusionSample) * _AlbedoMaskWearMultiplier)), _AlbedoMaskWearPower);
            float albedoColorMask = saturate (albedoColorMaskLow + albedoColorMaskHigh);

            // Used to drop some components to 0 on areas with 0,0,0 vertex color while keeping them at full intensity if R, G or B are at 1
            float vcClearMultiplier = saturate (IN.color.x + IN.color.y + IN.color.z);

            #if PART_USE_ARRAYS

                float4 paintIntensities = float4 (arrayData_colorPrimary.w, arrayData_colorSecondary.w, arrayData_colorTertiary.w, 0);
                float liveryPatternIntensity = SelectValueFromVC (IN.color, paintIntensities);

            #else

                float liveryPatternIntensity = SelectValueFromVC (IN.color, _liveryPatternIntensity);

            #endif


            float3 albedoLiveryPattern = lerp
            (
                0.5f,
                liveryPattern,
                liveryPatternIntensity * saturate ((1 - smoothnessSample) * 2) * vcClearMultiplier * occlusionSample
            );

            float3 emissionClean = 0;
            // Use tertiary color as emission color
            float3 emissionColor = _ColorTertiary;
            float effectIntensity = 0;

            #if PART_USE_ARRAYS

                float3 albedoColor = SelectColorFromVC
                (
                    IN.color,
                    _ColorBackground,
                    pow (arrayData_colorPrimary, 2.22),
                    pow (arrayData_colorSecondary, 2.22),
                    pow (arrayData_colorTertiary, 2.22)
                );
                // Override emission color with array data
                emissionColor = pow (arrayData_colorTertiary, 2.22);
                
                // Fallback case to make array-driven mats visible outside the game mode
                if (_GlobalPlayMode < 0.1)
                {
                    albedoColor = SelectColorFromVC
                    (
                        IN.color,
                        _ColorBackground,
                        _ColorPrimary,
                        _ColorSecondary,
                        _ColorTertiary
                    );
                }

                float4 effectInputForSelection = float4 (arrayData_effect.x, arrayData_effect.y, arrayData_effect.z, 0);
                effectIntensity = SelectValueFromVC (IN.color, effectInputForSelection);

            #else

                float3 albedoColor = SelectColorFromVC
                (
                    IN.color,
                    _ColorBackground,
                    _ColorPrimary,
                    _ColorSecondary,
                    _ColorTertiary
                );

            #endif

            // Adjust albedo saturation, used to make a color\value gradient spanning across the whole mech (less saturated at the bottom, saturated at the top)
            albedoColor = RGBSaturation(albedoColor, _AlbedoSaturation);

            // Optinonally tighten up emission color (useful when tertiary color is rather dark)
            emissionColor = saturate(emissionColor + _EmissionColorBoost);
            
            // Optionally increase or decrease saturation
            emissionColor = RGBSaturation(emissionColor, _EmissionColorSaturation);

            emissionClean = emissionSample * emissionColor * _EmissionIntensity;
            
            float3 albedoLiveryAndPaint = saturate (albedoColor * albedoLiveryPattern * 2);

            // Add fresnel based effect
            // TODO: Switch this to Tex2DArray later to enable a gamut of fresnel based effects, selecting an index using arrayData_effect.w
            float3 effectSample = tex2D (_GlobalUnitIridescenceTex, float2 (fresnel * 3, 0)).rgb;
            albedoLiveryAndPaint *= lerp (1, effectSample, effectIntensity);
            
            float3 albedoClean = Overlay (albedoLiveryAndPaint, main.xyz);
            float3 albedoPlain = main.xyz * pow (occlusionSample, 4);

            albedoClean = lerp
            (
                albedoClean,
                albedoPlain,
                _AlbedoMaskWear * albedoColorMask
            );
            
            // Adjust albedo brightness, used to make a color\value gradient spanning across the whole mech (dark at the bottom, bright at the top)
            albedoClean *= _AlbedoBrightness;
            albedoClean *= lerp (1, occlusionSample, _AlbedoOcclusionIntensity);

            #if PART_USE_ARRAYS
                float3 smoothnessRemapInputs = SelectColorFromVC
                (
                    IN.color,
                    float3 (0, 0, 0),
                    arrayData_smoothnessPrimary,
                    arrayData_smoothnessSecondary,
                    arrayData_smoothnessTertiary
                );
            #else
                float3 smoothnessRemapInputs = SelectColorFromVC
                (
                    IN.color,
                    float3 (0, 0, 0),
                    _SmoothnessMin,
                    _SmoothnessMed,
                    _SmoothnessMax
                );
            #endif

            float smoothnessClean = RemapSmoothness (smoothnessSample, smoothnessRemapInputs);
            smoothnessClean = lerp (smoothnessSample, smoothnessClean, vcClearMultiplier);
            
            float3 albedoAfterDamage;
            float smoothnessAfterDamage;
            float metalnessAfterDamage;
            float3 emissionAfterDamage;
            float3 normalAfterDamage;

            float albedoDamageMask = saturate ((mainGrayscale.x - 0.15) * 2);
            albedoDamageMask = pow (albedoDamageMask, 4);
            
            // maskTest = pow (saturate (((mainGrayscale * 2 - 1) * _AlbedoMaskWearMultiplier)), _AlbedoMaskWearPower);

            // Background color mask to reduce damage effects in dark areas of the armor
            float backgroundColorExclusionMask = saturate(IN.color.r + IN.color.g + IN.color.b);

            ApplyDamage
            (
                damageInput,
                damageSample,
                damageSampleSecondary,
                backsideFactor,
                IN.destructionProximity,
                albedoClean,
                albedoDamageMask,
                mainGrayscale,
                smoothnessClean,
                metalnessClean,
                emissionClean,
                normalClean,
                _AlbedoBrightness * backgroundColorExclusionMask,
                1.0f,
                albedoAfterDamage,
                smoothnessAfterDamage,
                metalnessAfterDamage,
                emissionAfterDamage,
                normalAfterDamage
            );

            // Uncomment to debug array index in UV2
            // albedoAfterDamage += lerp (float3 (1, 0, 0), float3 (0, 0, 1), IN.texcoord_uv2.x * 0.05);

            // Fresnel effect mask based on per-pixel normal, this uses the normal after damage has been applied
            float3 worldNormalAfterDamage = WorldNormalVector (IN, normalAfterDamage);
            float fresnelAfterDamage = pow(1.0 + dot(IN.worldCameraDir, worldNormalAfterDamage), 2) / 2;

            float highlightMask = fresnelAfterDamage * saturate (worldNormalAfterDamage.g + abs(worldNormalAfterDamage.r * 0.2));
            // Highlight effect, masked by AO and by smoothness map (this one is boosted quite a bit)
            emissionAfterDamage += highlightMask * TOD_AmbientColor * 5 * occlusionSample * saturate(smoothnessAfterDamage * 6) * _MechOverheadLightIntensity * _LocalHighlightIntensityTweak;
            // Overheating effect
            emissionAfterDamage += highlightMask * pow (saturate (IN.localPos.y * 0.2), 3) * _OverheatColor;

            // Pixel overlay
            if (_PixelOverlayIntensity > 0.01)
            {
                float glint = -(IN.localPos.x / 4) - (IN.localPos.y / 4) - ((1 - _PixelOverlayIntensity) * 4) + 1.8;
                glint = saturate (glint * step(glint, 1));
                float2 pixelPos = ((IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy);
                float damageTex = tex2D (_GlobalUnitDamageTex, IN.texcoord_uv1).r;
                float pixelOverlay = min(step(_PixelOverlayDotScale, pixelPos.x % _PixelOverlayUVScale), step(_PixelOverlayDotScale, pixelPos.y % _PixelOverlayUVScale)) * step(damageTex, _PixelOverlayIntensity).r;
                pixelOverlay += min(step(_PixelOverlayDotScale, pixelPos.x % (_PixelOverlayUVScale * 1.01)), step(_PixelOverlayDotScale, pixelPos.y % (_PixelOverlayUVScale * 1.01))) * step(damageTex, _PixelOverlayIntensity / 2).r;
                pixelOverlay = pixelOverlay * fresnelAfterDamage * 10;
                pixelOverlay *= step(damageTex, _PixelOverlayIntensity);
                pixelOverlay *= glint;
                emissionAfterDamage += saturate(pixelOverlay + pow(saturate((glint - 0.5) * 2), 4)) * _PixelOverlayColor;
            }
            
            o.Normal = normalAfterDamage;
            o.Albedo = albedoAfterDamage;
            o.Metallic = metalnessAfterDamage;
            o.Smoothness = smoothnessAfterDamage;
            o.Occlusion = occlusionSample;
            o.Emission = emissionAfterDamage;
            
            // Array override index test
            // float indexScaled = _ArrayOverrideIndex / 10;
            // float hue = _ArrayOverrideIndex < 0 ? 0 : 1;
            
            // if (abs (IN.texcoord_uv2.x - _ArrayIndexFinder) < 0.6)
            //     o.Emission = float3 (1, 1, 1);
            

            // Range test
            // if (smoothnessAfterDamage > 0.55) o.Albedo = float3 (smoothnessAfterDamage, 0, 0);
            // else if (smoothnessAfterDamage < 0.45) o.Albedo = float3 (0, 0, smoothnessAfterDamage);
            // else o.Albedo = float3 (0, smoothnessAfterDamage, 0);

            // Index test
            // float indexScaled = (IN.texcoord_uv2.x - _ArrayOffset) / 2;
            // float hue = (IN.texcoord_uv2.x - _ArrayOffset) < 0 ? 0 : 1;
            // o.Albedo = HSVToRGB (float3 (indexScaled, hue, 1 - saturate (indexScaled)));
            
            // Array override index test
            // float indexScaled = _ArrayOverrideIndex / 10;
            // float hue = _ArrayOverrideIndex < 0 ? 0 : 1;
            // o.Albedo = HSVToRGB (float3 (indexScaled, hue, 1 - saturate (indexScaled)));

            // Vertex color test
            // o.Albedo = lerp (lerp (albedoAfterDamage, float3(0, 0, 0), _DebugVertexRGB), float3(0, 0, 0), _DebugVertexA);
            // o.Emission = lerp (lerp (float3(0, 0, 0), IN.color.rgb, _DebugVertexRGB), IN.color.a, _DebugVertexA);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
