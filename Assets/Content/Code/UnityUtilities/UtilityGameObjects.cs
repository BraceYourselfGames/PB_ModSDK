using UnityEngine;
using System.Collections.Generic;

public static class DebugExtensions
{
    public static void DrawCube (Vector3 origin, Vector3 halfExtents, Color color = default, float time = 0f)
    {
        DrawCube (origin, Vector3.forward, Vector3.up, Vector3.right, halfExtents, color, time);
    }

    public static void DrawCube (Vector3 origin, Vector3 dirForward, Vector3 dirUp, Vector3 dirRight, Vector3 halfExtents, Color color = default, float time = 0f)
    {
        var offsetRight = dirRight * halfExtents.x;
        var offsetUp = dirUp * halfExtents.y;
        var offsetForward = dirForward * halfExtents.z;

        var c1 = origin - offsetRight - offsetUp - offsetForward;
        var c2 = origin - offsetRight + offsetUp - offsetForward;
        var c3 = origin + offsetRight - offsetUp - offsetForward;
        var c4 = origin + offsetRight + offsetUp - offsetForward;

        var c5 = origin - offsetRight - offsetUp + offsetForward;
        var c6 = origin - offsetRight + offsetUp + offsetForward;
        var c7 = origin + offsetRight - offsetUp + offsetForward;
        var c8 = origin + offsetRight + offsetUp + offsetForward;

        if (color == default)
            color = Color.white;

        Debug.DrawLine (c1, c2, color, time);
        Debug.DrawLine (c3, c4, color, time);
        Debug.DrawLine (c5, c6, color, time);
        Debug.DrawLine (c7, c8, color, time);

        Debug.DrawLine (c1, c5, color, time);
        Debug.DrawLine (c2, c6, color, time);
        Debug.DrawLine (c3, c7, color, time);
        Debug.DrawLine (c4, c8, color, time);

        Debug.DrawLine (c1, c3, color, time);
        Debug.DrawLine (c2, c4, color, time);
        Debug.DrawLine (c5, c7, color, time);
        Debug.DrawLine (c6, c8, color, time);

        Debug.DrawLine (origin - dirForward, origin + dirForward, color, time);
        Debug.DrawLine (origin - dirRight, origin + dirRight, color, time);
    }
}

public static class UtilityGameObjects
{
    public static bool RaycastInContext (GameObject holder, Vector3 start, Vector3 direction, out RaycastHit hit, float distance, LayerMask mask)
    {
        if (Application.isPlaying)
        {
            return Physics.Raycast (start, direction, out hit, 400f, mask);
        }
        else
        {
            var physicsScene = holder.scene.GetPhysicsScene ();
            return physicsScene.Raycast (start, direction, out hit, 400f, mask);
        }
    }

    public static Transform GetTransformSafely (ref Transform t, string name, HideFlags hideFlags, Vector3 scenePosition, string tag = null)
    {
        if (t == null)
        {
            GameObject tgo = GameObject.Find (name);
            if (tgo == null)
            {
                tgo = new GameObject ();
                tgo.name = name;
                tgo.hideFlags = hideFlags;
                tgo.transform.position = scenePosition;

                if (!string.IsNullOrEmpty (tag))
                    tgo.tag = tag;
            }
            t = tgo.transform;
        }
        return t;
    }

    public static List<T> GetComponentsInChildrenImmediate<T> (this Transform transform) where T : Component
    {
        if (transform == null)
            return null;

        var childCount = transform.childCount;
        var results = new List<T> ();

        for (int i = 0; i < childCount; ++i)
        {
            var child = transform.GetChild (i);
            var components = child.GetComponents<T> ();
            if (components != null && components.Length > 0)
                results.AddRange (components);
        }

        return results;
    }

    public static void CheckEmptyObjectExistence (ref GameObject gameObject, string name, Vector3 position)
    {
        if (gameObject == null)
        {
            GameObject[] topLevelObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().GetRootGameObjects ();
            for (int i = 0; i < topLevelObjects.Length; ++i)
            {
                GameObject topLevelObject = topLevelObjects[i];
                if (string.Equals (name, topLevelObject.name))
                {
                    Debug.Log ("UGO | CheckEmptyObjectExistence | Found the object " + name);
                    gameObject = topLevelObject;
                    break;
                }
            }
        }
        if (gameObject == null)
        {
            Debug.Log ("UGO | CheckEmptyObjectExistence | Creating the object " + name);
            gameObject = new GameObject (name);
            gameObject.transform.localPosition = position;
        }
    }

    public static void ClearChildren (GameObject parent, bool forceImmediate = false)
    {
        ClearChildren (parent.transform, forceImmediate);
    }

    public static void ClearChildren (Transform parent, bool forceImmediate = false)
    {
        if (parent == null)
            return;

        bool parentWasHidden = parent.gameObject.activeSelf == false;
        if (parentWasHidden) parent.gameObject.SetActive (true);

        for (int i = parent.childCount - 1; i >= 0; --i)
        {
            if (!Application.isPlaying || forceImmediate)
                GameObject.DestroyImmediate (parent.GetChild (i).gameObject);
            else
                GameObject.Destroy (parent.GetChild (i).gameObject);
        }

        if (parentWasHidden)
            parent.gameObject.SetActive (false);
    }

    public static T AddChildWithComponent<T> (this GameObject parent, string name = null) where T : Component
    {
        var go = new GameObject();
        if (parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }

        if (!string.IsNullOrEmpty (name))
            go.name = name;
        else
            go.name = typeof (T).Name;

        var comp = go.AddComponent<T> ();
        return comp;
    }

    public static void SetFlags (this GameObject parent, HideFlags hideFlags)
    {
        SetFlags (parent.transform, hideFlags);
    }

    public static void SetFlags (this Transform parent, HideFlags hideFlags)
    {
        parent.gameObject.hideFlags = hideFlags;
        for (int i = 0; i < parent.childCount; ++i)
            SetFlags (parent.GetChild (i), hideFlags);
    }

    public static void DestroyComponent (Component component)
    {
        DestroyComponent (component, false);
    }

	public static void DestroyComponent (Component component, bool withGameObject)
	{
		var o = component;
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.delayCall += () =>
        {
            if (o)
            {
                if (withGameObject)
                    GameObject.DestroyImmediate (o.gameObject);
                else
                    GameObject.DestroyImmediate (o);
            }
        };
		#else
		if (withGameObject)
            GameObject.Destroy (o.gameObject);
        else
            GameObject.Destroy (o);
		#endif
	}

    public static void DestroyAllComponents (GameObject gameObject, List<Component> components)
    {
        if (components.Count > 0)
        {
            for (int i = components.Count - 1; i >= 0; --i)
            {
                Component component = components[i];
                components.Remove (component);
                DestroyComponent (component, false);
            }
        }
    }

    #if PB_MODSDK
    public static T AddChild<T> (this GameObject parent) where T : Component
    {
        var go = new GameObject();
        #if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
        #endif
        if (parent != null)
        {
            var t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }

        var name = typeof(T).ToString();
        if (name.StartsWith("UI"))
            name = name.Substring(2);
        else if (name.StartsWith("UnityEngine."))
            name = name.Substring(12);

        go.name = name;
        return go.AddComponent<T>();
    }
    #endif
}
