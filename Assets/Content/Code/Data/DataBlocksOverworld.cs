using System;
using System.Collections.Generic;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
	public class DataBlockFSMMessage
	{
		public string msg;
		public float timer;
        public bool stateSpecific;
	}

    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockAI
    {
        public string currentOrder;
        public string previousOrder;

        public string role;
        
        public SortedDictionary<string, string> entityBlackboardNames;
        public SortedDictionary<string, float> dataBlackboardFloat;
        public SortedDictionary<string, Vector3> dataBlackboardVector;

        public string fsmName;
        public string fsmState;
        public List<DataBlockFSMMessage> fsmMessages;
    }
    
    static class AIRoles
    {
        public const string patrol = "patrol";
        public const string response = "response";
        public const string convoy = "convoy";
        public const string assault = "assault";
    }
    
    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockTransform
    {
        public Vector3 position;
        public Vector3 rotation;
        public DataBlockSavedVector3 positionDetected;
    }

    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockIntel
    {
        public bool known;
        public bool recognized;
        public bool visible;
        public bool locked;
        public float sensorContactSeconds;
    }

    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockDetection
    {
        public string targetNameInternal;
        public float tickTimer;
        public float detectionAmount;
        public PlayerDetectionStatus.DetectionState state;
    }
    
    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockFloatCurrentTarget
    {
        public float current;
        public float target;
    }

    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockInventory
    {
        public int supplies;
        public SortedDictionary<string, float> resources;
        public SortedDictionary<string, int> charges;
        public List<DataBlockSavedPart> parts;
        public List<DataBlockSavedSubsystem> subsystems;
    }

    [Serializable]
    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockRanges
    {
        public float sensorRange;
        public float recognitionRange;
        public float visionRange;
    }

    [HideReferenceObjectPicker]
    [HideLabel]
    public class DataBlockViewModelAssetPath
    {
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        public GameObject prefab;
        
        [ReadOnly][GUIColor ("GetPathColor")]
        public string path;
        
        public void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR
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

        public void OnAfterDeserialization (string key)
        {
            prefab = !string.IsNullOrEmpty (path) ? Resources.Load<GameObject> (path) : null;
            if (prefab == null)
                Debug.LogWarning ($"Failed to find visual prefab at path [{path}] for overworld entity config {key}");
        }
        
        #if UNITY_EDITOR
        
        private Color GetPathColor () => 
            Color.HSVToRGB (!string.IsNullOrEmpty (path) ? 0.55f : 0f, 0.5f, 1f);
        
        #endif
    }

    [Serializable]
    public class DataBlockInvocationCheck
    {
        [LabelWidth (200)]
        public bool sourceInvokedEncounter;

        [LabelWidth (200)]
        public bool otherInvokedEncounter;
    }

    [Serializable]
    public class DataBlockFactionCheck
    {
        [BoxGroup ("8", false)]
        [LabelWidth (200)]
        public bool sourceHasFaction;

        [BoxGroup ("8", false)]
        [LabelWidth (200)]
        public bool otherHasFaction;

        [BoxGroup ("8/1", false)]
        [LabelWidth (200)]
        [ShowIf ("otherHasFaction")]
        public bool defendedCheck;

        [BoxGroup ("8/1", false)]
        [LabelWidth (200)]
        [ShowIf ("otherHasFaction")]
        [ShowIf ("defendedCheck")]
        public bool otherIsDefended;

        [BoxGroup ("8/2", false)]
        [ShowIf ("@sourceHasFaction && otherHasFaction")]
        [LabelWidth (200)]
        public bool relationshipCheck;

        [BoxGroup ("8/2", false)]
        [LabelWidth (200)]
        [ShowIf ("relationshipCheck")]
        public Relationships relationship;
    }

    [Serializable]
    public class DataBlockPlayerCheck
    {
        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool sourceIsPlayer;
    }

    [Serializable]
    public class DataBlockUnitsCheck
    {
        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool sourceHasUnits;

        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool otherHasUnits;
    }

    [Serializable]
    public class DataBlockStructureCheck
    {
        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool sourceIsStructure;

        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool otherIsStructure;
    }

    [Serializable]
    public class DataBlockInventoryCheck
    {
        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool otherHasInventory;
    }

    [Serializable]
    public class DataBlockPilotsCheck
    {
        [BoxGroup ("0", false)]
        [LabelWidth (200)]
        public bool otherHasPilots;
    }

    public enum Relationships
    {
        unknown,
        neutral,
        friendly,
        hostile
    }
}
