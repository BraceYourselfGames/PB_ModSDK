using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldValidateProvinceVisit : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Should be visited" : "Should not be visited";

        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        public string provinceKey;
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var campaignState = overworld.hasCampaignState ? overworld.campaignState.s : null;
            bool visited = campaignState?.provincesVisited != null && !string.IsNullOrEmpty (provinceKey) && campaignState.provincesVisited.Contains (provinceKey);
            return visited == present;

            #else
            return false;
            #endif
        }
    }
    
    [Serializable]
    public class OverworldValidateCampaignStepVisit : DataBlockSubcheckBool, IOverworldGlobalValidationFunction
    {
        protected override string GetLabel () => present ? "Should be visited" : "Should not be visited";

        [ValueDropdown ("@DataMultiLinkerOverworldCampaignStep.data.Keys")]
        public string stepKey;
        
        public bool IsValid ()
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var campaignState = overworld.hasCampaignState ? overworld.campaignState.s : null;
            bool visited = campaignState?.stepsVisited != null && !string.IsNullOrEmpty (stepKey) && campaignState.stepsVisited.Contains (stepKey);
            return visited == present;

            #else
            return false;
            #endif
        }
    }
}