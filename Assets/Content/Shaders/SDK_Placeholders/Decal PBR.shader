Shader "Knife/Decals/PBR"
{
	Properties
	{
		[Header (Use this switch for UI decals)] [Space(5)]
		[Toggle (USE_ALBEDO_AND_EMISSION_ONLY)] _UseAlbedoAndEmissionOnly ("Use Diffuse and Emission Only", Float) = 0

		[Header (Color and Tint)] [Space(5)]
		_Color("Color", Color) = (1,1,1,1)
		[PerRendererData] _Tint("Tint", Color) = (1,1,1,1)
		[PerRendererData] _UV("UV", Vector) = (1,1,0,0)
		_TintValueShift("Tint Value Shift", Range(0, 1)) = 0.0

		[Header (Diffuse and Detail Textures)] [Space(5)]
		_MainTex ("Diffuse Map", 2D) = "white" {}
		_DetailTex ("Detail Map", 2D) = "white" {}
		_DetailSettingsBlendMode ("Detail Blend Mode (multiply/add)", Range (0, 1)) = 0
		_DetailSettingsIntensity ("Detail Intensity", Range (0, 1)) = 0

		[Header (Emission)] [Space(5)]
		[HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
		[NoScaleOffset]_EmissionMap ("Emission Map", 2D) = "white" {}
		_EmissionFromMain("Emission from main color", Range(0, 1)) = 0
		
		[Header (Normals)] [Space(5)]		
		[HideIfEnabled(USE_ALBEDO_AND_EMISSION_ONLY)] [NoScaleOffset]_BumpMap ("Normals Map", 2D) = "bump" {}
		[HideIfEnabled(USE_ALBEDO_AND_EMISSION_ONLY)] _NormalScale("Normal Scale", Float) = 1.0
		[HideIfEnabled(USE_ALBEDO_AND_EMISSION_ONLY)] _BlendNormals("Blend normals", Range(0, 1)) = 1.0

		[Header (Specular and Smoothness)] [Space(5)]	
		[HideIfEnabled(USE_ALBEDO_AND_EMISSION_ONLY)] [NoScaleOffset]_SpecularMap ("Specular Map", 2D) = "black" {}
		[HideIfEnabled(USE_ALBEDO_AND_EMISSION_ONLY)] _Smoothness("Smoothness", Range(0, 1)) = 1.0

		[Header (Weather Based Brighness)] [Space(5)]
		[Toggle (USE_TOD_BASED_BRIGHTNESS)] _UseTODBasedBrightness ("Use Weather Based Brightness", Float) = 0
		[HideIfDisabled(USE_TOD_BASED_BRIGHTNESS)] _TODBasedBrightnessNight ("Brightness At Night", Range(0, 2)) = 1.0
		[HideIfDisabled(USE_TOD_BASED_BRIGHTNESS)] _TODBasedBrightnessNoon ("Brightness At Noon", Range(0, 2)) = 1.0
		[HideIfDisabled(USE_TOD_BASED_BRIGHTNESS)] _BrightnessDuringPrecipitation ("Brightness Override During Precipitation", Range(0, 2)) = 0.25
		
		[Header (Polar UVs)] [Space(5)]
		[Toggle (USE_POLAR_UV)] _UsePolarUVMode ("Use Polar UVs", Float) = 0
		[BlockInfo(0.5, 0.5, 1, 1)] dummy_info_0 ("When enabled - You can choose to use planar UVs and use some features available with polar coordinates (UV stretch, etc.).", Float) = 0
		[HideIfDisabled(USE_POLAR_UV)] _PolarUVMode ("Switch between Planar/Polar UV for UI Decals", Range (0, 1)) = 0
		[HideIfDisabled(USE_POLAR_UV)] _PolarUVModeDetailMap ("Polar UV Mode for Detail Map", Range (0, 1)) = 0
		[HideIfDisabled(USE_POLAR_UV)] _FillAmount ("Fill amount", Range (0, 1)) = 1
		[HideIfDisabled(USE_POLAR_UV)] _FillOffset ("Fill offset", Range (0, 1)) = 0
		[HideIfDisabled(USE_POLAR_UV)] _FillUVStretch ("Fill UV stretch", Range (0, 1)) = 0
		[HideIfDisabled(USE_POLAR_UV)] _FillSettingsRadial ("Fill radial (from/to, transition in/out)", Vector) = (0, 1, 0, 0)
		[HideIfDisabled(USE_POLAR_UV)] _FillLinesRadial ("Fill lines (multiplier, -, range opacity, lines opacity)", Vector) = (300, 0, 1, 0)

		[Header (Smooth radial gradient    Requires Polar UVs)] [Space(5)]
		[Toggle (FILL_RADIAL_SMOOTH)] _FillRadialSmooth ("Use Smooth Radial Gradient", Float) = 0
		[HideIfDisabled(FILL_RADIAL_SMOOTH)] _FillRadialTransitionPower ("Fill Gradient Power", Float) = 1

		[Header (Shadows for UI Decals    Requires Polar UVs)] [Space(5)]
		[Toggle (USE_SHADOWS)] _UseShadows ("Use Shadows", Float) = 0
		[HideIfDisabled(USE_SHADOWS)] _ShadowTex ("Shadow Map", 2D) = "white" {}
		[HideIfDisabled(USE_SHADOWS)] _ShadowSettings ("Shadow sampling settings", Vector) = (0, 1, 0, 0)

		[Header (Use Scanlines for UI Decals   Requires Polar UVs)] [Space(5)]
		[Toggle (USE_SCANLINES)] _UseScanlines ("Use Scanlines", Float) = 0
		[HideIfDisabled(USE_SCANLINES)] _ScanlinesSpeed ("Scanlines Speed", Range(0, 2)) = 0.5
		[HideIfDisabled(USE_SCANLINES)] _ScanlinesIntensity ("Scanlines Intensity", Range(0, 2)) = 0.5

		[Header (Other Toggles)] [Space(5)]
		[Toggle(NOEXCLUSIONMASK)] _NoExclusionMask ("Ignore Exclusion Mask", Float) = 0
		[Space(5)]
		[Toggle(HIGHLIGHT_WALLS_ON_UI_DECALS)] _HighlightWallsOnUIDecals ("Highlight Walls on UI Decals", Float) = 0
		[Toggle(NORMAL_CLIP)] _NormalBlendOrClip ("Clip by Normals", Float) = 0
		_ClipNormals("Clip normals", Range(0, 1)) = 0.1
		[Toggle(NORMAL_EDGE_BLENDING)] _NormalEdgeBlending ("Normal Edge Blending", Float) = 0
		[Toggle(NORMAL_MASK)] _NormalsMask ("Normal Mask", Float) = 0
		//[Toggle(TERRAIN_DECAL)] _TerrainDecal ("Terrain Decal", Float) = 0
		//[HideIfDisabled(TERRAIN_DECAL)] _TerrainClipHeight("Terrain height clip", Range(-0.01, 0.01)) = 0.001
		//[HideIfDisabled(TERRAIN_DECAL)] _TerrainClipHeightPower("Terrain height clip power", Range(0, 15)) = 1
	}
	SubShader
	{
		Tags
		{
			"PreviewType" = "Plane"
		}
		Pass // before lighting OR after transparency (diffuse and ambient light)
		{
			Fog { Mode Off } // no fog in g-buffers pass
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt
			//#pragma multi_compile __ NO_TERRAIN
			#pragma multi_compile __ EXCLUSIONMASK
			#pragma multi_compile __ NOEXCLUSIONMASK
			//#pragma multi_compile __ TERRAIN_DECAL
			//#pragma multi_compile __ MULTI_TERRAIN_DECAL
			//#pragma multi_compile __ NORMAL_CLIP
			#pragma multi_compile __ NORMAL_EDGE_BLENDING
			#pragma multi_compile __ NORMAL_MASK
			//#pragma multi_compile __ UNITY_HDR_ON
			#pragma multi_compile __ PREVIEWCAMERA
			#pragma multi_compile_instancing
			#pragma shader_feature_local USE_SHADOWS
			#pragma shader_feature_local FILL_RADIAL_SMOOTH
			#pragma shader_feature_local USE_ALBEDO_AND_EMISSION_ONLY
			#pragma shader_feature_local USE_POLAR_UV
			//#pragma shader_feature_local USE_TOD_BASED_BRIGHTNESS
			//#pragma shader_feature_local USE_SCANLINES
			//#pragma shader_feature_local HIGHLIGHT_WALLS_ON_UI_DECALS
			
			#include "UnityCG.cginc"
			//#if TERRAIN_DECAL && !NO_TERRAIN
			//	#include "TerrainDecals.cginc"
			//#endif
			#include "UnityStandardUtils.cginc"
			#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
			
			struct appdata
			{
				float3 vertex : POSITION;
				float2 uv : TEXCOORD;
			#if UNITY_VERSION >= 550
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#else
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#endif
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float3 ray : TEXCOORD2;
				half3 orientation : TEXCOORD3;
				half3 orientationX : TEXCOORD4;
				half3 orientationZ : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			UNITY_INSTANCING_BUFFER_START(DecalProps)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
				UNITY_DEFINE_INSTANCED_PROP(float4, _UV)
			UNITY_INSTANCING_BUFFER_END(DecalProps)

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _DetailTex;
			float4 _DetailTex_ST;
			
			float4 _DetailSettings;
			float _DetailSettingsBlendMode;
			float _DetailSettingsIntensity;
			
			float4 _Color;
			float4 _NotInstancedColor;
			float4 _NotInstancedUV;
			sampler2D _BumpMap;
			sampler2D _SpecularMap;
			float4 _EmissionColor;
			sampler2D _EmissionMap;
			float _TintValueShift;
			
			float _EmissionFromMain;
			float _NormalScale;
			float _Smoothness;
			float _BlendNormals;

			float _HighlightWallsOnUIDecals;

			float _NormalBlendOrClip;
			float _ClipNormals;

			float _PolarUVMode;
			float _PolarUVModeDetailMap;
			float _FillAmount;
			float _FillOffset;
			float _FillUVStretch;
			float4 _FillSettingsRadial;
			float4 _FillLinesRadial;
			float _FillRadial;

			uniform float TOD_NightToNoonInterpolant;
			uniform float4 _WeatherParameters;

			uniform float _UseTODBasedBrightness;
			uniform float _TODBasedBrightnessNight;
			uniform float _TODBasedBrightnessNoon;
			uniform float _BrightnessDuringPrecipitation;

			#if USE_SHADOWS
			sampler2D _ShadowTex;
			float4 _ShadowSettings;
			#endif

			//#if USE_SCANLINES
			float _UseScanlines;
			float _ScanlinesSpeed;
			float _ScanlinesIntensity;
			//#endif

			#if FILL_RADIAL_SMOOTH
			float _FillRadialTransitionPower;
			#endif

			float4 _GlobalUnscaledTime;
			

			sampler2D_float _CameraDepthTexture;
			sampler2D _NormalsCopy;
			sampler2D _CameraTargetCopy;
			sampler2D_float _ExclusionMask;
			#include "DecalMain.cginc"

			float RemapToRange (float f, float a1, float a2, float b1, float b2)
		    {
		        float divisor = a2 - a1;
		        if (divisor < 0.001)
		            return f;

				return b1 + (f - a1) * (b2 - b1) / divisor;
		    }

			v2f vert (appdata v)
			{
				v2f o = vertDecal(v);
				return o;
			}
			
			void frag(v2f i, out float4 outDiffuse : COLOR0, out float4 outNormal : COLOR1, out float4 outEmission : COLOR2)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float4 uvInstanced = UNITY_ACCESS_INSTANCED_PROP(DecalProps, _UV);

				#if PREVIEWCAMERA
					float4 diffusePreview = tex2D (_MainTex, i.uv);
					float4 colPreview = diffusePreview;
					outDiffuse = colPreview;
					return;
				#endif
				
				float3 wpos;
				bool needCull;
				float4 meshUVAndScreenUV = fragDecalUV(i, wpos, needCull);

				outDiffuse = 0;
				outNormal = float4 (0, 0, 1, 0);
				outEmission = 0;
	
				#if EXCLUSIONMASK
					if(needCull)
						return;
				#endif

				float2 uv = meshUVAndScreenUV.zw;
				float2 meshUV = meshUVAndScreenUV.xy;

				i.uv = meshUV * _MainTex_ST.xy * (uvInstanced.xy * _NotInstancedUV.xy) + _MainTex_ST.zw + uvInstanced.zw + _NotInstancedUV.zw;

				float2 uvPlanar = i.uv;

				float2 uvFinal = uvPlanar;
				float2 uvFinalDetail = uvPlanar;

				float alphaMultiplierEarly = 1;

				#if USE_POLAR_UV
					/*
					// Currently unused method of deriving X in polar coordinates
					float pi = 3.1415926;
					float2 uvNormalized = normalize (uvTransformed);
					float arctan2 = atan2 (uvNormalized.y, uvNormalized.x);
					float arctan2Scaled = arctan2 / 3.14159;
					arctan2Scaled += 1;
					arctan2Scaled *= 0.5;
					arctan2Scaled = saturate (1 - arctan2Scaled);

					if (arctan2Scaled > _FillAmount)
						return;
					*/

					// Transforming UV from 0, 1 space to -1, -1 space, since we want all math to revolve around the center, not corner
					float2 uvPlanarCentered = uvPlanar * 2 + float2 (-1, -1);

					// 0-1 angle is just one component of polar coordinates - the second one is distance to center, which is easy to derive
					// This is just traditional vector length, or root of square magnitude
					float distanceFromOrigin = sqrt (uvPlanarCentered.x * uvPlanarCentered.x + uvPlanarCentered.y * uvPlanarCentered.y);

					// Arctan2 gives us radians from XY coordinate, but in a bit inconvenient range (-0.5pi to 0.5pi)
					float angle = atan (uvPlanarCentered.y / uvPlanarCentered.x);

					// The following operations give us angle in 0-1 range
					float pi2 = 6.2831853;
					float angleScaled = angle / pi2;
					angleScaled += 0.25;
					if (uvPlanarCentered.x < 0)
						angleScaled += 0.5;

					// This operation rotates the angle around with wrapping, letting us control where our fill effect and UVs terminate
					angleScaled = (angleScaled + _FillOffset) % 1;

					// We can cut if we're out of desired fill amount
					if (angleScaled > _FillAmount)
						return;

					float distanceFromOriginRescaled = distanceFromOrigin;

					#if FILL_RADIAL_SMOOTH
						float fillRadialFrom = _FillSettingsRadial.x;
						float fillRadialTo = _FillSettingsRadial.y;
						float fillRadialTransitionIn = max (_FillSettingsRadial.z, 0.01);
						float fillRadialTransitionOut = max (_FillSettingsRadial.w, 0.01);

						float za = fillRadialTo + fillRadialTransitionOut;
						float de = _FillLinesRadial.x * za;

						float fadeOut = 1 - saturate ((distanceFromOrigin - fillRadialTo + fillRadialTransitionOut) / fillRadialTransitionOut);
						float fadeIn = saturate ((distanceFromOrigin - fillRadialFrom) / fillRadialTransitionIn);
						alphaMultiplierEarly = saturate (pow (fadeIn, _FillRadialTransitionPower) * pow (fadeOut, _FillRadialTransitionPower));
						
						float deltaFrom = distanceFromOrigin - (fillRadialFrom + fillRadialTransitionIn);
						float lineDeltaFrom = saturate (abs (deltaFrom) * de);
						alphaMultiplierEarly *= lerp (1, lineDeltaFrom, _FillLinesRadial.w);
						
						float deltaTo = distanceFromOrigin - (fillRadialTo - fillRadialTransitionOut);
						float lineDeltaTo = saturate (abs (deltaTo) * de);
						alphaMultiplierEarly *= lerp (1, lineDeltaTo, _FillLinesRadial.w);

						float fillRadialRangeMask = 1 - saturate (deltaFrom * _FillLinesRadial.x) * (1 - saturate (deltaTo * _FillLinesRadial.x));
						fillRadialRangeMask = lerp (fillRadialRangeMask, 1, _FillLinesRadial.z);
						alphaMultiplierEarly *= fillRadialRangeMask;

						if (alphaMultiplierEarly < 0.01)
							return;
					#else
						if (_FillSettingsRadial.z > 0.5)
						{
							if (distanceFromOrigin < _FillSettingsRadial.x || distanceFromOrigin > _FillSettingsRadial.y)
								return;

							float distanceFromOriginRemapped = RemapToRange (distanceFromOrigin, _FillSettingsRadial.x, _FillSettingsRadial.y, 0, 1);
							distanceFromOriginRescaled = lerp (distanceFromOrigin, distanceFromOriginRemapped, _FillSettingsRadial.w);
						}
					#endif

					// Saving the polar coordinates for later use
					float2 uvPolar = float2 (angleScaled, distanceFromOrigin);

					// Next, we can scale polar angle by fill amount and shift it
					float angleStretched = angleScaled / _FillAmount;
					float angleShifted = (angleStretched + _FillOffset) % 1;

					// Saving the polar coordinates for later use
					float2 uvPolarStretched = float2 (angleShifted, distanceFromOriginRescaled);

					// Next we can start reverting all operations done to polar angle, returning it to useful range for deriving coords
					float angleReverted = angleShifted;
					if (uvPlanarCentered.x < 0)
						angleReverted -= 0.5;
					angleReverted -= 0.25;
					angleReverted *= pi2;

					// Next we derive XY coordinates back from reverted, now distorted, angle
					float xReverted = distanceFromOrigin * sin (angleReverted);
					float yReverted = distanceFromOrigin * -cos (angleReverted);

					// Returning to 0, 1 space from -1, -1 space
					xReverted = (xReverted + 1) * 0.5;
					yReverted = (yReverted + 1) * 0.5;

					// Generating new stretched UVs
					float2 uvPlanarStretched = float2 (xReverted, yReverted);
					float2 uvPlanarFinal = lerp (uvPlanar, uvPlanarStretched, _FillUVStretch);
					float2 uvPolarFinal = lerp (uvPolar, uvPolarStretched, _FillUVStretch);

					uvFinal = lerp (uvPlanarFinal, uvPolarFinal, _PolarUVMode);
					uvFinalDetail = lerp (uvPlanar, uvPolar, _PolarUVModeDetailMap);

				#endif

				float4 diffuse = tex2D (_MainTex, uvFinal);
				float4 col = diffuse * _Color;

				float2 detailUV = uvFinalDetail * _DetailTex_ST.xy * _DetailTex_ST.zw;
				float4 detail = tex2D (_DetailTex, detailUV);
				float alphaDetailed = lerp (col.a * detail.a, saturate (col.a + detail.a), _DetailSettingsBlendMode);
				col.a = lerp (col.a, alphaDetailed, _DetailSettingsIntensity);

				col.a *= alphaMultiplierEarly;
				
				col *= _NotInstancedColor;
				col *= UNITY_ACCESS_INSTANCED_PROP(DecalProps, _Tint);

				//#if USE_TOD_BASED_BRIGHTNESS
				if (_UseTODBasedBrightness > 0)
				{
					float nightToNoonInterpolantBoost = 1.5f; // keep the decal at maximum brightness for longer, so it doesn't only peak at noon
					float brightnessBasedOnTOD = lerp (_TODBasedBrightnessNight, _TODBasedBrightnessNoon, saturate (pow (TOD_NightToNoonInterpolant * nightToNoonInterpolantBoost, 2)));

					// Rain Intensity + Snowfall Intensity
					float precipitationIntensity = saturate ((_WeatherParameters.x + _WeatherParameters.z) * 1.2f);
					float brightnessBasedOnPrecipitation = _BrightnessDuringPrecipitation;

					// Precipitation brightness overrides TOD-based brightness
					col.rgb *= lerp (brightnessBasedOnTOD, brightnessBasedOnPrecipitation, precipitationIntensity);
				}
				//#endif

				// Normals to calculate normal-based clipping
				float3 normal = tex2D(_NormalsCopy, uv).rgb;
				float3 wnormal = normal.rgb * 2.0 - 1.0;
				float blendByNormal = 1;
				// Commented out normal dependent blending for now
				//#if NORMAL_CLIP
				if (_NormalBlendOrClip > 0)
					clip (dot(wnormal, i.orientation) - _ClipNormals);
				//#else
					// float dotResult = dot(wnormal, i.orientation);
					// blendByNormal = smoothstep(0, _ClipNormals, dotResult);
				//#endif

				#if USE_ALBEDO_AND_EMISSION_ONLY
				if (_HighlightWallsOnUIDecals > 0)
					col.rgb += col.rgb * (1 - saturate (wnormal.y)) * (1 - saturate (wpos.y / 15));
				#endif

				col.a *= blendByNormal;

				#ifndef USE_ALBEDO_AND_EMISSION_ONLY
					// Tangent space normals
					float3 nor = UnpackScaleNormal(tex2D(_BumpMap, uvFinal), _NormalScale);
					float3x3 norMat = float3x3(i.orientationX, i.orientationZ, i.orientation);
					nor = mul (nor, norMat);
					float4 normalResult;
					normalResult.xyz = lerp(nor, wnormal + nor, 1 - _BlendNormals);
					normalResult.xyz = normalize(normalResult.xyz);

					float normalBlendFactor;

					#if NORMAL_EDGE_BLENDING
						#if NORMAL_MASK
							normalBlendFactor = diffuse.a;
						#else
							float dist = distance(meshUV, float2(.5, .5)) * 2;
							dist = pow(dist, 3);
							dist = clamp(dist, 0, 1);
							normalBlendFactor = (1 - dist);
						#endif
					#else
						normalBlendFactor = (_BlendNormals) * col.a;
					#endif

					normalResult = float4(normalResult.xyz * 0.5f + 0.5f, normalBlendFactor);

					// !! ShadeSH9 does not work reliably when executed after transparency pass
					// UI decals are scheduled to render after transparency, so make sure you use 'Diffuse and Emission Only' mode on those
					float3 shColor = ShadeSH9(float4(nor.xyz, 1));
					col.rgb = shColor.rgb * col.rgb;

					outNormal = normalResult;
				#endif

				#if USE_POLAR_UV				
					#if USE_SHADOWS
						// Polar UV with circumference-wise remapping but without radius-wise remapping
						float2 shadowUV = float2 (angleShifted, distanceFromOrigin);
						float shadowValue = tex2D (_ShadowTex, shadowUV).x;

						// float shadowValue1 = tex2D (_ShadowTex, float2 (angleShifted - 0.015, distanceFromOrigin)).x;
						// float shadowValue2 = tex2D (_ShadowTex, float2 (angleShifted + 0.015, distanceFromOrigin)).x;
						// shadowValue = 0.333 * (shadowValue + shadowValue1 + shadowValue2);
					
						// Instead of hard cutoff, it's better to just smoothly modify alpha (with gradient controlled by offset and multiplier)
						float shadowDiff = (shadowValue.x - distanceFromOrigin + _ShadowSettings.x) * _ShadowSettings.y;
						shadowDiff = saturate (shadowDiff);

						// Blend shadow in with configurable intensity
						float shadowIntensity = saturate (_ShadowSettings.z);
						col.a = lerp (col.a, col.a * shadowDiff, shadowIntensity);
					#endif

					//#if USE_SCANLINES
					if (_UseScanlines > 0)
					{
						float scanlinePing_dir1 = 1 - frac (uvPolarFinal.x + _GlobalUnscaledTime.y * _ScanlinesSpeed);
						float scanlinePing_dir2 = frac (uvPolarFinal.x - _GlobalUnscaledTime.y * _ScanlinesSpeed);
						float3 scanlinePingCombined = col.rgb * (scanlinePing_dir1 + scanlinePing_dir2) * _ScanlinesIntensity;
						#if USE_SHADOWS
							scanlinePingCombined *= shadowDiff;
						#endif
						col.rgb += scanlinePingCombined;
					}
					//#endif
				#endif

				float blendTerrains = 1;
				//#if TERRAIN_DECAL && !NO_TERRAIN
				//	blendTerrains = GetTerrainBlending(wpos);
				//	col.a *= blendTerrains;
				//#endif

				outDiffuse = col;

				//#ifndef UNITY_HDR_ON
				//	col.rgb = exp2(-col.rgb);
				//#endif
				
				outEmission = float4(col.rgb + (tex2D (_EmissionMap, i.uv) * _EmissionColor).rgb * col.a, col.a);

				outEmission = lerp (outEmission, outDiffuse, _EmissionFromMain);
				outDiffuse.xyz = lerp (outDiffuse.xyz, float3 (0, 0, 0), _EmissionFromMain);

				float colMax = saturate (pow (diffuse.a, 8) * _TintValueShift);
				outEmission.rgb = lerp (outEmission.rgb, float3 (1, 1, 1), colMax);		
			}
			ENDCG
		}
		
		Pass// Editor Only - SELECTION
		{
			Fog { Mode Off } // no fog in g-buffers pass
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt
			//#pragma multi_compile __ TERRAIN_DECAL
			//#pragma multi_compile __ MULTI_TERRAIN_DECAL
			#pragma multi_compile __ NORMAL_EDGE_BLENDING
			#pragma multi_compile __ NORMAL_MASK
			
			#include "UnityCG.cginc"
			//#if TERRAIN_DECAL && !NO_TERRAIN
			//	#include "TerrainDecals.cginc"
			//#endif
			#include "UnityStandardUtils.cginc"
			
			#include "AutoLight.cginc"
			#include "UnityPBSLighting.cginc"

			struct appdata
			{
				float3 vertex : POSITION;
			#if UNITY_VERSION >= 550
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#else
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#endif
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float3 ray : TEXCOORD2;
				half3 orientation : TEXCOORD3;
				half3 orientationX : TEXCOORD4;
				half3 orientationZ : TEXCOORD5;
				float3 worldScale  : TEXCOORD6;
			};
			
			sampler2D _MainTex;
			sampler2D _BumpMap;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _NotInstancedColor;
			float4 _NotInstancedUV;
			float _NormalScale;
			sampler2D _BaseColorCopy;

			sampler2D_float _CameraDepthTexture;
			sampler2D_float _ExclusionMask;
			sampler2D _NormalsCopy;

			float SelectionTime;

			#include "DecalMain.cginc"
			
			v2f vert (appdata v)
			{
				v2f o = vertDecal(v);
				o.worldScale = float3(
					length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
					length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
					length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
				);
				return o;
			}

			float GetAlphaInPixel(float2 uv, float2 deltaUV)
			{
				if(_Color.a > 0)
				{
					fixed4 col10 = tex2D (_MainTex, clamp(uv + deltaUV, float2(0,0), _MainTex_ST.xy + _MainTex_ST.zw)) * _Color * _NotInstancedColor;

					return col10.a;
				} else
				{
					fixed4 col10 = tex2D (_MainTex, clamp(uv + deltaUV, float2(0,0), _MainTex_ST.xy + _MainTex_ST.zw));

					return 1 - col10.a;
				}
			}

			float4 CalcSelectionColor(float2 uv, float alpha, float2 deltaUV)
			{
				float alphaInRightPixel = GetAlphaInPixel(uv, float2(deltaUV.x, 0));
				float alphaInLeftPixel = GetAlphaInPixel(uv, float2(-deltaUV.x, 0));
				float alphaInUpPixel = GetAlphaInPixel(uv, float2(0, deltaUV.y));
				float alphaInBottomPixel = GetAlphaInPixel(uv, float2(0, -deltaUV.y));

				bool isOutlinedPixel = (alphaInRightPixel < 0.2 || alphaInLeftPixel < 0.2 || alphaInUpPixel < 0.2 || alphaInBottomPixel < 0.2);
				
				//float3 color = float3(255.0f / 255.0f, 102.0f / 255.0f, 0.0f / 255.0f); // orange
				float3 color = float3(0.0f / 255.0f, 174.0f / 255.0f, 239.0f / 255.0f); // blue
				return float4(color,isOutlinedPixel * pow(alpha, 0.5));//float4(color, alphaInRightPixel);
			}
			
			// return value is emission
			float4 frag(v2f i) : SV_TARGET
			{
				float3 wpos;
				float4 meshUVAndScreenUV = fragDecalUV(i, wpos);
				float2 uv = meshUVAndScreenUV.zw;
				float2 meshUV = meshUVAndScreenUV.xy;
				i.uv = meshUV * _MainTex_ST.xy * _NotInstancedUV.xy + _MainTex_ST.zw + _NotInstancedUV.zw;
				float notLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);

				half3 normal = tex2D(_NormalsCopy, uv).rgb;
				fixed3 wnormal = normal.rgb * 2.0 - 1.0;
				clip (dot(wnormal, i.orientation) - 0.1);
				
				fixed4 diffuse = tex2D (_MainTex, i.uv);
				fixed4 col = diffuse * _Color * _NotInstancedColor;
				float blendTerrains = 1;
				//#if TERRAIN_DECAL && !NO_TERRAIN
				//	blendTerrains = GetTerrainBlending(wpos);
				//	col.a *= blendTerrains;
				//#endif
				
				fixed4 currentEmission = tex2D(_BaseColorCopy, uv);

				float4 selectionColor = float4(0,0,0,0);
				float scaleLength = length(i.worldScale.xz);
				scaleLength *= scaleLength;

				float alpha = 1;
				#if NORMAL_EDGE_BLENDING
					#if NORMAL_MASK
						alpha = diffuse.a;
					#else
						float3 nor = UnpackScaleNormal(tex2D(_BumpMap, i.uv), _NormalScale);
						float3x3 norMat = float3x3(i.orientationX, i.orientationZ, i.orientation);
						//nor = mul (nor, norMat);
						alpha = 1 - dot(nor, float3(0.5, 0.5, 1));
						alpha = clamp(alpha, 0, 1);
						/*float dist = distance(meshUV, float2(.5, .5)) * 2;
						dist = pow(dist, 3);
						dist = clamp(dist, 0, 1);
						alpha = (1 - dist) * 0.01;*/
					#endif
				#else
					alpha = col.a;
				#endif

				selectionColor = CalcSelectionColor(i.uv, alpha, float2(0.0002 / notLinearDepth / scaleLength, 0.0002 / notLinearDepth / scaleLength)) * 2 * SelectionTime;
				//return selectionColor;

				return float4(selectionColor.rgb, diffuse.a * SelectionTime * selectionColor.a);
			}
			ENDCG
		}		
		
		Pass // before lighting (spec blend)
		{
			Fog { Mode Off } // no fog in g-buffers pass
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt
			//#pragma multi_compile __ NO_TERRAIN
			#pragma multi_compile __ EXCLUSIONMASK
			#pragma multi_compile __ NOEXCLUSIONMASK
			//#pragma multi_compile __ TERRAIN_DECAL
			//#pragma multi_compile __ MULTI_TERRAIN_DECAL
			//#pragma multi_compile __ NORMAL_CLIP
			#pragma multi_compile __ NORMAL_EDGE_BLENDING
			#pragma multi_compile __ NORMAL_MASK
			#pragma multi_compile_instancing
			
			#include "UnityCG.cginc"
			//#if TERRAIN_DECAL && !NO_TERRAIN
			//	#include "TerrainDecals.cginc"
			//#endif
			#include "UnityStandardUtils.cginc"

			struct appdata
			{
				float3 vertex : POSITION;
			#if UNITY_VERSION >= 550
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#else
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#endif
			};

			UNITY_INSTANCING_BUFFER_START(DecalProps)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Tint)
				UNITY_DEFINE_INSTANCED_PROP(float4, _UV)
			UNITY_INSTANCING_BUFFER_END(DecalProps)

			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float3 ray : TEXCOORD2;
				half3 orientation : TEXCOORD3;
				half3 orientationX : TEXCOORD4;
				half3 orientationZ : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _NotInstancedColor;
			float4 _NotInstancedUV;
			sampler2D _SpecularMap;

			float _NormalBlendOrClip;
			float _ClipNormals;
			float _BlendNormals;

			float _Smoothness;

			sampler2D_float _CameraDepthTexture;
			sampler2D_float _ExclusionMask;
			sampler2D _NormalsCopy;
			sampler2D _SpecRoughnessCopy;
			#include "DecalMain.cginc"

			v2f vert (appdata v)
			{
				v2f o = vertDecal(v);
				return o;
			}
			
			void frag(v2f i, out float4 outSpecular : COLOR0, out float4 outSmoothness : COLOR1)
			{
				outSpecular = 0;
				outSmoothness = 0;

				#ifndef USE_ALBEDO_AND_EMISSION_ONLY
					UNITY_SETUP_INSTANCE_ID(i);

					bool needCull;
					
					float3 wpos;
					float4 meshUVAndScreenUV = fragDecalUV(i, wpos, needCull);
					float2 uv = meshUVAndScreenUV.zw;
		
					#if EXCLUSIONMASK
						if(needCull)
							return;
					#endif

					float2 meshUV = meshUVAndScreenUV.xy;
					float4 uvInstanced = UNITY_ACCESS_INSTANCED_PROP(DecalProps, _UV);
					i.uv = meshUV * _MainTex_ST.xy * (uvInstanced.xy * _NotInstancedUV.xy) + _MainTex_ST.zw + uvInstanced.zw + _NotInstancedUV.zw;

					float3 normal = tex2D(_NormalsCopy, uv).rgb;
					float3 wnormal = normal.rgb * 2.0 - 1.0;
					float blendByNormal = 1;
					//#if NORMAL_CLIP
					if (_NormalBlendOrClip > 0)
						clip (dot(wnormal, i.orientation) - _ClipNormals);
					//#else
						// float dotResult = dot(wnormal, i.orientation);
						// blendByNormal = smoothstep(0, _ClipNormals, dotResult);
					//#endif
					
					float4 diffuse = tex2D (_MainTex, i.uv);
					float4 col = diffuse * _Color * _NotInstancedColor;
					col.a *= blendByNormal;
					col *= UNITY_ACCESS_INSTANCED_PROP(DecalProps, _Tint);
					float blendTerrains = 1;
					//#if TERRAIN_DECAL && !NO_TERRAIN
					//	blendTerrains = GetTerrainBlending(wpos);
					//	col.a *= blendTerrains;
					//#endif
					
					float normalBlendFactor;

					#if NORMAL_EDGE_BLENDING
						#if NORMAL_MASK
							normalBlendFactor = diffuse.a;
						#else
							float dist = distance(meshUV, float2(.5, .5)) * 2;
							dist = pow(dist, 3);
							dist = clamp(dist, 0, 1);
							normalBlendFactor = (1 - dist);
						#endif
					#else
						normalBlendFactor = (_BlendNormals) * col.a;
					#endif
					
					float4 currentSpecRoughness = tex2D(_SpecRoughnessCopy, uv);
					float4 spec = tex2D (_SpecularMap, i.uv);
					spec.a *= _Smoothness * normalBlendFactor;

					outSpecular = float4(spec.rgb, normalBlendFactor);
					outSmoothness = float4(spec.a, 0, 0, normalBlendFactor);
				#endif
			}
			ENDCG
		}

	}

	Fallback Off
}
