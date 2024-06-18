using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[LabelWidth (120f)]
[Serializable]
public class ObjectChainPrefabLink
{
    [AssetsOnly]
    [InlineButton ("Select")]
    [InlineButton ("Spawn")]
    public ObjectChainHelper asset;
    
    [NonSerialized]
    public ObjectChainManager parent;

    [PropertyRange (0, "GetOriginIndexLimit")]
    public int originIndex;

    private void Select ()
    {
        if (parent != null)
            parent.prefabSelected = this;
    }
    
    private void Spawn ()
    {
        if (parent != null)
            parent.SpawnAndAttach (asset);
    }

    public int GetOriginIndexLimit ()
    {
        if (asset == null || asset.links == null)
            return 0;
        else
            return asset.links.Count - 1;
    }
}

[LabelWidth (120f)]
[Serializable]
public class ObjectChainInstanceLink
{
    [InlineButton ("Select", ShowIf = "IsSelectButtonVisible")]
    public ObjectChainHelper component;
    
    [NonSerialized]
    public ObjectChainManager parent;

    private void Select ()
    {
        if (parent != null)
            parent.instanceLinkSelected = this;
    }

    private bool IsSelectButtonVisible ()
    {
        return 
            parent != null && 
            parent.instanceLinkSelected != this;
    }
}

[LabelWidth (120f)]
[ExecuteInEditMode]
public class ObjectChainManager : MonoBehaviour
{
    private const string fgReplacement = "Replacement";
    private const string fgConnections = "Connections";
    private const string hgReplacementPattern = "Replacement/Pattern";
    [Space(10)]
    [NonSerialized, ShowInInspector, HideReferenceObjectPicker, HideLabel, FoldoutGroup ("Selection", true)]
    public ObjectChainInstanceLink instanceLinkSelected;
    
    [FoldoutGroup (fgConnections, true)]
    [NonSerialized, ShowInInspector]
    [LabelText ("Recursive")]
    public bool connectionFixRecursive;
    
    [FoldoutGroup (fgConnections)]
    [Button ("Fix / Reattach"), ButtonGroup]
    private void FixConnectionsFromSelectionFlat ()
    {
        if (instanceLinkSelected == null || instanceLinkSelected.component == null)
            return;
        
        FixConnections (instanceLinkSelected.component, connectionFixRecursive, null);
    }

    [FoldoutGroup (fgReplacement, true)]
    [NonSerialized, ShowInInspector]
    [LabelText ("Reattach")]
    public bool replacementReattachment;
    
    [FoldoutGroup (fgReplacement)]
    [NonSerialized, ShowInInspector]
    [LabelText ("Generate Key")]
    public bool replacementKeyGeneration;

    [FoldoutGroup (fgReplacement)]
    [NonSerialized, HideIf ("replacementKeyGeneration"), ShowInInspector]
    [ValueDropdown ("GetReplacementKeys")]
    [LabelText ("Key")]
    public string replacementKey = string.Empty;
    
    [HorizontalGroup (hgReplacementPattern)]
    [NonSerialized, ShowIf ("replacementKeyGeneration"), ShowInInspector]
    [ValueDropdown ("replacementPatternOptions")]
    [LabelText ("Key Change (From/To)")]
    public string replacementFrom = string.Empty;
    
    [HorizontalGroup (hgReplacementPattern)]
    [NonSerialized, ShowIf ("replacementKeyGeneration"), ShowInInspector]
    [ValueDropdown ("replacementPatternOptions")]
    [HideLabel]
    public string replacementTo = string.Empty;
    
    [FoldoutGroup (fgReplacement)]
    [ShowIf ("IsReplaceButtonVisible")]
    [Button]
    private void Replace ()
    {
        if (instanceLinkSelected == null || instanceLinkSelected.component == null)
            return;

        var replacementKeyFinal = replacementKey;
        if (replacementKeyGeneration)
            replacementKeyFinal = instanceLinkSelected.component.name.Replace (replacementFrom, replacementTo);

        ReplaceFromPrefab (instanceLinkSelected, replacementKeyFinal, replacementReattachment);
    }



    [Space(10)]
    [AssetsOnly]
    [ListDrawerSettings (IsReadOnly = true, DefaultExpandedState = false, AlwaysAddDefaultValue = true)]
    [OnValueChanged ("UpdatePrefabList")]
    public List<ObjectChainPrefabLink> prefabs;

    
    [ListDrawerSettings (IsReadOnly = true, DefaultExpandedState = false, AlwaysAddDefaultValue = true, CustomRemoveElementFunction = "RemoveInstance")]
    [OnValueChanged ("UpdateInstanceList")]
    public List<ObjectChainInstanceLink> instances;

    [NonSerialized]
    public ObjectChainPrefabLink prefabSelected;



    public Transform placementHelper;
    
    
    
    
    private List<string> replacementKeys = new List<string> ();
    private List<string> replacementPatternOptions = new List<string> { "2x", "3x", "4x", "5x" };

    private List<string> GetReplacementKeys ()
    {
        if (prefabs != null && prefabs.Count != replacementKeys.Count)
            RefreshReplacementKeys ();
        return replacementKeys;
    }
    
    private void RefreshReplacementKeys ()
    {
        replacementKeys.Clear ();
        foreach (var prefab in prefabs)
        {
            if (prefab != null)
                replacementKeys.Add (prefab.asset.name);
        }
    }
    
    private bool IsReplaceSectionVisible ()
    {
        if (instanceLinkSelected == null || instanceLinkSelected.component == null)
            return false;
        
        return true;
    }
    
    private bool IsReplaceButtonVisible ()
    {
        if (instanceLinkSelected == null || instanceLinkSelected.component == null)
            return false;

        if (replacementKeyGeneration)
        {
            if (string.IsNullOrEmpty (replacementFrom) || string.IsNullOrEmpty (replacementTo))
                return false;

            if (replacementFrom == replacementTo)
                return false;
            
            if (!instanceLinkSelected.component.name.Contains (replacementFrom))
                return false;
        }
        else
        {
            if (string.IsNullOrEmpty (replacementKey))
                return false;

            if (instanceLinkSelected.component.name == replacementKey)
                return false;
        }

        return true;
    }
    
    

    private void OnEnable ()
    {
        UpdatePrefabList ();
        UpdateInstanceList ();
    }

    public void UpdatePrefabList ()
    {
        if (prefabs != null)
        {
            for (int i = 0; i < prefabs.Count; i++)
            {
                var prefab = prefabs[i];
                if (prefab == null)
                    continue;

                prefab.parent = this;
            }
        }
    }

    [Button]
    public void ResetInstances ()
    {
        instances.Clear ();
        var t = GetComponentsInChildren<ObjectChainHelper> ();
        foreach (var tx in t)
        {
            instances.Add (new ObjectChainInstanceLink
            {
                component = tx,
                parent = this
            });
        }
    }
    
    public void UpdateInstanceList ()
    {
        if (instances != null)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                if (instance == null)
                    continue;

                instance.parent = this;
            }
        }
    }

    public void TrySelectInstance (ObjectChainHelper instance)
    {
        if (instances == null || instance == null)
            return;

        foreach (var instanceLink in instances)
        {
            if (instanceLink == null || instanceLink.component != instance)
                continue;

            instanceLinkSelected = instanceLink;
            break;
        }
    }

    public ObjectChainHelper SpawnSelectedPrefab (ObjectChainLink link, bool flip = false)
    {
        if (prefabSelected == null)
        {
            Debug.LogWarning ("No prefab selected");
            return null;
        }
        
        if (prefabSelected.asset == null)
        {
            Debug.LogWarning ("Selected prefab link has no asset reference");
            return null;
        }

        if (link == null)
        {
            Debug.LogWarning ("Failed to spawn prefab due to missing link reference");
            return null;
        }
        
        var instance = SpawnAndAttach (prefabSelected.asset, link, prefabSelected.originIndex, flip);
        return instance;
    }

    public void ReplaceFromPrefab (ObjectChainInstanceLink instanceLink, string prefabName, bool reattach)
    {
        if (instanceLink == null || instanceLink.component == null)
        {
            Debug.LogWarning ($"Can't replace instance: no reference provided");
            return;
        }

        ObjectChainHelper prefab = null;
        foreach (var prefabLink in prefabs)
        {
            if (prefabLink != null && prefabLink.asset != null && prefabLink.asset.name == prefabName)
            {
                prefab = prefabLink.asset;
                break;
            }
        }
        
        if (prefab == null)
        {
            Debug.LogWarning ($"Can't replace instance {instanceLink.component.name}: no prefab with name {prefabName} found");
            return;
        }
        
        Debug.Log ($"Trying to replace instance {instanceLink.component.name} from prefab {prefabName}");
        
        #if UNITY_EDITOR
        var instance = UnityEditor.PrefabUtility.InstantiatePrefab (prefab, transform) as ObjectChainHelper;
        EditorUtility.SetDirty (transform);
        #else
        var instance = GameObject.Instantiate (prefab, transform) as ObjectChainHelper;
        #endif

        if (instance == null)
        {
            Debug.LogWarning ($"Failed to get instance using prefab {prefab.name}");
            return;
        }
        
        var instanceOld = instanceLink.component;

        instance.name = prefab.name;
        instanceLink.component = instance;

        var transformOld = instanceOld.transform;
        var transformNew = instance.transform;
        
        transformNew.position = transformOld.position;
        transformNew.rotation = transformOld.rotation;
        transformNew.localScale = transformOld.localScale;

        // Try to carry over attachment refs
        foreach (var link in instance.links)
        {
            bool linkMatched = false;
            foreach (var linkOld in instanceOld.links)
            {
                if (!string.Equals (linkOld.transform.name, link.transform.name))
                    continue;

                linkMatched = true;
                if (linkOld.attachment == null)
                    continue;
                
                Debug.Log ($"Migrated attachment to {linkOld.attachment.name} on link {linkOld.transform.name} to new instance");
                link.attachment = linkOld.attachment;

                foreach (var linkCounterpart in linkOld.attachment.links)
                {
                    if (linkCounterpart.attachment == instanceOld)
                        linkCounterpart.attachment = instance;
                }
            }
            
            if (!linkMatched)
                Debug.LogWarning ($"Skipping attachment migration on link {link.transform.name}, could not find anything with that name in old instance");
        }
        
        DestroyImmediate (instanceOld.gameObject);
        
        if (reattach)
            FixConnections (instance, true, null);
    }
    
    public ObjectChainHelper SpawnAndAttach (ObjectChainHelper prefab, ObjectChainLink linkHost = null, int originIndex = 0, bool flip = false, bool checkType = true)
    {
        bool linkHostPresent = linkHost != null;
        if (linkHostPresent)
        {
            if (linkHost.attachment != null)
            {
                Debug.LogWarning ($"Can't spawn object at chosen link due to existing attachment");
                return null;
            }

            if (linkHost.transform == null)
            {
                Debug.LogWarning ($"Can't spawn object at chosen link due to missing transform");
                return null;
            }
        }

        if (prefab == null)
        {
            Debug.LogWarning ($"Can't spawn object due to null prefab");
            return null;
        }

        if (prefab.links == null)
        {
            Debug.LogWarning ($"Can't spawn object due to missing links in prefab {prefab.name}");
            return null;
        }
        
        if (!originIndex.IsValidIndex (prefab.links))
        {
            Debug.LogWarning ($"Can't spawn object due to origin link index {originIndex} being invalid for selected prefab {prefab.name}");
            return null;
        }

        var linkOrigin = prefab.links[originIndex];
        if (linkOrigin == null)
        {
            Debug.LogWarning ($"Can't spawn object due to missing link {originIndex} in prefab {prefab.name}");
            return null;
        }

        if (checkType && linkHost != null && linkHost.type != linkOrigin.type)
        {
            Debug.LogWarning ($"Can't spawn object due to origin link type {linkOrigin.type} not matching host link type {linkHost.type}");
            return null;
        }

        #if UNITY_EDITOR
        var instance = UnityEditor.PrefabUtility.InstantiatePrefab (prefab, transform) as ObjectChainHelper;
        EditorUtility.SetDirty (transform);
        #else
        var instance = GameObject.Instantiate (prefab, transform) as ObjectChainHelper;
        #endif

        if (instance == null)
        {
            Debug.LogWarning ($"Failed to get instance using prefab {prefab.name}");
            return null;
        }
        
        instance.name = prefab.name;
        if (instances == null)
            instances = new List<ObjectChainInstanceLink> ();
        instances.Add (new ObjectChainInstanceLink { component = instance });
        
        ProcessAttachment (instance, linkHost, originIndex, flip, checkType);
        
        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo (instance.gameObject, $"Created chained object {instance.name}");
        if (linkHost != null && linkHost.parent != null)
            PrefabUtility.RecordPrefabInstancePropertyModifications (linkHost.parent);
        #endif
        
        return instance;
    }
    
    public void ProcessAttachment (ObjectChainHelper instance, ObjectChainLink linkHost = null, int originIndex = 0, bool flip = false, bool checkType = true)
    {
        if (instance == null)
        {
            Debug.LogWarning ($"Can't attach object: no instance provided");
            return;
        }

        if (instance.links == null)
        {
            Debug.LogWarning ($"Can't attach object {instance.name} due to missing links");
            return;
        }
        
        if (!originIndex.IsValidIndex (instance.links))
        {
            Debug.LogWarning ($"Can't attach object {instance.name} due to origin link index {originIndex} being invalid for link set size {instance.links.Count}");
            return;
        }

        var linkOrigin = instance.links[originIndex];
        if (linkOrigin == null)
        {
            Debug.LogWarning ($"Can't attach object due to missing link {originIndex} in prefab {instance.name}");
            return;
        }
        
        if (linkHost == null)
        {
            Debug.LogWarning ($"Can't attach object due to lack of host link");
            return;
        }
        
        if (linkHost.attachment != null && linkHost.attachment != instance)
        {
            Debug.LogWarning ($"Can't attach object at chosen link due to existing attachment");
            return;
        }

        if (linkHost.transform == null)
        {
            Debug.LogWarning ($"Can't attach object at chosen link due to missing transform");
            return;
        }

        if (checkType && linkHost.type != linkOrigin.type)
        {
            Debug.LogWarning ($"Can't attach object due to origin link type {linkOrigin.type} not matching host link type {linkHost.type}");
            return;
        }

        linkOrigin = instance.links[originIndex];
        linkHost.attachment = instance;
        linkOrigin.attachment = linkHost.parent;

        // Origin relative placement
        // Quick and dirty transform manipulation, this should ideally be switched to independent ops on position and rotation

        instance.transform.parent = transform;
        instance.transform.SetLocalTransformationToZero ();
        if (flip)
            instance.transform.localScale = new Vector3 (-1f, 1f, 1f);
        
        placementHelper.position = linkOrigin.transform.position;
        placementHelper.forward = -linkOrigin.transform.forward;
        instance.transform.parent = placementHelper;
        
        placementHelper.position = linkHost.transform.position;
        placementHelper.rotation = linkHost.transform.rotation;
        instance.transform.parent = transform;

        // Fixing imprecise scale
        var s = instance.transform.localScale;
        instance.transform.localScale = new Vector3 (Mathf.RoundToInt (s.x), Mathf.RoundToInt (s.y), Mathf.RoundToInt (s.z));
    }

    public void RemoveInstance (ObjectChainInstanceLink instanceLink)
    {
        instances.Remove (instanceLink);

        var instance = instanceLink.component;
        if (instance == null)
            return;

        ObjectChainHelper instanceToSelect = null;

        if (instance.links != null)
        {
            foreach (var link in instance.links)
            {
                if (link == null || link.attachment == null)
                    continue;

                var instanceOther = link.attachment;
                if (instanceOther.links == null)
                    continue;

                foreach (var linkOther in instanceOther.links)
                {
                    if (linkOther == null || linkOther.attachment == null)
                        continue;

                    if (linkOther.attachment == instance)
                    {
                        linkOther.attachment = null;
                        instanceToSelect = instanceOther;
                    }
                }
            }
        }
        
        DestroyImmediate (instance.gameObject);
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty (transform);
        #endif

        if (instanceToSelect != null)
        {
            foreach (var instanceOther in instances)
            {
                if (instanceOther == null)
                    continue;

                if (instanceOther.component == instanceToSelect)
                    instanceLinkSelected = instanceOther;
            }
        }
    }

    public void RemoveInstanceAttachedToLink (ObjectChainLink linkStart, bool continueRecursively)
    {
        if (linkStart == null || linkStart.attachment == null)
            return;

        var helperStart = linkStart.parent;
        if (helperStart == null)
            return;

        var helperOther = linkStart.attachment;
        if (helperOther.links != null && continueRecursively)
        {
            foreach (var linkOther in helperOther.links)
            {
                // Avoid going back
                if (linkOther == null || linkOther.attachment == null || linkOther.attachment == helperStart)
                    continue;
                
                RemoveInstanceAttachedToLink (linkOther, true); 
            }
        }

        if (instances != null)
        {
            for (int i = instances.Count - 1; i >= 0; --i)
            {
                var instanceLink = instances[i];
                if (instanceLink == null || instanceLink.component != helperOther)
                    continue;

                RemoveInstance (instanceLink);
            }
        }
    }
    
    #if UNITY_EDITOR

    [PropertyOrder (-1), InlineButton ("SortPrefabList", "Sort")]
    [NonSerialized, ShowInInspector, AssetsOnly]
    [FoldoutGroup ("Prefab drop-off", false)]
    [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
    [OnValueChanged ("PopulatePrefabList")]
    public List<ObjectChainHelper> prefabsTemp = new List<ObjectChainHelper> ();
    
    public static List<ObjectChainPrefabLink> prefabSubset = new List<ObjectChainPrefabLink> ();
    
    private void PopulatePrefabList ()
    {
        if (prefabsTemp == null)
            return;
            
        if (prefabs == null)
            prefabs = new List<ObjectChainPrefabLink> ();
    
        foreach (var prefabCandidate in prefabsTemp)
        {
            if (prefabCandidate == null)
                continue;

            bool registered = false;
            foreach (var prefabLink in prefabs)
            {
                if (prefabLink == null || prefabLink.asset != prefabCandidate)
                    continue;
                
                Debug.LogWarning ($"Skipping addition of {prefabCandidate.name} due to it already being registered");
                registered = true;
                break;
            }
            
            if (!registered)
                prefabs.Add (new ObjectChainPrefabLink { asset = prefabCandidate });
        }

        for (int i = prefabs.Count - 1; i >= 0; --i)
        {
            var prefabLink = prefabs[i];
            if (prefabLink.asset == null)
                prefabs.RemoveAt (i);
        }

        prefabsTemp.Clear ();
        SortPrefabList ();
        prefabSubset.Clear ();
    }
    
    #endif

    private void SortPrefabList ()
    {
        prefabs.Sort ((x, y) => String.Compare (x.asset.name, y.asset.name, StringComparison.Ordinal));
        prefabSelected = prefabs.FirstOrDefault ();
    }

    private void FixConnections (ObjectChainHelper instanceStart, bool continueRecursively, ObjectChainHelper instanceOrigin)
    {
        if (instanceStart == null || instanceStart.links == null)
            return;
        
        for (int i = 0; i < instanceStart.links.Count; ++i)
        {
            var linkStart = instanceStart.links[i];
            if (linkStart.attachment == null || linkStart.attachment == instanceOrigin || linkStart.transform == null)
                continue;

            var instanceOther = linkStart.attachment;
            if (instanceOther.links == null)
                continue;

            ObjectChainLink linkCounterpart = null;
            int linkCounterpartIndex = 0;
            bool linkCounterpartFlipped = instanceOther.transform.localScale.x < 0f;
            
            for (int o = 0; o < instanceOther.links.Count; ++o)
            {
                var linkOther = instanceOther.links[o];
                if (linkOther == null || linkOther.attachment != instanceStart)
                    continue;

                linkCounterpart = linkOther;
                linkCounterpartIndex = o;
                break;
            }

            if (linkCounterpart == null)
            {
                Debug.LogWarning ($"Attempting to fix missing connection from {instanceStart.name}/{linkStart.transform.name} ({instanceStart.GetInstanceID ()}) to {instanceOther.name} ({instanceOther.GetInstanceID ()})");
                var linkStartPos = linkStart.transform.position;
                
                for (int o = 0; o < instanceOther.links.Count; ++o)
                {
                    var linkOther = instanceOther.links[o];
                    if (linkOther == null || linkOther.attachment != null || linkOther.transform == null)
                        continue;

                    var linkOtherPos = linkOther.transform.position;
                    if ((linkOtherPos - linkStartPos).magnitude < 0.01f)
                    {
                        Debug.LogWarning ($"Found a link for missing connection: {instanceOther.name}/{linkOther.transform.name} ({instanceOther.GetInstanceID ()})");
                        linkOther.attachment = instanceStart;
                        linkCounterpart = linkOther;
                        linkCounterpartIndex = o;
                        break;
                    }
                }
            }

            if (linkCounterpart == null)
                continue;
            
            ProcessAttachment (instanceOther, linkStart, linkCounterpartIndex, linkCounterpartFlipped, false);

            if (continueRecursively)
            {
                FixConnections (instanceOther, true, instanceStart);
            }
        }
    }
}
