using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker][LabelWidth (160f)]
    public class DataContainerAssetPool : DataContainer
    {
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;
        
        [InfoBox ("When limit is set to 0, the object will only be available for direct instantiation via GetInstanceStandalone method - no pool would be created", InfoMessageType.Warning, "IsLimitInvalid")]
        [PropertyRange (0, 300)]
        public int limit;

        [HideIf ("IsLimitInvalid")]
        public bool thresholdsActivated = false;
        
        [ShowIf ("@thresholdsActivated && !IsLimitInvalid")]
        [LabelText ("Min. Distance")]
        public float thresholdDist = 0f;
        
        [ShowIf ("@thresholdsActivated && !IsLimitInvalid"), OnValueChanged("OnThresholdChanged")]
        [LabelText ("Min. Time Interval")]
        public float timeThreshold = 0f;

        [HideIf ("IsLimitInvalid")]
        public bool resetOnActivation = false;

        public bool clearInactiveChildren = true;

        public bool lifetimeUsed = false;
        
        [ShowIf ("lifetimeUsed")][InlineButton ("GetLifetimeFromPrefab", "Find")]
        public float lifetime = 0f;

        [YamlIgnore][ReadOnly][HideInEditorMode]
        public Transform holder;
        
        [YamlIgnore][ReadOnly][HideInEditorMode][ShowInInspector]
        public int instanceCountUsed = 0;
        
        [YamlIgnore][ReadOnly][HideInEditorMode][ShowInInspector]
        public int instanceIndexNext = 0;

        [YamlIgnore][ReadOnly][HideInEditorMode]
        public Vector3 lastRequestPosition = Vector3.negativeInfinity;

        [YamlIgnore][ReadOnly][HideInEditorMode]
        public float lastRequestTime = float.NegativeInfinity;

        [YamlIgnore][ReadOnly][HideInEditorMode]
        public float positionThresholdDistSq = 0f;


        public override void OnBeforeSerialization ()
        {

        }

        
        
        
        public override void OnAfterDeserialization (string key)
        {
            
        }
        
        private void ClearInactiveRecursive (Transform parent)
        {
            
        }

        private void GetLifetimeFromPrefab ()
        {
            
        }

        // Separate method to allow invocation from UI
        private void OnThresholdChanged ()
        {
	        positionThresholdDistSq = thresholdDist * thresholdDist;
        }

        public bool IsInstanceAvailable (bool blockReuse)
        {
            return false;
        }
        
        #if UNITY_EDITOR
        
        // private Color GetPrefabColor () => 
        //     Color.HSVToRGB (prefab != null ? 0.55f : 0f, 0.5f, 1f);
        
        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);

        private bool IsLimitInvalid =>
            limit <= 0;

        #endif
    }
}

