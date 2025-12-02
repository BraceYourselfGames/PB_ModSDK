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

    [HideReferenceObjectPicker]
    public class DataBlockPilotSuitVariant
    {
        public Vector3 hsvPrimary = new Vector3(0.0f, 0.5f, 0.5f);
        public bool generated = true;

        public Color backgroundFog = new Color (0.0f, 0.0f, 0.0f, 1.0f);

        public Color reflectionTint = new Color (0.0f, 0.0f, 0.0f, 1.0f);
    }

    [HideReferenceObjectPicker]
    public class DataBlockpilotMakeupPreset
    {
        [HorizontalGroup ("makeupColors", Title = "Colors")]
        [HideLabel, ColorUsage (showAlpha: true)]
        public Color colorPrimary = new Color (1.0f, 1.0f, 1.0f, 1.0f);

        [HorizontalGroup ("makeupColors")]
        [HideLabel, ColorUsage (showAlpha: true)]
        public Color colorSecondary = new Color (1.0f, 1.0f, 1.0f, 1.0f);

        [HorizontalGroup ("makeupMisc", Title = "Misc")]
        public bool generated = true;
        
        [HorizontalGroup ("makeupMisc")]
        public bool selectable = true;
                        
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
    
    public class DataContainerResourceAssetPilot<T> : DataContainerResourceAsset<T> where T : UnityEngine.Object
    {
        public int priority;
        public bool usableByFriendly = true;
        public bool usableByHostile = true;
        
        [PropertyRange (0f, 1f)]
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
            
            #if UNITY_EDITOR && !PB_MODSDK
            if (asset == null)
            {
                path = string.Empty;
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
            
            #if !PB_MODSDK
            asset = !string.IsNullOrEmpty (path) ? Resources.Load<GameObject> (path) : null;
            if (asset == null)
            {
                Debug.LogWarning ($"Failed to load asset GameObject from path [{path}]");
                return;
            }
            #endif
        }
        
        #if UNITY_EDITOR

        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);

        #endif
    }

    public class DataContainerResourceAsset<T> : DataContainer where T : UnityEngine.Object // Changed from ScriptableObject to generic Object to support more asset types
    {
        #if !PB_MODSDK
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        // [GUIColor ("GetPrefabColor")]
        public T asset;
        #endif
        
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;
        
        public override void OnBeforeSerialization ()
        {
            #if !PB_MODSDK
            base.OnBeforeSerialization ();
        
            #if UNITY_EDITOR
            if (asset == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (asset);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
            #endif
        }

        public override void OnAfterDeserialization (string key)
        {
            #if !PB_MODSDK
            base.OnAfterDeserialization (key);
            
            asset = !string.IsNullOrEmpty (path) ? Resources.Load<T> (path) : null;
            if (asset == null)
            {
                Debug.LogWarning ($"Failed to load asset {typeof (T).Name} from path [{path}]");
                return;
            }
            #endif
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

    // Struct to contain references to a mesh blendshape with user defined min and max influence range, used to populate the list of character customization options
    public struct DataBlockPilotBlendShapeClamped
    {
        [ValueDropdown ("GetBlendShapeKeys")]
        public string blendShape;

        [PropertyRange (-100, 0), OnValueChanged ("SnapInfluenceMin")]
        public int influenceMin;
        
        [PropertyRange (0, 100), OnValueChanged ("SnapInfluenceMax")]
        public int influenceMax;

        private void SnapInfluenceMin () => influenceMin = Mathf.RoundToInt (influenceMin / 10f) * 10;
        private void SnapInfluenceMax () => influenceMax = Mathf.RoundToInt (influenceMax / 10f) * 10;

        private IEnumerable<string> GetBlendShapeKeys ()
        {
            var db = DataLinkerSettingsPilot.data;
            DataContainerPilotModel modelLink = null;
            bool modelFound = !string.IsNullOrEmpty ("model_f") && db.models.TryGetValue ("model_f", out modelLink);
            return modelFound && modelLink != null && modelLink.blendShapeKeys != null ? modelLink.blendShapeKeys : null;
        }
    }

    // Class to contain a key-influence pair for a customization option, used to populate a list of customizations to apply in the pilot appearance data block
    [HideReferenceObjectPicker]
    public class DataBlockPilotBlendAppearanceCustomization
    {
        // References a key from the list of possible customization options
        [ValueDropdown ("@DataLinkerSettingsPilot.data?.blendshapesForCustomization.Keys")]
        public string key;
        
        [PropertyRange ("@InfluenceRange (false)", "@InfluenceRange (true)")]
        public int influence;
        
        #if UNITY_EDITOR
        
        private int InfluenceRange (bool clampMaxInfluence)
        {
            int outInfluence = clampMaxInfluence ? 100 : -100;            

            if (key != null)
            {
                bool blendshapeClampedFound = DataLinkerSettingsPilot.data.blendshapesForCustomization.TryGetValue (key, out var blendshapeClamped);
                if (blendshapeClampedFound)
                    outInfluence = clampMaxInfluence ? blendshapeClamped.influenceMax : blendshapeClamped.influenceMin;      
            }

            return outInfluence;
        }

        #endif
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
        
        [YamlIgnore]
        public int persistentIDLinked = IDUtility.invalidID;
        
        [HideInInspector]
        public int version = 0;
        
        [ValueDropdown ("GetModelKeys")]
        public string model;
        
        [ValueDropdown ("GetSkinMainKeys")]
        public string skinMain;

        [Range (-0.01f, 0.015f)]
        public float heightOffset = 0.0f;
        
        [ValueDropdown ("GetSkinOverlayKeys")]
        public string skinOverlay;

        [ValueDropdown ("GetPilotSuitColorVariantKeys")]
        public string pilotSuitColorVariant;
        
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
        
        [ValueDropdown ("GetEyesIrisPresetsKeys")]
        public string eyesIrisPreset;

        [ValueDropdown ("GetLipsTintKeys")]
        public string lipsTintPreset;

        [ValueDropdown ("GetMakeupPatternsKeys")]
        public string makeupPatternKey;

        [ValueDropdown ("GetMakeupColorVariantsKeys")]
        [ShowIf ("@!string.IsNullOrEmpty (makeupPatternKey)")]
        public string makeupColorVariantKey;

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
        
        [ValueDropdown ("GetBlendshapesFacePresetsKeys")]
        [InlineButtonClear]
        public string blendPresetArchetype;

        [DictionaryKeyDropdown("@DataLinkerSettingsPilot.data.blendshapesForCustomization.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = 200f)]
        public Dictionary<string, int> blendAppearanceCustomization = new Dictionary<string, int> ();

        [InlineButtonClear]
        [ShowIf ("@blendShapes != null")]
        [ListDrawerSettings (DefaultExpandedState = false, CustomAddFunction = "@new DataBlockPilotBlendShape ()")]
        public List<DataBlockPilotBlendShape> blendShapes;

        [InlineButtonClear]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.PilotPortraits)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.PilotPortraits, 128)", false)]
        public string portrait;
        
        [InlineButtonClear]
        [ValueDropdown ("@DataShortcuts.pilots.overlayVariants?.Keys")]
        public string portraitVariant;

        [InlineButtonClear]
        [ValueDropdown ("GetCameraAnglePresetKeys")]
        public string cameraAnglePreset;
        
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

            heightOffset = source.heightOffset;
 
            skinMain = source.skinMain;
            skinOverlay = source.skinOverlay;
            skinTintPreset = source.skinTintPreset;
            eyesTintPreset = source.eyesTintPreset;
            eyesIrisPreset = source.eyesIrisPreset;

            lipsTintPreset = source.lipsTintPreset;
            makeupPatternKey = source.makeupPatternKey;
            makeupColorVariantKey = source.makeupColorVariantKey;

            pilotSuitColorVariant = source.pilotSuitColorVariant;
            
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

            portrait = source.portrait;
            portraitVariant = source.portraitVariant;

            cameraAnglePreset = source.cameraAnglePreset;

            if (blendAppearanceCustomization != null)
                blendAppearanceCustomization.Clear ();

            if (source.blendAppearanceCustomization != null && source.blendAppearanceCustomization.Count > 0)
            {
                foreach (var kv in source.blendAppearanceCustomization)
                {
                    // Don't copy customization parameters that have 0 influence
                    if (kv.Value != 0)
                    {
                        if (blendAppearanceCustomization == null)
                            blendAppearanceCustomization = new Dictionary<string, int> ();
                        blendAppearanceCustomization[kv.Key] = kv.Value;
                    }
                }
            }

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
        private IEnumerable<string> GetEyesIrisPresetsKeys => DataLinkerSettingsPilot.data?.eyeIrisPresets.Keys;
        private IEnumerable<string> GetLipsTintKeys => DataLinkerSettingsPilot.data?.lipsTints.Keys;

        private IEnumerable<string> GetMakeupPatternsKeys => DataLinkerSettingsPilot.data?.makeupPatterns.Keys;

        private IEnumerable<string> GetMakeupColorVariantsKeys
        {
            get
            {
                #if !PB_MODSDK
                if (!string.IsNullOrEmpty(makeupPatternKey) && DataLinkerSettingsPilot.data.makeupPatterns.ContainsKey(makeupPatternKey))
                {
                    bool assetFound = DataLinkerSettingsPilot.data.makeupPatterns.TryGetValue (makeupPatternKey, out var assetLink);
                    if (assetFound && assetLink.asset != null)
                    {
                        return DataLinkerSettingsPilot.data.makeupColors[assetLink.asset.MakeupColorsGroupKey].Keys;
                    }
                }
                #endif
                
                return Array.Empty<string> ();
            }
        }

        private IEnumerable<string> GetPilotSuitColorVariantKeys => DataLinkerSettingsPilot.data?.pilotSuitColorVariants.Keys;

        private IEnumerable<string> GetCameraAnglePresetKeys => DataLinkerSettingsPilot.data?.cameraAnglePresets.Keys;

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
        
        private IEnumerable<string> GetBlendshapesFacePresetsKeys => DataLinkerSettingsPilot.data?.blendshapesFacePresets.Keys;
        private IEnumerable<string> GetBlendshapesCustomizationKeys => DataLinkerSettingsPilot.data?.blendshapesForCustomization.Keys;

        #endif
    }

    public class DataContainerPilotModel : DataContainer
    {
        public bool enabled;
        public string viewKey;
        
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> blendShapeKeys;

        // Editor-only utility to refresh the Blend Shape Keys list from the specified mesh
        #if UNITY_EDITOR
        [Button ("Refresh Blendshape Keys")]
        private void RefreshListOfBlendshapeNames (SkinnedMeshRenderer characterMesh)
        {
            if (characterMesh)
            {
                if (blendShapeKeys == null)
                    blendShapeKeys = new List<string> ();
                else
                    blendShapeKeys.Clear ();

                for (int i = 0; i < characterMesh.sharedMesh.blendShapeCount; i++)
                {
                    string blendshapeName = characterMesh.sharedMesh.GetBlendShapeName (i);
                    // Filter out any blendshapes that aren't related to customization
                    if (blendshapeName.StartsWith("PB_C_"))
                        blendShapeKeys.Add (blendshapeName);
                }

                blendShapeKeys.Sort ();
            }
        }
        #endif
    }

    [HideReferenceObjectPicker]
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
    public class DataContainerPilotEyeIrisPreset : DataContainer
    {
        [HideLabel, HorizontalGroup]
        public float irisSize = 0.25f;
        
        [HideLabel, HorizontalGroup]
        public float pupilDilation = -0.05f;
        
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

    [HideReferenceObjectPicker]
    public class DataContainerPilotTintLips : DataContainer
    {
        [HideLabel, HorizontalGroup]
        [ColorUsage (showAlpha: true)]
        public Color color;
        
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

    public class DataBlockCameraAnglePreset
    {
        public class DataBlockCameraAngleTweakPerPersonality
        {
            [ValueDropdown ("@DataMultiLinkerPilotPersonality.data.Keys")]
            public string personalityKey;

            public Vector3 additionalOffset = new Vector3 (0f, 0f, 0f);
        }

        public Vector3 angle = new Vector3 (0f, 0f, 0f);
        public Vector3 offset = new Vector3 (0f, 0f, 0f);
        public float fov = 35f;
        public float dolly = 0f;
        public bool enemiesOnly = false;

        // A list of optional additional offsets for Camera Center per personality type
        public List <DataBlockCameraAngleTweakPerPersonality> additionalOffsetPerPersonality = new List <DataBlockCameraAngleTweakPerPersonality> ();
    }


    [Serializable]
    public class DataContainerSettingsPilot : DataContainerUnique
    {
        private const string tgValues = "Values";
        private const string tgPresets = "Presets";
        private const string tgShapes = "Shapes";
        private const string tgAssets = "Assets";
        private const string tgColors = "Colors";
        private const string tgProgression = "Progression";
        
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
        public float facialHairChance = 0.15f;

        [TabGroup (tgValues)]
        public float makeupChance = 0.25f;
                
        [TabGroup (tgValues)]
        public float blendPresetBuildChance = 0.3f;

        [TabGroup (tgValues)]
        public float blendPresetFaceVariationEyesChance = 0.5f;

        [TabGroup (tgValues)]
        public float blendPresetFaceVariationNoseChance = 0.5f;

        [TabGroup (tgValues)]
        public float blendPresetFaceVariationCheeksChance = 0.5f;

         [TabGroup (tgValues)]
        public float blendPresetFaceVariationLipsChance = 0.5f;
        
        [TabGroup (tgValues)]
        public float blendPresetFaceVariationJawChance = 0.5f;

        [TabGroup (tgValues)]
        public float blendPresetFaceVariationEarsChance = 0.5f;

        [TabGroup (tgValues)]
        public bool overlaySupport = false;

        [TabGroup (tgValues)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine)]
        public SortedDictionary<string, DataBlockCameraAnglePreset> cameraAnglePresets;
        
        [InfoBox ("This list stores blendshapes of face presets that serve as a foundation that can be further customized.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockPilotBlendPresetUnified> blendshapesFacePresets;

        [Space(15)]
        [InfoBox ("This list stores blendshape names used for body and face customization. Min and max values limit the range of blendshape's influence on the character mesh.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockPilotBlendShapeClamped> blendshapesForCustomization;
                
        [Space(25)]
        [InfoBox ("[Internal use only] - Eyes customization presets for random appearance generator.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsFaceVariationEyes;

        [Space(15)]
        [InfoBox ("[Internal use only] - Nose customization presets for random appearance generator.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsFaceVariationNose;

        [Space(15)]
        [InfoBox ("[Internal use only] - Cheeks customization presets for random appearance generator.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsFaceVariationCheeks;

        [Space(15)]
        [InfoBox ("[Internal use only] - Lips customization presets for random appearance generator.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsFaceVariationLips;

        [Space(15)]
        [InfoBox ("[Internal use only] - Jaw and chin customization presets for random appearance generator.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsFaceVariationJaw;

        [Space(15)]
        [InfoBox ("[Internal use only] - Ears customization presets for random appearance generator.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsFaceVariationEars;

        [Space(15)]
        [InfoBox ("[Internal use only] - Body shape presets for random appearance generator - musculature, weight, etc. Current content uses key scheme W*_M* where W is amount of weight and M is amount of muscle.")]
        [TabGroup (tgShapes)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> blendPresetsBuilds;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotModel> models;
        
        [YamlIgnore, HideInInspector]
        public List<DataContainerPilotModel> modelsEnabled = new List<DataContainerPilotModel> ();
    
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceAssetPilot<CharacterHairData>> hairMain;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceAssetPilot<CharacterHairData>> hairFacial;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotAccessoryHeadTop> accessoriesHeadTop;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerPilotAccessoryHeadFront> accessoriesHeadFront;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceAssetPilot<CharacterSkinTexturesData>> skinsMain;
        
        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceAssetPilot<CharacterBaseClothTexturesData>> skinsOverlays;

        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine)]
        [InfoBox ("Background Fog parameter is placed together with suit color variants on purpose, to 'ground' suit color in pilot portrait environment and increase perceivable variety")]
        public SortedDictionary<string, DataBlockPilotSuitVariant> pilotSuitColorVariants;

        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine)]
        public SortedDictionary<string, DataContainerPilotEyebrows> hairEyebrows;

        [TabGroup (tgAssets)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataContainerResourceAssetPilot<CharacterMakeupPatternData>> makeupPatterns;

        [TabGroup (tgColors)]
        [InfoBox ("These colors are multiplied by skin textures, check the effect of the multiplication with skin textures before configuring them. A specific color entered here will not yield a final skin color that is anywhere near.")]
        [LabelText ("Skin color presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine, KeyColumnWidth = 32, ValueLabel = "Color, Redness, Darkening, Axis (X/Y), Generated, Selectable")]
        public SortedDictionary<string, DataContainerPilotTintSkin> skinTints;

        [TabGroup (tgColors)]
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
        
        [TabGroup (tgColors)]
        [LabelText ("Eye iris presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine, KeyColumnWidth = 32, ValueLabel = "Iris Size, Pupil Dilation, Generated, Selectable")]
        public SortedDictionary<string, DataContainerPilotEyeIrisPreset> eyeIrisPresets;

        [TabGroup (tgColors)]
        [LabelText ("Lips color presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine, KeyColumnWidth = 32, ValueLabel = "Color, Axis (X), Generated, Selectable")]
        public SortedDictionary<string, DataContainerPilotTintLips> lipsTints;

        [TabGroup (tgColors)]
        [LabelText ("Makeup color presets")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.OneLine)]
        public SortedDictionary<string, SortedDictionary<string, DataBlockpilotMakeupPreset>> makeupColors;

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

            if (makeupPatterns != null)
            {
                foreach (var kvp in makeupPatterns)
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
            
            if (eyeIrisPresets != null)
            {
                foreach (var kvp in eyeIrisPresets)
                {
                    var link = kvp.Value;
                    if (link != null)
                        link.OnAfterDeserialization (kvp.Key);
                }
            }

            if (lipsTints != null)
            {
                foreach (var kvp in lipsTints)
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
            (int factionFilter, string modelFilter, IDictionary<string, DataContainerResourceAssetPilot<T>> collection, bool includeEmpty = false, float interpolantFilter = -1f) where T : UnityEngine.Object
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
        
        #if !PB_MODSDK
        
        public DataContainerResourceAssetPilot<T> GetRandomResource<T> 
            (int factionFilter, string modelFilter, float interpolantFilter, IDictionary<string, DataContainerResourceAssetPilot<T>> collection) where T : UnityEngine.Object // Changed from ScriptableObject to generic Object to support more asset types
        {
            var filteredKeysFound = GetFilteredResourceKeys (factionFilter, modelFilter, collection, false, interpolantFilter);
            if (filteredKeysFound.Count == 0)
                return null;

            var selectedKey = filteredKeysFound.GetRandomEntry ();
            var selection = collection[selectedKey];
            return selection;
        }
        
        public string GetShiftedResourceKey<T>
            (string keyCurrent, bool forward, bool insertEmpty, int factionFilter, string modelFilter, float interpolantFilter, IDictionary<string, DataContainerResourceAssetPilot<T>> collection) where T : UnityEngine.Object // Changed from ScriptableObject to generic Object to support more asset types
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

        // TODO: Can refactor these two methods below and unify them
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
        
        public DataContainerPilotTintLips GetTintPresetLipsFromInterpolant (float interpolant)
        {
            interpolant = Mathf.Clamp01 (interpolant);
            float distanceBest = 1f;
            DataContainerPilotTintLips presetBest = null;
            
            foreach (var kvp in lipsTints)
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

        public string GetRandomSuitTintPresetKey ()
        {
            string keyBest;
            List<string> keyCandidates = new List<string> ();
            
            foreach (var kvp in pilotSuitColorVariants)
            {
                var presetCandidate = kvp.Value;
                if (presetCandidate == null || !presetCandidate.generated)
                    continue;

                keyCandidates.Add (kvp.Key);
            }

            keyBest = keyCandidates.GetRandomEntry ();

            return keyBest;
        }

        public string GetRandomCameraAnglePresetKey ()
        {
            string keyBest;
            List<string> keyCandidates = new List<string> ();
            
            foreach (var kvp in cameraAnglePresets)
            {
                var presetCandidate = kvp.Value;
                if (presetCandidate == null || presetCandidate.enemiesOnly)
                    continue;

                keyCandidates.Add (kvp.Key);
            }

            keyBest = keyCandidates.GetRandomEntry ();

            return keyBest;
        }

         public string GetRandomMakeupColorPresetVariantKey (string makeupColorsGroupKey)
        {
            string keyBest;
            List<string> keyCandidates = new List<string> ();

            if (!string.IsNullOrEmpty (makeupColorsGroupKey))
            {
                if (makeupColors.Count > 0 && makeupColors.ContainsKey(makeupColorsGroupKey) && makeupColors[makeupColorsGroupKey]?.Count > 0)
                {
                    // Build a list of keys that satisfy the criteria (available for random generation)
                    foreach (var kvp in makeupColors[makeupColorsGroupKey])
                    {
                        var presetCandidate = kvp.Value;
                        if (presetCandidate == null || !presetCandidate.generated)
                            continue;

                        keyCandidates.Add (kvp.Key);
                    }
                }
            }

            // Choose a random makeup color preset key from the list we've made
            keyBest = keyCandidates.GetRandomEntry ();
            return keyBest;
        }
        
        public List<string> GetFilteredKeys<T> (IDictionary<string, T> dictionary, Func<T, bool> onFilter = null)
        {
            if (dictionary == null || dictionary.Count == 0)
                return null;
            
            filteredKeys.Clear ();
            if (onFilter != null)
            {
                foreach (var kvp in dictionary)
                {
                    if (onFilter.Invoke (kvp.Value))
                        filteredKeys.Add (kvp.Key);
                }
            }
            else
                filteredKeys.AddRange (dictionary.Keys);
            return filteredKeys;
        }
        
        public string GetShiftedKey<T> (string keyCurrent, bool forward, bool insertEmpty, IDictionary<string, T> dictionary, Func<T, bool> onFilter = null)
        {
            if (dictionary == null || dictionary.Count == 0)
                return null;
            
            bool keyCurrentEmpty = string.IsNullOrEmpty (keyCurrent);
            var keysFilteredLocal = GetFilteredKeys (dictionary, onFilter);

            var indexCurrent = keysFilteredLocal.IndexOf (keyCurrent);
            if (indexCurrent == -1 && keysFilteredLocal.Count > 0)
            {
                // If forward, empty is inserted, and current key is also empty, do not advance to empty position, go to first filled one
                if (forward)
                    return insertEmpty ? keysFilteredLocal[1] : keysFilteredLocal[0];
                else
                    return keysFilteredLocal[keysFilteredLocal.Count - 1];
            }

            var indexShifted = indexCurrent.OffsetAndWrap (forward, keysFilteredLocal);
            var keyShifted = keysFilteredLocal[indexShifted];
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
        
        public DataContainerResourceAssetPilot<CharacterHairData> GetRandomAssetHairMain (bool friendly, string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, hairMain);
        
        public DataContainerResourceAssetPilot<CharacterHairData> GetRandomAssetHairFacial (bool friendly, string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, hairFacial);
        
        public DataContainerResourceAssetPilot<CharacterSkinTexturesData> GetRandomAssetSkinMain (string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (0, modelFilter, interpolantFilter, skinsMain);
            
        public DataContainerResourceAssetPilot<CharacterBaseClothTexturesData> GetRandomAssetSkinOverlay (bool friendly, string modelFilter, float interpolantFilter = -1f) => 
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, skinsOverlays);
        
        public DataContainerPilotAccessoryHeadTop GetRandomAccessoryHeadTop (bool friendly) => 
            GetRandomPrefab (friendly ? 1 : 2, accessoriesHeadTop);
        
        public DataContainerPilotAccessoryHeadFront GetRandomAccessoryHeadFront (bool friendly) => 
            GetRandomPrefab (friendly ? 1 : 2, accessoriesHeadFront);

        public DataContainerResourceAssetPilot<CharacterMakeupPatternData> GetRandomMakeupPattern (bool friendly, string modelFilter, float interpolantFilter = -1f) =>
            GetRandomResource (friendly ? 1 : 2, modelFilter, interpolantFilter, makeupPatterns);
        
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

            data.heightOffset = Random.Range (-0.01f, 0.015f);
            
            var faceKey = blendshapesFacePresets.GetRandomKey ();
            bool faceKeyBasedGeneration = false;
            float hairInterpolant = 0.5f;
            
            string skinMainKey = null;
            string skinTintPreset = null;
            string hairMainTintPreset = null;
            string hairFacialTintPreset = null;
            string eyesIrisPreset = null;
            string eyesTintPreset = null;
            string lipsTintPreset = null;

            if (!string.IsNullOrEmpty (faceKey))
            {
                if (faceKey.StartsWith ("a_"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_a_alt" : "skin_a";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0.6f, 1f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0.7f, 1f))?.key;
                    hairInterpolant = 1f;
                }
                else if (faceKey.StartsWith ("e_"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_b_alt" : "skin_b";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0.15f, 0.4f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0.6f, 1f))?.key;
                    hairInterpolant = 0f;
                }
                else if (faceKey.StartsWith ("w_"))
                {
                    faceKeyBasedGeneration = true;
                    skinMainKey = sharedRandomPercentage > 0.5f ? "skin_b_alt" : "skin_b";
                    skinTintPreset = GetTintPresetSkinFromInterpolant (Random.Range (0f, 0.6f))?.key;
                    hairMainTintPreset = hairFacialTintPreset = GetTintPresetHairFromInterpolant (Random.Range (0f, 0.6f))?.key;
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

            // Apply random face and body appearance settings from a preset
            data.blendAppearanceCustomization.Clear ();
            string presetKey = null;
            bool buildUsed = Random.Range (0f, 1f) < config.blendPresetBuildChance;
            if (buildUsed && config.blendPresetsBuilds != null && config.blendPresetsBuilds.Count > 0)
            {
                presetKey = config.blendPresetsBuilds.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsBuilds);
            }
            bool faceVariationJawUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationJawChance;
            if (faceVariationJawUsed && config.blendPresetsFaceVariationJaw != null && config.blendPresetsFaceVariationJaw.Count > 0)
            {
                presetKey = config.blendPresetsFaceVariationJaw.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsFaceVariationJaw);
            }
            bool faceVariationNoseUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationNoseChance;
            if (faceVariationNoseUsed && config.blendPresetsFaceVariationNose != null && config.blendPresetsFaceVariationNose.Count > 0)
            {
                presetKey = config.blendPresetsFaceVariationNose.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsFaceVariationNose);
            }
            bool faceVariationEyesUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationEyesChance;
            if (faceVariationEyesUsed && config.blendPresetsFaceVariationEyes != null && config.blendPresetsFaceVariationEyes.Count > 0)
            {
                presetKey = config.blendPresetsFaceVariationEyes.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsFaceVariationEyes);
            }
            bool faceVariationCheeksUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationCheeksChance;
            if (faceVariationCheeksUsed && config.blendPresetsFaceVariationCheeks != null && config.blendPresetsFaceVariationCheeks.Count > 0)
            {
                presetKey = config.blendPresetsFaceVariationCheeks.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsFaceVariationCheeks);
            }
            bool faceVariationLipsUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationLipsChance;
            if (faceVariationLipsUsed && config.blendPresetsFaceVariationLips != null && config.blendPresetsFaceVariationLips.Count > 0)
            {
                presetKey = config.blendPresetsFaceVariationLips.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsFaceVariationLips);
            }
            bool faceVariationEarsUsed = Random.Range (0f, 1f) < config.blendPresetFaceVariationEarsChance;
            if (faceVariationEarsUsed && config.blendPresetsFaceVariationEars != null && config.blendPresetsFaceVariationEars.Count > 0)
            {
                presetKey = config.blendPresetsFaceVariationEars.GetRandomKey ();
                ApplyAppearanceCustomizationFromPreset (ref data, presetKey, ref config.blendPresetsFaceVariationEars);
            }

            data.blendPresetArchetype = faceKey;
            
            eyesIrisPreset = eyeIrisPresets != null ? eyeIrisPresets.GetRandomKeyChecked ((x) => x != null && x.generated) : "01";
            data.eyesIrisPreset = eyesIrisPreset;

            eyesTintPreset = GetTintPresetEyesFromInterpolant (Random.Range (0f, 1f))?.key;
            data.eyesTintPreset = eyesTintPreset;

            lipsTintPreset = GetTintPresetLipsFromInterpolant (Random.Range (0f, 1f))?.key;
            data.lipsTintPreset = lipsTintPreset;

            data.skinMain = skinMainKey;
            data.skinTintPreset = skinTintPreset;
            data.hairMainTintPreset = hairMainTintPreset;
            data.hairFacialTintPreset = hairFacialTintPreset;

            if (config.pilotSuitColorVariants != null && config.pilotSuitColorVariants.Count > 0)
            {
                string pilotSuitColorVariant = GetRandomSuitTintPresetKey ();
                data.pilotSuitColorVariant = pilotSuitColorVariant;
            }

            if (config.cameraAnglePresets != null && config.cameraAnglePresets.Count > 0)
            {
                string cameraAnglePreset = GetRandomCameraAnglePresetKey ();
                data.cameraAnglePreset = cameraAnglePreset;
            }
            
            var hairMain = GetRandomAssetHairMain (friendly, modelKey, hairInterpolant);
            var hairMainKey = hairMain?.key;
            data.hairMain = hairMainKey;
            
            bool hairFacialUsed = Random.Range (0f, 1f) < facialHairChance;
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

            // Commented out, but left in the code (if we decide to bring overlays on random generation back)
            //var portraitKeys = TextureManager.GetExposedTextureKeys (TextureGroupKeys.PilotPortraits);
            //data.portrait = portraitKeys.GetRandomEntry ();
            //data.portraitVariant = overlayVariants.GetRandomKey ();

            bool makeuplUsed = Random.Range (0f, 1f) < makeupChance;
            if (!makeuplUsed)
            {
                data.makeupPatternKey = null;
                data.makeupColorVariantKey = null;
            }
            else
            {
                var pilotMakeupPatternEntry = GetRandomMakeupPattern (friendly, modelKey, 0.5f);
                var pilotMakeupEntryPatternKey = pilotMakeupPatternEntry?.key;
                var makeupColorsGroupStoredKey = pilotMakeupPatternEntry?.asset?.MakeupColorsGroupKey;
                var pilotMakeupColorVariantKey = GetRandomMakeupColorPresetVariantKey (makeupColorsGroupStoredKey);

                data.makeupPatternKey = pilotMakeupEntryPatternKey;
                data.makeupColorVariantKey = pilotMakeupColorVariantKey;
            }
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

        private void ApplyAppearanceCustomizationFromPreset (ref DataBlockPilotAppearance data, string key, ref SortedDictionary<string, List<DataBlockPilotBlendAppearanceCustomization>> dict)
        {
            if (data != null)
            {
                bool customizationPresetFound = dict.TryGetValue (key, out var blendAppearanceCustomizationPresets);
                if (customizationPresetFound)
                {
                    if (blendAppearanceCustomizationPresets != null && blendAppearanceCustomizationPresets.Count > 0)
                    {
                        foreach (var entry in blendAppearanceCustomizationPresets)
                        {
                            if (!string.IsNullOrEmpty (entry.key))
                                data.blendAppearanceCustomization[entry.key] = entry.influence;
                        }
                    }
                }
            }
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
        #endif
    }
}