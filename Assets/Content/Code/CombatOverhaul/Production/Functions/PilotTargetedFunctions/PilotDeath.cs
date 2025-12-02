using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotDeath : IPilotTargetedFunction
    {
        public string cause;

        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK
            
            if (pilot != null)
                PilotUtility.DeclareDeath (pilot, cause);
            
            #endif
        }
    }
}