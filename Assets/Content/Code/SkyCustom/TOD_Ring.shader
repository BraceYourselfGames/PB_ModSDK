Shader "Time of Day/Ring"
{
    Properties
    {
        _MainTex ("Main map", 2D) = "black" {}
        _ShadowTex ("Shadow map", 2D) = "white" {}
        _Brightness ("Brightness", Range (0, 16)) = 1
        _FadePower ("Fade power", Range (1, 4)) = 2
        _ShadowIntensity ("Shadow intensity", Range (0, 1)) = 1
        _ShadowWidth ("Shadow width", Range (0.5, 2)) = 1
        _ShadowHeight ("Shadow height", Range (0.5, 2)) = 1
        _ShadowOffsetX ("Shadow offset on X", Range (-1, 1)) = 0
        _ShadowOffsetY ("Shadow offset on Y", Range (-1, 1)) = 0
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    uniform sampler2D _MainTex;
    uniform float4 _MainTex_ST;

    uniform sampler2D _ShadowTex;
    uniform float4 _ShadowTex_ST;

    uniform float _Brightness;
    uniform float _FadePower;

    uniform float _ShadowIntensity;
    uniform float _ShadowWidth;
    uniform float _ShadowHeight;
    uniform float _ShadowOffsetX;
    uniform float _ShadowOffsetY;

    uniform float4 _WeatherParameters;

    struct v2f
    {
        float2 uv_main : TEXCOORD0;
        float2 uv_shadow : TEXCOORD1;
        float4 position : SV_POSITION;
        float4 viewdir  : TEXCOORD2;
    };

    v2f vert (appdata_full v)
    {
        v2f o;

        o.position = UnityObjectToClipPos (v.vertex);

        float3 vertnorm = normalize (v.vertex.xyz);
        float3 worldNormal = normalize (mul ((float3x3)unity_ObjectToWorld, vertnorm));

        o.viewdir.xyz = vertnorm;
        o.viewdir.w = saturate (_Brightness * (1 - pow (1 - worldNormal.y, _FadePower))); // * TOD_StarVisibility

        o.uv_main = TRANSFORM_TEX (v.texcoord, _MainTex);
        o.uv_shadow = v.texcoord.xy * float2 ((1 / _ShadowWidth), 0.125 * (1 / _ShadowHeight)) + float2 (_ShadowOffsetX, _ShadowOffsetY) + float2 (0.5 - ((1 / _ShadowWidth) / 2), 0.5 - (0.125 * (1 / _ShadowHeight)) / 2);

        return o;
    }

    half4 frag (v2f i) : COLOR
    {
        half3 color = tex2D (_MainTex, i.uv_main).rgb * i.viewdir.w;
        half3 shadow = tex2D (_ShadowTex, i.uv_shadow).rgb;
        color = color * (1 - (1 - shadow) * _ShadowIntensity);

        // Fade the ring if there's precipitation (fog doesn't affect it, so we manually dim the ring)
		// Rain Intensity + Snowfall Intensity
		float precipitationIntensity = saturate (_WeatherParameters.x + _WeatherParameters.z);
        color *= 1 - precipitationIntensity;

        return half4 (color, 0);
    }

    ENDCG

    SubShader
    {
        Tags
        {
            "Queue" = "Background+10"
            "RenderType" = "Background"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            ZWrite Off
            ZTest LEqual
            Blend One OneMinusSrcAlpha
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }

    Fallback Off
}
