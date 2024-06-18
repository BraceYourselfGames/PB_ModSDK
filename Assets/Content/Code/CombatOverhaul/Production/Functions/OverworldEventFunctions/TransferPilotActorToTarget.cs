using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class TransferPilotActorToTarget : IOverworldEventFunction, IOverworldEventFunctionEarly, IOverworldEventParent
	{
		public bool Early () => true;
		public DataContainerOverworldEvent ParentEvent { get; set; }

		[ValueDropdown ("@ParentEvent.GetActorKeys()")]
		public string actorKey;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);

			var overworld = Contexts.sharedInstance.overworld;
			var actors =
				overworld.hasEventInProgressActorPilots ?
					overworld.eventInProgressActorPilots.persistentIDs :
					null;

			if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
			{
				Debug.LogWarning ($"Failed to find pilot actor slot {actorKey} in event {eventData.key} to transfer base pilot");
				return;
			}

			var actorPersistentID = actors[actorKey];
			var pilot = IDUtility.GetPersistentEntity (actorPersistentID);
			pilot.ReplaceEntityLinkPersistentParent (targetPersistent.id.id);

			Debug.LogWarning ($"Transferring preexisting pilot {pilot.ToLog ()} to persistent parent {targetPersistent.ToLog ()}");
			Contexts.sharedInstance.persistent.isPlayerCombatReadinessChecked = true;
			
			#endif
		}
	}
}
