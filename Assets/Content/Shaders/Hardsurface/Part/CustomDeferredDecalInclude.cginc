half _CompositeMode;

#include "CustomDeferredDecal_UnityStandardCore.cginc"

half _Overall_Alpha;
half _Diffuse_Alpha;
half _Normal_Alpha;
half _Specular_Alpha;
half _Smoothness_Alpha;
half _Occlusion_Alpha;

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

    half4 mainTex = tex2D (_MainTex, i.tex.xy);
    half alphaSamplePrimary = mainTex.a; // normal mask in composite
    half alphaSampleSecondary = lerp (mainTex.a, mainTex.b, _CompositeMode); // albedo mask in composite

    half alphaPrimaryMultiplied = alphaSamplePrimary * _Overall_Alpha;
    half alphaSecondaryMultiplied = alphaSampleSecondary * _Overall_Alpha;

    half alphaAlbedo = alphaSecondaryMultiplied * _Diffuse_Alpha;
    half alphaOcclusion = alphaSecondaryMultiplied * _Occlusion_Alpha;
    half alphaSpecular = alphaPrimaryMultiplied * _Specular_Alpha;
    half alphaSmoothness = alphaSecondaryMultiplied * _Smoothness_Alpha;
    half alphaNormal = alphaPrimaryMultiplied * _Normal_Alpha;

	outGBuffer0 = half4(1 - alphaAlbedo, 1 - alphaAlbedo, 1 - alphaAlbedo, 1 - alphaOcclusion);
	outGBuffer1 = half4(1 - alphaSpecular, 1 - alphaSpecular, 1 - alphaSpecular, 1 - alphaSmoothness);
	outGBuffer2 = half4(1 - alphaNormal, 1 - alphaNormal, 1 - alphaNormal, 1 - alphaPrimaryMultiplied);
	outEmission = half4(1 - alphaAlbedo, 1 - alphaAlbedo, 1 - alphaAlbedo, 1 - alphaAlbedo);

    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	    outShadowMask = half4(1 - alphaPrimaryMultiplied, 1 - alphaPrimaryMultiplied, 1 - alphaPrimaryMultiplied, 1 - alphaPrimaryMultiplied);
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
    #if (SHADER_TARGET < 30)
        outGBuffer0 = 1;
        outGBuffer1 = 1;
        outGBuffer2 = 0;
        outEmission = 0;
        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
            outShadowMask = 1;
        #endif
        return;
    #endif

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

    half4 mainTex = tex2D (_MainTex, i.tex.xy);
    half alphaSamplePrimary = mainTex.a;
    half alphaSampleSecondary = lerp (mainTex.a, mainTex.b, _CompositeMode);

    half alphaPrimaryMultiplied = alphaSamplePrimary * _Overall_Alpha;
    half alphaSecondaryMultiplied = alphaSampleSecondary * _Overall_Alpha;

    half alphaAlbedo = alphaSecondaryMultiplied * _Diffuse_Alpha;
    half alphaOcclusion = alphaSecondaryMultiplied * _Occlusion_Alpha;
    half alphaSpecular = alphaPrimaryMultiplied * _Specular_Alpha;
    half alphaSmoothness = alphaSecondaryMultiplied * _Smoothness_Alpha;
    half alphaNormal = alphaPrimaryMultiplied * _Normal_Alpha;

    outGBuffer0 = outGBuffer0 * half4(alphaAlbedo, alphaAlbedo, alphaAlbedo, alphaOcclusion);
    outGBuffer1 = outGBuffer1 * half4(alphaSpecular, alphaSpecular, alphaSpecular, alphaSmoothness);
    outGBuffer2 = outGBuffer2 * half4(alphaNormal, alphaNormal, alphaNormal, alphaPrimaryMultiplied);
    outEmission = half4(emissiveColor, 1) * half4(alphaAlbedo, alphaAlbedo, alphaAlbedo, alphaOcclusion);

    // Baked direct lighting occlusion if any
    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i)) * alphaPrimaryMultiplied;
    #endif
}