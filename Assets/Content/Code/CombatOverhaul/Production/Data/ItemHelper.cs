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
    public static Dictionary<string, ItemVisual> prefabsFromMod = new Dictionary<string, ItemVisual> ();

    public static void CheckDatabase ()
    {
        if (!autoloadAttempted)
        {
            autoloadAttempted = true;
            LoadVisuals ();
            if (itemVisualFileEntries == null)
                Debug.LogWarning ("Could not load the item visual DB, check existence of related files!");
        }
    }

    public static bool AreAssetsPresent ()
    {
        return itemVisualPrefabs != null && itemVisualPrefabs.Count > 0;
    }

    public static void ResetLoadedFlag ()
    {
        autoloadAttempted = false;
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
        
        if (prefabsFromMod != null)
        {
            foreach (var kvp in prefabsFromMod)
            {
                var key = kvp.Key;
                var prefab = kvp.Value;
                if (!string.IsNullOrEmpty (key) && prefab != null)
                    itemVisualPrefabs[key] = prefab;
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
