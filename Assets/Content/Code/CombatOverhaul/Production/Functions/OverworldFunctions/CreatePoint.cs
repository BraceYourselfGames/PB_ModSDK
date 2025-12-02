using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	[Serializable]
	public class CreatePointFilteredBatch : IOverworldFunction
	{
		public int pointLimit = 3;
		
		[DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"CreatePointFilteredBatch\", \"GetTags\")")]
		[DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
		public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

		private IEnumerable<string> GetTags ()
		{
			var data = DataMultiLinkerOverworldPointPreset.data;
			return DataMultiLinkerOverworldPointPreset.tags;
		}
        
		public void Run ()
		{
			#if !PB_MODSDK

			OverworldPointUtility.GenerateNewPointBatch (null, pointLimit, filter);

			#endif
		}
	}
	
	[Serializable]
	public class CreatePointRandom : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK

			OverworldPointUtility.GenerateNewPoint ();
			
			#endif
		}
	}
	
	[Serializable]
	public class CreatePointFiltered : IOverworldFunction
	{
		[DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"CreatePointFiltered\", \"GetTags\")")]
		[DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
		public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

		private IEnumerable<string> GetTags ()
		{
			var data = DataMultiLinkerOverworldPointPreset.data;
			return DataMultiLinkerOverworldPointPreset.tags;
		}
        
		public void Run ()
		{
			#if !PB_MODSDK

			if (filter == null)
				return;
			
			var data = DataMultiLinkerOverworldPointPreset.data;
			var keys = DataTagUtility.GetKeysWithTags (data, filter);
			var key = keys.GetRandomEntry ();
			
			OverworldPointUtility.GenerateNewPoint (key);

			#endif
		}
	}

	[Serializable]
	public class DataFilterPointPreset : DataBlockFilterLinked<DataContainerOverworldPointPreset>
	{
		public override IEnumerable<string> GetTags () => DataMultiLinkerOverworldPointPreset.GetTags ();
		public override SortedDictionary<string, DataContainerOverworldPointPreset> GetData () => DataMultiLinkerOverworldPointPreset.data;
	}

	[Serializable]
	public class CreatePoint : DataFilterPointPreset, IOverworldFunction
	{
		[PropertyOrder (-2)]
		public PointDistancePriority spawnPriority = PointDistancePriority.None;
		
		[PropertyOrder (-2)]
		[DropdownReference (true)]
		public string spawnGroup;
		
		[PropertyOrder (-2)]
		[DropdownReference (true)]
		public DataBlockVector2 spawnRange;
		
		[PropertyOrder (-2)]
		[DropdownReference (true)]
		[ValueDropdown("$GetProvinceKeys")]
		public string provinceOverride;
		
		[PropertyOrder (-2)]
		[DropdownReference (true)]
		public string nameOverride;
		
		[PropertyOrder (-2)]
		[DropdownReference (true)]
		[ValueDropdown("$GetQuestKeys")]
		public string questKey;
		
		[PropertyOrder (2)]
		[DropdownReference]
		public List<IOverworldTargetedFunction> functionsOnSpawn;
        
		public void Run ()
		{
			#if !PB_MODSDK
			
			var key = GetRandomKey (DataMultiLinkerOverworldPointPreset.data);
			if (string.IsNullOrEmpty (key))
				return;

			OverworldPointUtility.GenerateNewPoint 
			(
				key, 
				provinceOverride, 
				nameOverride, 
				questKey, 
				null,
				spawnRange != null ? spawnRange.v : default,
				spawnGroup,
				spawnPriority,
				null,
				functionsOnSpawn
			);

			#endif
		}
		
		#region Editor
		#if UNITY_EDITOR
        
		[ShowInInspector, PropertyOrder (100)]
		private DataEditor.DropdownReferenceHelper helper;

		public CreatePoint () => 
			helper = new DataEditor.DropdownReferenceHelper (this);

		private IEnumerable<string> GetProvinceKeys () => DataMultiLinkerOverworldProvinceBlueprints.data.Keys;
		private IEnumerable<string> GetQuestKeys () => DataMultiLinkerOverworldQuest.data.Keys;

		#endif
		#endregion
	}
	
	public class PointQuery
	{
		[PropertyOrder (-2)]
		public bool onlyCombat;
		
		[PropertyOrder (-2)]
		public bool onlyHostile;
		
		[PropertyOrder (-2)]
		public List<IOverworldEntityValidationFunction> checks;
	}

	public class OverworldIntValuePointInstances : PointQuery, IOverworldIntValueFunction
	{
		public int Resolve (OverworldEntity entityOverworld)
		{
			#if !PB_MODSDK
			
			var entities = OverworldPointUtility.GetActivePoints (onlyCombat, onlyHostile, checks: checks);
			return entities.Count;

			#else
			return 0;
			#endif
		}
	}
	
	public class DeletePoints : PointQuery, IOverworldFunction
	{
		public enum Mode
		{
			All,
			DeleteUntilTarget,
			DeleteByTarget
		}

		public Mode mode = Mode.DeleteUntilTarget;
		
		[HideIf ("@mode == Mode.All")]
		public int target = 0;
		
		public void Run ()
		{
			#if !PB_MODSDK
			
			var entities = OverworldPointUtility.GetActivePoints (onlyCombat, onlyHostile, checks: checks);
			int entitiesCount = entities.Count;
			int pointsToRemove = 0;
			
			if (mode == Mode.All)
				pointsToRemove = entities.Count;
			else if (mode == Mode.DeleteByTarget)
				pointsToRemove = Mathf.Min (entities.Count, target);
			else if (mode == Mode.DeleteUntilTarget)
				pointsToRemove = Mathf.Max (0, entities.Count - target);
			
			if (pointsToRemove == 0)
			{
				Debug.Log ($"No points to remove | Current entities: {entitiesCount} | Target: {target} | Mode: {mode}");
				return;
			}

			if (pointsToRemove >= entitiesCount)
			{
				Debug.Log ($"Removing all filtered points: {entitiesCount}");
				foreach (var entityOverworld in entities)
					OverworldUtility.TryDestroySite (entityOverworld);
			}
			else
			{
				Debug.Log ($"Removing {pointsToRemove} filtered points | Current entities: {entitiesCount} | Target: {target} | Mode: {mode}");
				for (int i = 0; i < pointsToRemove; ++i)
				{
					var entityOverworld = entities.GetRandomEntry ();
					if (entityOverworld != null)
					{
						Debug.Log ($"- Removing point {entityOverworld.ToLog ()} ({entityOverworld.dataLinkPointPreset.data.key})");
						entities.Remove (entityOverworld);
						OverworldUtility.TryDestroySite (entityOverworld);
					}
				}
			}
			
			#endif
		}
		
		#region Editor
		#if UNITY_EDITOR
        
		[ShowInInspector, PropertyOrder (100)]
		private DataEditor.DropdownReferenceHelper helper;

		public DeletePoints () => 
			helper = new DataEditor.DropdownReferenceHelper (this);
			
		#endif
		#endregion
	}
}
