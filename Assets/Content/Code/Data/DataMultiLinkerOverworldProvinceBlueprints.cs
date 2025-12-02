using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldProvinceBlueprints : DataMultiLinker<DataContainerOverworldProvinceBlueprint>
    {
        public DataMultiLinkerOverworldProvinceBlueprints ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldProvinces); 
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool testSpawnGeneral = false;
            
            [ShowInInspector, ShowIf (nameof(testSpawnGeneral))]
            public static Vector3Int testSpawnInputs = new Vector3Int (75, 150, 5);

            [ShowInInspector, ShowIf (nameof (testSpawnGeneral))]
            public static PointDistancePriority testSpawnPriority = PointDistancePriority.None;
            
            [ShowInInspector]
            public static bool showTags = false;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [HideLabel, BoxGroup ("Selected"), PropertyOrder (90)]
        public static DataContainerOverworldProvinceBlueprint selection;

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerOverworldProvinceBlueprints.Presentation.showTagCollections")]
        public static HashSet<string> spawnGroupKeys;
        
        [ShowIf ("@DataMultiLinkerOverworldProvinceBlueprints.Presentation.showTagCollections")]
        [ShowInInspector]
        public static HashSet<string> tags = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldProvinceBlueprints.Presentation.showTagCollections")]
        [ShowInInspector] [ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }

        public static void OnAfterDeserialization ()
        {
            if (spawnGroupKeys == null)
                spawnGroupKeys = new HashSet<string> ();
            else
                spawnGroupKeys.Clear ();
            
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
        }
    }
}
