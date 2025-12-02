using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace PhantomBrigade.Data
{
    public class DataBlockCombatStrikeFiring
    {
        public float time;
        public float timeTravel;
        
        [ValueDropdown ("@DataMultiLinkerPartPreset.data.Keys")]
        public string partPresetKey;
        
        public bool transformBasedDirection = false;
        
        [PropertyRange (0f, 1f)]
        public float transformBasedVelocityMultiplier = 0f;
        
        [ValueDropdown ("transformKeys")]
        public List<string> transforms = new List<string> { "front_horizontal" };
        
        public Vector3 startPosition;
        public Vector3 targetPosition;
        public float positionDeviation;
        
        [Min (1)]
        public int repeats = 1;
        public float timeOffset = 0.1f;
        public Vector3 startPositionOffset;
        public Vector3 targetPositionOffset;

        public static List<string> transformKeys = new List<string>
        {
            "bottom",
            "front_horizontal",
            "front_pitched",
            "outer_left",
            "outer_right",
            "top",
            "top_left",
            "top_right"
        };
    }

    public class DataBlockStrikeFunctionTimedProcessed
    {
        public bool completed;
        public float timeNormalized;
        public DataBlockStrikeFunctionTimed source;
    }
    
    public class DataBlockStrikeFunctionTimed
    {
        [PropertyRange (0f, 1f)]
        public float timeNormalized;
        
        [DropdownReference (true)]
        public DataBlockActionFunctionRepeat repeat;
        
        [DropdownReference (true)]
        public string transformOverrideKey;
        
        [DropdownReference]
        public List<ICombatFunctionSpatial> functionsSpatial;
        
        [DropdownReference]
        public List<ICombatFunction> functions;
        
        #if !PB_MODSDK
        public void Run (CombatStrikeHelper.StrikeJob job)
        {
            if (job == null)
                return;
            
            if (functionsSpatial != null && job.visualUsed)
            {
                var tRoot = job.visualInstance.visualLinker.transform;
                var position = tRoot.position;
                var direction = tRoot.forward;
                
                if (!string.IsNullOrEmpty (transformOverrideKey) && job.visualInstance.visualLinker.firingTransforms != null)
                {
                    var transforms = job.visualInstance.visualLinker.firingTransforms;
                    if (transforms.TryGetValue (transformOverrideKey, out var t))
                    {
                        position = t.position;
                        direction = t.forward;
                    }
                }

                foreach (var f in functionsSpatial)
                {
                    if (f != null)
                        f.Run (position, direction);
                }
            }

            if (functions != null)
            {
                foreach (var f in functions)
                {
                    if (f != null)
                        f.Run ();
                }
            }
        }
        #endif
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockStrikeFunctionTimed () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }

    public class DataBlockCombatStrikeVisual
    {
        public Vector3 startPositionLocal = new Vector3 (0f, 100f, -200f);
        public Vector3 startRotationLocal = new Vector3 (0f, 0f, 0f);
        
        public DataBlockResourceStrikeLinker linker = new DataBlockResourceStrikeLinker ();
        public DataBlockResourceAnimationClip animation = new DataBlockResourceAnimationClip ();
        
        [ShowInInspector]
        public List<DataBlockStrikeFunctionTimed> functionsTimed;
    }
    
    public class DataBlockResourceReference<T> where T : Object
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        public T resource;
        
        [ReadOnly]
        public string path;

        public void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR && !PB_MODSDK
            if (resource == null)
            {
                path = string.Empty;
                return;
            };

            var fullPath = UnityEditor.AssetDatabase.GetAssetPath (resource);
            string extension = System.IO.Path.GetExtension (fullPath);
                
            fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
            fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            path = fullPath;
            #endif
        }

        public void OnAfterDeserialization ()
        {
            #if !PB_MODSDK
            resource = !string.IsNullOrEmpty (path) ? Resources.Load<T> (path) : null;
            if (resource == null)
                Debug.LogWarning ($"Failed to load resource of type ({typeof(T).Name}) from path {path}");
            #endif
        }
    }

    public class DataBlockResourceStrikeLinker : DataBlockResourceReference<AssetLinkerCombatStrike> { }
    public class DataBlockResourceAnimationClip : DataBlockResourceReference<AnimationClip> { }

    [Serializable]
    public class DataContainerCombatStrike : DataContainer
    {
        public List<DataBlockCombatStrikeFiring> firings;
        public DataBlockCombatStrikeVisual visual;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (visual != null && visual.linker != null)
                visual.linker.OnAfterDeserialization ();
            
            if (visual != null && visual.animation != null)
                visual.animation.OnAfterDeserialization ();
        }
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();

            if (visual != null && visual.linker != null)
                visual.linker.OnBeforeSerialization ();
            
            if (visual != null && visual.animation != null)
                visual.animation.OnBeforeSerialization ();
        }

        #if !PB_MODSDK
        [Button, PropertyOrder (-1), HideInEditorMode]
        public void Test ()
        {
            if (!Application.isPlaying || !IDUtility.IsGameLoaded () || IDUtility.IsGameState (GameStates.combat))
                return;

            CombatStrikeHelper.AddStrike (this);
        }
        #endif
    }
}