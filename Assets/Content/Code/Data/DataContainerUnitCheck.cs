using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum EntityCheckMethod
    {
        RequireAll,
        RequireOne
    }
    
    public enum EntityCheckSeverity
    {
        Critical = 2,
        Warning = 1,
        Hint = 0
    }
    
    public enum UnitSubcheckPartRequirement
    {
        Present,
        Damaged,
        Destroyed,
        Tags
    }

    // Extend with damage check once subsystem damage is added
    public enum UnitSubcheckSubsystemRequirement
    {
        Present
    }

    
    public enum UnitSubcheckStatTarget
    {
        Unit,
        Part
    }
    
    public enum UnitSubcheckStatRequirement
    {
        AboveThreshold,
        BelowThreshold,
    }
    
    // Extend with additional checks (experience, health etc.) once pilots are expanded on
    public enum UnitSubcheckPilotRequirement
    {
        Present
    }

    [Serializable][HideReferenceObjectPicker]
    public class DataBlockUnitSubcheckClassTag
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetClassTags ()")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public string classTag;
        
        [HideInInspector]
        public bool not;

        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockUnitSubcheckBlueprint
    {
        [ValueDropdown ("@DataMultiLinkerUnitBlueprint.data.Keys")]
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public string blueprint;
        
        [HideInInspector]
        public bool not;
        
        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockUnitSubcheckPart
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        
        [HideInInspector]
        public bool not;

        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public UnitSubcheckPartRequirement requirement = UnitSubcheckPartRequirement.Present;

        [ShowIf ("IsTagVisible")]
        [DictionaryKeyDropdown ("@DataMultiLinkerSubsystem.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;

        #if UNITY_EDITOR
        private IEnumerable<string> GetGroupNames () { return DataMultiLinkerEquipmentGroup.data.Keys; }
        private bool IsTagVisible => requirement == UnitSubcheckPartRequirement.Tags;
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockUnitSubcheckSubsystem
    {
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        [ValueDropdown ("@DataHelperUnitEquipment.GetExposedHardpoints ()")]
        public string hardpoint;
        
        [HideInInspector]
        public bool not;
        
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public UnitSubcheckSubsystemRequirement requirement;
        
        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockUnitSubcheckStat
    {
        [ValueDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        public string stat;
        public UnitSubcheckStatTarget target;
        public UnitSubcheckStatRequirement requirement;
        [ShowIf ("@target == UnitSubcheckStatTarget.Part")]
        [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        public string socket;
        public float threshold;
    }
    
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockUnitSubcheckPilot
    {
        [HideInInspector]
        public bool not;
        
        [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public UnitSubcheckPilotRequirement requirement;
        
        #if UNITY_EDITOR
        private void Invert () => not = !not;
        private string GetInversionLabel () => not ? "Prohibited" : "Required";
        private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }

    [Serializable]
    public class DataBlockUnitCheck
    {
        public EntityCheckMethod encompassingMethod = EntityCheckMethod.RequireAll;
        
        [ShowIf ("@tags != null && tags.Count > 0")]
        public EntityCheckMethod tagsMethod = EntityCheckMethod.RequireAll;
        
        [ShowIf ("@blueprints != null && blueprints.Count > 0")]
        public EntityCheckMethod blueprintsMethod = EntityCheckMethod.RequireAll;
        
        [ShowIf ("@parts != null && parts.Count > 0")]
        public EntityCheckMethod partsMethod = EntityCheckMethod.RequireAll;
        
        [ShowIf ("@subsystems != null && subsystems.Count > 0")]
        public EntityCheckMethod subsystemsMethod = EntityCheckMethod.RequireAll;
        
        [ShowIf ("@stats != null && stats.Count > 0")]
        public EntityCheckMethod statsMethod = EntityCheckMethod.RequireAll;

        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockUnitSubcheckClassTag> tags;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockUnitSubcheckBlueprint> blueprints;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockUnitSubcheckPart> parts;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockUnitSubcheckSubsystem> subsystems;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockUnitSubcheckStat> stats;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockUnitSubcheckPilot> pilot;

        [YamlIgnore, HideInInspector] 
        public string key;
    }
    
    [Serializable][LabelWidth (150f)]
    public class DataContainerUnitCheck : DataContainerWithText
    {
        [YamlIgnore]
        [LabelText ("Name / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [HideLabel, TextArea]
        public string textDesc;
        
        public bool hidden = false;
        
        public string icon;
        public EntityCheckSeverity severity;

        [HideLabel]
        public DataBlockUnitCheck check = new DataBlockUnitCheck ();

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            check.key = key;
        }

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            if (check.parts == null)
                return;

            // Keeping configs clean of unwanted info
            foreach (var subcheck in check.parts)
            {
                if (subcheck.requirement != UnitSubcheckPartRequirement.Tags)
                    subcheck.tags = null;
            }
        }
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitChecks, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.unitChecks, $"{key}__text");
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.unitChecks, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.unitChecks, $"{key}__text", textDesc);
        }
        
        #endif
        #endregion
    }
}

