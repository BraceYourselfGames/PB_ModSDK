Shader "Hardsurface/Prop/Decal (experimental)" 
{
	Properties 
	{
		_HSBOffsetsPrimary ("HSB offsets (primary)", Vector) = (0, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary)", Vector) = (0, 0.5, 0.5, 1)

		_MainTex ("AH", 2D) = "white" {}
		_UV ("UV data (scale, offset)", Vector) = (0, 0, 0, 0)
		_BaseColorPrimary ("Base color (primary)", Color) = (1, 0.5, 0.5, 1)
		_BaseColorSecondary ("Base color (secondary)", Color) = (0, 0.8, 0.8, 1)

		_Cutoff ("Alpha cutoff", Range (0, 1)) = 0.5
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1

		_SmoothnessMin ("Smoothness (min.)", Range (0, 1)) = 0.0
		_SmoothnessMed ("Smoothness (med.", Range (0, 1)) = 0.2
		_SmoothnessMax ("Smoothness (max.", Range (0, 1)) = 0.8

		_EmissionToggle ("Emission toggle", Range (0, 1)) = 1
		_EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
		_EmissionColor ("Emission color", Color) = (0, 0, 0, 1)

		_Scale ("Scale", Vector) = (1, 1, 1, 1)
		_CrushParameters ("Crush pos. (XY), add. (Z), mul. (W)", Vector) = (0, 0, 0, 1)
		_CrushIntensity ("Crush intensity", Range (0, 1)) = 0

		_Visibility ("Visibility", Range (0, 1)) = 1
		_ExplosionAnimation ("Explosion animation", Range (0, 1)) = 0
		_InvFade ("Soft particle factor", Range (0.01, 3.0)) = 1.0
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "AlphaTest" 
			"RenderType" = "AlphaTest" 
		}
		
		Blend SrcAlpha OneMinusSrcAlpha
		// ZWrite Off
		Cull Off
		Offset -1, -1
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard vertex:vert exclude_path:forward exclude_path:prepass noforwardadd nolppv noshadowmask novertexlights keepalpha finalgbuffer:fgbuffer
		#pragma target 5.0
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

		struct Input
		{
			float2 coords : TEXCOORD0;
			float3 localPos;
			float3 localNormal;
			float3 worldPos1;
			float3 worldNormal1;
			float3 viewDir;
			float4 screenPos;
		};

		sampler2D _MainTex;
		half _Smoothness;

		UNITY_INSTANCING_BUFFER_START (Props)
			UNITY_DEFINE_INSTANCED_PROP (float4, _UV)
		#define _UV_arr Props
			UNITY_DEFINE_INSTANCED_PROP (float4, _HSBOffsetsPrimary)
		#define _HSBOffsetsPrimary_arr Props
			UNITY_DEFINE_INSTANCED_PROP (float4, _HSBOffsetsSecondary)
		#define _HSBOffsetsSecondary_arr Props
			UNITY_DEFINE_INSTANCED_PROP (float, _Visibility)
		#define _Visibility_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);

			// v.vertex.xyz += (v.normal + frac (sin (dot (v.vertex.xz, float2 (12.9898, 78.233))) * 43758.5453)) * pow (UNITY_ACCESS_INSTANCED_PROP (_ExplosionAnimation_arr, _ExplosionAnimation), 2) * 1;
			float4 uv_Data = UNITY_ACCESS_INSTANCED_PROP (_UV_arr, _UV);
			o.coords = v.texcoord.xy * uv_Data.xy + float2 (-uv_Data.z, uv_Data.w);
		}

		half _Cutoff;
		fixed4 _BaseColorPrimary;
		fixed4 _BaseColorSecondary;

		void surf (Input IN, inout SurfaceOutputStandard output) 
		{
			float opacity = UNITY_ACCESS_INSTANCED_PROP (_Visibility_arr, _Visibility);
			float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
			clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);

			fixed4 main = tex2D (_MainTex, IN.coords);
			clip (main.w - _Cutoff);

			fixed4 hsbOffsetsPrimary = UNITY_ACCESS_INSTANCED_PROP (_HSBOffsetsPrimary_arr, _HSBOffsetsPrimary);
			fixed4 hsbOffsetsSecondary = UNITY_ACCESS_INSTANCED_PROP (_HSBOffsetsSecondary_arr, _HSBOffsetsSecondary);

			float albedoMaskMinToMed = saturate (main.g * 2);
			float albedoMaskMedToMax = saturate (main.g - 0.5) * 2;
			float3 albedoBase = lerp (lerp (_BaseColorPrimary, float3 (1, 1, 1), albedoMaskMinToMed), _BaseColorSecondary, albedoMaskMedToMax) * main.xxx;

			output.Albedo = albedoBase; // lerp (float3 (0.2, 0.3, 1), main.xxx, main.w);
			output.Metallic = 0;
			output.Smoothness = _Smoothness;
			output.Alpha = main.w;
		}

		void fgbuffer (Input IN, SurfaceOutputStandard o, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
		{
			diffuse.a = o.Alpha;
			specSmoothness.a = o.Alpha;
			normal.a = o.Alpha;
			emission.a = o.Alpha;
		}

		ENDCG
	}

}