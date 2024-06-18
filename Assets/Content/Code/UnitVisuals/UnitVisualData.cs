using System;
using System.Collections.Generic;
using UnityEngine;
using PhantomBrigade;

public enum UnitArea
{
    CorePrimary = 0,
    LeftOptional = 1,
    RightOptional = 2,
    CoreSecondary = 3
}

public enum UnitCustomizationLocation
{
    ArmorHead = 0,
    ArmorTorso = 1,
    ArmorPelvis = 2,
    ArmorLeftShoulder = 3,
    ArmorRightShoulder = 4,
    ArmorLeftForearm = 5,
    ArmorRightForearm = 6,
    ArmorLeftThigh = 7,
    ArmorRightThigh = 8,
    ArmorLeftLeg = 9,
    ArmorRightLeg = 10,
    ArmorLeftFoot = 11,
    ArmorRightFoot = 12,
    FrameHead = 13,
    FrameTorso = 14,
    FramePelvis = 15,
    FrameLeftShoulder = 16,
    FrameRightShoulder = 17,
    FrameLeftForearm = 18,
    FrameRightForearm = 19,
    FrameLeftHand = 20,
    FrameRightHand = 21,
    FrameLeftThigh = 22,
    FrameRightThigh = 23,
    FrameLeftLeg = 24,
    FrameRightLeg = 25,
    FrameLeftFoot = 26,
    FrameRightFoot = 27,
    WeaponLeft = 28,
    WeaponRight = 29,
    ArmorPod = 50
}

public enum UnitHardpoint
{
    ArmorHead = 0,
    ArmorTorso = 1,
    ArmorPelvis = 2,
    ArmorLeftShoulder = 3,
    ArmorRightShoulder = 4,
    ArmorLeftForearm = 5,
    ArmorRightForearm = 6,
    ArmorLeftThigh = 7,
    ArmorRightThigh = 8,
    ArmorLeftLeg = 9,
    ArmorRightLeg = 10,
    ArmorLeftFoot = 11,
    ArmorRightFoot = 12,
    WeaponLeft = 13,
    WeaponRight = 14,
    InternalHead = 20,
    InternalReactor = 21,
    InternalPod = 22,
    InternalLeftArm = 30,
    InternalRightArm = 31,
    InternalLeftLeg = 40,
    InternalRightLeg = 41,
    InternalMagazine = 42,
    ArmorPod = 50,
}

public enum UnitCustomizationType
{
    Color,
    Coating,
    Material,
    Texture,
    Intensity
}

public enum UnitCustomizationFields
{
    ColorPrimary,
    ColorSecondary,
    ColorTertiary,
    MaterialPrimary,
    MaterialSecondary,
    MaterialTertiary,
    CoatingPrimary,
    CoatingSecondary,
    CoatingTertiary,
    PatternIntensityPrimary,
    PatternIntensitySecondary,
    PatternIntensityTertiary
}

public enum UnitCustomizationColor
{
    OffWhite = 99,
    LightGray = 100,
    MiddleGray = 101,
    DarkGray = 102,
    OffBlack = 103,

    StandardCoolRed = 1000,
    StandardNeutralRed = 1100,
    StandardWarmRed = 1200,
    StandardOrange = 1300,
    StandardYellow = 1400,
    StandardLimeYellow = 1500,
    StandardLimeGreen = 1600,
    StandardWarmGreen = 1700,
    StandardNeutralGreen = 1800,
    StandardCoolGreen = 1900,
    StandardCyan = 2000,
    StandardSkyBlue = 2100,
    StandardNeutralBlue = 2200,
    StandardDeepBlue = 2300,
    StandardVioletBlue = 2400,
    StandardViolet = 2500,
    StandardMagenta = 2600,

    VividCoolRed = 1001,
    VividNeutralRed = 1101,
    VividWarmRed = 1201,
    VividOrange = 1301,
    VividYellow = 1401,
    VividLimeYellow = 1501,
    VividLimeGreen = 1601,
    VividWarmGreen = 1701,
    VividNeutralGreen = 1801,
    VividCoolGreen = 1901,
    VividCyan = 2001,
    VividSkyBlue = 2101,
    VividNeutralBlue = 2201,
    VividDeepBlue = 2301,
    VividVioletBlue = 2401,
    VividViolet = 2501,
    VividMagenta = 2601,

}

public enum UnitCustomizationCoating
{
    Matte = 0,
    Subdued = 4,
    Normal = 1,
    Reflective = 2,
    FullRange = 3
}

public enum UnitCustomizationMaterial
{
    Dielectric = 0,
    Metallic = 1
}

public enum UnitCustomizationPatternType
{
    None = 0,
    WoodlandA = 1,
    WoodlandB = 2,
    DigitalA = 4,
    DigitalB = 5,
    DigitalC = 6,
    DazzleA = 7,
    DazzleB = 8,
    DazzleC = 9,
    DazzleD = 10
}

public enum UnitCustomizationPatternIntensity
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}

[Serializable]
public struct UnitCustomizationBlockSerialized
{
    public UnitCustomizationBlockSerialized (UnitCustomizationBlock source)
    {
        c1 = (int)source.colorPrimary;
        c2 = (int)source.colorSecondary;
        c3 = (int)source.colorTertiary;
        m1 = (int)source.materialPrimary;
        m2 = (int)source.materialSecondary;
        m3 = (int)source.materialTertiary;
        s1 = (int)source.coatingPrimary;
        s2 = (int)source.coatingSecondary;
        s3 = (int)source.coatingTertiary;
        p1 = (int)source.patternIntensityPrimary;
        p2 = (int)source.patternIntensitySecondary;
        p3 = (int)source.patternIntensityTertiary;
    }

    public int c1;
    public int c2;
    public int c3;
    public int m1;
    public int m2;
    public int m3;
    public int s1;
    public int s2;
    public int s3;
    public int p1;
    public int p2;
    public int p3;
}

[System.Serializable]
public class UnitCustomizationBlock
{
    public UnitCustomizationBlock () { }

    public UnitCustomizationBlock (UnitCustomizationBlockSerialized source)
    {
        colorPrimary = (UnitCustomizationColor)source.c1;
        colorSecondary = (UnitCustomizationColor)source.c2;
        colorTertiary = (UnitCustomizationColor)source.c3;
        materialPrimary = (UnitCustomizationMaterial)source.m1;
        materialSecondary = (UnitCustomizationMaterial)source.m2;
        materialTertiary = (UnitCustomizationMaterial)source.m3;
        coatingPrimary = (UnitCustomizationCoating)source.s1;
        coatingSecondary = (UnitCustomizationCoating)source.s2;
        coatingTertiary = (UnitCustomizationCoating)source.s3;
        patternIntensityPrimary = (UnitCustomizationPatternIntensity)source.p1;
        patternIntensitySecondary = (UnitCustomizationPatternIntensity)source.p2;
        patternIntensityTertiary = (UnitCustomizationPatternIntensity)source.p3;
    }

    public UnitCustomizationBlock (UnitCustomizationBlock source)
    {
        CopyValues (source);
    }

    public void CopyValues (UnitCustomizationBlock source)
    {
        if (source == this)
            return;

        colorPrimary = source.colorPrimary;
        colorSecondary = source.colorSecondary;
        colorTertiary = source.colorTertiary;
        materialPrimary = source.materialPrimary;
        materialSecondary = source.materialSecondary;
        materialTertiary = source.materialTertiary;
        coatingPrimary = source.coatingPrimary;
        coatingSecondary = source.coatingSecondary;
        coatingTertiary = source.coatingTertiary;
        patternIntensityPrimary = source.patternIntensityPrimary;
        patternIntensitySecondary = source.patternIntensitySecondary;
        patternIntensityTertiary = source.patternIntensityTertiary;
    }

    public UnitCustomizationBlock
    (
        UnitCustomizationColor colorPrimary,
        UnitCustomizationColor colorSecondary,
        UnitCustomizationColor colorTertiary,
        UnitCustomizationMaterial materialPrimary,
        UnitCustomizationMaterial materialSecondary,
        UnitCustomizationMaterial materialTertiary,
        UnitCustomizationCoating coatingPrimary,
        UnitCustomizationCoating coatingSecondary,
        UnitCustomizationCoating coatingTertiary,
        UnitCustomizationPatternIntensity patternIntensityPrimary,
        UnitCustomizationPatternIntensity patternIntensitySecondary,
        UnitCustomizationPatternIntensity patternIntensityTertiary
    )
    {
        this.colorPrimary = colorPrimary;
        this.colorSecondary = colorSecondary;
        this.colorTertiary = colorTertiary;
        this.materialPrimary = materialPrimary;
        this.materialSecondary = materialSecondary;
        this.materialTertiary = materialTertiary;
        this.coatingPrimary = coatingPrimary;
        this.coatingSecondary = coatingSecondary;
        this.coatingTertiary = coatingTertiary;
        this.patternIntensityPrimary = patternIntensityPrimary;
        this.patternIntensitySecondary = patternIntensitySecondary;
        this.patternIntensityTertiary = patternIntensityTertiary;
    }

    public UnitCustomizationColor colorPrimary;
    public UnitCustomizationColor colorSecondary;
    public UnitCustomizationColor colorTertiary;
    public UnitCustomizationMaterial materialPrimary;
    public UnitCustomizationMaterial materialSecondary;
    public UnitCustomizationMaterial materialTertiary;
    public UnitCustomizationCoating coatingPrimary;
    public UnitCustomizationCoating coatingSecondary;
    public UnitCustomizationCoating coatingTertiary;
    public UnitCustomizationPatternIntensity patternIntensityPrimary;
    public UnitCustomizationPatternIntensity patternIntensitySecondary;
    public UnitCustomizationPatternIntensity patternIntensityTertiary;

    public int GetValueFromFieldType (UnitCustomizationFields field)
    {
        if (field == UnitCustomizationFields.ColorPrimary)
            return (int)colorPrimary;
        else if (field == UnitCustomizationFields.ColorSecondary)
            return (int)colorSecondary;
        else if (field == UnitCustomizationFields.ColorTertiary)
            return (int)colorTertiary;

        else if (field == UnitCustomizationFields.MaterialPrimary)
            return (int)coatingPrimary;
        else if (field == UnitCustomizationFields.MaterialSecondary)
            return (int)materialSecondary;
        else if (field == UnitCustomizationFields.MaterialTertiary)
            return (int)materialTertiary;

        else if (field == UnitCustomizationFields.CoatingPrimary)
            return (int)coatingPrimary;
        else if (field == UnitCustomizationFields.CoatingSecondary)
            return (int)coatingSecondary;
        else if (field == UnitCustomizationFields.CoatingTertiary)
            return (int)coatingTertiary;

        else if (field == UnitCustomizationFields.PatternIntensityPrimary)
            return (int)patternIntensityPrimary;
        else if (field == UnitCustomizationFields.PatternIntensitySecondary)
            return (int)patternIntensitySecondary;
        else
            return (int)patternIntensityTertiary;
    }

    public void SetValueWithType (UnitCustomizationFields field, int value)
    {
        if (field == UnitCustomizationFields.ColorPrimary)
            colorPrimary = (UnitCustomizationColor)value;
        else if (field == UnitCustomizationFields.ColorSecondary)
            colorSecondary = (UnitCustomizationColor)value;
        else if (field == UnitCustomizationFields.ColorTertiary)
            colorTertiary = (UnitCustomizationColor)value;

        else if (field == UnitCustomizationFields.MaterialPrimary)
            materialPrimary = (UnitCustomizationMaterial)value;
        else if (field == UnitCustomizationFields.MaterialSecondary)
            materialSecondary = (UnitCustomizationMaterial)value;
        else if (field == UnitCustomizationFields.MaterialTertiary)
            materialTertiary = (UnitCustomizationMaterial)value;

        else if (field == UnitCustomizationFields.CoatingPrimary)
            coatingPrimary = (UnitCustomizationCoating)value;
        else if (field == UnitCustomizationFields.CoatingSecondary)
            coatingSecondary = (UnitCustomizationCoating)value;
        else if (field == UnitCustomizationFields.CoatingTertiary)
            coatingTertiary = (UnitCustomizationCoating)value;

        else if (field == UnitCustomizationFields.PatternIntensityPrimary)
            patternIntensityPrimary = (UnitCustomizationPatternIntensity)value;
        else if (field == UnitCustomizationFields.PatternIntensitySecondary)
            patternIntensitySecondary = (UnitCustomizationPatternIntensity)value;
        else
            patternIntensityTertiary = (UnitCustomizationPatternIntensity)value;
    }
}

public static class UnitHelper
{
    private static Dictionary<UnitArea, List<UnitHardpoint>> hardpointsForAreaTypes;
    public static List<UnitHardpoint> GetHardpointsForAreaType (UnitArea type)
    {
        if (hardpointsForAreaTypes == null)
        {
            hardpointsForAreaTypes = new Dictionary<UnitArea, List<UnitHardpoint>> ();

            List<UnitHardpoint> hardpointsForUpperBody = new List<UnitHardpoint> ();
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorHead);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorTorso);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorPelvis);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorPod);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalHead);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalReactor);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalPod);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalMagazine);
            hardpointsForAreaTypes.Add (UnitArea.CorePrimary, hardpointsForUpperBody);

            List<UnitHardpoint> hardpointsForLeftArm = new List<UnitHardpoint> ();
            hardpointsForLeftArm.Add (UnitHardpoint.ArmorLeftShoulder);
            hardpointsForLeftArm.Add (UnitHardpoint.ArmorLeftForearm);
            hardpointsForLeftArm.Add (UnitHardpoint.WeaponLeft);
            hardpointsForLeftArm.Add (UnitHardpoint.InternalLeftArm);
            hardpointsForAreaTypes.Add (UnitArea.LeftOptional, hardpointsForLeftArm);

            List<UnitHardpoint> hardpointsForRightArm = new List<UnitHardpoint> ();
            hardpointsForRightArm.Add (UnitHardpoint.ArmorRightShoulder);
            hardpointsForRightArm.Add (UnitHardpoint.ArmorRightForearm);
            hardpointsForRightArm.Add (UnitHardpoint.WeaponRight);
            hardpointsForRightArm.Add (UnitHardpoint.InternalRightArm);
            hardpointsForAreaTypes.Add (UnitArea.RightOptional, hardpointsForRightArm);

            List<UnitHardpoint> hardpointsForLowerBody = new List<UnitHardpoint> ();
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftThigh);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightThigh);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftFoot);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightFoot);
            hardpointsForLowerBody.Add (UnitHardpoint.InternalLeftLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.InternalRightLeg);
            hardpointsForAreaTypes.Add (UnitArea.CoreSecondary, hardpointsForLowerBody);
        }
        return hardpointsForAreaTypes[type];
    }

    private static Dictionary<UnitArea, List<UnitHardpoint>> hardpointsForAreaTypes_Armor;
    private static Dictionary<UnitArea, List<UnitHardpoint>> hardpointsForAreaTypes_Other;
    public static List<UnitHardpoint> GetHardpointsForAreaTypeAndLayer (UnitArea type, bool armor)
    {
        if (hardpointsForAreaTypes_Armor == null)
        {
            hardpointsForAreaTypes_Armor = new Dictionary<UnitArea, List<UnitHardpoint>> ();

            List<UnitHardpoint> hardpointsForUpperBody = new List<UnitHardpoint> ();
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorHead);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorTorso);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorPelvis);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorPod);
            hardpointsForAreaTypes_Armor.Add (UnitArea.CorePrimary, hardpointsForUpperBody);

            List<UnitHardpoint> hardpointsForLeftArm = new List<UnitHardpoint> ();
            hardpointsForLeftArm.Add (UnitHardpoint.ArmorLeftShoulder);
            hardpointsForLeftArm.Add (UnitHardpoint.ArmorLeftForearm);
            hardpointsForAreaTypes_Armor.Add (UnitArea.LeftOptional, hardpointsForLeftArm);

            List<UnitHardpoint> hardpointsForRightArm = new List<UnitHardpoint> ();
            hardpointsForRightArm.Add (UnitHardpoint.ArmorRightShoulder);
            hardpointsForRightArm.Add (UnitHardpoint.ArmorRightForearm);
            hardpointsForAreaTypes_Armor.Add (UnitArea.RightOptional, hardpointsForRightArm);

            List<UnitHardpoint> hardpointsForLowerBody = new List<UnitHardpoint> ();
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftThigh);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightThigh);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftFoot);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightFoot);
            hardpointsForAreaTypes_Armor.Add (UnitArea.CoreSecondary, hardpointsForLowerBody);
        }

        if (hardpointsForAreaTypes_Other == null)
        {
            hardpointsForAreaTypes_Other = new Dictionary<UnitArea, List<UnitHardpoint>> ();

            List<UnitHardpoint> hardpointsForUpperBody = new List<UnitHardpoint> ();
            hardpointsForUpperBody.Add (UnitHardpoint.InternalHead);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalReactor);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalPod);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalMagazine);
            hardpointsForAreaTypes_Other.Add (UnitArea.CorePrimary, hardpointsForUpperBody);

            List<UnitHardpoint> hardpointsForLeftArm = new List<UnitHardpoint> ();
            hardpointsForLeftArm.Add (UnitHardpoint.WeaponLeft);
            hardpointsForLeftArm.Add (UnitHardpoint.InternalLeftArm);
            hardpointsForAreaTypes_Other.Add (UnitArea.LeftOptional, hardpointsForLeftArm);

            List<UnitHardpoint> hardpointsForRightArm = new List<UnitHardpoint> ();
            hardpointsForRightArm.Add (UnitHardpoint.WeaponRight);
            hardpointsForRightArm.Add (UnitHardpoint.InternalRightArm);
            hardpointsForAreaTypes_Other.Add (UnitArea.RightOptional, hardpointsForRightArm);

            List<UnitHardpoint> hardpointsForLowerBody = new List<UnitHardpoint> ();
            hardpointsForLowerBody.Add (UnitHardpoint.InternalLeftLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.InternalRightLeg);
            hardpointsForAreaTypes_Other.Add (UnitArea.CoreSecondary, hardpointsForLowerBody);
        }

        return armor ? hardpointsForAreaTypes_Armor[type] : hardpointsForAreaTypes_Other[type];
    }

    private static Dictionary<int, List<UnitHardpoint>> hardpointsForGroupIndex;
    public static List<UnitHardpoint> GetHardpointsForGroupIndex (int index)
    {
        if (hardpointsForGroupIndex == null)
        {
            hardpointsForGroupIndex = new Dictionary<int, List<UnitHardpoint>> ();

            List<UnitHardpoint> hardpointsForUpperBody = new List<UnitHardpoint> ();
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorHead);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorTorso);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorPelvis);
            hardpointsForUpperBody.Add (UnitHardpoint.ArmorPod);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalHead);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalReactor);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalPod);
            hardpointsForUpperBody.Add (UnitHardpoint.InternalMagazine);
            hardpointsForGroupIndex.Add (0, hardpointsForUpperBody);

            List<UnitHardpoint> hardpointsForLeftArm = new List<UnitHardpoint> ();
            hardpointsForLeftArm.Add (UnitHardpoint.ArmorLeftShoulder);
            hardpointsForLeftArm.Add (UnitHardpoint.ArmorLeftForearm);
            hardpointsForLeftArm.Add (UnitHardpoint.WeaponLeft);
            hardpointsForLeftArm.Add (UnitHardpoint.InternalLeftArm);
            hardpointsForGroupIndex.Add (1, hardpointsForLeftArm);

            List<UnitHardpoint> hardpointsForRightArm = new List<UnitHardpoint> ();
            hardpointsForRightArm.Add (UnitHardpoint.ArmorRightShoulder);
            hardpointsForRightArm.Add (UnitHardpoint.ArmorRightForearm);
            hardpointsForRightArm.Add (UnitHardpoint.WeaponRight);
            hardpointsForRightArm.Add (UnitHardpoint.InternalRightArm);
            hardpointsForGroupIndex.Add (2, hardpointsForRightArm);

            List<UnitHardpoint> hardpointsForLowerBody = new List<UnitHardpoint> ();
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftThigh);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightThigh);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorLeftFoot);
            hardpointsForLowerBody.Add (UnitHardpoint.ArmorRightFoot);
            hardpointsForLowerBody.Add (UnitHardpoint.InternalLeftLeg);
            hardpointsForLowerBody.Add (UnitHardpoint.InternalRightLeg);
            hardpointsForGroupIndex.Add (3, hardpointsForLowerBody);

            List<UnitHardpoint> hardpointsForWeapons = new List<UnitHardpoint> ();
            hardpointsForWeapons.Add (UnitHardpoint.WeaponRight);
            hardpointsForWeapons.Add (UnitHardpoint.WeaponLeft);
            hardpointsForGroupIndex.Add (4, hardpointsForWeapons);
        }

        if (hardpointsForGroupIndex.ContainsKey (index))
            return hardpointsForGroupIndex[index];
        else
            return null;
    }

    private static Dictionary<UnitHardpoint, UnitArea> areaTypesForHardpoints;
    public static UnitArea GetAreaTypeForHardpoint (UnitHardpoint hardpoint)
    {
        if (areaTypesForHardpoints == null)
        {
            areaTypesForHardpoints = new Dictionary<UnitHardpoint, UnitArea> ();

            areaTypesForHardpoints.Add (UnitHardpoint.ArmorHead, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorTorso, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorPelvis, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorPod, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalHead, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalReactor, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalPod, UnitArea.CorePrimary);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalMagazine, UnitArea.CorePrimary);

            areaTypesForHardpoints.Add (UnitHardpoint.ArmorLeftShoulder, UnitArea.LeftOptional);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorLeftForearm, UnitArea.LeftOptional);
            areaTypesForHardpoints.Add (UnitHardpoint.WeaponLeft, UnitArea.LeftOptional);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalLeftArm, UnitArea.LeftOptional);

            areaTypesForHardpoints.Add (UnitHardpoint.ArmorRightShoulder, UnitArea.RightOptional);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorRightForearm, UnitArea.RightOptional);
            areaTypesForHardpoints.Add (UnitHardpoint.WeaponRight, UnitArea.RightOptional);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalRightArm, UnitArea.RightOptional);

            areaTypesForHardpoints.Add (UnitHardpoint.ArmorLeftThigh, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorRightThigh, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorLeftLeg, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorRightLeg, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorLeftFoot, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.ArmorRightFoot, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalLeftLeg, UnitArea.CoreSecondary);
            areaTypesForHardpoints.Add (UnitHardpoint.InternalRightLeg, UnitArea.CoreSecondary);
        }
        return areaTypesForHardpoints[hardpoint];
    }

    private static Dictionary<UnitHardpoint, List<UnitHardpoint>> hardpointsForArmorCoverage;
    public static List<UnitHardpoint> GetHardpointsOver (UnitHardpoint harpointInside)
    {
        if (hardpointsForArmorCoverage == null)
        {
            hardpointsForArmorCoverage = new Dictionary<UnitHardpoint, List<UnitHardpoint>> ();
            UnitHardpoint[] hardpoints = (UnitHardpoint[])Enum.GetValues (typeof (UnitHardpoint));
            for (int i = 0; i < hardpoints.Length; ++i)
                hardpointsForArmorCoverage.Add (hardpoints[i], new List<UnitHardpoint> ());

            hardpointsForArmorCoverage[UnitHardpoint.InternalHead].Add (UnitHardpoint.ArmorHead);

            hardpointsForArmorCoverage[UnitHardpoint.InternalLeftArm].Add (UnitHardpoint.ArmorLeftForearm);
            hardpointsForArmorCoverage[UnitHardpoint.InternalLeftArm].Add (UnitHardpoint.ArmorLeftShoulder);

            hardpointsForArmorCoverage[UnitHardpoint.InternalRightArm].Add (UnitHardpoint.ArmorRightForearm);
            hardpointsForArmorCoverage[UnitHardpoint.InternalRightArm].Add (UnitHardpoint.ArmorRightShoulder);

            hardpointsForArmorCoverage[UnitHardpoint.InternalLeftLeg].Add (UnitHardpoint.ArmorLeftThigh);
            hardpointsForArmorCoverage[UnitHardpoint.InternalLeftLeg].Add (UnitHardpoint.ArmorLeftLeg);

            hardpointsForArmorCoverage[UnitHardpoint.InternalRightLeg].Add (UnitHardpoint.ArmorRightThigh);
            hardpointsForArmorCoverage[UnitHardpoint.InternalRightLeg].Add (UnitHardpoint.ArmorRightLeg);

            hardpointsForArmorCoverage[UnitHardpoint.InternalMagazine].Add (UnitHardpoint.ArmorTorso);
            hardpointsForArmorCoverage[UnitHardpoint.InternalReactor].Add (UnitHardpoint.ArmorTorso);

            hardpointsForArmorCoverage[UnitHardpoint.InternalPod].Add (UnitHardpoint.ArmorPod);

            hardpointsForArmorCoverage[UnitHardpoint.WeaponLeft].Add (UnitHardpoint.ArmorLeftForearm);
            hardpointsForArmorCoverage[UnitHardpoint.WeaponRight].Add (UnitHardpoint.ArmorRightForearm);
        }

        return hardpointsForArmorCoverage[harpointInside];
    }

    private static Dictionary<UnitHardpoint, List<UnitHardpoint>> hardpointsForPrecisePenetration;
    public static List<UnitHardpoint> GetHardpointsUnderneath (UnitHardpoint hardpointOutside)
    {
        if (hardpointsForPrecisePenetration == null)
        {
            hardpointsForPrecisePenetration = new Dictionary<UnitHardpoint, List<UnitHardpoint>> ();
            UnitHardpoint[] hardpoints = (UnitHardpoint[])Enum.GetValues (typeof (UnitHardpoint));
            for (int i = 0; i < hardpoints.Length; ++i)
                hardpointsForPrecisePenetration.Add (hardpoints[i], new List<UnitHardpoint> ());

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorHead].Add (UnitHardpoint.InternalHead);

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorTorso].Add (UnitHardpoint.InternalReactor);
            hardpointsForPrecisePenetration[UnitHardpoint.ArmorTorso].Add (UnitHardpoint.InternalMagazine);

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorPod].Add (UnitHardpoint.InternalPod);

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorLeftThigh].Add (UnitHardpoint.InternalLeftLeg);
            hardpointsForPrecisePenetration[UnitHardpoint.ArmorLeftLeg].Add (UnitHardpoint.InternalLeftLeg);

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorRightThigh].Add (UnitHardpoint.InternalRightLeg);
            hardpointsForPrecisePenetration[UnitHardpoint.ArmorRightLeg].Add (UnitHardpoint.InternalRightLeg);

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorLeftShoulder].Add (UnitHardpoint.InternalLeftArm);
            hardpointsForPrecisePenetration[UnitHardpoint.ArmorLeftForearm].Add (UnitHardpoint.InternalLeftArm);

            hardpointsForPrecisePenetration[UnitHardpoint.ArmorRightShoulder].Add (UnitHardpoint.InternalRightArm);
            hardpointsForPrecisePenetration[UnitHardpoint.ArmorRightForearm].Add (UnitHardpoint.InternalRightArm);
        }

        return hardpointsForPrecisePenetration[hardpointOutside];
    }

    private static Dictionary<UnitCustomizationLocation, UnitArea> areaTypesForLocations;
    public static UnitArea GetAreaTypeForLocation (UnitCustomizationLocation location)
    {
        if (areaTypesForLocations == null)
        {
            areaTypesForLocations = new Dictionary<UnitCustomizationLocation, UnitArea> ();
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorHead, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorTorso, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorPelvis, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorPod, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorLeftShoulder, UnitArea.LeftOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorRightShoulder, UnitArea.RightOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorLeftForearm, UnitArea.LeftOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorRightForearm, UnitArea.RightOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorLeftThigh, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorRightThigh, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorLeftLeg, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorRightLeg, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorLeftFoot, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.ArmorRightFoot, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameHead, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameTorso, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FramePelvis, UnitArea.CorePrimary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameLeftShoulder, UnitArea.LeftOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameRightShoulder, UnitArea.RightOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameLeftForearm, UnitArea.LeftOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameRightForearm, UnitArea.RightOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameLeftHand, UnitArea.LeftOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameRightHand, UnitArea.RightOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameLeftThigh, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameRightThigh, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameLeftLeg, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameRightLeg, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameLeftFoot, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.FrameRightFoot, UnitArea.CoreSecondary);
            areaTypesForLocations.Add (UnitCustomizationLocation.WeaponLeft, UnitArea.LeftOptional);
            areaTypesForLocations.Add (UnitCustomizationLocation.WeaponRight, UnitArea.RightOptional);
        }
        return areaTypesForLocations[location];
    }

    private static Dictionary<UnitHardpoint, UnitHardpoint?> mirrorHardpoints;
    public static UnitHardpoint? GetMirrorHardpoint (UnitHardpoint hardpoint)
    {
        if (mirrorHardpoints == null)
        {
            mirrorHardpoints = new Dictionary<UnitHardpoint, UnitHardpoint?> ();
            UnitHardpoint[] hardpoints = (UnitHardpoint[])Enum.GetValues (typeof (UnitHardpoint));
            for (int i = 0; i < hardpoints.Length; ++i)
            {
                mirrorHardpoints.Add (hardpoints[i], null);
            }

            mirrorHardpoints[UnitHardpoint.InternalLeftArm] = UnitHardpoint.InternalRightArm;
            mirrorHardpoints[UnitHardpoint.InternalRightArm] = UnitHardpoint.InternalLeftArm;

            mirrorHardpoints[UnitHardpoint.InternalLeftLeg] = UnitHardpoint.InternalRightLeg;
            mirrorHardpoints[UnitHardpoint.InternalRightLeg] = UnitHardpoint.InternalLeftLeg;

            mirrorHardpoints[UnitHardpoint.WeaponLeft] = UnitHardpoint.WeaponRight;
            mirrorHardpoints[UnitHardpoint.WeaponRight] = UnitHardpoint.WeaponLeft;

            mirrorHardpoints[UnitHardpoint.ArmorLeftShoulder] = UnitHardpoint.ArmorRightShoulder;
            mirrorHardpoints[UnitHardpoint.ArmorRightShoulder] = UnitHardpoint.ArmorLeftShoulder;

            mirrorHardpoints[UnitHardpoint.ArmorLeftForearm] = UnitHardpoint.ArmorRightForearm;
            mirrorHardpoints[UnitHardpoint.ArmorRightForearm] = UnitHardpoint.ArmorLeftForearm;

            mirrorHardpoints[UnitHardpoint.ArmorLeftThigh] = UnitHardpoint.ArmorRightThigh;
            mirrorHardpoints[UnitHardpoint.ArmorRightThigh] = UnitHardpoint.ArmorLeftThigh;

            mirrorHardpoints[UnitHardpoint.ArmorLeftLeg] = UnitHardpoint.ArmorRightLeg;
            mirrorHardpoints[UnitHardpoint.ArmorRightLeg] = UnitHardpoint.ArmorLeftLeg;

            mirrorHardpoints[UnitHardpoint.ArmorLeftFoot] = UnitHardpoint.ArmorRightFoot;
            mirrorHardpoints[UnitHardpoint.ArmorRightFoot] = UnitHardpoint.ArmorLeftFoot;
        }
        return mirrorHardpoints[hardpoint];
    }

    public class LocationInfo
    {
        public bool skinned;
        public int skinnedIndex;
        public string transformName;

        public LocationInfo (bool skinned, int skinnedIndex, string transformName)
        {
            this.skinned = skinned;
            this.skinnedIndex = skinnedIndex;
            this.transformName = transformName;
        }
    }

    private static Dictionary<UnitCustomizationLocation, LocationInfo> infoAboutLocations;
    public static LocationInfo GetLocationInfo (UnitCustomizationLocation location)
    {
        if (infoAboutLocations == null)
        {
            infoAboutLocations = new Dictionary<UnitCustomizationLocation, LocationInfo> ();
            UnitCustomizationLocation[] locations = (UnitCustomizationLocation[])Enum.GetValues (typeof (UnitCustomizationLocation));
            for (int i = 0; i < locations.Length; ++i)
                infoAboutLocations.Add (locations[i], null);

            // FrameHead = 1 + head autojoint
            // FrameTorso = 2
            // FramePelvis = 3 + pelvis autojoint
            // FrameLeftShoulder = 4
            // FrameRightShoulder = 5
            // FrameLeftForearm = 6 + U shaped autojoint
            // FrameRightForearm = 7 + U shaped autojoint
            // FrameLeftHand = 8 + small part autojoint
            // FrameRightHand = 9 + small part autojoint
            // FrameLeftThigh = 10
            // FrameRightThigh = 11
            // FrameLeftLeg = 12
            // FrameRightLeg = 13
            // FrameLeftFoot = 14 + entire foot autojoint
            // FrameRightFoot = 15 + entire foot autojoint
            // Pistons = 16

            infoAboutLocations[UnitCustomizationLocation.ArmorHead] = new LocationInfo (false, 0, "hardpoint_head");
            infoAboutLocations[UnitCustomizationLocation.ArmorTorso] = new LocationInfo (false, 0, "hardpoint_torso");
            infoAboutLocations[UnitCustomizationLocation.ArmorPelvis] = new LocationInfo (false, 0, "hardpoint_pelvis");
            infoAboutLocations[UnitCustomizationLocation.ArmorPod] = new LocationInfo (false, 0, "hardpoint_pod");
            infoAboutLocations[UnitCustomizationLocation.ArmorLeftShoulder] = new LocationInfo (false, 0, "hardpoint_left_arm");
            infoAboutLocations[UnitCustomizationLocation.ArmorRightShoulder] = new LocationInfo (false, 0, "hardpoint_right_arm");
            infoAboutLocations[UnitCustomizationLocation.ArmorLeftForearm] = new LocationInfo (false, 0, "hardpoint_left_forearm");
            infoAboutLocations[UnitCustomizationLocation.ArmorRightForearm] = new LocationInfo (false, 0, "hardpoint_right_forearm");
            infoAboutLocations[UnitCustomizationLocation.ArmorLeftThigh] = new LocationInfo (false, 0, "hardpoint_left_thigh");
            infoAboutLocations[UnitCustomizationLocation.ArmorRightThigh] = new LocationInfo (false, 0, "hardpoint_right_thigh");
            infoAboutLocations[UnitCustomizationLocation.ArmorLeftLeg] = new LocationInfo (false, 0, "hardpoint_left_leg");
            infoAboutLocations[UnitCustomizationLocation.ArmorRightLeg] = new LocationInfo (false, 0, "hardpoint_right_leg");
            infoAboutLocations[UnitCustomizationLocation.ArmorLeftFoot] = new LocationInfo (false, 0, "hardpoint_left_foot");
            infoAboutLocations[UnitCustomizationLocation.ArmorRightFoot] = new LocationInfo (false, 0, "hardpoint_right_foot");
            infoAboutLocations[UnitCustomizationLocation.FrameHead] = new LocationInfo (true, 15, "mesh_head");
            infoAboutLocations[UnitCustomizationLocation.FrameTorso] = new LocationInfo (true, 2, "mesh_torso");
            infoAboutLocations[UnitCustomizationLocation.FramePelvis] = new LocationInfo (true, 3, "mesh_pelvis");
            infoAboutLocations[UnitCustomizationLocation.FrameLeftShoulder] = new LocationInfo (true, 4, "mesh_left_arm");
            infoAboutLocations[UnitCustomizationLocation.FrameRightShoulder] = new LocationInfo (true, 5, "mesh_right_arm");
            infoAboutLocations[UnitCustomizationLocation.FrameLeftForearm] = new LocationInfo (true, 6, "mesh_left_forearm");
            infoAboutLocations[UnitCustomizationLocation.FrameRightForearm] = new LocationInfo (true, 7, "mesh_right_forearm");
            infoAboutLocations[UnitCustomizationLocation.FrameLeftHand] = new LocationInfo (true, 8, "mesh_left_hand");
            infoAboutLocations[UnitCustomizationLocation.FrameRightHand] = new LocationInfo (true, 9, "mesh_right_hand");
            infoAboutLocations[UnitCustomizationLocation.FrameLeftThigh] = new LocationInfo (true, 10, "mesh_left_thigh");
            infoAboutLocations[UnitCustomizationLocation.FrameRightThigh] = new LocationInfo (true, 11, "mesh_right_thigh");
            infoAboutLocations[UnitCustomizationLocation.FrameLeftLeg] = new LocationInfo (true, 12, "mesh_left_leg");
            infoAboutLocations[UnitCustomizationLocation.FrameRightLeg] = new LocationInfo (true, 13, "mesh_right_leg");
            infoAboutLocations[UnitCustomizationLocation.FrameLeftFoot] = new LocationInfo (true, 14, "mesh_left_foot");
            infoAboutLocations[UnitCustomizationLocation.FrameRightFoot] = new LocationInfo (true, 1, "mesh_right_foot");
            infoAboutLocations[UnitCustomizationLocation.WeaponLeft] = new LocationInfo (false, 0, "hardpoint_left_weapon");
            infoAboutLocations[UnitCustomizationLocation.WeaponRight] = new LocationInfo (false, 0, "hardpoint_right_weapon");
        }

        if (infoAboutLocations.ContainsKey (location))
            return infoAboutLocations[location];
        else
            return null;
    }

    public static readonly int locationShaderArraySize = 30;

    private static Dictionary<UnitCustomizationLocation, int> arrayIndexesForLocations;
    public static int GetLocationIndexInArray (UnitCustomizationLocation location)
    {
        if (arrayIndexesForLocations == null)
        {
            arrayIndexesForLocations = new Dictionary<UnitCustomizationLocation, int> ();
            UnitCustomizationLocation[] locations = (UnitCustomizationLocation[])Enum.GetValues (typeof (UnitCustomizationLocation));
            for (int i = 0; i < locations.Length; ++i)
                arrayIndexesForLocations.Add (locations[i], 0);

            /*
            ArmorHead = 0,
            ArmorTorso = 1,
            ArmorPelvis = 2,
            ArmorLeftShoulder = 3,
            ArmorRightShoulder = 4,
            ArmorLeftForearm = 5,
            ArmorRightForearm = 6,
            ArmorLeftThigh = 7,
            ArmorRightThigh = 8,
            ArmorLeftLeg = 9,
            ArmorRightLeg = 10,
            ArmorLeftFoot = 11,
            ArmorRightFoot = 12,
            FrameHead = 13,
            FrameTorso = 14,
            FramePelvis = 15,
            FrameLeftShoulder = 16,
            FrameRightShoulder = 17,
            FrameLeftForearm = 18,
            FrameRightForearm = 19,
            FrameLeftHand = 20,
            FrameRightHand = 21,
            FrameLeftThigh = 22,
            FrameRightThigh = 23,
            FrameLeftLeg = 24,
            FrameRightLeg = 25,
            FrameLeftFoot = 26,
            FrameRightFoot = 27,
            WeaponLeft = 28,
            WeaponRight = 29,
            ArmorPod = 50
            */

            arrayIndexesForLocations[UnitCustomizationLocation.ArmorHead] = 0;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorTorso] = 1;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorPelvis] = 2;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorPod] = 1;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorLeftShoulder] = 3;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorRightShoulder] = 4;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorLeftForearm] = 5;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorRightForearm] = 6;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorLeftThigh] = 7;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorRightThigh] = 8;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorLeftLeg] = 9;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorRightLeg] = 10;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorLeftFoot] = 11;
            arrayIndexesForLocations[UnitCustomizationLocation.ArmorRightFoot] = 12;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameHead] = 13;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameTorso] = 14;
            arrayIndexesForLocations[UnitCustomizationLocation.FramePelvis] = 15;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameLeftShoulder] = 16;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameRightShoulder] = 17;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameLeftForearm] = 18;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameRightForearm] = 19;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameLeftHand] = 20;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameRightHand] = 21;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameLeftThigh] = 22;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameRightThigh] = 23;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameLeftLeg] = 24;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameRightLeg] = 25;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameLeftFoot] = 26;
            arrayIndexesForLocations[UnitCustomizationLocation.FrameRightFoot] = 27;
            arrayIndexesForLocations[UnitCustomizationLocation.WeaponLeft] = 28;
            arrayIndexesForLocations[UnitCustomizationLocation.WeaponRight] = 29;
        }

        if (arrayIndexesForLocations.ContainsKey (location))
            return arrayIndexesForLocations[location];
        else
            return 0;
    }

    private static Dictionary<UnitArea, List<string>> transformNamesForAreaType;
    public static List<string> GetTransformNamesForAreaType (UnitArea areaType)
    {
        if (transformNamesForAreaType == null)
        {
            transformNamesForAreaType = new Dictionary<UnitArea, List<string>> ();
            transformNamesForAreaType.Add (UnitArea.CorePrimary, new List<string> (new string[] { "joint_pelvis_xyz" }));
            transformNamesForAreaType.Add (UnitArea.LeftOptional, new List<string> (new string[] { "joint_left_arm_xyz" }));
            transformNamesForAreaType.Add (UnitArea.RightOptional, new List<string> (new string[] { "joint_right_arm_xyz" }));
            transformNamesForAreaType.Add (UnitArea.CoreSecondary, new List<string> (new string[] { "joint_left_thigh_xyz", "joint_right_thigh_xyz" }));
        }

        if (transformNamesForAreaType.ContainsKey (areaType))
            return transformNamesForAreaType[areaType];
        else
            return null;
    }

    private static Dictionary<UnitHardpoint, string> transformNamesHardpoints;
    public static string GetTransformNameForHardpoint (UnitHardpoint hardpoint)
    {
        if (transformNamesHardpoints == null)
        {
            transformNamesHardpoints = new Dictionary<UnitHardpoint, string> ();
            transformNamesHardpoints.Add (UnitHardpoint.InternalHead, "hardpoint_internal_head");
            transformNamesHardpoints.Add (UnitHardpoint.InternalReactor, "hardpoint_internal_reactor");
            transformNamesHardpoints.Add (UnitHardpoint.InternalPod, "hardpoint_internal_pod");
            transformNamesHardpoints.Add (UnitHardpoint.InternalLeftArm, "hardpoint_internal_left_arm");
            transformNamesHardpoints.Add (UnitHardpoint.InternalRightArm, "hardpoint_internal_right_arm");
            transformNamesHardpoints.Add (UnitHardpoint.InternalLeftLeg, "hardpoint_internal_left_leg");
            transformNamesHardpoints.Add (UnitHardpoint.InternalRightLeg, "hardpoint_internal_right_leg");
            transformNamesHardpoints.Add (UnitHardpoint.InternalMagazine, "hardpoint_internal_magazine");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorHead, "hardpoint_head");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorTorso, "hardpoint_torso");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorPelvis, "hardpoint_pelvis");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorPod, "hardpoint_pod");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorLeftShoulder, "hardpoint_left_arm");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorRightShoulder, "hardpoint_right_arm");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorLeftForearm, "hardpoint_left_forearm");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorRightForearm, "hardpoint_right_forearm");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorLeftThigh, "hardpoint_left_thigh");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorRightThigh, "hardpoint_right_thigh");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorLeftLeg, "hardpoint_left_leg");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorRightLeg, "hardpoint_right_leg");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorLeftFoot, "hardpoint_left_foot");
            transformNamesHardpoints.Add (UnitHardpoint.ArmorRightFoot, "hardpoint_right_foot");
            transformNamesHardpoints.Add (UnitHardpoint.WeaponLeft, "hardpoint_left_weapon");
            transformNamesHardpoints.Add (UnitHardpoint.WeaponRight, "hardpoint_right_weapon");
        }

        if (transformNamesHardpoints.ContainsKey (hardpoint))
            return transformNamesHardpoints[hardpoint];
        else
            return null;
    }
    
    


    public class SocketLocationLink
    {
        public string socket;
        public int socketHash;
        public UnitCustomizationLocation location;
        public float damageVisualWeight;
        
        public SocketLocationLink (string socket, UnitCustomizationLocation location, float damageVisualWeight = 1f)
        {
            this.socket = socket;
            this.socketHash = socket.GetHashCode ();
            this.location = location;
            this.damageVisualWeight = Mathf.Clamp01 (damageVisualWeight);
        }
    }
    
    private static void AddSocketLocationLink (string socket, UnitCustomizationLocation location, float damageVisualWeight = 1f)
    {
        if (!socketLocationLinks.ContainsKey (socket))
            socketLocationLinks.Add (socket, new List<SocketLocationLink> ());
            
        socketLocationLinks[socket].Add (new SocketLocationLink (socket, location, damageVisualWeight));
    }

    private static Dictionary<string, List<SocketLocationLink>> socketLocationLinks;
    private static Dictionary<int, List<SocketLocationLink>> socketHashLocationLinks;

    private static void CheckSocketLocationLinks ()
    {
        if (socketLocationLinks == null)
        {
            socketLocationLinks = new Dictionary<string, List<SocketLocationLink>> ();

            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.FrameHead);
            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.FrameTorso);
            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.FramePelvis);
            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.ArmorHead);
            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.ArmorTorso);
            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.ArmorPelvis);
            AddSocketLocationLink (LoadoutSockets.corePart, UnitCustomizationLocation.ArmorPod);

            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.FrameLeftThigh);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.FrameRightThigh);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.FrameLeftLeg);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.FrameRightLeg);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.FrameLeftFoot);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.FrameRightFoot);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.ArmorLeftThigh);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.ArmorRightThigh);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.ArmorLeftLeg);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.ArmorRightLeg);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.ArmorLeftFoot);
            AddSocketLocationLink (LoadoutSockets.secondaryPart, UnitCustomizationLocation.ArmorRightFoot);

            AddSocketLocationLink (LoadoutSockets.leftOptionalPart, UnitCustomizationLocation.FrameLeftShoulder);
            AddSocketLocationLink (LoadoutSockets.leftOptionalPart, UnitCustomizationLocation.FrameLeftForearm);
            AddSocketLocationLink (LoadoutSockets.leftOptionalPart, UnitCustomizationLocation.FrameLeftHand);
            AddSocketLocationLink (LoadoutSockets.leftOptionalPart, UnitCustomizationLocation.ArmorLeftShoulder);
            AddSocketLocationLink (LoadoutSockets.leftOptionalPart, UnitCustomizationLocation.ArmorLeftForearm);

            AddSocketLocationLink (LoadoutSockets.rightOptionalPart, UnitCustomizationLocation.FrameRightShoulder);
            AddSocketLocationLink (LoadoutSockets.rightOptionalPart, UnitCustomizationLocation.FrameRightForearm);
            AddSocketLocationLink (LoadoutSockets.rightOptionalPart, UnitCustomizationLocation.FrameRightHand);
            AddSocketLocationLink (LoadoutSockets.rightOptionalPart, UnitCustomizationLocation.ArmorRightShoulder);
            AddSocketLocationLink (LoadoutSockets.rightOptionalPart, UnitCustomizationLocation.ArmorRightForearm);

            AddSocketLocationLink (LoadoutSockets.leftEquipment, UnitCustomizationLocation.WeaponLeft);
            AddSocketLocationLink (LoadoutSockets.rightEquipment, UnitCustomizationLocation.WeaponRight);

            socketHashLocationLinks = new Dictionary<int, List<SocketLocationLink>> (socketLocationLinks.Count);
            foreach (var kvp in socketLocationLinks)
                socketHashLocationLinks.Add (kvp.Key.GetHashCode (), kvp.Value);
        }
    }

    public static List<SocketLocationLink> GetSocketLocationLinks (string socket)
    {
        CheckSocketLocationLinks ();
        if (socketLocationLinks.ContainsKey (socket))
            return socketLocationLinks[socket];
        else
            return null;
    }

    public static List<SocketLocationLink> GetSocketLocationLinks (int socketHash)
    {
        CheckSocketLocationLinks ();
        if (socketHashLocationLinks.ContainsKey (socketHash))
            return socketHashLocationLinks[socketHash];
        else
            return null;
    }






    public class VisualDamageConnection
    {
        public float weight;
        public UnitHardpoint hardpoint;
        public UnitCustomizationLocation location;

        // public bool ignoreUnit;
        // public float integrityOverride;

        public VisualDamageConnection (float weight, UnitHardpoint hardpoint, UnitCustomizationLocation location)
        {
            this.weight = weight;
            this.hardpoint = hardpoint;
            this.location = location;
        }
    }

    private static Dictionary<UnitHardpoint, List<VisualDamageConnection>> weightsPerHardpoint;
    private static Dictionary<UnitCustomizationLocation, List<VisualDamageConnection>> weightsPerLocation;
    private static void SetupWeights ()
    {
        if (weightsPerHardpoint == null || weightsPerLocation == null)
        {
            weightsPerHardpoint = new Dictionary<UnitHardpoint, List<VisualDamageConnection>> ();
            UnitHardpoint[] hardpoints = (UnitHardpoint[])Enum.GetValues (typeof (UnitHardpoint));
            for (int i = 0; i < hardpoints.Length; ++i)
            {
                weightsPerHardpoint.Add (hardpoints[i], new List<VisualDamageConnection> ());
            }

            AddWeightForHardpoint (UnitHardpoint.InternalHead, 1f, UnitCustomizationLocation.FrameHead);

            AddWeightForHardpoint (UnitHardpoint.InternalReactor, 1f, UnitCustomizationLocation.FrameTorso);

            AddWeightForHardpoint (UnitHardpoint.InternalPod, 1f, UnitCustomizationLocation.FrameTorso);

            AddWeightForHardpoint (UnitHardpoint.InternalLeftArm, 1f, UnitCustomizationLocation.FrameLeftShoulder);
            AddWeightForHardpoint (UnitHardpoint.InternalLeftArm, 1f, UnitCustomizationLocation.FrameLeftForearm);

            AddWeightForHardpoint (UnitHardpoint.InternalRightArm, 1f, UnitCustomizationLocation.FrameRightShoulder);
            AddWeightForHardpoint (UnitHardpoint.InternalRightArm, 1f, UnitCustomizationLocation.FrameRightForearm);

            AddWeightForHardpoint (UnitHardpoint.InternalLeftLeg, 1f, UnitCustomizationLocation.FramePelvis);
            AddWeightForHardpoint (UnitHardpoint.InternalLeftLeg, 1f, UnitCustomizationLocation.FrameLeftThigh);
            AddWeightForHardpoint (UnitHardpoint.InternalLeftLeg, 1f, UnitCustomizationLocation.FrameLeftLeg);
            AddWeightForHardpoint (UnitHardpoint.InternalLeftLeg, 1f, UnitCustomizationLocation.FrameLeftFoot);

            AddWeightForHardpoint (UnitHardpoint.InternalRightLeg, 1f, UnitCustomizationLocation.FramePelvis);
            AddWeightForHardpoint (UnitHardpoint.InternalRightLeg, 1f, UnitCustomizationLocation.FrameRightThigh);
            AddWeightForHardpoint (UnitHardpoint.InternalRightLeg, 1f, UnitCustomizationLocation.FrameRightLeg);
            AddWeightForHardpoint (UnitHardpoint.InternalRightLeg, 1f, UnitCustomizationLocation.FrameRightFoot);

            AddWeightForHardpoint (UnitHardpoint.ArmorHead, 1f, UnitCustomizationLocation.ArmorHead);
            AddWeightForHardpoint (UnitHardpoint.ArmorHead, 0.2f, UnitCustomizationLocation.FrameHead);

            AddWeightForHardpoint (UnitHardpoint.ArmorTorso, 1f, UnitCustomizationLocation.ArmorTorso);
            AddWeightForHardpoint (UnitHardpoint.ArmorTorso, 0.2f, UnitCustomizationLocation.FrameTorso);

            AddWeightForHardpoint (UnitHardpoint.ArmorPelvis, 1f, UnitCustomizationLocation.ArmorPelvis);
            AddWeightForHardpoint (UnitHardpoint.ArmorPelvis, 0.2f, UnitCustomizationLocation.FramePelvis);

            AddWeightForHardpoint (UnitHardpoint.ArmorPod, 1f, UnitCustomizationLocation.ArmorPod);

            AddWeightForHardpoint (UnitHardpoint.ArmorLeftShoulder, 1f, UnitCustomizationLocation.ArmorLeftShoulder);
            AddWeightForHardpoint (UnitHardpoint.ArmorLeftShoulder, 0.2f, UnitCustomizationLocation.FrameLeftShoulder);

            AddWeightForHardpoint (UnitHardpoint.ArmorRightShoulder, 1f, UnitCustomizationLocation.ArmorRightShoulder);
            AddWeightForHardpoint (UnitHardpoint.ArmorRightShoulder, 0.2f, UnitCustomizationLocation.FrameRightShoulder);

            AddWeightForHardpoint (UnitHardpoint.ArmorLeftForearm, 1f, UnitCustomizationLocation.ArmorLeftForearm);
            AddWeightForHardpoint (UnitHardpoint.ArmorLeftForearm, 0.2f, UnitCustomizationLocation.FrameLeftForearm);

            AddWeightForHardpoint (UnitHardpoint.ArmorRightForearm, 1f, UnitCustomizationLocation.ArmorRightForearm);
            AddWeightForHardpoint (UnitHardpoint.ArmorRightForearm, 0.2f, UnitCustomizationLocation.FrameRightForearm);

            AddWeightForHardpoint (UnitHardpoint.ArmorLeftThigh, 1f, UnitCustomizationLocation.ArmorLeftThigh);
            AddWeightForHardpoint (UnitHardpoint.ArmorLeftThigh, 0.2f, UnitCustomizationLocation.FrameLeftThigh);

            AddWeightForHardpoint (UnitHardpoint.ArmorRightThigh, 1f, UnitCustomizationLocation.ArmorRightThigh);
            AddWeightForHardpoint (UnitHardpoint.ArmorRightThigh, 0.2f, UnitCustomizationLocation.FrameRightThigh);

            AddWeightForHardpoint (UnitHardpoint.ArmorLeftLeg, 1f, UnitCustomizationLocation.ArmorLeftLeg);
            AddWeightForHardpoint (UnitHardpoint.ArmorLeftLeg, 0.2f, UnitCustomizationLocation.FrameLeftLeg);

            AddWeightForHardpoint (UnitHardpoint.ArmorRightLeg, 1f, UnitCustomizationLocation.ArmorRightLeg);
            AddWeightForHardpoint (UnitHardpoint.ArmorRightLeg, 0.2f, UnitCustomizationLocation.FrameRightLeg);

            AddWeightForHardpoint (UnitHardpoint.ArmorLeftFoot, 1f, UnitCustomizationLocation.ArmorLeftFoot);
            AddWeightForHardpoint (UnitHardpoint.ArmorLeftFoot, 0.2f, UnitCustomizationLocation.FrameLeftFoot);

            AddWeightForHardpoint (UnitHardpoint.ArmorRightFoot, 1f, UnitCustomizationLocation.ArmorRightFoot);
            AddWeightForHardpoint (UnitHardpoint.ArmorRightFoot, 0.2f, UnitCustomizationLocation.FrameRightFoot);

            AddWeightForHardpoint (UnitHardpoint.WeaponLeft, 1f, UnitCustomizationLocation.WeaponLeft);
            AddWeightForHardpoint (UnitHardpoint.WeaponLeft, 1f, UnitCustomizationLocation.FrameLeftHand);

            AddWeightForHardpoint (UnitHardpoint.WeaponRight, 1f, UnitCustomizationLocation.WeaponRight);
            AddWeightForHardpoint (UnitHardpoint.WeaponRight, 1f, UnitCustomizationLocation.FrameRightHand);

            weightsPerLocation = new Dictionary<UnitCustomizationLocation, List<VisualDamageConnection>> ();
            UnitCustomizationLocation[] locations = (UnitCustomizationLocation[])Enum.GetValues (typeof (UnitCustomizationLocation));
            for (int i = 0; i < locations.Length; ++i)
                weightsPerLocation.Add (locations[i], new List<VisualDamageConnection> ());

            foreach (KeyValuePair<UnitHardpoint, List<VisualDamageConnection>> kvp in weightsPerHardpoint)
            {
                List<VisualDamageConnection> connectionsFromHardpoint = kvp.Value;
                for (int i = 0; i < connectionsFromHardpoint.Count; ++i)
                {
                    VisualDamageConnection weightData = connectionsFromHardpoint[i];
                    weightsPerLocation[weightData.location].Add (weightData);
                }
            }

            // Todo: Add unit-ignoring weight data with integrity override of 1f to make certain meshes never rendered at full damage
        }
    }

    private static void AddWeightForHardpoint (UnitHardpoint hardpoint, float weight, UnitCustomizationLocation location)
    {
        weightsPerHardpoint[hardpoint].Add (new VisualDamageConnection (1f, hardpoint, location));
    }

    public static List<VisualDamageConnection> GetHardpointsInfluencingLocation (UnitCustomizationLocation location)
    {
        SetupWeights ();
        return weightsPerLocation[location];
    }

    public static List<VisualDamageConnection> GetLocationsInfluencedByHardpoint (UnitHardpoint hardpoint)
    {
        SetupWeights ();
        return weightsPerHardpoint[hardpoint];
    }

    private static Dictionary<UnitHardpoint, UnitCustomizationLocation> locationsForHardpoints;
    public static UnitCustomizationLocation GetCustomizationLocationForHardpoint (UnitHardpoint hardpoint, out bool found)
    {
        if (locationsForHardpoints == null)
        {
            locationsForHardpoints = new Dictionary<UnitHardpoint, UnitCustomizationLocation> ();

            // locationsForHardpoints.Add (UnitHardpoint.InternalHead, UnitCustomizationLocation.FrameHead);
            // locationsForHardpoints.Add (UnitHardpoint.InternalReactor, UnitCustomizationLocation.FrameTorso);
            // locationsForHardpoints.Add (UnitHardpoint.InternalPod, UnitCustomizationLocation.FramePelvis);
            // locationsForHardpoints.Add (UnitHardpoint.InternalLeftArm, UnitCustomizationLocation.FrameLeftShoulder);
            // locationsForHardpoints.Add (UnitHardpoint.InternalRightArm, UnitCustomizationLocation.FrameRightShoulder);
            // locationsForHardpoints.Add (UnitHardpoint.InternalLeftLeg, UnitCustomizationLocation.FrameLeftThigh);
            // locationsForHardpoints.Add (UnitHardpoint.InternalRightLeg, UnitCustomizationLocation.FrameRightThigh);

            locationsForHardpoints.Add (UnitHardpoint.ArmorHead, UnitCustomizationLocation.ArmorHead);
            locationsForHardpoints.Add (UnitHardpoint.ArmorTorso, UnitCustomizationLocation.ArmorTorso);
            locationsForHardpoints.Add (UnitHardpoint.ArmorPelvis, UnitCustomizationLocation.ArmorPelvis);
            locationsForHardpoints.Add (UnitHardpoint.ArmorPod, UnitCustomizationLocation.ArmorPod);
            locationsForHardpoints.Add (UnitHardpoint.ArmorLeftShoulder, UnitCustomizationLocation.ArmorLeftShoulder);
            locationsForHardpoints.Add (UnitHardpoint.ArmorRightShoulder, UnitCustomizationLocation.ArmorRightShoulder);
            locationsForHardpoints.Add (UnitHardpoint.ArmorLeftForearm, UnitCustomizationLocation.ArmorLeftForearm);
            locationsForHardpoints.Add (UnitHardpoint.ArmorRightForearm, UnitCustomizationLocation.ArmorRightForearm);
            locationsForHardpoints.Add (UnitHardpoint.ArmorLeftThigh, UnitCustomizationLocation.ArmorLeftThigh);
            locationsForHardpoints.Add (UnitHardpoint.ArmorRightThigh, UnitCustomizationLocation.ArmorRightThigh);
            locationsForHardpoints.Add (UnitHardpoint.ArmorLeftLeg, UnitCustomizationLocation.ArmorLeftLeg);
            locationsForHardpoints.Add (UnitHardpoint.ArmorRightLeg, UnitCustomizationLocation.ArmorRightLeg);
            locationsForHardpoints.Add (UnitHardpoint.ArmorLeftFoot, UnitCustomizationLocation.ArmorLeftFoot);
            locationsForHardpoints.Add (UnitHardpoint.ArmorRightFoot, UnitCustomizationLocation.ArmorRightFoot);
            locationsForHardpoints.Add (UnitHardpoint.WeaponLeft, UnitCustomizationLocation.WeaponLeft);
            locationsForHardpoints.Add (UnitHardpoint.WeaponRight, UnitCustomizationLocation.WeaponRight);

        }

        if (locationsForHardpoints.ContainsKey (hardpoint))
        {
            found = true;
            return locationsForHardpoints[hardpoint];
        }
        else
        {
            found = false;
            return UnitCustomizationLocation.ArmorHead;
        }
    }

    public enum UnitCustomizationLocationSubset
    {
        Full,
        ArmorOnly,
        WeaponsOnly,
        HardpointsOnly,
        FrameOnly,
    }

    private static Dictionary<UnitArea, Dictionary<UnitCustomizationLocation, Vector2>> damageDistributionsForAreas;
    public static Dictionary<UnitCustomizationLocation, Vector2> GetRandomDamageDistributionForArea (UnitArea areaType)
    {
        if (damageDistributionsForAreas == null)
        {
            damageDistributionsForAreas = new Dictionary<UnitArea, Dictionary<UnitCustomizationLocation, Vector2>> ();
            UnitArea[] areaTypeValues = (UnitArea[])Enum.GetValues (typeof (UnitArea));

            // Create the foundation - all locations covered with uniform values, guaranteed key coverage
            for (int a = 0; a < areaTypeValues.Length; ++a)
            {
                UnitArea areaTypeValue = areaTypeValues[a];
                Dictionary<UnitCustomizationLocation, Vector2> damageDistribution = new Dictionary<UnitCustomizationLocation, Vector2> ();
                damageDistributionsForAreas.Add (areaTypeValue, damageDistribution);

                List<UnitCustomizationLocation> locationsFull = GetLocationsForAreaType (areaTypeValue, UnitCustomizationLocationSubset.Full);
                for (int i = 0; i < locationsFull.Count; ++i)
                    damageDistribution.Add (locationsFull[i], new Vector2 (0.5f, 1f));

                List<UnitCustomizationLocation> locationsFrame = GetLocationsForAreaType (areaTypeValue, UnitCustomizationLocationSubset.FrameOnly);
                for (int f = 0; f < locationsFrame.Count; ++f)
                    damageDistribution[locationsFrame[f]] = new Vector2 (0.4f, 1f);

                List<UnitCustomizationLocation> locationsHardpoints = GetLocationsForAreaType (areaTypeValue, UnitCustomizationLocationSubset.HardpointsOnly);
                for (int h = 0; h < locationsHardpoints.Count; ++h)
                    damageDistribution[locationsHardpoints[h]] = new Vector2 (0.3f, 1f);
            }

            Dictionary<UnitCustomizationLocation, Vector2> damageDistributionLowerBody = damageDistributionsForAreas[UnitArea.CoreSecondary];
            damageDistributionLowerBody[UnitCustomizationLocation.FrameLeftThigh] = new Vector2 (0.65f, 0f);
            damageDistributionLowerBody[UnitCustomizationLocation.FrameRightThigh] = new Vector2 (0.55f, 0f);
            damageDistributionLowerBody[UnitCustomizationLocation.FrameLeftLeg] = new Vector2 (0.35f, 0f);
            damageDistributionLowerBody[UnitCustomizationLocation.FrameRightLeg] = new Vector2 (0.45f, 0f);
            damageDistributionLowerBody[UnitCustomizationLocation.FrameLeftFoot] = new Vector2 (0.5f, 1f);
            damageDistributionLowerBody[UnitCustomizationLocation.FrameRightFoot] = new Vector2 (0.6f, 0f);

            damageDistributionLowerBody[UnitCustomizationLocation.ArmorLeftThigh] = new Vector2 (0.55f, 0f);
            damageDistributionLowerBody[UnitCustomizationLocation.ArmorRightThigh] = new Vector2 (0.25f, 1f);
            damageDistributionLowerBody[UnitCustomizationLocation.ArmorLeftLeg] = new Vector2 (0.35f, 1f);
            damageDistributionLowerBody[UnitCustomizationLocation.ArmorRightLeg] = new Vector2 (0.5f, 0f);
            damageDistributionLowerBody[UnitCustomizationLocation.ArmorLeftFoot] = new Vector2 (0.4f, 1f);
            damageDistributionLowerBody[UnitCustomizationLocation.ArmorRightFoot] = new Vector2 (0.5f, 0f);
        }

        if (damageDistributionsForAreas.ContainsKey (areaType))
            return damageDistributionsForAreas[areaType];
        else
            return null;
    }

    private static Dictionary<UnitCustomizationLocationSubset, Dictionary<UnitArea, List<UnitCustomizationLocation>>> locationsForAreasForSubsets;
    private static List<UnitCustomizationLocation> locationsForAreasForSubsetsNotFound;

    public static List<UnitCustomizationLocation> GetLocationsForAreaType (UnitArea area, UnitCustomizationLocationSubset subset)
    {
        if (locationsForAreasForSubsets == null)
        {
            locationsForAreasForSubsets = new Dictionary<UnitCustomizationLocationSubset, Dictionary<UnitArea, List<UnitCustomizationLocation>>> ();
            locationsForAreasForSubsetsNotFound = new List<UnitCustomizationLocation> ();

            Dictionary<UnitArea, List<UnitCustomizationLocation>> locationsForAreasFull = new Dictionary<UnitArea, List<UnitCustomizationLocation>> ();
            locationsForAreasForSubsets.Add (UnitCustomizationLocationSubset.Full, locationsForAreasFull);

            List<UnitCustomizationLocation> locationsForUpperBodyFull = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLeftArmFull = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForRightArmFull = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLowerBodyFull = new List<UnitCustomizationLocation> ();

            locationsForAreasFull.Add (UnitArea.CorePrimary, locationsForUpperBodyFull);
            locationsForAreasFull.Add (UnitArea.LeftOptional, locationsForLeftArmFull);
            locationsForAreasFull.Add (UnitArea.RightOptional, locationsForRightArmFull);
            locationsForAreasFull.Add (UnitArea.CoreSecondary, locationsForLowerBodyFull);

            locationsForUpperBodyFull.Add (UnitCustomizationLocation.FrameHead);
            locationsForUpperBodyFull.Add (UnitCustomizationLocation.FrameTorso);
            locationsForUpperBodyFull.Add (UnitCustomizationLocation.FramePelvis);
            locationsForUpperBodyFull.Add (UnitCustomizationLocation.ArmorHead);
            locationsForUpperBodyFull.Add (UnitCustomizationLocation.ArmorTorso);
            locationsForUpperBodyFull.Add (UnitCustomizationLocation.ArmorPelvis);
            locationsForUpperBodyFull.Add (UnitCustomizationLocation.ArmorPod);

            locationsForLeftArmFull.Add (UnitCustomizationLocation.FrameLeftShoulder);
            locationsForLeftArmFull.Add (UnitCustomizationLocation.FrameLeftForearm);
            locationsForLeftArmFull.Add (UnitCustomizationLocation.FrameLeftHand);
            locationsForLeftArmFull.Add (UnitCustomizationLocation.ArmorLeftShoulder);
            locationsForLeftArmFull.Add (UnitCustomizationLocation.ArmorLeftForearm);
            locationsForLeftArmFull.Add (UnitCustomizationLocation.WeaponLeft);

            locationsForRightArmFull.Add (UnitCustomizationLocation.FrameRightShoulder);
            locationsForRightArmFull.Add (UnitCustomizationLocation.FrameRightForearm);
            locationsForRightArmFull.Add (UnitCustomizationLocation.FrameRightHand);
            locationsForRightArmFull.Add (UnitCustomizationLocation.ArmorRightShoulder);
            locationsForRightArmFull.Add (UnitCustomizationLocation.ArmorRightForearm);
            locationsForRightArmFull.Add (UnitCustomizationLocation.WeaponRight);

            locationsForLowerBodyFull.Add (UnitCustomizationLocation.FrameLeftThigh);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.FrameRightThigh);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.FrameLeftLeg);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.FrameRightLeg);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.FrameLeftFoot);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.FrameRightFoot);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.ArmorLeftThigh);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.ArmorRightThigh);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.ArmorLeftLeg);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.ArmorRightLeg);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.ArmorLeftFoot);
            locationsForLowerBodyFull.Add (UnitCustomizationLocation.ArmorRightFoot);

            Dictionary<UnitArea, List<UnitCustomizationLocation>> locationsForAreasArmor = new Dictionary<UnitArea, List<UnitCustomizationLocation>> ();
            locationsForAreasForSubsets.Add (UnitCustomizationLocationSubset.ArmorOnly, locationsForAreasArmor);

            List<UnitCustomizationLocation> locationsForUpperBodyArmor = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLeftArmArmor = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForRightArmArmor = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLowerBodyArmor = new List<UnitCustomizationLocation> ();

            locationsForAreasArmor.Add (UnitArea.CorePrimary, locationsForUpperBodyArmor);
            locationsForAreasArmor.Add (UnitArea.LeftOptional, locationsForLeftArmArmor);
            locationsForAreasArmor.Add (UnitArea.RightOptional, locationsForRightArmArmor);
            locationsForAreasArmor.Add (UnitArea.CoreSecondary, locationsForLowerBodyArmor);

            locationsForUpperBodyArmor.Add (UnitCustomizationLocation.ArmorHead);
            locationsForUpperBodyArmor.Add (UnitCustomizationLocation.ArmorTorso);
            locationsForUpperBodyArmor.Add (UnitCustomizationLocation.ArmorPelvis);
            locationsForUpperBodyArmor.Add (UnitCustomizationLocation.ArmorPod);

            locationsForLeftArmArmor.Add (UnitCustomizationLocation.ArmorLeftShoulder);
            locationsForLeftArmArmor.Add (UnitCustomizationLocation.ArmorLeftForearm);

            locationsForRightArmArmor.Add (UnitCustomizationLocation.ArmorRightShoulder);
            locationsForRightArmArmor.Add (UnitCustomizationLocation.ArmorRightForearm);

            locationsForLowerBodyArmor.Add (UnitCustomizationLocation.ArmorLeftThigh);
            locationsForLowerBodyArmor.Add (UnitCustomizationLocation.ArmorRightThigh);
            locationsForLowerBodyArmor.Add (UnitCustomizationLocation.ArmorLeftLeg);
            locationsForLowerBodyArmor.Add (UnitCustomizationLocation.ArmorRightLeg);
            locationsForLowerBodyArmor.Add (UnitCustomizationLocation.ArmorLeftFoot);
            locationsForLowerBodyArmor.Add (UnitCustomizationLocation.ArmorRightFoot);

            Dictionary<UnitArea, List<UnitCustomizationLocation>> locationsForAreasWeapons = new Dictionary<UnitArea, List<UnitCustomizationLocation>> ();
            locationsForAreasForSubsets.Add (UnitCustomizationLocationSubset.WeaponsOnly, locationsForAreasWeapons);

            List<UnitCustomizationLocation> locationsForLeftArmWeapons = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForRightArmWeapons = new List<UnitCustomizationLocation> ();

            locationsForAreasWeapons.Add (UnitArea.LeftOptional, locationsForLeftArmWeapons);
            locationsForAreasWeapons.Add (UnitArea.RightOptional, locationsForRightArmWeapons);

            locationsForLeftArmWeapons.Add (UnitCustomizationLocation.WeaponLeft);
            locationsForRightArmWeapons.Add (UnitCustomizationLocation.WeaponRight);

            Dictionary<UnitArea, List<UnitCustomizationLocation>> locationsForAreasHardpoints = new Dictionary<UnitArea, List<UnitCustomizationLocation>> ();
            locationsForAreasForSubsets.Add (UnitCustomizationLocationSubset.HardpointsOnly, locationsForAreasHardpoints);

            List<UnitCustomizationLocation> locationsForUpperBodyHardpoints = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLeftArmHardpoints = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForRightArmHardpoints = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLowerBodyHardpoints = new List<UnitCustomizationLocation> ();

            locationsForAreasHardpoints.Add (UnitArea.CorePrimary, locationsForUpperBodyHardpoints);
            locationsForAreasHardpoints.Add (UnitArea.LeftOptional, locationsForLeftArmHardpoints);
            locationsForAreasHardpoints.Add (UnitArea.RightOptional, locationsForRightArmHardpoints);
            locationsForAreasHardpoints.Add (UnitArea.CoreSecondary, locationsForLowerBodyHardpoints);

            foreach (KeyValuePair<UnitArea, List<UnitCustomizationLocation>> kvp in locationsForAreasArmor)
            {
                for (int i = 0; i < kvp.Value.Count; ++i)
                {
                    if (!locationsForAreasHardpoints[kvp.Key].Contains (kvp.Value[i]))
                        locationsForAreasHardpoints[kvp.Key].Add (kvp.Value[i]);
                }
            }

            foreach (KeyValuePair<UnitArea, List<UnitCustomizationLocation>> kvp in locationsForAreasWeapons)
            {
                for (int i = 0; i < kvp.Value.Count; ++i)
                {
                    if (!locationsForAreasHardpoints[kvp.Key].Contains (kvp.Value[i]))
                        locationsForAreasHardpoints[kvp.Key].Add (kvp.Value[i]);
                }
            }

            Dictionary<UnitArea, List<UnitCustomizationLocation>> locationsForAreasFrame = new Dictionary<UnitArea, List<UnitCustomizationLocation>> ();
            locationsForAreasForSubsets.Add (UnitCustomizationLocationSubset.FrameOnly, locationsForAreasFrame);

            List<UnitCustomizationLocation> locationsForUpperBodyFrame = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLeftArmFrame = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForRightArmFrame = new List<UnitCustomizationLocation> ();
            List<UnitCustomizationLocation> locationsForLowerBodyFrame = new List<UnitCustomizationLocation> ();

            locationsForAreasFrame.Add (UnitArea.CorePrimary, locationsForUpperBodyFrame);
            locationsForAreasFrame.Add (UnitArea.LeftOptional, locationsForLeftArmFrame);
            locationsForAreasFrame.Add (UnitArea.RightOptional, locationsForRightArmFrame);
            locationsForAreasFrame.Add (UnitArea.CoreSecondary, locationsForLowerBodyFrame);

            locationsForUpperBodyFrame.Add (UnitCustomizationLocation.FrameHead);
            locationsForUpperBodyFrame.Add (UnitCustomizationLocation.FrameTorso);
            locationsForUpperBodyFrame.Add (UnitCustomizationLocation.FramePelvis);

            locationsForLeftArmFrame.Add (UnitCustomizationLocation.FrameLeftShoulder);
            locationsForLeftArmFrame.Add (UnitCustomizationLocation.FrameLeftForearm);
            locationsForLeftArmFrame.Add (UnitCustomizationLocation.FrameLeftHand);

            locationsForRightArmFrame.Add (UnitCustomizationLocation.FrameRightShoulder);
            locationsForRightArmFrame.Add (UnitCustomizationLocation.FrameRightForearm);
            locationsForRightArmFrame.Add (UnitCustomizationLocation.FrameRightHand);

            locationsForLowerBodyFrame.Add (UnitCustomizationLocation.FrameLeftThigh);
            locationsForLowerBodyFrame.Add (UnitCustomizationLocation.FrameRightThigh);
            locationsForLowerBodyFrame.Add (UnitCustomizationLocation.FrameLeftLeg);
            locationsForLowerBodyFrame.Add (UnitCustomizationLocation.FrameRightLeg);
            locationsForLowerBodyFrame.Add (UnitCustomizationLocation.FrameLeftFoot);
            locationsForLowerBodyFrame.Add (UnitCustomizationLocation.FrameRightFoot);
        }

        if (locationsForAreasForSubsets[subset].ContainsKey (area))
            return locationsForAreasForSubsets[subset][area];
        else
            return locationsForAreasForSubsetsNotFound;
    }

    public static Dictionary<UnitCustomizationLocation, UnitCustomizationBlock> GetCustomizationSubsetForAreaType (Dictionary<UnitCustomizationLocation, UnitCustomizationBlock> source, int areaIndex, bool includeFrame, bool includeArmor)
    {
        Dictionary<UnitCustomizationLocation, UnitCustomizationBlock> result = new Dictionary<UnitCustomizationLocation, UnitCustomizationBlock> ();
        if (areaIndex.InRange (0, 3))
        {
            UnitArea area = (UnitArea)areaIndex;
            if (area == UnitArea.CorePrimary)
            {
                if (includeFrame)
                {
                    result.AddLocation (source, UnitCustomizationLocation.FrameHead);
                    result.AddLocation (source, UnitCustomizationLocation.FrameTorso);
                    result.AddLocation (source, UnitCustomizationLocation.FramePelvis);
                }
                if (includeArmor)
                {
                    result.AddLocation (source, UnitCustomizationLocation.ArmorHead);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorTorso);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorPelvis);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorPod);
                }
            }
            else if (area == UnitArea.LeftOptional)
            {
                if (includeFrame)
                {
                    result.AddLocation (source, UnitCustomizationLocation.FrameLeftShoulder);
                    result.AddLocation (source, UnitCustomizationLocation.FrameLeftForearm);
                    result.AddLocation (source, UnitCustomizationLocation.FrameLeftHand);
                }
                if (includeArmor)
                {
                    result.AddLocation (source, UnitCustomizationLocation.ArmorLeftShoulder);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorLeftForearm);
                    result.AddLocation (source, UnitCustomizationLocation.WeaponLeft);
                }
            }
            else if (area == UnitArea.RightOptional)
            {
                if (includeFrame)
                {
                    result.AddLocation (source, UnitCustomizationLocation.FrameRightShoulder);
                    result.AddLocation (source, UnitCustomizationLocation.FrameRightForearm);
                    result.AddLocation (source, UnitCustomizationLocation.FrameRightHand);
                }
                if (includeArmor)
                {
                    result.AddLocation (source, UnitCustomizationLocation.ArmorRightShoulder);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorRightForearm);
                    result.AddLocation (source, UnitCustomizationLocation.WeaponRight);
                }
            }
            else
            {
                if (includeFrame)
                {
                    result.AddLocation (source, UnitCustomizationLocation.FrameLeftThigh);
                    result.AddLocation (source, UnitCustomizationLocation.FrameRightThigh);
                    result.AddLocation (source, UnitCustomizationLocation.FrameLeftLeg);
                    result.AddLocation (source, UnitCustomizationLocation.FrameRightLeg);
                    result.AddLocation (source, UnitCustomizationLocation.FrameLeftFoot);
                    result.AddLocation (source, UnitCustomizationLocation.FrameRightFoot);
                }
                if (includeArmor)
                {
                    result.AddLocation (source, UnitCustomizationLocation.ArmorLeftThigh);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorRightThigh);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorLeftLeg);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorRightLeg);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorLeftFoot);
                    result.AddLocation (source, UnitCustomizationLocation.ArmorRightFoot);
                }
            }
        }
        else if (areaIndex == 4)
        {
            result.AddLocation (source, UnitCustomizationLocation.WeaponLeft);
            result.AddLocation (source, UnitCustomizationLocation.WeaponRight);
        }
        
        return result;
    }

    private static void AddLocation (this Dictionary<UnitCustomizationLocation, UnitCustomizationBlock> result, Dictionary<UnitCustomizationLocation, UnitCustomizationBlock> source, UnitCustomizationLocation key)
    {
        result.Add (key, source[key]);
    }

    private static Dictionary<UnitCustomizationColor, Color> colors;
    public static Color GetColor (UnitCustomizationColor value)
    {
        if (colors == null)
        {
            colors = new Dictionary<UnitCustomizationColor, Color> ();
            colors.Add (UnitCustomizationColor.OffWhite, new HSBColor (0f, 0f, 0.8f).ToColor ());
            colors.Add (UnitCustomizationColor.LightGray, new HSBColor (0f, 0f, 0.65f).ToColor ());
            colors.Add (UnitCustomizationColor.MiddleGray, new HSBColor (0f, 0f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.DarkGray, new HSBColor (0f, 0f, 0.35f).ToColor ());
            colors.Add (UnitCustomizationColor.OffBlack, new HSBColor (0f, 0f, 0.2f).ToColor ());

            colors.Add (UnitCustomizationColor.StandardCoolRed, new HSBColor (0.95f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardNeutralRed, new HSBColor (0.0f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardWarmRed, new HSBColor (0.05f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardOrange, new HSBColor (0.1f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardYellow, new HSBColor (0.14f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardLimeYellow, new HSBColor (0.18f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardLimeGreen, new HSBColor (0.20f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardWarmGreen, new HSBColor (0.25f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardNeutralGreen, new HSBColor (0.3f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardCoolGreen, new HSBColor (0.4f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardCyan, new HSBColor (0.5f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardSkyBlue, new HSBColor (0.55f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardNeutralBlue, new HSBColor (0.6f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardDeepBlue, new HSBColor (0.65f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardVioletBlue, new HSBColor (0.7f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardViolet, new HSBColor (0.75f, 0.5f, 0.5f).ToColor ());
            colors.Add (UnitCustomizationColor.StandardMagenta, new HSBColor (0.8f, 0.5f, 0.5f).ToColor ());

            colors.Add (UnitCustomizationColor.VividCoolRed, new HSBColor (0.95f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividNeutralRed, new HSBColor (0.0f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividWarmRed, new HSBColor (0.05f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividOrange, new HSBColor (0.1f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividYellow, new HSBColor (0.14f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividLimeYellow, new HSBColor (0.18f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividLimeGreen, new HSBColor (0.20f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividWarmGreen, new HSBColor (0.25f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividNeutralGreen, new HSBColor (0.3f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividCoolGreen, new HSBColor (0.4f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividCyan, new HSBColor (0.5f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividSkyBlue, new HSBColor (0.55f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividNeutralBlue, new HSBColor (0.6f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividDeepBlue, new HSBColor (0.65f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividVioletBlue, new HSBColor (0.7f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividViolet, new HSBColor (0.75f, 0.75f, 0.75f).ToColor ());
            colors.Add (UnitCustomizationColor.VividMagenta, new HSBColor (0.8f, 0.75f, 0.75f).ToColor ());
        }
        return colors[value];
    }

    public static float GetMetalness (UnitCustomizationMaterial value)
    {
        if (value == UnitCustomizationMaterial.Dielectric)
            return 0f;
        else
            return 1f;
    }

    private static Dictionary<UnitCustomizationCoating, Vector3> smoothness;
    public static Vector3 GetSmoothness (UnitCustomizationCoating value)
    {
        if (smoothness == null)
        {
            smoothness = new Dictionary<UnitCustomizationCoating, Vector3> ();
            smoothness.Add (UnitCustomizationCoating.Matte, new Vector3 (0.0f, 0.2f, 0.5f));
            smoothness.Add (UnitCustomizationCoating.Subdued, new Vector3 (0.0f, 0.5f, 0.65f));
            smoothness.Add (UnitCustomizationCoating.Normal, new Vector3 (0.0f, 0.5f, 0.8f));
            smoothness.Add (UnitCustomizationCoating.Reflective, new Vector3 (0.1f, 0.6f, 0.9f));
            smoothness.Add (UnitCustomizationCoating.FullRange, new Vector3 (0.0f, 0.5f, 1.0f));
        }
        return smoothness[value];
    }

    private static Dictionary<UnitCustomizationPatternType, Texture> patternTextures;
    public static Texture GetPatternTexture (UnitCustomizationPatternType value)
    {
        
        if (patternTextures == null)
        {
            patternTextures = new Dictionary<UnitCustomizationPatternType, Texture> ();
            /*
            patternTextures.Add (UnitCustomizationPatternType.None, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_Blank)));
            patternTextures.Add (UnitCustomizationPatternType.WoodlandA, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_WoodlandA)));
            patternTextures.Add (UnitCustomizationPatternType.WoodlandB, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_WoodlandB)));
            patternTextures.Add (UnitCustomizationPatternType.DigitalA, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DigitalA)));
            patternTextures.Add (UnitCustomizationPatternType.DigitalB, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DigitalB)));
            patternTextures.Add (UnitCustomizationPatternType.DigitalC, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DigitalC)));
            patternTextures.Add (UnitCustomizationPatternType.DazzleA, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DazzleA)));
            patternTextures.Add (UnitCustomizationPatternType.DazzleB, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DazzleB)));
            patternTextures.Add (UnitCustomizationPatternType.DazzleC, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DazzleC)));
            patternTextures.Add (UnitCustomizationPatternType.DazzleD, Resources.Load<Texture> (TextHelper.GetTextInternal (TextKeyInternal.Path_Texture_Pattern_DazzleD)));
            */
        }

        if (patternTextures.ContainsKey (value))
            return patternTextures[value];
        else
            return null;
    }

    private static Dictionary<UnitCustomizationPatternIntensity, float> patternIntensities;
    public static float GetPatternIntensity (UnitCustomizationPatternIntensity value)
    {
        if (patternIntensities == null)
        {
            patternIntensities = new Dictionary<UnitCustomizationPatternIntensity, float> ();
            patternIntensities.Add (UnitCustomizationPatternIntensity.None, 0f);
            patternIntensities.Add (UnitCustomizationPatternIntensity.Low, 0.25f);
            patternIntensities.Add (UnitCustomizationPatternIntensity.Medium, 0.5f);
            patternIntensities.Add (UnitCustomizationPatternIntensity.High, 0.75f);
        }
        return patternIntensities[value];
    }

    private static List<UnitHardpoint> hardpointsCached;
    public static List<UnitHardpoint> GetAllHardpoints ()
    {
        if (hardpointsCached == null)
            hardpointsCached = new List<UnitHardpoint> ((UnitHardpoint[])Enum.GetValues (typeof (UnitHardpoint)));
        return hardpointsCached;
    }

    private static List<UnitArea> areasCached;
    public static List<UnitArea> GetAllAreas ()
    {
        if (areasCached == null)
            areasCached = new List<UnitArea> ((UnitArea[])Enum.GetValues (typeof (UnitArea)));
        return areasCached;
    }
}

public static class ExtensionsEnum
{
    public static T Next<T> (this T src, bool forward) where T : struct
    {
        if (!typeof (T).IsEnum)
            throw new ArgumentException (String.Format ("ExtensionsEnum | Next | Argumnent {0} is not an Enum", typeof (T).FullName));

        T[] values = (T[])Enum.GetValues (src.GetType ());

        if (forward)
        {
            int j = Array.IndexOf<T> (values, src) + 1;
            return (values.Length == j) ? values[0] : values[j];
        }
        else
        {
            int j = Array.IndexOf<T> (values, src) - 1;
            return (j < 0) ? values[values.Length - 1] : values[j];
        }
    }

    public static string ToStringFormatted (this Enum value)
    {
        string valueAsString = value.ToString ();
        var sb = new System.Text.StringBuilder ();

        for (var i = 0; i < valueAsString.Length; i++)
        {
            if (char.IsUpper (valueAsString[i]))
                sb.Append (' ');
            sb.Append (valueAsString[i]);
        }

        return sb.ToString ();
    }
}