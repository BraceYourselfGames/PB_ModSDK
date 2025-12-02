using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotLogTraitActivation : IPilotTargetedFunction
    {
        [LabelText ("Trait"), PropertyOrder (-10)]
        [ValueDropdown ("@DataMultiLinkerPilotTrait.data.Keys")]
        public string key;

        public void Run (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag || string.IsNullOrEmpty (key))
                return;

            var trait = DataMultiLinkerPilotTrait.GetEntry (key);
            if (trait == null)
                return;
            
            TextUtility.GetPilotIdentificationText (pilot, out var textName, out var textCallsign);
            var textActivation = "Trait activated";
            var textFull = $"{textCallsign} ({textName}): {textActivation} ({trait.GetTextName ()})";

            if (IDUtility.IsGameState (GameStates.combat))
            {
                var unitPersistent = IDUtility.GetPersistentParent (pilot);
                var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                CIViewCombatEventLog.AddMessageEventUnit (unitCombat, textFull);
            }
            else
            {
                CIViewOverworldLog.AddMessage (textFull);
            }

            #endif
        }
    }
}