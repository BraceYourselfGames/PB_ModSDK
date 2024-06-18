using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using PhantomBrigade.Data.UI;

namespace PhantomBrigade.Data
{
    public enum UIColorType
    {
        Main,
        Hover,
        Press,
        Lock
    }
    
    [Serializable]
    public class DataBlockUIColorCache
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorMain;

        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorHover;
        
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorPress;
        
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorLock;
        
        [HorizontalGroup ("info")]
        [ShowIf ("IsCacheInfoVisible")]
        [LabelText ("LCH"), YamlIgnore, HideReferenceObjectPicker]
        public ColorLch colorMainLch;

        [HorizontalGroup ("info")]
        [ShowIf ("IsCacheInfoVisible")]
        [LabelText ("HSB"), YamlIgnore, HideReferenceObjectPicker]
        public HSBColor colorMainHsb;

        public Color GetColorByType (UIColorType type)
        {
            switch (type)
            {
                case UIColorType.Main:
                    return colorMain;
                case UIColorType.Hover:
                    return colorHover;
                case UIColorType.Press:
                    return colorPress;
                case UIColorType.Lock:
                    return colorLock;
                default:
                    return Color.magenta;
            }
        }
        
        #if UNITY_EDITOR

        private bool IsCacheInfoVisible => DataMultiLinkerUIColor.Presentation.showCacheInfo;

        #endif
    }

    [Serializable][LabelWidth (180f)][HideReferenceObjectPicker]
    public class DataContainerUIColor : DataContainer
    {
        [HideLabel]
        public string comment;

        [ShowIf ("IsCacheVisible")]
        [HideLabel, YamlIgnore, HideReferenceObjectPicker]
        public DataBlockUIColorCache colorCache;
        
        [OnValueChanged ("UpdateColorFromGlobals")]
        [LabelText ("Luminosity"), PropertyRange (0f, 1f)]
        public float l = 0.5f;
        
        [OnValueChanged ("UpdateColorFromGlobals")]
        [LabelText ("Chroma"), PropertyRange (0f, 1f)]
        public float c = 0.5f;

        [OnValueChanged ("UpdateColorFromGlobals")]
        [LabelText ("Hue offset"), PropertyRange (0f, 1f)]
        public float h = 0f;



        public void UpdateColor (ColorBase colorBase)
        {
            if (colorBase == null)
                return;
            
            var lMain = ConvertLC (l);
            var cMain = ConvertLC (c);
            var hMain = ConvertH (h, colorBase.hueMain);
            
            colorCache.colorMainLch = new ColorLch (lMain, cMain, hMain);
            colorCache.colorMain = colorCache.colorMainLch.ToRgb (true);
            colorCache.colorMainHsb = new HSBColor (colorCache.colorMain);

            // var lHover = ConvertLC (l + luminosityOffsetHover);
            // var cHover = ConvertLC (c + chromaOffsetHover);
            // var hHover = hMain;
            // colorCache.colorHover = new ColorLch (lHover, cHover, hHover).ToRgb (true);
            colorCache.colorHover = ColorSpaceUtility.LerpInLab (colorCache.colorMain, colorBase.basisHover, colorBase.factorHover);
            
            // var lPress = ConvertLC (l + luminosityOffsetPress);
            // var cPress = ConvertLC (c + chromaOffsetPress);
            // var hPress = hMain;
            // colorCache.colorPressed = new ColorLch (lPress, cPress, hPress).ToRgb (true);
            colorCache.colorPress = ColorSpaceUtility.LerpInLab (colorCache.colorMain, colorBase.basisPress, colorBase.factorPress);
            
            // var lDisabled = ConvertLC (l);
            // var cDisabled = ConvertLC (Mathf.Lerp (0f, c, chromaFactorDisabled));
            // var hDisabled = hMain;
            // colorCache.colorDisabled = new ColorLch (lDisabled, cDisabled, hDisabled).ToRgb (true);
            colorCache.colorLock = ColorSpaceUtility.LerpInLab (colorCache.colorMain, colorBase.basisLock, colorBase.factorLock);
        }
        
        private void UpdateColorFromGlobals ()
        {
            var ui = DataShortcuts.ui;
            if (ui == null || ui.colorBase == null)
                return;

            UpdateColor (ui.colorBase);
        }

        private float SanitizeLC (float value) =>
            Mathf.Clamp (value, 0f, 100f);
        
        private float SanitizeH (float value) =>
            value % 360f;
        
        private float ConvertLC (float value) =>
            Mathf.Clamp (value * 100f, 0f, 100f);

        private float ConvertH (float value, float basis) =>
            (value * 360f + basis) % 360f;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            l = Mathf.Clamp01 (l);
            c = Mathf.Clamp01 (c);
            h = Mathf.Clamp01 (h);
            
            colorCache = new DataBlockUIColorCache ();
            UpdateColorFromGlobals ();
        }

        #if UNITY_EDITOR
        
        private bool IsCacheVisible => DataMultiLinkerUIColor.Presentation.showCache;
        
        #endif
    }

    public class ColorLch
    {
        public float l;
        public float c;
        public float h;

        public ColorLch (float l, float c, float h)
        {
            this.l = l;
            this.c = c;
            this.h = h;
        }
    } 
    
    public class ColorLab
    {
        public float l;
        public float a;
        public float b;

        public ColorLab (float l, float a, float b)
        {
            this.l = l;
            this.a = a;
            this.b = b;
        }
    }
    
    public static class ColorSpaceUtility
    {
        public static ColorLab ToLab (this Color color)
        {
            return VectorToLab (RgbToVector (color));
        }

        public static Color ToRgb (this ColorLab color, bool clamp = false)
        {
            return VectorToRgb ((LabToVector (color)), clamp);
        }

        public static Color ToRgb (this ColorLch color, bool clamp = false)
        {
            return VectorToRgb (LabToVector (LchToLab (color)), clamp);
        }

        public static Color LerpInLab (Color from, Color to, float t)
        {
            return ToRgb (LabLerp (ToLab (from), ToLab (to), t));
        }

        private static ColorLab LabLerp (ColorLab Color1, ColorLab Color2, float lerpFactor)
        {
            float l = Color1.l + lerpFactor * (Color2.l - Color1.l);
            float a = Color1.a + lerpFactor * (Color2.a - Color1.a);
            float b = Color1.b + lerpFactor * (Color2.b - Color1.b);
            return new ColorLab (l, a, b);
        }

        private static ColorLab LchToLab (ColorLch color)
        {
            float l = color.l;
            float a = color.c * Mathf.Cos (Mathf.Deg2Rad * color.h);
            float b = color.c * Mathf.Sin (Mathf.Deg2Rad * color.h);
            return new ColorLab (l, a, b);
        }

        private static float TransformColorForward (float t)
        {
            if (t > 0.008856452f)
            {
                return Mathf.Pow (t, 0.3333333f);
            }
            else
            {
                return t / 0.1284185f + 0.1379310f;
            }
        }

        private static float TransformColorReverse (float t)
        {
            if (t > 0.2068965f)
            {
                return Mathf.Pow (t, 3f);
            }
            else
            {
                return 0.1284185f * (t - 0.1379310f);
            }
        }

        private static ColorLab VectorToLab (Vector3 vector)
        {
            float x = TransformColorForward (vector.x / VectorColorConstant.x);
            float y = TransformColorForward (vector.y / VectorColorConstant.y);
            float z = TransformColorForward (vector.z / VectorColorConstant.z);

            float l = 116 * y - 16;
            float a = 500 * (x - y);
            float b = 200 * (y - z);
            return new ColorLab (l, a, b);
        }

        private static Vector3 LabToVector (ColorLab color)
        {
            float x = VectorColorConstant.x * TransformColorReverse ((color.l + 16) / 116 + color.a / 500);
            float y = VectorColorConstant.y * TransformColorReverse ((color.l + 16) / 116);
            float z = VectorColorConstant.z * TransformColorReverse ((color.l + 16) / 116 - color.b / 200);
            return new Vector3 (x, y, z);
        }

        private static Vector3 VectorColorConstant = new Vector3 (0.95047f, 1f, 1.08883f);

        private static Vector3 RgbToVector (Color color)
        {
            float r = StandardToLinear (color.r);
            float g = StandardToLinear (color.g);
            float b = StandardToLinear (color.b);

            float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
            float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
            float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;
            return new Vector3 (x, y, z);
        }

        private static Color VectorToRgb (Vector3 vector, bool clamp = false)
        {
            float r = vector.x * 3.2406f + vector.y * -1.5372f + vector.z * -0.4986f;
            float g = vector.x * -0.9689f + vector.y * 1.8758f + vector.z * 0.0415f;
            float b = vector.x * 0.0557f + vector.y * -0.2040f + vector.z * 1.0570f;

            float R = LinearToStandard (r);
            float G = LinearToStandard (g);
            float B = LinearToStandard (b);
            
            if (clamp)
            {
                R = Mathf.Clamp01 (R);
                G = Mathf.Clamp01 (G);
                B = Mathf.Clamp01 (B);
            }

            return new Color (R, G, B);
        }

        private static float StandardToLinear (float c)
        {
            if (c <= 0.04045f)
            {
                return c / 12.92f;
            }
            else
            {
                return Mathf.Pow ((c + 0.055f) / 1.055f, 2.4f);
            }
        }

        private static float LinearToStandard (float c)
        {
            if (c <= 0.0031308f)
            {
                return c * 12.92f;
            }
            else
            {
                return 1.055f * Mathf.Pow (c, 0.4166667f) - 0.055f;
            }
        }
    }
}


