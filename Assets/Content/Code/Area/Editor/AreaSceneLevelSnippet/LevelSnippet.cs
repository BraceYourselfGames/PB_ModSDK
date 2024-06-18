using System;
using System.Collections.Generic;
using System.IO;

using PhantomBrigade.Data;

using YamlDotNet.Serialization;

using UnityEngine;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    sealed class LevelSnippet
    {
        [YamlIgnore]
        [HideInInspector]
        public string key;

        public string originalModID;
        public string originalAreaKey;
        public string description;
        public Vector3Int size;
        public List<ILevelSnippetContent> content;

        public (bool OK, LevelData LevelData) Load ()
        {
            try
            {
                content.Sort (OrderByPriority);
                var data = new LevelData ()
                {
                    Bounds = size,
                    Points = new List<AreaVolumePoint> (),
                    Props = new List<AreaPlacementProp> (),
                };
                foreach (var chunk in content)
                {
                    var (pathOK, pathChunk) = chunk.GetContentPath (pathSource);
                    if (!pathOK)
                    {
                        Debug.LogErrorFormat
                        (
                            "Level snippet chunk deserialization error -- unable to resolve chunk path | snippet path: {0} | chunk: {1}",
                            pathSource.FullName,
                            chunk.GetType().Name
                        );
                        return (false, default);
                    }
                    var (ok, errorMessage) = chunk.Deserialize (pathChunk, data);
                    if (ok)
                    {
                        continue;
                    }
                    Debug.LogErrorFormat
                    (
                        "Level snippet chunk deserialization error | snippet path: {0} | chunk: {1} | chunk path: {2}\n{3}",
                        pathSource.FullName,
                        chunk.GetType().Name,
                        pathChunk.FullName,
                        errorMessage
                    );
                    return (false, default);
                }
                return (true, data);
            }
            catch (Exception ex)
            {
                Debug.LogError ("Loading level snippet threw an exception | path: " + pathSource.FullName);
                Debug.LogException (ex);
            }
            return (false, default);
        }

        public bool Serialize (DirectoryInfo path, LevelData levelData)
        {
            var filePath = DataPathHelper.GetCombinedCleanPath (path.FullName, FileName);
            try
            {
                UtilitiesYAML.PrepareClearDirectory(path.FullName, warnAboutDeletions: false, appendApplicationPath: false);
                content.Sort (OrderByPriority);
                var output = new List<ILevelSnippetContent> ();
                foreach (var chunk in content)
                {
                    #if PB_MODSDK
                    var modID = DataContainerModData.selectedMod.id;
                    #else
                    var modID = "";
                    #endif
                    var (result, errorMessage) = chunk.Serialize (modID, path, levelData);
                    if (result == SerializationResult.Error)
                    {
                        Debug.LogErrorFormat
                        (
                            "Level snippet chunk serialization error | snippet path: {0} | chunk: {1}\n{2}",
                            path.FullName,
                            chunk.GetType().Name,
                            errorMessage
                        );
                        return false;
                    }
                    if (result == SerializationResult.Success)
                    {
                        output.Add (chunk);
                    }
                }
                content.Clear ();
                content.AddRange (output);
                UtilitiesYAML.SaveToFile (filePath, this);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError ("Saving level snippet threw an exception | path: " + path.FullName);
                Debug.LogException (ex);
            }
            return false;
        }

        [YamlIgnore]
        [HideInInspector]
        public DirectoryInfo pathSource;

        static int OrderByPriority (ILevelSnippetContent lhs, ILevelSnippetContent rhs) => lhs.GetPriority ().CompareTo (rhs.GetPriority ());

        public const string FileName = "snippet.yaml";
    }
}
