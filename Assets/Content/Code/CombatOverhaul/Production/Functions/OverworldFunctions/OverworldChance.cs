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
	public class OverworldChance : IOverworldFunction
	{
		[PropertyRange (0f, 1f)]
		public float chance = 0.5f;
        
		[BoxGroup, HideLabel]
		public IOverworldFunction function;

		public void Run ()
		{
			#if !PB_MODSDK
            
			var chanceFinal = Mathf.Clamp01 (chance);
			bool passed = Random.Range (0f, 1f) < chanceFinal;
			if (passed && function != null)
				function.Run ();
            
			#endif
		}
	}
    
	[Serializable]
	public class OverworldChanceGroup : IOverworldFunction
	{
		[PropertyRange (0f, 1f)]
		public float chance = 0.5f;
		public List<IOverworldFunction> functions;

		public void Run ()
		{
			#if !PB_MODSDK
            
			var chanceFinal = Mathf.Clamp01 (chance);
			bool passed = Random.Range (0f, 1f) < chanceFinal;
			if (passed && functions != null)
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
	
	[Serializable]
	public class OverworldChanceBranch : IOverworldFunction
	{
		[PropertyRange (0f, 1f)]
		public float chance = 0.5f;
        
		[LabelText ("Then")]
		public IOverworldFunction function;
        
		[LabelText ("Else")]
		public IOverworldFunction functionElse;

		public void Run ()
		{
			#if !PB_MODSDK
            
			var chanceFinal = Mathf.Clamp01 (chance);
			bool passed = Random.Range (0f, 1f) < chanceFinal;
			if (passed)
			{
				if (function != null)
					function.Run ();
			}
			else
			{
				if (functionElse != null)
					functionElse.Run ();
			}
            
			#endif
		}
	}
	
	[Serializable]
	public class OverworldChanceBranchGroup : IOverworldFunction
	{
		[PropertyRange (0f, 1f)]
		public float chance = 0.5f;
        
		[LabelText ("Then")]
		public List<IOverworldFunction> functions;
        
		[LabelText ("Else")]
		public List<IOverworldFunction> functionsElse;

		public void Run ()
		{
			#if !PB_MODSDK
            
			var chanceFinal = Mathf.Clamp01 (chance);
			bool passed = Random.Range (0f, 1f) < chanceFinal;
			if (passed)
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
			else
			{
				if (functionsElse != null)
				{
					foreach (var function in functionsElse)
					{
						if (function != null)
							function.Run ();
					}
				}
			}
            
			#endif
		}
	}
	
	[Serializable]
	public class OverworldChanceWeighted : IOverworldFunction
	{
		public Dictionary<float, IOverworldFunction> functions = new Dictionary<float, IOverworldFunction> ();

		public void Run ()
		{
			#if !PB_MODSDK

			var weightSum = 0f;
			if (functions == null)
				return;

			foreach (var kvp in functions)
				weightSum += kvp.Key;

			float triggerPoint = Random.Range (0f, weightSum);
			float valueAccumulated = 0f;
			
			foreach (var kvp in functions)
			{
				valueAccumulated += kvp.Key;
				if (triggerPoint > valueAccumulated)
					continue;
				
				var function = kvp.Value;
				if (function != null)
					function.Run ();
				break;
			}
            
			#endif
		}
	}
	
	[Serializable]
	public class OverworldChanceWeightedGroup : IOverworldFunction
	{
		public Dictionary<float, List<IOverworldFunction>> functions = new Dictionary<float, List<IOverworldFunction>> ();

		public void Run ()
		{
			#if !PB_MODSDK

			var weightSum = 0f;
			if (functions == null)
				return;

			foreach (var kvp in functions)
				weightSum += kvp.Key;

			float triggerPoint = Random.Range (0f, weightSum);
			float valueAccumulated = 0f;
			
			foreach (var kvp in functions)
			{
				valueAccumulated += kvp.Key;
				if (triggerPoint > valueAccumulated)
					continue;
				
				var functionsUsed = kvp.Value;
				if (functionsUsed != null)
				{
					foreach (var function in functionsUsed)
					{
						if (function != null)
							function.Run ();
					}
				}
				break;
			}
            
			#endif
		}
	}
}
