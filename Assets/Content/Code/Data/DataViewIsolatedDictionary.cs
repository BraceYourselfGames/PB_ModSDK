using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using System.Text;
#endif


namespace PhantomBrigade.Data
{
    [HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class DataViewIsolatedDictionary<T> where T : class, new ()
    {
        #if UNITY_EDITOR
        
        [PropertyOrder (-3)]
        [BoxGroup ("Header", false), HorizontalGroup ("Header/H"), VerticalGroup ("Header/H/V")]
        [YamlIgnore, ShowInInspector]
        [OnValueChanged (nameof (OnKeyModified))]
        [ValueDropdown (nameof (GetKeyList))]
        [Title ("$" + nameof(label), "$" + nameof(GetHeaderLabel), TitleAlignments.Split), HideLabel]
        [InlineButton (nameof (OnKeyDuplication), "Copy")]
        [InlineButton (nameof (OnKeyRemoval), "×")]
        private string key;

        [PropertyOrder (-2)]
        [BoxGroup ("Header", false), HorizontalGroup ("Header/H"), VerticalGroup ("Header/H/V")]
        [GUIColor ("$" + nameof (GetKeyReplacementColor))]
        [YamlIgnore, ShowInInspector]
        [HideLabel, SuffixLabel ("$" + nameof (GetKeyReplacementHint), true)]
        [InlineButton (nameof (OnKeyReplacement), "Rename", ShowIf = nameof(IsKeyReplacementPossible))]
        private string keyModified;
        
        [Title (" ")]
        [BoxGroup ("Header", false), GUIColor ("$" + nameof (GetEntryColor))]
        [YamlIgnore, ShowInInspector, HideReferenceObjectPicker, HideDuplicateReferenceBox, HideLabel]
        [OnValueChanged (nameof(OnChange), true)]
        private T valueIsolated
        {
            get
            {
                var dict = dictionaryResolver != null ? dictionaryResolver.Invoke () : null;
                if (dict == null || dict.Count == 0)
                    return null;

                if (string.IsNullOrEmpty (key) || !dict.TryGetValue (key, out var v))
                {
                    key = keyModified = dict.Keys.First ();
                    v = dict[key];
                }

                return v;
            }
            set
            {
                
            }
        }

        [PropertyOrder (-4)]
        [BoxGroup ("Header", false), HorizontalGroup ("Header/H", 36f)]
        [Button ("<", ButtonHeight = 72)]
        private void OnNavLeft ()
        {
            OnNav (false);
        }

        [PropertyOrder (-1)]
        [BoxGroup ("Header", false), HorizontalGroup ("Header/H", 36f)]
        [Button (">", ButtonHeight = 72)]
        private void OnNavRight ()
        {
            OnNav (true);
        }

        private void OnNav (bool forward)
        {
            var keys = GetKeyList ();
            if (keys == null)
                return;
            
            keyList.Clear ();
            keyList.AddRange (keys);
            
            int keyCount = keyList.Count;
            if (keyCount <= 1)
                return;

            int indexSelected = GetSelectedIndex ();
            if (indexSelected < 0)
                indexSelected = 0;

            indexSelected = indexSelected.OffsetAndWrap (forward, keyCount - 1);
            key = keyList[indexSelected];
            OnKeyModified ();
        }

        private IEnumerable<string> GetKeyList ()
        {
            if (keyListResolver != null)
                return keyListResolver.Invoke ();
            return null;
        }

        private void OnChange ()
        {
            if (onValueChange != null)
                onValueChange.Invoke ();
        }

        private void OnKeyModified ()
        {
            keyModified = key;
        }

        private int GetSelectedIndex ()
        {
            var dict = dictionaryResolver.Invoke ();
            if (dict == null)
                return -1;
            
            int iCurrent = -1;
            int i = 0;
            foreach (var kvp in dict)
            {
                if (string.Equals (kvp.Key, key))
                {
                    iCurrent = i;
                    break;
                }
                ++i;
            }
            
            return iCurrent;
        }

        private string GetSecondaryLabel ()
        {
            var dict = dictionaryResolver.Invoke ();
            if (dict == null)
                return "null";

            int iCurrent = GetSelectedIndex ();
            if (iCurrent < 0)
                return $"?/{dict.Count}";
            return $"{iCurrent + 1}/{dict.Count}";
        }
        
        private string GetHeaderLabel ()
        {
            var dict = dictionaryResolver.Invoke ();
            if (dict == null)
                return "null";
            
            sb.Clear ();
            
            int iCurrent = GetSelectedIndex ();
            sb.Append (iCurrent + 1);
            sb.Append ('/');
            sb.Append (dict.Count);
            sb.Append (' ');

            int i = 0;
            int iSelected = 0;
            foreach (var kvp in dict)
            {
                sb.Append (' ');
                if (string.Equals (kvp.Key, key))
                    sb.Append ('█');
                else
                    sb.Append ('░');
                i += 1;
            }
            
            return sb.ToString ();
        }

        private bool IsKeyReplacementPossible ()
        {
            if (string.IsNullOrWhiteSpace (keyModified))
                return false;

            if (string.Equals (key, keyModified, StringComparison.Ordinal))
                return false;
            
            var dict = dictionaryResolver.Invoke ();
            if (dict == null || dict.TryGetValue (keyModified, out var v))
                return false;
            
            return true;
        }

        private List<string> keyList = new List<string> ();
        private static Color colorFallback = new Color (1f, 1f, 1f, 1f);
        private static Color colorReplaceable = new Color (0.85f, 1f, 0.8f, 1f);
        private static Color colorMatched = new Color (1f, 1f, 1f, 0.5f);
        private static Color colorError = new Color (1f, 0.75f, 0.7f, 1f);

        private Color GetKeyReplacementColor ()
        {
            if (string.IsNullOrWhiteSpace (keyModified))
                return colorError;

            if (string.Equals (key, keyModified, StringComparison.Ordinal))
                return colorMatched;
            
            var dict = dictionaryResolver.Invoke ();
            if (dict == null || dict.TryGetValue (keyModified, out var v))
                return colorError;
            
            return colorReplaceable;
        }
        
        private Color GetEntryColor ()
        {
            var dict = dictionaryResolver.Invoke ();
            if (dict == null)
                return colorFallback;
            
            int iCurrent = -1;
            int i = 0;
            foreach (var kvp in dict)
            {
                if (string.Equals (kvp.Key, key))
                    iCurrent = i;
                ++i;
            }

            if (iCurrent < 0)
                return colorFallback;
            
            return DataEditor.GetColorFromElementIndexBright (iCurrent, 0.6f, 0.1f);
        }

        private const string strEmpty = "Can't be empty!";
        private const string strCollision = "Key already in use!";
        private const string strReplaceable = "Valid";
        
        private string GetKeyReplacementHint ()
        {
            if (string.IsNullOrWhiteSpace (keyModified))
                return strEmpty;

            if (string.Equals (key, keyModified, StringComparison.Ordinal))
                return string.Empty;
            
            var dict = dictionaryResolver.Invoke ();
            if (dict == null || dict.TryGetValue (keyModified, out var v))
                return strCollision;
            
            return strReplaceable;
        }
        
        private void OnKeyReplacement ()
        {
            if (!IsKeyReplacementPossible ())
                return;

            var dict = dictionaryResolver.Invoke ();
            if (dict == null || !dict.TryGetValue (key, out var v) || v == null)
                return;

            dict.Remove (key);
            dict.Add (keyModified, v);
            key = keyModified;
        }
        
        private void OnKeyRemoval ()
        {
            var dict = dictionaryResolver.Invoke ();
            if (dict == null || !dict.TryGetValue (key, out var v) || v == null)
                return;

            dict.Remove (key);
            key = keyModified = dict.Count > 0 ? dict.Keys.First () : null;
        }
        
        private void OnKeyDuplication ()
        {
            var dict = dictionaryResolver.Invoke ();
            if (dict == null || !dict.TryGetValue (key, out var v) || v == null)
                return;

            var keyNew = key;
            int i = 0;
            while (dict.ContainsKey (keyNew))
            {
                keyNew = $"{key}_{i:00}";
                ++i;

                if (i > 99)
                    return;
            }

            var copy = UtilitiesYAML.CloneThroughYaml (v);
            dict.Add (keyNew, copy);
            key = keyModified = keyNew;
        }

        private string label;
        private Func<IDictionary<string, T>> dictionaryResolver = null;
        private Func<IEnumerable<string>> keyListResolver = null;
        private System.Action onValueChange = null;
        private static StringBuilder sb = new StringBuilder ();

        public DataViewIsolatedDictionary (string label, Func<IDictionary<string, T>> dictionaryResolver, Func<IEnumerable<string>> keyListResolver, System.Action onValueChange = null)
        {
            this.label = label;
            this.dictionaryResolver = dictionaryResolver;
            this.keyListResolver = keyListResolver;
            this.onValueChange = onValueChange;
        }
        
        #endif
    }
}

