using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ChangeTargetView : IOverworldEventFunction, IOverworldActionFunction
	{
		public string assetKey;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			#if !PB_MODSDK

			OverworldIndirectFunctions.ChangeTargetView (target, assetKey);
			
			#endif
		}
        
		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK

			var target = IDUtility.GetOverworldActionOverworldTarget (source);
			OverworldIndirectFunctions.ChangeTargetView (target, assetKey);
			
			#endif
		}
	}
}
