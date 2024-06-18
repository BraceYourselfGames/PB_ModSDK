using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyResourcesTarget : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunctionLog
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

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK
            
            var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
            Run (targetPersistent);
            
            #endif
        }
        
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK
            
            var targetPersistent = IDUtility.GetOverworldActionPersistentTarget (source);
            Run (targetPersistent);
            
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
    }
}