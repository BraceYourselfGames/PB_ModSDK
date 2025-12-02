using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCutsceneVideo : DataMultiLinker<DataContainerCutsceneVideo>
    {
        public DataMultiLinkerCutsceneVideo ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.cutscenesVideo); 
        }
    }
}


