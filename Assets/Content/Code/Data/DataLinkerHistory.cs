using System.Collections.Generic;
using Sirenix.OdinInspector;

using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataLinkerHistory : DataLinker<DataContainerHistory>
    {
        public static void RegisterKeyChange (string group, string keyOld, string keyNew)
        {
            if (string.IsNullOrEmpty (group) || string.IsNullOrEmpty (keyOld) || string.IsNullOrEmpty (keyNew))
            {
                Debug.LogWarning ($"Can't register key replacement - group key, old key or new key string is null or empty");
                return;
            }
            
            if (data == null || data.keyReplacementGroups == null)
            {
                Debug.LogWarning ($"Can't register key replacement - data not available");
                return;
            }

            var keyReplacementGroups = data.keyReplacementGroups;
            if (!keyReplacementGroups.ContainsKey (group))
                keyReplacementGroups.Add (group, new KeyReplacementGroup { keyReplacements = new List<KeyReplacement> () });

            var keyReplacements = keyReplacementGroups[group].keyReplacements;
            bool keyReplacementCollision = false;
            
            foreach (var keyReplacement in keyReplacements)
            {
                if (keyReplacement.keyOld == keyOld)
                {
                    Debug.LogWarning ($"Key replacement history for type {group} already contains pair {keyOld} -> {keyReplacement.keyNew}), overwriting it with {keyOld} -> {keyNew}");
                    keyReplacement.keyNew = keyNew;
                    keyReplacementCollision = true;
                }
            }

            if (!keyReplacementCollision)
                keyReplacements.Add (new KeyReplacement { keyOld = keyOld, keyNew = keyNew });
        }
        
        public static bool IsKeyReplacementAvailable (string type, string keyOld, out string keyNew, out int index)
        {
            index = -1;
            keyNew = null;
            
            // If input key is null or empty or no replacement data is present, we can bail
            if (data == null || data.keyReplacementGroups == null)
                return false;

            // If provided type key is null or can't be found, we can bail
            if (string.IsNullOrEmpty (type) || !data.keyReplacementGroups.ContainsKey (type))
                return false;

            // If group is null or has no replacement list, we can bail
            var group = data.keyReplacementGroups[type];
            if (group == null || group.keyReplacements == null)
                return false;

            for (int i = 0, count = group.keyReplacements.Count; i < count; ++i)
            {
                var replacement = group.keyReplacements[i];
                if (replacement == null)
                    continue;

                if (replacement.keyOld == keyOld)
                {
                    // A match, time to break out
                    keyNew = replacement.keyNew;
                    index = i;
                    return true;
                }
            }
            
            // No hits happened
            return false;
        }

        public static string GetUpgradedKey (string type, string keyInput, out bool replaced, bool log = true)
        {
            replaced = false;
            
            // If input key is null or empty or no replacement data is present, we can bail
            if (string.IsNullOrEmpty (keyInput) || data == null || data.keyReplacementGroups == null)
                return keyInput;
            
            // If provided type key is null or can't be found, we can bail
            if (string.IsNullOrEmpty (type) || !data.keyReplacementGroups.ContainsKey (type))
                return keyInput;
            
            // If group is null or has no replacement list, we can bail
            var group = data.keyReplacementGroups[type];
            if (group == null || group.keyReplacements == null)
                return keyInput;

            // If fast lookup doesn't contain this key, iterating on the list will not yield any replacements either, we can safely bail
            if (group.keyLookupFast != null && !group.keyLookupFast.Contains (keyInput))
                return keyInput;
            
            var keyCurrent = keyInput;
            int loops = 0;

            while (IsKeyReplacementAvailable (type, keyCurrent, out string keyNew, out int index))
            {
                if (log)
                    Debug.Log ($"{type} | Upgraded key {keyCurrent} to {keyNew}");

                keyCurrent = keyNew;
                replaced = true;
                loops += 1;

                if (loops > 10)
                {
                    Debug.LogWarning ($"{type} | Rename has too many steps, breaking out, history likely has a loop | Origin: {keyInput}");
                    break;
                }
            }

            return keyCurrent;
        }
        
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void CompressHistory ()
        {
            if (data == null || data.keyReplacementGroups == null)
                return;

            var keysToRemove = new HashSet<string> ();
            
            foreach (var kvp in data.keyReplacementGroups)
            {
                var type = kvp.Key;
                var group = kvp.Value;
                
                if (group == null || group.keyReplacements == null)
                    continue;
                
                keysToRemove.Clear ();

                for (int i = 0, count = group.keyReplacements.Count; i < count; ++i)
                {
                    var replacement = group.keyReplacements[i];
                    var keyOld = replacement.keyOld;
                    var keyNew = replacement.keyNew;
                    
                    if (keysToRemove.Contains (keyOld))
                        continue;
                    
                    int loops = 0;
                    bool keyNewReplaced = false;

                    while (IsKeyReplacementAvailable (type, keyNew, out string keyNewReplacement, out int indexToRemove))
                    {
                        Debug.Log ($"{type} | Encountered chained key replacement {loops} starting with {keyOld}\n{keyNew} -> {keyNewReplacement} (index {indexToRemove})");
                        
                        if (!keysToRemove.Contains (keyNew))
                            keysToRemove.Add (keyNew);
                        
                        keyNew = keyNewReplacement;
                        keyNewReplaced = true;
                        loops += 1;

                        if (loops > 10)
                        {
                            Debug.LogWarning ($"{type} | Rename has too many steps, breaking out, history likely has a loop | Origin: {keyOld}");
                            keyNewReplaced = false;
                            break;
                        }
                    }
                    
                    if (!keyNewReplaced)
                        continue;

                    Debug.Log ($"{type} | Compressed key replacement:\n{keyOld} -> {keyNew}");
                    replacement.keyNew = keyNew;
                }
                
                foreach (var keyToRemove in keysToRemove)
                {
                    for (int i = group.keyReplacements.Count - 1; i >= 0; --i)
                    {
                        var replacement = group.keyReplacements[i];
                        if (replacement.keyOld == keyToRemove)
                        {
                            Debug.LogWarning ($"{type} | Removed key replacement pair {replacement.keyOld} -> {replacement.keyNew}");
                            group.keyReplacements.RemoveAt (i);
                        }
                    }
                }
            }
        }

        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void ReplaceRatingZero ()
        {
            if (data == null || data.keyReplacementGroups == null)
                return;

            var valuesToReplace = new List<(int, string)> ();
            var groupKeys = new List<string> { nameof (DataContainerSubsystem), nameof (DataContainerPartPreset) };

            foreach (var groupKey in groupKeys)
            {
                if (!data.keyReplacementGroups.ContainsKey (groupKey))
                    continue;

                var group = data.keyReplacementGroups[groupKey];
                if (group == null || group.keyReplacements == null)
                    continue;
                
                valuesToReplace.Clear ();
                for (int i = 0, count = group.keyReplacements.Count; i < count; ++i)
                {
                    var r = group.keyReplacements[i];
                    if (r == null)
                        continue;
                    
                    if (r.keyOld.Contains ("vhc"))
                        continue;
                    
                    if (r.keyNew.EndsWith ("_r0"))
                        valuesToReplace.Add ((i, r.keyNew.Replace ("_r0", "_r1")));
                }

                foreach (var valueTuple in valuesToReplace)
                {
                    var r = group.keyReplacements[valueTuple.Item1];
                    Debug.LogWarning ($"Replacing new key at {r.keyOld}: {r.keyNew} -> {valueTuple.Item2}");
                    r.keyNew = valueTuple.Item2;
                }
            }
            
        }
    }
}


