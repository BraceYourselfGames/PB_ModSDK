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
    
    [Serializable]
    public class CombatValidateUnitPart : DataBlockSubcheckBoolPart, ICombatUnitValidationFunction
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
            
            return false;
            
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
}