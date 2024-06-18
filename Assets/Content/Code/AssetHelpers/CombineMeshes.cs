using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CombineMeshes : MonoBehaviour
{
    public const int vertexLimit = UInt16.MaxValue - 2;
    public bool combineOnStart = true;
    public bool hideSourceObjects = true;
    public static bool log = false;
    public bool combined = false;
    public Transform destinationMeshHolder;
    public Transform[] sourceMeshHolders;

    static System.Diagnostics.Stopwatch timer;

    private void Start()
    {
        if (combineOnStart)
        {
            Combine();
        }
    }

    //Collect all renderers
    private static void CollectRenderers(List<Transform> transforms, List<MeshRenderer> renderers)
    {
        for (int i = transforms.Count - 1; i >= 0; --i)
        {
            if (transforms[i] == null)
            {
                Debug.LogWarning("Transform null, continuing ");
                continue;
            }

            MeshRenderer renderer = transforms[i].GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderers.Add(renderer);
            }
        }
    }

    //Takes a list or renderers and collects all materials into a hashed dictionary
    public static void CollectMaterials(List<MeshRenderer> renderers, Dictionary<int, Material> materials)
    {
        int hash = 0;
        for (int i = 0; i < renderers.Count; ++i)
        {
            for (int j = 0; j < renderers[i].sharedMaterials.Length; ++j)
            {
                if (renderers[i].sharedMaterials[j] == null)
                    continue;
                
                //hash for faster access speed
                hash = renderers[i].sharedMaterials[j].name.GetHashCode();

                if (!materials.ContainsKey(hash))
                {
                    materials.Add(hash, renderers[i].sharedMaterials[j]);
                }
            }
        }
    }

    public static void MaterialBatch()
    {

    }

    public static void CollectCombineInstances(List<MeshRenderer> renderers, Dictionary<int, List<CombineInstance>> instances)
    {
        //Collect all the combine instances, by shared material into a hashed dictionary
        MeshFilter filter;

        for (int i = 0; i < renderers.Count; ++i)
        {
            filter = renderers[i].GetComponent<MeshFilter>();

            if (filter == null)
            {
                Debug.LogWarning("Renderer " + renderers[i].name + " Contains no mesh filter reference, continuing ");
                continue;
            }

            int materialHash = 0;
            for (int j = 0; j < filter.sharedMesh.subMeshCount; ++j)
            {
                if (renderers[i].sharedMaterials[j] == null)
                    continue;
                
                CombineInstance instance = new CombineInstance();
                instance.transform = renderers[i].transform.localToWorldMatrix;
                instance.subMeshIndex = j;
                instance.mesh = filter.sharedMesh;

                materialHash = renderers[i].sharedMaterials[j].name.GetHashCode();

                if (!instances.ContainsKey(materialHash))
                {
                    instances.Add(materialHash, new List<CombineInstance>());
                    instances[materialHash].Add(instance);
                }
                else
                {
                    instances[materialHash].Add(instance);
                }
            }
        }
    }

    public static void CombineSubBatch(CombineInstance[] combination, Dictionary<int, Material> materials, int hash, GameObject parent)
    {
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combination, true, true);

        GameObject batch = new GameObject("Batch " + hash);
        batch.transform.parent = parent.transform;
        MeshFilter filter = batch.AddComponent<MeshFilter>();
        MeshRenderer renderer = batch.AddComponent<MeshRenderer>();
        renderer.material = materials[hash];
        filter.sharedMesh = mesh;
    }

    public static void CombineInstances(Dictionary<int, List<CombineInstance>> instances, Dictionary<int, Material> materials, GameObject parent)
    {
        int vertexCount = 0;

        // List<CombineInstance> finalInstances = new List<CombineInstance>();

        foreach (KeyValuePair<int, List<CombineInstance>> kvp in instances)
        {
            // CombineInstance[] combineArray = kvp.Value.ToArray();
            List<CombineInstance> subBatch = new List<CombineInstance>();

            for (int k = 0; k < kvp.Value.Count; ++k)
            {
                vertexCount += kvp.Value[k].mesh.vertexCount;

                if (vertexCount < vertexLimit)
                {
                    subBatch.Add(kvp.Value[k]);
                }
                else
                {
                    --k;
                    CombineSubBatch(subBatch.ToArray(), materials, kvp.Key, parent);
                    subBatch.Clear();
                    vertexCount = 0;
                }
            }

            if (subBatch.Count != 0)
                CombineSubBatch(subBatch.ToArray(), materials, kvp.Key, parent);
        }
    }

    public static void Batch (List<MeshRenderer> renderers, Dictionary<int, Material> materials, GameObject rootParent)
    {
        Dictionary<int, List<CombineInstance>> instances = new Dictionary<int, List<CombineInstance>>();

        CollectCombineInstances(renderers, instances);
        CombineInstances(instances, materials, rootParent);
    }

    //Recursively find all transforms and children
    public static void FindTransforms (List<Transform> transforms, Transform parent)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild (i);
            transforms.Add (child);
            FindTransforms (transforms, child);
        }
    }


    /// <summary>
    /// Cleaning up the destination transform batches are parented to
    /// </summary>
    public static void CleanupDestination(Transform destination)
    {
        if (destination == null) return;
        for (int i = destination.childCount - 1; i >= 0; --i)
        {
            DestroyImmediate(destination.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Combines all meshes and renderers that are children of the source transform, and parents them to a new gameobject
    /// </summary>
    /// <param name="source"></param>
    /// <returns>
    /// Returns the top level parent of the created batch
    /// </returns>
    public static GameObject CombineChildren(Transform source, Transform destination)
    {
        if (source == null)
            return null;

        // Cache destination local position and parent, since we'll need to clear those
        Vector3 destinationPosition = destination.localPosition;
        Transform destinationParent = destination.parent;

        // Clear destination parent and and position
        destination.parent = source.transform.parent;
        destination.localPosition = Vector3.zero;

        // Create merge result object and match it's position and rotation to the source object
        GameObject topObject = new GameObject (source.name);
        topObject.transform.position = source.position;
        topObject.transform.rotation = source.rotation;

        // Find all the transforms within the source (with a recursive method)
        List<Transform> children = new List<Transform>();
        FindTransforms (children, source);

        // Collect all mesh renderers
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        MeshRenderer rendererOnRoot = source.GetComponent<MeshRenderer> ();
        if (rendererOnRoot != null)
            renderers.Add (rendererOnRoot);
        CollectRenderers (children, renderers);

        // Collect all the materials into a hashed dictionary
        Dictionary<int, Material> materials = new Dictionary<int, Material>();
        CollectMaterials (renderers, materials);

        // Perform the batching operation
        Batch (renderers, materials, topObject);

        // Set the proper parent for the object containing batching results
        topObject.transform.parent = destination;

        // Restore destination parent and position
        destination.parent = destinationParent;
        destination.localPosition = destinationPosition;

        return topObject;
    }

    /// <summary>
    /// Combines all meshes and renderers that are children of the source transform, and parents them to a new gameobject
    /// </summary>
    /// <param name="source"></param>
    /// <returns>
    /// Returns the top level parent of the created batch
    /// </returns>
    public static GameObject CombineChildrenAndSaveToAsset(Transform source, Transform destination, string folder, string filename)
    {
        GameObject prefab = null;
#if UNITY_EDITOR
        if (source != null && AssetDatabase.IsValidFolder(folder))
        {
            // Get an empty GameObject with per-material single-material mesh GameObjects as children
            Vector3 sourceLocalPosition = source.localPosition;
            source.localPosition = Vector3.zero;

            Vector3 destinationLocalPosition = destination.localPosition;
            destination.localPosition = Vector3.zero;
            Transform destinationParent = destination.parent;

            Debug.Log("CM | CombineChildrenAndSaveToAsset | Source: " + source.name + " | Destination: " + destination.name);

            prefab = CombineChildren(source, destination);

            MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] instances = new CombineInstance[filters.Length];
            List<Material> materials = new List<Material>();

            for (int i = 0; i < filters.Length; ++i)
            {
                instances[i].mesh = filters[i].sharedMesh;
                instances[i].transform = filters[i].transform.worldToLocalMatrix;

                Material material = filters[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                if (!materials.Contains(material)) materials.Add(material);
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(instances, false);
            AssetDatabase.CreateAsset(combinedMesh, folder + "/" + filename + ".asset");

            for (int i = prefab.transform.childCount - 1; i >= 0; --i)
            {
                DestroyImmediate(prefab.transform.GetChild(i).gameObject);
            }

            MeshFilter filter = prefab.AddComponent<MeshFilter>();
            filter.mesh = AssetDatabase.LoadAssetAtPath(folder + "/" + filename + ".asset", typeof(Mesh)) as Mesh;
            MeshRenderer renderer = prefab.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials.ToArray();

            destination.parent = destinationParent;
            destination.localPosition = destinationLocalPosition;
            source.localPosition = sourceLocalPosition;

            prefab.transform.parent = destination;
            prefab.transform.localPosition = sourceLocalPosition;

            prefab = PrefabUtility.CreatePrefab(folder + "/" + filename + ".prefab", prefab, ReplacePrefabOptions.Default);
        }
        else
        {
            Debug.LogWarning("CM | Either the specified folder for asset export is invalid: " + folder + " or source transform is null");
        }
#else
        Debug.LogWarning ("CM | This is an editor-only method");
#endif
        return prefab;
    }

    #region Runtime Version

    private GameObject runtimeTopObject;
    private Transform runtimeSource;
    private Transform runtimeDestination;
    private Transform runtimeDestinationParent;
    private Vector3 runtimeDestinationPosition;
    private List<Transform> runtimeChildren;
    private List<MeshRenderer> runtimeRenderers;
    private Dictionary<int, Material> runtimeMaterials;
    private bool batching = false;

    public delegate void CompletionCallback();

    public void BeginRuntimeBatch(Transform source, CompletionCallback callback)
    {
        if (!batching)
        {
            batching = true;
            Debug.Log("Starting Combine Co-Routine");
            StartCoroutine(RuntimeCombine(source, destinationMeshHolder));

            if (callback != null)
            {
                callback();
            }
        }
    }

    public System.Collections.IEnumerator RuntimeCombine(Transform source, Transform destination)
    {
        Debug.Log("Running Runtime Combine");
        runtimeTopObject = source.gameObject;
        runtimeSource = source;
        runtimeDestination = destination;

        runtimeDestinationPosition = destination.localPosition;
        runtimeDestinationParent = destination.parent;

        destination.parent = source.transform.parent;
        destination.localPosition = Vector3.zero;

        runtimeTopObject = new GameObject(source.name);
        runtimeTopObject.transform.position = source.position;
        runtimeTopObject.transform.rotation = source.rotation;

        //Find all the children (recursively), of the source transform
        runtimeChildren = new List<Transform>();
        FindTransforms(runtimeChildren, source);

        yield return null;

        //Collect all mesh renderers
        runtimeRenderers = new List<MeshRenderer>();
        CollectRenderers(runtimeChildren, runtimeRenderers);

        yield return null;

        //Collect all the materials into a hashed dictionary, we can keep this around and re-use it
        if (runtimeMaterials == null)
            runtimeMaterials = new Dictionary<int, Material>();

        CollectMaterials(runtimeRenderers, runtimeMaterials);

        yield return null;

        Debug.Log("Starting Batch Co-Routine");
        StartCoroutine(RuntimeBatch());

        yield return null;
    }

    Dictionary<int, List<CombineInstance>> runTimeInstances = new Dictionary<int, List<CombineInstance>>();
    public System.Collections.IEnumerator RuntimeBatch()
    {
        Debug.Log("Runtime Batching");
        runTimeInstances.Clear();

        CollectCombineInstances(runtimeRenderers, runTimeInstances);
        yield return null;

        CombineInstances(runTimeInstances, runtimeMaterials, runtimeTopObject);
        yield return null;

        for(int i = 0; i < runtimeRenderers.Count; ++i)
        {
            runtimeRenderers[i].enabled = false;
        }

        yield return null;

        runtimeTopObject.transform.parent = runtimeDestination;

        runtimeDestination.parent = runtimeDestinationParent;
        runtimeDestination.localPosition = runtimeDestinationPosition;

        batching = false;
        yield return null;
    }

    #endregion

    /// <summary>
    /// Combines all meshes in the source mesh holders (testing function for the editor)
    /// </summary>
    [ContextMenu("Combine")]
    public void Combine()
    {
        if (combined)
        {
            if (log) Debug.LogWarning("CM | Meshes already combined once!");
            return;
        }

        if (timer == null)
            timer = new System.Diagnostics.Stopwatch();

        timer.Reset();
        timer.Start();

        CleanupDestination(destinationMeshHolder);

        for (int i = 0; i < sourceMeshHolders.Length; ++i)
        {
            if (sourceMeshHolders[i] == null)
            {
                if (log) Debug.LogWarning("CM | Holder " + i + " reference is missing");
                continue;
            }

            CombineChildren(sourceMeshHolders[i], destinationMeshHolder);
            if (hideSourceObjects)
            {
                sourceMeshHolders[i].gameObject.SetActive(false);
            }
        }

        timer.Stop();

        Debug.Log(" Batch Operation Completed in " + timer.Elapsed.Milliseconds.ToString() + " ms ");
        combined = true;
    }
}