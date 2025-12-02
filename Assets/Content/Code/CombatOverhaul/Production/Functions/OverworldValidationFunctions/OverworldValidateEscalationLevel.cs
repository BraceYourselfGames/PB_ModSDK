using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateEscalationLevel : DataBlockOverworldEventSubcheckIntRange, IOverworldEntityValidationFunction
    {
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            var escalationLevel = ScenarioUtilityGeneration.GetCombatTargetEscalationLevel (entityOverworld);
            bool escalationValid = IsPassed (escalationLevel);
            return escalationValid;

            #else
            return false;
            #endif
        }
    }
}