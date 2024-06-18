using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Character Hair Data", menuName = "Pilots/Character Hair Data", order = 1)]
public class CharacterHairData : ScriptableObject
{
    public GameObject HairMesh;
    
    [Space(10)]
    public Texture HairCapTex;
    [Range(0, 1)]
    public float HairCapIntensity = 1.0f;  
    [Range(0, 2)]
    public float HairCapBrightness = 1.25f;

    [ShowIf("HairMeshAssigned"), Space(20), Range(0, 1)]
    public float AlphaTestCutoff = 0.5f;


    [ShowIf("HairMeshAssigned"), Space(10), Range(0, 1)]
    public float HairSmoothness = 0.5f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairPBRSmoothness = 0.1f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairMainSpecularity = 0.05f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairTangentShift = 0.15f;


    [ShowIf("HairMeshAssigned"), Space(10), Range(0, 1)]
    public float HairAOIntensity = 0.9f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairOutlineIntensity = 0.75f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairGradientIntensity = 0.5f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairGradientUseVColor = 0.0f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float HairGradientPower = 0.0f;



    [ShowIf("HairMeshAssigned"), Space(10), Range(0, 1)]
    public float ShadowPassOpacityCut = 0.1f;

    [ShowIf("HairMeshAssigned"), Range(1, 10)]
    public float OpacityAmplifyCoef = 2.5f;

    [ShowIf("HairMeshAssigned"), Range(0, 10)]
    public float NormalAmplifyCoef = 4.0f;

    [ShowIf("HairMeshAssigned"), Range(0, 20)]
    public float OpacityAmplifyDistance = 5.0f;

    [ShowIf("HairMeshAssigned"), Range(0, 20)]
    public float OpacityAmplifyDistanceOffset = 1.0f;

    [ShowIf("HairMeshAssigned"), Range(0, 1)]
    public float OpacityBlueNoiseBlendFactor = 0.6f;
    

    [ShowIf("HairMeshAssigned"), Space(10), Range(0, 0.1f)]
    public float HairInertiaMultiplier = 0.03f;


    [InfoBox("If no vertical gradient present in G channel of hair's Vertex Color map, only Bottom parameter is used.")]
    [ShowIf("HairMeshAssigned"), Space(10), Range(0, 1.0f)]
    public float HairGravityTiltStrengthTop = 0.01f;

    [ShowIf("HairMeshAssigned"), Range(0, 1.0f)]
    public float HairGravityTiltStrengthBottom = 0.01f;

    [ShowIf("HairMeshAssigned"), Range(-1.0f, 1.0f)]
    public float HairGravityTiltVerticalGradientOffset = 0.0f;

	#if UNITY_EDITOR
        private bool HairMeshAssigned()
        {
            return (HairMesh != null);
        }
    #endif

}
