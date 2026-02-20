float _ArrayOverrideIndex;
float _ArrayOverrideMode;

// Arrays can't be serialized, so you can't declare them in Properties block - we set them at runtime
float4 _ArrayForColorPrimary[31];
float4 _ArrayForColorSecondary[31];
float4 _ArrayForColorTertiary[31];
float4 _ArrayForSmoothnessPrimary[31];
float4 _ArrayForSmoothnessSecondary[31];
float4 _ArrayForSmoothnessTertiary[31];
float4 _ArrayForMetalness[31];
float4 _ArrayForEffect[31];
float4 _ArrayForDamage[31];
float4 _ArrayForSpecialContent[31];