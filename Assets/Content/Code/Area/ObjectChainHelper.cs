using System;
using System.Collections.Generic;
using System.Linq;
using PhantomBrigade;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectChainLinkTypes
{
    public const string rail = "Rail";
    public const string road2x = "Road 2x";
    public const string road3x = "Road 3x";
    public const string road4x = "Road 4x";
    public const string road5x = "Road 5x";
}

[Serializable]
public class ObjectChainLink
{
    [ValueDropdown ("GetKeys")]
    public string type;
    
    public Transform transform;
    
    [ReadOnly]
    public ObjectChainHelper attachment;

    [NonSerialized]
    public ObjectChainHelper parent;

    public static List<string> GetKeys () =>
        FieldReflectionUtility.GetConstantStringFieldValues (typeof (ObjectChainLinkTypes), false);
}

[ExecuteInEditMode]
public class ObjectChainHelper : MonoBehaviour
{
    public Vector3 center;

    [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true)]
    [OnValueChanged ("UpdateLinks")]
    public List<ObjectChainLink> links;

    private void OnEnable ()
    {
        UpdateLinks ();
    }

    private void UpdateLinks ()
    {
        if (links == null)
            return;

        for (int i = 0; i < links.Count; i++)
        {
            var link = links[i];
            if (link == null)
                continue;
            
            link.parent = this;
        }
    }

    [DisableIn(PrefabKind.PrefabAsset)]
    [Button ("Fill default links", ButtonSizes.Medium), ButtonGroup]
    private void InsertStandardLinks ()
    {
        #if UNITY_EDITOR

        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset (gameObject))
        {
            Debug.LogWarning ($"Can't modify prefab asset");
            return;
        }
        
        links = new List<ObjectChainLink> ();
        
        var linkBack = new ObjectChainLink ();
        linkBack.type = ObjectChainLink.GetKeys ().FirstOrDefault ();
        linkBack.transform = new GameObject ("link_back").transform;
        linkBack.transform.parent = transform;
        linkBack.transform.SetLocalTransformationToZero ();
        linkBack.transform.localRotation = Quaternion.Euler (0f, 180f, 0f);
        links.Add (linkBack);
        
        var linkForward = new ObjectChainLink ();
        linkForward.type = ObjectChainLink.GetKeys ().FirstOrDefault ();
        linkForward.transform = new GameObject ("link_forward").transform;
        linkForward.transform.parent = transform;
        linkForward.transform.SetLocalTransformationToZero ();
        linkForward.transform.localPosition = new Vector3 (0f, 0f, 9f);
        linkForward.transform.localRotation = Quaternion.Euler (0f, 0f, 0f);
        links.Add (linkForward);

        #endif
    }
    
    [DisableIn(PrefabKind.PrefabAsset)]
    [Button ("Average center", ButtonSizes.Medium), ButtonGroup]
    private void SetAverageCenter ()
    {
        if (links == null || links.Count == 0)
            return;

        var posAverage = Vector3.zero;
        for (int i = 0; i < links.Count; i++)
        {
            var link = links[i];
            if (link == null || link.transform == null)
                continue;

            posAverage += link.transform.localPosition;
        }

        posAverage /= links.Count;
        center = posAverage;
    }
}
