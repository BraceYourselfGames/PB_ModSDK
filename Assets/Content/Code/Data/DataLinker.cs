using System;
using PhantomBrigade.Mods;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

#if PB_MODSDK
using System.IO;
using PhantomBrigade.ModTools;
using PhantomBrigade.SDK.ModTools;
using UnityEditor;
#endif

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataLinker<T> : MonoBehaviour where T : DataContainerUnique
    {
        private static Type dataTypeStatic = typeof (T);
        protected Type dataType = typeof (T);

        // protected static DataLinker<T> ins;
        private static T dataInternal;

        [NonSerialized]
        private static bool loadedOnce = false;

        [NonSerialized][ShowIf ("unsavedChangesPossible")]
        private static bool unsavedChangesPossible = false;

        #if PB_MODSDK
        bool hasSelectedMod => DataContainerModData.hasSelectedConfigs;

        [ShowInInspector, PropertyOrder (-100), HideReferenceObjectPicker, HideLabel, HideDuplicateReferenceBox]
        private static ModConfigStatus status = new ModConfigStatus ();

        public static string configsPath => DataContainerModData.selectedMod.GetModPathConfigs ();
        #endif

        // [DetailedInfoBox ("@GetLinkerDescription ()", "@GetLinkerDetails ()", InfoMessageType.Warning)]
        #if PB_MODSDK
        [PropertyTooltip ("$" + nameof(path))]
        #endif
        [FolderPath][ShowInInspector][ReadOnly]
        public static string path;

        [LabelText ("Auto-load in game")]
        public bool autoloadInGame = false;

        [LabelText ("Auto-load in editor")]
        public bool autoloadInEditor = false;

        public bool log = false;

        [ShowInInspector][OnValueChanged ("ValidateData", true)]
        [PropertySpace (8f)]
        public static T data
        {
            get
            {
                if (dataInternal == null && !loadedOnce)
                    LoadData ();
                return dataInternal;
            }
            set
            {
                dataInternal = value;
            }
        }

        private void OnEnable ()
        {
            // OnEnable is reliably called first in Edit mode and in Play mode (as opposed to Awake), so we'll use it to set up the instance
            // ins = this;

            #if UNITY_EDITOR
            // Since some data linkers are interdependent, we need to wait until every single singleton is set up before we do initial load.
            // Editor usually schedules an Update right after OnEnable, but in case it doesn't, the following call should guarantee it.
            // Since Application.isPlaying can't be reliably used in OnEnable of ExecuteInEditMode, we use a utility function.
            if (UtilityECS.IsApplicationNotPlaying ())
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate ();
            #endif
        }

        private void Update ()
        {
            // Update is the only function that reliably works for the purpose of initial loading in Edit mode and in Play mode.
            if (loadedOnce)
                return;

            bool inEditor = UtilityECS.IsApplicationNotPlaying ();
            bool load = inEditor ? autoloadInEditor : autoloadInGame;
            #if PB_MODSDK && UNITY_EDITOR
            load |= reloadData;
            #endif

            if (load)
            {
                #if PB_MODSDK && UNITY_EDITOR
                reloadData = false;
                #endif
                // In some rare cases, the data might have been used very early and might have already been loaded - then we don't need to bother
                loadedOnce = true;
                if (data == null)
                    LoadData ();
            }
        }

        #if PB_MODSDK && UNITY_EDITOR
        [NonSerialized]
        bool reloadData;

        // Used through reflection. See UtilityDatabaseSerialization.ResetLoadedOnce
        public void ResetLoadedOnce ()
        {
            if (!loadedOnce)
            {
                return;
            }

            loadedOnce = false;
            path = null;
            data = null;
            reloadData = true;
        }

        bool hasChanges => hasSelectedMod && (unsavedChangesPossible || ModToolsHelper.HasChanges (DataContainerModData.selectedMod, typeof(T)));
        private Color colorSepia => ModToolsColors.HighlightNeonSepia;

        [PropertyOrder (-10f)]
        [ShowIf(nameof(hasSelectedMod))]
        [EnableIf (nameof(hasChanges))]
        [GUIColor (nameof(colorSepia))]
        [PropertyTooltip ("Reset config to match SDK. This will undo all your changes.")]
        [Button ("Restore from SDK", ButtonHeight = 32, Icon = SdfIconType.BoxArrowInRight, IconAlignment = IconAlignment.LeftOfText)]
        public void RestoreFromSDK ()
        {
            if (DataContainerModData.selectedMod.linkerChecksumMap == null)
            {
                return;
            }
            if (!DataContainerModData.selectedMod.linkerChecksumMap.TryGetValue (typeof(T), out var pair))
            {
                return;
            }
            if (!EditorUtility.DisplayDialog ("Restore From SDK", $"Are you sure you'd like to overwrite all of your changes to this config?\n\nConfig file: \n{path}", "Confirm", "Cancel"))
            {
                return;
            }

            DataContainerModData.selectedMod.DeleteConfigOverride (pair.Mod);
            if (pair.SDK == null)
            {
                // This shouldn't happen because linker DBs can't be added through the SDK.
                Debug.LogErrorFormat ("Linker DB mod file that isn't present in SDK | type: {0} | path: {1}", typeof(T).Name, pair.Mod.RelativePath);
            }
            else
            {
                var sdkPath = Path.Combine (DataPathHelper.GetApplicationFolder (), "Configs", pair.SDK.RelativePath);
                var modPath = Path.Combine (configsPath, pair.Mod.RelativePath);
                File.Copy (sdkPath, modPath, true);
                LoadData ();
            }
            SaveChecksums ();

            unsavedChangesPossible = false;
        }
        #endif

        protected static bool CheckPathResolved (bool isUser = false)
        {
            path ??= DataPathUtility.GetPath (typeof(T)); // (MethodBase.GetCurrentMethod().DeclaringType);
            #if PB_MODSDK && UNITY_EDITOR
            if (isUser)
            {
                return true;
            }
            if (DataContainerModData.selectedMod == null)
            {
                return true;
            }
            if (!Directory.Exists (DataContainerModData.selectedMod.GetModPathConfigs ()))
            {
                return true;
            }
            var projectPath = DataContainerModData.selectedMod.GetModPathProject ();
            path = DataPathHelper.GetCombinedCleanPath (projectPath, path);
            return false;
            #else
            return true;
            #endif
        }

        [ButtonGroup ("LoadSave", Order = -9f)]
        [Button ("Load data", ButtonSizes.Large)]
        public static void LoadData ()
        {
            var appendApplicationPath = CheckPathResolved ();
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogError ($"Failed to load global data container of type {typeof (T).Name} due to missing path");
                return;
            }

            dataInternal = UtilitiesYAML.LoadDataFromFile<T> (path + ".yaml", appendApplicationPath: appendApplicationPath);
            unsavedChangesPossible = false;
            loadedOnce = true;

            dataInternal = ModManager.ProcessConfigModsForLinker (dataTypeStatic, dataInternal);

            if (dataInternal != null)
                dataInternal.OnAfterDeserialization ();

            #if PB_MODSDK && UNITY_EDITOR
            UpdateChecksum ();
            #endif
        }

        [ButtonGroup ("LoadSave")]
        [Button ("@unsavedChangesPossible ? \"Save data*\" : \"Save data\"", ButtonSizes.Large)]
        #if PB_MODSDK
        [EnableIf (nameof(hasSelectedMod))]
        #endif
        public static void SaveData ()
        {
            if (dataInternal == null)
                return;

            var appendApplicationPath = CheckPathResolved ();
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogError ($"Failed to save global data container of type {typeof (T).Name} due to missing path");
                return;
            }

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs || !unsavedChangesPossible)
            {
                return;
            }
            #endif

            dataInternal.OnBeforeSerialization ();
            UtilitiesYAML.SaveDataToFile (path + ".yaml", dataInternal, appendApplicationPath: appendApplicationPath);
            unsavedChangesPossible = false;

            #if PB_MODSDK && UNITY_EDITOR
            SaveChecksums ();
            #endif
        }

        public void ValidateData ()
        {
            if (dataInternal == null)
                return;

            #if PB_MODSDK
            unsavedChangesPossible = DataContainerModData.hasSelectedConfigs;
            #else
            unsavedChangesPossible = true;
            #endif
        }

        [NonSerialized]
        private string linkerDescription;
        private string GetLinkerDescription ()
        {
            if (string.IsNullOrEmpty (linkerDescription))
            {
                linkerDescription = $"This is a linker component for class of type {typeof (T).GetNiceName ()}. Nothing is serialized on this component: ";
                linkerDescription += "instead, the data is saved to YAML file under location/name specified below. <b>Do not forget to save any changes ";
                linkerDescription += "whenever you modify the data!</b>. Expand this box for more details...";
            }

            return linkerDescription;
        }

        [NonSerialized]
        private string linkerDetails;
        private string GetLinkerDetails ()
        {
            if (string.IsNullOrEmpty (linkerDetails))
            {
                linkerDetails = "Linkers are components that allow us not to write boilerplate every time we want to put another piece of data into YAML. ";
                linkerDetails += "They automatically handle saving, loading and singleton-style access to the data, making transition to YAML easier. ";
                linkerDetails += "\n\nThis particular class is a linker for so-called unique data, of which only one instance is allowed to exist. ";
                linkerDetails += "Examples of that data can include global gameplay balance scalars, rendering pipeline settings, animation constants etc. ";
                linkerDetails += "The data is easily accessible through singleton-like static field on the linker.";
            }

            return linkerDetails;
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

        #if PB_MODSDK && UNITY_EDITOR
        static void UpdateChecksum ()
        {
            if (!DataContainerModData.hasSelectedConfigs)
            {
                return;
            }
            var selectedMod = DataContainerModData.selectedMod;
            if (selectedMod.linkerChecksumMap == null)
            {
                return;
            }
            if (!selectedMod.linkerChecksumMap.TryGetValue (typeof(T), out var pair))
            {
                return;
            }
            try
            {
                var checksum = pair.Mod.Checksum;
                pair.Mod.Update (ConfigChecksums.EntrySource.Mod, File.ReadAllBytes(path + ".yaml"));
                if (!ConfigChecksums.ChecksumEqual (checksum, pair.Mod.Checksum))
                {
                    Debug.LogFormat
                    (
                        "{0}: on-disk changes detected in mod config -- refreshing checksum\nold: {1:x16}{2:x16}\nnew: {3:x16}{4:x16}",
                        typeof(T).Name,
                        checksum.HalfSum2,
                        checksum.HalfSum1,
                        pair.Mod.Checksum.HalfSum2,
                        pair.Mod.Checksum.HalfSum1);
                }
                if (pair.SDK != null && ConfigChecksums.ChecksumEqual (pair.SDK.Checksum, pair.Mod.Checksum))
                {
                    pair.Mod.Source = ConfigChecksums.EntrySource.SDK;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError ("Error during checksum operation | path: " + path + ".yaml");
                Debug.LogException (ex);
            }
        }

        static void SaveChecksums ()
        {
            UpdateChecksum ();
            ModToolsHelper.SaveChecksums (DataContainerModData.selectedMod);
        }
        #endif
    }
}
