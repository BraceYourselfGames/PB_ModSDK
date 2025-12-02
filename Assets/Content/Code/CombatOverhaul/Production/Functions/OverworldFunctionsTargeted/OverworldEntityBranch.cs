using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldEntityBranch : IOverworldTargetedFunction
    {
        [ValueDropdown("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string key;
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK

            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;

            var branchData = DataMultiLinkerOverworldFactionBranch.GetEntry (key);
            if (branchData == null)
                return;
            
            entityPersistent.ReplaceFactionBranch (branchData.key);

            #endif
        }
    }
    
    [Serializable]
    public class OverworldEntityBranchOutsideProvince : IOverworldTargetedFunction
    {
        private static List<string> branchesFiltered = new List<string> ();
        
        public void Run (OverworldEntity entityOverworld)
        {
            #if !PB_MODSDK
            
            if (entityOverworld == null || entityOverworld.isDestroyed)
                return;

            var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
            if (entityPersistent == null)
                return;
            
            var factionBranchActive = DataHelperProvince.GetProvinceBranchActive ();
            var branches = DataMultiLinkerOverworldFactionBranch.data;
            
            branchesFiltered.Clear ();
            foreach (var kvp in branches)
            {
                var branchData = kvp.Value;
                if (branchData.training)
                    continue;
                
                if (!string.Equals (factionBranchActive, kvp.Key, StringComparison.Ordinal))
                    branchesFiltered.Add (kvp.Key);
            }

            if (branchesFiltered.Count == 0)
                return;
            
            var key = branchesFiltered.GetRandomEntry ();
            entityPersistent.ReplaceFactionBranch (key);
            ScenarioSetupUtility.UpdateCombatDescription (entityOverworld, 0, null);

            #endif
        }
    }
}