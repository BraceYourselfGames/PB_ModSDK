using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using PhantomBrigade.Data.UI;

#if !PB_MODSDK
using ColorPlus;
#endif

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockUIColorInteractable
    {
        public Color hovered;
        public Color pressed;
        public Color locked;
    }
    
    [Serializable]
    public struct DataBlockUIColorTest
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorFromHsb;
        
        [HideLabel, HorizontalGroup, ProgressBar (0, 100, DrawValueLabel = false)]
        public float luminanceFromHsb;
        
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorFromLch;
        
        [HideLabel, HorizontalGroup, ProgressBar (0, 100, DrawValueLabel = false)]
        public float luminanceFromLch;
    }

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
        
        #if !PB_MODSDK
        [HorizontalGroup ("info")]
        [ShowIf ("IsCacheInfoVisible")]
        [LabelText ("LCH"), YamlIgnore, HideReferenceObjectPicker]
        public ColorLch colorMainLch;
        #endif
        
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
            
            #if !PB_MODSDK
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
            #endif
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
}

