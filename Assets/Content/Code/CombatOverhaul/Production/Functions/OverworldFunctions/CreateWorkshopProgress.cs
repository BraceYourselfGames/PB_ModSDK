using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class WorkshopProgressPart : IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string partPresetKey;
        public int rating = 1;
        public int offset = 1;

        public void Run ()
        {
            #if !PB_MODSDK

            offset = Mathf.Clamp (offset, 0, 10);
            var status = WorkshopUtility.TryProgressPartUnlock (partPresetKey, rating, offset);

            if (CIViewBaseWorkshopV2.ins != null && CIViewBaseWorkshopV2.ins.IsEntered ())
                CIViewBaseWorkshopV2.ins.RefreshList ();
            
            #endif
        }
    }
    
    [Serializable]
    public class WorkshopProgressSubsystem : IOverworldFunction
    {
        [ValueDropdown ("@DataMultiLinkerSubsystem.data.Keys")]
        public string subsystemBlueprintKey;
        public int offset = 1;

        public void Run ()
        {
            #if !PB_MODSDK

            offset = Mathf.Clamp (offset, 0, 10);
            var status = WorkshopUtility.TryProgressSubsystemUnlock (subsystemBlueprintKey, offset);

            if (CIViewBaseWorkshopV2.ins != null && CIViewBaseWorkshopV2.ins.IsEntered ())
                CIViewBaseWorkshopV2.ins.RefreshList ();
            
            #endif
        }
    }
}