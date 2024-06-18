using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerBaseStatGroup : DataMultiLinker<DataContainerBaseStatGroup>
    {
        public DataMultiLinkerBaseStatGroup ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            
            textSectorKeys = new List<string> { TextLibs.baseStatsGroups };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.baseStatsGroups),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.baseStatsGroups)
            );
        }

        public static SortedDictionary<string, List<DataContainerBaseStat>> dataSortedByGroup = new SortedDictionary<string, List<DataContainerBaseStat>> ();

        public static void OnAfterDeserialization ()
        {
            dataSortedByGroup.Clear ();

            var statData = DataMultiLinkerBaseStat.data;
            
            foreach (var kvp in data)
            {
                var groupKey = kvp.Key;
                var group = kvp.Value;

                foreach (var kvp2 in statData)
                {
                    var statKey = kvp2.Key;
                    var stat = kvp2.Value;
                    
                    if (string.IsNullOrEmpty (stat.group) || stat.group != groupKey)
                        continue;

                    if (!dataSortedByGroup.ContainsKey (groupKey))
                        dataSortedByGroup.Add (groupKey, new List<DataContainerBaseStat> { stat });
                    else
                        dataSortedByGroup[groupKey].Add (stat);
                }
            }

            var keyOther = "z_other";
            var groupOther = GetEntry (keyOther);
            if (groupOther != null)
            {
                foreach (var kvp in statData)
                {
                    var stat = kvp.Value;
                    bool groupEntered = !string.IsNullOrEmpty (stat.group);
                    if (groupEntered)
                        continue;
                    
                    if (!dataSortedByGroup.ContainsKey (keyOther))
                        dataSortedByGroup.Add (keyOther, new List<DataContainerBaseStat> { stat });
                    else
                        dataSortedByGroup[keyOther].Add (stat);
                }
            }

            foreach (var kvp in dataSortedByGroup)
            {
                var list = kvp.Value;
                if (list.Count > 1)
                    list.Sort ((x, y) => x.priority.CompareTo (y.priority));

                var group = GetEntry (kvp.Key, false);
                group.children = new List<string> (list.Count);
                foreach (var stat in list)
                    group.children.Add (stat.key);
            }
        }
    }
}


