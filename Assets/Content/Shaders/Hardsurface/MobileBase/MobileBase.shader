// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hardsurface/MobileBase"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_AOTex("AOTex", 2D) = "white" {}
		_AOSecondaryTex("AOSecondaryTex", 2D) = "white" {}
		[Gamma]_CurvatureTex("CurvatureTex", 2D) = "gray" {}
		_AOAlbedoIntensity("AO Albedo Intensity", Range( 0 , 1)) = 0.5
		_AOBlend("AO Blend", Range( 0 , 1)) = 0
		_CurvatureHighlightIntensity("Curvature Highlight Intensity", Range( 0 , 1)) = 0
		_CurvatureCavityIntensity("Curvature Cavity Intensity", Range( 0 , 1)) = 0.6
		_Metalness("Metalness", Range( 0 , 1)) = 0.5
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		[HDR]_EmissiveColor("EmissiveColor", Color) = (0,0,0,0)
		[HDR]_Color("Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv2_texcoord2;
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _CurvatureHighlightIntensity;
		uniform sampler2D _CurvatureTex;
		uniform float4 _CurvatureTex_ST;
		uniform sampler2D _AOTex;
		uniform float4 _AOTex_ST;
		uniform sampler2D _AOSecondaryTex;
		uniform float4 _AOSecondaryTex_ST;
		uniform float _AOBlend;
		uniform float _AOAlbedoIntensity;
		uniform float _CurvatureCavityIntensity;
		uniform float4 _EmissiveColor;
		uniform float _Metalness;
		uniform float _Smoothness;


		inline float HighlightMask35( float input )
		{
			return saturate ((input - 0.5) * 2);
		}


		inline float CavityMask36( float input )
		{
			return saturate (input * 2);
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv2_MainTex = i.uv2_texcoord2 * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 temp_output_20_0 = ( _Color * tex2D( _MainTex, uv2_MainTex ) );
			float2 uv_CurvatureTex = i.uv_texcoord * _CurvatureTex_ST.xy + _CurvatureTex_ST.zw;
			float4 tex2DNode2 = tex2D( _CurvatureTex, uv_CurvatureTex );
			float input35 = tex2DNode2.r;
			float localHighlightMask35 = HighlightMask35( input35 );
			float4 lerpResult39 = lerp( temp_output_20_0 , ( temp_output_20_0 + temp_output_20_0 ) , ( _CurvatureHighlightIntensity * localHighlightMask35 ));
			float4 color10 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
			float2 uv_AOTex = i.uv_texcoord * _AOTex_ST.xy + _AOTex_ST.zw;
			float2 uv_AOSecondaryTex = i.uv_texcoord * _AOSecondaryTex_ST.xy + _AOSecondaryTex_ST.zw;
			float4 lerpResult46 = lerp( tex2D( _AOTex, uv_AOTex ) , tex2D( _AOSecondaryTex, uv_AOSecondaryTex ) , _AOBlend);
			float4 lerpResult9 = lerp( color10 , lerpResult46 , _AOAlbedoIntensity);
			float input36 = tex2DNode2.r;
			float localCavityMask36 = CavityMask36( input36 );
			float lerpResult42 = lerp( 1.0 , localCavityMask36 , _CurvatureCavityIntensity);
			o.Albedo = ( lerpResult39 * lerpResult9 * lerpResult42 ).rgb;
			o.Emission = _EmissiveColor.rgb;
			o.Metallic = _Metalness;
			o.Smoothness = _Smoothness;
			o.Occlusion = lerpResult9.r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16900
709;527;1650;852;2109.796;-346.5474;1;True;True
Node;AmplifyShaderEditor.ColorNode;19;-1508.061,-429.1517;Float;False;Property;_Color;Color;11;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0.0754717,0.0754717,0.0754717,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-1560.204,-258.6329;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;None;None;True;1;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-1558.355,31.49998;Float;True;Property;_CurvatureTex;CurvatureTex;3;1;[Gamma];Create;True;0;0;False;0;None;84ae15b39ca9fc94bb7b1a4d5324f0ba;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1597.556,549.8455;Float;True;Property;_AOTex;AOTex;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;44;-1598.456,739.2423;Float;True;Property;_AOSecondaryTex;AOSecondaryTex;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;45;-1579.724,933.9926;Float;False;Property;_AOBlend;AO Blend;5;0;Create;True;0;0;False;0;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;35;-1216.648,37.98683;Float;False;saturate ((input - 0.5) * 2);1;False;1;True;input;FLOAT;0;In;;Float;False;HighlightMask;True;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-1545.683,-54.03156;Float;False;Property;_CurvatureHighlightIntensity;Curvature Highlight Intensity;6;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1176.185,-277.062;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;36;-1211.208,111.6968;Float;False;saturate (input * 2);1;False;1;True;input;FLOAT;0;In;;Float;False;CavityMask;True;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-1003.488,-389.7316;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1311.072,469.9457;Float;False;Property;_AOAlbedoIntensity;AO Albedo Intensity;4;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-1318.202,282.3596;Float;False;Property;_CurvatureCavityIntensity;Curvature Cavity Intensity;7;0;Create;True;0;0;False;0;0.6;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1185.381,198.0826;Float;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;10;-1512.925,377.697;Float;False;Constant;_Color0;Color 0;3;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1027.349,-49.02176;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;46;-1194.185,555.7339;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;9;-907.8371,383.737;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;39;-731.0701,-412.1988;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;42;-813.2856,87.65157;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-403.5872,482.4932;Float;False;Property;_Metalness;Metalness;8;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-404.5872,559.4932;Float;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-345.4514,173.0647;Float;False;Property;_EmissiveColor;EmissiveColor;10;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-283.4004,51.52283;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;50.88317,271.36;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Hardsurface/MobileBase;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;35;0;2;0
WireConnection;20;0;19;0
WireConnection;20;1;6;0
WireConnection;36;0;2;0
WireConnection;38;0;20;0
WireConnection;38;1;20;0
WireConnection;40;0;22;0
WireConnection;40;1;35;0
WireConnection;46;0;1;0
WireConnection;46;1;44;0
WireConnection;46;2;45;0
WireConnection;9;0;10;0
WireConnection;9;1;46;0
WireConnection;9;2;11;0
WireConnection;39;0;20;0
WireConnection;39;1;38;0
WireConnection;39;2;40;0
WireConnection;42;0;43;0
WireConnection;42;1;36;0
WireConnection;42;2;41;0
WireConnection;12;0;39;0
WireConnection;12;1;9;0
WireConnection;12;2;42;0
WireConnection;0;0;12;0
WireConnection;0;2;15;0
WireConnection;0;3;13;0
WireConnection;0;4;14;0
WireConnection;0;5;9;0
ASEEND*/
//CHKSM=E9B1AD34BC3690AD9D4736FC8ECC1BB194846183