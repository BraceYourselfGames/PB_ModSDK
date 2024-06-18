using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (ShaderHelper))]
public class ShaderHelperInspector : Editor
{
    public override void OnInspectorGUI ()
    {
        ShaderGlobalConfig c = ShaderGlobalHelper.config;
        ShaderHelper t = target as ShaderHelper;

        EditorGUILayout.BeginVertical ("Box");
        GUILayout.Label ("Config is " + c.ToStringNullCheck (), EditorStyles.miniLabel);
        EditorGUILayout.EndVertical ();

        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.BeginVertical ();
        if (GUILayout.Button ("Load config"))
            ShaderGlobalHelper.LoadConfig ();
        if (GUILayout.Button ("Load textures"))
            t.LoadTextures ();
        EditorGUILayout.EndVertical ();

        EditorGUILayout.BeginVertical ();
        if (GUILayout.Button ("Save config"))
            ShaderGlobalHelper.SaveConfig ();
        if (GUILayout.Button ("Set defaults"))
        {
            if (c == null)
            {
                c = new ShaderGlobalConfig ();
                ShaderGlobalHelper.config = c;
            }

            c.globalTriplanarScale = 2f;
            c.globalEnvironmentDetailScale = 4f;
            c.globalEnvironmentDetailContrast = 0.5f;
            c.globalEnvironmentRampScale = 1.1f;
            c.globalEnvironmentRampInfluence = 1f;
            c.globalEnvironmentDamageOffset = 2f;

            c.globalEnvironmentAOSize = 10f;
            c.globalEnvironmentAOShift = 21f;
            c.globalEnvironmentAOToggle = 1f;
            c.globalEnvironmentColorSaturation = 1f;
        }
        if (GUILayout.Button ("Set globals"))
            ShaderGlobalHelper.UpdateGlobals ();
        EditorGUILayout.EndVertical ();
        EditorGUILayout.EndHorizontal ();

        if (c != null)
        {
            EditorGUILayout.Space ();
            c.globalUnitRampSize = EditorGUILayout.Slider ("Ramp scale", c.globalUnitRampSize, 0.01f, 2f);
            c.globalTriplanarScale = EditorGUILayout.Slider ("Triplanar scale", c.globalTriplanarScale, 0.25f, 8f);
            c.globalEnvironmentDetailScale = EditorGUILayout.Slider ("Environment detail scale", c.globalEnvironmentDetailScale, 0.25f, 8f);
            c.globalEnvironmentDetailContrast = EditorGUILayout.Slider ("Environment detail contrast", c.globalEnvironmentDetailContrast, 0f, 1f);
            c.globalEnvironmentRampScale = EditorGUILayout.Slider ("Environment ramp scale", c.globalEnvironmentRampScale, 0.25f, 8f);
            c.globalEnvironmentRampInfluence = EditorGUILayout.Slider ("Environment ramp influence", c.globalEnvironmentRampInfluence, 0f, 1f);
            c.globalEnvironmentDamageOffset = EditorGUILayout.Slider ("Environment damage offset", c.globalEnvironmentDamageOffset, 1f, 8f);

            EditorGUILayout.Space ();
            c.globalEnvironmentAOSize = EditorGUILayout.Slider ("Environment AO gradient size", c.globalEnvironmentAOSize, 1f, 100f);
            c.globalEnvironmentAOShift = EditorGUILayout.Slider ("Environment AO gradient shift", c.globalEnvironmentAOShift, 0f, 100f);
            c.globalEnvironmentAOToggle = EditorGUILayout.Slider ("Environment AO gradient toggle", c.globalEnvironmentAOToggle, 0f, 1f);
            c.globalEnvironmentColorSaturation = EditorGUILayout.Slider ("Environment fade", c.globalEnvironmentColorSaturation, 0f, 1f);

            EditorGUILayout.Space ();
            c.treeBillboardStart = EditorGUILayout.Slider ("Tree billboard start", c.treeBillboardStart, 30f, 150f);
            c.treeBillboardFadeLength = EditorGUILayout.Slider ("Tree billboard fade length", c.treeBillboardFadeLength, 1f, 15f);
            c.treeBillboardFadeOut = EditorGUILayout.Slider ("Tree billboard fade out", c.treeBillboardFadeOut, 1f, 15f);

            EditorGUILayout.Space ();
            c.backgroundSwapDistanceStart = EditorGUILayout.Slider ("Background swap distance start", c.backgroundSwapDistanceStart, 0f, 25f);
            c.backgroundSwapDistanceEnd = EditorGUILayout.Slider ("Background swap distance end", c.backgroundSwapDistanceEnd, 50f, 200f);
            c.backgroundGrassScale = EditorGUILayout.Slider ("Background grass scale", c.backgroundGrassScale, 6f, 24f);
            c.backgroundGrassScaleAtDistance = EditorGUILayout.Slider ("Background grass distant resize", c.backgroundGrassScaleAtDistance, 0.25f, 0.5f);
            c.backgroundAsphaltScale = EditorGUILayout.Slider ("Background asphalt scale", c.backgroundAsphaltScale, 6f, 24f);
            c.backgroundAsphaltScaleAtDistance = EditorGUILayout.Slider ("Background asphalt distant resize", c.backgroundAsphaltScaleAtDistance, 0.25f, 0.5f);
            
            c.heightGradientBottom = EditorGUILayout.Slider ("Height gradient bottom", c.heightGradientBottom, -30f, 0f);
            c.heightGradientTop = EditorGUILayout.Slider ("Height gradient top", c.heightGradientTop, -30f, 0f);
            c.brightnessOffsetBottom = EditorGUILayout.Slider ("Brightness offset bottom", c.brightnessOffsetBottom, -2f, 2f);
            c.brightnessOffsetTop = EditorGUILayout.Slider ("Brightness offset top", c.brightnessOffsetTop, -2f, 2f);

            if (GUI.changed)
            {
                ShaderGlobalHelper.UpdateGlobals ();
                SceneView.RepaintAll ();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews ();
            }
        }

        DrawDefaultInspector ();
    }
}
