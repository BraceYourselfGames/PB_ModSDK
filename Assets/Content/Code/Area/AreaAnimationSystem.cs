using Area;
using CustomRendering;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

public class AreaAnimationSystem : MonoBehaviour
{
    public static AreaAnimationSystem ins;

    public float propDestructionDurationFall = 4f;
    public float propDestructionDurationBasic = 0.5f;
    
    [LabelText ("Prop fall curve")]
    public AnimationCurve propFallCurveEditable = new AnimationCurve 
    (
        new Keyframe (0f, 0f),
        new Keyframe (0.75f, 1f),
        new Keyframe (0.8f, 0.8f),
        new Keyframe (0.85f, 1f),
        new Keyframe (0.9f, 0.9f),
        new Keyframe (1f, 1f)
    );
    
    //private static bool initialized = false;
    public static AnimationCurve propFallCurve;

    private void Awake ()
    {
        ins = this;
        propFallCurve = propFallCurveEditable;
    }

    public static void UpdatePropAnimation (float timeRequested, bool conservativeSampling)
    {        
        if (!AreaManager.IsECSSafe ())
            return;
        
        var am = CombatSceneHelper.ins.areaManager;
        var placements = am.placementsProps;

        for (int i = 0, count = placements.Count; i < count; ++i)
        {
            var placement = placements[i];
            placement.UpdatePackedPropertiesForTime (timeRequested, conservativeSampling);
        }
    }
    
    public static void OnRemoval
    (
        AreaPlacementProp prop,
        bool removed
    )
    {
        if (ins == null || prop == null || prop.entitiesChildren == null || prop.prototype == null)
            return;

        if (prop.removed == removed)
            return;

        prop.removed = removed;
        
        if (prop.instanceCollision != null)
            prop.instanceCollision.enabled = !prop.removed && !prop.destroyed;
        
        if (removed)
        {
            prop.removalTime = GetSimulationTime ();
        }
        else
        {
            prop.removalTime = -100f;
        }
    }
    
    public static void OnReveal
    (
        AreaPlacementProp prop,
        bool revealed,
        float delay,
        Vector3 startingOffset = default
    )
    {
        if (ins == null || prop == null || prop.entitiesChildren == null || prop.prototype == null)
            return;
        
        if (prop.revealed == revealed)
            return;

        prop.revealed = revealed;
        
        if (revealed)
        {
            prop.revealTime = GetSimulationTime () + delay;
        }
        else
        {
            prop.revealTime = -100f;
        }
    }
    
    private static float GetSimulationTime ()
    {
        return 0f;
    }
}