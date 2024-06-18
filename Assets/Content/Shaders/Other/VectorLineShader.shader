Shader "Custom/VectorLineShader"
{
    Properties
    {
        _TintColor ("Color", Color) = (1, 1, 1, 1)
        _Brightness ("_Brightness", Range (0, 6)) = 1
        _MainTex ("Texture", 2D) = "white" {}
        _FillExtents ("Fill extents (X min/max, Y min/max)", Vector) = (0, 1, 0, 1)
        _FillTransitions ("Fill transitions (X min/max, Y min/max)", Vector) = (0.1, 0.1, 0.1, 0.1)
        _InvFade ("Soft Particles Factor", Range (0.01,10.0)) = 1.0
        
        [Toggle (_USE_FILLS)]
		_UseFills ("Use fills", Float) = 0
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
            "RenderType" = "Transparent" 
            "PreviewType" = "Plane"
            "ForceNoShadowCasting" = "True"
        }


		ZWrite Off
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert noshadow noambient alpha:fade
        #pragma target 3.0
        #pragma shader_feature_local _USE_FILLS

        sampler2D _MainTex;
        fixed4 _TintColor;
        float _Brightness;
        float4 _FillExtents;
        float4 _FillTransitions;
        float _InvFade;

        struct Input
        {
            float2 uv_MainTex;
            float4 eyeDepth;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)
        UNITY_DECLARE_DEPTH_TEXTURE (_CameraDepthTexture);

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT (Input, o);
            float4 vertexClip = UnityObjectToClipPos (v.vertex);
            o.eyeDepth = ComputeScreenPos (vertexClip);
            COMPUTE_EYEDEPTH (o.eyeDepth.z);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
            float3 color = _Brightness * tex.rgb * _TintColor;
            float alpha = tex.a * _TintColor.a;

            // Hard cut
            // alpha *= IN.uv_MainTex.x < _FillXFrom ? 0 : 1;
            // alpha *= (1 - IN.uv_MainTex.x) < (1 - _FillXTo) ? 0 : 1;
            // alpha *= IN.uv_MainTex.y < _FillYFrom ? 0 : 1;
            // alpha *= (1 - IN.uv_MainTex.y) < (1 - _FillYTo) ? 0 : 1;

            float2 uv = IN.uv_MainTex.xy;
            
            #if _USE_FILLS
            
            float4 extents = float4 (saturate (_FillExtents.x), saturate (_FillExtents.y), saturate (_FillExtents.z), saturate (_FillExtents.w));
            float4 transitions = float4 (saturate (_FillTransitions.x), saturate (_FillTransitions.y), saturate (_FillTransitions.z), saturate (_FillTransitions.w));

            alpha *= saturate (saturate ((uv.x - extents.x + transitions.x * (1 - extents.x)) * (1 / transitions.x)));
            alpha *= saturate (saturate ((extents.y - uv.x + transitions.y * extents.y) * (1 / transitions.y)));
            alpha *= saturate (saturate ((uv.y - extents.z + transitions.z * (1 - extents.z)) * (1 / transitions.z)));
            alpha *= saturate (saturate ((extents.w - uv.y + transitions.w * extents.w) * (1 / transitions.w)));
            
            #endif

            // Depth fade
            float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (IN.eyeDepth)));
            float partZ = IN.eyeDepth.z;
            float fade = saturate (1 / _InvFade * (sceneZ - partZ));

            o.Albedo = 0;
            o.Emission = color;
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
