// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TextureArrayTest" 
{
	Properties 
	{
		_TexArrayAH ("Texture array / AH", 2DArray) = "" {}
		_TexArrayMSEO ("Texture array / MSEO", 2DArray) = "" {}
		_ArrayIndex ("Array index", Range (0,16)) = 6
		_ArrayIndexShift ("Array index shift", Range (-1,1)) = 0
		_ArrayTestMode ("Array test mode", Range (0,1)) = 0
	}
	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque" 
		}
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 5.0

		struct Input
		{
			float2 texcoord_uv1 : TEXCOORD0;
			float2 texcoord_uv2 : TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			INTERNAL_DATA
		};

		float _ArrayIndex;
		float _ArrayIndexShift;
		float _ArrayTestMode;

		// Arrays can't be serialized, so you can't declare them in Properties block
		// they are set at runtime
		float4 _PropertyArray[10];

		// Properties that must not break instancing go here
		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		// Texture arrays are declared last (not sure why, they just are)
		UNITY_DECLARE_TEX2DARRAY (_TexArrayAH);

		void vert (inout appdata_full v, out Input o) 
		{
			UNITY_SETUP_INSTANCE_ID (v);
			UNITY_INITIALIZE_OUTPUT (Input, o);
			UNITY_TRANSFER_INSTANCE_ID (v, o);

			// There are no traditional 2D samplers used in this surface shader, so UV inputs won't be auto-generated
			// we have to fill the UV1 and UV2 manually (UV2 packs array index)
			o.texcoord_uv1 = v.texcoord;
			o.texcoord_uv2 = v.texcoord1;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float arrayIndexFromUV2 = lerp (IN.texcoord_uv2.x, _ArrayIndex, _ArrayTestMode);
			float arrayIndexOffset = arrayIndexFromUV2 + _ArrayIndexShift;
			float3 arrayUV = float3 (IN.texcoord_uv1.x, IN.texcoord_uv1.y, arrayIndexOffset);

			fixed4 arraySample = UNITY_SAMPLE_TEX2DARRAY (_TexArrayAH, arrayUV);
			float4 properties = _PropertyArray[arrayIndexFromUV2];

			o.Albedo = arraySample.rgb;
			o.Metallic = properties.w;
			o.Smoothness = properties.y;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
