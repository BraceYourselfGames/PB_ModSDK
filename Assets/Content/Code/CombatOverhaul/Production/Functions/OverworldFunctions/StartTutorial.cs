using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class StartTutorial : IOverworldActionFunction, IOverworldFunction, ICombatFunction, ICombatFunctionDelayed
	{
		[ValueDropdown("@DataMultiLinkerTutorial.data.Keys")]
		public string key;
		public bool allowRepeat = true;
		
		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK

			CIViewTutorial.ins.OnTutorialStartFromKey (key, allowRepeat);
			
			#endif
		}

		public void Run ()
		{
			#if !PB_MODSDK

			CIViewTutorial.ins.OnTutorialStartFromKey (key, allowRepeat);
			
			#endif
		}

		public bool IsDelayed () => true;
	}
}
