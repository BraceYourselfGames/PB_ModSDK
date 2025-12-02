using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldProvinceModifier : DataMultiLinker<DataContainerOverworldProvinceModifier>
    {
        public DataMultiLinkerOverworldProvinceModifier ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldModifiers); 
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showTags = false;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerOverworldProvinceModifier.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldProvinceModifier.Presentation.showTagCollections")]
        [ShowInInspector] [ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }

        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
    }
}
