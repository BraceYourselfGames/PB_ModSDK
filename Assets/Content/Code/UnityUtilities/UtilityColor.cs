using UnityEngine;
using System.Collections.Generic;
using System.Text;

public static class UtilityParticles
{
    public static void SetSystemPlaying (this ParticleSystem ps, bool play, bool withChildren = false, bool clear = false)
    {
        if (play)
        {
            if (!ps.isEmitting || !ps.isPlaying)
            {
                // Debug.Log ($"Playing PS {ps.gameObject.name}");
                // ps.Play (withChildren);
                ps.Play ();
            }
        } 
        else 
        {
            ps.Stop (withChildren, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }
    }
    
    public static void SetSystemsPlaying (this List<ParticleSystem> list, bool play, bool withChildren = false, bool clear = false)
    {
        int count = list.Count;
        for (int i = 0; i < count; ++i)
            SetSystemPlaying (list[i], play, withChildren, clear);
    }
    
    public static void SetSystemsPlaying (this ParticleSystem[] array, bool play, bool withChildren = false, bool clear = false)
    {
        int length = array.Length;
        for (int i = 0; i < length; ++i)
            SetSystemPlaying (array[i], play, withChildren, clear);
    }
}

public static class UtilityColor
{
    private const char lBracket = '[';
    private const char rBracket = ']';
    private const string hexFormat = "X2";
    
    //2 x 3
    private static StringBuilder toHexRGB = new StringBuilder (6);
    public static string ToHexRGB (Color color)
    {
        var color32 = new Color32
        (
            (byte) Mathf.Clamp (Mathf.RoundToInt(color.r * (float) byte.MaxValue), 0, (int) byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.g * (float) byte.MaxValue), 0, (int) byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.b * (float) byte.MaxValue), 0, (int) byte.MaxValue), 
            0
        );
        
        toHexRGB.Clear ();
        toHexRGB.Append(color32.r.ToString(hexFormat));
        toHexRGB.Append(color32.g.ToString(hexFormat));
        toHexRGB.Append(color32.b.ToString(hexFormat));
        return toHexRGB.ToString();
    }

    private static StringBuilder toHexTagRGB = new StringBuilder (11); // 3 x 3 + 2
    public static string ToHexTagRGB (Color color)
    {
        var color32 = new Color32
        (
            (byte) Mathf.Clamp (Mathf.RoundToInt(color.r * byte.MaxValue), 0, byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.g * byte.MaxValue), 0,  byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.b * byte.MaxValue), 0,  byte.MaxValue), 
            0
        );

        toHexTagRGB.Clear ();
        toHexTagRGB.Append (lBracket);
        toHexTagRGB.Append(color32.r.ToString(hexFormat));
        toHexTagRGB.Append(color32.g.ToString(hexFormat));
        toHexTagRGB.Append(color32.b.ToString(hexFormat));
        toHexTagRGB.Append(rBracket);
        return toHexTagRGB.ToString();
    }

    // 2 + 2
    private static StringBuilder toHexTagA = new StringBuilder (4);
    public static string ToHexTagA (float a)
    {
        byte alpha = (byte) Mathf.Clamp(Mathf.RoundToInt(a * (float) byte.MaxValue), 0, (int) byte.MaxValue);
        
        toHexTagA.Clear ();
        toHexTagA.Append(lBracket);
        toHexTagA.Append(alpha.ToString(hexFormat));
        toHexTagA.Append(rBracket);
        return toHexTagA.ToString();
    }

    private static StringBuilder toHexTagRGBA = new StringBuilder (10); // 2 x 4 + 2
    public static string ToHexTagRGBA (Color color)
    {
        var color32 = new Color32
        (
            (byte) Mathf.Clamp (Mathf.RoundToInt(color.r * (float) byte.MaxValue), 0, (int) byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.g * (float) byte.MaxValue), 0, (int) byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.b * (float) byte.MaxValue), 0, (int) byte.MaxValue), 
            (byte) Mathf.Clamp(Mathf.RoundToInt(color.a * (float) byte.MaxValue), 0, (int) byte.MaxValue)
        );
        
        toHexTagRGBA.Clear ();
        toHexTagRGBA.Append(lBracket);
        toHexTagRGBA.Append(color32.r.ToString(hexFormat));
        toHexTagRGBA.Append(color32.g.ToString(hexFormat));
        toHexTagRGBA.Append(color32.b.ToString(hexFormat));
        toHexTagRGBA.Append(color32.a.ToString(hexFormat));
        toHexTagRGBA.Append(rBracket);
        return toHexTagRGBA.ToString();
    }

    public static Color WithAlpha (this Color color, float alpha)
    {
        alpha = Mathf.Clamp01 (alpha);
        return new Color (color.r, color.g, color.b, alpha);
    }
    
    public static Color MultiplyAlpha (this Color color, float alpha)
    {
        alpha = Mathf.Clamp01 (alpha);
        return new Color (color.r, color.g, color.b, color.a * alpha);
    }
    
    public static Color Opaque (this Color color)
    {
        return new Color (color.r, color.g, color.b, 1f);
    }

    public static Color AdjustBrightness (this Color color, float brightness)
    {
        HSBColor hsb = new HSBColor (color);
        if (brightness > 0.5f)
            hsb.b = Mathf.Lerp (hsb.b, 1f, (brightness - 0.5f) * 2f);
        else
            hsb.b = Mathf.Lerp (0f, hsb.b, brightness * 2f);
        return hsb.ToColor ();
    }
    
    public static Color OffsetBrightnessAndSaturation (this Color color, float brightnessOffset, float saturationOffset)
    {
        HSBColor hsb = new HSBColor (color);
        hsb.b = Mathf.Clamp01 (hsb.b + brightnessOffset);
        hsb.s = Mathf.Clamp01 (hsb.s + saturationOffset);
        return hsb.ToColor ();
    }
    
    public static Color OverrideFromHSB (this Color colorOriginal, HSBColor colorOverrideHSB)
    {
        var colorFinalHSB = new HSBColor (colorOriginal);
        colorFinalHSB.h = colorOverrideHSB.h;
        colorFinalHSB.s *= colorOverrideHSB.s;
        colorFinalHSB.b *= colorOverrideHSB.b;
        return colorFinalHSB.ToColor ();
    }

    public static Color OverrideFromHSB (this Color colorOriginal, Color colorOverride)
    {
        var colorOverrideHSB = new HSBColor (colorOverride);
        return OverrideFromHSB (colorOriginal, colorOverrideHSB);
    }

    public static Color AdjustHSB (this Color color, float hueOffset, float saturation, float brightness)
    {
        HSBColor hsb = new HSBColor (color);
        hsb.h = (hsb.h + hueOffset) % 1f;
        if (saturation > 0.5f)
            hsb.s = Mathf.Lerp (hsb.s, 1f, (saturation - 0.5f) * 2f);
        else
            hsb.s = Mathf.Lerp (0f, hsb.s, saturation * 2f);
        if (brightness > 0.5f)
            hsb.b = Mathf.Lerp (hsb.b, 1f, (brightness - 0.5f) * 2f);
        else
            hsb.b = Mathf.Lerp (0f, hsb.b, brightness * 2f);
        return hsb.ToColor ();
    }
    
    public static Color SetHue (this Color color, float hue)
    {
        HSBColor hsb = new HSBColor (color);
        hsb.h = hue;
        return hsb.ToColor ();
    }
    
    public static Color SetHue (this Color color, Color source)
    {
        HSBColor hsbSource = new HSBColor (source);
        HSBColor hsb = new HSBColor (color);
        hsb.h = hsbSource.h;
        return hsb.ToColor ();
    }

    public static Color GetLegibleOverlay (Color value)
    {
        if (HSBColor.FromColor (value).b > 0.9f && HSBColor.FromColor (value).s < 0.9f) return Color.black;
        else return Color.white;
    }

	public static Color ColorFromDistance (int distance)
	{
		if (distance <= 8 * 3) return ColorFromHSB (343, 69, 100);
		if (distance <= 16 * 3) return ColorFromHSB (180, 50, 99);
		if (distance <= 24 * 3) return ColorFromHSB (180, 100, 69);
		if (distance <= 32 * 3) return ColorFromHSB (31, 68, 100);
		if (distance <= 48 * 3) return ColorFromHSB (200, 100, 80);
		if (distance <= 56 * 3) return ColorFromHSB (231, 61, 67);
		if (distance <= 64 * 3) return ColorFromHSB (52, 65, 99);
		if (distance <= 72 * 3) return ColorFromHSB (257, 67, 62);
		if (distance <= 80 * 3) return ColorFromHSB (156, 77, 76);
		if (distance <= 88 * 3) return ColorFromHSB (174, 100, 47);
		else return ColorFromHSB (0, 100, 47);
	}

	public static Color ColorFromHSB (int hue, int saturation, int brightness)
	{
		return new HSBColor ((float)hue % 360f, (float)saturation / 100f, (float)brightness / 100f, 1.0f).ToColor ();
	}

    public static string ColorToHex (Color32 color)
    {
        string hex = color.r.ToString ("X2") + color.g.ToString ("X2") + color.b.ToString ("X2");
        return hex;
    }

    public static Color ColorFromHex (string hex)
    {
        byte r = byte.Parse (hex.Substring (0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse (hex.Substring (2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse (hex.Substring (4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32 (r, g, b, 255);
    }

    public static Color LerpThroughHSB (Color a, Color b, float value)
    {
        return HSBColor.Lerp (HSBColor.FromColor (a), HSBColor.FromColor (b), value).ToColor ();
    }
}
