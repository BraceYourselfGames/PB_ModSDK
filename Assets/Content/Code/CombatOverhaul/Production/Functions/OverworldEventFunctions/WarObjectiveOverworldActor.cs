using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class WarObjectiveOverworldActor : IOverworldEventFunction, IOverworldEventFunctionEarly, IOverworldEventParent
    {
        public DataContainerOverworldEvent ParentEvent { get; set; }
        public bool Early () => true;

        [ValueDropdown ("@ParentEvent.GetActorKeys()")]
        public string actorKey;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var actors = overworld.hasEventInProgressActorWorld ? overworld.eventInProgressActorWorld.persistentIDs : null;

            if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
            {
                Debug.LogWarning ($"Failed to reveal overworld entity - no actors or failed to find actor with index of {actorKey}");
                return;
            }

            int actorPersistentID = actors[actorKey];
            var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);
            var actorOverworld = IDUtility.GetLinkedOverworldEntity (actorPersistent);
            
            if (actorOverworld == null)
                return;
            
            Debug.LogWarning ($"Revealing of overworld actor {actorKey} | {actorOverworld.ToLog ()}");

            actorOverworld.isWarObjective = true;
            actorOverworld.isPlayerKnown = true;
            actorOverworld.isPlayerRecognized = true;
            actorOverworld.isInPlayerVisionRange = true;
            actorOverworld.isIntelLocked = true;
            actorOverworld.ReplacePositionDetectedLast (actorOverworld.position.v);
            if (actorOverworld.hasMovementSpeedLimit)
                actorOverworld.ReplaceMovementSpeedLimit (actorOverworld.movementSpeedLimit.f * 0.5f);

            actorPersistent.SetMemoryFloat (EventMemoryIntAutofilled.World_Tag_Auto_WarObjective, 1);
            Debug.Log ($"WarObjectiveOverworldActor | {actorKey} | {actorOverworld.ToLog ()}: Designated as war objective!");
            
            #endif
        }
    }
}