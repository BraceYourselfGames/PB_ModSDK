using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ChangeOverworldActorFaction : IOverworldEventFunction, IOverworldEventParent
	{
		public DataContainerOverworldEvent ParentEvent { get; set; }

		[ValueDropdown ("@ParentEvent.GetActorKeys()")]
		public string actorKey;

		[ValueDropdown ("@Factions.GetList ()")]
		public string faction;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			if (faction == null)
			{
				Debug.LogWarning ($"Changing actor faction failed: no offset argument provided, second slot should have an integer");
				return;
			}

			var overworld = Contexts.sharedInstance.overworld;
			var actors = overworld.hasEventInProgressActorWorld ? overworld.eventInProgressActorWorld.persistentIDs : null;

			if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
			{
				Debug.LogWarning ($"Changing actor {actorKey} faction failed: no actors or failed to find actor with that key");
				return;
			}

			int actorPersistentID = actors[actorKey];
			var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);

			if (actorPersistent == null || !actorPersistent.isOverworldTag)
			{
				Debug.LogWarning ($"Changing actor {actorKey} faction failed: missing actor entity");
				return;
			}

			var factionCurrent = actorPersistent.faction.s;
			var factionNew = faction == Factions.player ? Factions.player : Factions.enemy;
			actorPersistent.ReplaceFaction (factionNew);

			Debug.Log ($"Changing actor {actorKey} faction | {actorPersistent.ToLog ()} | {factionCurrent} -> {factionNew}");
			
			#endif
		}
	}
}
