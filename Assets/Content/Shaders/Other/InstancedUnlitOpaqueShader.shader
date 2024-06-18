Shader "Instanced/Unlit Opaque"
{
	Properties
	{
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		//Lighting Off

		CGPROGRAM
		#pragma surface surf Lambert addshadow //exclude_path:prepass noforwardadd
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

		UNITY_INSTANCING_BUFFER_START (Props)
		UNITY_DEFINE_INSTANCED_PROP (fixed4, _Color)
#define _Color_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 col = tex2D (_MainTex, IN.uv_MainTex);
			col *= UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);

			o.Albedo = col;
			o.Emission = col;
		}
		ENDCG
	}
}
