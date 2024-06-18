using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatStat : DataMultiLinker<DataContainerCombatStat>
    {
        public DataMultiLinkerCombatStat ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.combatStats); 
        }
    }
}


