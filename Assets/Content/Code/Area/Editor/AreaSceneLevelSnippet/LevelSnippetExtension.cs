using System.IO;

using PhantomBrigade.Data;
using PhantomBrigade.Mods;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    public interface ILevelSnippetExtension : ILevelExtension { }

    public enum SerializationResult
    {
        Error = 0,
        Success,
        Empty,
    }

    // This is the class that modders will inherit from if they have a mod that extends snippet content.
    public abstract class LevelSnippetContent : ILevelSnippetExtension, ILevelSnippetContent
    {
        public abstract (bool OK, string ErrorMessage) Deserialize (DirectoryInfo contentPath, LevelData data);
        public abstract (SerializationResult Result, string ErrorMessage) Serialize (string modID, DirectoryInfo contentPath, LevelData data);

        public int priority;
        public string modID;
        public string relativePath;

        // Below here are internals.

        // Implementations of interface methods have to be public in C# even if the interface is internal
        // These two implementation methods should really be internal as well.

        public int GetPriority () => GetPriorityInternal ();
        public (bool OK, DirectoryInfo ContentPath) GetContentPath (DirectoryInfo snippetPath)
        {
            var path = snippetPath.FullName;
            #if PB_MODSDK
            if (DataContainerModData.hasSelectedConfigs && modID == DataContainerModData.selectedMod.id)
            {
                if (!string.IsNullOrWhiteSpace (relativePath))
                {
                    path = DataPathHelper.GetCombinedCleanPath (snippetPath.FullName, relativePath);
                }
                var diMod = new DirectoryInfo (path);
                return (diMod.Exists, diMod);
            }
            #endif
            if (ModManager.loadedModsLookup.TryGetValue (modID, out var mod) && mod != null)
            {
                path = DataPathHelper.GetCombinedCleanPath (mod.metadata.path, LevelSnippetManager.DirectoryName, snippetPath.Name);
            }
            if (!string.IsNullOrWhiteSpace (relativePath))
            {
                path = DataPathHelper.GetCombinedCleanPath (snippetPath.FullName, relativePath);
            }
            var di = new DirectoryInfo (path);
            return (di.Exists, di);
        }

        // Negative priority values are reserved for core (built-in) content.
        protected private virtual int GetPriorityInternal() =>  priority < 0 ? 0 : priority;
    }

    // This interface is internal because modders should be inheriting from the LevelSnippetContentExtension class.
    interface ILevelSnippetContent
    {
        int GetPriority ();
        (bool OK, DirectoryInfo ContentPath) GetContentPath (DirectoryInfo snippetPath);
        (bool OK, string ErrorMessage) Deserialize (DirectoryInfo contentPath, LevelData data);
        (SerializationResult Result, string ErrorMessage) Serialize (string modID, DirectoryInfo contentPath, LevelData data);
    }
}
