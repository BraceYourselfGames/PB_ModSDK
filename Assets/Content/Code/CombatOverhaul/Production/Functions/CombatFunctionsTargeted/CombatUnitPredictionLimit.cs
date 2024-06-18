using System;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitPredictionLimit : ICombatFunctionTargeted
    {
        [PropertyRange (0f, 6f)]
        public float limit = 1f;
        
        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return;

            bool limited = limit < 5f;
            if (limited)
                unitCombat.ReplacePredictionTimeHorizon (limit);
            else if (unitCombat.hasPredictionTimeHorizon)
                unitCombat.RemovePredictionTimeHorizon ();

            var combat = Contexts.sharedInstance.combat;
            var selectionID = combat.hasUnitSelected ? combat.unitSelected.id : IDUtility.invalidID;
            if (unitCombat.id.id == selectionID)
                CIViewCombatTimeline.ins.OnSelectedUnitChange (unitPersistent.id.id);
            
            #endif
        }
    }
}