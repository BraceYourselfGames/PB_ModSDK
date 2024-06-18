using System;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class WorkshopBuildCancel : IOverworldActionFunction
    {
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null)
                return;

            // Do any additional logic related to project/variant cancellation here
            // No need to handle refunding of resources here, that's automatically handled by action cleanup system

            // We want to refresh project view if it's entered, but only 1 frame later, once the system handling final destruction refunds everything
            Co.DelayFrames (1, () =>
            {
                if (CIViewBaseWorkshopV2.ins.IsEntered ())
                {
                    CIViewBaseWorkshopV2.ins.RefreshList ();
                    CIViewBaseWorkshopV2.ins.RedrawProjectInfo ();
                    CIViewBaseWorkshopV2.ins.RefreshSceneVisuals ();
                }
            });
            
            #endif
        }
    }
}