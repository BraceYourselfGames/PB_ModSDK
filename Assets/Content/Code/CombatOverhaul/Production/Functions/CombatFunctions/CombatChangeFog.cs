using System;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatChangeFog : ICombatFunction
    {
        public bool fogChangeInstant;
        
        [PropertyRange (0f, 1f)]
        public float fogIntensityTarget = 0.33f;

        [PropertyRange (0.1f, 10f)]
        [HideIf ("fogChangeInstant")]
        public float fogProgressionSpeed = 0.2f;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (fogChangeInstant)
                PostprocessingHelper.SetFogTarget (fogIntensityTarget, true);
            else
            {
                PostprocessingHelper.SetFogProgressionSpeed (fogProgressionSpeed);
                PostprocessingHelper.SetFogTarget (fogIntensityTarget, false);
            }
            
            #endif
        }
    }
}