using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AssetLinkerCombatStrike : MonoBehaviour
{
    [Serializable]
    public class FiringLink
    {
        public string key;
        public Transform transform;
    }

    public List<FiringLink> firingLinks = new List<FiringLink> ();
    
    [ShowInInspector, ReadOnly]
    public Dictionary<string, Transform> firingTransforms = new Dictionary<string, Transform> ();

    public ParticleSystem particleSystem;
    
    #if !PB_MODSDK
    public List<Ara.AraTrail> trails;

    [HideInEditorMode, HideInPrefabAssets, HideInPrefabInstances]
    [Button (ButtonSizes.Medium), PropertyOrder (-1)]
    public void Setup ()
    {
        firingTransforms.Clear ();
        for (int i = 0; i < firingLinks.Count; ++i)
        {
            var link = firingLinks[i];
            if (link.transform != null && !string.IsNullOrEmpty (link.key) && !firingTransforms.ContainsKey (link.key))
                firingTransforms.Add (link.key, link.transform);
        }
    }

    public void Play (bool clearPS = false)
    {
        if (particleSystem != null)
            particleSystem.SetSystemPlaying (true, false, clearPS);
        
        if (trails != null)
        {
            foreach (var trail in trails)
            {
                trail.Clear ();
                trail.emit = true;
            }
        }
    }
    
    public void Stop (bool clearPS = false)
    {
        if (particleSystem != null)
            particleSystem.SetSystemPlaying (false, true, clearPS);

        if (trails != null)
        {
            foreach (var trail in trails)
            {
                trail.emit = false;
                if (clearPS)
                    trail.Clear ();
            }
        }
    }
    #endif
}
