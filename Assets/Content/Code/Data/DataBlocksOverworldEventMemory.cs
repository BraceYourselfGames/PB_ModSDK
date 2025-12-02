using System;
using System.Collections;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum DictionaryEntryChangeInt
    {
        Bool,
        Remove,
        Set,
        Offset
    }
    
    public enum DictionaryEntryChangeFloat
    {
        Remove,
        Add,
        Set,
        Offset
    }
    
    
    
    public enum MemoryChangeContextEvent
    {
        Source,
        SourceProvince,
        Target,
        TargetProvince,
        SpecificProvince,
        ActorUnit,
        ActorPilot,
        ActorWorld
    }
    
    /// <summary>
    /// New unified memory change block for events
    /// </summary>
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockMemoryChangeGroupEvent
    {
        public bool visible = true;
        
        public MemoryChangeContextEvent context = MemoryChangeContextEvent.Source;

        [ShowIf ("IsActorKeyVisible")]
        [ValueDropdown ("GetActorKeys")]
        public string actorKey;
        
        [ShowIf ("IsProvinceKeyVisible")]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        public string provinceKey;
        
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> () { new DataBlockMemoryChangeFloat () };

        #region Editor
        #if UNITY_EDITOR
        
        private bool IsActorKeyVisible => 
            context == MemoryChangeContextEvent.ActorUnit || context == MemoryChangeContextEvent.ActorPilot || context == MemoryChangeContextEvent.ActorWorld;

        private bool IsProvinceKeyVisible =>
            context == MemoryChangeContextEvent.SpecificProvince;

        #endif
        #endregion
    }
    
    
    
    
    public enum MemoryChangeContextAction
    {
        Source,
        SourceProvince,
        Target,
        TargetProvince,
        SpecificProvince
    }
    
    /// <summary>
    /// New unified memory change block for overworld entity actions
    /// </summary>
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockMemoryChangeGroupAction
    {
        public MemoryChangeContextAction context = MemoryChangeContextAction.Source;

        [ShowIf ("IsProvinceKeyVisible")]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        public string provinceKey;
        
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> ();

        #region Editor
        #if UNITY_EDITOR

        private bool IsProvinceKeyVisible =>
            context == MemoryChangeContextAction.SpecificProvince;

        #endif
        #endregion
    }
    
    
    
    
    public enum MemoryChangeContextScenario
    {
        Source,
        Target,
        Province,
        SpecificProvince
    }
    
    /// <summary>
    /// New unified memory change container for scenarios
    /// </summary>
    [Serializable] [HideReferenceObjectPicker]
    public class DataBlockMemoryChangeGroupScenario
    {
        public MemoryChangeContextScenario context = MemoryChangeContextScenario.Source;

        [ShowIf ("IsProvinceKeyVisible")]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        public string provinceKey;
        
        [ListDrawerSettings (CustomAddFunction = "@new DataBlockMemoryChangeFloat ()")]
        public List<DataBlockMemoryChangeFloat> changes = new List<DataBlockMemoryChangeFloat> ();

        #region Editor
        #if UNITY_EDITOR

        private bool IsProvinceKeyVisible =>
            context == MemoryChangeContextScenario.SpecificProvince;

        #endif
        #endregion
    }
    

    public static class DataUtilityMemoryChanges
    {
        public static void FillEditableKeyLookup 
        (
            ref Dictionary<MemoryChangeContextEvent, List<string>> keysEditable, 
            HashSet<string> keysRegistered, 
            Type typeAutofilled
        )
        {
            if (keysEditable != null)
                return;

            keysEditable = new Dictionary<MemoryChangeContextEvent, List<string>> ();
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.Source, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.Target, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.SourceProvince, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.TargetProvince, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.SpecificProvince, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.ActorUnit, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.ActorPilot, typeAutofilled);
            FillEditableKeyLookupForContext (ref keysEditable, keysRegistered, MemoryChangeContextEvent.ActorWorld, typeAutofilled);
        }

        private static void FillEditableKeyLookupForContext 
        (
            ref Dictionary<MemoryChangeContextEvent, List<string>> keysEditable,
            HashSet<string> keysRegistered, 
            MemoryChangeContextEvent context, 
            Type typeAutofilled
        )
        {
            var keysForContext = new List<string> ();
            keysEditable.Add (context, keysForContext);

            string keyPrefixExpected = GetContextKeyPrefix (context);
            bool keyPrefixChecked = !string.IsNullOrEmpty (keyPrefixExpected);
            var keysAutofilled = FieldReflectionUtility.GetConstantStringFieldValues (typeAutofilled);
                
            foreach (var key in keysRegistered)
            {
                if (keyPrefixChecked && !key.StartsWith (keyPrefixExpected))
                    continue;
                if (keysAutofilled.Contains (key))
                    continue;
                keysForContext.Add (key);
            }
        }

        public static string GetContextKeyPrefix (MemoryChangeContextEvent context)
        {
            if (context == MemoryChangeContextEvent.Source || context == MemoryChangeContextEvent.Target || context == MemoryChangeContextEvent.ActorWorld)
                return "World";
            if (context == MemoryChangeContextEvent.SourceProvince || context == MemoryChangeContextEvent.TargetProvince || context == MemoryChangeContextEvent.SpecificProvince)
                return "Province";
            if (context == MemoryChangeContextEvent.ActorUnit)
                return "Unit";
            if (context == MemoryChangeContextEvent.ActorPilot)
                return "Pilot";
            return null;
        }
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockMemoryChangeInt
    {
        [HorizontalGroup]
        [HideLabel]
        public DictionaryEntryChangeInt change = DictionaryEntryChangeInt.Offset;
        
        [PropertyOrder (-1)]
        [ValueDropdown ("GetEditableKeys")]
        [HorizontalGroup (0.4f)]
        [HideLabel]
        public string key;
        
        [PropertyOrder (1)]
        [HorizontalGroup (0.15f)]
        [HideLabel]
        [ShowIf ("IsValueVisible")]
        public int value = 1;

        #region Editor
        #if UNITY_EDITOR
        
        [PropertyOrder (2)]
        [HorizontalGroup (0.15f)]
        [HideIf ("@change != DictionaryEntryChangeInt.Bool")]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue ()
        {
            if (value == 0)
                value = 1;
            else
                value = 0;
        }

        private void OnCheckModeChange ()
        {
            if (change == DictionaryEntryChangeInt.Bool)
                value = value > 0 ? 1 : 0;
        }
        
        private string GetBoolLabel => value > 0 ? "True" : "False";
        private Color GetBoolColor => Color.HSVToRGB (value > 0 ? 0.55f : 0f, 0.5f, 1f);
        
        private bool IsValueVisible => 
            change != DictionaryEntryChangeInt.Remove && change != DictionaryEntryChangeInt.Bool;

        private IEnumerable GetEditableKeys => DataMultiLinkerOverworldMemory.data.Keys;

        #endif
        #endregion
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockMemoryChangeFloat
    {
        [HorizontalGroup ("Change")]
        [HideLabel, GUIColor ("GetChangeColor")]
        public DictionaryEntryChangeFloat change = DictionaryEntryChangeFloat.Offset;
        
        [PropertyOrder (-1)]
        [ValueDropdown ("GetEditableKeys")]
        [HideLabel]
        public string key;
        
        [PropertyOrder (1)]
        [HorizontalGroup ("Change", 0.15f)]
        [HideLabel]
        [ShowIf ("IsValueVisible")]
        public float value = 1;
        
        [HideInInspector]
        public bool valueFromMemory = false;
        
        [PropertyOrder (6)]
        [ShowIf ("valueFromMemory")]
        [ValueDropdown ("GetEditableKeys")]
        [HideLabel]
        public string valueFromMemoryKey;

        public override string ToString ()
        {
            string keyText = !string.IsNullOrEmpty (key) ? key : "[no key]";
            var data = DataMultiLinkerOverworldMemory.GetEntry (key, false);
            if (data != null && data.ui != null && !string.IsNullOrEmpty (data.ui.textName))
                keyText = $"{keyText} ({data.ui.textName})";
            
            switch (change)
            {
                case DictionaryEntryChangeFloat.Add:
                    return $"{keyText}: Add";
                case DictionaryEntryChangeFloat.Remove:
                    return $"{keyText}: Remove";
                case DictionaryEntryChangeFloat.Set:
                    return $"{keyText}: Set to {value}";
                case DictionaryEntryChangeFloat.Offset:
                    return $"{keyText}: Offset by {value}";
                default:
                    return $"{keyText}: Unknown change";
            }
        }

        #region Editor
        #if UNITY_EDITOR
        
        private bool IsValueVisible => 
            change != DictionaryEntryChangeFloat.Remove && change != DictionaryEntryChangeFloat.Add;

        private IEnumerable<string> GetEditableKeys ()
        {
            // Ensure memory DB is loaded
            var data = DataMultiLinkerOverworldMemory.data;
            return DataMultiLinkerOverworldMemory.keysEditableSorted;
        }

        private Color GetChangeColor ()
        {
            switch (change)
            {
                case DictionaryEntryChangeFloat.Add:
                    return Color.HSVToRGB (0.4f, 0.5f, 1f);
                case DictionaryEntryChangeFloat.Remove:
                    return Color.HSVToRGB (0f, 0.5f, 1f);
                case DictionaryEntryChangeFloat.Set:
                    return Color.HSVToRGB (0.55f, 0.5f, 1f);
                case DictionaryEntryChangeFloat.Offset:
                    return Color.HSVToRGB (0.45f, 0.5f, 1f);
                default:
                    return Color.white;
            }
        }
        
        [PropertyOrder (5)]
        [HorizontalGroup ("Change", 64f)]
        [ShowIf ("IsValueVisible")]
        [Button ("@GetMemoryLabel"), GUIColor ("GetMemoryColor")]
        private void ToggleValueFromMemory ()
        {
            valueFromMemory = !valueFromMemory;
        }

        private string GetMemoryLabel => valueFromMemory ? "◄ + ▼" : "◄";
        private Color GetMemoryColor => new Color (1f, 1f, 1f, valueFromMemory ? 1f : 0.5f);

        #endif
        #endregion
    }

    public class DataBlockMemoryChangeAdv
    {
        [PropertyOrder (-1)]
        [ValueDropdown ("GetEditableKeys")]
        [LabelText ("Input"), LabelWidth (90f)]
        [InlineButton ("@keyOutput = null", "Change output key", ShowIf = "@string.IsNullOrEmpty (keyOutput)")]
        public string keyInput;
        
        [PropertyOrder (-1), HideIf ("@string.IsNullOrEmpty (keyOutput)")]
        [ValueDropdown ("GetEditableKeys"), InlineButtonClear]
        [LabelText ("Output"), LabelWidth (90f)]
        public string keyOutput = null;
        
        public List<IFloatOperation> operations = new List<IFloatOperation> ();
        
        #region Editor
        #if UNITY_EDITOR

        private IEnumerable<string> GetEditableKeys ()
        {
            // Ensure memory DB is loaded
            var data = DataMultiLinkerOverworldMemory.data;
            return DataMultiLinkerOverworldMemory.keysEditableSorted;
        }

        #endif
        #endregion
    }
}