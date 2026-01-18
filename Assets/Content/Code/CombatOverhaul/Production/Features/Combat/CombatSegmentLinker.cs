using System;
using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CombatSegmentParticles
{
    public ParticleSystem target;

    [LabelText("Play On Start")]
    public bool activeOnStart;
    
    public float duration = 5.0f;
}

[Serializable]
public class CombatSegmentTween
{
    public FXTween target;
    
    [LabelText("Play On Start")]
    public bool activeOnStart;
    
    // Variables are hidden, but made public for easy access for scenario logic
    [HideInInspector]
    public float duration;

    [HideInInspector]
    public Vector2 range;
}

public enum SegmentObjectMode
{
    SimpleActivation = 0,
    ParticleSystem = 1,
    TweenComponent = 2
}

[Serializable]
public class CombatSegmentChild
{
    [Space (3)]
    public string key;

    [BoxGroup("Params", false)]
    [GUIColor ("ModeSelectorColor")]
    public GameObject root;

    [BoxGroup("Params", false)]
    [GUIColor ("ModeSelectorColor")]
    public bool activeOnStart;

    [BoxGroup("Params/Mode", false)]
    [GUIColor ("ModeSelectorColor")]
    public SegmentObjectMode mode = SegmentObjectMode.SimpleActivation;
    
    [SerializeReference]
    [BoxGroup("Params/Mode", false)]
    [ShowIf ("@mode == SegmentObjectMode.ParticleSystem")]
    [GUIColor ("ModeSelectorColor")]
    public CombatSegmentParticles particles;
    
    [SerializeReference]
    [BoxGroup("Params/Mode", false)]
    [ShowIf ("@mode == SegmentObjectMode.TweenComponent")]
    [GUIColor ("ModeSelectorColor")]
    public CombatSegmentTween tween;

    private Color ModeSelectorColor ()
    {
        Color col = Color.gray;
        if (mode == SegmentObjectMode.SimpleActivation)
        {
            col = new Color (1.0f, 0.8f, 0.7f);
        }
        else if (mode == SegmentObjectMode.TweenComponent)
        {
            col = new Color (0.8f, 1.0f, 0.7f);
        }
        else if (mode == SegmentObjectMode.ParticleSystem)
        {
            col = new Color (0.7f, 0.8f, 1.0f);
        }
        return col;
    }
}

public class CombatSegmentLinker : MonoBehaviour
{
    [HideInEditorMode]
    [PropertyOrder (-2), BoxGroup ("Test", false)]
    [ValueDropdown ("GetKeys"), ShowInInspector, NonSerialized]
    private string testKey = string.Empty;
    
    [ListDrawerSettings (ElementColor = "GetElementColor")]
    public List<CombatSegmentChild> objectList = new List<CombatSegmentChild> ();

    public List<GameObject> objectsActiveAtNight = new List<GameObject> ();

    #if UNITY_EDITOR
    private Color GetElementColor (int index, Color defaultColor)
    {
        float b = index % 2 == 0 ? 0.5f : 0.8f;
        return new Color (b, b, b, 0.2f);
    }
    #endif
}