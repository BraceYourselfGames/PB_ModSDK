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
	public class RebuildTravelPoints : OverworldFunctionQuest, IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null || string.IsNullOrEmpty (questState.province))
				return;
			
			DataHelperProvince.RebuildTravelPoints (questState.key);

			#endif
		}
	}
	
	[Serializable]
	public class UpdateCampaignOnProvinceArrival : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK

			DataHelperProvince.UpdateCampaignOnProvinceArrival ();

			#endif
		}
	}
	
	[Serializable]
	public class UpdateCampaignOnFullStart : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK
			
			CIViewLoader.SetGameplayUIVisible (false);
			PostprocessingHelper.SetBlackoutTarget (1f, true);

			#endif
		}
	}
	
	[Serializable]
	public class DelayGroupRealtime : IOverworldFunction
	{
		public float delay;
		public List<IOverworldFunction> functions = new List<IOverworldFunction> ();
		
		public void Run ()
		{
			#if !PB_MODSDK

			if (delay > 0f)
				Co.Delay (delay, RunDelayed);
			else
				RunDelayed ();

			#endif
		}

		private void RunDelayed ()
		{
			if (functions != null)
			{
				foreach (var function in functions)
				{
					if (function != null)
						function.Run ();
				}
			}
		}
	}
}
