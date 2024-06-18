using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitStatus : DataMultiLinker<DataContainerUnitStatus>
    {
        public DataMultiLinkerUnitStatus ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.unitStatus); 
        }
    }
}


