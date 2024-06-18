Shader "Vegetation/Billboard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.6)
		_ColorVariation ("Color variation", Color) = (1,1,1,0)
        _ColorVariationUseVerticalGradient ("Color variation - mask by vertical gradient", Range (0, 1)) = 1

        [Space(5)]
        [Enum (UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
		[NoScaleOffset] _MainTex ("Albedo (RGB), opacity (A)", 2D) = "white" {}
		[NoScaleOffset] _Bump ("Normal (RGB)", 2D) = "bump" {}

        _Cutoff ("Alpha cutoff", Range (0, 1)) = 0.3
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1
		_OcclusionFromUV ("Occlusion from UV.y gradient", Range (0, 1)) = 0
        _TranslucencyIntensity ("Translucency Intensity", Range (0, 4)) = 0.65
		_AlbedoBoost ("Albedo boost", Range (0, 1)) = 0

        _WindSpeed("Wind Speed", Range(0, 5)) = 0.0
        _WindIntensity("Wind Intensity", Range(0, 0.01)) = 0.004

        [Space(20)]
        _WeatherMultiplier ("Weather Multiplier", Range (0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
            "RenderType" = "AlphaTest"
        }

        LOD 200
        Cull[_Culling]

        CGPROGRAM
        #pragma surface surf Standard vertex:vert alphatest:_Cutoff
        #pragma target 5.0

        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Assets/Content/Shaders/Hardsurface/Environment/Environment_Shared.cginc"

        sampler2D _MainTex;
		sampler2D _Bump;

		float4 _Color;
        float4 _ColorVariation;
        float _ColorVariationUseVerticalGradient;
		float _NormalIntensity;
		float _OcclusionFromUV;
        float _TranslucencyIntensity;
		float _AlbedoBoost;

        float _WindSpeed;
        float _WindIntensity;
        float _AfsFoliageWaveSize;

		float4 _CombatVegetationColor1;
		float4 _CombatVegetationColor2;

        UNITY_INSTANCING_BUFFER_START (Props)
            // UNITY_DEFINE_INSTANCED_PROP (float, _BillboardIndex)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT (Input, o);

            o.texcoord_uv1 = v.texcoord;

            // Taken from AfsFoliageBendingInstanced.cginc - needed for color variation
            float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;
            float3 variations = abs (frac (worldPos.xyz * _AfsFoliageWaveSize) - 0.5);
            float verticalMaskForLeaves = lerp (1.0f, saturate (pow (o.texcoord_uv1.y * 2, 2)), _ColorVariationUseVerticalGradient);
            o.colorVariation = dot (variations, float3(1,1,1)) * verticalMaskForLeaves;

            // Use this as initial sine wave offset to randomize tree's burning noise mask UVs
            float3 pivotWorldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
            float positionBasedOffset = (pivotWorldPos.x + pivotWorldPos.y + pivotWorldPos.z) * 0.15;

            // Simple wind noise
            v.vertex.xz += sin (v.vertex + positionBasedOffset + _Time.y * _WindSpeed) * ((sin (positionBasedOffset + _Time.y) + 1) * 0.5) * _WindIntensity * verticalMaskForLeaves;

            // get the camera basis vectors
            float3 forward = normalize (UNITY_MATRIX_V._m20_m21_m22);
            float3 up = float3(0, 1, 0); // normalize(UNITY_MATRIX_V._m10_m11_m12);
            float3 right = -normalize (UNITY_MATRIX_V._m00_m01_m02);

            // rotate to face camera
            float4x4 rotationMatrix = float4x4
            (
                right, 0,
                up, 0,
                forward, 0,
                0, 0, 0, 1
            );

            // apply the resulting matrix
            v.vertex = mul (v.vertex, rotationMatrix);
            v.normal = mul (v.normal, rotationMatrix);
			v.tangent = mul (v.tangent, rotationMatrix);

            // mul by float3x3 version of world-to-object matrix, which results in inverted rotation and inverted scale being applied
            v.vertex.xyz = mul (v.vertex.xyz, (float3x3)unity_ObjectToWorld);
            v.normal.xyz = mul (v.normal.xyz, (float3x3)unity_ObjectToWorld);
			v.tangent.xyz = mul (v.tangent.xyz, (float3x3)unity_ObjectToWorld);

            // but we do not want any scaling applied, so we cancel out the influence of the prior scaling by applying WTO scale
            float scaleX = abs (unity_WorldToObject._m11);
            float3 scale = float3 (1, 1, 1) * scaleX;
            v.vertex.xyz *= scale;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // temporarily unused index based UV generation for billboard atlases
            // unnecessary if texture arrays would be used

            // float index = UNITY_ACCESS_INSTANCED_PROP (_BillboardIndex);
            // float2 uv = IN.texcoord_uv1 * 0.125;
            // uv.x += frac (index * 0.125);
            // uv.y += frac ((7 - floor (index / 8)) * 0.125);

            fixed4 albedoTex = tex2D (_MainTex, IN.texcoord_uv1);
			albedoTex.rgb = lerp (albedoTex.rgb, saturate (albedoTex.rgb * 4), _AlbedoBoost);
            float albedoMaskForLeaves = saturate(RGBToGrayscale(albedoTex.rgb) * 8);

			//float3 albedoFinal = lerp (albedoTex.rgb, (albedoTex.rgb * _ColorVariation.rgb), IN.colorVariation * _ColorVariation.a);

			float variationOverrideStrength = _CombatVegetationColor1.w;
			float3 variationOverrideColor1 = _CombatVegetationColor1.xyz;
			float3 variationOverrideColor2 = _CombatVegetationColor2.xyz;
			float3 variationOverrideColor = lerp (variationOverrideColor1, variationOverrideColor2, IN.colorVariation);

			// Blend original color variation setup from per-material properties
			float3 albedoTinted = lerp (albedoTex.rgb * _Color.rgb, albedoTex.rgb * _ColorVariation.rgb, IN.colorVariation * _ColorVariation.a);

			// Blend global color variation
			float variationGlobalInfluence = 1 - _Color.a; // Only vegetation with 0 local color A accepts global color
			float3 albedoFinal = lerp (albedoTinted, Overlay (albedoTinted, variationOverrideColor), albedoMaskForLeaves * variationOverrideStrength * variationGlobalInfluence);

            float smoothnessFinal = 0.1;

            float translucencyMask = 1 - saturate((1 - abs (IN.texcoord_uv1.x - 0.5) * 2) * (1 - IN.texcoord_uv1.y) * 3);

            float2 translucencyFinal = float2(saturate((translucencyMask + 0.1) * albedoMaskForLeaves * _TranslucencyIntensity), 1);
			float3 normalFinal = lerp (fixed3 (0, 0, 1), UnpackNormal (tex2D (_Bump, IN.texcoord_uv1.xy)), _NormalIntensity);
            float occlusionFinal = lerp (1, IN.texcoord_uv1.y, _OcclusionFromUV);

            float3 emissionFinal = 0;

            float metalnessFinal = 0;

			if (_WeatherMultiplier > 0)
			{
                float verticalFactor2 = IN.texcoord_uv1.y * albedoMaskForLeaves;
                verticalFactor2 = saturate (pow(verticalFactor2, 2) * 2);
				float weatherOcclusionMask = 1.0f;
				ApplyWeatherLightweight (_WeatherMultiplier, albedoFinal, smoothnessFinal, metalnessFinal, IN.worldPos1, verticalFactor2, weatherOcclusionMask);
			}

			o.Albedo = albedoFinal;
			o.Smoothness = smoothnessFinal;
			o.Normal = normalFinal;
			o.Occlusion = occlusionFinal;
			o.Alpha = albedoTex.a;
            o.Emission = emissionFinal;
        }
        ENDCG
    }
    FallBack Off
}

// reference for constructing matrices
/*

    float4x4 w = unity_WorldToObject;
    float3x3 matrixCopy_RotationAndScale = float3x3
    (
    w._m00_m10_m20,
    w._m01_m11_m21,
    w._m02_m12_m22
    );

    float3x3 matrixCopy_Rotation = float3x3
    (
    1, w._m10, w._m20,
    w._m01, 1, w._m21,
    w._m02, w._m12, 1
    );

    float4 RotateAroundYInDegrees (float4 vertex, float degrees)
    {
        float alpha = degrees * UNITY_PI / 180.0;
        float sina, cosa;
        sincos (alpha, sina, cosa);
        float2x2 m = float2x2(cosa, -sina, sina, cosa);
        return float4(mul (m, vertex.xz), vertex.yw).xzyw;
    }

    float4x4 GetRotationMatrix (float3 r, float4 d)
    {
        float cx, cy, cz, sx, sy, sz;
        sincos (r.x, sx, cx);
        sincos (r.y, sy, cy);
        sincos (r.z, sz, cz);
        return float4x4
        (
            cy * cz, -sz, sy, d.x,
            sz, cx * cz, -sx, d.y,
            -sy, sx, cx * cy, d.z,
            0, 0, 0, d.w
        );
    }
*/