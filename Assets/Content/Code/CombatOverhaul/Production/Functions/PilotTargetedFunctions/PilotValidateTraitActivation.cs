using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotValidateTraitActivation : DataBlockOverworldEventSubcheckFloat, IPilotValidationFunction
    {
        [LabelText ("$GetKeyLabel"), PropertyOrder (-10)]
        [ValueDropdown ("$GetKeys")]
        public string key;
        
        [PropertyOrder (-10)]
        public bool tag;
                
        [PropertyOrder (-10)]
        public bool combatContext;

        private IEnumerable<string> GetKeys ()
        {
            if (tag)
                return DataMultiLinkerPilotTrait.GetTags ();
            else
                return DataMultiLinkerPilotTrait.GetKeys ();
        }

        private string GetKeyLabel => tag ? "Trait tag" : "Trait key";
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool activationFound = false;
            float activationTime = -1f;
            
            var activationHistory = pilot.hasPilotTraitActivations ? pilot.pilotTraitActivations.s : null;
            if (activationHistory != null && activationHistory.Count > 0)
            {
                bool recordFound = activationHistory.TryGetValue (key, out var record);
                if (recordFound)
                {
                    activationFound = true;
                    activationTime = combatContext ? record.timeCombat : record.timeOverworld;
                }
            }

            var timeSinceLast = 100000f;
            if (activationFound)
            {
                if (combatContext)
                {
                    var combat = Contexts.sharedInstance.combat;
                    var timeCombat = IDUtility.IsGameState (GameStates.combat) && combat.hasSimulationTime ? combat.simulationTime.f : 0f;
                    timeSinceLast = timeCombat - activationTime;
                }
                else
                {
                    var overworld = Contexts.sharedInstance.overworld;
                    var timeOverworld = overworld.hasSimulationTime ? overworld.simulationTime.f : 0f;
                    timeSinceLast = timeOverworld - activationTime;
                }
            }
            
            bool passed = IsPassed (true, timeSinceLast);
            return passed;

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateTraitTriggered : DataBlockSubcheckBool, IPilotValidationFunction
    {
        [LabelText ("$GetKeyLabel"), PropertyOrder (-10)]
        [ValueDropdown ("$GetKeys")]
        public string key;
        
        [PropertyOrder (-10)]
        public bool tag;
        
        private IEnumerable<string> GetKeys ()
        {
            if (tag)
                return DataMultiLinkerPilotTrait.GetTags ();
            else
                return DataMultiLinkerPilotTrait.GetKeys ();
        }

        private string GetKeyLabel => tag ? "Trait tag" : "Trait key";
        
        protected override string GetLabel () => present ? "Trait should be in history" : "Trait should not be in history";
        
        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool activationFound = false;
            var activationHistory = pilot.hasPilotTraitActivations ? pilot.pilotTraitActivations.s : null;
            if (activationHistory != null && activationHistory.Count > 0)
                activationFound = activationHistory.TryGetValue (key, out var record);

            bool passed = present == activationFound;
            return passed;

            #else
            return false;
            #endif
        }
    }
}