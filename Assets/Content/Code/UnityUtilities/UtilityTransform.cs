using UnityEngine;
using System.Collections.Generic;

public static class UtilityTransform
{
    public static string ToString (Transform t)
    {
        return t != null ? t.name : "null";
    }

    public static string GetTransformPath (Transform t)
    {
        if (t == null)
            return "null";

        string result = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            result = $"{t.name}/{result}";
        }

        return result;
    }

    public static string GetTransformPath (Component t)
    {
        if (t == null)
            return "null";

        return GetTransformPath (t.transform);
    }

    private static List<Transform> childListReused = new List<Transform> ();

    public static List<Transform> FindChildrenDeep (this Transform parent, string name, bool partialMatches, List<Transform> listToUse = null)
    {
        if (listToUse == null)
        {
            listToUse = childListReused;
            listToUse.Clear ();
        }

        if (partialMatches ? parent.name.Contains (name) : string.Equals (parent.name, name))
            listToUse.Add (parent);

        for (int i = 0; i < parent.childCount; ++i)
            parent.GetChild (i).FindChildrenDeep (name, partialMatches, listToUse);

        return listToUse;
    }

    public static Transform FindChildDeep (this Transform parent, string name, bool partialMatches)
    {
        if (partialMatches ? parent.name.Contains (name) : string.Equals (parent.name, name))
            return parent;

        for (int i = 0; i < parent.childCount; ++i)
        {
            var result = parent.GetChild (i).FindChildDeep (name, partialMatches);
            if (result != null)
                return result;
        }

        return null;
    }

    public static string GetNameWithParent (this Transform t)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder ();
        sb.Append (t.parent == null ? "null" : t.parent.name);
        sb.Append ('/');
        sb.Append (t.name);
        return sb.ToString ();
    }

    public static void SetLocalTransformationToZero (this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    public static void MatchTransform (this Transform t, Transform t2)
    {
        if (t2 == null)
            return;

        t.localPosition = t2.localPosition;
        t.localRotation = t2.localRotation;
    }

    public static Quaternion SmoothDamp (Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
    {
        // account for double-cover
        var Dot = Quaternion.Dot (rot, target);
        var Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        var Result = new Vector4
        (
            Mathf.SmoothDamp (rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp (rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp (rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp (rot.w, target.w, ref deriv.w, time)
        ).normalized;
        // compute deriv
        var dtInv = 1f / Time.deltaTime;
        deriv.x = (Result.x - rot.x) * dtInv;
        deriv.y = (Result.y - rot.y) * dtInv;
        deriv.z = (Result.z - rot.z) * dtInv;
        deriv.w = (Result.w - rot.w) * dtInv;
        return new Quaternion (Result.x, Result.y, Result.z, Result.w);
    }

    public static void SetPositionLocalX (this Transform t, float x)
    {
        var p = t.localPosition;
        t.localPosition = new Vector3 (x, p.y, p.z);
    }

    public static void SetPositionLocalY (this Transform t, float y)
    {
        var p = t.localPosition;
        t.localPosition = new Vector3 (p.x, y, p.z);
    }

    public static void SetPositionLocalZ (this Transform t, float z)
    {
        var p = t.localPosition;
        t.localPosition = new Vector3 (p.x, p.y, z);
    }

    public static GameObject CreateChild (this Transform t, string name)
    {
        var go = new GameObject (name);
        go.transform.parent = t;
        go.transform.SetLocalTransformationToZero ();
        go.gameObject.layer = t.gameObject.layer;

        return go;
    }

    public static void DestroyChildren (this Transform t)
    {
        foreach (var child in t)
        {
            Object.Destroy (((Transform)child).gameObject);
        }
    }
}