// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Hardsurface/Environment/Block (standard)"
{
	Properties
	{
		_HSBOffsetsPrimary ("HSB offsets (primary) + Emission", Vector) = (0.5, 0.5, 0.5, 1)
		_HSBOffsetsSecondary ("HSB offsets (secondary) + Damage", Vector) = (0.5, 0.5, 0.5, 1)
		_MainTex ("AH", 2D) = "white" {}
		_MSEO ("MSEO", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}
		_SmoothnessMin ("SmoothnessMin", Range (0, 1)) = 0.0
		_SmoothnessMed ("SmoothnessMed", Range (0, 1)) = 0.2
		_SmoothnessMax ("SmoothnessMax", Range (0, 1)) = 0.8
		_EmissionIntensity ("Emission intensity", Range (0, 16)) = 0
		_EmissionColor ("Emission color", Color) = (0, 0, 0, 1)
        _WorldSpaceUVOverride ("World space UV override", Range (0, 10)) = 0

		_Scale ("Scale", Vector) = (1, 1, 1, 1)
		_StructureColor ("Structure color", Color) = (0, 0, 0, 1)
		_DestructionAnimation ("Destruction animation", Range (0, 1)) = 0
		_DamageTop ("Damage anim. (0-3)", Vector) = (1, 1, 1, 1)
		_DamageBottom ("Damage anim. (4-7)", Vector) = (1, 1, 1, 1)
		_IntegrityTop ("Integrities (0-3)", Vector) = (1, 1, 1, 1)
		_IntegrityBottom ("Integrities (4-7)", Vector) = (1, 1, 1, 1)
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

		#pragma surface surf Standard fullforwardshadows vertex:SharedVertexFunction addshadow
		#pragma target 5.0
		#pragma only_renderers d3d11 vulkan
		#include "UnityCG.cginc"
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Environment_Shared.cginc"
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup

		// Config maxcount. See manual page.
		// #pragma instancing_options


		sampler2D _MainTex;
		sampler2D _MSEO;
		sampler2D _Bump;

		half _SmoothnessMin;
		half _SmoothnessMed;
		half _SmoothnessMax;

		fixed4 _EmissionColor;
		float _EmissionIntensity;
        half _WorldSpaceUVOverride;

		void surf (Input IN, inout SurfaceOutputStandard output)
		{
			float destructionAnimation = 0;
			float4 hsbPrimary = float4(0,0.5,0.5,0);
			float4 hsbSecondary = float4(0,0.5,0.5,0);
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

				HalfVector8 cachedHSB = hsbData[unity_InstanceID];
				hsbPrimary = cachedHSB.UnpackPrimary();
				hsbSecondary = cachedHSB.UnpackSecondary();
				destructionAnimation = hsbSecondary.w;//cachedHSBSecondary = float4(0,0,0,1);.w; // UNITY_ACCESS_INSTANCED_PROP (_DestructionAnimation_arr, _DestructionAnimation);

			#endif

			if (destructionAnimation > 0.99)
			{
				clip (-1);
			}
			else
			{
				fixed4 ah = lerp (tex2D (_MainTex, IN.texcoord_uv1), tex2D (_MainTex, IN.worldPos1.xz * 0.09765625), _WorldSpaceUVOverride);
				fixed4 mseo = lerp (tex2D (_MSEO, IN.texcoord_uv1), tex2D (_MSEO, IN.worldPos1.xz * 0.09765625), _WorldSpaceUVOverride);

				fixed3 albedoBase = ah.rgb;
				fixed albedoMask = ah.a;
				fixed metalness = mseo.x;
				fixed smoothness = mseo.y;
				fixed emissionFromTexture = mseo.z;
				fixed occlusionFromTexture = mseo.w;

				// Normal correction necessary due to disabled backface culling
				float3 n = float3 (0, 0, 1);
				float backsideFactor = GetBacksideFactor (IN.viewDir);
				float3 normalFinal = lerp (n, -n, backsideFactor);

				float3 albedoFinal = GetAlbedo (hsbPrimary.xyz, hsbSecondary.xyz, albedoBase, albedoMask, occlusionFromTexture);
				float smoothnessFinal = GetSmoothness (smoothness, _SmoothnessMin, _SmoothnessMed, _SmoothnessMax, backsideFactor);
				float3 emissionFinal = GetEmission (IN.damageIntegrityCriticality.x, hsbSecondary.w, emissionFromTexture, _EmissionColor, _EmissionIntensity);
				float occlusionFinal = GetOcclusion (IN.worldPos1.y, IN.worldNormal1.y, occlusionFromTexture);

				float4 detail = GetDetailSample (IN.worldPos1, IN.worldNormal1);
				float integrityMultiplier = GetIntegrityMultiplier (IN.damageIntegrityCriticality.y, detail);

				albedoFinal *= integrityMultiplier;
				smoothnessFinal *= integrityMultiplier;
				emissionFinal *= integrityMultiplier;

				// Finally, time for damage
				if (IN.damageIntegrityCriticality.x > 0.001)
				{
					// Extracting noise and using it to set opacity from damage, along with applying contrast to the noise
					float detailNoise = saturate ((detail.z - 0.5) * lerp (_GlobalEnvironmentDetailContrast, 1, destructionAnimation) + 0.5 - 0.25);
					float detailStructure = detail.y / 2;

					// Noise is a rich set of uniformly brigtness-distributed values which damage pushes below zero to create the cuts
					float subtractionTestNoise = detailNoise - IN.damageIntegrityCriticality.x * 1.25;
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
						float rampAlphaMultiplier = saturate ((1 - subtractionTestNoise) * IN.damageIntegrityCriticality.x * 5); // IN.damage * 5;
						ramp.w *= rampAlphaMultiplier;

						// Time to add this
						albedoFinal = lerp (albedoFinal, albedoFinal * ramp.x, ramp.w * _GlobalEnvironmentRampInfluence);
						albedoFinal = lerp (albedoFinal, _StructureColor * structureMaskShadow, structureMask);
						smoothnessFinal = lerp (smoothnessFinal, smoothnessFinal * ramp.x, ramp.w);
						smoothnessFinal = lerp (smoothnessFinal, 0, structureMask);

						if (destructionAnimation > 0.5)
						{
							emissionFinal = saturate (emissionFinal + ramp.xyz * ramp.w);
						}
					}
				}

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

				// Rain
				// metalness = saturate(metalness + _RainIntensity);
				// smoothnessFinal = saturate(smoothnessFinal + ((smoothnessFinal + 0.8) * _RainIntensity));

				output.Albedo = albedoFinal;//lerp(float4(0,0,0,0), float4(1,1,1,1), IN.outOfBounds);
				output.Metallic = metalness;
				output.Smoothness = smoothnessFinal;
				output.Emission = emissionFinal;
				output.Occlusion = occlusionFinal;
				output.Normal = normalFinal;
				output.Alpha = 1 - IN.damageIntegrityCriticality.x;

				output.Albedo = 0;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}

			/*
			// Use this to verify if masks are linear, with midpoint at 0.5
			float3 colorFinal = float3 (0, 0, 0);
			if (albedoMask < 0.49)
				colorFinal = float3 (0, 0, 0);
			else if (albedoMask > 0.51)
				colorFinal = float3 (1, 1, 1);
			else
				colorFinal = float3 (1, 0, 0);
			*/

			// Noise/damage
			/*
			const float epsilon = 0.0001;
			float2 uv = 1 * 4.0 + float2(0.2, 1) * _DamageOffset;
			float o = 0.5;
			float s = 1.0;
			float w = 0.5;

			for (int i = 0; i < 6; i++)
			{
				float3 coord = IN.worldPos * s * _DamageSize + float3 (_DamageOffset, _DamageOffset, _DamageOffset);
				float3 period = float3(s, s, 1.0) * 2.0;

				o += pnoise(coord, period) * w;
				s *= 2.0;
				w *= 0.5;
			}
			float alphaFinal = lerp (1, o, _Damage);
			*/

			// Noise/damage
			/*
			float3 mod(float3 x, float3 y)
			{
			return x - y * floor(x / y);
			}

			float3 mod289(float3 x)
			{
			return x - floor(x / 289.0) * 289.0;
			}

			float4 mod289(float4 x)
			{
			return x - floor(x / 289.0) * 289.0;
			}

			float4 permute(float4 x)
			{
			return mod289(((x*34.0) + 1.0)*x);
			}

			float4 taylorInvSqrt(float4 r)
			{
			return (float4)1.79284291400159 - r * 0.85373472095314;
			}

			float3 fade(float3 t) {
			return t*t*t*(t*(t*6.0 - 15.0) + 10.0);
			}

			// Classic Perlin noise
			float cnoise(float3 P)
			{
			float3 Pi0 = floor(P); // Integer part for indexing
			float3 Pi1 = Pi0 + (float3)1.0; // Integer part + 1
			Pi0 = mod289(Pi0);
			Pi1 = mod289(Pi1);
			float3 Pf0 = frac(P); // Fractional part for interpolation
			float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
			float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
			float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
			float4 iz0 = (float4)Pi0.z;
			float4 iz1 = (float4)Pi1.z;

			float4 ixy = permute(permute(ix) + iy);
			float4 ixy0 = permute(ixy + iz0);
			float4 ixy1 = permute(ixy + iz1);

			float4 gx0 = ixy0 / 7.0;
			float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
			gx0 = frac(gx0);
			float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
			float4 sz0 = step(gz0, (float4)0.0);
			gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
			gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

			float4 gx1 = ixy1 / 7.0;
			float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
			gx1 = frac(gx1);
			float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
			float4 sz1 = step(gz1, (float4)0.0);
			gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
			gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

			float3 g000 = float3(gx0.x, gy0.x, gz0.x);
			float3 g100 = float3(gx0.y, gy0.y, gz0.y);
			float3 g010 = float3(gx0.z, gy0.z, gz0.z);
			float3 g110 = float3(gx0.w, gy0.w, gz0.w);
			float3 g001 = float3(gx1.x, gy1.x, gz1.x);
			float3 g101 = float3(gx1.y, gy1.y, gz1.y);
			float3 g011 = float3(gx1.z, gy1.z, gz1.z);
			float3 g111 = float3(gx1.w, gy1.w, gz1.w);

			float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
			g000 *= norm0.x;
			g010 *= norm0.y;
			g100 *= norm0.z;
			g110 *= norm0.w;

			float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
			g001 *= norm1.x;
			g011 *= norm1.y;
			g101 *= norm1.z;
			g111 *= norm1.w;

			float n000 = dot(g000, Pf0);
			float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
			float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
			float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
			float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
			float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
			float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
			float n111 = dot(g111, Pf1);

			float3 fade_xyz = fade(Pf0);
			float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
			float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
			float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
			return 2.2 * n_xyz;
			}

			// Classic Perlin noise, periodic variant
			float pnoise(float3 P, float3 rep)
			{
			float3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
			float3 Pi1 = mod(Pi0 + (float3)1.0, rep); // Integer part + 1, mod period
			Pi0 = mod289(Pi0);
			Pi1 = mod289(Pi1);
			float3 Pf0 = frac(P); // Fractional part for interpolation
			float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
			float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
			float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
			float4 iz0 = (float4)Pi0.z;
			float4 iz1 = (float4)Pi1.z;

			float4 ixy = permute(permute(ix) + iy);
			float4 ixy0 = permute(ixy + iz0);
			float4 ixy1 = permute(ixy + iz1);

			float4 gx0 = ixy0 / 7.0;
			float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
			gx0 = frac(gx0);
			float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
			float4 sz0 = step(gz0, (float4)0.0);
			gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
			gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

			float4 gx1 = ixy1 / 7.0;
			float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
			gx1 = frac(gx1);
			float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
			float4 sz1 = step(gz1, (float4)0.0);
			gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
			gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

			float3 g000 = float3(gx0.x, gy0.x, gz0.x);
			float3 g100 = float3(gx0.y, gy0.y, gz0.y);
			float3 g010 = float3(gx0.z, gy0.z, gz0.z);
			float3 g110 = float3(gx0.w, gy0.w, gz0.w);
			float3 g001 = float3(gx1.x, gy1.x, gz1.x);
			float3 g101 = float3(gx1.y, gy1.y, gz1.y);
			float3 g011 = float3(gx1.z, gy1.z, gz1.z);
			float3 g111 = float3(gx1.w, gy1.w, gz1.w);

			float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
			g000 *= norm0.x;
			g010 *= norm0.y;
			g100 *= norm0.z;
			g110 *= norm0.w;
			float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
			g001 *= norm1.x;
			g011 *= norm1.y;
			g101 *= norm1.z;
			g111 *= norm1.w;

			float n000 = dot(g000, Pf0);
			float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
			float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
			float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
			float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
			float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
			float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
			float n111 = dot(g111, Pf1);

			float3 fade_xyz = fade(Pf0);
			float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
			float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
			float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
			return 2.2 * n_xyz;
			}
			*/
