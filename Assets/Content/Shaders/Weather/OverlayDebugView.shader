Shader "Unlit/Overlays/DebugViewProvinceArea"
{
    Properties
    {
        _MainTex ("Province Texture", 2D) = "white" {}
        _SeasonTex ("Season Texture", 2D) = "white" {}
        _WeatherTex ("Weather Texture", 2D) = "white" {}
        
        _SelectionTint ("Selection Tint", Color) = (1, 1, 1, 1)
        _SelectionColor ("Selection Color", Color) = (1, 0.5, 0, 0)
        _SelectionRange ("Selection Range", Float) = 0
        _SelectionFuzziness ("Selection Fuzziness", Float) = 1
        _WorldScale ("World Scale", Float) = 3072
        _Cutoff ("Cutoff", Float) = 0
        _Opacity ("Opacity", Range (0,1)) = 0.5
        _Mode ("Mode", Range (0,1)) = 0
        _SeasonColorBase ("Season Color (Base)", Color) = (0.5, 0.6, 0, 0)
        _SeasonColorAutumn ("Season Color (Autumn)", Color) = (0.8, 0.4, 0, 0)
        _SeasonColorWinter ("Season Color (Winter)", Color) = (0.6, 0.7, 0.8, 0)
        _Outline ("Outline", Float) = 0
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
        _BlurOffset ("Blur Offset", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
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

            sampler2D _SeasonTex;
            sampler2D _WeatherTex;
            
            float4 _Tint;

            float4 _SelectionTint;
            float4 _SelectionColor;
            float _SelectionRange;
            float _SelectionFuzziness;
            
            float _Cutoff;
            float _Opacity;
            float _Mode;
            float _Outline;
            float _WorldScale;

            float4 _SeasonColorBase;
            float4 _SeasonColorAutumn;
            float4 _SeasonColorWinter;

            sampler2D _CameraDepthTexture;
            float _InvFade;

            float _BlurOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPosXY = float2(o.worldPos.x + _MainTex_ST.z, o.worldPos.z + _MainTex_ST.w) / _WorldScale;

                o.projPos = ComputeScreenPos(o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 worldPosNoScale = i.worldPosXY * _WorldScale * 0.5;
                float medGrid = saturate(((worldPosNoScale.x / 4) % 1) * ((worldPosNoScale.y / 4) % 1) - 0.8) * 5;
                float smallGrid = saturate(((worldPosNoScale.x / 2) % 1) * ((worldPosNoScale.y / 2) % 1) - 0.6) * 2.5;
                float tinyGrid = saturate(((worldPosNoScale.x) % 1) * ((worldPosNoScale.y) % 1) - 0.6) * 2.5;
                
                float4 texProvinces = tex2D (_MainTex, i.worldPosXY);
                float selectionDistance = distance (_SelectionColor.xyz, texProvinces.xyz);
                float selectionMask = saturate (1 - (selectionDistance - _SelectionRange) / max (_SelectionFuzziness, 1e-5));
                float3 colProvinces = lerp (texProvinces.xyz, texProvinces.xyz * 0.3 + _SelectionTint.xyz * selectionMask, _SelectionTint.a);
                colProvinces += smallGrid * _SelectionTint.xyz * _SelectionTint.a * selectionMask;
                colProvinces += smallGrid * colProvinces;
                
                float4 texSeasons = tex2D (_SeasonTex, i.worldPosXY);
                float3 colSeasons = lerp (_SeasonColorBase, _SeasonColorAutumn, texSeasons.x);
                colSeasons = lerp (colSeasons, _SeasonColorWinter, texSeasons.y);
                colSeasons += smallGrid * colSeasons;

                float4 texWeather = tex2D (_WeatherTex, i.worldPosXY);
                float3 colWeather = texWeather.xyz;

                float4 col = float4(0, 0, 0, _Opacity);

                if (_Mode < 0.5)
                {
                    col.xyz = colProvinces;
                    col.xyz = saturate (col.xyz * 1 - texSeasons.w * 0.5);
                }

                else if (_Mode < 1.5)
                {
                    col.xyz = colSeasons;
                    col.xyz = saturate (col.xyz * 1 - texSeasons.w * 0.5);
                }

                else if (_Mode < 2.5)
                {
                    col.xyz = texWeather.xyz * 4 + texWeather.a * 2;
                    col.a *= texWeather.b;
                }
                
                UNITY_APPLY_FOG (i.fogCoord, col);

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
