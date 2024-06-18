using System.Collections.Generic;


[System.Serializable]
public class ResourceDatabaseEntrySerialized
{
    public enum Filetype
    {
        Folder,
        Asset,
        Prefab,
        Material,
        Texture,
        Undefined
    }


    public string path;
    public string pathTrimmed;
    public string pathParent;
    public List<string> pathsChildren;
    public Filetype type;

    public ResourceDatabaseEntrySerialized (string path, string pathParent)
    {
        this.path = path;
        this.pathParent = pathParent;
        this.pathsChildren = new List<string> ();
        ParseExtension ();
    }

    public ResourceDatabaseEntrySerialized (string path, string pathParent, List<string> children)
    {
        this.path = path;
        this.pathParent = pathParent;
        this.pathsChildren = children;
        ParseExtension ();
    }

    public string GetChild (int index)
    {
        if (index < 0 || index > pathsChildren.Count - 1)
            return string.Empty;
        else
            return pathsChildren[index];
    }

    public bool IsDirectory ()
    {
        return pathsChildren.Count > 0;
    }

    private void ParseExtension ()
    {
        string[] splitPath = path.Split ('.');
        string extension = splitPath[splitPath.Length - 1];

        if (pathsChildren.Count > 0)
            type = Filetype.Folder;
        else if (string.Equals (extension, "asset"))
            type = Filetype.Asset;
        else if (string.Equals (extension, "prefab"))
            type = Filetype.Prefab;
        else if (string.Equals (extension, "mat"))
            type = Filetype.Material;
        else if (string.Equals (extension, "png"))
            type = Filetype.Texture;
        else
            type = Filetype.Undefined;
    }
}
