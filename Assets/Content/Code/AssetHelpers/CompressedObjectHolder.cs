using System;
using UnityEngine;
using System.Collections.Generic;
using PhantomBrigade;
using Sirenix.OdinInspector;


public static class HorizonElementLibrary
{
    public static string pathToTrees = "Content/Background/Trees";
    public static List<ResourceDatabaseEntryRuntime> resourcesForTrees;
    public static Dictionary<string, GameObject> prefabsForTrees;
    public static List<GameObject> prefabsForTreesList;

    public static string pathToVegetation = "Content/Props/vegetation";

    public static string pathToStructures = "Content/Background/Structures";
    public static List<ResourceDatabaseEntryRuntime> resourcesForStructures;
    public static Dictionary<string, GameObject> prefabsForStructures;
    public static List<GameObject> prefabsForStructuresList;

    private static bool initialized = false;

    public static void CheckInitialization ()
    {
        if (initialized)
            return;

        initialized = true;
        LoadDatabase ();
    }

    public static void LoadDatabase ()
    {
        //I moved the clearing of lists outside the resources import, to allow trees to come in from multiple locations
        //Apparently the overworld uses a different folder path, than all the other level objects
        resourcesForTrees?.Clear();
        prefabsForTrees?.Clear();
        prefabsForTreesList?.Clear();

        resourcesForStructures?.Clear();
        prefabsForStructures?.Clear();
        prefabsForStructuresList?.Clear();

        AttemptResourceImport (pathToTrees, ref resourcesForTrees, ref prefabsForTrees, ref prefabsForTreesList);
        AttemptResourceImport (pathToVegetation, ref resourcesForTrees, ref prefabsForTrees, ref prefabsForTreesList);
        AttemptResourceImport (pathToStructures, ref resourcesForStructures, ref prefabsForStructures, ref prefabsForStructuresList);
        if (prefabsForTrees == null || prefabsForStructures == null)
            Debug.LogError ("Prefab references were lost, you need to reimport your Resources/Content directory");
    }

    private static void AttemptResourceImport (string path, ref List<ResourceDatabaseEntryRuntime> resources, ref Dictionary<string, GameObject> assetDictionary, ref List<GameObject> assetList)
    {
        if (resources == null)
            resources = new List<ResourceDatabaseEntryRuntime> ();

        var rdb = ResourceDatabaseManager.GetDatabase ();
        if (rdb == null || rdb.entries == null)
            return;

        var rdbEntiesLookup = rdb.entries;
        if (rdbEntiesLookup.ContainsKey (path))
        {
            ResourceDatabaseEntryRuntime infoDir = ResourceDatabaseManager.GetEntryByPath (path);
            ResourceDatabaseManager.FindResourcesRecursive (resources, infoDir, 1, ResourceDatabaseEntrySerialized.Filetype.Prefab);
        }

        if (assetDictionary == null)
            assetDictionary = new Dictionary<string, GameObject> ();

        if (assetList == null)
            assetList = new List<GameObject> ();

        for (int i = 0; i < resources.Count; ++i)
        {
            ResourceDatabaseEntryRuntime entry = resources[i];
            GameObject prefab = entry.content as GameObject;
            if (prefab != null && !assetDictionary.ContainsKey(prefab.name))
            {
                assetDictionary.Add (prefab.name, prefab);
                assetList.Add (prefab);
            }
        }
    }

    public static GameObject GetPrefabTree (string name)
    {
        CheckInitialization ();
        return prefabsForTrees.ContainsKey (name) ? prefabsForTrees[name] : null;
    }

    public static GameObject GetPrefabStructure (string name)
    {
        CheckInitialization ();
        return prefabsForStructures.ContainsKey (name) ? prefabsForStructures[name] : null;
    }
}

[Serializable]
public class CompressedObjectGroup
{
    public string name;
    
    [NonSerialized, ShowInInspector, ReadOnly]
    public Mesh mesh;
    
    [NonSerialized]
    public Material[] materials;

    [ShowInInspector]
    public int count
    {
        get
        {
            return placements != null ? placements.Count : 0;
        }
    }
    
    [HideInInspector]
    public List<CompressedObjectTransform> placements = new List<CompressedObjectTransform> ();
}

[Serializable]
public struct CompressedObjectTransform
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}



[ExecuteInEditMode]
public class CompressedObjectHolder : MonoBehaviour
{
    public bool objectMode = false;
    public bool compressed = false;
    public bool decompressOnStart = false;
    public int instanceCount = 0;

    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<CompressedObjectGroup> groups;

    [NonSerialized]
    private bool loaded = false;
    
    [NonSerialized]
    private bool refsChecked = false;

    public bool allowEditorDestruction = true;


    public void Start ()
    {
        if (Application.isPlaying && decompressOnStart)
        {
            Decompress ();
        }
    }
    
    private void OnEnable ()
    {
        if (!refsChecked)
            CheckReferences ();
        
        /*
        #if UNITY_EDITOR
        // Debug.Log ("Started on object " + gameObject.name);
        if (!Application.isPlaying && !loaded)
        {
            loaded = true;
            bool wasCompressed = compressed;
            Decompress ();
            if (wasCompressed)
                Compress ();
        }
        #endif
        */
    }

    public void RefreshInstanceCount ()
    {
        instanceCount = 0;
        if (groups != null)
        {
            for (int i = 0; i < groups.Count; ++i)
            {
                CompressedObjectGroup group = groups[i];
                if (group != null && group.placements != null)
                    instanceCount += group.placements.Count;
            }
        }
    }
    
    #if UNITY_EDITOR

    public void OnDestroy ()
    {
        if (!Application.isPlaying && allowEditorDestruction)
        {
            Debug.Log ("Cleaning up in-editor state for object group " + gameObject.name + " which is " + (compressed ? "compressed" : "decomposed"));
            if (!compressed)
            {
                GenerateDescription ();
                for (int i = transform.childCount - 1; i >= 0; --i)
                    DestroyImmediate (transform.GetChild (i).gameObject);
            }
            else
            {
                MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
                if (mf != null)
                {
                    if (mf.sharedMesh != null && string.Equals (mf.sharedMesh.name, objectMode ? "ObjectGroup_Baked" : "TreeGroup_Baked"))
                        DestroyImmediate (mf.sharedMesh);
                }
            }
        }
    }

    #endif
    
    [HideIf ("compressed")]
    [Button ("Save objects (compress)", ButtonSizes.Large), PropertyOrder (-1), ButtonGroup]
    public void Compress ()
    {
        if (compressed)
        {
            Debug.Log ("Object is compressed", gameObject);
            return;
        }

        int childCount = transform.childCount;
        if (childCount == 0)
        {
            Debug.Log ("There are no child objects under the currently selected group, compression is not possible");
            return;
        }

        for (int i = 0; i < childCount; i++)
        {
            Transform currentTransform = transform.GetChild (i);
            if (currentTransform.GetComponent<MeshFilter> () == null)
            {
                Debug.Log ("There are children in the current group that have no MeshFilter component. Remove any invalid objects and try again!");
                return;
            } 
        }

        GenerateDescription ();
        if(allowEditorDestruction)
            transform.DestroyChildren ();
        compressed = true;
    }
    
    [HideIf ("compressed")]
    [Button ("Ground", ButtonSizes.Large), PropertyOrder (-1), ButtonGroup]
    public void Ground ()
    {
        if (compressed)
            return;

        int childCount = transform.childCount;
        if (childCount == 0)
            return;

        for (int i = 0; i < childCount; i++)
        {
            Transform currentTransform = transform.GetChild (i);
            var lp = currentTransform.localPosition;
            
            var groundingRayOrigin = new Vector3 (lp.x, 400f, lp.z);
            var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
            
            if (Physics.Raycast (groundingRay, out var hit, 800f, LayerMasks.environmentMask))
            {
                Debug.DrawLine (currentTransform.localPosition, hit.point, Color.green, 5f);
                
                lp = hit.point;
                currentTransform.localPosition = lp;
            }

            else
            {
                Debug.DrawLine (currentTransform.localPosition, groundingRayOrigin + Vector3.down * 800f, Color.red, 5f);
            }
        }
    }
    
    [HideIf ("compressed")]
    [Button ("Update info", ButtonSizes.Large), PropertyOrder (-1), ButtonGroup]
    public void UpdateInfo ()
    {
        if (compressed)
            return;

        int childCount = transform.childCount;
        if (childCount == 0)
            return;

        var sizeAverage = 0f;
        
        for (int i = 0; i < childCount; i++)
        {
            Transform currentTransform = transform.GetChild (i);
            var lp = currentTransform.localPosition;

            var ls = currentTransform.localScale;
            sizeAverage += ls.magnitude;
        }

        sizeAverage /= childCount;
        Debug.LogWarning ($"Average size: {sizeAverage}");
    }
    
    [HideIf ("compressed")]
    [Button ("Reset/reload", ButtonSizes.Large), PropertyOrder (-1), ButtonGroup]
    public void Reset ()
    {
        if (!compressed)
            return;

        int childCount = transform.childCount;
        if (childCount == 0)
            return;

        for (int i = childCount - 1; i >= 0; --i)
            DestroyImmediate (transform.GetChild (i).gameObject);
        
        Decompress ();
    }





    public void GenerateDescription ()
    {
        Debug.LogWarning ($"Generating descriptions on compressed group holder {gameObject.name}"); 
        int childCount = transform.childCount;
        groups = new List<CompressedObjectGroup> ();
        Dictionary<string, CompressedObjectGroup> groupsAsDictionary = new Dictionary<string, CompressedObjectGroup> ();

        for (int i = 0; i < childCount; ++i)
        {
            Transform t = transform.GetChild (i);
            if (!groupsAsDictionary.ContainsKey (t.name))
            {
                CompressedObjectGroup groupNew = new CompressedObjectGroup ();
                groupNew.name = t.name;
                groupNew.mesh = t.GetComponent<MeshFilter>().sharedMesh;
                groupsAsDictionary.Add (t.name, groupNew);
                groups.Add (groupNew);
            }

            CompressedObjectGroup group = groupsAsDictionary[t.name];
            if (group.placements == null)
                group.placements = new List<CompressedObjectTransform> ();

            CompressedObjectTransform placement = new CompressedObjectTransform ();
            placement.position = t.position;
            placement.rotation = t.rotation;
            placement.scale = t.localScale;
            group.placements.Add (placement);
        }
    }

    public void CheckReferences ()
    {
        refsChecked = true;
        if (groups == null)
        {
            Debug.Log ($"There is no data available to perform reference setup on compressed group holder {gameObject.name}!", gameObject);
            return;
        }

        HorizonElementLibrary.CheckInitialization ();
        RefreshInstanceCount ();
        
        for (int i = 0; i < groups.Count; ++i)
        {
            CompressedObjectGroup group = groups[i];
            var prefab = 
                objectMode ? 
                HorizonElementLibrary.GetPrefabStructure (group.name) : 
                HorizonElementLibrary.GetPrefabTree (group.name);
            
            if (prefab == null)
            {
                Debug.Log ($"Element {i} prefab {group.name} could not be found");
                continue;
            }
            
            var mf = prefab.GetComponent<MeshFilter> ();
            if (mf != null)
            {
                // Debug.Log ($"Element {i} prefab {group.name} mesh: {mf.sharedMesh.ToStringNullCheck ()}");
                group.mesh = mf.sharedMesh;
            }
            else
                Debug.Log ($"Element {i} prefab {group.name} has no root mesh filter");
            
            var mr = prefab.GetComponent<MeshRenderer> ();
            if (mr != null)
            {
                // Debug.Log ($"Element {i} prefab {group.name} materials: {mr.sharedMaterials.ToStringNullCheck ()}");
                group.materials = mr.sharedMaterials;
            }
            else
                Debug.Log ($"Element {i} prefab {group.name} has no root mesh renderer");
        }
    }


    [ShowIf ("compressed")]
    [Button ("Create objects (decompress)", ButtonSizes.Large), PropertyOrder (-1)]
    public void Decompress ()
    {
        if (groups == null)
        {
            Debug.Log ($"There is no data available to perform decompression on compressed group holder {gameObject.name}!", gameObject);
            return;
        }
        
        if (objectMode)
        {
            if (HorizonElementLibrary.prefabsForStructures == null)
                HorizonElementLibrary.LoadDatabase ();
        }
        else
        {
            if (HorizonElementLibrary.prefabsForTrees == null)
                HorizonElementLibrary.LoadDatabase ();
        }

        UtilityGameObjects.ClearChildren (gameObject);
        // SphereCollider sc = null;
        // bool useCollider = !Application.isPlaying;

        for (int i = 0; i < groups.Count; ++i)
        {
            CompressedObjectGroup group = groups[i];
            GameObject prefab = 
                objectMode ? 
                HorizonElementLibrary.GetPrefabStructure (group.name) : 
                HorizonElementLibrary.GetPrefabTree (group.name);
            
            if (prefab == null)
            {
                Debug.Log ($"Element {i} prefab {group.name} could not be found");
                continue;
            }

            for (int p = 0; p < group.placements.Count; ++p)
            {
                CompressedObjectTransform placement = group.placements[p];
                GameObject instance = null;

                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    instance = UnityEditor.PrefabUtility.InstantiatePrefab (prefab) as GameObject;
                else
                    instance = Instantiate (prefab);
                #else
                instance = Instantiate (prefab);
                #endif

                instance.name = prefab.name;
                instance.transform.parent = transform;
                instance.transform.position = placement.position;
                instance.transform.rotation = placement.rotation;
                instance.transform.localScale = placement.scale;
                
                if(allowEditorDestruction)
                    instance.hideFlags = HideFlags.DontSave;

                /*
                if (useCollider)
                {
                    sc = instance.AddComponent<SphereCollider> ();
                    sc.radius = 4f;
                }
                */
            }
        }

        compressed = false;
        // if (!gameObject.name.EndsWith (suffixDecomposed))
        //     gameObject.name += suffixDecomposed;
    }

    public void SkipLoading ()
    {
        loaded = true;
    }

    public bool AnyWithinRadius (Vector3 origin, float radius, float objectSize = 4f)
    {
        if (transform.childCount == 0)
            return false;

        bool neighbourPresent = false;
        float radiusSquared = Mathf.Pow (radius + objectSize, 2f);

        // Debug.Log ("Place in radius: " + radius + " | Added object size: " + objectSize + " | Squared value: " + radiusSquared + " | Child count: " + transform.childCount);


        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform t = transform.GetChild (i);
            if ((t.position - origin).sqrMagnitude < radiusSquared)
            {
                neighbourPresent = true;
                break;
            }
        }

        return neighbourPresent;
    }

    public void DeleteInRadius (Vector3 origin, float radius, float objectSize = 4f)
    {
        if (transform.childCount == 0)
            return;

        float radiusSquared = Mathf.Pow (radius + objectSize, 2f);
        // Debug.Log ("Delete in radius: " + radius + " | Added object size: " + objectSize + " | Squared value: " + radiusSquared + " | Child count: " + transform.childCount);


        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform t = transform.GetChild (i);
            if ((t.position - origin).sqrMagnitude < radiusSquared)
            {
                DestroyImmediate (t.gameObject);
            }
        }
    }
}
