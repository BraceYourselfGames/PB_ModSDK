// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vegetation/Billboard (advanced)"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float3 _CameraUpForBillboards;
            
            v2f vert (appdata v)
            {
                v2f o;

                float3 center = v.vertex; //mul(_Object2World, v.vertex);
                float3 eyeVector = ObjSpaceViewDir (v.vertex);

				float3 upVector = _CameraUpForBillboards; // float3 (0, 1, 0);
                float3 sideVector = normalize (cross (eyeVector, upVector));

                float3 finalposition = center;
                finalposition += (v.uv.x - 0.5f)*sideVector;
                finalposition += (v.uv.y - 0.5f)*upVector;

                float4 pos = float4 (finalposition, 1);
                o.vertex = UnityObjectToClipPos (pos);
                o.uv = v.uv;

                // o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
