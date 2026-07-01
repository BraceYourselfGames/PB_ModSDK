using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerSitrep : DataMultiLinker<DataContainerSitrep>
    {
        public DataMultiLinkerSitrep ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.combatSitreps); 
        }
    }
}


