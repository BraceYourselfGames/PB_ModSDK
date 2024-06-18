// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space
#define _TANGENT_TO_WORLD 1

//---------------------------------------

sampler2D   _MainTex;
float4      _MainTex_ST;

sampler2D   _NormalTex;
half        _NormalScale;

half        _Metallic;
float       _Glossiness;

sampler2D   _OcclusionMap;
half        _OcclusionStrength;

sampler2D   _HeightTex;
half        _ParallaxScale;
half        _ParallaxShift;

half4       _EmissionColor;
sampler2D   _EmissionMap;

half        _Cutoff;

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
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
    half occ = tex2D(_OcclusionMap, uv).r;
    return LerpOneTo (occ, _OcclusionStrength);
}

half3 Emission(float2 uv)
{
    return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
}

half3 NormalInTangentSpace(float4 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (_NormalTex, texcoords.xy), _NormalScale);
    return normalTangent;
}

float4 Parallax (float4 texcoords, half3 viewDir, half3 eyeVec, half3 normalWorld)
{
    // half h = tex2D (_ParallaxMap, texcoords.xy).g;
    // float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
    // return float4(texcoords.xy + offset, texcoords.zw + offset);

    half3 directionalInput = viewDir;
    float _MinParallaxSamples = 25;
    float _MaxParallaxSamples = 75;

    float fParallaxLimit = -length (directionalInput.xy) / directionalInput.z;
    fParallaxLimit *= _ParallaxScale;

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
        fCurrSampledHeight = tex2Dgrad (_HeightTex, texcoords.xy + vCurrOffset, dx, dy).x;
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

    float4 texcoordsPOM = float4(texcoords.xy + vCurrOffset, texcoords.zw);

    // float2 offsetUniform = 0;
    // texcoordsPOM = float4(texcoordsPOM.xy + offsetUniform, texcoordsPOM.zw + offsetUniform);

    return texcoordsPOM;
}

#endif // UNITY_STANDARD_INPUT_INCLUDED
