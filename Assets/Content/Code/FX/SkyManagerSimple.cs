using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class SkyManagerSimple : MonoBehaviour
{
	[Serializable]
	public class SkyAmbientHarmonicsLight
	{
		public Vector4 vector;
		public Color color;
	}
	
	[Serializable]
    public class SkyPreset
    {
	    [PropertyRange (0f, 24f)]
        public float hour;
        
        public Vector3 lightRotation;
        public Color lightColor;
        public float lightIntensity;
        
        public Color ambientColor;
        public float ambientIntensity;
        public Color renderFogColor;

        public Color shaderFogColor;
        public Color shaderGroundColor;
        public Color shaderAmbientColor;

        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<SkyAmbientHarmonicsLight> ambientHarmonicsLights;
    }

	[BoxGroup ("References")]
	public Material skyboxMaterial;

	[BoxGroup ("References")]
	public Light light;
	
	[BoxGroup ("References")]
	public TOD_RingControl ring; 
	
	[BoxGroup ("References")]
	public ReflectionProbe probe;

	[BoxGroup ("References")]
	public TOD_SkyOverlayControl overlay;

	[OnValueChanged ("ApplyHourCurrent")]
	public bool fog;
	
	[BoxGroup ("Presets")]
	[OnValueChanged ("ApplyHourCurrent")]
	[PropertyRange (0f, 24f)]
	public float hour = 0f;
	
    [BoxGroup ("Presets")]
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<SkyPreset> presets = new List<SkyPreset> ();
    
    private int presetIndexCurrent = 0;
    
    private static int ID_FogColor     = Shader.PropertyToID("TOD_FogColor");
    private static int ID_GroundColor  = Shader.PropertyToID("TOD_GroundColor");
    private static int ID_AmbientColor = Shader.PropertyToID("TOD_AmbientColor");

    private static SkyPreset presetFallbackEmpty = new SkyPreset ();
    public static SkyManagerSimple ins = null;
    private static bool init = false;
    
    private void CheckInit ()
    {
	    if (init)
		    return;

	    init = true;
	    ins = this;
    }

    private void Update ()
    {
	    CheckInit ();
    }

    public bool IsNight ()
    {
	    return hour < 4.5f || hour > 19.5f;
    }

    [ButtonGroup ("BtPresets"), PropertyOrder (-1)]
    [Button ("Apply current hour", ButtonSizes.Large)]
    private void ApplyHourCurrent ()
    {
	    presetIndexCurrent = Mathf.RoundToInt (hour % 24f);
	    ApplyPresetCurrent ();
    }

    public void ApplyPresetCurrent ()
    {
	    CheckInit ();
	    
	    if (presets == null)
		    return;
	    
	    int count = presets.Count;
		if (presetIndexCurrent < 0 || presetIndexCurrent >= count)
			return;

		var preset = presets[presetIndexCurrent];
		if (preset == null)
			return;
		
		var sh = new SphericalHarmonicsL2 ();
		if (preset.ambientHarmonicsLights != null)
		{
			foreach (var shl in preset.ambientHarmonicsLights)
			{
				if (shl == null)
					continue;

				var dir = new Vector3 (shl.vector.x, shl.vector.y, shl.vector.z);
				var scale = shl.vector.w;
				sh.AddDirectionalLight (dir, shl.color, scale);
			}
		}

		RenderSettings.ambientMode = AmbientMode.Skybox;
		RenderSettings.ambientIntensity = preset.ambientIntensity;
		RenderSettings.ambientLight = preset.ambientColor;
		RenderSettings.ambientProbe = sh;

		RenderSettings.fog = fog;
		RenderSettings.fogDensity = 0.001f;
		RenderSettings.fogColor = preset.renderFogColor;

		Shader.SetGlobalColor (ID_FogColor, preset.shaderFogColor);
		Shader.SetGlobalColor (ID_GroundColor, preset.shaderGroundColor);
		Shader.SetGlobalColor (ID_AmbientColor, preset.shaderAmbientColor);

		if (light != null)
		{
			light.transform.rotation = Quaternion.Euler (preset.lightRotation);
			light.color = preset.lightColor;
			light.intensity = preset.lightIntensity;
		}

		RenderSettings.reflectionIntensity = 1f;
		RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
		RenderSettings.skybox = skyboxMaterial;
		RenderSettings.sun = light;

		if (ring != null)
			ring.UpdateToHour (preset.hour);

		if (probe != null)
		{
			probe.RenderProbe ();
		}
		
		if (overlay != null)
			overlay.UpdateToHour (preset.hour, preset.lightColor, preset.lightIntensity);
    }
}
