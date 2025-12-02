using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotStat : DataMultiLinker<DataContainerPilotStat>
    {
        public DataMultiLinkerPilotStat ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotStats); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            // [ShowInInspector]
            // public static bool showTagCollections = false;
        }
        
        // [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        // public Presentation presentation = new Presentation ();

        public static List<string> keysSorted = new List<string> ();
        
        public static void OnAfterDeserialization ()
        {
            keysSorted.Clear ();
            if (data != null)
            {
                var list = new List<DataContainerPilotStat> ();
                foreach (var kvp in data)
                    list.Add (kvp.Value);
                list.Sort ((x, y) => x.priority.CompareTo (y.priority));
                foreach (var entry in list)
                    keysSorted.Add (entry.key);
            }
        }
    }
}


