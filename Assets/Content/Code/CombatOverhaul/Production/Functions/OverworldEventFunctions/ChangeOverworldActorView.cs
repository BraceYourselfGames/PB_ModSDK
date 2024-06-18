using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ChangeOverworldActorView : IOverworldEventFunction, IOverworldEventParent
	{
		public DataContainerOverworldEvent ParentEvent { get; set; }
		
		[ValueDropdown("@ParentEvent.GetActorKeys ()")]
		public string actorKey;
		
		public string assetKey;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			var overworld = Contexts.sharedInstance.overworld;
			var actors = overworld.hasEventInProgressActorWorld ? overworld.eventInProgressActorWorld.persistentIDs : null;

			if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
			{
				Debug.LogWarning ($"Changing actor {actorKey} view failed: no actors or failed to find actor with that key");
				return;
			}

			int actorPersistentID = actors[actorKey];
			var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);
			var actorOverworld = IDUtility.GetLinkedOverworldEntity (actorPersistent);

			if (actorPersistent == null || !actorPersistent.isOverworldTag || actorOverworld == null)
			{
				Debug.LogWarning ($"Changing actor {actorKey} view failed: missing actor");
				return;
			}

			OverworldIndirectFunctions.ChangeTargetView (actorOverworld, assetKey);
			
			#endif
		}
	}
}
