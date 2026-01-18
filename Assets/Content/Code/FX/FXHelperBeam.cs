using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEngine;

public class FXHelperBeam : MonoBehaviour
{
    public static MaterialPropertyBlock mpb;

    public DataBlockColorInterpolated colorOverride;
    
    public Color colorFrom = new Color (0.0f, 0.0f, 0.0f);
    public Color colorTo = new Color (1.0f, 0.0f, 0.0f);

    public GameObject holderScaled;
    public GameObject holderFlare;
    public MeshRenderer rendererFlare;

    public float flareSizeMultiplier = 1f;
    public float fadeInDuration = 0.1f;
    public float fadeOutDuration = 0.5f;
    public ParticleSystem systemFlare;
    public ParticleSystem systemEmbers;
    
    public List<Renderer> renderers;
}
