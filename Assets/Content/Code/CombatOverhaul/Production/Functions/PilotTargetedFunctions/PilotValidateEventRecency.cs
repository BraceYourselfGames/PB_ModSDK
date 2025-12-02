using System;
using System.Collections.Generic;
using PhantomBrigade.Combat;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class PilotValidateEventRecency : DataBlockSubcheckBool, IPilotValidationFunction
    {
        public PilotEventType type;
        public bool combatContext;
        public float timeThreshold;
        
        protected override string GetLabel () => present ? "Event should be found within this time" : "Event should be absent within this time";

        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            bool eventFound = false;
            float eventTime = -1f;
            
            var eventHistory = pilot.hasPilotEventHistory ? pilot.pilotEventHistory.s : null;
            if (eventHistory != null && eventHistory.Count > 0)
            {
                for (int i = eventHistory.Count - 1; i >= 0; --i)
                {
                    var eventRecord = eventHistory[i];
                    if (eventRecord.type != type)
                        continue;

                    eventFound = true;
                    eventTime = combatContext ? eventRecord.timeCombat : eventRecord.timeOverworld;
                }
            }

            if (combatContext)
            {
                var combat = Contexts.sharedInstance.combat;
                var timeCombat = IDUtility.IsGameState (GameStates.combat) && combat.hasSimulationTime ? combat.simulationTime.f : 0f;
                
                bool presentInTime = eventFound && (timeCombat - eventTime) < timeThreshold;
                if (presentInTime != present)
                    return false;
            }
            else
            {
                var overworld = Contexts.sharedInstance.overworld;
                var timeOverworld = overworld.hasSimulationTime ? overworld.simulationTime.f : 0f;
                
                bool presentInTime = eventFound && (timeOverworld - eventTime) < timeThreshold;
                if (presentInTime != present)
                    return false;
            }

            return true;

            #else
            return false;
            #endif
        }
    }
    
    public class PilotValidateEventCount : DataBlockOverworldEventSubcheckInt, IPilotValidationFunction
    {
        public PilotEventType type;

        public bool IsValid (PersistentEntity pilot, PersistentEntity entityPersistentLinked)
        {
            #if !PB_MODSDK

            if (pilot == null || !pilot.isPilotTag)
                return false;

            int eventCount = 0;
            var eventHistory = pilot.hasPilotEventHistory ? pilot.pilotEventHistory.s : null;
            if (eventHistory != null && eventHistory.Count > 0)
            {
                for (int i = eventHistory.Count - 1; i >= 0; --i)
                {
                    var eventRecord = eventHistory[i];
                    if (eventRecord.type == type)
                        eventCount += 1;
                }
            }

            return IsPassed (true, eventCount);

            #else
            return false;
            #endif
        }
    }
}