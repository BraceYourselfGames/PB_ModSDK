using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
	public class QuestFunctionPointQuery : OverworldFunctionQuest
	{
		[PropertyOrder (-2)]
		public bool onlyCombat;
		
		[PropertyOrder (-2)]
		public bool onlyHostile;
		
		[PropertyOrder (-2)]
		public List<IOverworldEntityValidationFunction> checks;
	}
	
	[Serializable]
	public class QuestDeletePoints : QuestFunctionPointQuery, IOverworldFunction
	{
		[HideInEditorMode, Button ("Test")]
		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null)
				return;

			var entities = OverworldPointUtility.GetActivePoints (onlyCombat, onlyHostile, questKeyOverride: questState.key, checks: checks);
			foreach (var entityOverworld in entities)
			{
				// // Hack to remove these entities from a recording if they are waiting to be presented
				// CIViewOverworldPointList.RecordingCancel (entityOverworld.id.id);
                    
				// Full destruction
				OverworldUtility.TryDestroySite (entityOverworld);
			}

			#endif
		}
	}
	
	[Serializable]
	public class QuestUnlinkPoints : QuestFunctionPointQuery, IOverworldFunction
	{
		[HideInEditorMode, Button ("Test")]
		public void Run ()
		{
			#if !PB_MODSDK
			
			var questState = GetQuestState ();
			if (questState == null)
				return;

			var entities = OverworldPointUtility.GetActivePoints (onlyCombat, onlyHostile, questKeyOverride: questState.key, checks: checks);
			foreach (var entityOverworld in entities)
			{
				if (entityOverworld.hasQuestLink)
					entityOverworld.RemoveQuestLink ();
			}

			#endif
		}
	}

	[Serializable]
	public class QuestReducePointsToCount : OverworldFunctionQuest, IOverworldFunction
	{
		public int countTarget = 1;
		
		[HideInEditorMode, Button ("Test")]
		public void Run ()
		{
			#if !PB_MODSDK
			
			var questState = GetQuestState ();
			if (questState == null)
				return;
			
			var entities = OverworldPointUtility.GetActivePoints (true, true, questKeyOverride: questState.key);
			int pointsToRemove = Mathf.Max (0, entities.Count - countTarget);
			if (pointsToRemove == 0)
			{
				Debug.Log ($"No points to remove for quest {questState.key} | Current entities: {entities.Count} | Target count: {countTarget}");
				return;
			}

			Debug.Log ($"Removing points for quest {questState.key} to achieve target count | Removals: {pointsToRemove} | Point count target: {countTarget} | Current active points: {entities.Count}");
			for (int i = 0; i < pointsToRemove; ++i)
			{
				var entityOverworldRemoved = entities.GetRandomEntry ();
				if (entityOverworldRemoved != null)
				{
					Debug.Log ($"- Removing point {entityOverworldRemoved.ToLog ()} ({entityOverworldRemoved.dataLinkPointPreset.data.key})");
					entities.Remove (entityOverworldRemoved);
					OverworldUtility.TryDestroySite (entityOverworldRemoved);
				}
			}
			
			#endif
		}
	}

	[Serializable]
	public class QuestProvinceCreatePointFilteredBatch : OverworldFunctionQuest, IOverworldFunction
	{
		public int pointLimit = 3;
		public int escalationMin = 0;
		public int escalationMax = 0;
		
		[DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"QuestProvinceCreatePointFilteredBatch\", \"GetTags\")")]
		[DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
		public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

		[DropdownReference]
		public List<IOverworldTargetedFunction> functionsOnSpawn;

		private IEnumerable<string> GetTags ()
		{
			var data = DataMultiLinkerOverworldPointPreset.data;
			return DataMultiLinkerOverworldPointPreset.tags;
		}
        
		[HideInEditorMode, Button ("Test")]
		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null)
				return;
			
			if (filter == null)
				return;

			OverworldPointUtility.GenerateNewPointBatch 
			(
				questState.province, 
				pointLimit, 
				filter, 
				questState.key, 
				new Vector2Int (escalationMin, escalationMax),
				functionsOnSpawn
			);

			#endif
		}
		
		#region Editor
		#if UNITY_EDITOR
        
		[ShowInInspector]
		private DataEditor.DropdownReferenceHelper helper;
        
		public QuestProvinceCreatePointFilteredBatch () => 
			helper = new DataEditor.DropdownReferenceHelper (this);
        
		#endif
		#endregion
	}

	public class CreateSimplePatrol : IOverworldFunction, IFunctionLocalizedText
	{
		private static SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> { { "role_patrol", true } };
		private static List<IOverworldTargetedFunction> functionsOnSpawn = new List<IOverworldTargetedFunction> { new OverworldEntityExpiration { duration = 6f } };
		private static Vector2 spawnRange = new Vector2 (150f, 200f);
		
		public void Run ()
		{
			#if !PB_MODSDK

			var provinceActive = DataHelperProvince.GetProvinceKeyActive ();
			if (string.IsNullOrEmpty (provinceActive))
				return;
			
			OverworldPointUtility.GenerateNewPoint 
			(
				null, 
				provinceActive, 
				null, 
				questKeyLinked: null, 
				tagFilterExternal: filter,
				functionsOnSpawn: functionsOnSpawn,
				spawnRangeOverride: spawnRange,
				spawnDistancePriority: PointDistancePriority.None
                
			);
			
			#endif
		}
		
		public string GetLocalizedText ()
		{
			#if !PB_MODSDK
			
			return Txt.Get (TextLibs.uiOverworld, "int_sh_effect_patrol_spawn");

			#else
            return null;
			#endif
		}
	}
	
	[Serializable]
	public class QuestProvinceCreatePointFiltered : OverworldFunctionQuest, IOverworldFunction
	{
		[PropertyOrder (-1)]
		public Vector2 spawnRange = new Vector2 (0, 0);	
		
		[PropertyOrder (-1)]
		public PointDistancePriority spawnPriority = PointDistancePriority.None;
		
		[DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"QuestProvinceCreatePointFiltered\", \"GetTags\")")]
		[DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
		public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

		[DropdownReference (true)]
		public string spawnGroupOverride;
		
		[DropdownReference (true)]
		public string nameOverride;
		
		[DropdownReference]
		public List<IOverworldTargetedFunction> functionsOnSpawn;

		private IEnumerable<string> GetTags ()
		{
			var data = DataMultiLinkerOverworldPointPreset.data;
			return DataMultiLinkerOverworldPointPreset.tags;
		}
        
		[HideInEditorMode, Button ("Test")]
		public virtual void Run ()
		{
			#if !PB_MODSDK
			
			var questState = GetQuestState ();
			if (questState == null)
				return;

			if (filter == null)
				return;
			
			OverworldPointUtility.GenerateNewPoint 
			(
				null, 
				questState.province, 
				nameOverride, 
				questKeyLinked: questState.key, 
				tagFilterExternal: filter,
				functionsOnSpawn: functionsOnSpawn,
				spawnRangeOverride: spawnRange,
				spawnGroupOverride: spawnGroupOverride,
				spawnDistancePriority: spawnPriority
                
			);

			#endif
		}
		
		#region Editor
		#if UNITY_EDITOR
        
		[ShowInInspector]
		private DataEditor.DropdownReferenceHelper helper;
        
		public QuestProvinceCreatePointFiltered () => 
			helper = new DataEditor.DropdownReferenceHelper (this);
        
		#endif
		#endregion
	}
	
	[Serializable]
	public class QuestProvinceCreatePoint : OverworldFunctionQuest, IOverworldFunction
	{
		[ValueDropdown (nameof(GetKeys))]
		public string key;

		[DropdownReference (true)]
		public string spawnGroupOverride;
		
		[DropdownReference (true)]
		public string nameOverride;
		
		[DropdownReference]
		public List<IOverworldTargetedFunction> functionsOnSpawn;

		private IEnumerable<string> GetKeys () => DataMultiLinkerOverworldPointPreset.GetKeys ();
        
		[HideInEditorMode, Button ("Test")]
		public void Run ()
		{
			#if !PB_MODSDK
			
			var questState = GetQuestState ();
			if (questState == null)
				return;

			if (string.IsNullOrEmpty (key))
				return;

			OverworldPointUtility.GenerateNewPoint 
			(
				key, 
				questState.province, 
				nameOverride, 
				questState.key,
				spawnRangeOverride: new Vector2 (0f, 100000f),
				spawnGroupOverride: spawnGroupOverride,
				functionsOnSpawn: functionsOnSpawn
			);

			#endif
		}
		
		#region Editor
		#if UNITY_EDITOR
        
		[ShowInInspector]
		private DataEditor.DropdownReferenceHelper helper;
        
		public QuestProvinceCreatePoint () => 
			helper = new DataEditor.DropdownReferenceHelper (this);
        
		#endif
		#endregion
	}
}
