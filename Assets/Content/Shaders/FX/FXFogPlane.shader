Shader "Unlit/FXFogPlane"
{
    Properties
    {
        _FogColor("Fog Color", Color) = (1,1,1,1)
        _FogIntensity("Fog Intensity", Range(0, 1)) = 0.5
        _FogDistance("Fog Distance", Range(0.1, 50)) = 25
    }
    SubShader
    {
        Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
        ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos1 : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float eyeDepth : TEXCOORD4;
            };

            float4 _FogColor;
            float _FogIntensity;
            float _FogDistance;

            sampler2D _CameraDepthTexture;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                COMPUTE_EYEDEPTH (o.eyeDepth);
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // Get scene depth and pixel (surface) depth values
    			float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
    			float sceneZ = LinearEyeDepth (depth);
    			float surfZ = IN.eyeDepth;
                // Get screen pixel world position behind our translucent surface for camera stable water depth reconstruction
    			float3 pixelWorldPos = RestorePixelWorldPosBehindTranslucency(IN.worldPos1, sceneZ, surfZ);

                float fog_mask = saturate(distance(IN.worldPos1, pixelWorldPos) / _FogDistance);

                float4 color_out = float4(_FogColor.rgb, _FogIntensity * fog_mask * IN.color.r);
                // apply fog
                UNITY_APPLY_FOG(IN.fogCoord, color_out);
                return color_out;
            }
            ENDCG
        }
    }
}
