using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [HideReferenceObjectPicker]
    public class DataBlockInputHint
    {
        [HideLabel, HorizontalGroup]
        [ValueDropdown ("@DataMultiLinkerInputAction.data.Keys")]
        public string action;

        [HideInInspector]
        public bool newline;
        
        #region Editor
        #if UNITY_EDITOR
        
        [HorizontalGroup (60f)]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue ()
        {
            newline = !newline;
        }
        
        private string GetBoolLabel => newline ? "Newline" : "Inline";
        private Color GetBoolColor => Color.HSVToRGB (0.55f, newline ? 0.5f : 0f, 1f);

        #endif
        #endregion
    }

    public class DataContainerInputHint : DataContainerWithText
    {
        public bool extended = true; 
        
        [ShowIf ("extended")]
        [YamlIgnore, HideLabel]
        public string textHeader;

        [ShowIf ("extended")]
        [YamlIgnore, HideLabel, Multiline (10)]
        public string textOverview;

        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockInputHintLine ()")]
        public List<DataBlockInputHintLine> lines = new List<DataBlockInputHintLine> ();

        public override void ResolveText ()
        {
            if (extended)
            {
                textHeader = DataManagerText.GetText (TextLibs.uiInputHints, $"{key}__header");
                textOverview = DataManagerText.GetText (TextLibs.uiInputHints, $"{key}__overview");
            }

            if (lines != null)
            {
                for (int i = 0; i < lines.Count; ++i)
                {
                    var line = lines[i];
                    if (line == null)
                        continue;
                    
                    line.text = DataManagerText.GetText (TextLibs.uiInputHints, $"{key}__s{i}");
                }
            }
        }
        
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (extended)
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.uiInputHints, $"{key}__header", textHeader);
                DataManagerText.TryAddingTextToLibrary (TextLibs.uiInputHints, $"{key}__overview", textOverview);
            }

            if (lines != null)
            {
                for (int i = 0; i < lines.Count; ++i)
                {
                    var line = lines[i];
                    if (line == null)
                        continue;
                    
                    if (!string.IsNullOrEmpty (line.text))
                        DataManagerText.TryAddingTextToLibrary (TextLibs.uiInputHints, $"{key}__s{i}", line.text);
                }
            }
        }

        #if !PB_MODSDK
        [Button, PropertyOrder (-1), HideInEditorMode]
        private void Apply ()
        {
            if (Application.isPlaying && CIViewPauseRoot.ins != null)
                CIViewPauseRoot.ins.RefreshInputText (key);
        }
        #endif

        #endif
    }
}