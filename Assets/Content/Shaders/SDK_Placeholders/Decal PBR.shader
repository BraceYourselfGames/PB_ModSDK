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
		// Regular color & lighting pass
        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100
            ColorMask RGB
            ZWrite On

            // Write to Stencil buffer (so that silouette pass can read)
            Stencil
            {
                Ref 4
                Comp always
                Pass replace
                ZFail keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

            sampler2D _MainTex;
            sampler2D _RampTex;
            sampler2D_float _CameraDepthTexture;
            float _InvFade;
            float _InvFadeOuter;
            half4 _Color;
            half4 _ContactColor;
            half _Brightness;
            half _AmbientMultiplier;
            float _Hue;
            float _HSVInfluence;
            float _Opacity;

            float _SizeAdjustmentAmount;
            float4 _SizeAdjustment;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 projPos : TEXCOORD3;
                fixed4 diff : COLOR0;
                float scaleInterpolant : TEXTCOORD4;
            };

            v2f vert (appdata_full v)
            {
                v2f o;

                float4 clipPosUnmodified = UnityObjectToClipPos (v.vertex);
                o.projPos = ComputeScreenPos (clipPosUnmodified);
                half3 wn = UnityObjectToWorldNormal (v.normal);
                o.diff = max (0, dot (wn, _WorldSpaceLightPos0.xyz)) * _LightColor0;
                o.diff.rgb += ShadeSH9 (half4(wn, 1));
                COMPUTE_EYEDEPTH (o.projPos.z);

                // Calculate the distance between camera and object's pivot and make it a 0-1 gradient over the specified distance
				// We also slightly offset initial distance values to make meshes appear\disappear at the moment of  overlap between LODs

                float3 worldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
				float dist = distance (_WorldSpaceCameraPos, worldPos);
                o.scaleInterpolant = saturate ((dist - _SizeAdjustment.x) / max (1, _SizeAdjustment.y)) * _SizeAdjustmentAmount;

                v.vertex = lerp
                (
                    v.vertex,
                    v.vertex * _SizeAdjustment.z,
                    o.scaleInterpolant
                );
                
                float4 clipPosModified = UnityObjectToClipPos (v.vertex);
                o.vertex = clipPosModified;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (i.projPos)));
                float partZ = i.projPos.z;
                float diffZ = sceneZ - partZ;
                float fade = saturate (_InvFade * diffZ);
                fade *= 1 - saturate (_InvFadeOuter * (sceneZ - partZ));

                float4 color = lerp (_ContactColor, _Color, fade);
                float3 colorHSV = RGBToHSV (color.xyz);
                colorHSV.x = _Hue;
                color.xyz = lerp (color.xyz, HSVToRGB (colorHSV), _HSVInfluence) * _Brightness;
                color.xyz *= lerp (1, _SizeAdjustment.w, i.scaleInterpolant);
                
                color.a = _Opacity * fade;
                
                color.rgb += (i.diff.rgb * _AmbientMultiplier);
                return color;
            }
      

            ENDCG
        }
	}

	Fallback Off
}
