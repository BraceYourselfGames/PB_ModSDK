using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockCutsceneSubtitle
    {
        public float time;
        public float duration;

        public bool animated;
        public bool commsAudioUsed;
    
        [TextArea, YamlIgnore] 
        public string textContent = string.Empty;
        
        #region Editor
        #if UNITY_EDITOR

        #if !PB_MODSDK
        [HideInEditorMode]
        [Button ("Test"), PropertyOrder (-1)]
        public void Test ()
        {
            CIViewSubtitleBottom.PushSubtitle (duration, textContent, animated);
        }
        #endif

        #endif
        #endregion
    }
    
    [Serializable][HideReferenceObjectPicker][LabelWidth (160f)]
    public class DataContainerCutsceneVideo : DataContainerWithText
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        public VideoClip clip;
        
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;

        public Vector2 audioFadeOnStart = new Vector2 (0f, 0f);
        
        [ListDrawerSettings (DefaultExpandedState = false, AlwaysAddDefaultValue = true)]
        public List<DataBlockCutsceneSubtitle> subtitles;

        public override void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR && !PB_MODSDK
            if (clip == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (clip);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring (0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        
        
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            #if !PB_MODSDK
            clip = !string.IsNullOrEmpty (path) ? Resources.Load<VideoClip> (path) : null;
            if (clip == null)
            {
                Debug.LogWarning ($"Failed to load video clip from path [{path}] for video cutscene config {key}");
                return;
            }
            #endif
        }

        public override void ResolveText ()
        {
            if (subtitles != null)
            {
                for (int i = 0; i < subtitles.Count; ++i)
                {
                    var subtitle = subtitles[i];
                    if (subtitle != null)
                        subtitle.textContent = DataManagerText.GetText (TextLibs.cutscenesVideo, $"{key}__sub_{i:D2}");
                }
            }
        }

        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (subtitles != null)
            {
                for (int i = 0; i < subtitles.Count; ++i)
                {
                    var subtitle = subtitles[i];
                    if (subtitle != null)
                        DataManagerText.TryAddingTextToLibrary (TextLibs.cutscenesVideo, $"{key}__sub_{i:D2}", subtitle.textContent);
                }
            }
        }

        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);

        #if !PB_MODSDK
        [HideInEditorMode, Button, PropertyOrder (-1)]
        private void Test ()
        {
            if (!Application.isPlaying)
                return;
            
            CutsceneService.TryPlayingCutsceneVideo (key);
        }
        #endif

        #endif
    }
}

