namespace PhantomBrigade.Data
{
    public class DataMultiLinkerWorkshopCategory : DataMultiLinker<DataContainerWorkshopCategory>
    { 
        public DataMultiLinkerWorkshopCategory ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.workshopCategories); 
        }
    }
}


