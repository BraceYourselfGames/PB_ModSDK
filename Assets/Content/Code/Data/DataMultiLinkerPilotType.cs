using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotType : DataMultiLinker<DataContainerPilotType>
    {
        public override bool IsDeserializedOnCopy () => true;
        
        public DataMultiLinkerPilotType ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotTypes); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showIsolatedEntries = true;
        }

        [FoldoutGroup ("View options", true), ShowInInspector, HideLabel]
        public Presentation presentation = new Presentation ();

        public static void OnAfterDeserialization ()
        {
            
        }
    }
}


