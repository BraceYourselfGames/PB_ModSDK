float _GlobalPlayMode;
//float4 _GlobalSimulationTime;

// Matrix-based RGB color adjustment using hue shift
// Expects normalized degree range of 0-1, saturation multiplier and value multiplier (both expect default to be at 0.5)
// Source: https://beesbuzz.biz/code/16-hsv-color-transforms
float3 RGBAdjustWithHSV(float3 _in, float _h, float _s, float _v)
{
	// Adjust parameters to make them work with shader inputs
	_h *= -6.28;
	_s *= 2;
	_v *= 2;
	float vsu = _v * (_s) * cos (_h);
	float vsw = _v * (_s) * sin (_h);

	float3 Out;
	Out.r = (0.299*_v + 0.701*vsu + 0.168*vsw) * _in.r + (0.587*_v - 0.587*vsu + 0.330*vsw) * _in.g + (0.114*_v - 0.114*vsu - 0.497*vsw) * _in.b;
	Out.g = (0.299*_v - 0.299*vsu - 0.328*vsw) * _in.r + (0.587*_v + 0.413*vsu + 0.035*vsw) * _in.g + (0.114*_v - 0.114*vsu + 0.292*vsw) * _in.b;
	Out.b = (0.299*_v - 0.300*vsu + 1.250*vsw) * _in.r + (0.587*_v - 0.588*vsu - 1.050*vsw) * _in.g + (0.114*_v + 0.886*vsu - 0.203*vsw) * _in.b;

	return Out;
}

// A wrapper function to blend HSV offsets based on albedo A mask
float3 RGBTweakHSV(float3 _in, float _maskMinToMed, float _maskMedtoMax, float _hPrimary, float _sPrimary, float _vPrimary, float _hSecondary, float _sSecondary, float _vSecondary, float _occlusionFromTexture)
{
	float3 albedoBaseHSV;
	float3 albedoFinal;

	albedoBaseHSV = RGBAdjustWithHSV
	(
		_in,
		(_hPrimary * _maskMedtoMax) + (_hSecondary * (1 - _maskMinToMed)),
		(_sPrimary * _maskMedtoMax) + (_sSecondary * (1 - _maskMinToMed)),
		(_vPrimary * _maskMedtoMax) + (_vSecondary * (1 - _maskMinToMed))
	);

	albedoFinal = saturate (lerp (_in, albedoBaseHSV * saturate (_occlusionFromTexture * 2), 1 - (1 - _maskMedtoMax) * _maskMinToMed));

	return albedoFinal;
}

static const float4 k_to_hsv = float4 (0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
static const float e = 1.0e-10;
static const float4 k_to_rgb = float4 (1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
static const float3 xFrac = float3 (0.0, -1.0 / 3.0, 1.0 / 3.0);

// Convert RGB to HSV color space (old method)
inline float3 RGBToHSV (float3 _in)
{
	float4 p = lerp (float4 (_in.zy, k_to_hsv.wz), float4 (_in.yz, k_to_hsv.xy), step (_in.z, _in.y));
	float4 q = lerp (float4 (p.xyw, _in.x), float4 (_in.x, p.yzx), step (p.x, _in.x));
	float d = q.x - min (q.w, q.y);
	return float3 (abs (q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// Convert HSV to RGB color space (old method)
inline float3 HSVToRGB (float3 _in)
{
	return float3 (lerp (float3 (1, 1, 1), saturate (3.0 * abs (1.0 - 2.0 * frac (_in.x + xFrac)) - 1), _in.y) * _in.z);
}

// A wrapper function to blend HSV offsets based on albedo A mask - USES OLD METHOD (currently only used on all area props)
float3 RGBTweakHSVOld(float3 _in, float _maskMinToMed, float _maskMedtoMax, float _hPrimary, float _sPrimary, float _vPrimary, float _hSecondary, float _sSecondary, float _vSecondary, float _occlusionFromTexture)
{
	float3 albedoBaseHSV;
	float3 albedoFinal;

	albedoBaseHSV = RGBToHSV (_in);
	albedoBaseHSV.x = frac ( albedoBaseHSV.x + (_hPrimary * _maskMedtoMax) + (_hSecondary * (1 - _maskMinToMed)) );
	albedoBaseHSV.y = saturate ( albedoBaseHSV.y + (((_sPrimary - 0.5) * 2) * _maskMedtoMax) + (((_sSecondary - 0.5) * 2) * (1 - _maskMinToMed)) );
	albedoBaseHSV.z = saturate ( albedoBaseHSV.z + (((_vPrimary - 0.5) * 2) * _maskMedtoMax) + (((_vSecondary - 0.5) * 2) * (1 - _maskMinToMed)) );

	albedoFinal = saturate (lerp (_in, HSVToRGB (albedoBaseHSV) * saturate (_occlusionFromTexture * 2), 1 - (1 - _maskMedtoMax) * _maskMinToMed));

	return albedoFinal;
}

// Convert RGB to grayscale
inline float RGBToGrayscale (float3 _in)
{
	return saturate ((_in.x + _in.y + _in.z) / 3);
}

// Control RGB saturation
// Taken from ShaderGraph documentation
inline float3 RGBSaturation (float3 _in, float Saturation)
{
	float luma = dot (_in, float3(0.2126729, 0.7151522, 0.0721750));
	return float3 ( luma.xxx + Saturation.xxx * (_in - luma.xxx) );
}

// Contrast for grayscale textures/values
float ContrastGrayscale (float color, float contrast)
{
	float contrastLo = -contrast;
	float contrastHi = contrast + 1;
	return saturate (lerp (contrastLo, contrastHi, color));
}

// Contrast for grayscale textures/values
float ContrastGrayscaleMid (float color, float contrast)
{
	contrast *= 0.5;
	float contrastLo = 0.5 - contrast;
	float contrastHi = 0.5 + contrast;
	return saturate (lerp (contrastLo, contrastHi, color));
}

// Overlay blend
inline float3 Overlay (float3 src, float3 dst)
{
    return lerp (1 - 2 * (1 - src) * (1 - dst), 2 * src * dst, step (src, 0.5));
}

inline float OverlayGrayscale (float src, float dst)
{
	return lerp (1 - 2 * (1 - src) * (1 - dst), 2 * src * dst, step (src, 0.5));
}

// Overlay blend
inline float3 ScreenMultiply (float3 base, float3 tint)
{
	float r = lerp (base.x * tint.x, saturate (base.x + base.x * tint.x), saturate ((tint.x - 0.5) * 10 + 0.5));
	float g = lerp (base.y * tint.y, saturate (base.y + base.y * tint.y), saturate ((tint.y - 0.5) * 10 + 0.5));
	float b = lerp (base.z * tint.z, saturate (base.z + base.z * tint.z), saturate ((tint.z - 0.5) * 10 + 0.5));
	return float3 (r, g, b);
}

// Overlay blend with alpha
inline float3 OverlayWithAlpha (float3 top, float alpha, float3 back)
{
    top.xyz = float3 (top.x - 0.5, top.y - 0.5, top.z - 0.5);
    top.xyz *= saturate (alpha);
    top.xyz = float3 (top.x + 0.5, top.y + 0.5, top.z + 0.5);
    return lerp (1 - 2 * (1 - top) * (1 - back), 2 * top * back, step (top, 0.5));
}

// Alpha blend
inline float3 AlphaBlend (float3 top, float alpha, float3 back)
{
    return alpha * top + (1 - alpha) * back;
}

static const float4x4 thresholdMatrix =
{
	1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
	13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
	4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
	16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
};

static const float4x4 rowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };

// Get face's backside factor against viewing direction (faces looking away from camera = 1, faces looking at camera = 0)
inline float GetBacksideFactor (float3 viewDir)
{
	return dot (viewDir, float3 (0, 0, 1)) > 0 ? 0 : 1;
}

// Creates a sphere mask originating from input Center. The sphere is calculated using Distance and modified using the Radius and Hardness inputs.
// Taken from ShaderGraph documentation
float3 SphereMask (float3 Coords, float3 Center, float Radius, float Hardness)
{
	float3 Out = 1 - saturate ((distance (Coords, Center) - Radius) / (1 - Hardness));
	return Out;
}

float2 SphereMaskFloat2 (float2 Coords, float2 Center, float Radius, float Hardness)
{
	float2 Out = 1 - saturate ((distance (Coords, Center) - Radius) / (1 - Hardness));
	return Out;
}

float SphereMaskFloat (float Coords, float Center, float Radius, float Hardness)
{
	float Out = 1 - saturate ((distance (Coords, Center) - Radius) / (1 - Hardness));
	return Out;
}

// Rotate vertices around Y axis (angle in degrees)
float4 RotateAroundYInDegrees (float4 vertex, float degrees)
{
    float alpha = degrees * UNITY_PI / 180.0;
    float sina, cosa;
    sincos (alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return float4(mul (m, vertex.xz), vertex.yw).xzyw;
}

// Rotate vetrices around Z axis (angle in degrees)
float3 RotateAroundZInDegrees (float3 vertex, float degrees)
{
	float alpha = degrees * UNITY_PI / 180.0;
	float sina, cosa;
	sincos (alpha, sina, cosa);
	float2x2 m = float2x2(cosa, -sina, sina, cosa);
	return float3(mul (m, vertex.xy), vertex.z).zxy;
}

// Rotate vertices around an arbitrary axis (angle in radians)
float3 RotateAboutAxis_Radians (float3 In, float3 Axis, float Rotation)
{
	float s = sin (Rotation);
	float c = cos (Rotation);
	float one_minus_c = 1.0 - c;

	Axis = normalize (Axis);
	float3x3 rot_mat = 
	{   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
		one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
		one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
	};
	return mul (rot_mat,  In);
}

// Construct a rotation matrix around an arbitrary axis
float3x3 AngleAxis3x3 (float angle, float3 axis)
{
	float c, s;
	sincos (angle, s, c);

	float t = 1 - c;
	float x = axis.x;
	float y = axis.y;
	float z = axis.z;

	return float3x3
	(
		t * x * x + c, t * x * y - s * z, t * x * z + s * y,
		t * x * y + s * z, t * y * y + c, t * y * z - s * x,
		t * x * z - s * y, t * y * z + s * x, t * z * z + c
	);
}

// Given two height values (from textures) and a height value for the current pixel (from vertex)
// Compute the blend factor between the two with a small blending area between them.
half HeightBlend (half h1, half h2, half slope, half contrast)
{
	h2 = 1 - h2;
	half tween = saturate ((slope - min (h1, h2)) / max (abs (h1 - h2), 0.001));
	half threshold = contrast;
	half width = 1.0 - contrast;
	return saturate ((tween - threshold) / max (width, 0.001));
}

float3 NormalBlend (float3 firstNormal, float3 secondNormal)
{
    return normalize (float3 (firstNormal.rg + secondNormal.rg, firstNormal.b * secondNormal.b));
}

inline float2 ParallaxOffsetCustom (half h, half height, float3 viewDir)
{
	h = h * height - height / 2.0;
	// viewDir = mul (unity_ObjectToWorld, float4 (viewDir.x, viewDir.y, viewDir.z, 1));
	float3 v = normalize (viewDir);
	v.z += 0.42;
	return h * (v.xy / v.z);
}

// Reconstruct pixel world position behind a translucent surface using scene depth
float3 RestorePixelWorldPosBehindTranslucency (float3 pixelWorldPos, fixed sceneDepth, fixed pixelDepth)
{
	fixed a = sceneDepth / pixelDepth;
	float3 b = pixelWorldPos - _WorldSpaceCameraPos;
	return a * b + _WorldSpaceCameraPos;
}

// Sawtooth wave
float sawtooth (float x)
{
	return (x - floor (x + 0.5));
}

// Linear wave
float lin (float x)
{
	float a = frac (x) * 2;
	float b = (1 - frac (x)) * 2;
	return lerp (a, b, floor (a));
}

// Partially inspired by a Twitter thread by Ben Golus
// https://twitter.com/bgolus/status/1517231675385675776
float4 FlipbookAnimBlend (sampler2D tex, float2 uv, float speed, float columns, float rows)
{
	/*float time = 0.0;
	// Fallback case to use separate time variable during gameplay
	if (_GlobalPlayMode < 0.1)
	{
		time = _Time.x;
	}
	else
	{
		time = _GlobalSimulationTime.x;
	}*/
	float time = _Time.x;

	int totalFrames = rows * columns;
	// Frame animation progression, from 0 to totalFrames (0 to 36)
	float frameAnim = frac (time * speed) * max (0.0, totalFrames - 1.0);

	float halfFrame = frameAnim * 0.5;
	float evenFrame = floor (halfFrame) * 2.0 + 1;
	float oddFrame = floor (halfFrame + 0.5) * 2.0;

	// Calculate how 'far' we need to move the UVs to progress to the next column or row (in 0-1 UV space)
	float2 frameStep = float2(1 / (float)columns, 1 / (float)rows);
	// UVs scaled to the size of one frame in the flipbook
	float2 frameUVs = float2 (uv.x * frameStep.x, uv.y * frameStep.y);
	// Calculate rowIndex using mod()
	// Get integer result when dividing current frame by number of columns (5/6 = 0, 8/6 = 1, 24/6 = 4, etc.)
	float rowIndex;
	modf(evenFrame / (float)columns, rowIndex);
	float rowIndexNextFrame;
	modf(oddFrame / (float)columns, rowIndexNextFrame);

	float2 currentFrame;     
	currentFrame.x = frac (evenFrame * frameStep.x);
	currentFrame.y = 1 - rowIndex * frameStep.y;

	float2 nextFrame;
	nextFrame.x = frac (oddFrame * frameStep.x);
	nextFrame.y = 1 - rowIndexNextFrame * frameStep.y;
	
	float2 spriteUV = (frameUVs + currentFrame);
	float2 spriteUV_2 = (frameUVs + nextFrame);

	float4 firstTex = tex2D (tex, spriteUV);
	float4 secondTex = tex2D (tex, spriteUV_2);

	float frameBlend = abs (frac (halfFrame) * 2.0 - 1.0);

	return lerp(firstTex, secondTex, frameBlend);
}

float2 RotateUVsAroundPoint (float2 inputUV, float rotAngle, float2 rotCenter)
{
	float2 UVsWithOffset = inputUV - rotCenter;

	float rotAngleSine = sin (rotAngle);
	float rotAngleCosine = cos (rotAngle);

	float rotatedUVsX = dot (UVsWithOffset, float2 (rotAngleCosine, -rotAngleSine));
	float rotatedUVsY = dot (UVsWithOffset, float2 (rotAngleSine, rotAngleCosine));

	return float2 (rotatedUVsX, rotatedUVsY) + rotCenter;	
}