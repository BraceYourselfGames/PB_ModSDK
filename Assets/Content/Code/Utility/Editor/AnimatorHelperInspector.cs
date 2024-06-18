

/*
[CustomEditor (typeof (AnimatorHelper))]
public class AnimatorHelperInspector : Editor
{
    public override void OnInspectorGUI ()
    {
        AnimatorHelper t = target as AnimatorHelper;

        t.clip = (AnimationClip)EditorGUILayout.ObjectField ("Clip", t.clip, typeof (AnimationClip), true);
        t.clipTime = EditorGUILayout.Slider ("Time", t.clipTime, 0f, 1f);

        if (GUI.changed)
        {
            GUI.changed = false;
            t.Evaluate ();
        }

        if (t.anm == null)
            t.anm = t.gameObject.GetComponent<Animator> ();

        if (GUILayout.Button ("Grab clips from animator"))
        {
            if (t.anm.runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox ("Animator is missing a controller, sampling is not possible", MessageType.Warning);
                return;
            }

            AnimationClip[] clips = t.anm.runtimeAnimatorController.animationClips;
            if (clips == null || clips.Length == 0)
            {
                EditorGUILayout.HelpBox ("Animator controller is missing clips, sampling is not possible", MessageType.Warning);
                return;
            }

            t.clips = new List<AnimationClip> (clips);
            t.clips.Sort ((a, b) => (a.name.CompareTo (b.name)));
        }


        if (t.clips != null && t.clips.Count > 0)
        {
            EditorGUILayout.BeginVertical ();
            for (int i = 0; i < t.clips.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal ("Box");
                if (GUILayout.Button ("Select", EditorStyles.miniButton, GUILayout.Width (100f)))
                {
                    t.clip = t.clips[i];
                    t.Evaluate ();
                }
                GUILayout.Label (t.clips[i].name, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal ();
            }
            EditorGUILayout.EndVertical ();
        }
    }
}

*/