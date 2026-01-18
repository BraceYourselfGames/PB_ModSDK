using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepRenderer
    {
        [OnValueChanged ("InitializeWithoutParent")][InlineButton ("InitializeWithoutParent", "@GetButtonText ()")]
        public GameObject target;
        
        [ReadOnly][ShowInInspector][ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)][Space (4f)]
        [NonSerialized][ShowIf ("AreRenderersVisible")]
        public Renderer[] renderers;

        [HideIf ("@!FXSystem.showInactiveBlocks && !targetActivation.enabled")]
        public StepRendererActivation targetActivation = new StepRendererActivation ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !targetPosition.enabled")]
        public StepRendererPosition targetPosition = new StepRendererPosition ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !targetRotation.enabled")]
        public StepRendererRotation targetRotation = new StepRendererRotation ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !targetScale.enabled")]
        public StepRendererScale targetScale = new StepRendererScale ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !rendererActivation.enabled")]
        public StepRendererActivation rendererActivation = new StepRendererActivation ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !rendererPosition.enabled")]
        public StepRendererPosition rendererPosition = new StepRendererPosition ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !rendererRotation.enabled")]
        public StepRendererRotation rendererRotation = new StepRendererRotation ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !rendererScale.enabled")]
        public StepRendererScale rendererScale = new StepRendererScale ();
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !rendererMaterial.enabled")]
        public StepRendererMaterial rendererMaterial = new StepRendererMaterial ();

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
            
            if (targetActivation == null)
                targetActivation = new StepRendererActivation ();
            
            if (targetPosition == null)
                targetPosition = new StepRendererPosition ();
            
            if (targetRotation == null)
                targetRotation = new StepRendererRotation ();
            
            if (targetScale == null)
                targetScale = new StepRendererScale ();
        
            if (rendererActivation == null)
                rendererActivation = new StepRendererActivation ();
            
            if (rendererPosition == null)
                rendererPosition = new StepRendererPosition ();
            
            if (rendererRotation == null)
                rendererRotation = new StepRendererRotation ();
            
            if (rendererScale == null)
                rendererScale = new StepRendererScale ();
            
            if (rendererMaterial == null)
                rendererMaterial = new StepRendererMaterial ();
            
            renderers = target.GetComponentsInChildren<Renderer> (true);
            targetRotation.UpdateQuaternions ();
            rendererRotation.UpdateQuaternions ();
            OnStop ();

            for (int i = 0; i < rendererMaterial.floats.Count; ++i)
                rendererMaterial.floats[i].Initialize ();

            for (int i = 0; i < rendererMaterial.vectors.Count; ++i)
                rendererMaterial.vectors[i].Initialize ();

            for (int i = 0; i < rendererMaterial.colors.Count; ++i)
                rendererMaterial.colors[i].Initialize ();
        }

        public void OnPlay ()
        {
            if (target == null)
                return;
            
            if (targetActivation.enabled)
                target.SetActive (true);
            
            if (rendererActivation.enabled && renderers != null && renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; ++i)
                    renderers[i].enabled = true;
            }
        }

        public void Animate (float timeNormalized)
        {
            if (target == null)
                return;

            // Debug.Log ($"Animating {target.name} in step {parent.name} ({target.GetInstanceID ()}) to {timeNormalized}");
            
            if (targetPosition.enabled)
            {
                var sample = targetPosition.linear ? timeNormalized.RemapTo01 (targetPosition.linearRange) : targetPosition.curveContainer.GetCurveSample (timeNormalized);
                target.transform.localPosition = Vector3.LerpUnclamped (targetPosition.from, targetPosition.to, sample);
            }

            if (targetRotation.enabled)
            {
                var sample = targetRotation.linear ? timeNormalized.RemapTo01 (targetRotation.linearRange) : targetRotation.curveContainer.GetCurveSample (timeNormalized);
                target.transform.localRotation = Quaternion.Lerp (targetRotation.fromQt, targetRotation.toQt, sample);
            }

            if (targetScale.enabled)
            {
                var sample = targetScale.linear ? timeNormalized.RemapTo01 (targetScale.linearRange) : targetScale.curveContainer.GetCurveSample (timeNormalized);
                if (targetScale.uniform)
                    target.transform.localScale = Vector3.one * Mathf.Lerp (targetScale.range.x, targetScale.range.y, sample);
                else
                    target.transform.localScale = Vector3.LerpUnclamped (targetScale.from, targetScale.to, sample);
            }

            bool renderersPresent = renderers != null && renderers.Length > 0;
            if (renderersPresent)
            {
                if (rendererPosition.enabled)
                {
                    var sample = rendererPosition.linear ? timeNormalized.RemapTo01 (rendererPosition.linearRange) : rendererPosition.curveContainer.GetCurveSample (timeNormalized);
                    var localPosition = Vector3.LerpUnclamped (rendererPosition.from, rendererPosition.to, sample);
                    for (int i = 0; i < renderers.Length; ++i)
                        renderers[i].transform.localPosition = localPosition;
                }
                
                if (rendererRotation.enabled)
                {
                    var sample = rendererRotation.linear ? timeNormalized.RemapTo01 (rendererRotation.linearRange) : rendererRotation.curveContainer.GetCurveSample (timeNormalized);
                    var localRotation = Quaternion.Lerp (rendererRotation.fromQt, rendererRotation.toQt, sample);
                    for (int i = 0; i < renderers.Length; ++i)
                        renderers[i].transform.localRotation = localRotation;
                }
                
                if (rendererScale.enabled)
                {
                    var sample = rendererScale.linear ? timeNormalized.RemapTo01 (rendererScale.linearRange) : rendererScale.curveContainer.GetCurveSample (timeNormalized);
                    var localScale =
                        rendererScale.uniform ? 
                        Vector3.one * Mathf.Lerp (rendererScale.range.x, rendererScale.range.y, sample) : 
                        Vector3.LerpUnclamped (rendererScale.from, rendererScale.to, sample);

                    for (int i = 0; i < renderers.Length; ++i)
                        renderers[i].transform.localScale = localScale;
                }
                
                if (rendererMaterial.enabled)
                {
                    if (mpb == null)
                        mpb = new MaterialPropertyBlock ();

                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        Renderer currentRenderer = renderers[i];
                        mpb.Clear ();
                        currentRenderer.GetPropertyBlock (mpb);

                        for (int j = 0; j < rendererMaterial.floats.Count; ++j)
                            rendererMaterial.floats[j].UpdateAndApply (mpb, timeNormalized);

                        for (int j = 0; j < rendererMaterial.vectors.Count; ++j)
                            rendererMaterial.vectors[j].UpdateAndApply (mpb, timeNormalized);

                        for (int j = 0; j < rendererMaterial.colors.Count; ++j)
                            rendererMaterial.colors[j].UpdateAndApply (mpb, timeNormalized);

                        currentRenderer.SetPropertyBlock (mpb);
                    }
                }
            }
        }

        public void OnStop (float timeNormalizedForced = 1f)
        {
            if (target == null)
                return;
            
            Animate (timeNormalizedForced);
            
            if (targetActivation.enabled)
                target.SetActive (false);
            
            if (renderers != null && renderers.Length > 0)
            {
                if (rendererActivation.enabled)
                {
                    for (int i = 0; i < renderers.Length; ++i)
                        renderers[i].enabled = false;
                }

                /*
                if (rendererMaterial.enabled)
                {
                    for (int i = 0; i < renderers.Length; ++i)
                        renderers[i].SetPropertyBlock (null);
                }
                */
            }
        }
        
        #if UNITY_EDITOR
        
        private string GetButtonText () =>
            target == null ? "Set target" : $"Recheck children ({(renderers != null ? renderers.Length.ToString () : "none")})";

        public void UpdateInspectorProperties ()
        {
            if (target != null)
            {
                targetPosition.parent = this;
                targetPosition.curveContainer.indentFix = true;
                // targetPosition.current = target.transform.localPosition;

                targetRotation.parent = this;
                targetRotation.curveContainer.indentFix = true;
                // targetRotation.current = target.transform.localRotation.eulerAngles;

                targetScale.parent = this;
                targetScale.curveContainer.indentFix = true;
                // targetScale.current = target.transform.localScale;
            }

            if (renderers != null && renderers.Length > 0 && renderers[0] != null)
            {
                var renderer = renderers[0].transform;

                rendererPosition.parent = this;
                rendererPosition.curveContainer.indentFix = true;
                // rendererPosition.current = target.transform.localPosition;

                rendererRotation.parent = this;
                rendererRotation.curveContainer.indentFix = true;
                // rendererRotation.current = target.transform.localRotation.eulerAngles;

                rendererScale.parent = this;
                rendererScale.curveContainer.indentFix = true;
                // rendererScale.current = target.transform.localScale;

                for (int i = 0; i < rendererMaterial.floats.Count; ++i)
                    rendererMaterial.floats[i].parent = this;

                for (int i = 0; i < rendererMaterial.vectors.Count; ++i)
                    rendererMaterial.vectors[i].parent = this;

                for (int i = 0; i < rendererMaterial.colors.Count; ++i)
                    rendererMaterial.colors[i].parent = this;
            }
        }

        private bool AreRenderersVisible ()
        {
            if (target == null || renderers == null)
                return false;
            else if (renderers.Length == 1)
                return !(renderers[0] != null && renderers[0].gameObject == target);
            else
                return true;
        }
        
        #endif
    }
}
