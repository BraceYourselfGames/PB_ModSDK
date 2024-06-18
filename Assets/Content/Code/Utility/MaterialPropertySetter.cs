using UnityEngine;

[ExecuteInEditMode]
public class MaterialPropertySetter : MonoBehaviour
{
    public static MaterialPropertyBlock mpbEnvironmentLights;
    public static MaterialPropertyBlock mpbMechParts;

    private MeshRenderer[] mr;
    //private bool started = false;

    public float emissionIntensity = 0;
    public Color emissionColor = Color.white;

    public Color colorPrimary = Color.gray;
    public Color colorSecondary = Color.gray;
    public Color colorTertiary = Color.gray;
    public Color colorBackground = Color.black;
    public Color colorMarkings = Color.white;
    public Texture paintTex;
    public float paintIntensity = 1f;
    public float damage = 1f;

    public enum Mode
    {
        EnvironmentLights,
        MechPart
    }
    public Mode mode = Mode.EnvironmentLights;

    private void OnEnable ()
    {
        UpdateProperties ();
    }

    private void Awake ()
    {
        UpdateProperties ();
    }

    private void Start ()
    {
        UpdateProperties ();
    }

    [ContextMenu ("Update")]
    public void UpdateProperties ()
    {
        if (Utilities.isPlaymodeChanging)
            return;

        //started = true;

        bool rebuildRequired = false;
        if (mr != null)
        {
            for (int i = 0; i < mr.Length; ++i)
            {
                if (mr[i] == null)
                {
                    rebuildRequired = true;
                    break;
                }
            }
        }

        if (mr == null || rebuildRequired)
            mr = gameObject.GetComponentsInChildren<MeshRenderer> ();

        if (mr == null)
            return;

        if (mr.Length == 0)
            return;

        if (mode == Mode.EnvironmentLights)
        {
            if (mpbEnvironmentLights == null)
                mpbEnvironmentLights = new MaterialPropertyBlock ();

            mpbEnvironmentLights.SetColor ("_EmissionColor", emissionColor);
            mpbEnvironmentLights.SetFloat ("_EmissionIntensity", emissionIntensity);

            for (int i = 0; i < mr.Length; ++i)
            {
                if (mr[i] != null)
                    mr[i].SetPropertyBlock (mpbEnvironmentLights);
            }
        }

        else if (mode == Mode.MechPart)
        {
            if (mpbMechParts == null)
                mpbMechParts = new MaterialPropertyBlock ();

            mpbMechParts.SetColor ("_ColorPrimary", colorPrimary);
            mpbMechParts.SetColor ("_ColorSecondary", colorSecondary);
            mpbMechParts.SetColor ("_ColorTertiary", colorTertiary);
            mpbMechParts.SetColor ("_ColorBackground", colorBackground);
            mpbMechParts.SetColor ("_ColorMarkings", colorMarkings);
            mpbMechParts.SetFloat ("_PaintIntensity", paintIntensity);
            mpbMechParts.SetFloat ("_Damage", damage);

            if (paintTex != null)
                mpbMechParts.SetTexture ("_PaintTex", paintTex);

            for (int i = 0; i < mr.Length; ++i)
            {
                if (mr[i] != null)
                    mr[i].SetPropertyBlock (mpbMechParts);
            }
        }
    }
}

