using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerAssetPools : DataMultiLinker<DataContainerAssetPool>
    {
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool debug = false;
            
            [ShowInInspector, ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys"), ShowIf ("debug")]
            public static string debugKey = string.Empty;
        }
        
        [ShowInInspector][HideLabel]
        public Presentation presentation = new Presentation ();
        
        private static Transform holder;
        public static bool uncappedMode = false;

        public static Transform GetHolder ()
        {
            if (holder == null)
                holder = new GameObject ("Holder_AssetPool").transform;
            return holder;
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogLifetime ()
        {
            if (data == null)
                return;

            var dataSorted = new List<DataContainerAssetPool> ();
            foreach (var kvp in data)
            {
                var config = kvp.Value;
                if (config.lifetimeUsed)
                    dataSorted.Add (config);
            }
            
            dataSorted.Sort ((a, b) => a.lifetime.CompareTo (b.lifetime));
            Debug.Log (dataSorted.ToStringFormatted (true, toStringOverride: (config) => $"{config.key}: {config.lifetime:0.##}"));
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void LogLimits ()
        {
            if (data == null)
                return;

            var dataSorted = new List<DataContainerAssetPool> ();
            foreach (var kvp in data)
            {
                var config = kvp.Value;
                if (config.lifetimeUsed)
                    dataSorted.Add (config);
            }
            
            dataSorted.Sort ((a, b) => a.limit.CompareTo (b.limit));
            Debug.Log (dataSorted.ToStringFormatted (true, toStringOverride: (config) => $"{config.key}: {config.limit}"));
        }
    }
}


