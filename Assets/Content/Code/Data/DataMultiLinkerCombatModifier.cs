using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatModifier : DataMultiLinker<DataContainerCombatModifier>
    {
        public DataMultiLinkerCombatModifier ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.scenarioModifiers); 
        }
    }
}


