using System;
using PhantomBrigade.Data;
using PhantomBrigade.Linking;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class DeleteOverworldActor : IOverworldEventFunction, IOverworldEventFunctionEarly, IOverworldEventParent
    {
        public bool Early () => true;
        public DataContainerOverworldEvent ParentEvent { get; set; }

        [ValueDropdown ("@ParentEvent.GetActorKeys()")]
        public string actorKey;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            var overworld = Contexts.sharedInstance.overworld;
            var actors = overworld.hasEventInProgressActorWorld ? overworld.eventInProgressActorWorld.persistentIDs : null;

            if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
            {
                Debug.LogWarning ($"Failed to destroy overworld entity - no actors or failed to find actor with index of {actorKey}");
                return;
            }

            int actorPersistentID = actors[actorKey];
            var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);
            var actorOverworld = IDUtility.GetLinkedOverworldEntity (actorPersistent);
            
            Debug.LogWarning ($"Destroying overworld actor {actorKey} | {actorOverworld.ToLog ()} | {actorPersistent.ToLog ()}");

            if (actorPersistent != null)
                actorPersistent.isDestroyed = true;

            if (actorOverworld != null)
            {
                actorOverworld.isDestroyed = true;
                
                // Destruction link seems to be firing one frame late when destruction flag is set in this system and I have absolutely no idea why
                if (actorOverworld.hasOverworldView)
                {
                    Debug.LogWarning ($"Manually triggering on-destruction link of overworld parent entity {target.ToLog ()} as proper link system never triggers for some bizarre reason");
                    IOnDestroyLink destroyLink = actorOverworld.overworldView.view;
                    destroyLink?.OnDestroyLink ();
                }
            }
            
            #endif
        }
    }
}