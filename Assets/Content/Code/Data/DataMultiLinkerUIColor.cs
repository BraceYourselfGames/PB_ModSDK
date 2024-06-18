using PhantomBrigade.Data.UI;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUIColor : DataMultiLinker<DataContainerUIColor>
    {
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showCache = true;
            
            [ShowInInspector]
            public static bool showCacheInfo = false;
        }
        
        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        #if UNITY_EDITOR

        private void SaveGlobalUI ()
        {
            DataLinkerUI.SaveData ();
        }
        
        #endif
    }
}


