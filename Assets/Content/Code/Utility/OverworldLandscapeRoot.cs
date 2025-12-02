using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class OverworldLandscapeRoot : MonoBehaviour
{
    public string key;
    
    public Mesh mesh;
    public Mesh meshSkirt;

    [LabelText ("Mesh Collider (Optional)")]
    public Mesh meshCollider;

    public Texture2D textureMain;

    public Texture2D textureSplat;

    public Texture2D textureNormal;

    public Material materialCustom;

    [PropertyRange (0f, 1f), ShowIf ("@textureNormal != null")]
    public float materialNormalIntensity = 0.5f;
    
    public bool uvInverted = true;

    [PropertyRange (0.25f, 8f)]
    public float scaleHorizonal = 1f;

    [PropertyRange (0.25f, 2f)]
    public float scaleVertical = 1f;

    public bool propSupport = true;

    public List<GameObject> holdersHiddenOnTravel = new List<GameObject> ();

    public List<OverworldLandscapeSegment> segments = new List<OverworldLandscapeSegment> ();

    [Button, PropertyOrder (-1), DisableIn (PrefabKind.PrefabInstance)]
    private void Load ()
    {
        if (string.IsNullOrEmpty (key))
            return;

        OverworldLandscapeManager.TryLoadingVisual (key);
    }
}
