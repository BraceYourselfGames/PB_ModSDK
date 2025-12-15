using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldCampaignStart : DataMultiLinker<DataContainerOverworldCampaignStart>
    {
        public DataMultiLinkerOverworldCampaignStart ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldStart); 
        }
    }
}


