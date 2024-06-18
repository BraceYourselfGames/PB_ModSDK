// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Instanced/Unlit"
{
	Properties
	{
		//_TintColor ("Tint color (unused)", Color) = (1,1,1,1)
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_FresnelFade ("Fresnel fade", Range (0, 1)) = 0
		_FresnelShift ("Fresnel shift", Range (-1, 1)) = 0
		_FresnelPower ("Fresnel power", Range (1, 5)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Off
		Lighting Off
		ZWrite Off

		CGPROGRAM
		#pragma surface surf Lambert exclude_path:prepass noforwardadd alpha:fade
		#pragma target 3.0
		#pragma multi_compile_instancing

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float _FresnelFade;
		float _FresnelPower;
		float _FresnelShift;

		struct Input
		{
			float2 uv_MainTex;
			float3 viewDir;
		};

		// half4 _Color;
		UNITY_INSTANCING_BUFFER_START (Props)
			UNITY_DEFINE_INSTANCED_PROP (fixed4, _Color)
#define _Color_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		/*
		v2f vert (appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos (v.vertex);
			o.uv = TRANSFORM_TEX (v.uv, _MainTex);
			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			fixed4 col = tex2D (_MainTex, i.uv);
			// col *= _Color;
			col *= UNITY_ACCESS_INSTANCED_PROP (_Color);
			return col;
		}
		*/

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 col = tex2D (_MainTex, IN.uv_MainTex);
			col *= UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);

			float fresnelFade = lerp (1, saturate ((dot (normalize (IN.viewDir), o.Normal) + _FresnelShift) * _FresnelPower), _FresnelFade);
			col.a *= fresnelFade;

			o.Albedo = col;
			o.Emission = col;
			o.Alpha = col.a;
		}
		ENDCG
	}
}
