// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Hardsurface/Environment/Terrain (damageable old)" 
{
    Properties 
    {
        _MainTex1 ("AH", 2D) = "white" {}
        _MainTex2 ("AH", 2D) = "white" {}
        _MainTex3 ("AH", 2D) = "white" {}
        _MainTex4 ("AH", 2D) = "white" {}

        _SmoothnessMin ("SmoothnessMin", Range (0, 1)) = 0.0
        _SmoothnessMed ("SmoothnessMed", Range (0, 1)) = 0.2
        _SmoothnessMax ("SmoothnessMax", Range (0, 1)) = 0.8
        _EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
        _EmissionColor ("Emission color", Color) = (0, 0, 0, 1)
        _StructureColor ("Structure color", Color) = (0, 0, 0, 1)

        _DestructionAnimation ("Destruction animation", Range (0, 1)) = 0
        _IntegritiesA ("Integrities (0-3)", Color) = (1, 1, 1, 1)
        _IntegritiesB ("Integrities (4-7)", Color) = (1, 1, 1, 1)
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

        #pragma surface surf Standard addshadow vertex:vert
        #pragma target 5.0
        #pragma multi_compile_instancing
        #include "UnityCG.cginc"
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"

        // Config maxcount. See manual page.
        // #pragma instancing_options

        struct Input
        {
            float2 uv_MainTex;
            float3 localPos;
            float3 localNormal;
            float3 worldPos1;
            float3 worldNormal1;
            float damage;
            float3 viewDir;
            float outOfBounds;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            INTERNAL_DATA
        };


        sampler2D _MainTex;
        sampler2D _MSEO;
        sampler2D _Bump;

        half _SmoothnessMin;
        half _SmoothnessMed;
        half _SmoothnessMax;

        fixed4 _StructureColor;
        fixed4 _EmissionColor;
        float _EmissionIntensity;

        sampler2D _GlobalDetailTex;
        sampler2D _GlobalRampBurnTex;
        float _GlobalEnvironmentDetailScale;
        float _GlobalEnvironmentDetailContrast;
        float _GlobalEnvironmentRampScale;
        float _GlobalEnvironmentRampInfluence;
        float _GlobalEnvironmentDamageOffset;
        float4 _GlobalEnvironmentAmbientSettings;

        float _DestructionAnimation;
        float _DestructionMaskContrast;

        // Declare instanced properties inside a cbuffer.
        // Each instanced property is an array of by default 500(D3D)/128(GL) elements. Since D3D and GL imposes a certain limitation
        // of 64KB and 16KB respectively on the size of a cubffer, the default array size thus allows two matrix arrays in one cbuffer.
        // Use maxcount option on #pragma instancing_options directive to specify array size other than default (divided by 4 when used
        // for GL).

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP (fixed4, _HSBOffsetsPrimary)
#define _HSBOffsetsPrimary_arr Props
            UNITY_DEFINE_INSTANCED_PROP (fixed4, _HSBOffsetsSecondary)
#define _HSBOffsetsSecondary_arr Props
            UNITY_DEFINE_INSTANCED_PROP (fixed4, _IntegritiesA)
#define _IntegritiesA_arr Props
            UNITY_DEFINE_INSTANCED_PROP (fixed4, _IntegritiesB)
#define _IntegritiesB_arr Props
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o) 
        {
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(Input, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            
            float3 worldPosVertex = mul (unity_ObjectToWorld, v.vertex);			
            float3 worldPosPivot = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1));
            float3 bounds = float3(1.5, 1.5, 1.5);

            float3 transformedPos = worldPosVertex - worldPosPivot;

            float3 positive = clamp((transformedPos + bounds) * 0.33333, -1, 1);			
            float3 negative = clamp((-transformedPos + bounds) * 0.33333, -1, 1);

            float4 topCorners = float4(
            negative.x * positive.y * negative.z,
            positive.x * positive.y * negative.z,
            negative.x * positive.y * positive.z,
            positive.x * positive.y * positive.z);
             
            float4 bottomCorners = float4(
            negative.x * negative.y * negative.z,
            positive.x * negative.y * negative.z,
            negative.x * negative.y * positive.z,
            positive.x * negative.y * positive.z);

            fixed4 integritiesA = fixed4(1,1,1,1) - pow (UNITY_ACCESS_INSTANCED_PROP (_IntegritiesA_arr, _IntegritiesA), 0.454545);
            fixed4 integritiesB = fixed4(1,1,1,1) - pow (UNITY_ACCESS_INSTANCED_PROP (_IntegritiesB_arr, _IntegritiesB), 0.454545);

            float4 topScaled = topCorners * integritiesA;
            float4 bottomScaled = bottomCorners * integritiesB;

            float vectorDifference = 1 - topScaled.x - topScaled.y - topScaled.z - topScaled.w - bottomScaled.x - bottomScaled.y - bottomScaled.z - bottomScaled.w;

            o.damage = 1 - max(0, vectorDifference);			
            o.damage = saturate (o.damage * _GlobalEnvironmentDamageOffset);			

            if (_DestructionAnimation > 0.01)
            {
                o.damage = saturate (o.damage + 0.1 + _DestructionAnimation / 1.1111);
                v.vertex.xyz += (v.normal + v.vertex.xyz * 0.5) * pow (o.damage, 2) * 3;
            }

            o.localPos = v.vertex.xyz;
            o.localNormal = v.normal;
            o.worldPos1 = mul(unity_ObjectToWorld, v.vertex);
            o.worldNormal1 = UnityObjectToWorldNormal(v.normal);
            o.outOfBounds = o.damage;
        }

        void surf (Input IN, inout SurfaceOutputStandard output) 
        {
            fixed4 hsbOffsetsPrimary = pow (UNITY_ACCESS_INSTANCED_PROP (_HSBOffsetsPrimary_arr, _HSBOffsetsPrimary), 0.454545);
            fixed4 hsbOffsetsSecondary = pow (UNITY_ACCESS_INSTANCED_PROP (_HSBOffsetsSecondary_arr, _HSBOffsetsSecondary), 0.454545);

            fixed4 ah = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 mseo = tex2D (_MSEO, IN.uv_MainTex);

            fixed3 albedoBase = ah.rgb;
            fixed albedoMask = ah.a;
            fixed metalness = mseo.x;
            fixed smoothness = mseo.y;
            fixed emission = mseo.z;
            fixed occlusion = mseo.w;

            float3 n = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));
            float backsideFactor = dot (IN.viewDir, float3 (0, 0, 1)) > -0.1 ? 0 : 1;
            float3 normalFinal = lerp (n, -n, backsideFactor);

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
            float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax) * (1 - backsideFactor);
            float3 emissionFinal = emission * _EmissionColor * _EmissionIntensity * (lerp (hsbOffsetsPrimary.w, 0, saturate (IN.damage * 10))) * (1 - backsideFactor);

            float occlusionHeightFactor = saturate ((IN.worldPos1.y + _GlobalEnvironmentAmbientSettings.y) / _GlobalEnvironmentAmbientSettings.x);  // saturate ((IN.worldPos1.y * _GlobalEnvironmentAmbientSettings.x + _GlobalEnvironmentAmbientSettings.y) / _GlobalEnvironmentAmbientSettings.x);
            float occlusionHeightGraded = lerp (0.25, 1, occlusionHeightFactor);
            float occlusionFinal = occlusion * lerp (1, occlusionHeightGraded, _GlobalEnvironmentAmbientSettings.z * saturate (1 - abs (IN.worldNormal1.y))) * lerp (1, 0, backsideFactor);

            // Damage
            // /*
            if (IN.damage > 0.001)
            {
                // Triplanar projection of shared detail texture
                float3 projNormal = saturate (pow (IN.worldNormal1 * 1.4, 4));
                float4 detailX = tex2D (_GlobalDetailTex, frac (IN.worldPos1.zy / _GlobalEnvironmentDetailScale) + float2 (0.25, 0.125)) * abs (IN.worldNormal1.x);
                float4 detailY = tex2D (_GlobalDetailTex, frac (IN.worldPos1.zx / _GlobalEnvironmentDetailScale) + float2 (-0.25, -0.125)) * abs (IN.worldNormal1.y);
                float4 detailZ = tex2D (_GlobalDetailTex, frac (IN.worldPos1.xy / _GlobalEnvironmentDetailScale) + float2 (0.35, 0.35)) * abs (IN.worldNormal1.z);
                float4 detail = detailZ;
                detail = lerp (detail, detailX, projNormal.x);
                detail = lerp (detail, detailY, projNormal.y);

                // Extracting noise and using it to set opacity from damage, along with applying contrast to the noise
                float detailNoise = saturate ((detail.z - 0.5) * lerp (_GlobalEnvironmentDetailContrast, 1, _DestructionAnimation) + 0.5 - 0.25);
                float detailStructure = detail.y / 2;

                // Noise is a rich set of uniformly brigtness-distributed values which damage pushes below zero to create the cuts
                float subtractionTestNoise = detailNoise - IN.damage * 1.25;
                float subtractionTestFull = subtractionTestNoise + detailStructure * 0.25;
                clip (subtractionTestFull);

                // Next we need to get a mask of an area where surface is already peeled but structure is not yet peeled
                float structureMask = lerp (0, 1, saturate (-subtractionTestNoise * 100)); // float structureMask = abs (subtractionTestNoise - subtractionTestFull);
                float structureMaskShadow = lerp (0, 1, saturate (-subtractionTestNoise * 6));

                // Next we reuse the noise subtraction test to draw ramps
                if (subtractionTestNoise < _GlobalEnvironmentRampScale)
                {
                    // Traditional ramp mapping - use UV with zero Y and X based on our subtraction test
                    float4 ramp = tex2D (_GlobalRampBurnTex, float2 (subtractionTestNoise * (1 / _GlobalEnvironmentRampScale * 2 + 0.5), 0));

                    // Necessary to smoothly kill the ramp influence towards clean areas approaching edges of the blocks, without killing ramp intensity too much
                    float rampAlphaMultiplier = saturate ((1 - subtractionTestNoise) * IN.damage * 5); // IN.damage * 5;
                    ramp.w *= rampAlphaMultiplier;

                    // Time to add this 
                    albedoFinal = lerp (albedoFinal, albedoFinal * ramp.x, ramp.w * _GlobalEnvironmentRampInfluence);
                    albedoFinal = lerp (albedoFinal, _StructureColor * structureMaskShadow, structureMask);
                    smoothnessFinal = lerp (smoothnessFinal, smoothnessFinal * ramp.x, ramp.w);
                    smoothnessFinal = lerp (smoothnessFinal, 0, structureMask);

                    if (_DestructionAnimation > 0.01)
                    {
                        emissionFinal = saturate (emissionFinal + ramp.xyz * ramp.w);
                    }
                }
            }
            // */

            // Use this to test range of variables
            /*
            float variableSource = structureMaskShadow;
            float3 variableTest = float3 (variableSource, variableSource, variableSource);
            if (variableSource < 0)
                variableTest = float3 (1, 0.2, 0.2);
            else if (variableSource > 1)
                variableTest = float3 (0.2, 0.2, 1);
            else if (variableSource < 0.51 && variableSource > 0.49)
                variableTest = float3 (0.2, 1, 0.2);

            albedoFinal = variableTest;
            emissionFinal = variableTest;
            */
            

            // Use this to test local vs transformed positions
            // albedoFinal = lerp((IN.localPos.xxz + float3 (1.5, 1.5, 1.5)) / 3, (IN.transformedPos.xxz + float3 (1.5, 1.5, 1.5)) / 3, _Cutoff);

            output.Albedo = albedoFinal;
            output.Metallic = metalness;
            output.Smoothness = smoothnessFinal;
            output.Emission = emissionFinal;
            output.Occlusion = occlusionFinal;
            output.Normal = normalFinal;
            output.Alpha = 1 - IN.damage;
        }
        ENDCG
    }
    FallBack "Diffuse"
}