using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepSystem
    {
        [OnValueChanged ("InitializeWithoutParent")][InlineButton ("InitializeWithoutParent", "@GetButtonText ()")]
        public GameObject target;
        
        [ReadOnly][ShowInInspector][ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)][Space (4f)]
        [NonSerialized][ShowIf ("AreSystemsVisible")]
        public ParticleSystem[] systems;
        
        [HideIf ("@!FXSystem.showInactiveBlocks && !playback.enabled")]
        public StepSystemPlayback playback = new StepSystemPlayback ();

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

            if (playback == null)
                playback = new StepSystemPlayback ();
            
            systems = target.GetComponentsInChildren<ParticleSystem> (true);

            bool clear = false;
            if (parent != null)
                clear = parent.parentSystem != null ? parent.parentSystem.clearOnReset : (parent.parentTween != null ? parent.parentTween.clearOnReset : false);
            
            OnStop (clear: clear);
        }

        public void OnStop (float timeNormalizedForced = 1f, bool clear = false)
        {
            if (target == null || systems == null)
                return;
            
            // Animate (timeNormalizedForced);

            if (playback.enabled)
            {
                if (Mathf.Approximately (timeNormalizedForced, 0f))
                {
                    for (int i = 0; i < systems.Length; ++i)
                        systems[i].SetSystemPlaying (false, clear: clear);
                }

                else if (Mathf.Approximately (timeNormalizedForced, 1f) && playback.stopOnFinish)
                {
                    for (int i = 0; i < systems.Length; ++i)
                        systems[i].SetSystemPlaying (false, clear: clear);
                }
            }
        }

        public void OnPlay ()
        {
            if (target == null || systems == null)
                return;
            
            if (playback.enabled && playback.playOnStart)
            {
                for (int i = 0; i < systems.Length; ++i)
                    systems[i].SetSystemPlaying (true);
            }
        }

        public void Animate (float timeNormalized)
        {
            // if (target == null || systems == null)
            //     return;

            // We only directly force time on particle systems for edit mode previewing - elsewhere they should play traditionally
            if (!Application.isPlaying && parent != null)
            {
                var durationFromParent = parent.duration;
                var timeAbsolute = timeNormalized * durationFromParent;
                
                for (int i = 0; i < systems.Length; ++i)
                {
                    var system = systems[i];
                    var duration = system.main.duration;
                    var systemTime = timeNormalized * duration;
                    systems[i].Simulate (timeAbsolute, true, true);
                }
            }
        }
        
        #if UNITY_EDITOR
        
        private string GetButtonText () =>
            target == null ? "Set target" : $"Recheck children ({(systems != null ? systems.Length.ToString () : "none")})";

        private bool AreSystemsVisible ()
        {
            if (target == null || systems == null)
                return false;
            else if (systems.Length == 1)
                return !(systems[0] != null && systems[0].gameObject == target);
            else
                return true;
        }
        
        #endif
    }
}
