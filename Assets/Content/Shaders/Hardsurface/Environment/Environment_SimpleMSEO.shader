Shader "Hardsurface/Environment/Background (UV2)"
{
	Properties
	{		
		[NoScaleOffset] _MainTex ("Albedo UV1", 2D) = "white" {}
		_AlbedoTint ("Albedo Tint", Color) = (1, 1, 1, 1)
		[Space(20)]
		[Toggle (_USE_HSBOFFSETS)] _UseHSB ("Use HSB Offsets", Float) = 1
        [HideIfDisabled(_USE_HSBOFFSETS)] _HSBOffsetsUV1 ("HSB Offsets Primary (Albedo UV1)", Vector) = (0, 0.5, 0.5, 1)
		[Toggle (_USE_HSBOFFSETSSECONDARY)] _UseHSBOffsetsUV1Secondary ("Use HSB Offsets Secondary (Albedo UV1)", Float) = 0
		[HideIfDisabled(_USE_HSBOFFSETSSECONDARY)] _HSBOffsetsUV1Secondary ("HSB offsets Secondary (Albedo UV1)", Vector) = (0, 0.5, 0.5, 1)

		[Space(20)]
		[Toggle (_USE_MSEO)] _UseMSEO ("Use MSEO", Float) = 1
		[HideIfDisabled(_USE_MSEO)] [NoScaleOffset] _MSEO ("MSEO", 2D) = "white" {}
		[HideIfDisabled(_USE_MSEO)] _EmissionIntensity ("Emission Intensity", Range (0, 64)) = 0
		[HideIfDisabled(_USE_MSEO)] [HDR] _EmissionColor ("EmissionColor", Color) = (0, 0, 0, 1)
		[HideIfDisabled(_USE_MSEO)] _EmissionColorFromAlbedo ("Use Albedo as Emission Color", Range (0, 1)) = 0
		[HideIfDisabled(_USE_MSEO)] _EnableEmissionDuringNightOnly ("Enable Emission During Night Only", Range (0, 1)) = 0
		[Space(5)]
		[HideIfDisabled(_USE_MSEO)] _OcclusionIntensity ("Occlusion Intensity", Range (0, 1)) = 1
		[Space(5)]
		_Smoothness ("Smoothness Boost", Range (-1.0, 1.0)) = 0.0
		_Metallic ("Metalness Boost", Range (-1.0, 1.0)) = 0.0
		_VCOcclusionIntensity ("Vertex Color Occlusion Intensity (A)", Range (0.0, 1.0)) = 1.0

		[Space(20)]
		[NoScaleOffset] _NormalTex ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal Intensity", Range (0.0, 1.0)) = 1.0

		[Space(20)]
		[Toggle (_USE_UV2ALBEDO)] _UseAlbedoUV2 ("Use Albedo UV2", Float) = 0
		[HideIfDisabled(_USE_UV2ALBEDO)] [NoScaleOffset] _SecondaryTex ("Albedo UV2", 2D) = "black" {}
		[HideIfDisabled(_USE_UV2ALBEDO)] _SecondatyTexTile ("Albedo UV2 Tile", Range (0.1, 20.0)) = 1.0
		[HideIfDisabled(_USE_UV2ALBEDO, _USE_HSBOFFSETS)] _HSBOffsetsUV2 ("HSB Offsets Primary (Albedo UV2)", Vector) = (0, 0.5, 0.5, 1)
		[Space(5)]
		[BlockInfo(0.5, 0.5, 1, 1)] dummy_info_1 ("If Albedo UV2 is used: False - use (Albedo UV1) alpha, True - use (Albedo UV2) alpha for blending", Float) = 0
		[Toggle (_USE_ALPHAFROMUV2ALBEDO)] _UseAlphaFromAlbedoUV2 ("Use (Albedo UV2) alpha for blending two albedo textures", Float) = 0
		[Space(5)]
		[BlockInfo(0.5, 0.5, 1, 1)] dummy_info_2 ("If Albedo UV2 is used: if value > 0 then it is used instead of texture's alpha for blending between two Albedo maps", Float) = 0
		[HideIfDisabled(_USE_UV2ALBEDO)] _CustomUniformBlendValueUV2 ("Custom uniform blend amount (Albedo UV2)", Range (0.0, 1.0)) = 0.0

		[Space(20)]
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
		_DarkenBacksideFaces ("Darken backside faces", Range (0, 1)) = 0
		[Toggle (_USE_ALPHATEST)] _UseAlphaTest ("Use alpha test", Float) = 0
		[HideIfDisabled(_USE_ALPHATEST)] _Cutoff ("Alpha cutoff", Range (0, 1)) = 0.5

		[Space(20)]
		[Toggle (_USE_PROXIMITY_FADE)] _UseProximityFade("Use Proximity Fade", Float) = 0
		[HideIfDisabled(_USE_PROXIMITY_FADE)] _ProximityFade ("Proximity Fade (Intensity, Start, Interval)", Vector) = (0, 1, 1, 0)

		[Space(20)]
		[Toggle (_USE_BUILDINGS_WAR_DAMAGE)] _UseBuildingssWarDamage ("Use War Damage for Buildings", Float) = 0
		[HideIfDisabled(_USE_BUILDINGS_WAR_DAMAGE)] _WarDamageMaskOffset ("War Damage Mask Offset", Range (-10, 10)) = 0
		[HideIfDisabled(_USE_BUILDINGS_WAR_DAMAGE)] _WarDamageMaskSize ("War Damage Mask Size", Range (0.1, 1)) = 0.8
		[HideIfDisabled(_USE_BUILDINGS_WAR_DAMAGE)] _WarDamageAlphaCutoutSize ("War Damage Alpha Cutout Size", Range (0.1, 1.1)) = 0.5

		[Space(20)]
		_WeatherMultiplier ("Weather Multiplier", Range (0, 1)) = 1
		_WeatherOcclusionIntensity ("Weather Occlusion Intensity", Range (0, 1)) = 0
		_WeatherOcclusionMaskPower ("Weather Occlusion Mask Power", Range (1, 32)) = 1
		[Toggle(_USE_FULL_WEATHER_EFFECTS)] _UseFullWeatherFX ("Use Full Weather Effects", Int) = 0

	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}
		Cull [_Cull]

		Stencil
		{
			Ref 192
			ReadMask 255
			WriteMask 207
			Comp Always
			Fail Keep
			ZFail Keep
			Pass Replace
		}

        CGPROGRAM

        #pragma surface surf Standard vertex:vert finalgbuffer:ColorFunctionSliceShading
        #pragma target 5.0
		#pragma shader_feature_local _USE_HSBOFFSETS
		#pragma shader_feature_local _USE_HSBOFFSETSSECONDARY
		#pragma shader_feature_local _USE_UV2ALBEDO
		#pragma shader_feature_local _USE_ALPHAFROMUV2ALBEDO
		#pragma shader_feature_local _USE_MSEO
		#pragma shader_feature_local _USE_ALPHATEST
		#pragma shader_feature_local _USE_PROXIMITY_FADE
		#pragma shader_feature_local _USE_BUILDINGS_WAR_DAMAGE
		#pragma shader_feature_local _USE_FULL_WEATHER_EFFECTS

		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Environment_Shared.cginc"

        float4 _HSBOffsetsUV1;
		float4 _HSBOffsetsUV1Secondary;
        float4 _HSBOffsetsUV2;

		float4 _AlbedoTint;
        sampler2D _MainTex;

		#ifdef _USE_UV2ALBEDO
			sampler2D _SecondaryTex;
			float _SecondatyTexTile;
			float _CustomUniformBlendValueUV2;
		#endif

		#ifdef _USE_MSEO
			sampler2D _MSEO;
			float _EmissionIntensity;
			float4 _EmissionColor;
			float _EmissionColorFromAlbedo;
			float _EnableEmissionDuringNightOnly;
			float _OcclusionIntensity;
		#endif

		float _Smoothness;
        float _Metallic;
		float _VCOcclusionIntensity;

        sampler2D _NormalTex;
		float _NormalIntensity;
        float4 _ProximityFade;

		float _WarDamageMaskOffset;
		float _WarDamageMaskSize;
		float _WarDamageAlphaCutoutSize;

		float _Cutoff;
		float _DarkenBacksideFaces;

		float smoothnessMaskMinToMed;
		float smoothnessMaskMedToMax;
		float3 albedoFinal;
		float4 ah;
		float4 ahUV2;
		float blendingMask;

        void vert (inout appdata_full v, out Input o) 
        {
            UNITY_SETUP_INSTANCE_ID (v);
            UNITY_INITIALIZE_OUTPUT (Input, o);
            UNITY_TRANSFER_INSTANCE_ID (v, o);

			o.texcoord_uv1 = v.texcoord;
			o.texcoord_uv2 = v.texcoord1;

            o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
            o.worldNormal1 = UnityObjectToWorldNormal (v.normal);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			ah = tex2D (_MainTex, IN.texcoord_uv1);

        	ApplySliceCutoff (IN);

			#ifdef _USE_ALPHATEST
				clip (ah.a - _Cutoff);
			#endif

			#ifdef _USE_HSBOFFSETS
				#ifdef _USE_HSBOFFSETSSECONDARY
					float albedoMaskMinToMed = saturate (saturate (ah.a - 0.4) * 32);
					float albedoMaskMedToMax = saturate (saturate (ah.a - 0.5) * 32);

					float3 albedoFinal = RGBTweakHSV
					(
						ah.rgb,
						albedoMaskMinToMed,
						albedoMaskMedToMax,
						_HSBOffsetsUV1.x,
						_HSBOffsetsUV1.y,
						_HSBOffsetsUV1.z,
						_HSBOffsetsUV1Secondary.x,
						_HSBOffsetsUV1Secondary.y,
						_HSBOffsetsUV1Secondary.z,
						1.0f
					);

					ah.rgb = albedoFinal;
				#else
					ah.rgb = RGBAdjustWithHSV(ah.rgb, _HSBOffsetsUV1.x, _HSBOffsetsUV1.y, _HSBOffsetsUV1.z);
				#endif
			#endif

			float vcOcclusion = lerp (1.0, IN.color.a, _VCOcclusionIntensity);

			// Albedo
			#ifdef _USE_UV2ALBEDO
				ahUV2 = tex2D (_SecondaryTex, IN.texcoord_uv2 * _SecondatyTexTile);

				#ifdef _USE_HSBOFFSETS
					ahUV2.rgb = RGBAdjustWithHSV(ahUV2.rgb, _HSBOffsetsUV2.x, _HSBOffsetsUV2.y, _HSBOffsetsUV2.z);
				#endif

				#ifdef _USE_ALPHAFROMUV2ALBEDO
					blendingMask = ahUV2.a;
				#else
					// Need to invert the mask for correct blending order
					blendingMask = 1 - ah.a;
				#endif

				if (_CustomUniformBlendValueUV2 > 0)
					blendingMask = _CustomUniformBlendValueUV2;

				albedoFinal = lerp (ah.rgb, ahUV2.rgb, blendingMask) * vcOcclusion * _AlbedoTint.rgb;
			#else
				albedoFinal = ah.rgb * vcOcclusion* _AlbedoTint.rgb;
			#endif

			float smoothnessFinal;
			float metalnessFinal;
			float3 emissionFinal;
			float occlusionFinal;
            
			#ifdef _USE_MSEO
				float4 mseo = tex2D (_MSEO, IN.texcoord_uv1);
				// Smoothness
				smoothnessFinal = saturate (mseo.g + _Smoothness);
				// Metalness
				metalnessFinal = saturate (mseo.r + _Metallic);
				// Emission
				float emissionIntensity = lerp (_EmissionIntensity, _EmissionIntensity * TOD_NightTimeSwitch, _EnableEmissionDuringNightOnly);
				emissionFinal = mseo.b * lerp (_EmissionColor, albedoFinal, _EmissionColorFromAlbedo) * emissionIntensity;
				// Occlusion
				occlusionFinal = lerp (1, mseo.a * vcOcclusion, _OcclusionIntensity);
			#else
				// Smoothness
				smoothnessFinal = saturate (_Smoothness * vcOcclusion);
				// Metalness
				metalnessFinal = saturate (_Metallic);
				// Emission
				emissionFinal = 0.0f;
				// Occlusion
				occlusionFinal = vcOcclusion;
			#endif

			// Normal map
			float3 normal = UnpackNormal (tex2D (_NormalTex, IN.texcoord_uv1));
			float3 normalFinal = lerp (float3 (0, 0, 1), normal, _NormalIntensity);

			if (_WeatherMultiplier > 0)
			{
				#if defined(_USE_FULL_WEATHER_EFFECTS)
					float3 normalFinalWorld = WorldNormalVector (IN, normalFinal);
					float verticalFactor2 = saturate (normalFinalWorld.y);
				#else
					float verticalFactor2 = saturate (IN.worldNormal1.y);
				#endif

				verticalFactor2 = pow(verticalFactor2, 2);
				float weatherOcclusionMask = lerp (1.0f, pow (occlusionFinal, _WeatherOcclusionMaskPower), _WeatherOcclusionIntensity);
				
				#if defined(_USE_FULL_WEATHER_EFFECTS)
					ApplyWeather (_WeatherMultiplier, albedoFinal, smoothnessFinal, metalnessFinal, normalFinal, IN.worldPos1, 1, verticalFactor2, verticalFactor2, weatherOcclusionMask);
				#else
					ApplyWeatherLightweight (_WeatherMultiplier, albedoFinal, smoothnessFinal, metalnessFinal, IN.worldPos1, verticalFactor2, weatherOcclusionMask);
				#endif
			}

			#ifdef _USE_PROXIMITY_FADE
				float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
				float distanceToCamera = distance (_WorldSpaceCameraPos, IN.worldPos1);
				float distanceToCameraShifted = max (0, distanceToCamera - _ProximityFade.y);
				float interval = max (0.01, _ProximityFade.z);
				float opacity = saturate (distanceToCameraShifted / interval + (1 - _ProximityFade.x));
				
				clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);
			#endif
		
			#ifdef _USE_BUILDINGS_WAR_DAMAGE
				float4 detailTex = tex2D (_GlobalDetailTex, IN.texcoord_uv2);
				float noiseDamageDistort = (detailTex.b - 0.5) * 2;

				float3 worldPosWithOffset = (IN.worldPos1 * _WarDamageMaskSize) + _WarDamageMaskOffset + noiseDamageDistort;
				float buildingDamageMask = (sin (worldPosWithOffset.x / 3) * sin (worldPosWithOffset.z / 1.5) * sin (worldPosWithOffset.y / 2) + 1) * 0.5;
				buildingDamageMask = pow (saturate ((1 - buildingDamageMask) * 2), 2);
				float buildingDamageCutoutDetails = saturate (detailTex.g * buildingDamageMask);
				float buildingDamageDetails = saturate (detailTex.r + saturate ((buildingDamageMask - 0.5) * 1.5));
				float verticalGradientFromVCOcclusion = pow (lerp (0, 1 - IN.color.a, _VCOcclusionIntensity) * 2, 2);

				buildingDamageMask = saturate (buildingDamageMask + buildingDamageCutoutDetails + verticalGradientFromVCOcclusion);
				clip (buildingDamageMask - _WarDamageAlphaCutoutSize);

				albedoFinal *= buildingDamageMask * buildingDamageDetails;
			#endif

			// Darken backside faces option
			albedoFinal *= saturate (saturate (IN.facingSign) + (1 - _DarkenBacksideFaces));
			smoothnessFinal *= saturate (saturate (IN.facingSign) + (1 - _DarkenBacksideFaces));
			metalnessFinal = lerp (metalnessFinal, 1.0, (1 - saturate (IN.facingSign)) * _DarkenBacksideFaces);
			emissionFinal *= saturate (saturate (IN.facingSign) + (1 - _DarkenBacksideFaces));
			
			o.Albedo = albedoFinal;
			o.Smoothness = smoothnessFinal;
			o.Metallic = metalnessFinal;
			o.Emission = emissionFinal;
			o.Occlusion = occlusionFinal;
			o.Normal = normalFinal;
        }

        ENDCG
	}
	FallBack "Diffuse"
}
