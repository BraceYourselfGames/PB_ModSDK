using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

[CanEditMultipleObjects]
[CustomEditor (typeof (FXSystem))]
public class FXSystemInspector : OdinEditor
{
    private static Rect sceneRect;
    private static GUIStyle sceneStyle;
    private static Texture2D sceneTex;
    private static Color colorSamplingButton = new Color (1f, 0.5f, 0.3f, 1f);
    private static Color colorFadedLabel = new Color (1f, 1f, 1f, 0.6f);
    private static float timePlayingLast = 0f;
    
    public override void OnInspectorGUI ()
    {
        var tree = this.Tree;
        var obj = this.target as FXSystem;
        
        DrawDefaultInspector ();

        // InspectorUtilities.BeginDrawPropertyTree (tree, true);
        // tree.GetPropertyAtPath ("available").Draw ();
        // InspectorUtilities.EndDrawPropertyTree (tree);
    }

    public void OnSceneGUI ()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
        var obj = this.target as FXSystem;
        var col = GUI.color;
        var width = EditorGUIUtility.labelWidth;
        
        if (sceneStyle == null)
        {
            sceneTex = new Texture2D (1,1);
            sceneTex.SetPixel(0, 0, new Color (0.3f, 0.3f, 0.3f, 1f)); 
            sceneTex.Apply();
            
            sceneStyle = new GUIStyle (EditorStyles.helpBox);
            // sceneStyle.normal.background = sceneTex;
        }
        
        Handles.BeginGUI ();

        var areaRect = new Rect (new Vector2 (Screen.width - 310f, Screen.height - 90f - 48f), new Vector2 (300f, 90f));
        GUILayout.BeginArea (areaRect, sceneStyle);
        
        EditorGUILayout.BeginHorizontal ();

        GUILayout.Label (obj.sampling ? "↔" : (obj.playing ? "►" : "■"), EditorStyles.label);
        GUI.color = colorFadedLabel;
        GUILayout.Label (obj.sampling ? "Sampling " : (obj.playing ? "Playing " : "Idle "), EditorStyles.miniLabel);
        GUI.color = col;
        GUILayout.FlexibleSpace ();
        EditorGUILayout.EndHorizontal ();
        
        EditorGUILayout.BeginHorizontal ();
        
        EditorGUI.BeginDisabledGroup (!obj.IsPlaying () || obj.sampling);
        if (GUILayout.Button ("Stop", SirenixGUIStyles.ButtonLeft)) 
        {
            obj.StopAll ();
        }
        EditorGUI.EndDisabledGroup ();
        
        EditorGUI.BeginDisabledGroup (obj.IsPlaying () || obj.sampling);
        if (GUILayout.Button ("Play", SirenixGUIStyles.ButtonMid)) 
        {
            obj.Play ();
        }
        EditorGUI.EndDisabledGroup ();

        var colorCached = GUI.backgroundColor;
        if (obj.sampling)
            GUI.backgroundColor = colorSamplingButton;

        if (GUILayout.Button ("Sampling", SirenixGUIStyles.ButtonRight))
        {
            obj.sampling = !obj.sampling;
            if (obj.sampling && obj.playing)
            {
                obj.samplingTime = obj.timePlaying;
                obj.StopAll ();
                obj.Update ();
            }
            else
            {
                obj.StopAll ();
            }
        }

        if (obj.sampling)
            GUI.backgroundColor = colorCached;

        EditorGUILayout.EndHorizontal ();

        
        EditorGUIUtility.labelWidth = 60f;
        EditorGUILayout.Space ();
        
        if (obj.sampling)
        {
            EditorGUI.BeginChangeCheck ();
            obj.samplingTime = EditorGUILayout.Slider ("Time", obj.samplingTime, 0f, obj.samplingTimeLimit);
            if (EditorGUI.EndChangeCheck ())
                Repaint ();
            
            // GUI.color = colorFadedLabel;
            // GUILayout.Label ("This mode allows you to scrub through the effect lifetime", EditorStyles.miniLabel);
            // GUI.color = col;
        }
        else
        {
            EditorGUI.BeginDisabledGroup (true);
            EditorGUILayout.Slider ("Time", obj.timePlaying, 0f, obj.samplingTimeLimit);
            EditorGUI.EndDisabledGroup ();
            obj.playbackSpeed = EditorGUILayout.Slider ("Speed", obj.playbackSpeed, 0.1f, 1f);

            if (obj.timePlaying != timePlayingLast)
            {
                timePlayingLast = obj.timePlaying;
                Repaint ();
            }
        }

        EditorGUIUtility.labelWidth = width;

        GUILayout.EndArea ();
        
        Handles.EndGUI ();
    }

    private void OnWindowGUI (int windowID)
    {
        if (GUILayout.Button("Hello World"))
        {
            Debug.Log ("Test");
        }
    }
}