//
// A physically based shader with triplanar mapping
//
Shader "Hardsurface/Environment/Triplanar Split"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTexFloor ("Albedo (floor)", 2D) = "white" {}
		_MainTexCeiling ("Albedo (ceiling)", 2D) = "white" {}
		_MainTexVertical ("Albedo (vertical)", 2D) = "white" {}

		_SmoothnessMinFloor ("Smoothness min. (floor)", Range (0.0, 1.0)) = 0
		_SmoothnessMedFloor ("Smoothness med. (floor)", Range (0.0, 1.0)) = 0.25
		_SmoothnessMaxFloor ("Smoothness max. (floor)", Range (0.0, 1.0)) = 0.5

		_SmoothnessMinCeiling ("Smoothness min. (ceiling)", Range (0.0, 1.0)) = 0
		_SmoothnessMedCeiling ("Smoothness med. (ceiling)", Range (0.0, 1.0)) = 0.25
		_SmoothnessMaxCeiling ("Smoothness max. (ceiling)", Range (0.0, 1.0)) = 0.5

		_SmoothnessMinVertical ("Smoothness min. (vertical)", Range (0.0, 1.0)) = 0
		_SmoothnessMedVertical ("Smoothness med. (vertical)", Range (0.0, 1.0)) = 0.25
		_SmoothnessMaxVertical ("Smoothness max. (vertical)", Range (0.0, 1.0)) = 0.5

        [Gamma] _Metallic ("Metalness", Range (0.0, 1.0)) = 0.0

		_NormalTexFloor ("Normal (floor)", 2D) = "bump" {}
		_NormalTexCeiling ("Normal (ceiling)", 2D) = "bump" {}
		_NormalTexVertical ("Normal (vertical)", 2D) = "bump" {}

        _MapScale("Mapping scale", Float) = 1.0
		_PlaneOffsetFactor ("Plane offset factor", Float) = 1.0
		_AlbedoToOcclusion ("Albedo to AO", Range (0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags 
		{ 
			"RenderType" = "Opaque" 
		}
        
        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows
        #pragma target 3.0

        half4 _Color;
        sampler2D _MainTexFloor;
		sampler2D _MainTexCeiling;
		sampler2D _MainTexVertical;

		half _SmoothnessMinFloor;
		half _SmoothnessMedFloor;
		half _SmoothnessMaxFloor;

		half _SmoothnessMinCeiling;
		half _SmoothnessMedCeiling;
		half _SmoothnessMaxCeiling;

		half _SmoothnessMinVertical;
		half _SmoothnessMedVertical;
		half _SmoothnessMaxVertical;

        half _Metallic;
        sampler2D _NormalTexFloor;
		sampler2D _NormalTexCeiling;
		sampler2D _NormalTexVertical;

        half _MapScale;
		half _PlaneOffsetFactor;
		half _AlbedoToOcclusion;

        struct Input 
		{
            float3 localCoord;
            float3 localNormal;
			float4 vertexColor : COLOR;
        };

        void vert (inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.localCoord = v.vertex.xyz;
            data.localNormal = v.normal.xyz;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate a blend factor for triplanar mapping.
            float3 surfaceDirection = normalize (abs (IN.localNormal));
			float surfaceDirectionTest = dot (surfaceDirection, (float3)1);
			float surfaceDirectionUp = saturate (dot (IN.localNormal, float3 (0, 1, 0)));
			surfaceDirection /= surfaceDirectionTest;

            // Get texture coordinates.
			float2 coordsX = IN.localCoord.zy * _MapScale;
			float2 coordsY = IN.localCoord.zx * _MapScale;
			float2 coordsZ = IN.localCoord.xy * _MapScale;

            // Albedo
            half4 colorX = tex2D (_MainTexVertical, coordsX) * surfaceDirection.x;
            half4 colorY = lerp (tex2D (_MainTexCeiling, coordsY), tex2D (_MainTexFloor, coordsY), surfaceDirectionUp) * surfaceDirection.y;
            half4 colorZ = tex2D (_MainTexVertical, coordsZ) * surfaceDirection.z;

            half4 color = (colorX + colorY + colorZ);
            o.Albedo = color.rgb * _Color * IN.vertexColor.a;

			//Smoothness
			float smoothnessMin = _SmoothnessMinVertical * surfaceDirection.x + _SmoothnessMinVertical * surfaceDirection.z + lerp (_SmoothnessMinCeiling, _SmoothnessMinFloor, surfaceDirectionUp) * surfaceDirection.y;
			float smoothnessMed = _SmoothnessMedVertical * surfaceDirection.x + _SmoothnessMedVertical * surfaceDirection.z + lerp (_SmoothnessMedCeiling, _SmoothnessMedFloor, surfaceDirectionUp) * surfaceDirection.y;
			float smoothnessMax = _SmoothnessMaxVertical * surfaceDirection.x + _SmoothnessMaxVertical * surfaceDirection.z + lerp (_SmoothnessMaxCeiling, _SmoothnessMaxFloor, surfaceDirectionUp) * surfaceDirection.y;
			float smoothnessMaskMinToMed = saturate (color.a * 2);
			float smoothnessMaskMedToMax = saturate (color.a - 0.5) * 2;
			float smoothnessFinal = lerp (lerp (smoothnessMin, smoothnessMed, smoothnessMaskMinToMed), smoothnessMax, smoothnessMaskMedToMax) * IN.vertexColor.a;
			o.Smoothness = smoothnessFinal;

            // Normal map
            half4 normalX = tex2D (_NormalTexVertical, coordsX) * surfaceDirection.x;
            half4 normalY = lerp (tex2D (_NormalTexCeiling, coordsY), tex2D (_NormalTexFloor, coordsY), surfaceDirectionUp) * surfaceDirection.y;
            half4 normalZ = tex2D (_NormalTexVertical, coordsZ) * surfaceDirection.z;
            o.Normal = UnpackNormal (normalX + normalY + normalZ);

			// Other parameters
            o.Metallic = _Metallic;
			o.Occlusion = lerp (1, o.Albedo, _AlbedoToOcclusion);

        }
        ENDCG
    } 
    FallBack "Diffuse"
}
