// Upgrade NOTE: upgraded instancing buffer 'FoliageInstance' to new syntax.

Shader "Vegetation/Main foliage shadow"
{
	Properties
	{
		[Space (12)]
		[Enum (UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0

		[Header (Base Settings)]
		[Space (12)]
		_MainTex ("Albedo (RGB), opacity (A)", 2D) = "white" {}
		_CutoffShadow ("Alpha cutoff (shadow)", Range (0,1)) = 0.5
		_PackedPropData ("Visibility, explosion, compression, bending deadzone height", Vector) = (0, 0.5, 0.5, 1)

		[NoScaleOffset] _BumpTransSpecMap ("Trans. (R), norm. (GA), sm. (B)", 2D) = "bump" {}
		_ModifyNormalsForBackfaces ("Modify normal for backfaces", Range (0.0, 1.0)) = 1.0

		_RemoveBendingFlutter ("Remove bending flutter", Range (0, 1)) = 1
		_RemovePhaseDifferences ("Remove phase differences", Range (0, 1)) = 1

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

		[Space (12)]
		[Toggle (GEOM_TYPE_BRANCH)] _Pivots ("Baked Pivots", Float) = 0
		[Toggle (SHADOW_CAPTURE_MODE)] _ShadowCaptureMode ("Shadow capture mode", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
			"RenderType" = "AlphaTest"
		}

		LOD 200
		Cull[_Culling]

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert exclude_path:forward exclude_path:prepass addshadow noforwardadd nolppv noshadowmask novertexlights //dithercrossfade //finalcolor:FinalColorFunction
		#pragma target 5.0
		#pragma multi_compile_instancing
		#pragma multi_compile _ GEOM_TYPE_BRANCH
		#pragma shader_feature EFFECT_BUMP
		#pragma instancing_options procedural:setup
		
		#include "Assets/Content/Shaders/Hardsurface/Environment/Instancing_Shared.cginc"
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

		sampler2D _MainTex;
		sampler2D _BumpTransSpecMap;

		fixed _CutoffShadow;
		fixed _ModifyNormalsForBackfaces;
		
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

		struct Input
		{
			float2 uv_MainTex;
			fixed4 color : COLOR0;
			float4 vertex;
			float facingSign : VFACE;
			float3 viewDir;
			float4 animParams;
			float animDeadzone;
			float4 screenPos;
			INTERNAL_DATA
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);
			UNITY_SETUP_INSTANCE_ID(v);

			//return;

			float variation = v.color.b * 2;
			float bendingLerpFactorPrimary = saturate ((v.vertex.y - _BendingPrimaryBottomHeight) / _BendingPrimaryTopHeight) * _BendingPrimaryMultiplier;
			float bendingLerpFactorSecondary = saturate ((v.vertex.y - _BendingSecondaryBottomHeight) / _BendingSecondaryTopHeight) * _BendingSecondaryMultiplier;
			float2 bendingHeightBased = float2 (lerp (0, 1, bendingLerpFactorPrimary), lerp (0, 1, bendingLerpFactorSecondary));
			
			o.animParams = float4 (0,0,0,0);
			v.normal = normalize (v.normal);
			v.tangent.xyz = normalize (v.tangent.xyz);
			
			// Destruction animation
			o.vertex = v.vertex;
			
			// UNITY_TRANSFER_DITHER_CROSSFADE (o, v.vertex);

		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex.xy);
			clip (c.a - _CutoffShadow);
			return;
		}

		ENDCG
	}

	Fallback "Transparent/Cutout/VertexLit"
}
