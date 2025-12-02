using System;
using System.Collections.Generic;
using PhantomBrigade.Game;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
#endif

namespace PhantomBrigade.Data
{
    public enum CombatCommsVariantOrder
    {
        Random = 0,
        OrderedForward = 1,
        OrderedReverse = 2
    }
    
    [Serializable, LabelWidth (120f)]
    public class DataContainerCombatComms : DataContainerWithText
    {
        [DropdownReference, HideLabel, HideReferenceObjectPicker] 
        public DataBlockComment comment;
        
        [ValueDropdown ("@DataMultiLinkerCombatCommsSource.data.Keys")]
        public string source;

        [PropertyTooltip ("This message will attempt to find the unit who sent it and use unit pilot name if it can be found. The flag can't be used on its own and comms with it should be called via a targeted unit function.")]
        public bool sourceFromEntity = false;
        
        [PropertyTooltip ("If false, this message will not be shown in the middle of the screen and will only appear in the event log.")]
        public bool priority = true;
        
        [PropertyTooltip ("Disable the semitransparent layer letting the player read entire text immediately. Used in rare cases where text must reveal something midway.")]
        public bool previewDisabled = false;
        
        [PropertyTooltip ("The time this message takes to animate to full length. Each message stays around for additional fixed time once text animation is done (unless there's queued message immediately after). Set to 0 to default to (messageLength / 20).")]
        [ShowIf ("priority")]
        public float duration = 0f;
        
        [DropdownReference]
        [ShowIf ("priority")]
        public string audioKey;
        
        [YamlIgnore, ShowIf ("@!string.IsNullOrEmpty (audioKey)"), LabelText (" ")]
        public DataBlockAudioEvent audioTest;
        
        public CombatCommsVariantOrder order = CombatCommsVariantOrder.Random;

        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public List<string> textContent = new List<string> ();

        public override void OnBeforeSerialization ()
        {

        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void ResolveText ()
        {
            var textHeaderKey = $"cm_{key}_header";

            var prefix = $"cm_{key}";
            int prefixLength = prefix.Length;
            int candidateLengthExpected = prefixLength + 8;
            var textKeys = DataManagerText.GetLibraryTextKeys (TextLibs.scenarioComms);
            
            if (textContent == null)
                textContent = new List<string> ();
            else
                textContent.Clear ();

            foreach (var textKeyCandidate in textKeys)
            {
                if (!textKeyCandidate.StartsWith (prefix) || textKeyCandidate == textHeaderKey)
                    continue;
                
                // Confirm the string is the right length
                int candidateLength = textKeyCandidate.Length; 
                if (candidateLength != candidateLengthExpected)
                    continue;
                
                // Confirm the string has right format separator
                var elementSeparator = textKeyCandidate.Substring (prefixLength, 1);
                if (elementSeparator != "_")
                    continue;
                
                var textContentEntry = DataManagerText.GetText (TextLibs.scenarioComms, textKeyCandidate, true);
                textContent.Add (textContentEntry);
            }
        }

        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;
            
            if (textContent != null)
            {
                for (int i = 0; i < textContent.Count; ++i)
                {
                    var line = textContent[i];
                    if (!string.IsNullOrEmpty (line))
                        DataManagerText.TryAddingTextToLibrary (TextLibs.scenarioComms, $"cm_{key}_text_{i:D2}", line);
                }
            }
        }

        private void ResetSource ()
        {
            source = string.Empty;
            ResolveText ();
        }

        #if !PB_MODSDK
        [HideInEditorMode, Button, PropertyOrder (-1)]
        private void Test ()
        {
            if (Application.isPlaying)
                CIViewCombatComms.ScheduleMessage (key, 0f);
        }
        #endif
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerCombatComms () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion

        #endif
    }
}

