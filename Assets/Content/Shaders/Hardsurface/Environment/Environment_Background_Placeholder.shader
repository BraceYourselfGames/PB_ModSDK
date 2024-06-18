Shader "Hardsurface/Environment/Backgrounds (placeholder)"
{
    Properties
    {
        _MainTex ("Color ID bake", 2D) = "gray" {}
        _CurvatureTex ("Curvature bake", 2D) = "gray" {}
        _AOTex ("AO bake", 2D) = "white" {}

        _Smoothness ("Smoothness", Range (0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200
        Cull [_Cull]


        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

        sampler2D _MainTex;
        sampler2D _CurvatureTex;
        sampler2D _AOTex;

        half _Smoothness;

        struct Input
        {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 id = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 crv = tex2D (_CurvatureTex, IN.uv_MainTex);
            fixed4 ao = tex2D (_AOTex, IN.uv_MainTex);

            float crv1 = saturate ((crv.x + crv.y + crv.z) / 3);
            float ao1 = saturate ((ao.x + ao.y + ao.z) / 3);

			float3 crvHigh = saturate ((crv1 - 0.5) * 2 * id.xyz * 4);
			float crvLow = saturate (crv1 * 2);

            o.Albedo = saturate (id.xyz + crvHigh) * crvLow * lerp (1, ao1, 0.5);
            o.Metallic = 0;
            o.Smoothness = _Smoothness * saturate (crv1 * 2);
            o.Occlusion = ao1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
