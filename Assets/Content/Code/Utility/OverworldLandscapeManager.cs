using System;
using System.Collections.Generic;
using CustomRendering;
using PhantomBrigade;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

#if !PB_MODSDK
using PhantomBrigade.Overworld.View;
#endif

[Serializable]
public class OverworldLandscapeRootLink
{
    [HideLabel, InlineButton ("Load")]
    public OverworldLandscapeRoot prefab;
    
    private void Load ()
    {
        if (prefab == null || string.IsNullOrEmpty (prefab.key))
            return;

        OverworldLandscapeManager.TryLoadingVisual (prefab.key);
    }
}


[Serializable]
public class OverworldLandscapeTile
{
    public GameObject gameObject;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public MeshRenderer meshRendererComponent;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public MeshFilter meshFilterComponent;

    [NonSerialized, ShowInInspector, ReadOnly]
    public MeshCollider meshColliderComponent;
}

[Serializable]
public class OverworldLandscapePropAsset
{
    public GameObject prefab;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public int id;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public Mesh mesh;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public Material material;
}

[ShowInInspector]
public class OverworldLandscapePropGroup
{
    public string name;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public Mesh mesh;
    
    [NonSerialized]
    public Material material;

    [ShowInInspector]
    public int count
    {
        get
        {
            return transforms != null ? transforms.Count : 0;
        }
    }
    
    [HideInInspector]
    public List<OverworldLandscapePropTransform> transforms = new List<OverworldLandscapePropTransform> ();
}

[ShowInInspector]
public struct OverworldLandscapePropTransform
{
    public Vector3 position;
    public float scale;
}

[ExecuteInEditMode]
public class OverworldLandscapeManager : MonoBehaviour
{
    [ShowInInspector, ReadOnly, InfoBox ("@GetInstanceText ()")]
    public static OverworldLandscapeManager ins;

    public GameObject holder;

    public GameObject rootHolder;
    public GameObject segmentHolder;

    public GameObject cloudsHolder;
    
    public GameObject propHolder;
    public int propLimit = 100;
    public int propGeneratorResolution = 100;
    public int propGeneratorMip = 2;
    public bool propCoordFlipX = true;
    public bool propCoordFlipZ = true;
    public bool propCoordDebug = false;

    [PropertyRange (0f, 1f)]
    public float propColorThreshold = 0.5f;
    
    public float propColorPower = 2f;

    [PropertyRange (0f, 1f)]
    public float propOffsetRandom = 1f;

    [PropertyRange (0.01f, 2f)]
    public float propScaleMin = 0.2f;
    
    [PropertyRange (0.01f, 2f)]
    public float propScaleMax = 0.3f;
    
    public float mainScale = 800;
    public Material mainMaterial;
    
    [ShowInInspector, FoldoutGroup ("Globals", true)]
    [OnValueChanged (nameof(RefreshGlobals))]
    public static bool alwaysHideRootObjects = false;
    
    [ShowInInspector, FoldoutGroup ("Globals", true)]
    [OnValueChanged (nameof(RefreshGlobals))]
    public static float shaderSpotlightSelectionRadius = 32f;

    [ShowInInspector, FoldoutGroup ("Globals", true)]
    [OnValueChanged (nameof(RefreshGlobals))]
    [PropertyRange (0f, 1f)]
    public static float shaderSpotlightRadius = 1f;
    
    [ShowInInspector, FoldoutGroup ("Globals")]
    [OnValueChanged (nameof(RefreshGlobals))]
    public static Vector3 shaderSpotlightOrigin = Vector3.zero;
    
    [ShowInInspector, FoldoutGroup ("Globals")]
    [OnValueChanged (nameof(RefreshGlobals))]
    [PropertyRange (0f, 1f)]
    public static float shaderDimensionSlice = 0f;
    
    [ShowInInspector, FoldoutGroup ("Globals")]
    [OnValueChanged (nameof(RefreshGlobals))]
    [PropertyRange (0f, 1f)]
    public static float shaderDimensionOpacity = 0f;

    [ShowInInspector, FoldoutGroup ("Generation", false)]
    public static float navSlopeLimit = 40f;
    
    [ShowInInspector, FoldoutGroup ("Generation")]
    public static Vector2 propNormalDotRange = new Vector2 (0f, 1f);

    [FoldoutGroup ("Tiles", false)]
    [OnValueChanged (nameof(UpdateAssets), true)]
    public OverworldLandscapeTile terrain = new OverworldLandscapeTile ();

    [FoldoutGroup ("Tiles", false)]
    [OnValueChanged (nameof(UpdateAssets), true)]
    public OverworldLandscapeTile terrainSkirt = new OverworldLandscapeTile ();
    
    [OnValueChanged (nameof(UpdateAssets), true)]
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<OverworldLandscapeRootLink> assets = new List<OverworldLandscapeRootLink> ();

    [OnValueChanged (nameof(UpdateAssets), true)]
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<OverworldLandscapePropAsset> propAssets = new List<OverworldLandscapePropAsset> ();
    
    [OnValueChanged (nameof(UpdateAssets), true)]
    [ShowInInspector, NonSerialized]
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<OverworldLandscapePropGroup> propPlacements = new List<OverworldLandscapePropGroup> ();
    
    [FoldoutGroup ("Lookup"), ShowInInspector, NonSerialized]
    private Dictionary<string, OverworldLandscapeRootLink> assetLookup = new Dictionary<string, OverworldLandscapeRootLink> ();
    private static MaterialPropertyBlock mpb;
    
    // private static string keyLoadedLast = null;
    private static OverworldLandscapeRoot rootInstanceLast = null;
    private static List<OverworldLandscapeSegment> segmentsActive = new List<OverworldLandscapeSegment> ();

    private static readonly int propertyID_NormalTex = Shader.PropertyToID ("_NormalTex");
    private static readonly int propertyID_NormalIntensity = Shader.PropertyToID ("_NormalIntensity");
    private static readonly int propertyID_UVScale = Shader.PropertyToID ("_UVScale");

    private static readonly int propertyID_GlobalLandscapeMainTex = Shader.PropertyToID ("_GlobalLandscapeMainTex");
    private static readonly int propertyID_GlobalLandscapeSplatTex = Shader.PropertyToID ("_GlobalLandscapeSplatTex");
    
    private static readonly int propertyID_GlobalLandscapeSelectionData = Shader.PropertyToID ("_GlobalLandscapeSelectionData");
    private static readonly int propertyID_GlobalLandscapeSpotlightData = Shader.PropertyToID ("_GlobalLandscapeSpotlightData");
    private static readonly int propertyID_GlobalLandscapeDimensionData = Shader.PropertyToID ("_GlobalLandscapeDimensionData");

    private static int globalRefreshCount = 0;
    private static bool travelInProgressLast = false;

    private void OnEnable ()
    {
        ins = this;
        UpdateAssets ();
        RefreshGlobals ();
    }

    private void Update ()
    {
        if (enabled)
            ins = this;
    }

    private string GetInstanceText ()
    {
        if (ins == null)
            return "No singleton";

        if (ins == this)
            return "Singleton is this object";

        return "Singleton is another object!";
    }

    public static void ResetSpotlight ()
    {
        shaderSpotlightRadius = 1f;
        shaderSpotlightOrigin = Vector3.zero;
        RefreshGlobalsWithInput (shaderSpotlightRadius);
    }

    public static void AnimateSpotlight (float interpolant, float radius, Vector3 position)
    {
        float shaderSpotlightRadiusAnimated = Mathf.Lerp (shaderSpotlightRadius, radius, interpolant);
        shaderSpotlightOrigin = Vector3.Lerp (Vector3.zero, position, interpolant);
        
        // Debug.Log ($"Animating spotlight: lerp ({shaderSpotlightRadius} <-> {radius}, {interpolant})");
        
        RefreshGlobalsWithInput (shaderSpotlightRadiusAnimated);
    }

    public static void OnTravelEnd ()
    {
        ResetSpotlight ();
        
        if (rootInstanceLast != null)
        {
            if (rootInstanceLast.holdersHiddenOnTravel != null)
            {
                foreach (var holder in rootInstanceLast.holdersHiddenOnTravel)
                    holder.SetActive (true);
            }
        }
    }

    private void UpdateAssets ()
    {
        assetLookup.Clear ();
        
        if (assets != null)
        {
            foreach (var link in assets)
            {
                if (link.prefab == null || string.IsNullOrEmpty (link.prefab.key))
                    continue;

                assetLookup[link.prefab.key] = link;

                // if (asset == null || string.IsNullOrEmpty (asset.key))
                //     continue;

                // assetLookup[asset.key] = asset;
                // asset.scaleHorizonal = Mathf.Clamp (asset.scaleHorizonal, 0.25f, 8f);
                // asset.scaleVertical = Mathf.Clamp (asset.scaleVertical, 0.25f, 2f);
            }
        }
        
        /*
        // Allow mods to inject additional visuals
        if (Application.isPlaying && ModManager.AreModsActive ())
        {
            var prefabExtension = ".prefab";
            foreach (var modLoadedData in ModManager.loadedMods)
            {
                if (modLoadedData.assetBundles == null || !modLoadedData.metadata.includesAssetBundles)
                    continue;
                
                foreach (var assetBundle in modLoadedData.assetBundles)
                {
                    var assetPaths = assetBundle.GetAllAssetNames ();
                    foreach (var assetPath in assetPaths)
                    {
                        if (!assetPath.EndsWith (prefabExtension))
                            continue;

                        var asset = assetBundle.LoadAsset (assetPath);
                        if (asset == null || asset is GameObject == false)
                            continue;

                        var prefab = (GameObject)asset;
                        var component = prefab.GetComponent<OverworldLandscapeRoot> ();
                        if (component == null)
                            continue;

                        var visualKey = $"{assetBundle.name}/{prefab.name}";
                        var link = new OverworldLandscapeRootLink { prefab = component };
                        assetLookup[visualKey] = link;
                        Debug.Log ($"Mod {modLoadedData.metadata.id} | Loaded new landscape from asset bundle {assetBundle.name}: {prefab.name}");
                    }
                }
            }
        }
        */

        if (terrain != null && terrain.gameObject != null)
        {
            terrain.meshRendererComponent = terrain.gameObject.GetComponent<MeshRenderer> ();
            terrain.meshFilterComponent = terrain.gameObject.GetComponent<MeshFilter> ();
            terrain.meshColliderComponent = terrain.gameObject.GetComponent<MeshCollider> ();
        }

        if (terrainSkirt != null && terrainSkirt.gameObject != null)
        {
            terrainSkirt.meshFilterComponent = terrainSkirt.gameObject.GetComponent<MeshFilter> ();
        }
        
        if (propAssets != null)
        {
            foreach (var asset in propAssets)
            {
                if (asset == null || asset.prefab == null)
                    continue;

                asset.id = asset.prefab.GetInstanceID ();

                var meshFilter = asset.prefab.GetComponent<MeshFilter> ();
                if (meshFilter != null)
                    asset.mesh = meshFilter.sharedMesh;
                
                var meshRenderer = asset.prefab.GetComponent<MeshRenderer> ();
                if (meshRenderer != null)
                    asset.material = meshRenderer.sharedMaterial;
            }
        }
    }

    [Button, PropertyOrder (-1)]
    public void GenerateProps ()
    {
        if (propPlacements == null)
            propPlacements = new List<OverworldLandscapePropGroup> ();
        else
            propPlacements.Clear ();
        
        if (propHolder == null)
            return;

        if (rootInstanceLast == null)
        {
            Debug.LogWarning ($"Can't generate props for landscape, no root instance found");
            return;
        }

        string keyLast = rootInstanceLast.key;
        if (string.IsNullOrEmpty (keyLast))
            return;

        var textureSplat = rootInstanceLast.textureSplat;
        if (textureSplat == null)
        {
            Debug.LogWarning ($"Can't generate props for landscape {keyLast}, no splat map found");
            return;
        }
        
        var bounds = GetBounds ();
        if (bounds.x <= 0f || bounds.z <= 0f)
        {
            Debug.LogWarning ($"Can't generate props for landscape {keyLast}, no valid bounds");
            return;
        }

        if (!textureSplat.isReadable)
        {
            Debug.LogWarning ($"Can't generate props for landscape {keyLast}, splat texture is not readable", textureSplat);
            return;
        }

        float radius = Mathf.Min (bounds.x, bounds.z) * 0.5f;
        float radiusSqr = radius * radius;
        var propPlacementLookup = new Dictionary<int, OverworldLandscapePropGroup> ();

        propLimit = Mathf.Clamp (propLimit, 10, 12000);
        propGeneratorResolution = Mathf.Clamp (propGeneratorResolution, 16, 1024);
        propGeneratorMip = Mathf.Clamp (propGeneratorMip, 0, 4);
        
        int limitMinusOne = propGeneratorResolution - 1;

        Color32[] colorArray = textureSplat.GetPixels32 (propGeneratorMip);
        int mipDivider = propGeneratorMip <= 0 ? 1 : Mathf.RoundToInt (Mathf.Pow (2, propGeneratorMip));

        int textureWidth = textureSplat.width / mipDivider;
        int textureWidthMinusOne = textureWidth - 1;
        int textureHeightMinusOne = textureSplat.height / mipDivider - 1;
        int count = 0;
        bool terminated = false;
        float offsetScale = radius / propGeneratorResolution;
        
        Debug.Log ($"Generating props on landscape {keyLast} | Radius: {radius:F0} | Prop limit: {propLimit} | Prop resolution: {propGeneratorResolution} | Splat mip: {propGeneratorMip} | Texture size: {textureWidth}");
        
        for (int xIndex = 0; xIndex < propGeneratorResolution; ++xIndex)
        {
            for (int zIndex = 0; zIndex < propGeneratorResolution; ++zIndex)
            {
                // var xInterpolant = coordXOrigin + coordXMul * ((float)xIndex / limitMinusOne);
                // var zInterpolant = coordZOrigin + coordZMul * ((float)zIndex / limitMinusOne);
                
                var xInterpolant = ((float)xIndex / limitMinusOne);
                var zInterpolant = ((float)zIndex / limitMinusOne);
                
                var pos = new Vector3 (xInterpolant * bounds.x - bounds.x * 0.5f, 0f, zInterpolant * bounds.z - bounds.z * 0.5f);
                if (pos.sqrMagnitude > radiusSqr)
                    continue;
                
                if (propCoordFlipX)
                    xInterpolant = 1f - xInterpolant;

                if (propCoordFlipZ)
                    zInterpolant = 1f - zInterpolant;
                
                var xCoord = Mathf.FloorToInt (xInterpolant * textureWidthMinusOne);
                var zCoord = Mathf.FloorToInt (zInterpolant * textureHeightMinusOne);
                
                var colorIndex = Mathf.FloorToInt(zCoord * textureWidth + xCoord);
                try
                {
                    var color = colorArray[colorIndex];
                    float g = color.g / 255f;
                    float b = color.b / 255f;
                    float density = (1f - g) * (1f - b);

                    density = Mathf.Pow (density, propColorPower);
                    
                    if (propCoordDebug && xIndex % 2 == 0 && zIndex % 2 == 0)
                    {
                        var rayDebug = new Ray (new Vector3 (pos.x, 200f, pos.z), Vector3.down);
                        if (Physics.Raycast (rayDebug, out var hitDebug, 400f, LayerMasks.environmentMask))
                            Debug.DrawLine (hitDebug.point, hitDebug.point + new Vector3 (0f, 2f, 0f), new Color32 (color.r, color.g, color.b, 128), 5f);
                    }
                    
                    if (propCoordDebug || density < propColorThreshold)
                        continue;
                    
                    // if (g < 0.25f)
                    //     continue;

                    pos += new Vector3 (offsetScale * Random.Range (-1f, 1f), 0f, offsetScale * Random.Range (-1f, 1f)) * propOffsetRandom;
                    
                    var positionRay = new Vector3 (pos.x, 200f, pos.z);
                    var ray = new Ray (positionRay, Vector3.down);

                    if (!Physics.Raycast (ray, out var hit, 400f, LayerMasks.environmentMask))
                        continue;
                    
                    pos = hit.point;
                    var dot = Vector3.Dot (hit.normal, Vector3.up);
                    if (dot < propNormalDotRange.x || dot > propNormalDotRange.y)
                        continue;
                
                    // Debug.DrawLine (pos, pos + new Vector3 (0f, 10f * density, 0f), Color.Lerp (Color.yellow, Color.green, density), 5f);
                    Debug.DrawLine (pos + new Vector3 (0f, 3f, 0f), pos + new Vector3 (0f, 2f, 0f) + new Vector3 (0f, 10f * density, 0f), Color.white, 5f);

                    count += 1;

                    var propScale = Random.Range (propScaleMin, propScaleMax);
                    var propTransform = new OverworldLandscapePropTransform { position = pos, scale = propScale };
                    var propAsset = propAssets.GetRandomEntry ();
                    
                    if (propPlacementLookup.TryGetValue (propAsset.id, out var propGroup))
                        propGroup.transforms.Add (propTransform);
                    else
                    {
                        propPlacementLookup.Add (propAsset.id, new OverworldLandscapePropGroup
                        {
                            name = propAsset.prefab.name,
                            material = propAsset.material,
                            mesh = propAsset.mesh,
                            transforms = new List<OverworldLandscapePropTransform> { propTransform }
                        });
                    }

                    if (count > propLimit)
                        break;
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.Log ($"Bad index | Indexes: {xIndex}, {zIndex} | Interpolants: {xInterpolant}, {zInterpolant} | Coords: {xCoord}, {zCoord} | Index: {colorIndex}/{colorArray.Length}");
                    terminated = true;
                }
                
                if (terminated)
                    break;
            }
            
            if (terminated)
                break;
        }

        foreach (var kvp in propPlacementLookup)
            propPlacements.Add (kvp.Value);
    }
    
    private static Entity parentEntity;

    [Button, PropertyOrder (-1)]
    public static void SubmitProps ()
    {
        if (!Application.isPlaying)
            return;
        
        if (!ECSRenderingBatcher.IsECSSafe ())
            return;
        
        ClearProps ();
        
        if (ins == null)
            return;
        
        if (ins.propPlacements == null || ins.propPlacements.Count == 0)
            return;

        int batchID = ins.gameObject.name.GetHashCode (); // keyLoadedLast.GetHashCode ()
        bool batchRegistered = ECSRenderingBatcher.AreBatchInstancesRegistered (batchID);
        if (batchRegistered)
        {
            Debug.Log ($"OLM | Multiple batch registrations not possible for ID {batchID}");
            return;
        }
        
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (parentEntity == Entity.Null)
        {
            parentEntity = manager.CreateEntity ();
            manager.AddComponent (parentEntity, typeof (LocalToWorld));
            manager.SetComponentData (parentEntity, new LocalToWorld { Value = ins.transform.localToWorldMatrix });
            Debug.Log ($"OLM | Creating parent entity for prop batch {batchID}");
        }
        
        Debug.Log ($"OLM | Registering prop batch {batchID}");
        ECSRenderingBatcher.RegisterBatchInstances (parentEntity, ins.propPlacements, batchID, ins.gameObject.name);
        ECSRenderingBatcher.SubmitBatches ();
    }

    [Button]
    public static void ClearProps ()
    {
        if (!Application.isPlaying)
            return;

        if (!ECSRenderingBatcher.IsECSSafe ())
            return;
        
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (parentEntity != Entity.Null) 
        {
            if (manager.ExistsNonNull (parentEntity))
                manager.DestroyEntity (parentEntity);
            parentEntity = Entity.Null;
            Debug.Log ("OLM | Deleting prop parent entity");
        }

        if (ins == null || ins.gameObject == null)
            return;
        
        int batchID = ins.gameObject.name.GetHashCode (); // keyLoadedLast.GetHashCode ()
        bool batchRegistered = ECSRenderingBatcher.AreBatchInstancesRegistered (batchID);
        if (batchRegistered)
        {
            Debug.Log ($"OLM | Deleting registered prop batch {batchID}");
            ECSRenderingBatcher.CleanupGroup (batchID);
        }
    }

    [Button]
    public static void ClearAllEntities ()
    {
        ECSRenderingBatcher.FullCleanup ();
    }

    [Button, ButtonGroup]
    public static void Activate ()
    {
        SetActive (true);
    }

    [Button, ButtonGroup]
    public static void Deactivate ()
    {
        SetActive (false);
    }
    
    public static void SetActive (bool active)
    {
        if (ins != null && ins.holder != null)
            ins.holder.SetActive (active);
    }

    public static IEnumerable<string> GetAssetKeys ()
    {
        if (ins == null || ins.assetLookup == null)
            return null;

        return ins.assetLookup.Keys;
    }

    [Button]
    public static Vector3 GetBounds ()
    {
        if (ins == null)
            return Vector3.one;

        var sizeTransform = ins.holder.transform.localScale;
        float height = sizeTransform.y;

        if (ins.terrain.meshRendererComponent != null)
        {
            var bounds = ins.terrain.meshRendererComponent.bounds;
            var heightUnused = Mathf.Max (0f, bounds.center.y - bounds.size.y * 0.5f);
            
            // Horizontal renderer bounds are no longer reliable due to skirt meshes
            // Debug.Log ($"Horizontal: {bounds.size.x} | Height: {bounds.size.y} | Center: {bounds.center.y} | Height unused: {heightUnused}");
            
            // var size = new Vector3 (bounds.size.x, bounds.size.y + heightUnused, bounds.size.z);
            // return size;

            height = bounds.size.y + heightUnused;
        }

        // var sizeFallback = new Vector3 (ins.mainScale, ins.mainScale * 0.25f, ins.mainScale);
        // return sizeFallback;
        
        var sizeFinal = new Vector3 (sizeTransform.x, height, sizeTransform.z);
        return sizeFinal;
    }

    public static bool IsVisualLoaded (string key)
    {
        return rootInstanceLast != null && string.Equals (key, rootInstanceLast.key, StringComparison.Ordinal);
    }

    public static OverworldLandscapeRoot GetVisualPrefab (string key)
    {
        if (string.IsNullOrEmpty (key))
            return null;

        if (ins == null)
            return null;

        var linkFound = ins.assetLookup.TryGetValue (key, out var link);
        if (!linkFound)
            return null;

        var prefab = link.prefab;
        return prefab;
    }

    public static void TryGeneratingProps (Vector2 propNormalDotRangeOverride = default)
    {
        if (!Application.isPlaying)
            return;
        
        if (rootInstanceLast == null)
            return;
        
        if (propNormalDotRangeOverride.y > propNormalDotRangeOverride.x)
            propNormalDotRange = propNormalDotRangeOverride;
            
        if (rootInstanceLast.propSupport)
        {
            ins.GenerateProps ();
            SubmitProps ();
        }
        else
            ECSRenderingBatcher.FullCleanup ();
    }

    public static void TryGeneratingNav (float navSlopeLimitOverride = -1f)
    {
        if (navSlopeLimitOverride > 0f)
            navSlopeLimit = navSlopeLimitOverride;
        
        PathfindingGraphManager.RescanGraphOverworld (navSlopeLimit);
    }

    [Button]
    public void GenerateSegments ()
    {
        segmentsActive.Clear ();
        if (segmentHolder == null)
            return;
        
        if (rootInstanceLast == null)
            return;
        
        UtilityGameObjects.ClearChildren (segmentHolder);
        var t = segmentHolder.transform;

        if (rootInstanceLast.segments != null)
        {
            foreach (var segmentPrefab in rootInstanceLast.segments)
            {
                if (segmentPrefab == null)
                    continue;
                
                #if UNITY_EDITOR
                var segmentObject = UnityEditor.PrefabUtility.InstantiatePrefab (segmentPrefab.gameObject, t) as GameObject;
                #else
                var segmentObject = GameObject.Instantiate (segmentPrefab.gameObject, t) as GameObject;
                #endif
                
                if (segmentObject == null)
                    continue;

                var key = segmentPrefab.name;
                segmentObject.transform.SetLocalTransformationToZero ();
                segmentObject.name = key;
                
                var segmentComponent = segmentObject.GetComponent<OverworldLandscapeSegment> ();
                if (segmentComponent != null)
                    segmentComponent.OnSpawn ();
                
                segmentsActive.Add (segmentComponent);
            }
        }
    }

    public static bool TryLoadingVisual 
    (
        [ValueDropdown(nameof(GetAssetKeys))] string key, 
        bool reloadIfLast = false,
        float navSlopeLimitOverride = -1f,
        Vector2 propNormalDotRangeOverride = default
    )
    {
        if (string.IsNullOrEmpty (key))
        {
            // Debug.LogWarning ($"Can't apply landscape visual: no key provided.");
            SetActive (false);
            return false;
        }

        if (!reloadIfLast && IsVisualLoaded (key))
            return true;
        
        #if PB_MODSDK
        if (ins == null)
        {
            Debug.LogWarning ($"Failed to find the OverworldLandscapeManager component. Make sure you install the optional asset package and open the extended scene (game_extended_sdk).");
            return false;
        }
        
        ins.UpdateAssets ();
        #endif
        
        if (ins.rootHolder == null)
            return false;

        UtilityGameObjects.ClearChildren (ins.rootHolder);
        
        if (ins == null)
        {
            // Debug.LogWarning ($"Can't apply landscape visual {key}: no manager");
            SetActive (false);
            return false;
        }

        if (ins.terrain == null || ins.terrainSkirt == null)
        {
            Debug.LogWarning ($"Can't apply landscape visual {key}: terrain renderers not found.");
            SetActive (false);
            return false;
        }

        var linkFound = ins.assetLookup.TryGetValue (key, out var link);
        if (!linkFound || link.prefab == null)
        {
            Debug.LogWarning ($"Can't apply landscape visual {key}: asset with this key not found.");
            SetActive (false);
            return false;
        }
        
        #if UNITY_EDITOR
        var rootObject = UnityEditor.PrefabUtility.InstantiatePrefab (link.prefab.gameObject, ins.rootHolder.transform) as GameObject;
        #else
        var rootObject = GameObject.Instantiate (link.prefab.gameObject, ins.rootHolder.transform) as GameObject;
        #endif

        var rootComponent = rootObject != null ? rootObject.GetComponent<OverworldLandscapeRoot> () : null;
        if (rootComponent == null)
        {
            Debug.LogWarning ($"Can't apply landscape visual {key}: root component not found on prefab {link.prefab.name}");
            SetActive (false);
            return false;
        }

        if (ins.cloudsHolder != null)
            ins.cloudsHolder.transform.localRotation = Quaternion.Euler (0f, Random.Range (-180f, 180f), 0f);

        rootInstanceLast = rootComponent;
        if (alwaysHideRootObjects)
            rootInstanceLast.gameObject.SetActive (false);

        if (propNormalDotRangeOverride.y > propNormalDotRangeOverride.x)
            propNormalDotRange = propNormalDotRangeOverride;

        if (navSlopeLimitOverride > 0f)
            navSlopeLimit = navSlopeLimitOverride;
        
        var materialUsed = ins.mainMaterial;
        bool materialCustomFound = rootComponent.materialCustom != null;
        if (materialCustomFound)
            materialUsed = rootComponent.materialCustom;
        
        // if (mpb == null)
        //     mpb = new MaterialPropertyBlock ();
        // else
        //     mpb.Clear ();
            
        // Main texture needs to be accessible globally, so it's set via Shader.SetGlobal
        if (rootComponent.textureMain != null)
            Shader.SetGlobalTexture (propertyID_GlobalLandscapeMainTex, rootComponent.textureMain);
            
        // Splat texture needs to be accessible globally, so it's set via Shader.SetGlobal
        if (rootComponent.textureSplat != null)
            Shader.SetGlobalTexture (propertyID_GlobalLandscapeSplatTex, rootComponent.textureSplat);
        
        // Normal texture needs to be accessible only in the main material, so it's directly modified
        // MPBs are not used since they are in use for spotlight coords and some other tasks
        ins.mainMaterial.SetVector (propertyID_UVScale, new Vector4 (1f, rootComponent.uvInverted ? -1f : 1f, 0f, 0f));
        
        if (rootComponent.textureNormal != null)
        {
            ins.mainMaterial.SetTexture (propertyID_NormalTex, rootComponent.textureNormal);
            ins.mainMaterial.SetFloat (propertyID_NormalIntensity, Mathf.Clamp01 (rootComponent.materialNormalIntensity));
        }
        else
        {
            ins.mainMaterial.SetTexture (propertyID_NormalTex, null);
            ins.mainMaterial.SetFloat (propertyID_NormalIntensity, 0f);
        }

        if (ins.terrain != null)
        {
            ins.terrain.meshFilterComponent.sharedMesh = rootComponent.mesh;
            ins.terrain.meshRendererComponent.sharedMaterial = materialUsed;
            // Mesh collider field is optional, so if not set we use visual terrain as collider geometry
            if (rootComponent.meshCollider != null)
            {
                ins.terrain.meshColliderComponent.sharedMesh = rootComponent.meshCollider;
            }
            else
            {
                ins.terrain.meshColliderComponent.sharedMesh = rootComponent.mesh;
            }
        }

        if (ins.terrainSkirt != null)
            ins.terrainSkirt.meshFilterComponent.sharedMesh = rootComponent.meshSkirt;

        ins.holder.transform.localScale = new Vector3 (rootComponent.scaleHorizonal, rootComponent.scaleVertical, rootComponent.scaleHorizonal) * ins.mainScale;
        
        Debug.Log ($"Applied landscape visual {key}");
        SetActive (true);
        RefreshGlobals ();

        if (Application.isPlaying)
        {
            PathfindingGraphManager.RescanGraphOverworld (navSlopeLimit);

            if (rootComponent.propSupport)
            {
                ins.GenerateProps ();
                SubmitProps ();
            }
            else
                ClearProps ();
        }
        
        #if !PB_MODSDK
        var grounders = rootInstanceLast.GetComponentsInChildren<OverworldViewHelperGround> ();
        if (grounders != null && grounders.Length > 0)
        {
            foreach (var grounder in grounders)
                grounder.GroundAll ();
        }
        #endif

        ins.GenerateSegments ();

        #if !PB_MODSDK
        bool travelInProgress = DataHelperProvince.IsTravelInProgress ();
        if (rootInstanceLast.holdersHiddenOnTravel != null)
        {
            foreach (var holder in rootInstanceLast.holdersHiddenOnTravel)
                holder.SetActive (!travelInProgress);
        }
        #endif

        return true;
    }

    [Button]
    public static void RefreshGlobals ()
    {
        RefreshGlobalsWithInput (shaderSpotlightRadius);
    }

    private static void RefreshGlobalsWithInput (float spotlightRadiusInput)
    {
        if (ins == null)
            return;
        
        // Debug.Log ($"Spotlight radius: {spotlightRadiusInput} | Origin: {shaderSpotlightOrigin}");
        
        var spotlightData = new Vector4 (shaderSpotlightOrigin.x, shaderSpotlightOrigin.y, shaderSpotlightOrigin.z, spotlightRadiusInput);
        Shader.SetGlobalVector (propertyID_GlobalLandscapeSpotlightData, spotlightData);
        
        var bounds = GetBounds ();
        var dimensionData = new Vector4 (Mathf.Max (bounds.x, bounds.y) * 0.5f, bounds.y, shaderDimensionSlice, shaderDimensionOpacity);
        Shader.SetGlobalVector (propertyID_GlobalLandscapeDimensionData, dimensionData);
        
        // Debug.Log ($"Dimension shader prop: {dimensionData.x:F3}, {dimensionData.y:F3}, {dimensionData.z:F3}, {dimensionData.w:F3}");

        bool selectionSet = false;
        
        #if !PB_MODSDK
        if (Application.isPlaying)
        {
            var overworld = Contexts.sharedInstance.overworld;
            var entityOverworldSelected = overworld.hasSelectedEntity ? IDUtility.GetOverworldEntity (overworld.selectedEntity.id) : null;
            if (entityOverworldSelected != null && entityOverworldSelected.hasPosition && !entityOverworldSelected.isDestroyed && entityOverworldSelected.isPlayerRecognized)
            {
                var entityPersistent = IDUtility.GetLinkedPersistentEntity (entityOverworldSelected);
                if (entityPersistent != null && !entityPersistent.isPlayerBase && !entityPersistent.isHidden)
                {
                    selectionSet = true;
                    var pos = entityOverworldSelected.position.v;
                    var radius = shaderSpotlightSelectionRadius * spotlightRadiusInput;
                    Shader.SetGlobalVector (propertyID_GlobalLandscapeSelectionData, new Vector4 (pos.x, pos.z, radius, 1f));
                }
            }
        }
        #endif
        
        if (!selectionSet)
            Shader.SetGlobalVector (propertyID_GlobalLandscapeSelectionData, new Vector4 (0f, 0f, 1f, 0f));

        if (globalRefreshCount < 10000)
            globalRefreshCount += 1;
    }
}
