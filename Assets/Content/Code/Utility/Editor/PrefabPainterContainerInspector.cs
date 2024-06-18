using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor (typeof (PrefabPainterContainer))]
public class PrefabPainterContainerInspector : Editor
{
    private bool isPainting = false;
    private List<string> layerNames;

    private string gizmoObjectName = "Gizmo Location";
    private Transform gizmoTransform;

    private Rect windowBounds = new Rect (0, 0, 0, 0);
    private Vector3 currentMousePos = Vector3.zero;
    private Vector3 lastPaintPos = Vector3.zero;
    private RaycastHit mouseHitPoint;
    private Event currentEvent;

    private Color activeColor = Color.cyan;
    private Color passiveColor = Color.red;
    private Color spacingColor = Color.white.WithAlpha (0.5f);

    private float gizmoNormalLength = 1;
    private bool showGizmoInfo = true;
    private GUIStyle gizmoGUIStyle = null;

    private static string[] maskOptions;
    private static List<PrefabPainterEntry> maskedEntries;
    private static int maskSelectedLast = -1;
    private static bool maskChanged = true;
    public static bool showMasks = false;

    public override void OnInspectorGUI ()
    {
        PrefabPainterContainer t = target as PrefabPainterContainer;
        
        if (t.settings == null || t.settings.masks == null)
            return;

        if (maskOptions == null || maskOptions.Length != (t.settings.masks.Count + 1))
            UpdateMaskOptions ();

        if (t.settings.maskSelected < 0 || t.settings.maskSelected > t.settings.masks.Count)
            t.settings.maskSelected = 0;

        t.settings.maskSelected = EditorGUILayout.Popup ("Mask", t.settings.maskSelected, maskOptions);
        if (t.settings.maskSelected != maskSelectedLast || maskChanged)
        {
            maskSelectedLast = t.settings.maskSelected;
            if (maskedEntries == null)
                maskedEntries = new List<PrefabPainterEntry> (t.settings.entries.Count);
            else
                maskedEntries.Clear ();
            if (t.settings.maskSelected == 0)
            {
                for (int i = 0; i < t.settings.entries.Count; ++i)
                    maskedEntries.Add (t.settings.entries[i]);
            }
            else
            {


                List<bool> toggles = t.settings.masks[t.settings.maskSelected - 1].toggles;
                for (int i = 0; i < t.settings.entries.Count; ++i)
                {
                    if (toggles[i])
                        maskedEntries.Add (t.settings.entries[i]);
                }
            }
        }

        EditorGUILayout.BeginVertical ("Box");
        UtilityCustomInspector.DrawList 
        (
            "Current mask", 
            maskedEntries, 
            (PrefabPainterEntry e) =>
            {
                if (e.prefab != null)
                    GUILayout.Label (e.prefab.name, EditorStyles.miniLabel);
                else
                    GUILayout.Label ("null", EditorStyles.miniLabel);
            }, 
            null, 
            false, 
            false
        );
        EditorGUILayout.EndVertical ();

        showMasks = EditorGUILayout.Foldout (showMasks, "Show masks");
        if (showMasks)
            UtilityCustomInspector.DrawList (t.settings.masks, DrawMask, AddMask, true, true);

        GUILayout.Space (10f);
        EditorGUILayout.HelpBox("- When starting from scratch, make sure you have Entries list populated\n- Masks allow you to use a subset of assets from Entries list\n- Edit or add new masks in the Show masks foldout", MessageType.Info, true); 
        DrawDefaultInspector ();

        if (GUILayout.Button ("Clear children"))
            UtilityGameObjects.ClearChildren (t.gameObject);

        if (GUILayout.Button("Ground children"))
        {
            var children = t.transform.childCount;
            for (var i = 0; i < children; ++i)
            {
                var pos = t.transform.GetChild (i).transform.position;
                pos.y = 1000;
                if (Physics.Raycast (pos, -Vector3.up, out var hit))
                {
                    t.transform.GetChild (i).transform.position = hit.point;
                }
            }
        }
    }

    private void UpdateMaskOptions ()
    {
        PrefabPainterContainer t = target as PrefabPainterContainer;
        if (t.settings == null || t.settings.masks == null)
            return;

        maskOptions = new string[t.settings.masks.Count + 1];
        maskOptions[0] = "None";
        for (int i = 0; i < t.settings.masks.Count; ++i)
        {
            maskOptions[i + 1] = string.IsNullOrEmpty (t.settings.masks[i].name) ? ("Mask " + i) : t.settings.masks[i].name;
        }
    }

    private void AddMask ()
    {
        PrefabPainterContainer t = target as PrefabPainterContainer;
        t.settings.masks.Add (new PrefabPainterMask (t.settings.entries.Count));
    }

    private void DrawMask (PrefabPainterMask mask)
    {
        EditorGUI.BeginChangeCheck ();
        PrefabPainterContainer t = target as PrefabPainterContainer;

        EditorGUILayout.BeginHorizontal ();
        GUILayout.Label ("Name", GUILayout.Width (70f));
        mask.name = EditorGUILayout.TextField (mask.name);
        EditorGUILayout.EndHorizontal ();

        if (t.settings.entries.Count != mask.toggles.Count)
        {
            GUILayout.Label ("Mask length doesn't match entry count");
            if (GUILayout.Button ("Fix"))
                mask.toggles = new List<bool> (new bool[t.settings.entries.Count]);
            return;
        }

        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.BeginVertical (GUILayout.Width (30f));

        for (int a = 0; a < mask.toggles.Count; ++a)
        {
            mask.toggles[a] = EditorGUILayout.Toggle (mask.toggles[a]);
        }

        EditorGUILayout.EndVertical ();
        EditorGUILayout.BeginVertical ();

        for (int a = 0; a < mask.toggles.Count; ++a)
        {
            GUILayout.Label (t.settings.entries[a].prefab != null ? t.settings.entries[a].prefab.name : "?");
        }
        EditorGUILayout.EndVertical ();
        EditorGUILayout.EndHorizontal ();
        if (EditorGUI.EndChangeCheck ())
        {
            EditorUtility.SetDirty (t);
            maskChanged = true;
        }
    }




    private void OnEnable ()
    {
        layerNames = new List<string> ();
        for (int i = 0; i <= 10; i++)
            layerNames.Add (LayerMask.LayerToName (i));

        EditorApplication.hierarchyChanged += HierarchyChanged;
    }

    private void OnDisable ()
    {
        if (gizmoTransform != null)
            DestroyImmediate (gizmoTransform.gameObject);

        if (GameObject.Find (gizmoObjectName) != null)
            DestroyImmediate (GameObject.Find (gizmoObjectName));

        EditorApplication.hierarchyChanged -= HierarchyChanged;
    }




    public void OnSceneGUI ()
    {
        Camera[] cameras = SceneView.GetAllSceneCameras ();
        if (cameras == null || cameras.Length == 0)
            return;

        Camera camera = cameras[0];

        PrefabPainterContainer t = target as PrefabPainterContainer;

        windowBounds.width = Screen.width;
        windowBounds.height = Screen.height;
        currentEvent = Event.current;

        // Mouse pos

        if (currentEvent.control)
            HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay (new Vector2 (currentEvent.mousePosition.x, camera.pixelHeight - currentEvent.mousePosition.y));
        if (Physics.Raycast (ray, out hit, 1000, t.settings.paintMask))
        {
            currentMousePos = hit.point;
            mouseHitPoint = hit;
        }
        else
            mouseHitPoint = new RaycastHit ();

        // Draw gizmo

        if (isPainting)
            Handles.color = activeColor;
        else
            Handles.color = passiveColor;

        if (mouseHitPoint.transform != null)
        {
            if (gizmoTransform == null)
            {
                GameObject gizmoTransformObject = GameObject.Find (gizmoObjectName);
                if (gizmoTransformObject == null)
                    gizmoTransform = new GameObject (gizmoObjectName).transform;
                else
                    gizmoTransform = gizmoTransformObject.transform;
            }

            gizmoTransform.rotation = mouseHitPoint.transform.rotation;
            gizmoTransform.forward = mouseHitPoint.normal;
            Handles.ArrowHandleCap (3, mouseHitPoint.point, gizmoTransform.rotation, gizmoNormalLength * t.settings.brushRadius, EventType.Repaint);
            Handles.CircleHandleCap (2, currentMousePos, gizmoTransform.rotation, t.settings.brushRadius, EventType.Repaint);

            Handles.color = spacingColor;
            Handles.CircleHandleCap (4, currentMousePos, gizmoTransform.rotation, t.settings.brushSpacing, EventType.Repaint);
            Handles.ArrowHandleCap (5, lastPaintPos, Quaternion.identity, gizmoNormalLength * t.settings.brushRadius, EventType.Repaint);
            gizmoTransform.up = mouseHitPoint.normal;
        }




        Handles.BeginGUI ();

        if (gizmoGUIStyle == null)
        {
            gizmoGUIStyle = new GUIStyle ();
            gizmoGUIStyle.normal.textColor = Color.white;
        }

        GUILayout.BeginArea (new Rect (currentEvent.mousePosition.x + 10, currentEvent.mousePosition.y + 10, 250, 150));
        if (showGizmoInfo)
        {
            var activeCollider = mouseHitPoint.collider;
            if (activeCollider)
            {
                GUILayout.TextField ("Size: " + System.Math.Round (t.settings.brushRadius, 2) + " (Ctrl + Scroll)", gizmoGUIStyle);
                GUILayout.TextField ("Spacing: " + System.Math.Round (t.settings.brushSpacing, 2) + " (Alt + Scroll)", gizmoGUIStyle);
                GUILayout.TextField ("Density: " + System.Math.Round ((double)t.settings.brushDensity, 2), gizmoGUIStyle);
                GUILayout.TextField ("Surface: " + (activeCollider ? mouseHitPoint.collider.name : "none"), gizmoGUIStyle);
                GUILayout.TextField ("Position: " + currentMousePos.ToString (), gizmoGUIStyle);
                GUILayout.Space(10);
                GUILayout.TextField ("Ctrl + LMB - Paint on a surface", gizmoGUIStyle);
                GUILayout.TextField ("Ctrl + Shift + LMB - Erase", gizmoGUIStyle);
            }
            else
            {
                GUILayout.TextField ("No collider detected", gizmoGUIStyle);
            }

        }

        GUILayout.EndArea ();
        Handles.EndGUI ();




        // Scene input

        if (PreventCustomUserHotkey (EventType.ScrollWheel, EventModifiers.Control, KeyCode.None))
        {
            t.settings.brushRadius = t.settings.brushRadius + (currentEvent.delta.y > 0 ? 0.5f : -0.5f);
            t.settings.brushRadius = Mathf.Clamp (t.settings.brushRadius, 0.1f, 100f);
            Repaint ();
        }

        else if (PreventCustomUserHotkey (EventType.ScrollWheel, EventModifiers.Alt, KeyCode.None))
        {
            t.settings.brushSpacing = t.settings.brushSpacing + (currentEvent.delta.y > 0f ? 0.5f : -0.5f);
            t.settings.brushSpacing = Mathf.Clamp (t.settings.brushSpacing, 0.25f, 100);
            Repaint ();
        }

        else if (currentEvent.control && (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown))
        {
            isPainting = true;
            if (currentEvent.shift)
            {
                OnErase (t);
            }
            else
            {
                OnPainting (t);
            }
        }

        else if (isPainting && !currentEvent.control || (currentEvent.button != 0 || currentEvent.type == EventType.MouseUp))
        {
            lastPaintPos = Vector3.zero;
            isPainting = false;
        }

        else if (isPainting && (currentEvent.type == EventType.MouseDrag))
        {
            if (currentEvent.shift)
            {
                OnErase (t);
            }
            else
            {
                OnPainting (t);
            }
        }

        if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
            SceneView.RepaintAll ();
    }

    private int overlapCount;
    private Collider[] overlapColliders;
    private Collider overlapCollider;

    private void OnErase (PrefabPainterContainer t)
    {
        /*
        Debug.Log ("OnErase");
        if (overlapColliders == null)
            overlapColliders = new Collider[10];

        overlapCount = Physics.OverlapSphereNonAlloc (currentMousePos, t.settings.brushRadius, overlapColliders);
        for (int i = 0; i < overlapCount; ++i)
        {
            overlapCollider = overlapColliders[i];
            if (overlapCollider.transform.parent != null && overlapCollider.transform.parent == t.transform)
                DestroyImmediate (overlapCollider.gameObject);
        }
        */

        DeleteInRadius (t, currentMousePos, t.settings.brushSpacing, 1f);
    }

    public void DeleteInRadius (PrefabPainterContainer t, Vector3 origin, float radius, float objectSize = 4f)
    {
        if (t.transform.childCount == 0)
            return;

        float radiusSquared = Mathf.Pow (Mathf.Min (1f, radius) + objectSize, 2f);
        for (int i = t.transform.childCount - 1; i >= 0; --i)
        {
            Transform child = t.transform.GetChild (i);
            if ((child.position - origin).sqrMagnitude < radiusSquared)
            {
                DestroyImmediate (child.gameObject);
            }
        }
    }



    private void OnPainting (PrefabPainterContainer t)
    {
        if (maskedEntries == null || maskedEntries.Count == 0)
        {
            Debug.Log ($"No masked entries available");
            return;
        }

        if (Vector3.Distance (lastPaintPos, currentMousePos) > t.settings.brushSpacing)
        {
            Vector3[] spawnPoint = new Vector3[t.settings.brushDensity];

            for (int i = 0; i < t.settings.brushDensity; ++i)
            {
                // dir = Quaternion.AngleAxis (Random.Range (0, 360), Vector3.up) * Vector3.right;

                Vector2 randomOnCircle = Random.insideUnitCircle;
                Vector3 spawnPos1 = currentMousePos + gizmoTransform.rotation * new Vector3 (randomOnCircle.x, 0f, randomOnCircle.y) * t.settings.brushRadius;

                spawnPoint[i] = spawnPos1;
                if (spawnPos1 == Vector3.zero || Vector3.Distance (spawnPos1, lastPaintPos) < t.settings.brushSpacing)
                    continue;

                PrefabPainterEntry entry = maskedEntries[Random.Range (0, maskedEntries.Count)];
                if (entry.prefab == null)
                    continue;

                if (overlapColliders == null)
                    overlapColliders = new Collider[10];

                bool overlapClear = true;

                float radiusSquared = Mathf.Pow (Mathf.Min (1f, t.settings.brushSpacing), 2f);
                for (int c = t.transform.childCount - 1; c >= 0; --c)
                {
                    Transform child = t.transform.GetChild (c);
                    if ((child.position - currentMousePos).sqrMagnitude < radiusSquared)
                    {
                        overlapClear = false;
                        break;
                    }
                }

                /*
                overlapCount = Physics.OverlapSphereNonAlloc (currentMousePos, t.settings.brushRadius, overlapColliders);
                for (int o = 0; o < overlapCount; ++o)
                {
                    overlapCollider = overlapColliders[o];
                    if 
                    (
                        overlapCollider.transform.parent != null && 
                        overlapCollider.transform.parent == t.transform && 
                        Vector3.Distance (spawnPos1, overlapCollider.transform.position) < t.settings.brushSpacing
                    )
                    {
                        overlapClear = false;
                        break;
                    }
                }
                */

                if (!overlapClear)
                    continue;

                lastPaintPos = spawnPos1;

                Vector3 up = entry.orientVertically ? Vector3.up : gizmoTransform.up;
                Vector3 spawnPosTest = spawnPos1 + up * t.settings.brushRadius;

                RaycastHit groundHit;
                if (Physics.Raycast (spawnPosTest, -up, out groundHit))
                {
                    if (!LayerContains (t.settings.paintMask, groundHit.collider.gameObject.layer))
                        continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab (entry.prefab);

                if (gizmoTransform)
                {
                    instance.transform.rotation = gizmoTransform.rotation;
                    if (entry.orientWithZUp)
                        instance.transform.forward = entry.orientVertically ? Vector3.up : gizmoTransform.up;
                    else
                        instance.transform.up = entry.orientVertically ? Vector3.up : gizmoTransform.up;
                }
                else
                    instance.transform.rotation = Quaternion.identity;

                if (entry.randomRotationX)
                    instance.transform.Rotate (instance.transform.right, Random.Range (-180, 180), Space.Self);

                if (entry.randomRotationY)
                    instance.transform.Rotate (entry.orientWithZUp ? Vector3.forward : Vector3.up, Random.Range (-180, 180), Space.Self);

                if (entry.randomRotationZ)
                    instance.transform.Rotate (entry.orientWithZUp ? Vector3.up : Vector3.forward, Random.Range (-180, 180), Space.Self);

                if (entry.scaleMinMax != Vector2.one && entry.scaleMinMax != Vector2.zero)
                    instance.transform.localScale *= Random.Range (entry.scaleMinMax.x, entry.scaleMinMax.y);

                instance.transform.position = spawnPos1;
                instance.transform.parent = t.transform;
            }
        }
    }

    public bool PreventCustomUserHotkey (EventType type, EventModifiers codeModifier, KeyCode hotkey)
    {
        Event currentevent = Event.current;
        if (currentevent.type == type && currentevent.modifiers == codeModifier && currentevent.keyCode == hotkey)
        {
            currentevent.Use ();
            return true;
        }
        return false;
    }

    private void HierarchyChanged ()
    {
        // Repaint ();
    }

    public bool LayerContains (LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
