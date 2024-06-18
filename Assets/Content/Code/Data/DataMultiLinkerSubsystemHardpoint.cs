using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerSubsystemHardpoint : DataMultiLinker<DataContainerSubsystemHardpoint>
    {
        public DataMultiLinkerSubsystemHardpoint ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.equipmentHardpoints); 
        }
    
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCore = true;
            
            [ShowInInspector]
            public static bool showTags = true;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
        }
        
        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerSubsystemHardpoint.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> groups = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerSubsystemHardpoint.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> groupsMap = new Dictionary<string, HashSet<string>> ();
        
        [ShowIf ("@DataMultiLinkerSubsystemHardpoint.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerSubsystemHardpoint.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        private static List<string> groupsTempSorted = new List<string> ();
        
        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);

            if (groups == null)
                groups = new HashSet<string> ();
            else
                groups.Clear ();
            
            if (groupsMap == null)
                groupsMap = new Dictionary<string, HashSet<string>> ();
            else
                groupsMap.Clear ();
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;
                
                if (string.IsNullOrEmpty (container.visualGroup))
                    continue;

                if (!groups.Contains (container.visualGroup))
                    groups.Add (container.visualGroup);
                    
                if (!groupsMap.ContainsKey (container.visualGroup))
                    groupsMap.Add (container.visualGroup, new HashSet<string> ());

                var map = groupsMap[container.visualGroup];
                if (!map.Contains (key))
                    map.Add (key);
            }
            
            groupsTempSorted.Clear ();
            groupsTempSorted.AddRange (groups);
            groupsTempSorted.Sort ();
            
            groups.Clear ();
            foreach (var group in groupsTempSorted)
                groups.Add (group);
        }
    }
}


