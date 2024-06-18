using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerScenarioGroup : DataMultiLinker<DataContainerScenarioGroup> 
    {
        public DataMultiLinkerScenarioGroup ()
        {
            textSectorKeys = new List<string> { TextLibs.scenarioGroups };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.scenarioGroups),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.scenarioGroups)
            );
        }

        [FoldoutGroup ("Utilities", false)]
        [ShowInInspector]
        public static SortedDictionary<string, List<string>> lookup = new SortedDictionary<string, List<string>> ();

        [FoldoutGroup ("Utilities", false)]
        [Button]
        public static void RefreshGroups ()
        {
            lookup.Clear ();
            foreach (var kvp in DataMultiLinkerScenario.data)
            {
                var scenario = kvp.Value;
                if (scenario.hidden)
                    continue;
                
                var presetKey = kvp.Key;
                scenario.UpdateGroups ();

                foreach (var groupKey in scenario.groupKeys)
                {
                    if (!lookup.ContainsKey (groupKey))
                        lookup.Add (groupKey, new List<string> { presetKey });
                    else
                        lookup[groupKey].Add (presetKey);
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button]
        public static void AdjustHue (string filter, int hue)
        {
            foreach (var kvp in data)
            {
                if (!kvp.Key.Contains (filter))
                    continue;
                
                var group = kvp.Value;
                group.hue = hue;
            }
        }
    }
}


