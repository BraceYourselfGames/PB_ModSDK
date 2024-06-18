Shader "Unlit/RainArea"
{
    Properties
    {
        _RampTex("Ramp Texture", 2D) = "white" {}
        _PatternTex("Pattern Texture", 2D) = "white" {}
        _MainTex("Weather Texture", 2D) = "white" {}
        [HDR] _Tint("Tint", Color) = (1, 1, 1, 1)
        _CellSize("Cell Size", Float) = 50
        _WorldScale("World Scale", Float) = 5120
        _Cutoff("Cutoff", Float) = 0
        _Opacity("Opacity", Float) = 0.5
        _Outline("Outline", Float) = 0
        _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        _BlurOffset("Blur Offset", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float2 worldPosXY : TEXCOORD3;
                float4 projPos : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _RampTex;
            sampler2D _PatternTex;
            float4 _PatternTex_ST;
            
            float4 _Tint;
            float _Cutoff;
            float _Opacity;
            float _Outline;

            float _CellSize;
            float _WorldScale;

            sampler2D _CameraDepthTexture;
            float _InvFade;

            float _BlurOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPosXY = (float2(o.worldPos.x, o.worldPos.z) / _WorldScale) + float2(_CellSize / 2, _CellSize / 2);

                o.projPos = ComputeScreenPos(o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float4 tex = tex2D(_MainTex, i.worldPosXY) * 0.29411764705882354;
                tex += tex2D(_MainTex, (i.worldPosXY + float2(_BlurOffset, 0))) * 0.17647058823529413;
                tex += tex2D(_MainTex, (i.worldPosXY - float2(_BlurOffset, 0))) * 0.17647058823529413;
                tex += tex2D(_MainTex, (i.worldPosXY + float2(0, _BlurOffset))) * 0.17647058823529413;
                tex += tex2D(_MainTex, (i.worldPosXY - float2(0, _BlurOffset))) * 0.17647058823529413;
                
                float4 col = float4 (0, 0, 0, 1);

                float rainRemapped = saturate(tex.b - 0.5) * 2;

                float2 worldPosNoScale = i.worldPosXY * _WorldScale;

                if (tex.b > 0.25)
                {
					float rampSample = tex2D (_RampTex, float2 (saturate (tex.b - 0.3333) * 1.5, 0)).x;
                    float4 patternSample = tex2D (_PatternTex, i.worldPosXY * _WorldScale * _PatternTex_ST.xy);
                    
                    col.xyz += rampSample * 0.25;
                    float patternMult = lerp (rainRemapped, rampSample, 0.75) * 0.2 * lerp (1, 0.25, saturate (rainRemapped * 5 - 4));

                    if (tex.b > 0.8333)
                    {
                        col.xyz += patternSample.x * patternMult;
                        // col.xyz += float3 (1, 0.5, 0.5) * 0.2;
                    }
                        
                    else if (tex.b > 0.6666)
                    {
                        col.xyz += patternSample.y * patternMult;
                        // col.xyz += float3 (0.5, 1, 0.5) * 0.2;
                    }
                        
                    else if (tex.b > 0.5)
                    {
                        col.xyz += patternSample.w * 0.035;
                        // col.xyz += float3 (0.5, 0.5, 1) * 0.2;
                    }
                        
                    
                    /*
                    float rainEdge = 0; // Rain area edge
                    float medGrid = saturate(((worldPosNoScale.x / 4) % 1) * ((worldPosNoScale.y / 4) % 1) - 0.8) * 5;
                    float smallGrid = saturate(((worldPosNoScale.x / 2) % 1) * ((worldPosNoScale.y / 2) % 1) - 0.6) * 2.5;
                    float tinyGrid = saturate(((worldPosNoScale.x) % 1) * ((worldPosNoScale.y) % 1) - 0.6) * 2.5;

                    col += saturate(medGrid * (tex.b * tex.b)) * 0.2; // Light rain
                
                    // Rain edge
                    if (rainRemapped > 0) {

                        rainEdge = pow(saturate(1 - rainRemapped), 10); // Rain area edge
                        col += rainEdge * 0.1;
                        col += (rainEdge * tinyGrid) * 0.15;
                        if (rainRemapped > 0.9) { // Heavy rain
                            col += saturate(smallGrid * tex) * 0.15;
                        } 

                    }
                    */

                }

                float fadeByDistance = 1 - saturate ((i.projPos.z / 800) - 0.1);
                
                col.a *= _Opacity * fadeByDistance;
                col.xyz *= _Tint * fadeByDistance;

                UNITY_APPLY_FOG(i.fogCoord, col);

                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float projZ = i.projPos.z;
                float fade = saturate(_InvFade * (sceneDepth - projZ));
                col.a *= fade;

                return col;
            }
            ENDCG
        }
    }
}
