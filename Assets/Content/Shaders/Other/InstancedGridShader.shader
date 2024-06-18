Shader "Custom/InstancedGridShader" 
{
	Properties 
	{
		_AlbedoColor ("Albedo color", Color) = (1,1,1,1)
		_EmissionColor ("Emission color", Color) = (0,0,0,0)
		_EmissionIntensity ("Emission intensity", Range (0, 8)) = 0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Toggle(XRAY)]
        _XrayMode ("X-Ray mode", Float) = 0
		_XrayMainColor ("X-Ray main", Color) = (1,1,1,1)
		_XrayRimColor ("X-Ray rim color", Color) = (1,1,1,1)
		_XrayIntensity ("X-Ray intensity", Range (0, 8)) = 0
		_XrayRimWidth ("X-Ray rim width", Float) = 1
		_XrayInvFade ("X-Ray depth fade factor", Float) = -0.01
		_XrayDepthOffset ("X-Ray depth offset", Range (-5, 5)) = 0

		_ScanTex ("Scan map", 2D) = "gray" {}
		_GlitchTex ("Glitch map", 2D) = "black" {}
		_GlitchFactors ("Glitch factors", Vector) = (1, 1, 0.01, 0)
		_ScanSize ("Scan size", Range (64, 512)) = 256
		_ScanTimeFactor ("Scan time factors", Range (0.01, 0.1)) = 0.02

	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite Off
		ZTest Greater
		// Blend One One
		Cull Off
     
		CGPROGRAM

		#pragma surface surf Lambert vertex:vert alpha:fade noforwardadd nolppv noshadowmask novertexlights exclude_path:forward
		#pragma target 5.0
		#pragma shader_feature_local XRAY
		#include "UnityCG.cginc"

		struct Input 
		{
			float2 uv_MainTex;
			float4 vertex;
			float3 viewDir;
			float4 projPos;
			float4 screenPos;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);
			o.vertex = UnityObjectToClipPos (v.vertex);
			o.projPos = ComputeScreenPos (o.vertex);
			COMPUTE_EYEDEPTH (o.projPos.z);
		}

		sampler2D _CameraDepthTexture;
		float4 _XrayMainColor;
		float4 _XrayRimColor;
		float _XrayIntensity;
		float _XrayRimWidth;
		float _XrayInvFade;
		float _XrayDepthOffset;

		sampler2D _ScanTex;
		sampler2D _GlitchTex;
		float4 _GlitchFactors;
		float _ScanColorIntensity;
		float _ScanSize;
		float4 _ScanTimeFactor;

		void surf (Input IN, inout SurfaceOutput o) 
		{
			#ifdef XRAY

			float3 n = float3 (0, 0, 1);
			float backsideFactor = dot (IN.viewDir, n) > -0.1 ? 0 : 1;
			float3 normalFinal = lerp (n, -n, backsideFactor);
			float fresnelFade = pow (saturate (dot (normalize (IN.viewDir), normalFinal)), _XrayRimWidth);
			o.Emission = lerp (_XrayMainColor, _XrayRimColor, fresnelFade) * _XrayIntensity;

			float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH (tex2Dproj (_CameraDepthTexture, UNITY_PROJ_COORD (IN.projPos))));
			float partZ = IN.projPos.z;
			float depthFade = saturate (_XrayInvFade * (sceneZ - (partZ + _XrayDepthOffset)));

			float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
			screenPos *= _ScreenParams.xy; // pixel position

			float2 screenUV = screenPos.xy;
			float sinCompressed4 = ((_SinTime.y + 1) / 2);
			float sinCompressed = ((_SinTime.w + 1) / 2);

			screenUV /= _ScanSize.xx;
			half4 glitchNormal = tex2D (_GlitchTex, screenUV * _GlitchFactors.x);
			float random = frac (sin (dot ((glitchNormal.xy - 0.5) * sinCompressed4, float2 (12.9898, 78.233))) * 43758.5453);

			screenUV.xy += (glitchNormal.xy - 0.5) * sinCompressed * _GlitchFactors.y;
			screenUV.xy += (glitchNormal.xy) * random * _GlitchFactors.z;
			screenUV.y += _Time.w * _ScanTimeFactor;

			fixed4 scan = tex2D (_ScanTex, screenUV);
			float alphaFinal = scan.w * depthFade * pow (saturate (sinCompressed), 4);

			o.Alpha = alphaFinal;

			#else

			o.Alpha = 0;

			#endif
		}

		ENDCG

		/*

		ZWrite On
		ZTest LEqual
		Blend One Zero
		Cull Off

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows
		#pragma target 5.0
		#include "UnityCG.cginc"

		struct Input 
		{
			float2 uv_MainTex;
		};

		fixed4 _AlbedoColor;
		fixed4 _EmissionColor;
		half _EmissionIntensity;
		half _Glossiness;
		half _Metallic;

		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = _AlbedoColor;
			o.Emission = _EmissionColor * _EmissionIntensity;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}

		ENDCG
		*/
	}
}
