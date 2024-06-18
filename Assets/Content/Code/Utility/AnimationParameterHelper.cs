using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AnimationLayerLink
{
    [ReadOnly, HideLabel, HorizontalGroup]
    public string name;
    
    [ReadOnly, HideLabel, HorizontalGroup (48f)]
    public int index = -1;
    
    [PropertyRange (0f, 1f), HideLabel]
    public float value;
}

[Serializable]
public class AnimationParameterBoolLink
{
    [ReadOnly, HideLabel, HorizontalGroup ("A")]
    public string name;
    
    [ReadOnly, HideLabel, HorizontalGroup ("A", 48f)]
    public int id = -1;

    [HideLabel, HorizontalGroup ("A", 48f)]
    public bool value;
}

[Serializable]
public class AnimationParameterIntLink
{
    [ReadOnly, HideLabel, HorizontalGroup ("A")]
    public string name;
    
    [ReadOnly, HideLabel, HorizontalGroup ("A", 48f)]
    public int id = -1;

    [HideLabel, HorizontalGroup ("B", 48f)]
    public int min;
    
    [HideLabel, HorizontalGroup ("B"), PropertyRange ("min", "max")]
    public int value;
    
    [HideLabel, HorizontalGroup ("B", 48f)]
    public int max;
}

[Serializable]
public class AnimationParameterFloatLink
{
    [ReadOnly, HideLabel, HorizontalGroup ("A")]
    public string name;
    
    [ReadOnly, HideLabel, HorizontalGroup ("A", 48f)]
    public int id = -1;

    [HideLabel, HorizontalGroup ("B", 48f)]
    public float min;
    
    [HideLabel, HorizontalGroup ("B"), PropertyRange ("min", "max")]
    public float value;
    
    [HideLabel, HorizontalGroup ("B", 48f)]
    public float max;
}

public class AnimationParameterHelper : MonoBehaviour
{
    public Animator animator;
    public bool updateLayers = true;
    public bool updateParameters = true;

    [LabelText ("Layers"), PropertyOrder (2)]
    [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true, DraggableItems = false, HideAddButton = true)]
    public List<AnimationLayerLink> layerLinks;
    
    [LabelText ("Bool Parameters"), PropertyOrder (2)]
    [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true, DraggableItems = false, HideAddButton = true)]
    public List<AnimationParameterBoolLink> parameterBoolLinks;

    [LabelText ("Integer Parameters"), PropertyOrder (2)]
    [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true, DraggableItems = false, HideAddButton = true)]
    public List<AnimationParameterIntLink> parameterIntLinks;
    
    [LabelText ("Float Parameters"), PropertyOrder (2)]
    [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true, DraggableItems = false, HideAddButton = true)]
    public List<AnimationParameterFloatLink> parameterFloatLinks;

    private Dictionary<string, AnimationParameterIntLink> parameterIntLinksLookup = new Dictionary<string, AnimationParameterIntLink> ();
    private Dictionary<string, AnimationParameterFloatLink> parameterFloatLinksLookup = new Dictionary<string, AnimationParameterFloatLink> ();
    
    
    
    [Button, ButtonGroup, PropertyOrder (1)]
    private void FindLayers ()
    {
        if (animator == null)
            return;
        
        layerLinks.Clear ();

        var layerCount = animator.layerCount;
        for (int i = 0; i < layerCount; ++i)
        {
            var layerName = animator.GetLayerName (i);
            var layerWeight = animator.GetLayerWeight (i);
            layerLinks.Add (new AnimationLayerLink
            {
                index = i,
                name = layerName,
                value = layerWeight
            });
        }
    }
    
    [Button, ButtonGroup, PropertyOrder (1)]
    private void FindParameters ()
    {
        if (animator == null)
            return;
        
        parameterBoolLinks.Clear ();
        parameterIntLinks.Clear ();
        parameterFloatLinks.Clear ();

        var parameters = animator.parameters;
        foreach (var parameter in parameters)
        {
            var nameHash = parameter.nameHash;
            var type = parameter.type;
            
            switch (type) 
            {
                case AnimatorControllerParameterType.Bool:
                {
                    var value = animator.GetBool (nameHash);
                    parameterBoolLinks.Add (new AnimationParameterBoolLink
                    {
                        id = nameHash,
                        name = parameter.name,
                        value = value
                    });
                    break;
                }
                case AnimatorControllerParameterType.Int:
                {
                    int value = animator.GetInteger (nameHash);
                    int min = 0;
                    int max = 1;
                    
                    var linkOld = parameterIntLinksLookup.ContainsKey (parameter.name) ? parameterIntLinksLookup[parameter.name] : null;
                    if (linkOld != null)
                    {
                        min = linkOld.min;
                        max = linkOld.max;
                    }
                    
                    parameterIntLinks.Add (new AnimationParameterIntLink
                    {
                        id = nameHash,
                        name = parameter.name,
                        value = value,
                        min = Mathf.Min (min, value),
                        max = Mathf.Max (max, value)
                    });
                    break;
                }
                case AnimatorControllerParameterType.Float:
                {
                    float value = animator.GetFloat (nameHash);
                    float min = 0f;
                    float max = 1f;
                    
                    var linkOld = parameterFloatLinksLookup.ContainsKey (parameter.name) ? parameterFloatLinksLookup[parameter.name] : null;
                    if (linkOld != null)
                    {
                        min = linkOld.min;
                        max = linkOld.max;
                    }
                    
                    parameterFloatLinks.Add (new AnimationParameterFloatLink
                    {
                        id = nameHash,
                        name = parameter.name,
                        value = value,
                        min = Mathf.Min (min, value),
                        max = Mathf.Max (max, value)
                    });
                    break;
                }
                case AnimatorControllerParameterType.Trigger:
                {
                    var value = animator.GetBool (nameHash);
                    break;
                }
            }
        }

        parameterIntLinksLookup.Clear ();
        parameterFloatLinksLookup.Clear ();

        foreach (var link in parameterIntLinks)
            parameterIntLinksLookup.Add (link.name, link);
        
        foreach (var link in parameterFloatLinks)
            parameterFloatLinksLookup.Add (link.name, link);
    }

    private void Start ()
    {

    }

    private void Update ()
    {
        if (animator == null || !Application.isPlaying)
            return;

        if (updateLayers)
            ApplyLayers ();

        if (updateParameters)
            ApplyParameters ();
    }

    private void ApplyLayers ()
    {
        foreach (var link in layerLinks)
            animator.SetLayerWeight (link.index, link.value);
    }

    private void ApplyParameters ()
    {
        foreach (var link in parameterBoolLinks)
            animator.SetBool (link.id, link.value);
        
        foreach (var link in parameterIntLinks)
            animator.SetInteger (link.id, link.value);
        
        foreach (var link in parameterFloatLinks)
            animator.SetFloat (link.id, link.value);
    }
}
