using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

public class AnimationKeyframeHelper : MonoBehaviour
{
    public Transform root;
    public AnimationClip clip;
    public Animator animator;
    
    [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true)]
    public List<float> keyframes;
    
    [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false, AlwaysAddDefaultValue = true)]
    public List<Transform> targets;

    [Button, ButtonGroup, PropertyOrder (-1)]
    private void FindTransforms ()
    {
        if (root == null)
            return;

        var children = root.GetComponentsInChildren<Transform> (true);
        targets = new List<Transform> (children.Length);
        targets.AddRange (children);
    }
    
    [Button, ButtonGroup, PropertyOrder (-1)]
    private void WriteToClip ()
    {
        if (root == null || targets == null || targets.Count == 0 || clip == null || keyframes == null || keyframes.Count == 0)
        {
            Debug.LogWarning ($"Nothing to write - no parent or registered joints or clip");
            return;
        }
        
        clip.ClearCurves ();

        var type = typeof (Transform);
        var pathList = new List<Transform> ();
        var sb = new StringBuilder ();
        var count = targets.Count;
        
        var propertyNamePositionX = "localPosition.x";
        var propertyNamePositionY = "localPosition.y";
        var propertyNamePositionZ = "localPosition.z";
        
        var propertyNameRotationX = "localRotation.x";
        var propertyNameRotationY = "localRotation.y";
        var propertyNameRotationZ = "localRotation.z";
        var propertyNameRotationW = "localRotation.w";
        
        for (int i = 0; i < count; ++i)
        {
            Transform transformTarget = targets[i];
            if (transformTarget == null)
            {
                Debug.LogWarning ($"Target {i} is null, skipping writing from it");
                continue;
            }

            pathList.Clear ();
            sb.Clear ();
            
            Transform transformCurrent = transformTarget;
            int a = 0;
            
            while (a < 100)
            {
                if (transformCurrent == root)
                    break;

                pathList.Add (transformCurrent);
                transformCurrent = transformCurrent.parent;
                a += 1;
            }

            for (int x = pathList.Count - 1; x >= 0; --x)
            {
                transformCurrent = pathList[x];
                sb.Append (transformCurrent.name);
                if (x > 0)
                    sb.Append ("/");
            }
            
            var pos = transformTarget.localPosition;
            var rot = transformTarget.localRotation;
            
            var path = sb.ToString ();
            Debug.Log ($"{i} / {path}");

            clip.SetCurve (path, type, propertyNamePositionX, GetCurve (pos.x));
            clip.SetCurve (path, type, propertyNamePositionY, GetCurve (pos.y));
            clip.SetCurve (path, type, propertyNamePositionZ, GetCurve (pos.z));

            clip.SetCurve (path, type, propertyNameRotationX, GetCurve (rot.x));
            clip.SetCurve (path, type, propertyNameRotationY, GetCurve (rot.y));
            clip.SetCurve (path, type, propertyNameRotationZ, GetCurve (rot.z));
            clip.SetCurve (path, type, propertyNameRotationW, GetCurve (rot.w));
        }
    }

    private AnimationCurve GetCurve (float value)
    {
        var count = keyframes.Count;
        var keys = new Keyframe[count];
        
        for (int i = 0; i < count; ++i)
        {
            var time = keyframes[i];
            keys[i] = new Keyframe (time, value);
        }
        
        var curve = new AnimationCurve (keys);
        return curve;
    }
}
