Shader "Hardsurface/Environment/Terrain (damageable)"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _NormalIntensity ("Normal intensity", Range (0,1)) = 1.0
        _GlossinessMain ("Smoothness", Range (0,1)) = 0.0
        _BorderFactorA ("Border factor A", Float) = 15
        _BorderFactorB ("Border factor B", Float) = -0.8
        _BorderFactors ("Border factors", Vector) = (15, 15, -0.8, -0.8)
        _TexScaleSediment ("Texture scale (sediment)", Float) = 1
        _BrightnessMultiplier ("Brightness multiplier", Range (1, 1.3)) = 1

        _BorderMaskTex ("BorderMask", 2D) = "white" {}
        _BorderMaskScale ("Texture Scale", Float) = 1

        [Space (12)]
        _DistBlendMin ("Distance Blend Begin", Float) = 0
        _DistBlendMax ("Distance Blend Max", Float) = 100

        [Space (12)]
        _MultiplyAlbedoByHeight ("Multiply albedo by height", Range (0, 1)) = 1
        _CurvatureSettings ("Curvature power/blend (ridge/cavity)", Vector) = (1, 1, 1, 1)

        [Space (12)]
        _Scale ("Scale", Vector) = (1, 1, 1, 1)
        _StructureColor ("Structure color", Color) = (0, 0, 0, 1)
        _DestructionAnimation ("Destruction animation", Range (0, 1)) = 0
        _DamageTop ("Damage anim. (0-3)", Vector) = (1, 1, 1, 1)
        _DamageBottom ("Damage anim. (4-7)", Vector) = (1, 1, 1, 1)
        _IntegrityTop ("Integrities (0-3)", Vector) = (1, 1, 1, 1)
        _IntegrityBottom ("Integrities (4-7)", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry+1"
        }
        Cull Off
        ZTest Less
        LOD 200


        CGPROGRAM
        #pragma surface surf Standard vertex:SharedVertexFunction exclude_path:forward exclude_path:prepass finalgbuffer:ColorFunctionSliceShading
        #pragma target 5.0
        #pragma only_renderers d3d11 d3d11_9x vulkan
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Environment_Shared.cginc"
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        float4 _Color;
        sampler2D _BorderMaskTex;
        fixed4 _TintSide;
        half _NormalIntensity;
        half _GlossinessMain;
        float _BorderFactorA;
        float _BorderFactorB;
        float _BrightnessMultiplier;
        float4 _BorderFactors;

        float _BorderMaskScale;
        float _DistBlendMin;
        float _DistBlendMax;
        half4 _CurvatureSettings;

        uniform float4 _GlobalHeightGradientData;
        uniform sampler2D _GlobalRampTerraceTexture;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            ApplySliceCutoff (IN);

            float2 origin = float2 (150, 150);
            float distanceFromCenter = distance (origin, IN.worldPos1.xz);
            float distanceInterpolant = saturate ((distanceFromCenter - _DistBlendMin) / max (1, _DistBlendMax - _DistBlendMin));

            if (distanceInterpolant > 0.99)
            {
                float2 distanceUV = float2 (IN.worldPos1.x, -IN.worldPos1.z) * 0.001;
                float4 distanceMap = tex2D (_CombatTerrainTexDistant, distanceUV);

                o.Normal = float3(0,0,1);

                o.Albedo = distanceMap.xyz;
                o.Metallic = 0;
                o.Smoothness = 0;
                o.Emission = 0;
                o.Occlusion = 1;
                o.Alpha = 1 - IN.damageIntegrityCriticality.x;
            }
            else
            {
                float borderMask = tex2D (_BorderMaskTex, IN.worldPos1.xz / _BorderMaskScale).x;
                float3 albedoFinal = float4 (1, 1, 1, 0);
                fixed3 normalFinal = float3 (0, 0, 1);

                float3 albedoVerticalSedimentXY = tex2D (_CombatTerrainTexSlope, float2 (IN.worldPos1.x * 0.666, IN.worldPos1.y) / _TexScaleSediment).xyz;
                float3 albedoVerticalSedimentZY = tex2D (_CombatTerrainTexSlope, float2 (IN.worldPos1.z * 0.666, IN.worldPos1.y) / _TexScaleSediment).xyz;

                float4 asTerrain = float4 (0, 0, 0, 0.5);
                float4 mseoTerrain = float4 (0, 0, 0, 1);
                float4 normalTerrain = float4 (0, 0, 1, 0.5);
                SampleTerrain (asTerrain, mseoTerrain, normalTerrain, IN.worldPos1);

                fixed4 albedoHorizontal = asTerrain;
                float3 normalFlat = normalize(float3(IN.worldNormal1.x, 0.01f, IN.worldNormal1.z));
                float3 albedoVerticalSediment = albedoVerticalSedimentXY * abs(normalFlat.z) + albedoVerticalSedimentZY * abs(normalFlat.x);
                albedoVerticalSediment *= _TintSide;

                float borderMaskInterpolant = pow (abs (borderMask - 0.5) * 2, 4);
                float borderFactorA = lerp (_BorderFactors.x, _BorderFactors.y, borderMaskInterpolant);
                float borderFactorB = lerp (_BorderFactors.z, _BorderFactors.w, borderMaskInterpolant);

                float verticalFactor = saturate (dot (IN.worldNormal1 * borderFactorA, float3 (0, 1, 0)) + borderFactorB * borderFactorA);
                verticalFactor *= lerp (normalTerrain.a, 1, pow (verticalFactor, 4));

                albedoFinal = lerp (albedoVerticalSediment, albedoHorizontal, verticalFactor);
                half smoothnessFinal = lerp (0, mseoTerrain.y, verticalFactor);

                fixed3 normalHorizontal = float3 (normalTerrain.x * 2 - 1, normalTerrain.y * 2 - 1, normalTerrain.z * 2 - 1);
                normalHorizontal = normalize (normalHorizontal);
                normalFinal = lerp (float3 (0, 0, 1), normalHorizontal, verticalFactor);

                normalFinal = normalize(normalFinal);
                float backsideFactor = GetBacksideFactor(IN.viewDir);
                normalFinal = lerp(normalFinal, -normalFinal, backsideFactor);

                float occlusionFinal = GetOcclusion(IN.worldPos1.y, IN.worldNormal1.y, 1);
                float4 detail = GetDetailSample(IN.worldPos1, IN.worldNormal1);
                float integrityMultiplier = GetIntegrityMultiplier(IN.damageIntegrityCriticality.y, detail);

                albedoFinal *= _Color;
                albedoFinal *= integrityMultiplier;
                smoothnessFinal *= integrityMultiplier;

                // Dot based brightening
                float verticalFactor2 = saturate(dot (IN.worldNormal1, float3 (0, 1, 0)));
                verticalFactor2 = pow (verticalFactor2, 4);
                verticalFactor2 = saturate ((clamp (verticalFactor2, 0.8, 1) - 0.8) * 5);
                float verticalFactor3 = saturate (verticalFactor2 + (1 - verticalFactor));
                albedoFinal.xyz = saturate (lerp (albedoFinal.xyz * 1.5, albedoFinal.xyz, verticalFactor3));

                float verticalFactorSnow = saturate (dot (IN.worldNormal1, float3 (0, 1, 0)));
                verticalFactorSnow = pow (verticalFactorSnow, 2);
                verticalFactorSnow *= verticalFactor;

                float metalnessFinal = 0;

                _WeatherMultiplier = 1.0f;
                float weatherOcclusionMask = 1.0f;
                ApplyWeather (_WeatherMultiplier, albedoFinal.xyz, smoothnessFinal, metalnessFinal, normalFinal, IN.worldPos1, 1, verticalFactor2, verticalFactorSnow, weatherOcclusionMask);

                float curvaturePowerRidge = _CurvatureSettings.x;
                float curvatureBlendRidge = _CurvatureSettings.y;
                float curvaturePowerCavity = _CurvatureSettings.z;
                float curvatureBlendCavity = _CurvatureSettings.w;
                float cv = IN.color.a * 2 - 1; // 0, 0.25, 0.5, 0.75, 1 -> 0, 0.5, 1, 1.5, 2 -> -1, -0.5, 0, 0.5, 1

                if (cv > 0)
                {
                    cv = pow (cv, curvaturePowerRidge); //   0,  0.5,  1  ->   0,  0.25, 1
                    // cv = (cv + 1) * 0.5; //   0,  0.25, 1  ->   1,  1.25, 2  ->  0.5, 0.625, 1

                    float factor = detail.w * cv;
                    factor = 1 - factor;
                    factor = pow (factor, 8);
                    factor = 1 - factor;
                    factor *= borderMask;

                    albedoFinal.xyz = saturate (albedoFinal.xyz + albedoFinal.xyz * factor * curvatureBlendRidge);
                }
                else
                {
                    cv = -cv; //  -1, -0.5,  0  ->   1,  0.5,  0
                    cv = 1 - cv;
                    cv = pow(cv, curvaturePowerCavity); //   1,  0.5,  0  ->   1,  0.25, 0
                    cv = 1 - cv;
                    cv = -cv; //   1,  0.25, 0  ->  -1, -0.25, 0
                    cv = (cv + 1); //  -1, -0.25, 0  ->   0,  0.25, 1

                    albedoFinal.xyz = lerp(albedoFinal.xyz, saturate(albedoFinal.xyz * lerp (cv, 1, borderMask)), curvatureBlendCavity);
                }

                if (distanceInterpolant > 0.01)
                {
                    float2 distanceUV = float2 (IN.worldPos1.x, -IN.worldPos1.z) * 0.001;
                    float4 distanceMap = tex2D (_CombatTerrainTexDistant, distanceUV);

                    albedoFinal.xyz = lerp (albedoFinal.xyz, distanceMap.xyz, distanceInterpolant * lerp (borderMask, 1, distanceInterpolant));
                    metalnessFinal = lerp (metalnessFinal, 0, distanceInterpolant);
                    smoothnessFinal = lerp (smoothnessFinal, 0, distanceInterpolant);
                    occlusionFinal = lerp (occlusionFinal, 1, distanceInterpolant);
                }

                float3 emissionFinal = float3 (0, 0, 0);
                ApplyIsolines (albedoFinal, emissionFinal, IN.worldPos1, IN.worldNormal1);

                o.Albedo = albedoFinal;
                o.Metallic = metalnessFinal;
                o.Smoothness = smoothnessFinal;
                o.Emission = emissionFinal;
                o.Occlusion = occlusionFinal;
                o.Normal = normalFinal;
                o.Alpha = 1 - IN.damageIntegrityCriticality.x;
            }

        }
        ENDCG
    }
    Fallback "Hardsurface/Environment/Terrain (Shadow)"
}
