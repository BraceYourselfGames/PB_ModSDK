using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyResources : IOverworldActionFunction, IOverworldFunctionLog, IOverworldFunction, IFunctionLocalizedText
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockResourceChange ()")]
        public List<DataBlockResourceChange> resourceChanges = new List<DataBlockResourceChange>
        {
            new DataBlockResourceChange
            {
                check = false,
                key = ResourceKeys.supplies,
                offset = true,
                value = 100
            }
        };
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            var selfOverworld = IDUtility.GetOverworldActionOwner (source);
            var selfPersistent = IDUtility.GetLinkedPersistentEntity (selfOverworld);
            Run (selfPersistent);
            
            #endif
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var basePersistent = IDUtility.playerBasePersistent;
            Run (basePersistent);
            
            #endif
        }
        
        public void Run (PersistentEntity inventory)
        {
            #if !PB_MODSDK
            
            OverworldUtility.ApplyResourceChanges (resourceChanges, inventory);
            
            #endif
        }

        public string ToLog ()
        {
            if (resourceChanges == null || resourceChanges.Count == 0)
                return "null";

            return resourceChanges.ToStringFormatted ();
        }

        private static StringBuilder sb = new StringBuilder ();
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            if (resourceChanges == null || resourceChanges.Count == 0)
                return string.Empty;
            
            var basePersistent = Application.isPlaying ? IDUtility.playerBasePersistent : null;
            var resources = basePersistent != null && basePersistent.hasInventoryResources ? basePersistent.inventoryResources.s : null;
            
            sb.Clear ();
            foreach (var change in resourceChanges)
            {
                if (change == null)
                    continue;
                
                var resourceData = DataMultiLinkerResource.GetEntry (change.key, false);
                if (resourceData == null)
                    continue;
                
                float limit = EquipmentUtility.GetResourceLimit (resourceData);
                var amountCurrent = resources != null && resources.TryGetValue (change.key, out var v) ? v : 0f;
                var amountModified = change.offset ? Mathf.Clamp (amountCurrent + change.value, 0f, limit) : Mathf.Clamp (change.value, 0f, limit);
                if (amountModified.RoughlyEqual (amountCurrent))
                    continue;

                if (sb.Length > 0)
                    sb.Append (", ");

                sb.Append ("[cc]");
                sb.Append (resourceData.textName);
                sb.Append (": [ff]");
                
                if (change.offset)
                {
                    var positive = change.value > 0f;
                    if (positive)
                        sb.Append ("+");
                    sb.Append (change.value.ToString (resourceData.format));
                }
                else
                {
                    sb.Append (amountModified.ToString (resourceData.format));
                }
            }
            
            return sb.ToString ();

            #else
            return null;
            #endif
        }
    }
}