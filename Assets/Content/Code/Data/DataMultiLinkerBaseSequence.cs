using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerBaseSequence : DataMultiLinker<DataContainerBaseSequence>
    {
        public DataMultiLinkerBaseSequence ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
    
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showTagCollections = false;
        }
        
        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerBaseSequence.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerBaseSequence.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static Dictionary<string, List<DataContainerBaseSequence>> sequencesByTag = new Dictionary<string, List<DataContainerBaseSequence>> ();
        
        [ShowIf ("@DataMultiLinkerBaseEffect.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, List<string>> sequenceKeysByTag = new Dictionary<string, List<string>> ();

        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);

        
            sequencesByTag.Clear ();
            sequenceKeysByTag.Clear ();
            
            foreach (var kvp in data)
            {
                var value = kvp.Value;
                if (value == null || value.IsHidden () || value.tags == null || value.tags.Count == 0)
                    continue;
                
                if (value.steps == null || value.steps.Count == 0)
                    continue;

                foreach (var tag in value.tags)
                {
                    if (string.IsNullOrEmpty (tag))
                        continue;
                        
                    if (!sequencesByTag.TryGetValue (tag, out var list))
                    {
                        list = new List<DataContainerBaseSequence> ();
                        sequencesByTag[tag] = list;
                    }
                    
                    list.Add (value);
                }
                    
            }

            foreach (var kvp in sequencesByTag)
            {
                var list = kvp.Value;
                // list.Sort ((x, y) => x.priority.CompareTo (y.priority));
                        
                var listKeys = new List<string> ();
                sequenceKeysByTag.Add (kvp.Key, listKeys);
                foreach (var value in list)
                    listKeys.Add (value.key);
            }
        }
        
        public static List<DataContainerBaseSequence> GetSequencesWithTag (string tag)
        {
            LoadDataChecked ();
            return sequencesByTag.TryGetValue (tag, out var v) ? v : null;
        }
    }
}


