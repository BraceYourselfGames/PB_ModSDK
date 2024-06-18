using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateBaseParts : IOverworldValidationFunction
    {
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> baseParts = new SortedDictionary<string, int> ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            bool basePartsValid = true;
            if (baseParts != null && baseParts.Count > 0)
            {
                var overworld = Contexts.sharedInstance.overworld;
                var partsInstalled = overworld.hasBasePartsInstalled ? overworld.basePartsInstalled.s : null;
                bool partsInstalledFound = partsInstalled != null && partsInstalled.Count > 0;
                
                foreach (var kvp in baseParts)
                {
                    var partKey = kvp.Key;
                    var countRequired = kvp.Value;
                    int countCurrent = partsInstalledFound && partsInstalled.ContainsKey (partKey) ? partsInstalled[partKey] : 0;

                    if (countCurrent < countRequired)
                    {
                        basePartsValid = false;
                        break;
                    }
                }
            }

            return basePartsValid;

            #else
            return false;
            #endif
        }
    }
}