Shader "Unlit/RainAreaCurved"
{
    Properties
    {
        _MainTex("Weather Texture", 2D) = "white" {}
        _CellSize("Cell Size", Float) = 50
        _Cutoff("Cutoff", Float) = 0
        _Opacity("Opacity", Float) = 0.5
        _Outline("Outline", Float) = 0
        _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            float _Cutoff;
            float _Opacity;
            float _Outline;

            float _CellSize;

            sampler2D _CameraDepthTexture;
            float _InvFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPosXY = float2(o.worldPos.x, o.worldPos.z) + float2(_CellSize / 2, _CellSize / 2);

                o.projPos = ComputeScreenPos(o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                // col = saturate(col - _Cutoff);
                
                fixed4 col = fixed4(0, 0, 0, 0);

                float rainRemaped = saturate(tex.b - 0.25) * 4;

                if (rainRemaped > 0) {
                    if (rainRemaped < 0.5) {
                        // float edgeGrid = saturate(((i.worldPosXY.x / 2) % 1) * ((i.worldPosXY.y / 2) % 1));
                        float edgeGrid = 1;
                        if (edgeGrid > 0.7) {
                            col = float4(rainRemaped * 2, rainRemaped * 1.2, rainRemaped, rainRemaped) * 1.5;
                            col *= col * col;
                        }
                    } else if (rainRemaped < 0.55) {
                        float edgeGrid = saturate(((i.worldPosXY.x) % 1) * ((i.worldPosXY.y) % 1));
                        if (edgeGrid > 0.7) {
                            col = float4(rainRemaped, rainRemaped * 1.2, rainRemaped * 2, rainRemaped) * 1.5;
                            col *= col * col;
                        }
                    } else {
                        float fillGrid = saturate(((i.worldPosXY.x / 6) % 1) * ((i.worldPosXY.y / 6) % 1));
                        if (fillGrid > 0.9) {
                            col = float4(0, 0.2, 2, .25);
                        }
                    }
                    /*
                    * Storms
                    * 
                    if (tex.b > 0.9) {
                        float edgeGrid = saturate(((i.worldPosXY.x / 2) % 1) * ((i.worldPosXY.y / 2) % 1));
                        if (edgeGrid > 0.7) {
                            col = float4(1, 1, 0, .25);
                        }
                    }
                    */
                }
                col.a *= _Opacity;

                /*
                float fillGrid = saturate(((i.worldPosXY.x / 6) % 1) * ((i.worldPosXY.y / 6) % 1));
                col = tex;
                if (fillGrid > 0.5) {
                    col = float4(0, 0.2, 2, .25);
                }
                */

                // col.a *= saturate(1 - (i.projPos.z/500));

                UNITY_APPLY_FOG(i.fogCoord, col);

                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float projZ = i.projPos.z;
                float fade = saturate(_InvFade * (sceneDepth - projZ));
                col.a *= fade;

                return col * 2;
            }
            ENDCG
        }
    }
}
