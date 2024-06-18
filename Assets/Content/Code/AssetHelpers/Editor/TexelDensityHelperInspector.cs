using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (TexelDensityHelper))]
public class TexelDensityHelperInspector : Editor
{
    public override void OnInspectorGUI ()
    {
        DrawDefaultInspector ();
        TexelDensityHelper t = (TexelDensityHelper) target;
        if (GUILayout.Button ("Evaluate size and apply"))
        {
			Undo.RecordObject (t, "TexelDensityHelper refreshed");
            t.Evaluate ();
        }
    }
}
