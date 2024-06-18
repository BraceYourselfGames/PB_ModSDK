using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerCombatReinforcement : DataContainer
    {
        [DropdownReference]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true)]
        public List<DataBlockScenarioUnitGroup> unitGroups = new List<DataBlockScenarioUnitGroup> ();
    }
}