using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ColorParser
{
    private static Dictionary<string, Color> colorLookup = new Dictionary<string, Color> ();
    private static bool initialized = false;

    private static void CheckSetup ()
    {
        if (initialized)
            return;

        initialized = true;
        
        colorLookup = new Dictionary<string, Color>();

        var colorProperties = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public);
        foreach (var prop in colorProperties)
        {
            if (prop.CanRead && !prop.CanWrite)
            {
                var propReader = prop.GetMethod;
                if (propReader.ReturnType == typeof(Color))
                    colorLookup.Add (prop.Name, (Color)propReader.Invoke(null, Array.Empty<object>()));
            }
        }
    }

    public static Color Parse (string value)
    {
        CheckSetup ();
        
        if (colorLookup.ContainsKey (value.ToLower ()))
            return colorLookup[value.ToLower ()];

        try
        {
            if (value.StartsWith ("#") && !value.Contains (","))
            {
                // Trim the "#" prefix
                value = value.Substring (1, value.Length - 1);
                return ParseHexColor(value);
            }
            else if (value.StartsWith("HSB ") || value.StartsWith ("HSV "))
            {
                // Trim the "HSB " or "HSV " prefix
                value = value.Substring (4, value.Length - 4);
                return ParseHSBAColor(value);
            }
            else
            {
                return ParseRGBAColor(value);
            }
        }
        catch (FormatException e)
        {
            throw new Exception ($"{e.Message}\nThe format must be either of:" +
                                $"\n   - R,G,B (0-1 or 0-255)" +
                                $"\n   - R,G,B,A (0-1 or 0-255)" +
                                $"\n   - HSB H,S,B (0-1 or H 0-360 & SB 0-100)" +
                                $"\n   - HSB H,S,B,A (0-1 or H 0-360 & SBA 0-100)" +
                                $"\n   - #RRGGBB (hex)" +
                                $"\n   - #RRGGBBAA (hex)" +
                                $"\n   - A preset color such as 'red'", e);
        }
    }

    private static Color ParseRGBAColor (string value)
    {
        string[] colorParts = value.Split(',');
        Color parsedColor = Color.white;
        int i = 0;

        if (colorParts.Length < 3 || colorParts.Length > 4) { throw new FormatException($"Cannot parse '{value}' as a Color."); }

        try
        {
            for (i = 0; i < colorParts.Length; i++)
            {
                var colorPart = colorParts[i];
                var valParsed = float.TryParse (colorPart, out float val);
                if (!valParsed)
                {
                    throw new FormatException($"Couldn't parse {val} falls outside of the valid range for a component of a HSB Color.");
                }

                if (val < 0f)
                    val = 0f;

                // Deal with (255, 255, 255) format
                if (val > 1f)
                    val = Mathf.Clamp01 (val / 255f);

                parsedColor[i] = val;
            }

            return parsedColor;
        }
        catch (FormatException)
        {
            throw new FormatException($"Cannot parse '{colorParts[i]}' as part of a Color, it must be numerical and in the valid range [0,1].");
        }
    }
    
    private static Color ParseHSBAColor(string value)
    {
        string[] colorParts = value.Split(',');
        Vector4 parsedVector = new Vector4 (0f, 0f, 0f, 1f);
        int i = 0;

        if (colorParts.Length < 3 || colorParts.Length > 4) { throw new FormatException($"Cannot parse '{value}' as a HSBColor."); }

        try
        {
            for (i = 0; i < colorParts.Length; i++)
            {
                var colorPart = colorParts[i];
                var valParsed = float.TryParse (colorPart, out float val);
                if (!valParsed)
                {
                    throw new FormatException($"Couldn't parse {val} falls outside of the valid range for a component of a HSB Color.");
                }

                if (val < 0f)
                    val = 0f;

                // Deal with (360, 100, 100) format
                if (val > 1f)
                {
                    if (i == 0)
                        val = Mathf.Clamp01 (val / 360f);
                    else
                        val = Mathf.Clamp01 (val / 100f);
                }

                parsedVector[i] = val;
            }

            var parsedColor = Color.HSVToRGB (parsedVector.x, parsedVector.y, parsedVector.z);
            parsedColor = new Color (parsedColor.r, parsedColor.g, parsedColor.b, parsedVector.w);
            
            Debug.Log ($"HSB Input: {value} | Vector: {parsedVector} | Color: {parsedColor}");
            
            return parsedColor;
        }
        catch (FormatException)
        {
            throw new FormatException($"Cannot parse '{colorParts[i]}' as part of a Color");
        }
    }

    private static Color ParseHexColor(string value)
    {
        int digitCount = value.Length;
        if (digitCount != 6 && digitCount != 8)
        {
            throw new FormatException("Hex colors must contain either 6 or 8 hex digits.");
        }

        Color parsedColor = Color.white;
        int byteCount = digitCount / 2;
        int i = 0;

        try
        {
            for (i = 0; i < byteCount; i++)
                parsedColor[i] = int.Parse(value.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            return parsedColor;
        }
        catch (FormatException)
        {
            throw new FormatException($"Cannot parse '{value.Substring(2 * i, 2)}' as part of a Color as it was invalid hex.");
        }
    }
}
