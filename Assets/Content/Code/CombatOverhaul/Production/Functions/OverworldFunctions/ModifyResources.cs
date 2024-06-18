using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyResources : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunctionLog, IOverworldFunction
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
            
            var basePersistent = IDUtility.playerBasePersistent;
            Run (basePersistent);
            
            #endif
        }
        
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
    }
}