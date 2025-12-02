using System;
using PhantomBrigade.Data;
using PhantomBrigade.Linking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldCombatForced : IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerScenario.data.Keys"), InlineButtonClear]
        public string scenarioKey;
        
        [ValueDropdown ("@DataMultiLinkerCombatArea.data.Keys"), InlineButtonClear]
        public string areaKey;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            var scenario = DataMultiLinkerScenario.GetEntry (scenarioKey, false);
            if (scenario == null)
            {
                Debug.LogWarning ($"Failed to start intro due to missing scenario data for key {scenarioKey}");
                return;
            }
        
            var siteNameInternal = IDReserved.RootEnemy;
            var targetPersistent = IDUtility.GetPersistentEntity (siteNameInternal);
            var targetOverworld = IDUtility.GetLinkedOverworldEntity (targetPersistent);
            if (targetPersistent == null || targetOverworld == null)
            {
                Debug.LogWarning ($"Failed to start intro due to missing site entity {siteNameInternal}");
                return;
            }
            
            var area = DataMultiLinkerCombatArea.GetEntry (areaKey, false);
            ScenarioUtility.ForceScenarioAndArea (scenario, area, 0, targetOverworld, false);
            
            #endif
        }
    }
}