Shader "Hardsurface/Environment/Glass (Instanced)" 
{
	Properties 
	{
	    _InstancePropsOverride ("Instanced properties override", Range (0, 1)) = 0
		_PackedPropData ("Visibility, explosion, compression, integrity", Vector) = (1, 0, 0, 1)
		_Opacity ("Opacity", Range (0, 1)) = 0.5
		_MainTex ("AH", 2D) = "white" {}
		_MSEO ("MSEO", 2D) = "white" {}
		_Bump ("Normal map", 2D) = "bump" {}
		_SmoothnessMin ("SmoothnessMin", Range (0, 1)) = 0.0
		_SmoothnessMed ("SmoothnessMed", Range (0, 1)) = 0.2
		_SmoothnessMax ("SmoothnessMax", Range (0, 1)) = 0.8
		_Scale ("Scale", Vector) = (1, 1, 1, 1)

		[Toggle (_USE_CAR_PARTS_ROTATION)]
		_UseCarPartsRotation ("Use Car Parts Rotation", Float) = 0
		[HideIfDisabled(_USE_CAR_PARTS_ROTATION)] _CarPartsRotationMin ("Car Parts Rotation Angles Min", Vector) = (-30, 0, 0, 0)
		[HideIfDisabled(_USE_CAR_PARTS_ROTATION)] _CarPartsRotationMax ("Car Parts Rotation Angles Max", Vector) = (30, -20, 20, 0)
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "Transparent"
			"RenderType" = "Transparent" 
		}
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard vertex:SharedVertexFunctionProp alpha:blend noshadow finalgbuffer:ColorFunctionSliceShading
		#pragma target 5.0
		#pragma instancing_options procedural:setup
        #pragma shader_feature_local _USE_CAR_PARTS_ROTATION

        #include "Assets/Content/Shaders/Other/Utilities_Shared.cginc"
        #include "Prop_Shared.cginc"
        
        fixed _Opacity;

		void surf (Input IN, inout SurfaceOutputStandard output) 
		{
			ApplySliceCutoff (IN);
			
		    float visibility = IN.packedPropData.x;
            float explosion = IN.packedPropData.y;
			float integrity = IN.packedPropData.w;

			if (_InstancePropsOverride > 0.0)
			{
				visibility = _PackedPropData.x;
				explosion = _PackedPropData.y;
				integrity = _PackedPropData.w;
			}

            if (explosion > 0.99)
            {
                clip (-1);
            }
            else
            {
				fixed4 ah = tex2D (_MainTex, IN.uv_MainTex);
                fixed4 mseo = tex2D (_MSEO, IN.uv_MainTex);
                fixed3 nrm = UnpackNormal (tex2D (_Bump, IN.uv_MainTex));
                
                fixed3 albedoBase = ah.rgb;
                fixed albedoMask = ah.a;
                fixed metalness = mseo.x;
                fixed smoothness = mseo.y;
                fixed emission = mseo.z;
                fixed occlusion = mseo.w;
                fixed3 normalFinal = lerp (fixed3 (0, 0, 1), nrm, _NormalIntensity);
                
                float smoothnessMaskMinToMed = saturate (smoothness * 2);
                float smoothnessMaskMedToMax = saturate (smoothness - 0.5) * 2;
                float smoothnessFinal = lerp (lerp (_SmoothnessMin, _SmoothnessMed, smoothnessMaskMinToMed), _SmoothnessMax, smoothnessMaskMedToMax);
                
                float opacityFinal = _Opacity * occlusion;
                float metalnessFinal = metalness * occlusion;
                
                output.Albedo = albedoBase;
                output.Metallic = metalnessFinal;
                output.Smoothness = smoothnessFinal;
                output.Emission = 0;
                output.Occlusion = 1;
                output.Alpha = opacityFinal;
                
                float opacity = saturate (visibility * 1.001) * (1 - explosion);
                float2 screenPosPixel = (IN.screenPos.xy / IN.screenPos.w) * _ScreenParams.xy;
                clip (opacity - thresholdMatrix[fmod (screenPosPixel.x, 4)] * rowAccess[fmod (screenPosPixel.y, 4)]);
            }
		}
		ENDCG
	}
}