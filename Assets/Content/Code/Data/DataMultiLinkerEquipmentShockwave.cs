using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerEquipmentShockwave : DataMultiLinker<DataContainerEquipmentShockwave>
    {
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showControls = true;

            [ShowInInspector]
            public static bool showData = true;
        }
        
        [ShowInInspector][HideLabel]
        public Presentation presentation = new Presentation ();
        
        /*
        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-2)]
        public void LogOverrideUse ()
        {
            
        }
        */
    }
}


