using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateBaseParts : IOverworldGlobalValidationFunction
    {
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, int> baseParts = new SortedDictionary<string, int> ();
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

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
    
    [Serializable]
    public class OverworldValidateBasePart : DataBlockOverworldEventSubcheckInt, IOverworldGlobalValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        public string partKey;
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var partsInstalled = overworld.hasBasePartsInstalled ? overworld.basePartsInstalled.s : null;
            bool partsInstalledFound = partsInstalled != null && partsInstalled.Count > 0;
            int countCurrent = partsInstalledFound && partsInstalled.TryGetValue (partKey, out var c) ? c : 0;

            return IsPassed (true, countCurrent);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateBaseResources : IOverworldGlobalValidationFunction
    {
        [DropdownReference, DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, DataBlockOverworldEventSubcheckInt> checks = new SortedDictionary<string, DataBlockOverworldEventSubcheckInt> ();
        
        public bool IsValid ()
        {
            #if !PB_MODSDK
            
            bool resourcesValid = true;
            if (checks != null && checks.Count > 0)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                var resources = basePersistent.hasInventoryResources ? basePersistent.inventoryResources.s : null;
                
                foreach (var kvp in checks)
                {
                    var key = kvp.Key;
                    var amountCurrent = resources != null && resources.ContainsKey (key) ? resources[key] : 0f;
                    var amountCurrentRound = Mathf.RoundToInt (amountCurrent);
                    
                    var check = kvp.Value;
                    bool passed = check.IsPassed (true, amountCurrentRound);
                    
                    if (!passed)
                    {
                        resourcesValid = false;
                        break;
                    }
                }
            }

            return resourcesValid;

            #else
            return false;
            #endif
        }
    }
}