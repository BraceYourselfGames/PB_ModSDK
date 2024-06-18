using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatComms : DataMultiLinker<DataContainerCombatComms>
    {
        public DataMultiLinkerCombatComms ()
        {
            textSectorKeys = new List<string> { TextLibs.scenarioComms };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.scenarioComms, "cm_"),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.scenarioComms)
            );
        }
    }
}


