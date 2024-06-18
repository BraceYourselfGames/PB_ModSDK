using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class RevealOverworldActor : IOverworldEventFunction, IOverworldEventFunctionEarly, IOverworldEventParent
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

            actorOverworld.isPlayerKnown = true;
            actorOverworld.isPlayerRecognized = true;
            
            #endif
        }
    }
}