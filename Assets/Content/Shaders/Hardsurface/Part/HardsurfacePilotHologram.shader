Shader "Hardsurface/PilotHologram" 
{
	Properties
	{
		_MainTex ("Attribute map", 2D) = "gray" {}
		_FilterColor ("Filter color", Color) = (1,1,1,0)
		_FilterInputs ("Filter inputs", Vector) = (0, 0, 0, 0)
		
		_ScanTex ("Scan map", 2D) = "gray" {}
		_GlitchTex ("Glitch map", 2D) = "black" {}
		_GlitchFactors ("Glitch factors", Vector) = (1, 1, 1, 1)
		_ScanColorMin ("Scan color (min)", Color) = (0.25,0.25,0.75,1)
		_ScanColorMax ("Scan color (max)", Color) = (1,1,1,1)
		_ScanColorIntensity ("Scan color intensity", Range (1, 8)) = 1
		_ScanFade ("Scan fade", Range (0, 1)) = 1
		_ScanSize ("Scan size", Range (64, 512)) = 256
		_ScanTimeFactors ("Scan time factors", Vector) = (1, 1, 1, 1)
		_FadePosition ("Fade position (XYZ)", Vector) = (0, 0, 0, 1)
		_FadeParameters ("Fade intensity (X), add. (Z), mul. (W)", Vector) = (0, 0, 0, 1)
		_Visibility ("Visibility", Range (0, 1)) = 1
		_CutoffFactors ("Cutoff (influence, offset)", Vector) = (0, 0, 0, 0)

        _AttributeInfluence ("Attribute map influence", Range (0, 1)) = 1
        _GlitchDisplacementIntensity ("Glitch Displacement Intensity", Float) = 0
        _GlitchSpeed ("Glitch Speed", Range (0, 50)) = 1.0
        _VertexGridDensity ("Vertex Grid Density (world)", Range (0, 1)) = 1.0
		_VertexGridDensityLocal ("Vertex Grid Density (local)", Range(0, 1)) = 0.0
        _RimColorMin ("Rim Color (min)", Color) = (1,1,1,1)
        _RimColorMax ("Rim Color (max)", Color) = (1,1,1,1)
        _RimShiftForColor ("Rim Shift (color)", Range (-0.5, 0.5)) = 0
        _RimPowerForColor ("Rim Power (color)", Range (0.1, 10)) = 5.0
        _RimShiftForOpacity ("Rim Shift (opacity)", Range (-0.5, 0.5)) = 0
		_RimPowerForOpacity ("Rim Power (opacity)", Range(0.1, 10)) = 5.0

		[Toggle(USE_CAMERAOFFSET)] _CamOffset("Use camera offset and extrusion", Int) = 0
		[HideIfDisabled(USE_CAMERAOFFSET)] _CamOffsetCoef ("Camera Space Z Offset", Range (-10.0, 10.0)) = 0.0
		[HideIfDisabled(USE_CAMERAOFFSET)] _ExtrusionCoef ("Vertex Extrusion Amount", Range (0.0, 0.25)) = 0.0
	}

    SubShader 
	{
        Tags 
		{
			"RenderType" = "Opaque"
			"ForceNoShadowCasting" = "True"
		}

		LOD 200
		// Blend One Zero
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert noshadow addshadow alpha:fade
		#pragma target 5.0
		#pragma shader_feature_local USE_CAMERAOFFSET

		#include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
		#include "Cginc/HardsurfacePartFunctions.cginc"

		struct Input 
		{
			float2 uv_MainTex;
			float4 screenPos;
			float3 viewDir;
            float3 worldNormal1;
            float3 worldPos1;
			float fadeFactor;
		};

		uniform sampler2D _MainTex;
		uniform sampler2D _ScanTex;
		uniform sampler2D _GlitchTex;
		uniform float4 _GlitchFactors;
        uniform float _GlitchSpeed;
        uniform float _GlitchDisplacementIntensity;
	    uniform fixed4 _ScanColorMin;
		uniform fixed4 _ScanColorMax;
		uniform float _ScanColorIntensity;
		uniform float _ScanFade;
		uniform float _ScanSize;
		uniform float4 _ScanTimeFactors;
		uniform float4 _FadePosition;
		uniform float4 _FadeParameters;
		uniform float _Visibility;
		uniform float _AttributeInfluence;
		uniform float4 _CutoffFactors;

		uniform float4 _FilterColor;
		uniform float4 _FilterInputs;

		#ifdef USE_CAMERAOFFSET
			uniform float _CamOffsetCoef;
			uniform float _ExtrusionCoef;
		#endif

        float _VertexGridDensity;
		float _VertexGridDensityLocal;
        fixed4 _RimColorMin;
        fixed4 _RimColorMax;
        float _RimShiftForColor;
        float _RimPowerForColor;
        float _RimShiftForOpacity;
        float _RimPowerForOpacity;
		
		float4 _GlobalUnscaledTime;

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);
			float3 difference = float3
			(
				v.vertex.x - _FadePosition.x,
				v.vertex.y - _FadePosition.y,
				v.vertex.z - _FadePosition.z
			);

			float distance = sqrt
			(
				difference.x * difference.x +
				difference.y * difference.y +
				difference.z * difference.z
			);

			distance = saturate (distance * lerp (_FadeParameters.w * 10, _FadeParameters.w, _Visibility) + _FadeParameters.z);
			distance = lerp (0, distance, _FadeParameters.x);
			o.fadeFactor = 1 - distance;
            o.worldPos1 = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
            o.worldNormal1 = UnityObjectToWorldNormal (v.normal);

            if (_VertexGridDensity > 0)
            {
                float3 wp = o.worldPos1;
                wp.x -= wp.x % _VertexGridDensity;
                wp.y -= wp.y % _VertexGridDensity;
                wp.z -= wp.z % _VertexGridDensity;
                v.vertex.xyz = mul (unity_WorldToObject, float4 (wp.xyz, 1)).xyz;
            }

			if (_VertexGridDensityLocal > 0)
			{
				float3 localPos = v.vertex.xyz;
				localPos.x -= localPos.x % _VertexGridDensityLocal;
				localPos.y -= localPos.y % _VertexGridDensityLocal;
				localPos.z -= localPos.z % _VertexGridDensityLocal;
				v.vertex.xyz = localPos.xyz;
			}

			#ifdef USE_CAMERAOFFSET
				v.vertex.xyz += v.normal * _ExtrusionCoef;
				float3 viewForward = mul(unity_WorldToObject, unity_CameraToWorld._m02_m12_m22);
				v.vertex.xyz += viewForward * _CamOffsetCoef;
			#endif

            float glitch = (step (0.5, sin (_GlobalUnscaledTime.y * 2.0 + v.vertex.y * 1.0)) * step (0.99, sin (_GlobalUnscaledTime.y * _GlitchSpeed * 0.5)));
            float3 offset = (v.normal + frac (sin (dot (v.vertex.xz, float2 (12.9898, 78.233))) * 43758.5453)) * glitch * _GlitchDisplacementIntensity;
            v.vertex.xyz += offset * _FilterInputs.z;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
			screenPos *= _ScreenParams.xy; // pixel position

			float2 screenUV = screenPos.xy;
			float sinCompressed8 = ((_SinTime.x + 1) / 2);
			float sinCompressed4 = ((_SinTime.y + 1) / 2);
			float sinCompressed2 = ((_SinTime.z + 1) / 2);
			float sinCompressed = ((_SinTime.w + 1) / 2);

			float sinTest = sinCompressed;

			// screenUV.y += _SinTime.w * _ScanTimeFactors.x;
			// screenUV.y += lerp (0, screenUV.x * _ScanTimeFactors.y * sinCompressed4 * 1.3213, round (sinCompressed));

			screenUV /= _ScanSize.xx;
			half4 glitchNormal = tex2D (_GlitchTex, screenUV * _GlitchFactors.x);
			float random = frac (sin (dot ((glitchNormal.xy - 0.5) * sinCompressed4, float2 (12.9898, 78.233))) * 43758.5453);
			
			screenUV.xy += (glitchNormal.xy - 0.5) * sinCompressed * _GlitchFactors.y;
			screenUV.xy += (glitchNormal.xy) * random * _GlitchFactors.z;
			screenUV.y += _GlobalUnscaledTime.w * _ScanTimeFactors.x;

			float2 uvMain = IN.uv_MainTex;

			// float glitch = (step (0.5, sin (_GlobalUnscaledTime.z * 2 + uvMain.y * 1.0)) * step (0.99, sin (_GlobalUnscaledTime.z * _GlitchSpeed * 0.5)));
            float glitch = (step (0.5, sin (_GlobalUnscaledTime.x * 2 + uvMain.y)) * step (0.1, sin (_GlobalUnscaledTime.x * _GlitchSpeed * 0.5)));
			float3 offset = (frac (sin (dot (uvMain, float2 (12.9898, 78.233))) * 43758.5453)) * glitch * _GlitchDisplacementIntensity;
			
			uvMain.xy += (glitchNormal.xy - 0.5) * offset * _GlitchFactors.y * _FilterInputs.z;
			uvMain.xy += (glitchNormal.xy) * offset * _GlitchFactors.z * _FilterInputs.z;

			float4 maintex = lerp (tex2D (_MainTex, uvMain), float4 (0.5, 0.5, 0.5, 1), _AttributeInfluence);
			maintex.rgb = lerp (maintex.rgb, Overlay (RGBToGrayscale(maintex.rgb).xxx, _FilterColor.rgb), _FilterColor.a);
			
			fixed4 scan = tex2D (_ScanTex, screenUV);

            half rimForOpacity = 1.0 - pow (saturate (dot (IN.viewDir, IN.worldNormal1) + _RimShiftForOpacity), _RimPowerForOpacity);
            half rimForColor = 1.0 - pow (saturate (dot (IN.viewDir, IN.worldNormal1) + _RimShiftForColor), _RimPowerForColor);
            float3 colorMain = lerp (_ScanColorMin, _ScanColorMax, maintex.x);
            float3 colorRim = lerp (_RimColorMin, _RimColorMax, maintex.x);
            float3 colorCombined = maintex.xyz * lerp (colorMain, colorRim, rimForColor);

			float3 albedoFinal = maintex.xyz;
			float3 emissionFinal = colorCombined * lerp (1, _ScanColorIntensity, _FilterInputs.y); // lerp (float3 (0, 0, 0), _ColorScan, saturate ((attributes.x - 0.5) * 2));
			
			float visibility = _Visibility;
			visibility = 1 - visibility;
			visibility = pow (visibility, 4);
			visibility = 1 - visibility;

			float scanIntensityExternal = saturate (_FilterInputs.x);
			float alphaFinal = lerp (1, scan.w, _ScanFade * scanIntensityExternal) * IN.fadeFactor * visibility * rimForOpacity * maintex.w;

			// Translate alpha to darkening if we disable transparency
			emissionFinal = lerp (emissionFinal * lerp (1, scan.w, _FilterInputs.z), emissionFinal, _FilterInputs.w);

			// Restore alpha to 1 if we disable transparency
			alphaFinal = lerp (1, alphaFinal, _FilterInputs.w);

			o.Albedo = 0;
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Occlusion = 0;
			o.Emission = emissionFinal * 2;
			o.Alpha = alphaFinal;

			if (_FilterInputs.w > 0.01)
			{
				float4x4 thresholdMatrix =
			   { 1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
				   13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
				   4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
				   16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
			   };
				float4x4 rowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };

				float clipFactor = alphaFinal * _ScanColorMin.w - thresholdMatrix[fmod (screenPos.x, 4)] * rowAccess[fmod (screenPos.y, 4)] - _CutoffFactors.y;
				float clipAlpha = clipFactor <= 0 ? 0 : 1;
				clipAlpha = lerp (clipAlpha, 1, saturate (1 - _CutoffFactors.x) * scan.w);
			
				float alphaModified = lerp (alphaFinal * clipAlpha, clipAlpha, saturate (_CutoffFactors.w));
				o.Alpha = alphaModified;
			}
		}

		ENDCG

        

	}

	FallBack "Diffuse"
}
