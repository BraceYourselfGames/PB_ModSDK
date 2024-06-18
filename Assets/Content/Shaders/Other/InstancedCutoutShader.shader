// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Instanced/Standard Cutout"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MixInVertexColor ("Vertex Color", Range (0.0, 1.0)) = 1.0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal Intensity", Range (0.0, 1.0)) = 1.0
		_Smoothness ("Smoothness", Range (0,1)) = 0.5
		_Metalness ("Metalness", Range (0,1)) = 0
		_Parallax ("Parallax", Range (0,1)) = 0
		_Cutoff ("Alpha cutoff", Range (0,1)) = 0.5
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "TransparentCutout"
			"Queue" = "AlphaTest"
		}

		LOD 200
		Cull [_Cull]
		Blend One Zero
		ZWrite Off

		CGPROGRAM
		#pragma surface surf Standard exclude_path:forward exclude_path:prepass alphatest:_Cutoff
		#pragma target 3.0
		#pragma multi_compile_instancing

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BumpMap;
		half _MixInVertexColor;
		half _NormalIntensity;

		float _Smoothness;
		float _Metalness;
		float _Parallax;

		struct Input
		{
			float4 color : COLOR;
			float2 uv_MainTex;
			float3 viewDir;
		};

		// half4 _Color;
		UNITY_INSTANCING_BUFFER_START (Props)
			UNITY_DEFINE_INSTANCED_PROP (fixed4, _Color)
			#define _Color_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			half h = tex2D (_BumpMap, IN.uv_MainTex).w;
			float2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);
			IN.uv_MainTex += offset;

			half4 col = tex2D (_MainTex, IN.uv_MainTex);
			col *= UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);
			col *= lerp (1, IN.color, _MixInVertexColor);

			half3 nrm = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
			fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);

			o.Albedo = col;
			o.Smoothness = _Smoothness;
			o.Metallic = _Metalness;
			o.Normal = normalFinal;
			o.Alpha = col.a;
		}
		ENDCG
	}
}
