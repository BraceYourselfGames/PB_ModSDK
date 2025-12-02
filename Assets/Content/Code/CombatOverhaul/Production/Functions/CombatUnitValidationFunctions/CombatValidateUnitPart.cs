using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class DataBlockSubcheckBoolPart : DataBlockSubcheckBool
    {
        protected override string GetLabel () => present ? "Part should be present" : "Part should be absent";
    }
    
    public class CombatValidateUnitPartCharges : DataBlockOverworldEventSubcheckInt, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return false;

            int charges = 0;
            var part = EquipmentUtility.GetPartInUnit (unitPersistent, socket);
            if (part != null)
                charges = part.hasChargeCount ? part.chargeCount.i : 0;
            
            return IsPassed (true, charges);

            #else
            return false;
            #endif
        }
    }
    
    public class CombatValidateUnitPartIntegrity : DataBlockOverworldEventSubcheckFloat, ICombatUnitValidationFunction
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return false;

            var part = EquipmentUtility.GetPartInUnit (unitPersistent, socket);
            float integrity = part != null && part.hasIntegrityNormalized ? part.integrityNormalized.f : 0f;
            return IsPassed (true, integrity);

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class CombatValidateUnitPart : DataBlockSubcheckBoolPart, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        [DropdownReference (true)]
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string preset;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        public Dictionary<string, bool> tags;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            bool matchingPartPresent = false;
            
            if (string.IsNullOrEmpty (socket))
            {
                var parts = EquipmentUtility.GetPartsInUnit (unitPersistent);
                foreach (var part in parts)
                {
                    matchingPartPresent = IsPartMatched (part);
                    if (matchingPartPresent)
                        break;
                }
            }
            else
            {
                var part = EquipmentUtility.GetPartInUnit (unitPersistent, socket);
                matchingPartPresent = IsPartMatched (part);
            }

            bool passed = present == matchingPartPresent;
            return passed;

            #else
            return false;
            #endif
        }

        private bool IsPartMatched (EquipmentEntity part)
        {
            #if !PB_MODSDK
            
            if (part == null || !part.isPart || part.isWrecked)
                return false;

            if (!string.IsNullOrEmpty (preset))
            {
                if (part.hasDataKeyPartPreset)
                    return string.Equals (part.dataKeyPartPreset.s, preset);
            }
            else if (tags != null && tags.Count > 0)
            {
                if (part.hasTagCache)
                {
                    var tagCache = part.tagCache.tags;
                    foreach (var kvp in tags)
                    {
                        var tag = kvp.Key;
                        bool required = kvp.Value;
                        bool tagPresent = tagCache.Contains (tag);
                        if (required != tagPresent)
                            return false;
                    }

                    return true;
                }
            }
            
            return true;
            
            #else
            return false;
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatValidateUnitPart () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class CombatValidateUnitFrameDefects : DataBlockOverworldEventSubcheckInt, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        protected override string GetLabel () => "Frame damage";

        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            int defects = unitPersistent.hasUnitFrameDefects ? unitPersistent.unitFrameDefects.i : 0;
            return IsPassed (true, defects);
            
            #else
            return false;
            #endif
        }
    }
    
    public class CombatValidateUnitFrameIntegrity : DataBlockOverworldEventSubcheckFloat, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            float hp = unitPersistent.hasUnitFrameIntegrity ? unitPersistent.unitFrameIntegrity.f : 1f;
            return IsPassed (true, hp);
            
            #else
            return false;
            #endif
        }
    }
}