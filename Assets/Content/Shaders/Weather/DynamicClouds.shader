Shader "Unlit/DynamicClouds"
{
    Properties
    {
        _MainTex("Weather Texture", 2D) = "white" {}
        [HDR] _LightColor("Light Color", Color) = (1, 1, 1, 1)
        _CoreSaturation("Core Saturation", Float) = 1
        _CellSize("Cell Size", Float) = 50
        _WorldScale("World Scale", Float) = 5120
        _Cutoff("Cutoff", Float) = 0
        _AlphaMult("Alpha Mult", Float) = 1
        _BumpLevelHighlight("Bump Level (Highlights)", Float) = 1
        _LightDirection("Light Direction", Float) = 0
        _LightIntensity("Light Intensity", Float) = 0.5
        _Opacity("Opacity", Float) = 0.5
        _OpacityHighlight("Opacity (Highlights)", Float) = 0.5
        _OpacityShadow("Opacity (Shadow)", Float) = 0.5
        _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseOffsets("Noise Offsets", Vector) = (0, 0, 0, 0)
        _NoiseStrength("Noise Strength", Float) = 0
        _NoiseScale("Noise Scale", Float) = 1
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
                float4 projPos : TEXCOORD5;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _LightColor;
            float _CoreSaturation;
            float _Cutoff;
            float _AlphaMult;
            float _BumpLevelHighlight;
            float _LightDirection;
            float _LightIntensity;
            float _Opacity;
            float _OpacityHighlight;
            float _OpacityShadow;

            float _CellSize;
            float _WorldScale;

            sampler2D _CameraDepthTexture;
            float _InvFade;

            sampler2D _NoiseTex;
            float4 _NoiseOffset;
            float _NoiseStrength;
            float _NoiseScale;

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


            // https://forum.unity.com/threads/sobel-operator-height-to-normal-map-on-gpu.33159/

                float3 height2normal_sobel(float3x3 c)
                {
                    float3x3 x = float3x3(1.0, 0.0, -1.0,
                        2.0, 0.0, -2.0,
                        1.0, 0.0, -1.0);

                    float3x3 y = float3x3(1.0, 2.0, 1.0,
                        0.0, 0.0, 0.0,
                        -1.0, -2.0, -1.0);

                    x = x * c;
                    y = y * c;

                    float cx = x[0][0] + x[0][2]
                        + x[1][0] + x[1][2]
                        + x[2][0] + x[2][2];

                    float cy = y[0][0] + y[0][1] + y[0][2]
                        + y[2][0] + y[2][1] + y[2][2];

                    float cz = sqrt(1 - clamp((cx * cx + cy * cy), -1, 1));

                    return float3(cx, cy, cz);
                }

                float3x3 img3x3(sampler2D color_map, float2 tc, float ts, int ch)
                {
                    float   d = 1.0 / ts; // ts, texture sampling size
                    float3x3 c;
                    c[0][0] = tex2D(color_map, tc + float2(-d, -d))[ch];
                    c[0][1] = tex2D(color_map, tc + float2(0, -d))[ch];
                    c[0][2] = tex2D(color_map, tc + float2(d, -d))[ch];

                    c[1][0] = tex2D(color_map, tc + float2(-d, 0))[ch];
                    c[1][1] = tex2D(color_map, tc)[ch];
                    c[1][2] = tex2D(color_map, tc + float2(d, 0))[ch];

                    c[2][0] = tex2D(color_map, tc + float2(-d, d))[ch];
                    c[2][1] = tex2D(color_map, tc + float2(0, d))[ch];
                    c[2][2] = tex2D(color_map, tc + float2(d, d))[ch];

                    return c;
                }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 sample = tex2D(_MainTex, i.worldPosXY).b;
                float4 col = sample - _Cutoff;

                col.a = saturate((col.b - 0.2) * 10);

                float lightDirection = _LightDirection * 6.283;

                float3x3 c = img3x3(_MainTex, i.worldPosXY, 512, 2);
                float3x3 n = img3x3(_NoiseTex, (i.worldPosXY * _NoiseScale) + float2(_NoiseOffset.x, _NoiseOffset.y), 512, 0);

                float3 normal = height2normal_sobel(c);
                float3 normalNoise = height2normal_sobel(n);
                normal = normal + (normalNoise * _NoiseStrength);
                normal = normalize(float3(normal.xy, normal.z * _BumpLevelHighlight)) * -1;

                float lighting = 0;
                lighting += sin(lightDirection) * normal.r;
                lighting += cos(lightDirection) * normal.g;
                
                float highlight = saturate(lighting);
                float shadow = saturate(lighting * -1);

                float3 shadowColor = float3(1 - _LightColor.r, 1 - _LightColor.g, 1 - _LightColor.b);
                shadowColor = lerp(shadowColor, dot(float3(0.299, 0.587, 0.114), shadowColor), 0.5);

                col.a *= _Opacity;

                col.rgb += shadow * shadowColor * _OpacityShadow * _LightIntensity;
                col.rgb += highlight * _LightColor * _OpacityHighlight * _LightIntensity;
                
                float fadeByDistance = 1 - saturate (max (i.projPos.z - 600, 0) / 1000);
                col.a *= fadeByDistance;

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
