using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepLightActivation : StepBlock
    {

    }
    
    [Serializable]
    public class StepLightIntensity : StepBlockWithCurve
    {
        [InlineButton ("SetFrom", "◄")]
        [LabelText ("Min/max")]
        public Vector2 range;

        #if UNITY_EDITOR
        
        [NonSerialized]
        public StepLight parent;

        private bool IsSettingSafe ()
        {
            return !(parent == null || parent.lights == null || parent.lights.Length == 0);
        }
        
        private void SetFrom ()
        {
            if (IsSettingSafe ())
                range.x = parent.lights[0].intensity;
        }
        
        private void SetTo ()
        {
            if (IsSettingSafe ())
                range.y = parent.lights[0].intensity;
        }
        
        #endif
    }
    
    [Serializable]
    public class StepLightRange : StepBlockWithCurve
    {
        [InlineButton ("SetFrom", "◄")]
        [LabelText ("Min/max")]
        public Vector2 range;

        #if UNITY_EDITOR
        
        [NonSerialized]
        public StepLight parent;

        private bool IsSettingSafe ()
        {
            return !(parent == null || parent.lights == null || parent.lights.Length == 0);
        }
        
        private void SetFrom ()
        {
            if (IsSettingSafe ())
                range.x = parent.lights[0].range;
        }
        
        private void SetTo ()
        {
            if (IsSettingSafe ())
                range.y = parent.lights[0].range;
        }
        
        #endif
    }
    
    [Serializable]
    public class StepLightColor : StepBlockWithCurve
    {
        [InlineButton ("SetFrom", "◄")][ColorUsage (false)]
        public Color from;

        [InlineButton ("SetTo", "◄")][ColorUsage (false)]
        public Color to;
        
        #if UNITY_EDITOR
        
        [NonSerialized]
        public StepLight parent;

        private bool IsSettingSafe ()
        {
            return !(parent == null || parent.lights == null || parent.lights.Length == 0);
        }
        
        private void SetFrom ()
        {
            if (IsSettingSafe ())
                from = parent.lights[0].color;
        }
        
        private void SetTo ()
        {
            if (IsSettingSafe ())
                to = parent.lights[0].color;
        }
        
        #endif
    }
}
