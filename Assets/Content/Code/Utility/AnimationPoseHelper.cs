using UnityEngine;

[ExecuteInEditMode]
public class AnimationPoseHelper : MonoBehaviour
{
    public Animator animator;

    [Range (0, 50)]
    public int animationClipIndex = 0;
    private float animationClipIndexLast = -10;
    
    [Range (0f, 1f)]
    public float animationClipTimeRounding = 0f;
    
    [Range (0f, 1f)]
    public float animationClipTime = 0f;
    private float animationClipTimeLast = -10f;
    
    

#if UNITY_EDITOR
    private void Update ()
    {
        if (!enabled)
            return;

        if (animator == null)
            return;

        if (animationClipIndexLast == animationClipIndex && animationClipTimeLast == animationClipTime)
            return;

        AnimatorStateInfo aniStateInfo = animator.GetCurrentAnimatorStateInfo (0);
        
        if (animationClipTimeRounding > 0f)
            animationClipTime = animationClipTime - animationClipTime % animationClipTimeRounding;

        animator.Play (aniStateInfo.shortNameHash, 0, animationClipTime);
        animator.Update (0f);

        animationClipIndexLast = animationClipIndex;
        animationClipTimeLast = animationClipTime;
    }
#endif


    /*
    public Animation animationComponent;

    [Range (0, 50)]
    public int animationClipIndex = 0;
    private float animationClipIndexLast = -10;

    [Range (0f, 1f)]
    public float animationClipTime = 0f;
    private float animationClipTimeLast = -10f;

    private void Update ()
    {
        if (animationComponent == null)
            return;

        if (animationClipIndexLast == animationClipIndex && animationClipTimeLast == animationClipTime)
            return;

        int clipCount = animationComponent.GetClipCount ();
        if (animationClipIndex < 0 || animationClipIndex >= clipCount)
        {
            Debug.LogWarning ("AnimationPoseHelper | Update | Clip index is out of range");
            return;
        }

        AnimationState state = GetStateByIndex (animationClipIndex);
        if (state == null)
        {
            Debug.LogWarning ("AnimationPoseHelper | Update | Clip not found at index");
            return;
        }

        state.enabled = true;
        state.weight = 1;
        state.normalizedTime = animationClipTime;
        animationComponent.Sample ();
        state.enabled = false;

        animationClipIndexLast = animationClipIndex;
        animationClipTimeLast = animationClipTime;
    }

    private AnimationState GetStateByIndex (int index)
    {
        if (animationComponent == null)
            return null;

        int i = 0;
        foreach (AnimationState animationState in animationComponent)
        {
            if (i == index)
                return animationState;
            i++;
        }
        return null;
    }
    */
}
