Shader "Vegetation/Basic (Instanced)"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_Metallic ("Metalness", Range(0, 1)) = 0.0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.0
		_FresnelFadeMultiplier ("Fresnel fade multiplier", Range (1, 10)) = 1
		_Cutoff ("Alpha cutoff", Range (0,1)) = 0.5
		_WindIntensity ("Wind intensity", Range(0, 1)) = 1
		_WindScale ("Wind scale", Range(0.01, 1)) = 0.1
		_WindOffset ("Wind offset", Range(0, 1)) = 0
		_WindWorldScale ("Wind world scale", Range(0, 1)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest"
			"RenderType" = "TransparentCutout"
		}
		LOD 200
		Blend One Zero
		
		CGPROGRAM

		#pragma surface surf Standard exclude_path:forward exclude_path:prepass vertex:vert alphatest:_Cutoff 
		#pragma target 3.0
		// #pragma multi_compile_instancing

		// Config maxcount. See manual page.
		// #pragma instancing_options

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
			float3 localPos;
			float3 localNormal;
			float3 worldRefl;
			float3 viewDir;
			float2 wind;
		};

		half _Smoothness;
		half _Metallic;
		half _FresnelFadeMultiplier;

		half _WindIntensity;
		half _WindScale;
		half _WindOffset;
		half _WindWorldScale;

		// Declare instanced properties inside a cbuffer.
		// Each instanced property is an array of by default 500(D3D)/128(GL) elements. Since D3D and GL imposes a certain limitation
		// of 64KB and 16KB respectively on the size of a cubffer, the default array size thus allows two matrix arrays in one cbuffer.
		// Use maxcount option on #pragma instancing_options directive to specify array size other than default (divided by 4 when used
		// for GL).

		//UNITY_INSTANCING_CBUFFER_START(Props)
		//	UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		//UNITY_INSTANCING_CBUFFER_END

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			// Displace

			/*
			float time = _Time.y;
			float windBase = sin ((_WindOffset * 3.141593) + time);
			float3 windDirection = normalize (float3 (1, 0.5, 0.5) + lerp // + v.normal);
			float windScale = _WindScale * (v.vertex.x + v.vertex.y + v.vertex.z);
			v.vertex.xyz += windBase * windDirection * windScale;
			*/

			o.wind = float2 (0, 0);
			float num = v.vertex.y;
			if ((num - _WindOffset) > 0.0) 
			{
				float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;

				/*
				float x = sin (worldPos.x / _WindWorldScale + (_Time.y * _WindIntensity)) * (num - _WindOffset) * _WindScale;
				float y = sin (worldPos.y / _WindWorldScale + (_Time.y * _WindIntensity)) * (num - _WindOffset) * _WindScale;
				*/

				float x = sin (worldPos.x / _WindWorldScale + (_Time.y * _WindIntensity)) * (num - _WindOffset) * _WindScale;
				float z = sin (worldPos.y / _WindWorldScale + (_Time.y * _WindIntensity)) * (num - _WindOffset) * _WindScale;

				v.vertex.x += x * 1;
				v.vertex.z += z * 1;
				o.wind = float2 (x, z);
			}


			o.localPos = v.vertex.xyz;
			o.localNormal = v.normal;

		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex);
			fixed3 albedo = col.rgb;

			float rim = pow (saturate (dot (normalize (IN.viewDir), o.Normal)), _FresnelFadeMultiplier);
			float alpha = col.a * rim;

			o.Albedo = albedo;
			o.Metallic = 0;
			o.Smoothness = _Smoothness;
			o.Occlusion = 1;
			o.Alpha = alpha;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
