using System;
using System.Linq;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ModifyPilotActorPersonality : IOverworldEventFunction, IOverworldEventFunctionEarly, IOverworldEventParent
	{
		public bool Early () => true;
		public DataContainerOverworldEvent ParentEvent { get; set; }

		[ValueDropdown ("@ParentEvent.GetActorKeys()")]
		public string actorKey;
		
		[HideIf ("random")]
		[ValueDropdown ("@DataMultiLinkerPilotPersonality.data.Keys")]
		public string personalityKey;
		
		public bool random = false;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			var overworld = Contexts.sharedInstance.overworld;
			var actors = overworld.hasEventInProgressActorPilots ? overworld.eventInProgressActorPilots.persistentIDs : null;
			if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
			{
				Debug.LogWarning ($"Failed to find pilot actor slot {actorKey} in event {eventData.key} to transfer base pilot");
				return;
			}
			
			var actorPersistentID = actors[actorKey];
			var pilot = IDUtility.GetPersistentEntity (actorPersistentID);
			var personalityKeyCurrent = pilot.hasPilotPersonality ? pilot.pilotPersonality.key : null;
			var personalityKeyNew = personalityKey;
			
			if (random)
			{
				var keys = DataMultiLinkerPilotPersonality.data.Keys.ToList ();
				if (!string.IsNullOrEmpty (personalityKeyCurrent) && keys.Contains (personalityKeyCurrent))
					keys.Remove (personalityKeyCurrent);
			
				personalityKeyNew = keys.GetRandomEntry ();
			}
			
			var personality = DataMultiLinkerPilotPersonality.GetEntry (personalityKey, false);
			if (personality == null)
			{
				Debug.LogWarning ($"Failed to find data for new pilot personality key {personalityKeyNew} to modify actor slot {actorKey} in event {eventData.key}");
				return;
			}

			pilot.ReplacePilotPersonality (personalityKeyNew);
			Debug.Log ($"Modified personality of pilot {pilot.ToLog ()} (actor slot {actorKey}) from {personalityKeyCurrent} to {personalityKeyNew}");
			
			#endif
		}
	}
}
