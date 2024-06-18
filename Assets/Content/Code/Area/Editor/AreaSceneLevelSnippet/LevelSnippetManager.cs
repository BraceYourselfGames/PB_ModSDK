using System;
using System.Collections.Generic;
using System.IO;

using PhantomBrigade.Data;

using UnityEngine;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    static class LevelSnippetManager
    {
        public sealed class SaveSpec
        {
            public string Key;
            public string Description;
            public string AreaKey;
            public readonly List<ILevelSnippetContent> Content = new List<ILevelSnippetContent> ();
            public LevelData LevelData;
        }

        public static IDictionary<string, LevelSnippet> data => dataInternal;

        public static void LoadData (bool force = false)
        {
            if (loadedOnce && !force)
            {
                return;
            }

            if (!mappingsRegistered)
            {
                // ILevelSnippetExtension is type-hinted but it lives in the Editor assembly which
                // isn't picked up by the code that initializes the tag mappings in UtilitiesYAML.
                // We need to do what mods do to inject their type-hinted types and we also need
                // to refresh the serializer since we also serialize data (unlike mods).
                UtilitiesYAML.AddTagMappingsHintedInAssembly (typeof(LevelSnippetManager).Assembly);
                UtilitiesYAML.RebuildDeserializer ();
                UtilitiesYAML.RebuildSerializer ();
                mappingsRegistered = true;
            }

            dataInternal.Clear ();

            var di = new DirectoryInfo (GetDirectoryPath ());
            if (!di.Exists)
            {
                return;
            }

            foreach (var subdir in di.EnumerateDirectories ())
            {
                try
                {
                    var filePath = DataPathHelper.GetCombinedCleanPath (subdir.FullName, LevelSnippet.FileName);
                    var snippet = UtilitiesYAML.ReadFromFile<LevelSnippet> (filePath);
                    if (snippet == null)
                    {
                        continue;
                    }
                    snippet.key = subdir.Name;
                    snippet.pathSource = subdir;
                    dataInternal[subdir.Name] = snippet;
                }
                catch (Exception ex)
                {
                    Debug.LogError ("Exception thrown while retrieving metadata for level snippet from directory | path: " + subdir.FullName);
                    Debug.LogException (ex);
                }
            }

            loadedOnce = true;

            // This is where we patch into the ModManager to load snippets from mods.
        }

        public static (bool, LevelSnippet) Save (SaveSpec spec)
        {
            var path = DataPathHelper.GetCombinedCleanPath (GetDirectoryPath (), spec.Key);
            var pathSource = new DirectoryInfo (path);
            var snippet = new LevelSnippet ()
            {
                key = spec.Key,
                originalAreaKey = spec.AreaKey,
                description = spec.Description,
                size = spec.LevelData.Bounds,
                content = spec.Content,
                pathSource = pathSource,
            };
            #if PB_MODSDK
            if (DataContainerModData.hasSelectedConfigs)
            {
                snippet.originalModID = DataContainerModData.selectedMod.id;
            }
            #endif
            var ok = snippet.Serialize (pathSource, spec.LevelData);
            if (!ok)
            {
                return (false, null);
            }
            dataInternal[spec.Key] = snippet;
            return (true, snippet);
        }

        static string GetDirectoryPath ()
        {
            #if PB_MODSDK
            if (DataContainerModData.hasSelectedConfigs)
            {
                return DataPathHelper.GetCombinedCleanPath (DataContainerModData.selectedMod.GetModPathProject (), DirectoryName);
            }
            #endif
            return DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), "Configs", DirectoryName);
        }

        static bool mappingsRegistered;
        static bool loadedOnce;
        static readonly SortedDictionary<string, LevelSnippet> dataInternal = new SortedDictionary<string, LevelSnippet> ();

        public const string DirectoryName = "LevelSnippets";
    }
}
