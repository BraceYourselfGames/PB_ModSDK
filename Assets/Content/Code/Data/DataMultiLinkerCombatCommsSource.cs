using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerCombatCommsSource : DataMultiLinker<DataContainerCombatCommsSource>
    {
        public DataMultiLinkerCombatCommsSource ()
        {
            textSectorKeys = new List<string> { TextLibs.scenarioComms };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.scenarioComms, "src_"),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.scenarioComms)
            );
        }

        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void PortComms ()
        {
            var ui = DataShortcuts.ui;
            var sources = ui.commSources;

            data.Clear ();
            
            foreach (var kvp in sources)
            {
                var v = kvp.Value;
                data.Add (kvp.Key, new DataContainerCombatCommsSource
                {
                    key = kvp.Key,
                    color = v.color,
                    imageLegacy = v.image,
                    text = v.text
                });
            }
        }
        */
    }
}


