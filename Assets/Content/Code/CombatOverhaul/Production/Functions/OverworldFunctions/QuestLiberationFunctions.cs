using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Functions
{
	public class ApplyProvinceArrivalEffects : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK
			
			bool provinceActiveFound = DataHelperProvince.TryGetProvinceDependenciesActive 
			(
				out var provinceActiveBlueprint, 
				out var provinceActivePersistent, 
				out var provinceActiveOverworld
			);
			
			if (!provinceActiveFound)
			{
				Debug.LogWarning ($"Can't find active province");
				return;
			}
			
			DataHelperProvince.OnArrivalScreenExit ();
			
			#endif
		}
	}

	public enum QuestLiberationSpawnMode
	{
		OrderedAll,
		ShuffleAll,
		ShuffleSubset
	}

	[Serializable]
	public class QuestActiveApplyEffectGroup : OverworldFunctionQuest, IOverworldFunction
	{
		[ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerOverworldQuest\", \"GetEffectKeys\")")]
		public string effectKey;
		
		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null)
				return;

			var questData = DataMultiLinkerOverworldQuest.GetEntry (questState.key);
			if (questData == null || questData.effectsSharedProc == null)
				return;
			
			if (string.IsNullOrEmpty (effectKey) || !questData.effectsSharedProc.TryGetValue (effectKey, out var effect))
				return;
			
			OverworldQuestUtility.ApplyEffectGroup (questData, effect);

			#endif
		}
	}

	[Serializable]
	public class QuestActivePointConditional : OverworldFunctionQuest, IOverworldFunction
	{
		[BoxGroup ("Active Points", false)]
		public DataBlockOverworldEventSubcheckInt countCheck = new DataBlockOverworldEventSubcheckInt ();
		
		[DropdownReference]
		public List<IOverworldEntityValidationFunction> pointChecks;
		
		[LabelText ("Then")]
		[DropdownReference]
		public List<IOverworldFunction> functions;
        
		[LabelText ("Else")]
		[DropdownReference]
		public List<IOverworldFunction> functionsElse;

		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null || countCheck == null)
				return;

			var entities = OverworldPointUtility.GetActivePoints (true, true, questKeyOverride: questState.key, checks: pointChecks);
			bool passed = countCheck.IsPassed (true, entities.Count);
			
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
		
		#region Editor
		#if UNITY_EDITOR
        
		[ShowInInspector]
		private DataEditor.DropdownReferenceHelper helper;
        
		public QuestActivePointConditional () => 
			helper = new DataEditor.DropdownReferenceHelper (this);
        
		#endif
		#endregion
	}
	
	[Serializable]
	public class OverworldOffsetCountdownStartTime : IOverworldFunction, IFunctionLocalizedText
	{
		public int hours = 0;

		public void Run ()
		{
			#if !PB_MODSDK
			
			var overworld = Contexts.sharedInstance.overworld;
			if (!overworld.hasOverworldResetCountdown)
				return;

			var c = overworld.overworldResetCountdown;
			float currentTime = overworld.simulationTime.f;
			float startTimeNew = Mathf.Min (c.startTime + Mathf.Max (0f, hours), currentTime);
			
			overworld.ReplaceOverworldResetCountdown (startTimeNew, c.duration);

			#endif
		}
		
		public string GetLocalizedText ()
		{
			#if !PB_MODSDK

			if (hours > 0)
				return Txt.Get (TextLibs.uiOverworld, "int_sh_effect_countdown_delay");
			return Txt.Get (TextLibs.uiOverworld, "int_sh_effect_countdown_shorten");
			
			#else
            return null;
			#endif
		}
	}
	
	[Serializable]
	public class OverworldResetCountdownStartTime : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK
			
			var overworld = Contexts.sharedInstance.overworld;
			if (!overworld.hasOverworldResetCountdown)
				return;

			var c = overworld.overworldResetCountdown;
			float currentTime = overworld.simulationTime.f;
			overworld.ReplaceOverworldResetCountdown (currentTime, c.duration);

			#endif
		}
	}

	[Serializable]
	public class OverworldSetCountdown : IOverworldFunction
	{
		public int min = 1;
		public int max = 1;

		public void Run ()
		{
			#if !PB_MODSDK
			
			var overworld = Contexts.sharedInstance.overworld;
			float currentTime = overworld.simulationTime.f;
			float durationUsed = Mathf.Max (1f, Random.Range (min, max));
			
			overworld.ReplaceOverworldResetCountdown (currentTime, durationUsed);

			#endif
		}
	}
	
	[Serializable]
	public class OverworldSetCountdownFromMemory : IOverworldFunction
	{
		[ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
		public string durationMemoryKey;
		
		public int durationFallback = 24;

		public void Run ()
		{
			#if !PB_MODSDK
			
			var overworld = Contexts.sharedInstance.overworld;
			float currentTime = overworld.simulationTime.f;

			var basePersistent = IDUtility.playerBasePersistent;
			float duration = basePersistent != null && basePersistent.TryGetMemoryFloat (durationMemoryKey, out var v) ? v : 0f;
			if (duration <= 0f)
				duration = Mathf.Max (1f, durationFallback);

			overworld.ReplaceOverworldResetCountdown (currentTime, duration);

			#endif
		}
	}
	
	[Serializable]
	public class OverworldRemoveCountdown : IOverworldFunction
	{
		public void Run ()
		{
			#if !PB_MODSDK
			
			var overworld = Contexts.sharedInstance.overworld;
			if (overworld.hasOverworldResetCountdown)
			{
				overworld.RemoveOverworldResetCountdown ();
			}

			#endif
		}
	}

	[Serializable]
	public class QuestLiberationUpdate : OverworldFunctionQuest, IOverworldFunction
	{
		public int pointsPreserved = 1;
		public List<IOverworldEntityValidationFunction> pointChecks;
		
		[ValueDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
		public string progressMemoryKey;
		
		[LabelText ("Preset Filter")]
		[DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"" + nameof(QuestLiberationUpdate) + "\", \"GetTags\")")]
		[DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
		public SortedDictionary<string, bool> filter = new SortedDictionary<string, bool> ();

		[Space (4f)]
		public string spawnGroup = null;
		public bool spawnShuffle = true;
		
		[Min (0)]
		public int spawnLimit = 0;

		public bool spawnLimitOverridesVariant = false;

		// [BoxGroup ("Countdown", false)]
		// [LabelText ("Countdown Drop Range")]
		// public DataBlockVector2 countdownOffsetRange;

		[InlineButtonClear]
		public string spawnVariantsKey;
		
		[HideIf ("@!string.IsNullOrEmpty (spawnVariantsKey)")]
		public List<QuestLiberationSpawnVariant> spawnVariantsCustom;
		
		private IEnumerable<string> GetTags ()
		{
			var data = DataMultiLinkerOverworldPointPreset.data;
			return DataMultiLinkerOverworldPointPreset.tags;
		}

		private static List<QuestLiberationSpawnVariant> spawnVariantsSelected = new List<QuestLiberationSpawnVariant> ();
		private List<Vector3> spawnPointsBuffer = new List<Vector3> ();

		#if UNITY_EDITOR

		[Button, ShowIf ("@spawnPointsBuffer != null && spawnPointsBuffer.Count > 0")]
		private void VisualizePoints ()
		{
			if (spawnPointsBuffer == null || spawnPointsBuffer.Count == 0)
				return;

			for (int i = 0, iLimit = spawnPointsBuffer.Count; i < iLimit; ++i)
			{
				var p = spawnPointsBuffer[i];
				var h = (float)i / (float)iLimit;
				Debug.DrawLine (p, p + Vector3.up * 5f, Color.HSVToRGB (h, 0.5f, 1f), 5f);
			}
		}
		
		#endif

		public void Run ()
		{
			#if !PB_MODSDK

			var questState = GetQuestState ();
			if (questState == null)
				return;
			
			var provinceData = DataMultiLinkerOverworldProvinceBlueprints.GetEntry (questState.province);
			if (provinceData == null)
				return;
			
			var overworld = Contexts.sharedInstance.overworld;
			var timeCurrent = overworld.hasSimulationTime ? overworld.simulationTime.f : 0f;

			var entities = OverworldPointUtility.GetActivePoints (true, true, questKeyOverride: questState.key, checks: pointChecks);
			if (pointsPreserved == 0)
			{
				var report = entities.ToStringFormatted (true, multilinePrefix: "- ", toStringOverride: (x) => x.ToLog ());
				Debug.Log ($"QLU | Quest {questState.key} | Removing all {entities.Count} points:\n{report}");
				for (int i = 0; i < entities.Count; ++i)
				{
					var entityOverworldRemoved = entities[i];
					if (entityOverworldRemoved != null)
					{
						Debug.Log ($"- QLU | Removing point {entityOverworldRemoved.ToLog ()} ({entityOverworldRemoved.dataLinkPointPreset.data.key})");
						OverworldUtility.TryDestroySite (entityOverworldRemoved);
					}
				}
			}
			else
			{
				int pointsToRemove = Mathf.Max (0, entities.Count - pointsPreserved);
				if (pointsToRemove > 0)
				{
					Debug.Log ($"QLU | Quest {questState.key} | Removing points: {pointsToRemove} | Point count target: {pointsPreserved} | Current active points: {entities.Count}");
					for (int i = 0; i < pointsToRemove; ++i)
					{
						OverworldEntity entityOverworldExpiringSoonest = null;
						float countdownLowest = 100000f;

						foreach (var entity in entities)
						{
							if (entity == null || entity.isDestroyed || !entity.hasExpirationTimer)
								continue;

							var timer = entity.expirationTimer;
							float timeLocal = timeCurrent - timer.startTime;
							float countdown = timer.duration - timeLocal;
							if (countdownLowest > countdown)
							{
								entityOverworldExpiringSoonest = entity;
								countdownLowest = countdown;
							}
						}

						var report = entities.ToStringFormatted (true, multilinePrefix: "- ", toStringOverride: (x) => x.ToLog ());
						if (entityOverworldExpiringSoonest != null)
						{
							var entityOverworldRemoved = entityOverworldExpiringSoonest;
							Debug.Log ($"- QLU | Removing point {entityOverworldRemoved.ToLog ()} ({entityOverworldRemoved.dataLinkPointPreset.data.key}) | Expiring soonest | {entities.Count} entities considered:\n{report}");
							OverworldUtility.TryDestroySite (entityOverworldRemoved);
						}
						else
						{
							var entityOverworldRemoved = entities.GetRandomEntry ();
							Debug.Log ($"- QLU | Removing point {entityOverworldRemoved.ToLog ()} ({entityOverworldRemoved.dataLinkPointPreset.data.key}) | Random pick | {entities.Count} entities considered:\n{report}");
							OverworldUtility.TryDestroySite (entityOverworldRemoved);
						}
					}
				}
			}

			int spawnLimitFinal = spawnLimit;
			var spawnVariantsFinal = spawnVariantsCustom;
			
			if (!string.IsNullOrEmpty (spawnVariantsKey))
			{
				var spawnVariantsGlobal = DataShortcuts.overworld.liberationSpawnVariants;
				if (spawnVariantsGlobal != null && spawnVariantsGlobal.TryGetValue (spawnVariantsKey, out var sv1) && sv1?.variants != null)
				{
					spawnLimitFinal = sv1.limit;
					spawnVariantsFinal = sv1.variants;
					Debug.Log ($"QLU | Quest {questState.key} | Retrieving spawn variants from global overworld settings using list key {spawnVariantsKey} | Spawn limit: {spawnLimitFinal} | Variants: {spawnVariantsFinal.Count}");
				}

				var modifiers = DataHelperProvince.GetProvinceActiveModifiers ();
				if (modifiers != null)
				{
					foreach (var modifierKey in modifiers)
					{
						var modifierData = DataMultiLinkerOverworldProvinceModifier.GetEntry (modifierKey, false);
						if (modifierData == null)
							continue;
						
						var spawnVariantsModifier = modifierData.liberationSpawnVariants;
						if (spawnVariantsModifier != null && spawnVariantsModifier.TryGetValue (spawnVariantsKey, out var sv2) && sv2?.variants != null)
						{
							spawnLimitFinal = sv2.limit;
							spawnVariantsFinal = sv2.variants;
							Debug.Log ($"QLU | Quest {questState.key} | Retrievingspawn variants from modifier {modifierKey} using list key {spawnVariantsKey} | Spawn limit: {spawnLimitFinal} | Variants: {spawnVariantsFinal.Count}");
						}
					}
				}
			}

			if (spawnLimitOverridesVariant)
				spawnLimitFinal = spawnLimit;

			if (spawnVariantsFinal != null && spawnVariantsFinal.Count > 0 && filter != null && filter.Count > 0)
			{
				spawnVariantsSelected.Clear ();
				spawnVariantsSelected.AddRange (spawnVariantsFinal);
				
				if (spawnShuffle)
					spawnVariantsSelected.Shuffle ();
				
				if (spawnLimitFinal > 0)
				{
					while (spawnVariantsSelected.Count > Mathf.Max (0, spawnLimitFinal))
						spawnVariantsSelected.RemoveRandomEntry ();
				}

				for (int i = 0, iLimit = spawnVariantsSelected.Count; i < iLimit; ++i)
				{
					var spawnVariant = spawnVariantsSelected[i];
					if (spawnVariant == null)
						continue;

					var entityOverworld = OverworldPointUtility.GenerateNewPoint
					(
						provinceKeyOverride: questState.province,
						questKeyLinked: questState.key,
						tagFilterExternal: filter,
						spawnGroupOverride: spawnGroup,
						generateCombatDescription: true
						// spawnPointsOverride: spawnPositionsBuffer
					);

					var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworld);
					if (entityOverworld == null || entityPersistent == null)
					{
						Debug.Log ($"QLU | Quest {questState.key} | Skipping spawn step {i} due to point generation being unsuccessful (e.g. due to no valid candidates)");
						continue;
					}
					
					spawnPointsBuffer.Clear ();
					if (OverworldPointUtility.spawnPointsBuffer != null)
						spawnPointsBuffer.AddRange (OverworldPointUtility.spawnPointsBuffer);
					
					entityOverworld.ReplaceCombatEscalationLevel (spawnVariant.escalationValue);
					
					if (!string.IsNullOrEmpty (progressMemoryKey))
						entityPersistent.SetMemoryFloat (progressMemoryKey, spawnVariant.progressMemoryValue);

					var countdownOffset = Mathf.Max (0, Mathf.RoundToInt (Random.Range (spawnVariant.countdownOffset.x, spawnVariant.countdownOffset.y)));
					if (countdownOffset > 0)
						entityPersistent.SetMemoryFloat (EventMemoryIntAutofilled.Campaign_Countdown_Offset, countdownOffset);
				}
			}
			
			overworld.ReplaceSimulationTime (timeCurrent);
			OverworldExpirationTimerSystem.ForceExecute ();

			#endif
		}
	}
}
