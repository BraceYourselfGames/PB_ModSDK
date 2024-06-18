using System.Collections.Generic;

using UnityEngine;
using Unity.Entities;

public struct StringHandle : IComponentData
{
    public uint Value;
}

public static class StringProvider
{
    private static readonly Dictionary<uint, string> storage = new Dictionary<uint, string> ();
    private static uint nextHandle = 0;
    private static readonly string fallback = "StringNotFound";




    private static uint GetNextHandle ()
    {
        // This is not exactly safe, there might be a better solution
        while (storage.ContainsKey (++nextHandle))
        {
        }

        return nextHandle;
    }

    public static uint Allocate (string text)
    {
        var handle = GetNextHandle ();
        storage.Add (handle, text);
        return handle;
    }

    public static uint AllocateAndGetHandle (string text, out uint handle)
    {
        handle = GetNextHandle ();
        storage.Add (handle, text);
        return handle;
    }

    public static string Get (uint handle)
    {
        if (!storage.TryGetValue (handle, out var value))
        {
            Debug.LogWarning ("StringProvider does not contain the handle " + handle);
            return fallback;
        }

        return value;
    }

    public static string GetFromEntity (Entity entity, EntityManager entityManager)
    {
        if (!entityManager.HasComponent<StringHandle> (entity))
        {
            Debug.LogWarning ("Failed to get a string, provided entity has no handle component");
            return fallback;
        }

        var component = entityManager.GetComponentData<StringHandle> (entity);
        var text = Get (component.Value);
        return text;
    }

    public static void Set (uint handle, string value)
    {
        if (!storage.ContainsKey (handle))
        {
            Debug.LogWarning ("StringProvider does not contain the handle " + handle);
            return;
        }

        storage[handle] = value;
    }

    public static void SetOnEntity (Entity entity, EntityManager entityManager, string text)
    {
        if (!entityManager.HasComponent<StringHandle> (entity))
        {
            var component = new StringHandle { Value = Allocate (text) };
            entityManager.AddComponentData (entity, component);
        }
        else
        {
            var component = entityManager.GetComponentData<StringHandle> (entity);
            var s = Get (component.Value);
            s = text;
            Set (component.Value, s);
        }
    }

    public static void Remove (uint handle)
    {
        storage.Remove (handle);
    }

    public static void RemoveFromEntity (Entity entity, EntityManager entityManager)
    {
        if (!entityManager.HasComponent<StringHandle> (entity))
        {
            Debug.LogWarning ("Failed to remove a string, provided entity has no handle component");
            return;
        }

        var component = entityManager.GetComponentData<StringHandle> (entity);
        Remove (component.Value);
        entityManager.RemoveComponent<StringHandle> (entity);
    }
}