using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public partial class FXSystem : MonoBehaviour
{
    [Serializable]
    public class StepRendererActivation : StepBlock
    {

    }
    
    [Serializable]
    public class StepRendererPosition : StepBlockWithCurve
    {
        [InlineButton ("SetFrom", "◄")]
        public Vector3 from;

        [InlineButton ("SetTo", "◄")]
        public Vector3 to;
        
        #if UNITY_EDITOR
        
        [NonSerialized]
        public StepRenderer parent;
        
        [NonSerialized]
        public bool useTarget = true;
        
        private bool IsSettingSafe ()
        {
            return !(parent == null || (useTarget && parent.target == null) || (!useTarget && (parent.renderers == null || parent.renderers.Length == 0)));
        }
        
        private void SetFrom ()
        {
            if (!IsSettingSafe ())
                return;

            if (useTarget)
                from = parent.target.transform.localPosition;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                from = parent.renderers[0].transform.localPosition;
        }
        
        private void SetTo ()
        {
            if (!IsSettingSafe ())
                return;

            if (useTarget)
                to = parent.target.transform.localPosition;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                to = parent.renderers[0].transform.localPosition;
        }
        
        #endif
    }
    
    [Serializable]
    public class StepRendererRotation : StepBlockWithCurve
    {
        [OnValueChanged ("UpdateQuaternions")]
        [InlineButton ("SetFrom", "Set")]
        public Vector3 from;
        
        [OnValueChanged ("UpdateQuaternions")]
        [InlineButton ("SetTo", "Set")]
        public Vector3 to;
        
        [NonSerialized]
        public Quaternion fromQt;
        
        [NonSerialized]
        public Quaternion toQt;
        
        public void UpdateQuaternions ()
        {
            fromQt = Quaternion.Euler (from);
            toQt = Quaternion.Euler (to);
        }
        
        #if UNITY_EDITOR
        
        [NonSerialized]
        public StepRenderer parent;
        
        [NonSerialized]
        public bool useTarget = true;

        private bool IsSettingSafe ()
        {
            return !(parent == null || (useTarget && parent.target == null) || (!useTarget && (parent.renderers == null || parent.renderers.Length == 0)));
        }
        
        private void SetFrom ()
        {
            if (!IsSettingSafe ())
                return;

            if (useTarget)
                from = parent.target.transform.localRotation.eulerAngles;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                from = parent.renderers[0].transform.localRotation.eulerAngles;
            UpdateQuaternions ();
        }
        
        private void SetTo ()
        {
            if (!IsSettingSafe ())
                return;

            if (useTarget)
                to = parent.target.transform.localRotation.eulerAngles;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                to = parent.renderers[0].transform.localRotation.eulerAngles;
            UpdateQuaternions ();
        }
        
        #endif
    }
    
    [Serializable]
    public class StepRendererScale : StepBlockWithCurve
    {
        public bool uniform = false;

        [HideIf ("uniform")]
        [InlineButton ("SetFrom", "◄")]
        public Vector3 from;
        
        [HideIf ("uniform")]
        [InlineButton ("SetTo", "◄")]
        public Vector3 to;
        
        [ShowIf ("uniform")]
        [InlineButton ("SetRangeFrom", "◄ min")]
        [InlineButton ("SetRangeTo", "◄ max")]
        [LabelText ("From/to")]
        public Vector2 range;
        
        #if UNITY_EDITOR
        
        [NonSerialized]
        public StepRenderer parent;
        
        [NonSerialized]
        public bool useTarget = true;
        
        private bool IsSettingSafe ()
        {
            return !(parent == null || (useTarget && parent.target == null) || (!useTarget && (parent.renderers == null || parent.renderers.Length == 0)));
        }
        
        private void SetFrom ()
        {
            if (!IsSettingSafe ())
                return;

            if (useTarget)
                from = parent.target.transform.localScale;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                from = parent.renderers[0].transform.localScale;
        }
        
        private void SetTo ()
        {
            if (!IsSettingSafe ())
                return;
            
            if (useTarget)
                to = parent.target.transform.localScale;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                to = parent.renderers[0].transform.localScale;
        }
        
        private void SetRangeFrom ()
        {
            if (!IsSettingSafe ())
                return;
            
            if (useTarget)
                range.x = parent.target.transform.localScale.x;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                range.x = parent.renderers[0].transform.localScale.x;
        }
        
        private void SetRangeTo ()
        {
            if (!IsSettingSafe ())
                return;
            
            if (useTarget)
                range.y = parent.target.transform.localScale.x;
            else if (parent.renderers != null && parent.renderers.Length > 0 && parent.renderers[0] != null)
                range.y = parent.renderers[0].transform.localScale.x;
        }
        
        #endif
    }
    
    [Serializable]
    public class StepRendererMaterial : StepBlock
    {
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public List<StepRendererPropertyFloat> floats = new List<StepRendererPropertyFloat> ();
        
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public List<StepRendererPropertyVector> vectors = new List<StepRendererPropertyVector> ();
        
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false)]
        public List<StepRendererPropertyColor> colors = new List<StepRendererPropertyColor> ();
    }
}
