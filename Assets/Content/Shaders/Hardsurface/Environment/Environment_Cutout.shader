Shader "Hardsurface/Environment/Simple Decal"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}

		_SmoothnessMin ("Smoothness min", Range (0.0, 1.0)) = 0
		_SmoothnessMed ("Smoothness med", Range (0.0, 1.0)) = 0.25
		_SmoothnessMax ("Smoothness max", Range (0.0, 1.0)) = 0.5

        [Gamma] _Metallic ("Metalness", Range (0.0, 1.0)) = 0.0

		_NormalTex ("Normal", 2D) = "bump" {}
		_Cutoff ("Cutoff", Range (0.0, 1.0)) = 0.5
		_FadeDistance ("Fade dist.", Range (0, 3)) = 0
		_IntersectionDistance ("Intersection dist.", Range (0.01, 0.2)) = 0.01

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
		{
			"RenderType" = "AlphaTest"
		}
        Cull [_Cull]
        
		Offset -1, -1

        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows alphatest:_Cutoff
        #pragma target 3.0

        half4 _Color;
        sampler2D _MainTex;

		half _SmoothnessMin;
		half _SmoothnessMed;
		half _SmoothnessMax;

        half _Metallic;
        sampler2D _NormalTex;

		float _FadeDistance;
		float _IntersectionDistance;

		sampler2D_float _CameraDepthTexture;
		float4 _CameraDepthTexture_TexelSize;

        struct Input
		{
            float2 uv_MainTex;
			float4 vertexColor : COLOR;
			float4 screenPos;
			float eyeDepth;
        };

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);
			COMPUTE_EYEDEPTH (o.eyeDepth);
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half4 color = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = color.rgb * _Color * IN.vertexColor.a;

			float smoothnessFromAlbedo = (color.r + color.g + color.b) / 3;
			float smoothnessMaskMinToMed = saturate (smoothnessFromAlbedo * 2);
			float smoothnessMaskMedToMax = saturate (smoothnessFromAlbedo - 0.5) * 2;
			float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax) * IN.vertexColor.a;
			o.Smoothness = smoothnessFinal;

            // Normal map
            half4 normal = tex2D (_NormalTex, IN.uv_MainTex);
            o.Normal = UnpackNormal (normal);

			// Other parameters
            o.Metallic = _Metallic;

			float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (IN.screenPos));
			float sceneZ = LinearEyeDepth (rawZ);
			float partZ = IN.eyeDepth;

			float fade = 1.0;
			if (rawZ > 0.0) // Make sure the depth texture exists
			{
				float difference = sceneZ - partZ;
				fade = (1.0 - saturate (difference / _FadeDistance)) * saturate (difference / _IntersectionDistance);
			}

			o.Alpha = color.a * fade;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
