using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

[CanEditMultipleObjects]
[CustomEditor (typeof (FXTween))]
public class FXTweenInspector : OdinEditor
{
    private static Rect sceneRect;
    private static GUIStyle sceneStyle;
    private static Texture2D sceneTex;
    private static Color colorSamplingButton = new Color (1f, 0.5f, 0.3f, 1f);
    private static Color colorFadedLabel = new Color (1f, 1f, 1f, 0.6f);
    private static float timePlayingLast = 0f;
    private static FXTween obj;
    
    public override void OnInspectorGUI ()
    {
        var tree = this.Tree;
        obj = target as FXTween;
        
        DrawDefaultInspector ();

        // InspectorUtilities.BeginDrawPropertyTree (tree, true);
        // tree.GetPropertyAtPath ("available").Draw ();
        // InspectorUtilities.EndDrawPropertyTree (tree);
    }

    public void OnSceneGUI ()
    {
        if (obj == null)
            return;
        
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

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

        bool prefabModeActive = IsInPrefabMode(obj);
        
        Handles.BeginGUI ();

        float heightSamplingMode = prefabModeActive ? 83f : 63f;
        float height = obj.samplingFromEditor ? heightSamplingMode : 44f;
        var areaRect = new Rect (new Vector2 (Screen.width - 310f, Screen.height - height - 80f), new Vector2 (300f, height));
        GUILayout.BeginArea (areaRect, sceneStyle);
        
        EditorGUILayout.BeginHorizontal ();

        GUILayout.Label ("↔", EditorStyles.label);
        GUI.color = colorFadedLabel;
        GUILayout.Label (obj.samplingFromEditor ? "Sampling " : "Idle ", EditorStyles.miniLabel);
        GUI.color = col;
        GUILayout.FlexibleSpace ();
        EditorGUILayout.EndHorizontal ();
        
        EditorGUILayout.BeginHorizontal ();
        var colorCached = GUI.backgroundColor;
        if (obj.samplingFromEditor)
            GUI.backgroundColor = colorSamplingButton;

        if (GUILayout.Button ("Sampling", SirenixGUIStyles.ButtonRight))
            obj.samplingFromEditor = !obj.samplingFromEditor;

        if (obj.samplingFromEditor)
            GUI.backgroundColor = colorCached;
        EditorGUILayout.EndHorizontal ();

        EditorGUIUtility.labelWidth = 60f;
        if (obj.samplingFromEditor)
        {
            EditorGUILayout.BeginVertical ();

            if (prefabModeActive)
            {
                obj.samplingSafetyCheckOverride = EditorGUILayout.Toggle (
                    new GUIContent ("Force", "Force sampling in prefab mode.\n\nUse at your own risk! Make sure you don't save your prefab in an undesirable state"),
                    obj.samplingSafetyCheckOverride,
                    GUILayout.MinWidth (120f), GUILayout.MaxWidth (120f));
            }    

            EditorGUI.BeginChangeCheck ();
            obj.samplingTime = EditorGUILayout.Slider ("Time", obj.samplingTime, 0f, obj.samplingTimeLimit);
            
            if (EditorGUI.EndChangeCheck ())
                Repaint ();

            EditorGUILayout.EndVertical ();
        }

        EditorGUIUtility.labelWidth = width;
        GUILayout.EndArea ();
        Handles.EndGUI ();
    }

    private bool IsInPrefabMode(FXTween obj)
    {
        var mainStage = UnityEditor.SceneManagement.StageUtility.GetMainStageHandle ();
        var currentStage = UnityEditor.SceneManagement.StageUtility.GetStageHandle (obj.gameObject);
        return currentStage != mainStage;
    }
}