Shader "Hardsurface/Invisible"
{
    //
    //
    // This shader was initially created for use with 'empty_slot' weapon prefab as an easy way to avoid rendering of small triangles
    // That are used as geometry placeholders inside the prefab
    //
    //
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata {};

            struct v2f {};

            v2f vert (appdata v)
            {
                v2f o;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                discard;
                return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
