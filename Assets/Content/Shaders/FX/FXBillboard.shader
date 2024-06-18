// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX/Billboard"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		[HDR]_ColorR("ColorR", Color) = (1,1,1,1)
		[HDR]_ColorG("ColorG", Color) = (1,1,1,1)
		[HDR]_ColorB("ColorB", Color) = (1,1,1,1)
		_SoftDistance("SoftDistance", Float) = 1
		_Hue("Hue", Range( 0 , 1)) = 0
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_VertexOffset("VertexOffset", Vector) = (0,0,0,0)
		_Saturation("Saturation", Range( 0 , 1)) = 1
		_ColorTint("ColorTint", Color) = (1,1,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Offset  0 , 0
		Blend SrcAlpha One , One One
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float3 _VertexOffset;
		uniform float _Hue;
		uniform float _Saturation;
		uniform float4 _ColorTint;
		uniform float4 _ColorR;
		uniform float4 _ColorG;
		uniform float4 _ColorB;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _SoftDistance;
		uniform float _Opacity;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += _VertexOffset;
			//Calculate new billboard vertex position and normal;
			float3 upCamVec = normalize ( UNITY_MATRIX_V._m10_m11_m12 );
			float3 forwardCamVec = -normalize ( UNITY_MATRIX_V._m20_m21_m22 );
			float3 rightCamVec = normalize( UNITY_MATRIX_V._m00_m01_m02 );
			float4x4 rotationCamMatrix = float4x4( rightCamVec, 0, upCamVec, 0, forwardCamVec, 0, 0, 0, 0, 1 );
			v.normal = normalize( mul( float4( v.normal , 0 ), rotationCamMatrix )).xyz;
			v.vertex.x *= length( unity_ObjectToWorld._m00_m10_m20 );
			v.vertex.y *= length( unity_ObjectToWorld._m01_m11_m21 );
			v.vertex.z *= length( unity_ObjectToWorld._m02_m12_m22 );
			v.vertex = mul( v.vertex, rotationCamMatrix );
			v.vertex.xyz += unity_ObjectToWorld._m03_m13_m23;
			//Need to nullify rotation inserted by generated surface shader;
			v.vertex = mul( unity_WorldToObject, v.vertex );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode3 = tex2D( _MainTex, uv_MainTex );
			float4 lerpResult24 = lerp( _ColorG , _ColorB , tex2DNode3.b);
			float4 lerpResult25 = lerp( _ColorR , lerpResult24 , tex2DNode3.g);
			float3 hsvTorgb26 = RGBToHSV( ( _ColorTint * lerpResult25 ).rgb );
			float3 hsvTorgb30 = HSVToRGB( float3(_Hue,( _Saturation * hsvTorgb26.y ),hsvTorgb26.z) );
			o.Emission = max( hsvTorgb30 , float3(0,0,0) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth17 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth17 = abs( ( screenDepth17 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _SoftDistance ) );
			o.Alpha = ( ( tex2DNode3.r + tex2DNode3.g + tex2DNode3.b ) * pow( saturate( distanceDepth17 ) , 2.0 ) * _Opacity );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
0;0;2560;1379;1221.99;1050.991;1;True;True
Node;AmplifyShaderEditor.SamplerNode;3;-864.8568,-31.20686;Float;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;None;4fe85b5c14da32e4886570c8e8dffda0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-780.1112,-375.558;Float;False;Property;_ColorG;ColorG;3;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-775.5012,-204.1507;Float;False;Property;_ColorB;ColorB;4;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0.6792453,0.6792453,0.6792453,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;24;-392.6711,-222.7654;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;4;-777.7071,-548.8765;Float;False;Property;_ColorR;ColorR;2;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;25;-158.5546,-244.7547;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;57;-29.41631,-302.1999;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;58;-176.1163,-312.1999;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;53;-777.9164,-723.3001;Float;False;Property;_ColorTint;ColorTint;10;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-743.788,168.0027;Float;False;Property;_SoftDistance;SoftDistance;5;0;Create;True;0;0;False;0;1;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-146.8163,-446.3997;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;17;-561.3698,150.2238;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;11.44996,-626.292;Float;False;Property;_Saturation;Saturation;9;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;26;67.99987,-441.6926;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;315.6498,-419.5916;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;8.200254,-704.2927;Float;False;Property;_Hue;Hue;6;0;Create;True;0;0;False;0;0;0.509;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;21;-311.8078,149.6984;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-310.5877,222.9155;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;22;-156.8318,149.6985;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-363.4958,-3.206034;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;32;118.7001,-260.9919;Float;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.HSVToRGBNode;30;491.8004,-418.2923;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;33;-308.9999,327.9081;Float;False;Property;_Opacity;Opacity;7;0;Create;True;0;0;False;0;1;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;141.6957,-115.9491;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;31;747.8997,-272.6919;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;49;-210.2,416.3082;Float;False;Property;_VertexOffset;VertexOffset;8;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;935.1667,-319.2218;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;FX/Billboard;False;False;False;False;True;True;True;True;True;False;False;False;False;False;True;False;False;False;False;False;False;Back;2;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;8;5;False;-1;1;False;-1;4;1;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;True;Spherical;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;24;0;7;0
WireConnection;24;1;8;0
WireConnection;24;2;3;3
WireConnection;25;0;4;0
WireConnection;25;1;24;0
WireConnection;25;2;3;2
WireConnection;57;0;25;0
WireConnection;58;0;57;0
WireConnection;54;0;53;0
WireConnection;54;1;58;0
WireConnection;17;0;18;0
WireConnection;26;0;54;0
WireConnection;52;0;51;0
WireConnection;52;1;26;2
WireConnection;21;0;17;0
WireConnection;22;0;21;0
WireConnection;22;1;23;0
WireConnection;14;0;3;1
WireConnection;14;1;3;2
WireConnection;14;2;3;3
WireConnection;30;0;27;0
WireConnection;30;1;52;0
WireConnection;30;2;26;3
WireConnection;19;0;14;0
WireConnection;19;1;22;0
WireConnection;19;2;33;0
WireConnection;31;0;30;0
WireConnection;31;1;32;0
WireConnection;2;2;31;0
WireConnection;2;9;19;0
WireConnection;2;11;49;0
ASEEND*/
//CHKSM=97ED7E1D0901F79706E6A74BC97E2AEB7D669FF7