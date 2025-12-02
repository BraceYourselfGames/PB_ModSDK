using System.Collections.Generic;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotAppearancePreset : DataMultiLinker<DataContainerPilotAppearancePreset>
    {
        private static List<string> filteredKeys = new List<string> ();
        
        public static List<string> GetFilteredAppearancePresetKeys (int factionFilter, string modelFilter)
        {
            bool factionFilterUsed = factionFilter == 1 || factionFilter == 2;
            bool modelFilterUsed = !string.IsNullOrEmpty (modelFilter);
            filteredKeys.Clear ();

            foreach (var kvp in data)
            {
                var link = kvp.Value;
                if (link == null || link.appearance == null)
                    continue;
                
                if (factionFilterUsed)
                {
                    if (factionFilter == 1 && !link.usableByFriendly)
                        continue;

                    if (factionFilter == 2 && !link.usableByHostile)
                        continue;
                }

                if (modelFilterUsed && link.appearance.model != modelFilter)
                    continue;
                
                filteredKeys.Add (link.key);
            }

            return filteredKeys;
        }
        
        public static DataContainerPilotAppearancePreset GetRandomAppearancePreset (int factionFilter, string modelFilter)
        {
            var filteredKeysFound = GetFilteredAppearancePresetKeys (factionFilter, modelFilter);
            if (filteredKeysFound.Count == 0)
                return null;

            var selectedKey = filteredKeysFound.GetRandomEntry ();
            var selection = data[selectedKey];
            return selection;
        }
    }
}


