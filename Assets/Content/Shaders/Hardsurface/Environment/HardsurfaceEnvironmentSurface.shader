Shader "Hardsurface/Environment/Surface (Instanced)"
{
	Properties
	{
		_HSBOffsetsPrimary ("HSB offsets (primary) + Emission", Color) = (0.5, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary) + Damage", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex ("AH", 2D) = "white" {}
		_AlbedoTint ("Albedo Tint", Color) = (1, 1, 1, 1)
		_MSEO ("MSEO", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1
		_SmoothnessMin ("Smoothness Min", Range (0, 1)) = 0.0
		_SmoothnessMed ("Smoothness Med", Range (0, 1)) = 0.2
		_SmoothnessMax ("Smoothness Max", Range (0, 1)) = 0.8
		_EmissionIntensity ("Emission Intensity", Range (0, 16)) = 0
		_EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
		_EmissionColorFromAlbedo ("Use Albedo as Emission Color", Range (0, 1)) = 0
		_OcclusionIntensity ("Occlusion Intensity", Range (0, 1)) = 1
		_AlbedoIntensityVColorA ("Albedo Intensity From VColor A", Range (0, 1)) = 0
		_OcclusionIntensityVColorA ("AO Intensity From VColor A", Range (0, 1)) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
		
		[Space(30)]
		[Header(Pilot Fake Cockpit Lighting)]
		[BlockInfo(0.5, 0.5, 1, 1)] dummy_info_1 ("The toggle + parameters are set from Character Reflection Helper script.", Float) = 0
		[Toggle(USE_COCKPIT_LIGHTING)]  _UseCockpitLighting("Use fake cockpit lighting", Int) = 0
		[HideIfDisabled(USE_COCKPIT_LIGHTING)][NoScaleOffset] _ReflectionTex ("Reflection texture", Cube) = "black" {}
		[HideIfDisabled(USE_COCKPIT_LIGHTING)] _ReflectionTint("Reflection Tint", Color) = (1.0, 1.0, 1.0, 1)
		[HideIfDisabled(USE_COCKPIT_LIGHTING)] _ReflectionBrightness ("Reflection brightness", Range (0, 1)) = 0.2
		[HideIfDisabled(USE_COCKPIT_LIGHTING)] _ReflectionMip ("Reflection mip", Range (0, 5)) = 3
		[HideIfDisabled(USE_COCKPIT_LIGHTING)] _ReflectionPositionRange ("Reflection position/range", Vector) = (0, 0, 0, 0.5)
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}
		LOD 200
		Cull [_Cull]

		CGPROGRAM

		#pragma surface surf Standard vertex:VertexFunction addshadow
		#pragma target 5.0
		#pragma multi_compile_instancing

		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

		struct Input
		{
			float2 uv_MainTex;
			float4 vertexColor : COLOR;
			float3 localPos;
			float3 localNormal;
			float3 worldPos1;
			float3 worldNormal1;
			float3 viewDir;
			float3 viewDir1;
			float3 worldRefl;

			INTERNAL_DATA
		};

		sampler2D _MainTex;
		fixed4 _AlbedoTint;
		sampler2D _MSEO;
		sampler2D _Bump;

		half _NormalIntensity;
		half _SmoothnessMin;
		half _SmoothnessMed;
		half _SmoothnessMax;

		fixed4 _EmissionColor;
		float _EmissionIntensity;
		float _EmissionColorFromAlbedo;

		float _OcclusionIntensity;

		float _AlbedoIntensityVColorA;
		float _OcclusionIntensityVColorA;

		// float4 _GlobalEnvironmentAmbientSettings;

		// Declare instanced properties inside a cbuffer.
		// Each instanced property is an array of by default 500(D3D)/128(GL) elements. Since D3D and GL imposes a certain limitation
		// of 64KB and 16KB respectively on the size of a cubffer, the default array size thus allows two matrix arrays in one cbuffer.
		// Use maxcount option on #pragma instancing_options directive to specify array size other than default (divided by 4 when used
		// for GL).

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP (fixed4, _HSBOffsetsPrimary)
		#define _HSBOffsetsPrimary_arr Props
		UNITY_DEFINE_INSTANCED_PROP (fixed4, _HSBOffsetsSecondary)
		#define _HSBOffsetsSecondary_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		void VertexFunction (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);

			o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
			o.worldNormal1 = UnityObjectToWorldNormal (v.normal);
			o.viewDir1 = WorldSpaceViewDir (v.vertex);
		}

		void surf (Input IN, inout SurfaceOutputStandard output)
		{
			fixed4 hsbOffsetsPrimary = pow (UNITY_ACCESS_INSTANCED_PROP (_HSBOffsetsPrimary_arr, _HSBOffsetsPrimary), 0.454545);
			fixed4 hsbOffsetsSecondary = pow (UNITY_ACCESS_INSTANCED_PROP (_HSBOffsetsSecondary_arr, _HSBOffsetsSecondary), 0.454545);

			fixed4 ah = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 mseo = tex2D (_MSEO, IN.uv_MainTex);
			fixed3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));

			fixed3 albedoBase = ah.rgb * _AlbedoTint.rgb;
			fixed albedoMask = ah.a;
			fixed metalness = mseo.x;
			fixed smoothness = mseo.y;
			fixed emission_mask = mseo.z;
			fixed occlusion = mseo.w;

			fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);

			float occlusionFinal = lerp (1, occlusion, _OcclusionIntensity);

			// AO intensity and Albedo darkening from vertex color alpha
			occlusionFinal *= lerp (1, IN.vertexColor.a, _OcclusionIntensityVColorA);
			albedoBase = lerp (albedoBase, Overlay (albedoBase, IN.vertexColor.a), _AlbedoIntensityVColorA);

			float albedoMaskMinToMed = saturate(albedoMask * 2);
			float albedoMaskMedToMax = saturate(albedoMask - 0.5) * 2;

			float3 albedoFinal = RGBTweakHSV
			(
				albedoBase,
				albedoMaskMinToMed,
				albedoMaskMedToMax,
				hsbOffsetsPrimary.x,
				hsbOffsetsPrimary.y,
				hsbOffsetsPrimary.z,
				hsbOffsetsSecondary.x,
				hsbOffsetsSecondary.y,
				hsbOffsetsSecondary.z,
				1
			);

			float smoothnessMaskMinToMed = saturate (smoothness * 2);
			float smoothnessMaskMedToMax = saturate (smoothness - 0.5) * 2;
			float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax);

			float3 emissionFinal = lerp (_EmissionColor, albedoFinal, _EmissionColorFromAlbedo) * _EmissionIntensity * emission_mask;


			/*
			float occlusionHeightFactor = saturate ((IN.worldPos1.y + _GlobalEnvironmentAmbientSettings.y) / _GlobalEnvironmentAmbientSettings.x);  // saturate ((IN.worldPos1.y * _GlobalEnvironmentAmbientSettings.x + _GlobalEnvironmentAmbientSettings.y) / _GlobalEnvironmentAmbientSettings.x);
			float occlusionHeightGraded = lerp (0.25, 1, occlusionHeightFactor);
			float occlusionFinal = occlusion * lerp (1, occlusionHeightGraded, _GlobalEnvironmentAmbientSettings.z * saturate (1 - abs (IN.worldNormal1.y)));
			*/

			output.Albedo = albedoFinal;
			output.Metallic = metalness;
			output.Smoothness = smoothnessFinal;
			output.Emission = emissionFinal;
			output.Occlusion = occlusionFinal;
			output.Normal = normalFinal;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
