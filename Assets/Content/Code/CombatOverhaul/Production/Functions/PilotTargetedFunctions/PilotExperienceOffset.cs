using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotExperienceOffset : IPilotTargetedFunction
    {
        public int xp = 0;

        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (xp == 0)
                return;
            
            if (pilot != null)
                PilotUtility.OffsetExperience (pilot, xp);
            
            #endif
        }
    }
}