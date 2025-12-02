using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotValidateRosterSelection : DataBlockSubcheckBool, IPilotValidationFunction
    {
        protected override string GetLabel () => present ? "Should be selected" : "Should not be selected";

        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            var id = pilot.id.id;
            var selected = OverworldPointUtility.campEntitiesSelected != null && OverworldPointUtility.campEntitiesSelected.Contains (id);

            return selected == present;

            #else
            return false;
            #endif
        }
    }
}