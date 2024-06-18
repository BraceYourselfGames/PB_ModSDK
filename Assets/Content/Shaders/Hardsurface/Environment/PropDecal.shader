Shader "Hardsurface/Prop/Decal" 
{
	Properties 
	{
		_InstancePropsOverride ("Instanced properties override", Range (0, 1)) = 0
		_HSBOffsetsPrimary ("HSB offsets, emission toggle", Vector) = (0, 0.5, 0.5, 1)
		_PackedPropData ("Visibility, explosion, compression, integrity", Vector) = (1, 0, 0, 1)

		_MainTex ("Albedo Tex", 2D) = "white" {}
		_AlbedoTint ("Albedo Tint", Color) = (1, 1, 1, 1)
		_MSEO ("MSEO", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}

		_Cutoff ("Alpha cutoff", Range (0, 1)) = 0.5
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1

		_SmoothnessMin ("Smoothness (min.)", Range (0, 1)) = 0.0
		_SmoothnessMed ("Smoothness (med.", Range (0, 1)) = 0.2
		_SmoothnessMax ("Smoothness (max.", Range (0, 1)) = 0.8

		_Scale ("Scale", Vector) = (1, 1, 1, 1)
		_CrushParameters ("Crush pos. (XY), add. (Z), mul. (W)", Vector) = (0, 0, 0, 1)
		_CrushIntensity ("Crush intensity", Range (0, 1)) = 0

		[Space(20)]
		_WeatherMultiplier ("Weather Multiplier", Range (0, 1)) = 1
	}

	SubShader 
	{
		Tags { "RenderType" = "AlphaTest" "Queue" = "Transparent" }
		
		ZWrite Off
		Cull Off
		Offset -1, -1
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard vertex:SharedVertexFunctionProp addshadow
		#pragma target 5.0
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Prop_Shared.cginc"
		#pragma instancing_options procedural:setup

		float3 _AlbedoTint;
		float _Cutoff;

		void surf (Input IN, inout SurfaceOutputStandard output) 
		{
            float4 hsbPrimaryProp = fixed4 (0, 0.5, 0.5, 1);
			float visibility = IN.packedPropData.x;
            float explosion = IN.packedPropData.y;
			float integrity = IN.packedPropData.w;

			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				HalfVector8 packedHSB = hsbData[unity_InstanceID];
				hsbPrimaryProp = packedHSB.UnpackPrimary();
			#endif

			if (_InstancePropsOverride > 0.0)
			{
				hsbPrimaryProp = _HSBOffsetsPrimary;
				visibility = _PackedPropData.x;
				explosion = _PackedPropData.y;
				integrity = _PackedPropData.w;
			}

            if (explosion > 0.99)
            {
                clip (-1);
            }
            else
            {
				float4 albedoBase = tex2D (_MainTex, IN.uv_MainTex);
				albedoBase.rgb *= _AlbedoTint;

				fixed hueOffsetPrimary = hsbPrimaryProp.x;
				fixed saturationOffsetPrimary = hsbPrimaryProp.y;
				fixed brightnessOffsetPrimary = hsbPrimaryProp.z;
				fixed emissionMultiplier = hsbPrimaryProp.w;

				float3 albedoFinal = RGBAdjustWithHSV(albedoBase, hueOffsetPrimary, saturationOffsetPrimary, brightnessOffsetPrimary);

				float4 mseo = tex2D (_MSEO, IN.uv_MainTex);
				float3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));

				float metalness = mseo.x;
				float smoothness = mseo.y;
				float emission = mseo.z;
				float occlusion = mseo.w;

				float3 normalFinal = lerp (float3 (0, 0, 1), nrm, _NormalIntensity);

				float smoothnessMaskMinToMed = saturate (smoothness * 2);
				float smoothnessMaskMedToMax = saturate (smoothness - 0.5) * 2;
				float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax);
				float occlusionFinal = occlusion;

				if (_WeatherMultiplier > 0)
				{
					float verticalFactor = 1;
					float weatherOcclusionMask = 1;
					ApplyWeather (_WeatherMultiplier, albedoFinal.xyz, smoothnessFinal, metalness, normalFinal, IN.worldPos1, 1, verticalFactor, verticalFactor, weatherOcclusionMask);
				}

				output.Albedo = albedoFinal;
				output.Normal = normalFinal;
				output.Metallic = metalness;
				output.Smoothness = smoothnessFinal;
				output.Occlusion = occlusionFinal;
				
				clip ((albedoBase.w - _Cutoff) - lerp ((1 - _Cutoff + 0.01), saturate (1 - integrity) * 0.6, saturate (integrity * 2)));

                float opacity = saturate (visibility * 1.001) * (1 - explosion);
               	float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
              	clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);
			}
		}
		ENDCG
	}

}