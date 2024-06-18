using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class PlayCutscene : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction
	{
		[ValueDropdown("@DataMultiLinkerCutsceneVideo.data.Keys")]
		public string key;
		
		public bool instant;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			CutsceneService.TryPlayingCutsceneVideo (key, instant);
			
			#endif
		}
		
		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK

			CutsceneService.TryPlayingCutsceneVideo (key, instant);
			
			#endif
		}

		public void Run ()
		{
			#if !PB_MODSDK

			CutsceneService.TryPlayingCutsceneVideo (key, instant);
			
			#endif
		}
	}
}
