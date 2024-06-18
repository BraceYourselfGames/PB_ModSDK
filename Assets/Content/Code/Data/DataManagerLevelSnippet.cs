using System.Collections.Generic;
using Area;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataManagerLevelSnippet : MonoBehaviour
    {
        public static bool unsavedChangesPossible = false;

        [LabelText ("Folder")][FolderPath][ShowInInspector, ReadOnly]
        public static string folderPath = "Configs/LevelSnippets/";

        private static bool keyListLoadedOnce = false;

        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = true)]
        private static List<string> keyListInternal;

        [ShowInInspector][ListDrawerSettings (DefaultExpandedState = false, ShowPaging = true)]
        private static List<string> keyList
        {
            get
            {
                if (keyListInternal == null && !keyListLoadedOnce)
                    RefreshList ();
                return keyListInternal;
            }
            set
            {
                keyListInternal = value;
            }
        }

        [PropertyOrder (-5)]
        [LabelText ("Loaded key")][ShowInInspector]
        [ValueDropdown ("keyList")]
        public static string keySelected = "test";

        [ShowInInspector]
        public static bool log = false;

        [ShowInInspector]
        public static bool applyToArea = true;

        // [ShowInInspector]
        // [PropertySpace (8f)]
        // public static DataContainerLevel data;

        [PropertyOrder (-4)][ButtonGroup]
        [Button ("Load data", ButtonSizes.Large)]
        public static void LoadData ()
        {
            LoadDataFromKey (keySelected, applyToArea, out bool success);
        }

        public static void LoadDataFromKey (string keyLoaded, bool applyToManager, out bool success)
        {
            success = false;

            if (string.IsNullOrEmpty (folderPath))
            {
                Debug.LogError ($"Failed to load level data ({keyLoaded}) due null or empty root folder path");
                return;
            }

            var levelPath = $"{folderPath}{keyLoaded}/";
            LoadDataFromPath (levelPath, keyLoaded);

            if (applyToManager)
                ApplyToManager (out success);
            // else
            //     success = data != null && data.dataUnpacked != null;
        }

        public static void LoadDataFromPath (string levelPath, string keyLoaded)
        {
            /*
            // Debug.Log ($"Loading level snippet data from {levelPath}");
            keySelected = keyLoaded;

            // Load root config
            var core = UtilitiesYAML.LoadDataFromFile<AreaDataCore> (levelPath, "core.yaml");
            if (core == null)
            {
                Debug.LogWarning ($"Failed to load core level snippet data from {levelPath}");
                return;
            }

            data = new DataContainerLevel ();
            data.dataCollections = new DataBlockLevelCollections ();

            // Load content of all fields marked with BinaryData attribute
            BinaryDataUtility.LoadFieldsFromBinary (core, data.dataCollections, levelPath);

            // Generate unpacked data from that
            data.dataUnpacked = new AreaDataContainer (keyLoaded, core, data.dataCollections);

            unsavedChangesPossible = false;
            RefreshList ();
            */
        }

        [PropertyOrder (-1)][ButtonGroup][GUIColor ("GetSaveButtonColor")]
        [Button ("@unsavedChangesPossible ? \"Save data*\" : \"Save data\"", ButtonSizes.Large)]
        public static void SaveData ()
        {
            /*
            DataManagerLevel.SaveData (new DataBlockLevelSaveSpec()
            {
                Key = keySelected,
                Data = data,
                FolderPath = folderPath,
            });
            unsavedChangesPossible = false;
            RefreshList ();
            */
        }

        [FoldoutGroup ("Utilities")]
        [Button ("Save from manager", ButtonSizes.Large)]
        public static void SaveFromManager ()
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null)
                return;

            SaveFromManager (sceneHelper.areaManager);
        }

        public static void SaveFromManager (AreaManager am)
        {
            // if (am == null || am.clipboard == null)
            // {
            //     Debug.LogWarning ($"Failed to save level snippet data due to missing area manager or clipboard object");
            //     return;
            // }

            // if (string.IsNullOrEmpty (am.clipboard.name))
            // {
            //     Debug.LogWarning ($"Failed to save: invalid filename");
            //     return;
            // }

            // keySelected = am.clipboard.name;
            // data = new DataContainerLevel ();
            // data.dataUnpacked = new AreaDataContainer (am.clipboard);
            //
            // SaveData ();
        }

        [FoldoutGroup ("Utilities")]
        [Button ("Apply to manager", ButtonSizes.Large)]
        public static void ApplyToManager ()
        {
            ApplyToManager (out bool success);
        }

        public static void ApplyToManager (out bool success)
        {
            success = false;

            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
            {
                Debug.LogWarning ($"Failed to apply level data due to missing references to area manager");
                return;
            }

            // if (data == null || data.dataUnpacked == null)
            // {
            //     Debug.LogWarning ($"Failed to apply level data to area manager due to loaded data or unpacked data being null");
            //     return;
            // }
            success = true;
        }




        [Button ("Refresh list", ButtonSizes.Large), ButtonGroup, PropertyOrder (-5)]
        private static void RefreshList ()
        {
            keyListLoadedOnce = true;
            keyList = UtilitiesYAML.GetDirectoryList (folderPath);
        }

        public static List<string> GetKeyList ()
        {
            if (keyList == null || keyList.Count == 0)
                RefreshList ();

            return keyList;
        }

        #if UNITY_EDITOR

        private Color GetSaveButtonColor ()
        {
            if (unsavedChangesPossible)
                return Color.HSVToRGB (Mathf.Cos ((float) UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1f, 1f);
            else
                return GUI.color;
        }

        #endif
    }
}
