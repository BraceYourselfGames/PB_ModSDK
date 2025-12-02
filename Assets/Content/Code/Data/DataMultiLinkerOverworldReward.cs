using System.Collections.Generic;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldReward : DataMultiLinker<DataContainerOverworldReward>
    {
        public DataMultiLinkerOverworldReward ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldRewards);
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [FoldoutGroup ("View options", false), ShowInInspector, HideLabel]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerOverworldReward.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldReward.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
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


