using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerWorkshopVariants : DataMultiLinker<DataContainerWorkshopVariants>
    {
        public DataMultiLinkerWorkshopVariants ()
        {
            textSectorKeys = new List<string> { TextLibs.workshopVariants };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.workshopVariants),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.workshopVariants)
            );
        }
    }
}


