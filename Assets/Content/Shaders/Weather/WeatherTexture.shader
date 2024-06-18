Shader "PBWeatherTexture"
{
    Properties
    {
        _ControlTex("Control Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseOffsets("Noise Offsets", Vector) = (0, 0, 0, 0)
        _CampaignSeed("Campaign Seed", Float) = 0
        _ControlTexOffset("Control Tex Offset", Vector) = (0, 0, 0, 0)
        _SmallCloudScale("Small Cloud Scale", Float) = 5
        _SmallCloudIntensity("Small Cloud Intensity", Float) = 0.5
    }
    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            sampler2D _ControlTex;
            float4 _ControlTex_ST;

            float4 _NoiseOffsets;
            float4 _ControlTexOffset;

            float _CampaignSeed;

            float _SmallCloudScale;
            float _SmallCloudIntensity;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {

                float2 uv1 = IN.localTexcoord.xy;
                uv1.x += _NoiseOffsets.x + _CampaignSeed;
                uv1.y += _NoiseOffsets.y + _CampaignSeed;

                float2 uv2 = IN.localTexcoord.xy;
                uv2.x += _NoiseOffsets.z + (_CampaignSeed / 2);
                uv2.y += _NoiseOffsets.w + (_CampaignSeed / 2);

                float2 uv3 = IN.localTexcoord.xy;
                uv2.x += (_NoiseOffsets.z / _SmallCloudScale) + (_CampaignSeed / 3);
                uv2.y += (_NoiseOffsets.w / _SmallCloudScale) + (_CampaignSeed / 3);

                float2 uv_controlTex = IN.localTexcoord.xy;

                fixed rainNoise = ((tex2D(_NoiseTex, uv1).b - 0.5) * 2);
                rainNoise += ((tex2D(_NoiseTex, uv2).b - 0.5) * 2);
                rainNoise += ((tex2D(_NoiseTex, uv3 * _SmallCloudScale).b - 0.5) * 2) * _SmallCloudIntensity;

                fixed4 weatherController = tex2D(_ControlTex, uv_controlTex);
                float rainIntensity = saturate(rainNoise * weatherController.b * 2);
                rainIntensity += saturate(rainIntensity + ((weatherController.b - 0.8) * 5));

                return saturate(fixed4(rainNoise, rainNoise, rainIntensity, 1));

            }
            ENDCG
        }
    }
}
