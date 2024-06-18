using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitBlueprint : DataMultiLinker<DataContainerUnitBlueprint>
    {
        public DataMultiLinkerUnitBlueprint ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.unitBlueprints); 
        }
    }
}


