using System.Collections.Generic;
using PhantomBrigade.Overworld;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerBaseStat : DataMultiLinker<DataContainerBaseStat>
    {
        public DataMultiLinkerBaseStat ()
        {
            textSectorKeys = new List<string> { TextLibs.baseStats };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.baseStats),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.baseStats)
            );
        }
    }
}


