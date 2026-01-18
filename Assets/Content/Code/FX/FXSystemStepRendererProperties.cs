using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepRendererProperty
    {
        [OnValueChanged ("UpdatePropertyID")][HorizontalGroup]
        public string propertyName;
        protected string propertyIDNameSource;
        
        [ShowInInspector][ReadOnly][HorizontalGroup (80f)]
        protected int propertyID;
        protected bool valueUpdated = false;
        
        [HideReferenceObjectPicker][HideLabel][HideIf ("linear")]
        public AnimationCurveContainer curveContainer = new AnimationCurveContainer ();

        [LabelWidth(2f)][LabelText("")][MinMaxSlider (0f, 1f)][ShowIf ("linear")]
        public Vector2 linearRange = new Vector2 (0f, 1f);
        
        public bool linear;
        protected bool propertyValid = false;
        
        // public AnimationCurve curve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (1f, 1f));

        public void Initialize ()
        {
            UpdatePropertyID ();
        }

        protected void UpdatePropertyID ()
        {
            propertyValid = !string.IsNullOrEmpty (propertyName);
            propertyID = propertyValid ? Shader.PropertyToID (propertyName) : -1;
        }
        
        
        
        
        #if UNITY_EDITOR

        [NonSerialized]
        public StepRenderer parent;
        
        [ShowInInspector][PropertyOrder (-1)][LabelText (" ")]
        [ValueDropdown ("GetPropertyNames")][InlineButton ("ApplyDropdownName", "Apply")]
        [NonSerialized]
        protected string propertyNameDropdown = string.Empty;

        [NonSerialized]
        protected static Shader shaderLast = null;

        [NonSerialized]
        protected static List<string> shaderPropertyFloatNames = new List<string> ();
        
        [NonSerialized]
        protected static List<string> shaderPropertyVectorNames = new List<string> ();
        
        [NonSerialized]
        protected static List<string> shaderPropertyColorNames = new List<string> ();

        protected void ApplyDropdownName ()
        {
            propertyName = propertyNameDropdown;
            UpdatePropertyID ();
        }
        
        protected virtual List<string> GetPropertyNames ()
        {
            return null;
        }

        protected virtual void CheckPropertyCache ()
        {
            if (!IsSettingSafe ())
                return;
            
            var sh = parent.renderers[0].sharedMaterial.shader;
            if (shaderLast != null && sh == shaderLast)
                return;

            shaderLast = sh;
            var propertyCount = ShaderUtil.GetPropertyCount (sh);
            shaderPropertyFloatNames.Clear ();
            shaderPropertyVectorNames.Clear ();
            shaderPropertyColorNames.Clear ();
            
            for (int i = 0; i < propertyCount; ++i)
            {
                var propertyType = ShaderUtil.GetPropertyType (sh, i);
                if (propertyType == ShaderUtil.ShaderPropertyType.Float || propertyType == ShaderUtil.ShaderPropertyType.Range)
                    shaderPropertyFloatNames.Add (ShaderUtil.GetPropertyName (sh, i));
                else if (propertyType == ShaderUtil.ShaderPropertyType.Vector)
                    shaderPropertyVectorNames.Add (ShaderUtil.GetPropertyName (sh, i));
                else if (propertyType == ShaderUtil.ShaderPropertyType.Color)
                    shaderPropertyColorNames.Add (ShaderUtil.GetPropertyName (sh, i));
            }
        }
        
        protected bool IsSettingSafe ()
        {
            return !(parent == null || parent.renderers == null || parent.renderers.Length == 0 || parent.renderers[0] == null);
        }
        
        #endif
    }

    
    
    
    [Serializable]
    public class StepRendererPropertyFloat : StepRendererProperty
    {
        [InlineButton ("SetFrom", "◄")]
        public float from;
        [InlineButton ("SetTo", "◄")]
        public float to;
        
        [NonSerialized][ShowInInspector][ReadOnly]
        private float valueLast;

        public void UpdateAndApply (MaterialPropertyBlock mpb, float timeNormalized)
        {
            if (!propertyValid)
                return;
            
            var sample = linear ? timeNormalized.RemapTo01 (linearRange) : curveContainer.GetCurveSample (timeNormalized);
            valueLast = Mathf.Lerp (from, to, sample);
            mpb.SetFloat (propertyID, valueLast);
        }

        public void Apply (MaterialPropertyBlock mpb)
        {
            if (!propertyValid)
                return;
            
            if (!valueUpdated)
                valueLast = from;
            mpb.SetFloat (propertyID, valueLast);
        }
        
        #if UNITY_EDITOR

        protected override List<string> GetPropertyNames ()
        {
            CheckPropertyCache ();
            return shaderPropertyFloatNames;
        }
        
        private void SetFrom ()
        {
            var value = GetValueFromMaterial (out bool success);
            if (success)
                from = value;
        }
        
        private void SetTo ()
        {
            var value = GetValueFromMaterial (out bool success);
            if (success)
                to = value;
        }

        private float GetValueFromMaterial (out bool success)
        {
            success = false;
            if (!IsSettingSafe ())
                return default;

            var r = parent.renderers[0];
            var sm = r.sharedMaterial;
            
            if (!sm.HasProperty (propertyID))
                return default;

            var value = sm.GetFloat (propertyID);
            success = true;
            return value;
        }

        #endif
    }

    [Serializable]
    public class StepRendererPropertyVector : StepRendererProperty
    {
        [InlineButton ("SetFrom", "◄")]
        public Vector4 from;
        [InlineButton ("SetTo", "◄")]
        public Vector4 to;
        
        [NonSerialized]
        private Vector4 valueLast;

        public void UpdateAndApply (MaterialPropertyBlock mpb, float timeNormalized)
        {
            if (!propertyValid)
                return;
            
            var sample = linear ? timeNormalized.RemapTo01 (linearRange) : curveContainer.GetCurveSample (timeNormalized);
            valueLast = Vector4.Lerp (from, to, sample);
            mpb.SetVector (propertyID, valueLast);
        }

        public void Apply (MaterialPropertyBlock mpb)
        {
            if (!propertyValid)
                return;
            
            if (!valueUpdated)
                valueLast = from;
            mpb.SetVector (propertyID, valueLast);
        }
        
        #if UNITY_EDITOR

        protected override List<string> GetPropertyNames ()
        {
            CheckPropertyCache ();
            return shaderPropertyVectorNames;
        }
        
        private void SetFrom ()
        {
            var value = GetValueFromMaterial (out bool success);
            if (success)
                from = value;
        }
        
        private void SetTo ()
        {
            var value = GetValueFromMaterial (out bool success);
            if (success)
                to = value;
        }

        private Vector4 GetValueFromMaterial (out bool success)
        {
            success = false;
            if (!IsSettingSafe ())
                return default;

            var r = parent.renderers[0];
            var sm = r.sharedMaterial;
            
            if (!sm.HasProperty (propertyID))
                return default;

            var value = sm.GetVector (propertyID);
            success = true;
            return value;
        }

        #endif
    }

    [Serializable]
    public class StepRendererPropertyColor : StepRendererProperty
    {
        [InlineButton ("SetFrom", "◄")][ColorUsage (true, true)]
        public Color from = Color.white.WithAlpha (1f);
        [InlineButton ("SetTo", "◄")][ColorUsage (true, true)]
        public Color to = Color.white.WithAlpha (1f);
        
        [NonSerialized]
        private Color valueLast;

        public void UpdateAndApply (MaterialPropertyBlock mpb, float timeNormalized)
        {
            if (!propertyValid)
                return;
            
            var sample = linear ? timeNormalized.RemapTo01 (linearRange) : curveContainer.GetCurveSample (timeNormalized);
            valueLast = Color.Lerp (from, to, sample);
            mpb.SetColor (propertyID, valueLast);
        }

        public void Apply (MaterialPropertyBlock mpb)
        {
            if (!propertyValid)
                return;
            
            if (!valueUpdated)
                valueLast = from;
            mpb.SetColor (propertyID, valueLast);
        }
        
        #if UNITY_EDITOR

        protected override List<string> GetPropertyNames ()
        {
            CheckPropertyCache ();
            return shaderPropertyColorNames;
        }
        
        private void SetFrom ()
        {
            var value = GetValueFromMaterial (out bool success);
            if (success)
                from = value;
        }
        
        private void SetTo ()
        {
            var value = GetValueFromMaterial (out bool success);
            if (success)
                to = value;
        }

        private Color GetValueFromMaterial (out bool success)
        {
            success = false;
            if (!IsSettingSafe ())
                return default;

            var r = parent.renderers[0];
            var sm = r.sharedMaterial;
            
            if (!sm.HasProperty (propertyID))
                return default;

            var value = sm.GetColor (propertyID);
            success = true;
            return value;
        }

        #endif
    }
}
