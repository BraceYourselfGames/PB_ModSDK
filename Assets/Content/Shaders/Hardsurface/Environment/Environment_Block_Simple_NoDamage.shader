// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Hardsurface/Environment/Block (simple, no damage)"
{
	Properties 
	{
		_HSBOffsetsPrimary ("HSB offsets (primary) + Emission", Vector) = (0.5, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary) + Damage", Vector) = (0.5, 0.5, 0.5, 1)
		_MainTex ("AH", 2D) = "white" {}
		_MSEO ("MSEO", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}
		_SmoothnessMin ("SmoothnessMin", Range (0, 1)) = 0.0
		_SmoothnessMed ("SmoothnessMed", Range (0, 1)) = 0.2
		_SmoothnessMax ("SmoothnessMax", Range (0, 1)) = 0.8
		_EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
		_EmissionColor ("Emission color", Color) = (0, 0, 0, 1)
        _ScaleTestToggle ("Scale test", Range (0,1)) = 0
        _ScaleTestValue ("Scale test value", Vector) = (1, 1, 1, 1)

		_DestructionAnimation ("Destruction animation", Range (0, 1)) = 0
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
		}
		Cull Off
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:SharedVertexFunctionLight addshadow
		#pragma target 5.0
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup
		#include "UnityCG.cginc"
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Environment_Shared.cginc"

		// Config maxcount. See manual page.
		// #pragma instancing_options


		sampler2D _MainTex;
		sampler2D _MSEO;
		sampler2D _Bump;

		half _SmoothnessMin;
		half _SmoothnessMed;
		half _SmoothnessMax;

		fixed4 _EmissionColor;
		float _EmissionIntensity;
		
		uniform sampler3D _GlobalEnvironmentAOTex;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float destructionAnimation = 0;
			float4 hsbPrimary = float4(0,0.5,0.5,0);
			float4 hsbSecondary = float4(0,0.5,0.5,0);
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

				HalfVector8 cachedHSB = hsbData[unity_InstanceID];
				hsbPrimary = cachedHSB.UnpackPrimary();
				hsbSecondary = cachedHSB.UnpackSecondary();
				destructionAnimation = hsbSecondary.w;//cachedHSBSecondary = float4(0,0,0,1);.w; // UNITY_ACCESS_INSTANCED_PROP (_DestructionAnimation_arr, _DestructionAnimation);
				
			#endif
			
			if (destructionAnimation > 0.99)
			{
				clip (-1);
			}
			else
			{
				fixed4 ah = tex2D (_MainTex, IN.texcoord_uv1);
				fixed4 mseo = tex2D (_MSEO, IN.texcoord_uv1);

				fixed3 albedoBase = ah.rgb;
				fixed albedoMask = ah.a;
				fixed metalness = mseo.x;
				fixed smoothness = mseo.y;
				fixed emissionFromTexture = mseo.z;
				fixed occlusionFromTexture = mseo.w;

				// Normal correction necessary due to disabled backface culling
				float3 n = float3 (0, 0, 1);
				float backsideFactor = GetBacksideFactor (IN.viewDir);
				float3 normalFinal = lerp (n, -n, backsideFactor);

				float3 albedoFinal = GetAlbedo (hsbPrimary.xyz, hsbSecondary.xyz, albedoBase, albedoMask, occlusionFromTexture);
				float smoothnessFinal = GetSmoothness (smoothness, _SmoothnessMin, _SmoothnessMed, _SmoothnessMax, backsideFactor);
				float3 emissionFinal = GetEmission (IN.damageIntegrityCriticality.x, hsbSecondary.w, emissionFromTexture, _EmissionColor, _EmissionIntensity);
				float occlusionFinal = GetOcclusion (IN.worldPos1.y, IN.worldNormal1.y, occlusionFromTexture);

				float4 detail = GetDetailSample (IN.worldPos1, IN.worldNormal1);
				float integrityMultiplier = GetIntegrityMultiplier (IN.damageIntegrityCriticality.y, detail);

				albedoFinal *= integrityMultiplier;
				smoothnessFinal *= integrityMultiplier;
				emissionFinal *= integrityMultiplier;

				clip (saturate ((detail.z - 0.5) * lerp (_GlobalEnvironmentDetailContrast, 1, destructionAnimation) + 0.5 - 0.25) - 1 * destructionAnimation);

				// Rain
				// metalness = saturate(metalness + _RainIntensity);
				// smoothnessFinal = saturate(smoothnessFinal + ((smoothnessFinal + 0.8) * _RainIntensity));

				o.Albedo = albedoFinal;
				o.Metallic = metalness;
				o.Smoothness = smoothnessFinal;
				o.Emission = emissionFinal;
				o.Occlusion = occlusionFinal;
				o.Normal = normalFinal;
				o.Alpha = 1 - IN.damageIntegrityCriticality.x;
				
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
	FallBack "Diffuse"
}