Shader "Hardsurface/Environment/Terrain (plainest)"
{
	Properties
	{
		_MainTex ("Side map", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}

		
		[Space (12)]
		_TintSide ("Side tint", Color) = (1, 1, 1, 1)
		_NormalIntensity ("Normal intensity", Range (0,1)) = 1.0
		_BumpHorizontal ("Normal map (horizontal)", 2D) = "bump" {}
		_NormalIntensityHorizontal ("Normal intensity", Range (0,1)) = 1.0
		_GlossinessMain ("Smoothness", Range (0,1)) = 0.0
		_BorderFactorA ("Border factor A", Float) = 0.5
		_BorderFactorB ("Border factor B", Float) = 1
		_Contrast ("Contrast", Float) = 1

		[Space (12)]
		_ParallaxStrength ("Parallax strength", Range (0, 1)) = 1
		_MultiplyAlbedoByHeight ("Multiply albedo by height", Range (0, 1)) = 1
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull Off
		LOD 200
	

		CGPROGRAM

		#pragma surface surf Standard addshadow vertex:vert finalgbuffer:ColorFunctionSliceShading
		#pragma target 5.0
        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Environment_Shared.cginc"
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _Bump;
		
		fixed4 _TintSide;
		half _NormalIntensity;
		sampler2D _BumpHorizontal;
		half _NormalIntensityHorizontal;
		half _GlossinessMain;
		float _BorderFactorA;
		float _BorderFactorB;
		float _DistBlendMin;
		float _DistBlendMax;
		half _ParallaxStrength;
		half _Contrast;

		void vert (inout appdata_full v, out Input o) 
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

            v.vertex.xyz *= scaleAndSpinProp.xyz;
            v.normal.xyz *= scaleAndSpinProp.xyz;
            v.tangent.xyz *= scaleAndSpinProp.xyz;

            o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
            o.worldNormal1 = UnityObjectToWorldNormal (v.normal);
            o.color = v.color;
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			ApplySliceCutoff (IN);
			
			float dist = saturate ((distance (_WorldSpaceCameraPos, IN.worldPos1) / _DistBlendMax) - _DistBlendMin);
            float2 uvScaled = IN.texcoord_uv1 * _MainTex_ST.xy + _MainTex_ST.zw;

			float4 asTerrain = float4 (0,0,0,0.5);
			float4 mseoTerrain = float4 (0,0,0,1);
			float4 normalTerrain = float4 (0,0,1,0.5);
			SampleTerrain (asTerrain, mseoTerrain, normalTerrain, IN.worldPos1); 

			float4 albedoVertical = tex2D (_MainTex, uvScaled) * _TintSide;
			// albedoVertical.xyz = Overlay (albedoVertical.xyz, albedoVertical.www);
			
			float verticalFactor = saturate (dot (IN.worldNormal1 * _BorderFactorA, float3 (0, 1, 0)) + _BorderFactorB * _BorderFactorA);
			verticalFactor *= lerp (normalTerrain.a, 1, pow (verticalFactor, 4));

			float curvMask = saturate ((albedoVertical.a - 0.5) * 2);	
			verticalFactor = lerp (verticalFactor, 0, curvMask);
			
            // verticalFactor = HeightBlend (albedoVertical.a, ahTerrain.a, verticalFactor, _Contrast);
            
			float4 albedoFinal = lerp (albedoVertical, asTerrain, verticalFactor);			
			half smoothnessFinal = lerp (0, mseoTerrain.y, verticalFactor);
			//float backsideFactor = GetBacksideFactor (IN.viewDir);
			
			fixed3 normalHorizontal = float3 (normalTerrain.x * 2 - 1, normalTerrain.y * 2 - 1, normalTerrain.z * 2 - 1);
			normalHorizontal = normalize (normalHorizontal);
			
			fixed3 normalVertical = UnpackNormal (tex2D (_Bump, uvScaled));

			float3 normalFinal = lerp (lerp (fixed3 (0, 0, 1), normalVertical, _NormalIntensity), lerp (fixed3 (0, 0, 1), normalHorizontal, _NormalIntensityHorizontal), verticalFactor);			
			normalFinal = normalize (normalFinal);
			//normalFinal = lerp (normalFinal, -normalFinal, backsideFactor);

			float metalnessFinal = 0;
			float3 normalFinalWorld = WorldNormalVector (IN, normalFinal);
			float verticalFactor5 = saturate (dot (normalFinalWorld, float3 (0, 1, 0)));
			verticalFactor5 = saturate (pow (verticalFactor5, 2));

			// frustratingly, absolutely everything related to this calculation gets compiled out if we don't pipe it into output
			smoothnessFinal = lerp (verticalFactor5, smoothnessFinal, 0.9999);
			
            _WeatherMultiplier = 1.0f;
			float weatherOcclusionMask = 1.0f;
			ApplyWeather (_WeatherMultiplier, albedoFinal.xyz, smoothnessFinal, metalnessFinal, normalFinal, IN.worldPos1, 1, verticalFactor5, verticalFactor5, weatherOcclusionMask);


			float3 emissionFinal = float3 (0, 0, 0);
            ApplyIsolines (albedoFinal.xyz, emissionFinal, IN.worldPos1, IN.worldNormal1);
			
			o.Albedo = albedoFinal;
			o.Metallic = metalnessFinal;
			o.Smoothness = smoothnessFinal;
			o.Emission = emissionFinal;
			o.Occlusion = 1;
			o.Normal = normalFinal;
		}
		ENDCG
	}
	FallBack "Diffuse"
}