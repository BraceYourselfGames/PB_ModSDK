Shader "Hardsurface/Parts/Decal (mech)"
{
    Properties
    {
        [Toggle (PART_USE_TRIPLANAR)]
        _UseTriplanar ("Use triplanar", Float) = 0

        [Toggle (PART_USE_ARRAYS)]
        _UseArrays ("Use arrays", Float) = 0
        
        _Damage ("Damage (actual, critical, invisibility, alpha influence)", Vector) = (0, 0, 0, 1)
    
        [Header (Colors)]
        _ColorBackground        ("Background",         Color) = (0.2867647, 0.2867647, 0.2867647, 1)
        _ColorPrimary           ("Primary",            Color) = (0.6323577, 0.5669425, 0.3952267, 1)
        _ColorSecondary         ("Secondary",          Color) = (0.4972184, 0.5514706, 0.1581422, 1)
        _ColorTertiary          ("Tertiary",           Color) = (0.5588288, 0.239366, 0.1849105, 1)
        _ColorMarkingsCore      ("Markings core",      Color) = (1, 1, 1, 1)
        _ColorMarkingsOutline   ("Markings outline",   Color) = (0.5, 0.5, 0.5, 1)

        [Header (Textures)]
        _MainTex                ("Composite map",           2D) = "white" {}
        _NormalTex              ("Normal map",              2D) = "bump" {}
        _HeightTex              ("Height map",              2D) = "black" {}

        _AlbedoOverlayIntensity     ("Albedo overlay intensity",        Range (0.0, 1.0)) = 1
        _AlbedoOcclusionIntensity   ("Albedo occlusion intensity",      Range (0.0, 1.0)) = 1

		_OverallAlpha           ("Overall alpha",           Range (0.0, 1.0)) = 1
		_AlbedoAlpha            ("Albedo alpha",            Range (0.0, 1.0)) = 1
		_NormalAlpha            ("Normal alpha",            Range (0.0, 1.0)) = 1
		_SpecularAlpha          ("Specular alpha",          Range (0.0, 1.0)) = 1
		_SmoothnessAlpha        ("Smoothness alpha",        Range (0.0, 1.0)) = 1
		_OcclusionAlpha         ("Occlusion alpha",         Range (0.0, 1.0)) = 1

        _Glossiness             ("Smoothness",              Range (0.0, 1.0)) = 0.5
        _Metallic               ("Metallic",                Range (0.0, 1.0)) = 0.0

        _NormalScale            ("Scale",                   Float) = 1.0
        _ParallaxScale          ("Height Scale",            Range (-0.1, 0.1)) = 0.02
        _ParallaxShift          ("Height Shift",            Range (0, 1)) = 0

        _OcclusionStrength      ("Strength",                Range (0.0, 1.0)) = 1.0
        _OcclusionMap           ("Occlusion",               2D) = "white" {}

        _EmissionColor          ("Color",                   Color) = (0,0,0)
        _EmissionMap            ("Emission",                2D) = "white" {}

        _DecalCutoffDistance    ("Decal Cutoff Distance",   Range (1, 300)) = 35

        [Header (Debug)]
        _ArrayOverrideIndex ("Array test index", Range (0,17)) = 6
        _ArrayOverrideMode ("Array test mode", Range (0,1)) = 0

        [HideInInspector] 
        _Cutoff                 ("Alpha cutoff",            Range (0.00001, 1.0)) = 0.00001

        [HideInInspector] 
        _ZWrite                 ("ZWrite",                  Float) = 0.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "PerformanceChecks" =" False" 
        }
        LOD 300

        // ------------------------------------------------------------------
        //  Shadow rendering pass

        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"
            ENDCG
        }

        // ------------------------------------------------------------------
        //  Deferred pass mult

        Pass
        {
			Name "STANDARD DEFERRED DECAL DULL"
			Tags { "LightMode" = "Deferred" }

			Blend Zero SrcColor
			ZWrite[_ZWrite]
            Offset -1, -1

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _PARALLAXMAP
            #pragma shader_feature PART_USE_TRIPLANAR
            #pragma shader_feature_local PART_USE_ARRAYS

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing

            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferred
            #pragma fragment fragDeferredDull

            #include "UnityCG.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
            #include "MechDeferredDecalInclude.cginc"
            ENDCG
        }

		// ------------------------------------------------------------------
        //  Deferred pass add
        Pass
        {
            Name "STANDARD DEFERRED DECAL ADD"
            Tags { "LightMode" = "Deferred" }

			Blend One One
			ZWrite[_ZWrite]
            Offset -1, -1

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _PARALLAXMAP
            #pragma shader_feature PART_USE_TRIPLANAR
            #pragma shader_feature_local PART_USE_ARRAYS

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing

            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferred
            #pragma fragment fragDeferredPremultAlpha

            #include "UnityCG.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
            #include "MechDeferredDecalInclude.cginc"
            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering
        Pass
        {
            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "PerformanceChecks" = "False" 
        }
        LOD 150

        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"
            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    FallBack "Standard"
    // CustomEditor "MechDeferredDecalGUI"
}
