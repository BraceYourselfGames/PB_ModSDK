using System;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
 
    [Serializable]
    public class DataContainerUnitFactionCheck
    {
	    [ValueDropdown ("@Factions.GetList ()")]
        public string requireFaction;
    }
}

