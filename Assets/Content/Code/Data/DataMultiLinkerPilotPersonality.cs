using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotPersonality : DataMultiLinker<DataContainerPilotPersonality>
    {
        public DataMultiLinkerPilotPersonality ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotPersonalities); 
        }
    }
}


