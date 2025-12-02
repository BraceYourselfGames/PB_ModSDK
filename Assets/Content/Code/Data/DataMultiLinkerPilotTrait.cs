using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotTrait : DataMultiLinker<DataContainerPilotTrait>
    {
        public DataMultiLinkerPilotTrait ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotTraits); 
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

        [ShowIf ("@DataMultiLinkerPilotTrait.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerPilotTrait.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        [ShowIf ("@DataMultiLinkerPilotTrait.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static SortedDictionary<int, List<string>> levelMap = new SortedDictionary<int, List<string>> ();

        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }
        
        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);

            levelMap.Clear ();
            foreach (var kvp in data)
            {
                var trait = kvp.Value;
                if (trait == null)
                    continue;

                int level = Mathf.Max (trait.level, 0);
                if (levelMap.ContainsKey (level))
                    levelMap[level].Add (trait.key);
                else
                    levelMap.Add (level, new List<string> { trait.key });
            }
        }

        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        private void ConvertToIncompatible ()
        {
            var tagsToRemove = new HashSet<string> ();
            foreach (var kvp in data)
            {
                var trait = kvp.Value;
                if (trait == null || trait.tags == null)
                    continue;

                foreach (var tag in trait.tags)
                {
                    if (tag.StartsWith ("type_") && tag.EndsWith ("_incompatible"))
                    {
                        tagsToRemove.Add (tag);
                        var typeKey = tag.TrimStart ("type_").TrimEnd ("_incompatible");

                        if (trait.pilotTypesIncompatible == null)
                            trait.pilotTypesIncompatible = new HashSet<string> ();
                        trait.pilotTypesIncompatible.Add (typeKey);
                        
                        Debug.Log ($"{trait.key}: incompatible with {typeKey}");
                    }
                }

                if (tagsToRemove.Count > 0)
                {
                    foreach (var tag in tagsToRemove)
                        trait.tags.Remove (tag);
                }
            }
        }
    }
}


