// Upgrade NOTE: upgraded instancing buffer 'FoliageInstance' to new syntax.

Shader "Vegetation/Main foliage shader (custom)"
{
	Properties
	{
		[Space (12)]
		[Enum (UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0

		[Header (Base Settings)]
		[Space (12)]
		_Color ("Color (A - Biome Color Influence)", Color) = (1,1,1,1)
		_ColorVariation ("Color Variation (A - Variation Intensity)", Color) = (1,1,1,0)
		_RimFadeMultiplier ("Rim fade multiplier", Range (0,1)) = 0
		_MainTex ("Albedo (RGB), opacity (A)", 2D) = "white" {}
		_CutoffCustom ("Alpha cutoff (custom)", Range (0,1)) = 0.3
		_CutoffShadow ("Alpha cutoff (shadow)", Range (0,1)) = 0.5
		_Cutoff ("Alpha cutoff (fallback)", Range (0,1)) = 0.3
		_PackedPropData ("Visibility, explosion, compression, bending deadzone height", Vector) = (0, 0.5, 0.5, 1)

		[NoScaleOffset] _BumpTransSpecMap ("Trans. (R), norm. (GA), sm. (B)", 2D) = "bump" {}
		[BlockInfo(0.5, 0.5, 1, 1)] dummy_info_0 ("Modify normal for backfaces - always disable it on vegetation with custom normals", Float) = 0
		_ModifyNormalsForBackfaces ("Modify normal for backfaces", Range (0.0, 1.0)) = 1.0
		_SmoothnessOverride ("Smoothness override value", Range (0.0, 1.0)) = 0.5
		_SmoothnessOverrideToggle ("Smoothness override toggle", Range (0.0, 1.0)) = 0.0
		_TranslucencyOverride ("Translucency override value", Range (0.0, 1.0)) = 0.5
		_TranslucencyOverrideToggle ("Translucency override toggle", Range (0.0, 1.0)) = 0.0
		_TranslucencyViewDependency ("Translucency View Dependency", Range(0,1)) = 0.8
		
		_RemoveVertexOcclusion ("Remove vertex occlusion", Range (0.0, 1.0)) = 0.0
		_RemoveBendingFlutter ("Remove bending flutter", Range (0, 1)) = 1
		_RemovePhaseDifferences ("Remove phase differences", Range (0, 1)) = 1

		[Space (12)]
		_SpecularReflectivity ("Specular", Color) = (0.2,0.2,0.2)
		_TranslucencyStrength ("Translucency", Range (0,1)) = 0.5
		_BackfaceSmoothness ("Backface smoothness", Range (0,2)) = 1
		_BouncedLighting ("Bounced lighting", Range (0.0, 5.0)) = 0.0
		_HorizonFade ("Horizon fade", Range (0.0, 5.0)) = 1.0
		_FresnelFadeMultiplier ("Fresnel fade multiplier", Range (0, 10)) = 1
		_TerrainGradientParams ("Terrain gradient params (start Y, fade length Y, -, amount)", Vector) = (0, 10, 0, 0)
		_Height ("Fresnel fade multiplier", Range (0, 20)) = 0.05

		[Header (Wind Settings)]
		[Space (12)]
		[KeywordEnum (Legacy Vertex Colors, UV4 And Vertex Colors, Vertex Colors, HeightBased)]
		_BendingControls ("Bending parameters", Float) = 0 // 0 = legacy vertex colors, 1 = uv4, 2 = vertex colors, 3 = height based
		_LeafTurbulence ("Leaf turbulence", Range (0,1)) = 0.2

		_BendingPrimaryBottomHeight ("Prim. bending height start", Range (-9, 9)) = 1
		_BendingPrimaryTopHeight ("Prim. bending height end", Range (0, 30)) = 1
		_BendingPrimaryMultiplier ("Prim. bending multiplier", Range (0, 1)) = 1

		_BendingSecondaryBottomHeight ("Sec. bending height start", Range (-9, 9)) = 1
		_BendingSecondaryTopHeight ("Sec. bending height end", Range (0, 30)) = 1
		_BendingSecondaryMultiplier ("Sec. bending multiplier", Range (0, 1)) = 1

		_BendingDeadzoneTopHeight ("Deadzone height end", Float) = 1
		_BendingDeadzoneReshapingOffset ("Deadzone reshaping offset", Float) = 0.1
		_BendingDeadzoneMultiplier ("Deadzone multiplier", Range (0, 1)) = 1



		[Header (Debug)]
		[Space (12)]
		// [KeywordEnum (Off, VertexRGB, VertexA, WindPhase, WindFlutter, WindPrimary, WindSecondary, WindDeadzone)]
		[KeywordEnum (Off, VertexRGB)]
		_DebugMode ("Debug mode", Float) = 0
		_DebugChannel ("Debug channel (R0 G1 B2 A3)", Range (0, 3)) = 0

		[Header (Rain Detail Settings)]
		[Space (12)]
		[Toggle (EFFECT_BUMP)] _RainDetails ("Enable Rain Details", Float) = 0
		[NoScaleOffset] _RainBumpMask ("Rain Normal (GA) Mask (B)", 2D) = "bump" {}
		_RainTexScale ("Rain Texture Scale", Float) = 4

		[Space (12)]
		[Toggle (GEOM_TYPE_BRANCH)] _Pivots ("Baked Pivots", Float) = 0
		[Toggle (SHADOW_CAPTURE_MODE)] _ShadowCaptureMode ("Shadow capture mode", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest+10"
			"IgnoreProjector" = "True"
			"RenderType" = "AlphaTest"
		}

		LOD 200
		Cull[_Culling]
		ZTest Less

		CGPROGRAM
		#pragma surface surf Standard vertex:vert exclude_path:forward exclude_path:prepass noforwardadd nolppv noshadowmask novertexlights finalgbuffer:ColorFunctionSliceShading //dithercrossfade //finalcolor:FinalColorFunction
		#pragma target 5.0
		#pragma multi_compile_instancing
		#pragma multi_compile _ GEOM_TYPE_BRANCH
		#pragma multi_compile _ SHADOW_CAPTURE_MODE
		#pragma multi_compile _ LOD_FADE_CROSSFADE
		#pragma shader_feature EFFECT_BUMP
		#pragma instancing_options procedural:setup
		
		#include "Assets/Content/Shaders/Hardsurface/Environment/Instancing_Shared.cginc"
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

		sampler2D _MainTex;
		sampler2D _BumpTransSpecMap;

		fixed4 _Color;
		fixed4 _ColorVariation;
		fixed _RimFadeMultiplier;

		fixed3 _SpecularReflectivity;
		fixed _BackfaceSmoothness;
		fixed _CutoffCustom;

		fixed _ModifyNormalsForBackfaces;
		half _SmoothnessOverride;
		half _SmoothnessOverrideToggle;
		half _TranslucencyOverride;
		half _TranslucencyOverrideToggle;
		half _TranslucencyStrength;
		fixed _TranslucencyViewDependency;
		float _Height;

		half _RemoveVertexOcclusion;
		half _RemoveBendingFlutter;
		half _RemovePhaseDifferences;

		half _BendingPrimaryBottomHeight;
		half _BendingPrimaryTopHeight;
		half _BendingPrimaryMultiplier;

		half _BendingSecondaryBottomHeight;
		half _BendingSecondaryTopHeight;
		half _BendingSecondaryMultiplier;

		half _BendingDeadzoneTopHeight;
		half _BendingDeadzoneMultiplier;
		half _BendingDeadzoneReshapingOffset;
		
		float4 _TerrainGradientParams;
		
		half _FresnelFadeMultiplier;
		float _DebugMode;
		float _DebugChannel;

		// Global vars
		float2 _AfsSpecFade;
		float4 _CombatTerrainParamsScale;
		sampler2D _CombatTerrainTexGradient;
		float4 _WeatherParameters;

		float4 _CombatVegetationColor1;
		float4 _CombatVegetationColor2;

		struct Input
		{
			float2 uv_MainTex;
			fixed4 color : COLOR0;
			float4 vertex;
			float3 worldPos1;
			//float3 worldNormal1;
			float3 worldNormal;
			float3 worldCameraDir;
			float facingSign : VFACE;
			float3 viewDir;
			float4 animParams;
			float animDeadzone;
			fixed4 vertexColorUnmodified;
			float4 screenPos;
			float4 colorFromGradient;
			INTERNAL_DATA
			// UNITY_DITHER_CROSSFADE_COORDS
		};

		float4 _GlobalEnvironmentSliceInputs;
		float4 _GlobalEnvironmentSliceColor;

		#pragma multi_compile __ _USE_SLICE_SHADING
		#pragma multi_compile __ _USE_SLICE_CUTOFF

		void ColorFunctionSliceShading
		(
			Input IN,
			SurfaceOutputStandard surf,
			inout half4 outDiffuse : SV_Target0,          // RT0: diffuse color (rgb), occlusion (a)
			inout half4 outSpecSmoothness : SV_Target1,   // RT1: spec color (rgb), smoothness (a)
			inout half4 outNormal : SV_Target2,           // RT2: normal (rgb), --unused, very low precision-- (a)
			inout half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
		)
		{
			#ifdef _USE_SLICE_SHADING

			float y = IN.worldPos1.y;
			float fadeHeight = _GlobalEnvironmentSliceInputs.x;
			float fadeLength = max (0.1, _GlobalEnvironmentSliceInputs.y);
			float glowLength = max (0.1, _GlobalEnvironmentSliceInputs.z);

			float fade = 1;
			float glow = 0;

			if (_GlobalEnvironmentSliceInputs.w < 0.5)
			{
				fade = 1 - saturate ((y - fadeHeight + fadeLength) / fadeLength);
				glow = saturate ((y - fadeHeight + glowLength) / glowLength);
			}
			else
			{
				fade = saturate ((y - fadeHeight) / fadeLength);
				glow = 1 - saturate ((y - fadeHeight) / glowLength);
			}

			fade = lerp (1, fade, saturate ((_GlobalEnvironmentSliceInputs.y - 0.01) * 100));
			glow *= saturate (max (0, _GlobalEnvironmentSliceInputs.z - 0.01) * 100);
			glow = pow (glow, 2);
 
			outDiffuse *= fade;
			outEmission *= fade;
			outSpecSmoothness *= fade;
			outEmission.xyz += _GlobalEnvironmentSliceColor.xyz * (glow * _GlobalEnvironmentSliceColor.w);
			
			#endif
		}

		inline void ApplySliceCutoff (Input IN)
		{
			#ifdef _USE_SLICE_CUTOFF
			        
			float fadeHeight = _GlobalEnvironmentSliceInputs.x; 
			float y = IN.worldPos1.y;

			if (_GlobalEnvironmentSliceInputs.w < 0.5)
				clip (fadeHeight - y);
			else
				clip (y - fadeHeight);

			#endif
		}

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);
			UNITY_SETUP_INSTANCE_ID(v);

			o.vertex = v.vertex;
			float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;

			float bendingLerpFactorPrimary = saturate ((v.vertex.y - _BendingPrimaryBottomHeight) / _BendingPrimaryTopHeight) * _BendingPrimaryMultiplier;
			float bendingLerpFactorSecondary = saturate ((v.vertex.y - _BendingSecondaryBottomHeight) / _BendingSecondaryTopHeight) * _BendingSecondaryMultiplier;
			float2 bendingHeightBased = float2 (lerp (0, 1, bendingLerpFactorPrimary), lerp (0, 1, bendingLerpFactorSecondary));

			// float bendingDeadzone = saturate ((worldPos.y - deadzoneHeight) / _BendingDeadzoneTopHeight) * _BendingDeadzoneMultiplier;
			// o.animDeadzone = bendingDeadzone;
			// animParams.r = branch phase
			// animParams.g = edge flutter factor
			// animParams.b = primary factor
			// animParams.a = secondary factor

			// Legacy Bending: Primary and secondary bending stored in vertex color blue
			// New Bending: Primary and secondary bending stored in uv4: x = primary bending / y = secondary
			// VertexColors Only: Primary in vertex color A, secondary bending in vertex color blue
			o.animParams.xy = v.color.xy;
			o.animParams.zw = o.animParams.zw;

			o.animParams.x = lerp (o.animParams.x, 0, _RemovePhaseDifferences);
			o.animParams.y = lerp (o.animParams.y, 0, _RemoveBendingFlutter);
			// o.animParams.zw *= bendingDeadzone;

			// Add variation only if the shader uses UV4
			float variation = v.color.b * 2;			

			o.worldCameraDir = WorldSpaceViewDir(v.vertex);

			// Apply terrain gradient
			float gradientY = worldPos.y - v.vertex.y;
            float gradientSize = _CombatTerrainParamsScale.w - _CombatTerrainParamsScale.z;
			float gradientUVBase = saturate ((gradientY - _CombatTerrainParamsScale.z) / max (0.01, gradientSize));

			float2 gradientUV = float2 (gradientUVBase, 0);
			float4 gradientSample = tex2Dlod (_CombatTerrainTexGradient, float4 (gradientUV, 0, 0));
			float gradientLength = max (0.01, _TerrainGradientParams.y - _TerrainGradientParams.x);
			float gradientAmount = saturate ((1 - ((v.vertex.y - _TerrainGradientParams.x) / gradientLength)) * _TerrainGradientParams.w);
			o.colorFromGradient = float4 (gradientSample.xyz, gradientSample.w * gradientAmount);

			// Store Fade for specular highlights
			v.color.r = variation; // (_BendingControls == 2) ? 1.0 - saturate(storedVariation) : variation;
			v.color.b = saturate ((_AfsSpecFade.x - distance (_WorldSpaceCameraPos, worldPos)) / _AfsSpecFade.y);
			v.color.a = lerp (v.color.a, 1, _RemoveVertexOcclusion);
			v.normal = normalize (v.normal);
			v.tangent.xyz = normalize (v.tangent.xyz);

			//Moving access of stored values much later
			o.vertexColorUnmodified = v.color.xyzw;
			o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;

			// UNITY_TRANSFER_DITHER_CROSSFADE (o, v.vertex);
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			ApplySliceCutoff (IN);
			
			#ifdef LOD_FADE_CROSSFADE
			float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
			UnityApplyDitherCrossFade(vpos);
			#endif

			// RIM FADE TERM for masking out polygons with normals that are perpendicular to camera view
			// This helps establishing much cleaner vegetation look (we avoid thin lines of perpendicular
			// vegetation polygons wedging into camera and get less shadowed vegetation overall)
			// Calculate pixel world normal for rim fade term
			float3 pixel_x_derivative = ddx (IN.vertex);
			float3 pixel_y_derivative = ddy (IN.vertex);
			float3 calculated_world_nrm = normalize (cross (pixel_x_derivative, pixel_y_derivative));
			calculated_world_nrm =  mul (unity_ObjectToWorld, float4( calculated_world_nrm, 0.0 )).xyz;
			
			// Calculate rim fade term
			float rim = saturate (((1 - (abs (dot (IN.worldCameraDir, calculated_world_nrm)))) + 1.5) * 1);
			rim *= _RimFadeMultiplier;

			half4 c = tex2D (_MainTex, IN.uv_MainTex.xy);
            half4 bumpTransSpec = tex2D (_BumpTransSpecMap, IN.uv_MainTex.xy);

			half branchMask = saturate (bumpTransSpec.r * 2);
			float branchCutoffFactor = 0; // (1 - _CombatVegetationColor2.w) * branchMask * (1 - _Color.a);
			
			clip (c.a - rim - _CutoffCustom - branchCutoffFactor);
			// clip (c.a - _CutoffCustom);

            // Add Color Variation
			half variationBlend = IN.color.r;
			half variationMask = saturate (bumpTransSpec.r * 10); // detect leaves by raising translucency

			float variationOverrideStrength = _CombatVegetationColor1.w;
			float3 variationOverrideColor1 = _CombatVegetationColor1.xyz;
			float3 variationOverrideColor2 = _CombatVegetationColor2.xyz;
			float3 variationOverrideColor = lerp (variationOverrideColor1, variationOverrideColor2, variationBlend);

			// Blend original color variation setup from per-material properties
			float3 albedoTinted = lerp (c.rgb * _Color.rgb, (c.rgb * _ColorVariation.rgb), variationBlend * variationMask * _ColorVariation.a);

			// Blend global color variation
			float variationGlobalInfluence = 1 - _Color.a; // Only vegetation with 0 local color A accepts global color
			float3 albedoFinal = lerp (albedoTinted, Overlay (albedoTinted, variationOverrideColor), variationOverrideStrength * variationMask * variationGlobalInfluence);

			// Blend gradient contribution
			albedoFinal = lerp (albedoFinal, albedoFinal * IN.colorFromGradient.xyz, IN.colorFromGradient.w);
						
			bumpTransSpec.b = lerp (bumpTransSpec.b, _SmoothnessOverride, _SmoothnessOverrideToggle);
			bumpTransSpec.r = lerp (bumpTransSpec.r, _TranslucencyOverride, _TranslucencyOverrideToggle);

			float smoothnessFinal = bumpTransSpec.b;

			// Backface Smoothness
			smoothnessFinal = (IN.facingSign > 0) ? smoothnessFinal : smoothnessFinal * _BackfaceSmoothness;
			o.Normal = UnpackNormalDXT5nm (bumpTransSpec) * half3(1, 1, lerp (1, IN.facingSign, _ModifyNormalsForBackfaces));
			o.Occlusion = IN.color.a;

			//	Fade out smoothness and translucency
			float invRim = 1 - rim;
			smoothnessFinal *= IN.color.b * invRim;
			
			fixed2 translucencyFinal = fixed2 (bumpTransSpec.r * _TranslucencyStrength * IN.color.b, _TranslucencyViewDependency);
            // o.Emission = saturate(rim);

			#if defined(SHADOW_CAPTURE_MODE)
			{
				//o.Emission = 1;
				albedoFinal = float3 (1, 1, 1);
				o.Occlusion = 0;
			}
			#endif

			float4 packedProp = float4(1,0,0,0);
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				packedProp = packedPropData[unity_InstanceID].Unpack();
			#endif
			
			
			float visibility = packedProp.x;
			float explosion = packedProp.y;
			float opacity = visibility * (1 - explosion);

			// float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
			// clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);

			// This is a very weird macro, it only works if you declare INTERNAL_DATA and worldNormal in surface input
            float3 normalFinalWorld = WorldNormalVector (IN, o.Normal);
			float verticalFactor = saturate (dot (normalFinalWorld, float3 (0, 1, 0)));
			float heightFactor = saturate (IN.vertex.y / max (0.01, _Height));
			verticalFactor = saturate (pow (verticalFactor, 2)) * IN.color.a * lerp (translucencyFinal, variationMask, heightFactor);

			// Rain Intensity + Snowfall Intensity
			float precipitationIntensity = saturate (_WeatherParameters.x + _WeatherParameters.z);
			float snowSurfaceIntensity = saturate (_WeatherParameters.y);
			// Darken foliage when there's precipitation
			smoothnessFinal = lerp (smoothnessFinal, smoothnessFinal + 0.2, precipitationIntensity);
			albedoFinal = lerp (albedoFinal, albedoFinal * 0.5, precipitationIntensity);

			float snowMask = -1 + snowSurfaceIntensity + ContrastGrayscale (saturate (IN.color.r * 1.5), 0.9) * saturate (verticalFactor * 1.5);
			snowMask = saturate (snowMask);
			snowMask *= heightFactor;

			float3 snowColor = float3 (0.5, 0.5, 0.55); // float3 (0.05, 0.1, 0.15);
			albedoFinal = lerp (albedoFinal, snowColor, snowMask);

			o.Albedo = albedoFinal;
			o.Alpha = c.a;
			o.Smoothness = smoothnessFinal;
		}

		ENDCG
	}

	Fallback "Vegetation/Main foliage shadow"
}
