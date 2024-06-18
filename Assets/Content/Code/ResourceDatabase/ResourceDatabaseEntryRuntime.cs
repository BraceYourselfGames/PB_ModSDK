using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceDatabaseEntryRuntime
{
    public int index;
    public string path;
    public string pathTrim;
    public string name;
    public ResourceDatabaseEntrySerialized.Filetype type;
    public UnityEngine.Object content;

    public int parent;
    public List<int> children;

    public ResourceDatabaseEntryRuntime (string path, ResourceDatabaseEntrySerialized.Filetype type)
    {
        this.path = path;
        this.type = type;

        string[] pathSplitDot = path.Split ('.');
        pathTrim = pathSplitDot.Length <= 1 ? path : path.Substring (0, path.Length - pathSplitDot[pathSplitDot.Length - 1].Length - 1);

        string[] pathSplitSlash = pathTrim.Split ('/');
        name = pathSplitSlash[pathSplitSlash.Length - 1];
    }

    public List<ResourceDatabaseEntryRuntime> GetChildrenOfType (ResourceDatabaseContainer db, ResourceDatabaseEntrySerialized.Filetype childType)
    {
        List<ResourceDatabaseEntryRuntime> results = new List<ResourceDatabaseEntryRuntime> ();
        for (int i = 0; i < children.Count; ++i)
        {
            int childIndex = children[i];
            if (db != null && db.entriesList != null && childIndex.IsValidIndex (db.entriesList))
            {
                ResourceDatabaseEntryRuntime child = db.entriesList[childIndex];
                if (child != null && child.type == childType)
                    results.Add (child);
            }
        }

        return results;
    }

    public void ResolveContent ()
    {
        if (type == ResourceDatabaseEntrySerialized.Filetype.Prefab)
            content = Resources.Load<GameObject> (pathTrim);
        else if (type != ResourceDatabaseEntrySerialized.Filetype.Folder && type != ResourceDatabaseEntrySerialized.Filetype.Undefined)
            content = Resources.Load<UnityEngine.Object> (pathTrim);

        if (content == null && type != ResourceDatabaseEntrySerialized.Filetype.Undefined && type != ResourceDatabaseEntrySerialized.Filetype.Folder)
            Debug.LogWarning (string.Format ("Failed to resolve asset from path {0} | Type: {1}", pathTrim, type));  
    }

    public T GetContent<T> () where T : class
    {
        return content != null ? content as T : null;
    }
}
