using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class OffsetTargetThreat : IOverworldActionFunction
	{
		public int offset;

		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK
            
			var target = IDUtility.GetOverworldActionOwner (source);
			Run (target);
			
			#endif
		}

		private void Run (OverworldEntity target)
		{
			#if !PB_MODSDK
            
			if (target == null)
			{
				Debug.LogWarning ($"Offsetting target threat failed due to missing target ({target.ToStringNullCheck ()})");
				return;
			}

			var targetPersistent = IDUtility.GetLinkedPersistentEntity (target);
			if (targetPersistent == null || targetPersistent == IDUtility.playerBasePersistent)
			{
				Debug.LogWarning ($"Offsetting target threat failed due to missing persistent counterpart or due to match with player base ({target.ToStringNullCheck ()})");
				return;
			}

			if (!targetPersistent.hasThreatRatingBase || !targetPersistent.hasCombatUnits)
			{
				Debug.LogWarning ($"Offsetting target threat failed: {targetPersistent.ToLog ()} has no threat component or no garrison flag");
				return;
			}

			int threatRatingBaseCurrent = targetPersistent.hasThreatRatingBase ? targetPersistent.threatRatingBase.f : 0;
			int threatRatingBaseNew = Mathf.Clamp (threatRatingBaseCurrent + offset, 0, 1000);

			Debug.Log ($"Offsetting target threat rating | {targetPersistent.ToLog ()} | {threatRatingBaseCurrent} -> {threatRatingBaseNew}");
			targetPersistent.ReplaceThreatRatingBase (threatRatingBaseNew);
			
			#endif
		}
	}
}
