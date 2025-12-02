using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitCheck : ICombatUnitValidationFunction
    {
        [HideLabel, HideReferenceObjectPicker]
        public DataBlockScenarioSubcheckUnit check = new DataBlockScenarioSubcheckUnit ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            bool match = ScenarioUtility.IsUnitMatchingCheck (unitPersistent, unitCombat, check, true, true, false);
            return match;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitChance : ICombatUnitValidationFunction
    {
        [PropertyRange (0f, 1f)]
        public float chance = 0.5f;

        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var chanceFinal = Mathf.Clamp01 (chance);
            bool passed = UnityEngine.Random.Range (0f, 1f) < chanceFinal;
            return passed;

            #else
            return false;
            #endif
        }
    }
}