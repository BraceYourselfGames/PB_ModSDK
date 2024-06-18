Shader "Hardsurface/Environment/Terrain (Shadow)"
{
	Properties
	{
	    _Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Side map", 2D) = "white" {}
		_SedimentTex ("Sediment map", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}
	    _NormalIntensity ("Normal intensity", Range (0,1)) = 1.0
		_BumpHorizontal ("Normal map (horizontal)", 2D) = "bump" {}
		_NormalIntensityHorizontal ("Normal intensity (hor.)", Range (0,1)) = 1.0
		_GlossinessMain ("Smoothness", Range (0,1)) = 0.0
		_BorderFactorA ("Border factor A", Float) = 15
		_BorderFactorB ("Border factor B", Float) = -0.8
		_BorderFactors ("Border factors", Vector) = (15, 15, -0.8, -0.8)
		_TexScaleSediment ("Texture scale (sediment)", Float) = 1
		_BrightnessMultiplier ("Brightness multiplier", Range (1, 1.3)) = 1
		
		_BorderMaskTex ("BorderMask", 2D) = "white" {}
		_BorderMaskScale ("Texture Scale", Float) = 1

	    [Space (12)]
		_DistBlendMin ("Distance Blend Begin", Float) = 0
		_DistBlendMax ("Distance Blend Max", Float) = 100
		_DistUVScale1 ("Distance UV Scale", Float) = 0.5
		_DistUVScale2 ("Distance UV Scale", Float) = 0.5
		_DistUVScale3 ("Distance UV Scale", Float) = 0.5
		_DistUVScale4 ("Distance UV Scale", Float) = 0.5

		[Space (12)]
		_TexSplat ("Splat map", 2D) = "white" {}
		_TexSplatScale ("Splat scale", Float) = 10
		_ParallaxStrength ("Parallax strength", Range (0, 1)) = 1
		_MultiplyAlbedoByHeight ("Multiply albedo by height", Range (0, 1)) = 1
		_CurvatureSettings ("Curvature power/blend (ridge/cavity)", Vector) = (1, 1, 1, 1)

		[Space (12)]
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
			"Queue" = "Geometry+1"
		}
        Cull Back
        ZTest Less
	

		CGPROGRAM

		#pragma surface surf Standard vertex:SharedShadowVertexFunctionNoDamage addshadow
		#pragma target 5.0
		#pragma only_renderers d3d11 d3d11_9x
		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Environment_Shared.cginc"
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
		

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = half3(0,0,0);
		}
		ENDCG
	}
	FallBack "Diffuse"
}