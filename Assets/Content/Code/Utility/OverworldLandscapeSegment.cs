using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class OverworldLandscapeSegmentChild
{
    public GameObject root;
    public GameObject holderDay;
    public GameObject holderNight;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public List<Renderer> renderers = new List<Renderer> ();
}

public class OverworldLandscapeSegment : MonoBehaviour
{
    public List<OverworldLandscapeSegmentChild> children = new List<OverworldLandscapeSegmentChild> ();

    private bool isDayLast = false;
    private int stateIndexLast = 0;

    public void OnSpawn ()
    {
        if (children == null)
            return;

        foreach (var child in children)
        {
            if (child == null || child.root == null)
                continue;

            if (child.renderers == null)
                child.renderers = new List<Renderer> ();
            else
                child.renderers.Clear ();
            
            var renderers = child.root.GetComponentsInChildren<MeshRenderer> ();
            if (renderers != null && renderers.Length > 0)
                child.renderers.AddRange (renderers);
        }
        
        #if !PB_MODSDK
        var grounders = gameObject.GetComponentsInChildren<OverworldViewHelperGround> ();
        if (grounders != null && grounders.Length > 0)
        {
            foreach (var grounder in grounders)
                grounder.GroundAll ();
        }
        #endif

        Refresh ();
    }
    
    public void Refresh ()
    {
        #if !PB_MODSDK
        bool isDay = TOD_Sky.Instance != null && !TOD_Sky.Instance.IsNight;
        Refresh (isDay);
        #else
        Refresh (true);
        #endif
    }

    public void Refresh (bool isDay)
    {
        if (children == null)
            return;

        foreach (var child in children)
        {
            if (child == null || child.root == null)
                continue;
            
            if (child.holderDay != null)
                child.holderDay.SetActive (isDay);
            
            if (child.holderNight != null)
                child.holderNight.SetActive (!isDay);
        }
    }
}
