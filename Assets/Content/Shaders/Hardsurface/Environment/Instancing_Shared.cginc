//https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/f16tof32--sm5---asm-
inline float4 Uint2ToFloat4(uint2 val)
{
	float4 result;
	result.x = f16tof32(val.x);
	result.y = f16tof32(val.x >> 16);
	
	result.z = f16tof32(val.y);
	result.w = f16tof32(val.y >> 16);

	return result;
}

inline float2 UintToFloat2(uint val)
{
	float2 result;

	result.x = f16tof32(val);
	result.y = f16tof32(val >> 16);

	return result;
}


//These are eventually coming out in SM 6_6, though they're not here yet. https://microsoft.github.io/DirectX-Specs/d3d/HLSL_SM_6_6_Pack_Unpack_Intrinsics.html
inline float4 UintToFloat4(uint val)
{
	//Since there's no 8 bit type in HLSL (equivalent to byte), we have to extract each 8 bit sequence into a uint, and then normalize them back to 0-1 space by dividing them by 255
	uint w = val >> 24;
	uint z = (val & 0x00ff0000) >> 16;
	uint y = (val & 0x0000ff00) >> 8;
	uint x = (val & 0x000000ff);

	return float4(x / 255.0,y / 255.0,z / 255.0,w / 255.0);
}

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

//Half Vector4's are from the c# data model, check out HalfVector4, and HalfVector8 in CustomProperties.cs
//They'll get marshalled here on the GPU with the structured buffer
//The total stride should be 128 bits, composed of 4 32 bit uints
struct HalfVector4
{
	uint2 data;

	inline float4 Unpack()
	{
		return Uint2ToFloat4(data);
	}
};

struct HalfVector8
{
	uint4 data;

	inline float4 UnpackPrimary()
	{
		return Uint2ToFloat4(data.xy);
	}

	inline float4 UnpackSecondary()
	{
		return Uint2ToFloat4(data.zw);		
	}
};

//Fixed Vector4's are from the c# data model, check out FixedVector4, and FixedVector8 in CustomProperties.cs
//They'll get marshalled here on the GPU with the structured buffer
//The total stride should be 32 bits, composed of 4 8 bit values packed in a single uint
struct FixedVector4
{
	uint data;

	inline float4 Unpack()
	{
		return UintToFloat4(data);
	}
};

struct FixedVector8
{
	uint2 data;

	inline float4 UnpackPrimary()
	{
		return UintToFloat4(data.x);
	}

	inline float4 UnpackSecondary()
	{
		return UintToFloat4(data.y);
	}
};


//Fixed vector 8's
//HSB Offset Property
StructuredBuffer<HalfVector8> hsbData;
//Integrity Property
StructuredBuffer<FixedVector8> integrityData;

//Half vector 4's
//Packed Prop Shader Property
StructuredBuffer<HalfVector4> packedPropData;
//Scale Shader Property Half Precision
StructuredBuffer<HalfVector4> scaleData;

//Half vector 8
//Damage Property 
StructuredBuffer<HalfVector8> damageData;

//Matrix 4x4 for local to world Transformations, 
StructuredBuffer<float4x4> instanceData;

inline float4x4 inverse(float4x4 m) {
	float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
	float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
	float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
	float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

	float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
	float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
	float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
	float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

	float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
	float idet = 1.0f / det;

	float4x4 ret;

	ret[0][0] = t11 * idet;
	ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
	ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
	ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

	ret[1][0] = t12 * idet;
	ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
	ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
	ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

	ret[2][0] = t13 * idet;
	ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
	ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
	ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

	ret[3][0] = t14 * idet;
	ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
	ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
	ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

	return ret;
}

//Ported from Doom3
//       _                       
//      | |                      
//   __| | ___   ___  _ __ ____ 
//  / _` |/ _ \ / _ \| '_ ` _  |
// | (_| | (_) | (_) | | | | | |
// \__,_|\___/ \___/|_| |_| |_|
// https://github.com/id-Software/DOOM-3-BFG/blob/master/neo/idlib/math/Matrix.cpp
inline float4x4 doomFastInverse(float4x4 mat)
{
	// 84+4+16 = 104 multiplications
	//			   1 division
	double det, invDet;

	// 2x2 sub-determinants required to calculate 4x4 determinant
	float det2_01_01 = mat[0][0] * mat[1][1] - mat[0][1] * mat[1][0];
	float det2_01_02 = mat[0][0] * mat[1][2] - mat[0][2] * mat[1][0];
	float det2_01_03 = mat[0][0] * mat[1][3] - mat[0][3] * mat[1][0];
	float det2_01_12 = mat[0][1] * mat[1][2] - mat[0][2] * mat[1][1];
	float det2_01_13 = mat[0][1] * mat[1][3] - mat[0][3] * mat[1][1];
	float det2_01_23 = mat[0][2] * mat[1][3] - mat[0][3] * mat[1][2];

	// 3x3 sub-determinants required to calculate 4x4 determinant
	float det3_201_012 = mat[2][0] * det2_01_12 - mat[2][1] * det2_01_02 + mat[2][2] * det2_01_01;
	float det3_201_013 = mat[2][0] * det2_01_13 - mat[2][1] * det2_01_03 + mat[2][3] * det2_01_01;
	float det3_201_023 = mat[2][0] * det2_01_23 - mat[2][2] * det2_01_03 + mat[2][3] * det2_01_02;
	float det3_201_123 = mat[2][1] * det2_01_23 - mat[2][2] * det2_01_13 + mat[2][3] * det2_01_12;

	det = ( - det3_201_123 * mat[3][0] + det3_201_023 * mat[3][1] - det3_201_013 * mat[3][2] + det3_201_012 * mat[3][3] );

	invDet = 1.0f / det;

	// remaining 2x2 sub-determinants
	float det2_03_01 = mat[0][0] * mat[3][1] - mat[0][1] * mat[3][0];
	float det2_03_02 = mat[0][0] * mat[3][2] - mat[0][2] * mat[3][0];
	float det2_03_03 = mat[0][0] * mat[3][3] - mat[0][3] * mat[3][0];
	float det2_03_12 = mat[0][1] * mat[3][2] - mat[0][2] * mat[3][1];
	float det2_03_13 = mat[0][1] * mat[3][3] - mat[0][3] * mat[3][1];
	float det2_03_23 = mat[0][2] * mat[3][3] - mat[0][3] * mat[3][2];

	float det2_13_01 = mat[1][0] * mat[3][1] - mat[1][1] * mat[3][0];
	float det2_13_02 = mat[1][0] * mat[3][2] - mat[1][2] * mat[3][0];
	float det2_13_03 = mat[1][0] * mat[3][3] - mat[1][3] * mat[3][0];
	float det2_13_12 = mat[1][1] * mat[3][2] - mat[1][2] * mat[3][1];
	float det2_13_13 = mat[1][1] * mat[3][3] - mat[1][3] * mat[3][1];
	float det2_13_23 = mat[1][2] * mat[3][3] - mat[1][3] * mat[3][2];

	// remaining 3x3 sub-determinants
	float det3_203_012 = mat[2][0] * det2_03_12 - mat[2][1] * det2_03_02 + mat[2][2] * det2_03_01;
	float det3_203_013 = mat[2][0] * det2_03_13 - mat[2][1] * det2_03_03 + mat[2][3] * det2_03_01;
	float det3_203_023 = mat[2][0] * det2_03_23 - mat[2][2] * det2_03_03 + mat[2][3] * det2_03_02;
	float det3_203_123 = mat[2][1] * det2_03_23 - mat[2][2] * det2_03_13 + mat[2][3] * det2_03_12;

	float det3_213_012 = mat[2][0] * det2_13_12 - mat[2][1] * det2_13_02 + mat[2][2] * det2_13_01;
	float det3_213_013 = mat[2][0] * det2_13_13 - mat[2][1] * det2_13_03 + mat[2][3] * det2_13_01;
	float det3_213_023 = mat[2][0] * det2_13_23 - mat[2][2] * det2_13_03 + mat[2][3] * det2_13_02;
	float det3_213_123 = mat[2][1] * det2_13_23 - mat[2][2] * det2_13_13 + mat[2][3] * det2_13_12;

	float det3_301_012 = mat[3][0] * det2_01_12 - mat[3][1] * det2_01_02 + mat[3][2] * det2_01_01;
	float det3_301_013 = mat[3][0] * det2_01_13 - mat[3][1] * det2_01_03 + mat[3][3] * det2_01_01;
	float det3_301_023 = mat[3][0] * det2_01_23 - mat[3][2] * det2_01_03 + mat[3][3] * det2_01_02;
	float det3_301_123 = mat[3][1] * det2_01_23 - mat[3][2] * det2_01_13 + mat[3][3] * det2_01_12;

	float4x4 ret;

	ret[0][0] =	- det3_213_123 * invDet;
	ret[1][0] = + det3_213_023 * invDet;
	ret[2][0] = - det3_213_013 * invDet;
	ret[3][0] = + det3_213_012 * invDet;

	ret[0][1] = + det3_203_123 * invDet;
	ret[1][1] = - det3_203_023 * invDet;
	ret[2][1] = + det3_203_013 * invDet;
	ret[3][1] = - det3_203_012 * invDet;

	ret[0][2] = + det3_301_123 * invDet;
	ret[1][2] = - det3_301_023 * invDet;
	ret[2][2] = + det3_301_013 * invDet;
	ret[3][2] = - det3_301_012 * invDet;

	ret[0][3] = - det3_201_123 * invDet;
	ret[1][3] = + det3_201_023 * invDet;
	ret[2][3] = - det3_201_013 * invDet;
	ret[3][3] = + det3_201_012 * invDet;

	return ret;
}


void setupFastDoom()
{
	unity_ObjectToWorld = instanceData[unity_InstanceID];
	unity_WorldToObject = doomFastInverse(unity_ObjectToWorld);
}

void setup()
{
	unity_ObjectToWorld = instanceData[unity_InstanceID];
	unity_WorldToObject = inverse(unity_ObjectToWorld);
	//setupFastDoom();
}
#endif
