using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class OffsetOverworldActorThreat : IOverworldEventFunction, IOverworldEventParent
	{
		public DataContainerOverworldEvent ParentEvent { get; set; }

		[ValueDropdown ("@ParentEvent.GetActorKeys()")]
		public string actorKey;
		
		public int offset;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			var overworld = Contexts.sharedInstance.overworld;
			var actors = overworld.hasEventInProgressActorWorld ? overworld.eventInProgressActorWorld.persistentIDs : null;

			if (actors == null || string.IsNullOrEmpty (actorKey) || !actors.ContainsKey (actorKey))
			{
				Debug.LogWarning ($"Offsetting site actor {actorKey} threat failed: no actors or failed to find actor with that key");
				return;
			}

			int actorPersistentID = actors[actorKey];
			var actorPersistent = IDUtility.GetPersistentEntity (actorPersistentID);
			var actorOverworld = IDUtility.GetLinkedOverworldEntity (actorPersistent);

			if (actorPersistent == null || !actorPersistent.isOverworldTag || !actorPersistent.hasThreatRatingBase || !actorPersistent.hasCombatUnits)
			{
				Debug.LogWarning ($"Offsetting site actor {actorKey} threat failed: missing actor or {actorPersistent.ToLog ()} has no threat component or no garrison flag");
				return;
			}

			int threatRatingBaseCurrent = actorPersistent.hasThreatRatingBase ? actorPersistent.threatRatingBase.f : 0;
			int threatRatingBaseNew = Mathf.Clamp (threatRatingBaseCurrent + offset, 0, 1000);

			Debug.Log ($"Offsetting site actor {actorKey} threat rating | {actorPersistent.ToLog ()} | {threatRatingBaseCurrent} -> {threatRatingBaseNew}");
			actorPersistent.ReplaceThreatRatingBase (threatRatingBaseNew);
			
			#endif
		}
	}
}
