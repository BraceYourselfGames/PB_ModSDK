using System;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class AnimationCurveContainer
{
    [CustomValueDrawer("Drawer")]
    public AnimationCurve curve;

    [LabelWidth(2f)][LabelText("")][MinMaxSlider (0f, 1f)]
    public Vector2 curveTimeRange = new Vector2 (0f, 1f);

    [NonSerialized]
    public float curveTimeLast = 0f; 
    
    [NonSerialized]
    public float curveSampleLast = 0f;

    public AnimationCurveContainer ()
    {
        curve = new AnimationCurve (new Keyframe (0f, 1f), new Keyframe (1f, 1f));
    }
    
    public AnimationCurveContainer (AnimationCurve curve)
    {
        this.curve = curve;
        var keys = curve.keys;
        for (int i = 0; i < keys.Length; ++i)
        {
            if (keys[i].time > 1f)
                keys[i].time = 1f;
        }
        this.curve.keys = keys;
    }
    
    public float GetCurveSample (float timeNormalized)
    {
        // Just to make sure our input is actually valid normalized time
        timeNormalized = Mathf.Clamp01 (timeNormalized);
        
        // Simple case where the curve is mapped from 0 to 1 and therefore no remapping is necessary
        if (curveTimeRange.x <= 0f && curveTimeRange.y >= 1f)
        {
            curveTimeLast = timeNormalized;
            curveSampleLast = Mathf.Clamp01 (curve.Evaluate (curveTimeLast));
            return curveSampleLast;
        }
        
        // Complex case
        else
        {
            curveTimeLast = RemapToRange (timeNormalized, curveTimeRange.x, curveTimeRange.y, 0f, 1f);
            curveSampleLast = Mathf.Clamp01 (curve.Evaluate (curveTimeLast));
            return curveSampleLast;
        }
    }

    public static float RemapToRange (float f, float a1, float a2, float b1, float b2)
    {
        f = b1 + (f - a1) * (b2 - b1) / (a2 - a1);
        return f;
    }

    #if UNITY_EDITOR

    [NonSerialized]
    public bool indentFix = true;

    public enum WrapModeSimple
    {
        Clamp = 8,
        Loop = 2,
        PingPong = 4
    }

    private AnimationCurve Drawer (AnimationCurve value, GUIContent label)
    {
        EditorGUILayout.BeginHorizontal ();
        //GUILayout.Label ("Start/end", GUILayout.Width (EditorGUIUtility.labelWidth));
        EditorGUI.BeginChangeCheck ();
        var preWrapMode = (WrapModeSimple)(int)curve.preWrapMode;
        preWrapMode = (WrapModeSimple) EditorGUILayout.EnumPopup (preWrapMode, GUILayout.Width (100f));
        if (EditorGUI.EndChangeCheck ())
            curve.preWrapMode = (WrapMode)(int)preWrapMode;
        
        GUILayout.Label ("Pre-wrap mode", EditorStyles.centeredGreyMiniLabel);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("Post-wrap mode", EditorStyles.centeredGreyMiniLabel);

        EditorGUI.BeginChangeCheck ();
        var postWrapMode = (WrapModeSimple)(int)curve.postWrapMode;
        postWrapMode = (WrapModeSimple) EditorGUILayout.EnumPopup (postWrapMode, GUILayout.Width (100f));
        if (EditorGUI.EndChangeCheck ())
            curve.postWrapMode = (WrapMode)(int)postWrapMode;
        EditorGUILayout.EndHorizontal ();

        var rect = new Rect (0, 0, 1, 1);
        EditorGUI.BeginChangeCheck ();
        var curveReference = EditorGUILayout.CurveField (curve, Color.green, rect, GUILayout.Height (32f));

        var rectLine = GUILayoutUtility.GetLastRect ();
        rectLine.x = rectLine.x;
        if (indentFix)
        {
            rectLine.x += 15f;
            rectLine.width -= 15f;
        }

        rectLine.y += 2f;
        rectLine.height -= 4f;
        rectLine.width -= 2f;

        var widthFull = rectLine.width;
        
        var playHeadPos = widthFull;
        if (curveTimeLast >= 0f && curveTimeLast <= 1f)
            playHeadPos *= curveTimeLast;
        else
        {
            var wrap = curveTimeLast < 0f ? curve.preWrapMode : curve.postWrapMode;
            if (wrap == WrapMode.ClampForever)
                playHeadPos *= Mathf.Clamp01 (curveTimeLast);
            else if (wrap == WrapMode.Loop)
                playHeadPos *= curveTimeLast % 1;
            else if (wrap == WrapMode.Loop)
                playHeadPos *= PingPong (curveTimeLast);
        }
        
        rectLine.width = 1f;
        rectLine.x = rectLine.x + playHeadPos;
        EditorGUI.DrawRect (rectLine, new Color (0.5f, 0.5f, 0.5f, 1f));

        return curveReference;
    }
    
    private float PingPong (float value)
    {
        bool ascending = value % 2 == 0;
        float modulus = value % 1f;
        return ascending ? modulus : 1f - modulus;
    }
    
    #endif
}
