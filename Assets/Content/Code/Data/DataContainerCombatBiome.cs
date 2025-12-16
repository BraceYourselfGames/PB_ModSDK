using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][LabelWidth (70f)][HideReferenceObjectPicker]
    public class DataBlockResourceTexture
    {
        #if !PB_MODSDK
        [PreviewField (96f, ObjectFieldAlignment.Left)]
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        public Texture2D texture;
        #endif
        
        #if !PB_MODSDK
        [ReadOnly]
        #endif
        public string path;

        public void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR && !PB_MODSDK
            if (texture == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (texture);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        public void OnAfterDeserialization ()
        {
            #if !PB_MODSDK
            texture = !string.IsNullOrEmpty (path) ? Resources.Load<Texture2D> (path) : null;
            if (texture == null)
                Debug.LogWarning ($"Failed to load texture from path {path}");
            #endif
        }
    }

    [LabelWidth (120f)]
    public class DataContainerCombatBiome : DataContainer, IDataContainerTagged
    {
        [ValueDropdown ("GetTagsForDropdown")]
        public HashSet<string> tags = new HashSet<string> { "default" };
        
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
        [OnValueChanged ("ApplyOnChange", true)]
        public Color slopeColor;
        
        [BoxGroup ("S", false)]
        [OnValueChanged ("ApplyOnChange", true)]
        [ValueDropdown("@DataLinkerCombatBiomes.data.GetTextureSlopeKeys")]
        public string textureSlope = "default";
        
        [BoxGroup ("S", false)]
        [OnValueChanged ("ApplyOnChange", true)]
        [ValueDropdown("@DataLinkerCombatBiomes.data.GetTextureDistantKeys")]
        public string textureDistant = "default";
        
        [BoxGroup ("S", false)]
        [OnValueChanged ("ApplyOnChange", true)]
        [ValueDropdown("@DataLinkerCombatBiomes.data.GetTextureSplatKeys")]
        public string textureSplat = "default";
        
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

        [BoxGroup ("V", false)]
        [OnValueChanged ("ApplyOnChange", true)]
        [Tooltip ("RGB - primary tint, A - tint amount")]
        [LabelText ("Control Color A")]
        public Color vegetationColorPrimary = new Color (1f, 1f, 1f, 1f);
        
        [BoxGroup ("V", false)]
        [OnValueChanged ("ApplyOnChange", true)]
        [Tooltip ("RGB - secondary tint, A - foliage amount")]
        [LabelText ("Control Color B")]
        public Color vegetationColorSecondary = new Color (1f, 1f, 1f, 1f);
        
        [BoxGroup ("W", false)]
        [OnValueChanged ("ApplyOnChange", true)]
        [PropertyRange (0f, 1f)]
        [LabelText ("Snow")]
        public float weatherSnowAmount = 0f;

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
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();

            gradientSerialized = (GradientSerialized)gradient;
        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            gradient = (Gradient)gradientSerialized;
            GenerateGradientTex ();
        }
        
        public HashSet<string> GetTags (bool processed) => 
            tags;
        
        public bool IsHidden () => false;
        
        #region Editor
        #if UNITY_EDITOR

        [Button, PropertyOrder (-1)]
        private void Apply ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.materialHelper == null)
                return;

            var m = sceneHelper.materialHelper;
            m.ApplyBiome (this);
        }
        
        private void ApplyOnChange ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.materialHelper == null)
                return;

            var m = sceneHelper.materialHelper;
            m.ApplyBiomeIfLast (this);
        }
        
        private IEnumerable<string> GetTagsForDropdown =>
            DataMultiLinkerCombatBiome.tags;
        
        #endif
        #endregion
    }
}

