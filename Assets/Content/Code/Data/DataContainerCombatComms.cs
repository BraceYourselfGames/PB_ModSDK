using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
#endif

namespace PhantomBrigade.Data
{
    [Serializable, LabelWidth (120f)]
    public class DataContainerCombatComms : DataContainerWithText
    {
        [ValueDropdown ("@DataMultiLinkerCombatCommsSource.data.Keys")]
        public string source;
        
        public bool priority = true;
        
        [ShowIf ("priority")]
        public float duration = 1f;
        
        [ShowIf ("priority")]
        [HorizontalGroup ("audio")]
        public bool audioUsed = false;
        
        [HorizontalGroup ("audio")]
        [ShowIf ("@priority && audioUsed"), HideLabel]
        public string audioKey;

        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public List<string> textContent;

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
            textContent = new List<string> ();

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

        #endif
    }
}

