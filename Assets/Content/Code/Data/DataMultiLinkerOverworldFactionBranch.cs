using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldFactionBranch : DataMultiLinker<DataContainerOverworldFactionBranch>
    {
        public DataMultiLinkerOverworldFactionBranch ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldBranches); 
        }
    }
}


