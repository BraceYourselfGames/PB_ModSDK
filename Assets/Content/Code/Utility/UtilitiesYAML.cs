using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Content.Code.Utility;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PhantomBrigade.Data;

#if !PB_MODSDK
using PhantomBrigade.AI.BT;
using PhantomBrigade.AI.BT.Nodes;
#endif

public static class UtilitiesYAML
{
    private static ISerializer serializer;
    private static StringWriter outputWriter;
    private static StringBuilder outputBuilder;
    private static readonly MemoryStream cloneBuffer = new MemoryStream();

    private static bool logResolvedMappingCollisions = false;
    private static bool tagMappingsInitialized = false;
    private static Dictionary<string, Type> tagMappings;

    public static Dictionary<string, Type> GetTagMappings ()
    {
        if (!tagMappingsInitialized)
            RebuildTagMappings ();

        return tagMappings;
    }

    public static void RebuildTagMappings ()
    {
        tagMappingsInitialized = true;
        if (tagMappings == null)
            tagMappings = new Dictionary<string, Type> ();
        else
            tagMappings.Clear ();

        AddTagMappingsBuiltin ();
    }

    private static void AddTagMappingsBuiltin ()
    {
        //Doing this manually for now, replace this with reflection or something more adaptable long term
        tagMappings.Add ("!UnitFilter", typeof (DataBlockScenarioUnitFilter));
        tagMappings.Add ("!UnitPresetLink", typeof (DataBlockScenarioUnitPresetLink));
        tagMappings.Add ("!UnitPresetEmbedded", typeof (DataBlockScenarioUnitPresetEmbedded));

        tagMappings.Add ("!UnitGroupLink", typeof (DataBlockScenarioUnitGroupLink));
        tagMappings.Add ("!UnitGroupFilter", typeof (DataBlockScenarioUnitGroupFilter));
        tagMappings.Add ("!UnitGroupEmbedded", typeof (DataBlockScenarioUnitGroupEmbedded));

        tagMappings.Add ("!UnitSlotSortingPlayer", typeof (DataBlockScenarioSlotSortingDistancePlayer));
        tagMappings.Add ("!UnitSlotSortingEnemy", typeof (DataBlockScenarioSlotSortingDistanceEnemy));
        tagMappings.Add ("!UnitSlotSortingState", typeof (DataBlockScenarioSlotSortingDistanceState));
        tagMappings.Add ("!UnitSlotSortingRetreat", typeof (DataBlockScenarioSlotSortingDistanceRetreat));
        tagMappings.Add ("!UnitSlotSortingLocation", typeof (DataBlockScenarioSlotSortingDistanceLocation));
        tagMappings.Add ("!UnitSlotSortingSpawn", typeof (DataBlockScenarioSlotSortingDistanceSpawn));

        tagMappings.Add ("!AreaLocation", typeof (DataBlockAreaLocation));
        tagMappings.Add ("!AreaLocationKey", typeof (DataBlockAreaLocationKey));
        tagMappings.Add ("!AreaLocationFilter", typeof (DataBlockAreaLocationTagFilter));
        tagMappings.Add ("!AreaLocationFromState", typeof (DataBlockAreaLocationFromState));

        tagMappings.Add ("!AreaVolume", typeof (DataBlockAreaVolume));
        tagMappings.Add ("!AreaVolumeKey", typeof (DataBlockAreaVolumeKey));
        tagMappings.Add ("!AreaVolumeFilter", typeof (DataBlockAreaVolumeTagFilter));
        tagMappings.Add ("!AreaVolumeFromState", typeof (DataBlockAreaVolumeFromState));

        tagMappings.Add ("!DataBlockGuidanceInputConstant", typeof (DataBlockGuidanceInputConstant));
        tagMappings.Add ("!DataBlockGuidanceInputLinear", typeof (DataBlockGuidanceInputLinear));
        tagMappings.Add ("!DataBlockGuidanceInputCurve", typeof (DataBlockGuidanceInputCurve));

        tagMappings.Add ("!SubsystemResolverHardpoint", typeof (DataBlockSubsystemSlotResolverHardpoint));
        tagMappings.Add ("!SubsystemResolverKeys", typeof (DataBlockSubsystemSlotResolverKeys));
        tagMappings.Add ("!SubsystemResolverTags", typeof (DataBlockSubsystemSlotResolverTags));
        tagMappings.Add ("!SubsystemResolverRules", typeof (DataBlockSubsystemSlotResolverRules));

        tagMappings.Add ("!PartResolverClear", typeof (DataBlockPartSlotResolverClear));
        tagMappings.Add ("!PartResolverKeys", typeof (DataBlockPartSlotResolverKeys));
        tagMappings.Add ("!PartResolverTags", typeof (DataBlockPartSlotResolverTags));

        #if !PB_MODSDK
        //Tags for behaviour tree classes
        {
            tagMappings.Add ("!BehaviorTree", typeof (BehaviorTree));
            tagMappings.Add ("!BTFlatStateSummary", typeof (BTFlatStateSummary));

            var subclassTypes = Assembly
                .GetAssembly (typeof (BTNode))
                .GetTypes ()
                .Where (t => t.IsSubclassOf (typeof (BTNode)) && !t.IsInterface);

            foreach (var type in subclassTypes)
            {
                tagMappings.Add ($"!{type.Name}", type);
            }

            var interfaceTypesVariableSource = Assembly
                .GetAssembly (typeof (IVariableSource))
                .GetTypes ()
                .Where (t => typeof (IVariableSource).IsAssignableFrom (t));

            foreach (var type in interfaceTypesVariableSource)
            {
                tagMappings.Add ($"!{type.Name}", type);
            }

            var interfaceTypesDataBlock = Assembly
                .GetAssembly (typeof (IBTNodeData))
                .GetTypes ()
                .Where (t => typeof (IBTNodeData).IsAssignableFrom (t));

            foreach (var type in interfaceTypesDataBlock)
            {
                tagMappings.Add ($"!{type.Name}", type);
            }
        }
        #endif

        var assemblyBuiltin = Assembly.GetAssembly (typeof (TypeHintedAttribute));
        AddTagMappingsHintedInAssembly (assemblyBuiltin);
    }
    
    public static void AddTagMappingsHintedInAssembly (Assembly assembly)
    {
        if (assembly == null)
            return;

        var typesInAssembly = assembly.DefinedTypes;
        foreach (var type in typesInAssembly)
        {
            bool typeHinted = type.GetCustomAttribute<TypeHintedAttribute> (true) != null;
            if (!typeHinted)
            {
                var interfaces = type.GetInterfaces ();
                foreach (var interfaceType in interfaces)
                {
                    typeHinted = interfaceType.GetCustomAttribute<TypeHintedAttribute> (true) != null;
                    if (typeHinted)
                        break;
                }
            }

            if (!typeHinted)
                continue;

            var typeNameFriendly = GetUserFriendlyName (type);
            var alias = $"!{typeNameFriendly}";

            var typeHintedPrefix = type.GetCustomAttribute<TypeHintedPrefixAttribute> (true);
            if (typeHintedPrefix != null && !string.IsNullOrEmpty (typeHintedPrefix.prefix))
                alias = $"!{typeHintedPrefix.prefix}.{alias.TrimStart ('!')}";

            if (tagMappings.TryGetValue (alias, out var typeConflicting))
            {
                #if PB_MODSDK
                Debug.LogWarning ($"YAML alias conflict unresolved:\n- {typeConflicting.Namespace}.{typeConflicting.GetNiceTypeName ()} already using alias: {alias}\n- {type.Namespace}.{type.GetNiceTypeName ()} can't be registered as fully clarified alias is also already in use: {alias}");
                continue;
                #else
                string aliasExtended = $"!{type.Namespace}.{alias.TrimStart ('!')}";
                if (!tagMappings.ContainsKey (aliasExtended))
                {
                    if (logResolvedMappingCollisions)
                        Debug.Log ($"YAML alias conflict resolved:\n- {typeConflicting.Namespace}.{typeConflicting.GetNiceTypeName ()} already using alias: {alias} | Switching to extended alias: {aliasExtended}");
                    alias = aliasExtended;
                }
                else
                {
                    Debug.LogWarning ($"YAML alias conflict unresolved:\n- {typeConflicting.Namespace}.{typeConflicting.GetNiceTypeName ()} already using alias: {alias}\n- {type.Namespace}.{type.GetNiceTypeName ()} can't be registered as fully clarified alias is also already in use: {aliasExtended}");
                    continue;
                }
                #endif
            }

            tagMappings.Add (alias, type);
        }
    }

    private const string ClosureClassPrefix = "<>c__DisplayClass_";
    private static readonly Dictionary<Type, string> typeDefinitionsToUserFriendlyNames = new Dictionary<Type, string>();
    public static string GetUserFriendlyName(this Type typeDefinition)
    {
        if (!typeDefinitionsToUserFriendlyNames.ContainsKey(typeDefinition))
        {
            var userFriendlyName =
                !typeDefinition.Name.Contains(ClosureClassPrefix)
                    ? typeDefinition.Name
                    : typeDefinition.Name.Replace(ClosureClassPrefix, $"{typeDefinition.DeclaringType.Name}.");

            typeDefinitionsToUserFriendlyNames.Add(typeDefinition, userFriendlyName);
        }

        return typeDefinitionsToUserFriendlyNames[typeDefinition];
    }

    private static void AddTagsToSerializer (SerializerBuilder builder, Dictionary<string, Type> tagMappingsAdded)
    {
        if (tagMappingsAdded == null)
            return;

        foreach (var kvp in tagMappingsAdded)
        {
            builder.WithTagMapping (kvp.Key, kvp.Value);
        }
    }

    private static void AddTagsToDeserializer (DeserializerBuilder builder, Dictionary<string, Type> tagMappingsAdded)
    {
        if (tagMappingsAdded == null)
            return;

        foreach (var kvp in tagMappingsAdded)
        {
            builder.WithTagMapping (kvp.Key, kvp.Value);
        }
    }

    private static void SetupWriter ()
    {
        if (serializer == null)
        {
            var serializerBuilder = new SerializerBuilder ();

            // RebuildTagMappings ();
            var tagMappingsAdded = GetTagMappings ();
            AddTagsToSerializer (serializerBuilder, tagMappingsAdded);

            serializerBuilder.EnsureRoundtrip ();
            serializerBuilder.EmitDefaults ();
            serializerBuilder.IgnoreProperties ();
            serializerBuilder.DisableAliases ();
            serializer = serializerBuilder.Build ();
            //serializer = new Serializer (options: SerializationOptions.Roundtrip | SerializationOptions.EmitDefaults);
        }

        if (outputBuilder == null)
        {
            outputBuilder = new StringBuilder ();
        }

        if (outputWriter == null)
        {
            outputWriter = new StringWriter (outputBuilder);
        }
    }

    private static IDeserializer deserializer;
    private static StringReader inputReader;
    private static StreamReader inputStream;

    private static void CheckDeserializer ()
    {
        if (deserializer != null)
            return;

        RebuildDeserializer ();
    }

    public static void RebuildDeserializer ()
    {
        var deserializerBuilder = new DeserializerBuilder ();
        var tagMappingsAdded = GetTagMappings ();

        AddTagsToDeserializer (deserializerBuilder, tagMappingsAdded);
        deserializerBuilder.IgnoreUnmatchedProperties ();
        deserializerBuilder.WithNamingConvention (new NullNamingConvention ());
        deserializer = deserializerBuilder.Build ();
    }

    #if PB_MODSDK
    // The SDK adds type-hinted types but their in the Editor assembly. In order for
    // files to be properly serialized, we need a way to inject those types into the
    // serializer.
    public static void RebuildSerializer () => serializer = null;
    #endif

    public static void SaveDataToFile<T> (string filePathAndName, T savedObject, bool appendApplicationPath = true)
    {
        if (appendApplicationPath)
            filePathAndName = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), filePathAndName);

        SaveToFile<T> (filePathAndName, savedObject);
    }

    public static void SaveDataToFile<T> (string filePath, string fileName, T savedObject, bool appendApplicationPath = true)
    {
        var filePathAndName =
            appendApplicationPath ?
            DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), filePath, fileName) :
            DataPathHelper.GetCombinedCleanPath (filePath, fileName);

        SaveToFile<T> (filePathAndName, savedObject);
    }

    public const string filenameDirectoryModeMain = "main.yaml";
    private static List<DirectoryInfo> dirsListModifiable = new List<DirectoryInfo> ();

    public static void SaveDecomposedDictionary<T>
    (
        string folderPath,
        IDictionary<string, T> savedDictionary,
        bool warnAboutDeletions = true,
        bool appendApplicationPath = true,
        bool directoryMode = false,
        bool forceLowerCase = true
    ) where T : DataContainer
    {
        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPath);

        if (!Directory.Exists (folderPath))
        {
            try
            {
                Directory.CreateDirectory (folderPath);
                // Debug.Log ($"Utilities | CheckDirectorySafety | Could not find directory, created a new one:\n{folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogException (e);
            }
        }

        var di = new DirectoryInfo (folderPath);

        var dirs = directoryMode ? di.GetDirectories () : null;
        int dirsCount = dirs != null ? dirs.Length : 0;
        bool dirsFound = dirsCount > 0;

        // If we're in directory save mode and there are some dirs present, we need to determine which ones should be preserved
        if (directoryMode && savedDictionary != null && dirsFound)
        {
            // Copy dirs to list for easier modification
            dirsListModifiable.Clear ();
            dirsListModifiable.AddRange (dirs);
            // Debug.Log ($"Folder mode: checking {dirsCount} for exclusions | Path:\n{folderPath}\n\nDirs:\n{dirsListModifiable.ToStringFormatted (true, (x) => $"- {x.Name}")}");

            // Iterate per entry in the dictionary
            foreach (var kvp in savedDictionary)
            {
                if (string.IsNullOrEmpty (kvp.Key))
                    continue;

                // Iterate over current list of dirs backwards to make removals simple
                var key = kvp.Key;
                if (forceLowerCase)
                    key = key.ToLowerInvariant ();

                for (int i = dirsListModifiable.Count - 1; i >= 0; --i)
                {
                    // If there is a name match, eliminate the matched dir from the list and break
                    var dir = dirsListModifiable[i];
                    if (string.Equals (dir.Name, key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        dirsListModifiable.RemoveAt (i);
                        // Debug.Log ($"Folder mode: Excluded dir. {i}/{dirsListModifiable.Count} {dir.Name} from cleanup based on entry {iEntry} {keyLower} | Path:\n{folderPath}\n\nDirs (modified):\n{dirsListModifiable.ToStringFormatted (true, (x) => $"- {x.Name}")}");
                        break;
                    }
                }
            }

            // If there are modifications, update the dir array argument passed to DeleteData
            int dirsCountModified = dirsListModifiable.Count;
            if (dirsCountModified != dirs.Length)
            {
                // Debug.Log ($"Folder mode: Exclusions applied | Dir count changed: {dirs.Length} -> {dirsCountModified} | Path:\n{folderPath}\n\nDirs (modified):\n{dirsListModifiable.ToStringFormatted (true, (x) => $"- {x.Name}")}");
                if (dirsCountModified > 0)
                {
                    // Push to array
                    dirs = dirsListModifiable.ToArray ();
                    dirsCount = dirsCountModified;
                }
                else
                {
                    // If there are none left, no point converting to array, just null
                    dirs = null;
                    dirsCount = dirsCountModified;
                    dirsFound = false;
                }
            }
        }

        var files = di.GetFiles ();
        int filesCount = files != null ? files.Length : 0;
        bool filesFound = filesCount > 0;

        if (dirsFound || filesFound)
        {
            if (!warnAboutDeletions)
                Debug.LogWarning ($"{dirsCount} dirs & {filesCount} files in {folderPath} removed before writing");

            #if UNITY_EDITOR
            if (warnAboutDeletions)
            {
                if (UnityEditor.EditorUtility.DisplayDialog
                    (
                        "Existing files",
                        $"{dirsCount} dirs & {filesCount} need to be removed before writing. Proceed?",
                        "Continue",
                        "Cancel"
                    ))
                {
                    DeleteData (dirs, files, log: true);
                }
            }
            else
                DeleteData (dirs, files, log: false);
            #else
                DeleteData (dirs, files, log: warnAboutDeletions);
            #endif
        }

        if (savedDictionary != null)
        {
            foreach (var kvp in savedDictionary)
            {
                if (string.IsNullOrEmpty (kvp.Key))
                    continue;

                var key = kvp.Key;
                if (forceLowerCase)
                    key = key.ToLowerInvariant ();

                if (directoryMode)
                {
                    var folderPathPerKey = Path.Combine (folderPath, key);
                    if (!Directory.Exists (folderPathPerKey))
                    {
                        try
                        {
                            Directory.CreateDirectory (folderPathPerKey);
                            // Debug.Log ($"Utilities | CheckDirectorySafety | Could not find directory, created a new one:\n{folderPath}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogException (e);
                        }
                    }

                    SaveDataToFile (folderPathPerKey, filenameDirectoryModeMain, kvp.Value);
                }
                else
                {
                    SaveDataToFile (folderPath, key + ".yaml", kvp.Value);
                }
            }
        }
    }

    public static void SaveDecomposedEntryIsolated<T>
    (
        string folderPath,
        string key,
        T value,
        bool appendApplicationPath = true,
        bool directoryMode = false,
        bool forceLowerCase = true
    ) where T : DataContainer
    {
        if (string.IsNullOrEmpty (key) || value == null)
        {
            Debug.LogWarning ($"Failed to save data entry: invalid key or null value");
            return;
        }

        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPath);

        if (!Directory.Exists (folderPath))
        {
            try
            {
                Directory.CreateDirectory (folderPath);
                // Debug.Log ($"Utilities | CheckDirectorySafety | Could not find directory, created a new one:\n{folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogException (e);
            }
        }

        if (forceLowerCase)
            key = key.ToLowerInvariant ();

        if (directoryMode)
        {
            var folderPathPerKey = Path.Combine (folderPath, key);
            if (!Directory.Exists (folderPathPerKey))
            {
                try
                {
                    Directory.CreateDirectory (folderPathPerKey);
                    // Debug.Log ($"Utilities | CheckDirectorySafety | Could not find directory, created a new one:\n{folderPath}");
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                }
            }

            SaveDataToFile (folderPathPerKey, filenameDirectoryModeMain, value);
        }
        else
        {
            SaveDataToFile (folderPath, key + ".yaml", value);
        }
    }

    public const int fileNameLengthMin = 3;
    public const int fileNameLengthMax = 100;

    public const int dirNameLengthMin = 3;
    public const int dirNameLengthMax = 100;

    public static readonly char[] invalidFilenameCharsOther = { '/', '\\', '.', ',' };
    public static readonly char[] invalidPathCharsOther = { '/', '\\', ',' };

    private static bool IsAnyCharPresent (string input, char[] chars, out char charFound)
    {
        charFound = default;

        if (string.IsNullOrEmpty (input))
            return false;

        if (chars == null || chars.Length == 0)
            return false;

        for (int i = 0, iLimit = chars.Length; i < iLimit; ++i)
        {
            var charChecked = chars[i];
            if (input.IndexOf (charChecked) != -1)
            {
                charFound = charChecked;
                return true;
            }
        }

        return false;
    }

    public static bool IsFileNameValid (string name, out string errorDesc)
    {
        if (string.IsNullOrEmpty (name))
        {
            errorDesc = "No input provided";
            return false;
        }

        if (name.Length < fileNameLengthMin)
        {
            errorDesc = "Input is too short";
            return false;
        }

        if (name.Length > fileNameLengthMax)
        {
            errorDesc = "Input is too long";
            return false;
        }

        var invalidFilenameChars = Path.GetInvalidFileNameChars ();
        if (IsAnyCharPresent (name, invalidFilenameChars, out char char0))
        {
            errorDesc = string.Format ("Input has invalid character: {0}", char0);
            return false;
        }

        if (IsAnyCharPresent (name, invalidFilenameCharsOther, out char char1))
        {
            errorDesc = string.Format ("Input has invalid character: {0}", char1);
            return false;
        }

        errorDesc = null;
        return true;
    }

    public static bool IsDirectoryNameValid (string name, out string errorDesc)
    {
        if (string.IsNullOrEmpty (name))
        {
            errorDesc = "No input provided";
            return false;
        }

        if (name.Length < dirNameLengthMin)
        {
            errorDesc = "Input is too short";
            return false;
        }

        if (name.Length > dirNameLengthMax)
        {
            errorDesc = "Input is too long";
            return false;
        }

        var invalidFilenameChars = Path.GetInvalidFileNameChars ();
        if (IsAnyCharPresent (name, invalidFilenameChars, out char char0))
        {
            errorDesc = string.Format ("Input has invalid character: {0}", char0);
            return false;
        }

        var invalidPathChars = Path.GetInvalidPathChars ();
        if (IsAnyCharPresent (name, invalidPathChars, out char char1))
        {
            errorDesc = string.Format ("Input has invalid character: {0}", char1);
            return false;
        }

        if (IsAnyCharPresent (name, invalidPathCharsOther, out char char2))
        {
            errorDesc = string.Format ("Input has invalid character: {0}", char2);
            return false;
        }

        errorDesc = null;
        return true;
    }

    private static int copyDirCount = 0;
    private static int copyFileCount = 0;

    public static void CopyDirectory (string sourcePath, string destPath, bool destClear)
    {
        copyDirCount = 0;
        copyFileCount = 0;

        CopyDirectoryInternal (sourcePath, destPath, destClear, 0);
        Debug.Log ($"Operation concluded with {copyDirCount} directories and {copyFileCount} files copied!\nSource: {sourcePath}\nDestination: {destPath}");
    }

    private static void CopyDirectoryInternal (string sourcePath, string destPath, bool destClear, int depth)
    {
        try
        {
            if (string.IsNullOrEmpty (sourcePath) || string.IsNullOrEmpty (destPath))
            {
                Debug.LogWarning ($"Failed to copy between two directories: one of the inputs is null or empty");
                return;
            }

            var sourceDir = new DirectoryInfo (sourcePath);
            if (!sourceDir.Exists)
            {
                Debug.LogWarning ($"Failed to copy between two directories, source directory doesn't exist: {sourcePath}");
                return;
            }

            bool destExists = Directory.Exists (destPath);
            if (!destExists)
                Directory.CreateDirectory (destPath);
            else if (destClear)
                PrepareClearDirectory (destPath, false, false);

            // Cache directories before we start copying
            var sourceDirChildren = sourceDir.GetDirectories ();
            copyDirCount += sourceDirChildren.Length;

            // Create the destination directory
            if (!Directory.Exists (destPath))
                Directory.CreateDirectory (destPath);

            // Get the files in the source directory and copy to the destination directory
            var files = sourceDir.GetFiles ();
            copyFileCount += files.Length;

            foreach (FileInfo file in files)
            {
                string targetFilePath = Path.Combine (destPath, file.Name);
                file.CopyTo (targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            foreach (DirectoryInfo subDir in sourceDirChildren)
            {
                string newDestinationDir = Path.Combine (destPath, subDir.Name);
                CopyDirectoryInternal (subDir.FullName, newDestinationDir, true, depth + 1);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning (e);
            throw;
        }
    }

    public static void PrepareClearDirectory (string folderPathInput, bool warnAboutDeletions, bool appendApplicationPath)
    {
        var folderPath = folderPathInput;
        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPathInput);

        if (!Directory.Exists (folderPath))
        {
            try
            {
                Directory.CreateDirectory (folderPath);
                Debug.Log ($"Utilities | Could not find directory, created a new one:\n{folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogException (e);
            }
        }

        var di = new DirectoryInfo (folderPath);
        var dirs = di.GetDirectories ();
        var files = di.GetFiles ();

        if (dirs.Length > 0 || files.Length > 0)
        {
            if (!warnAboutDeletions)
                Debug.LogWarning ($"There were {dirs.Length} directories and {files.Length} files in {folderPath}, deleted them");

            #if UNITY_EDITOR
            if (warnAboutDeletions)
            {
                if (UnityEditor.EditorUtility.DisplayDialog
                    (
                        "Existing files",
                        $"There are {dirs.Length} directories and {files.Length} files in {folderPath} - continuing will delete them. Proceed?",
                        "Continue",
                        "Cancel"
                    ))
                {
                    DeleteData (dirs, files, log: true);
                }
            }
            else
                DeleteData (dirs, files, log: false);
            #else
                DeleteData (dirs, files, log: warnAboutDeletions);
            #endif
        }
    }

    public static void DeleteData (DirectoryInfo[] dirs, FileInfo[] files, string[] filters = null, bool log = false)
    {
        if (files != null && files.Length > 0)
        {
            // Debug.LogWarning ($"Deleting {files.Length} files");
            foreach (FileInfo file in files)
            {
                if (file == null)
                    continue;

                try
                {
                    if(filters == null || filters.Length == 0)
                        file.Delete();
                    else
                    {
                        bool isMatch = false;
                        foreach (var filter in filters)
                        {
                            if(file.Name.Equals(filter, StringComparison.InvariantCulture))
                            {
                                isMatch = true;
                                break;
                            }
                        }

                        if(!isMatch)
                            file.Delete();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                }
            }
        }

        if (dirs != null && dirs.Length > 0)
        {
            // Debug.LogWarning ($"Deleting {dirs.Length} directories");
            foreach (DirectoryInfo dir in dirs)
            {
                if (dir == null)
                    continue;

                try
                {
                    dir.Delete (true);
                }
                catch (Exception e)
                {
                    Debug.LogException (e);
                }
            }
        }
    }

    public static T LoadDataFromFile<T> (string filePathAndName, bool warnIfMissing = true, bool appendApplicationPath = true)
    {
        if (appendApplicationPath)
            filePathAndName = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), filePathAndName);
        return ReadFromFile<T> (filePathAndName, warnIfMissing);
    }

    public static T LoadDataFromFile<T> (string filePath, string fileName, bool warnIfMissing = true, bool appendApplicationPath = true)
    {
        var filePathAndName =
            appendApplicationPath ?
            DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), filePath, fileName) :
            DataPathHelper.GetCombinedCleanPath (filePath, fileName);

        return ReadFromFile<T> (filePathAndName, warnIfMissing);
    }

    public static object LoadDataFromFile (Type type, string filePath, string fileName, bool warnIfMissing = true, bool appendApplicationPath = true)
    {
        var filePathAndName =
            appendApplicationPath ?
                DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), filePath, fileName) :
                DataPathHelper.GetCombinedCleanPath (filePath, fileName);

        return ReadFromFile (type, filePathAndName, warnIfMissing);
    }

    public static List<string> GetFileList (string folderPath, string extension = "yaml", bool appendApplicationPath = true)
    {
        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPath);
        var result = new List<string> ();

        try
        {
            var files = Directory.GetFiles (folderPath);
            for (int i = 0; i < files.Length; ++i)
            {
                try
                {
                    string filePath = files[i];
                    var fileExtension = Path.GetExtension (filePath);
                    if (!string.Equals (fileExtension, extension))
                        continue;

                    var key = Path.GetFileNameWithoutExtension (filePath);
                    result.Add (key);
                }
                catch (Exception e)
                {
                    Debug.Log ($"Failed to access file at path: {folderPath} | Exception: {e}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log ($"Failed to fetch a list of files at path: {folderPath} | Exception: {e}");
        }

        return result;
    }

    public static List<string> GetDirectoryList (string folderPath, bool appendApplicationPath = true)
    {
        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPath);
        var result = new List<string> ();

        try
        {
            var dirs = Directory.GetDirectories (folderPath);
            foreach (var dirPath in dirs)
            {
                var dirInfo = new DirectoryInfo (dirPath);
                result.Add (dirInfo.Name);
            }

        }
        catch(DirectoryNotFoundException e)
        {
        }
        catch (Exception e)
        {
            Debug.Log ($"Failed to fetch a list of files at path: {folderPath} | Exception: {e}");
        }

        return result;
    }

    private const string yamlExtension = ".yaml";
    private const string gitFolderName = ".git";

    public static SortedDictionary<string, T> LoadDecomposedDictionary <T>
    (
        string folderPath,
        bool logExceptions = false,
        bool appendApplicationPath = true,
        bool directoryMode = false,
        string directoryModeFilename = null,
        bool forceLowerCase = true
    ) where T : DataContainer
    {
        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPath);
        var result = new SortedDictionary<string, T> ();

        try
        {
            if (directoryMode)
            {
                var filenameMain = filenameDirectoryModeMain;
                if (!string.IsNullOrEmpty (directoryModeFilename))
                    filenameMain = directoryModeFilename + yamlExtension;

                var dirs = Directory.GetDirectories (folderPath);
                for (int i = 0; i < dirs.Length; ++i)
                {
                    string dirPath = dirs[i];
                    if (dirPath.EndsWith (gitFolderName))
                        continue;
                    
                    var key = Path.GetFileName (dirPath);
                    if (forceLowerCase)
                        key = key.ToLowerInvariant ();

                    if (string.IsNullOrEmpty (key) || result.ContainsKey (key))
                        continue;

                    var filePath = Path.Combine (dirPath, filenameMain);
                    try
                    {
                        T dataObject = ReadFromFile<T> (filePath);
                        if (dataObject != null)
                            result.Add (key, dataObject);
                    }
                    catch (Exception e)
                    {
                        Debug.Log ($"Failed to load file at path: {filePath} | Exception: {e}");
                    }
                }
            }
            else
            {
                var files = Directory.GetFiles (folderPath);
                for (int i = 0; i < files.Length; ++i)
                {
                    string filePath = files[i];
                    try
                    {
                        var fileExtension = Path.GetExtension (filePath);
                        if (!string.Equals (fileExtension, yamlExtension))
                            continue;

                        T dataObject = ReadFromFile<T> (filePath);
                        var key = Path.GetFileNameWithoutExtension (filePath);
                        if (forceLowerCase)
                            key = key.ToLowerInvariant ();

                        if (!string.IsNullOrEmpty (key) && !result.ContainsKey (key) && dataObject != null)
                            result.Add (key, dataObject);
                    }
                    catch (Exception e)
                    {
                        Debug.Log ($"Failed to load file at path: {filePath} | Exception: {e}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (logExceptions)
                Debug.Log ($"Failed to fetch a list of files at path: {folderPath} | Exception: {e}");
        }

        return result;
    }

    public static T LoadDecomposedEntryIsolated <T>
    (
        string folderPath,
        string key,
        bool logExceptions = false,
        bool appendApplicationPath = true,
        bool directoryMode = false,
        bool forceLowerCase = true
    ) where T : DataContainer
    {
        if (string.IsNullOrEmpty (key))
        {
            Debug.Log ($"Failed to load isolated data container: invalid key provided");
            return null;
        }

        if (appendApplicationPath)
            folderPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), folderPath);

        T result = null;

        if (forceLowerCase)
            key = key.ToLowerInvariant ();

        try
        {
            if (directoryMode)
            {
                var dirPath = Path.Combine (folderPath, key);
                if (!Directory.Exists (dirPath))
                {
                    Debug.Log ($"Failed to load isolated data container: folder {key} not found at {folderPath}");
                    return null;
                }

                var filePath = Path.Combine (dirPath, filenameDirectoryModeMain);
                try
                {
                    result = ReadFromFile<T> (filePath);
                }
                catch (Exception e)
                {
                    Debug.Log ($"Failed to load file at path: {filePath} | Exception: {e}");
                }
            }
            else
            {
                var filePath = Path.Combine (folderPath, $"{key}.yaml");
                try
                {
                    result = ReadFromFile<T> (filePath);
                }
                catch (Exception e)
                {
                    Debug.Log ($"Failed to load file at path: {filePath} | Exception: {e}");
                }
            }
        }
        catch (Exception e)
        {
            if (logExceptions)
                Debug.Log ($"Failed to fetch a file at path: {folderPath} | Exception: {e}");
        }

        return result;
    }

    private static void CheckDirectorySafety (string absolutePath, string fileName)
    {
        string path = Path.Combine (absolutePath, fileName).Replace ('\\', '/').Replace ("//", "/");
        string[] pathSplit = path.Split ('/');

        // Important to do this since fileName argument is not guaranteed to contain only filename and might have subfolders
        int lengthToTrim = pathSplit[pathSplit.Length - 1].Length + 1;
        string pathToCheck = path.Substring (0, path.Length - lengthToTrim);

        if (!Directory.Exists (pathToCheck))
        {
            Debug.LogWarning ("Utilities | CheckDirectorySafety | Could not find directory: " + pathToCheck);
            try
            {
                System.IO.Directory.CreateDirectory (pathToCheck);
                Debug.Log ("Utilities | CheckDirectorySafety | Created directory: " + pathToCheck);
            }
            catch (Exception e)
            {
                Debug.LogException (e);
            }
        }
    }

    public static void SaveToFile<T> (string absolutePath, string fileName, T savedObject)
    {
        CheckDirectorySafety (absolutePath, fileName);
        string path = Path.Combine (absolutePath, fileName).Replace ('\\', '/').Replace ("//", "/");
        SaveToFileFinalized (path, savedObject);
    }

    public static void SaveToFile<T> (string absolutePathWithFilename, T savedObject)
    {
        var filename = Path.GetFileName (absolutePathWithFilename);
        var folder = Path.GetDirectoryName (absolutePathWithFilename);
        CheckDirectorySafety (folder, filename);
        string path = absolutePathWithFilename.Replace ('\\', '/').Replace ("//", "/");
        SaveToFileFinalized (path, savedObject);
    }

    private static void SaveToFileFinalized<T> (string path, T savedObject)
    {
        SetupWriter ();
        outputBuilder.Length = 0;

        serializer.Serialize (outputWriter, savedObject);
        File.WriteAllText (path, outputBuilder.ToString ());

        if (Constants.debugSaving)
        {
            Debug.Log ("Utilities | SaveToFile | Saved file to: " + path);
            Debug.Log (outputBuilder.ToString ());
        }
    }

    /// <summary>
    /// Loads data from an absolute path (typically you want to use the application paths)
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <param name="FileName"></param>
    /// <param name="Object"></param>
    public static T ReadFromFile<T> (string absolutePathWithFileName, bool warnIfMissing = true)
    {
        CheckDeserializer ();

        if (!File.Exists (absolutePathWithFileName))
        {
            if (warnIfMissing)
                Debug.Log ("Utilities | ReadFromFile | File doesn't exist: " + absolutePathWithFileName);
            return default (T);
        }

        try
        {
            if (Constants.debugSaving)
                Debug.Log ("Utilities | ReadFromFile | Using path: " + absolutePathWithFileName);

            inputStream = File.OpenText (absolutePathWithFileName);
            inputReader = new StringReader (inputStream.ReadToEnd ());
            inputStream.Close ();
        }
        catch (Exception e)
        {
            Debug.LogWarning ($"Failed to read from file {absolutePathWithFileName} (no file access or read error), exception follows:");
            Debug.LogException (e);
            return default (T);
        }

        try
        {
            var deserialized = deserializer.Deserialize<T> (inputReader);

            if (Constants.debugSaving)
                Debug.Log ("Utilities | ReadFromFile | File successfully loaded from: " + absolutePathWithFileName);

            return (T)deserialized;
        }
        catch (Exception e)
        {
            Debug.LogWarning ($"Failed to read from file {absolutePathWithFileName} (deserialization failed), exception follows:");
            Debug.LogException (e);
            return default (T);
        }
    }

    /// <summary>
    /// Loads data from an absolute path (typically you want to use the application paths).
    /// Use this version in dynamic contexts where generics can't be used.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="absolutePath"></param>
    /// <param name="FileName"></param>
    /// <param name="Object"></param>
    public static object ReadFromFile (Type type, string absolutePathWithFileName, bool warnIfMissing = true)
    {
        if (type == null)
        {
            Debug.Log ("Utilities | ReadFromFile | Can't read due to null type reference");
            return null;
        }
        CheckDeserializer ();

        if (!File.Exists (absolutePathWithFileName))
        {
            if (warnIfMissing)
                Debug.Log ("Utilities | ReadFromFile | File doesn't exist: " + absolutePathWithFileName);
            return null;
        }

        try
        {
            if (Constants.debugSaving)
                Debug.Log ("Utilities | ReadFromFile | Using path: " + absolutePathWithFileName);

            inputStream = File.OpenText (absolutePathWithFileName);
            inputReader = new StringReader (inputStream.ReadToEnd ());
            inputStream.Close ();
        }
        catch (Exception e)
        {
            Debug.LogException (e);
            return null;
        }

        if (Constants.debugSaving)
            Debug.Log ("Utilities | ReadFromFile | File successfully loaded from: " + absolutePathWithFileName);

        var deserialized = deserializer.Deserialize (inputReader, type);
        return deserialized;
    }

    /// <summary>
    /// deep copy and sanitize a serializable object by serializing and deserializing it through a memory stream
    /// </summary>
    /// <param name="sourceObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CloneThroughYaml<T> (T sourceObject)
    {
        cloneBuffer.Position = 0;
        cloneBuffer.SetLength (0);

        var writer = new StreamWriter (cloneBuffer);
        var reader = new StreamReader (cloneBuffer);

        SetupWriter ();
        serializer.Serialize (writer, sourceObject);
        writer.Flush ();

        cloneBuffer.Position = 0;

        CheckDeserializer ();
        return deserializer.Deserialize<T> (reader);
    }

    public static string ConvertToYaml<T> (T sourceObject)
    {
        if (sourceObject == null)
            return null;

        SetupWriter ();
        outputBuilder.Length = 0;

        serializer.Serialize (outputWriter, sourceObject);
        var result = outputBuilder.ToString ();
        return result;
    }

    private static StringBuilder sb = new StringBuilder ();
    private static Dictionary<string, string> typeNameSubstitutions = new Dictionary<string, string>
    {
        { "String", "string" },
        { "Boolean", "bool" },
        { "Int32", "int" },
        { "Single", "float" },
        { "Double", "double" }
    };

    public static string GetNiceTypeName (this Type t)
    {
        if (t == null)
            return null;

        if (!t.IsGenericType)
        {
            var typeNameValue = t.Name;
            if (typeNameSubstitutions.TryGetValue (typeNameValue, out var typeNameAdjusted))
                typeNameValue = typeNameAdjusted;

            return typeNameValue;
        }

        var typeName = t.Name;

        sb.Clear ();
        sb.Append (typeName.Substring(0, typeName.IndexOf ('`')));
        sb.Append ('<');

        bool appendComma = false;
        foreach (Type arg in t.GetGenericArguments ())
        {
            if (appendComma)
                sb.Append (',');
            sb.Append (GetNiceTypeName (arg));
            appendComma = true;
        }

        sb.Append ('>');
        return sb.ToString ();
    }
}
