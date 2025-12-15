using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerEquipmentLivery : DataMultiLinker<DataContainerEquipmentLivery>
    {
        public DataMultiLinkerEquipmentLivery ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        public struct ResourceConversionPath
        {
            public string resourceKey;
            public float multiplier;
        }

        public static List<DataContainerEquipmentLivery> dataSorted = new List<DataContainerEquipmentLivery> ();

        public static void OnAfterDeserialization ()
        {
            dataSorted.Clear ();

            foreach (var kvp in data)
            {
                var livery = kvp.Value;
                if (livery.hidden)
                    continue;
                
                dataSorted.Add (livery);
            }
            
            dataSorted.Sort (CompareLiveryForSorting);
        }
        
        private static int CompareLiveryForSorting (DataContainerEquipmentLivery s1, DataContainerEquipmentLivery s2)
        {
            if (s1 == null)
            {
                if (s2 == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (s2 == null)
                    return 1;
                else
                {
                    var priority1 = s1.priority;
                    var priority2 = s2.priority;
                    var priorityComp = priority1.CompareTo (priority2);
                    if (priorityComp != 0)
                        return priorityComp;

                    var key1 = s1.key;
                    var key2 = s2.key;
                    var keyComp = string.Compare (key1, key2, StringComparison.InvariantCultureIgnoreCase);
                    return keyComp;
                }
            }
        }
    }
}
