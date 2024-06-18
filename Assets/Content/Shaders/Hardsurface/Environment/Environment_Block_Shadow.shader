// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hardsurface/Environment/Block (Shadow)"
{
    Properties
    {

    }
    SubShader
    {

        Tags
        {
            "RenderType" = "Opaque"
        }
        Cull Off
        Ztest less

        CGPROGRAM
        #pragma target 5.0
        #pragma only_renderers d3d11
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma surface surf Standard vertex:SharedShadowVertexFunction exclude_path:forward exclude_path:prepass noforwardadd nolppv noshadowmask novertexlights addshadow

        #include "UnityCG.cginc"
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Environment_Shared.cginc"
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float destructionAnimation = 0;
            
            if (destructionAnimation > 0.99)
            {
                clip(-1);
            }
            else
            {
                float4 detail = GetDetailSample(IN.worldPos1, IN.worldNormal1);

                // Finally, time for damage
                float damageBase = saturate(IN.damageIntegrityCriticality.x + destructionAnimation);
                if (damageBase > 0.001)
                {
                    // Extracting noise and using it to set opacity from damage, along with applying contrast to the noise
                    float detailNoise = saturate(
                        (detail.z - 0.5) * lerp(_GlobalEnvironmentDetailContrast, 1,
                                                destructionAnimation) + 0.5 - 0.25);
                    float detailStructure = detail.y / 2;

                    // Noise is a rich set of uniformly brigtness-distributed values which damage pushes below zero to create the cuts
                    float subtractionTestNoise = detailNoise - damageBase * 1.25;
                    float subtractionTestFull = subtractionTestNoise + detailStructure * 0.25;
                    clip(subtractionTestFull);
                }
            }
        }
        ENDCG
    }
}