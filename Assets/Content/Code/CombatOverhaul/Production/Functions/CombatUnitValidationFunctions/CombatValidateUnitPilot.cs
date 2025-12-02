using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitPilot : ICombatUnitValidationFunction
    {
        public List<IPilotValidationFunction> checks = new List<IPilotValidationFunction> ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || checks == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;

            var pilot = IDUtility.GetLinkedPilot (unitPersistent);
            if (pilot == null || pilot.isDestroyed)
                return false;

            foreach (var check in checks)
            {
                if (check != null && !check.IsValid (pilot, null))
                    return false;
            }

            return true;

            #else
            return false;
            #endif
        }
    }
}