float _ArrayOverrideIndex;
float _ArrayOverrideMode;

// Arrays can't be serialized, so you can't declare them in Properties block - we set them at runtime
float4 _ArrayForColorPrimary[30];
float4 _ArrayForColorSecondary[30];
float4 _ArrayForColorTertiary[30];
float4 _ArrayForSmoothnessPrimary[30];
float4 _ArrayForSmoothnessSecondary[30];
float4 _ArrayForSmoothnessTertiary[30];
float4 _ArrayForMetalness[30];
float4 _ArrayForEffect[30];
float4 _ArrayForDamage[30];