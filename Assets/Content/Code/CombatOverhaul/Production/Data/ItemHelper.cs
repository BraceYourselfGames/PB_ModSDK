using System.Collections.Generic;
using PhantomBrigade.Mods;
using PhantomBrigade.SDK.ModTools;
using UnityEngine;

public static class ItemHelper
{
    public static readonly string itemVisualPrefabsPath = "Content/Items";
    private static List<ResourceDatabaseEntryRuntime> itemVisualFileEntries = new List<ResourceDatabaseEntryRuntime> ();
    public static Dictionary<string, ItemVisual> itemVisualPrefabs = new Dictionary<string, ItemVisual> ();

    private static bool autoloadAttempted = false;

    public static void CheckDatabase ()
    {
        if (!autoloadAttempted)
        {
            autoloadAttempted = true;
            
            if (itemVisualFileEntries == null)
            {
                LoadVisuals ();
                if (itemVisualFileEntries == null)
                    Debug.LogWarning ("Could not load the item visual DB, check existence of related files!");
            }
        }
    }

    public static bool AreAssetsPresent ()
    {
        return itemVisualPrefabs != null && itemVisualPrefabs.Count > 0;
    }

    public static void LoadVisuals ()
    {
        ResourceDatabaseContainer resourceDatabase = ResourceDatabaseManager.GetDatabase ();
        if (resourceDatabase == null || resourceDatabase.entries == null || resourceDatabase.entries.Count == 0)
            return;
        
        itemVisualFileEntries.Clear ();
        itemVisualPrefabs.Clear ();
        
        if (!resourceDatabase.entries.ContainsKey (itemVisualPrefabsPath))
        {
            Debug.LogWarning ($"Failed to find item visual path {itemVisualPrefabsPath} in the resource DB!");
            return;
        }
        
        var visualInfoDir = ResourceDatabaseManager.GetEntryByPath (itemVisualPrefabsPath);
        ResourceDatabaseManager.FindResourcesRecursive (itemVisualFileEntries, visualInfoDir, 1, ResourceDatabaseEntrySerialized.Filetype.Prefab);
        Debug.Log ($"Located {itemVisualFileEntries.Count} built-in prefabs for unit item visuals");

        bool prefabWarningIssued = false;

        for (int i = 0; i < itemVisualFileEntries.Count; ++i)
        {
            var entry = itemVisualFileEntries[i];
            var prefab = entry.GetContent<GameObject> ();
            if (prefab == null)
            {
                if (!prefabWarningIssued)
                {
                    prefabWarningIssued = true;
                    Debug.LogWarning ($"Failed to find a prefab at path {entry.path} | Consider reimporting or rebuilding ResourceDatabase");
                }
                
                continue;
            }
            
            var component = prefab.GetComponent<ItemVisual> ();
            if (component == null)
            {
                // Debug.Log ($"Failed to find ItemVisual component on item visual prefab {entry.path}", prefab);
                continue;
            }

            itemVisualPrefabs.Add (prefab.name, component);
        }

        #if UNITY_EDITOR
        
        if (DataContainerModData.selectedMod != null)
        {
            var prefabExtension = ".prefab";
            var mod = DataContainerModData.selectedMod;
            if (mod.assetBundles != null && mod.assetBundles.bundleDefinitions != null)
            {
                foreach (var assetBundleDefinition in mod.assetBundles.bundleDefinitions)
                {
                    if (assetBundleDefinition == null)
                        continue;

                    if (!assetBundleDefinition.enabled)
                        continue;

                    if (string.IsNullOrEmpty (assetBundleDefinition.name))
                    {
                        Debug.LogWarning ($"Mod {mod.id} | Asset bundle has no name");
                        continue;
                    }
                    
                    if (assetBundleDefinition.files == null || assetBundleDefinition.files.Count == 0)
                        continue;

                    foreach (var file in assetBundleDefinition.files)
                    {
                        if (file == null)
                            continue;
                        
                        if (string.IsNullOrEmpty (file.path))
                            continue;

                        if (!file.path.EndsWith (prefabExtension))
                            continue;

                        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject> (file.path);
                        if (prefab == null)
                        {
                            Debug.LogWarning ($"Mod {mod.id} | Asset bundle {assetBundleDefinition.name} has invalid prefab path: {file.path}");
                            continue;
                        }
                        
                        var component = prefab.GetComponent<ItemVisual> ();
                        if (component == null)
                            continue;
                        
                        var visualKey = $"{assetBundleDefinition.name}/{prefab.name}";
                        itemVisualPrefabs.Add (visualKey, component);
                        Debug.Log ($"Mod {mod.id} | Loaded new ItemVisual from AssetBundle: {visualKey}");
                    }
                }
            }
        }
        
        #endif
    }

    public static ItemVisual GetVisual (string visualName, bool logAbsence = true)
    {
        CheckDatabase ();

        if (itemVisualPrefabs.ContainsKey (visualName))
            return itemVisualPrefabs[visualName];
        else
        {
            if (logAbsence)
                Debug.LogWarning ($"Failed to find item visual named {visualName}");
            return null;
        }
    }
    
    public static bool IsVisualValid (string visualName)
    {
        CheckDatabase ();

        if (itemVisualPrefabs.ContainsKey (visualName))
            return true;
        return false;
    }

    public static Dictionary<string, ItemVisual> GetAllVisuals ()
    {
        CheckDatabase ();
        return itemVisualPrefabs;
    }
    
    public static IEnumerable<string> GetAllVisualKeys ()
    {
        CheckDatabase ();
        return itemVisualPrefabs != null ? itemVisualPrefabs.Keys : null;
    }
}
