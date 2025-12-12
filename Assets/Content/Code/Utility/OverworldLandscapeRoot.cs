using System.Collections.Generic;
using System.IO;
using PhantomBrigade.Data;
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

    [Button, PropertyOrder (-1), HideIn (PrefabKind.PrefabInstance)]
    private void Load ()
    {
        OverworldLandscapeManager.TryLoadingVisual (key);
    }
    
    [Button, PropertyOrder (-1), HideIn (PrefabKind.PrefabInstance)]
    private void InjectToLandscapeManager ()
    {
        OverworldLandscapeManager.TryInjectingVisual (this);
    }
    
    [Button, PropertyOrder (-1), HideIn (PrefabKind.PrefabInstance)]
    private void OpenMeshGenerator ()
    {
        var pathSDK = DataPathHelper.GetApplicationFolder ();
        var pathTools = pathSDK + "Tools/Art";
        if (!Directory.Exists (pathTools))
        {
            Debug.LogWarning ($"Tools directory doesn't exist: make sure you have the latest version of the SDK installed.");
            return;
        }
        
        Application.OpenURL ("file://" + pathTools);
    }
}