using System;
using System.IO;
using UnityEngine;
using HarmonyLib;

namespace PhantomBrigade.Mods
{
    public class ModLink
    {
        public static ModLink instance;
        public ModMetadata metadata;

        internal static int modIndex;
        internal static string modID;
        internal static string modPath;

        internal sealed class ModSettings
        {
            public bool log = false;
        }

        internal static ModSettings settings;

        static void LoadSettings ()
        {
            var settingsPath = Path.Combine (modPath, "settings.yaml");
            settings = UtilitiesYAML.ReadFromFile<ModSettings> (settingsPath, false);

            if (settings == null)
            {
                settings = new ModSettings ();
                Debug.LogFormat ("Mod {0} ({1}) no settings file found, using defaults | Path: {2}", modIndex, modID, settingsPath);
            }

            if (settings.log)
            {
                Debug.LogFormat ("Mod {0} ({1}) diagnostic logging is on: {2}", modIndex, modID, settings.log);
            }
        }

        public virtual void OnLoad (Harmony harmonyInstance)
        {
            // Uncomment to get a file on the desktop showing the IL of the patched methods.
            // Output from FileLog.Log() will trigger the generation of that file regardless if this is set so
            // FileLog.Log() should be put in a guard.
            //Harmony.DEBUG = true;

            modIndex = ModManager.loadedMods.Count;
            modID = metadata.id;
            modPath = metadata.path;

            LoadSettings ();

            var patchAssembly = typeof (ModLink).Assembly;
            Debug.LogFormat 
            (
                "Mod {0} ({1}) is executing OnLoad | Using HarmonyInstance.PatchAll on assembly ({2}) | Directory: {3} | Full path: {4}",
                modIndex,
                modID,
                patchAssembly.FullName,
                metadata.directory,
                metadata.path
            );

            harmonyInstance.PatchAll (patchAssembly);

            if (Harmony.DEBUG)
            {
                FileLog.Log ($"{new string ('=', 10)} [ {DateTime.Now:u} ] {new string ('=', 10)}");
                FileLog.Log ("Mod patch applied!");
            }

            // This ensures modifications to utilities themselves have a full effect
            ModUtilities.Initialize ();

            Debug.LogFormat ("Mod {0} ({1}) is initialized", modIndex, modID);
        }
    }
}