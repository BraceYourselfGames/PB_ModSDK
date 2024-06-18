using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// [RequireComponent (typeof (Animator))]
public class AnimationClipHelper : MonoBehaviour 
{
    [System.Serializable]
    public class ProtectedTransformData
    {
        public Transform target;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public int depth;

        public void Reset ()
        {
            if (target == null)
                return;

            target.localPosition = localPosition;
            target.localRotation = localRotation;
            target.localScale = localScale;
        }
    }

    [System.Serializable]
    public class ClipData
    {
        public AnimationClip clip;
        public GameObject root;
    }

    [OnValueChanged ("EvaluateWithUpdateCheck")]
    [InlineButton ("Toggle")]
    public bool update = false;

    [OnValueChanged ("EvaluateWithUpdateCheck")]
    public bool useFrames = false;

    [OnValueChanged ("EvaluateWithUpdateCheck")]
    [HideIf ("useFrames")]
    [Range (0f, 1f)]
    public float time = 0f;

    [OnValueChanged ("EvaluateWithUpdateCheck")]
    [InlineButton ("NextFrame", "Next")]
    [InlineButton ("PrevFrame", "Prev.")]
    [ShowIf ("useFrames")]
    [PropertyRange (0, "GetFrameLimit")]
    public int frame = 0;

    [OnValueChanged ("EvaluateOnClipChange")]
    [InlineButton ("NextClip", "Next")]
    [InlineButton ("PrevClip", "Prev.")]
    public int clipIndex = 0;

    public List<ClipData> clips;

    public List<string> protectedTransformNames;

    [InlineButton ("FindProtectedTransforms", "Find")]
    public List<ProtectedTransformData> protectedTransforms;

    [InlineButton ("FindAllTransforms", "Find")]
    public List<ProtectedTransformData> cachedTransforms;

    private bool updateStatusLast = false;

    
    
    public void EvaluateWithUpdateCheck ()
    {
        if (update)
            Evaluate ();
        else if (update != updateStatusLast)
            ResetEverything ();
        updateStatusLast = update;
    }

    public void EvaluateOnClipChange ()
    {
        if (!update)
            return;

        ResetEverything ();
        EvaluateWithUpdateCheck ();
    }

    private int GetFrameLimit ()
    {
        if (!clipIndex.IsValidIndex (clips))
            return 0;

        var clipData = clips[clipIndex];
        if (clipData == null || clipData.clip == null)
            return 0;

        int frameLimit = Mathf.RoundToInt (clipData.clip.frameRate * clipData.clip.length);
        return frameLimit;
    }

    [Button ("Evaluate", ButtonSizes.Large)]
    public void Evaluate () 
	{
        if (!clipIndex.IsValidIndex (clips) || clips[clipIndex] == null)
        {
            Debug.Log ("Exiting | Clip index invalid: " + (!clipIndex.IsValidIndex (clips)) + " | Selected clip is null: " + (clips[clipIndex] == null));
            return;
        }

        if (clipIndex.IsValidIndex (clips))
        {
            var clipData = clips[clipIndex];
            if (clipData != null && clipData.clip != null && clipData.root != null)
            {
                if (useFrames)
                {
                    int frameLimit = Mathf.RoundToInt (clipData.clip.frameRate * clipData.clip.length);
                    if (frame > frameLimit)
                        frame = frameLimit;
                    else if (frame < 0)
                        frame = 0;

                    clipData.clip.SampleAnimation (clipData.root, (float)frame * (1f / clipData.clip.frameRate));
                }
                else
                    clipData.clip.SampleAnimation (clipData.root, time * clipData.clip.length);
            }
        }

        ResetProtectedTransforms ();
    }




    public void PrevFrame ()
    {
        frame -= 1;
        Evaluate ();
    }

    public void NextFrame ()
    {
        frame += 1;
        Evaluate ();
    }

    public void PrevClip ()
    {
        clipIndex = clipIndex.OffsetAndWrap (false, clips);
        Evaluate ();
    }

    public void NextClip ()
    {
        clipIndex = clipIndex.OffsetAndWrap (true, clips);
        Evaluate ();
    }

    public void Toggle ()
    {
        update = !update;
        AnimationClipHelper[] helpers = gameObject.GetComponents<AnimationClipHelper> ();
        foreach (AnimationClipHelper h in helpers)
        {
            if (h != this && this.update && h.update)
                h.update = false;
        }
    }




    public void FindProtectedTransforms ()
    {
        protectedTransforms = new List<ProtectedTransformData> ();
        FindProtectedTransformsRecursive (transform, 0);
    }

    private void FindProtectedTransformsRecursive (Transform parent, int depth)
    {
        if (parent == null)
            return;

        for (int i = 0; i < protectedTransformNames.Count; ++i)
        {
            if (!parent.name.Contains (protectedTransformNames[i]))
                continue;

            ProtectedTransformData data = new ProtectedTransformData ();
            data.target = parent;
            data.localPosition = parent.localPosition;
            data.localRotation = parent.localRotation;
            data.localScale = parent.localScale;
            data.depth = depth;
            protectedTransforms.Add (data);
            break;
        }

        depth += 1;
        for (int i = 0; i < parent.childCount; ++i)
            FindProtectedTransformsRecursive (parent.GetChild (i), depth);
    }

    public void ResetProtectedTransforms ()
    {
        for (int i = 0; i < protectedTransforms.Count; ++i)
            protectedTransforms[i].Reset ();
    }

    public void FindAllTransforms ()
    {
        cachedTransforms = new List<ProtectedTransformData> ();
        FindAllTransformsRecursive (transform, 0);
    }

    private void FindAllTransformsRecursive (Transform parent, int depth)
    {
        if (parent == null)
            return;

        ProtectedTransformData data = new ProtectedTransformData ();
        data.target = parent;
        data.localPosition = parent.localPosition;
        data.localRotation = parent.localRotation;
        data.localScale = parent.localScale;
        data.depth = depth;
        cachedTransforms.Add (data);

        depth += 1;
        for (int i = 0; i < parent.childCount; ++i)
            FindAllTransformsRecursive (parent.GetChild (i), depth);
    }

    public void ResetEverything ()
    {
        for (int i = 0; i < cachedTransforms.Count; ++i)
            cachedTransforms[i].Reset ();
    }
}
