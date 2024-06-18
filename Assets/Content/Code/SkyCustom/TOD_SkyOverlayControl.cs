using UnityEngine;
using Sirenix.OdinInspector;

public class TOD_SkyOverlayControl : MonoBehaviour
{
    [ReadOnly, ShowInInspector, PropertyRange (0f, 1f)]
    private float interpolantLocal = 0f; 
    
    [ReadOnly, ShowInInspector, PropertyRange (0f, 1f)]
    private float interpolant = 0f;
    
    public Gradient gradientSky = new Gradient
    {
        alphaKeys = new []
        {
            new GradientAlphaKey (1f, 0f),
            new GradientAlphaKey (1f, 0.25f),
            new GradientAlphaKey (1f, 0.5f),
            new GradientAlphaKey (1f, 0.75f),
            new GradientAlphaKey (1f, 1f)
        },
        colorKeys = new []
        {
            new GradientColorKey (Color.white, 0f),
            new GradientColorKey (Color.white, 0.25f),
            new GradientColorKey (Color.white, 0.5f),
            new GradientColorKey (Color.white, 0.75f),
            new GradientColorKey (Color.white, 1f)
        }
    };
    
    public Gradient gradientHorizon = new Gradient
    {
        alphaKeys = new []
        {
            new GradientAlphaKey (1f, 0f),
            new GradientAlphaKey (1f, 0.25f),
            new GradientAlphaKey (1f, 0.5f),
            new GradientAlphaKey (1f, 0.75f),
            new GradientAlphaKey (1f, 1f)
        },
        colorKeys = new []
        {
            new GradientColorKey (Color.white, 0f),
            new GradientColorKey (Color.white, 0.25f),
            new GradientColorKey (Color.white, 0.5f),
            new GradientColorKey (Color.white, 0.75f),
            new GradientColorKey (Color.white, 1f)
        }
    };

    public Gradient gradientGround = new Gradient
    {
        alphaKeys = new []
        {
            new GradientAlphaKey (1f, 0f),
            new GradientAlphaKey (1f, 0.25f),
            new GradientAlphaKey (1f, 0.5f),
            new GradientAlphaKey (1f, 0.75f),
            new GradientAlphaKey (1f, 1f)
        },
        colorKeys = new []
        {
            new GradientColorKey (Color.white, 0f),
            new GradientColorKey (Color.white, 0.25f),
            new GradientColorKey (Color.white, 0.5f),
            new GradientColorKey (Color.white, 0.75f),
            new GradientColorKey (Color.white, 1f)
        }
    };
    
    [ReadOnly, ShowInInspector, PropertyRange (0f, 1f)]
    private float lightMultiplier = 0f;

    public AnimationCurve lightCurve = new AnimationCurve
    (
        new Keyframe (0f, 1f),
        new Keyframe (0.4f, 1f),
        new Keyframe (0.5f, 0f),
        new Keyframe (0.6f, 1f),
        new Keyframe (1f, 1f)
    );

    [ShowInInspector, BoxGroup ("Timing")]
    [LabelText ("Hour")]
    [PropertyRange (0f, 24f)]
    public float timeCurrent = 0f;
    
    [ShowInInspector, BoxGroup ("Timing")]
    [LabelText ("Sunrise/Sunset")]
    public Vector2 timeRangeDay = new Vector2 (4.9f, 19.3f);

    [ShowInInspector, BoxGroup ("Timing")]
    public float timeSunriseOffset = 0f;
    
    [ShowInInspector, BoxGroup ("Timing")]
    public float timeSunsetOffset = 0f;
    
    [ShowInInspector, BoxGroup ("Timing")]
    private bool timeIsNight
    {
        get
        {
            var time = timeCurrent % 24;
            var sunrise = timeRangeDay.x;
            var sunset = timeRangeDay.y;
            var night = time < (sunrise + timeSunriseOffset) || time > (sunset + timeSunsetOffset);
            return night;
        }
    }

    public void UpdateToHour (float hour, Color lightColor, float lightIntensity)
    {
        var time = hour % 24f;
        timeCurrent = hour % 24f;

        var sunrise = timeRangeDay.x;
        var sunset = timeRangeDay.y;
        var night = time < (sunrise + timeSunriseOffset) || time > sunset + timeSunsetOffset;

        lightMultiplier = 0f;
        interpolant = 0f;

        // Sample left side of the gradient
        if (time <= sunrise)
        {
            interpolantLocal = time / Mathf.Max (0.01f, sunrise);
            interpolant = interpolantLocal * 0.5f;
        }
        else if (time < sunset)
        {
            var midpoint = (sunset + sunrise) * 0.5f;
            if (time < midpoint)
            {
                interpolantLocal = (time - sunrise) / Mathf.Max (0.01f, midpoint - sunrise);
                interpolant = interpolantLocal * 0.5f + 0.5f;
            }
            else
            {
                interpolantLocal = 1f - (time - midpoint) / Mathf.Max (0.01f, sunset - midpoint);
                interpolant = interpolantLocal * 0.5f + 0.5f;
            }
        }
        else
        {
            interpolantLocal = 1f - (time - sunset) / Mathf.Max (0.01f, 24f - sunset);
            interpolant = interpolantLocal * 0.5f;
        }
        
        var colorSky = gradientSky.Evaluate (interpolant);
        var colorHorizon = gradientHorizon.Evaluate (interpolant);
        var colorGround = gradientGround.Evaluate (interpolant);
        lightMultiplier = lightCurve.Evaluate (interpolant);
        
        Shader.SetGlobalColor ("TOD_OverlayColorSky", colorSky);
        Shader.SetGlobalColor ("TOD_OverlayColorHorizon", colorHorizon);
        Shader.SetGlobalColor ("TOD_OverlayColorGround", colorGround);
        Shader.SetGlobalFloat ("TOD_OverlaySunIntensity", lightMultiplier);
        Shader.SetGlobalFloat ("TOD_NightTimeSwitch", (night ? 1f : 0f));
        Shader.SetGlobalColor ("TOD_SkyLightColor", lightColor * Mathf.Clamp (lightIntensity, 0.3f, 1.0f));
        Shader.SetGlobalFloat ("TOD_NightToNoonInterpolant", interpolant);

    }

    [Button, PropertyOrder (-1)]
    private void ResetGradients ()
    {
        gradientSky = new Gradient
        {
            alphaKeys = new []
            {
                new GradientAlphaKey (1f, 0f),
                new GradientAlphaKey (1f, 0.25f),
                new GradientAlphaKey (1f, 0.5f),
                new GradientAlphaKey (1f, 0.75f),
                new GradientAlphaKey (1f, 1f)
            },
            colorKeys = new []
            {
                new GradientColorKey (Color.white, 0f),
                new GradientColorKey (Color.white, 0.25f),
                new GradientColorKey (Color.white, 0.5f),
                new GradientColorKey (Color.white, 0.75f),
                new GradientColorKey (Color.white, 1f)
            }
        };
        
        gradientHorizon = new Gradient
        {
            alphaKeys = new []
            {
                new GradientAlphaKey (1f, 0f),
                new GradientAlphaKey (1f, 0.25f),
                new GradientAlphaKey (1f, 0.5f),
                new GradientAlphaKey (1f, 0.75f),
                new GradientAlphaKey (1f, 1f)
            },
            colorKeys = new []
            {
                new GradientColorKey (Color.white, 0f),
                new GradientColorKey (Color.white, 0.25f),
                new GradientColorKey (Color.white, 0.5f),
                new GradientColorKey (Color.white, 0.75f),
                new GradientColorKey (Color.white, 1f)
            }
        };
        
        gradientGround = new Gradient
        {
            alphaKeys = new []
            {
                new GradientAlphaKey (1f, 0f),
                new GradientAlphaKey (1f, 0.25f),
                new GradientAlphaKey (1f, 0.5f),
                new GradientAlphaKey (1f, 0.75f),
                new GradientAlphaKey (1f, 1f)
            },
            colorKeys = new []
            {
                new GradientColorKey (Color.white, 0f),
                new GradientColorKey (Color.white, 0.25f),
                new GradientColorKey (Color.white, 0.5f),
                new GradientColorKey (Color.white, 0.75f),
                new GradientColorKey (Color.white, 1f)
            }
        };
    }
}
