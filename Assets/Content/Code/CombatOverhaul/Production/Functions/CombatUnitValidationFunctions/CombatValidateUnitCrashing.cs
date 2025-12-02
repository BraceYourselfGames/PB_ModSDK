using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitCrashing : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;
            
            return unitCombat.isCrashing == present;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitAllied : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
                return false;
            
            return unitCombat.isOwnerAllied == present;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitClass : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (UnitClassKeys), false)")]
        public string classKey = "mech";
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.hasDataKeyUnitClass)
                return false;

            bool classMatched = string.Equals (classKey, unitPersistent.dataKeyUnitClass.s);
            return classMatched == present;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitBlueprint : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerUnitBlueprint.data.Keys")]
        public string blueprintKey = "mech";
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.hasDataKeyUnitBlueprint)
                return false;

            bool classMatched = string.Equals (blueprintKey, unitPersistent.dataKeyUnitBlueprint.s);
            return classMatched == present;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitPreset : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        public string presetKey = "mech";
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.hasDataKeyUnitPreset)
                return false;

            bool classMatched = string.Equals (presetKey, unitPersistent.dataKeyUnitPreset.s);
            return classMatched == present;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitWithPilotProfile : DataBlockSubcheckBool, ICombatUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            var pilot = IDUtility.GetLinkedPilot (unitPersistent);
            var pilotProfilePresent = pilot != null && pilot.hasPilotProfileLink;
            
            return pilotProfilePresent == present;

            #else
            return false;
            #endif
        }
    }
}