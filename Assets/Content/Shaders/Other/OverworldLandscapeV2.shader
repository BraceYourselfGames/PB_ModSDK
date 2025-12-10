// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Overworld/LandscapeV2"
{
	Properties
	{
		[Header(Normal)][Space(10)]_NormalTex("NormalTex", 2D) = "bump" {}
		_NormalIntensity("NormalIntensity", Range( 0 , 1)) = 0
		[Header(Fog)][Space(10)]_FogAmount("FogAmount", Range( 0 , 1)) = 0
		_FogFlowTex("FogFlowTex", 2D) = "white" {}
		_FogSpeed("FogSpeed", Range( 0 , 1)) = 0
		_FogNoiseScale("FogNoiseScale", Range( 10 , 1000)) = 10
		_FogNoiseOffset("FogNoiseOffset", Float) = 0
		_FogDisplacement("FogDisplacement", Range( 0 , 1)) = 0
		_FogContrast("FogContrast", Float) = 0
		[Header(Grid)][Space(10)]_GridColor("GridColor", Color) = (0.4764151,0.9100755,1,0)
		[Header(Grid)][Space(10)]_GridColorSelected("GridColorSelected", Color) = (0.4764151,0.9100755,1,0)
		_GridPattern("GridPattern", 2D) = "white" {}
		_GridInputs("GridInputs", Vector) = (100,0,0,0)
		_IsolineInputs("IsolineInputs", Vector) = (1,0.5,1,0)
		_IsolineThicknessMask("IsolineThicknessMask", Vector) = (100,200,1,1)
		_IsolineThicknessFar("IsolineThicknessFar", Range( 1 , 4)) = 1
		_DetailWarpTex("DetailWarpTex", 2D) = "white" {}
		_DetailWarpDistMask("DetailWarpDistMask", Vector) = (100,200,1,1)
		_DetailWarpAmount("DetailWarpAmount", Range( 0 , 1)) = 0.008
		_DetailPackedTex("DetailPackedTex", 2D) = "white" {}
		_DetailAmount("DetailAmount", Range( 0 , 1)) = 0
		_DetailScale("DetailScale", Float) = 0
		[Space(10)]_DetailFadeMask("DetailFadeMask", Vector) = (100,200,1,1)
		[Header(Other)][Space(10)]_ShadowColor("ShadowColor", Color) = (0,0,0,0)
		_WaterSmoothness("WaterSmoothness", Range( 0 , 1)) = 1
		_FlattenWaterNormals("FlattenWaterNormals", Range( 0 , 1)) = 0
		_FlatteningInputs("FlatteningInputs", Vector) = (0,0,0,0)
		_SmoothnessVariation("SmoothnessVariation", Float) = 0.25
		_DebugMode("DebugMode", Range( 0 , 1)) = 0
		_UVScale("UVScale", Vector) = (1,1,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 vertexToFrag592;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float4 _GlobalLandscapeSpotlightData;
		uniform float4 _GlobalLandscapeDimensionData;
		uniform float _NormalIntensity;
		uniform sampler2D _NormalTex;
		uniform sampler2D _DetailWarpTex;
		uniform float4 _DetailWarpTex_ST;
		uniform float _DetailWarpAmount;
		uniform float4 _DetailWarpDistMask;
		uniform float4 _UVScale;
		uniform sampler2D _FogFlowTex;
		uniform float _FogSpeed;
		uniform float _FogNoiseScale;
		uniform float _FogNoiseOffset;
		uniform sampler2D _GlobalLandscapeMainTex;
		uniform float _FlattenWaterNormals;
		uniform sampler2D _DetailPackedTex;
		uniform float _DetailScale;
		uniform sampler2D _GlobalLandscapeSplatTex;
		uniform float4 _ShadowColor;
		uniform float _FogContrast;
		uniform float _FogAmount;
		uniform float _DetailAmount;
		uniform float4 _DetailFadeMask;
		uniform float _DebugMode;
		uniform float4 _FlatteningInputs;
		uniform float _FogDisplacement;
		uniform float4 _GridColor;
		uniform float4 _GridColorSelected;
		uniform float4 _GlobalLandscapeSelectionData;
		uniform sampler2D _GridPattern;
		uniform float4 _GridInputs;
		uniform float4 _IsolineInputs;
		uniform float4 _IsolineThicknessMask;
		uniform float _IsolineThicknessFar;
		uniform float4 _GlobalFog_AmbientColorAndInfluence;
		uniform float _SmoothnessVariation;
		uniform float _WaterSmoothness;


		float MyCustomExpression34_g51( float input , float contrast )
		{
			contrast *= 0.5;
			float contrastLo = 0.5 - contrast;
			float contrastHi = 0.5 + contrast;
			return saturate (lerp (contrastLo, contrastHi, input));
		}


		float ContrastCustom346( float input , float contrast )
		{
			contrast *= 0.5;
			float contrastLo = 0.5 - contrast;
			float contrastHi = 0.5 + contrast;
			return saturate (lerp (contrastLo, contrastHi, input));
		}


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float IsolineMask34_g113( float thickness , float spacing , float offset , float maskOffset , float3 worldPos , float3 worldNormal )
		{
			float lineWidth = max (0.1, thickness * length (cross (worldNormal, float3(0, 1, 0))));
			float y = worldPos.y / max (0.1, spacing);
			float a1 = frac ((y + offset));
			float mask = saturate (a1);
			mask = abs ((mask - 0.5) * 2);
			mask = saturate (mask - (1 - lineWidth));
			mask = mask * (1 / lineWidth);
			float dotMask = dot (worldNormal, float3(0, 1, 0));
			dotMask = saturate (dotMask * maskOffset - maskOffset + 1);
			mask = lerp (mask, 0, dotMask);
			return saturate (mask);
		}


		float IsolineMask34_g112( float thickness , float spacing , float offset , float maskOffset , float3 worldPos , float3 worldNormal )
		{
			float lineWidth = max (0.1, thickness * length (cross (worldNormal, float3(0, 1, 0))));
			float y = worldPos.y / max (0.1, spacing);
			float a1 = frac ((y + offset));
			float mask = saturate (a1);
			mask = abs ((mask - 0.5) * 2);
			mask = saturate (mask - (1 - lineWidth));
			mask = mask * (1 / lineWidth);
			float dotMask = dot (worldNormal, float3(0, 1, 0));
			dotMask = saturate (dotMask * maskOffset - maskOffset + 1);
			mask = lerp (mask, 0, dotMask);
			return saturate (mask);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 _NormalYUp = float3(0,1,0);
			float3 break22_g79 = (_GlobalLandscapeSpotlightData).xyz;
			float2 appendResult29_g79 = (float2(break22_g79.x , break22_g79.z));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult28_g79 = (float2(ase_worldPos.x , ase_worldPos.z));
			float temp_output_262_0 = distance( appendResult29_g79 , appendResult28_g79 );
			float temp_output_486_0 = ( _GlobalLandscapeDimensionData.x * _GlobalLandscapeSpotlightData.w );
			float temp_output_2_0_g55 = temp_output_486_0;
			float temp_output_517_0 = temp_output_2_0_g55;
			float temp_output_34_0_g110 = temp_output_517_0;
			float temp_output_517_38 = ( temp_output_2_0_g55 + ( temp_output_2_0_g55 * 0.25 ) );
			float fogFade461 = ( 1.0 - saturate( ( max( ( temp_output_262_0 - temp_output_34_0_g110 ) , 0.0 ) / max( ( temp_output_517_38 - temp_output_34_0_g110 ) , 0.01 ) ) ) );
			float3 lerpResult98 = lerp( ase_vertexNormal , _NormalYUp , saturate( ( 1.0 - fogFade461 ) ));
			v.normal = lerpResult98;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
			half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
			float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * tangentSign;
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir = mul( ase_worldToTangent, _NormalYUp);
			o.vertexToFrag592 = worldToTangentDir;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float3 break22_g79 = (_GlobalLandscapeSpotlightData).xyz;
			float2 appendResult29_g79 = (float2(break22_g79.x , break22_g79.z));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult28_g79 = (float2(ase_worldPos.x , ase_worldPos.z));
			float temp_output_262_0 = distance( appendResult29_g79 , appendResult28_g79 );
			float temp_output_486_0 = ( _GlobalLandscapeDimensionData.x * _GlobalLandscapeSpotlightData.w );
			float temp_output_2_0_g99 = temp_output_486_0;
			float temp_output_34_0_g100 = ( temp_output_2_0_g99 + ( temp_output_2_0_g99 * -0.25 ) );
			float spolightMask259 = saturate( ( max( ( temp_output_262_0 - temp_output_34_0_g100 ) , 0.0 ) / max( ( temp_output_2_0_g99 - temp_output_34_0_g100 ) , 0.01 ) ) );
			float2 uv_DetailWarpTex = i.uv_texcoord * _DetailWarpTex_ST.xy + _DetailWarpTex_ST.zw;
			float4 tex2DNode162 = tex2D( _DetailWarpTex, uv_DetailWarpTex );
			float temp_output_242_0 = distance( ase_worldPos , _WorldSpaceCameraPos );
			float temp_output_34_0_g80 = _DetailWarpDistMask.x;
			float2 appendResult623 = (float2(_UVScale.x , _UVScale.y));
			float2 temp_cast_0 = (_FogSpeed).xx;
			float2 panner301 = ( _Time.y * temp_cast_0 + ( (ase_worldPos).xz / max( _FogNoiseScale , 0.1 ) ));
			float4 tex2DNode283 = tex2D( _FogFlowTex, panner301 );
			float fogMap318 = tex2DNode283.r;
			float temp_output_1_0_g81 = temp_output_262_0;
			float input34_g51 = fogMap318;
			float contrast34_g51 = 1.0;
			float localMyCustomExpression34_g51 = MyCustomExpression34_g51( input34_g51 , contrast34_g51 );
			float temp_output_403_0 = ( ( localMyCustomExpression34_g51 * 2.0 ) - 1.0 );
			float4 appendResult395 = (float4(( temp_output_403_0 * -1.0 ) , ( temp_output_403_0 * -0.5 ) , ( temp_output_403_0 * 1.5 ) , ( temp_output_403_0 * 2.0 )));
			float temp_output_2_0_g56 = temp_output_486_0;
			float temp_output_2_0_g55 = temp_output_486_0;
			float temp_output_517_0 = temp_output_2_0_g55;
			float temp_output_517_38 = ( temp_output_2_0_g55 + ( temp_output_2_0_g55 * 0.25 ) );
			float4 appendResult511 = (float4(( temp_output_2_0_g56 + ( temp_output_2_0_g56 * -0.125 ) ) , temp_output_2_0_g56 , temp_output_517_0 , temp_output_517_38));
			float4 break44_g81 = ( ( appendResult395 * _FogNoiseOffset ) + appendResult511 );
			float temp_output_46_0_g81 = max( break44_g81.y , ( break44_g81.x + 1.0 ) );
			float temp_output_49_0_g81 = max( break44_g81.z , ( temp_output_46_0_g81 + 1.0 ) );
			float fogMask279 = min( saturate( ( max( ( temp_output_1_0_g81 - break44_g81.x ) , 0.0 ) / max( ( temp_output_46_0_g81 - break44_g81.x ) , 0.01 ) ) ) , ( 1.0 - saturate( ( max( ( temp_output_1_0_g81 - temp_output_49_0_g81 ) , 0.0 ) / max( ( max( break44_g81.w , ( temp_output_49_0_g81 + 1.0 ) ) - temp_output_49_0_g81 ) , 0.01 ) ) ) ) );
			float2 temp_output_290_0 = ( ( ( ( ( (tex2DNode162).rg - float2( 0.5,0.5 ) ) * ( _DetailWarpAmount * ( 1.0 - saturate( ( max( ( temp_output_242_0 - temp_output_34_0_g80 ) , 0.0 ) / max( ( _DetailWarpDistMask.y - temp_output_34_0_g80 ) , 0.01 ) ) ) ) ) ) + i.uv_texcoord ) * appendResult623 ) + ( fogMap318 * pow( fogMask279 , 8.0 ) * 0.02 ) );
			float3 normalSample563 = UnpackScaleNormal( tex2D( _NormalTex, temp_output_290_0 ), ( ( 1.0 - spolightMask259 ) * _NormalIntensity ) );
			float4 tex2DNode1 = tex2D( _GlobalLandscapeMainTex, temp_output_290_0 );
			float waterMask254 = tex2DNode1.a;
			float3 lerpResult591 = lerp( normalSample563 , i.vertexToFrag592 , ( waterMask254 * _FlattenWaterNormals ));
			o.Normal = lerpResult591;
			float4 tex2DNode422 = tex2D( _DetailPackedTex, ( (ase_worldPos).xz / max( _DetailScale , 0.1 ) ) );
			float4 temp_cast_1 = (tex2DNode422.r).xxxx;
			float4 splatMap421 = tex2D( _GlobalLandscapeSplatTex, temp_output_290_0 );
			float4 break432 = splatMap421;
			float4 lerpResult430 = lerp( float4(0.5,0.5,0.5,0) , temp_cast_1 , ( pow( ( 1.0 - waterMask254 ) , 4.0 ) * break432.r ));
			float4 temp_cast_2 = (tex2DNode422.g).xxxx;
			float4 lerpResult433 = lerp( lerpResult430 , temp_cast_2 , break432.g);
			float4 temp_cast_3 = (tex2DNode422.b).xxxx;
			float4 lerpResult434 = lerp( lerpResult433 , temp_cast_3 , break432.b);
			float input346 = tex2DNode283.r;
			float contrast346 = _FogContrast;
			float localContrastCustom346 = ContrastCustom346( input346 , contrast346 );
			float temp_output_340_0 = saturate( ( ( 1.0 - pow( ( 1.0 - tex2DNode283.r ) , 4.0 ) ) + saturate( localContrastCustom346 ) ) );
			float fogMaskWide341 = ( temp_output_340_0 * fogMask279 );
			float fogAmount362 = _FogAmount;
			float4 lerpResult18 = lerp( _ShadowColor , tex2DNode1 , ( ( 1.0 - spolightMask259 ) * ( 1.0 - ( fogMaskWide341 * fogAmount362 ) ) ));
			float4 blendOpSrc108 = lerpResult434;
			float4 blendOpDest108 = lerpResult18;
			float cameraDistance447 = temp_output_242_0;
			float temp_output_34_0_g111 = _DetailFadeMask.x;
			float4 lerpBlendMode108 = lerp(blendOpDest108,(( blendOpDest108 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest108 ) * ( 1.0 - blendOpSrc108 ) ) : ( 2.0 * blendOpDest108 * blendOpSrc108 ) ),( _DetailAmount * ( 1.0 - saturate( ( max( ( cameraDistance447 - temp_output_34_0_g111 ) , 0.0 ) / max( ( _DetailFadeMask.y - temp_output_34_0_g111 ) , 0.01 ) ) ) ) * ( 1.0 - waterMask254 ) ));
			float4 lerpResult597 = lerp( ( saturate( lerpBlendMode108 )) , splatMap421 , _DebugMode);
			float4 albedo549 = lerpResult597;
			o.Albedo = albedo549.rgb;
			float4 color524 = IsGammaSpace() ? float4(0,0,0,1) : float4(0,0,0,1);
			float4 dimensionData485 = _GlobalLandscapeDimensionData;
			float4 break521 = dimensionData485;
			float temp_output_631_0 = ( ase_worldPos.y / break521.y );
			float lerpResult636 = lerp( 0.45 , 0.56 , temp_output_631_0);
			float lerpResult638 = lerp( 1.0 , 0.7 , temp_output_631_0);
			float temp_output_630_0 = saturate( temp_output_631_0 );
			float3 hsvTorgb626 = HSVToRGB( float3(lerpResult636,lerpResult638,temp_output_630_0) );
			float4 lerpResult526 = lerp( color524 , float4( hsvTorgb626 , 0.0 ) , saturate( ( ase_worldPos.y - ( break521.y * break521.z ) ) ));
			float thickness34_g113 = 1.5;
			float spacing34_g113 = ( break521.y / 64.0 );
			float offset34_g113 = 0.0;
			float maskOffset34_g113 = 32.0;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_2_0_g82 = temp_output_486_0;
			float temp_output_34_0_g83 = temp_output_2_0_g82;
			float flattenMask456 = saturate( ( max( ( temp_output_262_0 - temp_output_34_0_g83 ) , 0.0 ) / max( ( ( temp_output_2_0_g82 + ( temp_output_2_0_g82 * 0.75 ) ) - temp_output_34_0_g83 ) , 0.01 ) ) );
			float lerpResult56 = lerp( 0.0 , ( ( ( 1.0 - ase_vertex3Pos.y ) * _FlatteningInputs.x ) - _FlatteningInputs.y ) , pow( flattenMask456 , 1.0 ));
			float4 appendResult61 = (float4(0.0 , ( lerpResult56 + ( pow( fogMask279 , 8.0 ) * fogMap318 * _FogDisplacement ) ) , 0.0 , 0.0));
			float4 vertexOffset355 = appendResult61;
			float4 temp_output_357_0 = ( float4( ase_worldPos , 0.0 ) - vertexOffset355 );
			float3 worldPos34_g113 = temp_output_357_0.xyz;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 normalizeResult43_g113 = normalize( ase_worldNormal );
			float3 worldNormal34_g113 = normalizeResult43_g113;
			float localIsolineMask34_g113 = IsolineMask34_g113( thickness34_g113 , spacing34_g113 , offset34_g113 , maskOffset34_g113 , worldPos34_g113 , worldNormal34_g113 );
			float temp_output_532_0 = saturate( break521.w );
			float3 appendResult605 = (float3(_GlobalLandscapeSelectionData.x , 0.0 , _GlobalLandscapeSelectionData.y));
			float3 break22_g95 = appendResult605;
			float2 appendResult29_g95 = (float2(break22_g95.x , break22_g95.z));
			float2 appendResult28_g95 = (float2(ase_worldPos.x , ase_worldPos.z));
			float temp_output_34_0_g98 = _GlobalLandscapeSelectionData.z;
			float temp_output_609_0 = ( ( 1.0 - saturate( ( max( ( distance( appendResult29_g95 , appendResult28_g95 ) - temp_output_34_0_g98 ) , 0.0 ) / max( ( ( _GlobalLandscapeSelectionData.z * 1.5 ) - temp_output_34_0_g98 ) , 0.01 ) ) ) ) * _GlobalLandscapeSelectionData.w );
			float selectionMask615 = pow( temp_output_609_0 , 0.1 );
			float4 lerpResult617 = lerp( _GridColor , _GridColorSelected , selectionMask615);
			float temp_output_34_0_g107 = _IsolineThicknessMask.x;
			float thickness34_g112 = ( _IsolineInputs.x + ( saturate( ( max( ( distance( ase_worldPos , _WorldSpaceCameraPos ) - temp_output_34_0_g107 ) , 0.0 ) / max( ( _IsolineThicknessMask.y - temp_output_34_0_g107 ) , 0.01 ) ) ) * _IsolineThicknessFar ) );
			float spacing34_g112 = _IsolineInputs.y;
			float offset34_g112 = _IsolineInputs.z;
			float maskOffset34_g112 = _IsolineInputs.w;
			float3 worldPos34_g112 = temp_output_357_0.xyz;
			float3 normalizeResult43_g112 = normalize( ase_worldNormal );
			float3 worldNormal34_g112 = normalizeResult43_g112;
			float localIsolineMask34_g112 = IsolineMask34_g112( thickness34_g112 , spacing34_g112 , offset34_g112 , maskOffset34_g112 , worldPos34_g112 , worldNormal34_g112 );
			float temp_output_2_0_g108 = temp_output_486_0;
			float temp_output_34_0_g109 = ( temp_output_2_0_g108 + ( temp_output_2_0_g108 * -0.25 ) );
			float isolineMask261 = max( saturate( ( max( ( temp_output_262_0 - temp_output_34_0_g109 ) , 0.0 ) / max( ( temp_output_2_0_g108 - temp_output_34_0_g109 ) , 0.01 ) ) ) , temp_output_609_0 );
			float temp_output_34_0_g110 = temp_output_517_0;
			float fogFade461 = ( 1.0 - saturate( ( max( ( temp_output_262_0 - temp_output_34_0_g110 ) , 0.0 ) / max( ( temp_output_517_38 - temp_output_34_0_g110 ) , 0.01 ) ) ) );
			float4 lerpResult468 = lerp( ( _GlobalFog_AmbientColorAndInfluence * float4( 0.1718138,0.2327253,0.3113208,1 ) ) , _GlobalFog_AmbientColorAndInfluence , fogFade461);
			o.Emission = ( ( ( lerpResult526 * saturate( localIsolineMask34_g113 ) * 2.0 * temp_output_532_0 * ( 1.0 - spolightMask259 ) ) + max( ( ( lerpResult617 * tex2D( _GridPattern, ( (ase_worldPos).xz / max( _GridInputs.x , 0.1 ) ) ).g ) * pow( spolightMask259 , 4.0 ) * pow( ( 1.0 - spolightMask259 ) , 0.5 ) * ( 1.0 - fogMaskWide341 ) ) , ( lerpResult617 * saturate( saturate( localIsolineMask34_g112 ) ) * isolineMask261 * ( 1.0 - spolightMask259 ) ) ) + float4( ( hsvTorgb626 * pow( temp_output_630_0 , 2.0 ) * 0.5 * temp_output_532_0 ) , 0.0 ) ) + ( ( lerpResult468 * temp_output_340_0 ) * fogMask279 * fogAmount362 ) ).rgb;
			float4 color580 = IsGammaSpace() ? float4(0.04,0.04,0.04,1) : float4(0.003095975,0.003095975,0.003095975,1);
			float temp_output_273_0 = ( 1.0 - spolightMask259 );
			o.Specular = ( color580 * temp_output_273_0 ).rgb;
			float4 detailSample570 = tex2DNode162;
			float4 temp_cast_14 = (_WaterSmoothness).xxxx;
			float4 lerpResult588 = lerp( saturate( ( detailSample570 * ( 1.0 - splatMap421.g ) * _SmoothnessVariation ) ) , temp_cast_14 , waterMask254);
			o.Smoothness = ( lerpResult588 * temp_output_273_0 ).r;
			o.Occlusion = ( 1.0 - spolightMask259 );
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows exclude_path:forward novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyz = customInputData.vertexToFrag592;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.vertexToFrag592 = IN.customPack2.xyz;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
120;108;2323;1135;-439.0897;-1853.027;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;303;-26.874,1633.504;Float;False;Property;_FogNoiseScale;FogNoiseScale;6;0;Create;True;0;0;False;0;10;1000;10;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;308;68.83967,1489.563;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;311;266.1218,1484.553;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;310;256.8567,1639.026;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;309;491.7033,1490.17;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;302;293.2722,1831.341;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;287;194.0587,1752.34;Float;False;Property;_FogSpeed;FogSpeed;5;0;Create;True;0;0;False;0;0;0.005;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;301;550.2843,1717.785;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;283;758.0043,1689.519;Float;True;Property;_FogFlowTex;FogFlowTex;4;0;Create;True;0;0;False;0;None;4b508c6ab75ae80498a8eeca8955f309;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;318;1198.794,1855.335;Float;False;fogMap;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;393;-2169.563,2617.11;Float;False;318;fogMap;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;394;-1973.812,2623.781;Float;False;ContrastMidtone;-1;;51;a6faf6a407e1a4b43823df58fe9b5f76;0;2;2;FLOAT;0.5;False;35;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;402;-1738.423,2625.536;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;473;-1815.34,1310.304;Float;False;Global;_GlobalLandscapeDimensionData;_GlobalLandscapeDimensionData;32;0;Create;True;0;0;False;0;400,100,0,1;400,97.96397,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;474;-1808.303,1098.419;Float;False;Global;_GlobalLandscapeSpotlightData;_GlobalLandscapeSpotlightData;32;0;Create;True;0;0;False;0;0,0,1,0;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;486;-1425.596,1412.129;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;403;-1576.423,2624.536;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;405;-1380.151,2656.762;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;404;-1381.151,2569.762;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;518;-1360.898,2215.141;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;399;-1380.911,2472.889;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;401;-1383.193,2753.351;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;244;-1799.708,155.3779;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;243;-1730.708,-6.622125;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;395;-1188.198,2582.798;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;517;-1193.739,2336.445;Float;False;SplitWithOffset;-1;;55;9bf30020be21b0a49894b8e66bc87a47;0;2;2;FLOAT;0.5;False;37;FLOAT;0.25;False;2;FLOAT;0;FLOAT;38
Node;AmplifyShaderEditor.RangedFloatNode;398;-1435.485,2855.156;Float;False;Property;_FogNoiseOffset;FogNoiseOffset;7;0;Create;True;0;0;False;0;0;25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;516;-1192.239,2217.645;Float;False;SplitWithOffset;-1;;56;9bf30020be21b0a49894b8e66bc87a47;0;2;2;FLOAT;0.5;False;37;FLOAT;-0.125;False;2;FLOAT;0;FLOAT;38
Node;AmplifyShaderEditor.DistanceOpNode;242;-1421.929,64.8558;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;249;-1476.159,188.4343;Float;False;Property;_DetailWarpDistMask;DetailWarpDistMask;20;0;Create;True;0;0;False;0;100,200,1,1;100,200,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;397;-1035.507,2624.848;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;476;-1361.281,1234.125;Float;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;511;-951.0565,2234.54;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;396;-769.8962,2279.566;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;262;-1035.846,1423.698;Float;False;DistanceToWorld2D;-1;;79;56126796af48a8c4d9bb31e451cc88a3;0;1;2;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;162;-1293.484,-331.2935;Float;True;Property;_DetailWarpTex;DetailWarpTex;18;0;Create;True;0;0;False;0;None;f784272f47eb38d4cba0264fb9e97a5a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;477;-1210.787,62.4212;Float;False;DistanceToMask;-1;;80;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;253;-1274.915,-124.2873;Float;False;Property;_DetailWarpAmount;DetailWarpAmount;21;0;Create;True;0;0;False;0;0.008;0.003;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;164;-796.9447,-329.971;Float;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;252;-960.976,24.91358;Float;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;391;-603.6215,2255.872;Float;False;DistanceToMaskBand;-1;;81;9341fa0b9b9e31e4b9719f583b6144f4;0;2;1;FLOAT;0;False;2;FLOAT4;100,0.5,1,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;251;-754.6537,-121.3141;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;166;-569.7521,-325.6354;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;279;-300.9762,2254.475;Float;False;fogMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;624;-436.3447,-520.9075;Float;False;Property;_UVScale;UVScale;33;0;Create;True;0;0;False;0;1,1,0,0;1,-1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;9;-457.4181,-135.9889;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;-402.7521,-325.6354;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;508;-995.9919,2060.928;Float;False;SplitWithOffset;-1;;82;9bf30020be21b0a49894b8e66bc87a47;0;2;2;FLOAT;0.5;False;37;FLOAT;0.75;False;2;FLOAT;0;FLOAT;38
Node;AmplifyShaderEditor.GetLocalVarNode;320;-429.2123,74.88839;Float;False;279;fogMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;322;-223.845,80.38428;Float;False;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;165;-203.6031,-326.4575;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;623;-208,-432;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;478;-618.1479,2035.99;Float;False;DistanceToMask;-1;;83;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;334;884.0393,1888.513;Float;False;Property;_FogContrast;FogContrast;9;0;Create;True;0;0;False;0;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;330;1064.112,1649.809;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;55;-223.9472,432.1234;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;328;-225.7342,177.9871;Float;False;Constant;_Float1;Float 1;32;0;Create;True;0;0;False;0;0.02;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;319;-427.2123,-5.111627;Float;False;318;fogMap;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;600;-2196.804,1821.573;Float;False;Global;_GlobalLandscapeSelectionData;_GlobalLandscapeSelectionData;33;0;Create;True;0;0;False;0;0,0,32,0;0,0,1,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;346;1197.709,1748.832;Float;False;contrast *= 0.5@$float contrastLo = 0.5 - contrast@$float contrastHi = 0.5 + contrast@$return saturate (lerp (contrastLo, contrastHi, input))@;1;False;2;True;input;FLOAT;0;In;;Float;False;True;contrast;FLOAT;0;In;;Float;False;ContrastCustom;True;False;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;321;-12.78695,-216.1152;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;57;-18.93852,456.0522;Float;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;329;1233.104,1649.22;Float;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;66;-231.3403,584.2681;Float;False;Property;_FlatteningInputs;FlatteningInputs;29;0;Create;True;0;0;False;0;0,0,0,0;1,0.95,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;456;-343.0548,2022.967;Float;False;flattenMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;-11.84799,-326.5287;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;605;-1650.792,1828.8;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;323;563.4845,507.832;Float;False;279;fogMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;156.9215,456.1857;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;335;1511.055,1734.955;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;457;100.8516,341.9418;Float;False;456;flattenMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;331;1494.128,1646.421;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;290;223.632,-327.8616;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;327;449.9278,724.7681;Float;False;Property;_FogDisplacement;FogDisplacement;8;0;Create;True;0;0;False;0;0;0.01;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;603;-1061.114,1850.126;Float;False;DistanceToWorld2D;-1;;95;56126796af48a8c4d9bb31e451cc88a3;0;1;2;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;67;324.9857,352.0629;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;620;-1653.726,1965.192;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;326;552.8173,642.9393;Float;False;318;fogMap;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;415.886,-357.9707;Float;True;Global;_GlobalLandscapeMainTex;_GlobalLandscapeMainTex;3;0;Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;324;780.4845,506.832;Float;False;2;0;FLOAT;0;False;1;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;336;1648.258,1658.024;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;65;329.8949,454.5729;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;254;894.5836,-260.062;Float;False;waterMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;418;417.6278,-159.7;Float;True;Global;_GlobalLandscapeSplatTex;_GlobalLandscapeSplatTex;19;0;Create;True;0;0;False;2;Header(Detail);Space(10);None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;325;956.4851,524.832;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;340;1783.158,1661.621;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;56;558.1838,377.4624;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;601;-618.4231,1844.834;Float;False;DistanceToMask;-1;;98;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;1;False;35;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;277;1654.689,1795.676;Float;False;279;fogMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;238;782.1201,1436.658;Float;False;Property;_FogAmount;FogAmount;2;0;Create;True;0;0;False;2;Header(Fog);Space(10);0;0.258;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;450;439.8669,-1285.571;Float;False;254;waterMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;407;-223.7465,3120.425;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;614;-347.019,1828.553;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;423;107.6477,-851.076;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;421;728.1963,-159.0499;Float;False;splatMap;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;509;-987.3524,1525.996;Float;False;SplitWithOffset;-1;;99;9bf30020be21b0a49894b8e66bc87a47;0;2;2;FLOAT;0.5;False;37;FLOAT;-0.25;False;2;FLOAT;0;FLOAT;38
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;345;1994.47,1776.16;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;428;128.0262,-707.2115;Float;False;Property;_DetailScale;DetailScale;24;0;Create;True;0;0;False;0;0;50;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;313;1156.417,402.3576;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;408;-154.7465,2958.426;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;480;-617.7554,1501.607;Float;False;DistanceToMask;-1;;100;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;452;609.5781,-1280.164;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;609;-334.9938,1906.267;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;341;2194.582,1769.617;Float;False;fogMaskWide;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;485;-1361.626,1312.306;Float;False;dimensionData;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;424;296.6257,-855.9323;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DistanceOpNode;409;154.0308,3029.904;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;425;371.1378,-775.5002;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;410;99.8008,3153.482;Float;False;Property;_IsolineThicknessMask;IsolineThicknessMask;16;0;Create;True;0;0;False;0;100,200,1,1;25,1000,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;362;1240.04,1210.809;Float;False;fogAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;429;566.0088,-1027.337;Float;False;421;splatMap;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;61;1378.569,380.5942;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;259;-349.3475,1512.423;Float;False;spolightMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;355;1601.075,293.0606;Float;False;vertexOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;481;365.1723,3027.469;Float;False;DistanceToMask;-1;;107;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;377;468.8135,3799.198;Float;False;Property;_GridInputs;GridInputs;14;0;Create;True;0;0;False;0;100,0,0,0;10,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;432;753.9401,-1026.32;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;447;-1206.606,216.1587;Float;False;cameraDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;371;1035.243,-141.6823;Float;False;341;fogMaskWide;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;453;826.4857,-1301.701;Float;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;426;533.3003,-851.076;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;364;1048.172,-55.85332;Float;False;362;fogAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;510;-995.0568,1734.544;Float;False;SplitWithOffset;-1;;108;9bf30020be21b0a49894b8e66bc87a47;0;2;2;FLOAT;0.5;False;37;FLOAT;-0.25;False;2;FLOAT;0;FLOAT;38
Node;AmplifyShaderEditor.WorldPosInputsNode;375;461.2466,3652.342;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PowerNode;621;-146.485,1906.128;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;414;48.1565,3328.225;Float;False;Property;_IsolineThicknessFar;IsolineThicknessFar;17;0;Create;True;0;0;False;0;1;1.5;1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;520;477.533,2051.023;Float;False;485;dimensionData;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;521;766.533,2057.023;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;271;1040.947,-235.511;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;379;650.2246,3647.486;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;482;-620.5171,1709.546;Float;False;DistanceToMask;-1;;109;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;79;434.0696,2694.771;Float;False;Property;_IsolineInputs;IsolineInputs;15;0;Create;True;0;0;False;0;1,0.5,1,0;-0.75,2,1,15;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;75;436.3576,2399.191;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;431;771.8432,-1203.346;Float;False;Constant;_Color0;Color 0;20;0;Create;True;0;0;False;0;0.5,0.5,0.5,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;378;724.7366,3727.918;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;448;723.3364,-671.7816;Float;False;447;cameraDistance;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;451;1017.678,-1128.271;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;441;741.0239,-594.2365;Float;False;Property;_DetailFadeMask;DetailFadeMask;25;0;Create;True;0;0;False;1;Space(10);100,200,1,1;100,250,1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;416;599.1568,3079.226;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;422;687.5116,-880.6092;Float;True;Property;_DetailPackedTex;DetailPackedTex;22;0;Create;True;0;0;False;0;None;2a77c064bcf4e6e47af3ac4e5954b1f4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;374;1284.386,-135.5171;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;615;148.9113,2061.175;Float;False;selectionMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;479;-603.2419,2423.115;Float;False;DistanceToMask;-1;;110;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;356;429.7457,2323.278;Float;False;355;vertexOffset;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;483;1016.1,-615.4353;Float;False;DistanceToMask;-1;;111;afe8591b753a6fe448abe1439de79b51;0;3;1;FLOAT;0;False;34;FLOAT;0;False;35;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;373;1287.386,-230.5171;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;48;227.7916,3554.601;Float;True;Property;_GridPattern;GridPattern;13;0;Create;True;0;0;False;0;None;870cf7eada14e464785ffe78dce471f1;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.LerpOp;430;1199.813,-1005.142;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;539;1303.861,2470.744;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;577;-48.85775,137.2034;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;376;886.8992,3652.342;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;386;1182.468,3921.427;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;372;1411.386,-135.5171;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;76;431.0017,2552.056;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;631;1137.539,2589.573;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;463;-286.3907,2424.5;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;887.1367,3198.143;Float;False;Property;_GridColor;GridColor;11;0;Create;True;0;0;False;2;Header(Grid);Space(10);0.4764151,0.9100755,1,0;0.4196041,0.825098,0.972549,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;612;-147.8055,1727.094;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;357;682.0464,2303.977;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;552;1091.251,-496.4078;Float;False;254;waterMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;415;751.1567,3013.226;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;571;2476.625,-563.1874;Float;False;421;splatMap;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;616;889.9103,3374.93;Float;False;Property;_GridColorSelected;GridColorSelected;12;0;Create;True;0;0;False;2;Header(Grid);Space(10);0.4764151,0.9100755,1,0;0.972549,0.4196075,0.4196075,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;618;941.3543,3114.125;Float;False;615;selectionMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;553;1265.52,-543.473;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;561;-54.8893,213.9348;Float;False;Property;_NormalIntensity;NormalIntensity;1;0;Create;True;0;0;False;0;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;387;1379.012,3919.068;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;636;1498.539,2538.573;Float;False;3;0;FLOAT;0.45;False;1;FLOAT;0.56;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;617;1232.18,3258.403;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;461;-61.98737,2420.36;Float;False;fogFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;540;1459.428,2449.048;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;545;1045.654,2826.186;Float;False;IsolineMask;-1;;112;9bf5b44479a4ae94f811532f83a31648;0;6;2;FLOAT;1;False;38;FLOAT;1;False;39;FLOAT;0;False;40;FLOAT;15;False;41;FLOAT3;0,0,0;False;42;FLOAT3;0,1,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;433;1384.886,-906.2606;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;630;1499.781,2769.012;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;261;151.5637,1981.393;Float;False;isolineMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;638;1499.539,2656.573;Float;False;3;0;FLOAT;1;False;1;FLOAT;0.7;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;579;123.9745,-5.251082;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;381;1410.456,3724.283;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;369;1237.646,3391.587;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;109;1103.046,-692.8868;Float;False;Property;_DetailAmount;DetailAmount;23;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;183;749.8682,1517.795;Float;False;Global;_GlobalFog_AmbientColorAndInfluence;_GlobalFog_AmbientColorAndInfluence;26;0;Create;True;0;0;False;0;0.6132076,0.6132076,0.6132076,0;0.4080652,0.4344922,0.5692505,0.8;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;382;1190.477,4001.644;Float;False;341;fogMaskWide;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;572;2665.626,-560.1874;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ColorNode;19;1352.566,-461.8024;Float;False;Property;_ShadowColor;ShadowColor;26;0;Create;True;0;0;False;2;Header(Other);Space(10);0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;49;1089.924,3557.173;Float;True;Property;_TextureSample2;Texture Sample 2;10;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;570;-558.3072,323.8204;Float;False;detailSample;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;347;1558.434,-230.2477;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;449;1261.402,-616.4976;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;91;1419.825,2861.852;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;626;1735.465,2631.576;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;547;1732.459,2547.61;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;354;1496.165,3315.894;Float;False;261;isolineMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;1267.709,1393.072;Float;False;461;fogFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;595;3032.651,712.7242;Float;False;897.0881;358.5801;Force vertical vertex normals for terrain edges (outside of spotlight area);6;98;96;585;586;103;97;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;569;2911.892,-630.5381;Float;False;570;detailSample;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;385;1641.259,4001.003;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;1701.968,-369.7885;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;573;2942.626,-543.1874;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;1649.547,3583.804;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;519;1037.166,2080.055;Float;False;2;0;FLOAT;0;False;1;FLOAT;64;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;574;2882.626,-464.1873;Float;False;Property;_SmoothnessVariation;SmoothnessVariation;30;0;Create;True;0;0;False;0;0.25;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;434;1553.022,-816.1237;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;544;1626.013,2449.255;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;524;1858.032,1987.726;Float;False;Constant;_Color1;Color 1;28;0;Create;True;0;0;False;0;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;578;281.3642,91.42403;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;383;1674.54,3719.581;Float;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;467;1303.253,1286.873;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.1718138,0.2327253,0.3113208,1;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;370;1522.798,3395.289;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;445;1572.874,-690.219;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;388;1639.846,3898.885;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;97;3082.651,871.6409;Float;False;Constant;_NormalYUp;NormalYUp;15;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;529;2147.243,2231.623;Float;False;Constant;_Float0;Float 0;28;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;585;3260.35,955.304;Float;False;461;fogFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;641;2055.362,2766.937;Float;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;643;2059.262,2864.061;Float;False;Constant;_Float2;Float 2;34;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;546;1306.875,2207.71;Float;False;IsolineMask;-1;;113;9bf5b44479a4ae94f811532f83a31648;0;6;2;FLOAT;1.5;False;38;FLOAT;1;False;39;FLOAT;0;False;40;FLOAT;32;False;41;FLOAT3;0,0,0;False;42;FLOAT3;0,1,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;568;3250.719,-478.4347;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0.25;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;556;416.1995,38.6474;Float;True;Property;_NormalTex;NormalTex;0;0;Create;True;0;0;False;2;Header(Normal);Space(10);None;876425090fda1744188d7ee25600f7f5;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;468;1653.996,1448.473;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;1736.09,3271.441;Float;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;526;2144.124,2344.747;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;108;2072.937,-389.985;Float;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;599;1989.087,-85.03259;Float;False;Property;_DebugMode;DebugMode;32;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;548;2148.459,2479.809;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;594;3309.839,153.393;Float;False;699.824;509.5968;Force vertical pixel normals for bodies of water;7;591;592;593;255;99;100;564;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;1858.645,3584.901;Float;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;532;1312.056,2126.475;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;598;2092.028,-209.5948;Float;False;421;splatMap;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;589;3408.769,-476.829;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;523;2349.313,2053.656;Float;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;256;3156.124,-653.4924;Float;False;Property;_WaterSmoothness;WaterSmoothness;27;0;Create;True;0;0;False;0;1;0.841;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;587;3249.633,-355.5239;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;597;2348.144,-249.337;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;239;2203.065,2968.542;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;642;2231.855,2633.623;Float;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;563;1255.504,70.26004;Float;False;normalSample;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;363;1951.709,1881.579;Float;False;362;fogAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;593;3359.839,305.871;Float;False;World;Tangent;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;99;3359.855,546.99;Float;False;Property;_FlattenWaterNormals;FlattenWaterNormals;28;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;255;3454.826,471.7475;Float;False;254;waterMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;258;3254.977,-567.9579;Float;False;254;waterMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;586;3421.56,958.6987;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;305;1961.562,1543.585;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;3644.438,496.6764;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;564;3587.028,203.393;Float;False;563;normalSample;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;592;3568.839,301.871;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;273;3479.628,-343.6137;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;217;2170.17,1604.944;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;96;3542.821,762.7242;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;103;3576.43,955.9953;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;275;3645.6,28.26891;Float;False;259;spolightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;588;3586.868,-584.7291;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;580;3835.435,-632.4517;Float;False;Constant;_Color4;Color 4;28;0;Create;True;0;0;False;0;0.04,0.04,0.04,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;522;2615.715,2181.445;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;549;2592.781,-307.2032;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;590;4153.733,-380.6052;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;390;-2137.717,2019.145;Float;False;Property;_FogMask;FogMask;10;0;Create;True;0;0;False;0;0,0,0,0;350,400,400,500;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;525;1866.183,2187.851;Float;False;Constant;_Color2;Color 2;28;0;Create;True;0;0;False;0;0.5086274,1,0.2980392,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;582;3902.026,-220.0167;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;270;3251.532,-269.6298;Float;False;461;fogFade;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;216;2917.791,1580.233;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RelayNode;576;2302.29,389.5287;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;550;3917.011,-114.4374;Float;False;549;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;484;-1363.021,1098.468;Float;False;spotlightData;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;276;3846.6,33.26897;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;625;-154.8156,-609.2064;Float;False;Constant;_Vector0;Vector 0;34;0;Create;True;0;0;False;0;1,-1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;591;3827.663,227.7893;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DitheringNode;583;3470.802,-240.4122;Float;False;2;False;3;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;584;3191.136,-181.6659;Float;True;Property;_NoiseTexDither;NoiseTexDither;31;0;Create;True;0;0;False;0;None;7dcc3b82e99bf8542b8348c6fcd86cd1;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;257;3769.99,-416.3253;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;602;-1855.416,1781.818;Float;False;selectionInputs;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;98;3747.739,853.5845;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;4372.354,-124.8948;Float;False;True;7;Float;ASEMaterialInspector;0;0;StandardSpecular;Overworld/LandscapeV2;False;False;False;False;False;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;DeferredOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;311;0;308;0
WireConnection;310;0;303;0
WireConnection;309;0;311;0
WireConnection;309;1;310;0
WireConnection;301;0;309;0
WireConnection;301;2;287;0
WireConnection;301;1;302;0
WireConnection;283;1;301;0
WireConnection;318;0;283;1
WireConnection;394;2;393;0
WireConnection;402;0;394;0
WireConnection;486;0;473;1
WireConnection;486;1;474;4
WireConnection;403;0;402;0
WireConnection;405;0;403;0
WireConnection;404;0;403;0
WireConnection;518;0;486;0
WireConnection;399;0;403;0
WireConnection;401;0;403;0
WireConnection;395;0;399;0
WireConnection;395;1;404;0
WireConnection;395;2;405;0
WireConnection;395;3;401;0
WireConnection;517;2;518;0
WireConnection;516;2;518;0
WireConnection;242;0;243;0
WireConnection;242;1;244;0
WireConnection;397;0;395;0
WireConnection;397;1;398;0
WireConnection;476;0;474;0
WireConnection;511;0;516;38
WireConnection;511;1;516;0
WireConnection;511;2;517;0
WireConnection;511;3;517;38
WireConnection;396;0;397;0
WireConnection;396;1;511;0
WireConnection;262;2;476;0
WireConnection;477;1;242;0
WireConnection;477;34;249;1
WireConnection;477;35;249;2
WireConnection;164;0;162;0
WireConnection;252;1;477;0
WireConnection;391;1;262;0
WireConnection;391;2;396;0
WireConnection;251;0;253;0
WireConnection;251;1;252;0
WireConnection;166;0;164;0
WireConnection;279;0;391;0
WireConnection;167;0;166;0
WireConnection;167;1;251;0
WireConnection;508;2;486;0
WireConnection;322;0;320;0
WireConnection;165;0;167;0
WireConnection;165;1;9;0
WireConnection;623;0;624;1
WireConnection;623;1;624;2
WireConnection;478;1;262;0
WireConnection;478;34;508;0
WireConnection;478;35;508;38
WireConnection;330;0;283;1
WireConnection;346;0;283;0
WireConnection;346;1;334;0
WireConnection;321;0;319;0
WireConnection;321;1;322;0
WireConnection;321;2;328;0
WireConnection;57;1;55;2
WireConnection;329;0;330;0
WireConnection;456;0;478;0
WireConnection;169;0;165;0
WireConnection;169;1;623;0
WireConnection;605;0;600;1
WireConnection;605;2;600;2
WireConnection;59;0;57;0
WireConnection;59;1;66;1
WireConnection;335;0;346;0
WireConnection;331;0;329;0
WireConnection;290;0;169;0
WireConnection;290;1;321;0
WireConnection;603;2;605;0
WireConnection;67;0;457;0
WireConnection;620;0;600;3
WireConnection;1;1;290;0
WireConnection;324;0;323;0
WireConnection;336;0;331;0
WireConnection;336;1;335;0
WireConnection;65;0;59;0
WireConnection;65;1;66;2
WireConnection;254;0;1;4
WireConnection;418;1;290;0
WireConnection;325;0;324;0
WireConnection;325;1;326;0
WireConnection;325;2;327;0
WireConnection;340;0;336;0
WireConnection;56;1;65;0
WireConnection;56;2;67;0
WireConnection;601;1;603;0
WireConnection;601;34;600;3
WireConnection;601;35;620;0
WireConnection;614;0;601;0
WireConnection;421;0;418;0
WireConnection;509;2;486;0
WireConnection;345;0;340;0
WireConnection;345;1;277;0
WireConnection;313;0;56;0
WireConnection;313;1;325;0
WireConnection;480;1;262;0
WireConnection;480;34;509;38
WireConnection;480;35;509;0
WireConnection;452;0;450;0
WireConnection;609;0;614;0
WireConnection;609;1;600;4
WireConnection;341;0;345;0
WireConnection;485;0;473;0
WireConnection;424;0;423;0
WireConnection;409;0;408;0
WireConnection;409;1;407;0
WireConnection;425;0;428;0
WireConnection;362;0;238;0
WireConnection;61;1;313;0
WireConnection;259;0;480;0
WireConnection;355;0;61;0
WireConnection;481;1;409;0
WireConnection;481;34;410;1
WireConnection;481;35;410;2
WireConnection;432;0;429;0
WireConnection;447;0;242;0
WireConnection;453;0;452;0
WireConnection;426;0;424;0
WireConnection;426;1;425;0
WireConnection;510;2;486;0
WireConnection;621;0;609;0
WireConnection;521;0;520;0
WireConnection;379;0;375;0
WireConnection;482;1;262;0
WireConnection;482;34;510;38
WireConnection;482;35;510;0
WireConnection;378;0;377;1
WireConnection;451;0;453;0
WireConnection;451;1;432;0
WireConnection;416;0;481;0
WireConnection;416;1;414;0
WireConnection;422;1;426;0
WireConnection;374;0;371;0
WireConnection;374;1;364;0
WireConnection;615;0;621;0
WireConnection;479;1;262;0
WireConnection;479;34;517;0
WireConnection;479;35;517;38
WireConnection;483;1;448;0
WireConnection;483;34;441;1
WireConnection;483;35;441;2
WireConnection;373;0;271;0
WireConnection;430;0;431;0
WireConnection;430;1;422;1
WireConnection;430;2;451;0
WireConnection;539;0;521;1
WireConnection;539;1;521;2
WireConnection;376;0;379;0
WireConnection;376;1;378;0
WireConnection;372;0;374;0
WireConnection;631;0;75;2
WireConnection;631;1;521;1
WireConnection;463;0;479;0
WireConnection;612;0;482;0
WireConnection;612;1;609;0
WireConnection;357;0;75;0
WireConnection;357;1;356;0
WireConnection;415;0;79;1
WireConnection;415;1;416;0
WireConnection;553;0;552;0
WireConnection;387;0;386;0
WireConnection;636;2;631;0
WireConnection;617;0;13;0
WireConnection;617;1;616;0
WireConnection;617;2;618;0
WireConnection;461;0;463;0
WireConnection;540;0;75;2
WireConnection;540;1;539;0
WireConnection;545;2;415;0
WireConnection;545;38;79;2
WireConnection;545;39;79;3
WireConnection;545;40;79;4
WireConnection;545;41;357;0
WireConnection;545;42;76;0
WireConnection;433;0;430;0
WireConnection;433;1;422;2
WireConnection;433;2;432;1
WireConnection;630;0;631;0
WireConnection;261;0;612;0
WireConnection;638;2;631;0
WireConnection;579;0;577;0
WireConnection;572;0;571;0
WireConnection;49;0;48;0
WireConnection;49;1;376;0
WireConnection;570;0;162;0
WireConnection;347;0;373;0
WireConnection;347;1;372;0
WireConnection;449;0;483;0
WireConnection;91;0;545;0
WireConnection;626;0;636;0
WireConnection;626;1;638;0
WireConnection;626;2;630;0
WireConnection;385;0;382;0
WireConnection;18;0;19;0
WireConnection;18;1;1;0
WireConnection;18;2;347;0
WireConnection;573;0;572;1
WireConnection;17;0;617;0
WireConnection;17;1;49;2
WireConnection;519;0;521;1
WireConnection;434;0;433;0
WireConnection;434;1;422;3
WireConnection;434;2;432;2
WireConnection;544;0;540;0
WireConnection;578;0;579;0
WireConnection;578;1;561;0
WireConnection;383;0;381;0
WireConnection;467;0;183;0
WireConnection;370;0;369;0
WireConnection;445;0;109;0
WireConnection;445;1;449;0
WireConnection;445;2;553;0
WireConnection;388;0;387;0
WireConnection;641;0;630;0
WireConnection;546;38;519;0
WireConnection;546;41;357;0
WireConnection;546;42;76;0
WireConnection;568;0;569;0
WireConnection;568;1;573;0
WireConnection;568;2;574;0
WireConnection;556;1;290;0
WireConnection;556;5;578;0
WireConnection;468;0;467;0
WireConnection;468;1;183;0
WireConnection;468;2;469;0
WireConnection;83;0;617;0
WireConnection;83;1;91;0
WireConnection;83;2;354;0
WireConnection;83;3;370;0
WireConnection;526;0;524;0
WireConnection;526;1;626;0
WireConnection;526;2;544;0
WireConnection;108;0;434;0
WireConnection;108;1;18;0
WireConnection;108;2;445;0
WireConnection;548;0;547;0
WireConnection;42;0;17;0
WireConnection;42;1;383;0
WireConnection;42;2;388;0
WireConnection;42;3;385;0
WireConnection;532;0;521;3
WireConnection;589;0;568;0
WireConnection;523;0;526;0
WireConnection;523;1;546;0
WireConnection;523;2;529;0
WireConnection;523;3;532;0
WireConnection;523;4;548;0
WireConnection;597;0;108;0
WireConnection;597;1;598;0
WireConnection;597;2;599;0
WireConnection;239;0;42;0
WireConnection;239;1;83;0
WireConnection;642;0;626;0
WireConnection;642;1;641;0
WireConnection;642;2;643;0
WireConnection;642;3;532;0
WireConnection;563;0;556;0
WireConnection;593;0;97;0
WireConnection;586;0;585;0
WireConnection;305;0;468;0
WireConnection;305;1;340;0
WireConnection;100;0;255;0
WireConnection;100;1;99;0
WireConnection;592;0;593;0
WireConnection;273;0;587;0
WireConnection;217;0;305;0
WireConnection;217;1;277;0
WireConnection;217;2;363;0
WireConnection;103;0;586;0
WireConnection;588;0;589;0
WireConnection;588;1;256;0
WireConnection;588;2;258;0
WireConnection;522;0;523;0
WireConnection;522;1;239;0
WireConnection;522;2;642;0
WireConnection;549;0;597;0
WireConnection;590;0;580;0
WireConnection;590;1;273;0
WireConnection;582;0;580;0
WireConnection;582;1;583;0
WireConnection;216;0;522;0
WireConnection;216;1;217;0
WireConnection;576;0;61;0
WireConnection;484;0;474;0
WireConnection;276;0;275;0
WireConnection;591;0;564;0
WireConnection;591;1;592;0
WireConnection;591;2;100;0
WireConnection;583;0;270;0
WireConnection;583;1;584;0
WireConnection;257;0;588;0
WireConnection;257;1;273;0
WireConnection;602;0;600;0
WireConnection;98;0;96;0
WireConnection;98;1;97;0
WireConnection;98;2;103;0
WireConnection;0;0;550;0
WireConnection;0;1;591;0
WireConnection;0;2;216;0
WireConnection;0;3;590;0
WireConnection;0;4;257;0
WireConnection;0;5;276;0
WireConnection;0;12;98;0
ASEEND*/
//CHKSM=9968BBECE1B02B384A6A4908B32982E1F9B68F50