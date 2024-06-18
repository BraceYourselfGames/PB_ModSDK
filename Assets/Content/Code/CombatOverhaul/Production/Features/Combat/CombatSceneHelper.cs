using System;
using System.Collections;
using System.Collections.Generic;
using Area;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class CombatSceneHelper : MonoBehaviour
{
    public static CombatSceneHelper ins;
    public GameObject reflectionProbeHolder;
    private bool active = false;

    public AreaManager areaManager;
    public AreaSegmentHelper segmentHelper;
    public AreaFieldHelper fieldHelper;
    public ProceduralMeshBoundaryV2 boundary;
    public ProceduralMeshTerrainV2 terrain;
    public AreaBackgroundHelper background;
    public AreaLightHelper ambientLight;
    public CombatMaterialHelper materialHelper;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    private bool initialized = false;
    
    [NonSerialized]
    private static AnimationCurve fallbackBoundaryCurve = 
        new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
    
    private void Awake ()
    {
        Setup ();
    }

    private void OnEnable ()
    {
        Setup ();
    }

    private void Setup ()
    {
        if (initialized)
            return;

        initialized = true;
        ins = this;
        
        if (Application.isPlaying)
            background.Setup ();
    }

    public static bool IsActive ()
    {
        if (!Application.isPlaying || ins == null)
            return false;
        return ins.active;
    }

    public static void SetActive (bool active)
    {
        if (!Application.isPlaying || ins == null)
            return;

        ins.SetActiveDirectly (active);
    }

    public void SetActiveDirectly (bool active)
    {
        this.active = active;
        reflectionProbeHolder.gameObject.SetActive (active);  
        
        if (!active)
        {
            areaManager.UnloadArea (false);
            background.gameObject.SetActive (false);
            if (ambientLight != null)
                ambientLight.gameObject.SetActive (false);
            boundary.gameObject.SetActive (false);
            terrain.gameObject.SetActive (false);
            segmentHelper.gameObject.SetActive (false);
            // Debug.LogWarning ("Unloading combat environment");
        }
        else
        {
            Co.DelayFrames (10, RenderProbes);
        }
    }

    public void RenderProbes ()
    {
        if (reflectionProbeHolder == null)
            return;
        
        var probes = reflectionProbeHolder.GetComponentsInChildren<ReflectionProbe> ();
        foreach (var probe in probes)
        {
            probe.RenderProbe ();
        }
    }

    public void LoadBoundary (DataBlockEnvironmentBoundary boundaryCustom)
    {
        var bh = boundary;
        var ic = bh.interpolationCurves;
        
        if (boundaryCustom != null)
        {
            // Custom data
            bh.UpdateInterpolationBounds ();
            var offset = boundaryCustom.curveBoundsOffset;
            var height = boundaryCustom.curveBoundsHeight;
            var ground = boundaryCustom.groundHeight;
            var size = boundaryCustom.innerSkirtSize;

            bh.innerSkirtSize = Mathf.Clamp (size, 3f, 150f);
            ic.boundsCurve.extents = bh.interpolationCurves.boundsLevel.extents + new Vector3 (offset, height, offset);
            ic.groundHeight = ground;
            ic.northernCurve = boundaryCustom.north != null && boundaryCustom.north.curve != null ? boundaryCustom.north.curve : fallbackBoundaryCurve;
            ic.southernCurve = boundaryCustom.south != null && boundaryCustom.south.curve != null ? boundaryCustom.south.curve : fallbackBoundaryCurve;
            ic.westernCurve = boundaryCustom.west != null && boundaryCustom.west.curve != null ? boundaryCustom.west.curve : fallbackBoundaryCurve;
            ic.easternCurve = boundaryCustom.east != null && boundaryCustom.east.curve != null ? boundaryCustom.east.curve : fallbackBoundaryCurve;
        }
        else
        {
            // Reasonable defaults
            bh.UpdateInterpolationBounds ();
            var offset = 100f;
            var height = 0f;
            var ground = bh.interpolationCurves.boundsLevel.center.y;
            var size = 60f;
                
            bh.innerSkirtSize = size;
            ic.boundsCurve.extents = bh.interpolationCurves.boundsLevel.extents + new Vector3 (offset, 0f, offset);
            ic.groundHeight = ground;
            ic.northernCurve = ic.southernCurve = ic.westernCurve = ic.easternCurve = fallbackBoundaryCurve;
        }
        
        bh.RebuildFromArea ();
    }

    public void DestroyTerrainMeshes ()
    {
        var bh = boundary;
        boundary.DestroyMeshes ();
    }
}
