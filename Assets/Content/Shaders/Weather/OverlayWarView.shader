Shader "Unlit/Overlays/WarView"
{
    Properties
    {
        _MainTex ("Province Texture", 2D) = "white" {}
        _RampTex("Ramp Texture", 2D) = "white" {}
        _PatternTex("Pattern Texture", 2D) = "white" {}
        
        _OpacityCurrent ("Opacity (Current)", Range (0, 1)) = 1
        _MaskColorCurrent ("Mask Color (Current)", Color) = (1, 0.5, 0, 0)
        _RampColorCurrent ("Selection Tint", Color) = (1, 1, 1, 1)
        _RampInputsCurrent ("Ramp Inputs (Current)", Vector) = (0, 0, 0, 0)
        
        _OpacityWar ("Opacity (War)", Range (0, 1)) = 1
        _MaskColorWar ("Mask Color (War)", Color) = (1, 0.5, 0, 0)
        _RampColorWar ("Selection Tint", Color) = (1, 1, 1, 1)
        _RampInputsWar ("Ramp Inputs (War)", Vector) = (0, 0, 0, 0)

        _SelectionRange ("Selection Range", Float) = 0
        _SelectionFuzziness ("Selection Fuzziness", Float) = 1
        _WorldScale ("World Scale", Float) = 3072
        _Cutoff ("Cutoff", Float) = 0
        _Opacity ("Opacity", Range (0,1)) = 0.5
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
            sampler2D _RampTex;
            sampler2D _PatternTex;
            float4 _PatternTex_ST;

            float4 _MaskColorCurrent;
            float4 _RampColorCurrent;
            float4 _RampInputsCurrent;
            float _OpacityCurrent;

            float4 _MaskColorWar;
            float4 _RampColorWar;
            float4 _RampInputsWar;
            float _OpacityWar;
            
            float _SelectionRange;
            float _SelectionFuzziness;
            
            float _Cutoff;
            float _Opacity;
            float _Outline;
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
                o.worldPosXY = float2(o.worldPos.x + _MainTex_ST.z, o.worldPos.z + _MainTex_ST.w) / _WorldScale;

                o.projPos = ComputeScreenPos(o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 texProvinces = tex2D (_MainTex, i.worldPosXY);
                float4 patternSample = tex2D (_PatternTex, i.worldPosXY * _WorldScale * _PatternTex_ST.xy);
                float4 finalColor = float4 (0, 0, 0, 0);

                // Current province
                if (_OpacityCurrent > 0.01)
                {
                    float crSelectionDistance = distance (_MaskColorCurrent.xyz, texProvinces.xyz);
                    float crSelectionMask = saturate (1 - (crSelectionDistance - _SelectionRange) / max (_SelectionFuzziness, 1e-5));

                    // X shifts ramp texcoord, Y of allowsto flip the texture direction via multiplication (with texture clamped, that necessitates an additional offset)
                    float crRampCoord = _RampInputsCurrent.x + _RampInputsCurrent.y * ((1 - texProvinces.w) * crSelectionMask) + saturate (-_RampInputsCurrent.y);
                    float crRampCoordFrac = frac (crRampCoord); // fractional version of coord can be useful for detail texturing
                
                    float4 crRampSample = tex2D (_RampTex, float2 (crRampCoord, 0)).x;
                    float crMask = saturate ((1 - pow (texProvinces.w, 16)) * (1 - pow ((1 - texProvinces.w), 16)) * crSelectionMask);
                    float4 crColor = float4 ((crRampSample.xyz * _RampColorCurrent.xyz) * 0.35, crMask * crRampSample.x * _RampInputsCurrent.w);

                    // Detail texturing
                    float detail = 0;
                    if (crRampCoordFrac < 0.2)
                        detail = patternSample.x;
                    else if (crRampCoordFrac < 0.4)
                        detail = patternSample.y;
                    else
                        detail = patternSample.w;

                    crColor.xyz += _RampColorCurrent.xyz * detail;
                    crColor.w *= _OpacityCurrent;
                    finalColor += crColor * crSelectionMask;
                }

                // War province
                if (_OpacityWar > 0.01)
                {
                    float warSelectionDistance = distance (_MaskColorWar.xyz, texProvinces.xyz);
                    float warSelectionMask = saturate (1 - (warSelectionDistance - _SelectionRange) / max (_SelectionFuzziness, 1e-5));

                    // X shifts ramp texcoord, Y of allowsto flip the texture direction via multiplication (with texture clamped, that necessitates an additional offset)
                    float warRampCoord = _RampInputsWar.x + _RampInputsWar.y * ((1 - texProvinces.w) * warSelectionMask) + saturate (-_RampInputsWar.y);
                    float warRampCoordFrac = frac (warRampCoord); // fractional version of coord can be useful for detail texturing
                
                    float4 warRampSample = tex2D (_RampTex, float2 (warRampCoord, 0)).x;
                    float warMask = saturate ((1 - pow (texProvinces.w, 16)) * (1 - pow ((1 - texProvinces.w), 16)) * warSelectionMask);
                    float4 warColor = float4 ((warRampSample.xyz * _RampColorWar.xyz) * 0.35, warMask * warRampSample.x * _RampInputsWar.w);

                    warColor += float4 (_RampColorWar.xyz * warMask, warMask * 0.1);

                    // Detail texturing
                    float detail = 0;
                    if (warRampCoordFrac < 0.2)
                        detail = patternSample.x;
                    else if (warRampCoordFrac < 0.4)
                        detail = patternSample.y;
                    else
                        detail = patternSample.w;

                    warColor.xyz += _RampColorWar.xyz * detail;
                    warColor.w *= _OpacityWar;
                    finalColor += warColor * warSelectionMask;

                    // finalColor = float4 (warSelectionMask.xxx, 1);
                }
                
                UNITY_APPLY_FOG (i.fogCoord, finalColor);

                // float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                // float projZ = i.projPos.z;
                // float fade = saturate(_InvFade * (sceneDepth - projZ));
                // col.a *= fade;

                return finalColor;
            }
            ENDCG
        }
    }
}
