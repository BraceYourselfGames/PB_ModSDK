// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hardsurface/Environment/Block (array-based)"
{
    Properties
    {
        _TintColor ("Tint color", Color) = (1, 1, 1, 1)
        _TexArrayAH ("Texture array / AH", 2DArray) = "" {}
        _TexArrayMSEO ("Texture array / MSEO", 2DArray) = "" {}
        _ArrayIndex ("Array index", Range (0,16)) = 6
        _ArrayIndexShift ("Array index shift", Range (-1,1)) = 0
        _ArrayTestMode ("Array test mode", Range (0,1)) = 0
        _ScaleTestToggle ("Scale test", Range (0,1)) = 0
        _ScaleTestValue ("Scale test value", Vector) = (1, 1, 1, 1)
        _PositionTest ("Position test", Range (0,1)) = 0

        _HSBOffsetsPrimary ("HSB color A (RGB), overlay override (A)", Vector) = (0.5, 0.5, 0.5, 1)
        _HSBOffsetsSecondary ("HSB color B (RGB), unused (A)", Vector) = (0.5, 0.5, 0.5, 1)

        _EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
        _EmissionColor ("Emission color", Color) = (0, 0, 0, 1)

        _StructureColor ("Structure color", Color) = (0, 0, 0, 1)
        _DestructionAnimation ("Destruction animation", Range (0, 1)) = 0
        _DamageTop ("Damage anim. (0-3)", Vector) = (1, 1, 1, 1)
        _DamageBottom ("Damage anim. (4-7)", Vector) = (1, 1, 1, 1)
        _IntegrityTop ("Integrities (0-3)", Vector) = (1, 1, 1, 1)
        _IntegrityBottom ("Integrities (4-7)", Vector) = (1, 1, 1, 1)

        _TerrainParams ("Terrain params. (index, -, -, -)", Vector) = (1, 1, 1, 1)
        _MaskTestMin ("Mask test min", Range (0,1)) = 0.45
        _MaskTestMax ("Mask test max", Range (0,1)) = 0.55
    }
    SubShader
    {

        Tags
        {
            "RenderType" = "Opaque"
        }
        Cull Off
        Ztest Less

        CGPROGRAM
        #pragma target 5.0
        #pragma only_renderers d3d11 vulkan
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        //#pragma surface surf Standard fullforwardshadows vertex:SharedVertexFunction addshadow
        #pragma surface surf Standard vertex:SharedVertexFunction exclude_path:forward exclude_path:prepass finalgbuffer:ColorFunctionSliceShading

        #include "UnityCG.cginc"
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Environment_Shared.cginc"

        fixed4 _TintColor;
        float _ArrayIndex;
        float _ArrayIndexShift;
        float _ArrayTestMode;

        fixed4 _EmissionColor;
        float _EmissionIntensity;
        float _MaskTestMin;
        float _MaskTestMax;
        float4 _TerrainParams;

        // Arrays can't be serialized, so you can't declare them in Properties block
        // they are set at runtime
        float4 _PropertyArray[10];
        uniform sampler3D _GlobalEnvironmentAOTex;

        // Texture arrays are declared last (not sure why, they just are)
        UNITY_DECLARE_TEX2DARRAY(_TexArrayAH);
        UNITY_DECLARE_TEX2DARRAY(_TexArrayMSEO);

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float destructionAnimation = 0;

            float4 hsbPrimary = float4(0, 0.5, 0.5, 0);
            float4 hsbSecondary = float4(0, 0.5, 0.5, 0);
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

					HalfVector8 cachedHSB = hsbData[unity_InstanceID];
					hsbSecondary = cachedHSB.UnpackSecondary();
					destructionAnimation = hsbSecondary.w;//cachedHSBSecondary = float4(0,0,0,1);.w; // UNITY_ACCESS_INSTANCED_PROP (_DestructionAnimation_arr, _DestructionAnimation);
					hsbPrimary = cachedHSB.UnpackPrimary();
            #endif

            if (destructionAnimation > 0.99)
            {
                clip(-1);
                // o.Albedo = float3 (1, 0, 0);
            }
            else
            {
                ApplySliceCutoff (IN);

                float arrayIndexFromUV2 = lerp(IN.texcoord_uv2.x, _ArrayIndex, _ArrayTestMode);
                float arrayIndexOffset = arrayIndexFromUV2 + _ArrayIndexShift;
                float3 arrayUVNormal = float3(IN.texcoord_uv1.x, IN.texcoord_uv1.y, arrayIndexOffset);

                // Minimum, medium and maximum smoothness (RGB), world space UV override (A)
                float4 properties = _PropertyArray[arrayIndexFromUV2 + 0.5];

                // Albedo (RGB), bidirectional tinting mask (A)
                fixed4 arraySampleAH = UNITY_SAMPLE_TEX2DARRAY(_TexArrayAH, arrayUVNormal);
                fixed4 arraySampleMSEO = UNITY_SAMPLE_TEX2DARRAY(_TexArrayMSEO, arrayUVNormal);
                float3 normal = float3(0, 0, 1);


                // <0 - no world UV, otherwise it points to array slice to sample
                bool overlayUsedByDefault = properties.w > -0.5;
                bool overlayOverride = hsbPrimary.w < -0.5;
                float terrainFactor = 0;

                if (overlayUsedByDefault || overlayOverride)
                {
                    // We have no need for emission intensity in blended areas, so we reuse the same value for array slice
                    float worldOverlayIndex = overlayOverride ? -(hsbPrimary.w + 1) : properties.w;
                    float worldOverlayIndexOffset = worldOverlayIndex + _ArrayIndexShift;

                    // Tileset textures don't need occlusion so we use W channel to store world mapping mask
                    float worldOverlayMask = arraySampleMSEO.w;
                    float3 arrayUVWorld = float3(IN.worldPos1.x * 0.09765625, IN.worldPos1.z * 0.09765625,
                                                 worldOverlayIndexOffset);

                    arraySampleAH = lerp(
                        UNITY_SAMPLE_TEX2DARRAY(_TexArrayAH, arrayUVWorld), arraySampleAH, worldOverlayMask);
                    // arraySampleAH = lerp (fixed4 (1, 0, 0, 0.5), arraySampleAH, worldOverlayMask);
                    arraySampleMSEO = lerp(
                        UNITY_SAMPLE_TEX2DARRAY(_TexArrayMSEO, arrayUVWorld), arraySampleMSEO, worldOverlayMask);

                    float4 propertiesOverlay = _PropertyArray[worldOverlayIndexOffset];
                    properties = lerp(propertiesOverlay, properties, worldOverlayMask);

                    float terrainIndex = _TerrainParams.x;
                    terrainFactor = worldOverlayIndex > (terrainIndex - 0.1) && worldOverlayIndex < (terrainIndex + 0.1)
                                        ? 1
                                        : 0;
                    terrainFactor = saturate(terrainFactor * (1 - worldOverlayMask));

                    float4 asTerrain = float4(0, 0, 0, 0.5);
                    float4 mseoTerrain = float4(0, 0, 0, 1);
                    float4 normalTerrain = float4(0, 0, 1, 0.5);
                    SampleTerrain(asTerrain, mseoTerrain, normalTerrain, IN.worldPos1);

                    arraySampleAH.rgb = lerp(arraySampleAH.rgb, asTerrain.rgb, terrainFactor);
                    // Ensure terrain will not receive any HSB adjustments
                    arraySampleAH.a = lerp(arraySampleAH.a, 0.5f, terrainFactor);
                    arraySampleMSEO = lerp(arraySampleMSEO, mseoTerrain, terrainFactor);

                    float3 normalTerrainHorizontal = float3(normalTerrain.x * 2 - 1, normalTerrain.y * 2 - 1,
                                                            normalTerrain.z * 2 - 1);
                    normal = lerp(normal, normalTerrainHorizontal, terrainFactor);
                }

                fixed3 albedoBase = arraySampleAH.rgb;
                fixed albedoMask = arraySampleAH.a;
                fixed metalnessFromTexture = arraySampleMSEO.x;
                fixed smoothnessFromTexture = arraySampleMSEO.y;
                fixed emissionFromTexture = arraySampleMSEO.z;
                fixed occlusionFromTexture = 1; // alpha is now used for overlay mapping masks

                // Normal correction necessary due to disabled backface culling
                // float3 n = float3 (0, 0, 1);
                float backsideFactor = GetBacksideFactor(IN.viewDir);
                float3 normalFinal = lerp(normal, -normal, backsideFactor);

                float3 albedoFinal = GetAlbedo(hsbPrimary.xyz, hsbSecondary.xyz, albedoBase, albedoMask,
                                               occlusionFromTexture) * _TintColor;
                float smoothnessFinal = GetSmoothness(smoothnessFromTexture, properties.x, properties.y, properties.z,
                                                      backsideFactor);
                float3 emissionFinal = GetEmission(IN.damageIntegrityCriticality.x, hsbPrimary.w, emissionFromTexture,
                                                   _EmissionColor, _EmissionIntensity);
                float occlusionFinal = GetOcclusion(IN.worldPos1.y, IN.worldNormal1.y, occlusionFromTexture);

                // Dropping everything to black on backfaces
                float backsideZeroMultiplier = 1 - backsideFactor;
                smoothnessFinal *= backsideZeroMultiplier;
                emissionFinal *= backsideZeroMultiplier;

                float4 detail = GetDetailSample(IN.worldPos1, IN.worldNormal1);
                float factor = 1 - IN.damageIntegrityCriticality.x;
                factor = pow(factor, 16);
                factor = lerp(0.66, factor, IN.damageIntegrityCriticality.y);
                float integrityMultiplier = GetIntegrityMultiplier(factor, detail);

                albedoFinal *= integrityMultiplier;
                smoothnessFinal *= integrityMultiplier;

                float emissionMultiplier = saturate(IN.damageIntegrityCriticality.y * 100 - 99);
                emissionFinal *= emissionMultiplier;

                // Finally, time for damage
                float damageBase = saturate(IN.damageIntegrityCriticality.x + destructionAnimation);
                if (damageBase > 0.001)
                {
                    // Extracting noise and using it to set opacity from damage, along with applying contrast to the noise
                    float detailNoise = saturate(
                        (detail.z - 0.5) * lerp(_GlobalEnvironmentDetailContrast, 1,
                                                destructionAnimation) + 0.5 - 0.25);
                    float detailStructure = detail.y / 2;

                    // Noise is a rich set of uniformly brigtness-distributed values which damage pushes below zero to create the cuts
                    float subtractionTestNoise = detailNoise - damageBase * 1.25;
                    float subtractionTestFull = subtractionTestNoise + detailStructure * 0.25;
                    clip(subtractionTestFull);

                    // Next we need to get a mask of an area where surface is already peeled but structure is not yet peeled
                    float structureMask = lerp(0, 1, saturate(-subtractionTestNoise * 100));
                    // float structureMask = abs (subtractionTestNoise - subtractionTestFull);
                    float structureMaskShadow = lerp(0, 1, saturate(-subtractionTestNoise * 6));

                    // Next we reuse the noise subtraction test to draw ramps
                    if (subtractionTestNoise < _GlobalEnvironmentRampScale)
                    {
                        // Traditional ramp mapping - use UV with zero Y and X based on our subtraction test
                        float4 ramp = tex2D(_GlobalRampBurnTex,
                                            float2(subtractionTestNoise * (1 / _GlobalEnvironmentRampScale * 2 + 0.5),
                                                   0));

                        // Necessary to smoothly kill the ramp influence towards clean areas approaching edges of the blocks, without killing ramp intensity too much
                        float rampAlphaMultiplier = saturate((1 - subtractionTestNoise) * damageBase * 5);
                        // IN.damage * 5;
                        ramp.w *= rampAlphaMultiplier;

                        // Time to add this
                        albedoFinal = lerp(albedoFinal, albedoFinal * ramp.x, ramp.w * _GlobalEnvironmentRampInfluence);
                        albedoFinal = lerp(albedoFinal, _StructureColor * structureMaskShadow, structureMask);
                        smoothnessFinal = lerp(smoothnessFinal, smoothnessFinal * ramp.x, ramp.w);
                        smoothnessFinal = lerp(smoothnessFinal, 0, structureMask);
                    }
                }

                // Rain
                // metalnessFromTexture = lerp(metalnessFromTexture, saturate(metalnessFromTexture * 3), _RainIntensity);
                // smoothnessFinal = saturate(smoothnessFinal + ((smoothnessFinal + 0.8) * _RainIntensity * 0.75));

                // Rain
                /*
                float verticalFactor = saturate (dot (IN.worldNormal1, float3 (0, 1, 0)));

                float distanceFadeRain = 1 - saturate ((distance (_WorldSpaceCameraPos, IN.worldPos1) / 200) - 0.1);
                float puddleIntensity = (_RainIntensity + 0.5) * .7;
                float puddles = distanceFadeRain * saturate ((normalFinal.r - (1 - puddleIntensity)) * 5) * puddleIntensity * (verticalFactor * verticalFactor) * saturate((1 - normalDoubleScale.g) * 10);
                float metalness = lerp(0, _RainIntensity * 0.9, puddles);
                smoothnessFinal = lerp(smoothnessFinal, saturate(smoothnessFinal + (_RainIntensity * 0.9)), puddles);
                */

                float metalnessFinal = metalnessFromTexture;

                _WeatherMultiplier = 1.0f;
                float verticalFactor = saturate(dot(IN.worldNormal1, float3(0, 1, 0)));
                verticalFactor = saturate(pow(verticalFactor, 16));
				float weatherOcclusionMask = 1.0f;
				ApplyWeather (_WeatherMultiplier, albedoFinal.xyz, smoothnessFinal, metalnessFinal, normalFinal, IN.worldPos1, 1, verticalFactor, verticalFactor, weatherOcclusionMask);

                ApplyIsolines (albedoFinal, emissionFinal, IN.worldPos1, IN.worldNormal1);

                o.Albedo = albedoFinal;
                o.Metallic = metalnessFinal;
                o.Smoothness = smoothnessFinal;
                o.Emission = emissionFinal; // emissionFinal;
                o.Occlusion = occlusionFinal;
                o.Normal = normalFinal;

                /*
                // 3D texture AO test

                float gridSize = 3;
                float3 aoNormal = IN.worldNormal1;
                float3 areaSize = float3 (100, 9, 100) * gridSize;
                float3 aoOffset = float3 (1.5, -1.5, 1.5);

                float3 aoUVUnscaledMain = aoOffset + IN.worldPos1.xyz + aoNormal * gridSize;
                float3 aoUVMain = float3 (aoUVUnscaledMain.x / areaSize.x, (aoUVUnscaledMain.y + areaSize.y) / areaSize.y, aoUVUnscaledMain.z / areaSize.z);
                float aoSampleMain = tex3D (_GlobalEnvironmentAOTex, aoUVMain).a;

                float3 aoUVUnscaledSecondary = aoOffset + IN.worldPos1.xyz + aoNormal * gridSize * 3;
                float3 aoUVSecondary = float3 (aoUVUnscaledSecondary.x / areaSize.x, (aoUVUnscaledSecondary.y + areaSize.y) / areaSize.y, aoUVUnscaledSecondary.z / areaSize.z);
                float aoSampleSecondary = tex3D (_GlobalEnvironmentAOTex, aoUVSecondary).a;

                float aoSkydome = saturate ((dot (float3 (0, 1, 0), aoNormal) + 1) * 0.5);
                float aoFinal = aoSampleMain * lerp (aoSampleSecondary, 1, 0.5);

                o.Albedo *= aoFinal;
                o.Occlusion *= aoFinal;

                // o.Albedo = 0;
                // o.Occlusion = 1;
                // o.Emission = aoFinal;
                */
            }
        }
        ENDCG
    }

    Fallback "Hardsurface/Environment/Block (Shadow)"
}
