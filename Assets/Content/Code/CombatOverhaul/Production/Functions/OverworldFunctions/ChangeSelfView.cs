using System;
using PhantomBrigade.Data;
using PhantomBrigade.Overworld;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class ChangeSelfView : IOverworldFunction
    {
        public string assetKey;
        
        public void Run ()
        {
            #if !PB_MODSDK

            var baseOverworld = IDUtility.playerBaseOverworld;
            OverworldUtility.ChangeTargetView (baseOverworld, assetKey);
            
            #endif
        }
    }
}