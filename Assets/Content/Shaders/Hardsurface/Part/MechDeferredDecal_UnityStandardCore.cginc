// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_CORE_INCLUDED
#define UNITY_STANDARD_CORE_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "MechDeferredDecal_UnityStandardInput.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardBRDF.cginc"
#include "AutoLight.cginc"
#include "Cginc/MechPartUtility.cginc"








//-------------------------------------------------------------------------------------
// counterpart for NormalizePerPixelNormal
// skips normalization per-vertex and expects normalization to happen per-pixel

half3 NormalizePerVertexNormal (float3 n) // takes float to avoid overflow
{
    return n; // will normalize per-pixel instead
}

float3 NormalizePerPixelNormal (float3 n)
{
    return normalize((float3)n); // takes float to avoid overflow
}




//-------------------------------------------------------------------------------------

UnityLight DummyLight ()
{
    UnityLight l;
    l.color = 0;
    l.dir = half3 (0,1,0);
    return l;
}

UnityIndirect ZeroIndirect ()
{
    UnityIndirect ind;
    ind.diffuse = 0;
    ind.specular = 0;
    return ind;
}




//-------------------------------------------------------------------------------------
// Common fragment setup

// deprecated
half3 WorldNormal(half4 tan2world[3])
{
    return normalize(tan2world[2].xyz);
}

// deprecated
#ifdef _TANGENT_TO_WORLD
half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
{
    half3 t = tan2world[0].xyz;
    half3 b = tan2world[1].xyz;
    half3 n = tan2world[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        n = NormalizePerPixelNormal(n);

        // ortho-normalize Tangent
        t = normalize (t - n * dot(t, n));

        // recalculate Binormal
        half3 newB = cross(n, t);
        b = newB * sign (dot (newB, b));
    #endif

    return half3x3(t, b, n);
}
#else
half3x3 ExtractTangentToWorldPerPixel(half4 tan2world[3])
{
    return half3x3(0,0,0,0,0,0,0,0,0);
}
#endif




float3 PerPixelWorldNormalEarly (float4 tangentToWorld[3])
{
    float3 normalWorld = normalize (tangentToWorld[2].xyz);
    return normalWorld;
}

float3 PerPixelWorldNormal(float4 i_tex, float4 tangentToWorld[3])
{
    half3 tangent = tangentToWorld[0].xyz;
    half3 binormal = tangentToWorld[1].xyz;
    half3 normal = tangentToWorld[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        normal = NormalizePerPixelNormal(normal);
        tangent = normalize (tangent - normal * dot(tangent, normal));
        half3 newB = cross(normal, tangent);
        binormal = newB * sign (dot (newB, binormal));
    #endif

    half3 normalTangent = NormalInTangentSpace(i_tex);
    float3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
    return normalWorld;
}




#define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w))

#if UNITY_REQUIRE_FRAG_WORLDPOS
    #if UNITY_PACK_WORLDPOS_WITH_TANGENT
        #define IN_WORLDPOS(i) half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w)
    #else
        #define IN_WORLDPOS(i) i.posWorld
    #endif
#else
    #define IN_WORLDPOS(i) half3(0,0,0)
#endif




#define FRAGMENT_SETUP(x) FragmentCommonData x = \
    FragmentSetup(i.tex, i.eyeVec.xyz, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i), i.color, i.localPos, i.localNormal);

struct FragmentCommonData
{
    half3 diffColor, specColor;
    // Note: smoothness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
    // Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
    half oneMinusReflectivity, smoothness;
    float3 normalWorld;
    float3 eyeVec;
    half alpha;
    float3 posWorld;
    half4 color;
    half3 emission;
};




inline FragmentCommonData MetallicSetup (float4 i_tex, float4 i_color, float3 i_localPos, float3 i_localNormal)
{
    half metalnessClean = _Metallic;
    half smoothnessClean = _Glossiness; // this is 1 minus the square root of real roughness m.
    half3 emissionClean = 0;




    half4 mainTex = tex2D (_MainTex, i_tex.xy);
    half occlusionInfluence = lerp (1, tex2D (_OcclusionMap, i_tex.xy).r, _AlbedoOcclusionIntensity);
    occlusionInfluence = pow (occlusionInfluence, 4);

    half stickerMask = pow (1 - mainTex.a, 8);

    half stickerMaskFromNormals = pow (1 - mainTex.a, 8);
    half stickerWithShadow = saturate (mainTex.r * 2);
    half stickerShadowSubtraction = saturate (stickerWithShadow - mainTex.b) * stickerMaskFromNormals;
    half3 stickerColor = lerp (_ColorMarkingsCore.xyz, _ColorMarkingsOutline.xyz, stickerShadowSubtraction);

    float3 albedoBase = mainTex.rrr;
    float albedoBaseAlpha = mainTex.b * _AlbedoOverlayIntensity;
    float3 albedoCustomized = SelectColorFromVC (i_color, _ColorBackground, _ColorPrimary, _ColorSecondary, _ColorTertiary);
    albedoCustomized = OverlayWithAlpha (albedoBase, mainTex.b * _AlbedoOverlayIntensity, albedoCustomized);
    float3 albedoClean = lerp (albedoBase * lerp (1, stickerColor, stickerMask), albedoCustomized, mainTex.g);
    albedoClean = albedoClean * occlusionInfluence;
    
    #if PART_USE_TRIPLANAR

        float3 triblend = saturate (pow (i_localNormal, 4));
        triblend /= max (dot (triblend, half3(1, 1, 1)), 0.0001);

        float damageScale = 0.25; // 1 / (_GlobalUnitDamageScale * 0.5);
        float4 damageSampleX = tex2D (_GlobalUnitDamageTexNew, i_localPos.yz * damageScale + _GlobalUnitDamageOffset.yz);
        float4 damageSampleY = tex2D (_GlobalUnitDamageTexNew, i_localPos.xz * damageScale + _GlobalUnitDamageOffset.xz);
        float4 damageSampleZ = tex2D (_GlobalUnitDamageTexNew, i_localPos.xy * damageScale + _GlobalUnitDamageOffset.xy);
        float4 damageSample = damageSampleX.xyzw * triblend.x + damageSampleY.xyzw * triblend.y + damageSampleZ.xyzw * triblend.z;
        float4 damageSampleSecondary = tex2D (_GlobalUnitDamageTexNewSecondary, i_tex * damageScale + _GlobalUnitDamageOffset.xy);

    #else

        float damageScale = 1; // 1 / _GlobalUnitDamageScale;
        float4 damageSample = tex2D (_GlobalUnitDamageTexNew, i_tex.xy * damageScale + _GlobalUnitDamageOffset.xy);
        float4 damageSampleSecondary = tex2D (_GlobalUnitDamageTexNewSecondary, i_tex * damageScale + _GlobalUnitDamageOffset.xy);

    #endif
    
    #if PART_USE_ARRAYS

        float arrayIndex = lerp (i_tex.z, _ArrayOverrideIndex, _ArrayOverrideMode) + 0.5;
        float4 damageInput = _ArrayForDamage[arrayIndex];

    #else
    
        float4 damageInput = _Damage;

    #endif
    
    float3 albedoAfterDamage = albedoClean;
    float smoothnessAfterDamage = smoothnessClean;
    float metalnessAfterDamage = metalnessClean;
    float3 emissionAfterDamage = emissionClean;
    float3 normalAfterDamage = float3(0, 0, 1);
    
    ApplyDamage
    (
        damageInput,
        damageSample,
        damageSampleSecondary,
        0.0f,
        0.0f,
        albedoClean,
        1.0f,
        0.5f,
        smoothnessClean,
        metalnessClean,
        emissionClean,
        float3(0, 0, 1),
        1.0f,
        1.0f,
        albedoAfterDamage,
        smoothnessAfterDamage,
        metalnessAfterDamage,
        emissionAfterDamage,
        normalAfterDamage
    );
    
    // clip (1 - damageInput.y * 2);

    // Uncomment to debug array index in UV2
    // albedoAfterDamage += lerp (float3 (1, 0, 0), float3 (0, 0, 1), i_tex.z * 0.05);
    
    #if PART_USE_ARRAYS

    // albedoAfterDamage = lerp (albedoAfterDamage, float3 (1, 1, 1), abs (i_tex.z - 3) < 0.1 ? 1 : 0);
    // albedoAfterDamage = damageInput.x;

    #endif


    
    
    
    
     
    half oneMinusReflectivity;
    half3 specColor;
    half3 diffColor = DiffuseAndSpecularFromMetallic (albedoAfterDamage, metalnessAfterDamage, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothnessAfterDamage;
    o.emission = emissionAfterDamage;
    return o;
}




// parallax transformed texcoord is used to sample occlusion
inline FragmentCommonData FragmentSetup 
(
    inout float4 i_tex, 
    float3 i_eyeVec, 
    half3 i_viewDirForParallax, 
    float4 tangentToWorld[3], 
    float3 i_posWorld, 
    half4 i_color, 
    float3 i_localPos, 
    float3 i_localNormal
)
{
    i_tex = Parallax(i_tex, i_viewDirForParallax, i_eyeVec, PerPixelWorldNormalEarly (tangentToWorld));

    half4 mainTex = tex2D (_MainTex, i_tex.xy);
    half alphaCumulative = saturate (mainTex.g + mainTex.b + mainTex.a);

    /*
    #if defined(_ALPHATEST_ON)
        clip (alphaCumulative - 0.00001);
    #endif
    */

    FragmentCommonData o = MetallicSetup (i_tex, i_color, i_localPos, i_localNormal);
    o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
    o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
    o.posWorld = i_posWorld;

    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    o.diffColor = PreMultiplyAlpha (o.diffColor, alphaCumulative, o.oneMinusReflectivity, /*out*/ o.alpha);
    return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
        d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        d.boxMax[0] = unity_SpecCube0_BoxMax;
        d.probePosition[0] = unity_SpecCube0_ProbePosition;
        d.boxMax[1] = unity_SpecCube1_BoxMax;
        d.boxMin[1] = unity_SpecCube1_BoxMin;
        d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    if (reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif
        return UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
    }
    else
    {
        return UnityGlobalIllumination (d, occlusion, s.normalWorld);
    }
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
}






// ------------------------------------------------------------------
//  Deferred pass

struct VertexOutputDeferred
{
    UNITY_POSITION(pos);
    float4 tex                              : TEXCOORD0;
    float3 eyeVec                           : TEXCOORD1;
    float4 tangentToWorldAndPackedData[3]   : TEXCOORD2; // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV               : TEXCOORD5; // SH or Lightmap UVs
    fixed4 color                            : COLOR;

    #if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
        float3 posWorld                     : TEXCOORD6;
    #endif
    
    float3 localPos : TEXCOORD7;
    float3 localNormal : TEXCOORD8;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputDeferred vertDeferred (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputDeferred o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float distanceToCamera = length(ObjSpaceViewDir(float4(0, 0, 0, 1)));

    // Discard all vertices if decals are too far from camera
    if (distanceToCamera > _DecalCutoffDistance)
    {
        // Pass NaN to vertex pos to discard them
        o.pos = asfloat(0x7fc00000);
        o.color = 0.0;
        o.localPos = 0.0;
        o.localNormal = 0.0;
        o.tex = 0.0;
        o.eyeVec = 0.0;
        o.ambientOrLightmapUV = 0.0;
    }
    else
    {
        float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
        #if UNITY_REQUIRE_FRAG_WORLDPOS
            #if UNITY_PACK_WORLDPOS_WITH_TANGENT
                o.tangentToWorldAndPackedData[0].w = posWorld.x;
                o.tangentToWorldAndPackedData[1].w = posWorld.y;
                o.tangentToWorldAndPackedData[2].w = posWorld.z;
            #else
                o.posWorld = posWorld.xyz;
            #endif
        #endif
        o.pos = UnityObjectToClipPos(v.vertex);

        o.tex = TexCoords(v);
        o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
        float3 normalWorld = UnityObjectToWorldNormal(v.normal);
        #ifdef _TANGENT_TO_WORLD
            float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

            float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
            o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
            o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
            o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
        #else
            o.tangentToWorldAndPackedData[0].xyz = 0;
            o.tangentToWorldAndPackedData[1].xyz = 0;
            o.tangentToWorldAndPackedData[2].xyz = normalWorld;
        #endif

        o.ambientOrLightmapUV = 0;
        #ifdef LIGHTMAP_ON
            o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        #elif UNITY_SHOULD_SAMPLE_SH
            o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
        #endif
        #ifdef DYNAMICLIGHTMAP_ON
            o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
        #endif

        #ifdef _PARALLAXMAP
            TANGENT_SPACE_ROTATION;
            half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
            o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
            o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
            o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
        #endif

        o.color = v.color;
        o.localPos = v.vertex.xyz;
        o.localNormal = v.normal.xyz;
    }

    return o;
}

void fragDeferred 
(
    VertexOutputDeferred i,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        ,out half4 outShadowMask : SV_Target4
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
    UNITY_SETUP_INSTANCE_ID(i);

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

    
    emissiveColor += s.emission;

    #ifndef UNITY_HDR_ON
        emissiveColor.rgb = exp2(-emissiveColor.rgb);
    #endif

    UnityStandardData data;
    data.diffuseColor   = s.diffColor;
    data.occlusion      = occlusion;
    data.specularColor  = s.specColor;
    data.smoothness     = s.smoothness;
    data.normalWorld    = s.normalWorld;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);
    outEmission = half4(emissiveColor, 1);

    #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i));
    #endif
}

#endif // UNITY_STANDARD_CORE_INCLUDED
