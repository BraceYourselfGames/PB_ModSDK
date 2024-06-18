using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitPerformanceClass : DataMultiLinker<DataContainerUnitPerformanceClass>
    {
        public DataMultiLinkerUnitPerformanceClass ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.unitPerformanceClasses); 
        }

        // [ShowInInspector, ReadOnly, FoldoutGroup ("Sorted Lists")]
        public static List<DataContainerUnitPerformanceClass> classesWeight = new List<DataContainerUnitPerformanceClass> ();
        
        // [ShowInInspector, ReadOnly, FoldoutGroup ("Sorted Lists")]
        public static List<DataContainerUnitPerformanceClass> classesSpeed = new List<DataContainerUnitPerformanceClass> ();
        
        // [ShowInInspector, ReadOnly, FoldoutGroup ("Sorted Lists")]
        public static List<DataContainerUnitPerformanceClass> classesDash = new List<DataContainerUnitPerformanceClass> ();
        
        public static void OnAfterDeserialization ()
        {
            classesWeight.Clear ();
            classesSpeed.Clear ();
            classesDash.Clear ();
            
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                
                if (string.Equals (c.group, UnitPerformanceGroups.weight) && c.valuesWeight != null)
                    classesWeight.Add (c);
                
                else if (string.Equals (c.group, UnitPerformanceGroups.speed))
                    classesSpeed.Add (c);
                
                else if (string.Equals (c.group, UnitPerformanceGroups.dash))
                    classesDash.Add (c);
            }
            
            classesWeight.Sort ((x, y) => x.threshold.CompareTo (y.threshold));
            classesSpeed.Sort ((x, y) => x.threshold.CompareTo (y.threshold));
            classesDash.Sort ((x, y) => x.threshold.CompareTo (y.threshold));

            for (int i = 0, count = classesWeight.Count; i < count; ++i)
                classesWeight[i].index = i;
                
            for (int i = 0, count = classesSpeed.Count; i < count; ++i)
                classesSpeed[i].index = i;
            
            for (int i = 0, count = classesDash.Count; i < count; ++i)
                classesDash[i].index = i;
        }

        public static DataContainerUnitPerformanceClass GetPerformanceClass (List<DataContainerUnitPerformanceClass> classesSorted, float value)
        {
            // Ensure full DB is loaded
            var classes = data; 
            
            DataContainerUnitPerformanceClass classSelected = null;
            for (int i = classesSorted.Count - 1; i >= 0; --i)
            {
                var classCandidate = classesSorted[i];
                if (value < classCandidate.threshold)
                    continue;

                classSelected = classCandidate;
                break;
            }

            if (classSelected == null)
                classSelected = classesSorted[0]; 

            return classSelected;
        }
    }
}


