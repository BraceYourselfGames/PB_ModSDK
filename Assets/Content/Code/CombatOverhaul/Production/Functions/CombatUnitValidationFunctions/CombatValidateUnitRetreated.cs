using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitRetreated : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        public bool countEjection = true;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;

            bool retreated = unitCombat.isRetreated;
            
            if (!unitCombat.isUnitUncrewed)
            {
                bool pilotLinkPresent = unitPersistent.hasEntityLinkPilot;
                var pilot = pilotLinkPresent ? IDUtility.GetPersistentEntity (unitPersistent.entityLinkPilot.persistentID) : null;
                bool pilotMissing = pilot == null;

                if (countEjection)
                    retreated = retreated || pilotMissing;
            }

            bool passed = retreated == present;
            // Debug.Log ($"Checking for unit {unitPersistent.ToLog ()} retreat | Unit retreated: {unitCombat.isRetreated} | Pilot missing: {pilotMissing} | Desired value: {present} | Compared value: {retreated} | Check passed: {passed}");
            
            return passed;

            #else
            return false;
            #endif
        }
    }
}