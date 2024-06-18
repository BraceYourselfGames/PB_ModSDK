Shader "Hardsurface/Environment/Backgrounds (main)"
{
    Properties
    {
        [NoScaleOffset]
        _MainTex ("Albedo/mask map", 2D) = "white" {}
        _AlbedoTint ("Albedo tint", Color) = (1, 1, 1, 1)

        [Space (20)]
        [NoScaleOffset]
        _MSEO ("MSEO map", 2D) = "white" {}
        _EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
        _EmissionColor ("Emission color", Color) = (0, 0, 0, 1)

        [Space (20)]
        [NoScaleOffset]
        _NormalTex ("Normal map", 2D) = "bump" {}
        _NormalIntensity ("Normal intensity", Range (0.0, 1.0)) = 1.0

        [Space (20)]
        _DetailScale ("Detail scale", Float) = 12
        _DetailIntensityOnAlbedo ("Detail intensity on albedo", Range (0, 1)) = 0.5
        _DetailIntensityOnSmoothness ("Detail intensity on smoothness", Range (0, 1)) = 0.8

        [Toggle (_USE_PROJECTED_MAPS)]
        _UseProjectedMaps ("Use projected maps (grass/asphalt)", Float) = 0
        [HideIfDisabled(_USE_PROJECTED_MAPS)] _ProjMapsScaleModifier ("Projected Maps Scale Modifier", Range (0.1, 5)) = 1

        [Toggle (_USE_VMAP_FOR_PROJ_MAPS)]
        _UseVmapsForProjMaps ("Use vertex color for projected maps (red - grass, green - asphalt)", Float) = 0
        [HideIfDisabled(_USE_VMAP_FOR_PROJ_MAPS)] _VMAPAOIntensity ("AO intensity from alpha channel", Range (0, 1)) = 0.75

        [Space (20)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        _DarkenBacksideFaces ("Darken backside faces", Range (0, 1)) = 0
		[Toggle (_USE_ALPHATEST)] _UseAlphaTest ("Use alpha test", Float) = 0
		[HideIfDisabled(_USE_ALPHATEST)] _Cutoff ("Alpha cutoff", Range (0, 1)) = 0.5

		[Space(20)]
		_WeatherMultiplier ("Weather Multiplier", Range (0, 1)) = 1
		_WeatherOcclusionIntensity ("Weather Occlusion Intensity", Range (0, 1)) = 0
		_WeatherOcclusionMaskPower ("Weather Occlusion Mask Power", Range (1, 32)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Cull [_Cull]

        CGPROGRAM

        #pragma surface surf Standard vertex:vert finalgbuffer:ColorFunctionSliceShading
        #pragma target 5.0
        #pragma only_renderers d3d11 d3d11_9x
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Environment_Shared.cginc"

        #pragma shader_feature_local _USE_PROJECTED_MAPS
        #pragma shader_feature_local _USE_VMAP_FOR_PROJ_MAPS
        #pragma shader_feature_local _USE_ALPHATEST

        sampler2D _MainTex;
        float4 _AlbedoTint;
        sampler2D _MSEO;
        float _EmissionIntensity;
        half4 _EmissionColor;
        sampler2D _NormalTex;
        half _NormalIntensity;

        sampler2D _DetailTex;
        float _DetailScale;
        float _DetailScaleAtDistance;
        float _DetailIntensityOnAlbedo;
        float _DetailIntensityOnSmoothness;

        float _ProjMapsScaleModifier;
        float _VMAPAOIntensity;

        sampler2D _GlobalBackgroundDetailTex;
        sampler2D _GlobalBackgroundAsphaltTex;
        float4 _GlobalBackgroundSwapSettings;
        float4 _GlobalBackgroundSizeSettings;

        float _Cutoff;
        float _DarkenBacksideFaces;

        float3 Overlay3 (float3 src, float3 dst)
        {
            return lerp (1 - 2 * (1 - src) * (1 - dst), 2 * src * dst, step (src, 0.5));
        }

        float Overlay1 (float src, float dst)
        {
            return lerp (1 - 2 * (1 - src) * (1 - dst), 2 * src * dst, step (src, 0.5));
        }

        void vert (inout appdata_full v, out Input o) 
        {
            UNITY_SETUP_INSTANCE_ID (v);
            UNITY_INITIALIZE_OUTPUT (Input, o);
            UNITY_TRANSFER_INSTANCE_ID (v, o);

            o.texcoord_uv1 = v.texcoord;
            o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
            o.worldNormal1 = UnityObjectToWorldNormal (v.normal);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            ApplySliceCutoff (IN);
            
            fixed4 ah = tex2D (_MainTex, IN.texcoord_uv1);

			#ifdef _USE_ALPHATEST
				clip (ah.a - _Cutoff);
			#endif

            fixed4 mseo = tex2D (_MSEO, IN.texcoord_uv1);
            fixed4 detail = tex2D (_GlobalBackgroundDetailTex, IN.texcoord_uv1 * _DetailScale);

            float3 albedoFinal = lerp (ah.rgb, Overlay3 (ah.rgb, detail.rgb), _DetailIntensityOnAlbedo) * _AlbedoTint.rgb;
            
			// Darken backside faces option
			albedoFinal *= saturate (saturate (IN.facingSign) + (1 - _DarkenBacksideFaces));

            float smoothnessFinal = lerp (mseo.g, Overlay1 (mseo.g, detail.a), _DetailIntensityOnSmoothness);

            #ifdef _USE_PROJECTED_MAPS

                float dist = saturate ((distance (_WorldSpaceCameraPos, IN.worldPos1) / _GlobalBackgroundSwapSettings.y) - _GlobalBackgroundSwapSettings.x);
                float2 uvAsphalt = IN.worldPos1.xz / (_GlobalBackgroundSizeSettings.z * _ProjMapsScaleModifier);

                float4 asTerrain = float4(0, 0, 0, 0.5);
                float4 mseoTerrain = float4(0, 0, 0, 1);
                float4 normalTerrain = float4(0, 0, 1, 0.5);
                SampleTerrain(asTerrain, mseoTerrain, normalTerrain, IN.worldPos1);

                fixed4 asAsphalt = lerp (tex2D (_GlobalBackgroundAsphaltTex, uvAsphalt), tex2D (_GlobalBackgroundAsphaltTex, uvAsphalt * _GlobalBackgroundSizeSettings.w), dist);

                #ifdef _USE_VMAP_FOR_PROJ_MAPS
                    float maskGrass = 1 - IN.color.r;
                    float maskAsphalt = IN.color.g;

                    float vertexMapAO = lerp (1.0f, IN.color.a, _VMAPAOIntensity);

                    albedoFinal *= vertexMapAO;
                    mseo.a *= vertexMapAO;
                #else
                    float maskGrass = saturate (ah.a * 2);
                    float maskAsphalt = saturate (ah.a - 0.5) * 2;
                #endif

                albedoFinal = lerp (lerp (asTerrain.rgb, albedoFinal, maskGrass), asAsphalt.rgb, maskAsphalt);
                smoothnessFinal = lerp (lerp (mseoTerrain.g, smoothnessFinal, maskGrass), asAsphalt.a, maskAsphalt);

			#endif

            // Normal map
            half3 normal = UnpackNormal (tex2D (_NormalTex, IN.texcoord_uv1));
            fixed3 normalFinal = lerp (fixed3 (0, 0, 1), normal, _NormalIntensity);
            
            // Other parameters
            float metalnessFinal = mseo.r;
            float3 emissionFinal = mseo.b * _EmissionColor * _EmissionIntensity;
            float occlusionFinal = mseo.a;

			if (_WeatherMultiplier > 0)
			{
				float verticalFactor2 = saturate (IN.worldNormal1.y);
				verticalFactor2 = pow(verticalFactor2, 2);
				float weatherOcclusionMask = lerp (1.0f, pow (occlusionFinal, _WeatherOcclusionMaskPower), _WeatherOcclusionIntensity);
				ApplyWeatherLightweight (_WeatherMultiplier, albedoFinal, smoothnessFinal, metalnessFinal, IN.worldPos1, verticalFactor2, weatherOcclusionMask);
			}

            o.Albedo = albedoFinal;
            o.Smoothness = smoothnessFinal;
            o.Metallic = metalnessFinal;
            o.Normal = normalFinal;
            o.Emission = emissionFinal;
            o.Occlusion = occlusionFinal;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
