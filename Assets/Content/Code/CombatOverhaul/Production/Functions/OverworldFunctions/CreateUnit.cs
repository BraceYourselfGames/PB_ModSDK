using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CreateUnit : IOverworldFunction
    {
        public int levelOffset = 0;
        
        [ValueDropdown ("@DataMultiLinkerQualityTable.data.Keys")]
        [InlineButtonClear]
        public string qualityTable;
        
        [InlineButtonClear]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string factionBranch;
        
        [ValueDropdown ("@DataMultiLinkerUnitPreset.data.Keys")]
        [InlineButtonClear]
        public string presetKey;

        [DropdownReference]
        public SortedDictionary<string, bool> presetTags;

        [DropdownReference]
        public SortedDictionary<string, float> memory;

        
        

        public void Run ()
        {
            #if !PB_MODSDK
            
            var playerBasePersistent = IDUtility.playerBasePersistent;
            if (playerBasePersistent == null)
                return;
            
            var nameInternalSafe = IDUtility.GetSafePersistentInternalName ("unit_new");
            var level = Mathf.Max (1, Contexts.sharedInstance.persistent.workshopLevel.level + levelOffset);
            var hostName = playerBasePersistent.nameInternal.s;

            PersistentEntity unitPersistent = null;
            DataContainerUnitPreset preset = null;
            
            if (!string.IsNullOrEmpty (presetKey)) 
                preset = DataMultiLinkerUnitPreset.GetEntry (presetKey);
            else if (presetTags != null)
            {
                var presetKeysWithTags = DataTagUtility.GetKeysWithTags (DataMultiLinkerUnitPreset.data, presetTags);
                if (presetKeysWithTags.Count == 0)
                {
                    Debug.LogWarning ($"Failed to find any unit presets matching filter: {presetTags.ToStringFormattedKeyValuePairs ()}");
                    return;
                }
                
                var presetKeyFromTag = presetKeysWithTags.GetRandomEntry ();
                preset = DataMultiLinkerUnitPreset.GetEntry (presetKeyFromTag);
            }
            
            if (preset != null)
            {
                var factionData = DataMultiLinkerOverworldFactionBranch.GetEntry (factionBranch, false);
                var qualityTableData = DataMultiLinkerQualityTable.GetEntry (qualityTable, false);
                
                unitPersistent = UnitUtilities.CreatePersistentUnit 
                (
                    preset, 
                    nameInternalSafe, 
                    level,
                    parentEntityNameInternal: hostName,
                    factionData: factionData,
                    equipmentQualityTableKey: qualityTableData != null ? qualityTable : null,
                    acceptLowerQuality: true
                );
            }
            else
            {
                unitPersistent = UnitUtilities.CreatePersistentUnit 
                (
                    nameInternalSafe,
                    "unit_mech",
                    hostName,
                    Factions.player,
                    installDefaultParts: true
                );
            }

            if (unitPersistent == null)
                return;
            
            if (memory != null)
            {
                foreach (var kvp in memory)
                    unitPersistent.SetMemoryFloat (kvp.Key, kvp.Value);
            }

            DataHelperStats.RefreshStatCacheForUnit (unitPersistent);
            Contexts.sharedInstance.persistent.isPlayerCombatReadinessChecked = true;
            
            #endif
        }
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CreateUnit () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
}