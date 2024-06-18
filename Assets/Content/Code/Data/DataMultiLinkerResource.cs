using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerResource : DataMultiLinker<DataContainerResource>
    {
        public DataMultiLinkerResource ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldResources); 
        }

        public struct ResourceConversionPath
        {
            public string resourceKey;
            public float multiplier;
        }

        [ShowInInspector, ReadOnly, ListDrawerSettings (DefaultExpandedState = false)]
        public static List<string> keysSorted = new List<string> ();
        public static List<DataContainerResource> dataSorted = new List<DataContainerResource> ();
        public static Dictionary<string, List<ResourceConversionPath>> statsToResources = new Dictionary<string, List<ResourceConversionPath>> ();

        public static void OnAfterDeserialization ()
        {
            keysSorted.Clear ();
            dataSorted.Clear ();
            statsToResources.Clear ();

            foreach (var kvp in data)
            {
                var resourceKey = kvp.Key;
                var resource = kvp.Value;
                
                if (resource.hidden)
                    continue;
                
                dataSorted.Add (resource);
                
                if (resource.statLinks == null || resource.statLinks.Count == 0)
                    continue;

                foreach (var kvp2 in resource.statLinks)
                {
                    var statKey = kvp2.Key;
                    var statData = DataMultiLinkerUnitStats.GetEntry (statKey);

                    if (statData == null)
                    {
                        Debug.LogWarning ($"Resource {resourceKey} defines conversion from nonexistent unit stat {statKey}");
                        continue;
                    }
                    
                    var block = kvp2.Value;
                    if (block == null || block.multiplier <= 0f)
                    {
                        Debug.LogWarning ($"Resource {resourceKey} defines invalid conversion from unit stat {statKey}: {block?.multiplier}");
                        continue;
                    }
                    
                    if (!statsToResources.ContainsKey (statKey))
                        statsToResources.Add (statKey, new List<ResourceConversionPath> ());
                    
                    var list = statsToResources[statKey];
                    var c = new ResourceConversionPath { resourceKey = resourceKey, multiplier = block.multiplier };
                    list.Add (c);
                }
            }
            
            dataSorted.Sort ((x, y) => x.priority.CompareTo (y.priority));
            foreach (var resource in dataSorted)
                keysSorted.Add (resource.key);
            
        }
    }
}


