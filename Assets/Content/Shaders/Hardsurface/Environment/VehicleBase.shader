Shader "Hardsurface/Parts/Base (vehicle)" 
{
	Properties
	{
        [Toggle (PART_USE_TRIPLANAR)]
        _UseTriplanar ("Use triplanar", Float) = 0

        [Space (10)]
        [Header (Livery Color Customization)]
        [Space (5)]
        _HSBOffsetsPrimary ("HSB offsets (primary)", Vector) = (0, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary)", Vector) = (0, 0.5, 0.5, 1)
		_MaterialDataPrimary ("Material (primary)", Vector) = (0, 0.5, 1, 0)
		_MaterialDataSecondary ("Material (secondary)", Vector) = (0, 0.5, 1, 0)

        [Space (10)]
        [Header (Main Textures)]
        [Space (5)]
		[NoScaleOffset] _MainTex ("AH map", 2D) = "white" {}
        [NoScaleOffset] _MSEO ("MSEO map", 2D) = "white" {}
        [NoScaleOffset] _Bump ("Normal map", 2D) = "bump" {}
		_NormalIntensity ("Normal intensity", Range (0, 1)) = 1

        [Space (10)]
        [Header (Surface Details)]
        [Space (5)]
        [Toggle (PART_USE_DETAIL_TEX)]
        _UseDetailTex ("Use Detail Texture", Float) = 0
        [HideIfDisabled(PART_USE_DETAIL_TEX)] _DetailTexAlbedoIntensity ("Detail Albedo Intensity", Range (0.01, 1)) = 0.5
        [HideIfDisabled(PART_USE_DETAIL_TEX)] _DetailTexNormalIntensity ("Detail Normals Intensity", Range (0.01, 1)) = 0.5
		[HideIfDisabled(PART_USE_DETAIL_TEX)] _DetailTexTiling ("Detail tiling multiplier", Range (0.1, 30)) = 1
		
        [Space (10)]
        [Header (Emission)]
        [Space (5)]
		_EmissionToggle ("Emission toggle", Range (0, 1)) = 1
		_EmissionIntensity ("Emission intensity", Range (0, 64)) = 0
		_EmissionColor ("Emission color multiplier", Color) = (1, 1, 1, 1)
        [BlockInfo(0.5, 0.5, 1, 1)] dummy_info_0 ("Optionally 'shrink' the emission mask to avoid emission leaking on MIPs.", Float) = 0
        _EmissionMaskShrink ("Emission Mask Shrink Amount", Range (0, 1)) = 0
        _HighlightIntensity ("Optional highlight intensity tweak", Range(0, 1)) = 1

        [Space (10)]
        [Header (Occlusion)]
        [Space (5)]
		_OcclusionIntensity ("Occlusion intensity", Range(0, 1)) = 1.0

        [Space (10)]
        [Header (Damage effects)]
        [Space (5)]
        [BlockInfo(0.5, 0.5, 1, 1)] dummy_info_1 ("NOTE: You can mark surfaces as internals if you move their UVs beyond 1,1 coords. See shader for more info", Float) = 0
        _IntegrityPacked ("Integrity (per channel)", Vector) = (1,1,1,1)
        _DestructionPacked ("Destruction (per channel)", Vector) = (0,0,0,0)
        [BlockInfo(0.5, 0.5, 1, 1)] dummy_info_2 ("NOTE: Destruction limit for bosses is controlled here, limit for other units is controlled in prefabs", Float) = 0
		_DestructionLimit ("Destruction limit", Range(0, 1)) = 1.0
		
        _DamageMapScale ("Damage map scale", Range (0.25, 4)) = 1
        _CrushAddition ("Destruction area addition", Range (0, 20)) = 2
        _CrushMultiplier ("Destruction area multiplier", Range (-10, 0)) = -0.75
        _CrushParameters ("Destruction area position (XYZ)", Vector) = (0, 0, 0, 1)
        _StripParameters ("Stripping direction (XYZ)", Vector) = (0, 0, -1, 1)

		[Space (10)]
        [Header (Overheating)]
        [Space (5)]
        [HDR] _OverheatColor ("Overheat color", Color) = (0, 0, 0, 0)
		_OverheatSettings ("Overheat settings (power, multiplier, addition, blend)", Vector) = (1,1,0,0.5)
        
		[Space (5)]
        [Header (Animated Treads Currently Not In Use)]
        [Space (5)]
		[Toggle(USE_TREADS_ANIM)] _UseTreadsAnim("Treads Material - Use Animation", Int) = 0
		[HideIfDisabled(USE_TREADS_ANIM)] _TreadsAnimLeft ("Treads Scroll Left (back, mid-back, mid-front, front)", Vector) = (0, 0, 0, 0)
        [HideIfDisabled(USE_TREADS_ANIM)] _TreadsAnimRight ("Treads Scroll Right (back, mid-back, mid-front, front)", Vector) = (0, 0, 0, 0)

		[Space (5)]
        [Header (Animated Fans For Drones)]
        [Space (5)]
		[Toggle(USE_FANS_ANIM)] _UseFansAnim("Drones - Use Fan Spinning Anim", Int) = 0
        [HideIfDisabled(USE_FANS_ANIM)] _FansAnimSpeed("Fan Spinning Anim Speed", Range (0, 15)) = 1

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
        #pragma shader_feature_local PART_USE_TRIPLANAR
        #pragma shader_feature_local USE_TREADS_ANIM
        #pragma shader_feature_local PART_USE_DETAIL_TEX
        #pragma shader_feature_local USE_FANS_ANIM

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
            float partInternalsMask;
            INTERNAL_DATA
        };

        float4 _HSBOffsetsPrimary;
        float4 _HSBOffsetsSecondary;

		float4 _MaterialDataPrimary;
        float4 _MaterialDataSecondary;

        sampler2D _MainTex;
        sampler2D _MSEO;
        sampler2D _Bump;

        float _DetailTexAlbedoIntensity;
        float _DetailTexNormalIntensity;
		float _DetailTexTiling;

        float _DamageMapScale;

        fixed _NormalIntensity;

		fixed _OcclusionIntensity;

        fixed4 _EmissionColor;
        float _EmissionIntensity;
        float _EmissionToggle;
        float _EmissionMaskShrink;

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

        float _FansAnimSpeed;

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

            // Support for tank treads UV animation (supports for up to 8 independent treads)
            #ifdef USE_TREADS_ANIM
                // vtexcoord1.x = 1 for mid treads, mix [back + mid-back] tread animation and [front + mid-front] tread animation
                float uvOffsetTreads_Back_MidBack_Left = lerp (_TreadsAnimLeft.x, _TreadsAnimLeft.y, abs (v.texcoord1.x));
                float uvOffsetTreads_Front_MidFront_Left = lerp (_TreadsAnimLeft.w, _TreadsAnimLeft.z, abs (v.texcoord1.x));
                float uvOffsetTreads_Back_MidBack_Right = lerp (_TreadsAnimRight.x, _TreadsAnimRight.y, v.texcoord1.x);
                float uvOffsetTreads_Front_MidFront_Right = lerp (_TreadsAnimRight.w, _TreadsAnimRight.z, v.texcoord1.x);

                // vtexcoord1.y = 1 for front and mid-front treads, mix back and front treads animation
                float uvOffsetTreads_BackToFront_Left = lerp (uvOffsetTreads_Back_MidBack_Left, uvOffsetTreads_Front_MidFront_Left, v.texcoord1.y);
                float uvOffsetTreads_BackToFront_Right = lerp (uvOffsetTreads_Back_MidBack_Right, uvOffsetTreads_Front_MidFront_Right, v.texcoord1.y);

                // Left tank treads are mapped to a negative UV X area.
                // More precisely, left treads are mapped to (-1 - -0.001) range and right treads are on (0.001 - 1) range,
                // this setup makes it possible to easily distinguish left and right treads without any overlaps.
                if (v.texcoord1.x > 0)
                {
                    v.texcoord.y += uvOffsetTreads_BackToFront_Right;
                }
                else
                {
                    v.texcoord.y += uvOffsetTreads_BackToFront_Left;
                }
            #endif

            #ifdef USE_FANS_ANIM
                // Fans should have their UV0 coordinates to be in -1,-1 quadrant
                if (v.texcoord.x < 0 && v.texcoord.y < 0)
                {
                    v.vertex.xz += v.texcoord1.xy * float2 (1, -1); // convert from modo to unity coords with float2 mult
                    v.vertex = RotateAroundYInDegrees (v.vertex, _Time.y * _FansAnimSpeed * 360);
                    v.vertex.xz -= v.texcoord1.xy * float2 (1, -1); // convert from modo to unity coords with float2 mult
                }
            #endif

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
            float damageCritical = saturate
            (
                (_DestructionPacked.x) * v.color.x +
                (_DestructionPacked.y) * v.color.y +
                (_DestructionPacked.z) * v.color.z
            );
            // (0, Critical, 0, Critical Damage Clamp Value)
            float4 damageInput = float4(0, damageCritical, 0, _DestructionLimit);

        	// Adjust destruction by distance to center
            float destructionPremul = lerp (damageCritical * destructionDistance, 1, pow (damageCritical, 8));

        	// Pack the output
            o.destructionAmountDistancePremul = float3 (damageCritical, destructionDistance, destructionPremul);

            // You can mark parts of the model as internals if their UV coordinates are located beyond 0-1 UV range
            // Specifically if both UV.x > 1 and UV.y > 1
            // partInternalsMask is 1 for internals and 0 for everything else
            o.partInternalsMask = saturate ((v.texcoord.x - 1) * 32) * saturate ((v.texcoord.y - 1) * 32);

            float3 vertexDistorted;

            ApplyDamageVert
            (
                damageInput,
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
			fixed4 mseo = tex2D (_MSEO, IN.uv_MainTex);
			fixed3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));

			fixed3 albedoBase = ah.rgb;
			fixed albedoMask = ah.a;
			fixed metalness = mseo.x;
			fixed smoothness = mseo.y;
			fixed emission = mseo.z;
			fixed occlusion = mseo.w;

			fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);

            #if PART_USE_DETAIL_TEX
                float4 detailTex = tex2D (_GlobalUnitDetailTexNew, IN.uv_MainTex * _DetailTexTiling);

                float detailAlbedo = lerp (0.5f, detailTex.r, _DetailTexAlbedoIntensity);
                albedoBase = Overlay (albedoBase, detailAlbedo);

                float3 detailNormal = float3 (((detailTex.gb - 0.5) * 2), 0.0f) * _DetailTexNormalIntensity;
                normalFinal = normalize (normalFinal + detailNormal);
            #endif

            // The values here are not arbitrary - we 'deep fry' the hue mask a bit to get rid of a transitional line
            // between two color areas that gets in there because of texture filtering. Default albedo color for the paint is
            // dark red, so it's important to hide it on any custom paint scheme
            float albedoMaskMinToMed = saturate (saturate (albedoMask - 0.4) * 32);
            float albedoMaskMedToMax = saturate (saturate (albedoMask - 0.5) * 32);

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

        	float smoothnessFinal = lerp (RemapSmoothness (smoothness, _MaterialDataSecondary.xyz), smoothness, albedoMaskMinToMed);
			smoothnessFinal = lerp (smoothnessFinal, RemapSmoothness (smoothness, _MaterialDataPrimary.xyz), albedoMaskMedToMax);

        	float metalnessFinal = lerp (saturate (metalness + _MaterialDataSecondary.w), metalness, albedoMaskMinToMed);
			metalnessFinal = lerp (metalnessFinal, saturate (metalness + _MaterialDataPrimary.w), albedoMaskMedToMax);

            float emissionMaskAdjusted = saturate (emission - lerp (0.0, 0.25, _EmissionMaskShrink));
            float3 emissionFinal = albedoFinal * _EmissionColor * _EmissionIntensity * emissionMaskAdjusted * _EmissionToggle;
			float occlusionFinal = lerp (1.0f, occlusion, _OcclusionIntensity);

			// float opacity = 1 - _ExplosionAnimation;
			// float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
            // clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);

            // damage and destruction
            float4 damageSample = 0.0f;
            float4 damageSampleSecondary = 0.0f;

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

            // Modify damageIntegrity value for internal parts - make sure it never decreases and stays at 1.0
            // partInternalsMask is 1 for internals and 0 for everything else
            damageIntegrity = saturate (damageIntegrity + IN.partInternalsMask);
            // Make internals instantly disappear the moment damageCritical goes above 0
            // You can opt certain internals out of that behavior if you keep their vertex color at 0
            // Internals that have their vcolor > 0.5 will be insta clipped
            clip (IN.partInternalsMask * -damageCritical);

            float detailScale = 1;
            float4 detailSample = tex2D (_GlobalUnitDetailTex, IN.uv_MainTex * detailScale + _GlobalUnitDetailOffset.xy);

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
                #else
                    damageSample = tex2D (_GlobalUnitDamageTexNew, IN.uv_MainTex * damageScale + _GlobalUnitDamageOffset.xy);
                    damageSampleSecondary = tex2D (_GlobalUnitDamageTexNewSecondary, IN.uv_MainTex * damageScale + _GlobalUnitDamageOffset.xy);
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

        	float highlightMask = fresnelAfterDamage * saturate (worldNormalAfterDamage.g + abs(worldNormalAfterDamage.r * 0.2));
        	// Highlight effect, masked by AO and by smoothness map (this one is boosted quite a bit)
            emissionAfterDamage += (highlightMask * TOD_AmbientColor * 5 * pow(occlusionFinal, 4) * saturate(smoothnessAfterDamage * 6)) * _HighlightIntensity;
            // Overheating effect
            float destructionDistance = IN.destructionAmountDistancePremul.y;
			float heatFromFresnel = highlightMask * pow (saturate (IN.localPos.y * 0.3), 3);
        	float heatFromDistance = max (0, (pow (destructionDistance, _OverheatSettings.x) * _OverheatSettings.y) + _OverheatSettings.z);
        	float3 heatBlended = lerp (heatFromFresnel, heatFromDistance, saturate (_OverheatSettings.w)) * _OverheatColor;
        	emissionAfterDamage += heatBlended;

            // Darken surface internals and remove any emission, smoothness, etc. from them
            // NOTE: Darkening of inside faces\smoothness removal etc. happens inside ApplyDamage function
            // here we do it again to make sure inside faces are still dark when there's no damage
            float partInternalsMaskInv = 1 - IN.partInternalsMask;
            albedoAfterDamage *= partInternalsMaskInv;
            smoothnessAfterDamage *= backsideFactor * partInternalsMaskInv;
            metalnessAfterDamage *= backsideFactor * partInternalsMaskInv;
            emissionAfterDamage *= backsideFactor * partInternalsMaskInv;

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
