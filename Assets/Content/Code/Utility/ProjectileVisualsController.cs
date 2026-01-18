using PhantomBrigade.Data;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class ProjectileVisualsController : MonoBehaviour
{
    const string updVisualsFunction = "UpdateVisuals";

    [OnValueChanged(updVisualsFunction)]
    public MeshRenderer TracerMeshRenderer;

    [ShowInInspector]
    public DataBlockColorInterpolated colorOverride;

    [OnValueChanged(updVisualsFunction)]
    // [ColorUsage (true, true)]
    public Color ColorFront = new Color (0.0f, 0.0f, 0.0f);

    [OnValueChanged(updVisualsFunction)]
    // [ColorUsage (true, true)]
    public Color ColorTail = new Color (1.0f, 0.0f, 0.0f);

    [OnValueChanged(updVisualsFunction), Range(-5.0f, 5.0f)]
    public float ColorFrontOffset = 0.0f;
    
    [OnValueChanged(updVisualsFunction), Range(-5.0f, 5.0f)]
    public float ColorTailOffset = 0.0f;

    [OnValueChanged(updVisualsFunction), Range(1.0f, 64.0f)]
    public float EmissionIntensity = 10.0f;

    [Space(5)]
    [OnValueChanged(updVisualsFunction), Range(0.0f, 1.0f)]
    public float BulletPartOffset = 0.25f;
    
    [OnValueChanged(updVisualsFunction), Range(0.0f, 1.0f)]
    public float BulletPartDarkness = 0.95f;

    [Space(5)]
    [OnValueChanged(updVisualsFunction), Range(-15.0f, 15.0f)]
    public float TailLength = 0.0f;

    [Space(5)]
    public bool RandomizeRotationOnZ = false;

    private bool initialized = false;
    private MaterialPropertyBlock _propBlock;
    private float randomValuePerRenderer;

    private static int propertyID_Power = Shader.PropertyToID ("_Power");
    private static int propertyID_MaskReveal = Shader.PropertyToID ("_MaskReveal");
    private float lastMaskRevealValue = 0;

    [Button("Apply Settings Manually")]
    public void UpdateVisuals()
    {
        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        if (TracerMeshRenderer != null)
        {
            TracerMeshRenderer.GetPropertyBlock(_propBlock);

            var colorFront = ColorFront;
            var colorTail = ColorTail;

            if (colorOverride != null)
            {
                colorFront = colorOverride.colorFrom;
                colorTail = colorOverride.colorTo;
            }

            _propBlock.SetColor("_ColorFront", colorFront);
            _propBlock.SetColor("_ColorTail", colorTail);
            _propBlock.SetFloat("_ColorFrontOffset", ColorFrontOffset);
            _propBlock.SetFloat("_ColorTailOffset", ColorTailOffset);
            _propBlock.SetFloat("_EmissionIntensity", EmissionIntensity);
            _propBlock.SetFloat("_BulletPartOffset", BulletPartOffset);
            _propBlock.SetFloat("_BulletPartDarkness", BulletPartDarkness);
            _propBlock.SetFloat("_TailLength", TailLength);

            randomValuePerRenderer = Random.Range (0.0f, 1.0f);
            _propBlock.SetFloat("_randomValuePerRenderer", randomValuePerRenderer);

            TracerMeshRenderer.SetPropertyBlock(_propBlock);

            if (RandomizeRotationOnZ)
                gameObject.transform.localRotation = Quaternion.Euler (0, 0, Random.Range (-180.0f, 180.0f));

            initialized = true;
        }
    }
    
    public void UpdateMaskReveal (float distance)
    {
        float projectileSize = TracerMeshRenderer.bounds.size.z; 
        float projectileDistance = Mathf.Clamp01 (distance / projectileSize);
        
        if (lastMaskRevealValue.Equals (projectileDistance))
            return;
        
        lastMaskRevealValue = projectileDistance;
        
        TracerMeshRenderer.GetPropertyBlock (_propBlock);
        _propBlock.SetFloat (propertyID_MaskReveal, projectileDistance);
        TracerMeshRenderer.SetPropertyBlock (_propBlock);
    }
    
    
    // Use Awake for runtime
    void Awake()
    {
        UpdateVisuals ();
    }

    // Utilize OnEnable call only in-editor to avoid frame hitches
    // when lots of bullets become visible and need their settings applied
    void OnEnable()
    {
        if (Application.isPlaying)
            return;
    
        if (!initialized)
            UpdateVisuals ();
    }

}
