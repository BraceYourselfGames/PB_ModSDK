using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if !PB_MODSDK
using ColorPlus;
using PhantomBrigade.Input.Components;
#endif

namespace PhantomBrigade.Data.UI
{
    [Serializable]
    public class UnitProjectionConfig
    {
        public Color scanColorMin = Color.white.WithAlpha (1f);
        public Color scanColorMax = Color.white.WithAlpha (1f);
        public Color rimColorMin = Color.white.WithAlpha (1f);
        public Color rimColorMax = Color.white.WithAlpha (1f);
    }
    
    [Serializable]
    public class ColorBase
    {
        public Color basisMain;
        public float hueMain;
        
        public Color basisHover;
        public float factorHover = 0.25f;
        
        public Color basisPress;
        public float factorPress = 0.25f;
        
        public Color basisLock;
        public float factorLock = 0.25f;
    }

    [Serializable]
    public class CombatUIModeConfig
    {
        public Color color;
        public string icon;

        public bool overlayUsed;
        public bool inWorld;
        public bool inTimeline;
    }
    
    [Serializable]
    public class FactionColorGroup
    {
        public Color friendly = Color.white.WithAlpha (1f);
        public Color friendlySelected = Color.white.WithAlpha (1f);
        public Color enemy = Color.white.WithAlpha (1f);
        public Color enemySelected = Color.white.WithAlpha (1f);
    }

    
    
    [Serializable][LabelWidth (70f)][HideReferenceObjectPicker]
    public class DataBlockSpriteName
    {
        public string name;
    }
    
    [Serializable][LabelWidth (70f)][HideReferenceObjectPicker]
    public class DataBlockMarkupColor
    {
        public Color color;
        private string _hex;
    
        #if !PB_MODSDK
        [YamlIgnore]
        public string hex
        {
            get
            {
                if (_hex == null)
                    _hex = NGUIText.EncodeColor24 (color).ToLower ();
                return _hex;
            }
        }
        #endif
    }
    
    public static class UIColorKeys
    {
        public const string unitInfoAccentFriendly = "unitInfo_AccentFriendly";
        public const string unitInfoAccentFriendlyDisposable = "unitInfo_AccentFriendlyDisposable";
        public const string unitInfoAccentFriendlyLocked = "unitInfo_AccentFriendlyLocked";
        public const string unitInfoAccentEnemy = "unitInfo_AccentEnemy";
        public const string unitInfoBlueprintIntact = "unitInfo_BlueprintIntact";
        public const string unitInfoBlueprintBroken = "unitInfo_BlueprintBroken";
        
        public const string unitInfoIntegrityColorIdle = "unitInfo_IntegrityColorIdle";
        public const string unitInfoIntegrityColorRepair = "unitInfo_IntegrityColorRepair";
        public const string unitInfoIntegrityColorDamage = "unitInfo_IntegrityColorDamage";
        
        public const string unitInfoBarrierColorIdle = "unitInfo_BarrierColorIdle";
        public const string unitInfoBarrierColorRepair = "unitInfo_BarrierColorRepair";
        public const string unitInfoBarrierColorDamage = "unitInfo_BarrierColorDamage";
        
        public const string unitOverlayTintEnemy = "unitOverlayTintEnemy";
        public const string unitOverlayTintAllied = "unitOverlayTintAllied";
        public const string unitOverlayTintFriendly = "unitOverlayTintFriendly";
        public const string unitOverlayTintLocked = "unitOverlayTintLocked";
        public const string unitOverlayTintObjective = "unitOverlayTintObjective";
    }

    public class DataBlockMainMenuTrackCue
    {
        public int bar;
        public float time;
        public float duration;
    }

    public class DataBlockMainMenuTrack
    {
        public int trackBPM;

        [TableList]
        public List<DataBlockMainMenuTrackCue> cues;
    }

    public class DataBlockEfficiencyPrediction
    {
        public int slices = 300;

        public float distanceLimit = 300;
        
        public float boundaryThreshold = 0.2f;

        public float optimumThreshold = 0.8f;

        public bool normalization = false;

        public bool coverageUsed = true;

        public bool coverageInv = false;

        public bool coverageWeightOffset = false;
        
        public bool decalFade = false;
    }

    public class DataBlockDebugDrawing
    {
        public float guidanceDelay = 0.5f;
        public float fontSize = 10f;
        
        public float lineThicknessDefault = 0.1f;
        public float lineHighlightThicknessMul = 0.5f;
        public float lineHighlightIntensity = 2f;
        public float lineHighlightInterpolant = 0.5f;
        public float lineSizeProjectile = 0.35f;
        public float timePersistent = 5f;
        public float lineLengthTargetPoint = 50f;
        
        public bool showGuidanceData = false;
        public bool showProjectileCollisions = false;
        public bool showBeamReflections = false;
        public bool showTargetingDirectional = false;
        public bool showTargetingInterpolation = false;
        public bool showTargetingScatter = false;
        public bool showTargetingAim = false;
        
        [ColorUsage (true, true)]
        public Color colorDefaultBackground = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        
        [ColorUsage (true, true)]
        public Color colorDefaultFront = new Color (1f, 1f, 1f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorDefaultRed = new Color (1f, 0.6f, 0.6f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorDefaultGreen = new Color (0.7f, 1f, 0.6f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorDefaultBlue = new Color (0.5f, 0.7f, 1f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorInputYaw = new Color (0.4f, 0.9f, 0.55f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorInputPitch = new Color (0.9f, 0.2f, 0.4f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorInputThrottle = new Color (0.4f, 0.7f, 0.9f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorInputVelocityComp = new Color (0.6f, 0.7f, 1f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorInputFuseProximity = new Color (0.9f, 0.7f, 0.3f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorProjectileCollision = new Color (0.9f, 0.2f, 0.4f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorProjFiring = new Color (1f, 1f, 1f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorProjProjectileFriendly = new Color (0.4f, 0.7f, 0.9f, 1f);
        
        [ColorUsage (true, true)]
        public Color colorProjProjectileEnemy = new Color (0.9f, 0.2f, 0.4f, 1f);
    }

    [Serializable] 
    public class DataContainerUI : DataContainerUnique
    {
        [TabGroup ("Textures")]
        [Space (4f)]
        public Dictionary<string, DataBlockResourceTexture> textures;

        [TabGroup ("Patterns")]
        [Space (4f)]
        public Dictionary<string, DataBlockResourceTexture> unitPatterns;

        [TabGroup ("Projection")]
        public UnitProjectionConfig projectionCollision;
        
        [TabGroup ("Projection")]
        public UnitProjectionConfig projectionFriendly;
        
        [TabGroup ("Projection")]
        public UnitProjectionConfig projectionFriendlySelected;
        
        [TabGroup ("Projection")]
        public UnitProjectionConfig projectionEnemy;
        
        [TabGroup ("Projection")]
        public UnitProjectionConfig projectionEnemySelected;
        
        [TabGroup ("Colors")]
        [Space (4f)]
        public Dictionary<string, Color> colors;

        [TabGroup ("Colors")]
        [Space (4f)]
        public SortedDictionary<string, FactionColorGroup> factionColors;
        
        [TabGroup ("Other")]
        public bool workshopDisplayed = false;
        
        [TabGroup ("Other")]
        public bool combatIsolinesDisplayed = true;
        
        [TabGroup ("Other")]
        public bool combatMultiTargetSupport = false;
        
        [TabGroup ("Other")]
        public bool minifiedStatsVisible = false;
        
        [TabGroup ("Other")]
        public float overlayThresholdHeat = 0.05f;
        
        [TabGroup ("Other")]
        public float overlayThresholdPilotHealth = 0.05f;
        
        [TabGroup ("Other")]
        public float overlayThresholdStagger = 0.05f;
        
        [TabGroup ("Other")]
        public float overlayOverworldSwitchHeight = 300f;
        
        [TabGroup ("Other")]
        public Vector2 overlayOverworldSwitchHeights = new Vector2 (200f, 300f);
        
        [TabGroup ("Other")]
        public Vector2 overlayOverworldFadeDistancesClose = new Vector2 (500f, 800f);
        
        [TabGroup ("Other")]
        public Vector2 overlayOverworldFadeDistancesFar = new Vector2 (1200f, 1800f);
        
        [TabGroup ("Other")]
        public Vector2 overlayOverworldSwitchDistanceProvince = new Vector2 (1250f, 5000f);
        
        [TabGroup ("Other")]
        public Vector2 overlayOverworldSwitchDistanceEntity = new Vector2 (900f, 5000f);

        [TabGroup ("Other")]
        [Space (4f)]
        public Dictionary<UIBasicSprite.Pivot, Vector2> tooltipPivotOffsets;

        [TabGroup ("Other")] 
        [Space (4f)]
        public Dictionary<CombatUIModes, CombatUIModeConfig> modeConfigs;
        
        [TabGroup ("Colors")]
        public Color fallbackColor = Color.white.WithAlpha (1f);
        
        [TabGroup ("Colors")]
        [Space (4f)]
        [ShowInInspector, OnValueChanged ("UpdateColorBasisAndDatabase", true)]
        public ColorBase colorBase;

        [TabGroup ("Music")]
        public SortedDictionary<string, DataBlockMainMenuTrack> mainMenuTracks;
        
        [TabGroup ("Targeting")]
        public DataBlockEfficiencyPrediction efficiencyPrediction = new DataBlockEfficiencyPrediction ();
        
        [TabGroup ("Targeting")]
        public DataBlockDebugDrawing debugDrawing = new DataBlockDebugDrawing ();
        
        [TabGroup ("Other")]
        public Dictionary<int, string> equipmentRatingSuffixes = new Dictionary<int, string> 
        {
            { 0, " [99]-[ff]" },
            { 2, " [ceffb9][99]+[ff]" },
            { 3, " [99ddff][99]++[ff]" }
        };
        
        [TabGroup ("Other")]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string iconStateProgressTime;
        
        [TabGroup ("Other")]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string iconStateProgressDestruction;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            if (textures != null)
            {
                foreach (var entry in textures)
                    entry.Value.OnBeforeSerialization ();
            }
            
            if (unitPatterns != null)
            {
                foreach (var entry in unitPatterns)
                    entry.Value.OnBeforeSerialization ();
            }
        }

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();

            UpdateColorBase ();
            
            if (textures != null)
            {
                foreach (var entry in textures)
                    entry.Value.OnAfterDeserialization ();
            }
            
            if (unitPatterns != null)
            {
                foreach (var entry in unitPatterns)
                    entry.Value.OnAfterDeserialization ();
            }
        }

        public void UpdateColorBase (bool updateColorDatabase = false)
        {
            if (colorBase == null)
                colorBase = new ColorBase ();

            #if !PB_MODSDK
            colorBase.hueMain = colorBase.basisMain.ToLch ().h;
            #else
            colorBase.hueMain = new HSBColor (colorBase.basisMain).h;
            #endif
            
            if (!updateColorDatabase)
                return;
            
            var colorDatabase = DataMultiLinkerUIColor.data;
            if (colorDatabase == null)
                return;

            foreach (var kvp in colorDatabase)
            {
                var colorContainer = kvp.Value;
                if (colorContainer == null)
                    continue;
                
                colorContainer.UpdateColor (colorBase);
            }
        }
        
        #if UNITY_EDITOR

        public void UpdateColorBasisAndDatabase () =>
            UpdateColorBase (true);

        #endif
    }
}

