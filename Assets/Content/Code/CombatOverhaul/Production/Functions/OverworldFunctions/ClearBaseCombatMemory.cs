using System;
using System.Linq;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ClearBaseCombatMemory : ICombatFunction
    {
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
            if (basePersistent == null || !basePersistent.hasEventMemory)
                return;

            var memory = basePersistent.eventMemory.s;
            var keyList = memory.Keys.ToList ();
            
            foreach (var key in keyList)
            {
                if (key.Contains ("combat_sc_"))
                    memory.Remove (key);
            }
            
            basePersistent.ReplaceEventMemory (memory);
            
            #endif
        }
    }
}