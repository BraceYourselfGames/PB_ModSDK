using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class OverworldValidateContestPossible : IOverworldEntityValidationFunction
    {
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var entityOverworld = IDUtility.GetLinkedOverworldEntity (entityPersistent);
            if (entityPersistent == null || entityOverworld == null)
                return false;

            if (!entityOverworld.hasFrontlineBase || !entityOverworld.frontlineBase.exitPoint)
                return false;
            
            var questStateLiberation = OverworldQuestUtility.GetQuestState ("province_liberation");
            if (questStateLiberation != null)
                return false;
            
            bool provinceActiveFound = DataHelperProvince.TryGetProvinceDependenciesActive 
            (
                out var provinceActiveBlueprint, 
                out var provinceActivePersistent, 
                out var provinceActiveOverworld
            );

            if (!provinceActiveFound)
                return false;
            
            var provinces = DataHelperProvince.GetProvincesOverworld ();
            bool linkFound = false;
            
            foreach (var provinceNextOverworld in provinces)
            {
                if (provinceNextOverworld == provinceActiveOverworld)
                    continue;
                
                if (provinceNextOverworld == null || !provinceNextOverworld.hasProvinceProperties)
                    continue;

                var props = provinceNextOverworld.provinceProperties.s;
                if (props == null || !string.Equals (provinceActiveBlueprint.key, props.provinceKeyOrigin))
                    continue;
                
                var provinceNextPersistent = IDUtility.GetLinkedPersistentEntity (provinceNextOverworld);
                if (provinceNextPersistent == null || !provinceNextPersistent.hasFaction || string.Equals (provinceNextPersistent.faction.s, Factions.player))
                    return false;

                linkFound = true;
                break;
            }

            if (!linkFound)
                return false;
            
            return true;

            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateCampaignProgress : DataBlockOverworldEventSubcheckInt, IOverworldEntityValidationFunction
    {
        protected override string GetLabel () => "Campaign progress";
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var campaignState = overworld.campaignState.s;
            var campaignProgress = campaignState.progress;

            return IsPassed (true, campaignProgress);

            #else
            return false;
            #endif
        }
    }
    
    public class OverworldValidateCampaignStep : DataBlockSubcheckBool, IOverworldEntityValidationFunction
    {
        [ValueDropdown ("@DataMultiLinkerOverworldCampaignStep.data.Keys")]
        public string stepKey;

        protected override string GetLabel () => present ? "Should be current step" : "Should not be current step";
        
        public bool IsValid (PersistentEntity entityPersistent)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var campaignState = overworld.campaignState.s;
            var currentStep = string.Equals (campaignState.stepKey, stepKey);
            return present == currentStep;

            #else
            return false;
            #endif
        }
    }
}