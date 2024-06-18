using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class SummonPatrolActor : IOverworldEventFunction, IOverworldEventParent
	{
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
				Debug.LogWarning ($"Failed to summon patrol to actor - no actors or failed to find actor with index of {actorKey}");
				return;
			}

			int actorPersistentID = actors[actorKey];
			var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);
			var actorOverworld = IDUtility.GetLinkedOverworldEntity (actorPersistent);

			Debug.LogWarning ($"Summoning patrol to actor {actorKey} | {actorOverworld.ToLog ()} | {actorPersistent.ToLog ()}");

			if (actorOverworld != null)
			{
				CommanderUtil.SendIntel (AIIntel.investigationRequest, actorOverworld.position.v, null, actorOverworld);
			}
			
			#endif
		}
	}
}
