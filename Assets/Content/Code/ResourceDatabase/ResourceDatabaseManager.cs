using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Static class handling the resource database, allowing easy asset search and path fetching for Resources.Load calls.
/// Specifically, the class handles reading of project hierarchy in Resources, creation of ResourceDatabaseContainer scriptable object, 
/// loading of that object at request and parsing all the data into a dictionary with ResourceDatabaseEntryRuntime objects.
/// </summary>

public static class ResourceDatabaseManager
{
    private static List<string> extensionsSkipped = new List<string>
    {
        ".meta",
        ".bytes",
        ".compute",
        ".cs",
        ".gvdesign",
        ".shader",
        ".svg"
    };
    
    private static List<string> extensionsExpected = new List<string>
    {
        ".asset",
        ".prefab",
        ".mat",
        ".png"
    };

    private const string databaseFilename = "ResourceDatabase.asset";
    private const string databaseFolderPath = "Assets/Resources";
    private const string databasePath = databaseFolderPath + "/" + databaseFilename;
    
    private static bool databaseLoaded = false;
    private static bool databaseGenerationAttempted = false;
    private static ResourceDatabaseContainer database;
    


    // Dirty hack for preventing bad data on some unit prefabs from breaking DB generator
    private static string pathFilter = "Objects/Units";

    #if UNITY_EDITOR
    [UnityEditor.MenuItem ("PB Mod SDK/Rebuild resource DB", priority = -150)]
    #endif

    public static void RebuildDatabase ()
    {
        databaseGenerationAttempted = true;
        
        #if UNITY_EDITOR

        string progressBarHeader = "Rebuilding resource DB";
        float progressBarPercentage = 0.0f;
        UnityEditor.EditorUtility.DisplayProgressBar (progressBarHeader, "Starting...", progressBarPercentage);

        List<ResourceDatabaseEntrySerialized> files = new List<ResourceDatabaseEntrySerialized> ();
        files.AddRange (GetFiles (databaseFolderPath));

        ResourceDatabaseContainer db = ScriptableObject.CreateInstance<ResourceDatabaseContainer> ();
        db.files = files;
        
        db.entries = new Dictionary<string, int> (db.files.Count);
        db.entriesList = new List<ResourceDatabaseEntryRuntime> (db.files.Count);

        for (int i = 0, count = db.files.Count; i < count; ++i)
        {
            ResourceDatabaseEntrySerialized rdf = db.files[i];
            ResourceDatabaseEntryRuntime entry = new ResourceDatabaseEntryRuntime (rdf.path, rdf.type);
            db.entries.Add (rdf.path, i);
            db.entriesList.Add (entry);
        }

        for (int i = 0; i < db.entriesList.Count; ++i)
        {
            ResourceDatabaseEntrySerialized rds = db.files[i];
            ResourceDatabaseEntryRuntime entry = db.entriesList[i];
            entry.index = i;
            // Debug.Log ("Registered resource entry " + entry.index + ": (" + entry.type + ") " + entry.pathTrim);

            entry.parent = -1;
            if (!string.IsNullOrEmpty (rds.pathParent) && db.entries.ContainsKey (rds.pathParent))
            {
                ResourceDatabaseEntryRuntime entryParent = db.entriesList[db.entries[rds.pathParent]];
                if (entryParent != null)
                    entry.parent = db.entriesList.IndexOf (entryParent);
            }

            entry.children = new List<int> ();
            for (int c = 0; c < rds.pathsChildren.Count; ++c)
            {
                string pathChild = rds.pathsChildren[c];
                if (!string.IsNullOrEmpty (pathChild) && db.entries.ContainsKey (pathChild))
                {
                    ResourceDatabaseEntryRuntime entryChild = db.entriesList[db.entries[pathChild]];
                    entry.children.Add (db.entriesList.IndexOf (entryChild));
                }
            }
        }

        var progressDesc = string.Empty;

        for (int i = 0, count = db.entriesList.Count; i < count; ++i)
        {
            progressBarPercentage = Mathf.Min (1f, (float)(i + 1) / (float)count);
            progressDesc = (int)(progressBarPercentage * 100f) + "% done | Processing file: " + db.entriesList[i].name;
            UnityEditor.EditorUtility.DisplayProgressBar (progressBarHeader, progressDesc, progressBarPercentage);

            db.entriesList[i].ResolveContent ();
        }
        
        

        var folderPathFull = $"{Application.dataPath}/Resources";
        if (!Directory.Exists (folderPathFull))
        {
            Debug.Log ($"Resource folder doesn't exist, creating one: {folderPathFull}");
            Directory.CreateDirectory (folderPathFull);
            UnityEditor.AssetDatabase.Refresh (UnityEditor.ImportAssetOptions.ForceSynchronousImport);
        }

        UnityEditor.AssetDatabase.CreateAsset (db, databasePath);
        UnityEditor.AssetDatabase.SaveAssets ();
        UnityEditor.AssetDatabase.Refresh (UnityEditor.ImportAssetOptions.ForceSynchronousImport);

        databaseLoaded = false;
        Debug.Log ("Finished rebuilding asset database");
        UnityEditor.EditorUtility.ClearProgressBar ();

        #else

        Debug.Log ("ResourceDatabaseManager | SaveDatabase | This method has no effect at runtime");
        
        #endif
    }

    private static void CheckDatabase ()
    {
        if (!databaseLoaded)
            LoadDatabase ();
    }

    public static bool IsDatabaseAvailable ()
    {
        CheckDatabase ();
        return database != null && database.entriesList != null && database.entriesList.Count > 0;
    }

    public static ResourceDatabaseContainer GetDatabase ()
    {
        CheckDatabase ();

        return database;
    }

    public static void LoadDatabase ()
    {
        if (database != null)
            return;
            
        database = (ResourceDatabaseContainer) Resources.Load ("ResourceDatabase");
        
        if (database == null && !databaseGenerationAttempted)
        {
            Debug.Log ("ResourceDatabaseManager | LoadDatabase | Failed to load the resource database, attempting to generate it...");
            databaseGenerationAttempted = true;
            RebuildDatabase ();
        }

        if (database == null)
        {
            Debug.Log ("ResourceDatabaseManager | LoadDatabase | Failed to load the resource database (generation already attempted)");
            return;
        }

        // Dictionaries are not serialized, sidestepping this with a list here
        database.entries = new Dictionary<string, int> (database.entriesList.Count);
        for (int i = 0; i < database.entriesList.Count; ++i)
            database.entries.Add (database.entriesList[i].path, database.entriesList[i].index);

        databaseLoaded = true;
    }

    private static IEnumerable<ResourceDatabaseEntrySerialized> GetFiles (string path)
    {
        // At first, the queue consists of only our initial path
        Queue<string> queue = new Queue<string> ();
        queue.Enqueue (path);

        int depth = 0;
        var extensions = new SortedDictionary<string, int> ();

        int w = 0;
        while (queue.Count > 0)
        {
            path = queue.Dequeue ().Replace (replacementSlashBefore, replacementSlashAfter);

            // But at every iteration, we use the current path to get child directories
            string[] childDirectories = null;
            try
            {
                childDirectories = Directory.GetDirectories (path);
                for (int i = 0; i < childDirectories.Length; ++i)
                {
                    // And if there are child directories, they are added to the queue, ensuring they themselves get the same treatment
                    queue.Enqueue (childDirectories[i]);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine (ex);
            }

            // Next we grab the files for a given directory path
            List<string> childFiles = null;
            try
            {
                childFiles = new List<string> (Directory.GetFiles (path));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine (ex);
            }

            // Finally, we use the files
            if (childFiles != null)
            {
                for (int i = childFiles.Count - 1; i >= 0; --i)
                {
                    var filePath = childFiles[i];
                    
                    if (filePath.Contains (pathFilter))
                        childFiles.RemoveAt (i);

                    if (w == 0 && filePath.Contains (databaseFilename))
                        childFiles.RemoveAt (i);
                }

                List<string> formattedPathsFiles = new List<string> ();
                List<string> formattedPathsFilesDirectories = new List<string> ();

                if (childDirectories != null)
                {
                    for (int i = 0; i < childDirectories.Length; ++i)
                        formattedPathsFilesDirectories.Add (ConvertPathToResourceFormat (childDirectories[i]));
                }

                for (int i = 0; i < childFiles.Count; i++)
                {
                    var filePathFull = childFiles[i];
                    bool extensionUnknown = true;
                    
                    foreach (var extensionCompared in extensionsExpected)
                    {
                        if (filePathFull.EndsWith (extensionCompared))
                        {
                            extensionUnknown = false;
                            break;
                        }
                    }
                    
                    if (extensionUnknown)
                        continue;
                    
                    formattedPathsFiles.Add (ConvertPathToResourceFormat (filePathFull));
                    formattedPathsFilesDirectories.Add (ConvertPathToResourceFormat (filePathFull));
                }

                string pathFormatted = ConvertPathToResourceFormat (path);
                if (w > 0)
                {
                    ResourceDatabaseEntrySerialized directoryDescription = new ResourceDatabaseEntrySerialized (pathFormatted, ConvertPathToParentPath (pathFormatted), formattedPathsFilesDirectories);
                    yield return directoryDescription;
                }

                for (int i = 0; i < formattedPathsFiles.Count; i++)
                {
                    string filePath = formattedPathsFiles[i];
                    string[] filePathSplit = filePath.Split ('.');
                    var extension = filePathSplit.Length > 0 ? filePathSplit[filePathSplit.Length - 1] : null;
                    if (extension != null)
                    {
                        if (extensions.ContainsKey (extension))
                            extensions[extension] += 1;
                        else
                            extensions.Add (extension, 1);
                    }

                    ResourceDatabaseEntrySerialized fileDescription = new ResourceDatabaseEntrySerialized (filePath, pathFormatted);
                    yield return fileDescription;
                }
            }

            ++w;
        }
        
        Debug.Log ($"Discovered {extensions.Count} file types:\n{extensions.ToStringFormattedKeyValuePairs (true, multilinePrefix: "- ")}");
    }

    private static string replacementSlashBefore = "\\";
    private static string replacementSlashAfter = "/";
    private static string replacementPrefixBefore = "Assets/Resources/";
    private static string replacementPrefixAfter = "";

    private static string ConvertPathToResourceFormat (string path)
    {
        if (path.Length <= 17)
            return string.Empty;
        else
            return path.Replace (replacementSlashBefore, replacementSlashAfter).Replace (replacementPrefixBefore, replacementPrefixAfter);
    }

    private static string ConvertPathToParentPath (string path)
    {
        string[] pathSplit = path.Split ('/');
        int length = path.Length - pathSplit[pathSplit.Length - 1].Length - 1;
        if (length <= 0)
            return string.Empty;
        else
            return path.Substring (0, path.Length - pathSplit[pathSplit.Length - 1].Length - 1);
    }

    private static ResourceDatabaseEntryRuntime r_Entry;

    public static void FindResourcesRecursive (List<ResourceDatabaseEntryRuntime> results, ResourceDatabaseEntryRuntime entry, int depth, ResourceDatabaseEntrySerialized.Filetype filetype)
    {
        if (depth <= 1)
            CheckDatabase ();

        if (results == null)
            return;

        if (entry.children.Count == 0)
        {
            if (entry.type == filetype)
                results.Add (entry);
        }
        else
        {
            depth += 1;
            for (int i = 0; i < entry.children.Count; ++i)
            {
                int childIndex = entry.children[i];
                r_Entry = database.entriesList[childIndex];
                if (r_Entry != null)
                    FindResourcesRecursive (results, r_Entry, depth, filetype);
            }
        }
    }

    public static List<ResourceDatabaseEntryRuntime> GetChildrenOfType (this ResourceDatabaseEntryRuntime target, ResourceDatabaseEntrySerialized.Filetype type)
    {
        CheckDatabase ();

        if (database == null)
            return null;
        else
            return target.GetChildrenOfType (database, type);
    }

    public static ResourceDatabaseEntryRuntime GetEntryByPath (this string path)
    {
        CheckDatabase ();

        if (string.IsNullOrEmpty (path))
            return null;
        else
        {
            if (database.entries.ContainsKey (path))
            {
                int index = database.entries[path];
                return database.entriesList[index];
            }
            else
                return null;
        }
    }
}
