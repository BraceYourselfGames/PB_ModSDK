using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerBaseAction : DataMultiLinker<DataContainerBaseAction>
    {
        public DataMultiLinkerBaseAction ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.baseActions); 
        }
    }
}


