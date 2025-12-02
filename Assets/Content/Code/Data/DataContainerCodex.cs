using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text;
using Content.Code.Utility;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Game;
using UnityEngine;
using UnityEngine.Video;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum CodexSectionType
    {
        Image,
        Video,
        TextHeader,
        TextSimple,
        TextExternalHeader,
        TextExternalSimple,
        TextOffset,
        TextQuote,
        TextControls
    }

    [TypeHinted]
    public interface ICodexSection
    {
        public CodexSectionType GetSectionType ();
        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height);
        public void OnBeforeSerialization (string key, int index);
        public void OnAfterDeserialization (string key, int index);
        
        #if UNITY_EDITOR
        public void SaveText (string key, int index);
        #endif
    }
    
    public class CodexSectionImage : ICodexSection
    {
        [ValueDropdown ("GetTextureKeys")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, group, 256)", false)]
        public string image;
        
        [ValueDropdown ("GetGroupKeys")]
        [InlineButton ("ReloadTextureGroup", "Reload")]
        public string group = TextureGroupKeys.OverworldEvents;

        public CodexSectionType GetSectionType () => CodexSectionType.Image;

        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK         
            
            var helper = go != null ? go.GetComponent<CIHelperCodexImage> () : null;
            if (helper == null)
                return;
            
            var imageTex = !string.IsNullOrEmpty (image) ? TextureManager.GetTexture (group, image) : null;
            if (imageTex != null)
            {
                helper.texture.gameObject.SetActive (true);
                helper.texture.mainTexture = imageTex;
                helper.texture.height = helper.widget.height = Mathf.RoundToInt (512f * ((float)imageTex.height / imageTex.width));
                height = helper.widget.height;
            }
            else
                helper.texture.gameObject.SetActive (false);

            height = helper.widget.height;
            
            #endif
        }
        
        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index) { }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index) { }

        private IEnumerable<string> GetGroupKeys () => FieldReflectionUtility.GetConstantStringFieldValues (typeof (TextureGroupKeys));
        private IEnumerable<string> GetTextureKeys () => TextureManager.GetExposedTextureKeys (group);

        private void ReloadTextureGroup ()
        {
            TextureManager.LoadGroup (group);
        }
        
        #endif
    }
    
    public class CodexSectionVideo : ICodexSection
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("UpdatePath")]
        [InlineButton ("UpdatePath", "Update path")]
        public VideoClip clip;
        
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;

        public CodexSectionType GetSectionType () => CodexSectionType.Video;

        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexVideo> () : null;
            if (helper == null)
                return;
            
            if (clip == null && !string.IsNullOrEmpty (path))
                clip = Resources.Load<VideoClip> (path);

            if (clip == null)
                return;

            var vp = CIViewCodex.ins.contentVideoPlayer;
            vp.enabled = true;
            vp.clip = clip;
            vp.isLooping = true;
            vp.Play ();

            CIViewCodex.ins.ApplyVideoClip (clip);

            if (vp.targetTexture == null)
                return;

            helper.texture.mainTexture = vp.targetTexture;
            helper.texture.height = helper.widget.height = vp.targetTexture.height; // Mathf.RoundToInt (512f * ((float)clip.height / clip.width));
            height = helper.widget.height;
            
            #endif
        }

        public void OnBeforeSerialization (string key, int index)
        {
            #if UNITY_EDITOR
            UpdatePath ();
            #endif
        }

        public void OnAfterDeserialization (string key, int index)
        {
            #if !PB_MODSDK
            clip = !string.IsNullOrEmpty (path) ? Resources.Load<VideoClip> (path) : null;
            if (clip == null)
            {
                Debug.LogWarning ($"Failed to load video clip from path [{path}] for codex config {key} index {index}");
                return;
            }
            #endif
        }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index) { }
        
        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);
        
        public void UpdatePath ()
        {
            #if !PB_MODSDK
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
        
        #endif
    }

    public class CodexSectionTextSimple : ICodexSection
    {
        [HideLabel]
        public Color color = new Color (1f, 1f, 1f, 1f);
        
        [YamlIgnore, TextArea (2, 10)]
        public string text;

        public CodexSectionType GetSectionType () => CodexSectionType.TextSimple;

        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextSimple> () : null;
            if (helper == null)
                return;

            helper.label.color = color.WithAlpha (1f);
            helper.label.text = text;
            helper.label.gameObject.SetActive (false);
            helper.label.gameObject.SetActive (true);
            helper.widget.height = helper.label.height + 32;
            height = helper.widget.height;
            
            #endif
        }
        
        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index)
        {
            text = DataManagerText.GetText (TextLibs.codex, $"{key}__s{index}_ts_text");
        }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index)
        {
            DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__s{index}_ts_text", text);
        }
        
        #endif
    }
    
    public class CodexSectionTextExternal
    {
        [PropertyOrder (-1)]
        [YamlIgnore, ShowInInspector, DisplayAsString (true), ReadOnly, BoxGroup, HideLabel]
        public string text => DataManagerText.GetText (textSector, textKey, true);
        
        [ValueDropdown ("@DataManagerText.GetLibrarySectorKeys ()")]
        public string textSector = "ui_base";
        
        [ValueDropdown ("@DataManagerText.GetLibraryTextKeys (textSector)")]
        public string textKey;

        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index) { }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index) { }
        
        #endif
    }
    
    public class CodexSectionTextExternalSimple : CodexSectionTextExternal, ICodexSection
    {
        [HideLabel]
        public Color color = new Color (1f, 1f, 1f, 1f);
        
        public CodexSectionType GetSectionType () => CodexSectionType.TextExternalSimple;
        
        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextSimple> () : null;
            if (helper == null)
                return;

            helper.label.color = color.WithAlpha (1f);
            helper.label.text = text;
            helper.label.gameObject.SetActive (false);
            helper.label.gameObject.SetActive (true);
            helper.widget.height = helper.label.height + 32;
            height = helper.widget.height;
            
            #endif
        }
    }
    
    public class CodexSectionTextExternalHeader : CodexSectionTextExternal, ICodexSection
    {
        [HideLabel]
        public Color color = new Color (1f, 1f, 1f, 1f);
        
        [DataEditor.SpriteNameAttribute (true, 32f)]
        [InlineButtonClear]
        public string icon;
        
        public CodexSectionType GetSectionType () => CodexSectionType.TextExternalHeader;
        
        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextHeader> () : null;
            if (helper == null)
                return;

            helper.label.color = color.WithAlpha (1f);
            helper.label.text = text;
            helper.label.gameObject.SetActive (false);
            helper.label.gameObject.SetActive (true);
            helper.widget.height = helper.label.height + 32;
            height = helper.widget.height;
            
            bool iconPresent = !string.IsNullOrEmpty (icon);
            helper.spriteIcon.gameObject.SetActive (iconPresent);

            if (iconPresent)
            {
                helper.spriteIcon.color = color;
                helper.spriteIcon.spriteName = icon;
                helper.label.transform.SetPositionLocalX (helper.labelOffsets.x);
                helper.label.width = Mathf.RoundToInt (helper.labelWidths.x);
            }
            else
            {
                helper.label.transform.SetPositionLocalX (helper.labelOffsets.y);
                helper.label.width = Mathf.RoundToInt (helper.labelWidths.y);
            }

            #endif
        }
    }
    
    public class CodexSectionTextHeader : ICodexSection
    {
        [HideLabel]
        public Color color = new Color (1f, 1f, 1f, 1f);
        
        [DataEditor.SpriteNameAttribute (true, 32f)]
        [InlineButtonClear]
        public string icon;
        
        [YamlIgnore]
        public string text;

        public CodexSectionType GetSectionType () => CodexSectionType.TextHeader;

        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextHeader> () : null;
            if (helper == null)
                return;

            helper.label.color = color.WithAlpha (1f);
            helper.label.text = text;
            helper.label.gameObject.SetActive (false);
            helper.label.gameObject.SetActive (true);
            helper.widget.height = helper.label.height + 32;
            height = helper.widget.height;
            
            bool iconPresent = !string.IsNullOrEmpty (icon);
            helper.spriteIcon.gameObject.SetActive (iconPresent);

            if (iconPresent)
            {
                helper.spriteIcon.color = color;
                helper.spriteIcon.spriteName = icon;
                helper.label.transform.SetPositionLocalX (helper.labelOffsets.x);
                helper.label.width = Mathf.RoundToInt (helper.labelWidths.x);
            }
            else
            {
                helper.label.transform.SetPositionLocalX (helper.labelOffsets.y);
                helper.label.width = Mathf.RoundToInt (helper.labelWidths.y);
            }

            #endif
        }
        
        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index)
        {
            text = DataManagerText.GetText (TextLibs.codex, $"{key}__s{index}_th_header");
        }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index)
        {
            DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__s{index}_th_header", text);
        }
        
        #endif
    }
    
    public class CodexSectionTextQuote : ICodexSection
    {
        [YamlIgnore, TextArea (2, 10)]
        public string textPrimary;
        
        [YamlIgnore]
        public string textSubtitle;

        public CodexSectionType GetSectionType () => CodexSectionType.TextQuote;

        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextQuote> () : null;
            if (helper == null)
                return;

            helper.labelSubtitle.text = $"— {textSubtitle}";
            helper.labelPrimary.text = $"[i]{textPrimary}[/i]";
            helper.labelPrimary.gameObject.SetActive (false);
            helper.labelPrimary.gameObject.SetActive (true);
            helper.widget.height = helper.labelPrimary.height + 58;
            height = helper.widget.height;

            var hue = content != null ? content.hue : 0.388f;
            helper.tintControl.OverrideHue (hue);

            #endif
        }
        
        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index)
        {
            textPrimary = DataManagerText.GetText (TextLibs.codex, $"{key}__s{index}_tq_quote");
            textSubtitle = DataManagerText.GetText (TextLibs.codex, $"{key}__s{index}_tq_subtitle");
        }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index)
        {
            DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__s{index}_tq_quote", textPrimary);
            DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__s{index}_tq_subtitle", textSubtitle);
        }
        
        #endif
    }
    
    public class CodexSectionTextOffset : ICodexSection
    {
        [YamlIgnore, TextArea (2, 10)]
        public string text;

        public CodexSectionType GetSectionType () => CodexSectionType.TextOffset;

        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextOffset> () : null;
            if (helper == null)
                return;
            
            helper.label.text = text;
            helper.label.gameObject.SetActive (false);
            helper.label.gameObject.SetActive (true);
            helper.widget.height = helper.label.height + 32;
            height = helper.widget.height;
            
            var hue = content != null ? content.hue : 0.388f;
            helper.tintControl.OverrideHue (hue);

            #endif
        }
        
        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index)
        {
            text = DataManagerText.GetText (TextLibs.codex, $"{key}__s{index}_to_text");
        }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index)
        {
            DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__s{index}_to_text", text);
        }
        
        #endif
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockInputHintAction
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

    public class DataBlockInputHintLine
    {
        public InputHintMode mode = InputHintMode.All;
        public string text;
            
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockInputHintAction ()")]
        public List<DataBlockInputHintAction> actions = new List<DataBlockInputHintAction> ();
    }

    public class CodexSectionTextControls : ICodexSection
    {
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockInputHintLine ()")]
        public List<DataBlockInputHintLine> lines = new List<DataBlockInputHintLine> ();
        
        private static StringBuilder sb1 = new StringBuilder ();
        private static StringBuilder sb2 = new StringBuilder ();
        
        public CodexSectionType GetSectionType () => CodexSectionType.TextControls;
        
        public void ApplyToObject (GameObject go, DataContainerCodex content, out int height)
        {
            height = 0;
            #if !PB_MODSDK
            
            var helper = go != null ? go.GetComponent<CIHelperCodexTextControls> () : null;
            if (helper == null)
                return;

            sb1.Clear ();
            sb2.Clear ();

            bool gamepadMode = InputHelper.gamepad;
            var mode = InputHelper.gamepad ? InputHintMode.Controller : InputHintMode.KeyboardMouse;

            if (lines != null)
            {
                bool linesStarted = false;
                foreach (var line in lines)
                {
                    if (line == null || string.IsNullOrEmpty (line.text) || line.actions == null || line.actions.Count == 0)
                        continue;
                    
                    if (line.mode != InputHintMode.All && !mode.HasFlag (line.mode))
                        continue;
                    
                    bool lineUsed = false;
                    foreach (var action in line.actions)
                    {
                        if (action == null || string.IsNullOrEmpty (action.action))
                            continue;
                        
                        var insert = SettingUtility.GetValueTextForInputAction (action.action, true, mode, false);
                        if (string.IsNullOrEmpty (insert))
                            continue;
                        
                        if (!lineUsed)
                        {
                            lineUsed = true;
                            if (linesStarted)
                            {
                                sb1.Append ("\n");
                                sb2.Append ("\n");
                            }
                            else
                                linesStarted = true;

                            sb1.Append (line.text);
                        }

                        if (action.newline)
                        {
                            sb1.Append ("\n");
                            sb2.Append ("\n");
                        }
                        
                        sb2.Append (insert);
                    }
                }
            }
            
            helper.labelNames.text = sb1.ToString ();
            helper.labelValues.text = sb2.ToString ();

            helper.labelNames.gameObject.SetActive (false);
            helper.labelNames.gameObject.SetActive (true);
            helper.widget.height = helper.labelNames.height + 32;
            height = helper.widget.height;

            #endif
        }
        
        public void OnBeforeSerialization (string key, int index) { }
        
        public void OnAfterDeserialization (string key, int index)
        {
            if (lines != null)
            {
                for (int i = 0; i < lines.Count; ++i)
                {
                    var line = lines[i];
                    if (line == null)
                        continue;
                    
                    line.text = DataManagerText.GetText (TextLibs.codex, $"{key}__s{index}_input{i}");
                }
            }
            
            
        }
        
        #if UNITY_EDITOR
        
        public void SaveText (string key, int index)
        {
            if (lines != null)
            {
                for (int i = 0; i < lines.Count; ++i)
                {
                    var line = lines[i];
                    if (line == null)
                        continue;
                    
                    if (!string.IsNullOrEmpty (line.text))
                        DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__s{index}_input{i}", line.text);
                }
            }
        }
        
        #endif
    }
    
    
    
    public class DataContainerCodex : DataContainerWithText
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment noteLoc;
        
        [OnInspectorGUI ("DrawHeaderGUI", false)]
        [PropertyRange (0f, 1f)]
        public float hue = 0.388f;

        public int priority;
        
        [ValueDropdown("@DataMultiLinkerCodex.GetKeys ()")]
        public string parent;

        public bool listed = true;
        public bool unlockable = false;

        [ShowIf ("unlockable")]
        public bool unlockOnQuickStart = true;
        
        [YamlIgnore]
        public string textTitle;

        [OnValueChanged("OnChange", true)]
        public List<ICodexSection> sections;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (sections != null)
            {
                for (int i = 0; i < sections.Count; ++i)
                {
                    var section = sections[i];
                    if (section != null)
                        section.OnAfterDeserialization (key, i);
                }
            }
        }

        public override void ResolveText ()
        {
            textTitle = DataManagerText.GetText (TextLibs.codex, $"{key}__a_title");

            if (sections != null)
            {
                for (int i = 0; i < sections.Count; ++i)
                {
                    var section = sections[i];
                    if (section != null)
                        section.OnAfterDeserialization (key, i);
                }
            }
        }

        private void OnChange ()
        {
            #if UNITY_EDITOR && !PB_MODSDK
            if (Application.isPlaying && DataMultiLinkerCodex.Presentation.redrawOnChanges)
            {
                RefreshNav ();
                Display ();
            }
            #endif
        }

        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.codex, $"{key}__a_title", textTitle, noteLoc?.comment);
            
            if (sections != null)
            {
                for (int i = 0; i < sections.Count; ++i)
                {
                    var section = sections[i];
                    if (section != null)
                        section.SaveText (key, i);
                }
            }
        }
        
        private void DrawHeaderGUI ()
        {
            var rect = UnityEditor.EditorGUILayout.BeginVertical ();
            GUILayout.Label (" ", GUILayout.Height (12));
            UnityEditor.EditorGUILayout.EndVertical ();

            var gc = GUI.color;
            GUI.color = new HSBColor (hue, 0.5f, 1f).ToColor ();
            GUI.DrawTexture (rect, Texture2D.whiteTexture);
            GUI.color = gc;
        }

        #if !PB_MODSDK
        [Button, ButtonGroup, HideInEditorMode, PropertyOrder (-10)]
        private void Display ()
        {
            CIViewCodex.ins.RefreshContent (this);
        }
        
        [Button, ButtonGroup, HideInEditorMode, PropertyOrder (-10)]
        private void RefreshNav ()
        {
            DataMultiLinkerCodex.OnAfterDeserialization ();
            CIViewCodex.ins.RefreshNav ();
        }
        
        [Button, ButtonGroup, HideInEditorMode, PropertyOrder (-10)]
        private void MarkNew ()
        {
            var overworld = Contexts.sharedInstance.overworld;
            var keysNew = overworld.hasCodexKeysNew ? overworld.codexKeysNew.s : new HashSet<string> ();
            if (!keysNew.Contains (key))
                keysNew.Add (key);
            overworld.ReplaceCodexKeysNew (keysNew);
            
            CIViewCodex.ins.RefreshNav ();
            CIViewOverworldNav.ins.RefreshCodex ();
            CIViewCombatEventLog.ins.RefreshCodex ();
        }
        
        [ShowIf ("unlockable")]
        [Button, ButtonGroup, HideInEditorMode, PropertyOrder (-10)]
        private void Unlock ()
        {
            CodexUtility.TryUnlockEntry (key);
        }
        #endif
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerCodex () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }

    #if !PB_MODSDK
    public static class CodexUtility
    {
        private static StringBuilder sb = new StringBuilder ();
        
        public static void TryUnlockEntry (string key, bool notificationUsed = true)
        {
            if (string.IsNullOrEmpty (key))
                return;

            var entry = DataMultiLinkerCodex.GetEntry (key);
            if (entry == null || !entry.unlockable)
                return;
            
            var overworld = Contexts.sharedInstance.overworld;
            var keysUnlocked = overworld.hasCodexKeysUnlocked ? overworld.codexKeysUnlocked.s : null;
            if (keysUnlocked != null && keysUnlocked.Contains (key))
                return;

            if (keysUnlocked == null)
                keysUnlocked = new HashSet<string> { key };
            else
                keysUnlocked.Add (key);
            overworld.ReplaceCodexKeysUnlocked (keysUnlocked);

            var keysNew = overworld.hasCodexKeysNew ? overworld.codexKeysNew.s : null;
            if (keysNew == null)
                keysNew = new HashSet<string> { key };
            else if (!keysNew.Contains (key))
                keysNew.Add (key);
            overworld.ReplaceCodexKeysNew (keysNew);
            Debug.Log ($"Unlocked codex article: {key}");
            
            if (notificationUsed && !CIViewTutorial.ins.IsEntered ())
            {
                sb.Clear ();
                sb.Append (Txt.Get (TextLibs.uiOverworld, "codex_unlock_notification"));
                sb.Append (": ");
                sb.Append (entry.textTitle);
                CIViewOverworldLog.AddMessage (sb.ToString (), DataKeysEventColor.Restorative);

                AudioUtility.CreateAudioEvent (AudioEventUIOverworld.CodexEntryUnlocked);
            }
        }
        
        public static void TryUnlockEntries (HashSet<string> keys)
        {
            if (keys == null || keys.Count == 0)
                return;

            foreach (var key in keys)
            {
                TryUnlockEntry (key, true);
            }
        }
    }
    #endif
}

