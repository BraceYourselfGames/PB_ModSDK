using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerInputHint : DataMultiLinker<DataContainerInputHint>
    {
        public DataMultiLinkerInputHint ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.uiInputHints); 
        }
    }
}


