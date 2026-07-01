using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatActionValidationEject : ICombatActionValidationFunction
    {
        public bool IsValid (CombatEntity unitCombat)
        {
            #if !PB_MODSDK

            var unitPersistent = IDUtility.GetLinkedPersistentEntity (unitCombat);
            var pilotPersistent = IDUtility.GetLinkedPilot (unitPersistent);
            if (pilotPersistent == null)
                return false;
            
            if (pilotPersistent.hasPilotProfileLink)
            {
                bool friendly = CombatUIUtility.IsUnitFriendly (unitCombat);
                if (!friendly)
                {
                    int traumaCurrent = pilotPersistent.GetPilotStatRounded (PilotStatKeys.trauma);
                    int traumaLimit = pilotPersistent.GetPilotStatMaxRounded (PilotStatKeys.trauma);
                    if (traumaCurrent >= traumaLimit)
                    {
                        // Block ejections on persistent pilots about to die
                        return false;
                    }
                }
            }

            
            if (unitPersistent.isDisposableOutsideCombat && unitCombat.isOwnerAllied)
            {
                // Block ejections on disposable non-mech units
                var classKey = unitPersistent.hasDataKeyUnitClass ? unitPersistent.dataKeyUnitClass.s : null;
                if (classKey != UnitClassKeys.mech)
                    return false;
                
                // Block ejections on 3F squads in generated missions
                if (unitCombat.IsUnitTagged (ScenarioUnitTags.CustomGeneratedSquad))
                    return false;
            }
            
            return true;

            #else
            return false;
            #endif
        }
    }
}