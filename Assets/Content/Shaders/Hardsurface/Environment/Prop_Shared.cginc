#include "Instancing_Shared.cginc"

struct Input
{
	float2 uv_MainTex;
	float2 texcoord_uv1 : TEXCOORD1;
	float3 localPos;
	float3 worldPos1;
	float3 worldNormal1;
	float3 worldNormal;
	float4 screenPos;
	float facingSign : VFACE;
    float4 packedPropData;
	float4 color : COLOR;
	INTERNAL_DATA
};

sampler2D _MainTex;
sampler2D _MSEO;
sampler2D _Bump;

uniform float _InstancePropsOverride;
float4 _HSBOffsetsPrimary;
float4 _HSBOffsetsSecondary;
float4 _PackedPropData;

fixed _NormalIntensity;
half _SmoothnessMin;
half _SmoothnessMed;
half _SmoothnessMax;

fixed _OcclusionIntensity;

fixed4 _EmissionColor;
float _EmissionIntensity;

float4 _CrushParameters;
float _CrushIntensity;

#ifdef _USE_WIND_DEFORMATION
	float _WindIntensity;
	float3 _WaveLengthScale;
	float3 _WaveTimeSpeedScale;
	float3 _WaveTimeOffset;
	float3 _WaveAmplitudeScale;
	float _WaveNoiseMask;
	float _WaveNoiseMaskSpeed;
#endif

float4 _CarPartsRotationMin;
float4 _CarPartsRotationMax;

float TOD_NightTimeSwitch;

float4 _GlobalEnvironmentSliceInputs;
float4 _GlobalEnvironmentSliceColor;

#pragma multi_compile __ _USE_SLICE_SHADING
#pragma multi_compile __ _USE_SLICE_CUTOFF

void ColorFunctionSliceShading
(
	Input IN,
	SurfaceOutputStandard surf,
	inout half4 outDiffuse : SV_Target0,          // RT0: diffuse color (rgb), occlusion (a)
	inout half4 outSpecSmoothness : SV_Target1,   // RT1: spec color (rgb), smoothness (a)
	inout half4 outNormal : SV_Target2,           // RT2: normal (rgb), --unused, very low precision-- (a)
	inout half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
)
{
	#ifdef _USE_SLICE_SHADING

	float y = IN.worldPos1.y;
	float fadeHeight = _GlobalEnvironmentSliceInputs.x;
	float fadeLength = max (0.1, _GlobalEnvironmentSliceInputs.y);
	float glowLength = max (0.1, _GlobalEnvironmentSliceInputs.z);

	float fade = 1;
	float glow = 0;

	if (_GlobalEnvironmentSliceInputs.w < 0.5)
	{
		fade = 1 - saturate ((y - fadeHeight + fadeLength) / fadeLength);
		glow = saturate ((y - fadeHeight + glowLength) / glowLength);
	}
	else
	{
		fade = saturate ((y - fadeHeight) / fadeLength);
		glow = 1 - saturate ((y - fadeHeight) / glowLength);
	}

	fade = lerp (1, fade, saturate ((_GlobalEnvironmentSliceInputs.y - 0.01) * 100));
	glow *= saturate (max (0, _GlobalEnvironmentSliceInputs.z - 0.01) * 100);
	glow = pow (glow, 2);
 
	outDiffuse *= fade;
	outEmission *= fade;
	outSpecSmoothness *= fade;
	outEmission.xyz += _GlobalEnvironmentSliceColor.xyz * (glow * _GlobalEnvironmentSliceColor.w);
			
	#endif
}

inline void ApplySliceCutoff (Input IN)
{
	#ifdef _USE_SLICE_CUTOFF
	
	float fadeHeight = _GlobalEnvironmentSliceInputs.x;
	float y = IN.worldPos1.y;

	if (_GlobalEnvironmentSliceInputs.w < 0.5)
		clip (fadeHeight - y);
	else
		clip (y - fadeHeight);

	#endif
}

void ApplyCarPartsVertexRotation(in float2 texcoord1, in float4 vcolor, in float4 hsbSecondaryProp, inout float4 vertexPos, inout float3 vertexNormal)
{
	#ifdef _USE_CAR_PARTS_ROTATION
		// Here's a little trick. If secondary HSB's Saturation value has '5' in the third place after decimal point - it means we need to disable the rotation.
		// Good to use for parked cars that should appear locked. So with S = 0.5 it's enabled, with 0.505 it's disabled, etc.
		float hsbSaturationMult = hsbSecondaryProp.y * 100;
		float rotationDisableFlag = (hsbSaturationMult - trunc (hsbSaturationMult)) * 10;
		// rotationDisableFlag shouldn't be 5 for the function to proceed, also taking floating point imprecision into account
		if (rotationDisableFlag < 4.1 || rotationDisableFlag > 5.9)
		{
			// Baked car part's pivot position value (X and Z axes only)
			float2 tempOffset = float2 (-texcoord1.x, texcoord1.y);

			// Use this as initial sine wave offset to randomize car part's rotation angle based on object's position + car part pivot position
			float3 pivotWorldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
			float positionBasedOffset = (pivotWorldPos.x + pivotWorldPos.y + pivotWorldPos.z + tempOffset.x + tempOffset.y) * 0.5;
			// Use this pseudo-noise wave to introduce randomness into the effect
			float pseudoNoiseMask = (sin (positionBasedOffset) + 1) * 0.5;

			// Check if there's any pivot position information baked into UV2 at all
			if (abs (texcoord1.x + texcoord1.y) > 0)
			{
				// Put car part in object's center before rotation
				vertexPos.xz -= tempOffset;
				// Mask for parts with 0.1 R value
				float partMask1 = saturate ((vcolor.r - 0.09) * 512);
				float partAngle1 = lerp (_CarPartsRotationMin.x, _CarPartsRotationMax.x, pseudoNoiseMask);
				// Mask for parts with 0.2 R value
				float partMask2 = saturate ((vcolor.r - 0.19) * 512);
				float partAngle2 = lerp (_CarPartsRotationMin.y, _CarPartsRotationMax.y, pseudoNoiseMask);
				// Mask for parts with 0.3 R value
				float partMask3 = saturate ((vcolor.r - 0.29) * 512);
				float partAngle3 = lerp (_CarPartsRotationMin.z, _CarPartsRotationMax.z, pseudoNoiseMask);
				// Mask for parts with 0.4 R value
				float partMask4 = saturate ((vcolor.r - 0.39) * 512);
				float partAngle4 = lerp (_CarPartsRotationMin.w, _CarPartsRotationMax.w, pseudoNoiseMask);
				// Feed in different angle value depending on the R vertex color mask value
				float angleValue = lerp (lerp (lerp (lerp (0, partAngle1, partMask1), partAngle2, partMask2), partAngle3, partMask3), partAngle4, partMask4);
				// Perform vertex-based rotation (rotate normals as well)
				vertexPos = RotateAroundYInDegrees (vertexPos, angleValue);
				vertexNormal = RotateAroundYInDegrees (float4 (vertexNormal, 0), angleValue).xyz;
				// Put car part back in its place
				vertexPos.xz += tempOffset;
			}
		}
	#endif
}

void SharedVertexFunctionProp (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT (Input, o);

	float4 scaleProp = float4(1, 1, 1, 1);
	float4 packedProp = float4(1, 0, 0, 1);
	float4 hsbSecondaryTemporaryProp = float4(0, 0, 0, 0);

	o.texcoord_uv1 = v.texcoord1;

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        scaleProp = scaleData[unity_InstanceID].Unpack();
        packedProp = packedPropData[unity_InstanceID].Unpack();
		hsbSecondaryTemporaryProp = hsbData[unity_InstanceID].UnpackSecondary();
    #endif

	if (_InstancePropsOverride > 0.0)
	{
        packedProp = _PackedPropData;
		hsbSecondaryTemporaryProp = _HSBOffsetsSecondary;
    }

	o.packedPropData = packedProp;
    float explosion = o.packedPropData.y;

	ApplyCarPartsVertexRotation(v.texcoord1, v.color, hsbSecondaryTemporaryProp, v.vertex, v.normal);

    v.vertex.xyz *= scaleProp;
	v.normal.xyz *= scaleProp;
	v.tangent.xyz *= scaleProp;

	o.localPos = v.vertex.xyz;
	o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
	o.worldNormal1 = mul (unity_ObjectToWorld, float4 (v.normal, 0)).xyz;

	v.vertex.xyz += (v.normal + frac (sin (dot (v.vertex.xz, float2 (12.9898, 78.233))) * 43758.5453)) * pow (explosion, 2) * 1;
}

void SharedVertexFunctionPropDeformable (inout appdata_full v, out Input o)
{
	UNITY_INITIALIZE_OUTPUT (Input, o);

	float4 scaleProp = float4(1, 1, 1, 1);
	float4 packedProp = float4(1, 0, 1, 1);
	float4 hsbSecondaryTemporaryProp = float4(0, 0, 0, 0);

	o.texcoord_uv1 = v.texcoord1;

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        scaleProp = scaleData[unity_InstanceID].Unpack();
        packedProp = packedPropData[unity_InstanceID].Unpack();
		hsbSecondaryTemporaryProp = hsbData[unity_InstanceID].UnpackSecondary();
    #endif

	if (_InstancePropsOverride > 0.0)
	{
        packedProp = _PackedPropData;
		hsbSecondaryTemporaryProp = _HSBOffsetsSecondary;
    }

	o.packedPropData = packedProp;
    float explosion = o.packedPropData.y;

	ApplyCarPartsVertexRotation(v.texcoord1, v.color, hsbSecondaryTemporaryProp, v.vertex, v.normal);
	
    v.vertex.xyz *= scaleProp;
	v.normal.xyz *= scaleProp;
	v.tangent.xyz *= scaleProp;

	o.localPos = v.vertex.xyz;
	o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
	o.worldNormal1 = mul (unity_ObjectToWorld, float4 (v.normal, 0.0f)).xyz;

	#ifdef _USE_WIND_DEFORMATION
		// Use this as initial sine wave offset to randomize the wind waves based on object's position
		float3 pivotWorldPos = mul (unity_ObjectToWorld, float4 (0, 0, 0, 1)).xyz;
		float positionBasedOffset = (pivotWorldPos.x + pivotWorldPos.y + pivotWorldPos.z) * 0.1;
		// Use this pseudo-noise wave to introduce randomness into the effect
		float pseudoNoiseMask = saturate (lerp (1 , (cos (sin (positionBasedOffset + (_Time.y * _WaveNoiseMaskSpeed) + cos (_Time.y * 2 * _WaveNoiseMaskSpeed))) - 0.5) * 2, _WaveNoiseMask));
		// The effect is always masked by vertex color alpha
		float windMask = v.color.a;

		float3 waves = sin (((positionBasedOffset + o.localPos.xyz) * _WaveLengthScale.xyz) + (_Time.y * _WaveTimeSpeedScale.xyz) + _WaveTimeOffset.xyz) * _WaveAmplitudeScale.xyz;
		float3 windWaves = waves * pseudoNoiseMask * windMask * _WindIntensity;
		v.vertex.xyz += windWaves;
	#endif

	float3 difference = float3
	(
		o.localPos.x - _CrushParameters.x,
		0,
		o.localPos.z - _CrushParameters.y
	);

	float distance = sqrt
	(
		difference.x * difference.x +
		difference.y * difference.y +
		difference.z * difference.z
	);

	distance = saturate (distance * _CrushParameters.w + _CrushParameters.z);
	distance = lerp (1, distance, _CrushIntensity);

	float integrity = packedProp.w;
	v.vertex.y = lerp (v.vertex.y, v.vertex.y - 1, pow (saturate (1 - integrity), 4));
	
	v.vertex.y = lerp (v.vertex.y * 0.01, v.vertex.y, distance);
	v.vertex.xyz += (v.normal + frac (sin (dot (v.vertex.xz, float2 (12.9898, 78.233))) * 43758.5453)) * pow (explosion, 2) * 1;
}

#if defined(SHADER_API_D3D11)
	float4 _WeatherParameters;
	UNITY_DECLARE_TEX2D(_CombatTerrainTexSplat);
	UNITY_DECLARE_TEX2D(_CombatTerrainTexWeather);
	float4 _CombatTerrainParamsScale;
#endif

// TODO: Make props use the ApplyWeather function from Environment_Shared include file instead
// Current problem: can't just include Environment_Shared in the prop shader, as it defines a different Input structure
float _WeatherMultiplier;
float _WeatherOcclusionIntensity;
float _WeatherOcclusionMaskPower;

// NOTE: This function is just copy-pasted from Environment_Shared.cginc
void ApplyWeather
(
	float weatherEffectsIntensity,
	inout float3 albedoFinal,
	inout float smoothnessFinal,
	inout float metalnessFinal,
	inout float3 normalFinal,
	float3 worldPos,
	float detailMask,
	float verticalFactor,
	float verticalFactorSnow,
	float occlusionMask = 1.0f
)
{
	#if defined(SHADER_API_D3D11)

	float distanceFadeRain = 1 - saturate ((distance (_WorldSpaceCameraPos, worldPos) / 200) - 0.1);

	float2 splatUV = float2 (worldPos.x, worldPos.z) / _CombatTerrainParamsScale.x; // Invert on Z avoid sync with terrain texturing
	splatUV += float2 (33, -44); // Offset to avoid sync with terrain texturing

	// Breakup map
	float4 splatSampleLarge = UNITY_SAMPLE_TEX2D (_CombatTerrainTexSplat, splatUV);

	// Secondary, higher frequency noise, with increased contrast
	float4 splatSampleSmall = UNITY_SAMPLE_TEX2D (_CombatTerrainTexSplat, splatUV * 10);

	// Weather-specific masks (snow mask, snow glint, rain mask, etc.)
	float4 weatherMasksSample = UNITY_SAMPLE_TEX2D (_CombatTerrainTexWeather, splatUV * 10);
	
	float rainIntensity = saturate (_WeatherParameters.x);
	float snowSurfaceIntensity = saturate (_WeatherParameters.y);
	// snowFallIntensity is not used here (i.e. not combined into precipitationIntensity as it has no direct effect on any surfaces)

	// Snow should mask out rain on all surfaces
	rainIntensity = lerp (rainIntensity, 0, snowSurfaceIntensity);

	// Base mask - large scale noise pushed up by rain intensity, with pushing masked by detail
	float rainMask = saturate (splatSampleLarge.x + lerp (0, 0.5, rainIntensity * detailMask));

	// Sharpen
	rainMask = saturate (pow (rainMask, 4));

	// Shrink proportionally to intensity
	rainMask *= saturate ((rainIntensity + 0.5) * 0.7);
	
	float rainMaskAccent = ContrastGrayscale (weatherMasksSample.b, lerp (0.9, 0.01, rainIntensity)) * 0.5;

	// Overlay, preserving overall shape of original mask
	rainMask = OverlayGrayscale (rainMask, rainMaskAccent);

	// Make higher frequency mask push through empty areas proportional to overall rain intensity
	rainMask = saturate (rainMask + rainMaskAccent * rainIntensity);

	// Make everywhere fill up a bit proportionally to exp of rain intensity, so that no dry spots remain at max rain
	rainMask = saturate (rainMask + pow (max (0, rainIntensity - 0.1), 2) * 0.5);

	// Fade by distance and slope
	rainMask *= verticalFactor * distanceFadeRain;

	// Cheat a bit and push it on all surfaces, for as long as they aren't pitch black
	rainMask = saturate (rainMask + 0.5 * rainIntensity * saturate (albedoFinal.x * 4));

	// Ensure rain mask can't reach too high of an absolute value
	rainMask = min (rainMask, 0.8);

	// Ensure rain mask is at absolute 0 at 0 rain
	rainMask = lerp (0, rainMask, saturate (rainIntensity * 10));

	// Apply optional occlusion to the rainMask
	rainMask *= occlusionMask;

	// Enable or disable weather effects if desired
	rainMask *= weatherEffectsIntensity;

	// Wetness pushes albedo to water color, but up to a limit, as "depth" isn't that deep
	float3 rainColor = float3 (0.05, 0.065, 0.08); // float3 (0.05, 0.1, 0.15);
	albedoFinal.xyz = lerp (albedoFinal.xyz, rainColor, rainMask * 0.66);

	// float smoothnessAccent = saturate (splatSampleSmall.y * (_SinTime.w * 0.5 + 1) + splatSampleSmall.w * (_CosTime.w * 0.5 + 1) - 2);
	float smoothnessRain = lerp (0.9, 0.75, weatherMasksSample.b);

	// Wetness pushes smoothness and metalness up, but up to a limit, to avoid shrinking highlights too much
	metalnessFinal = lerp (metalnessFinal, rainIntensity * 0.9, rainMask);

	// Mix in some glint noise into rain mask (gets stronger further away from)
	float rainMaskNoiseMixIn = saturate (weatherMasksSample.g + lerp (0.95, 0.75, 1 - saturate ((distanceFadeRain - 0.75) * 4)));
	// Make sure original smoothness texture is taken into account as well
	smoothnessRain = smoothnessRain * saturate (smoothnessFinal + 0.7);
	smoothnessFinal = lerp (smoothnessFinal, smoothnessRain, rainMask * rainMaskNoiseMixIn);

	// Increased reflectivity looks bad with prominent original normals so normals should fade at greater (inv. exp.) rate
	normalFinal = lerp (normalFinal, float3 (0,0,1), 1 - pow (1 - rainMask, 2));

	// Base snow mask - two noise sizes pushed up from negative point
	float snowMask = saturate (splatSampleLarge.z + lerp (weatherMasksSample.r, 1, pow (splatSampleLarge.z, 2) * snowSurfaceIntensity) - 1 + snowSurfaceIntensity);

	// Boost snow at higher elevations if it is intense enough
	float gradientSize = _CombatTerrainParamsScale.w - _CombatTerrainParamsScale.z;
	float gradientUVBase = saturate ((worldPos.y - _CombatTerrainParamsScale.z) / max (0.01, gradientSize));
	float snowBoost = pow (gradientUVBase, 2) * saturate (splatSampleLarge + 0.75) * saturate (weatherMasksSample.r + 0.9);
	snowBoost = saturate (snowBoost - splatSampleLarge * (1 - snowSurfaceIntensity));
	snowMask = saturate (snowMask + snowBoost);

	// Fade by slope
	snowMask *= verticalFactorSnow;

	// Ensure snow mask can't be above 0 at snow intensity 0
	snowMask *= saturate (snowSurfaceIntensity * 5);

	// Apply optional occlusion to the snowMask
	snowMask *= occlusionMask;

	// Enable or disable weather effects if desired
	snowMask *= weatherEffectsIntensity;

	// Wetness pushes albedo to water color, but up to a limit, as "depth" isn't that deep
	float3 snowColor = float3 (0.5, 0.5, 0.55); // float3 (0.05, 0.1, 0.15);
	albedoFinal.xyz = lerp (albedoFinal.xyz, snowColor, snowMask);

	// Wetness pushes smoothness and metalness up, but up to a limit, to avoid shrinking highlights too much
	metalnessFinal = lerp (metalnessFinal, 0, snowMask);
	float snowSmoothness = lerp (0.2, 0.6, weatherMasksSample.g);
	smoothnessFinal = lerp (smoothnessFinal, snowSmoothness, snowMask);

	// Normals should fade at greater (inv. exp.) rate
	normalFinal = lerp (normalFinal, float3 (0,0,1), 1 - pow (1 - snowMask, 2));	
				
	#endif
}