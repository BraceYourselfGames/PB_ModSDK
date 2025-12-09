using System;
using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AreaFieldLinkPrefab
{
    public string key;
    public AreaFieldLink prefab;
}

public class AreaFieldHelper : MonoBehaviour
{
    public Transform holder;
    public Transform holderStateLocation;
    
    public List<AreaFieldLinkPrefab> prefabs;
    public AreaFieldLink prefabStateLocationRect;
    public AreaFieldLink prefabStateLocationCircle;
    
    [ShowInInspector, ReadOnly]
    private Dictionary<string, AreaFieldLink> prefabLookup = new Dictionary<string, AreaFieldLink> ();
    
    [ShowInInspector, ReadOnly]
    private List<AreaFieldLink> instances = new List<AreaFieldLink> ();
    
    [ShowInInspector, ReadOnly]
    private Dictionary<string, AreaFieldLink> instancesStateLocation = new Dictionary<string, AreaFieldLink> ();

    [NonSerialized]
    private bool prefabLookupInitialized = false;

    private void CheckLookup ()
    {
        if (prefabLookupInitialized)
            return;

        prefabLookupInitialized = true;
        prefabLookup.Clear ();
        
        if (prefabs != null)
        {
            foreach (var link in prefabs)
            {
                if (!string.IsNullOrEmpty (link.key) && !prefabLookup.ContainsKey (link.key))
                    prefabLookup.Add (link.key, link.prefab);
            }
        }
    }

    private void RecheckVisibility ()
    {
        bool visible = instances.Count > 0 || instancesStateLocation.Count > 0;
        gameObject.SetActive (visible);
    }
    
    public void LoadFields (List<DataBlockAreaField> fields)
    {
        ClearFields ();

        if (fields == null)
        {
            RecheckVisibility ();
            return;
        }

        CheckLookup ();
        
        for (int i = 0, count = fields.Count; i < count; ++i)
        {
            var field = fields[i];
            if (field == null)
            {
                Debug.LogWarning ($"Skipping area field {i} | Null data block");
                continue;
            }
            
            if (string.IsNullOrEmpty (field.type) || !prefabLookup.TryGetValue (field.type, out var prefab))
            {
                // Debug.LogWarning ($"Skipping area field {i} due to invalid field type key: {field.type} | Registered keys: {prefabLookup.ToStringFormattedKeys ()}");
                continue;
            }

            #if UNITY_EDITOR
                var instance = (Application.isPlaying ? GameObject.Instantiate (prefab, holder) : UnityEditor.PrefabUtility.InstantiatePrefab (prefab, holder)) as AreaFieldLink;
            #else
                var instance = GameObject.Instantiate (prefab, holder) as AreaFieldLink;
            #endif

            if (instance == null)
                continue;

            instances.Add (instance);
            instance.name = $"{i:D2}_{field.type}";
            instance.type = field.type;
            instance.tag = $"in_field_{field.type}";

            var t = instance.transform;
            t.SetLocalTransformationToZero ();
            t.position = field.origin;
            t.rotation = Quaternion.Euler (0f, field.rotation, 0f);
            t.localScale = field.size;

            if (!field.visible)
            {
                if (instance.mr != null)
                    instance.mr.enabled = false;
            }
        }

        RecheckVisibility ();
    }

    public void ClearStateLocations ()
    {
        UtilityGameObjects.ClearChildren (holderStateLocation);
        instancesStateLocation.Clear ();
    }

    public void ClearFields ()
    {
        UtilityGameObjects.ClearChildren (holder);
        instances.Clear ();
    }
}
