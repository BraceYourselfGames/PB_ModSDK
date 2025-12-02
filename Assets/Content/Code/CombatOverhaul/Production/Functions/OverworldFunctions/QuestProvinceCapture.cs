using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class QuestProvinceWarState : OverworldFunctionQuest, IOverworldFunction
	{
		public bool active = false;
		
		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null || string.IsNullOrEmpty (questState.province))
				return;

			var provincePersistent = IDUtility.GetPersistentEntity (questState.province);
			if (provincePersistent == null)
				return;
			
			var overworld = Contexts.sharedInstance.overworld;
			if (active)
				overworld.ReplaceProvinceAtWar (provincePersistent.id.id);
			else
			{
				if (overworld.hasProvinceAtWar)
					overworld.RemoveProvinceAtWar ();
			}

			#endif
		}
	}

	[Serializable]
	public class QuestProvinceCapture : OverworldFunctionQuest, IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null || string.IsNullOrEmpty (questState.province))
				return;
			
			var provincePersistent = IDUtility.GetPersistentEntity (questState.province);
			if (provincePersistent != null)
				DataHelperProvince.CaptureProvince (provincePersistent, false);

			#endif
		}
	}
}
