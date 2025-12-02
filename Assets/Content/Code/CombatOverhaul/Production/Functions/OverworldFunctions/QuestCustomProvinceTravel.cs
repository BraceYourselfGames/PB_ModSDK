using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
	public class OverworldTravelFrontline : OverworldTravelBase, IOverworldTargetedFunction
	{
		public void Run (OverworldEntity entityOverworld)
		{
			#if !PB_MODSDK

			if (entityOverworld == null || !entityOverworld.hasFrontlineBase)
			{
				Debug.Log ("Can't travel to next province: no frontline component");
				return;
			}

			var frontlineLink = entityOverworld.frontlineBase;
			var destinationOverworld = IDUtility.GetOverworldEntity (frontlineLink.siteNameInternalConnected);
			RunBase (frontlineLink.provinceKeyConnected, destinationOverworld);

			#endif
		}
	}
	
	public class OverworldTravel : OverworldTravelBase, IOverworldFunction
	{
		[ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
		public string provinceKey;
		
		public void Run ()
		{
			#if !PB_MODSDK
			
			RunBase (provinceKey, null);

			#endif
		}
	}
	
	public class OverworldTravelBase
	{
		[DropdownReference]
		public List<IOverworldFunction> functionsGlobal;
        
		[DropdownReference]
		public List<IOverworldTargetedFunction> functionsBase;
        
		[DropdownReference]
		public List<IOverworldTargetedFunction> functionsDestination;

		public void RunBase (string provinceKey, OverworldEntity destinationOverworld)
		{
			#if !PB_MODSDK
			bool loadingStarted = DataHelperProvince.TryLoadingProvince 
			(
				provinceKey, 
				destinationOverworld,
				functionsGlobal,
				functionsBase,
				functionsDestination
			);
			
			if (!loadingStarted)
				return;
			#endif
		}
		
		#region Editor
		#if UNITY_EDITOR

		[ShowInInspector]
		private DataEditor.DropdownReferenceHelper helper;

		public OverworldTravelBase () =>
			helper = new DataEditor.DropdownReferenceHelper (this);

		#endif
		#endregion
	}
	
	public class OverworldAfterTravelDebug : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK
			
			

			#endif
		}
	}
}
