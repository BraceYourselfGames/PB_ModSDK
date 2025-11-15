Shader "Hardsurface/Environment/Terrain (plain)"
{
	Properties
	{
		_MainTex ("Side map", 2D) = "white" {}
		_SedimentTex ("Sediment map", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}

		[Space (12)]
		[Toggle (_USE_GLOBAL_MAP)]
		_UseGlobalMap ("Global map used", Float) = 1
        [HideIfDisabled(_USE_GLOBAL_MAP)] _GlobalAlbedoTex ("Global albedo map", 2D) = "black" {}
		[HideIfDisabled(_USE_GLOBAL_MAP)] _GlobalNormalTex ("Global normal map", 2D) = "bump" {}
		[HideIfDisabled(_USE_GLOBAL_MAP)] _GlobalMapTint ("Global map tint", Color) = (1, 1, 1, 1)
		[HideIfDisabled(_USE_GLOBAL_MAP)] _GlobalSmoothness ("Global layer smoothness", Range (0, 1)) = 0

		[Space (12)]
		_TintSide ("Side tint", Color) = (1, 1, 1, 1)
		_NormalIntensity ("Normal intensity", Range (0,1)) = 1.0
		_NormalIntensityHorizontal ("Normal intensity", Range (0,1)) = 1.0
		_GlossinessMain ("Smoothness", Range (0,1)) = 0.0
		_BorderFactorA ("Border factor A", Float) = 1
		_BorderFactorB ("Border factor B", Float) = 1
		_TexScaleSediment ("Texture scale (sediment)", Float) = 1

		[Space (12)]
		_DistBlendMin ("Distance Blend Begin", Float) = 0
		_DistBlendMax ("Distance Blend Max", Float) = 100

		[Space (12)]
		_TexSplat ("Splat map", 2D) = "white" {}
		_TexSplatScale ("Splat scale", Float) = 10
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull Off
		LOD 200


		CGPROGRAM

		#pragma surface surf Standard addshadow vertex:vert
		#pragma target 5.0
		#pragma only_renderers d3d11 d3d11_9x vulkan
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Environment_Shared.cginc"
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma shader_feature_local _USE_GLOBAL_MAP

		sampler2D _MainTex;
		sampler2D _Bump;
		sampler2D _SedimentTex;
		sampler2D _TexSplat;

		fixed4 _TintSide;
		half _NormalIntensity;
		half _NormalIntensityHorizontal;
		half _GlossinessMain;
		float _BorderFactorA;
		float _BorderFactorB;
		float _BrightnessMultiplier;
		float4 _BorderFactors;

		float _TexSplatScale;
		float _DistBlendMin;
		float _DistBlendMax;

		#if _USE_GLOBAL_MAP
			sampler2D _GlobalAlbedoTex;
			sampler2D _GlobalNormalTex;
			fixed4 _GlobalMapTint;
			float _GlobalSmoothness;
        #endif

		void vert (inout appdata_full v, out Input o)
        {
            UNITY_SETUP_INSTANCE_ID (v);
            UNITY_INITIALIZE_OUTPUT (Input, o);
            UNITY_TRANSFER_INSTANCE_ID (v, o);

            // There are no traditional 2D samplers used in this surface shader, so UV inputs won't be auto-generated
            // we have to fill the UV1 and UV2 manually (UV2 packs array index)
            o.texcoord_uv1 = v.texcoord;
            o.texcoord_uv2 = v.texcoord1;

            float4 scaleAndSpinProp = float4(1, 1, 1, 0);

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            	scaleAndSpinProp = scaleData[unity_InstanceID].Unpack();
            #endif

            v.vertex.xyz *= scaleAndSpinProp.xyz;
            v.normal.xyz *= scaleAndSpinProp.xyz;
            v.tangent.xyz *= scaleAndSpinProp.xyz;

            o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
            o.worldNormal1 = UnityObjectToWorldNormal (v.normal);
            o.color = v.color;
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float dist = saturate ((distance (_WorldSpaceCameraPos, IN.worldPos1) / _DistBlendMax) - _DistBlendMin);
			float4 splat = tex2D (_TexSplat, IN.worldPos1.xz / _TexSplatScale);

			float4 asTerrain = float4 (0,0,0,0.5);
			float4 mseoTerrain = float4 (0,0,0,1);
			float4 normalTerrain = float4 (0,0,1,0.5);
			SampleTerrain (asTerrain, mseoTerrain, normalTerrain, IN.worldPos1);

			fixed4 albedoHorizontal = asTerrain;

			float4 albedoVertical = tex2D (_MainTex, IN.texcoord_uv1);
			albedoVertical.a = _GlossinessMain;

			float3 albedoVerticalSedimentXY = tex2D (_SedimentTex, float2 (IN.worldPos1.x * 0.666, IN.worldPos1.y) / _TexScaleSediment).xyz;
			float3 albedoVerticalSedimentZY = tex2D (_SedimentTex, float2 (IN.worldPos1.z * 0.666, IN.worldPos1.y) / _TexScaleSediment).xyz;
			float3 normalFlat = normalize (float3 (IN.worldNormal1.x, 0.01f, IN.worldNormal1.z));
			float3 albedoVerticalSediment = albedoVerticalSedimentXY * abs (normalFlat.z) + albedoVerticalSedimentZY * abs (normalFlat.x);

			albedoVertical.xyz = lerp (albedoVerticalSediment.xyz, Overlay (albedoVertical.xyz, albedoVerticalSediment.xyz), _TintSide.w) * _TintSide.xyz;

			float verticalFactor = saturate (dot (IN.worldNormal1 * _BorderFactorA, float3 (0, 1, 0)) + _BorderFactorB * _BorderFactorA);
			verticalFactor *= lerp (normalTerrain.a, 1, pow (verticalFactor, 4));

			float4 albedoFinal = lerp (albedoVertical, albedoHorizontal, verticalFactor);

			#if _USE_GLOBAL_MAP
                float4 albedoGlobalOverride = tex2D (_GlobalAlbedoTex, IN.texcoord_uv2) * _GlobalMapTint;
                float maskGlobalOverride = IN.color.r; // albedoGlobalOverride.a * IN.color.r;
                albedoFinal = lerp (albedoFinal, albedoGlobalOverride, maskGlobalOverride);
			#endif

			half smoothnessFinal = lerp (0, mseoTerrain.y, verticalFactor);

			fixed3 normalHorizontal = float3 (normalTerrain.x * 2 - 1, normalTerrain.y * 2 - 1, normalTerrain.z * 2 - 1);
			normalHorizontal = normalize (normalHorizontal);

			fixed3 normalVertical = UnpackNormal (tex2D (_Bump, IN.texcoord_uv1));
			normalVertical = lerp (fixed3 (0, 0, 1), normalVertical, _NormalIntensity);

			#if _USE_GLOBAL_MAP
                smoothnessFinal = lerp (smoothnessFinal, _GlobalSmoothness, maskGlobalOverride);
			#endif

			float3 normalFinal = lerp (normalVertical, normalHorizontal, verticalFactor);

			#if _USE_GLOBAL_MAP
				fixed3 normalGlobalOverride = UnpackNormal (tex2D (_GlobalNormalTex, IN.texcoord_uv2));
				normalFinal = lerp (normalFinal, normalGlobalOverride, maskGlobalOverride);
			#endif

			normalFinal = normalize (normalFinal);

			float metalnessFinal = 0;
			float3 normalFinalWorld = WorldNormalVector (IN, normalFinal);
			float verticalFactor5 = saturate (dot (normalFinalWorld, float3 (0, 1, 0)));
			verticalFactor5 = saturate (pow (verticalFactor5, 2));

			// frustratingly, absolutely everything related to this calculation gets compiled out if we don't pipe it into output
			smoothnessFinal = lerp (verticalFactor5, smoothnessFinal, 0.9999);

            _WeatherMultiplier = 1.0f;
			float weatherOcclusionMask = 1.0f;
			ApplyWeather (_WeatherMultiplier, albedoFinal.xyz, smoothnessFinal, metalnessFinal, normalFinal, IN.worldPos1, 1, verticalFactor5, verticalFactor5, weatherOcclusionMask);

			float3 emissionFinal = float3 (0, 0, 0);
            ApplyIsolines (albedoFinal.xyz, emissionFinal, IN.worldPos1, IN.worldNormal1);

			o.Albedo = albedoFinal;
			o.Metallic = metalnessFinal;
			o.Smoothness = smoothnessFinal;
			o.Emission = emissionFinal;
			o.Occlusion = 1;
			o.Normal = normalFinal;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
