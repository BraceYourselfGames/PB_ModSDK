using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyMemoryBase : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunctionLog, IOverworldFunction, ICombatFunction
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            Run ();
        }
        
        public void Run (OverworldActionEntity source)
        {
            Run ();
        }
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var basePersistent = IDUtility.playerBasePersistent;
            basePersistent.ApplyEventMemoryChangeFloat (changes);
            
            #endif
        }

        public string ToLog ()
        {
            if (changes == null || changes.Count == 0)
                return "null";

            return changes.ToStringFormatted ();
        }
    }
}