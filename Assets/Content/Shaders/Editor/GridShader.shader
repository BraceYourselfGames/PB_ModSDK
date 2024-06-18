Shader "Custom/GridShader" 
{
    Properties 
    {
        _MainTex ("Main", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
		[HDR]
		_EmissiveColor ("Emissive color", Color) = (0,0,0,0)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader 
    {
        Tags 
        { 
            "Queue" = "AlphaTest"
            "RenderType" = "Opaque"
            "ForceNoShadowCasting" = "True" 
        }
        LOD 200
        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard exclude_path:forward exclude_path:prepass noshadow noforwardadd keepalpha finalgbuffer:DecalFinalGBuffer

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input 
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float4 _EmissiveColor;

        #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) 
        {
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			o.Emission = _EmissiveColor;
            o.Alpha = c.a;
        }

        void DecalFinalGBuffer (Input IN, SurfaceOutputStandard o, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
        {
            diffuse.a = o.Alpha;
            specSmoothness.a = o.Alpha;
            normal.a = o.Alpha;
            emission.a = o.Alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
