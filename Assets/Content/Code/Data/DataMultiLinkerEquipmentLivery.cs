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
            
            dataSorted.Sort ((x, y) => x.priority.CompareTo (y.priority));
        }
    }
}
