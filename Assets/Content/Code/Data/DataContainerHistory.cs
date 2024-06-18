using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class KeyReplacementGroup
    {
        [TableList, ListDrawerSettings (DefaultExpandedState = false)]
        public List<KeyReplacement> keyReplacements = new List<KeyReplacement> ();

        [YamlIgnore, NonSerialized, HideInInspector]
        public HashSet<string> keyLookupFast = new HashSet<string> ();
    }
    
    [HideReferenceObjectPicker]
    public class KeyReplacement
    {
        [HideLabel]
        public string keyOld;
        
        [HideLabel]
        public string keyNew;
    }

    public class LocInvalidations
    {
        public HashSet<string> entryKeys;
        public HashSet<int> collectionIndexes;
    }
    
    [Serializable] 
    public class DataContainerHistory : DataContainerUnique
    {
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<string, KeyReplacementGroup> keyReplacementGroups = new Dictionary<string, KeyReplacementGroup> ();

        public Dictionary<string, LocInvalidations> locInvalidations = new Dictionary<string, LocInvalidations> ();
        
        public Dictionary<string, LocInvalidations> locInvalidationsSaved = new Dictionary<string, LocInvalidations> ();

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();
            
            // Compiling a fast lookup of all old keys per group, allowing us to bail without iterating over full list for some key upgrade attempts
            if (keyReplacementGroups != null)
            {
                foreach (var kvp in keyReplacementGroups)
                {
                    var group = kvp.Value;
                    if (group == null || group.keyReplacements == null || group.keyReplacements.Count == 0)
                        continue;

                    var replacements = group.keyReplacements;
                    
                    if (group.keyLookupFast == null)
                        group.keyLookupFast = new HashSet<string> ();
                    else
                        group.keyLookupFast.Clear ();

                    var lookup = group.keyLookupFast;
                    foreach (var replacement in replacements)
                    {
                        var keyOld = replacement.keyOld;
                        if (replacement == null || string.IsNullOrEmpty (keyOld) || lookup.Contains (keyOld))
                            continue;

                        lookup.Add (keyOld);
                    }
                }
            }
        }
    }
}

