using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
	public static class CombatAITargetingUtils
	{
		public enum StatType
		{
			//stats that are populated once
			CurrentHP,
			MaxHP,
			CurrentHPNormalized,
			MinCriticalHPNormalized,
			Speed,

			TargetedCount,

			//stats that are updated per-entity doing the targeting
			Random,
			Distance,
			PrimaryAttackFalloff,
			SecondaryAttackFalloff,
			AnyAttackFalloff,
			LineOfSight,

			Count
		}
	}

	[Serializable][LabelWidth (180f)]
    public class DataContainerAITargetingProfile : DataContainer
    {
        public int priority = 0;
	    public Dictionary<CombatAITargetingUtils.StatType, float> values = new Dictionary<CombatAITargetingUtils.StatType, float>();
    }
}