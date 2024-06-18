// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

//---------------------------------------
half4       _Color;
half        _Cutoff;

sampler2D   _MainTex;
float4      _MainTex_ST;

sampler2D   _BumpMap;
half        _BumpScale;

sampler2D   _SpecGlossMap;
sampler2D   _MetallicGlossMap;
half        _Metallic;
float       _Glossiness;
float       _GlossMapScale;

sampler2D   _OcclusionMap;
half        _OcclusionStrength;

sampler2D   _ParallaxMap;
half        _Parallax;
half        _ParallaxShift;

half4       _EmissionColor;
sampler2D   _EmissionMap;

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
    #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
        float2 uv2      : TEXCOORD2;
    #endif
    #ifdef _TANGENT_TO_WORLD
        half4 tangent   : TANGENT;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords(VertexInput v)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
    texcoord.zw = v.uv1;
    return texcoord;
}

half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return tex2D(_OcclusionMap, uv).g;
#else
    half occ = tex2D(_OcclusionMap, uv).g;
    return LerpOneTo (occ, _OcclusionStrength);
#endif
}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        sg.rgb = tex2D(_SpecGlossMap, uv).rgb;
        sg.a = tex2D(_MainTex, uv).a;
    #else
        sg = tex2D(_SpecGlossMap, uv);
    #endif
    sg.a *= _GlossMapScale;
#else
    sg.rgb = _SpecColor.rgb;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        sg.a = tex2D(_MainTex, uv).a * _GlossMapScale;
    #else
        sg.a = _Glossiness;
    #endif
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.r = tex2D(_MetallicGlossMap, uv).r;
        mg.g = tex2D(_MainTex, uv).a;
    #else
        mg = tex2D(_MetallicGlossMap, uv).ra;
    #endif
    mg.g *= _GlossMapScale;
#else
    mg.r = _Metallic;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.g = tex2D(_MainTex, uv).a * _GlossMapScale;
    #else
        mg.g = _Glossiness;
    #endif
#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
#ifdef _METALLICGLOSSMAP
    mg.r = tex2D(_MetallicGlossMap, uv).r;
#else
    mg.r = _Metallic;
#endif

#ifdef _SPECGLOSSMAP
    mg.g = 1.0f - tex2D(_SpecGlossMap, uv).r;
#else
    mg.g = 1.0f - _Glossiness;
#endif
    return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
    return 0;
#else
    return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords.xy), _BumpScale);
    return normalTangent;
}
#endif

float4 Parallax (float4 texcoords, half3 viewDir, half3 eyeVec, half3 normalWorld)
{
    #if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
        // Disable parallax on pre-SM3.0 shader target models
        return texcoords;
    #else

        // half h = tex2D (_ParallaxMap, texcoords.xy).g;
        // float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
        // return float4(texcoords.xy + offset, texcoords.zw + offset);

        half3 directionalInput = viewDir;
        float _MinParallaxSamples = 25;
        float _MaxParallaxSamples = 75;

        float fParallaxLimit = -length (directionalInput.xy) / directionalInput.z;
        fParallaxLimit *= _Parallax;

        float2 vOffsetDir = normalize (directionalInput.xy);
        float2 vMaxOffset = vOffsetDir * fParallaxLimit;

        int nNumSamples = (int)lerp (_MaxParallaxSamples, _MinParallaxSamples, dot (normalize (directionalInput), normalize (normalWorld)));
        float fStepSize = 1.0 / (float)nNumSamples;

        float2 dx = ddx (texcoords.xy);
        float2 dy = ddy (texcoords.xy);

        float fCurrRayHeight = 1;
        float2 vCurrOffset = float2(0, 0);
        float2 vLastOffset = float2(0, 0);
        float2 vCurrOffsetShifted = float2(0, 0);

        vCurrOffset -= fStepSize * vMaxOffset * (float)nNumSamples * _ParallaxShift;

        float fLastSampledHeight = 1;
        float fCurrSampledHeight = 1;

        int nCurrSample = 0;
        while (nCurrSample < nNumSamples)
        {
            fCurrSampledHeight = tex2Dgrad (_ParallaxMap, texcoords.xy + vCurrOffset, dx, dy).x;
            if (fCurrSampledHeight > fCurrRayHeight)
            {
                float delta1 = fCurrSampledHeight - fCurrRayHeight;
                float delta2 = (fCurrRayHeight + fStepSize) - fLastSampledHeight;
                float ratio = delta1 / (delta1 + delta2);

                vCurrOffset = lerp (vCurrOffset, vLastOffset, ratio);
                vCurrOffsetShifted = vCurrOffset - vMaxOffset;

                fLastSampledHeight = lerp (fCurrSampledHeight, fLastSampledHeight, ratio);

                nCurrSample = nNumSamples + 1;
            }
            else
            {
                nCurrSample++;

                fCurrRayHeight -= fStepSize;

                vLastOffset = vCurrOffset;
                vCurrOffset += fStepSize * vMaxOffset;
                vCurrOffsetShifted = vCurrOffset - vMaxOffset;

                fLastSampledHeight = fCurrSampledHeight;
            }
        }

        float4 texcoordsPOM = float4(texcoords.xy + vCurrOffset, texcoords.zw + vCurrOffset);

        // float2 offsetUniform = 0;
        // texcoordsPOM = float4(texcoordsPOM.xy + offsetUniform, texcoordsPOM.zw + offsetUniform);

        return texcoordsPOM;
    #endif
}

#endif // UNITY_STANDARD_INPUT_INCLUDED
