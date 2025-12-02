using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(DataMultiLinkerOverworldLandscape))]
public class DataMultiLinkerOverworldLandscapeInspector : OdinEditor
{
    private Color colorUnused = Color.gray;
    private Color colorMain = Color.white;
    private Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);
    private Color colorFriendly = Color.Lerp (Color.cyan, Color.white, 0.5f);

    private Color colorLinkActors = Color.HSVToRGB (0.6f, 0.5f, 1f);
    private Color colorLinkConvoys = Color.HSVToRGB (0.25f, 0.5f, 0.75f);
    private Color colorLinkCounterAttacks = Color.HSVToRGB (0.1f, 0.5f, 1f);
    private Color colorLinkWar = Color.HSVToRGB (0f, 0.5f, 1f);
    
    private Color colorNoDecisiveObjectives = Color.HSVToRGB (0.25f, 0.5f, 1f);

    private Vector3 posLast = Vector3.zero;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    // private int spawnIndexSelected = -1;
    // private string spawnGroupKeySelected = null;

    public void OnSceneGUI ()
    {
        var landscape = DataMultiLinkerOverworldLandscape.selection;
        if (landscape == null)
            return;
        
        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var circleRotation = Quaternion.Euler (90f, 0f, 0f);
        bool showSpawnsGroupKeys = DataMultiLinkerOverworldLandscape.Presentation.showSpawnsGroupKeys;
        bool showSpawnsGrouped = DataMultiLinkerOverworldLandscape.Presentation.showSpawnsGrouped;
        bool showSpawnsGeneral = DataMultiLinkerOverworldLandscape.Presentation.showSpawnsGeneral;
        
        var e = Event.current;
        bool eventPresent = e.type == EventType.MouseDown;
        // if (eventPresent)
        //     e.Use ();

        var buttonPressed = KeyCode.None;
        if (eventPresent)
        {
            if (e.button == 0)
                buttonPressed = KeyCode.Mouse0;
            if (e.button == 1)
                buttonPressed = KeyCode.Mouse1;
            if (e.button == 2)
                buttonPressed = KeyCode.Mouse2;
        }
        
        var worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
        bool inputCheckSuccessful = buttonPressed == KeyCode.Mouse0 && e.type != EventType.Used;
        bool alt = e.alt;

        Handles.color = colorEnemy;
        
        if (showSpawnsGeneral && landscape.spawnsGeneral != null && landscape.spawnsGeneral.Count > 0)
        {
            for (int s = 0, sLimit = landscape.spawnsGeneral.Count; s < sLimit; ++s)
            {
                var pos = landscape.spawnsGeneral[s];
                var interpolant = (float)s / landscape.spawnsGeneral.Count;
                var col = Color.HSVToRGB (interpolant, 0.5f, 1f);
                Handles.color = col.WithAlpha (0.35f);
                Handles.DrawLine (pos, pos + Vector3.up * 5f);

                Handles.color = Color.white;
                Handles.DrawLine (pos, pos + Vector3.up);
            }
        }
        
        if (showSpawnsGrouped && landscape.pointGroups != null && landscape.pointGroups.Count > 0)
        {
            var selectionPointGroupKey = DataMultiLinkerOverworldLandscape.selectionPointGroupKey;
            var selectionPointIndex = DataMultiLinkerOverworldLandscape.selectionPointIndex;
            
            int pointGroupIndex = -1;
            int pointGroupCount = landscape.pointGroups.Count;

            foreach (var kvp2 in landscape.pointGroups)
            {
                pointGroupIndex += 1;
                var pointGroupKey = kvp2.Key;
                var pointGroup = kvp2.Value;
                
                if (pointGroup == null)
                    return; 
                
                var pointGroupSelected = pointGroupKey == selectionPointGroupKey;
                Handles.color = Color.HSVToRGB ((float)pointGroupIndex / pointGroupCount, 0.25f, pointGroupSelected ? 1f : 0.65f);

                if (pointGroup.points != null)
                {
                    for (int i = pointGroup.points.Count - 1; i >= 0; --i)
                    {
                        var point = pointGroup.points[i];
                        bool pointIsSelected = i == selectionPointIndex;

                        Handles.DrawLine (point, point + Vector3.up * 25f);
                        Handles.CircleHandleCap (0, point, circleRotation, 12f, EventType.Repaint);

                        if (pointGroupSelected)
                        {
                            Handles.Label (point + Vector3.up * 30f, i == 0 && showSpawnsGroupKeys ? $"   {pointGroupKey}\n   {i}" : $"   {i}");

                            if (i > 0)
                            {
                                var c = Handles.color;
                                Handles.color = colorUnused;
                                var pointPrev = pointGroup.points[i - 1];
                                Handles.DrawLine (point, pointPrev);
                                Handles.color = c;
                            }
                        }

                        var size = HandleUtility.GetHandleSize (point);
                        if (Handles.Button (point, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                        {
                            if (pointGroupSelected)
                            {
                                if (e.alt)
                                    pointGroup.points.RemoveAt (i);
                                else
                                    selectionPointIndex = DataMultiLinkerOverworldLandscape.selectionPointIndex = i;
                            }
                            else
                            {
                                selectionPointGroupKey = DataMultiLinkerOverworldLandscape.selectionPointGroupKey = pointGroupKey;
                                selectionPointIndex = DataMultiLinkerOverworldLandscape.selectionPointIndex = -1;
                            }

                            Repaint ();
                        }

                        if (pointGroupSelected && pointIsSelected)
                        {
                            EditorGUI.BeginChangeCheck ();
                            var positionModified = Handles.DoPositionHandle (point, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck ())
                            {
                                var groundingRayOrigin = new Vector3 (positionModified.x, 200f, positionModified.z);
                                var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                                if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                                    positionModified = hit.point;

                                // Debug.Log ($"Adjusted position of point {i} to {position} | Shift: {e.shift} | Alt: {e.alt}");
                                pointGroup.points[i] = positionModified;
                                Repaint ();
                            }
                        }
                    }
                }
                
                if (pointGroupSelected)
                {
                    if (Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask) && alt)
                    {
                        var point = hitInfo.point;
                        Handles.color = colorMain;
                        Handles.DrawLine (hitInfo.point, hitInfo.point + Vector3.up * 5f);

                        bool pointValid = true;
                        if (pointGroup.points != null)
                        {
                            foreach (var pointExisting in pointGroup.points)
                            {
                                var sqrMagnitude = (point - pointExisting).sqrMagnitude;
                                if (sqrMagnitude < 900)
                                {
                                    // Debug.LogWarning ($"Can't place a new point at {hitInfo.point}: too close to existing spawn");
                                    pointValid = false;
                                    break;
                                }
                            }
                        }

                        Handles.color = pointValid ? colorMain : colorEnemy;
                        Handles.CircleHandleCap (0, hitInfo.point, circleRotation, 12f, EventType.Repaint);

                        if (inputCheckSuccessful && pointValid)
                        {
                            if (pointGroup.points == null)
                                pointGroup.points = new List<Vector3> ();
                            pointGroup.points.Add (point);
                            selectionPointIndex = DataMultiLinkerOverworldLandscape.selectionPointIndex = -1;
                            
                            Repaint ();
                        }
                    }
                }
            }
            
            

        }

        Handles.BeginGUI();

        GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
        GUILayout.Label ("Landscape editor", EditorStyles.boldLabel);
        GUILayout.Space (8f);
        GUILayout.Label (landscape.key, EditorStyles.miniLabel);
        GUILayout.Label (posLast.ToString (), EditorStyles.miniLabel);
        GUILayout.BeginHorizontal ();
        GUILayout.Label ("LMB (point)\nAlt+LMB (point)\nLMB (empty)\nAlt+LMB (empty)", EditorStyles.miniLabel);
        GUILayout.Label ("Select point\nDelete point\nSelect province\nInsert point", EditorStyles.miniLabel);
        GUILayout.EndHorizontal ();
        GUILayout.Space (8f);
        GUILayout.EndVertical ();
        
        Handles.EndGUI ();
        
        if (GUI.changed)
        {
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            view.Repaint ();
        }
        
        SceneView.RepaintAll ();
    }
    
    void OnEnable()
    {
        Tools.hidden = true;
    }
 
    void OnDisable()
    {
        Tools.hidden = false;
    }
}
