#include "Instancing_Shared.cginc"
#pragma exclude_renderers gles
#pragma only_renderers d3d11 d3d11_9x vulkan

struct Input
{
	float2 texcoord_uv1 : TEXCOORD0;
	float2 texcoord_uv2 : TEXCOORD1;

	float3 worldPos1;
	float3 worldNormal1;
	float3 worldNormal;

	float3 damageIntegrityCriticality;

    float4 color : COLOR;
	float3 viewDir;
    float2 triplanarUV;

	float4 screenPos;

	float facingSign : VFACE;
	float colorVariation;

	UNITY_VERTEX_INPUT_INSTANCE_ID
	INTERNAL_DATA
};

sampler2D _GlobalDetailTex;
sampler2D _GlobalRampBurnTex;
sampler2D _GlobalBackgroundGrassTex;

sampler2D _SideTex;
float _TexScaleSediment;
float _SideDeformIntensity;

float _GlobalEnvironmentDetailScale;
float _GlobalEnvironmentDetailContrast;
float _GlobalEnvironmentRampScale;
float _GlobalEnvironmentRampInfluence;
float _GlobalEnvironmentDamageOffset;
float4 _GlobalEnvironmentAmbientSettings;
float _DestructionMaskContrast;

fixed4 _StructureColor;
float _ScaleTestToggle;
float3 _ScaleTestValue;
fixed _PositionTest;
float4 _WeatherParameters;

float TOD_NightTimeSwitch;
float TOD_NightToNoonInterpolant;

float4 _GlobalIsolineColor;
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

// Terrain fallbacks in case arrays won't work






// Built-in Unity defines for reference
// #define TEXTURE2D(textureName)                Texture2D textureName
// #define SAMPLER(samplerName)                  SamplerState samplerName
// #define TEXTURE2DSAMPLER(tex,samplertex) tex, sampler##samplertex

// #define SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)                      textureName.SampleLevel(samplerName, coord2, lod)
// #define SAMPLE_TEXTURE2D_BIAS(textureName, samplerName, coord2, bias)                    textureName.SampleBias(samplerName, coord2, bias)
// #define SAMPLE_TEXTURE2D_GRAD(textureName, samplerName, coord2, dpdx, dpdy)              textureName.SampleGrad(samplerName, coord2, dpdx, dpdy)

// #define UNITY_DECLARE_TEX2D(tex) Texture2D tex; SamplerState sampler##tex
// #define UNITY_DECLARE_TEX2D_NOSAMPLER(tex) Texture2D tex

// This compiles at all times
// UNITY_DECLARE_TEX2D(_CombatTerrainTexSplat);

float4 _CombatTerrainSlopeParams;
sampler2D _CombatTerrainTexDistant;
sampler2D _CombatTerrainTexSlope;

#if defined(SHADER_API_D3D11) || defined(SHADER_API_VULKAN)

// Terrain texturing properties
float4 _CombatTerrainPresetMapping;
float4 _CombatTerrainParamsScale;
float4 _CombatTerrainParamsBlendContrast;
float4 _CombatTerrainParamsBlendOffset;

float4 _CombatTerrainDetail1HSB;
float4 _CombatTerrainDetail2HSB;
float4 _CombatTerrainDetail3HSB;
float4 _CombatTerrainDetail4HSB;

// Testing whether replicating built-in defines compiles fine
#define DECLARE_TEX2D(tex)										Texture2D tex; SamplerState sampler##tex
#define DECLARE_TEX2D_NOSAMPLER(tex)							Texture2D tex

// This doesn't compile
// DECLARE_TEX2D(_CombatTerrainTexSplat);

// Actual final defines that all need to work
// Used in function signatures
#define ARGS_TEX2D(tex)											Texture2D tex, SamplerState sampler##tex
#define ARGS_TEX2D_SAMPLER(tex, samplertex)						Texture2D tex, SamplerState sampler##samplertex

// Used in function bodies receiving arguments via ARGS
#define SAMPLE_TEX2D(tex, samplertex, coord)					tex.Sample (sampler##samplertex,coord)
#define SAMPLE_TEX2D_LOD(tex, samplertex, coord, lod)			tex.SampleLevel(sampler##samplertex, coord, lod)
#define SAMPLE_TEX2D_BIAS(tex, samplertex, coord, bias)			tex.SampleBias(sampler##samplertex, coord, bias)
#define SAMPLE_TEX2D_GRAD(tex, samplertex, coord, dpdx, dpdy)	tex.SampleGrad(sampler##samplertex, coord, dpdx, dpdy)

// Used in function invocations
#define PASS_TEX2D(tex) tex, sampler##tex
#define PASS_TEX2D_SAMPLER(tex, samplertex) tex, sampler##samplertex

DECLARE_TEX2D(_CombatTerrainTexGradient);
DECLARE_TEX2D(_CombatTerrainTexSplat);
DECLARE_TEX2D(_CombatTerrainTexWeather);

DECLARE_TEX2D(_CombatTerrainTexDetail1AH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail1NH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail2AH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail2NH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail3AH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail3NH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail4AH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail4NH);

#endif








/*
// Actual final declarations that need to work
DECLARE_TEX2D(_CombatTerrainTexSplat);
DECLARE_TEX2D(_CombatTerrainTexDetail1AH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail1NH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail2AH);
DECLARE_TEX2D_NOSAMPLER(_CombatTerrainTexDetail2NH);
*/











// Example of 4 custom defines wrapping DX11 types
/*
// properties
_MyTexArray ("My Texture Array", 2DArray) = "white" {}

// unform definition
UNITY_DECLARE_TEX2DARRAY(_MyTexArray);

// custom function to sample texture
half4 MyFunction(UNITY_ARGS_TEX2DARRAY(_MyTexArray, float2 uv, float index)
{
return UNITY_SAMPLE_TEX2DARRAY(_MyTexArray, float3(uv, index));
}

// calling function
half4 col = MyFunction(UNITY_PASS_TEX2DARRAY(_MyTexArray), uv, index);
*/

//hash for randomness
float2 hash2D2D (float2 s)
{
	//magic numbers
	return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159))*43758.5453);
}

// #define SAMPLE_STH(tex, sampler, uv, BW_vx, dx, dy) mul(tex.SampleGrad(sampler, uv + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) + mul(tex.SampleGrad(sampler, uv + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) + mul(tex.SampleGrad(sampler, uv + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z)
// #define SAMPLE_TEXTURE2D_GRAD(textureName, samplerName, coord2, dpdx, dpdy)              textureName.SampleGrad(samplerName, coord2, dpdx, dpdy)
// #define SAMPLE_STH2(textureName, samplerName, coord2) textureName.SampleGrad(samplerName, coord2, dpdx, dpdy)

void SharedShadowVertexFunctionNoDamage(inout appdata_full v, out Input o)
{
	UNITY_SETUP_INSTANCE_ID (v);
	UNITY_INITIALIZE_OUTPUT (Input, o);
	UNITY_TRANSFER_INSTANCE_ID (v, o);
}

void SharedShadowVertexFunction(inout appdata_full v, out Input o)
{
    UNITY_SETUP_INSTANCE_ID (v);
	UNITY_INITIALIZE_OUTPUT (Input, o);
	UNITY_TRANSFER_INSTANCE_ID (v, o);

	float4 scaleAndSpinProp = float4(1, 1, 1, 0);
	fixed4 damageTopProp = fixed4(1, 1, 1, 1);
	fixed4 damageBottomProp = fixed4(1, 1, 1, 1);
	fixed4 integrityTopProp = fixed4(1, 1, 1, 1);
	fixed4 integrityBottomProp = fixed4(1, 1, 1, 1);

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		scaleAndSpinProp = scaleData[unity_InstanceID].Unpack();

		HalfVector8 damagePacked = damageData[unity_InstanceID];
		damageTopProp = damagePacked.UnpackPrimary();
		damageBottomProp = damagePacked.UnpackSecondary();

		FixedVector8 integrityPacked = integrityData[unity_InstanceID];
		integrityTopProp = integrityPacked.UnpackPrimary();
		integrityBottomProp = integrityPacked.UnpackSecondary();
    #endif

    scaleAndSpinProp.xyz = lerp (scaleAndSpinProp.xyz, _ScaleTestValue.xyz, _ScaleTestToggle);

    v.vertex.xyz *= scaleAndSpinProp.xyz;
    v.normal.xyz *= scaleAndSpinProp.xyz;
    v.tangent.xyz *= scaleAndSpinProp.xyz;

	// o.localNormal = v.normal.xyz;
	// o.localPos = v.vertex.xyz;
	o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
    o.worldNormal1 = UnityObjectToWorldNormal (v.normal);

	float3 bounds = float3(1.5, 1.5, 1.5);
    float4 transformedPos = RotateAroundYInDegrees (float4 (v.vertex.xyz, 0), -scaleAndSpinProp.w);

	float3 positive = clamp ((transformedPos.xyz + bounds) * 0.33333, -1, 1);
	float3 negative = clamp ((-transformedPos.xyz + bounds) * 0.33333, -1, 1);

	float4 topCorners = float4
	(
		negative.x * positive.y * negative.z,
		positive.x * positive.y * negative.z,
		negative.x * positive.y * positive.z,
		positive.x * positive.y * positive.z
	);

	float4 bottomCorners = float4
	(
		negative.x * negative.y * negative.z,
		positive.x * negative.y * negative.z,
		negative.x * negative.y * positive.z,
		positive.x * negative.y * positive.z
	);

	fixed4 damageTop = fixed4 (1, 1, 1, 1) - damageTopProp;
	fixed4 damageBottom = fixed4 (1, 1, 1, 1) - damageBottomProp;
	fixed4 integrityTop = fixed4 (1, 1, 1, 1) - integrityTopProp;
	fixed4 integrityBottom = fixed4 (1, 1, 1, 1) - integrityBottomProp;

	float4 damageTopScaled = topCorners * damageTop;
	float4 damageBottomScaled = bottomCorners * damageBottom;

	float4 integrityTopScaled = topCorners * integrityTop;
	float4 integrityBottomScaled = bottomCorners * integrityBottom;

	float damageVectorDifference =
	(
		1 -
		damageTopScaled.x -
		damageTopScaled.y -
		damageTopScaled.z -
		damageTopScaled.w -
		damageBottomScaled.x -
		damageBottomScaled.y -
		damageBottomScaled.z -
		damageBottomScaled.w
	);

	float integrityVectorDifference =
	(
		1 -
		integrityTopScaled.x -
		integrityTopScaled.y -
		integrityTopScaled.z -
		integrityTopScaled.w -
		integrityBottomScaled.x -
		integrityBottomScaled.y -
		integrityBottomScaled.z -
		integrityBottomScaled.w
	);

	float damage = 1 - max (0, damageVectorDifference);
	damage = saturate (damage * _GlobalEnvironmentDamageOffset);
	float integrity = saturate (integrityVectorDifference);

    o.damageIntegrityCriticality = float3 (damage, integrity, 0);


	// Distort the shape a bit
	v.vertex.xyz = lerp (v.vertex.xyz, v.vertex.xyz - v.normal * 2, damage * damage);
}

void SharedVertexFunction (inout appdata_full v, out Input o)
{
    UNITY_SETUP_INSTANCE_ID (v);
	UNITY_INITIALIZE_OUTPUT (Input, o);
	UNITY_TRANSFER_INSTANCE_ID (v, o);

	// There are no traditional 2D samplers used in this surface shader, so UV inputs won't be auto-generated
	// we have to fill the UV1 and UV2 manually (UV2 packs array index)
	o.texcoord_uv1 = v.texcoord;
	o.texcoord_uv2 = v.texcoord1;

	float4 scaleAndSpinProp = float4(1, 1, 1, 0);
	fixed4 damageTopProp = fixed4(1, 1, 1, 1);
	fixed4 damageBottomProp = fixed4(1, 1, 1, 1);
	fixed4 integrityTopProp = fixed4(1, 1, 1, 1);
	fixed4 integrityBottomProp = fixed4(1, 1, 1, 1);

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		scaleAndSpinProp = scaleData[unity_InstanceID].Unpack();

		HalfVector8 damagePacked = damageData[unity_InstanceID];
		damageTopProp = damagePacked.UnpackPrimary();
		damageBottomProp = damagePacked.UnpackSecondary();

		FixedVector8 integrityPacked = integrityData[unity_InstanceID];
		integrityTopProp = integrityPacked.UnpackPrimary();
		integrityBottomProp = integrityPacked.UnpackSecondary();
    #endif

    scaleAndSpinProp.xyz = lerp (scaleAndSpinProp.xyz, _ScaleTestValue.xyz, _ScaleTestToggle);

    v.vertex.xyz *= scaleAndSpinProp.xyz;
    v.normal.xyz *= scaleAndSpinProp.xyz;
    v.tangent.xyz *= scaleAndSpinProp.xyz;

	// o.localNormal = v.normal.xyz;
	// o.localPos = v.vertex.xyz;
	o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
    o.worldNormal1 = UnityObjectToWorldNormal (v.normal);

	float3 bounds = float3(1.5, 1.5, 1.5);
    float4 transformedPos = RotateAroundYInDegrees (float4 (v.vertex.xyz, 0), -scaleAndSpinProp.w);

	float3 positive = clamp ((transformedPos.xyz + bounds) * 0.33333, -1, 1);
	float3 negative = clamp ((-transformedPos.xyz + bounds) * 0.33333, -1, 1);

	float4 topCorners = float4
	(
		negative.x * positive.y * negative.z,
		positive.x * positive.y * negative.z,
		negative.x * positive.y * positive.z,
		positive.x * positive.y * positive.z
	);

	float4 bottomCorners = float4
	(
		negative.x * negative.y * negative.z,
		positive.x * negative.y * negative.z,
		negative.x * negative.y * positive.z,
		positive.x * negative.y * positive.z
	);

	fixed4 damageTop = fixed4 (1, 1, 1, 1) - damageTopProp;
	fixed4 damageBottom = fixed4 (1, 1, 1, 1) - damageBottomProp;
	fixed4 integrityTop = fixed4 (1, 1, 1, 1) - integrityTopProp;
	fixed4 integrityBottom = fixed4 (1, 1, 1, 1) - integrityBottomProp;

	float4 damageTopScaled = topCorners * damageTop;
	float4 damageBottomScaled = bottomCorners * damageBottom;

	float4 integrityTopScaled = topCorners * integrityTop;
	float4 integrityBottomScaled = bottomCorners * integrityBottom;

	float damageVectorDifference =
	(
		1 -
		damageTopScaled.x -
		damageTopScaled.y -
		damageTopScaled.z -
		damageTopScaled.w -
		damageBottomScaled.x -
		damageBottomScaled.y -
		damageBottomScaled.z -
		damageBottomScaled.w
	);

	float integrityVectorDifference =
	(
		1 -
		integrityTopScaled.x -
		integrityTopScaled.y -
		integrityTopScaled.z -
		integrityTopScaled.w -
		integrityBottomScaled.x -
		integrityBottomScaled.y -
		integrityBottomScaled.z -
		integrityBottomScaled.w
	);

	float damage = 1 - max (0, damageVectorDifference);
	damage = saturate (damage * _GlobalEnvironmentDamageOffset);
	float integrity = saturate (integrityVectorDifference);

    o.damageIntegrityCriticality = float3 (damage, integrity, 0);

    // Triplanar projection of shared detail texture - should ideally be replaced by a single 3d noise sample
    float3 triplanarNormal = saturate (pow (v.normal * 1.4, 4));
    float2 uvX = (v.vertex.zy / _GlobalEnvironmentDetailScale + float2 (0.25, 0.125)) * abs (v.normal.x);
    float2 uvY = (v.vertex.zx / _GlobalEnvironmentDetailScale + float2 (-0.25, -0.125)) * abs (v.normal.y);
    float2 uvZ = (v.vertex.xy / _GlobalEnvironmentDetailScale + float2 (0.35, 0.35)) * abs (v.normal.z);
    o.triplanarUV = uvZ;
    o.triplanarUV = lerp (o.triplanarUV, uvX, triplanarNormal.x);
    o.triplanarUV = lerp (o.triplanarUV, uvY, triplanarNormal.y);

	/*float3 normalFlat = normalize (float3 (o.worldNormal1.x, 0.01f, o.worldNormal1.z));

	float3 sideMask = saturate(abs(o.worldNormal1.x) + abs(o.worldNormal1.z));

	float4 masksVerticalSedimentXY = tex2Dlod (_SideTex, float4 (o.worldPos1.x * 0.666, o.worldPos1.y, 0, 0) / _TexScaleSediment);
	float4 masksVerticalSedimentZY = tex2Dlod (_SideTex, float4 (o.worldPos1.z * 0.666, o.worldPos1.y, 0, 0) / _TexScaleSediment);
	float4 masksVerticalSediment = masksVerticalSedimentXY * abs (normalFlat.z) + masksVerticalSedimentZY * abs (normalFlat.x);

	v.vertex.xyz += ((v.normal.xyz * ((masksVerticalSediment.b - 0.5) * 2) * _SideDeformIntensity) * sideMask);*/

	// Distort the shape a bit
	v.vertex.xyz = lerp (v.vertex.xyz, v.vertex.xyz - v.normal * 2, damage * damage);
}

void SharedVertexFunctionLight (inout appdata_full v, out Input o)
{
    UNITY_SETUP_INSTANCE_ID (v);
	UNITY_INITIALIZE_OUTPUT (Input, o);
	UNITY_TRANSFER_INSTANCE_ID (v, o);

	// There are no traditional 2D samplers used in this surface shader, so UV inputs won't be auto-generated
	// we have to fill the UV1 and UV2 manually (UV2 packs array index)
	o.texcoord_uv1 = v.texcoord;
	o.texcoord_uv2 = v.texcoord1;

	float4 scaleAndSpinProp = float4(1, 1, 1, 0);

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	    scaleAndSpinProp = scaleData[unity_InstanceID].Unpack();
    #endif

    scaleAndSpinProp.xyz = lerp (scaleAndSpinProp.xyz, _ScaleTestValue.xyz, _ScaleTestToggle);

    v.vertex.xyz *= scaleAndSpinProp.xyz;
    v.normal.xyz *= scaleAndSpinProp.xyz;
    v.tangent.xyz *= scaleAndSpinProp.xyz;

    // o.localPos = v.vertex.xyz;
    // o.localNormal = v.normal.xyz;
    o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
    o.worldNormal1 = UnityObjectToWorldNormal (v.normal);
}

float3 GetAlbedo (float3 hsbOffsetPrimaryXYZ, float3 hsbOffsetSecondaryXYZ, float3 albedoBase, float albedoMask, float occlusionFromTexture)
{
	// Separate AH texture alpha into two tinting masks
	float albedoMaskMinToMed = saturate (albedoMask * 2);
	float albedoMaskMedToMax = saturate (albedoMask - 0.5) * 2;

	float3 albedoFinal = RGBTweakHSV
	(
		albedoBase,
		albedoMaskMinToMed,
		albedoMaskMedToMax,
		hsbOffsetPrimaryXYZ.x,
		hsbOffsetPrimaryXYZ.y,
		hsbOffsetPrimaryXYZ.z,
		hsbOffsetSecondaryXYZ.x,
		hsbOffsetSecondaryXYZ.y,
		hsbOffsetSecondaryXYZ.z,
		occlusionFromTexture
	);

	return albedoFinal;
	/*
	// Convert albedo into HSV format
	float3 albedoBaseHSV = RGBToHSV (albedoBase);

	// We push the hue (first HSV component) around using first offset component and wrap the result using frac operation
	albedoBaseHSV.x = frac (albedoBaseHSV.x + lerp (0, IN.hsbOffsetsPrimary.x, albedoMaskMedToMax) + lerp (IN.hsbOffsetsSecondary.x, 0, albedoMaskMinToMed));

	// Secondary and tertiary components (X, Z) storing saturation and brightness changes are remapped from [0,1] to [-1,1] range before being applied
	// Saturation and brightness are 0-1 values not supposed to be wrapped, so after modifying them, we clamp them
	albedoBaseHSV.y = saturate (albedoBaseHSV.y + lerp (0, (IN.hsbOffsetsPrimary.y - 0.5) * 2, albedoMaskMedToMax) + lerp ((IN.hsbOffsetsSecondary.y - 0.5) * 2, 0, albedoMaskMinToMed));
	albedoBaseHSV.z = saturate (albedoBaseHSV.z + lerp (0, (IN.hsbOffsetsPrimary.z - 0.5) * 2, albedoMaskMedToMax) + lerp ((IN.hsbOffsetsSecondary.z - 0.5) * 2, 0, albedoMaskMinToMed));

	// Calculating final albedo involves mixing base albedo with RGB conversion result of our shifted HSV result (not sure what occlusion is doing here)
	// Interpolation factor is a value that turns to 0 at 0.5 of the original hue mask channel and to 1 at it's extremes
	return saturate
	(
		lerp
		(
			albedoBase,
			HSVToRGB (albedoBaseHSV) * saturate (occlusionFromTexture * 2),
			1 - (1 - albedoMaskMedToMax) * albedoMaskMinToMed
		)
	);*/
}

float GetSmoothness (float mask, float min, float med, float max, float backsideFactor)
{
	float smoothnessMaskMinToMed = saturate (mask * 2);
	float smoothnessMaskMedToMax = saturate (mask - 0.5) * 2;
	return lerp (lerp (min, med, smoothnessMaskMinToMed), max, smoothnessMaskMedToMax) * (1 - backsideFactor);
}

float3 GetEmission (float damageCriticalityX, float hsbOffsetsSecondaryW, float mask, float3 color, float intensity)
{
	// Emission is fairly straightforward - just keep an eye out for 4th channel of primary offset property (it can be used to drop emission to 0)
	// Other factors which can drop emission to 0 are damage intensity and backside multiplier
	float3 emissionHSV = RGBToHSV (mask * color);
	emissionHSV.x = frac (emissionHSV.x + hsbOffsetsSecondaryW);
	return HSVToRGB (emissionHSV) * intensity * (lerp (saturate (hsbOffsetsSecondaryW), 0, saturate (damageCriticalityX * 10)));
}

float GetOcclusion (float worldPos1Y, float worldNormal1Y, float occlusionFromTexture)
{
	float occlusionHeightFactor = saturate ((worldPos1Y + _GlobalEnvironmentAmbientSettings.y) / _GlobalEnvironmentAmbientSettings.x);  // saturate ((IN.worldPos1.y * _GlobalEnvironmentAmbientSettings.x + _GlobalEnvironmentAmbientSettings.y) / _GlobalEnvironmentAmbientSettings.x);
    float occlusionHeightGraded = lerp (0.25, 1, occlusionHeightFactor);
	return occlusionFromTexture * lerp (1, occlusionHeightGraded, _GlobalEnvironmentAmbientSettings.z * saturate (1 - worldNormal1Y));
    // return occlusionFromTexture;
}

float4 GetDetailSample (float3 worldPos1, float3 worldNormal1)
{
    // Triplanar projection of shared detail texture - should ideally be replaced by a single 3d noise sample
	float3 projNormal = saturate (pow (worldNormal1 * 1.4, 4));
	float4 detailX = tex2D (_GlobalDetailTex, worldPos1.zy / _GlobalEnvironmentDetailScale + float2 (0.25, 0.125)) * abs (worldNormal1.x);
	float4 detailY = tex2D (_GlobalDetailTex, worldPos1.zx / _GlobalEnvironmentDetailScale + float2 (-0.25, -0.125)) * abs (worldNormal1.y);
	float4 detailZ = tex2D (_GlobalDetailTex, worldPos1.xy / _GlobalEnvironmentDetailScale + float2 (0.35, 0.35)) * abs (worldNormal1.z);

	// Combining projections in a single sample weighted by normal components
	float4 detail = detailZ;
	detail = lerp (detail, detailX, projNormal.x);
	detail = lerp (detail, detailY, projNormal.y);
	return detail;

    // float4 detail = tex2D (_GlobalDetailTex, IN.triplanarUV);
    // return detail;
}

float GetIntegrityMultiplier (float integrity, float4 detail)
{
	float integrityMask = saturate (saturate (((detail.z - 0.5) * max (4, 0)) + 0.5) + pow (integrity, 4) - (1 - pow (integrity, 4)));
	return lerp (detail.x * saturate (detail.w * 2), 1, integrityMask);
}

#if defined(SHADER_API_D3D11)
//stochastic sampling
float4 tex2DStochastic(ARGS_TEX2D_SAMPLER(tex, samplertex), float2 uv)
{
	//triangle vertices and blend weights
	//BW_vx[0...2].xyz = triangle verts
	//BW_vx[3].xy = blend weights (z is unused)
	float4x3 BW_vx;

	//uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
	float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), uv * 3.464);

	//vertex IDs and barycentric coords
	float2 vxID = float2 (floor(skewUV));
	float3 barry = float3 (frac(skewUV), 0);
	barry.z = 1.0-barry.x-barry.y;

	BW_vx = ((barry.z>0) ?
		float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
		float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0-barry.y, 1.0-barry.x)));

	//calculate derivatives to avoid triangular grid artifacts
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	// float4 sample1 = tex2D(tex, uv + hash2D2D(BW_vx[0].xy), dx, dy);
	// float4 sample2 = tex2D(tex, uv + hash2D2D(BW_vx[1].xy), dx, dy);
	// float4 sample3 = tex2D(tex, uv + hash2D2D(BW_vx[2].xy), dx, dy);

	float4 sample1 = SAMPLE_TEX2D_GRAD(tex, samplertex, uv + hash2D2D(BW_vx[0].xy), dx, dy);
	float4 sample2 = SAMPLE_TEX2D_GRAD(tex, samplertex, uv + hash2D2D(BW_vx[1].xy), dx, dy);
	float4 sample3 = SAMPLE_TEX2D_GRAD(tex, samplertex, uv + hash2D2D(BW_vx[2].xy), dx, dy);

	float4 result = mul (sample1, BW_vx[3].x) + mul (sample2, BW_vx[3].y) + mul (sample3, BW_vx[3].z);
	return result;
}
#endif

/*
void SampleTerrainFallbackSimple (inout float4 ah, inout float4 mseo, float3 worldPos)
{
	ah = float4 (1,1,1,0);
	mseo = float4 (0,0,0,1);

	float2 uvMain = worldPos.xz / _CombatTerrainParams.y;
	float4 result = tex2D (_CombatTerrainTexFallbackAS, uvMain);

	ah = result;
}
*/

/*
void SampleTerrainFallbackStohastic (inout float4 ah, inout float4 mseo, float3 worldPos)
{
	ah = float4 (1,1,1,0);
	mseo = float4 (0,0,0,1);

	float2 uvMain = worldPos.xz / _CombatTerrainParams.y;

	//triangle vertices and blend weights
	//BW_vx[0...2].xyz = triangle verts
	//BW_vx[3].xy = blend weights (z is unused)
	float4x3 BW_vx;

	//uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
	float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), uvMain * 3.464);

	//vertex IDs and barycentric coords
	float2 vxID = float2 (floor(skewUV));
	float3 barry = float3 (frac(skewUV), 0);
	barry.z = 1.0-barry.x-barry.y;

	BW_vx = ((barry.z>0) ?
		float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
		float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0-barry.y, 1.0-barry.x)));

	//calculate derivatives to avoid triangular grid artifacts
	float2 dx = ddx(uvMain);
	float2 dy = ddy(uvMain);

	float4 sample1 = tex2D(_CombatTerrainTexFallbackAS, uvMain + hash2D2D(BW_vx[0].xy), dx, dy);
	float4 sample2 = tex2D(_CombatTerrainTexFallbackAS, uvMain + hash2D2D(BW_vx[1].xy), dx, dy);
	float4 sample3 = tex2D(_CombatTerrainTexFallbackAS, uvMain + hash2D2D(BW_vx[2].xy), dx, dy);

	// float4 sample1 = SAMPLE_TEX2D_GRAD(tex, samplertex, uvMain + hash2D2D(BW_vx[0].xy), dx, dy);
	// float4 sample2 = SAMPLE_TEX2D_GRAD(tex, samplertex, uvMain + hash2D2D(BW_vx[1].xy), dx, dy);
	// float4 sample3 = SAMPLE_TEX2D_GRAD(tex, samplertex, uvMain + hash2D2D(BW_vx[2].xy), dx, dy);

	float4 result = mul (sample1, BW_vx[3].x) + mul (sample2, BW_vx[3].y) + mul (sample3, BW_vx[3].z);

	ah = result;
}
*/

float RemapTo01 (float f, float a1, float a2)
{
	float divisor = a2 - a1;
	if (divisor < 0.001)
		return f;

	return (f - a1) / divisor;
}

half BH1 (half texture1height,  half texture2height,  half control1height,  half control2height,  half overlapDepth,  out half textureHeightOut,  out half controlHeightOut)
{
	half texture1heightPrefilter = texture1height * sign(control1height);
	half texture2heightPrefilter = texture2height * sign(control2height);
	half height1 = texture1heightPrefilter + control1height;
	half height2 = texture2heightPrefilter + control2height;
	half blendFactor = (clamp(((height1 - height2) / overlapDepth), -1, 1) + 1) / 2;
	// Subtract positive differences of the other control height to not make one texture height benefit too much from the other.
	textureHeightOut = max(0, texture1heightPrefilter - max(0, control2height-control1height)) * blendFactor + max(0, texture2heightPrefilter - max(0, control1height-control2height)) * (1 - blendFactor);
	// Propagate sum of control heights to not loose height.
	controlHeightOut = control1height + control2height;
	return blendFactor;
}

void SampleTerrain (inout float4 as, inout float4 mseo, inout float4 nh, float3 worldPos)
{
	as = float4 (1,1,1,0); // tex2DStochastic (_GlobalBackgroundGrassTex, worldPos.xz / 30);
	mseo = float4 (0,0,0,1);
	nh = float4 (0,0,1,0.5);

	#if defined(SHADER_API_D3D11) || defined(SHADER_API_VULKAN)

	float gradientSize = _CombatTerrainParamsScale.w - _CombatTerrainParamsScale.z;
	float gradientUVBase = saturate ((worldPos.y - _CombatTerrainParamsScale.z) / max (0.01, gradientSize));

	float2 gradientUV = float2 (gradientUVBase, 0);
	float4 gradientSample = UNITY_SAMPLE_TEX2D(_CombatTerrainTexGradient, gradientUV);

	float2 splatUV = float2 (worldPos.x, -worldPos.z) / _CombatTerrainParamsScale.x;
	float4 splatSample = UNITY_SAMPLE_TEX2D(_CombatTerrainTexSplat, splatUV);

	float2 mainUV = float2 (worldPos.x, -worldPos.z) / _CombatTerrainParamsScale.y;

	/*
	// Uncomment if blending only requires RGB and alpha channel of splat map contains breakup map.
	// If breakup map is above a certain threshold:
	if (splatSample.w > 1)
	{
		// Offset UV by half tile on both axes
		mainUV += float2 (0.3256, 0.6127);

		// Rotate UV by 90
		float angle = 1.5708;
		mainUV = float2 (mainUV.x * cos (angle) - mainUV.y * sin(angle), mainUV.y * cos (angle) - mainUV.x * sin(angle));
	}
	*/

	float4 as1 = float4 (0, 0, 0, 0);
	float4 nh1 = float4 (0, 0, 1, 0.5);

	float4 as2 = float4 (0, 0, 0, 0);
	float4 nh2 = float4 (0, 0, 1, 0.5);

	float4 as3 = float4 (0, 0, 0, 0);
	float4 nh3 = float4 (0, 0, 1, 0.5);

	float4 as4 = float4 (0, 0, 0, 0);
	float4 nh4 = float4 (0, 0, 1, 0.5);

	as1 = UNITY_SAMPLE_TEX2D (_CombatTerrainTexDetail1AH, mainUV);
	nh1 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail1NH, _CombatTerrainTexDetail1AH, mainUV);

	float4 hsb1 = _CombatTerrainDetail1HSB;
	as1.xyz = lerp (as1.xyz, RGBAdjustWithHSV (as1, hsb1.x, hsb1.y, hsb1.z), hsb1.w);

	// Offset main UV for detail 2, in case it has same texture
	mainUV += float2 (0.25, 0.25);

	as2 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail2AH, _CombatTerrainTexDetail1AH, mainUV);
	nh2 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail2NH, _CombatTerrainTexDetail1AH, mainUV);

	float4 hsb2 = _CombatTerrainDetail2HSB;
	as2.xyz = lerp (as2.xyz, RGBAdjustWithHSV (as2, hsb2.x, hsb2.y, hsb2.z), hsb2.w);

	// Offset main UV for detail 3, in case it has same texture
	mainUV += float2 (0.25, 0.25);

	as3 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail3AH, _CombatTerrainTexDetail1AH, mainUV);
	nh3 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail3NH, _CombatTerrainTexDetail1AH, mainUV);

	float4 hsb3 = _CombatTerrainDetail3HSB;
	as3.xyz = lerp (as3.xyz, RGBAdjustWithHSV (as3, hsb3.x, hsb3.y, hsb3.z), hsb3.w);

	// Offset main UV for detail 4, in case it has same texture
	mainUV += float2 (0.25, 0.25);

	as4 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail4AH, _CombatTerrainTexDetail1AH, mainUV);
	nh4 = UNITY_SAMPLE_TEX2D_SAMPLER (_CombatTerrainTexDetail4NH, _CombatTerrainTexDetail1AH, mainUV);

	float4 hsb4 = _CombatTerrainDetail4HSB;
	as4.xyz = lerp (as4.xyz, RGBAdjustWithHSV (as4, hsb4.x, hsb4.y, hsb4.z), hsb4.w);


	/*
	float lambda = 0.0001;

	//if(splatSample.x >= lambda)
	{
		as1 = tex2DStochastic (PASS_TEX2D (_CombatTerrainTexDetail1AH), mainUV);
		//as1 = float4(1,0,0,1);
		nh1 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail1NH, _CombatTerrainTexDetail1AH), mainUV);

		float4 hsb1 = _CombatTerrainDetail1HSB;
		as1.xyz = lerp (as1.xyz, RGBAdjustWithHSV (as1, hsb1.x, hsb1.y, hsb1.z), hsb1.w);
	}

	//if(splatSample.x >= lambda)
	{
		as2 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail2AH, _CombatTerrainTexDetail1AH), mainUV);
		nh2 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail2NH, _CombatTerrainTexDetail1AH), mainUV);

		float4 hsb2 = _CombatTerrainDetail2HSB;
		as2.xyz = lerp (as2.xyz, RGBAdjustWithHSV (as2, hsb2.x, hsb2.y, hsb2.z), hsb2.w);
	}

	//if(splatSample.x >= lambda)
	{
		as3 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail3AH, _CombatTerrainTexDetail1AH), mainUV);
		nh3 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail3NH, _CombatTerrainTexDetail1AH), mainUV);

		float4 hsb3 = _CombatTerrainDetail3HSB;
		as3.xyz = lerp (as3.xyz, RGBAdjustWithHSV (as3, hsb3.x, hsb3.y, hsb3.z), hsb3.w);
	}

	//if(splatSample.x >= lambda)
	{
		as4 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail4AH, _CombatTerrainTexDetail1AH), mainUV);
		nh4 = tex2DStochastic (PASS_TEX2D_SAMPLER (_CombatTerrainTexDetail4NH, _CombatTerrainTexDetail1AH), mainUV);

		float4 hsb4 = _CombatTerrainDetail4HSB;
		as4.xyz = lerp (as4.xyz, RGBAdjustWithHSV (as4, hsb4.x, hsb4.y, hsb4.z), hsb4.w);
	}

	float weight1 = splatSample.x;
	float weight2 = splatSample.y;
	float weight3 = splatSample.z;


	// final height per detail type (offset this later, if needed, e.g. with a float4 property that shifts each channel up or down)
	half h1o = saturate (nh1.w + _CombatTerrainParamsBlendOffset.x);
	half h2o = saturate (nh2.w + _CombatTerrainParamsBlendOffset.y);
	half h3o = saturate (nh3.w + _CombatTerrainParamsBlendOffset.z);
	half h4o = saturate (nh4.w + _CombatTerrainParamsBlendOffset.w);

	// swap 0.5 to contrast value from a float4 property, if necessary (making transition to each channel harsher or softer)
	half b1 = HeightBlend (h1o, h2o, weight1, _CombatTerrainParamsBlendContrast.x);
	fixed h1 = lerp (h1o, h2o, b1);
	half b2 = HeightBlend (h1, h3o, weight2, _CombatTerrainParamsBlendContrast.y);
	fixed h2 = lerp (h1, h2o, b1);
	half b3 = HeightBlend (h2, h4o, weight3, _CombatTerrainParamsBlendContrast.z);


	float4 combinedAlbedoSmoothness = lerp (lerp (lerp (as1, as2, b1), as3, b2), as4, b3);
	float4 combinedNormalHeight = lerp (lerp (lerp (nh1, nh2, b1), nh3, b2), nh4, b3);
	*/

	/*
	float height1 = saturate (splatSample.x);
	float height2 = saturate (splatSample.y);
	float height3 = saturate (splatSample.z);

	half h1o = saturate (nh1.w + _CombatTerrainParamsBlendOffset.x);
	half h2o = saturate (nh2.w + _CombatTerrainParamsBlendOffset.y);
	half h3o = saturate (nh3.w + _CombatTerrainParamsBlendOffset.z);
	half h4o = saturate (nh4.w + _CombatTerrainParamsBlendOffset.w);

	half b1 = HeightBlend (h1o, h2o, height1, _CombatTerrainParamsBlendContrast.x);
	half h1 = lerp (h1o, h2o, b1);
	half b2 = HeightBlend (h1, h3o, height2, _CombatTerrainParamsBlendContrast.y);
	half h2 = lerp (h1, h2o, b1);
	half b3 = HeightBlend (h2, h4o, height3, _CombatTerrainParamsBlendContrast.z);

	float4 combinedAlbedoSmoothness = lerp (lerp (lerp (as1, as2, b1), as3, b2), as4, b3);
	float4 combinedNormalHeight = lerp (lerp (lerp (nh1, nh2, b1), nh3, b2), nh4, b3);
	*/

	half h1 = saturate (nh1.w + (_CombatTerrainParamsBlendOffset.x - 0.5) * 2);
	half h2 = saturate (nh2.w + (_CombatTerrainParamsBlendOffset.y - 0.5) * 2);
	half h3 = saturate (nh3.w + (_CombatTerrainParamsBlendOffset.z - 0.5) * 2);
	half h4 = saturate (nh4.w + (_CombatTerrainParamsBlendOffset.w - 0.5) * 2);

	float splatR = saturate (splatSample.x);
	float splatG = saturate (splatSample.y);
	float splatB = saturate (splatSample.z);
	float overlapDepth = 0.25;

	half textHeight1, textHeight2, textHeight3;
	half ctrlHeight1, ctrlHeight2, ctrlHeight3;
	half blendFactor01 = BH1 (h1, h2, splatR, splatG, overlapDepth, textHeight1, ctrlHeight1);
	half blendFactor12 = BH1 (textHeight1, h3, ctrlHeight1, splatB, overlapDepth, textHeight2, ctrlHeight2);
	half blendFactor23 = BH1 (textHeight2, h4, ctrlHeight2, splatSample.w, overlapDepth, textHeight3, ctrlHeight3);

	float4 combinedAlbedoSmoothness = float4 (0, 0, 0, 0);
	combinedAlbedoSmoothness = as1 * blendFactor01 + as2 * (1 - blendFactor01);
	combinedAlbedoSmoothness = combinedAlbedoSmoothness * blendFactor12 + as3 * (1 - blendFactor12);
	combinedAlbedoSmoothness = combinedAlbedoSmoothness * blendFactor23 + as4 * (1 - blendFactor23);

	float4 combinedNormalHeight = float4 (0, 0, 0, 0);
	combinedNormalHeight = nh1 * blendFactor01 + nh2 * (1 - blendFactor01);
	combinedNormalHeight = combinedNormalHeight * blendFactor12 + nh3 * (1 - blendFactor12);
	combinedNormalHeight = combinedNormalHeight * blendFactor23 + nh4 * (1 - blendFactor23);


	as = float4 (combinedAlbedoSmoothness.x, combinedAlbedoSmoothness.y, combinedAlbedoSmoothness.z, 0);
	as.xyz = lerp (as.xyz, as.xyz * gradientSample.xyz, gradientSample.w);

	// Allowing textures to use full range while ensuring smoothness never peaks too high for what terrain depicts
	float smoothness = combinedAlbedoSmoothness.w * 0.25;

	mseo = float4 (0, smoothness, 0, 1);
	nh = combinedNormalHeight;
	#endif
}

void ApplyIsolines (inout float3 albedoFinal, inout float3 emissionFinal, float3 worldPos, float3 worldNormal)
{
	// Constants for now, no point exposing them atm
	float lineWidth = 0.25;
	float heightOffset = 0;
	float4 isolineColor1 = _GlobalIsolineColor;

	// Ensure isolines don't become thicker on more horizontal slopes:
	// the factor they need to be multiplied by is exactly proportional to this length
	lineWidth *= length (cross (worldNormal, float3 (0, 1, 0)));

	// Height interval is 1 atm so I can simplify the factor calculation by cutting a few divisions out,
	// but I'm keeping the full version just in case:
	// float heightInterval1 = 1;
	// float a1 = frac ((IN.worldPos1.y + heightOffset) / heightInterval1) - 1 + lineWidth * isolineThicknessMultiplier / heightInterval1;
	float a1 = frac ((worldPos.y + heightOffset)) - 1 + lineWidth;

	// To avoid a conditional to isolate the isoline pixels, we can just multiply the factor and saturate it
	float isolineMask = saturate (a1 * 100);

	// Emission gets added to if isoline factor is above 0 and it's night
	emissionFinal = lerp (emissionFinal, emissionFinal + isolineColor1.xyz * isolineColor1.w, isolineMask * TOD_NightTimeSwitch);

	// Albedo gets subtracted from (more strongly than emission addition due to gamma blending) if isoline factor is above 0 and it's day
	albedoFinal = lerp (albedoFinal, albedoFinal * (1 - saturate (isolineColor1.w * 4)), isolineMask * (1 - TOD_NightTimeSwitch));
}

// Declaring these parameters here, so we don't have to copy-paste them across all shaders using weather effects
float _WeatherMultiplier;
float _WeatherOcclusionIntensity;
float _WeatherOcclusionMaskPower;

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

// Lightweight version of the same function for background objects (no texture sampling etc.)
void ApplyWeatherLightweight
(
	float weatherEffectsIntensity,
	inout float3 albedoFinal,
	inout float smoothnessFinal,
	inout float metalnessFinal,
	float3 worldPos,
	float verticalFactor,
	float occlusionMask = 1.0f
)
{
	#if defined(SHADER_API_D3D11)
	float distanceFadeRain = 1 - saturate ((distance (_WorldSpaceCameraPos, worldPos) / 200) - 0.1);
	float rainIntensity = saturate (_WeatherParameters.x);
	float snowSurfaceIntensity = saturate (_WeatherParameters.y);

	// ======================================================================= RAIN
	// Snow should mask out rain on all surfaces
	rainIntensity = lerp (rainIntensity, 0, snowSurfaceIntensity);
	float rainMask = rainIntensity * verticalFactor * occlusionMask;
	// Enable or disable weather effects if desired
	rainMask *= weatherEffectsIntensity;

	// Wetness pushes albedo to water color, but up to a limit, as "depth" isn't that deep
	float3 rainColor = float3 (0.05, 0.065, 0.08);
	albedoFinal.xyz = lerp (albedoFinal.xyz, rainColor, rainMask * 0.66);

	float metalnessRain = rainIntensity * 0.8;
	float smoothnessRain = 0.1;

	// Wetness pushes smoothness and metalness up, but up to a limit, to avoid shrinking highlights too much
	metalnessFinal = lerp (metalnessFinal, metalnessRain, rainMask);
	smoothnessFinal = lerp (smoothnessFinal, smoothnessFinal + smoothnessRain, rainMask);

	// ======================================================================= SNOW
	// Base snow mask - fade by slope
	float snowMask = snowSurfaceIntensity * verticalFactor * occlusionMask;
	// Enable or disable weather effects if desired
	snowMask *= weatherEffectsIntensity;

	// Wetness pushes albedo to water color, but up to a limit, as "depth" isn't that deep
	float3 snowColor = float3 (0.5, 0.5, 0.55);
	albedoFinal.xyz = lerp (albedoFinal.xyz, snowColor, snowMask);

	// Wetness pushes smoothness and metalness up, but up to a limit, to avoid shrinking highlights too much
	metalnessFinal = lerp (metalnessFinal, 0, snowMask);
	smoothnessFinal = lerp (smoothnessFinal, 0.2, snowMask);
	#endif
}
