using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatDisplayTitle : ICombatFunction
    {
        public float durationOverride = -1f;

        public void Run ()
        {
            #if !PB_MODSDK
            
            ScenarioUtility.DisplayTitleAnimation (null, null, durationOverride);
            
            #endif
        }
    }
}