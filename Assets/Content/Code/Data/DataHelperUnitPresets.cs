using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public static class DataHelperUnitPresets
    {
        private static Dictionary<string, List<string>> presetsByBlueprint;
        private static Dictionary<string, List<string>> presetsByClass;

        private static bool initialized;

        public static void CheckSetup ()
        {
            if (!initialized)
                Setup ();
        }

        private static void ResetCache ()
        {
            initialized = false;
            Setup ();
        }

        private static void Setup ()
        {
            if (initialized)
                return;

            initialized = true;
            presetsByBlueprint = new Dictionary<string, List<string>> ();
            presetsByClass = new Dictionary<string, List<string>> ();

            var presets = DataMultiLinkerUnitPreset.data;
            if (presets == null)
                return;

            foreach (var kvp in presets)
            {
                var key = kvp.Key;
                var preset = kvp.Value;

                if (preset == null || string.IsNullOrEmpty (preset.blueprintProcessed))
                {
                    Debug.LogWarning ($"Failed to add unit preset {key} to library due to null reference or null/empty blueprint string");
                    continue;
                }

                var blueprint = DataMultiLinkerUnitBlueprint.GetEntry (preset.blueprintProcessed);
                if (blueprint == null)
                {
                    Debug.LogWarning ($"Failed to add unit preset {key} to library due to blueprint {preset.blueprintProcessed} not being found");
                    continue;
                }

                if (presetsByBlueprint.ContainsKey (preset.blueprintProcessed))
                {
                    var entries = presetsByBlueprint[preset.blueprintProcessed];
                    if (entries.Contains (key))
                    {
                        Debug.LogWarning ($"Failed to add {key} to blueprint-to-preset dictionary, same preset already registered");
                        return;
                    }

                    entries.Add (key);
                }
                else
                    presetsByBlueprint.Add (preset.blueprintProcessed, new List<string> {key});

                if (presetsByClass.ContainsKey (blueprint.classTag))
                {
                    var entries = presetsByClass[blueprint.classTag];
                    if (entries.Contains (key))
                    {
                        Debug.LogWarning ($"Failed to add {key} to class-to-preset dictionary, same preset already registered");
                        return;
                    }

                    entries.Add (key);
                }
                else
                    presetsByClass.Add (blueprint.classTag, new List<string> {key});
            }
        }

        public static string GetRandomPresetForBlueprint (string unitBlueprintKey, List<string> validUnitPresets)
        {
            CheckSetup ();

            if (presetsByBlueprint == null || string.IsNullOrEmpty (unitBlueprintKey) || !presetsByBlueprint.ContainsKey (unitBlueprintKey))
            {
                Debug.LogWarning ($"Failed to locate unit preset for blueprint [{unitBlueprintKey}]");
                return null;
            }

            var entries = new List<string> (presetsByBlueprint[unitBlueprintKey]);

            for (var i = entries.Count - 1; i >= 0; i--)
            {
                var isValidPreset = false;
                for (var j = 0; j < validUnitPresets.Count; j++)
                {
                    if (entries[i] != validUnitPresets[j])
                        continue;
                    
                    isValidPreset = true;
                    break;
                }

                if (!isValidPreset)
                    entries.RemoveAt (i);
            }

           
            if (entries.Count != 0)
            {
                var index = Random.Range (0, entries.Count);
                var entry = entries[index];
                return entry; 
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetRandomPresetForClass (string unitClassKey)
        {
            CheckSetup ();

            if (presetsByClass == null || string.IsNullOrEmpty (unitClassKey) || presetsByClass.ContainsKey (unitClassKey))
            {
                Debug.LogWarning ($"Failed to locate unit preset for class [{unitClassKey}]");
                return null;
            }

            var entries = presetsByClass[unitClassKey];
            var index = Random.Range (0, entries.Count);
            var entry = entries[index];
            return entry;
        }
    }
}