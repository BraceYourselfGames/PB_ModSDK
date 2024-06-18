Shader "Hardsurface/Parts/Base (projectile)" 
{
	Properties
	{
		_HSBOffsetsPrimary ("HSB offsets (primary)", Vector) = (0, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary)", Vector) = (0, 0.5, 0.5, 1)

        [Space (20)]
		_MainTex ("AH", 2D) = "white" {}
        [NoScaleOffset]
		_MSEO ("MSEO", 2D) = "white" {}
        [NoScaleOffset]
		_Bump ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1

        [Space (20)]
		_SmoothnessMin ("Smoothness (min.)", Range (0, 1)) = 0.0
		_SmoothnessMed ("Smoothness (med.", Range (0, 1)) = 0.5
		_SmoothnessMax ("Smoothness (max.", Range (0, 1)) = 1

		[Space (20)]
        [HDR]
		_EmissionColor ("Emission color", Color) = (1, 1, 1, 1)
		_OcclusionIntensity ("Occlusion intensity", Range(0, 1)) = 1.0

        [Space (20)]
        _Visibility ("Visibility", Range (0, 1)) = 1
        _Destruction ("Destruction", Range (0, 1)) = 0

        [Space (20)]
        [HDR]
        _BurnColor ("Burn color", Color) = (1,1,1,1)
        [NoScaleOffset]
        _BurnTex ("Burn map", 2D) = "white" {}
        _BurnScale ("Burn map scale", Range (0.1, 10)) = 1
        _BurnOffset ("Burn offset", Vector) = (0, 0, 0, 0)
        [NoScaleOffset]
        _BurnRamp ("Burn ramp", 2D) = "white" {}
        _BurnRampSize ("Burn ramp size", Range (0.0, 1.0)) = 0.25

        [Space (20)]
        _CrushAddition ("Crush addition", Range (0, 4)) = 2
        _CrushMultiplier ("Crush multiplier", Range (-2, 0)) = -0.75
        _CrushParameters ("Crush position (XYZ)", Vector) = (0, 0, 0, 1)

        [Space (20)]
        _StripPower ("Stripping power", Range (2, 16)) = 16
        _StripDistance ("Stripping distance", Float) = 20
        _StripParameters ("Stripping direction (XYZ)", Vector) = (0, 0, -1, 0)

        [Space (20)]
        [Toggle(USE_MISSILE_FLAPS_ANIM)] _UseMissileFlapsAnim("Use missile flaps animation", Int) = 0
        [HideIfDisabled(USE_MISSILE_FLAPS_ANIM)] _MissileFlapsAnimProgress ("Missile Flaps Animation", Range (0, 1)) = 0

	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
		}
		Cull Off
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard vertex:vert addshadow
		#pragma target 5.0
        #pragma shader_feature_local USE_MISSILE_FLAPS_ANIM

        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

        struct Input
        {
	        float2 uv_MainTex;
	        float4 color : COLOR;
	        float3 localPos;
	        float3 localNormal;
	        float3 worldPos1;
	        float3 worldNormal1;
	        float3 viewDir;
	        float4 screenPos;
	        float eyeDepth;
            float destruction;
            float distance;
        };

        float4 _HSBOffsetsPrimary;
        float4 _HSBOffsetsSecondary;

        sampler2D _MainTex;
        sampler2D _MSEO;
        sampler2D _Bump;

        fixed _NormalIntensity;
        half _SmoothnessMin;
        half _SmoothnessMed;
        half _SmoothnessMax;

        float4 _EmissionColor;
		fixed _OcclusionIntensity;

        float _Visibility;
        float _Destruction;

        fixed4 _BurnColor;
        sampler2D _BurnTex;
        float _BurnScale;
        float4 _BurnOffset;
        sampler2D _BurnRamp;
        float _BurnRampSize;

        float _CrushAddition;
        float _CrushMultiplier;
        float4 _CrushParameters;

        float _StripPower;
        float _StripDistance;
        float4 _StripParameters;

        float _MissileFlapsAnimProgress;

        UNITY_INSTANCING_BUFFER_START (Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o)
        {
	        UNITY_INITIALIZE_OUTPUT (Input, o);

	        o.localPos = v.vertex.xyz;
	        o.localNormal = v.normal;
	        o.worldPos1 = mul (unity_ObjectToWorld, v.vertex);
	        o.worldNormal1 = mul (unity_ObjectToWorld, float4 (v.normal, 0.0f)).xyz;
	        o.color = v.color;

            float3 difference = float3
            (
                o.localPos.x - _CrushParameters.x,
                o.localPos.y - _CrushParameters.y,
                o.localPos.z - _CrushParameters.z
            );

            float distance = sqrt
            (
                difference.x * difference.x +
                difference.y * difference.y +
                difference.z * difference.z
            );

            distance = saturate (distance * _CrushMultiplier + _CrushAddition);
            o.distance = distance;

            float destructionInput = _Destruction;
            half destructionFinal = lerp (destructionInput * distance, 1, pow (destructionInput, 8));
            o.destruction = destructionFinal;

            float adjustedDistance = lerp (pow (distance, _StripPower), 1, destructionFinal);

            // Missile flaps animation
            // ammo_missile_01.fbx: UV1.xy and UV2.x have pivot position encoded for each flap. UV2.y works as a mask for animation
            #ifdef USE_MISSILE_FLAPS_ANIM
                float3 pivotPos = float3(v.texcoord1.x, v.texcoord1.y, v.texcoord2.x);

                // Everything below 0.5 on UV2.y won't be affected by the animation
                if (v.texcoord2.y > 0.5)
                {
                    // Need this mask to reverse rotation angle for two flaps sitting on an opposite diagonal
                    float detectFlapsXYQuadrantMask = saturate( abs( dot(float3(0.5, 0.5, 0), pivotPos * 100) ) );
                    // Somehow it works by supplying pivotPos for rotation axis - this is unexpected and needs more research (both for shader and vertex data bake solutions)
                    v.vertex.xyz = RotateAboutAxis_Radians(v.vertex.xyz, pivotPos, _MissileFlapsAnimProgress * lerp(1, -1, detectFlapsXYQuadrantMask));
                }
            #endif

            v.vertex.xyz += adjustedDistance  * (v.normal + frac (sin (dot (v.vertex.xz, float2 (12.9898, 78.233))) * 43758.5453)) * destructionFinal;
            v.vertex.xyz = mul (unity_ObjectToWorld, v.vertex.xyz);
            v.vertex.xyz += adjustedDistance * _StripDistance * _StripParameters.xyz * destructionFinal * destructionFinal;

            // add position offset to the matrix, otherwise it's fairly useless since rotation moves most of the vertices underground with most pivots
            // float3x3 rotation = AngleAxis3x3 (destructionInput * 600 * 0.0174533, _StripParameters.xyz);
            // float3 rotatedPosition = mul (rotation, v.vertex.xyz);
            // v.vertex.xyz = lerp (v.vertex.xyz, rotatedPosition, adjustedDistance);

            v.vertex.xyz = mul (unity_WorldToObject, v.vertex.xyz);

	        COMPUTE_EYEDEPTH (o.eyeDepth);
        }

		void surf (Input IN, inout SurfaceOutputStandard output)
		{
            float opacity = _Visibility;
			float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
            clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);
		
			fixed4 hsbOffsetsPrimary = _HSBOffsetsPrimary;
			fixed4 hsbOffsetsSecondary = _HSBOffsetsSecondary;

			fixed4 ah = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 mseo = tex2D (_MSEO, IN.uv_MainTex);
			fixed3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));

			fixed3 albedoBase = ah.rgb;
			fixed albedoMask = ah.a;
			fixed metalness = mseo.x;
			fixed smoothness = mseo.y;
			fixed emission = mseo.z;
			fixed occlusion = mseo.w;

			fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);

			float albedoMaskMinToMed = saturate(albedoMask * 2);
			float albedoMaskMedToMax = saturate(albedoMask - 0.5) * 2;

            float3 albedoFinal = RGBTweakHSV
			(
				albedoBase,
				albedoMaskMinToMed,
				albedoMaskMedToMax,
				hsbOffsetsPrimary.x,
				hsbOffsetsPrimary.y,
				hsbOffsetsPrimary.z,
				hsbOffsetsSecondary.x,
				hsbOffsetsSecondary.y,
				hsbOffsetsSecondary.z,
				occlusion
			);

			float smoothnessMaskMinToMed = saturate (smoothness * 2);
			float smoothnessMaskMedToMax = saturate (smoothness - 0.5) * 2;
			float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax);

            float3 emissionFinal = albedoFinal * _EmissionColor * emission;
			float occlusionFinal = lerp (1.0f, occlusion, _OcclusionIntensity);

            half destructionEarly = saturate (IN.destruction * 10);
            half destructionFast = lerp (IN.destruction, saturate (IN.destruction * 3), IN.destruction);
            half2 destructionUV = (IN.uv_MainTex.xy / _BurnScale) * lerp (1, 1.5, 1 - pow (1 - IN.destruction, 4)) + _BurnOffset.xy;
            half burnSampleFinal = tex2D (_BurnTex, destructionUV).rgb - destructionFast;

            clip (burnSampleFinal);
            if (burnSampleFinal < _BurnRampSize)
            {
                float3 burnRamp = tex2D (_BurnRamp, float2(burnSampleFinal * (1 / _BurnRampSize), 0)) * destructionEarly;
                float3 burnEmission = burnRamp * _BurnColor;
                emissionFinal += burnEmission;

                float3 burnRamp2 = tex2D (_BurnRamp, float2(burnSampleFinal * (1 / _BurnRampSize * 0.5), 0)) * destructionEarly;
                float burnAlbedo = 1 - saturate (burnRamp2.xxx);
                burnAlbedo = pow (burnAlbedo, 8);
                albedoFinal *= burnAlbedo;
            }

            output.Albedo = albedoFinal;
            output.Normal = normalFinal;
            output.Metallic = metalness;
            output.Smoothness = smoothnessFinal;
            output.Emission = emissionFinal;
            output.Occlusion = occlusionFinal;
		}
		ENDCG
	}

}
