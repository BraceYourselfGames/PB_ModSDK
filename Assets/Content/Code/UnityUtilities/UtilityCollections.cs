using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public static class UtilityCollections
{
    public static void ClearOrCreate<T> (ref List<T> collection)
    {
        if (collection == null)
            collection = new List<T> ();
        else
            collection.Clear ();
    }
    
    public static void ClearOrCreate<T1,T2> (ref Dictionary<T1,T2> collection)
    {
        if (collection == null)
            collection = new Dictionary<T1,T2> ();
        else
            collection.Clear ();
    }
    
    public static void ClearOrCreate<T1,T2> (ref SortedDictionary<T1,T2> collection)
    {
        if (collection == null)
            collection = new SortedDictionary<T1,T2> ();
        else
            collection.Clear ();
    }
    
    public static void ClearOrCreate<T> (ref HashSet<T> collection)
    {
        if (collection == null)
            collection = new HashSet<T> ();
        else
            collection.Clear ();
    }
    
    public static void SortByInput<T> (this List<T> list, Func<T, float> inputFunction)
    {
        if (list == null || list.Count <= 1 || inputFunction == null)
            return;
            
        list.Sort ((x, y) => inputFunction (x).CompareTo (inputFunction (y)));
    }
    
    public static T GetLastEntry<T> (this List<T> list) where T : class, new ()
    {
        if (list == null)
            return null;

        var c = list.Count;
        return list[c - 1];
    }
    
    public static T GetFirstOrLastEntry<T> (this List<T> list, bool last) where T : class, new ()
    {
        if (list == null)
            return null;

        var c = list.Count;
        if (c == 1 || !last)
            return list[0];
        return list[c - 1];
    }
    
    public static TKey GetRandomKey<TKey, TValue> (this IDictionary<TKey, TValue> dict)
    {
        if (dict == null || dict.Count == 0)
            return default;
        return dict.ElementAt (Random.Range (0, dict.Count)).Key;
    }

    private static List<string> keysFiltered = new List<string> ();
    
    public static string GetRandomKeyChecked<T> (this IDictionary<string, T> dict, Func<T, bool> valueCheck)
    {
        if (dict == null || dict.Count == 0 || valueCheck == null)
            return default;
        
        keysFiltered.Clear ();
        foreach (var kvp in dict)
        {
            if (valueCheck.Invoke (kvp.Value))
                keysFiltered.Add (kvp.Key);
        }

        if (keysFiltered.Count == 0)
            return default;

        return keysFiltered.GetRandomEntry ();
    }

    public static TValue GetRandomValue<TKey, TValue> (this IDictionary<TKey, TValue> dict)
    {
        return dict.ElementAt (Random.Range (0, dict.Count)).Value;
    }
    
    public static T GetRandomEntry <T> (this HashSet<T> set)
    {
        if (set == null || set.Count == 0)
            return default;
        return set.ElementAt (Random.Range (0, set.Count));
    }
    
    public static T GetRandomEntry <T> (this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default;
        if (list.Count == 1)
            return list[0];
        return list[Random.Range (0, list.Count)];
    }
    
    public static T GetAndRemoveRandomEntry <T> (this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default;
        
        var index = Random.Range (0, list.Count);
        var entry = list[index];
        list.RemoveAt (index);
        return entry;
    }
    
    public static void RemoveRandomEntry <T> (this List<T> list)
    {
        if (list == null || list.Count == 0)
            return;

        var index = Random.Range (0, list.Count);
        list.RemoveAt (index);
    }

    private static readonly List<(float weight, object element)> weightCache = new List<(float weight, object element)> ();
    public static T GetRandomWeighted <T> (this Dictionary<T, float> weights)
    {
        if (weights == null || weights.Count == 0)
            return default;

        weightCache.Clear ();
        
        var totalWeight = 0.0f;
        foreach (var element in weights)
        {
            weightCache.Add ((element.Value, element.Key));
            totalWeight += element.Value;
        }
        
        weightCache.Shuffle ();

        var selected = Random.Range (0.0f, totalWeight);
        foreach (var element in weightCache)
        {
            selected -= element.weight;
            if (selected <= 0.0f)
            {
                return (T)element.element;
            }
        }

        return default;
    }
    
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    
    public static void ProportionalBinaryAction<T> (IList<T> list, float proportion, Action<T, bool> action)
    {
        if (list == null || list.Count == 0 || proportion <= 0f || proportion >= 1f)
            return;

        float fractionalIndex = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            fractionalIndex += proportion;
            bool fractionReached = fractionalIndex >= 1f;
                
            if (fractionReached)
                fractionalIndex -= 1f;
                
            var element = list[i];
            action.Invoke (element, fractionReached);
        }
    }
    
    public static void Sort<T> (ref HashSet<T> set)
    {
        if (set == null || set.Count == 0)
            return;
        
        var list = set.ToList ();
        list.Sort ();
        set = new HashSet<T> (list);
    }

    public interface IWeightedCollectionEntry
    {
        float GetWeight ();
    }
    
    public static T RandomElementByWeight<T> (this IEnumerable<T> sequence) where T : class, IWeightedCollectionEntry
    {
        if (sequence == null || sequence.Count() == 0)
            return null;
        
        float weightTotal = 0f;
        foreach (var entry in sequence)
            weightTotal += entry.GetWeight ();

        float weightRandom = Random.Range (0f, 1f) * weightTotal;
        float weightCurrent = 0f;

        foreach (var entry in sequence)
        {
            weightCurrent += entry.GetWeight ();
            if (weightCurrent >= weightRandom)
                return entry;
        }

        return null;
    }
    
    public static T RandomElementByWeight<T> (this List<T> sequence, List<float> weights) where T : class, IWeightedCollectionEntry
    {
        if (sequence == null || sequence.Count == 0 || weights == null || weights.Count == 0 || sequence.Count != weights.Count)
        {
            Debug.LogWarning ($"Failed to select random element by weight using external weight list");
            return null;
        }
        
        float weightTotal = 0f;
        foreach (var weight in weights)
            weightTotal += weight;

        float weightRandom = Random.Range (0f, 1f) * weightTotal;
        float weightCurrent = 0f;

        for (int i = 0; i < sequence.Count; ++i)
        {
            var entry = sequence[i];
            var weight = weights[i];
            
            weightCurrent += weight;
            if (weightCurrent >= weightRandom)
                return entry;
        }

        return null;
    }
}

public static class ThreadSafeRandom
{
    [ThreadStatic] 
    private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get
        {
            return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
        }
    }
}
