float4 _Damage;

sampler2D _GlobalUnitDetailTex;
sampler2D _GlobalUnitDetailTexNew;
sampler2D _GlobalUnitDamageTex;
sampler2D _GlobalUnitRampTex;
sampler2D _GlobalUnitIridescenceTex;

sampler2D _GlobalUnitDamageTexNew;
sampler2D _GlobalUnitDamageTexNewSecondary;

float3 _GlobalUnitDetailOffset;
float3 _GlobalUnitDamageOffset; 

float _GlobalUnitDetailScale;
float _GlobalUnitDamageScale;
float _GlobalUnitRampSize;

float _GlobalEnvironmentRampScale;
float _GlobalEnvironmentRampInfluence;
float4 _GlobalSimulationTime;

inline float Dissolve (float noise, float damage, float subtract)
{
	return floor (min (saturate (noise - subtract), 0.99) + damage);
}

inline float Tighten (float input, float factor)
{
	return saturate (1 - (1 / (1 + pow (factor, (input - 0.5) * 12))));
}

inline float SelectValueFromVC (float4 vc, float4 values)
{
	return
	lerp
	(
		lerp
		(
			lerp
			(
				values.w,
				values.x,
				vc.x
			),
			values.y,
			vc.y
		),
		values.z,
		vc.z
	);
}

// Only areas marked with 0.5 on mask receive custom value
// areas with 0 mask value get 0 and areas with 1 mask value get 1

inline float SelectMaskedValueFromVC (float4 vc, float mask, float4 values)
{
    float maskLow = saturate (mask * 2);
    float maskHigh = saturate ((mask - 0.5) * 2);

    float result = lerp
    (
        lerp
        (
            lerp
            (
                values.w,
                values.x,
                vc.x
            ),
            values.y,
            vc.y
        ),
        values.z,
        vc.z
    );

    result = lerp (0, result, maskLow);
    result = lerp (result, 1, maskHigh);
    return result;
}

inline float3 SelectColorFromVC (float4 vc, float3 colorW, float3 colorX, float3 colorY, float3 colorZ)
{
	return
	lerp
	(
		lerp
		(
			lerp
			(
				colorW,
				colorX,
				vc.x
			),
			colorY,
			vc.y
		),
		colorZ,
		vc.z
	);
}

inline float RemapSmoothness (float smoothnessSample, float edgeMask, float min, float med, float max)
{
    float lowHalf = saturate (smoothnessSample * 2);
    float topHalf = saturate ((smoothnessSample - 0.5) * 2);
    float4 smoothnessRemapped = lerp
    (
        lerp
        (
            min,
            med,
            lowHalf
        ),
        max,
        topHalf * edgeMask
    );
    return smoothnessRemapped;
}

inline float RemapSmoothness (float smoothnessSample, float3 knots)
{
    float min = knots.x;
    float med = knots.y;
    float max = knots.z;
    float lowHalf = saturate (smoothnessSample * 2);
    float topHalf = saturate ((smoothnessSample - 0.5) * 2);
	
    float smoothnessRemapped = lerp
    (
        lerp
        (
            min,
            med,
            lowHalf
        ),
        max,
        topHalf
    );
	
    return smoothnessRemapped;
}

inline float2 GetUV (float3 pos, float3 normal, float scale)
{
	float xAboveY = normal.x >= normal.y;
	float xAboveZ = normal.x >= normal.z;
	float yAboveZ = normal.y > normal.z;

	float2 uvs =
	(
		xAboveY * pos.zy * xAboveZ +
		pos.xy * (xAboveZ == 0)
	) +
	(
		(xAboveY == 0) * pos.zx * yAboveZ +
		pos.xy * (yAboveZ == 0)
	);

	uvs /= scale;
	return uvs;
}

inline void ProcessDamageInput
(
    float4 damageInput,
    out float damageIntegrity,
    out float damageCriticalUnclamped,
    out float damageCriticalClamped,
    out float damageCriticalClampedSlow
)
{
    damageIntegrity = saturate (damageInput.x);

	// Damage input always progressing from 0 to 1 in fixed time during destruction
    damageCriticalUnclamped = saturate (damageInput.y);
	// Stores the limit for where critical destruction needs to stop at, such as 0.25 or 1.0, etc.
	float damageCriticalLimit = max (0.01, saturate (damageInput.w));

    // Clamped input that stops at the limit. Used to prevent full evaporation from happening
    damageCriticalClamped = clamp (damageCriticalUnclamped, 0, damageCriticalLimit);
    // Offset the progression start. This offset\slow down allows evaporation and vertex distortion to kick in later
    // That way we can avoid evaporation when damageCriticalLimit is below 1 (i.e. if you set the limit to 0.3 you will only see glow kick in and fade shortly after)
    float startingOffset = 0.3;
    damageCriticalClampedSlow = saturate (saturate (damageCriticalClamped - startingOffset) * (1 / (1 - startingOffset)));
    damageCriticalClampedSlow = clamp (damageCriticalClampedSlow, 0, damageCriticalLimit);

    // !! This needs investigation, changes damage progression to be nicer, but at damageCritical = 1 cuts away more surface area than is intended !!
	// Clamp the destruction by the limit to stop it at the right point. Results in a curve that reaches limit as quickly as full 0-1 animation
	/*float damageCriticalClamped = saturate (damageCriticalUnclamped * 1 / damageCriticalLimit);
	// Next we want to soften the arrival to the limit. To do that, we exponentially crush the inverse of the value
	damageCriticalClamped = 1 - damageCriticalClamped;
	damageCriticalClamped = pow (damageCriticalClamped, 4);
	damageCriticalClamped = 1 - damageCriticalClamped;
	// Finally, we need to remap 0-1 value we've been working with to 0 to limit range
	damageCriticalClamped *= damageCriticalLimit;*/
}

inline void ApplyDamageVert
(
    float4 damageInput,
    float3 vertex,
    float3 vertexPreSkinning,
    float3 normal,
    float4 destructionAreaPosition,
    float destructionAreaMultiplier,
    float destructionAreaAddition,
    float4 stripParameters,
    out float destructionProximity,
    out float3 vertexDistorted
)
{
    float nope;
    float damageCriticalClampedSlow;
    ProcessDamageInput (damageInput, nope, nope, nope, damageCriticalClampedSlow);

    // Destruction origin vectors. Destruction origin (destructionAreaPosition) is meant to stay intact the longest when the part evaporates
    float3 difference = float3
    (
        vertexPreSkinning.x - destructionAreaPosition.x,
        vertexPreSkinning.y - destructionAreaPosition.y,
        vertexPreSkinning.z - destructionAreaPosition.z
    );

    float3 differenceDirection = normalize(difference);

    float distance = sqrt
    (
        difference.x * difference.x +
        difference.y * difference.y +
        difference.z * difference.z
    );

    distance = saturate (distance * destructionAreaMultiplier + destructionAreaAddition);
    // Output distance into destruction proximity
    destructionProximity = distance;
    float destructionProximityInv = 1 - destructionProximity;

    float3 adjustedVertex = 0.0f;
    // Main vertex noisy distortion on evaportaion
    float vertexNoiseFrequency = 25.0f;
    adjustedVertex += destructionProximity * differenceDirection * frac (sin (vertexPreSkinning * vertexNoiseFrequency));

    // Additionally distort vertices by moving them in the strip direction
	float3 stripDirection = normalize (stripParameters.xyz);
	float stripIntensity = stripParameters.w;
    adjustedVertex += destructionProximity * stripDirection * stripIntensity;

    // Additionally 'inflate' the vertices of the mesh, originating from its pivot point
	float3 direction = normalize (vertex.xyz);
	adjustedVertex += destructionProximity * direction;

    adjustedVertex *= damageCriticalClampedSlow;

    vertexDistorted.xyz = vertex.xyz + adjustedVertex;
}

inline void ApplyDamage
(
    float4 damageInput,
    float4 damageSample,
    float4 damageSampleSecondary,
    float backsideFactor,
    float destructionProximity,
    float3 albedoClean,
    float albedoDamageMask,
    float albedoGrayscale,
    float smoothnessClean,
    float metalnessClean,
    float3 emissionClean,
    float3 normalClean,
    float albedoBrightnessCoef_optional, // Used on mechs: reduce damage fx based on albedo brightness value (makes feet have less emission etc.)
    float damageCriticalMaskedInverted_optional, // Used on tanks: damageCritical value with inverted 1-0 progression (can't invert it inside this function, because the value is multiplied by vcolor masks, it would lead to undesired results)
    out float3 albedoAfterDamage,
    out float smoothnessAfterDamage,
    out float metalnessAfterDamage,
    out float3 emissionAfterDamage,
    out float3 normalAfterDamage
)
{
    albedoAfterDamage = albedoClean;
    smoothnessAfterDamage = smoothnessClean;
    metalnessAfterDamage = metalnessClean;
    emissionAfterDamage = emissionClean;
    normalAfterDamage = normalClean;

    float damageIntegrity;
    float damageCriticalUnclamped;
    float damageCriticalClamped;
    float damageCriticalClampedSlow;
    ProcessDamageInput (damageInput, damageIntegrity, damageCriticalUnclamped, damageCriticalClamped, damageCriticalClampedSlow);

    // Increase damageIntegrity value along with damageCritical if damageIntegrity is below 1, this helps ensure
    // some of the tearing effects driven by damageIntegrity also get applied before the part fully evaporates
    float damageIntegrityInv = 1 - damageIntegrity;
    damageIntegrity += saturate (pow(damageCriticalClamped * 2, 2)) * damageIntegrityInv;

    float3 burnColor = float3 (15, 1.25, 0);

    if (damageIntegrity > 0.01f)
    {
        // Better naming for textures, convert raw RG channel data into a normalmap
        float4 damageTexture = damageSample;
        float4 damageTextureSecondary = damageSampleSecondary;
        float3 damageBulletholesNormal = float3 ((damageTextureSecondary.rg - 0.5) * 2, 1.0f);

        float damageScratchesProgression = saturate (damageIntegrity * 4);
        float damageBulletholesProgression = saturate (damageIntegrity * 2);
        // Add damageCritical to the equation, when the part evaporates it will continue the tearing effect
        float damageTearingProgression = saturate ((damageIntegrity - 0.4) * 2.5 + damageCriticalClamped);

        float damageLayerDirtSoft = pow (saturate (damageTexture.a * damageIntegrity * 8), 2);

        float damageLayerGrime = saturate ((damageTexture.g - (1 - damageScratchesProgression)) * 2 + damageLayerDirtSoft);
        float damageLayerBullets = saturate (saturate (pow (damageTexture.a, 2) * 8) - (1 - damageBulletholesProgression));

        float noisyScratchesMask = damageTextureSecondary.a;
        float damageLayerScratches = saturate (pow (saturate (saturate (damageTexture.g * 2) + saturate (damageTexture.a * 8)), 2) - (1 - damageScratchesProgression));
        float noisyScratches = saturate (noisyScratchesMask * damageLayerScratches);

        // Albedo area darkening for parts with tearing effect
        float tearingMask = saturate (damageTexture.g * (damageTearingProgression) * 3);
        // Tearing mask affecting albedo (exposed\torn metal kind of effect)
        float tearingMaskHighContrast = ContrastGrayscale (saturate(tearingMask * 2), 8);
        float tearingMaskAlbedo = saturate (tearingMaskHighContrast + noisyScratches) * albedoBrightnessCoef_optional;

        float tearingMaskAlbedoDarkEdges = (1 - pow (saturate (tearingMask * 2), 3)) * tearingMaskHighContrast;

        float albedoExposedMetal = albedoGrayscale * lerp (0.2f, 0.125f, tearingMaskAlbedoDarkEdges);
        albedoAfterDamage = lerp (albedoAfterDamage, albedoExposedMetal, tearingMaskAlbedo);

        // Reveal grime and reduce emission (not co2 one, sadly)
        albedoAfterDamage = lerp (albedoAfterDamage, albedoAfterDamage * damageTexture.r, damageLayerGrime * 0.5);
        emissionAfterDamage *= (1 - damageLayerGrime);

        // Reveal bulletholes on both albedo and normal
        albedoAfterDamage = lerp (albedoAfterDamage, Overlay(albedoAfterDamage, pow (damageTexture.b * 1.5, 2)), damageLayerBullets);
        // !!! NOTE: Make sure to include Utilities_Shared.cginc before this cginc file to make NormalBlend work !!!
        normalAfterDamage = lerp (normalClean, NormalBlend(normalClean, damageBulletholesNormal), damageLayerBullets);

    	// Make big pockmarks and scratches lit up at high integrity damage
        float pockmarksMask = pow ((1 - damageTexture.b) * damageTexture.a, 3);
        emissionAfterDamage += burnColor * (pockmarksMask * 1.5) * pow (saturate (damageIntegrity), 6) * albedoBrightnessCoef_optional;
        
        // Alphatest clipping
        float damageLayerTearing = saturate ((1 - damageTexture.g) * 1.5) - (damageTearingProgression);
        float edgesMask = saturate (((albedoDamageMask - 0.01f) * 64));
        clip (damageLayerTearing + damageTextureSecondary.b * 4 + edgesMask);

    	// Useful for debugging the input mask, just disable clipping and enable this line
    	//emissionAfterDamage = destructionProximity;
    
        if (damageCriticalClamped > 0.01f)
        {
            float burnDestruction = lerp (damageCriticalClamped, saturate (damageCriticalClamped * 3), damageCriticalClamped);
            float burnFactor = (1 - damageTexture.a) - burnDestruction - (1 - damageTextureSecondary.b) * damageTexture.r * damageCriticalClamped;

            clip (burnFactor);
            if (burnFactor < 0.2)
            {
                float rampSize = _GlobalUnitRampSize;
                float3 burnRamp = tex2D (_GlobalUnitRampTex, float2 (burnFactor * (1 / rampSize), 0)) * 1;
                float3 burnEmission = burnRamp * (burnColor * 0.5);
                
                // Paint peeling on critical damage
                float albedoStripMask = pow (1 - burnRamp.r, 4);
                float burntAlbedoDarkness = 0.15f;
                albedoAfterDamage = lerp (albedoAfterDamage * burntAlbedoDarkness, albedoAfterDamage, albedoStripMask);
                
                emissionAfterDamage += ((destructionProximity * 4) + 0.25) * burnEmission;
            }
        }

        // Calculate a multiplier expressing "as progress approaches 100%, animate from 1x at 80% to 0x at 100%)
        float burnFadeMultiplier = 1 - saturate ((damageCriticalUnclamped - 0.8) * 5);
        // Optional damageCritical inverted mask for tanks. Using it allows us to actually fade emission on vehicles\turrets
        damageCriticalMaskedInverted_optional = saturate (damageCriticalMaskedInverted_optional * 4);
        // Distort that multiplier exponentially (linear multiplier looks bad with a bright HDR color, has no effect then cuts abruptly)
        burnFadeMultiplier = pow (burnFadeMultiplier, 4) * damageCriticalMaskedInverted_optional;

        // Inside faces get a special albedo texture based on smoothness
        float3 albedoDamagedInside = smoothnessClean * 0.1f;
        albedoAfterDamage = lerp (albedoDamagedInside, albedoAfterDamage, backsideFactor);

        emissionAfterDamage *= burnFadeMultiplier;
        smoothnessAfterDamage *= burnFadeMultiplier;
        // Darken albedo at the end of destruction (useful when critical destruction limit is below 1)
        albedoAfterDamage *= saturate (burnFadeMultiplier + 0.5);

        // Remove damage effects from inside of the part
        smoothnessAfterDamage *= backsideFactor;
        metalnessAfterDamage *= backsideFactor;
    }
}