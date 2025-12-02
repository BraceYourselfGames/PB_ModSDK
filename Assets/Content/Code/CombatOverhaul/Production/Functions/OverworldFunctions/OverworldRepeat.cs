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
	public class OverworldRepeat : IOverworldFunction
	{
		public int repeats = 1;
        
		[BoxGroup, HideLabel]
		public IOverworldFunction function;

		public void Run ()
		{
			#if !PB_MODSDK
            
			if (repeats <= 0 || function == null)
				return;

			for (int i = 0; i < repeats; ++i)
				function.Run ();

			#endif
		}
	}
    
	[Serializable]
	public class OverworldRepeatGroup : IOverworldFunction
	{
		public int repeats = 1;
        
		public List<IOverworldFunction> functions;

		public void Run ()
		{
			#if !PB_MODSDK

			if (repeats <= 0 || functions == null)
				return;
            
			for (int i = 0; i < repeats; ++i)
			{
				foreach (var function in functions)
				{
					if (function != null)
						function.Run ();
				}
			}
            
			#endif
		}
	}
}
