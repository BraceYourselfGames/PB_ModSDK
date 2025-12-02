
using System;
using System.Collections.Generic;
using PhantomBrigade.Game.Systems;
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
    [Serializable] [HideReferenceObjectPicker]
    [Toggle ("enabled", CollapseOthersOnExpand = false)]
    [LabelWidth (120f)]
    public class DataBlockAudioEventTimed : DataBlockAudioEvent
    {
        [PropertyOrder (-1)]
        public float time = 0f;
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
        public string eventRegisteredType;

        [HorizontalGroup]
        [HideLabel]
        [ShowIf ("eventRegistered")]
        [ValueDropdown ("GetKeyNames")]
        [InlineButton ("ToggleMode", "Builtin")]
        [InlineButton ("ClearKeys", "-")]
        public string eventRegisteredKey;

        [HorizontalGroup]
        [HideLabel]
        [HideIf ("eventRegistered")]
        [InlineButton ("ToggleMode", "Custom")]
        [InlineButton ("ClearKeys", "-")]
        public string eventName;

        [HideInInspector]
        public bool eventRegistered = true;

        #if !PB_MODSDK
        private Coroutine delayCoroutine = null;

        public void PlayDelayed (float delay)
        {
            if (delay <= 0f)
                Play ();
            else
            {
                Co.StopAndClear (ref delayCoroutine);
                delayCoroutine = Co.Delay (delay, Play);
            }
        }
        
        [Button ("►"), HorizontalGroup (30f), HideInEditorMode]
        public void Play ()
        {
            Co.StopAndClear (ref delayCoroutine);
            
            // If it's a freeform string, we can attempt to use it directly
            if (!eventRegistered)
            {
                if (!string.IsNullOrEmpty (eventName))
                    AudioEventSystem.PostEventImmediately(eventName);
                return;
            }

            // If type name string or field name string are empty or type collection doesn't contain a given type, we can't proceed
            if (string.IsNullOrEmpty (eventRegisteredType) || string.IsNullOrEmpty (eventRegisteredKey) || !eventTypes.ContainsKey (eventRegisteredType))
                return;
            
            // Get a dictionary keyed to field names with field values - if it doesn't contain our field name string, abort
            var pairs = FieldReflectionUtility.GetConstantStringFieldNamesValues (eventTypes[eventRegisteredType]);
            if (!pairs.ContainsKey (eventRegisteredKey))
                return;
            
            // Get the pair responsible for field of interest, and if fetched string is valid, pass it to audio utility
            var eventRegisteredName = pairs[eventRegisteredKey];
            if (!string.IsNullOrEmpty (eventRegisteredName))
                AudioEventSystem.PostEventImmediately(eventRegisteredName);
        }
        #endif

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

        // private void OnTypeChange () =>
        //     eventRegisteredKey = string.Empty;

        // Used for audio key dropdown
        private List<string> GetKeyNames ()
        {
            if (string.IsNullOrEmpty (eventRegisteredType) || !eventTypes.ContainsKey (eventRegisteredType))
                return null;
            return FieldReflectionUtility.GetConstantStringFieldNames (eventTypes[eventRegisteredType]);
        }

        private void ClearKeys ()
        {
            eventName = string.Empty;
            eventRegisteredKey = string.Empty;
            eventRegisteredType = string.Empty;
        }

        private void ToggleMode (string value) =>
            eventRegistered = !eventRegistered;

        #endif
    }
}
