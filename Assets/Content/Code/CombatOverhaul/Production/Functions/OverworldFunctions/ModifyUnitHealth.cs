using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ModifyUnitHealth : IOverworldEventFunction, IOverworldActionFunction, IOverworldFunction, ICombatFunction
	{
		[Tooltip ("How many units can be affected")]
		[PropertyRange (1, 16)]
		public int units = 4;
		
		[Tooltip ("What proportion of random unit parts (0-1) will be affected")]
		[PropertyRange (0f, 1f)]
		public float proportion = 0.5f;
		
		[InfoBox ("Warning! Value of 0 means selected parts will be irreversibly destroyed.", InfoMessageType.Warning, VisibleIf = "@value <= 0f")]
		[Tooltip ("What the new HP would be (0-1)")]
		[PropertyRange (0f, 1f)]
		public float value = 0.5f;
		
		public void Run (OverworldEntity target, DataContainerOverworldEvent eventData)
		{
			Run ();
		}
		
		public void Run (OverworldActionEntity source)
		{
			Run ();
		}

		public void Run ()
		{
			#if !PB_MODSDK
            
			EquipmentUtility.ModifyPlayerUnitHealth (units, proportion, value);
			
			#endif
		}
	}
}
