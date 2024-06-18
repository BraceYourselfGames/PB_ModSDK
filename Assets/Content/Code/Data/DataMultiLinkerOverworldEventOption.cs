using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldEventOption : DataMultiLinker<DataContainerOverworldEventOption>
    {
        public static List<DataContainerOverworldEventOption> optionsInjected = new List<DataContainerOverworldEventOption> ();
        
        public DataMultiLinkerOverworldEventOption ()
        {
            textSectorKeys = new List<string> { TextLibs.overworldEvents };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport
            (
                dataType,
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.overworldEvents, "os_"),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.overworldEvents)
            );
        }

        public static void OnAfterDeserialization ()
        {
            optionsInjected.Clear ();

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;

                if (container.injection == null)
                    continue;
                
                optionsInjected.Add (container);
            }
        }
    }
}


