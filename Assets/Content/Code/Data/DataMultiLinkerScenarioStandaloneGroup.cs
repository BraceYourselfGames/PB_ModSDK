using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerScenarioStandaloneGroup : DataMultiLinker<DataContainerScenarioStandaloneGroup>
    {
        public DataMultiLinkerScenarioStandaloneGroup ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.overworldMissionLinks); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
        
        public static void OnAfterDeserialization ()
        {
           
        }

        #if !PB_MODSDK
        
        [Button ("Refresh UI"), HideInEditorMode, PropertyOrder (-1)]
        private static void RefreshUI ()
        {
            if (Application.isPlaying && CIViewBaseBriefingV2.ins.IsEntered ())
                CIViewBaseBriefingV2.ins.OnEntryToRoot ();
        }
        
        #endif
    }
}


