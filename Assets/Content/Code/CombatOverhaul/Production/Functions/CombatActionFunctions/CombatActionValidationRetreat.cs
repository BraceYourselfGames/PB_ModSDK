using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionValidationRetreat : ICombatActionValidationFunction
    {
        public bool IsValid (CombatEntity unitCombat)
        {
            #if !PB_MODSDK

            bool available = ScenarioUtility.IsRetreatAvailable ();
            return available;

            #else
            return false;
            #endif
        }
    }
}