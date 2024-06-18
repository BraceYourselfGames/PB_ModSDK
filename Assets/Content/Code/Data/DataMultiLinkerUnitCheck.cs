using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitCheck : DataMultiLinker<DataContainerUnitCheck>
    {
        public DataMultiLinkerUnitCheck ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.unitChecks); 
        }
    }
}


