using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class DataBlockSubcheckBoolSubsystem : DataBlockSubcheckBool
    {
        protected override string GetLabel () => present ? "Subsystem should be present" : "Subsystem should be absent";
    }
    
    [Serializable]
    public class CombatValidateUnitSubsystem : DataBlockSubcheckBoolSubsystem, ICombatUnitValidationFunction, IOverworldUnitValidationFunction
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
        public string hardpoint;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        public string blueprint;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        public Dictionary<string, bool> tags;
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null)
                return false;

            if (string.IsNullOrEmpty (socket) || string.IsNullOrEmpty (hardpoint))
                return false;

            bool matchingSubsystemPresent = false;
            var part = EquipmentUtility.GetPartInUnit (unitPersistent, socket);
            if (part != null && !part.isDestroyed && !part.isWrecked)
            {
                var subsystem = EquipmentUtility.GetSubsystemInPart (part, hardpoint);
                if (subsystem != null && subsystem.hasDataLinkSubsystem && !subsystem.isDestroyed)
                {
                    matchingSubsystemPresent = true;
                    if (!string.IsNullOrEmpty (blueprint))
                    {
                        var bp = subsystem.dataLinkSubsystem.data;
                        if (!string.Equals (bp.key, blueprint))
                            matchingSubsystemPresent = false;
                    }
                    else if (tags != null && tags.Count > 0)
                    {
                        var bp = subsystem.dataLinkSubsystem.data;
                        var bpt = bp.tagsProcessed;
                        if (bpt == null || bpt.Count == 0)
                            matchingSubsystemPresent = false;
                        else
                        {
                            foreach (var kvp in tags)
                            {
                                var tag = kvp.Key;
                                bool required = kvp.Value;
                                bool tagPresent = bpt.Contains (tag);
                                if (required != tagPresent)
                                {
                                    matchingSubsystemPresent = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            bool passed = present == matchingSubsystemPresent;
            return passed;

            #else
            return false;
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatValidateUnitSubsystem () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
}