using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class ModifyUnitHealth : IOverworldActionFunction, IOverworldFunction, ICombatFunction
	{
		[InfoBox ("All units will be affected", VisibleIf = "@units <= 0")]
		[Tooltip ("How many units can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int units = 0;

		[Tooltip ("What the new HP would be (0-1)")]
		[PropertyRange (0f, 1f)]
		public float value = 0.5f;

		public void Run (OverworldActionEntity source)
		{
			Run ();
		}

		public void Run ()
		{
			#if !PB_MODSDK
            
			EquipmentUtility.ModifyPlayerUnitsFrameIntegrity (units, value);
			
			#endif
		}
	}

	public class ModifyFrameHealth : ICombatFunctionTargeted
	{
		[PropertyRange (-1f, 1f)]
		public float offsetNormalized = 0f;
		
		public void Run (PersistentEntity unitPersistent)
		{
			#if !PB_MODSDK
			
			if (unitPersistent == null)
				return;
			
			DataHelperStats.GetUnitEHP (unitPersistent, out var ehpCurrent, out var ehpMax, out var ehpNormalized, out int ehpSocketsUsed);
			var ehpNormalizedModified = Mathf.Clamp01 (ehpNormalized + offsetNormalized);
			EquipmentUtility.ModifyUnitFrameIntegrity (unitPersistent, ehpNormalizedModified);
			
			#endif
		}
	}
	
	[Serializable]
	public class OverworldModifyPlayerUnits : IOverworldFunction
	{
        [InfoBox ("All units will be affected", VisibleIf = "@units <= 0")]
		[Tooltip ("How many units can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int units = 0;
		
		public List<ICombatUnitValidationFunction> checks;
		public List<ICombatFunctionTargeted> functions = new List<ICombatFunctionTargeted> ();

		public void Run ()
		{
			#if !PB_MODSDK
            
			UnitUtilities.ModifyPlayerUnits (units, false, functions, checks);
			
			#endif
		}
	}
	
	[Serializable]
	public class CombatModifyPlayerUnits : IOverworldFunction, ICombatFunction
	{
		[InfoBox ("All units will be affected", VisibleIf = "@units <= 0")]
		[Tooltip ("How many units can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int units = 0;

		public List<ICombatUnitValidationFunction> checks;
		public List<ICombatFunctionTargeted> functions = new List<ICombatFunctionTargeted> ();

		public void Run ()
		{
			#if !PB_MODSDK
            
			UnitUtilities.ModifyPlayerUnits (units, true, functions, checks);
			
			#endif
		}
	}
	
	[Serializable]
	public class CombatModifyUnits : IOverworldFunction, ICombatFunction
	{
		[InfoBox ("All units will be affected", VisibleIf = "@units <= 0")]
		[Tooltip ("How many units can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int units = 0;

		public List<ICombatUnitValidationFunction> checks;
		public List<ICombatFunctionTargeted> functions = new List<ICombatFunctionTargeted> ();

		public void Run ()
		{
			#if !PB_MODSDK
            
			UnitUtilities.ModifyUnits (units, true, functions, checks);
			
			#endif
		}
	}
	
	[Serializable]
	public class ModifyUnitsOverworld : IOverworldFunction
	{
		[InfoBox ("All units will be affected", VisibleIf = "@units <= 0")]
		[Tooltip ("How many units can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int units = 0;

		public List<IOverworldUnitValidationFunction> checks;
		public List<IOverworldUnitFunction> functions = new List<IOverworldUnitFunction> ();

		public void Run ()
		{
			#if !PB_MODSDK
            
			UnitUtilities.ModifyPlayerUnits (units, functions, checks);
			
			#endif
		}
	}
	
	[Serializable]
	public class ModifyPilots : IOverworldFunction, ICombatFunction
	{
		[InfoBox ("All pilots will be affected", VisibleIf = "@pilots <= 0")]
		[Tooltip ("How many pilots can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int pilots = 0;
		public bool deployed = false;
		
		public List<IPilotValidationFunction> checks;
		public List<IPilotTargetedFunction> functions = new List<IPilotTargetedFunction> ();

		public void Run ()
		{
			#if !PB_MODSDK
            
			PilotUtility.ModifyPlayerPilots (pilots, deployed, functions, checks);
			
			#endif
		}
	}
	
	[Serializable]
	public class ModifyOverworldPoints : IOverworldFunction
	{
		[InfoBox ("All points will be affected", VisibleIf = "@count <= 0")]
		[Tooltip ("How many points can be affected")]
		[PropertyRange (0, 16), PropertyOrder (-1)]
		public int count = 0;

		public bool onlyCombat = true;
		public bool onlyHostile = true;

		public List<IOverworldEntityValidationFunction> checks;
		public List<IOverworldTargetedFunction> functions = new List<IOverworldTargetedFunction> ();

		public void Run ()
		{
			#if !PB_MODSDK
			
			if (functions == null || functions.Count == 0)
				return;

			var points = OverworldPointUtility.GetActivePoints (true, true, checks: checks);
			if (points != null && points.Count > 0)
			{
				foreach (var point in points)
				{
					if (point == null)
						return;

					foreach (var function in functions)
					{
						if (function != null)
							function.Run (point);
					}
				}
			}

			#endif
		}
	}
}
