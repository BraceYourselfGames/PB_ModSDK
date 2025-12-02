using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class OverworldCampaignEffect : IOverworldFunction
	{
		public string effectKey;
        
		[BoxGroup, HideLabel]
		public DataBlockCampaignEffect effectFallback;

		public void Run ()
		{
			#if !PB_MODSDK

			var overworld = Contexts.sharedInstance.overworld;
			var campaignState = overworld.hasCampaignState ? overworld.campaignState.s : null;
			var campaignStepCurrent = DataMultiLinkerOverworldCampaignStep.GetEntry (campaignState?.stepKey);

			if (campaignStepCurrent == null)
			{
				Debug.LogWarning ($"OverworldCampaignEffect | Can't proceed, current campaign state points to invalid campaign step key {campaignState?.stepKey}");
				return;
			}

			bool effectExecuted = campaignStepCurrent.TryExecutingEffect (effectKey);
			if (!effectExecuted && effectFallback != null)
				effectFallback.Run ();

			#endif
		}
	}
}
