using System;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class AppearancePropertyLevel
    {
        public string textName;
    }
    
        
    [TypeHinted]
    public interface IPilotAppearanceImplementation
    {
        #if !PB_MODSDK
        public bool IsPauseNeeded ();
        public bool IsFullRedrawNeeded ();
        public bool IsAvailable (DataBlockPilotAppearance appearance);
        
        public bool TryRefreshUI 
        (
            DataContainerPilotAppearanceProperty propConfig, 
            DataBlockPilotAppearance appearance, 
            CIHelperPilotAppearanceEntry instance, 
            bool initialize
        );
        #endif
    }

    public class PilotPropBlendShape : IPilotAppearanceImplementation
    {
        public string blendShapeKey;
    }

    public interface IPilotPropLevels
    {
        #if !PB_MODSDK
        public ICollection<string> GetLevelKeys (DataBlockPilotAppearance appearance);
        public string GetCurrentLevelKey (DataBlockPilotAppearance appearance);
        public void OffsetLevelKey (DataBlockPilotAppearance appearance, bool forward);
        #endif
    }
    
    public class PilotPropLevels : IPilotAppearanceImplementation
    {
        
    }

    public class PilotPropSprite
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string spriteName;

        [ColorUsage (true, true)]
        public Color colorMultiplier = new Color (1f, 1f, 1f, 1f);
    }
    
    public class PilotPropLevelTint : PilotPropLevels
    {
        public PilotPropSprite spriteOverlay;
        public PilotPropSprite spriteTintSecondary;
        public PilotPropSprite spriteTintMain;
        public PilotPropSprite spriteBackground;
        protected static Color colorFallback = new Color (0f, 0f, 0f, 0f);
    }
    
    // Added
    public class PilotPropLevelPronouns : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelPersonality : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelModel : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelFace : PilotPropLevels, IPilotPropLevels
    {
        
    }

    // Added
    public class PilotPropLevelCameraAngle : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelSuit : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelHairMain : PilotPropLevels, IPilotPropLevels
    {
        
    }

    // Added
    public class PilotPropLevelHairFacial : PilotPropLevels, IPilotPropLevels
    {
        
    }

    // Added
    public class PilotPropLevelHairEyebrows : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelSkinTexture : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelTintSkin : PilotPropLevelTint, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelTintHairMain : PilotPropLevelTint, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelTintHairFacial : PilotPropLevelTintHairMain
    {
        
    }
    
    // Added
    public class PilotPropLevelEyeIrisPreset : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelTintEye : PilotPropLevelTint, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelTintLip : PilotPropLevelTint, IPilotPropLevels
    {
        
    }

    
    
    // Added
    public class PilotPropLevelAccessoryTop : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelAccessoryTopVariant : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelAccessoryFront : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelAccessoryFrontVariant : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    
    
    // Added
    public class PilotPropLevelOverlay : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelOverlayVariant : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    public class PilotPropLevelPattern : PilotPropLevels, IPilotPropLevels
    {
        
    }
    
    // Added
    public class PilotPropLevelPatternVariant : PilotPropLevels, IPilotPropLevels
    {
        
    }


    public class DataBlockPilotPropHeader
    {
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [YamlIgnore, HideLabel]
        public string text;
    }
    
    public class DataContainerPilotAppearanceProperty : DataContainerWithText
    {
        public bool hidden = false;
        public int priority = 0;
        public string group;

        [DropdownReference (true)]
        public DataBlockPilotPropHeader header;
        
        [YamlIgnore, ShowIf (DataEditor.textAttrArg), HideLabel]
        public string textName;

        [BoxGroup ("Implementation", false)]
        public IPilotAppearanceImplementation implementation;
        
        [ValueDropdown ("@DataMultiLinkerPilotAppearanceProperty.data.Keys")]
        [DropdownReference]
        public HashSet<string> keysDependent;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.pilotAppearance, $"{key}__header");

            if (header != null)
                header.text = DataManagerText.GetText (TextLibs.pilotAppearance, $"{key}__prefix");
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerPilotAppearanceProperty () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.pilotAppearance, $"{key}__header", textName);
            
            if (header != null)
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotAppearance, $"{key}__prefix", header.text);
        }

        #endif
        #endregion
    }
}

