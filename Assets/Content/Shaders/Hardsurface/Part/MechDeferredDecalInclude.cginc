half4 _ColorBackground;
half4 _ColorPrimary;
half4 _ColorSecondary;
half4 _ColorTertiary;
half4 _ColorMarkingsCore;
half4 _ColorMarkingsOutline;

half _AlbedoOverlayIntensity;
half _AlbedoOcclusionIntensity;
half _MarkingsShadowIntensity;

half _OverallAlpha;
half _AlbedoAlpha;
half _NormalAlpha;
half _SpecularAlpha;
half _SmoothnessAlpha;
half _OcclusionAlpha;

float _DecalCutoffDistance;

#include "Cginc/HardsurfacePartFunctions.cginc"
#include "MechDeferredDecal_UnityStandardCore.cginc"

inline void GetAlpha
(
    float2 uv,
    out half alphaNormalMultiplied,
    out half alphaAlbedoMultiplied,
    out half alphaAlbedo,
    out half alphaOcclusion,
    out half alphaSpecular,
    out half alphaSmoothness,
    out half alphaNormal
)
{
    half4 mainTex = tex2D (_MainTex, uv);
    half alphaSampleOcclusion = pow (tex2D (_OcclusionMap, uv).r, 4);
    half alphaSampleNormal = mainTex.a;
    half alphaSampleAlbedo = mainTex.b;

    half albedoMaskWithOcclusion = saturate (alphaSampleAlbedo + lerp (0, 1 - alphaSampleOcclusion, _AlbedoOcclusionIntensity));
    half overrideMask = mainTex.g;

    half stickerMaskFromNormals = pow (1 - alphaSampleNormal, 8);
    half stickerWithShadow = saturate (mainTex.r * 2);
    half stickerShadowSubtraction = saturate (stickerWithShadow - mainTex.b) * stickerMaskFromNormals * (1 - _ColorMarkingsOutline.a);

    half aoMask = 1 - saturate (1 - pow (1 - mainTex.r, 4)) * alphaSampleOcclusion;
    half nonAdjustableAreaMask = saturate (overrideMask + stickerMaskFromNormals + aoMask);

    alphaNormalMultiplied = alphaSampleNormal * _OverallAlpha;
    alphaAlbedoMultiplied = saturate (albedoMaskWithOcclusion + overrideMask + stickerWithShadow - stickerShadowSubtraction) * _OverallAlpha;
    alphaAlbedoMultiplied *= lerp (_AlbedoOverlayIntensity, 1, nonAdjustableAreaMask);

    alphaAlbedo = alphaAlbedoMultiplied * _AlbedoAlpha;
    alphaOcclusion = alphaAlbedoMultiplied * _OcclusionAlpha;
    alphaSpecular = alphaNormalMultiplied * _SpecularAlpha;
    alphaSmoothness = alphaAlbedoMultiplied * _SmoothnessAlpha;
    alphaNormal = alphaNormalMultiplied * _NormalAlpha;
}

void fragDeferredDull
(
	VertexOutputDeferred i,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3 // RT3: emission (rgb), unused- (a)

    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	, out half4 outShadowMask : SV_Target4 // RT4: shadowmask (rgba)
    #endif
)
{
	FRAGMENT_SETUP(s)

    half alphaNormalMultiplied = 0;
    half alphaAlbedoMultiplied = 0;
    half alphaAlbedo = 0;
    half alphaOcclusion = 0;
    half alphaSpecular = 0;
    half alphaSmoothness = 0;
    half alphaNormal = 0;
    GetAlpha (i.tex.xy, alphaNormalMultiplied, alphaAlbedoMultiplied, alphaAlbedo, alphaOcclusion, alphaSpecular, alphaSmoothness, alphaNormal);
    alphaAlbedo = 1 - alphaAlbedo;
    alphaOcclusion = 1 - alphaOcclusion;
    alphaSpecular = 1 - alphaSpecular;
    alphaSmoothness = 1 - alphaSmoothness;
    alphaNormal = 1 - alphaNormal;

	outGBuffer0 = half4 (alphaAlbedo, alphaAlbedo, alphaAlbedo, alphaOcclusion);
	outGBuffer1 = half4 (alphaSpecular, alphaSpecular, alphaSpecular, alphaSmoothness);
	outGBuffer2 = half4 (alphaNormal, alphaNormal, alphaNormal, 1 - alphaNormalMultiplied);
	outEmission = half4 (alphaAlbedo, alphaAlbedo, alphaAlbedo, alphaAlbedo);

    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	outShadowMask = half4(1 - alphaNormalMultiplied, 1 - alphaNormalMultiplied, 1 - alphaNormalMultiplied, 1 - alphaNormalMultiplied);
    #endif
}

void fragDeferredPremultAlpha
(
    VertexOutputDeferred i,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3 // RT3: emission (rgb), unused (a)

    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4 // RT4: shadowmask (rgba)
    #endif
)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    FRAGMENT_SETUP(s)

    // no analytic lights in this pass
    UnityLight dummyLight = DummyLight ();
    half atten = 1;

    // only GI
    half occlusion = Occlusion(i.tex.xy);
    #if UNITY_ENABLE_REFLECTION_BUFFERS
        bool sampleReflectionsInDeferred = false;
    #else
        bool sampleReflectionsInDeferred = true;
    #endif

    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);
    half3 emissiveColor = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;

    #ifdef _EMISSION
        emissiveColor += Emission (i.tex.xy);
    #endif

    #ifndef UNITY_HDR_ON
        emissiveColor.rgb = exp2(-emissiveColor.rgb);
    #endif


    UnityStandardData data;
	data.diffuseColor = s.diffColor;
	data.occlusion = occlusion;
	data.specularColor = s.specColor;
	data.smoothness = s.smoothness;
	data.normalWorld = s.normalWorld;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    half alphaNormalMultiplied = 1;
    half alphaAlbedoMultiplied = 1;
    half alphaAlbedo = 1;
    half alphaOcclusion = 1;
    half alphaSpecular = 1;
    half alphaSmoothness = 1;
    half alphaNormal = 1;
    GetAlpha (i.tex.xy, alphaNormalMultiplied, alphaAlbedoMultiplied, alphaAlbedo, alphaOcclusion, alphaSpecular, alphaSmoothness, alphaNormal);

    outGBuffer0 = outGBuffer0 * half4(alphaAlbedo, alphaAlbedo, alphaAlbedo, alphaOcclusion);
    outGBuffer1 = outGBuffer1 * half4(alphaSpecular, alphaSpecular, alphaSpecular, alphaSmoothness);
    outGBuffer2 = outGBuffer2 * half4(alphaNormal, alphaNormal, alphaNormal, alphaNormalMultiplied);
    outEmission = half4(emissiveColor, 1) * half4(alphaAlbedo, alphaAlbedo, alphaAlbedo, alphaOcclusion);

    // Baked direct lighting occlusion if any
    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i)) * alphaNormalMultiplied;
    #endif
}