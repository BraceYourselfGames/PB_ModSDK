using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateEscalationLevel : IOverworldValidationFunction
    {
        [HideReferenceObjectPicker, HideLabel]
        public DataBlockOverworldEventSubcheckIntRange check = new DataBlockOverworldEventSubcheckIntRange ();
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            if (entityPersistent == null)
                return false;

            if (check == null)
                return false;
            
            var escalationLevel = ScenarioUtilityGeneration.GetCombatTargetEscalationLevel ();
            bool escalationValid = check.IsPassed (escalationLevel);
            return escalationValid;

            #else
            return false;
            #endif
        }
    }
}