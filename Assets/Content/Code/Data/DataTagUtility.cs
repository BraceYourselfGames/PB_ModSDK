using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public interface IDataContainerTagged
    {
        HashSet<string> GetTags (bool processed);

        bool IsHidden ();
    }
    
    public interface IDataContainerGenerationCustom
    {
        DataContainer GetGeneratedInstance ();
    }

    public interface IDataContainerKeyReplacementWarning
    {
        void KeyReplacementWarning ();
    }
    
    public static class DataTagUtility
    {
        private static List<string> tagsTempSorted = new List<string> ();
        private const string suffixWarning = " ";
        
        public static void RegisterTags<T> 
        (
            IDictionary<string, T> data, 
            ref HashSet<string> tags, 
            ref Dictionary<string, HashSet<string>> tagsMap,
            bool processed = true
        )   
            where T : DataContainer, IDataContainerTagged, new()
        {
            var typeName = typeof (T).Name;
            
            if (tags == null)
                tags = new HashSet<string> ();
            else
                tags.Clear ();
            
            if (tagsMap == null)
                tagsMap = new Dictionary<string, HashSet<string>> ();
            else
                tagsMap.Clear ();
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;
                
                var tagsInContainer = container.GetTags (processed);
                if (tagsInContainer == null)
                    continue;

                foreach (var tag in tagsInContainer)
                {
                    if (!tags.Contains (tag))
                        tags.Add (tag);
                    
                    if (!tagsMap.ContainsKey (tag))
                        tagsMap.Add (tag, new HashSet<string> ());

                    var map = tagsMap[tag];
                    if (!map.Contains (key))
                        map.Add (key);
                    
                    if (tag.Contains ("\n") || tags.Contains (Environment.NewLine))
                        Debug.LogWarning ($"{typeName}/{key} contains newlines in tag: {tag}");
                    
                    if (tag.EndsWith (suffixWarning))
                        Debug.LogWarning ($"{typeName}/{key} ends tag with a space: {tag}");
                }
            }
            
            tagsTempSorted.Clear ();
            tagsTempSorted.AddRange (tags);
            tagsTempSorted.Sort ();
            
            tags.Clear ();
            foreach (var tag in tagsTempSorted)
                tags.Add (tag);
        }
        
        private static List<string> keysWithTags = new List<string> ();
        private const string tagSuffixMandatoryInclusion = "_req";
        
        public static List<string> GetKeysWithTags<T> 
        (
            IDictionary<string, T> data, 
            IDictionary<string, bool> tagsFilter, 
            bool processed = true, 
            bool returnAllOnEmptyFilter = false,
            bool filterHidden = true,
            int limit = 0
        ) 
            where T : DataContainer, IDataContainerTagged, new()
        {
            keysWithTags.Clear ();
            
            if (tagsFilter == null || tagsFilter.Count == 0)
            {
                if (returnAllOnEmptyFilter)
                {
                    foreach (var kvp in data)
                        keysWithTags.Add (kvp.Key);
                }
                else
                    Debug.LogWarning ("Can't return data container keys using null or empty tag collection");
                return keysWithTags;
            }
            
            if (data == null)
                return keysWithTags;

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;
                
                if (filterHidden && container.IsHidden ())
                    continue;
                
                bool invalid = false;
                var tagsInContainer = container.GetTags (processed);
                bool tagsInContainerPresent = tagsInContainer != null && tagsInContainer.Count > 0;
                
                if (!tagsInContainerPresent)
                    continue;
                
                foreach (var kvp2 in tagsFilter)
                {
                    string tag = kvp2.Key;
                    bool required = kvp2.Value;
                    bool present = tagsInContainer.Contains (tag);
                    
                    if (present != required)
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                    continue;
                
                // Iterate on every tag within the container to see if any tags are of required-mention type
                // Those tags must be present in a filter for a container to be successfully filtered
                // This enables inverted control over random generation for cases where chasing down every
                // filter is not viable, such as ensuring a special weapon fails to appear across 300+ unit presets
                
                bool requiredTagMissing = false;
                foreach (var tag in tagsInContainer)
                {
                    // Skip missing tags
                    if (string.IsNullOrEmpty (tag))
                        continue;
                    
                    // Skip tags without the desired prefix
                    if (!tag.EndsWith (tagSuffixMandatoryInclusion))
                        continue;
                    
                    bool tagFiltered = tagsFilter.TryGetValue (tag, out var tagFilterEntry);
                    bool tagSpecificallyRequested = tagFiltered && tagFilterEntry == true;

                    if (!tagSpecificallyRequested)
                    {
                        requiredTagMissing = true;
                        break;
                    }
                }
                
                if (requiredTagMissing)
                    continue;

                keysWithTags.Add (key);
            }

            if (limit > 0 && keysWithTags.Count > limit)
            {
                Sirenix.Utilities.ListExtensions.SetLength (keysWithTags, limit);
                keysWithTags.Add ($"... +{limit}");
            }

            return keysWithTags;
        }

        private static SortedDictionary<string, bool> tagsFilterTemp = new SortedDictionary<string, bool> ();
        
        public static List<string> GetKeysWithTags<T> 
        (
            IDictionary<string, T> data, 
            List<DataBlockOverworldEventSubcheckTag> tagsFilterLegacy,
            bool processed = true, 
            bool returnAllOnEmptyFilter = false,
            bool filterHidden = true
        ) 
            where T : DataContainer, IDataContainerTagged, new()
        {
            tagsFilterTemp.Clear ();

            if (tagsFilterLegacy != null)
            {
                foreach (var block in tagsFilterLegacy)
                {
                    string tag = block.tag;
                    bool required = !block.not;
                    tagsFilterTemp[tag] = required;
                }
            }

            return GetKeysWithTags (data, tagsFilterTemp, processed, returnAllOnEmptyFilter, filterHidden);
        }
        
        private static List<IDataContainerTagged> containersWithTags = new List<IDataContainerTagged> ();
        
        public static List<IDataContainerTagged> GetContainersWithTags<T>
        (
            IDictionary<string, T> data, 
            IDictionary<string, bool> tagsFilter,
            bool processed = true,
            EntityCheckMethod checkMethod = EntityCheckMethod.RequireAll, 
            bool returnAllOnEmptyFilter = false,
            bool filterHidden = true
        ) 
            where T : DataContainer, IDataContainerTagged, new()
        {
            containersWithTags.Clear ();
            
            if (tagsFilter == null || tagsFilter.Count == 0)
            {
                if (returnAllOnEmptyFilter)
                {
                    foreach (var kvp in data)
                        containersWithTags.Add (kvp.Value);
                }
                else
                    Debug.LogWarning ("Can't return data containers using null or empty tag collection");
                return containersWithTags;
            }
            
            if (data == null)
                return containersWithTags;

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var container = kvp.Value;
                
                if (filterHidden && container.IsHidden ())
                    continue;
                
                bool anyInvalid = false;
                bool anyValid = false;
                
                var tagsInContainer = container.GetTags (processed);
                bool tagsInContainerPresent = tagsInContainer != null && tagsInContainer.Count > 0;
                
                if (!tagsInContainerPresent)
                    continue;

                foreach (var kvp2 in tagsFilter)
                {
                    string tag = kvp2.Key;
                    bool required = kvp2.Value;
                    bool present = tagsInContainerPresent && tagsInContainer.Contains (tag);
                    
                    if (present != required)
	                    anyInvalid = true;
                    else
						anyValid = true;

                    if (anyInvalid && checkMethod == EntityCheckMethod.RequireAll)
						break;

                    if (anyValid && checkMethod == EntityCheckMethod.RequireOne)
						break;
                }
                
                if (anyInvalid && checkMethod == EntityCheckMethod.RequireAll)
                    continue;

                if (!anyValid && checkMethod == EntityCheckMethod.RequireOne)
					continue;

                // Iterate on every tag within the container to see if any tags are of required-mention type
                // Those tags must be present in a filter for a container to be successfully filtered
                // This enables inverted control over random generation for cases where chasing down every
                // filter is not viable, such as ensuring a special weapon fails to appear across 300+ unit presets

                bool requiredTagMissing = false;
                foreach (var tag in tagsInContainer)
                {
                    // Skip missing tags
                    if (string.IsNullOrEmpty (tag))
                        continue;
                    
                    // Skip tags without the desired prefix
                    if (!tag.EndsWith (tagSuffixMandatoryInclusion))
                        continue;
                    
                    bool tagFiltered = tagsFilter.TryGetValue (tag, out var tagFilterEntry);
                    bool tagSpecificallyRequested = tagFiltered && tagFilterEntry == true;

                    if (!tagSpecificallyRequested)
                    {
                        requiredTagMissing = true;
                        break;
                    }
                }
                
                if (requiredTagMissing)
                    continue;
                
                containersWithTags.Add (container);
            }

            return containersWithTags;
        }

        public static bool AreContainerTagsMatchingFilter<T>
        (
            T container,
            IDictionary<string, bool> tags,
            bool processed = true,
            EntityCheckMethod checkMethod = EntityCheckMethod.RequireAll
        )
            where T : DataContainer, IDataContainerTagged, new()
        {
            if (container == null)
                return false;
            
            bool anyInvalid = false;
            bool anyValid = false;
                
            var tagsInContainer = container.GetTags (processed);
            bool tagsInContainerPresent = tagsInContainer != null;

            foreach (var kvp2 in tags)
            {
                string tag = kvp2.Key;
                bool required = kvp2.Value;
                bool present = tagsInContainerPresent && tagsInContainer.Contains (tag);
                    
                if (present != required)
                    anyInvalid = true;
                else
                    anyValid = true;

                if (anyInvalid && checkMethod == EntityCheckMethod.RequireAll)
                    break;

                if (anyValid && checkMethod == EntityCheckMethod.RequireOne)
                    break;
            }
            
            if (anyInvalid && checkMethod == EntityCheckMethod.RequireAll)
                return false;

            if (!anyValid && checkMethod == EntityCheckMethod.RequireOne)
                return false;

            return true;
        }
    }
}

