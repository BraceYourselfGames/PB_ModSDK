
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Game
{
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class DataBlockAudioEventsInButton
    {
        public bool enabled;
        public DataBlockAudioEvent onHoverStart;
        public DataBlockAudioEvent onHoverEnd;
        public DataBlockAudioEvent onClick;
        public DataBlockAudioEvent onClickLocked;
        public DataBlockAudioEvent onClickLong;
    }
    
    // Special container for easily editable audio bindings
    [Serializable][HideReferenceObjectPicker]
    [Toggle ("enabled", CollapseOthersOnExpand = false)]
    [LabelWidth (120f)]
    public class DataBlockAudioEvent
    {
        public bool enabled;

        [HorizontalGroup]
        [HideLabel]
        [ShowIf ("eventRegistered")]
        [ValueDropdown ("GetTypeNames")]
        [OnValueChanged ("OnTypeChange")]
        public string eventRegisteredType;

        [HorizontalGroup]
        [HideLabel]
        [ShowIf ("eventRegistered")]
        [ValueDropdown ("GetKeyNames")]
        [InlineButton ("ToggleMode", "Registered")]
        public string eventRegisteredKey;

        [HorizontalGroup]
        [HideLabel]
        [HideIf ("eventRegistered")]
        [InlineButton ("ToggleMode", "Custom")]
        public string eventName;

        [HideInInspector]
        public bool eventRegistered = true;

        
        
         
        public void Play ()
        {
            
        }

        // Add new audio libraries here
        private static Dictionary<string, Type> eventTypes = new Dictionary<string, Type>
        {
            { typeof (AudioEventUIBase).Name, typeof (AudioEventUIBase) },
            { typeof (AudioEventUICustomization).Name, typeof (AudioEventUICustomization) },
            { typeof (AudioEventUISalvage).Name, typeof (AudioEventUISalvage) },
            { typeof (AudioEventUITitle).Name, typeof (AudioEventUITitle) },
            { typeof (AudioEventUIOverworld).Name, typeof(AudioEventUIOverworld) },
            { typeof (AudioEventUICombat).Name, typeof(AudioEventUICombat) },
            { typeof (AudioEvents).Name, typeof(AudioEvents)}
        };

        #if UNITY_EDITOR

        // Used for audio type dropdown
        private IEnumerable<string> GetTypeNames () => 
            eventTypes.Keys;

        private void OnTypeChange () =>
            eventRegisteredKey = string.Empty;

        // Used for audio key dropdown
        private List<string> GetKeyNames ()
        {
            if (string.IsNullOrEmpty (eventRegisteredType) || !eventTypes.ContainsKey (eventRegisteredType))
                return null;
            return FieldReflectionUtility.GetConstantStringFieldNames (eventTypes[eventRegisteredType]);
        }

        private void ToggleMode (string value) =>
            eventRegistered = !eventRegistered;

        #endif
    }
}
