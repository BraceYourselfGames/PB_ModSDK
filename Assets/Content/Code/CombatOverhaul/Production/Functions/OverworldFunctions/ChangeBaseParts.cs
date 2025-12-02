using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class BasePartInstall : IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        public string partKey;

        public void Run ()
        {
            #if !PB_MODSDK

            var partBlueprint = DataMultiLinkerBasePart.GetEntry (partKey);
            if (partBlueprint == null)
                return;
			
            var overworld = Contexts.sharedInstance.overworld;
            var partsInstalled = overworld.hasBasePartsInstalled ? overworld.basePartsInstalled.s : null;
            bool partsInstalledFound = partsInstalled != null && partsInstalled.Count > 0;
            int count = partsInstalledFound && partsInstalled.ContainsKey (partKey) ? partsInstalled[partKey] : 0;
            int limit = partBlueprint.limit;

            if (count >= limit)
            {
                Debug.LogWarning ($"Base part {partKey} is at limit: {count} / {limit}");
                return;
            }
				
            BasePartUtility.TryInstallPart (partBlueprint, false, false);
			
            overworld.isBaseAbilitiesUnlockedRefresh = true;
            BasePartUtility.RefreshBaseStats ();
            if (CIViewBaseParts.ins.IsEntered ())
                CIViewBaseParts.ins.RedrawGrid (false);
			
            Debug.Log ($"- Installed {partBlueprint.key} ({(partBlueprint.ui != null ? partBlueprint.ui.textName : "?")}) | New count: {count + 1}");
            
            #endif
        }
    }
}