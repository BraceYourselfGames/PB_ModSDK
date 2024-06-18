using System;
using System.Collections.Generic;
using Content.Code.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockUnitDescriptionDebug
    {
        public string preset;
        public string factionDataSource;
        public string qualityTableKey;
        public int quality;
        
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public Dictionary<string, DataBlockSavedPart> description;
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockSavedPart
    {
        public int version = -1;
        public int serial = -1;
        public int rating = -1;
        public int level = -1;
        public int levelOriginal = -1;
        
        [ValueDropdown ("GetPresetNames")]
        public string preset = string.Empty;
        
        [ValueDropdown ("GetLiveryNames")]
        public string livery = string.Empty;

        [HorizontalGroup, LabelText ("Integrity / Barrier")]
        public float integrity = 1f;
        
        [HorizontalGroup (0.3f), HideLabel]
        public float barrier = 1f;

        public bool salvageable;

        public bool inventoryAdded;
        public bool favorite;

        [ListDrawerSettings (DefaultExpandedState = false)]
        public HashSet<string> sockets;
        
        [ListDrawerSettings (DefaultExpandedState = false)]
        public HashSet<string> hardpoints;
        
        [DictionaryKeyDropdown (DictionaryKeyDropdownType.Hardpoint)]
        public Dictionary<string, DataBlockSavedSubsystem> systems = new Dictionary<string, DataBlockSavedSubsystem> ();

        #if UNITY_EDITOR

        private string socketKey;
        
        private IEnumerable<string> GetLiveryNames () => DataMultiLinkerEquipmentLivery.data.Keys;
        
        private IEnumerable<string> GetPresetNames ()
        {
            if (string.IsNullOrEmpty (socketKey))
                return DataMultiLinkerPartPreset.data.Keys;
            else
                return DataHelperUnitEquipment.GetPartPresetsForSocket (socketKey);
        }

        public void SetInspectorData (string socketKey, bool subsystemLevelUsed, bool subsystemDestroyedUsed, bool subsystemFusedUsed)
        {
            this.socketKey = socketKey;
            foreach (var kvp in systems)
                kvp.Value.SetInspectorData (kvp.Key, subsystemLevelUsed, subsystemDestroyedUsed, subsystemFusedUsed);
        }

        #endif
    }

    [Serializable][HideReferenceObjectPicker][LabelWidth (100f)]
    public class DataBlockSavedSubsystem
    {
        public int serial = -1;
        
        [ValueDropdown ("GetBlueprintNames")]
        public string blueprint;

        [ValueDropdown ("GetLiveryNames")]
        public string livery = string.Empty;

        [ShowIf ("@!string.IsNullOrEmpty (blueprint) && destroyedUsed")]
        public bool destroyed;

        [ShowIf ("@!string.IsNullOrEmpty (blueprint) && fusedUsed")]
        public bool fused;

        public bool salvageable;

        public bool inventoryAdded;
        public bool favorite;

        #if UNITY_EDITOR
        
        private string hardpointKey;
        private bool levelUsed = true;
        private bool destroyedUsed = true;
        private bool fusedUsed = true;
        
        private IEnumerable<string> GetLiveryNames () => DataMultiLinkerEquipmentLivery.data.Keys;
        
        private IEnumerable<string> GetBlueprintNames ()
        {
            if (string.IsNullOrEmpty (hardpointKey))
                return DataMultiLinkerSubsystem.data.Keys;
            else
                return DataHelperUnitEquipment.GetSubsystemsForHardpoint (hardpointKey);
        }

        public void SetInspectorData (string hardpointKey, bool levelUsed, bool destroyedUsed, bool fusedUsed)
        {
            this.hardpointKey = hardpointKey;
            this.levelUsed = levelUsed;
            this.destroyedUsed = destroyedUsed;
            this.fusedUsed = fusedUsed;
        }
                
        #endif
    }


    public abstract class CustomMemoryValue {}

    [TypeHinted]
    public class CustomMemoryString : CustomMemoryValue
    {
        public string value;
    }
    
    [TypeHinted]
    public class CustomMemoryVector : CustomMemoryValue
    {
        public Vector4 value;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockSavedInt
    {
        [LabelText ("Value")]
        public int i;
        
        public DataBlockSavedInt (int i) => this.i = i;
        public DataBlockSavedInt () { }
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockSavedFloat
    {
        [LabelText ("Value")]
        public float f;
        
        public DataBlockSavedFloat (float f) => this.f = f;
        public DataBlockSavedFloat () { }
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockSavedFloatNormalized
    {
        [LabelText ("Value")]
        [PropertyRange (0f, 1f)]
        public float f;
        
        public DataBlockSavedFloatNormalized (float f) => this.f = Mathf.Clamp01 (f);
        public DataBlockSavedFloatNormalized () { }
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockSavedVector3
    {
        [LabelText ("Value")]
        public Vector3 v;
        
        public DataBlockSavedVector3 (Vector3 v) => this.v = v;
        public DataBlockSavedVector3 () { }
    }
}
