using UnityEngine;

[CreateAssetMenu(fileName = "ShockwavePreset", menuName = "VFX/Shockwave Preset", order = 1)]
public class ShockwaveVisualPreset : ScriptableObject
{
    public Material materialMain;
    public Material materialPrediction;
}