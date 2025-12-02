using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotRecordTraitActivation : IPilotTargetedFunction
    {
        [LabelText ("Trait"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotTrait.data.Keys")]
        public string key;

        public bool log;

        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag || string.IsNullOrEmpty (key))
                return;

            var trait = DataMultiLinkerPilotTrait.GetEntry (key);
            if (trait == null)
                return;

            Dictionary<string, PilotTraitActivationRecord> activationHistory = null;
            if (pilot.hasPilotTraitActivations)
                activationHistory = pilot.pilotTraitActivations.s;
            if (activationHistory == null)
                activationHistory = new Dictionary<string, PilotTraitActivationRecord> ();

            var combat = Contexts.sharedInstance.combat;
            var timeCombat = IDUtility.IsGameState (GameStates.combat) && combat.hasSimulationTime ? combat.simulationTime.f : 0f;
            
            var overworld = Contexts.sharedInstance.overworld;
            var timeOverworld = overworld.hasSimulationTime ? overworld.simulationTime.f : 0f;
            
            var record = new PilotTraitActivationRecord
            {
                timeCombat = timeCombat,
                timeOverworld = timeOverworld
            };

            activationHistory[key] = record;
            
            // Register to more tags, e.g.
            if (trait.activationRecordTags != null && trait.activationRecordTags.Count > 0)
            {
                foreach (var tag in trait.activationRecordTags)
                {
                    if (!string.IsNullOrEmpty (tag))
                        activationHistory[tag] = record;
                }
            }
            
            pilot.ReplacePilotTraitActivations (activationHistory);

            if (log)
            {
                TextUtility.GetPilotIdentificationText (pilot, out var textName, out var textCallsign);
                var textActivation = "Trait activated";
                var textFull = $"{textCallsign} ({textName}): {textActivation} ({trait.GetTextName ()})";

                if (IDUtility.IsGameState (GameStates.combat))
                {
                    var unitPersistent = IDUtility.GetPersistentParent (pilot);
                    var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                    CIViewCombatEventLog.AddMessageEventUnit (unitCombat, textFull);
                    CIViewOverworldLog.AddMessage (textFull);
                }
                else
                {
                    CIViewOverworldLog.AddMessage (textFull);
                }
            }
            

            #endif
        }
    }
}