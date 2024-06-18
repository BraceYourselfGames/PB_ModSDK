using System;
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class TOD_RingControl : MonoBehaviour
{
    [Range (0f, 24f)]
    public float testHour = 12f;
    public bool applyTestHours = false;

    [Space]
    public AnimationCurve shadowOffsetOnX = new AnimationCurve ()
    {
        keys = new Keyframe[] 
        {
            new Keyframe (0, 0),
            new Keyframe (3, 0),
            new Keyframe (6, 0),
            new Keyframe (9, 0),
            new Keyframe (12, 0),
            new Keyframe (15, 0),
            new Keyframe (18, 0),
            new Keyframe (21, 0),
            new Keyframe (24, 0)
        }
    };

    public AnimationCurve shadowOffsetOnY = new AnimationCurve ()
    {
        keys = new Keyframe[]
        {
            new Keyframe (0, 0),
            new Keyframe (3, 0),
            new Keyframe (6, 0),
            new Keyframe (9, 0),
            new Keyframe (12, 0),
            new Keyframe (15, 0),
            new Keyframe (18, 0),
            new Keyframe (21, 0),
            new Keyframe (24, 0)
        }
    };
    public AnimationCurve ringBrightness = new AnimationCurve ()
    {
        keys = new Keyframe[]
        {
            new Keyframe (0, 16),
            new Keyframe (3, 16),
            new Keyframe (6, 16),
            new Keyframe (9, 16),
            new Keyframe (12, 16),
            new Keyframe (15, 16),
            new Keyframe (18, 16),
            new Keyframe (21, 16),
            new Keyframe (24, 16)
        }
    };

    public bool updateFromSkySystem = true;
    
    public float ringBrightnessMultiplier = 1;

    [Space]
    public MeshRenderer ringRenderer;
    private MaterialPropertyBlock mpb;
    private int idShadowOffsetX;
    private int idShadowOffsetY;
    private int idBrightness;

    private float hourLast = -1f;

    [Button ("Clear material")]
    private void ClearMaterialPropertyBlock ()
    {
        if (ringRenderer == null)
            return;

        CheckMPB ();

        mpb.Clear ();
        ringRenderer.SetPropertyBlock (mpb);
    }

    public void UpdateToHour (float hour, bool checkLastTime = true)
    {
        if (checkLastTime)
        {
            if (hour.RoughlyEqual (hourLast))
                return;
        }

        hourLast = hour;
        
        float shadowOffsetXValue = shadowOffsetOnX.Evaluate (hour);
        float shadowOffsetYValue = shadowOffsetOnY.Evaluate (hour);
        float brightnessValue = ringBrightness.Evaluate (hour) * ringBrightnessMultiplier;

        CheckMPB ();

        mpb.SetFloat (idShadowOffsetX, shadowOffsetXValue);
        mpb.SetFloat (idShadowOffsetY, shadowOffsetYValue);
        mpb.SetFloat (idBrightness, brightnessValue);

        ringRenderer.SetPropertyBlock (mpb);
    }

    private void CheckMPB ()
    {
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock ();
            idShadowOffsetX = Shader.PropertyToID ("_ShadowOffsetX");
            idShadowOffsetY = Shader.PropertyToID ("_ShadowOffsetY");
            idBrightness = Shader.PropertyToID ("_Brightness");
        }
    }
}
