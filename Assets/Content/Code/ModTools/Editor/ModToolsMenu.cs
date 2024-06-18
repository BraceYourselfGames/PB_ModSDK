using System;
using System.Collections.Generic;
using System.Reflection;
using Area;
using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FindMissingScriptsRecursively : EditorWindow
{
    [MenuItem ("PB Mod SDK/Refresh asset managers", priority = -150)]
    public static void RefreshAssetManagers ()
    {
        TextureManager.Load ();
        AreaTilesetHelper.LoadDatabase ();
        AreaAssetHelper.LoadResources ();
        ItemHelper.LoadVisuals ();
    }
    
    [MenuItem ("PB Mod SDK/Renderers/Enable all")]
    public static void EnableRenderers ()
    {
        SetRenderers (true);
    }

    [MenuItem ("PB Mod SDK/Renderers/Disable all")]
    public static void DisableRenderers ()
    {
        SetRenderers (false);
    }

    public static void SetRenderers (bool enabled)
    {
        GameObject[] go = Selection.gameObjects;
        foreach (GameObject g in go)
        {
            var rs = g.GetComponentsInChildren<Renderer> (true);
            foreach (var r in rs)
                r.enabled = enabled;
        }
    }
    
    [MenuItem ("PB Mod SDK/Renderers/Convert to MF-MR")]
    public static void ConvertToStandardRenderers ()
    {
        GameObject[] go = Selection.gameObjects;
        foreach (GameObject g in go)
        {
            var rs = g.GetComponentsInChildren<Renderer> (true);
            foreach (var r in rs)
            {
                if (r is SkinnedMeshRenderer smr)
                {
                    var mesh = smr.sharedMesh;
                    var mats = smr.sharedMaterials;
                    var host = smr.gameObject;
                    DestroyImmediate (smr);

                    var mf = host.AddComponent<MeshFilter> ();
                    var mr = host.AddComponent<MeshRenderer> ();

                    mf.sharedMesh = mesh;
                    mr.sharedMaterials = mats;
                }
            }
        }
    }
    
    [MenuItem ("PB Mod SDK/Renderers/Flatten SketchUp hierarchy")]
    public static void FlattenSketchUpHierarchy ()
    {
        GameObject[] go = Selection.gameObjects;
        foreach (GameObject host in go)
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot (host))
            {
                Debug.LogWarning ($"Skipping object {host.name} due to it not being a prefab instance root (FBX prefab instance)");
                continue;
            }
        
            PrefabUtility.UnpackPrefabInstance (host, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            
            var rs = host.GetComponentsInChildren<Renderer> (true);
            if (rs.Length == 0 || rs.Length > 1)
            {
                Debug.LogWarning ($"Skipping object {host.name} due to {rs.Length} renderers under it - expected 1");
                continue;
            }

            var mrOld = rs[0];
            var mfOld = mrOld.gameObject.GetComponent<MeshFilter> ();
            if (mfOld == null)
                continue;
            
            var mesh = mfOld.sharedMesh;
            var mats = mrOld.sharedMaterials;
            DestroyImmediate (mrOld.gameObject);
            
            var mf = host.AddComponent<MeshFilter> ();
            var mr = host.AddComponent<MeshRenderer> ();
            mf.sharedMesh = mesh;
            mr.sharedMaterials = mats;
        }
    }
    
    [MenuItem ("PB Mod SDK/Other/Find Hidden Objects")]
    public static void FindHiddenObjects ()
    {
        int goCount = 0;
        int componentCount = 0;
        
        GameObject[] topLevelObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().GetRootGameObjects ();
        for (int i = 0; i < topLevelObjects.Length; ++i)
        {
            GameObject topLevelObject = topLevelObjects[i];
            FindHiddenObjectsRecursive (topLevelObject.transform, 0, ref goCount, ref componentCount);
        }
        
        Debug.LogWarning ($"Search found {goCount} hidden gameobject and {componentCount} hidden components");
    }
    
    private static void FindHiddenObjectsRecursive (Transform current, int depth, ref int goCount, ref int componentCount)
    {
        if (current == null)
            return;

        var go = current.gameObject;
        string path = null;
        
        bool goHidden = (go.hideFlags & HideFlags.HideInInspector) != 0 || (go.hideFlags & HideFlags.HideInInspector) != 0;
        if (goHidden)
        {
            goCount += 1;
            path = GetTransformPath (current);
            Debug.LogWarning ($"{path}: Hidden gameobject!", go);
        }

        var components = go.GetComponents<Component> ();
        foreach (var component in components)
        {
            if (component == null)
                continue;
            
            bool componentHidden = (component.hideFlags & HideFlags.HideInInspector) != 0 || (component.hideFlags & HideFlags.HideInInspector) != 0;
            if (componentHidden)
            {
                componentCount += 1;
                if (path == null)
                    path = GetTransformPath (current);
                Debug.LogWarning ($"{path} - {component.GetType ().Name}: Hidden component!", component);
            }
        }

        var t = current.transform;
        var childCount = t.childCount;
        depth += 1;
        
        for (int i = 0; i < childCount; ++i)
        {
            var child = t.GetChild (i);
            FindHiddenObjectsRecursive (child, depth, ref goCount, ref componentCount);
        }
    }

    private static string GetTransformPath (Transform tr)
    {
        string result = tr.name;
        while (tr.parent != null)
        {
            tr = tr.parent;
            result = $"{tr.name}/{result}";
        }

        return result;
    }

    private static int goCount = 0;
    
    [MenuItem ("PB Mod SDK/Other/Find In-Scene Meshes")]
    public static void FindInSceneMeshes ()
    {
        ClearInSceneMeshes (false);
    }
    
    [MenuItem ("PB Mod SDK/Other/Destroy In-Scene Meshes")]
    public static void DestroyInSceneMeshes ()
    {
        ClearInSceneMeshes (true);
    }
    
    private static void ClearInSceneMeshes (bool destroy)
    {
        int meshCount = 0;
        GameObject[] topLevelObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().GetRootGameObjects ();
        for (int i = 0; i < topLevelObjects.Length; ++i)
        {
            GameObject topLevelObject = topLevelObjects[i];
            FindInSceneMeshesRecursive (topLevelObject.transform, 0, ref meshCount, destroy);
        }
        
        Debug.LogWarning ($"Search found {goCount} in-scene meshes hidden components");
    }
    
    private static void FindInSceneMeshesRecursive (Transform current, int depth, ref int meshCount, bool destroy)
    {
        if (current == null)
            return;

        var go = current.gameObject;
        string path = null;
        
        bool goHidden = (go.hideFlags & HideFlags.HideInInspector) != 0 || (go.hideFlags & HideFlags.HideInInspector) != 0;
        if (goHidden)
        {
            goCount += 1;
            path = GetTransformPath (current);
            // Debug.LogWarning ($"{path}: Hidden gameobject!", go);
        }

        var mf = go.GetComponent<MeshFilter> ();
        if (mf != null && mf.sharedMesh != null)
        {
            var mesh = mf.sharedMesh;
            if (mesh != null)
            {
                var pathToAssset = AssetDatabase.GetAssetPath (mesh);
                if (string.IsNullOrEmpty (pathToAssset))
                {
                    meshCount += 1;
                    if (path == null)
                        path = GetTransformPath (current);
                    Debug.LogWarning ($"{path} - MeshFilter: In-scene mesh {mesh.name} with {mesh.vertexCount} verts", mf.gameObject);

                    if (destroy)
                    {
                        DestroyImmediate (mesh);
                        mf.sharedMesh = null;
                    }
                }
            }
        }

        var t = current.transform;
        var childCount = t.childCount;
        depth += 1;
        
        for (int i = 0; i < childCount; ++i)
        {
            var child = t.GetChild (i);
            FindInSceneMeshesRecursive (child, depth, ref meshCount, destroy);
        }
    }

    [MenuItem ("PB Mod SDK/Item visuals/Separate renderer")]
    static void SeparateRendererToChildNormal () => SeparateRendererToChild (false);
    
    [MenuItem ("PB Mod SDK/Item visuals/Separate renderer and invert")]
    static void SeparateRendererToChildInvert () => SeparateRendererToChild (true);
    
    static void SeparateRendererToChild (bool invert)
    {
        foreach (var go in Selection.gameObjects)
        {
            if (go == null)
                continue;

            var t = go.transform;
            var mr = go.GetComponent<MeshRenderer> ();
            var mf = go.GetComponent<MeshFilter> ();
            
            if (mr == null || mf == null)
                continue;

            BoxCollider box = null;
            for (int i = 0; i < t.childCount; ++i)
            {
                var childOther = t.GetChild (i);
                var boxCandidate = childOther.GetComponent<BoxCollider> ();
                if (boxCandidate != null)
                {
                    box = boxCandidate;
                    var center = box.center;
                    var bt = box.transform;
                    bt.position -= center;
                    box.center = Vector3.zero;
                        
                    if (invert)
                        bt.SetPositionLocalY (-bt.localPosition.y);
                        
                    box.name = $"{go.name}_col";
                    Debug.Log ($"Cancelling out center, inverting height for box collider {childOther.name}");
                }
            }

            var mats = mr.sharedMaterials;
            var mesh = mf.sharedMesh;

            var child = new GameObject ();
            child.name = $"{go.name}_mesh";
            child.transform.parent = t;
            child.transform.SetLocalTransformationToZero ();
            
            var mrChild = child.AddComponent<MeshRenderer> ();
            mrChild.sharedMaterials = mats;

            var mfChild = child.AddComponent<MeshFilter> ();
            mfChild.sharedMesh = mesh;

            if (box != null)
                box.transform.parent = child.transform;
            
            GameObject.DestroyImmediate (mf);
            GameObject.DestroyImmediate (mr);
            UnityEditor.EditorUtility.SetDirty (go);

            if (invert)
            {
                child.transform.localScale = new Vector3 (1f, -1f, 1f);
                t.SetPositionLocalY (-t.localPosition.y);
                Debug.Log ($"Creating child {child.name} with MR/MF from {go.name}, inverting its scale, inverting height for parent");
            }
            else
                Debug.Log ($"Creating child {child.name} with MR/MF from {go.name}");
        }
    }
    
    [MenuItem ("PB Mod SDK/Item visuals/Reset collider origin")]
    static void ResetColliderOrigin ()
    {
        foreach (var go in Selection.gameObjects)
        {
            if (go == null)
                continue;

            var box = go.GetComponent<BoxCollider> ();
            if (box == null)
                continue;

            var center = box.center;
            box.transform.position -= center;
            box.center = Vector3.zero;
        }
    }
    
    [MenuItem ("PB Mod SDK/Item visuals/Add colliders to sel. renderers")]
    static void CreatePerRendererColliders ()
    {
        foreach (var go in Selection.gameObjects)
        {
            if (go == null)
                continue;
            
            bool boundsCreated = false;
            Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);
            
            var mrs = go.GetComponentsInChildren<MeshRenderer> ();
            for (int i = 0; i < mrs.Length; ++i)
            {
                var mr = mrs[i];
                if (mr != null)
                {
                    if (boundsCreated)
                    {
                        bounds.Encapsulate (mr.bounds);
                    }
                    else
                    {
                        bounds = mr.bounds;
                        boundsCreated = true;
                    }
                }
            }

            var t = go.transform;
            var colObject = new GameObject ($"{go.name}_col");
            colObject.transform.parent = t;
            colObject.transform.SetLocalTransformationToZero ();
            colObject.layer = LayerMasks.puppetRagdollLayerID;
            
            var col = colObject.AddComponent<BoxCollider> ();
            col.center = t.InverseTransformPoint (bounds.center);
            col.size = bounds.size;
        }
    }
    
    [MenuItem ("PB Mod SDK/Item visuals/Add root collider using children")]
    static void CreateRootCollider ()
    {
        var go = Selection.activeGameObject;
        if (go == null)
            return;
        
        bool boundsCreated = false;
        Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);

        var cls = go.GetComponentsInChildren<BoxCollider> ();
        for (int i = 0; i < cls.Length; ++i)
        {
            var cl = cls[i];
            if (cl != null)
            {
                if (boundsCreated)
                {
                    bounds.Encapsulate (cl.bounds);
                }
                else
                {
                    bounds = cl.bounds;
                    boundsCreated = true;
                }
            }
        }
        
        var t = go.transform;
        var colObject = new GameObject ($"col_root_impact");
        colObject.transform.parent = t;
        colObject.transform.SetLocalTransformationToZero ();
        colObject.layer = LayerMasks.impactTriggersLayerID;
            
        var col = colObject.AddComponent<BoxCollider> ();
        col.center = t.InverseTransformPoint (bounds.center);
        col.size = bounds.size;

        var iv = go.GetComponent<ItemVisual> ();
        if (iv != null)
        {
            iv.colliderImpactOverride = col;
            iv.colliderHitOverrides = new List<Collider> ();
            foreach (var cl in cls)
            {
                if (cl != null)
                    iv.colliderHitOverrides.Add (cl);
            }
        }
    }
    
    [MenuItem ("PB Mod SDK/Item visuals/Add socket links")]
    static void CreateSocketLinks ()
    {
        var go = Selection.activeGameObject;
        if (go == null)
            return;
        
        var iv = go.GetComponent<ItemVisual> ();
        if (iv == null || iv.renderers == null || iv.renderers.Count == 0)
            return;

        iv.renderersSocketMapped = new List<UnitRendererSocketMapped> ();
        foreach (var mr in iv.renderers)
        {
            iv.renderersSocketMapped.Add (new UnitRendererSocketMapped
            {
                renderer = mr,
                socketMappings = new List<UnitRendererSocketMapping>
                {
                    new UnitRendererSocketMapping
                    {
                        socketName = LoadoutSockets.corePart,
                        targetChannel = 0
                    }
                }
            });
        }
    }
}