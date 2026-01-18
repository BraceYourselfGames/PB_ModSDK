using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public enum StepState
        {
            Waiting,
            Playing,
            Finished
        }

        [ShowIf ("IsSystemInfoVisible")]
        public string name = "main";
        [Min (0f)]
        public float startTime = 0f;
        [Min (0.1f)]
        public float duration = 1f;

        [ShowInInspector][ReadOnly][ShowIf ("IsSystemInfoVisible")]
        public StepState state = StepState.Waiting;
        [ShowInInspector][ReadOnly][ShowIf ("IsSystemInfoVisible")]
        public float timePlaying = 0f;
        [ShowInInspector][ReadOnly][PropertyRange (0f, 1f)]
        public float timeNormalized = 0f;
        
        [NonSerialized]
        private Action onComplete;

        [ListDrawerSettings (DefaultExpandedState = true, NumberOfItemsPerPage = 1)]
        public List<StepRenderer> renderers = new List<StepRenderer> ();
        
        [ListDrawerSettings (DefaultExpandedState = true, NumberOfItemsPerPage = 1)]
        public List<StepLight> lights = new List<StepLight> ();

        [ListDrawerSettings (DefaultExpandedState = true, NumberOfItemsPerPage = 1)]
        public List<StepSystem> systems = new List<StepSystem> ();
        
        [ListDrawerSettings (DefaultExpandedState = true, NumberOfItemsPerPage = 1)]
        public List<StepAudio> audioEvent = new List<StepAudio> ();

        [NonSerialized]
        public FXSystem parentSystem;
        
        [NonSerialized]
        public FXTween parentTween;


        private void ValidateTimeValues ()
        {
            startTime = Mathf.Max (0f, startTime);
            duration = Mathf.Max (0.1f, duration);
        }
        
        public void Initialize (Action onComplete)
        {
            ValidateTimeValues ();
            this.onComplete = onComplete;

            for (int i = 0; i < renderers.Count; ++i)
                renderers[i].Initialize (this);
            
            for (int i = 0; i < lights.Count; ++i)
                lights[i].Initialize (this);
            
            for (int i = 0; i < systems.Count; ++i)
                systems[i].Initialize (this);
        }

        public void BeginWait ()
        {
            timeNormalized = 0f;
            state = StepState.Waiting;
            bool clear = parentSystem != null ? parentSystem.clearOnReset : (parentTween != null ? parentTween.clearOnReset : false);

            for (int i = 0; i < renderers.Count; ++i)
                renderers[i].OnStop (0f);
            
            for (int i = 0; i < lights.Count; ++i)
                lights[i].OnStop (0f);
            
            for (int i = 0; i < systems.Count; ++i)
                systems[i].OnStop (0f, clear);
        }

        public void Play ()
        {
            timePlaying = 0f;
            state = StepState.Playing;

            for (int i = 0; i < renderers.Count; ++i)
            {
                renderers[i].OnPlay ();
                renderers[i].Animate (0f);
            }

            for (int i = 0; i < lights.Count; ++i)
            {
                lights[i].OnPlay ();
                lights[i].Animate (0f);
            }

            for (int i = 0; i < systems.Count; ++i)
            {
                systems[i].OnPlay ();
                systems[i].Animate (0f);
            }

            for (int i = 0; i < audioEvent.Count; ++i)
            {
                audioEvent[i].OnPlay ();
            }
        }
        
        public void AnimateDirectly (float timeNormalized)
        {
            timeNormalized = Mathf.Clamp01 (timeNormalized);
            this.timeNormalized = timeNormalized;
            
            for (int i = 0; i < renderers.Count; ++i)
                renderers[i].Animate (timeNormalized);
            
            for (int i = 0; i < lights.Count; ++i)
                lights[i].Animate (timeNormalized);
            
            for (int i = 0; i < systems.Count; ++i)
                systems[i].Animate (timeNormalized);
        }

        public void Animate (float timeFromSystem, bool skipStateCheck = false)
        {
            if (!skipStateCheck && (parentSystem != null || (parentTween != null && !parentTween.manualAnimation)))
            {
                if (state != StepState.Playing || duration <= 0f)
                    return;
            }

            timePlaying = timeFromSystem - startTime;
            AnimateDirectly (Mathf.Clamp01 (timePlaying / duration));
            
            if (timePlaying > duration)
                Finish ();
        }

        public void Finish (float timeNormalizedForced = 1f)
        {
            timeNormalized = timeNormalizedForced;
            state = StepState.Finished;
            onComplete?.Invoke ();
            bool clear = parentSystem != null ? parentSystem.clearOnReset : (parentTween != null ? parentTween.clearOnReset : false);
            
            for (int i = 0; i < renderers.Count; ++i)
                renderers[i].OnStop (timeNormalizedForced);
            
            for (int i = 0; i < lights.Count; ++i)
                lights[i].OnStop (timeNormalizedForced);
            
            for (int i = 0; i < systems.Count; ++i)
                systems[i].OnStop (timeNormalizedForced, clear);
        }

        #if UNITY_EDITOR

        private bool IsSystemInfoVisible () => parentSystem != null;

        public void UpdateInspectorProperties ()
        {
            for (int i = 0; i < renderers.Count; ++i)
                renderers[i].UpdateInspectorProperties ();
            
            for (int i = 0; i < lights.Count; ++i)
                lights[i].UpdateInspectorProperties ();

            // for (int i = 0; i < systems.Count; ++i)
            //     systems[i].UpdateInspectorProperties (this);
        }
        #endif
    }
    
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class StepBlock
    {
        public bool enabled;
    }
    
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class StepBlockWithCurve
    {
        public bool enabled;
        
        [HideReferenceObjectPicker][HideLabel][HideIf ("linear")]
        public AnimationCurveContainer curveContainer = new AnimationCurveContainer ();
        
        [LabelWidth(2f)][LabelText("")][MinMaxSlider (0f, 1f)][ShowIf ("linear")]
        public Vector2 linearRange = new Vector2 (0f, 1f);
        
        public bool linear;
    }
}
