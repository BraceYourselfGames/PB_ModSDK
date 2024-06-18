using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using PhantomBrigade;

//using Random = Unity.Mathematics.Random;

public static class Utilities
{
    #region BitHelpers
    public static int ShiftAndWrap (int value, int positions)
    {
        positions = positions & 0x1F;

        // Save the existing bit pattern, but interpret it as an unsigned integer.
        uint number = BitConverter.ToUInt32 (BitConverter.GetBytes (value), 0);
        // Preserve the bits to be discarded.
        uint wrapped = number >> (32 - positions);
        // Shift and wrap the discarded bits.
        return BitConverter.ToInt32 (BitConverter.GetBytes ((number << positions) | wrapped), 0);
    }
    
    public static long MakeLong(int left, int right) {
        //implicit conversion of left to a long
        long res = left;

        //shift the bits creating an empty space on the right
        // ex: 0x0000CFFF becomes 0xCFFF0000
        res = (res << 32);

        //combine the bits on the right with the previous value
        // ex: 0xCFFF0000 | 0x0000ABCD becomes 0xCFFFABCD
        res = res | (long)(uint)right; //uint first to prevent loss of signed bit

        //return the combined result
        return res;
    }

    #endregion

    #region Helpers

    public static bool IndexIsRetrievable<T> (List<T> list, int index)
    {
        if (list == null || index < 0 || index > list.Count - 1)
            return false;
        else
            return true;
    }

    public static T RandomChoice<T>(T choiceOne, T choiceTwo)
    {
        return UnityEngine.Random.Range (0f, 1f) > 0.5f ? choiceOne : choiceTwo;
    }

    #endregion

    #region Quaternions / Rotations

    public enum RotationAxis
    {
        X,
        Y,
        Z,
    }

    public static Vector3 FacingRotationEuler(Transform rotator, Vector3 worldPoint, RotationAxis axis)
    {
        Vector3 localPoint = rotator.InverseTransformPoint(worldPoint);

        Vector3 eulerAngle = Vector3.zero;

        switch (axis)
        {
            case RotationAxis.X:
                eulerAngle.x = Mathf.Atan2(localPoint.y, localPoint.z) * Mathf.Rad2Deg;
                break;
            case RotationAxis.Y:
                eulerAngle.y = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
                break;
            case RotationAxis.Z:
                eulerAngle.z = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
                break;
        }

        return eulerAngle;
    }

    public static Quaternion FacingRotation(Transform rotator, Vector3 worldPoint, RotationAxis axis)
    {
        return Quaternion.Euler(FacingRotationEuler(rotator, worldPoint, axis));
    }

    #endregion


    #region Vectors
    //public static Vector2I WorldToGrid(Vector3 position)
    //{
    //    Vector2I gridPosition = new Vector2I();

    //    gridPosition.x = Mathf.Clamp(Mathf.RoundToInt(position.x), 0, BattleMap.instance.sizeX - 1);
    //    gridPosition.y = Mathf.Clamp(Mathf.RoundToInt(position.z), 0, BattleMap.instance.sizeY - 1);

    //    return gridPosition;
    //}

    //public static Vector3 GridToWorld (Vector2I gridPosition)
    //{
    //    Vector3 worldPosition = Vector3.zero;
    //    worldPosition.x = (float)gridPosition.x;
    //    worldPosition.z = (float)gridPosition.y;
    //    worldPosition.y = 0;

    //    return worldPosition;
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SquareDistance(Vector3 a, Vector3 b)
    {
        float x = a.x - b.x;
        x = x > 0 ? x : -x;
        float y = a.y - b.y;
        y = y > 0 ? y : -y;
        float z = a.z - b.z;
        z = z > 0 ? z : -z;
        return x * x + y * y + z * z;

    }

    public static Vector3 VectorFloor(Vector3 vector)
    {
        return new Vector3(Mathf.Floor(vector.x), Mathf.Floor(vector.y), Mathf.Floor(vector.z));
    }

    public static Vector3 VectorCiel(Vector3 vector)
    {
        return new Vector3(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y), Mathf.Ceil(vector.z));
    }

    public static Vector3 ClampPointToGrid(Vector3 point)
    {
        return new Vector3((Mathf.RoundToInt(point.x) / Constants.gridSize) * Constants.gridSize,
            (Mathf.RoundToInt(point.y) / Constants.gridSize) * Constants.gridSize,
            (Mathf.RoundToInt(point.z) / Constants.gridSize) * Constants.gridSize);
    }

    public static Vector3 ClampHeightToGrid(Vector3 point)
    {
        return new Vector3(point.x,
            (int)(point.y / Constants.gridSize) * Constants.gridSize,
            point.z);
    }

    public static Vector3 GetDirection(Vector3 from, Vector3 to)
    {
        Vector3 heading = (to - from);

        return heading.normalized;
    }

    public static Vector3 Flatten(Vector3 source)
    {
        return new Vector3(source.x, 0, source.z);
    }
    
    /// <summary>
    /// Returns a deflected vector that is aligned to the source vector. It will be at maximum X degrees deflected
    /// </summary>
    /// <returns></returns>
    public static Vector3 RandomAlignedVector (Vector3 source, float deflection)
    {
        Vector3 deflectionAngle = new Vector3 
        (
            UnityEngine.Random.value * deflection, 
            UnityEngine.Random.value * deflection, 
            UnityEngine.Random.value * deflection
        );

        return Quaternion.Euler (deflectionAngle) * source;
    }

    public static int TileDistance (Vector3 startingPoint, Vector3 endingPoint)
    {
        return Mathf.RoundToInt (Vector3.Distance (startingPoint, endingPoint)) / 3;
    }
    
    public static RaycastHit[] GetAllUnitsInFiringLine (Vector3 firingPoint, Vector3 targetPoint)
    {
        Ray ray = new Ray (firingPoint, Vector3.Normalize (targetPoint - firingPoint));

        return Physics.RaycastAll (ray, 1000f, LayerMasks.unitMask);
    }
    
    #endregion

    #region Transform

    /// <summary>
    /// Given a specific transform, search all children to find the correctly named object, returns null if nothing is found
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Transform FindTransformRecursive (Transform transform, string name, bool strict = true)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild (i);

            if (child.name == name)
            {
                return child;
            }
            else
            {
                if (!strict && child.name.Contains (name))
                    return child;

                Transform next = FindTransformRecursive (child, name, strict);
                if (next)
                    return next;
            }
        }

        return null;
    }


    /// <summary>
    /// Given a specific gameobject, search all children till you find the correctly named object, then locate the specified component on it
    /// Will be null if no object is found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static Component FindComponentRecursive<T> (Transform transform, T component, string name) where T : Component
    {
        Transform located = FindTransformRecursive (transform, name);

        if (located)
        {
            return located.GetComponent (typeof (T));
        }
        else
        {
            Debug.LogWarning ("Could not locate transform named " + name);
            return null;
        }
    }

    /// <summary>
    /// Given a specific transform, it will attempt to locate all components on the named object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <param name="component"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static List<T> FindComponentsRecursive<T> (Transform transform, string name, bool strict = true) where T : Component
    {
        List<T> foundComponents = new List<T> ();
        List<Transform> foundTransforms = FindTransformsRecursive (transform, name, strict);

        for (int i = 0; i < foundTransforms.Count; ++i)
        {
            T found = foundTransforms[i].GetComponent<T> ();

            if (found != null)
            {
                foundComponents.Add (found);
            }
        }

        return foundComponents;
    }

    /// <summary>
    /// Finds a transform recursively, uses a target transform and a name to match
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static List<Transform> FindTransformsRecursive (Transform transform, string name, bool strict = true)
    {
        List<Transform> foundTransforms = new List<Transform> ();

        AddMatchingTransforms (foundTransforms, transform, name, strict);

        return foundTransforms;
    }

    private static void AddMatchingTransforms (List<Transform> foundTransforms, Transform transform, string name, bool strict)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild (i);
            if (child.name == name)
            {
                foundTransforms.Add (child);
            }
            else
            {
                if (!strict && child.name.Contains (name))
                    foundTransforms.Add (child);
            }

            AddMatchingTransforms (foundTransforms, child, name, strict);
        }
    }

    public static T GetComponentInChildOrParent<T>(Transform target)
    {
        var component = target.GetComponent<T> ();

        if (component != null) return component;

        component = target.GetComponentInChildren<T> ();

        if (component != null) return component;

        component = target.GetComponentInParent<T> ();

        return component;
    }
    
    
    #endregion

    #region Databasing

    public static bool logDatabasing = false;

    public static Dictionary<T, string> MapEnumToDictionary<T> (Dictionary<T, string> dictionary) where T : struct, IConvertible
    {
        if (dictionary == null)
            dictionary = new Dictionary<T, string> ();

        foreach (T enumName in Enum.GetValues (typeof (T)))
        {
            if (!dictionary.ContainsKey (enumName))
                dictionary.Add (enumName, Enum.GetName (typeof (T), enumName));
        }

        return dictionary;
    }

    #endregion

    #region GameObjectHelpers
    private static int _debrisLayer = -99;
    public static int debrisLayer
    {
        get
        {
            if (_debrisLayer == -99)
            {
                _debrisLayer = LayerMask.NameToLayer ("Debris");
            }
            return _debrisLayer;
        }
    }

    public static GameObject GetInstanceFromPrefabPath (string path, Transform parent = null)
    {
        GameObject prefab = Resources.Load<GameObject> (path);
        if (prefab != null)
        {
            GameObject instance = GameObject.Instantiate (prefab);
            instance.transform.parent = parent;
            return instance;
        }
        else
        {
            Debug.LogError ("GetInstanceFromPrefabPath | Prefab not found at path: " + path);
            return null;
        }
    }
    #endregion

    #region File Tools

    public static void CopyDirectory (string sourceDirectory, string targetDirectory)
    {
        DirectoryInfo source = new DirectoryInfo (sourceDirectory);
        DirectoryInfo target = new DirectoryInfo (targetDirectory);

        CopyDirectoryRecursive (source, target);
    }

    public static void CopyDirectoryRecursive (DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory (target.FullName);

        // Copy each file into the new directory.
        foreach (FileInfo fi in source.GetFiles ())
        {
            Console.WriteLine (@"Copying {0}\{1}", target.FullName, fi.Name);
            fi.CopyTo (Path.Combine (target.FullName, fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories ())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory (diSourceSubDir.Name);
            CopyDirectoryRecursive (diSourceSubDir, nextTargetSubDir);
        }
    }


    #endregion

    /// <summary>
    /// Used to bail out in ExecuteInEditMode components which experience inappropriate event calls before play mode starts properly
    /// </summary>
    public static bool isPlaymodeChanging
    {
        get
        {
            #if UNITY_EDITOR
            return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying;
            #else
            return false;
            #endif
        }
    }
}