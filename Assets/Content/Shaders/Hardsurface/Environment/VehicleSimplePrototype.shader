Shader "Hardsurface/Parts/Base (simplified vehicle)" 
{
	Properties
	{
		[Toggle (PART_USE_SINGLE_COLOR)]
        _UseSingleColor ("Use single color", Float) = 0
		
        [Toggle (PART_USE_TRIPLANAR)]
        _UseTriplanar ("Use triplanar", Float) = 0
		
		[Toggle (PART_USE_OVERSIZE)]
        _UseOverise ("Use oversize features", Float) = 0
		
		_AlbedoColor ("Albedo color", Color) = (1, 1, 1, 1)
		_AlbedoColorAccent ("Albedo color accent", Color) = (1, 1, 1, 1)
		_AlbedoColorAccentParams ("Accent location (XYZ) & radius (W)", Vector) = (0, 0, 0, 0)

        [Space (10)]
        [Header (Livery Color Customization)]
        [Space (5)]
        _HSBOffsetsPrimary ("HSB offsets (primary)", Vector) = (0, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary)", Vector) = (0, 0.5, 0.5, 1)
		_MaterialDataPrimary ("Material (primary)", Vector) = (0, 0.5, 1, 0)
		_MaterialDataSecondary ("Material (secondary)", Vector) = (0, 0.5, 1, 0)

        [Space (10)]
        [Header (Textures)]
        [Space (5)]
		[NoScaleOffset] _MainTex ("AH", 2D) = "white" {}
        [NoScaleOffset] _Bump ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1
		
        [Space (10)]
        [Header (Smoothness)]
        [Space (5)]
		_SmoothnessMin ("Smoothness (min.)", Range (0, 1)) = 0.0
		_SmoothnessMed ("Smoothness (med.", Range (0, 1)) = 0.2
		_SmoothnessMax ("Smoothness (max.", Range (0, 1)) = 0.8

        [Space (10)]
        [Header (Emission)]
        [Space (5)]
		_EmissionToggle ("Emission toggle", Range (0, 1)) = 1
		_EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
		_EmissionColor ("Emission color multiplier", Color) = (1, 1, 1, 1)
        _HighlightIntensity ("Rim Highlight Intensity", Range(0, 1)) = 1

        [Space (10)]
        [Header (Occlusion)]
        [Space (5)]
		_OcclusionIntensity ("Occlusion intensity", Range(0, 1)) = 1.0

        [Space (10)]
        [Header (Damage effects)]
        [Space (5)]
        _IntegrityPacked ("Integrity (per channel)", Vector) = (1,1,1,1)
        _DestructionPacked ("Destruction (per channel)", Vector) = (0,0,0,0)
		_DestructionLimit ("Destruction limit", Range(0, 1)) = 1.0
		
        _DamageMapScale ("Damage map scale", Range (0.25, 4)) = 1
        _CrushAddition ("Destruction area addition", Range (0, 20)) = 2
        _CrushMultiplier ("Destruction area multiplier", Range (-10, 0)) = -0.75
        _CrushParameters ("Destruction area position (XYZ)", Vector) = (0, 0, 0, 1)
        _StripParameters ("Displacement dir. (XYZ) & amount (W)", Vector) = (0, 0, -1, 1)

		[Space (10)]
        [Header (Overheating)]
        [Space (5)]
        [HDR] _OverheatColor ("Overheat color", Color) = (0, 0, 0, 0)
		_OverheatSettings ("Overheat settings (power, multiplier, addition, blend)", Vector) = (1,1,0,0.5)
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}
		Cull Off
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard vertex:vert exclude_path:prepass addshadow noforwardadd keepalpha // alphatest:_Cutoff
		#pragma target 5.0
        #pragma shader_feature PART_USE_TRIPLANAR
		#pragma shader_feature PART_USE_OVERSIZE
		#pragma shader_feature PART_USE_SINGLE_COLOR
		
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Assets/Content/Shaders/Hardsurface/Part/Cginc/HardsurfacePartFunctions.cginc"

        struct Input
        {
	        float2 uv_MainTex;
	        float4 color : COLOR;
	        float3 localPos;
	        float3 localNormal;
	        float3 worldNormal;
	        float4 screenPos;
	        float3 viewDir;
            float destructionProximity;
            float3 worldCameraDir;
            float facingSign : VFACE;
            float3 destructionAmountDistancePremul;
            INTERNAL_DATA
        };

		float4 _AlbedoColor;
		float4 _AlbedoColorAccent;
		float4 _AlbedoColorAccentParams;
		
        float4 _HSBOffsetsPrimary;
        float4 _HSBOffsetsSecondary;

		float4 _MaterialDataPrimary;
        float4 _MaterialDataSecondary;

        sampler2D _MainTex;
        sampler2D _Bump;

        float _DamageMapScale;

		// fixed _ColorIntensity;
		// fixed4 _ColorPrimary;
		// fixed4 _ColorSecondary;

        fixed _NormalIntensity;
        half _SmoothnessMin;
        half _SmoothnessMed;
        half _SmoothnessMax;

		fixed _OcclusionIntensity;

        fixed4 _EmissionColor;
        float _EmissionIntensity;
        float _EmissionToggle;

        uniform float3 TOD_AmbientColor;
        uniform float _HighlightIntensity;

        float4 _IntegrityPacked;
        float4 _DestructionPacked;
		float _DestructionLimit;
		
		// float4 _DestructionTimePacked;
		// float _DestructionFadeDuration;
		
		float4 _OverheatColor;
		float4 _OverheatSettings;

        float _CrushAddition;
        float _CrushMultiplier;
        float4 _CrushParameters;

        float4 _StripParameters;

        float4 _TreadsAnimLeft;
        float4 _TreadsAnimRight;

        UNITY_INSTANCING_BUFFER_START (Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o)
        {
	        UNITY_INITIALIZE_OUTPUT (Input, o);

	        o.localPos = v.vertex.xyz;
	        o.localNormal = v.normal;

	        o.color = v.color;

            // World space direction from the camera towards given object space vertex position
            o.worldCameraDir = -normalize (WorldSpaceViewDir (v.vertex));

            float3 difference = float3
            (
                o.localPos.x - _CrushParameters.x,
                o.localPos.y - _CrushParameters.y,
                o.localPos.z - _CrushParameters.z
            );

            float destructionDistance = sqrt
            (
                difference.x * difference.x +
                difference.y * difference.y +
                difference.z * difference.z
            );

            destructionDistance = saturate (destructionDistance * _CrushMultiplier + _CrushAddition);
        	
        	// Prepare critical damage and critical damage clamp values for the ApplyDamageVert function
			#if PART_USE_SINGLE_COLOR

	            float damageCritical = saturate (_DestructionPacked.x);
        	
			#else

	            float damageCritical = saturate
	            (
	                (_DestructionPacked.x) * v.color.x +
	                (_DestructionPacked.y) * v.color.y +
	                (_DestructionPacked.z) * v.color.z
	            );
        	
        	#endif

            // (0, Critical, 0, Critical Damage Clamp Value)
            float4 damageInputVert = float4(0, damageCritical * _StripParameters.w, 0, _DestructionLimit);

        	// Adjust destruction by distance to center
            float destructionPremul = lerp (damageCritical * destructionDistance, 1, pow (damageCritical, 8));

        	// Pack the output
            o.destructionAmountDistancePremul = float3 (damageCritical, destructionDistance, destructionPremul);

            float3 vertexDistorted;

            ApplyDamageVert
            (
                damageInputVert,
                v.vertex,
                v.vertex, // Mechs use pre-skinned vertex data here, because they are combined into a skeletal mesh at runtime. Vehicles are fine with just regular mesh vertex data
                v.normal,
                _CrushParameters,
                _CrushMultiplier,
                _CrushAddition,
				_StripParameters,
                o.destructionProximity,
                vertexDistorted
            );

            v.vertex.xyz = vertexDistorted;
        }

		void surf (Input IN, inout SurfaceOutputStandard output)
		{
			fixed4 hsbOffsetsPrimary = _HSBOffsetsPrimary;
			fixed4 hsbOffsetsSecondary = _HSBOffsetsSecondary;

			fixed4 ah = tex2D (_MainTex, IN.uv_MainTex);
        	float ahg = RGBToGrayscale (ah.rgb);
			fixed4 mseo = fixed4 (0, (ahg - 0.5) * 0.5 + 0.5, 0, saturate (ahg * 2));
			fixed3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));

			fixed3 albedoBase = ah.rgb;
			fixed albedoMask = ah.a;
			fixed metalness = mseo.x;
			fixed smoothness = mseo.y;
			fixed emission = mseo.z;
			fixed occlusion = lerp (mseo.w, ah.w, _AlbedoColor.w);

			fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);

            // The values here are not arbitrary - we 'deep fry' the hue mask a bit to get rid of a transitional line
            // between two color areas that gets in there because of texture filtering. Default albedo color for the paint is
            // dark red, so it's important to hide it on any custom paint scheme
            float albedoMaskMinToMed = saturate (saturate (albedoMask - 0.4) * 32);
            float albedoMaskMedToMax = saturate (saturate (albedoMask - 0.5) * 32);

        	float3 albedoTint = _AlbedoColor.rgb;
        	
			if (_AlbedoColorAccentParams.w > 0)
			{
				float3 differenceAccent = float3
	            (
	                IN.localPos.x - _AlbedoColorAccentParams.x,
	                IN.localPos.y - _AlbedoColorAccentParams.y,
	                IN.localPos.z - _AlbedoColorAccentParams.z
	            );

	            float accentDistance = sqrt
	            (
	                differenceAccent.x * differenceAccent.x +
	                differenceAccent.y * differenceAccent.y +
	                differenceAccent.z * differenceAccent.z
	            );

				// float factor = pow (saturate (accentDistance - _AlbedoColorAccentParams.w), 16);
				// factor *= saturate (1 - dot (IN.localNormal, normalize (_AlbedoColorAccentParams.xyz)));

				albedoTint = lerp (_AlbedoColorAccent.xyz, albedoTint, pow (saturate (accentDistance - _AlbedoColorAccentParams.w), 16));
			}
        	
        	albedoBase.rgb *= albedoTint;
        	albedoBase.rgb *= lerp (1, occlusion, _AlbedoColor.w);

            float3 albedoFinal = RGBTweakHSV
			(
				albedoBase,
				albedoMaskMinToMed,
				albedoMaskMedToMax,
				0,
				0.5,
				0.5,
				0,
				0.5,
				0.5,
				occlusion
			);

        	float smoothnessFinal = lerp (RemapSmoothness (smoothness, _MaterialDataSecondary.xyz), smoothness, albedoMaskMinToMed);
			smoothnessFinal = lerp (smoothnessFinal, RemapSmoothness (smoothness, _MaterialDataPrimary.xyz), albedoMaskMedToMax);

        	float metalnessFinal = lerp (saturate (metalness + _MaterialDataSecondary.w), metalness, albedoMaskMinToMed);
			metalnessFinal = lerp (metalnessFinal, saturate (metalness + _MaterialDataPrimary.w), albedoMaskMedToMax);

            float3 emissionFinal = albedoFinal * _EmissionColor * _EmissionIntensity * emission * _EmissionToggle;
			float occlusionFinal = lerp (1.0f, occlusion, _OcclusionIntensity);

			// float opacity = 1 - _ExplosionAnimation;
			// float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
            // clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);

            // damage and destruction
            float4 damageSample = 0.0f;
            float4 damageSampleSecondary = 0.0f;

        	#if PART_USE_SINGLE_COLOR

	            float damageIntegrity = saturate (1 - _IntegrityPacked.x);
        		float damageCritical = saturate (_DestructionPacked.x);
        		float damageCriticalInverted = saturate (1 - _DestructionPacked.x);
        	
			#else

	            float damageIntegrity = saturate
	            (
	                (1 - _IntegrityPacked.x) * IN.color.x +
	                (1 - _IntegrityPacked.y) * IN.color.y +
	                (1 - _IntegrityPacked.z) * IN.color.z
	            );

	            float damageCritical = saturate
	            (
	                (_DestructionPacked.x) * IN.color.x +
	                (_DestructionPacked.y) * IN.color.y +
	                (_DestructionPacked.z) * IN.color.z
	            );

	            float damageCriticalInverted = saturate
	            (
	                (1 - _DestructionPacked.x) * IN.color.x +
	                (1 - _DestructionPacked.y) * IN.color.y +
	                (1 - _DestructionPacked.z) * IN.color.z
	            );
        	
        	#endif
        	
        	#if PART_USE_OVERSIZE
        	
        		float4 damageSampleOversize = float4 (0,0,0,0);
        	
        	#endif

            if (damageIntegrity + damageCritical > 0.0f)
            {
                float damageScale = _DamageMapScale;

                #if PART_USE_TRIPLANAR
            	
                    // calculate triplanar uvs
                    float2 uvX = IN.localPos.yz * (0.3 / damageScale) + _GlobalUnitDamageOffset.yz;
                    float2 uvY = IN.localPos.xz * (0.3 / damageScale) + _GlobalUnitDamageOffset.xz;
                    float2 uvZ = IN.localPos.xy * (0.3 / damageScale) + _GlobalUnitDamageOffset.xy;

                    half3 triblend = saturate (pow (IN.localNormal, 4));
                    triblend /= max (dot (triblend, half3(1, 1, 1)), 0.0001);

                    half4 dmgSampleX = tex2D (_GlobalUnitDamageTexNew, uvX);
                    half4 dmgSampleY = tex2D (_GlobalUnitDamageTexNew, uvY);
                    half4 dmgSampleZ = tex2D (_GlobalUnitDamageTexNew, uvZ);
                    half4 dmgSampleFinal = dmgSampleX.xyzw * triblend.x + dmgSampleY.xyzw * triblend.y + dmgSampleZ.xyzw * triblend.z;

                    half4 dmgSampleSX = tex2D (_GlobalUnitDamageTexNewSecondary, uvX);
                    half4 dmgSampleSY = tex2D (_GlobalUnitDamageTexNewSecondary, uvY);
                    half4 dmgSampleSZ = tex2D (_GlobalUnitDamageTexNewSecondary, uvZ);
                    half4 dmgSampleSFinal = dmgSampleSX.xyzw * triblend.x + dmgSampleSY.xyzw * triblend.y + dmgSampleSZ.xyzw * triblend.z;

                    damageSample = dmgSampleFinal;
                    damageSampleSecondary = dmgSampleSFinal;

					#if PART_USE_OVERSIZE

            			half4 dmgSampleOversizeX = tex2D (_GlobalUnitDamageTexNew, uvX * 0.5 + float2 (0.5, 0.5));
						half4 dmgSampleOversizeY = tex2D (_GlobalUnitDamageTexNew, uvY * 0.5 + float2 (0.5, 0.5));
						half4 dmgSampleOversizeZ = tex2D (_GlobalUnitDamageTexNew, uvZ * 0.5 + float2 (0.5, 0.5));
						damageSampleOversize = dmgSampleOversizeX.xyzw * triblend.x + dmgSampleOversizeY.xyzw * triblend.y + dmgSampleOversizeZ.xyzw * triblend.z;
            	
            		#endif
            	
                #else

            		float uvDamage = IN.uv_MainTex * damageScale + _GlobalUnitDamageOffset.xy;
                    damageSample = tex2D (_GlobalUnitDamageTexNew, uvDamage);
                    damageSampleSecondary = tex2D (_GlobalUnitDamageTexNewSecondary, uvDamage);

            		#if PART_USE_OVERSIZE
            	
						damageSampleOversize = tex2D (_GlobalUnitDamageTexNew, uvDamage * 0.25 + float2 (0.5, 0.5));
            	
            		#endif

            	#endif
            }

            // Combine damage values into a vector that is consumable by ApplyDamage function
            // (Integrity, Critical, 0, Critical Damage Clamp Value)
            float4 damageInput = float4(damageIntegrity, damageCritical, 0, _DestructionLimit);
        	
            float3 normalTest = float3 (0, 0, 1);
            float backsideFactor = saturate (IN.facingSign);

            float3 albedoAfterDamage;
            float smoothnessAfterDamage;
            float metalnessAfterDamage;
            float3 emissionAfterDamage;
            float3 normalAfterDamage;

            float mainGrayscale = RGBToGrayscale (albedoBase.rgb);

            float albedoDamageMask = saturate ((mainGrayscale - 0.05) * 1.5);
            albedoDamageMask = pow (albedoDamageMask, 4);

        	#if PART_USE_OVERSIZE

				float damageMask = damageSampleOversize.www;
				damageMask *= pow (ah.w, 4);
        		damageMask *= pow (saturate (ah.x * 2), 4);
        		damageMask = saturate (damageMask * 3);
        		damageMask = pow (damageMask, 2);
        	
				// albedoAfterDamage.xyz = lerp (albedoAfterDamage.xyz, damageMask, 0.999);
        	
				damageInput.x = lerp (damageInput.x * 0.1, damageInput.x, damageMask);
        		damageInput.y = lerp (damageInput.y * 0.1, damageInput.y, damageMask);
        		albedoDamageMask *= damageMask;
            	
            #endif

        	// Allow material based control of where the part disappears
			damageInput.w *= saturate (IN.destructionProximity); 
        	
            ApplyDamage
            (
                damageInput,
                damageSample,
                damageSampleSecondary,
                backsideFactor,
                IN.destructionProximity,
                albedoFinal,
                albedoDamageMask,
                mainGrayscale,
                smoothnessFinal,
                metalnessFinal,
                emissionFinal,
                normalFinal,
                1.0f,
                damageCriticalInverted,
                albedoAfterDamage,
                smoothnessAfterDamage,
                metalnessAfterDamage,
                emissionAfterDamage,
                normalAfterDamage
            );
        	
            // Fresnel effect mask based on per-pixel normal, creates a higher quality highlight
            float3 worldNormalAfterDamage = WorldNormalVector (IN, normalAfterDamage);
            float fresnelAfterDamage = pow(1.0 + dot(IN.worldCameraDir, worldNormalAfterDamage), 2) / 2;

        	float highlightMask = fresnelAfterDamage * saturate (worldNormalAfterDamage.g + abs(worldNormalAfterDamage.r * 0.2)) * lerp (1, albedoDamageMask, damageInput.x);
        	// Highlight effect, masked by AO and by smoothness map (this one is boosted quite a bit)
            emissionAfterDamage += (highlightMask * TOD_AmbientColor * 5 * pow(occlusionFinal, 4) * saturate(smoothnessAfterDamage * 6)) * _HighlightIntensity;
            // Overheating effect
            float destructionDistance = IN.destructionAmountDistancePremul.y;
			float heatFromFresnel = highlightMask * pow (saturate (IN.localPos.y * 0.3), 3);
        	float heatFromDistance = max (0, (pow (destructionDistance, _OverheatSettings.x) * _OverheatSettings.y) + _OverheatSettings.z);
        	float3 heatBlended = lerp (heatFromFresnel, heatFromDistance, saturate (_OverheatSettings.w)) * _OverheatColor;
        	emissionAfterDamage += heatBlended;

            // Darken back faces, remove any emission from them (that's intentional even on destruction)
            albedoAfterDamage *= backsideFactor;
            smoothnessAfterDamage *= backsideFactor;
            metalnessAfterDamage *= backsideFactor;
            emissionAfterDamage *= backsideFactor;

            output.Albedo = albedoAfterDamage;
            output.Smoothness = smoothnessAfterDamage;
            output.Metallic = metalnessAfterDamage;
            output.Emission = emissionAfterDamage;
            output.Normal = normalAfterDamage;
            output.Occlusion = occlusionFinal;
		}
		ENDCG
	}
    FallBack "Diffuse" 
}
