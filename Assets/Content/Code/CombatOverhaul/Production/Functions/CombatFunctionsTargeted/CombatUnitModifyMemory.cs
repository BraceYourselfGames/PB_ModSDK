using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class CombatUnitModifyMemory : ICombatFunctionTargeted
    {
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
         
            if (unitPersistent == null)
                return;

            unitPersistent.ApplyEventMemoryChangeFloat (changes);
            
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