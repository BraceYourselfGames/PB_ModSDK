Shader "Unlit/Billboard"
{
	Properties
	{
        [HDR]
        _Color ("Main color", Color) = (1, 1, 1, 1)
        _Opacity ("Mode resistant opacity", Range (0, 1)) = 1.0
		_MainTex ("Texture", 2D) = "white" {}
        [PowerSlider (2.0)]
        _InvFade ("Contact fade factor", Range (0.01,40.0)) = 1.0

        [Header (SrcAlpha OneMinusSrcAlpha    Traditional)]
        [Header (One OneMinusSrcAlpha             Premultiplied)]
        [Header (One One                                     Additive)]
        [Header (OneMinusDstColor One              Soft Additive)]
        [Header (DstColor Zero                            Multiplicative)]

        [Enum (UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1 //"One"
        [Enum (UnityEngine.Rendering.BlendMode)] _DstBlend ("DestBlend", Float) = 0 //"Zero"
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }

		ZWrite Off
		// Blend SrcAlpha OneMinusSrcAlpha
        Blend[_SrcBlend][_DstBlend]

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
				float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD1;
			};

            float4 _Color;
            float _Opacity;
			sampler2D _MainTex;
			float4 _MainTex_ST;
            float _SrcBlend;
            float _DstBlend;
            sampler2D_float _CameraDepthTexture;
            float _InvFade;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;

				float3 scale = float3(
					length(unity_ObjectToWorld._m00_m10_m20),
					length(unity_ObjectToWorld._m01_m11_m21),
					length(unity_ObjectToWorld._m02_m12_m22)
					);

				unity_ObjectToWorld._m00_m10_m20 = float3(scale.x, 0, 0);
				unity_ObjectToWorld._m01_m11_m21 = float3(0, scale.y, 0);
				unity_ObjectToWorld._m02_m12_m22 = float3(0, 0, scale.z);

				// billboard mesh towards camera
				float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);

				o.pos = outPos;
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH (o.projPos.z);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ (_CameraDepthTexture, UNITY_PROJ_COORD (i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ - partZ));

				float4 col = tex2D(_MainTex, i.uv) * _Color;
                if (_SrcBlend == 1)
                    col *= saturate (_Opacity * fade);
                else
                    col.a *= _Opacity * fade;

				return col;
			}
			ENDCG
		}
	}
}
