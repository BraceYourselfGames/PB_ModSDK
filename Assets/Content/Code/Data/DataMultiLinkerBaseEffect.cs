using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerBaseEffect : DataMultiLinker<DataContainerBaseEffect>
    {
        public DataMultiLinkerBaseEffect ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.baseEffects); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
    
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showTagCollections = false;
            
            [ShowInInspector]
            public static bool showContexts = false;
        }
        
        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerBaseEffect.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerBaseEffect.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();

        // [ShowIf ("@DataMultiLinkerBaseEffect.Presentation.showContexts")]
        // [ShowInInspector, ReadOnly]
        public static Dictionary<string, List<DataContainerBaseEffect>> effectsByTag = new Dictionary<string, List<DataContainerBaseEffect>> ();
        
        [ShowIf ("@DataMultiLinkerBaseEffect.Presentation.showContexts")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, List<string>> effectsKeysByTag = new Dictionary<string, List<string>> ();
        
        [ShowIf ("@DataMultiLinkerBaseEffect.Presentation.showContexts")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, string> tagsWithLocText = new Dictionary<string, string> ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }
        
        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);

            effectsByTag.Clear ();
            effectsKeysByTag.Clear ();
            tagsWithLocText.Clear ();
            
            foreach (var kvp in data)
            {
                var value = kvp.Value;
                if (value == null)
                    continue;

                if (value.textSource != null)
                {
                    var tagWithLoc = value.textSource.tagAssociated;
                    if (!string.IsNullOrEmpty (tagWithLoc))
                        tagsWithLocText[tagWithLoc] = value.textSource.text;
                }
                
                if (value.IsHidden () || value.tags == null || value.tags.Count == 0)
                    continue;

                foreach (var tag in value.tags)
                {
                    if (string.IsNullOrEmpty (tag))
                        continue;
                    
                    if (!effectsByTag.TryGetValue (tag, out var list))
                    {
                        list = new List<DataContainerBaseEffect> ();
                        effectsByTag[tag] = list;
                    }
                
                    list.Add (value);
                }
            }

            foreach (var kvp in effectsByTag)
            {
                var list = kvp.Value;
                list.Sort ((x, y) => x.priority.CompareTo (y.priority));
                
                var listKeys = new List<string> ();
                effectsKeysByTag.Add (kvp.Key, listKeys);
                foreach (var value in list)
                    listKeys.Add (value.key);
            }
            
        }

        public static List<DataContainerBaseEffect> GetSortedEffectsWithTag (string tag)
        {
            LoadDataChecked ();
            return effectsByTag.TryGetValue (tag, out var v) ? v : null;
        }
    }
}


