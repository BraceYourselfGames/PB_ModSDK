Shader "Hardsurface/Prop/Standard (crushable)"
{
	Properties
	{
		_InstancePropsOverride ("Instanced properties override", Range (0, 1)) = 0
		_HSBOffsetsPrimary ("HSB offsets (primary), emission toggle", Vector) = (0, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary)", Vector) = (0, 0.5, 0.5, 1)
        _PackedPropData ("Visibility, explosion, compression, integrity", Vector) = (1, 0, 0, 1)

		_MainTex ("AH", 2D) = "white" {}
		_MSEO ("MSEO", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}

		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1

		_SmoothnessMin ("Smoothness (min.)", Range (0, 1)) = 0.0
		_SmoothnessMed ("Smoothness (med.)", Range (0, 1)) = 0.2
		_SmoothnessMax ("Smoothness (max.)", Range (0, 1)) = 0.8

		_OcclusionIntensity ("Occlusion intensity", Range(0, 1)) = 1.0

		_EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
		_EmissionColor ("Emission color multiplier", Color) = (1, 1, 1, 1)

		_Scale ("Scale", Vector) = (1, 1, 1, 1)
		_CrushParameters ("Crush pos. (XY), add. (Z), mul. (W)", Vector) = (0, 0, 0, 1)
		_CrushIntensity ("Crush intensity", Range (0, 1)) = 0

		[Toggle (_USE_CAR_PARTS_ROTATION)]
		_UseCarPartsRotation ("Use Car Parts Rotation", Float) = 0
		[HideIfDisabled(_USE_CAR_PARTS_ROTATION)] _CarPartsRotationMin ("Car Parts Rotation Angles Min", Vector) = (-30, 0, 0, 0)
		[HideIfDisabled(_USE_CAR_PARTS_ROTATION)] _CarPartsRotationMax ("Car Parts Rotation Angles Max", Vector) = (30, -20, 20, 0)

		[Toggle (_USE_ALPHATEST)]
		_UseAlphaTest ("Use alpha test", Float) = 0
		_Cutoff ("Alpha cutoff", Range (0, 1)) = 0.5

		_DarkenBacksideFaces ("Darken backside faces", Range (0, 1)) = 0

		[Toggle (_USE_WIND_DEFORMATION)]
		_UseWindDeformation ("Use Wind Deformation", Float) = 0
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WindIntensity ("Wind Intensity", Range(0, 2)) = 1
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WaveLengthScale ("Wind Waves Length Scale", Vector) = (1, 1, 1, 1)
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WaveTimeSpeedScale ("Wind Waves Speed Scale", Vector) = (1, 1, 1, 1)
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WaveTimeOffset ("Wind Waves Time Offset", Vector) = (0, 0, 0, 1)
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WaveAmplitudeScale ("Wind Waves Amplitude Scale", Vector) = (1, 1, 1, 1)
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WaveNoiseMask ("Wind Noise Mask Intensity", Range(0, 1)) = 1
		[HideIfDisabled(_USE_WIND_DEFORMATION)] _WaveNoiseMaskSpeed ("Wind Noise Mask Speed", Range(0, 2)) = 1

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
		Cull Off
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard vertex:SharedVertexFunctionPropDeformable addshadow finalgbuffer:ColorFunctionSliceShading
		#pragma target 5.0
		#pragma instancing_options procedural:setup
		#pragma shader_feature_local _USE_ALPHATEST
		#pragma shader_feature_local _USE_CAR_PARTS_ROTATION
		#pragma shader_feature_local _USE_WIND_DEFORMATION

		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Prop_Shared.cginc"

		float _Cutoff;
		float _DarkenBacksideFaces;

		void surf (Input IN, inout SurfaceOutputStandard output)
		{
			ApplySliceCutoff (IN);
			
            float visibility = IN.packedPropData.x;
            float explosion = IN.packedPropData.y;
			float integrity = IN.packedPropData.w;

			float4 hsbPrimaryProp = fixed4 (0, 0.5, 0.5, 1);
			float4 hsbSecondaryProp = fixed4 (0, 0.5, 0.5, 1);

            // if (explosion > 0.99)
            // {
            //     clip (-1);
            // }
            // else
            // {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
					HalfVector8 packedHSB = hsbData[unity_InstanceID];
					hsbPrimaryProp = packedHSB.UnpackPrimary();
					hsbSecondaryProp = packedHSB.UnpackSecondary();
                #endif

				if (_InstancePropsOverride > 0.0)
				{
					hsbPrimaryProp = _HSBOffsetsPrimary;
					hsbSecondaryProp = _HSBOffsetsSecondary;
					visibility = _PackedPropData.x;
					explosion = _PackedPropData.y;
					integrity = _PackedPropData.w;
				}

				fixed4 hsbPrimaryAndEmission = hsbPrimaryProp;
				fixed4 hsbSecondaryAndOpacity = hsbSecondaryProp;
				
				fixed hueOffsetPrimary = hsbPrimaryAndEmission.x;
				fixed saturationOffsetPrimary = hsbPrimaryAndEmission.y;
				fixed brightnessOffsetPrimary = hsbPrimaryAndEmission.z;
				fixed emissionMultiplier = hsbPrimaryAndEmission.w;
				
				fixed hueOffsetSecondary = hsbSecondaryAndOpacity.x;
				fixed saturationOffsetSecondary = hsbSecondaryAndOpacity.y;
				fixed brightnessOffsetSecondary = hsbSecondaryAndOpacity.z;

                fixed4 ah = tex2D (_MainTex, IN.uv_MainTex);

                #if _USE_ALPHATEST
                    clip ((ah.w - _Cutoff) - lerp ((1 - _Cutoff + 0.01), saturate (1 - integrity) * 0.6, saturate (integrity * 2)));
                #endif

                fixed4 mseo = tex2D (_MSEO, IN.uv_MainTex);
                fixed3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));

                fixed3 albedoBase = ah.rgb;
                fixed albedoMask = ah.a;
                fixed metalnessFinal = mseo.x;
                fixed smoothness = mseo.y;
                fixed emission = mseo.z;
                fixed occlusion = mseo.w;
                fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);

                float albedoMaskMinToMed = saturate (albedoMask * 2);
                float albedoMaskMedToMax = saturate (albedoMask - 0.5) * 2;

				// The values here are not arbitrary - we 'deep fry' the hue mask a bit to get rid of a transitional line
				// between two color areas that gets in there because of texture filtering. Default albedo color for the paint is
				// dark red, so it's important to hide it on any custom paint scheme
				//float albedoMaskMinToMed = saturate (saturate (albedoMask - 0.5) * 32);
				//float albedoMaskMedToMax = saturate (saturate (albedoMask - 0.5) * 64);
				
				// Props use old HSV offsets method
				float3 albedoFinal = RGBTweakHSVOld
				(
					albedoBase,
					albedoMaskMinToMed,
					albedoMaskMedToMax,
					hueOffsetPrimary,
					saturationOffsetPrimary,
					brightnessOffsetPrimary,
					hueOffsetSecondary,
					saturationOffsetSecondary,
					brightnessOffsetSecondary,
					occlusion
				);

				// Option to darken backside faces. Introduced for debris piles using cutout alpha on geometry edges.
				// Since that leads to exposure of inner polygons it might be a good idea to darken them (i.e. use 0 for albedo)
				albedoFinal = lerp (0, albedoFinal, saturate (saturate (IN.facingSign) + (1 - _DarkenBacksideFaces)));

                float smoothnessMaskMinToMed = saturate (smoothness * 2);
                float smoothnessMaskMedToMax = saturate (smoothness - 0.5) * 2;
                float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax);

                float3 emissionFinal = albedoFinal * _EmissionColor * _EmissionIntensity * emission * emissionMultiplier;
                float occlusionFinal = lerp (1.0f, occlusion, _OcclusionIntensity);

            	// This is a very weird macro, it only works if you declare INTERNAL_DATA and worldNormal in surface input
            	float3 normalFinalWorld = WorldNormalVector (IN, normalFinal);

				float verticalFactor = saturate (normalFinalWorld.y);
				verticalFactor = pow(verticalFactor, 2);

            	// frustratingly, absolutely everything related to this calculation gets compiled out if we don't pipe it into output
				smoothnessFinal = lerp (verticalFactor, smoothnessFinal, 0.9999);
            	
				if (_WeatherMultiplier > 0)
				{
					float weatherOcclusionMask = lerp (1.0f, pow (occlusionFinal, _WeatherOcclusionMaskPower), _WeatherOcclusionIntensity);
					ApplyWeather (_WeatherMultiplier, albedoFinal.xyz, smoothnessFinal, metalnessFinal, normalFinal, IN.worldPos1, 1, verticalFactor, verticalFactor, weatherOcclusionMask);
				}

                output.Albedo = albedoFinal.xyz;
                output.Normal = normalFinal;
                output.Metallic = metalnessFinal;
                output.Smoothness = smoothnessFinal;
                output.Emission = emissionFinal;
                output.Occlusion = occlusionFinal;

                float opacity = saturate (visibility * 1.001) * (1 - explosion);
                float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
                clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);

				// output.Albedo = saturate (output.Albedo + float3 (1 - visibility, 0, 0));
				// output.Albedo = saturate (output.Albedo + float3 (0, explosion, 0));
            // }
		}
		ENDCG
	}

}
