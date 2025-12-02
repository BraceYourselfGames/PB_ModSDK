using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
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
        [LabelText ("Tags (from subsystems)")]
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
        
        [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.data.Keys")]
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
        
        #if !PB_MODSDK
        public bool IsTriggered (PersistentEntity unitPersistent, HashSet<EquipmentEntity> partEntities = null)
        {
            if (unitPersistent == null)
                return false;

            bool tagsRelevant = tags != null && tags.Count > 0;
            bool tagsTriggered = false;
            if (tagsRelevant)
            {
                var classTag = unitPersistent.dataKeyUnitClass.s;
                int subchecksTriggered = 0;
                for (int i = 0; i < tags.Count; ++i)
                {
                    if (subchecksTriggered > 0 && tagsMethod == EntityCheckMethod.RequireOne)
                    {
                        tagsTriggered = true;
                        break;
                    }
                    
                    var subcheck = tags[i];
                    bool equal = string.Equals (classTag, subcheck.classTag);
                    bool triggered = subcheck.not ? !equal : equal;
                    if (triggered)
                        subchecksTriggered += 1;
                }
                
                if (subchecksTriggered > 0 && tagsMethod == EntityCheckMethod.RequireOne)
                    tagsTriggered = true;

                if (tagsMethod == EntityCheckMethod.RequireAll && subchecksTriggered == tags.Count)
                    tagsTriggered = true;
            }

            bool blueprintsRelevant = blueprints != null && blueprints.Count > 0;
            bool blueprintsTriggered = false;
            if (blueprintsRelevant)
            {
                var blueprint = unitPersistent.dataKeyUnitBlueprint.s;
                int subchecksTriggered = 0;
                for (int i = 0; i < blueprints.Count; ++i)
                {
                    if (subchecksTriggered > 0 && blueprintsMethod == EntityCheckMethod.RequireOne)
                    {
                        tagsTriggered = true;
                        break;
                    }
                    
                    var subcheck = blueprints[i];
                    bool equal = string.Equals (blueprint, subcheck.blueprint);
                    bool triggered = subcheck.not ? !equal : equal;
                    if (triggered)
                        subchecksTriggered += 1;
                }
                
                if (subchecksTriggered > 0 && blueprintsMethod == EntityCheckMethod.RequireOne)
                    blueprintsTriggered = true;

                if (blueprintsMethod == EntityCheckMethod.RequireAll && subchecksTriggered == blueprints.Count)
                    blueprintsTriggered = true;
            }

            bool isOverworld = !IDUtility.IsGameState (GameStates.combat);
            bool partsRelevant = parts != null && parts.Count > 0;
            bool partsTriggered = false;
            if (partsRelevant)
            {
                int subchecksTriggered = 0;
                for (int i = 0; i < parts.Count; ++i)
                {
                    if (subchecksTriggered > 0 && partsMethod == EntityCheckMethod.RequireOne)
                    {
                        partsTriggered = true;
                        break;
                    }
                    
                    var subcheck = parts[i];
                    var partPersistent = EquipmentUtility.GetPartInUnit (unitPersistent, subcheck.socket, false, partEntities);
                    bool triggered = false;

                    if (subcheck.requirement == UnitSubcheckPartRequirement.Present)
                    {
                        triggered =
                        (
                            subcheck.not ? 
                            partPersistent == null : 
                            partPersistent != null
                        );
                    }
                    else if (subcheck.requirement == UnitSubcheckPartRequirement.Damaged)
                    {
                        float integrityNormalized = partPersistent != null ? partPersistent.integrityNormalized.f : 1f;
                        if (unitPersistent.hasUnitFrameIntegrity && isOverworld)
                            integrityNormalized = unitPersistent.unitFrameIntegrity.f;
                        
                        float threshold = 0.01f;
                        triggered = partPersistent != null && 
                        (
                            subcheck.not ? 
                            integrityNormalized.RoughlyEqual (1f, threshold) : 
                            integrityNormalized < (1f - threshold)
                        );
                    }
                    else if (subcheck.requirement == UnitSubcheckPartRequirement.Destroyed)
                    {
                        if (partPersistent != null)
                        {
                            triggered = 
                                subcheck.not ? 
                                (partPersistent.integrityNormalized.f > 0f && !partPersistent.isWrecked) : 
                                (partPersistent.integrityNormalized.f <= 0f || partPersistent.isWrecked);
                        }
                        else
                            triggered = !subcheck.not;
                    }
                    else if (subcheck.requirement == UnitSubcheckPartRequirement.Tags && subcheck.tags != null)
                    {
                        if (partPersistent != null && partPersistent.integrityNormalized.f > 0f && !partPersistent.isWrecked)
                        {
                            var tagCache = partPersistent.tagCache.tags;
                            triggered = true;

                            foreach (var kvp in subcheck.tags)
                            {
                                var tag = kvp.Key;
                                bool required = kvp.Value;
                                bool found = tagCache.Contains (tag);
                                if (found != required)
                                {
                                    triggered = false;
                                    break;
                                }
                            }

                            if (subcheck.not)
                                triggered = !triggered;
                        }
                        else
                        {
                            triggered = false;
                        }
                    }
                    
                    /*
                    else if (subcheck.requirement == UnitSubcheckPartRequirement.Archetype)
                    {
                        triggered = partPersistent != null && 
                        (
                            subcheck.not ? 
                            !string.Equals (subcheck.archetype, partPersistent.dataKeyPartArchetype.s) : 
                            string.Equals (subcheck.archetype, partPersistent.dataKeyPartArchetype.s)
                        );
                    }
                    */

                    if (triggered) 
                        subchecksTriggered += 1;
                }

                if (subchecksTriggered > 0 && statsMethod == EntityCheckMethod.RequireOne)
                    partsTriggered = true;

                if (partsMethod == EntityCheckMethod.RequireAll && subchecksTriggered == parts.Count)
                    partsTriggered = true;
            }

            bool subsystemsRelevant = subsystems != null && subsystems.Count > 0;
            bool subsystemsTriggered = false;
            if (subsystemsRelevant)
            {
                int subchecksTriggered = 0;
                for (int i = 0; i < subsystems.Count; ++i)
                {
                    if (subchecksTriggered > 0 && subsystemsMethod == EntityCheckMethod.RequireOne)
                    {
                        subsystemsTriggered = true;
                        break;
                    }
                    
                    var subcheck = subsystems[i];
                    var partPersistent = EquipmentUtility.GetPartInUnit (unitPersistent, subcheck.socket, false, partEntities);
                    bool triggered = false;
                    
                    if (subcheck.requirement == UnitSubcheckSubsystemRequirement.Present)
                    {
                        if (partPersistent != null)
                        {
                            var subsystem = EquipmentUtility.GetSubsystemInPart (partPersistent, subcheck.hardpoint);
                            triggered = subcheck.not ? subsystem == null : subsystem != null;
                        }
                    }
                    
                    if (triggered)
                        subchecksTriggered += 1;
                }
                
                if (subchecksTriggered > 0 && statsMethod == EntityCheckMethod.RequireOne)
                    subsystemsTriggered = true;

                if (subsystemsMethod == EntityCheckMethod.RequireAll && subchecksTriggered == subsystems.Count)
                    subsystemsTriggered = true;
            }

            bool statsRelevant = stats != null && stats.Count > 0;
            bool statsTriggered = false;
            if (statsRelevant)
            {
                int subchecksTriggered = 0;
                for (int i = 0; i < stats.Count; ++i)
                {
                    if (subchecksTriggered > 0 && statsMethod == EntityCheckMethod.RequireOne)
                    {
                        statsTriggered = true;
                        break;
                    }
                    
                    var subcheck = stats[i];
                    if (subcheck.target == UnitSubcheckStatTarget.Unit)
                    {
                        var total = DataHelperStats.GetCachedStatForUnit (subcheck.stat, unitPersistent);
                        if (subcheck.requirement == UnitSubcheckStatRequirement.AboveThreshold && total >= subcheck.threshold)
                            subchecksTriggered += 1;
                        else if (subcheck.requirement == UnitSubcheckStatRequirement.BelowThreshold && total < subcheck.threshold)
                            subchecksTriggered += 1;
                    }
                    else if (subcheck.target == UnitSubcheckStatTarget.Part)
                    {
                        var part = EquipmentUtility.GetPartInUnit (unitPersistent, subcheck.socket, false, partEntities);
                        if (part != null)
                        {
                            var total = DataHelperStats.GetCachedStatForPart (subcheck.stat, part);
                            if (subcheck.requirement == UnitSubcheckStatRequirement.AboveThreshold && total >= subcheck.threshold)
                                subchecksTriggered += 1;
                            else if (subcheck.requirement == UnitSubcheckStatRequirement.BelowThreshold && total < subcheck.threshold)
                                subchecksTriggered += 1;
                        }
                    }
                }
                
                if (subchecksTriggered > 0 && statsMethod == EntityCheckMethod.RequireOne)
                    statsTriggered = true;

                if (statsMethod == EntityCheckMethod.RequireAll && subchecksTriggered == stats.Count)
                    statsTriggered = true;
            }

            bool pilotRelevant = pilot != null && pilot.Count > 0;
            bool pilotTriggered = false;
            if (pilotRelevant)
            {
                int subchecksTriggered = 0;
                for (int i = 0; i < pilot.Count; ++i)
                {
                    if (subchecksTriggered > 0)
                    {
                        pilotTriggered = true;
                        break;
                    }
                    
                    var subcheck = pilot[i];
                    var pilotPersistent = IDUtility.GetLinkedPilot (unitPersistent);
                    bool triggered = false;
                    
                    if (subcheck.requirement == UnitSubcheckPilotRequirement.Present)
                    {
                        triggered =
                        (
                            subcheck.not ? 
                            pilotPersistent == null : 
                            pilotPersistent != null
                        );
                    }

                    if (triggered)
                        subchecksTriggered += 1;
                }

                if (subchecksTriggered > 0)
                    pilotTriggered = true;
            }

            if (encompassingMethod == EntityCheckMethod.RequireAll)
            {
                bool result = 
                    (tagsTriggered || !tagsRelevant) &&
                    (blueprintsTriggered || !blueprintsRelevant) &&
                    (partsTriggered || !partsRelevant) &&
                    (subsystemsTriggered || !subsystemsRelevant) &&
                    (statsTriggered || !statsRelevant) &&
                    (pilotTriggered || !pilotRelevant);

                return result;
            }
            else
            {
                bool result = 
                    tagsTriggered ||
                    blueprintsTriggered ||
                    partsTriggered ||
                    subsystemsTriggered ||
                    statsTriggered ||
                    pilotTriggered;

                return result;
            }
        }
        #endif
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

        public bool textDescInTooltips = false;
        public bool hidden = false;
        
        public string icon;
        public EntityCheckSeverity severity;
        
        [DropdownReference (true)]
        public DataBlockUnitCheck check;

        [DropdownReference]
        public List<ICombatUnitValidationFunction> functions;

        #if !PB_MODSDK
        public bool IsTriggered (PersistentEntity unitPersistent)
        {
            if (check != null)
            {
                if (!check.IsTriggered (unitPersistent))
                    return false;
            }

            if (functions != null)
            {
                foreach (var function in functions)
                {
                    if (function != null && !function.IsValid (unitPersistent))
                        return false;
                }
            }
            
            return true;
        }
        #endif

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            
            if (check != null)
                check.key = key;
        }

        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            if (check != null)
            {
                if (check.parts == null)
                    return;

                // Keeping configs clean of unwanted info
                foreach (var subcheck in check.parts)
                {
                    if (subcheck.requirement != UnitSubcheckPartRequirement.Tags)
                        subcheck.tags = null;
                }
            }
        }
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitChecks, $"{key}__header");
            textDesc = DataManagerText.GetText (TextLibs.unitChecks, $"{key}__text");
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerUnitCheck () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
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

