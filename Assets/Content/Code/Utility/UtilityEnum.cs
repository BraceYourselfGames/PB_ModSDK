using System;
using UnityEngine;

public static class UtilityEnum
{
    public static T ParseEnum<T> (string value) where T : struct, IComparable
    {
        bool success = Enum.TryParse<T> (value, true, out var result);
        if (!success)
        {
            Debug.LogWarning ($"Failed to parse string {value} as enum of type {typeof(T).Name}, returning default value of {default(T)}");
            return default(T);
        }
        else
        {
            return result;
        }
    }
}
