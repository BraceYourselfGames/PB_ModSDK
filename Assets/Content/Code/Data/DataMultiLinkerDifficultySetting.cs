using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerDifficultySetting : DataMultiLinker<DataContainerDifficultySetting>
    {
        public DataMultiLinkerDifficultySetting ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.uiDifficultySettings);
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        public static Dictionary<string, List<DataContainerDifficultySetting>> dataByGroup = new Dictionary<string, List<DataContainerDifficultySetting>> ();

        public static void OnAfterDeserialization ()
        {
            dataByGroup.Clear ();
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (string.IsNullOrEmpty (c.group))
                    continue;
                
                if (dataByGroup.ContainsKey (c.group))
                    dataByGroup[c.group].Add (c);
                else
                    dataByGroup.Add (c.group, new List<DataContainerDifficultySetting> { c });
            }

            foreach (var kvp in dataByGroup)
            {
                kvp.Value.Sort((x, y) => x.priority.CompareTo (y.priority));
            }
        }
    }
}

