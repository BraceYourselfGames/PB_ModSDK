Shader "Custom/X-Ray For Editor Use"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RampTex ("Ramp", 2D) = "white" {}

        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _ContactColor ("Contact Color", Color) = (1, 1, 1, 1)
        _DeepColor ("Deep Color", Color) = (0, 0, 0, 1)
        _Opacity ("Opacity", Range (0, 1)) = 1
        _HSVInfluence ("HSV Influence", Range (0, 1)) = 0
        _Hue ("Hue", Range (0, 1)) = 0
        _InvFade ("Contact fade factor", Range (0,10)) = 1.0
        _InvFadeDeep ("Deep fade factor", Range (0,10)) = 1.0
        _InvFadeOuter ("Outer fade factor", Range (0,10)) = 0
        _Brightness ("Brightness", Range (0.1, 6)) = 3.0
        _AmbientMultiplier ("Ambient multiplier", Range (0, 4)) = 2.0
        
        _SizeAdjustmentAmount ("Size adjustment amount", Range (0, 1)) = 0
        _SizeAdjustment ("Resize over distance (from, to, size mul., brightness mul.)", Vector) = (50, 100, 1, 0) 
    }

    SubShader
    {    
        // Regular color & lighting pass
        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100
            ColorMask RGB
            ZWrite On

            // Write to Stencil buffer (so that silouette pass can read)
            Stencil
            {
                Ref 4
                Comp always
                Pass replace
                ZFail keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

            sampler2D _MainTex;
            sampler2D _RampTex;
            sampler2D_float _CameraDepthTexture;
            float _InvFade;
            float _InvFadeOuter;
            half4 _Color;
            half4 _ContactColor;
            half _Brightness;
            half _AmbientMultiplier;
            float _Hue;
            float _HSVInfluence;
            float _Opacity;

            float _SizeAdjustmentAmount;
            float4 _SizeAdjustment;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 projPos : TEXCOORD3;
                fixed4 diff : COLOR0;
                float scaleInterpolant : TEXTCOORD4;
            };

            v2f vert (appdata_full v)
            {
                v2f o;

                float4 clipPosUnmodified = UnityObjectToClipPos (v.vertex);
                o.projPos = ComputeScreenPos (clipPosUnmodified);
                half3 wn = UnityObjectToWorldNormal (v.normal);
                o.diff = max (0, dot (wn, _WorldSpaceLightPos0.xyz)) * _LightColor0;
                o.diff.rgb += ShadeSH9 (half4(wn, 1));
                COMPUTE_EYEDEPTH (o.projPos.z);

                // Calculate the distance between camera and object's pivot and make it a 0-1 gradient over the specified distance
				// We also slightly offset initial distance values to make meshes appear\disappear at the moment of  overlap between LODs

                float3 worldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
				float dist = distance (_WorldSpaceCameraPos, worldPos);
                o.scaleInterpolant = saturate ((dist - _SizeAdjustment.x) / max (1, _SizeAdjustment.y)) * _SizeAdjustmentAmount;

                v.vertex = lerp
                (
                    v.vertex,
                    v.vertex * _SizeAdjustment.z,
                    o.scaleInterpolant
                );
                
                float4 clipPosModified = UnityObjectToClipPos (v.vertex);
                o.vertex = clipPosModified;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (i.projPos)));
                float partZ = i.projPos.z;
                float diffZ = sceneZ - partZ;
                float fade = saturate (_InvFade * diffZ);
                fade *= 1 - saturate (_InvFadeOuter * (sceneZ - partZ));

                float4 color = lerp (_ContactColor, _Color, fade);
                float3 colorHSV = RGBToHSV (color.xyz);
                colorHSV.x = _Hue;
                color.xyz = lerp (color.xyz, HSVToRGB (colorHSV), _HSVInfluence) * _Brightness;
                color.xyz *= lerp (1, _SizeAdjustment.w, i.scaleInterpolant);
                
                color.a = _Opacity * fade;
                
                color.rgb += (i.diff.rgb * _AmbientMultiplier);
                return color;
            }
      

            ENDCG
        }


        // Silouette pass 1 (backfaces)
        Pass
        {
            Tags
            {
                "Queue" = "Transparent"
            }
            // Won't draw where it sees ref value 4
            Cull Front // draw back faces
            ZWrite Off
            ZTest Always
            Stencil
            {
                Ref 3
                Comp Greater
                Fail keep
                Pass replace
            }
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

            float4 _DeepColor;
            float4 _ContactColor;
            sampler2D_float _CameraDepthTexture;
            float _InvFade;
            float _InvFadeDeep;
            float _InvFadeOuter;
            half _Brightness;
            half _AmbientMultiplier;
            float _Hue;
            float _HSVInfluence;
            float _Opacity;

            float _SizeAdjustmentAmount;
            float4 _SizeAdjustment;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 projPos : TEXCOORD3;
                fixed4 diff : COLOR0;
                float scaleInterpolant : TEXTCOORD4;
            };

            v2f vert (appdata_full v)
            {
                v2f o;

                float4 clipPosUnmodified = UnityObjectToClipPos (v.vertex);
                o.projPos = ComputeScreenPos (clipPosUnmodified);
                half3 wn = UnityObjectToWorldNormal (v.normal);
                o.diff = max (0, dot (wn, _WorldSpaceLightPos0.xyz)) * _LightColor0;
                o.diff.rgb += ShadeSH9 (half4(wn, 1));
                COMPUTE_EYEDEPTH (o.projPos.z);

                // Calculate the distance between camera and object's pivot and make it a 0-1 gradient over the specified distance
				// We also slightly offset initial distance values to make meshes appear\disappear at the moment of  overlap between LODs

                float3 worldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
				float dist = distance (_WorldSpaceCameraPos, worldPos);
                o.scaleInterpolant = saturate ((dist - _SizeAdjustment.x) / max (1, _SizeAdjustment.y)) * _SizeAdjustmentAmount;

                v.vertex = lerp
                (
                    v.vertex,
                    v.vertex * _SizeAdjustment.z,
                    o.scaleInterpolant
                );
                
                float4 clipPosModified = UnityObjectToClipPos (v.vertex);
                o.vertex = clipPosModified;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (i.projPos)));
                float partZ = i.projPos.z;
                float fade = 1 - saturate (_InvFade * (partZ - sceneZ));
                float fadeDeep = (1 - saturate (_InvFadeDeep * (partZ - sceneZ)));
                fade *= 1 - saturate (_InvFadeOuter * (sceneZ - partZ));

                float4 color = lerp (_ContactColor, _DeepColor, fade);
                float3 colorHSV = RGBToHSV (color.xyz);
                colorHSV.x = _Hue;
                color.xyz = lerp (color.xyz, HSVToRGB (colorHSV), _HSVInfluence) * _Brightness;
                color.xyz *= lerp (1, _SizeAdjustment.w, i.scaleInterpolant);
                
                color.a = _Opacity * fade;
                
                color.rgb += (i.diff.rgb * _AmbientMultiplier);
                return float4(color.rgb, color.a * fadeDeep);
            }

            ENDCG
        }

        // Silouette pass 2 (front faces)
        Pass
        {
            Tags
            {
                "Queue" = "Transparent"
            }
            // Won't draw where it sees ref value 4
            Cull Back // draw front faces
            ZWrite Off
            ZTest Always
            Stencil
            {
                Ref 4
                Comp NotEqual
                Pass keep
            }
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

            float4 _DeepColor;
            float4 _ContactColor;
            sampler2D_float _CameraDepthTexture;
            float _InvFade;
            float _InvFadeDeep;
            float _InvFadeOuter;
            half _Brightness;
            half _AmbientMultiplier;
            float _Hue;
            float _HSVInfluence;
            float _Opacity;

            float _SizeAdjustmentAmount;
            float4 _SizeAdjustment;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 projPos : TEXCOORD3;
                fixed4 diff : COLOR0;
                float scaleInterpolant : TEXTCOORD4;
            };

            v2f vert (appdata_full v)
            {
                v2f o;

                float4 clipPosUnmodified = UnityObjectToClipPos (v.vertex);
                o.projPos = ComputeScreenPos (clipPosUnmodified);
                half3 wn = UnityObjectToWorldNormal (v.normal);
                o.diff = max (0, dot (wn, _WorldSpaceLightPos0.xyz)) * _LightColor0;
                o.diff.rgb += ShadeSH9 (half4(wn, 1));
                COMPUTE_EYEDEPTH (o.projPos.z);

                // Calculate the distance between camera and object's pivot and make it a 0-1 gradient over the specified distance
				// We also slightly offset initial distance values to make meshes appear\disappear at the moment of  overlap between LODs

                float3 worldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
				float dist = distance (_WorldSpaceCameraPos, worldPos);
                o.scaleInterpolant = saturate ((dist - _SizeAdjustment.x) / max (1, _SizeAdjustment.y)) * _SizeAdjustmentAmount;

                v.vertex = lerp
                (
                    v.vertex,
                    v.vertex * _SizeAdjustment.z,
                    o.scaleInterpolant
                );
                
                float4 clipPosModified = UnityObjectToClipPos (v.vertex);
                o.vertex = clipPosModified;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (partZ - sceneZ));
                float fadeDeep = (1 - saturate (_InvFadeDeep * (partZ - sceneZ)));
                fade *= 1 - saturate (_InvFadeOuter * (sceneZ - partZ));

                float4 color = lerp (_ContactColor, _DeepColor, fade);
                float3 colorHSV = RGBToHSV (color.xyz);
                colorHSV.x = _Hue;
                color.xyz = lerp (color.xyz, HSVToRGB (colorHSV), _HSVInfluence) * _Brightness;
                color.xyz *= lerp (1, _SizeAdjustment.w, i.scaleInterpolant);
                
                color.a = _Opacity * fade;

                color.rgb += (i.diff.rgb * _AmbientMultiplier);
                
                return float4(color.rgb, color.a * fadeDeep);
            }
         

            ENDCG
        }
    }
}