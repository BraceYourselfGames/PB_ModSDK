using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldCampaignStep : DataMultiLinker<DataContainerOverworldCampaignStep>
    {
        public DataMultiLinkerOverworldCampaignStep ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldCampaign); 
        }
    }
}


