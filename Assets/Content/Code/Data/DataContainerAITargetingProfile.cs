using System;
using System.Collections.Generic;
using PhantomBrigade.AI;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [Serializable][LabelWidth (180f)]
    public class DataContainerAITargetingProfile : DataContainer
    {
        public int priority = 0;
	    public Dictionary<CombatAITargetingUtils.StatType, float> values = new Dictionary<CombatAITargetingUtils.StatType, float>();
    }
}