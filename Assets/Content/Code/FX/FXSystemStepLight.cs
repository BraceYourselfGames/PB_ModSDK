using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepLight
    {
        [OnValueChanged ("InitializeWithoutParent")][InlineButton ("InitializeWithoutParent", "@GetButtonText ()")]
        public GameObject target;

        [ReadOnly][ShowInInspector][ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)][Space (4f)]
        [NonSerialized][ShowIf ("AreLightsVisible")]
        public Light[] lights;
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !activation.enabled")]
        public StepLightActivation activation = new StepLightActivation ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !intensity.enabled")]
        public StepLightIntensity intensity = new StepLightIntensity ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !intensity.enabled")]
        public StepLightRange range = new StepLightRange ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !color.enabled")]
        public StepLightColor color = new StepLightColor ();

        [NonSerialized]
        private Step parent;
        
        
        public void InitializeWithoutParent ()
        {
            Initialize (null);
        }
        
        public void Initialize (Step parent)
        {
            if (parent != null)
                this.parent = parent;

            if (target == null)
                return;
            
            if (activation == null)
                activation = new StepLightActivation ();
            
            if (intensity == null)
                intensity = new StepLightIntensity ();

            if (color == null)
                color = new StepLightColor ();
            
            lights = target.GetComponentsInChildren<Light> (false);
            OnStop ();
        }

        public void OnStop (float timeNormalizedForced = 1f)
        {
            if (target == null || lights == null)
                return;
            
            Animate (timeNormalizedForced);
            
            if (activation.enabled)
            {
                for (int i = 0; i < lights.Length; ++i)
                    lights[i].enabled = false;
            }
        }

        public void OnPlay ()
        {
            if (target == null || lights == null)
                return;
            
            if (activation.enabled)
            {
                for (int i = 0; i < lights.Length; ++i)
                    lights[i].enabled = true;
            }
        }

        public void Animate (float timeNormalized)
        {
            if (target == null || lights == null)
                return;

            if (intensity.enabled)
            {
                var sample = intensity.linear ? timeNormalized.RemapTo01 (intensity.linearRange) : intensity.curveContainer.GetCurveSample (timeNormalized);
                var value = Mathf.Lerp (intensity.range.x, intensity.range.y, sample);
                
                for (int i = 0; i < lights.Length; ++i)
                    lights[i].intensity = value;
            }
            
            if (range.enabled)
            {
                var sample = range.linear ? timeNormalized.RemapTo01 (range.linearRange) : range.curveContainer.GetCurveSample (timeNormalized);
                var value = Mathf.Lerp (range.range.x, range.range.y, sample);
                
                for (int i = 0; i < lights.Length; ++i)
                    lights[i].range = value;
            }

            if (color.enabled)
            {
                var sample = color.linear ? timeNormalized.RemapTo01 (color.linearRange) : intensity.curveContainer.GetCurveSample (timeNormalized);
                var value = Color.Lerp (color.from, color.to, sample);
                
                for (int i = 0; i < lights.Length; ++i)
                    lights[i].color = value;
            }
            
            var activationControl = !activation.enabled;
            if (activationControl)
            {
                for (int i = 0; i < lights.Length; ++i)
                {
                    var light = lights[i];
                    bool lightEnabled = light.range > 0.1f && light.intensity > 0.1f;
                    if (light.enabled != lightEnabled)
                        light.enabled = lightEnabled;
                }
            }
        }
        
        #if UNITY_EDITOR
        
        private string GetButtonText () =>
            target == null ? "Set target" : $"Recheck children ({(lights != null ? lights.Length.ToString () : "none")})";

        public void UpdateInspectorProperties ()
        {
            if (lights != null && lights.Length > 0)
            {
                var light = lights[0];
                
                if (intensity.enabled)
                {
                    intensity.parent = this;
                    intensity.curveContainer.indentFix = true;
                    // intensity.current = light.intensity;
                }

                if (color.enabled)
                {
                    color.parent = this;
                    color.curveContainer.indentFix = true;
                    // color.current = light.color;
                }
            }
        }
        
        private bool AreLightsVisible ()
        {
            if (target == null || lights == null)
                return false;
            else if (lights.Length == 1)
                return !(lights[0] != null && lights[0].gameObject == target);
            else
                return true;
        }
        
        #endif
    }
}
