using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneCleanupHelper : MonoBehaviour
{
    #if UNITY_EDITOR
    
    [HideReferenceObjectPicker]
    public class AssetDiscovered
    {
        [HideLabel, HorizontalGroup (32f)]
        public bool selected;
        
        [HideLabel, HorizontalGroup]
        public string path;
        
        [HideLabel]
        public Object reference;
        
        [HideLabel]
        public GameObject gameObject;
    }

    [NonSerialized, ShowInInspector, ListDrawerSettings (DefaultExpandedState = true, IsReadOnly = true)]
    private List<AssetDiscovered> assets = new List<AssetDiscovered> ();

    [Button ("Find embedded assets", ButtonSizes.Large), ButtonGroup, PropertyOrder (-1)]
    private void RefreshList ()
    {
        assets.Clear ();
        var topLevelObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().GetRootGameObjects ();
        for (int i = 0; i < topLevelObjects.Length; ++i)
        {
            GameObject topLevelObject = topLevelObjects[i];
            RefreshListRecursive (topLevelObject.transform, 0);
        }
    }

    private void RefreshListRecursive (Transform current, int depth)
    {
        if (current == null)
            return;

        var go = current.gameObject;
        string path = null;
        
        bool goHidden = (go.hideFlags & HideFlags.HideInInspector) != 0 || (go.hideFlags & HideFlags.HideInInspector) != 0;
        if (goHidden)
        {
            path = GetTransformPath (current);
            Debug.LogWarning ($"{path}: Hidden gameobject!", go);
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
                    if (path == null)
                        path = GetTransformPath (current);
                    Debug.LogWarning ($"{path} - MeshFilter: In-scene mesh {mesh.name} with {mesh.vertexCount} verts", mf.gameObject);
                    assets.Add (new AssetDiscovered
                    {
                        selected = false,
                        path = path,
                        reference = mesh,
                        gameObject = go
                    });
                }
            }
        }

        var t = current.transform;
        var childCount = t.childCount;
        depth += 1;
        
        for (int i = 0; i < childCount; ++i)
        {
            var child = t.GetChild (i);
            RefreshListRecursive (child, depth);
        }
    }
    
    private string GetTransformPath (Transform tr)
    {
        string result = tr.name;
        while (tr.parent != null)
        {
            tr = tr.parent;
            result = $"{tr.name}/{result}";
        }

        return result;
    }

    [Button ("Destroy selections", ButtonSizes.Large), ButtonGroup, PropertyOrder (-1)]
    private void DestroySelections ()
    {
        for (int i = assets.Count - 1; i >= 0; --i)
        {
            var asset = assets[i];
            if (asset == null || asset.reference == null || !asset.selected)
                continue;
            
            Debug.Log ($"Destroying asset {i} of type {asset.reference.GetType ().Name} at {asset.path}", gameObject);
            DestroyImmediate (asset.reference);
        }
    }
    
    #endif
}
