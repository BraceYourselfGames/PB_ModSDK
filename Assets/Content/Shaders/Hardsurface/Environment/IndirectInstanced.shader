﻿Shader "CustomRendering/IndirectInstanced"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#include "Instancing_Shared.cginc"
			#pragma surface surf Standard addshadow fullforwardshadows vertex:vert
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup
			//#pragma instancing_options procedural:setupFast

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void vert (inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);

				float4 scaleProp = float4(1, 1, 1, 1);

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

				scaleProp = scaleData[unity_InstanceID];
#endif
			}

			void surf (Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
    }
    FallBack "Diffuse"
}
