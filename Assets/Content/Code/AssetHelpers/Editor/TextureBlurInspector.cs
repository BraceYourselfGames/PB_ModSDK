using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (TextureBlur))]
public class TextureBlurInspector : Editor
{
    public override void OnInspectorGUI ()
    {

        TextureBlur t = (TextureBlur) target;

        if (GUILayout.Button ("Apply blur"))
        {
            t.PrepareForBlur ();
            t.PerformBlur ();
            t.FinishBlur ();
        }

        DrawDefaultInspector ();
    }
}
