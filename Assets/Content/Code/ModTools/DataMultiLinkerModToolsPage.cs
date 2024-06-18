using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerModToolsPage : DataMultiLinker<DataContainerModToolsPage>
    {
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showIcons = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        public override bool IsModdable () => false;
        public override bool IsDisplayIsolated () => true;
    }
}


