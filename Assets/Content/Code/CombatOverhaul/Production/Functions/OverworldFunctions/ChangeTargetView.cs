using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ChangeTargetView : IOverworldActionFunction
	{
		public string assetKey;

		public void Run (OverworldActionEntity source)
		{
			#if !PB_MODSDK

			var target = IDUtility.GetOverworldActionOverworldTarget (source);
			OverworldUtility.ChangeTargetView (target, assetKey);
			
			#endif
		}
	}
}
