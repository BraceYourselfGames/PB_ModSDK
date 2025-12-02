using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public enum UnitMemoryContext
    {
        Unit,
        Pilot,
        MobileBase,
        BattleSite
    }
    
    [Serializable]
    public class CombatValidateUnitMemory : ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        public UnitMemoryContext context = UnitMemoryContext.MobileBase;
        
        [HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
        public DataBlockOverworldMemoryCheckGroup memory = new DataBlockOverworldMemoryCheckGroup ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            bool valid = false;
            if (context == UnitMemoryContext.Unit)
            {
                valid = memory.IsPassed (unitPersistent);
            }
            else if (context == UnitMemoryContext.Pilot)
            {
                var pilotPersistent = IDUtility.GetLinkedPilot (unitPersistent);
                valid = memory.IsPassed (pilotPersistent);
            }
            else if (context == UnitMemoryContext.MobileBase)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                valid = memory.IsPassed (basePersistent);
            }
            else if (context == UnitMemoryContext.BattleSite)
            {
                var sitePersistent = ScenarioUtility.GetCombatSite ();
                valid = memory.IsPassed (sitePersistent);
            }

            return valid;

            #else
            return false;
            #endif
        }
    }
}