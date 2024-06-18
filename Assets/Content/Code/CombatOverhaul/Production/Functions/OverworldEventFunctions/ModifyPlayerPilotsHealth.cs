using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ModifyPlayerPilotsHealth : IOverworldEventFunction, IOverworldEventParent, IOverworldEventFunctionEarly
    {
        public bool Early () => true;
        
        public DataContainerOverworldEvent ParentEvent { get; set; }

        [Tooltip ("By how much pilot HP would be offset")]
        public int offset;

        [Tooltip ("If this key is provided, only one pilot will be affected (actor with corresponding internal name). If no actor key is specified, all player pilots are affected")]
        [InlineButtonClear]
        [ValueDropdown ("@ParentEvent.GetActorKeys()")]
        public string actorKey;
        
        [Tooltip ("If this is off, pilots can't drop below 1 HP")]
        public bool allowDeath;

        public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
        {
            #if !PB_MODSDK

            float healthMinimum = allowDeath ? 0f : 1f;
            
            if (!string.IsNullOrEmpty (actorKey))
            {
                var overworld = Contexts.sharedInstance.overworld;
                var actors = 
                    overworld.hasEventInProgressActorPilots ? 
                    overworld.eventInProgressActorPilots.persistentIDs : 
                    null;

                if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
                {
                    Debug.LogWarning ($"Failed to find pilot actor slot {actorKey} in event {eventData.key} to modify pilot health");
                    return;
                }
                
                var actorPersistentID = actors[actorKey];
                var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);
                    
                if (!actorPersistent.isPilotTag || !actorPersistent.hasPilotHealth || !actorPersistent.hasPilotStats)
                {
                    Debug.LogWarning ($"Can't modify health of pilot {actorPersistent.ToLog ()} in actor slot {actorKey} in event {eventData.key} as it's missing appropriate components");
                    return;
                }
                
                var healthLimit = actorPersistent.pilotStats.healthLimit;
                var health = actorPersistent.pilotHealth.f;
                var healthModified = Mathf.Clamp (health + offset, healthMinimum, healthLimit);
                    
                if (!healthModified.RoughlyEqual (health))
                {
                    actorPersistent.ReplacePilotHealth (healthModified);
                    Debug.LogWarning ($"Pilot {actorPersistent.ToLog ()} (actor slot {actorKey} in event {eventData.key}) health modified from {health} to {healthModified}");
                }
            }
            else
            {
                var basePersistent = IDUtility.playerBasePersistent;
                var persistent = Contexts.sharedInstance.persistent;
                var childEntities = persistent.GetEntitiesWithEntityLinkPersistentParent (basePersistent.id.id);

                foreach (var childEntity in childEntities)
                {
                    if (!childEntity.isPilotTag)
                        continue;

                    if (!childEntity.hasPilotHealth || !childEntity.hasPilotStats)
                    {
                        Debug.LogWarning ($"Can't modify health of pilot {childEntity.ToLog ()} as it's missing health and/or stats components");
                        continue;
                    }

                    var healthLimit = childEntity.pilotStats.healthLimit;
                    var health = childEntity.pilotHealth.f;
                    var healthModified = Mathf.Clamp (health + offset, healthMinimum, healthLimit);
                    
                    if (!healthModified.RoughlyEqual (health))
                    {
                        childEntity.ReplacePilotHealth (healthModified);
                        Debug.LogWarning ($"Pilot {childEntity.ToLog ()} health modified from {health} to {healthModified}");
                    }
                }
            }
            
            #endif
        }
    }
}