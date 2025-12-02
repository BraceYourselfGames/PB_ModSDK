using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization; 

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace PhantomBrigade.Data
{
    public static class DataPathUtility
    {
        private static Dictionary<Type, string> pathsFixed = new Dictionary<Type, string>
        {
            { typeof (DataContainerPaths), "paths" },
            { typeof (DataContainerTextLibrary), "Text/" },
            #if !PB_MODSDK
            { typeof (DataContainerSave), "Saves/" }, 
            #else
            { typeof (DataContainerModToolsPage), "../ConfigsModTools/" },
            { typeof (DataContainerModData), "../ModConfigs/" },
            #endif
        };
        
        
        
        public static string GetPath (Type linkerType)
        {
            if (pathsFixed.ContainsKey (linkerType))
            {
                var pathEndFixed = pathsFixed[linkerType];
                // Debug.Log ($"{linkerType.Name} loads data from fixed path {pathEndFixed}");
                return $"Configs/{pathEndFixed}";
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
            return $"Configs/{pathEnd}";
        }

        public static string GetDataTypeFromPath (string path, bool fallbackAllowed = true)
        {
            if (DataLinkerPaths.data == null || DataLinkerPaths.data.pathsInverted == null)
            {
                // Debug.LogWarning ($"Failed to resolve data multi linker from path - DataLinkerPaths.data.pathsInverted is null");
                return null;
            }

            var pathData = DataLinkerPaths.data;
            var pathsInverted = pathData.pathsInverted;
            if (pathsInverted.TryGetValue (path, out var typeName))
                return typeName;

            if (fallbackAllowed)
            {
                foreach (var kvp in pathData.paths)
                {
                    var pathCompared = kvp.Value;
                    if (path.Contains (pathCompared))
                    {
                        var typeNameFallback = kvp.Key;
                        // Debug.Log ($"Fallback type determination based on path containing recognized pattern: {typeNameFallback} | Input:\n- {path}");
                        return typeNameFallback;
                    }
                }
            }

            return null;
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
                    var typeName = kvp.Key;
                    var path = kvp.Value;
                    
                    if (!pathsInverted.ContainsKey (path))
                        pathsInverted.Add (path, typeName);

                    foreach (var kvp2 in paths)
                    {
                        var typeNameCompared = kvp2.Key;
                        if (string.Equals (typeName, typeNameCompared, StringComparison.Ordinal))
                            continue;
                        
                        var pathCompared = kvp2.Value;
                        if (path.Contains (pathCompared, StringComparison.Ordinal))
                            Debug.LogWarning ($"Path for type {typeName} ({path}) contains another path for type {typeNameCompared} ({pathCompared})");
                    }
                }
            }
        }
    }
}

