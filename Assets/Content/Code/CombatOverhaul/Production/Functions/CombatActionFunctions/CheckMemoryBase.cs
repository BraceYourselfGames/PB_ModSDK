using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CheckMemoryBase : IOverworldActionFunction
    {
        [PropertyOrder (-1), BoxGroup]
        public DataBlockOverworldMemoryCheckGroup checks = new DataBlockOverworldMemoryCheckGroup ();
        
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true)]
        public List<IOverworldActionFunction> functions = new List<IOverworldActionFunction> ();

        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            bool memoryValid = true;
            if (checks != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                if (basePersistent != null)
                    memoryValid = checks.IsPassed (basePersistent);
            }

            if (!memoryValid)
                return;

            if (functions == null)
                return;

            foreach (var function in functions)
            {
                if (function != null)
                    function.Run (source);
            }
            
            #endif
        }
    }
}