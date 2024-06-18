Shader "Hardsurface/Environment/Surface (Helper)" 
{
	Properties 
	{
		_Color ("Albedo color", Color) = (0.5, 0.5, 0.5, 1)
		_Smoothness ("Smoothness", Range (0, 1)) = 0.5
		_Metalness ("Metalness", Range (0, 1)) = 0.0
		_EmissionIntensity ("Emission intensity", Range (0, 128)) = 0
		_EmissionColor ("Emission color", Color) = (0, 0, 0, 1)
		_Visibility ("Visibility", Range (0, 1)) = 1
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
			"ForceNoShadowCasting" = "True"
		}
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard
		#pragma target 5.0

		struct Input
		{
			float2 uv_MainTex;
			float3 viewDir;
			float4 screenPos;
		};

		half4 _Color;
		half _Smoothness;
		half _Metalness;
		half4 _EmissionColor;
		float _EmissionIntensity;
		half _Visibility;

		void surf (Input IN, inout SurfaceOutputStandard output) 
		{
			output.Albedo = _Color;
			output.Metallic = _Metalness;
			output.Smoothness = _Metalness;
			output.Emission = _EmissionColor * _EmissionIntensity;

			float4x4 thresholdMatrix =
			{ 
				1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
				13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
				4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
				16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
			};
			float4x4 rowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
			float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
			clip (_Visibility - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);
		}
		ENDCG
	}
	FallBack "Diffuse"
}