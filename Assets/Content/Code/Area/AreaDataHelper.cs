using UnityEngine;
using System;
using System.Collections.Generic;
using PhantomBrigade.Data;

namespace Area
{
    public interface ILevelDataChannel
    {
        public bool TrySaving (DataBlockAreaContent container, string path);
        public bool TryApplyingToScene (DataBlockAreaContent container);
    }
    
    public class LevelContentChannelInfo
    {
        // How the channel is referred to in the content dictionary
        public string alias;

        // Determines sorting
        public int priority;

        // Static functions for constructing instance of a channel from disk.
        // Not part of the interface to avoid allocations and due to missing static method support.
        public Func<DataBlockAreaContent, string, ILevelDataChannel> loadFromDisk;
        public Func<DataBlockAreaContent, ILevelDataChannel> loadFromScene;
    }

    public static class LevelContentHelper
    {
        private static Dictionary<string, LevelContentChannelInfo> channelsRegistered = new Dictionary<string, LevelContentChannelInfo> ();
        private static List<LevelContentChannelInfo> channelsSorted = new List<LevelContentChannelInfo> ();
        private static bool initialized = false;
        private static bool log = false;

        public static void Initialize ()
        {
            if (initialized)
                return;

            initialized = true;
            AreaChannelRoot.Register ();
            AreaChannelUntrackedPoints.Register ();
        }
        
        public static void RegisterDataChannel
        (
            string alias,
            int priority,
            Func<DataBlockAreaContent, string, ILevelDataChannel> loadFromDisk,
            Func<DataBlockAreaContent, ILevelDataChannel> loadFromScene
        )
        {
            if (string.IsNullOrEmpty (alias) || loadFromDisk == null || loadFromScene == null)
                return;

            bool channelAlreadyRegistered = channelsRegistered.ContainsKey (alias);
            if (channelAlreadyRegistered)
                return;
            
            var info = new LevelContentChannelInfo
            {
                alias = alias,
                priority = priority,
                loadFromDisk = loadFromDisk,
                loadFromScene = loadFromScene
            };

            channelsRegistered.Add (alias, info);
            channelsSorted.Add (info);
            channelsSorted.Sort ((x, y) => x.priority.CompareTo (y.priority));
        }

        public static void LoadDataFromDisk (DataBlockAreaContent areaContent, string path)
        {
            if (areaContent == null || areaContent.parent == null)
                return;
            
            var parentKey = areaContent.parent.key;
            areaContent.channels = new Dictionary<string, ILevelDataChannel> ();
            
            if (log)
                Debug.Log ($"Loading content under area {parentKey} from disk using {channelsSorted.Count} registered channels | Path:\n- {path}");

            for (int i = 0, iLimit = channelsSorted.Count; i < iLimit; ++i)
            {
                var channelInfo = channelsSorted[i];
                var channel = channelInfo.loadFromDisk.Invoke (areaContent, path);
                if (channel != null)
                {
                    areaContent.channels.Add (channelInfo.alias, channel);
                    if (log)
                        Debug.Log ($"- Successfully loaded channel {i+1}/{iLimit} {channelInfo.alias} under area {parentKey} from disk");
                }
                else
                {
                    if (log)
                        Debug.LogWarning ($"- Failed to load channel {i+1}/{iLimit} {channelInfo.alias} under area {parentKey} from disk");
                }
            }
        }
        
        public static void LoadDataFromScene (DataBlockAreaContent areaContent)
        {
            if (areaContent == null)
                return;

            var parentKey = areaContent.parent.key;
            areaContent.channels = new Dictionary<string, ILevelDataChannel> ();
            
            if (log)
                Debug.Log ($"Loading content under area {parentKey} from scene using {channelsSorted.Count} registered channels");
            
            for (int i = 0, iLimit = channelsSorted.Count; i < iLimit; ++i)
            {
                var channelInfo = channelsSorted[i];
                var channel = channelInfo.loadFromScene.Invoke (areaContent);
                if (channel != null)
                {
                    areaContent.channels.Add (channelInfo.alias, channel);
                    if (log)
                        Debug.Log ($"- Successfully loaded channel {i+1}/{iLimit} {channelInfo.alias} under area {parentKey} from scene");
                }
                else
                {
                    if (log)
                        Debug.LogWarning ($"- Failed to load channel {i+1}/{iLimit} {channelInfo.alias} under area {parentKey} from scene");
                }
            }
        }

        public static void SaveData (DataBlockAreaContent areaContent, string path)
        {
            if (areaContent == null || areaContent.channels == null)
                return;

            var parentKey = areaContent.parent.key;
            if (log)
                Debug.Log ($"Saving content under area {parentKey} to disk using {channelsSorted.Count} registered channels | Path:\n- {path}");
            
            for (int i = 0, iLimit = channelsSorted.Count; i < iLimit; ++i)
            {
                var channelInfo = channelsSorted[i];
                if (areaContent.channels.TryGetValue (channelInfo.alias, out var channel))
                {
                    bool success = channel.TrySaving (areaContent, path);
                    if (log)
                    {
                        if (success)
                            Debug.Log ($"- Successfully saved channel {i + 1}/{iLimit} {channelInfo.alias} under area {parentKey} to disk");
                        else
                            Debug.LogWarning ($"- Failed to save channel {i + 1}/{iLimit} {channelInfo.alias} under area {parentKey} to disk");
                    }
                }
            }
        }
        
        public static void ApplyDataToScene (DataBlockAreaContent areaContent)
        {
            if (areaContent == null || areaContent.channels == null)
                return;
            
            var parentKey = areaContent.parent.key;
            if (log)
                Debug.Log ($"Applying content under area {parentKey} to scene using {channelsSorted.Count} registered channels");

            for (int i = 0, iLimit = channelsSorted.Count; i < iLimit; ++i)
            {
                var channelInfo = channelsSorted[i];
                if (areaContent.channels.TryGetValue (channelInfo.alias, out var channel))
                {
                    bool success = channel.TryApplyingToScene (areaContent);
                    if (log)
                    {
                        if (success)
                            Debug.Log ($"- Successfully applied channel {i + 1}/{iLimit} {channelInfo.alias} under area {parentKey} to scene");
                        else
                            Debug.LogWarning ($"- Failed to apply channel {i + 1}/{iLimit} {channelInfo.alias} under area {parentKey} to scene");
                    }
                }
            }
        }
    }
}