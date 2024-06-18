using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotCheck : DataMultiLinker<DataContainerPilotCheck>
    {
        public DataMultiLinkerPilotCheck ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotChecks); 
        }
    }
}


