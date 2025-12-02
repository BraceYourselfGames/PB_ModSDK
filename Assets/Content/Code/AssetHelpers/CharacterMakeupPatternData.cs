using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Makeup Pattern Data", menuName = "Pilots/Character Makeup Pattern Data", order = 1)]
public class CharacterMakeupPatternData : ScriptableObject
{
    public Texture2D MakeupPatternTex;

    [ValueDropdown ("@DataLinkerSettingsPilot.data?.makeupColors?.Keys")]
    public string MakeupColorsGroupKey;

    [Space (10)]
    [PropertyRange (0f, 1f)]
    public float MakeupNormalIntensity = 0.0f;

    [Space (10)]
    [InfoBox ("1 is no change in shine (smoothness and spec), 0 disables it, 2+ increases it")]
    [PropertyRange (0f, 3f)]
    public float MakeupPrimaryShineTweak = 1.0f;

    [PropertyRange (0f, 3f)]
    public float MakeupSecondaryShineTweak = 1.0f;

    [Space (10)]
    [InfoBox ("Set to 1 if you want to make hair invisible in place of makeup, useful for scars removing parts of an eyebrow")]
    [PropertyRange (0f, 1f)]
    public float MakeupCutIntoHairIntensity = 0.5f;

    [ShowIf ("@MakeupCutIntoHairIntensity > 0.0f")]
    [PropertyRange (1f, 8f)]
    public float MakeupCutIntoHairAlphaPower = 1.0f;
}
