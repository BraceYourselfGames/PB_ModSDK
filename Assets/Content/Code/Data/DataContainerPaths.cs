using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    #if PB_MODSDK
    using SDK.ModTools;
    #endif

    public static class DataPathUtility
    {
        private static readonly Dictionary<Type, string> pathsFixed = new Dictionary<Type, string>
        {
            { typeof (DataContainerPaths), "paths" },
            { typeof (DataContainerTextLibrary), "Text/" },
            #if PB_MODSDK
            { typeof (DataContainerModToolsPage), "../ConfigsModTools/" },
            { typeof (DataContainerModData), "../ModConfigs/" },
            #endif
        };

        public static string GetPath (Type linkerType)
        {
            if (linkerType == null)
            {
                Debug.LogError ($"Received no linker type, path can't be determined");
                return null;
            }
            
            if (pathsFixed.TryGetValue (linkerType, out var pathEndFixed))
            {
                // Debug.Log ($"{linkerType.Name} loads data from fixed path {pathEndFixed}");
                return DataPathHelper.GetCombinedCleanPath ("Configs", pathEndFixed);
            }

            var typeName = linkerType.Name;
            if (DataLinkerPaths.data == null || DataLinkerPaths.data.paths == null)
            {
                Debug.LogError ($"{typeName} requires config to resolve its path, but DataLinkerPaths.data is null");
                return null;
            }

            var paths = DataLinkerPaths.data.paths;
            if (!paths.ContainsKey (typeName))
            {
                Debug.LogError ($"{typeName} requires config to resolve its path, but DataLinkerPaths.data doesn't contain a matching key");
                return null;
            }

            var pathEnd = paths[typeName];
            // Debug.Log ($"{typeName} loads data from config derived path {pathEnd}");
            return DataPathHelper.GetCombinedCleanPath ("Configs", pathEnd);
        }

        public static string GetDataTypeFromPath (string path)
        {
            if (DataLinkerPaths.data == null || DataLinkerPaths.data.pathsInverted == null)
            {
                // Debug.LogWarning ($"Failed to resolve data multi linker from path - DataLinkerPaths.data.pathsInverted is null");
                return null;
            }

            var pathsInverted = DataLinkerPaths.data.pathsInverted;
            if (!pathsInverted.ContainsKey (path))
            {
                // Debug.LogWarning ($"Failed to resolve data multi linker from path {path}, DataLinkerPaths.data.pathsInverted doesn't contain such a key");
                return null;
            }

            var typeName = pathsInverted[path];
            return typeName;
        }
    }

    [Serializable]
    public class DataContainerPaths : DataContainerUnique
    {
        [DictionaryDrawerSettings (KeyLabel = "Type", ValueLabel = "Path")]
        public Dictionary<string, string> paths;

        [YamlIgnore, ReadOnly]
        public Dictionary<string, string> pathsInverted;

        public override void OnAfterDeserialization ()
        {
            base.OnAfterDeserialization ();

            pathsInverted = new Dictionary<string, string> ();
            if (paths != null)
            {
                foreach (var kvp in paths)
                {
                    if (!pathsInverted.ContainsKey (kvp.Value))
                        pathsInverted.Add (kvp.Value, kvp.Key);
                }
            }
        }
    }
}
