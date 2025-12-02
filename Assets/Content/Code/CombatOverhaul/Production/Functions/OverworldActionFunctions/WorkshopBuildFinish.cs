using System;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class WorkshopBuildFinish : IOverworldActionFunction
    {
        public void Run (OverworldActionEntity source)
        {
            #if !PB_MODSDK

            var basePersistent = IDUtility.playerBasePersistent;
            if (basePersistent == null)
                return;

            if (!source.hasPayloadWorkshopProject)
            {
                Debug.LogWarning ($"Failed to finish workshop project - no payload component with key/variants found");
                return;
            }

            var p = source.payloadWorkshopProject;
            WorkshopUtility.FinishProjectBuild ((WorkshopProjectType)p.type, p.key, p.variantPrimaryKey, p.variantSecondaryKey, p.rating);
            
            #endif
        }
    }
}