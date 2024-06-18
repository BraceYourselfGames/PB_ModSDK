using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AFSManagerFallback : MonoBehaviour
{
    private static int propertyID_WindPID = Shader.PropertyToID("_Wind");
    private static int propertyID_AfsTreeWindMuliplierPID = Shader.PropertyToID("_AfsTreeWindMuliplier");
    private static int propertyID_AfsFoliageWindPID = Shader.PropertyToID("_AfsFoliageWind");
    private static int propertyID_AfsFoliageWindPulsMagnitudeFrequencyPID = Shader.PropertyToID("_AfsFoliageWindPulsMagnitudeFrequency");
    private static int propertyID_AfsFoliageWindMultiplierPID = Shader.PropertyToID("_AfsFoliageWindMultiplier");
    
    private static int propertyID_AfsFoliageTimeFrequencyPID = Shader.PropertyToID("_AfsFoliageTimeFrequency");
    private static int propertyID_AfsFoliageWaveSizePID = Shader.PropertyToID("_AfsFoliageWaveSize");
    private static int propertyID_AfsVertexLitTranslucencyPID = Shader.PropertyToID("_AfsVertexLitTranslucency");
    private static int propertyID_AfsVertexLitViewDependencyPID = Shader.PropertyToID("_AfsVertexLitViewDependency");
    private static int propertyID_AfsVertexLitVariationPID = Shader.PropertyToID("_AfsVertexLitVariation");
    private static int propertyID_AfsVertexLitHorizonFadePID = Shader.PropertyToID("_AfsVertexLitHorizonFade");
    
    private static int propertyID_AfsWindJitterVariationScalePID = Shader.PropertyToID("_AfsWindJitterVariationScale");
    private static int propertyID_AfsGrassWindPID = Shader.PropertyToID("_AfsGrassWind");
    private static int propertyID_AfsWaveAndDistancePID = Shader.PropertyToID("_AfsWaveAndDistance");
    private static int propertyID_AfsWindRotationPID = Shader.PropertyToID("_AfsWindRotation");

    private static int propertyID_AfsRainamountPID = Shader.PropertyToID("_AfsRainamount");
    private static int propertyID_AfsWavingTintPID = Shader.PropertyToID("_AfsWavingTint");
    private static int propertyID_AfsTreeColorPID = Shader.PropertyToID("_AfsTreeColor");
    private static int propertyID_AfsTerrainTreesPID = Shader.PropertyToID("_AfsTerrainTrees");
    private static int propertyID_AfsBillboardCameraForwardPID = Shader.PropertyToID("_AfsBillboardCameraForward");
    private static int propertyID_AfsBillboardBorderPID = Shader.PropertyToID("_AfsBillboardBorder");

    private static int propertyID_afs_SHAr = Shader.PropertyToID("afs_SHAr");
    private static int propertyID_afs_SHAg = Shader.PropertyToID("afs_SHAg");
    private static int propertyID_afs_SHAb = Shader.PropertyToID("afs_SHAb");
    private static int propertyID_afs_SHBr = Shader.PropertyToID("afs_SHBr");
    private static int propertyID_afs_SHBg = Shader.PropertyToID("afs_SHBg");
    private static int propertyID_afs_SHBb = Shader.PropertyToID("afs_SHBb");
    private static int propertyID_afs_SHC = Shader.PropertyToID("afs_SHC");

    [Space (8f)]
    public Vector4 value_WindPID;
    public Vector4 value_AfsTreeWindMuliplierPID;
    public Vector4 value_AfsFoliageWindPID;
    public Vector4 value_AfsFoliageWindPulsMagnitudeFrequencyPID;
    public Vector4 value_AfsFoliageWindMultiplierPID;
    
    [Space (8f)]
    public Vector4 value_AfsFoliageTimeFrequencyPID;
    public Vector4 value_AfsFoliageWaveSizePID;
    public float value_AfsVertexLitTranslucencyPID;
    public float value_AfsVertexLitViewDependencyPID;
    public float value_AfsVertexLitVariationPID;
    public float value_AfsVertexLitHorizonFadePID;
    
    [Space (8f)]
    public Vector4 value_AfsWindJitterVariationScalePID;
    public Vector4 value_AfsGrassWindPID;
    public Vector4 value_AfsWaveAndDistancePID;
    public Quaternion value_AfsWindRotationPID;
    
    [Space (8f)]
    public float value_AfsRainamountPID;
    public Color value_AfsWavingTintPID;
    public Color value_AfsTreeColorPID;
    public Vector4 value_AfsTerrainTreesPID;
    public Vector4 value_AfsBillboardCameraForwardPID;
    public float value_AfsBillboardBorderPID;
    
    [Space (8f)]
    public Vector4 value_afs_SHAr;
    public Vector4 value_afs_SHAg;
    public Vector4 value_afs_SHAb;
    public Vector4 value_afs_SHBr;
    public Vector4 value_afs_SHBg;
    public Vector4 value_afs_SHBb;
    public Vector4 value_afs_SHC;

    [Button, ButtonGroup, PropertyOrder (-1)]
    private void ReadValues ()
    {
        value_WindPID = Shader.GetGlobalVector (propertyID_WindPID);
        value_AfsTreeWindMuliplierPID = Shader.GetGlobalVector (propertyID_AfsTreeWindMuliplierPID);
        value_AfsFoliageWindPID = Shader.GetGlobalVector (propertyID_AfsFoliageWindPID);
        value_AfsFoliageWindPulsMagnitudeFrequencyPID = Shader.GetGlobalVector (propertyID_AfsFoliageWindPulsMagnitudeFrequencyPID);
        value_AfsFoliageWindMultiplierPID = Shader.GetGlobalVector (propertyID_AfsFoliageWindMultiplierPID);
        
        value_AfsFoliageTimeFrequencyPID = Shader.GetGlobalVector (propertyID_AfsFoliageTimeFrequencyPID);
        value_AfsFoliageWaveSizePID = Shader.GetGlobalVector (propertyID_AfsFoliageWaveSizePID);
        value_AfsVertexLitTranslucencyPID = Shader.GetGlobalFloat (propertyID_AfsVertexLitTranslucencyPID);
        value_AfsVertexLitViewDependencyPID = Shader.GetGlobalFloat (propertyID_AfsVertexLitViewDependencyPID);
        value_AfsVertexLitVariationPID = Shader.GetGlobalFloat (propertyID_AfsVertexLitVariationPID);
        value_AfsVertexLitHorizonFadePID = Shader.GetGlobalFloat (propertyID_AfsVertexLitHorizonFadePID);
        
        value_AfsWindJitterVariationScalePID = Shader.GetGlobalVector (propertyID_AfsWindJitterVariationScalePID);
        value_AfsGrassWindPID = Shader.GetGlobalVector (propertyID_AfsGrassWindPID);
        value_AfsWaveAndDistancePID = Shader.GetGlobalVector (propertyID_AfsWaveAndDistancePID);
        
        value_AfsRainamountPID = Shader.GetGlobalFloat (propertyID_AfsRainamountPID);
        value_AfsWavingTintPID = Shader.GetGlobalColor (propertyID_AfsWavingTintPID);
        value_AfsTreeColorPID = Shader.GetGlobalColor (propertyID_AfsTreeColorPID);
        value_AfsTerrainTreesPID = Shader.GetGlobalVector (propertyID_AfsTerrainTreesPID);
        value_AfsBillboardCameraForwardPID = Shader.GetGlobalVector (propertyID_AfsBillboardCameraForwardPID);
        value_AfsBillboardBorderPID = Shader.GetGlobalFloat (propertyID_AfsBillboardBorderPID);
        
        value_afs_SHAr = Shader.GetGlobalVector (propertyID_afs_SHAr);
        value_afs_SHAg = Shader.GetGlobalVector (propertyID_afs_SHAg);
        value_afs_SHAb = Shader.GetGlobalVector (propertyID_afs_SHAb);
        value_afs_SHBr = Shader.GetGlobalVector (propertyID_afs_SHBr);
        value_afs_SHBg = Shader.GetGlobalVector (propertyID_afs_SHBg);
        value_afs_SHBb = Shader.GetGlobalVector (propertyID_afs_SHBb);
        value_afs_SHC = Shader.GetGlobalVector (propertyID_afs_SHC);
    }
    
    [Button, ButtonGroup, PropertyOrder (-1)]
    private void SetValues ()
    {
        Shader.SetGlobalVector (propertyID_WindPID, value_WindPID);
        Shader.SetGlobalVector (propertyID_AfsTreeWindMuliplierPID, value_AfsTreeWindMuliplierPID);
        Shader.SetGlobalVector (propertyID_AfsFoliageWindPID, value_AfsFoliageWindPID);
        Shader.SetGlobalVector (propertyID_AfsFoliageWindPulsMagnitudeFrequencyPID, value_AfsFoliageWindPulsMagnitudeFrequencyPID);
        Shader.SetGlobalVector (propertyID_AfsFoliageWindMultiplierPID, value_AfsFoliageWindMultiplierPID);
        
        Shader.SetGlobalVector (propertyID_AfsFoliageTimeFrequencyPID, value_AfsFoliageTimeFrequencyPID);
        Shader.SetGlobalVector (propertyID_AfsFoliageWaveSizePID, value_AfsFoliageWaveSizePID);
        Shader.SetGlobalFloat (propertyID_AfsVertexLitTranslucencyPID, value_AfsVertexLitTranslucencyPID);
        Shader.SetGlobalFloat (propertyID_AfsVertexLitViewDependencyPID, value_AfsVertexLitViewDependencyPID);
        Shader.SetGlobalFloat (propertyID_AfsVertexLitVariationPID, value_AfsVertexLitVariationPID);
        Shader.SetGlobalFloat (propertyID_AfsVertexLitHorizonFadePID, value_AfsVertexLitHorizonFadePID);
        
        Shader.SetGlobalVector (propertyID_AfsWindJitterVariationScalePID, value_AfsWindJitterVariationScalePID);
        Shader.SetGlobalVector (propertyID_AfsGrassWindPID, value_AfsGrassWindPID);
        Shader.SetGlobalVector (propertyID_AfsWaveAndDistancePID, value_AfsWaveAndDistancePID);
        
        Shader.SetGlobalFloat (propertyID_AfsRainamountPID, value_AfsRainamountPID);
        Shader.SetGlobalColor (propertyID_AfsWavingTintPID, value_AfsWavingTintPID);
        Shader.SetGlobalColor (propertyID_AfsTreeColorPID, value_AfsTreeColorPID);
        Shader.SetGlobalVector (propertyID_AfsTerrainTreesPID, value_AfsTerrainTreesPID);
        Shader.SetGlobalVector (propertyID_AfsBillboardCameraForwardPID, value_AfsBillboardCameraForwardPID);
        Shader.SetGlobalFloat (propertyID_AfsBillboardBorderPID, value_AfsBillboardBorderPID);
        
        Shader.SetGlobalVector (propertyID_afs_SHAr, value_afs_SHAr);
        Shader.SetGlobalVector (propertyID_afs_SHAg, value_afs_SHAg);
        Shader.SetGlobalVector (propertyID_afs_SHAb, value_afs_SHAb);
        Shader.SetGlobalVector (propertyID_afs_SHBr, value_afs_SHBr);
        Shader.SetGlobalVector (propertyID_afs_SHBg, value_afs_SHBg);
        Shader.SetGlobalVector (propertyID_afs_SHBb, value_afs_SHBb);
        Shader.SetGlobalVector (propertyID_afs_SHC, value_afs_SHC);
    }
}
