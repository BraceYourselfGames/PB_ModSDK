using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataContainerPilotPersistentProfileParent : DataContainerParent
    {
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerPilotPersistentProfile.data.Keys;
    }

    [Serializable]
    public class DataBlockPilotProfileSection
    {
        [HideLabel, HorizontalGroup (120f)]
        public Color color;
        
        [HorizontalGroup]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [YamlIgnore]
        [ShowIf (DataEditor.textAttrArg)]
        public string textName;
        
        [YamlIgnore]
        [ShowIf (DataEditor.textAttrArg)]
        [Multiline (3)]
        public string textDesc;
    }


    public class DataBlockPilotProfileCore
    {
        public int version;
        
        [DropdownReference (true)]
        public string nameInternalSuffix;
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerOverworldProvinceModifier.data.Keys")]
        public string modifierRemovedOnDeath;
        
        [DropdownReference (true)]
        public DataBlockFloat01 injectionChance;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockPilotProfileCore () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public class DataContainerPilotPersistentProfile : DataContainerWithText
    {
        private const string tgCore = "Core";
        private const string tgEvents = "Events";
        
        
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;

        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataContainerPilotPersistentProfileParent ()")]
        public List<DataContainerPilotPersistentProfileParent> parents;
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children;
        
        
        [DropdownReference (true)]
        public DataBlockPilotProfileCore core;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockPilotProfileCore coreProc;
        
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textName;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockStringNonSerialized textNameProc;
        
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textCallsign;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockStringNonSerialized textCallsignProc;
        
        
        [DropdownReference (true)]
        public DataBlockStringNonSerialized textUnit;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockStringNonSerialized textUnitProc;
        
        
        [DropdownReference (true)]
        public DataBlockStringNonSerializedLong textDesc;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockStringNonSerializedLong textDescProc;
        
        
        [DropdownReference (true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockPilotProfileSection> textSections;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockPilotProfileSection> textSectionsProc;
        
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitRole.data.Keys")]
        public string roleKey;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public string roleKeyProc;
        
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerPilotAppearancePreset.data.Keys")]
        public string appearanceKey;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public string appearanceKeyProc;
        
        
        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerUnitLiveryPreset.data.Keys")]
        public string liveryPreset;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public string liveryPresetProc;
        
        
        [DropdownReference (true)]
        [OnValueChanged("RefreshTraits", true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.Foldout)]
        public SortedDictionary<string, DataBlockPilotTraitSlotBase> traitSlotsInjected;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<string, DataBlockPilotTraitSlotBase> traitSlotsInjectedProc;
        
        
        [DropdownReference (true)]
        public DataFilterUnitPreset unitPresetFilter;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataFilterUnitPreset unitPresetFilterProc;
        
        
        [DropdownReference (true)]
        public DataBlockScenarioGenerationInjection injectionCheck;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockScenarioGenerationInjection injectionCheckProc;
        
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt deploymentTurnCheck;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldEventSubcheckInt deploymentTurnCheckProc;
        
        
        [DropdownReference]
        public List<ICombatStateValidationFunction> deploymentConditions;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<ICombatStateValidationFunction> deploymentConditionsProc;
        
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnSpawn;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockUnitCombatEffect> effectsOnSpawnProc;
        
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnLandingStart;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockUnitCombatEffect> effectsOnLandingStartProc;
        
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnArrival;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockUnitCombatEffect> effectsOnArrivalProc;
        
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnTraumaCombat;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockUnitCombatEffect> effectsOnTraumaCombatProc;
        
        
        [DropdownReference]
        public List<DataBlockPilotOverworldEffect> effectsOnTraumaDebriefing;

        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockPilotOverworldEffect> effectsOnTraumaDebriefingProc;
        
        
        [DropdownReference]
        public List<DataBlockUnitCombatEffect> effectsOnDeathCombat;

        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockUnitCombatEffect> effectsOnDeathCombatProc;
        
        
        [DropdownReference]
        public List<DataBlockPilotOverworldEffect> effectsOnDeathDebriefing;

        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public List<DataBlockPilotOverworldEffect> effectsOnDeathDebriefingProc;
        
        
        [DropdownReference (true)]
        public DataBlockUnitDirector director;

        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockUnitDirector directorProc;
        
        
        

        public override void ResolveText ()
        {
            if (textName != null)
                textName.s = DataManagerText.GetText (TextLibs.pilotProfiles, $"{key}__name", true);
            
            if (textCallsign != null)
                textCallsign.s = DataManagerText.GetText (TextLibs.pilotProfiles, $"{key}__callsign", true);
            
            if (textUnit != null)
                textUnit.s = DataManagerText.GetText (TextLibs.pilotProfiles, $"{key}__unit", true);
            
            if (textDesc != null)
                textDesc.s = DataManagerText.GetText (TextLibs.pilotProfiles, $"{key}__desc", true);

            if (textSections != null)
            {
                foreach (var kvp in textSections)
                {
                    var sectionKey = kvp.Key;
                    var section = kvp.Value;
                    if (section == null)
                        continue;
                    
                    section.textName = DataManagerText.GetText (TextLibs.pilotProfiles, $"{key}__s_{sectionKey}_name", true);
                    section.textDesc = DataManagerText.GetText (TextLibs.pilotProfiles, $"{key}__s_{sectionKey}_desc", true);
                }
            }
        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            RefreshTraits ();
        }
        
        private void RefreshTraits ()
        {
            if (traitSlotsInjected != null)
            {
                foreach (var kvp in traitSlotsInjected)
                {
                    var tg = kvp.Value;
                    if (tg != null)
                    {
                        // tg.key = kvp.Key;
                        tg.Refresh ();
                    }
                }
            }
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerPilotPersistentProfile () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private static bool IsInheritanceVisible => DataMultiLinkerPilotPersistentProfile.Presentation.showInheritance;

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (textName != null && !string.IsNullOrEmpty (textName.s))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotProfiles, $"{key}__name", textName.s);
            
            if (textCallsign != null && !string.IsNullOrEmpty (textCallsign.s))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotProfiles, $"{key}__callsign", textCallsign.s);
            
            if (textUnit != null && !string.IsNullOrEmpty (textUnit.s))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotProfiles, $"{key}__unit", textUnit.s);
            
            if (textDesc != null && !string.IsNullOrEmpty (textDesc.s))
                DataManagerText.TryAddingTextToLibrary (TextLibs.pilotProfiles, $"{key}__desc", textDesc.s);
            
            if (textSections != null)
            {
                foreach (var kvp in textSections)
                {
                    var sectionKey = kvp.Key;
                    var section = kvp.Value;
                    if (section == null)
                        continue;
                    
                    if (!string.IsNullOrEmpty (section.textName))
                        DataManagerText.TryAddingTextToLibrary (TextLibs.pilotProfiles, $"{key}__s_{sectionKey}_name", section.textName);
                    
                    if (!string.IsNullOrEmpty (section.textDesc))
                        DataManagerText.TryAddingTextToLibrary (TextLibs.pilotProfiles, $"{key}__s_{sectionKey}_desc", section.textDesc);
                }
            }
        }
        
        #endif
        #endregion
    }
}