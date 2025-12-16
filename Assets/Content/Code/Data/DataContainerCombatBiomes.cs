using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using Area;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace PhantomBrigade.Data
{
    public class DataBlockAreaPropGroup
    {
        [ReadOnly, YamlIgnore, ShowInInspector]
        public string key;
        public Color color = Color.red;
        public float debugHeight;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false, OnBeginListElementGUI = "DrawEntryBeginGUI", OnEndListElementGUI = "DrawEntryEndGUI")]
        [LabelText ("Prop IDs")]
        [ValueDropdown ("GetPropIDs")]
        public List<int> propsIDs = new List<int> ();
        
        #if UNITY_EDITOR
        
        private void DrawEntryEndGUI (int index)
        {
            // GUI.backgroundColor = new Color (1f, 1f, 1f, 1f);
        }

        private void DrawEntryBeginGUI (int index)
        {
            var propID = propsIDs[index];
            
            // GUI.backgroundColor = bgColor;
            
            GUILayout.BeginHorizontal ();
            AreaAssetHelper.CheckResources ();
            var found = AreaAssetHelper.propsPrototypes.TryGetValue (propID, out var prototype);
            if (found && prototype.prefab != null)
            {
                GUILayout.Label (prototype.prefab.name, UnityEditor.EditorStyles.boldLabel);
            }
            
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
            
            // GUI.backgroundColor = bgColorOld;
        }

        private IEnumerable<int> GetPropIDs ()
        {
            AreaAssetHelper.CheckResources ();
            return AreaAssetHelper.propsPrototypes?.Keys;
        }

        #endif
    }
    
    public class DataBlockCombatBiomeLayer
    {
        [HorizontalGroup ("T")]
        [HideReferenceObjectPicker, BoxGroup ("T/AH"), HideLabel]
        public DataBlockResourceTexture albedoSmoothness = new DataBlockResourceTexture ();
        
        [HorizontalGroup ("T")]
        [HideReferenceObjectPicker, BoxGroup ("T/NH"), HideLabel]
        public DataBlockResourceTexture normalHeight = new DataBlockResourceTexture ();
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockCombatBiomeSlot
    {
        [ValueDropdown ("GetLayerKeys")]
        [HideLabel]
        [OnInspectorGUI ("DrawLayerPreview", false)]
        public string layerKey;

        [PropertyTooltip ("X - hue offset, Y - saturation offset (from 0.5), Z - value offset (from 0.5)")]
        [LabelText ("HSV")]
        [OnValueChanged ("ValidateHSV")]
        public Vector4 hsv = new Vector4 (0f, 0.5f, 0.5f, 0f);
        
        [PropertyTooltip ("X - height blending contrast, Y - height offset")]
        [LabelText ("Blending")]
        [OnValueChanged ("ValidateBlending")]
        public Vector2 blending = new Vector3 (0.5f, 0.5f);
        
        #region Editor
        #if UNITY_EDITOR

        private void ValidateHSV () => hsv = new Vector4
        (
            Mathf.Clamp (hsv.x, -1f, 1f),
            Mathf.Clamp01 (hsv.y),
            Mathf.Clamp01 (hsv.z),
            Mathf.Clamp01 (hsv.w)
        );
        
        private void ValidateBlending () => blending = new Vector2
        (
            Mathf.Clamp01 (blending.x),
            Mathf.Clamp01 (blending.y)
        );

        [YamlIgnore]
        private string layerKeyResolvedLast;
        
        [YamlIgnore]
        private Texture2D layerTexture;

        private void DrawLayerPreview ()
        {
            #if !PB_MODSDK
            
            if (layerKeyResolvedLast != layerKey)
            {
                var layerData = DataLinkerCombatBiomes.GetLayerData (layerKey);
                layerTexture = layerData?.albedoSmoothness?.texture;
                layerKeyResolvedLast = layerKey;
            }
            
            if (layerTexture == null)
                return;

            var width = Mathf.Min (GUIHelper.ContextWidth - 88f - 15f, 64f);
            var shrink = width / (float) layerTexture.width;
            var height = layerTexture.height * shrink;
            
            using (var horizontalScope = new GUILayout.HorizontalScope ())
            {
                GUILayout.Space (15f);
                using ( var verticalScope = new GUILayout.VerticalScope ())
                {
                    
                    GUILayout.Label (" ", GUILayout.Width (width * 4), GUILayout.Height (height));
                    var rect = GUILayoutUtility.GetLastRect ();
                    EditorGUI.DrawPreviewTexture (rect, layerTexture, null, ScaleMode.ScaleAndCrop, 1, 0, ColorWriteMask.All);
                }
            }

            #endif
        }

        private IEnumerable<string> GetLayerKeys ()
        {
            return DataLinkerCombatBiomes.data.GetLayerKeys;
        }

        #endif
        #endregion
    }
    
    public class DataBlockCombatBiome
    {
        [BoxGroup ("S1", false), HideLabel]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockCombatBiomeSlot slot1 = new DataBlockCombatBiomeSlot ();
        
        [BoxGroup ("S2", false), HideLabel]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockCombatBiomeSlot slot2 = new DataBlockCombatBiomeSlot ();
        
        [BoxGroup ("S3", false), HideLabel]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockCombatBiomeSlot slot3 = new DataBlockCombatBiomeSlot ();
        
        [BoxGroup ("S4", false), HideLabel]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockCombatBiomeSlot slot4 = new DataBlockCombatBiomeSlot ();

        [BoxGroup ("S", false)]
        [HideReferenceObjectPicker]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockResourceTexture splat = new DataBlockResourceTexture ();
        
        [BoxGroup ("S", false)]
        [HideReferenceObjectPicker]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockResourceTexture distant = new DataBlockResourceTexture ();
        
        [BoxGroup ("S", false)]
        [HideReferenceObjectPicker]
        [OnValueChanged ("ApplyOnChange", true)]
        public DataBlockResourceTexture slope = new DataBlockResourceTexture ();
        
        [BoxGroup ("C", false)]
        [OnValueChanged ("ApplyOnChange")]
        public Vector2 gradientRange = new Vector2 (0f, 30f);

        [BoxGroup ("C")]
        [YamlIgnore, HideReferenceObjectPicker]
        [OnValueChanged ("GenerateGradientTex", true)]
        public Gradient gradient;
        
        [YamlMember (Alias = "gradient"), HideInInspector] 
        public GradientSerialized gradientSerialized;
        
        [BoxGroup ("C")]
        [YamlIgnore, HideReferenceObjectPicker, ReadOnly]
        public Texture2D gradientTexture;

        [YamlIgnore]
        private const int gradientResolution = 64;

        public void GenerateGradientTex ()
        {
            if (gradient == null)
                return;
            
            if (gradientTexture == null)
            {
                gradientTexture = new Texture2D (gradientResolution, 1, TextureFormat.ARGB32, false)
                {
                    //Smooth interpolation
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            Color gradientPixel;
            for (int x = 0; x < gradientResolution; x++)
            {
                gradientPixel = gradient.Evaluate(x / (float)gradientResolution);
                gradientTexture.SetPixel (x, 1, gradientPixel);
            }

            gradientTexture.Apply ();

            #if UNITY_EDITOR
            ApplyOnChange ();
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR

        [Button, PropertyOrder (-1)]
        private void Apply ()
        {

        }
        
        private void ApplyOnChange ()
        {

        }
        
        #endif
        #endregion
    }
    
    [Serializable] 
    public class DataContainerCombatBiomes : DataContainerUnique
    {
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, DataBlockCombatBiomeLayer> layers = new Dictionary<string, DataBlockCombatBiomeLayer> ();
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, DataBlockResourceTexture> texturesSlopes = new Dictionary<string, DataBlockResourceTexture> ();
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, DataBlockResourceTexture> texturesDistant = new Dictionary<string, DataBlockResourceTexture> ();
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, DataBlockResourceTexture> texturesSplats = new Dictionary<string, DataBlockResourceTexture> ();
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockAreaPropGroup> propGroups = new SortedDictionary<string, DataBlockAreaPropGroup> ();
        
        [HideInInspector, YamlIgnore]
        public SortedDictionary<string, DataBlockAreaPropGroup> propGroupsByColor = new SortedDictionary<string, DataBlockAreaPropGroup> ();

        public IEnumerable<string> GetLayerKeys => layers?.Keys;
        public IEnumerable<string> GetTextureSlopeKeys => texturesSlopes?.Keys;
        public IEnumerable<string> GetTextureDistantKeys => texturesDistant?.Keys;
        public IEnumerable<string> GetTextureSplatKeys => texturesSplats?.Keys;
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            if (layers != null)
            {
                foreach (var kvp in layers)
                {
                    var layer = kvp.Value;
                    if (layer == null)
                        continue;
                    
                    if (layer.albedoSmoothness != null)
                        layer.albedoSmoothness.OnBeforeSerialization ();
                    
                    if (layer.normalHeight != null)
                        layer.normalHeight.OnBeforeSerialization ();
                }
            }

            if (texturesSlopes != null)
            {
                foreach (var kvp in texturesSlopes)
                    kvp.Value.OnBeforeSerialization ();
            }
            
            if (texturesDistant != null)
            {
                foreach (var kvp in texturesDistant)
                    kvp.Value.OnBeforeSerialization ();
            }
            
            if (texturesSplats != null)
            {
                foreach (var kvp in texturesSplats)
                    kvp.Value.OnBeforeSerialization ();
            }
        }
        
        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            
            if (layers != null)
            {
                foreach (var kvp in layers)
                {
                    var layer = kvp.Value;
                    if (layer == null)
                        continue;
                    
                    if (layer.albedoSmoothness != null)
                        layer.albedoSmoothness.OnAfterDeserialization ();
                    
                    if (layer.normalHeight != null)
                        layer.normalHeight.OnAfterDeserialization ();
                }
            }

            if (texturesSlopes != null)
            {
                foreach (var kvp in texturesSlopes)
                    kvp.Value.OnAfterDeserialization ();
            }
            
            if (texturesDistant != null)
            {
                foreach (var kvp in texturesDistant)
                    kvp.Value.OnAfterDeserialization ();
            }
            
            if (texturesSplats != null)
            {
                foreach (var kvp in texturesSplats)
                    kvp.Value.OnAfterDeserialization ();
            }
            
            propGroupsByColor.Clear ();
            if (propGroups != null)
            {
                foreach (var kvp in propGroups)
                {
                    kvp.Value.key = kvp.Key;
                    var hex = UtilityColor.ToHexRGB (kvp.Value.color);
                    propGroupsByColor[hex] = kvp.Value;
                }
            }
        }
    }
}

