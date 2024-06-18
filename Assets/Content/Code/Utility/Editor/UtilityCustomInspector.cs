using System;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEditor;

public static class UtilityCustomInspector
{
    private static List<EditorWindow> windows;

    public static void RepaintAllWindows ()
    {
        if (windows == null)
        {
            windows = new List<EditorWindow> ();
            var inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            var windowObjects = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            foreach (var obj in windowObjects)
            {
                var window = obj as EditorWindow;
                windows.Add (window);
            }
        }

        foreach (var window in windows)
            window.Repaint();
    }

    public static bool DrawFoldout (string name, bool source, GUILayoutOption[] layoutOptions = null)
    {
        var result = source;
        EditorGUILayout.BeginHorizontal ();
        if (layoutOptions == null)
        {
            GUILayout.FlexibleSpace ();
        }
        if (GUILayout.Button (string.Format ("{0} {1}", name, result ? "▲" : "▼"), SirenixGUIStyles.RightAlignedWhiteMiniLabel, layoutOptions))
        {
            result = !result;
        }
        EditorGUILayout.EndHorizontal ();
        return result;
    }

    public static Vector3Int DrawVector3Int (string label, Vector3Int target)
    {
        EditorGUILayout.BeginHorizontal ();
        if (!string.IsNullOrEmpty (label))
            GUILayout.Label (label, GUILayout.Width (EditorGUIUtility.labelWidth));
        int x = EditorGUILayout.IntField (target.x, GUILayout.MaxWidth (80f));
        int y = EditorGUILayout.IntField (target.y, GUILayout.MaxWidth (80f));
        int z = EditorGUILayout.IntField (target.z, GUILayout.MaxWidth (80f));
        EditorGUILayout.EndHorizontal ();
        if (x != target.x || y != target.y || z != target.z)
            return new Vector3Int (x, y, z);
        else
            return target;
    }

    public static void DrawDictionary<T1, T2> (IDictionary<T1, T2> dictionary, Action<T1, T2> onElement, Action onEnd, bool boxElement, bool allowRemoval)
    {
        DrawDictionary (string.Empty, dictionary, onElement, onEnd, boxElement, allowRemoval);
    }

    public static void DrawDictionary<T1, T2> (string description, IDictionary<T1, T2> dictionary, Action<T1, T2> onElement, Action onEnd, bool boxElement, bool allowRemoval)
    {
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label (description);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("Entries: " + dictionary.Count);
        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginVertical ("Box");
        if (dictionary != null)
        {
            bool keyToRemoveSelected = false;
            T1 keyToRemove = default (T1);
            foreach (KeyValuePair<T1, T2> kvp in dictionary)
            {
                if (boxElement)
                    EditorGUILayout.BeginHorizontal ("Box");
                else
                    EditorGUILayout.BeginHorizontal ();

                if (onElement != null)
                {
                    EditorGUILayout.BeginVertical ();
                    onElement.Invoke (kvp.Key, kvp.Value);
                    EditorGUILayout.EndVertical ();
                }

                if (allowRemoval)
                {
                    if (GUILayout.Button ("x", GUILayout.Width (20f)))
                    {
                        keyToRemoveSelected = true;
                        keyToRemove = kvp.Key;
                    }
                }

                if (boxElement)
                    EditorGUILayout.EndHorizontal ();
                else
                    EditorGUILayout.EndHorizontal ();
            }

            if (allowRemoval)
            {
                if (keyToRemoveSelected)
                    dictionary.Remove (keyToRemove);
            }

            if (onEnd != null)
                onEnd.Invoke ();
        }
        EditorGUILayout.EndVertical ();
    }

    public static void DrawList<T> (List<T> list, Action<T> onElement, Action onAdd, bool boxElement, bool allowResizing, Color? backgroundColor = null, bool allowShifting = false)
    {
        DrawList (string.Empty, list, onElement, onAdd, boxElement, allowResizing, backgroundColor: backgroundColor, allowShifting: allowShifting);
    }

    public static void DrawList<T> (string description, List<T> list, Action<T> onElement, Action onAdd, bool boxElement, bool allowResizing, Color? backgroundColor = null, bool allowShifting = false)
    {
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label (description);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("Entries: " + list.Count);
        EditorGUILayout.EndHorizontal ();

        if (backgroundColor != null)
        {
            Color backgroundColorCached = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor ?? GUI.backgroundColor;
            EditorGUILayout.BeginVertical ("Box");
            GUI.backgroundColor = backgroundColorCached;
        }
        else
        {
            EditorGUILayout.BeginVertical ();
        }

        if (list != null && list.Count != 0)
        {
            int indexToRemove = -1;
            int indexToShiftFurther = -1;
            int indexToShiftEarlier = -1;
            int indexToShiftCustom = -1;
            int indexToShiftCustomTarget = -1;

            for (int i = 0; i < list.Count; ++i)
            {
                if (boxElement)
                    EditorGUILayout.BeginHorizontal ("Box");
                else
                    EditorGUILayout.BeginHorizontal ();

                EditorGUILayout.BeginHorizontal (GUILayout.Width (20f));
                if (allowResizing)
                {
                    if (GUILayout.Button ("×", GUILayout.Width (20f)))
                        indexToRemove = i;
                }

                if (allowShifting)
                {
                    if (i > 0)
                    {
                        if (GUILayout.Button ("▲", GUILayout.Width (20f)))
                            indexToShiftEarlier = i;
                    }
                    else
                    {
                        GUILayout.Space (26f);
                    }
                    if (i < list.Count - 1)
                    {
                        if (GUILayout.Button ("▼", GUILayout.Width (20f)))
                            indexToShiftFurther = i;
                    }
                    else
                    {
                        GUILayout.Space (23f);
                    }

                    EditorGUI.BeginChangeCheck ();
                    int index = i;
                    index = EditorGUILayout.IntField (index, GUILayout.Width (20f));
                    if (EditorGUI.EndChangeCheck ())
                    {
                        indexToShiftCustom = i;
                        indexToShiftCustomTarget = index;
                        GUI.FocusControl (null);
                    }
                }
                EditorGUILayout.EndHorizontal ();

                T element = list[i];
                if (element != null && onElement != null)
                {
                    EditorGUILayout.BeginVertical ();
                    onElement.Invoke (element);
                    EditorGUILayout.EndVertical ();
                }

                if (boxElement)
                    EditorGUILayout.EndHorizontal ();
                else
                    EditorGUILayout.EndHorizontal ();
            }

            if (allowResizing)
            {
                if (indexToRemove != -1)
                    list.RemoveAt (indexToRemove);

                EditorGUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button ("Add", GUILayout.Width (50f)) && onAdd != null)
                    onAdd.Invoke ();
                EditorGUILayout.EndHorizontal ();
            }

            if (allowShifting)
            {
                if (indexToShiftFurther != -1)
                {
                    T elementFurther = list[indexToShiftFurther + 1];
                    list[indexToShiftFurther + 1] = list[indexToShiftFurther];
                    list[indexToShiftFurther] = elementFurther;
                }

                if (indexToShiftEarlier != -1)
                {
                    T elementEarlier = list[indexToShiftEarlier - 1];
                    list[indexToShiftEarlier - 1] = list[indexToShiftEarlier];
                    list[indexToShiftEarlier] = elementEarlier;
                }

                if (indexToShiftCustom != -1 && indexToShiftCustomTarget != -1 && indexToShiftCustom != indexToShiftCustomTarget && indexToShiftCustomTarget.IsValidIndex (list))
                {
                    T element = list[indexToShiftCustom];
                    list.RemoveAt (indexToShiftCustom);
                    list.Insert (indexToShiftCustomTarget, element);
                }
            }
        }
        else
        {
            if (allowResizing)
            {
                if (list == null)
                    list = new List<T> ();

                EditorGUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button ("Add", GUILayout.Width (50f)) && onAdd != null)
                    onAdd.Invoke ();
                EditorGUILayout.EndHorizontal ();
            }
        }
        EditorGUILayout.EndVertical ();
    }

    public static void DrawListGeneric<T> (string description, List<T> list, Action<T> onElement, Action<List<T>> onAdd, bool boxElement, bool allowResizing, Color? backgroundColor = null, bool insertSeparator = false)
    {
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label (description);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("Entries: " + list.Count);
        EditorGUILayout.EndHorizontal ();

        if (backgroundColor != null)
        {
            Color backgroundColorCached = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor ?? GUI.backgroundColor;
            EditorGUILayout.BeginVertical ("Box");
            GUI.backgroundColor = backgroundColorCached;
        }
        else
        {
            EditorGUILayout.BeginVertical ("Box");
        }

        if (list != null && list.Count != 0)
        {
            int indexToRemove = -1;
            for (int i = 0; i < list.Count; ++i)
            {
                if (insertSeparator)
                {
                    GUILayout.Space (4f);
                    GUILayout.Label ("____________________", EditorStyles.miniLabel);
                    GUILayout.Space (8f);
                }

                if (boxElement)
                    EditorGUILayout.BeginHorizontal ("Box");
                else
                    EditorGUILayout.BeginHorizontal ();

                T element = list[i];
                if (element != null && onElement != null)
                {
                    EditorGUILayout.BeginVertical ();
                    onElement.Invoke (element);
                    EditorGUILayout.EndVertical ();
                }

                if (allowResizing)
                {
                    if (GUILayout.Button ("x", GUILayout.Width (20f)))
                        indexToRemove = i;
                }

                if (boxElement)
                    EditorGUILayout.EndHorizontal ();
                else
                    EditorGUILayout.EndHorizontal ();
            }

            if (allowResizing)
            {
                if (indexToRemove != -1)
                    list.RemoveAt (indexToRemove);

                EditorGUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button ("Add", GUILayout.Width (50f)) && onAdd != null)
                    onAdd.Invoke (list);
                EditorGUILayout.EndHorizontal ();
            }
        }
        else
        {
            if (list == null) list = new List<T> ();

            EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Add", GUILayout.Width (50f)) && onAdd != null)
                onAdd.Invoke (list);
            EditorGUILayout.EndHorizontal ();
        }
        EditorGUILayout.EndVertical ();
    }

    public static void DrawListString (string description, List<string> list)
    {
        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label (description);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("Entries: " + list.Count);
        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginVertical ("Box");
        if (list != null)
        {
            int indexToRemove = -1;
            for (int i = 0; i < list.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal ();

                if (list[i] != null)
                    list[i] = EditorGUILayout.TextField (list[i]);

                if (GUILayout.Button ("x", GUILayout.Width (20f)))
                    indexToRemove = i;

                EditorGUILayout.EndHorizontal ();
            }

            if (indexToRemove != -1)
                list.RemoveAt (indexToRemove);

            EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            if (GUILayout.Button ("Add", GUILayout.Width (50f)))
                list.Add (string.Empty);
            EditorGUILayout.EndHorizontal ();
        }
        EditorGUILayout.EndVertical ();
    }

    public static float fieldLabelWidth = 100f;
    public static float fieldContentMaxWidth = 1000f;

    public static void DrawField (string name, Action onDraw, bool boldLabel = false)
    {
        EditorGUILayout.BeginHorizontal ();
        if (boldLabel)
        {
            GUILayout.Label (name, EditorStyles.boldLabel);
        }
        else
        {
            GUILayout.Label (name, EditorStyles.label);
        }
        GUILayout.Space(5f);
        if (onDraw != null)
            onDraw ();
        EditorGUILayout.EndHorizontal ();
    }

    public static void DrawField<T> (string name, T target, Action<T> onDraw, bool boldLabel = false) where T : Component
    {
        EditorGUILayout.BeginHorizontal ();
        if (boldLabel)
        {
            GUILayout.Label (name, EditorStyles.boldLabel);
        }
        else
        {
            GUILayout.Label (name, EditorStyles.label);
        }
        GUILayout.Space(5f);
        if (onDraw != null)
            onDraw (target);
        EditorGUILayout.EndHorizontal ();
    }

    public static void ResetLabelWidth ()
    {
        fieldLabelWidth = 100f;
    }

    public static void ResetСontentMaxWidth ()
    {
        fieldContentMaxWidth = 100f;
    }
}
