using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker][LabelWidth (160f)]
    public class DataContainerAssetPool : DataContainer
    {
        #if !PB_MODSDK
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        // [GUIColor ("GetPrefabColor")]
        public AssetLinker prefab;
        #endif
        
        #if !PB_MODSDK
        [ReadOnly]
        #endif
        [GUIColor ("GetPathColor")]
        public string path;
        
        [InfoBox ("When limit is set to 0, the object will only be available for direct instantiation via GetInstanceStandalone method - no pool would be created", InfoMessageType.Warning, "IsLimitInvalid")]
        [PropertyRange (0, 300)]
        public int limit;

        [HideIf ("IsLimitInvalid")]
        public bool thresholdsActivated = false;
        
        [ShowIf ("@thresholdsActivated && !IsLimitInvalid")]
        [LabelText ("Min. Distance")]
        public float thresholdDist = 0f;
        
        [ShowIf ("@thresholdsActivated && !IsLimitInvalid"), OnValueChanged("OnThresholdChanged")]
        [LabelText ("Min. Time Interval")]
        public float timeThreshold = 0f;

        [HideIf ("IsLimitInvalid")]
        public bool resetOnActivation = false;

        public bool clearInactiveChildren = true;

        public bool lifetimeUsed = false;
        
        [ShowIf ("lifetimeUsed")][InlineButton ("GetLifetimeFromPrefab", "Find")]
        public float lifetime = 0f;

        #if !PB_MODSDK
        
        [YamlIgnore][ReadOnly][HideInEditorMode]
        public Transform holder;
        
        [YamlIgnore][ReadOnly][HideInEditorMode][ShowInInspector]
        public int instanceCountUsed = 0;
        
        [YamlIgnore][ReadOnly][HideInEditorMode][ShowInInspector]
        public int instanceIndexNext = 0;

        [YamlIgnore][ReadOnly][HideInEditorMode][ShowInInspector]
        public List<AssetLinker> instances;
        
        [YamlIgnore][ReadOnly][HideInEditorMode][ShowInInspector]
        public List<AssetLinker> instancesStandalone;

        [YamlIgnore][ReadOnly][HideInEditorMode]
        public Vector3 lastRequestPosition = Vector3.negativeInfinity;

        [YamlIgnore][ReadOnly][HideInEditorMode]
        public float lastRequestTime = float.NegativeInfinity;
        
        #endif
        
        [YamlIgnore][ReadOnly][HideInEditorMode]
        public float positionThresholdDistSq = 0f;


        public override void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR && !PB_MODSDK
            if (prefab == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (prefab);
            string extension = System.IO.Path.GetExtension (fullPath);
            
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        
        
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            #if !PB_MODSDK
            prefab = !string.IsNullOrEmpty (path) ? Resources.Load<AssetLinker> (path) : null;
            if (prefab == null)
            {
                Debug.LogWarning ($"Failed to load pooled asset prefab from path [{path}] for config {key}");
                return;
            }

            if (!Application.isPlaying || limit <= 0)
                return;
            
            if (instances == null)
                instances = new List<AssetLinker> (limit);
            else
            {
                foreach (var instance in instances)
                {
                    if (instance == null)
                        continue;

                    instance.OnReturn ();
                }
                
                instances.Clear ();
            }

            if (holder == null)
            {
                holder = new GameObject (key).transform;
                holder.parent = DataMultiLinkerAssetPools.GetHolder ();
            }
            else
            {
                UtilityGameObjects.ClearChildren (holder);
            }

            instanceIndexNext = 0;

            for (int i = 0; i < limit; ++i)
            {
                var instance = GameObject.Instantiate (prefab, holder, false);
                instance.name = prefab.name;
                instance.Setup (key);
                instance.lifetimeUsed = lifetimeUsed;
                instance.lifetime = lifetime;
                instance.gameObject.SetActive (false);
                instance.Stop ();
                instances.Add (instance);

                if (clearInactiveChildren)
                {
                    var t = instance.transform;
                    int childCount = t.childCount;
                    if (childCount > 0)
                    {
                        for (int c = childCount - 1; c >= 0; --c)
                            ClearInactiveRecursive (t.GetChild (c), null);
                    }
                }
            }

            OnThresholdChanged ();
            #endif
        }
        
        #if !PB_MODSDK
        private void ClearInactiveRecursive (Transform parent, List<GameObject> exceptions)
        {
            int childCount = parent.childCount;
            if (childCount > 0)
            {
                for (int c = childCount - 1; c >= 0; --c)
                {
                    var child = parent.GetChild (c);
                    ClearInactiveRecursive (child, exceptions);
                }
            }

            var go = parent.gameObject;
            if (!go.activeSelf)
            {
                bool destroy = exceptions == null || !exceptions.Contains (go);
                if (destroy)
                    GameObject.Destroy (go);
            }
        }

        public bool IsInstanceAvailable (bool blockReuse)
        {
            if (instances == null)
                return false;
            
            // If reuse is blocked and everything is checked out, return nothing
            if (blockReuse && instanceCountUsed >= limit)
            {
                // Debug.LogWarning ($"Can't return asset {key}, reuse blocked | Instance count used/total: {instanceCountUsed}/{limit}");
                return false;
            }

            return true;
        }

        public AssetLinker GetInstanceStandalone ()
        {
            if (prefab == null)
            {
                Debug.LogWarning ($"Failed to instantiate standalone prefab instance from pool {key} - no reference to prefab available (path: {path})");
                return null;
            }
            
            var instance = GameObject.Instantiate (prefab);
            instance.name = prefab.name;
            instance.Setup (key);
            instance.Stop ();
            instance.standalone = true;

            if (instancesStandalone == null)
                instancesStandalone = new List<AssetLinker> ();
            instancesStandalone.Add (instance);
            
            return instance;
        }

        public AssetLinker GetInstance (bool blockReuse = false)
        {
            bool instanceAvailable = IsInstanceAvailable (blockReuse);
            if (!instanceAvailable)
                return null;

            // Used when reuse is blocked
            instanceCountUsed += 1;

            // When checking out, we advance the buffer selector index forward by 1
            var instance = instances[instanceIndexNext];
            instanceIndexNext += 1;
            if (instanceIndexNext >= limit)
                instanceIndexNext = 0;

            instance.OnReturn ();
            instance.SetActive (true);
            instance.Stop (resetOnActivation);

            return instance;
        }

        public void ReturnInstance (AssetLinker instance, bool reinsertAsNext = true, bool forceParentDeactivation = false)
        {
            if (instance == null)
                return;

            if (instance.standalone)
            {
                Debug.LogWarning ($"Destroying standalone asset {instance.name} on return");
                GameObject.Destroy (instance.gameObject);
                return;
            }
            
            // Used when reuse is blocked
            instanceCountUsed -= 1;
            
            instance.OnReturn ();
            instance.transform.parent = holder;
            instance.SetActive (false, forceParentDeactivation);
            instance.Stop ();

            // This step ensures that a returned instance will always be the next instance to be checked out
            if (reinsertAsNext)
            {
                // X - instance that's checked out (active)
                // O - instance that hasn't been checked out
                // Q - instance getting returned
                // ▲ - next checkout index (selector index)
                // ↑ - index of instance that's getting returned
                //
                // XXXXQXXXOO
                //     ↑   ▲
                // Find where instance is currently at (likely between other in-use instances)
                int indexCurrent = instances.IndexOf (instance);
                
                // XXXXQXXXOO
                //        ▲←
                // If multiple instances get returned in a sequence, shifting selector back by -1
                // ensures we're creating a stretch of free instances to be checked out
                instanceIndexNext -= 1;
                if (instanceIndexNext < 0)
                    instanceIndexNext = instances.Count - 1;

                //     ┌──┐
                // XXXX↓XX↑OO  →  XXXXXXXQOO
                //     └──┘              ▲
                // Swap places, making the instance we just returned the next in line to be checked out
                var instanceNext = instances[instanceIndexNext];
                instances[indexCurrent] = instanceNext;
                instances[instanceIndexNext] = instance;
            }
        }
        
        public void ReturnAll ()
        {
            if (instances == null)
                return;

            // When returning every instance at once, there is no point in reinserting each entry
            foreach (var instance in instances)
                ReturnInstance (instance, false, true);
            
            instanceCountUsed = 0;
            instanceIndexNext = 0;
        }

        public void ReturnAllInstanceStandalone ()
        {
            if (instancesStandalone != null)
            {
                foreach (var instance in instancesStandalone)
                    ReturnInstance (instance, false, true);
                instancesStandalone.Clear ();
            }
        }
        
        public void SetGraphicsSettingsOnInstances (int level)
        {
            foreach (AssetLinker instance in instances)
            {
                instance.SetGraphicsSettings (level);
            }
        }
        #endif
        
        private void GetLifetimeFromPrefab ()
        {
            #if !PB_MODSDK
            lifetime = -1f;
            
            if (prefab == null)
                return;

            var fxSystem = prefab.GetComponent<FXSystem> ();
            if (fxSystem != null && fxSystem.steps != null && fxSystem.steps.Count > 0)
            {
                foreach (var step in fxSystem.steps)
                {
                    float end = step.startTime + step.duration;
                    if (lifetime < end)
                        lifetime = end;
                }
                
                Debug.Log ($"{key} | Retrieved lifetime from FXSystem component: {lifetime}");
                return;
            }
        
            var fxTween = prefab.GetComponent<FXTween> ();
            if (fxTween != null && fxTween.step != null)
            {
                lifetime = fxTween.step.startTime + fxTween.step.duration;
                Debug.Log ($"{key} | Retrieved lifetime from FXTween component: {lifetime}");
                return;
            }

            var fxChain = prefab.GetComponent<FXParticleChain> ();
            if (fxChain != null && fxChain.steps != null && fxChain.steps.Count > 0)
            {
                foreach (var step in fxChain.steps)
                {
                    float end = step.startTime + step.duration;
                    if (lifetime < end)
                        lifetime = end;
                }
            
                Debug.Log ($"{key} | Retrieved lifetime from FXParticleChain component: {lifetime}");
                return;
            }

            var particleSystem = prefab.GetComponent<ParticleSystem> ();
            if (particleSystem != null)
            {
                lifetime = particleSystem.main.duration;
                Debug.Log ($"Retrieved lifetime from ParticleSystem component: {lifetime}");
                return;
            }
            
            Debug.Log ($"{key} | Failed to find any components that could inform the lifetime");
            #endif
        }

        // Separate method to allow invocation from UI
        private void OnThresholdChanged ()
        {
	        positionThresholdDistSq = thresholdDist * thresholdDist;
        }
        
        #if UNITY_EDITOR
        
        // private Color GetPrefabColor () => 
        //     Color.HSVToRGB (prefab != null ? 0.55f : 0f, 0.5f, 1f);
        
        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);

        private bool IsLimitInvalid =>
            limit <= 0;
        
        #if !PB_MODSDK
        [Button ("Select for testing"), PropertyOrder (-1), HideInEditorMode]
        private void SelectForTest ()
        {
            AssetPoolUtility.assetTestKey = key;
        }
        #endif

        #endif
    }
}

