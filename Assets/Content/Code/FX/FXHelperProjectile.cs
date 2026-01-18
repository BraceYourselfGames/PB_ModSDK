using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

public class FXHelperProjectile : MonoBehaviour
{
    public static MaterialPropertyBlock mpb;

    private float valueDestruction;
    private float valueRange;
    private float valueThrust;

    [BoxGroup ("A", false), ToggleLeft]
    public bool animateDestruction;
    [BoxGroup ("A"), HideLabel, ShowIf ("animateDestruction")]
    public FXSystem.StepRenderer rendererDestruction;
    
    [BoxGroup ("B", false), ToggleLeft]
    public bool animateRange;
    [BoxGroup ("B"), HideLabel, ShowIf ("animateRange")]
    public FXSystem.StepRenderer rendererRange;
    
    [BoxGroup ("C", false), ToggleLeft]
    public bool animateThrust;
    [BoxGroup ("C"), HideLabel, ShowIf ("animateThrust")]
    public FXSystem.StepRenderer rendererThrust;

    public bool destructionInPlace = false;

    private bool primingUsed = false;
    public GameObject primingHolder;

    private void Awake ()
    {
        Initialize ();

        primingUsed = primingHolder != null;
        if (primingUsed)
            OnPrimed (false);
    }
    
    public void Initialize ()
    {
        rendererDestruction.InitializeWithoutParent ();
        rendererRange.InitializeWithoutParent ();
        rendererThrust.InitializeWithoutParent ();
    }

    public void SetDestruction (float valueDestruction)
    {
        this.valueDestruction = Mathf.Clamp01 (valueDestruction);
        Refresh ();
    }
    
    public void SetRange (float valueRange)
    {
        this.valueRange = Mathf.Clamp01 (valueRange);
        Refresh ();
    }
    
    public void SetThrust (float valueThrust)
    {
        this.valueThrust = Mathf.Clamp01 (valueThrust);
        Refresh ();
    }
    
    public void SetAll (float valueDestruction, float valueRange, float valueThrust)
    {
        this.valueDestruction = Mathf.Clamp01 (valueDestruction);
        this.valueRange = Mathf.Clamp01 (valueRange);
        this.valueThrust = Mathf.Clamp01 (valueThrust);
        Refresh ();
    }

    private void Refresh ()
    {
        var valueDestructionInv = 1f - valueDestruction;
        
        if (animateDestruction && rendererDestruction != null)
            rendererDestruction.Animate (valueDestruction);
        
        if (animateRange && rendererRange != null)
            rendererRange.Animate (valueRange * valueDestructionInv);
        
        if (animateThrust && rendererThrust != null)
            rendererThrust.Animate (valueThrust * valueDestructionInv);
    }

    public void OnPrimed (bool primed)
    {
        if (primingUsed)
            primingHolder.SetActive (primed);
    }
}
