using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class UnitValidateRosterSelection : DataBlockSubcheckBool, IOverworldUnitValidationFunction
    {
        protected override string GetLabel () => present ? "Should be selected" : "Should not be selected";

        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var id = unitPersistent.id.id;
            var selected = OverworldPointUtility.campEntitiesSelected != null && OverworldPointUtility.campEntitiesSelected.Contains (id);
            
            return selected == present;
            
            #else
            return false;
            #endif
        }
    }
}