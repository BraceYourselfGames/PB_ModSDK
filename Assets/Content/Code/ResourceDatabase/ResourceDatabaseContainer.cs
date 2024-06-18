using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Scriptable object carrying the description of all files and directories in Resources. 
/// Isn't meant to be used directly in any class but ResourceDatabaseManager, which 
/// writes a file with this object to Resources and loads/parses the data from it as necessary.
/// </summary>
[System.Serializable]
public class ResourceDatabaseContainer : ScriptableObject
{
    public List<ResourceDatabaseEntrySerialized> files = new List<ResourceDatabaseEntrySerialized> ();
    public List<ResourceDatabaseEntryRuntime> entriesList = new List<ResourceDatabaseEntryRuntime> ();
    public Dictionary<string, int> entries;
}
