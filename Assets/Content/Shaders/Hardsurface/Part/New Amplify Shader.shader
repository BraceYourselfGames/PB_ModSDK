// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX/HologramTable"
{
	Properties
	{
		_ColorTop("ColorTop", Color) = (1,1,1,0)
		_ColorBottom("ColorBottom", Color) = (0,0,0,0)
		_HeightGradientPower("HeightGradientPower", Range( 0.1 , 10)) = 1
		_EmissionBlend("EmissionBlend", Range( 0 , 1)) = 1
		_AlbedoBlend("AlbedoBlend", Range( 0 , 1)) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_AnimationSpeed("AnimationSpeed", Float) = 1
		_CoordMultX("CoordMultX", Float) = 1
		_TimeSlideMultiplier("TimeSlideMultiplier", Float) = 1
		_WaveDisplacement("WaveDisplacement", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _WaveDisplacement;
		uniform float _TimeSlideMultiplier;
		uniform float _AnimationSpeed;
		uniform float _CoordMultX;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float4 _ColorBottom;
		uniform float4 _ColorTop;
		uniform float _HeightGradientPower;
		uniform float _AlbedoBlend;
		uniform float _EmissionBlend;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime10 = _Time.y * _AnimationSpeed;
			float temp_output_38_0 = frac( ( _TimeSlideMultiplier * mulTime10 ) );
			float4 appendResult43 = (float4(( ( v.color.r + temp_output_38_0 ) * _CoordMultX ) , mulTime10 , ( ( v.color.b + temp_output_38_0 ) * _CoordMultX ) , 0.0));
			float simplePerlin3D40 = snoise( appendResult43.xyz );
			v.vertex.xyz += ( ( float3(0,1,0) * _WaveDisplacement ) * ( pow( simplePerlin3D40 , 4.0 ) * pow( sin( mulTime10 ) , 4.0 ) ) );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 lerpResult3 = lerp( _ColorBottom , _ColorTop , pow( i.vertexColor.g , _HeightGradientPower ));
			float4 temp_output_59_0 = ( tex2D( _TextureSample0, uv_TextureSample0 ) * lerpResult3 );
			float4 lerpResult64 = lerp( float4( 0,0,0,0 ) , temp_output_59_0 , _AlbedoBlend);
			o.Albedo = lerpResult64.rgb;
			float4 lerpResult68 = lerp( float4( 0,0,0,0 ) , temp_output_59_0 , _EmissionBlend);
			o.Emission = lerpResult68.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
-2560;0;2560;1379;1296.934;1014.029;1.234124;True;True
Node;AmplifyShaderEditor.RangedFloatNode;9;-2625.294,751.1915;Float;False;Property;_AnimationSpeed;AnimationSpeed;6;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-2470.1,605.6412;Float;False;Property;_TimeSlideMultiplier;TimeSlideMultiplier;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;10;-2436.894,751.3915;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-2067.46,638.5054;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;38;-1831.637,661.6981;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;4;-1861.656,338.0256;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;55;-1585.851,698.532;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-1586.851,799.532;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1855.109,520.1085;Float;False;Property;_CoordMultX;CoordMultX;7;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1456.12,800.7794;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-1443.12,701.7794;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;43;-1223.074,725.8027;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-1623.255,144.2716;Float;False;Property;_HeightGradientPower;HeightGradientPower;2;0;Create;True;0;0;False;0;1;1;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-744.4787,-166.367;Float;False;Property;_ColorTop;ColorTop;0;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;5;-398.6788,210.633;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-744.4786,22.133;Float;False;Property;_ColorBottom;ColorBottom;1;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;40;-1070.03,716.7593;Float;False;Simplex3D;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;44;-1835.199,876.5899;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;16;-281.061,1107.272;Float;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PowerNode;52;-658.9352,941.9944;Float;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;3;-151.6787,-66.267;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;50;-559.9352,807.9944;Float;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-792.5788,-393.867;Float;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-345.2603,1268.392;Float;False;Property;_WaveDisplacement;WaveDisplacement;9;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-307.9352,921.9944;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-92.56096,1073.672;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;45.99905,-151.4918;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;69;196.7697,-259.7111;Float;False;Property;_AlbedoBlend;AlbedoBlend;4;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;188.9242,-41.06659;Float;False;Property;_EmissionBlend;EmissionBlend;3;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;64;589.0087,-252.0066;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;91.02592,405.0865;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;48;-554.9352,609.9944;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode;8;162.9212,521.3331;Float;False;0;False;3;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;61.85166,43.55069;Float;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;63.15176,116.3507;Float;False;Constant;_Float1;Float 1;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;49;-330.9352,705.9944;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;68;587.4414,-138.2152;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1012.381,-54.76239;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;FX/HologramTable;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;9;0
WireConnection;57;0;58;0
WireConnection;57;1;10;0
WireConnection;38;0;57;0
WireConnection;55;0;4;1
WireConnection;55;1;38;0
WireConnection;56;0;4;3
WireConnection;56;1;38;0
WireConnection;46;0;56;0
WireConnection;46;1;30;0
WireConnection;45;0;55;0
WireConnection;45;1;30;0
WireConnection;43;0;45;0
WireConnection;43;1;10;0
WireConnection;43;2;46;0
WireConnection;5;0;4;2
WireConnection;5;1;6;0
WireConnection;40;0;43;0
WireConnection;44;0;10;0
WireConnection;52;0;44;0
WireConnection;3;0;2;0
WireConnection;3;1;1;0
WireConnection;3;2;5;0
WireConnection;50;0;40;0
WireConnection;51;0;50;0
WireConnection;51;1;52;0
WireConnection;17;0;16;0
WireConnection;17;1;15;0
WireConnection;59;0;7;0
WireConnection;59;1;3;0
WireConnection;64;1;59;0
WireConnection;64;2;69;0
WireConnection;18;0;17;0
WireConnection;18;1;51;0
WireConnection;48;0;40;0
WireConnection;49;0;48;0
WireConnection;68;1;59;0
WireConnection;68;2;63;0
WireConnection;0;0;64;0
WireConnection;0;2;68;0
WireConnection;0;3;60;0
WireConnection;0;4;61;0
WireConnection;0;11;18;0
ASEEND*/
//CHKSM=A66F1390F02346DFDF20A8AF64CA28BF492695B5