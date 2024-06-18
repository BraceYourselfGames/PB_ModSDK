using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataLinkerSettingsDebug : DataLinker<DataContainerSettingsDebug>
    {
        private static DataContainerSettingsDebug dataOverriddenInternal;

        [NonSerialized]
        private static bool loadedOnceOverridden = false;

        [InfoBox ("This data linker is overriden if a copy of its YAML file is in the local settings folder.  You can inspect the 'final' values below.", InfoMessageType.Warning)]
        [ShowInInspector]
        [PropertySpace (8f)]
        public static DataContainerSettingsDebug dataOverridden
        {
            get
            {
                if (dataOverriddenInternal == null && !loadedOnceOverridden)
                {
                    LoadOverride ();
                    loadedOnceOverridden = true;
                }

                return dataOverriddenInternal != null ? dataOverriddenInternal : data;
            }
            set
            {
                if (dataOverriddenInternal != null)
                    dataOverriddenInternal = value;
            }
        }

        static void LoadOverride ()
        {
            CheckPathResolved (isUser: true);
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogError ($"Failed to load global data container of type {nameof (DataContainerSettingsDebug)} due to missing path");
                return;
            }

            var filename = Path.GetFileName (path + ".yaml");
            dataOverriddenInternal = UtilitiesYAML.LoadDataFromFile<DataContainerSettingsDebug> (DataPathHelper.GetSettingsFolder (), filename, false, false);

            if (Application.isPlaying)
                Debug.Log ($"Loaded '{filename}' override from settings folder.");
        }
    }
}
