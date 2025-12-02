using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatBiome : DataMultiLinker<DataContainerCombatBiome>
    {
        public DataMultiLinkerCombatBiome ()
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

        [ShowIf ("@DataMultiLinkerCombatBiome.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerCombatBiome.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }
        
        /*
        [Button, PropertyOrder (-1)]
        public void FixHeight ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                c.slot1.blending = new Vector2 (0.5f, 0.5f);
                c.slot2.blending = new Vector2 (0.5f, 0.5f);
                c.slot3.blending = new Vector2 (0.5f, 0.5f);
                c.slot4.blending = new Vector2 (0.5f, 0.5f);
            }
        }
        */
    }
}

