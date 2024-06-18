using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitPilotConcuss : ICombatFunctionTargeted
    {
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            var unitEntityCombat = IDUtility.GetSelectedCombatEntity ();
            var pilot = IDUtility.GetLinkedPilot (IDUtility.GetLinkedPersistentEntity (unitEntityCombat));
            if (pilot != null && !pilot.isKnockedOut)
            {
                var concussionThreshold = pilot.hasPilotConcussionInputs ? pilot.pilotConcussionInputs.concussionThreshold : 1f;
                pilot.ReplacePilotHealth (concussionThreshold);
            }
            
            #endif
        }
    }
}