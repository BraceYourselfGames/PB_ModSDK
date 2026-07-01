using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionValidationRetreat : ICombatActionValidationFunction
    {
        public bool IsValid (CombatEntity unitCombat)
        {
            #if !PB_MODSDK

            bool friendly = unitCombat != null && unitCombat.isOwnerAllied;
            bool available = friendly &&  ScenarioUtility.IsRetreatAvailable ();
            return available;

            #else
            return false;
            #endif
        }
    }
}