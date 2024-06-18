using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    public class DataBlockPilotAccessoryVariant
    {
        public Vector3 hsvPrimary = new Vector3(0.0f, 0.5f, 0.5f);
        public Vector3 hsvSecondary = new Vector3(0.0f, 0.5f, 0.5f);
        public float glassOpacityOverride = 0.0f;
    }
    
    public class DataContainerPilotAppearancePreset : DataContainer
    {
        [ToggleGroup ("portraitTextureUsed", groupTitle: "Related portrait")]
        public bool portraitTextureUsed = false;
    
        [ToggleGroup ("portraitTextureUsed")]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.PilotPortraits)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.PilotPortraits, 128)", false)]
        public string portraitTextureName = string.Empty;

        [HideInInspector]
        public bool usableByFriendly = true;
        
        [HideInInspector]
        public bool usableByHostile = true;
        
        [BoxGroup, HideReferenceObjectPicker, HideLabel]
        public DataBlockPilotAppearance appearance = new DataBlockPilotAppearance ();
    }

    public class DataContainerPilotAccessoryHeadTop : DataContainerResourcePrefabPilot
    {
        public string holderOverride;
        public string hairOverride;

        public SortedDictionary<string, DataBlockPilotAccessoryVariant> variants = new SortedDictionary<string, DataBlockPilotAccessoryVariant> ();
    }
    
    public class DataContainerPilotAccessoryHeadFront : DataContainerResourcePrefabPilot
    {
        public string holderOverride;
        
        [ValueDropdown ("@variants.Keys")]
        public string variantDefault = "01";
        
        [ValueDropdown ("@DataLinkerSettingsPilot.data.accessoriesHeadTop.Keys")]
        public List<string> accessoriesHeadTopIncompatible;
        
        public SortedDictionary<string, DataBlockPilotAccessoryVariant> variants = new SortedDictionary<string, DataBlockPilotAccessoryVariant> ();
    }
    
    public class DataContainerResourcePrefabPilot : DataContainerResourcePrefab
    {
        public int priority;
        public bool generated = true;
        public bool usableByFriendly = true;
        public bool usableByHostile = true;
    }
    
    public class DataContainerResourceScriptablePilot<T> : DataContainerResourceScriptable<T> where T : ScriptableObject
    {
        public int priority;
        public bool usableByFriendly = true;
        public bool usableByHostile = true;
        
        public Vector2 interpolantRange = new Vector2 (0f, 1f);
        public List<string> modelCompatibility = new List<string> { "model_f", "model_m" };
    }
    
    public class DataContainerResourcePrefab : DataContainer
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        // [GUIColor ("GetPrefabColor")]
        public GameObject asset;
        
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            #if UNITY_EDITOR
            if (asset == null)
            {
                // path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (asset);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (!ResourceDatabaseManager.IsDatabaseAvailable ())
                return;
            
            asset = !string.IsNullOrEmpty (path) ? Resources.Load<GameObject> (path) : null;
            if (asset == null)
            {
                // Debug.LogWarning ($"Failed to load asset GameObject from path [{path}]");
                return;
            }
        }
        
        #if UNITY_EDITOR

        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);

        #endif
    }

    public class DataContainerResourceScriptable<T> : DataContainer where T : ScriptableObject
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        // [GUIColor ("GetPrefabColor")]
        public T asset;
        
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
        
            #if UNITY_EDITOR
            if (asset == null)
            {
                // path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (asset);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (!ResourceDatabaseManager.IsDatabaseAvailable ())
                return;
            
            asset = !string.IsNullOrEmpty (path) ? Resources.Load<T> (path) : null;
            if (asset == null)
            {
                // Debug.LogWarning ($"Failed to load asset {typeof (T).Name} from path [{path}]");
                return;
            }
        }
        
        #if UNITY_EDITOR

        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);

        #endif
    }

    public struct DataBlockPilotBlendShape
    {
        [ValueDropdown ("@AttributeExpressionUtility.GetStringsFromParentMethod ($property, \"GetBlendShapeKeys\", 2)")]
        public string key;
        
        [PropertyRange (-100, 100), OnValueChanged ("SnapValue")]
        public int value;

        private void SnapValue () => value = Mathf.RoundToInt (value / 10f) * 10;
    }
    
    public struct DataBlockPilotBlendShapeF
    {
        [ValueDropdown ("@AttributeExpressionUtility.GetStringsFromParentMethod ($property, \"GetBlendShapeKeysF\", 2)")]
        public string key;
        
        [PropertyRange (-100, 100), OnValueChanged ("SnapValue")]
        public int value;
        
        private void SnapValue () => value = Mathf.RoundToInt (value / 10f) * 10;
    }
    
    public struct DataBlockPilotBlendShapeM
    {
        [ValueDropdown ("@AttributeExpressionUtility.GetStringsFromParentMethod ($property, \"GetBlendShapeKeysM\", 2)")]
        public string key;
        
        [PropertyRange (-100, 100), OnValueChanged ("SnapValue")]
        public int value;
        
        private void SnapValue () => value = Mathf.RoundToInt (value / 10f) * 10;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockPilotBlendPresetUnified
    {
        [ListDrawerSettings (DefaultExpandedState = false, CustomAddFunction = "@new DataBlockPilotBlendShape ()")]
        public List<DataBlockPilotBlendShape> blendShapes = new List<DataBlockPilotBlendShape> { new DataBlockPilotBlendShape { key = string.Empty } };

        #if UNITY_EDITOR

        private IEnumerable<string> GetBlendShapeKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotModel modelLink = null;
            bool modelFound = !string.IsNullOrEmpty ("model_f") && db.models.TryGetValue ("model_f", out modelLink);
            return modelFound && modelLink != null && modelLink.blendShapeKeys != null ? modelLink.blendShapeKeys : null;
        }

        #endif
    }

    [HideReferenceObjectPicker]
    public class DataBlockPilotBlendPresetSplit
    {
        [ListDrawerSettings (DefaultExpandedState = false, CustomAddFunction = "@new DataBlockPilotBlendShapeF ()")]
        public List<DataBlockPilotBlendShapeF> blendShapesF = new List<DataBlockPilotBlendShapeF> { new DataBlockPilotBlendShapeF { key = string.Empty } };
        
        [ListDrawerSettings (DefaultExpandedState = false, CustomAddFunction = "@new DataBlockPilotBlendShapeM ()")]
        public List<DataBlockPilotBlendShapeM> blendShapesM = new List<DataBlockPilotBlendShapeM> { new DataBlockPilotBlendShapeM { key = string.Empty } };

        #if UNITY_EDITOR

        private IEnumerable<string> GetBlendShapeKeysF ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotModel modelLink = null;
            bool modelFound = !string.IsNullOrEmpty ("model_f") && db.models.TryGetValue ("model_f", out modelLink);
            return modelFound && modelLink != null && modelLink.blendShapeKeys != null ? modelLink.blendShapeKeys : null;
        }
        
        private IEnumerable<string> GetBlendShapeKeysM ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotModel modelLink = null;
            bool modelFound = !string.IsNullOrEmpty ("model_m") && db.models.TryGetValue ("model_m", out modelLink);
            return modelFound && modelLink != null && modelLink.blendShapeKeys != null ? modelLink.blendShapeKeys : null;
        }

        #endif
    }

    public class DataBlockPilotAppearanceOverlay
    {
        public string key;
        public string variant;
    }

    public class DataBlockPilotAppearance
    {
        public const int versionExpected = 1;
        
        [HideInInspector]
        public int version = 0;
        
        [ValueDropdown ("GetModelKeys")]
        public string model;
        
        [ValueDropdown ("GetSkinMainKeys")]
        public string skinMain;
        
        [ValueDropdown ("GetSkinOverlayKeys")]
        public string skinOverlay;
        
        [ValueDropdown ("GetSkinTintKeys")]
        public string skinTintPreset;
        
        [ValueDropdown ("GetHairMainKeys")]
        [InlineButtonClear]
        public string hairMain;
        
        [ValueDropdown ("GetHairTintKeys")]
        public string hairMainTintPreset;
        
        [ValueDropdown ("GetHairFacialKeys")]
        [InlineButtonClear]
        public string hairFacial;

        [ValueDropdown ("GetHairEyebrowsKeys")]
        [InlineButtonClear]
        public string hairEyebrows;
        
        [ValueDropdown ("GetHairTintKeys")]
        public string hairFacialTintPreset;

        [ValueDropdown ("GetEyesTintKeys")]
        public string eyesTintPreset;

        [ValueDropdown ("GetAccessoryHeadTopKeys")]
        [InlineButtonClear]
        public string accessoryHeadTop;
        
        [ValueDropdown ("GetAccessoryHeadTopVariantKeys")]
        [ShowIf ("@!string.IsNullOrEmpty (accessoryHeadTop)")]
        [InlineButtonClear]
        public string accessoryHeadTopVariant;
        
        [ValueDropdown ("GetAccessoryHeadFrontKeys")]
        [InlineButtonClear]
        public string accessoryHeadFront;
        
        [ValueDropdown ("GetAccessoryHeadFrontVariantKeys")]
        [ShowIf ("@!string.IsNullOrEmpty (accessoryHeadFront)")]
        [InlineButtonClear]
        public string accessoryHeadFrontVariant;
        
        [ValueDropdown ("GetBlendPresetArchetypeKeys")]
        [InlineButtonClear]
        public string blendPresetArchetype;
        
        [ValueDropdown ("GetBlendPresetFaceVariationJawKeys")]
        [InlineButtonClear]
        public string blendPresetFaceVariationJaw;

        [ValueDropdown ("GetBlendPresetFaceVariationNoseKeys")]
        [InlineButtonClear]
        public string blendPresetFaceVariationNose;
        
        [ValueDropdown ("GetBlendPresetBuildKeys")]
        [InlineButtonClear]
        public string blendPresetBuild;

        [InlineButtonClear]
        [ShowIf ("@blendShapes != null")]
        [ListDrawerSettings (DefaultExpandedState = false, CustomAddFunction = "@new DataBlockPilotBlendShape ()")]
        public List<DataBlockPilotBlendShape> blendShapes;

        public string portrait;
        public string portraitVariant;
        
        public DataBlockPilotAppearance () { }

        public DataBlockPilotAppearance (DataBlockPilotAppearance source)
        {
            CopyFrom (source);
        }
        
        public void CopyFrom (DataBlockPilotAppearance source)
        {
            if (source == null)
                return;

            version = source.version;
            model = source.model;
 
            skinMain = source.skinMain;
            skinOverlay = source.skinOverlay;
            skinTintPreset = source.skinTintPreset;
            eyesTintPreset = source.eyesTintPreset;
            
            hairMain = source.hairMain;
            hairFacial = source.hairFacial;
            
            hairMainTintPreset = source.hairMainTintPreset;
            hairFacialTintPreset = source.hairFacialTintPreset;
            hairEyebrows = source.hairEyebrows;

            accessoryHeadTop = source.accessoryHeadTop;
            accessoryHeadTopVariant = source.accessoryHeadTopVariant;
            
            accessoryHeadFront = source.accessoryHeadFront;
            accessoryHeadFrontVariant = source.accessoryHeadFrontVariant;

            blendPresetArchetype = source.blendPresetArchetype;
            blendPresetBuild = source.blendPresetBuild;
            
            blendPresetFaceVariationJaw = source.blendPresetFaceVariationJaw;
            blendPresetFaceVariationNose = source.blendPresetFaceVariationNose;

            portrait = source.portrait;
            portraitVariant = source.portraitVariant;

            if (source.blendShapes != null && source.blendShapes.Count > 0)
            {
                if (blendShapes == null)
                    blendShapes = new List<DataBlockPilotBlendShape> (source.blendShapes);
                else
                {
                    blendShapes.Clear ();
                    blendShapes.AddRange (source.blendShapes);
                }
            }
            else
            {
                if (blendShapes != null)
                    blendShapes.Clear ();
            }    
        }
        
        #if UNITY_EDITOR

        [Button, ButtonGroup, ShowIf ("@blendShapes == null")]
        private void AddBlendShapes () => blendShapes = new List<DataBlockPilotBlendShape> { new DataBlockPilotBlendShape { key = string.Empty } };

        private IEnumerable<string> GetModelKeys => DataLinkerSettingsPilot.data?.models?.Keys;
        private IEnumerable<string> GetHairMainKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            return db?.GetFilteredResourceKeys (0, model, db?.hairMain, true);
        }
        
        private IEnumerable<string> GetHairFacialKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            return db?.GetFilteredResourceKeys (0, model, db?.hairFacial, true);
        }

        private IEnumerable<string> GetHairEyebrowsKeys => DataLinkerSettingsPilot.data?.hairEyebrows?.Keys;
        
        private IEnumerable<string> GetSkinMainKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            return db?.GetFilteredResourceKeys (0, model, db?.skinsMain);
        }
        
        private IEnumerable<string> GetSkinOverlayKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            return db?.GetFilteredResourceKeys (0, model, db?.skinsOverlays);
        }
        
        private IEnumerable<string> GetAccessoryHeadTopKeys => DataLinkerSettingsPilot.data?.accessoriesHeadTop?.Keys;
        private IEnumerable<string> GetAccessoryHeadFrontKeys => DataLinkerSettingsPilot.data?.accessoriesHeadFront?.Keys;
        
        private IEnumerable<string> GetSkinTintKeys => DataLinkerSettingsPilot.data?.skinTints.Keys;
        private IEnumerable<string> GetHairTintKeys => DataLinkerSettingsPilot.data?.hairTints.Keys;
        private IEnumerable<string> GetEyesTintKeys => DataLinkerSettingsPilot.data?.eyeTints.Keys;

        private IEnumerable<string> GetAccessoryHeadTopVariantKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotAccessoryHeadTop assetLink = null;
            bool assetFound = !string.IsNullOrEmpty (accessoryHeadTop) && db.accessoriesHeadTop.TryGetValue (accessoryHeadTop, out assetLink);
            return assetFound && assetLink != null && assetLink.variants != null ? assetLink.variants.Keys : null;
        }
        
        private IEnumerable<string> GetAccessoryHeadFrontVariantKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotAccessoryHeadFront assetLink = null;
            bool assetFound = !string.IsNullOrEmpty (accessoryHeadFront) && db.accessoriesHeadFront.TryGetValue (accessoryHeadFront, out assetLink);
            return assetFound && assetLink != null && assetLink.variants != null ? assetLink.variants.Keys : null;
        }
        
        private IEnumerable<string> GetBlendShapeKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotModel modelLink = null;
            bool modelFound = !string.IsNullOrEmpty (model) && db.models.TryGetValue (model, out modelLink);
            return modelFound && modelLink != null && modelLink.blendShapeKeys != null ? modelLink.blendShapeKeys : null;
        }
        
        private IEnumerable<string> GetBlendPresetArchetypeKeys => DataLinkerSettingsPilot.data?.blendPresetsArchetypes.Keys;
        private IEnumerable<string> GetBlendPresetBuildKeys => DataLinkerSettingsPilot.data?.blendPresetsBuilds.Keys;
        
        private IEnumerable<string> GetBlendPresetFaceVariationNoseKeys => DataLinkerSettingsPilot.data?.blendPresetsFaceVariationNose.Keys;
        private IEnumerable<string> GetBlendPresetFaceVariationJawKeys => DataLinkerSettingsPilot.data?.blendPresetsFaceVariationJaw.Keys;
        
        #endif
    }

    public class DataContainerPilotModel : DataContainer
    {
        public bool enabled;
        public string viewKey;
        
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> blendShapeKeys;
    }

    public class DataContainerPilotEyebrows : DataContainer
    {
        [PropertyRange (0f, 3f)]
        public float eyebrowsStyle = 0f;

        [PropertyRange (0f, 1f)]
        public float eyebrowsThickness = 0.5f;
    }
    
    [HideReferenceObjectPicker]
    public class DataContainerPilotTint : DataContainer
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color color;
        
        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genX = 0f;
        
        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genY = 0f;
        
        [HideLabel, HorizontalGroup (32f)]
        public bool generated = true;
    }
    
    [HideReferenceObjectPicker]
    public class DataContainerPilotTintSkin : DataContainer
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color color = new Color (0.7f, 0.65f, 0.6f);
        
        [HideLabel, HorizontalGroup (48f)]
        public float alphaRd = 0.05f;
        
        [HideLabel, HorizontalGroup (48f)]
        public float alphaDk = 0.05f;

        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genX = 0f;
        
        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genY = 0f;
        
        [HideLabel, HorizontalGroup (20f)]
        public bool generated = true;
        
        [HideLabel, HorizontalGroup (20f)]
        public bool selectable = true;
    }
    
    [HideReferenceObjectPicker]
    public class DataContainerPilotTintHair : DataContainer
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color color;
        
        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genX = 0f;
        
        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genY = 0f;
        
        [HideLabel, HorizontalGroup (24f)]
        public bool generated = true;
        
        [HideLabel, HorizontalGroup (24f)]
        public bool selectable = true;
    }

    [HideReferenceObjectPicker]
    public class DataContainerPilotTintEyes : DataContainer
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: false)]
        public Color colorMain;

        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: true)]
        public Color colorSecondary;
        
        [HideLabel, HorizontalGroup, PropertyRange (0f, 1f)]
        public float genX = 0f;
                
        [HideLabel, HorizontalGroup (24f)]
        public bool generated = true;
        
        [HideLabel, HorizontalGroup (24f)]
        public bool selectable = true;
    }

    public class DataBlockOverlayVariant
    {
        public Color filterColor = new Color (1f, 1f, 1f, 1f);
        public Vector4 filterInputs = new Vector4 (1f, 1f, 1f, 1f);
    }

    [Serializable]
    public class DataContainerSettingsPilot : DataContainerUnique
    {
        private const string tgValues = "Values";
        private const string tgPresets = "Presets";
        private const string tgShapes = "Shapes";
        private const string tgAssets = "Assets";
        private const string tgColors = "Colors";
        
        [TabGroup (tgValues)]
        public float pilotAnimationSpeedBase = 0.01f;
        
        [TabGroup (tgValues)]
        public float pilotAnimationSpeedEditor = 0f;
        
        [TabGroup (tgValues)]
        public float pilotAnimationSpeedCombat = 1f;
        
        [TabGroup (tgValues)]
        [ValueDropdown ("@DataMultiLinkerPilotPersonality.data.Keys"), InlineButtonClear]
        public string pilotPersonalityOverrideEditing = "pilot_personality_default";
        
        [TabGroup (tgValues)]
        public float accessoryHeadTopChanceFriendly = 0.25f;
        
        [TabGroup (tgValues)]
        public float accessoryHeadTopChanceHostile = 1f;
        
        [TabGroup (tgValues)]
        public float accessoryHeadFrontChanceFriendly = 0.15f;
        
        [TabGroup (tgValues)]
        public float accessoryHeadFrontChanceHostile = 0f;
        
        [TabGroup (tgValues)]
        public float blendPresetMultiplierJaw = 0.5f;
        
        [TabGroup (tgValues)]
        public float blendPresetMultiplierNose = 0.5f;
        
        [TabGroup (tgValues)]
        public float blendPresetBuildChance = 0.5f;
        
        [TabGroup (tgValues)]
        public float blendPresetFaceVariationJawChance = 0f;
        
        [TabGroup (tgValues)]
        public float blendPresetFaceVariationNoseChance = 0f;

        [TabGroup (tgValues)]
        public bool overlaySupport = false;

        [TabGroup (tgPresets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotAppearancePreset> appearancePresets;

        [InfoBox ("These variations should use face type blend shapes. Since each model has differently named face types, there are separate blend shape lists per model.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockPilotBlendPresetSplit> blendPresetsArchetypes;
        
        [InfoBox ("These variations cover jaw and chin blend shapes")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockPilotBlendPresetUnified> blendPresetsFaceVariationJaw;
        
        [InfoBox ("These variations cover various nose blend shapes. Current content uses key scheme N*_W* where N is nose variation number and W is width variation.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockPilotBlendPresetUnified> blendPresetsFaceVariationNose;
        
        [InfoBox ("These variations cover body shapes - musculature, weight, etc. Current content uses key scheme W*_M* where W is amount of weight and M is amount of muscle.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockPilotBlendPresetUnified> blendPresetsBuilds;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotModel> models;
        
        [YamlIgnore, HideInInspector]
        public List<DataContainerPilotModel> modelsEnabled = new List<DataContainerPilotModel> ();
    
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceScriptablePilot<CharacterHairData>> hairMain;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceScriptablePilot<CharacterHairData>> hairFacial;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotAccessoryHeadTop> accessoriesHeadTop;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotAccessoryHeadFront> accessoriesHeadFront;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceScriptablePilot<CharacterSkinTexturesData>> skinsMain;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceScriptablePilot<CharacterSkinTexturesData>> skinsOverlays;

        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotEyebrows> hairEyebrows;

        [TabGroup (tgColors)]
        [InfoBox ("These colors are multiplied by skin textures, check the effect of the multiplication with skin textures before configuring them. A specific color entered here will not yield a final skin color that is anywhere near.")]
        [LabelText ("Skin color presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine, KeyColumnWidth = 32, ValueLabel = "Color, Redness, Darkening, Axis (X/Y), Generated, Selectable")]
        public SortedDictionary<string, DataContainerPilotTintSkin> skinTints;

        public SortedDictionary<string, DataBlockOverlayVariant> overlayVariants;

        [TabGroup (tgColors)]
        [Button (ButtonSizes.Large), PropertyOrder (-1)]
        public void ApplyEqualDistributionToSkinTints()
        {
            int counter = 0;
            if (skinTints == null || skinTints.Count < 1)
                return;

            // The amount we need to add to an entry's axis value to make sure every dictionary entry is evenly spread
            float stepAmount = 1f/((float)skinTints.Count - 1f);
            foreach (KeyValuePair<string, DataContainerPilotTintSkin> skinTintEntry in skinTints)
            {
                skinTintEntry.Value.genX = (stepAmount * counter).Truncate(3);
                counter += 1;
            }
        }
        
        [TabGroup (tgColors)]
        [LabelText ("Hair color presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine, KeyColumnWidth = 32, ValueLabel = "Color, Axis (X/Y), Generated, Selectable")]
        public SortedDictionary<string, DataContainerPilotTintHair> hairTints;

        [TabGroup (tgColors)]
        [LabelText ("Eye color presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine, KeyColumnWidth = 32, ValueLabel = "Color, Color Secondary, Axis (X), Generated, Selectable")]
        public SortedDictionary<string, DataContainerPilotTintEyes> eyeTints;

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();

            if (hairMain != null)
            {
                foreach (var kvp in hairMain)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }

            if (hairFacial != null)
            {
                foreach (var kvp in hairFacial)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }

            if (accessoriesHeadTop != null)
            {
                foreach (var kvp in accessoriesHeadTop)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }
            
            if (accessoriesHeadFront != null)
            {
                foreach (var kvp in accessoriesHeadFront)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }
            
            if (skinsMain != null)
            {
                foreach (var kvp in skinsMain)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }
            
            if (skinsOverlays != null)
            {
                foreach (var kvp in skinsOverlays)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }
            
            if (skinTints != null)
            {
                foreach (var kvp in skinTints)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }
            
            if (hairTints != null)
            {
                foreach (var kvp in hairTints)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnBeforeSerialization ();
                }
            }
        }

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            
            modelsEnabled.Clear ();

            if (models != null)
            {
                foreach (var kvp in models)
                {
                    var link = kvp.Value;
                    if (link != null)
                    {
                        link.OnAfterDeserialization (kvp.Key);
                        if (link.enabled)
                            modelsEnabled.Add (link);
                    }
                }
            }
            
            if (hairMain != null)
            {
                foreach (var kvp in hairMain)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (hairFacial != null)
            {
                foreach (var kvp in hairFacial)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }

            if (hairEyebrows != null)
            {
                foreach (var kvp in hairEyebrows)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (accessoriesHeadTop != null)
            {
                foreach (var kvp in accessoriesHeadTop)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (accessoriesHeadFront != null)
            {
                foreach (var kvp in accessoriesHeadFront)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (skinsMain != null)
            {
                foreach (var kvp in skinsMain)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (skinsOverlays != null)
            {
                foreach (var kvp in skinsOverlays)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (skinTints != null)
            {
                foreach (var kvp in skinTints)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            if (hairTints != null)
            {
                foreach (var kvp in hairTints)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }

            if (eyeTints != null)
            {
                foreach (var kvp in eyeTints)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            
            /*
            if (skinTints != null)
            {
                foreach (var kvp in skinTints)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }
            */
        }

        private List<string> filteredKeys = new List<string> ();


        
        public List<string> GetFilteredResourceKeys<T> 
            (int factionFilter, string modelFilter, IDictionary<string, DataContainerResourceScriptablePilot<T>> collection, bool includeEmpty = false, float interpolantFilter = -1f) where T : ScriptableObject
        {
            bool interpolantFilterUsed = interpolantFilter >= 0f && interpolantFilter < 1f;
            bool factionFilterUsed = factionFilter == 1 || factionFilter == 2;
            bool modelFilterUsed = !string.IsNullOrEmpty (modelFilter);
            filteredKeys.Clear ();
            
            if (includeEmpty)
                filteredKeys.Add (string.Empty);

            foreach (var kvp in collection)
            {
                var link = kvp.Value;
                
                if (factionFilterUsed)
                {
                    if (factionFilter == 1 && !link.usableByFriendly)
                        continue;

                    if (factionFilter == 2 && !link.usableByHostile)
                        continue;
                }

                if (modelFilterUsed && link.modelCompatibility != null && !link.modelCompatibility.Contains (modelFilter))
                    continue;

                if (interpolantFilterUsed)
                {
                    var interpolantMin = Mathf.Clamp01 (link.interpolantRange.x);
                    var interpolantMax = Mathf.Clamp01 (link.interpolantRange.y);
                    if (interpolantFilter < interpolantMin || interpolantFilter > interpolantMax)
                        continue;
                }
                
                filteredKeys.Add (link.key);
            }

            return filteredKeys;
        }
        
        public DataContainerResourceScriptablePilot<T> GetRandomResource<T> 
            (int factionFilter, string modelFilter, float interpolantFilter, IDictionary<string, DataContainerResourceScriptablePilot<T>> collection) where T : ScriptableObject
        {
            var filteredKeysFound = GetFilteredResourceKeys (factionFilter, modelFilter, collection, false, interpolantFilter);
            if (filteredKeysFound.Count == 0)
                return null;

            var selectedKey = filteredKeysFound.GetRandomEntry ();
            var selection = collection[selectedKey];
            return selection;
        }
        
        public string GetShiftedResourceKey<T>
            (string keyCurrent, bool forward, bool insertEmpty, int factionFilter, string modelFilter, float interpolantFilter, IDictionary<string, DataContainerResourceScriptablePilot<T>> collection) where T : ScriptableObject
        {
            var filteredKeysFound = GetFilteredResourceKeys (factionFilter, modelFilter, collection, false, interpolantFilter);
            if (filteredKeysFound.Count == 0)
                return null;

            if (insertEmpty)
                filteredKeysFound.Insert (0, null);
            
            var indexCurrent = filteredKeysFound.IndexOf (keyCurrent);
            if (indexCurrent == -1)
                return filteredKeysFound[0];

            var indexShifted = indexCurrent.OffsetAndWrap (forward, filteredKeysFound);
            var keyShifted = filteredKeysFound[indexShifted];
            return keyShifted;
        }

        public List<string> GetFilteredPrefabKeys<T> 
            (int factionFilter, IDictionary<string, T> collection, bool includeEmpty = false, bool includeNonGenerated = false) where T : DataContainerResourcePrefabPilot
        {
            bool factionFilterUsed = factionFilter == 1 || factionFilter == 2;
            filteredKeys.Clear ();
            
            if (includeEmpty)
                filteredKeys.Add (string.Empty);

            foreach (var kvp in collection)
            {
                var link = kvp.Value;

                if (!link.generated && !includeNonGenerated)
                    continue;
                
                if (factionFilterUsed)
                {
                    if (factionFilter == 1 && !link.usableByFriendly)
                        continue;

                    if (factionFilter == 2 && !link.usableByHostile)
                        continue;
                }

                filteredKeys.Add (link.key);
            }
            
            return filteredKeys;
        }
        
        public T GetRandomPrefab<T> 
            (int factionFilter, IDictionary<string, T> collection) where T : DataContainerResourcePrefabPilot
        {
            var filteredKeysFound = GetFilteredPrefabKeys (factionFilter, collection);
            if (filteredKeysFound.Count == 0)
                return null;
        
            var selectedKey = filteredKeysFound.GetRandomEntry ();
            var selection = collection[selectedKey];
            return selection;
        }

        public string GetShiftedPrefabKey<T>
            (string keyCurrent, bool forward, bool insertEmpty, int factionFilter, IDictionary<string, T> collection) where T : DataContainerResourcePrefabPilot
        {
            var filteredKeysFound = GetFilteredPrefabKeys (factionFilter, collection, false, true);
            if (filteredKeysFound.Count == 0)
                return null;
            
            if (insertEmpty)
                filteredKeysFound.Insert (0, null);

            var indexCurrent = filteredKeysFound.IndexOf (keyCurrent);
            if (indexCurrent == -1)
                return filteredKeysFound[0];

            var indexShifted = indexCurrent.OffsetAndWrap (forward, filteredKeysFound);
            var keyShifted = filteredKeysFound[indexShifted];
            return keyShifted;
        }

        public DataContainerPilotTintSkin GetTintPresetSkinFromInterpolant (float interpolant)
        {
            interpolant = Mathf.Clamp01 (interpolant);
            float distanceBest = 1f;
            DataContainerPilotTintSkin presetBest = null;
            
            foreach (var kvp in skinTints)
            {
                var presetCandidate = kvp.Value;
                if (presetCandidate == null || !presetCandidate.generated)
                    continue;

                var distance = Mathf.Abs (presetCandidate.genX - interpolant);
                if (distance < distanceBest)
                {
                    distanceBest = distance;
                    presetBest = presetCandidate;
                }
            }
            
            return presetBest;
        }
        
        public DataContainerPilotTintHair GetTintPresetHairFromInterpolant (float interpolant)
        {
            interpolant = Mathf.Clamp01 (interpolant);
            float distanceBest = 1f;
            DataContainerPilotTintHair presetBest = null;
            
            foreach (var kvp in hairTints)
            {
                var presetCandidate = kvp.Value;
                if (presetCandidate == null || !presetCandidate.generated)
                    continue;

                var distance = Mathf.Abs (presetCandidate.genX - interpolant);
                if (distance < distanceBest)
                {
                    distanceBest = distance;
                    presetBest = presetCandidate;
                }
            }
            
            return presetBest;
        }

        public DataContainerPilotTintEyes GetTintPresetEyesFromInterpolant (float interpolant)
        {
            interpolant = Mathf.Clamp01 (interpolant);
            float distanceBest = 1f;
            DataContainerPilotTintEyes presetBest = null;
            
            foreach (var kvp in eyeTints)
            {
                var presetCandidate = kvp.Value;
                if (presetCandidate == null || !presetCandidate.generated)
                    continue;

                var distance = Mathf.Abs (presetCandidate.genX - interpolant);
                if (distance < distanceBest)
                {
                    distanceBest = distance;
                    presetBest = presetCandidate;
                }
            }

            return presetBest;
        }
        
        public string GetShiftedKey<T> (string keyCurrent, bool forward, bool insertEmpty, IDictionary<string, T> dictionary, Func<T, bool> onFilter = null)
        {
            if (dictionary == null || dictionary.Count == 0)
                return null;

            bool onFilterCallbackPresent = onFilter != null;
            
            filteredKeys.Clear ();
            
            if (insertEmpty)
                filteredKeys.Add (string.Empty);
            
            foreach (var kvp in dictionary)
            {
                if (onFilterCallbackPresent)
                {
                    bool filterPassed = onFilter.Invoke (kvp.Value);
                    if (!filterPassed)
                        continue;
                }
                
                filteredKeys.Add (kvp.Key);
            }

            var indexCurrent = filteredKeys.IndexOf (keyCurrent);
            if (indexCurrent == -1)
                return filteredKeys[0];

            var indexShifted = indexCurrent.OffsetAndWrap (forward, filteredKeys);
            var keyShifted = filteredKeys[indexShifted];
            return keyShifted;
        }
        
        public string GetKeyIndexFormatted<T> (string key, IDictionary<string, T> dictionary)
        {
            if (string.IsNullOrEmpty (key))
                return "—";
        
            if (dictionary == null || dictionary.Count == 0)
                return "—";

            int indexSelected = -1;
            int i = 0;
            foreach (var kvp in dictionary)
            {
                if (string.Equals (kvp.Key, key))
                {
                    indexSelected = i;
                    break;
                }
                ++i;
            }

            if (indexSelected == -1)
                return "—";

            return (indexSelected + 1).ToString ("D2");
        }
        
        public string GetKeyAdjusted (string key, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty (key))
                return "—";

            if (replacements == null || replacements.Count == 0)
                return key;

            foreach (var kvp in replacements)
            {
                key = key.Replace (kvp.Key, kvp.Value);
            }
            
            return key;
        }
        
        public DataContainerResourceScriptablePilot<CharacterHairData> GetRandomAssetHairMain (bool friendly, string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, hairMain);
        
        public DataContainerResourceScriptablePilot<CharacterHairData> GetRandomAssetHairFacial (bool friendly, string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, hairFacial);
        
        public DataContainerResourceScriptablePilot<CharacterSkinTexturesData> GetRandomAssetSkinMain (string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (0, modelFilter, interpolantFilter, skinsMain);
            
        public DataContainerResourceScriptablePilot<CharacterSkinTexturesData> GetRandomAssetSkinOverlay (bool friendly, string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, skinsOverlays);
        
        public DataContainerPilotAccessoryHeadTop GetRandomAccessoryHeadTop (bool friendly) => 
            GetRandomPrefab (friendly ? 1 : 2, accessoriesHeadTop);
        
        public DataContainerPilotAccessoryHeadFront GetRandomAccessoryHeadFront (bool friendly) => 
            GetRandomPrefab (friendly ? 1 : 2, accessoriesHeadFront);

        public List<string> GetFilteredAppearancePresetKeys (int factionFilter, string modelFilter)
        {
            bool factionFilterUsed = factionFilter == 1 || factionFilter == 2;
            bool modelFilterUsed = !string.IsNullOrEmpty (modelFilter);
            filteredKeys.Clear ();

            foreach (var kvp in appearancePresets)
            {
                var link = kvp.Value;
                if (link == null || link.appearance == null)
                    continue;
                
                if (factionFilterUsed)
                {
                    if (factionFilter == 1 && !link.usableByFriendly)
                        continue;

                    if (factionFilter == 2 && !link.usableByHostile)
                        continue;
                }

                if (modelFilterUsed && link.appearance.model != modelFilter)
                    continue;
                
                filteredKeys.Add (link.key);
            }

            return filteredKeys;
        }
        
        public DataBlockPilotAppearance GetRandomAppearancePreset (int factionFilter, string modelFilter)
        {
            var filteredKeysFound = GetFilteredAppearancePresetKeys (factionFilter, modelFilter);
            if (filteredKeysFound.Count == 0)
                return null;

            var selectedKey = filteredKeysFound.GetRandomEntry ();
            var selection = appearancePresets[selectedKey];
            var appearance = selection.appearance;
            return appearance;
        }
        
        public void RandomizePilotAppearance (DataBlockPilotAppearance data, bool friendly, string modelKeyOverride = null)
        {
            var model = modelsEnabled.GetRandomEntry ();
            var modelKey = !string.IsNullOrEmpty (modelKeyOverride) ? modelKeyOverride : model.key;
            
            var skinOverlay = GetRandomAssetSkinOverlay (friendly, modelKey, 0.5f);
            var skinOverlayKey = skinOverlay?.key;
            var sharedRandomPercentage = Random.Range (0.0f, 1.0f);

            data.version = DataBlockPilotAppearance.versionExpected;
            data.model = modelKey;
            data.skinOverlay = skinOverlayKey; 
            
            var faceKey = blendPresetsArchetypes.GetRandomKey ();
            bool faceKeyBasedGeneration = false;
            float hairInterpolant = 0.5f;
            
            string skinMainKey = null;
            string skinTintPreset = null;
            string hairMainTintPreset = null;
            string hairFacialTintPreset = null;
            string eyesTintPreset = null;

            if (!string.IsNullOrEmpty (faceKey))
            {
                if (faceKey.StartsWith ("african"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_a_alt" : "skin_a";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0.6f, 1f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0.7f, 1f))?.key;
                    hairInterpolant = 1f;
                }
                else if (faceKey.StartsWith ("eastern"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_b_alt" : "skin_b";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0.15f, 0.4f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0.6f, 1f))?.key;
                    hairInterpolant = 0f;
                }
                else if (faceKey.StartsWith ("western"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_b_alt" : "skin_b";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0f, 0.4f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0f, 0.4f))?.key;
                    hairInterpolant = 0.5f;
                }
                else if (faceKey.StartsWith ("generic"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_b_alt" : "skin_b";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0.3f, 0.6f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0.3f, 0.6f))?.key;
                    hairInterpolant = 0.5f;
                }
            }
            
            if (!faceKeyBasedGeneration)
            {
                skinMainKey = GetRandomAssetSkinMain (modelKey, sharedRandomPercentage)?.key;
                
                var sharedInterpolant = skinMainKey.Contains("skin_a") ? Random.Range (0.5f, 1f) : Random.Range (0f, 0.5f);
                var sharedInterpolantShifted = Mathf.Clamp01 (sharedInterpolant + Random.Range (-0.1f, 0.1f));
                
                skinTintPreset = GetTintPresetSkinFromInterpolant (sharedInterpolant)?.key;
                hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (sharedInterpolantShifted)?.key;
            }

            var config = DataShortcuts.pilots;

            string buildKey = null;
            bool buildUsed = Random.Range (0f, 1f) < config.blendPresetBuildChance;
            if (buildUsed && config.blendPresetsBuilds != null && config.blendPresetsBuilds.Count > 0)
                buildKey = blendPresetsBuilds.GetRandomKey ();
            
            string faceVariationJawKey = null;
            bool faceVariationJawUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationJawChance;
            if (faceVariationJawUsed && config.blendPresetsFaceVariationJaw != null && config.blendPresetsFaceVariationJaw.Count > 0)
                faceVariationJawKey = blendPresetsFaceVariationJaw.GetRandomKey ();
            
            string faceVariationNoseKey = null;
            bool faceVariationNoseUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationNoseChance;
            if (faceVariationNoseUsed && config.blendPresetsFaceVariationNose != null && config.blendPresetsFaceVariationNose.Count > 0)
                faceVariationNoseKey = blendPresetsFaceVariationNose.GetRandomKey ();

            eyesTintPreset = GetTintPresetEyesFromInterpolant (Random.Range (0f, 1f))?.key;
            data.eyesTintPreset = eyesTintPreset;

            data.blendPresetArchetype = faceKey;
            data.blendPresetBuild = buildKey;
            data.blendPresetFaceVariationJaw = faceVariationJawKey;
            data.blendPresetFaceVariationNose = faceVariationNoseKey;

            data.skinMain = skinMainKey;
            data.skinTintPreset = skinTintPreset;
            data.hairMainTintPreset = hairMainTintPreset;
            data.hairFacialTintPreset = hairFacialTintPreset;
            
            var hairMain = GetRandomAssetHairMain (friendly, modelKey, hairInterpolant);
            var hairMainKey = hairMain?.key;
            data.hairMain = hairMainKey;
            
            bool hairFacialUsed = Random.Range (0f, 1f) > 0.9f;
            if (!hairFacialUsed)
                data.hairFacial = null;
            else
            {
                var hairFacial = GetRandomAssetHairFacial (friendly, modelKey, hairInterpolant);
                var hairFacialKey = hairFacial?.key;
                data.hairFacial = hairFacialKey;
            }

            string hairEyebrowsPresetKey = null;
            if (hairEyebrows != null && hairEyebrows.Count > 0)
            {
                hairEyebrowsPresetKey = hairEyebrows.GetRandomKey ();
                data.hairEyebrows = hairEyebrowsPresetKey;
            }
            
            var accessoryHeadTopChance = friendly ? accessoryHeadTopChanceFriendly : accessoryHeadTopChanceHostile;
            var accessoryHeadTopUsed = Random.Range (0f, 1f) < accessoryHeadTopChance;
            var accessoryHeadTop = accessoryHeadTopUsed ? GetRandomAccessoryHeadTop (friendly) : null;
            var accessoryHeadTopKey = accessoryHeadTop?.key;
            
            var accessoryHeadTopVariant = 
                accessoryHeadTop != null && 
                accessoryHeadTop.variants != null && 
                accessoryHeadTop.variants.Count > 0 ? 
                accessoryHeadTop.variants.GetRandomKey () : null;
            
            data.accessoryHeadTop = accessoryHeadTopKey;
            data.accessoryHeadTopVariant = accessoryHeadTopVariant;
            
            var accessoryHeadFrontChance = friendly ? accessoryHeadFrontChanceFriendly : accessoryHeadFrontChanceHostile;
            var accessoryHeadFrontUsed = !accessoryHeadTopUsed && Random.Range (0f, 1f) < accessoryHeadFrontChance;
            var accessoryHeadFront = accessoryHeadFrontUsed ? GetRandomAccessoryHeadFront (friendly) : null;
            var accessoryHeadFrontKey = accessoryHeadFront?.key;
            var accessoryHeadFrontVariant = accessoryHeadFront?.variantDefault;

            data.accessoryHeadFront = accessoryHeadFrontKey;
            data.accessoryHeadFrontVariant = accessoryHeadFrontVariant;
        }
        
        public void GetRandomColors (Vector2 interpolantRange, out string skinTintKey, out string hairTintKey, out string eyeTintKey)
        {
            var sharedInterpolant = Random.Range (interpolantRange.x, interpolantRange.y);
            var sharedInterpolantShifted = Mathf.Clamp01 (sharedInterpolant + Random.Range (-0.1f, 0.1f));

            var skinTintPreset = GetTintPresetSkinFromInterpolant (sharedInterpolant);
            var hairTintPreset = GetTintPresetHairFromInterpolant (sharedInterpolantShifted);
            var eyeTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0.0f, 1.0f));

            skinTintKey = skinTintPreset?.key;
            hairTintKey = hairTintPreset?.key;
            eyeTintKey = eyeTintPreset?.key;
        }
        
        /*
        [Button, PropertyOrder (-1)]
        private void GeneratePortraitPresets ()
        {
            var keys = TextureManager.GetExposedTextureKeys (TextureGroupKeys.PilotPortraits);
            
            if (appearancePresets == null)
                appearancePresets = new SortedDictionary<string, DataContainerPilotAppearancePreset> ();

            foreach (string key in keys)
            {
                if (appearancePresets.ContainsKey (key))
                {
                    Debug.LogWarning ($"Appearance presets already contain portrait key {key}");
                    continue;                    
                }

                var preset = new DataContainerPilotAppearancePreset ();
                appearancePresets.Add (key, preset);
                
                preset.portraitTextureUsed = true;
                preset.portraitTextureName = key;
            }
        }
        */

        /*
        [Button, PropertyOrder (-1)]
        private void AdjustCapitalization ()
        {
            var blendPresetsArchetypesRenamed = new SortedDictionary<string, DataBlockPilotBlendPresetSplit> ();
            foreach (var kvp in blendPresetsArchetypes)
                blendPresetsArchetypesRenamed.Add (kvp.Key.ToLowerInvariant (), kvp.Value);
            
            blendPresetsArchetypes = blendPresetsArchetypesRenamed;
        }

        [Button, PropertyOrder (-1)]
        private void GenerateBlendPresets ()
        {
            var modelF = models["model_f"];
            var modelM = models["model_m"];

            var keysF = modelF.blendShapeKeys;
            var keysM = modelM.blendShapeKeys;

            var keyFilterFaceType = "PB_FaceType";
            var keyFilterFaceTypeAfrican = "African";
            var keyFilterFaceTypeEastern = "Eastern";
            var keyFilterFaceTypeWestern = "Western";
            var keyFilterFaceTypeGeneric = "Generic";
            
            var keysFaceTypeF = new Dictionary<string, List<string>>
            {
                { keyFilterFaceTypeAfrican, new List<string> () },
                { keyFilterFaceTypeEastern, new List<string> () },
                { keyFilterFaceTypeWestern, new List<string> () },
                { keyFilterFaceTypeGeneric, new List<string> () }
            };
            
            foreach (var keyCandidate in keysF)
            {
                if (keyCandidate.Contains (keyFilterFaceType))
                {
                    if (keyCandidate.Contains (keyFilterFaceTypeAfrican))
                        keysFaceTypeF[keyFilterFaceTypeAfrican].Add (keyCandidate);
                    else if (keyCandidate.Contains (keyFilterFaceTypeEastern))
                        keysFaceTypeF[keyFilterFaceTypeEastern].Add (keyCandidate);
                    else if (keyCandidate.Contains (keyFilterFaceTypeWestern))
                        keysFaceTypeF[keyFilterFaceTypeWestern].Add (keyCandidate);
                    else
                        keysFaceTypeF[keyFilterFaceTypeGeneric].Add (keyCandidate);
                }
            }
            
            var keysFaceTypeM = new Dictionary<string, List<string>>
            {
                { keyFilterFaceTypeAfrican, new List<string> () },
                { keyFilterFaceTypeEastern, new List<string> () },
                { keyFilterFaceTypeWestern, new List<string> () },
                { keyFilterFaceTypeGeneric, new List<string> () }
            };
            
            foreach (var keyCandidate in keysM)
            {
                if (keyCandidate.Contains (keyFilterFaceType))
                {
                    if (keyCandidate.Contains (keyFilterFaceTypeAfrican))
                        keysFaceTypeM[keyFilterFaceTypeAfrican].Add (keyCandidate);
                    else if (keyCandidate.Contains (keyFilterFaceTypeEastern))
                        keysFaceTypeM[keyFilterFaceTypeEastern].Add (keyCandidate);
                    else if (keyCandidate.Contains (keyFilterFaceTypeWestern))
                        keysFaceTypeM[keyFilterFaceTypeWestern].Add (keyCandidate);
                    else
                        keysFaceTypeM[keyFilterFaceTypeGeneric].Add (keyCandidate);
                }
            }

            foreach (var kvp in keysFaceTypeF)
                Debug.Log ($"Face type keys (F, {kvp.Key}) - {kvp.Value.Count}:\n{kvp.Value.ToStringFormatted (true)}");
            
            foreach (var kvp in keysFaceTypeM)
                Debug.Log ($"Face type keys (M, {kvp.Key}) - {kvp.Value.Count}:\n{kvp.Value.ToStringFormatted (true)}");

            blendPresetsArchetypes = new SortedDictionary<string, DataBlockPilotBlendPresetSplit> ();

            foreach (var kvp in keysFaceTypeF)
            {
                var groupKey = kvp.Key;
                var keysInGroupF = kvp.Value;

                if (!keysFaceTypeM.ContainsKey (groupKey))
                {
                    Debug.LogWarning ($"Can't process face type group \"{groupKey}\": equivalent group not found in list M");
                    continue;
                }

                var keysInGroupM = keysFaceTypeM[groupKey];
                int keysInGroupFCount = keysInGroupF.Count;
                int keysInGroupMCount = keysInGroupM.Count;
                int keysLimit = Mathf.Min (keysInGroupFCount, keysInGroupMCount);
                
                if (keysInGroupFCount != keysInGroupMCount)
                    Debug.LogWarning ($"Face type group \"{groupKey}\" will be truncated: group in list M has a different number of entries (F: {keysInGroupFCount}, M: {keysInGroupMCount})");
                
                for (int i = 0, count = keysLimit; i < count; ++i)
                {
                    var keyInGroupF = keysInGroupF[i];
                    var keyInGroupM = keysInGroupM[i];
                    var presetKey = $"{groupKey}_{i:D2}";
                    
                    blendPresetsArchetypes.Add (presetKey, new DataBlockPilotBlendPresetSplit
                    {
                        blendShapesF = new List<DataBlockPilotBlendShapeF>
                        {
                            new DataBlockPilotBlendShapeF
                            {
                                key = keyInGroupF,
                                value = 100
                            }
                        },
                        blendShapesM = new List<DataBlockPilotBlendShapeM>
                        {
                            new DataBlockPilotBlendShapeM
                            {
                                key = keyInGroupM,
                                value = 100
                            }
                        }
                    });
                }
            }
        }
        */
    }
}