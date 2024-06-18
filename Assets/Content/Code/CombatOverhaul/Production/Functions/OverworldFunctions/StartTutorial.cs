using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class StartTutorial : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction, ICombatFunctionDelayed
	{
		[ValueDropdown("@DataMultiLinkerTutorial.data.Keys")]
		public string key;

		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			CIViewTutorial.ins.OnTutorialStartFromKey (key, true);
			
			#endif
		}
		
		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK

			CIViewTutorial.ins.OnTutorialStartFromKey (key, true);
			
			#endif
		}

		public void Run ()
		{
			#if !PB_MODSDK

			CIViewTutorial.ins.OnTutorialStartFromKey (key, true);
			
			#endif
		}

		public bool IsDelayed () => true;
	}
}
