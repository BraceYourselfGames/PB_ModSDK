using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(DataMultiLinkerOverworldProvinceBlueprints))]
public class DataMultiLinkerOverworldProvinceBlueprintsInspector : OdinEditor
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
    private int selectedBorderPoint = -1;
    private bool selectedBorderPointPresent = false;
    private int selectedPointOfInterest = -1;
    private bool pointMode = true;
    
    private string provinceKeyLast = string.Empty;

    private bool snapRequested = false;
    private bool snapFound = false;
    private Vector3 snapPosLast = Vector3.zero;

    private double spawnTestTimeLast = 0;
    private Vector3 spawnTestOriginLast = Vector3.zero;
    private List<Vector3> spawnTestPointsLast = new List<Vector3> ();
    private string spawnTestProvinceLast = string.Empty;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    public void OnSceneGUI ()
    {
        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var provinces = DataMultiLinkerOverworldProvinceBlueprints.data;
        var selected = DataMultiLinkerOverworldProvinceBlueprints.selection;
        var circleRotation = Quaternion.Euler (90f, 0f, 0f);

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
        
        foreach (var kvp in provinces)
        {
            var province = kvp.Value;
            bool isSelected = selected == province;
            
            Color colorProvinceGeneral = colorEnemy;
            if (!province.hidden)
            {
                if (isSelected)
                    colorProvinceGeneral = colorMain;
                else
                    colorProvinceGeneral = colorEnemy;
            }
            else
                colorProvinceGeneral = colorUnused;

            if (isSelected)
            {
                var hc2 = Handles.color;
                Handles.color = colorProvinceGeneral;
                Handles.DrawLine (Vector3.zero, Vector3.zero + Vector3.up * 50f);
                Handles.Label (Vector3.zero + Vector3.up * 30f, $"      {province.key}");
                Handles.CircleHandleCap (0, Vector3.zero, circleRotation, 25f, EventType.Repaint);
            }
        }
        
        Handles.BeginGUI();

        GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
        GUILayout.Label ("Province editor", EditorStyles.boldLabel);
        GUILayout.Space (8f);
        GUILayout.Label (provinceKeyLast, EditorStyles.miniLabel);
        GUILayout.Label (posLast.ToString (), EditorStyles.miniLabel);
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

    /*
    private Vector3 ShowEditablePoint (int index)
    {
        Vector3 point = handleTransform.TransformPoint (spline.GetControlPoint (index));
        float size = HandleUtility.GetHandleSize (point);
        if (index == 0)
        {
            size *= 2f;
        }
        Handles.color = modeColors[(int)spline.GetControlPointMode (index)];
        if (Handles.Button (point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint ();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck ();
            point = Handles.DoPositionHandle (point, handleRotation);
            if (EditorGUI.EndChangeCheck ())
            {
                Undo.RecordObject (spline, "Move Point");
                EditorUtility.SetDirty (spline);
                spline.SetControlPoint (index, handleTransform.InverseTransformPoint (point));
            }
        }
        return point;
    }
    */
}
